using System.IO;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// Turns a file the user picked into a replacement (write plan D2): a .wem is taken as it is (its codec's plug-in ID
/// from the fmt tag); anything else becomes a PCM wem. A 16-bit PCM WAV is read directly; other audio (24-bit or float
/// WAV, ogg, flac…) is converted to 16-bit PCM by vgmstream first.
/// </summary>
public static class SoundImport
{
    /// <summary>File dialog filters.</summary>
    public static readonly string[] Filters = { "wav,wem,ogg,flac" };

    public static SoundMediaEdit Load(string file, string streamPath, out string error)
    {
        error = null;
        try
        {
            var bytes = File.ReadAllBytes(file);
            var label = Path.GetFileName(file);

            if (file.EndsWith(".wem", StringComparison.OrdinalIgnoreCase) || WemWriter.IsWem(bytes))
            {
                uint plugin = WemWriter.PluginIdFor(bytes);
                if (plugin == 0)
                {
                    error = LOC.Get("SOUND_Import_Unknown_Wem_Codec", label);
                    return null;
                }
                return new SoundMediaEdit { Wem = bytes, PluginId = plugin, Label = label, StreamPath = streamPath };
            }

            (byte[] Pcm, int Channels, int SampleRate) pcm;
            try
            {
                pcm = WemWriter.ReadPcm16Wav(bytes);
            }
            catch (InvalidDataException)
            {
                pcm = DecodeWithVgmstream(file, bytes);
            }

            if (WemWriter.SpeakerMask(pcm.Channels) == 0)
            {
                error = LOC.Get("SOUND_Import_Channels", label, pcm.Channels);
                return null;
            }

            var wem = WemWriter.WritePcm(pcm.Pcm, pcm.Channels, pcm.SampleRate);
            return new SoundMediaEdit { Wem = wem, PluginId = WemWriter.PcmPluginId, Label = label, StreamPath = streamPath };
        }
        catch (Exception e)
        {
            error = LOC.Get("SOUND_Import_Failed", Path.GetFileName(file), e.Message);
            return null;
        }
    }

    private static (byte[], int, int) DecodeWithVgmstream(string file, byte[] bytes)
    {
        if (!VgmstreamCli.IsAvailable)
            throw new InvalidOperationException(LOC.Get("SOUND_Log_No_Vgmstream", VgmstreamCli.ExePath));

        var ext = Path.GetExtension(file).TrimStart('.').ToLowerInvariant();
        var source = new SoundSource { Name = Path.GetFileName(file), Extension = ext, CacheKey = $"import|{file}|{bytes.Length}", ReadBytes = () => bytes };
        var outPath = Path.Combine(VgmstreamCli.TempFolder, $"import_{Guid.NewGuid():N}.wav");
        try
        {
            VgmstreamCli.DecodeToFile(source, outPath);
            return WemWriter.ReadPcm16Wav(File.ReadAllBytes(outPath));
        }
        finally
        {
            VgmstreamCli.Forget(source.CacheKey);
            try { File.Delete(outPath); } catch { }
        }
    }

    /// <summary>
    /// The game's own audio of a sound (for Revert to Vanilla): its streamed .wem, or the copy in the game's version of
    /// the bank, with the plug-in ID of the vanilla Sound object. Null if the game has neither.
    /// </summary>
    public static SoundMediaEdit Vanilla(SoundData data, SoundEntry e, out string error)
    {
        error = null;
        byte[] wem = null;
        uint plugin = 0;

        if (e.StreamPath != null)
            wem = data.ReadVanillaFile(e.StreamPath);

        var vanillaBank = data.ReadVanillaFile(e.Container.Path);
        if (vanillaBank != null)
        {
            var b = WwiseBank.Read(vanillaBank);
            var s = b.Sounds.FirstOrDefault(x => x.SourceId == e.MediaId);
            if (s.SourceId == e.MediaId) plugin = s.PluginId;
            if (wem == null)
            {
                var m = b.InBankMedia.FirstOrDefault(x => x.Id == e.MediaId);
                if (m.Id == e.MediaId && m.Size > 0)
                    wem = vanillaBank.AsSpan(m.Offset, m.Size).ToArray();
            }
        }

        if (wem == null)
        {
            error = LOC.Get("SOUND_Import_No_Vanilla", e.DisplayName);
            return null;
        }
        if (plugin == 0) plugin = WemWriter.PluginIdFor(wem);
        return new SoundMediaEdit { Wem = wem, PluginId = plugin, Label = LOC.Get("SOUND_Edit_Vanilla_Label"), StreamPath = e.StreamPath, IsVanilla = true };
    }
}
