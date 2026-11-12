using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace StudioCore.Editors.SoundEditor;

/// <summary>What vgmstream reports about one subsong (vgmstream-cli -m -I).</summary>
public sealed class SoundInfo
{
    public int SampleRate;
    public int Channels;
    public long Samples;
    public string Encoding;
    public string Metadata;
    public string StreamName;
    public int StreamIndex;
    public int StreamTotal;
    public long LoopStart = -1;
    public long LoopEnd = -1;

    public double Seconds => SampleRate > 0 ? (double)Samples / SampleRate : 0;
}

/// <summary>
/// Decoding through vgmstream-cli (ISC license, shipped in vgmstream/ next to the program; see Licenses/vgmstream).
/// The official Windows release has no libvgmstream.dll, so each decode is a short-lived process: the source's bytes
/// are written once to a temp file (vgmstream reads by file name and extension), and the decoded PCM16 WAV is read from
/// its stdout as it is produced, so playback can start before the decode ends.
/// <para>Kept free of Smithbox dependencies: Claude/Tools/SoundSurvey links this file.</para>
/// </summary>
public static class VgmstreamCli
{
    /// <summary>Folder holding vgmstream-cli.exe. Defaults to "vgmstream" next to the program.</summary>
    public static string Folder = Path.Combine(AppContext.BaseDirectory, "vgmstream");

    public static string ExePath => Path.Combine(Folder, OperatingSystem.IsWindows() ? "vgmstream-cli.exe" : "vgmstream-cli");

    public static bool IsAvailable => File.Exists(ExePath);

    /// <summary>Where source bytes are written for vgmstream to read.</summary>
    public static string TempFolder = Path.Combine(Path.GetTempPath(), "Smithbox", "Sound");

    private static readonly ConcurrentDictionary<string, string> TempFiles = new();
    private static readonly object WriteLock = new();

    /// <summary>The temp file holding the source's bytes, written on first use.</summary>
    public static string GetTempFile(SoundSource source)
    {
        var key = source.CacheKey ?? source.Name;
        if (TempFiles.TryGetValue(key, out var existing) && File.Exists(existing))
            return existing;

        lock (WriteLock)
        {
            if (TempFiles.TryGetValue(key, out existing) && File.Exists(existing))
                return existing;

            Directory.CreateDirectory(TempFolder);
            var hash = Convert.ToHexString(SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(key)))[..16];
            var path = Path.Combine(TempFolder, $"{hash}.{source.Extension}");
            var bytes = source.ReadBytes?.Invoke() ?? throw new InvalidOperationException($"No bytes for {source.Name}");
            var tmp = path + ".part";
            File.WriteAllBytes(tmp, bytes);
            File.Move(tmp, path, true);
            TempFiles[key] = path;
            return path;
        }
    }

    /// <summary>Drops the temp file of one container, e.g. after its bytes changed.</summary>
    public static void Forget(string cacheKey)
    {
        if (TempFiles.TryRemove(cacheKey, out var path))
            TryDelete(path);
    }

    /// <summary>Removes the temp files written this session (called when the editor's data is unloaded).</summary>
    public static void ClearTempFiles()
    {
        foreach (var key in TempFiles.Keys.ToArray())
            if (TempFiles.TryRemove(key, out var path))
                TryDelete(path);
    }

    private static void TryDelete(string path)
    {
        // a decode may still have it open; it is left for the next clear in that case
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    /// <summary>Reads format and length without decoding. Throws with vgmstream's message on failure.</summary>
    public static SoundInfo GetInfo(SoundSource source)
    {
        var file = GetTempFile(source);
        var psi = NewStartInfo();
        psi.ArgumentList.Add("-m");
        psi.ArgumentList.Add("-I");
        AddSubsong(psi, source);
        psi.ArgumentList.Add(file);

        using var p = Process.Start(psi) ?? throw new InvalidOperationException("vgmstream-cli did not start");
        var errTask = p.StandardError.ReadToEndAsync();
        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        var err = errTask.Result;

        // -I prints one JSON object; -m prints text metadata before it
        int start = output.IndexOf('{');
        if (p.ExitCode != 0 || start < 0)
            throw new InvalidOperationException($"vgmstream could not read {source.Name}: {FirstLine(err, output)}");

        using var doc = JsonDocument.Parse(output[start..]);
        var r = doc.RootElement;
        var info = new SoundInfo
        {
            SampleRate = GetInt(r, "sampleRate"),
            Channels = GetInt(r, "channels"),
            Samples = GetLong(r, "numberOfSamples"),
            Encoding = GetString(r, "encoding"),
            Metadata = GetString(r, "metadataSource"),
        };
        if (r.TryGetProperty("streamInfo", out var si) && si.ValueKind == JsonValueKind.Object)
        {
            info.StreamName = GetString(si, "name");
            info.StreamIndex = GetInt(si, "index");
            info.StreamTotal = GetInt(si, "total");
        }
        if (r.TryGetProperty("loopingInfo", out var li) && li.ValueKind == JsonValueKind.Object)
        {
            info.LoopStart = GetLong(li, "start");
            info.LoopEnd = GetLong(li, "end");
        }
        return info;
    }

    /// <summary>
    /// Starts a decode of one subsong, played once (loop points ignored), to a PCM16 WAV on the returned process's
    /// stdout. The caller reads <c>StandardOutput.BaseStream</c> and disposes the process; <paramref name="errors"/>
    /// collects stderr.
    /// </summary>
    public static Process StartDecode(SoundSource source, StringBuilder errors)
    {
        var file = GetTempFile(source);
        var psi = NewStartInfo();
        psi.ArgumentList.Add("-i");
        psi.ArgumentList.Add("-p");
        AddSubsong(psi, source);
        psi.ArgumentList.Add(file);

        var p = Process.Start(psi) ?? throw new InvalidOperationException("vgmstream-cli did not start");
        p.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (errors) errors.AppendLine(e.Data); };
        p.BeginErrorReadLine();
        return p;
    }

    /// <summary>Decodes one subsong to a WAV file (used by SoundSurvey and later by Export).</summary>
    public static void DecodeToFile(SoundSource source, string outPath)
    {
        var file = GetTempFile(source);
        var psi = NewStartInfo();
        psi.ArgumentList.Add("-i");
        psi.ArgumentList.Add("-o");
        psi.ArgumentList.Add(outPath);
        AddSubsong(psi, source);
        psi.ArgumentList.Add(file);

        using var p = Process.Start(psi) ?? throw new InvalidOperationException("vgmstream-cli did not start");
        var errTask = p.StandardError.ReadToEndAsync();
        var output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0 || !File.Exists(outPath))
            throw new InvalidOperationException($"vgmstream could not decode {source.Name}: {FirstLine(errTask.Result, output)}");
    }

    /// <summary>Kills a decode that is still running (stop pressed before it finished).</summary>
    public static void Kill(Process p)
    {
        try { if (!p.HasExited) p.Kill(); } catch { }
    }

    private static ProcessStartInfo NewStartInfo()
    {
        if (!IsAvailable)
            throw new FileNotFoundException($"vgmstream-cli not found at {ExePath}");

        return new ProcessStartInfo(ExePath)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Folder,
        };
    }

    private static void AddSubsong(ProcessStartInfo psi, SoundSource source)
    {
        if (source.Subsong <= 0) return;
        psi.ArgumentList.Add("-s");
        psi.ArgumentList.Add(source.Subsong.ToString());
    }

    private static string FirstLine(string err, string output)
    {
        var text = string.IsNullOrWhiteSpace(err) ? output : err;
        text = (text ?? "").Trim();
        int nl = text.IndexOf('\n');
        return nl < 0 ? text : text[..nl].Trim();
    }

    private static int GetInt(JsonElement e, string name)
        => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;

    private static long GetLong(JsonElement e, string name)
        => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt64() : -1;

    private static string GetString(JsonElement e, string name)
        => e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}
