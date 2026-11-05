using System.Text;

/// <summary>
/// Joins the fields the exe reads with the SoulsFormats properties at those positions
/// </summary>
public static class Report
{
    // the translation unit of each MSB wrapper, from its vtable functions; the part types its type data belongs to
    static readonly (ulong Lo, ulong Hi, string Unit, string[] Entries)[] Units =
    {
        (0x140CEF000, 0x140CF1500, "CSMsbParts", null),
        (0x140CF1500, 0x140CF22C0, "CSMsbPartsEne", new[] { "Part.Enemy", "Part.DummyEnemy" }),
        (0x140CF22C0, 0x140CF4D50, "CSMsbPartsGeom", new[] { "Part.Asset", "Part.DummyAsset" }),
        (0x140CF4D50, 0x140CF52F0, "CSMsbPartsHit", new[] { "Part.Collision", "Part.ConnectCollision" }),
        (0x140CF52F0, 0x140CF5560, "CSMsbPartsMap", new[] { "Part.MapPiece" }),
        (0x140CF5560, 0x140CF5600, "CSMsbPartsPlayer", new[] { "Part.Player" }),
        (0x140CF5600, 0x140CF6300, "CSMsbEvent", null),
        (0x140CF6300, 0x140CFBD00, "MsbResCap", null),
        (0x140CFBD00, 0x140D10000, "CSMsbPoint", null),
    };

    public static string UnitOf(ulong fn) => Units.FirstOrDefault(u => fn >= u.Lo && fn < u.Hi).Unit ?? "game";

    public static void Write(string dir)
    {
        var layout = Layout.Build();
        var sb = new StringBuilder();
        var read = new HashSet<(string Entry, string Path)>();

        foreach (var g in Flow.Accesses.Where(a => !a.Write).GroupBy(a => (a.Kind, a.Off)).OrderBy(g => g.Key.Kind).ThenBy(g => g.Key.Off))
        {
            var (kind, off) = g.Key;
            var units = g.Select(a => UnitOf(a.Func)).Distinct().ToList();
            var entries = units.SelectMany(u => Units.FirstOrDefault(x => x.Unit == u).Entries ?? Array.Empty<string>()).Distinct().ToList();

            var cands = layout.Where(f => f.Kind == kind && off >= f.Off && off < f.Off + Math.Max(1, f.Size)).ToList();
            // type data differs per type: keep the types of the units that read it, when they say
            if (kind.StartsWith("Part.Type") && entries.Count > 0 && cands.Any(c => entries.Contains(c.Entry)))
                cands = cands.Where(c => entries.Contains(c.Entry)).ToList();
            foreach (var c in cands) read.Add((c.Entry, c.Path));

            var names = cands.GroupBy(c => c.Path).Select(c => kind.Contains("Type") ? $"{c.Key} [{string.Join(",", c.Select(x => x.Entry.Split('.')[1]).Distinct())}]" : c.Key);
            var uses = g.SelectMany(a => a.Uses).GroupBy(u => u).OrderByDescending(u => u.Count()).Select(u => u.Count() > 1 ? $"{u.Key} x{u.Count()}" : u.Key);
            var fns = g.Select(a => a.Func).Distinct().ToList();
            var sizes = string.Join("/", g.Select(a => a.Size).Distinct());
            sb.AppendLine($"{kind}\t0x{off:X}\t{sizes}\t{string.Join(" | ", names)}\t{string.Join(",", units)}\t{fns.Count}\t{string.Join(" ", fns.Take(6).Select(f => f.ToString("X")))}\t{string.Join("; ", uses.Take(10))}");
        }
        File.WriteAllText(Path.Combine(dir, "report.tsv"), sb.ToString());

        // the unknowns: read by the exe or not
        sb.Clear();
        foreach (var f in layout.Where(f => f.Path.Contains("Unk", StringComparison.OrdinalIgnoreCase)).OrderBy(f => f.Entry).ThenBy(f => f.Kind).ThenBy(f => f.Off))
            sb.AppendLine($"{f.Entry}\t{f.Kind}\t0x{f.Off:X}\t{f.Path}\t{(read.Contains((f.Entry, f.Path)) ? "READ" : "-")}");
        File.WriteAllText(Path.Combine(dir, "unknowns.tsv"), sb.ToString());
        File.WriteAllLines(Path.Combine(dir, "layout.tsv"), layout.Select(f => $"{f.Entry}\t{f.Kind}\t0x{f.Off:X}\t{f.Size}\t{f.Path}\t{f.Type}"));
        Getters(dir, layout);
        Mismatches(dir, layout);
    }

    /// <summary>
    /// Reads that do not line up with a SoulsFormats property: no property there (SoulsFormats asserts or pads it), or
    /// a read narrower than the property and inside it (the property is several fields)
    /// </summary>
    static void Mismatches(string dir, List<Layout.Field> layout)
    {
        var sb = new StringBuilder();
        var ptrs = new HashSet<(string, int)>();
        foreach (var g in Flow.Accesses.Where(a => !a.Write && !a.Text.Contains("[idx]")).GroupBy(a => (a.Kind, a.Off, a.Size)).OrderBy(g => g.Key.Kind).ThenBy(g => g.Key.Off))
        {
            var (kind, off, size) = g.Key;
            if (kind is "Part" or "Point" or "Event" or "Model" || size == 0) continue; // headers hold the sub-struct pointers
            if (kind == "Part.Type" && size == 8 && off >= 0x68) continue;
            var units = g.Select(a => UnitOf(a.Func)).Distinct().ToList();
            var entries = units.SelectMany(u => Units.FirstOrDefault(x => x.Unit == u).Entries ?? Array.Empty<string>()).Distinct().ToList();
            var fields = layout.Where(f => f.Kind == kind && (!kind.StartsWith("Part.Type") || entries.Count == 0 || entries.Contains(f.Entry))).ToList();
            if (fields.Count == 0) continue;
            var hit = fields.Where(f => off >= f.Off && off < f.Off + Math.Max(1, f.Size)).ToList();
            var where = string.Join(",", units);
            var who = entries.Count > 0 ? string.Join(",", entries) : "";
            if (hit.Count == 0)
                sb.AppendLine($"NO PROPERTY\t{kind}\t0x{off:X}\t{size}\t{who}\t{where}\t{string.Join("; ", g.SelectMany(a => a.Uses).Distinct().Take(6))}\t{g.First().Func:X}");
            else if (hit.All(f => size < f.Size && (off != f.Off || size < f.Size)))
                sb.AppendLine($"SPLIT\t{kind}\t0x{off:X}\t{size}\t{who}\t{where}\tinside {string.Join("/", hit.Select(h => $"{h.Path}({h.Size})").Distinct())}\t{g.First().Func:X}");
        }
        File.WriteAllText(Path.Combine(dir, "mismatches.tsv"), sb.ToString());
    }

    /// <summary>
    /// The small MSB functions that read a field or two, and who calls them: the caller's class when it is a virtual
    /// function, and the strings it references
    /// </summary>
    static void Getters(string dir, List<Layout.Field> layout)
    {
        string Name(Flow.Access a)
        {
            var entries = Units.FirstOrDefault(x => x.Unit == UnitOf(a.Func)).Entries;
            var c = layout.Where(f => f.Kind == a.Kind && a.Off >= f.Off && a.Off < f.Off + Math.Max(1, f.Size)
                                      && (!a.Kind.StartsWith("Part.Type") || entries == null || entries.Contains(f.Entry))).Select(f => f.Path).Distinct().Take(3);
            return $"{a.Kind}+{a.Off:X}({string.Join("/", c)})";
        }

        var getters = Flow.Accesses.Where(a => !a.Write && UnitOf(a.Func) != "game").GroupBy(a => a.Func)
            .Where(g => g.Select(a => (a.Kind, a.Off)).Distinct().Count() <= 4)
            .ToDictionary(g => g.Key, g => string.Join(" ", g.Select(Name).Distinct()));

        var calls = new Dictionary<ulong, List<ulong>>();
        foreach (var ins in Exe.Sweep())
            if ((ins.FlowControl == Iced.Intel.FlowControl.Call || ins.FlowControl == Iced.Intel.FlowControl.UnconditionalBranch)
                && ins.Op0Kind == Iced.Intel.OpKind.NearBranch64 && getters.ContainsKey(ins.NearBranchTarget))
            {
                if (!calls.TryGetValue(ins.NearBranchTarget, out var l)) calls[ins.NearBranchTarget] = l = new();
                l.Add(ins.IP);
            }

        var sb = new StringBuilder();
        foreach (var (fn, fields) in getters.OrderBy(k => k.Key))
        {
            var sites = calls.GetValueOrDefault(fn) ?? new();
            sb.AppendLine($"{fn:X}\t{UnitOf(fn)}\t{fields}\t{sites.Count} callers");
            foreach (var site in sites.Take(12))
            {
                var caller = Extra.FuncStart(site);
                var cls = Exe.VirtualOf.TryGetValue(caller, out var v) ? $"{v[0].Class}#{v[0].Index}" : "";
                var strs = new List<string>();
                foreach (var ins in Exe.Function(caller, 1500))
                {
                    var t = Exe.RefTarget(ins);
                    if (t == 0 || ins.Op0Kind == Iced.Intel.OpKind.NearBranch64) continue;
                    var s = Exe.ReadString(t);
                    if (s != null && s.Length > 3 && !strs.Contains(s)) strs.Add(s);
                    if (strs.Count >= 4) break;
                }
                sb.AppendLine($"    {site:X} in {caller:X} {UnitOf(caller)} {cls} {string.Join(" | ", strs)}");
            }
        }
        File.WriteAllText(Path.Combine(dir, "getters.tsv"), sb.ToString());
    }
}
