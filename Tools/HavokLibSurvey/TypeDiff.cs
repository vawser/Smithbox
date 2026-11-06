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

    // for --buildregistry: types seen only without a body, every hash seen, and bodies that disagree
    private static readonly ConcurrentDictionary<string, HavokType> Bodiless = new();
    private static readonly ConcurrentDictionary<string, uint> Hashes = new();
    private static readonly ConcurrentDictionary<string, string> Conflicts = new();

    public static void Reset()
    {
        Types.Clear();
        Bodiless.Clear();
        Hashes.Clear();
        Conflicts.Clear();
    }

    /// <param name="sdkPrefix">Only collect files of this SDK (e.g. "2016"); null for any</param>
    public static void Collect(byte[] data, SniffInfo sniff, string example, string sdkPrefix = null)
    {
        if (sniff.Container is not ("Tagfile" or "Compendium")) return;
        if (sdkPrefix != null && sniff.Container == "Tagfile" && !sniff.Sdk.StartsWith(sdkPrefix)) return;
        IReadOnlyList<HavokType> types;
        try { types = HavokTagfileSerializer.ReadTypeDescriptions(data); }
        catch { return; }
        foreach (var t in types)
        {
            if (t == null) continue;
            if (t.Hash is { } hash) Hashes.TryAdd(t.Identity, hash);
            if (t.Optionals == HavokType.Optional.None) // no body in this file
            {
                Bodiless.TryAdd(t.Identity, t);
                continue;
            }

            var seen = Types.GetOrAdd(t.Identity, _ => new Seen(t, example));
            Interlocked.Increment(ref seen.Count);
            if (!ReferenceEquals(seen.Type, t) && Describe(seen.Type) != Describe(t))
                Conflicts.TryAdd(t.Identity, $"`{seen.Example}` vs `{example}`");
        }
    }

    /// <summary>
    /// --buildoverlay: writes an overlay of the collected types on the profile's base registry (no overlays).
    /// </summary>
    public static List<string> WriteOverlay(string path, HavokProfile profile)
    {
        string basePath = Path.Join(HavokProfile.ResourceFolder, profile.RegistryFile!);
        var baseRegistry = HavokTypeRegistry.Load(basePath);
        return OverlayBuilder.Write(path, basePath, profile.Game,
            Types.ToDictionary(x => x.Key, x => x.Value.Type), Hashes, Describe, baseRegistry);
    }

    /// <summary>
    /// --buildregistry: writes every type collected as a registry XML in the HKLib format, with serialized optionals
    /// (root Optionals="Serialized") and property counts, so Havok.Core and the class generator can load it.
    /// Returns report lines.
    /// </summary>
    public static List<string> WriteRegistry(string path, string sdkVersion, string game)
    {
        // a body wins over a name-only description; every referenced type must be present
        var all = new Dictionary<string, HavokType>();
        foreach (var (identity, seen) in Types) all[identity] = seen.Type;
        foreach (var (identity, type) in Bodiless) all.TryAdd(identity, type);

        var pending = new Queue<HavokType>(all.Values);
        void AddReferenced(HavokType t)
        {
            if (t != null && all.TryAdd(t.Identity, t)) pending.Enqueue(t);
        }
        while (pending.TryDequeue(out var t))
        {
            AddReferenced(t.Parent);
            AddReferenced(t.SubType);
            foreach (var p in t.TemplateParameters) AddReferenced(p.Type);
            foreach (var f in t.Fields) AddReferenced(f.Type);
            foreach (var i in t.Interfaces) AddReferenced(i.Type);
        }

        var ordered = all.Values.OrderBy(t => t.Identity, StringComparer.Ordinal).ToList();
        var ids = ordered.Select((t, i) => (t.Identity, Id: i + 1)).ToDictionary(x => x.Identity, x => x.Id);
        int Id(HavokType t) => t == null ? 0 : ids[t.Identity];

        var types = new System.Xml.Linq.XElement("Types");
        foreach (var t in ordered)
        {
            var e = new System.Xml.Linq.XElement("HavokType",
                new System.Xml.Linq.XAttribute("Id", Id(t)),
                new System.Xml.Linq.XAttribute("Name", t.Name),
                new System.Xml.Linq.XAttribute("Parent", Id(t.Parent)),
                new System.Xml.Linq.XAttribute("Optionals", (int)t.Optionals),
                new System.Xml.Linq.XAttribute("Format", t.Format),
                new System.Xml.Linq.XAttribute("SubType", Id(t.SubType)),
                new System.Xml.Linq.XAttribute("Size", t.Size),
                // files set high bits on some alignments (0x3008, 0xF010); registries keep the alignment itself, as HKLib's do
                new System.Xml.Linq.XAttribute("Alignment", t.Alignment & 0xFFF),
                new System.Xml.Linq.XAttribute("Flags", (int)t.Flags),
                new System.Xml.Linq.XAttribute("Version", t.Version));
            if (Hashes.TryGetValue(t.Identity, out var hash)) e.Add(new System.Xml.Linq.XAttribute("Hash", hash));
            if (t.Properties.Count > 0) e.Add(new System.Xml.Linq.XAttribute("PropertyCount", t.Properties.Count));

            if (t.TemplateParameters.Count > 0)
                e.Add(new System.Xml.Linq.XElement("Template", t.TemplateParameters.Select(p =>
                    new System.Xml.Linq.XElement("Parameter",
                        new System.Xml.Linq.XAttribute("Name", p.Name),
                        new System.Xml.Linq.XAttribute("Kind", p.Type is null ? "Value" : "Type"),
                        new System.Xml.Linq.XAttribute("Value", p.Type is null ? p.Value ?? 0 : Id(p.Type))))));

            var ownFields = t.Fields.Skip(t.Parent?.Fields.Count ?? 0).ToList();
            if (ownFields.Count > 0)
                e.Add(new System.Xml.Linq.XElement("Members", ownFields.Select(f =>
                    new System.Xml.Linq.XElement("Member",
                        new System.Xml.Linq.XAttribute("Name", f.Name),
                        new System.Xml.Linq.XAttribute("Offset", f.Offset),
                        new System.Xml.Linq.XAttribute("Type", Id(f.Type)),
                        new System.Xml.Linq.XAttribute("Flags", (int)f.Flags)))));

            var ownInterfaces = t.Interfaces.Skip(t.Parent?.Interfaces.Count ?? 0).ToList();
            if (ownInterfaces.Count > 0)
                e.Add(new System.Xml.Linq.XElement("Interfaces", ownInterfaces.Select(i =>
                    new System.Xml.Linq.XElement("Interface",
                        new System.Xml.Linq.XAttribute("Type", Id(i.Type)),
                        new System.Xml.Linq.XAttribute("Offset", i.Offset)))));

            types.Add(e);
        }

        var root = new System.Xml.Linq.XElement("HavokTypeRegistry",
            new System.Xml.Linq.XAttribute("Version", sdkVersion),
            new System.Xml.Linq.XAttribute("Optionals", "Serialized"),
            new System.Xml.Linq.XAttribute("Game", game),
            new System.Xml.Linq.XComment($" Built by Tools/HavokLibSurvey --buildregistry from the type sections of {game}'s tagfiles and compendiums. No enum value names: files don't store them. "),
            types);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        root.Save(path);

        var lines = new List<string>
        {
            $"- Wrote `{Path.GetFileName(path)}`: {ordered.Count} types ({Types.Count} with bodies, {ordered.Count - Types.Count} name only), {Hashes.Count} hashes, {Conflicts.Count} conflicting descriptions."
        };
        foreach (var (identity, where) in Conflicts.Take(30)) lines.Add($"  - conflict: `{identity}`: {where}");
        return lines;
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
