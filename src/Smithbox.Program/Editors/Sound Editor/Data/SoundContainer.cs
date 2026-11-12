using SoulsFormats;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.SoundEditor;

public enum SoundContainerKind
{
    /// <summary>A loose FMOD .fsb</summary>
    Fsb,
    /// <summary>A DS2 /sound/*.bnd holding .fev + .fsb</summary>
    FsbBinder,
    /// <summary>A Wwise .bnk</summary>
    WwiseBank,
}

public enum SoundLoadState
{
    NotLoaded,
    Loading,
    Loaded,
    Failed,
}

/// <summary>
/// One sound bank as listed in the Sound Editor. Listed from the file dictionary without reading anything; the file
/// is read and parsed the first time the container is selected (<see cref="BeginLoad"/>), on a worker.
/// </summary>
public class SoundContainer
{
    public SoundData Data;

    /// <summary>VFS path, e.g. /sound/fdp_c1070.fsb or /cs_c2010.bnk</summary>
    public string Path;

    /// <summary>File stem: fdp_c1070, cs_c2010</summary>
    public string Name;

    /// <summary>Category for the list: cs, aeg, s, vc (Wwise); c, m, rm, p (FMOD)</summary>
    public string Group;

    /// <summary>Wwise language folder (enus, ja) or empty for SFX</summary>
    public string Language = "";

    public SoundContainerKind Kind;

    public SoundLoadState State { get; private set; }
    public string Error;

    public List<SoundEntry> Entries = new();

    // shown in Properties once loaded
    public string FormatInfo = "";
    public int EventCount;
    public int GeneratedCount;

    // Kept while loaded: FSB samples and in-bank wems are handed to vgmstream from these
    private readonly Dictionary<string, byte[]> _bytes = new();

    private static readonly Regex GroupPattern = new(@"^([a-z]+)", RegexOptions.IgnoreCase);
    private static readonly string[] FmodPrefixes = { "fdp_", "frpg2_", "frpg_", "sprj_", "fdlc_" };

    public static SoundContainer Create(SoundData data, string path, SoundContainerKind kind)
    {
        var name = System.IO.Path.GetFileName(path);
        int dot = name.IndexOf('.');
        if (dot > 0) name = name[..dot];

        var c = new SoundContainer { Data = data, Path = path, Name = name, Kind = kind };

        if (kind == SoundContainerKind.WwiseBank)
        {
            var folder = path[..path.LastIndexOf('/')].Trim('/');
            var first = folder.Split('/')[0];
            if (SoundData.LanguageFolders.Contains(first)) c.Language = first;
            else if (folder.Contains("/eus", StringComparison.OrdinalIgnoreCase)) c.Language = "eus";
            c.Group = GroupPattern.Match(name).Groups[1].Value.ToLowerInvariant();
        }
        else
        {
            var stem = name;
            foreach (var p in FmodPrefixes)
                if (stem.StartsWith(p, StringComparison.OrdinalIgnoreCase)) { stem = stem[p.Length..]; break; }
            c.Group = GroupPattern.Match(stem).Groups[1].Value.ToLowerInvariant();
        }

        if (string.IsNullOrEmpty(c.Group)) c.Group = "other";
        return c;
    }

    /// <summary>Starts reading the file, if not already loaded or loading.</summary>
    public void BeginLoad()
    {
        if (State is SoundLoadState.Loading or SoundLoadState.Loaded)
            return;

        State = SoundLoadState.Loading;
        Error = null;

        Task.Run(() =>
        {
            try
            {
                var entries = Load();
                Entries = entries;
                State = SoundLoadState.Loaded;
            }
            catch (Exception e)
            {
                Error = e.Message;
                State = SoundLoadState.Failed;
                Smithbox.LogError(this, LOC.Get("SOUND_Data_Log_Failed_to_Read", Path, e.Message));
            }
        });
    }

    /// <summary>Drops the parsed data and bytes (reloaded on next selection).</summary>
    public void Unload()
    {
        if (State == SoundLoadState.Loading) return;
        Entries = new();
        _bytes.Clear();
        State = SoundLoadState.NotLoaded;
        VgmstreamCli.Forget(CacheKey(null));
    }

    private string CacheKey(string inner) => inner == null ? $"{Data.Project.Descriptor.ProjectPath}|{Path}" : $"{Data.Project.Descriptor.ProjectPath}|{Path}|{inner}";

    private List<SoundEntry> Load()
    {
        var raw = Data.ReadFile(Path) ?? throw new FileNotFoundException(LOC.Get("SOUND_Data_Log_Not_Found", Path));

        return Kind switch
        {
            SoundContainerKind.Fsb => LoadFsb(raw, null),
            SoundContainerKind.FsbBinder => LoadBinder(raw),
            _ => LoadBank(raw),
        };
    }

    private List<SoundEntry> LoadBinder(byte[] raw)
    {
        Memory<byte> data = DCX.Is(raw) ? DCX.Decompress(raw) : raw;
        IBinder bnd = data.Span[3] == '4' ? BND4.Read(data) : BND3.Read(data);

        var entries = new List<SoundEntry>();
        var formats = new List<string>();
        foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".fsb", StringComparison.OrdinalIgnoreCase)))
        {
            var inner = System.IO.Path.GetFileName(f.Name.Replace('\\', '/'));
            var list = LoadFsb(f.Bytes.ToArray(), inner);
            formats.Add(FormatInfo);
            foreach (var e in list) { e.Index = entries.Count; entries.Add(e); }
        }
        FormatInfo = string.Join(", ", formats.Distinct());
        return entries;
    }

    private List<SoundEntry> LoadFsb(byte[] raw, string inner)
    {
        var fsb = FsbFile.Read(raw);
        var key = inner ?? "";
        _bytes[key] = fsb.Bytes;
        FormatInfo = fsb.WasEncrypted ? $"{fsb.Version} {fsb.Codec} ({LOC.Get("SOUND_Label_Encrypted")})" : $"{fsb.Version} {fsb.Codec}";

        var cacheKey = CacheKey(inner);
        var entries = new List<SoundEntry>();
        foreach (var s in fsb.Samples)
        {
            var e = new SoundEntry
            {
                Container = this,
                Index = entries.Count,
                Name = s.Name ?? $"#{s.Index}",
                InnerFile = inner,
                Codec = s.Codec,
                SampleRate = s.SampleRate,
                Channels = s.Channels,
                Seconds = s.Seconds,
                LoopStart = s.LoopStart,
                LoopEnd = s.LoopEnd,
                IsEmpty = !s.HasData,
            };
            int subsong = s.Index + 1;
            e.Source = new SoundSource
            {
                Name = $"{Path}{(inner != null ? "|" + inner : "")} #{subsong}",
                Extension = "fsb",
                Subsong = subsong,
                CacheKey = cacheKey,
                ReadBytes = () => _bytes.TryGetValue(key, out var b) ? b : null,
            };
            entries.Add(e);
        }
        return entries;
    }

    private List<SoundEntry> LoadBank(byte[] raw)
    {
        var bank = WwiseBank.Read(raw);
        _bytes[""] = raw;
        FormatInfo = $"Wwise v{bank.Version}";
        EventCount = bank.EventIds.Count;
        GeneratedCount = bank.Sounds.Count(s => s.IsGenerated);

        var soundsByMedia = bank.Sounds.Where(s => !s.IsGenerated).GroupBy(s => s.SourceId).ToDictionary(g => g.Key, g => g.ToList());
        var inBank = bank.InBankMedia.ToDictionary(m => m.Id, m => m);

        // every media the bank's Sound objects play, then any in-bank media no Sound object names
        var ids = new List<uint>();
        var seen = new HashSet<uint>();
        foreach (var s in bank.Sounds)
            if (!s.IsGenerated && seen.Add(s.SourceId)) ids.Add(s.SourceId);
        foreach (var m in bank.InBankMedia)
            if (seen.Add(m.Id)) ids.Add(m.Id);

        var entries = new List<SoundEntry>();
        foreach (var id in ids)
        {
            soundsByMedia.TryGetValue(id, out var sounds);
            bool streamedByObject = sounds?.Any(s => s.IsStreamed) == true;
            var streamPath = streamedByObject || !inBank.ContainsKey(id) ? Data.FindStreamedWem(id, Language) : null;

            var e = new SoundEntry
            {
                Container = this,
                Index = entries.Count,
                MediaId = id,
                SoundObjectCount = sounds?.Count ?? 0,
                Codec = sounds != null ? WwiseBank.PluginName(sounds[0].PluginId) : "",
            };

            // the in-bank header (also present for prefetched sounds) gives format and the full length
            WemHeader header = null;
            if (inBank.TryGetValue(id, out var im) && im.Size > 0)
            {
                header = WemHeader.Read(raw.AsSpan(im.Offset, Math.Min(im.Size, 512)));
                if (header != null)
                {
                    e.Codec = header.Codec;
                    e.SampleRate = header.SampleRate;
                    e.Channels = header.Channels;
                    e.Seconds = header.Seconds;
                    e.SecondsApprox = !header.SecondsExact;
                }
            }

            // prefetched sounds have the start in DATA and the whole sound streamed: play the streamed file
            if (streamPath != null)
            {
                e.IsStreamed = true;
                e.StreamPath = streamPath;
                e.Source = new SoundSource
                {
                    Name = streamPath,
                    Extension = "wem",
                    CacheKey = $"{Data.Project.Descriptor.ProjectPath}|{streamPath}",
                    ReadBytes = () => Data.ReadFile(streamPath),
                };
            }
            else if (inBank.TryGetValue(id, out var m) && header == null)
            {
                // Wwise plug-in media (effect / IR data: AC6 "00 04 02 00…", DB_NT "PLUG"), not a wem
                e.IsNotAudio = true;
                e.Codec = LOC.Get("SOUND_Codec_Plugin_Data");
            }
            else if (inBank.TryGetValue(id, out m))
            {
                e.IsEmpty = m.Size == 0;
                var offset = m.Offset;
                var size = m.Size;
                e.Source = new SoundSource
                {
                    Name = $"{Path}#{id}",
                    Extension = "wem",
                    CacheKey = $"{Data.Project.Descriptor.ProjectPath}|{Path}#{id}",
                    ReadBytes = () => _bytes.TryGetValue("", out var b) ? b.AsSpan(offset, size).ToArray() : null,
                };
            }
            else
            {
                // streamed, but no .wem with this ID in the dictionary
                e.IsStreamed = true;
                e.IsEmpty = true;
            }

            entries.Add(e);
        }
        return entries;
    }
}
