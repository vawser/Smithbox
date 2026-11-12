using System.IO;
using System.Text;

namespace StudioCore.Editors.SoundEditor;

public enum SoundExportFormat
{
    /// <summary>Decoded by vgmstream to PCM16 WAV, played once (loop points ignored).</summary>
    Wav,
    /// <summary>Wwise only: the .wem bytes as the game has them (streamed file, or the bank's DIDX slice).</summary>
    Wem,
}

/// <summary>
/// One export run: a list of sounds written to a folder on a worker, a few decodes at a time. The view keeps the
/// running job and draws its progress in the Player window; a new export waits until it ends.
/// </summary>
public class SoundExportJob
{
    /// <summary>Decodes at once: each is a vgmstream process, so a few keep the CPU busy without flooding it.</summary>
    private const int Parallelism = 4;

    public string Folder { get; }
    public string Label { get; }
    public SoundExportFormat Format { get; }

    public int Total { get; private set; }
    public int Done => _done;
    public int Failed => _failed;
    public bool Running { get; private set; }
    public bool Cancelled => _cts.IsCancellationRequested;

    /// <summary>The first failure's message, shown under the progress.</summary>
    public string FirstError { get; private set; }

    private int _done, _failed;
    private readonly CancellationTokenSource _cts = new();

    private SoundExportJob(string folder, string label, SoundExportFormat format)
    {
        Folder = folder;
        Label = label;
        Format = format;
    }

    /// <summary>
    /// Starts exporting <paramref name="entries"/> into <paramref name="folder"/>. <paramref name="waitFor"/> is a
    /// bank that must finish loading first (exporting a bank that wasn't opened yet); its entries are taken once it has.
    /// </summary>
    public static SoundExportJob Start(string folder, string label, SoundExportFormat format,
        Func<IReadOnlyList<SoundEntry>> entries, SoundContainer waitFor = null)
    {
        var job = new SoundExportJob(folder, label, format) { Running = true };
        Task.Run(() => job.Run(entries, waitFor));
        return job;
    }

    public void Cancel() => _cts.Cancel();

    private void Run(Func<IReadOnlyList<SoundEntry>> getEntries, SoundContainer waitFor)
    {
        try
        {
            if (waitFor != null)
            {
                waitFor.BeginLoad();
                while (waitFor.State is SoundLoadState.NotLoaded or SoundLoadState.Loading && !Cancelled)
                    Thread.Sleep(50);
                if (waitFor.State == SoundLoadState.Failed)
                    throw new InvalidOperationException(waitFor.Error);
            }

            var items = Plan(getEntries());
            Total = items.Count;
            Directory.CreateDirectory(Folder);

            Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = Parallelism, CancellationToken = _cts.Token },
                item =>
                {
                    try
                    {
                        ExportOne(item.Entry, item.Path, Format);
                        Interlocked.Increment(ref _done);
                    }
                    catch (Exception e)
                    {
                        Interlocked.Increment(ref _failed);
                        lock (this) FirstError ??= $"{item.Entry.DisplayName}: {e.Message}";
                    }
                });
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            FirstError ??= e.Message;
        }
        finally
        {
            Running = false;
            Report();
        }
    }

    /// <summary>Picks a file name for each exportable entry, unique within the folder of this run.</summary>
    private List<(SoundEntry Entry, string Path)> Plan(IReadOnlyList<SoundEntry> entries)
    {
        var ext = Format == SoundExportFormat.Wav ? ".wav" : ".wem";
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = new List<(SoundEntry, string)>();

        foreach (var e in entries)
        {
            if (!CanExport(e, Format))
                continue;

            var stem = FileStem(e);
            var name = stem;
            // two samples of one name (FSB allows it), or the same media reached twice through an event
            if (!used.Add(name))
            {
                name = $"{stem}_{e.Index}";
                if (!used.Add(name))
                    continue;
            }
            list.Add((e, Path.Combine(Folder, name + ext)));
        }
        return list;
    }

    public static bool CanExport(SoundEntry e, SoundExportFormat format)
        => e.CanPlay && (format == SoundExportFormat.Wav || e.Container.Kind == SoundContainerKind.WwiseBank);

    private static void ExportOne(SoundEntry e, string path, SoundExportFormat format)
    {
        if (format == SoundExportFormat.Wem)
        {
            var bytes = e.Source.ReadBytes?.Invoke() ?? throw new InvalidOperationException(LOC.Get("SOUND_Export_No_Bytes"));
            File.WriteAllBytes(path, bytes);
            return;
        }

        VgmstreamCli.DecodeToFile(e.Source, path);
    }

    /// <summary>
    /// The file name (no extension) for an entry: FMOD sample name; Wwise media ID plus the original source name when
    /// the name table has it ("806100001_s000806100"), so files sort by ID and still say what they are.
    /// </summary>
    public static string FileStem(SoundEntry e)
    {
        string stem;
        if (e.Container.Kind == SoundContainerKind.WwiseBank)
        {
            var source = !string.IsNullOrEmpty(e.SourceFile)
                ? Path.GetFileNameWithoutExtension(e.SourceFile.Replace('\\', '/'))
                : !string.IsNullOrEmpty(e.Name) ? e.Name : null;
            stem = source != null && source != e.MediaId.ToString() ? $"{e.MediaId}_{source}" : e.MediaId.ToString();
        }
        else
        {
            stem = !string.IsNullOrEmpty(e.Name) ? Path.GetFileNameWithoutExtension(e.Name) : $"#{e.Index}";
            if (e.InnerFile != null)
                stem = $"{Path.GetFileNameWithoutExtension(e.InnerFile)}_{stem}";
        }
        return Sanitize(stem);
    }

    /// <summary>Makes a name safe as a file or folder name.</summary>
    public static string Sanitize(string name)
    {
        var bad = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);
        foreach (var ch in name)
            sb.Append(Array.IndexOf(bad, ch) >= 0 ? '_' : ch);
        var s = sb.ToString().Trim().TrimEnd('.');
        return string.IsNullOrEmpty(s) ? "sound" : s;
    }

    private void Report()
    {
        if (Cancelled)
            Smithbox.Log(this, LOC.Get("SOUND_Export_Log_Cancelled", Label, Done, Total, Folder));
        else if (Failed > 0 || (Total == 0 && FirstError != null))
            Smithbox.LogError(this, LOC.Get("SOUND_Export_Log_Failed", Label, Done, Total, Failed, Folder, FirstError ?? ""));
        else
            Smithbox.Log(this, LOC.Get("SOUND_Export_Log_Done", Label, Done, Folder));
    }
}
