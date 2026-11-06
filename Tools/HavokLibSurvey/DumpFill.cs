using System.Xml.Linq;

/// <summary>
/// --filldumps: old packfiles only describe the classes their exporter didn't consider built in, so the file-built
/// class dumps of DS1 and DeS miss exactly the core ones (spline animation, MOPP and mesh shapes...). Each class a game
/// uses but never describes is borrowed from the nearest SDK that does, with its members laid out again for the target
/// (pointer size, the parents' sizes there, Itanium tail reuse for PS3). Borrowed dumps carry borrowed="...".
///
/// --filldumps also measures how far that can be trusted: the same relayout applied to classes the target does
/// describe must give the target's own layout. Writes Havok Library Survey/Dump Fill.md.
/// </summary>
public static class DumpFill
{
    public sealed record DumpSet(string Label, string Folder, int PointerSize, bool Itanium)
    {
        public Dictionary<string, XElement> Classes { get; } = Directory.Exists(Folder)
            ? Directory.GetFiles(Folder, "*.xml").Select(f => XElement.Parse(File.ReadAllText(f)))
                .Where(e => e.Attribute("borrowed") == null) // a previous fill's, not the files'
                .ToDictionary(e => e.Attribute("name")!.Value)
            : new();
    }

    private sealed record Layout(int Size, int Align, int DataSize, List<int> Offsets, bool Pod);

    /// <summary>
    /// Lays a dump class out under a set's rules. Struct members and parents come from <paramref name="lookup" />
    /// (the target's own classes first, so borrowed classes sit on the target's parents).
    /// </summary>
    private sealed class Relayout(int ps, bool itanium, Func<string, XElement?> lookup, Func<string, bool>? recorded = null)
    {
        private readonly Dictionary<string, Layout> _cache = new();
        public List<string> Problems { get; } = new();

        public Layout Of(string name)
        {
            if (_cache.TryGetValue(name, out var l)) return l;
            var cls = lookup(name) ?? throw new KeyNotFoundException($"no dump for {name}");
            _cache[name] = new Layout(0, 1, 0, new(), true); // cycle guard
            l = recorded != null && recorded(name) ? Recorded(cls) : Compute(cls);
            _cache[name] = l;
            return l;
        }

        // a class the target's files describe: their offsets and size as they are (they include what no member lists,
        // like a secondary base's vtable); the data size is the end of its last member
        private Layout Recorded(XElement cls)
        {
            var members = cls.Element("members")?.Elements("member").ToList() ?? new();
            string? parent = cls.Attribute("parent")?.Value;
            var p = parent == null ? null : Of(parent);
            int size = int.Parse(cls.Attribute("size")!.Value);
            int align = p?.Align ?? 1, end = p?.DataSize ?? 0;
            bool pod = p == null;
            var offsets = new List<int>();
            foreach (var m in members)
            {
                var (s, a, memberPod) = Member(m);
                int offset = int.Parse(m.Attribute("offset")!.Value);
                offsets.Add(offset);
                end = Math.Max(end, offset + s);
                align = Math.Max(align, a);
                pod &= memberPod;
            }
            if (parent == null && members.Count == 0) { end = size; pod = cls.Attribute("name")!.Value != "hkBaseObject" && size != ps; if (!pod) align = ps; }
            return new Layout(size, align, end, offsets, pod);
        }

        private Layout Compute(XElement cls)
        {
            string name = cls.Attribute("name")!.Value;
            var members = cls.Element("members")?.Elements("member").ToList() ?? new();
            string? parent = cls.Attribute("parent")?.Value;
            if (parent == null && members.Count == 0)
            {
                // hkBaseObject, and interfaces (hkpShapeContainer): just a vtable pointer
                int recordedSize = int.Parse(cls.Attribute("size")!.Value);
                bool vtable = name == "hkBaseObject" || recordedSize is 4 or 8;
                int size = vtable ? ps : recordedSize;
                return new Layout(size, vtable ? ps : 1, size, new(), !vtable);
            }

            var p = parent == null ? null : Of(parent);
            int cursor = p == null ? 0 : itanium && !p.Pod ? p.DataSize : p.Size;
            int align = p?.Align ?? 1;
            var offsets = new List<int>();
            bool pod = p == null;
            foreach (var m in members)
            {
                var (size, a, memberPod) = Member(m);
                if (m.Attribute("flags")!.Value.Contains("ALIGN_16")) a = Math.Max(a, 16);
                if (m.Attribute("flags")!.Value.Contains("ALIGN_8")) a = Math.Max(a, 8);
                int offset = AlignUp(cursor, a);
                offsets.Add(offset);
                cursor = offset + size;
                align = Math.Max(align, a);
                pod &= memberPod;
            }
            return new Layout(AlignUp(cursor, align), align, cursor, offsets, pod);
        }

        private (int Size, int Align, bool Pod) Member(XElement m)
        {
            string vtype = m.Attribute("vtype")!.Value, vsubtype = m.Attribute("vsubtype")!.Value;
            int count = Math.Max(1, int.Parse(m.Attribute("arrsize")!.Value));
            var (size, align, pod) = vtype switch
            {
                "TYPE_STRUCT" => Struct(m.Attribute("ctype")!.Value),
                "TYPE_ENUM" or "TYPE_FLAGS" => Scalar(vsubtype),
                _ => Scalar(vtype)
            };
            return (size * count, align, pod);
        }

        private (int, int, bool) Struct(string ctype)
        {
            var l = Of(ctype);
            return (l.Size, l.Align, l.Pod);
        }

        private (int, int, bool) Scalar(string vtype) => vtype switch
        {
            "TYPE_BOOL" or "TYPE_CHAR" or "TYPE_INT8" or "TYPE_UINT8" => (1, 1, true),
            "TYPE_INT16" or "TYPE_UINT16" or "TYPE_HALF" => (2, 2, true),
            "TYPE_INT32" or "TYPE_UINT32" or "TYPE_REAL" => (4, 4, true),
            "TYPE_INT64" or "TYPE_UINT64" => (8, 8, true),
            "TYPE_ULONG" or "TYPE_POINTER" or "TYPE_FUNCTIONPOINTER" or "TYPE_CSTRING" => (ps, ps, true),
            "TYPE_STRINGPTR" => (ps, ps, false),
            "TYPE_VECTOR4" or "TYPE_QUATERNION" => (16, 16, true),
            "TYPE_MATRIX3" or "TYPE_ROTATION" or "TYPE_QSTRANSFORM" => (48, 16, true),
            "TYPE_MATRIX4" or "TYPE_TRANSFORM" => (64, 16, true),
            "TYPE_ARRAY" => (ps + 8, ps, false),
            "TYPE_SIMPLEARRAY" => (ps + 4, ps, true),
            "TYPE_HOMOGENEOUSARRAY" => (2 * ps + 4, ps, true),
            "TYPE_RELARRAY" => (4, 2, true),
            "TYPE_VARIANT" => (2 * ps, ps, true),
            "TYPE_VOID" => (0, 1, true),
            _ => throw new InvalidDataException($"no size for {vtype}")
        };
    }

    private static int AlignUp(int v, int a) => a <= 1 ? v : (v + a - 1) / a * a;

    /// <summary>
    /// Whether a dump's own offsets and size are what <paramref name="layout" /> gives it.
    /// </summary>
    private static string? Compare(XElement cls, Layout layout)
    {
        var members = cls.Element("members")?.Elements("member").ToList() ?? new();
        var problems = new List<string>();
        int size = int.Parse(cls.Attribute("size")!.Value);
        if (layout.Size != size) problems.Add($"size {layout.Size} vs {size}");
        for (int i = 0; i < members.Count; i++)
        {
            int offset = int.Parse(members[i].Attribute("offset")!.Value);
            if (layout.Offsets[i] != offset) problems.Add($"{members[i].Attribute("name")!.Value}@{layout.Offsets[i]} vs {offset}");
        }
        return problems.Count == 0 ? null : string.Join(", ", problems.Take(4));
    }

    private static string MemberSignature(XElement cls) => string.Join(";",
        (cls.Element("members")?.Elements("member") ?? Enumerable.Empty<XElement>())
        .Select(m => $"{m.Attribute("name")!.Value}:{m.Attribute("vtype")!.Value}:{m.Attribute("vsubtype")!.Value}:{m.Attribute("ctype")?.Value}:{m.Attribute("arrsize")!.Value}"));

    public static List<string> Run(string dumpsRoot, string ds3Folder)
    {
        var lines = new List<string>
        {
            "# Class dump gap fill",
            "",
            $"Generated by `Tools/HavokLibSurvey --filldumps` on {DateTime.Now:yyyy-MM-dd HH:mm}. DS1's and DeS's packfiles describe only some of the classes they use; the rest are borrowed from another SDK and laid out again for the target. See the [Unified Havok Library Plan](../Unified%20Havok%20Library%20Plan.md), Phase 5.",
            ""
        };

        var ds3 = new DumpSet("DS3 hk_2014", ds3Folder, 8, false);
        var ds1 = new DumpSet("DS1 hk_2010", Path.Combine(dumpsRoot, "DS1_hk_2010.2.0-r1"), 4, false);
        var des55 = new DumpSet("DeS Havok-5.5", Path.Combine(dumpsRoot, "DES_Havok-5.5.0-r1"), 4, true);
        var des51 = new DumpSet("DeS Havok-5.1", Path.Combine(dumpsRoot, "DES_Havok-5.1.0-r1"), 4, true);

        // ---- self check: each set's own layouts recompute from its own members --------------------------------
        lines.Add("## 1. Each set's layouts recompute from its members");
        lines.Add("");
        foreach (var set in new[] { ds3, ds1, des55, des51 })
        {
            var r = new Relayout(set.PointerSize, set.Itanium, n => set.Classes.GetValueOrDefault(n));
            var bad = new List<string>();
            foreach (var (name, cls) in set.Classes.OrderBy(x => x.Key))
            {
                try { if (Compare(cls, r.Of(name)) is { } d) bad.Add($"`{name}`: {d}"); }
                catch (Exception ex) { bad.Add($"`{name}`: {ex.Message}"); }
            }
            lines.Add($"- **{set.Label}** ({set.PointerSize * 8}-bit{(set.Itanium ? ", Itanium" : "")}): {set.Classes.Count - bad.Count}/{set.Classes.Count} recompute exactly.");
            foreach (var b in bad.Take(15)) lines.Add($"  - {b}");
        }

        // (a set loaded before a fill ignores the borrowed dumps of earlier fills: DumpSet skips borrowed="...")
        // ---- borrowing: donors, in order of preference, for each target -------------------------------------------
        var plans = new[]
        {
            (Target: ds1, Donors: new[] { ds3 }),
            (Target: des55, Donors: new[] { des51, ds1, ds3 }), // the nearest version first
            (Target: des51, Donors: new[] { des55, ds1, ds3 })
        };

        lines.Add("");
        lines.Add("## 2. How well a donor predicts classes the target describes");
        lines.Add("");
        lines.Add("For classes both describe with the same members, the donor's class laid out with the target's rules (on the target's parents) against the target's own layout.");
        lines.Add("");
        foreach (var (target, donors) in plans)
            foreach (var donor in donors)
            {
                var r = new Relayout(target.PointerSize, target.Itanium, n => target.Classes.GetValueOrDefault(n) ?? donor.Classes.GetValueOrDefault(n));
                int same = 0, differentMembers = 0;
                var wrong = new List<string>();
                foreach (var (name, cls) in target.Classes.OrderBy(x => x.Key))
                {
                    if (!donor.Classes.TryGetValue(name, out var d)) continue;
                    if (MemberSignature(d) != MemberSignature(cls)) { differentMembers++; continue; }
                    // lay the donor's description out, with the target's parents
                    var rr = new Relayout(target.PointerSize, target.Itanium, n => n == name ? d : target.Classes.GetValueOrDefault(n) ?? donor.Classes.GetValueOrDefault(n), n => n != name && target.Classes.ContainsKey(n));
                    try
                    {
                        if (Compare(cls, rr.Of(name)) is { } diff) wrong.Add($"`{name}`: {diff}");
                        else same++;
                    }
                    catch (Exception ex) { wrong.Add($"`{name}`: {ex.Message}"); }
                }
                lines.Add($"- **{donor.Label} → {target.Label}**: {same} predicted exactly, {wrong.Count} wrong, {differentMembers} have different members.");
                foreach (var w in wrong.Take(10)) lines.Add($"  - {w}");
            }

        // ---- fill -------------------------------------------------------------------------------------------------
        lines.Add("");
        lines.Add("## 3. Borrowed classes");
        lines.Add("");
        foreach (var (target, donors) in plans)
        {
            string usedPath = Path.Combine(target.Folder, "_used.txt");
            // the classes the target's files use, with their signatures there (name, tab, signature)
            var usedLines = File.Exists(usedPath) ? File.ReadAllLines(usedPath).Select(l => l.Split('	')).ToList() : new();
            var used = usedLines.Select(p => p[0]).ToHashSet();
            var targetSignatures = usedLines.Where(p => p.Length > 1).ToDictionary(p => p[0], p => p[1]);
            var borrowed = new Dictionary<string, (XElement Class, DumpSet Donor)>();
            var missing = new List<string>();

            XElement? Find(string n) => target.Classes.GetValueOrDefault(n) ?? (borrowed.TryGetValue(n, out var b) ? b.Class : null);

            // the classes the files use, and those the target's own classes refer to (hand-made dumps refer to borrowed ones)
            var referenced = target.Classes.Values.SelectMany(c => c.Descendants("member").Select(m => m.Attribute("ctype")?.Value)
                .Append(c.Attribute("parent")?.Value)).Where(n => n != null).Select(n => n!);
            var queue = new Queue<string>(used.Concat(referenced).Distinct().Where(n => !target.Classes.ContainsKey(n)).OrderBy(n => n, StringComparer.Ordinal));
            while (queue.TryDequeue(out var name))
            {
                if (Find(name) != null) continue;
                var donor = donors.FirstOrDefault(d => d.Classes.ContainsKey(name));
                if (donor == null) { missing.Add(name); continue; }
                var copy = new XElement(donor.Classes[name]);
                borrowed[name] = (copy, donor);
                if (copy.Attribute("parent")?.Value is { } p) queue.Enqueue(p);
                foreach (var c in copy.Descendants("member").Select(m => m.Attribute("ctype")?.Value).Where(c => c != null)) queue.Enqueue(c!);
            }

            // a donor class whose own layout doesn't recompute (unlisted bytes) can't be moved with confidence
            var unreliable = new HashSet<string>();
            foreach (var (name, (cls, donor)) in borrowed)
            {
                var self = new Relayout(donor.PointerSize, donor.Itanium, n => donor.Classes.GetValueOrDefault(n));
                try { if (Compare(donor.Classes[name], self.Of(name)) != null) unreliable.Add(name); }
                catch { unreliable.Add(name); }
            }

            // borrowed dumps of an earlier fill that this one doesn't produce go to the archive (they would reach the registry)
            foreach (var file in Directory.GetFiles(target.Folder, "*.xml"))
            {
                var e = XElement.Parse(File.ReadAllText(file));
                if (e.Attribute("borrowed") == null || borrowed.ContainsKey(e.Attribute("name")!.Value)) continue;
                string archive = Path.Combine("..", "..", ".archive", "Havok Phase 5 stale borrowed dumps", Path.GetFileName(target.Folder));
                Directory.CreateDirectory(archive);
                File.Move(file, Path.Combine(archive, Path.GetFileName(file)), true);
            }

            // lay the borrowed classes out for the target, on the target's (and other borrowed) classes
            // the target's own classes, and borrowed ones kept verbatim, are laid out as recorded when others sit on them
            bool Verbatim(string n) => unreliable.Contains(n) && borrowed[n].Donor.PointerSize == target.PointerSize;
            var r = new Relayout(target.PointerSize, target.Itanium, Find, n => target.Classes.ContainsKey(n) || Verbatim(n));
            var written = new List<string>();
            foreach (var (name, (cls, donor)) in borrowed.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                try
                {
                    var members = cls.Element("members")?.Elements("member").ToList() ?? new();
                    bool moved = false;
                    // an unreliable donor layout of the same pointer size is kept as it is: relaying would lose its unlisted bytes
                    if (!(unreliable.Contains(name) && donor.PointerSize == target.PointerSize))
                    {
                        var l = r.Of(name);
                        moved = Compare(cls, l) != null;
                        for (int i = 0; i < members.Count; i++) members[i].Attribute("offset")!.Value = l.Offsets[i].ToString();
                        cls.Attribute("size")!.Value = l.Size.ToString();
                    }
                    cls.SetAttributeValue("borrowed", donor.Label);
                    cls.Attribute("signature")!.Value = targetSignatures.GetValueOrDefault(name, "0x00000000");
                    File.WriteAllText(Path.Combine(target.Folder, name + ".xml"), cls.ToString());
                    written.Add($"`{name}` ({donor.Label}{(moved ? ", relaid" : "")}{(unreliable.Contains(name) ? ", **donor layout has unlisted bytes**" : "")})");
                    if (unreliable.Contains(name)) cls.SetAttributeValue("unreliable", "true");
                }
                catch (Exception ex) { missing.Add($"{name} ({ex.Message})"); }
            }

            lines.Add($"- **{target.Label}**: {target.Classes.Count} described by the files, {written.Count} borrowed, {missing.Count} found nowhere.");
            if (written.Count > 0) lines.Add($"  - borrowed: {string.Join(", ", written)}");
            if (missing.Count > 0) lines.Add($"  - found nowhere: {string.Join(", ", missing.Select(m => $"`{m}`"))}");
        }

        return lines;
    }
}
