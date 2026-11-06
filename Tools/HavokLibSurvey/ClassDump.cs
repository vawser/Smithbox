using System.Collections.Concurrent;
using System.Text;
using System.Xml.Linq;
using SoulsFormats;

/// <summary>
/// --dumpclasses: the hkClass reflection old packfiles carry in their __types__ section (DeS Havok-5.x; DS1 hk_2010 chr
/// physics files), written as class dumps in the format of HKX2's classxmlds3 (one XML per class), so
/// Tools/HavokClassDumpConvert can turn them into a registry. Collected over every surveyed file of an SDK: a class is
/// kept the first time it's seen, and any later file that describes it differently is recorded as a conflict.
///
/// The reflection layout (32-bit, the same in Havok 5.5 and hk_2010):
///   hkClass (48):         name*, parent*, objectSize, numInterfaces, enums*, numEnums, members*, numMembers,
///                         defaults*, attributes*, flags, describedVersion
///   hkClassMember (24):   name*, class*, enum*, u8 type, u8 subtype, i16 cArraySize, u16 flags, u16 offset, attributes*
///   hkClassEnum (20):     name*, items*, numItems, attributes*, flags
///   hkClassEnumItem (8):  value, name*
/// </summary>
public static class ClassDump
{
    // hkClassMember::Type, as hk_2010 numbers it (Havok 5.5 is the same up to TYPE_FLAGS)
    private static readonly string[] TypeNames =
    [
        "TYPE_VOID", "TYPE_BOOL", "TYPE_CHAR", "TYPE_INT8", "TYPE_UINT8", "TYPE_INT16", "TYPE_UINT16", "TYPE_INT32",
        "TYPE_UINT32", "TYPE_INT64", "TYPE_UINT64", "TYPE_REAL", "TYPE_VECTOR4", "TYPE_QUATERNION", "TYPE_MATRIX3",
        "TYPE_ROTATION", "TYPE_QSTRANSFORM", "TYPE_MATRIX4", "TYPE_TRANSFORM", "TYPE_ZERO", "TYPE_POINTER",
        "TYPE_FUNCTIONPOINTER", "TYPE_ARRAY", "TYPE_INPLACEARRAY", "TYPE_ENUM", "TYPE_STRUCT", "TYPE_SIMPLEARRAY",
        "TYPE_HOMOGENEOUSARRAY", "TYPE_VARIANT", "TYPE_CSTRING", "TYPE_ULONG", "TYPE_FLAGS", "TYPE_HALF",
        "TYPE_STRINGPTR", "TYPE_RELARRAY"
    ];

    private static readonly (int Bit, string Name)[] MemberFlags =
        [(8, "DEPRECATED_SIZE_8"), (128, "ALIGN_8"), (256, "ALIGN_16"), (512, "NOT_OWNED"), (1024, "SERIALIZE_IGNORED")];

    private sealed record Described(XElement Class, string Example);

    // SDK -> class name -> first description
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Described>> Classes = new();
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, uint>> Signatures = new();
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Conflicts = new();
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, int>> UsedClasses = new();
    private static readonly ConcurrentDictionary<string, string> Errors = new();

    public static void Reset()
    {
        Classes.Clear();
        Signatures.Clear();
        Conflicts.Clear();
        UsedClasses.Clear();
        Errors.Clear();
    }

    public static void Collect(byte[] data, SniffInfo sniff, string example)
    {
        if (sniff.Container != "Packfile" || sniff.PointerSize != 4) return;
        try { CollectFile(data, sniff.Sdk, example); }
        catch (Exception ex) { Errors.TryAdd($"{ex.GetType().Name}: {ex.Message}", example); }
    }

    private static void CollectFile(byte[] data, string sdk, string example)
    {
        bool be = data[0x11] == 0;
        var br = new BinaryReaderEx(be, data);
        int version = br.GetInt32(12);
        int sectionCount = br.GetInt32(0x14);
        int sectionsStart = version >= 0x0B ? 0x40 + br.GetInt16(0x3E) : 0x40;
        int headerSize = version >= 0x0B ? 0x40 : 0x30;

        (string Tag, uint Start, uint Local, uint Global, uint Virtual, uint Exports) Section(int i)
        {
            int h = sectionsStart + i * headerSize;
            return (br.GetASCII(h, 19).TrimEnd('\0'), br.GetUInt32(h + 20), br.GetUInt32(h + 24), br.GetUInt32(h + 28),
                br.GetUInt32(h + 32), br.GetUInt32(h + 36));
        }

        string CStr(uint abs)
        {
            int end = Array.IndexOf(data, (byte)0, (int)abs);
            return Encoding.ASCII.GetString(data, (int)abs, end - (int)abs);
        }

        // class signatures and the classes used, from the class-name section
        var names = Section(0);
        var signatures = Signatures.GetOrAdd(sdk, _ => new());
        var used = UsedClasses.GetOrAdd(sdk, _ => new());
        for (uint p = names.Start; p + 5 < names.Start + names.Local && br.GetUInt32(p) != 0xFFFFFFFF;)
        {
            uint signature = br.GetUInt32(p);
            string name = CStr(p + 5);
            signatures.TryAdd(name, signature);
            used.AddOrUpdate(name, 1, (_, n) => n + 1);
            p += 5 + (uint)name.Length + 1;
        }

        int typesIndex = Enumerable.Range(0, sectionCount).FirstOrDefault(i => Section(i).Tag == "__types__", -1);
        if (typesIndex < 0) return;
        var types = Section(typesIndex);
        if (types.Local == 0) return;

        var local = new Dictionary<uint, uint>();
        for (uint q = types.Start + types.Local; q + 8 <= types.Start + types.Global; q += 8)
            if (br.GetUInt32(q) != 0xFFFFFFFF) local[br.GetUInt32(q)] = br.GetUInt32(q + 4);
        var global = new Dictionary<uint, uint>();
        for (uint q = types.Start + types.Global; q + 12 <= types.Start + types.Virtual; q += 12)
            if (br.GetUInt32(q) != 0xFFFFFFFF) global[br.GetUInt32(q)] = br.GetUInt32(q + 8);
        var objects = new Dictionary<uint, string>();
        for (uint q = types.Start + types.Virtual; q + 12 <= types.Start + types.Exports; q += 12)
            if (br.GetUInt32(q) != 0xFFFFFFFF) objects[br.GetUInt32(q)] = CStr(names.Start + br.GetUInt32(q + 8));

        uint U(uint rel) => br.GetUInt32(types.Start + rel);
        string? Str(uint rel) => local.TryGetValue(rel, out uint dst) ? CStr(types.Start + dst) : null;
        string? ClassAt(uint rel) => global.TryGetValue(rel, out uint dst) ? Str(dst) : null; // hkClass.name is at +0

        XElement Enum(uint e)
        {
            var element = new XElement("enum", new XAttribute("name", Str(e) ?? ""), new XAttribute("flags", U(e + 16)));
            if (local.TryGetValue(e + 4, out uint items))
                for (uint i = 0; i < U(e + 8); i++)
                    element.Add(new XElement("enumitem",
                        new XAttribute("name", Str(items + i * 8 + 4) ?? ""),
                        new XAttribute("value", (int)U(items + i * 8))));
            return element;
        }

        var described = Classes.GetOrAdd(sdk, _ => new());
        var conflicts = Conflicts.GetOrAdd(sdk, _ => new());
        foreach (var (c, cls) in objects)
        {
            if (cls != "hkClass") continue;
            string name = Str(c) ?? throw new InvalidDataException($"hkClass at 0x{c:X} has no name");
            var element = new XElement("class",
                new XAttribute("name", name),
                new XAttribute("version", (int)U(c + 44)),
                new XAttribute("signature", "0x" + (signatures.TryGetValue(name, out var sig) ? sig : 0).ToString("x8")),
                new XAttribute("size", (int)U(c + 8)));
            if (ClassAt(c + 4) is { } parent) element.Add(new XAttribute("parent", parent));
            if (U(c + 12) != 0) element.Add(new XAttribute("interfaces", U(c + 12)));
            if (U(c + 40) != 0) element.Add(new XAttribute("flags", U(c + 40)));

            var enums = new XElement("enums");
            if (local.TryGetValue(c + 16, out uint enumData))
                for (uint i = 0; i < U(c + 20); i++) enums.Add(Enum(enumData + i * 20));
            if (enums.HasElements) element.Add(enums);

            var members = new XElement("members");
            if (local.TryGetValue(c + 24, out uint memberData))
                for (uint i = 0; i < U(c + 28); i++)
                {
                    uint m = memberData + i * 24;
                    byte type = data[types.Start + m + 12], subtype = data[types.Start + m + 13];
                    short arrsize = br.GetInt16(types.Start + m + 14);
                    ushort flags = br.GetUInt16(types.Start + m + 16), offset = br.GetUInt16(types.Start + m + 18);
                    var member = new XElement("member",
                        new XAttribute("name", Str(m) ?? ""),
                        new XAttribute("offset", offset));
                    if (ClassAt(m + 4) is { } ctype) member.Add(new XAttribute("ctype", ctype));
                    if (global.TryGetValue(m + 8, out uint enumAt)) member.Add(new XAttribute("etype", Str(enumAt) ?? ""));
                    member.Add(new XAttribute("vtype", TypeName(type)), new XAttribute("vsubtype", TypeName(subtype)),
                        new XAttribute("arrsize", arrsize),
                        new XAttribute("flags", FlagNames(flags)));
                    members.Add(member);
                }
            if (members.HasElements) element.Add(members);

            var first = described.GetOrAdd(name, _ => new Described(element, example));
            if (!ReferenceEquals(first.Class, element) && !SameDescription(first.Class, element))
                conflicts.TryAdd(name, $"`{first.Example}` vs `{example}`");
        }
    }

    private static string TypeName(byte t) => t < TypeNames.Length ? TypeNames[t] : $"TYPE_{t}";

    private static string FlagNames(ushort flags)
    {
        var names = MemberFlags.Where(f => (flags & f.Bit) != 0).Select(f => f.Name).ToList();
        int rest = flags & ~MemberFlags.Sum(f => f.Bit);
        if (rest != 0) names.Add($"0x{rest:X}");
        return names.Count == 0 ? "FLAGS_NONE" : string.Join("|", names);
    }

    // signatures differ when a file doesn't instantiate the class (0), so they don't count
    private static bool SameDescription(XElement a, XElement b)
    {
        var x = new XElement(a); var y = new XElement(b);
        x.Attribute("signature")!.Remove(); y.Attribute("signature")!.Remove();
        return XNode.DeepEquals(x, y);
    }

    /// <summary>
    /// Writes each SDK's classes to &lt;outDir&gt;/&lt;sdk&gt;/&lt;class&gt;.xml. Returns report lines.
    /// </summary>
    public static List<string> Write(string outDir, string game)
    {
        var lines = new List<string>();
        foreach (var (sdk, classes) in Classes.OrderBy(x => x.Key))
        {
            string folder = Path.Combine(outDir, game + "_" + new string(sdk.Where(ch => char.IsLetterOrDigit(ch) || ch is '.' or '-' or '_').ToArray()));
            Directory.CreateDirectory(folder);
            var signatures = Signatures[sdk];
            foreach (var (name, d) in classes)
            {
                var element = new XElement(d.Class);
                if (signatures.TryGetValue(name, out var sig)) element.Attribute("signature")!.Value = "0x" + sig.ToString("x8");
                File.WriteAllText(Path.Combine(folder, name + ".xml"), element.ToString());
            }

            var used = UsedClasses[sdk].Keys.Where(n => !n.StartsWith("hkClass")).ToList();
            File.WriteAllLines(Path.Combine(folder, "_used.txt"), used.OrderBy(n => n, StringComparer.Ordinal)
                .Select(n => $"{n}	0x{(signatures.TryGetValue(n, out var s) ? s : 0):x8}"));
            var undescribed = used.Where(n => !classes.ContainsKey(n)).OrderBy(n => n).ToList();
            var vtypes = classes.Values.SelectMany(d => d.Class.Descendants("member"))
                .SelectMany(m => new[] { m.Attribute("vtype")!.Value, m.Attribute("vsubtype")!.Value })
                .GroupBy(v => v).OrderBy(g => g.Key).Select(g => $"{g.Key} {g.Count()}");
            lines.Add($"- **{sdk}**: {classes.Count} classes described, written to `{Path.GetFileName(folder)}`. " +
                      $"{used.Count - undescribed.Count}/{used.Count} classes used by the files are described.");
            if (undescribed.Count > 0) lines.Add($"  - used but never described: {string.Join(", ", undescribed.Select(n => $"`{n}`"))}");
            lines.Add($"  - member types: {string.Join(", ", vtypes)}");
            if (Conflicts.TryGetValue(sdk, out var conflicts))
                foreach (var (name, where) in conflicts.Take(20)) lines.Add($"  - conflict: `{name}` ({where})");
        }
        foreach (var (error, example) in Errors.Take(10)) lines.Add($"- error: {error} (e.g. `{example}`)");
        return lines;
    }
}
