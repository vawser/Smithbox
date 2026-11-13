using SoulsFormats;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// FMOD event projects whose events play in banks of another name, by bank. A bank's own project (same stem: the loose
/// <c>.fev</c> next to the <c>.fsb</c>, or the <c>.fev</c> inside a DS2 binder) is read by the bank itself; this holds the
/// others, found from each project's header bank list:
/// <list type="bullet">
/// <item>DS1/DS1R: <c>frpg.fev</c> (154 banks), <c>fdlc.fev</c> (26) and ~25 character projects that share a second bank.</item>
/// <item>DS2S: the 16 projects in <c>frpg2.fevbnd.dcx</c> (frpg2_c1 → frpg2_c1000, frpg2_c1010…); its binders hold only the .fsb.</item>
/// </list>
/// DS2's binders aren't scanned (6 of them list a second bank). Built once, on a worker (bank loads and the sound index
/// call it); projects are parsed on first use and kept.
/// </summary>
public class SoundFevCatalog
{
    private readonly SoundData Data;
    private readonly object Lock = new();
    private bool Built;

    private sealed class Project
    {
        public string Name;
        public byte[] Bytes;
        public FevFile Parsed;
    }

    // bank stem -> projects (of another name) that list it
    private readonly Dictionary<string, List<Project>> _byBank = new(StringComparer.OrdinalIgnoreCase);

    public SoundFevCatalog(SoundData data)
    {
        Data = data;
    }

    /// <summary>The projects other than its own that list <paramref name="bank"/>, parsed.</summary>
    public List<(string Name, FevFile Fev)> ProjectsFor(string bank)
    {
        EnsureBuilt();
        var result = new List<(string, FevFile)>();
        if (!_byBank.TryGetValue(bank, out var list))
            return result;

        foreach (var p in list)
        {
            lock (p)
                p.Parsed ??= FevFile.Read(p.Bytes);
            result.Add((p.Name, p.Parsed));
        }
        return result;
    }

    /// <summary>Every catalogued project, parsed (for the sound index).</summary>
    public List<(string Name, FevFile Fev)> AllProjects()
    {
        EnsureBuilt();
        var all = _byBank.Values.SelectMany(l => l).Distinct().ToList();
        foreach (var p in all)
            lock (p)
                p.Parsed ??= FevFile.Read(p.Bytes);
        return all.Select(p => (p.Name, p.Parsed)).ToList();
    }

    private void EnsureBuilt()
    {
        if (Built) return;
        lock (Lock)
        {
            if (Built) return;
            try { Build(); }
            catch (Exception e) { Smithbox.LogError(this, LOC.Get("SOUND_Fev_Catalog_Failed", e.Message)); }
            Built = true;
        }
    }

    private void Build()
    {
        foreach (var path in Data.FmodProjectPaths())
        {
            var bytes = Data.ReadFile(path);
            if (bytes == null)
                continue;

            if (path.EndsWith(".fev", StringComparison.OrdinalIgnoreCase))
            {
                Add(System.IO.Path.GetFileNameWithoutExtension(path), bytes);
                continue;
            }

            // a binder of projects (DS2S frpg2.fevbnd.dcx)
            Memory<byte> raw = DCX.Is(bytes) ? DCX.Decompress(bytes) : bytes;
            IBinder bnd = raw.Span[3] == '4' ? BND4.Read(raw) : BND3.Read(raw);
            foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".fev", StringComparison.OrdinalIgnoreCase)))
                Add(System.IO.Path.GetFileNameWithoutExtension(f.Name.Replace('\\', '/')), f.Bytes.ToArray());
        }
    }

    private void Add(string name, byte[] bytes)
    {
        var banks = FevFile.ReadBankNames(bytes).Where(b => !b.Equals(name, StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (banks.Count == 0)
            return;

        var p = new Project { Name = name, Bytes = bytes };
        foreach (var b in banks)
            (_byBank.TryGetValue(b, out var l) ? l : _byBank[b] = new()).Add(p);
    }
}
