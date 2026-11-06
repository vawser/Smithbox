using System.Collections.Concurrent;
using Havok;
using Havok.Formats.Tagfile;
using Havok.Reflection;

/// <summary>
/// --typediff: every distinct type a game's tagfiles and compendiums describe, compared with the game's registry.
/// Finds what an overlay must change (as NR's did) and the types a registry lacks.
/// </summary>
public static class TypeDiff
{
    private sealed record Seen(HavokType Type, string Example)
    {
        public int Count;
    }

    private static readonly ConcurrentDictionary<string, Seen> Types = new();

    public static void Reset() => Types.Clear();

    public static void Collect(byte[] data, SniffInfo sniff, string example)
    {
        if (sniff.Container is not ("Tagfile" or "Compendium")) return;
        IReadOnlyList<HavokType> types;
        try { types = HavokTagfileSerializer.ReadTypeDescriptions(data); }
        catch { return; }
        foreach (var t in types)
        {
            if (t == null || t.Optionals == HavokType.Optional.None) continue; // no body in this file
            var seen = Types.GetOrAdd(t.Identity, _ => new Seen(t, example));
            Interlocked.Increment(ref seen.Count);
        }
    }

    private static string Describe(HavokType t) =>
        $"size={t.Size} align={t.Alignment & 0xFFF} format={t.Format} version={t.Version} flags={(int)t.Flags} " +
        $"parent={t.Parent?.Identity} fields=[{string.Join(",", t.Fields.Select(f => $"{f.Name}@{f.Offset}:{(int)f.Flags}:{f.Type.Identity}"))}] " +
        $"ifaces=[{string.Join(",", t.Interfaces.Select(i => $"{i.Type.Identity}@{i.Offset}"))}]";

    /// <summary>
    /// Report lines for the game: counts, then each differing or missing type.
    /// </summary>
    public static List<string> Report(HavokProfile profile)
    {
        var lines = new List<string>();
        var registry = profile?.GetRegistry();
        if (registry == null)
        {
            lines.Add($"- {Types.Count} distinct types described; no registry for {profile} to compare with.");
            return lines;
        }

        var missing = new List<Seen>();
        var hashOnly = new List<(Seen Seen, HavokType Reg)>();
        var layout = new List<(Seen Seen, HavokType Reg, string Diff)>();
        int same = 0;
        foreach (var seen in Types.Values.OrderBy(s => s.Type.Identity))
        {
            var reg = registry.GetType(seen.Type.Identity);
            if (reg == null) { missing.Add(seen); continue; }

            string a = Describe(seen.Type), b = Describe(reg);
            bool hashDiffers = seen.Type.Hash is { } h && reg.Hash != h;
            if (a == b && !hashDiffers) { same++; continue; }
            if (a == b) { hashOnly.Add((seen, reg)); continue; }

            var ta = a.Split(' ');
            var tb = b.Split(' ');
            var first = Enumerable.Range(0, Math.Min(ta.Length, tb.Length)).FirstOrDefault(i => ta[i] != tb[i]);
            layout.Add((seen, reg, $"file `{Trim(ta[first])}` vs registry `{Trim(tb[first])}`"));
        }

        lines.Add($"- **{Types.Count} distinct types** described by the files, compared with the {profile.Game} registry " +
                  $"({registry.SdkVersion}{(registry.Overlays.Count > 0 ? " + " + string.Join(", ", registry.Overlays) : "")}): " +
                  $"{same} identical, {hashOnly.Count} differ only in hash, {layout.Count} differ in layout, {missing.Count} not in the registry.");
        foreach (var (s, r) in hashOnly.Take(60))
            lines.Add($"  - hash only: `{s.Type.Identity}` file {s.Type.Hash} vs registry {r.Hash} ({s.Count} files, e.g. `{s.Example}`)");
        foreach (var (s, r, d) in layout.Take(60))
            lines.Add($"  - layout: `{s.Type.Identity}`: {d}; hash file {s.Type.Hash} vs registry {r.Hash} ({s.Count} files, e.g. `{s.Example}`)");
        foreach (var s in missing.Take(60))
            lines.Add($"  - missing: `{s.Type.Identity}` ({Trim(Describe(s.Type))}) ({s.Count} files, e.g. `{s.Example}`)");
        return lines;
    }

    private static string Trim(string s) => s.Length > 220 ? s[..220] + "…" : s;
}
