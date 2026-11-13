namespace StudioCore.Editors.SoundEditor;

/// <summary>A replaced sound: a media ID within one language (localized media share their ID across languages).</summary>
public readonly record struct SoundMediaKey(uint MediaId, string Language)
{
    public override string ToString() => string.IsNullOrEmpty(Language) ? MediaId.ToString() : $"{MediaId} [{Language}]";
}

/// <summary>
/// Wwise: every bank that holds a media in its DATA or references it from a HIRC Sound object, per language
/// (write plan W0: one media can be in up to 37 ER banks, so a replace has to rewrite all of them).
/// Built once on a worker by reading every bank; <see cref="IsBuilt"/> tells when it can be asked.
/// </summary>
public class SoundMediaMap
{
    private readonly SoundData Data;
    private readonly object Lock = new();
    private Task _build;

    private Dictionary<SoundMediaKey, List<SoundContainer>> _banks = new();

    public bool IsBuilt { get; private set; }
    public string Error { get; private set; }

    /// <summary>Banks read so far / to read, for a progress line.</summary>
    public int Progress, Total;

    public SoundMediaMap(SoundData data)
    {
        Data = data;
    }

    /// <summary>Starts building, if not built or building.</summary>
    public Task EnsureBuilt()
    {
        lock (Lock)
            return _build ??= Task.Run(Build);
    }

    /// <summary>The banks (of the media's language) that hold or reference it. Empty until built.</summary>
    public IReadOnlyList<SoundContainer> BanksFor(SoundMediaKey key)
        => IsBuilt && _banks.TryGetValue(key, out var l) ? l : Array.Empty<SoundContainer>();

    private void Build()
    {
        try
        {
            var banks = Data.Containers.Where(c => c.Kind == SoundContainerKind.WwiseBank).ToList();
            Total = banks.Count;
            var map = new Dictionary<SoundMediaKey, List<SoundContainer>>();

            Parallel.ForEach(banks, new ParallelOptions { MaxDegreeOfParallelism = 4 }, c =>
            {
                var ids = new HashSet<uint>();
                try
                {
                    var raw = Data.ReadFile(c.Path);
                    if (raw != null)
                    {
                        var b = WwiseBank.Read(raw);
                        foreach (var m in b.InBankMedia) ids.Add(m.Id);
                        foreach (var s in b.Sounds) if (!s.IsGenerated) ids.Add(s.SourceId);
                    }
                }
                catch { }

                lock (map)
                    foreach (var id in ids)
                    {
                        var key = new SoundMediaKey(id, c.Language ?? "");
                        (map.TryGetValue(key, out var l) ? l : map[key] = new()).Add(c);
                    }
                Interlocked.Increment(ref Progress);
            });

            foreach (var l in map.Values)
                l.Sort((a, b) => string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase));
            _banks = map;
            IsBuilt = true;
        }
        catch (Exception e)
        {
            Error = e.Message;
            Smithbox.LogError(this, LOC.Get("SOUND_MediaMap_Failed", e.Message));
        }
    }
}
