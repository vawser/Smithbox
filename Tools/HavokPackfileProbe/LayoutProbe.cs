using System.Buffers.Binary;
using System.Text;
using Havok;
using Havok.Formats.Packfile;
using Havok.Reflection;

/// <summary>
/// --layout: for a packfile with no type sections (BB), checks the profile's registry layouts against the file's
/// fixups. Every pointer member sits on a global fixup and every array/string member on a local fixup; for each class,
/// the fixups found inside its objects are compared with the offsets the registry gives. Also reports the read's
/// exception with its stack.
/// </summary>
public static class LayoutProbe
{
    private sealed record Section(uint Start, uint Local, uint Global, uint Virtual, uint Exports);

    public static void Run(byte[] data, HavokProfile profile, string name)
    {
        var registry = profile.GetRegistry()!;
        Console.WriteLine($"{name}: {data.Length} bytes, profile {profile.Game} ({string.Join(" + ", registry.Overlays.Prepend(registry.SdkVersion))})");

        try
        {
            var serializer = new HavokPackfileSerializer(Havok.hk2014.ClassSet.For(profile));
            serializer.Read(data);
            Console.WriteLine("read: OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"read: {ex.GetType().Name}: {ex.Message}");
            foreach (var line in (ex.StackTrace ?? "").Split('\n').Take(12)) Console.WriteLine("    " + line.Trim());
        }

        int sectionHeaders = 0x40 + BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(0x3E));
        Section ReadSection(int i)
        {
            int h = sectionHeaders + i * 0x40;
            uint U(int o) => BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(h + o));
            return new Section(U(20), U(24), U(28), U(32), U(36));
        }

        var classnames = ReadSection(0);
        var section = ReadSection(2);
        uint R(uint abs) => BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan((int)abs));

        string ClassName(uint offset)
        {
            int p = (int)(classnames.Start + offset);
            int end = Array.IndexOf(data, (byte)0, p);
            return Encoding.ASCII.GetString(data, p, end - p);
        }

        var objects = new SortedDictionary<uint, string>();
        for (uint q = section.Start + section.Virtual; q + 12 <= section.Start + section.Exports; q += 12)
        {
            uint src = R(q);
            if (src == 0xFFFFFFFF) continue;
            objects[src] = ClassName(R(q + 8));
        }

        var localMap = new Dictionary<uint, uint>();
        for (uint q = section.Start + section.Local; q + 8 <= section.Start + section.Global; q += 8)
            if (R(q) != 0xFFFFFFFF) localMap[R(q)] = R(q + 4);
        var local = localMap.Keys.ToHashSet();
        var global = new HashSet<uint>();
        for (uint q = section.Start + section.Global; q + 12 <= section.Start + section.Virtual; q += 12)
            if (R(q) != 0xFFFFFFFF) global.Add(R(q));

        Console.WriteLine($"{objects.Count} objects, {local.Count} local and {global.Count} global fixups");

        // walk every object and the elements of its arrays of records: a fixup the registry doesn't expect means a
        // member, or an element stride, is wrong
        var bad = new SortedDictionary<string, SortedSet<string>>();
        void Bad(string t, string what) => (bad.TryGetValue(t, out var s) ? s : bad[t] = new SortedSet<string>()).Add(what);
        var visited = new HashSet<(uint, string)>();
        void Walk(HavokType t, uint abs)
        {
            if (!visited.Add((abs, t.Identity))) return;
            var exp = new List<(int Offset, bool Global, string Name)>();
            Expected(t, 0, "", exp);
            var at = exp.GroupBy(e => e.Offset).ToDictionary(g => g.Key, g => g.First());
            for (uint o = abs; o < abs + t.Size; o += 4)
            {
                bool l = local.Contains(o), g = global.Contains(o);
                if (!l && !g) continue;
                int rel = (int)(o - abs);
                if (!at.TryGetValue(rel, out var e) || e.Global != g) Bad(t.Identity, $"{(g ? "global" : "local")}@{rel}");
            }

            void Arrays(HavokType rt, uint b)
            {
                foreach (var f in rt.Fields)
                {
                    uint p = b + (uint)f.Offset;
                    if (f.Type.Kind == HavokType.TypeKind.Record) Arrays(f.Type, p);
                    if (f.Type is { Kind: HavokType.TypeKind.Array, Name: "hkArray" or "hkSimpleArray", SubType: { Kind: HavokType.TypeKind.Record } el } &&
                        localMap.TryGetValue(p, out uint dst))
                    {
                        uint n = R(section.Start + p + 8);
                        if (dst + n * (uint)el.Size > section.Local) Bad(t.Identity, $"{f.Name}: {n} x {el.Identity} ({el.Size}) runs past the data");
                        else for (uint i = 0; i < n; i++) Walk(el, dst + i * (uint)el.Size);
                    }
                }
            }
            Arrays(t, abs);
        }

        // PROBE_DUMP=<class>: the first two objects of a class, in hex, 16 bytes a line
        if (Environment.GetEnvironmentVariable("PROBE_DUMP") is { Length: > 0 } dumpClass)
            foreach (var (start, _) in objects.Where(o => o.Value == dumpClass).Take(2))
            {
                int size = registry.GetType(dumpClass)?.Size ?? 64;
                Console.WriteLine($"{dumpClass} @0x{start:X}:");
                for (int o = 0; o < size + 64; o += 16)
                    Console.WriteLine($"  +{o,3}: {Convert.ToHexString(data, (int)(section.Start + start + o), 16)}");
            }

        foreach (var (start, cls) in objects)
            if (registry.GetType(cls) is { } ot) Walk(ot, start);
        Console.WriteLine(bad.Count == 0 ? "walk: every fixup is where the registry expects" : "walk: unexpected fixups by type:");
        foreach (var (t, s) in bad) Console.WriteLine($"  - {t}: {string.Join(", ", s.Take(12))}");

        var starts = objects.Keys.ToList();
        foreach (var group in objects.GroupBy(o => o.Value).OrderBy(g => g.Key))
        {
            var type = registry.GetType(group.Key);
            if (type == null) { Console.WriteLine($"- {group.Key}: not in the registry"); continue; }
            var expected = new List<(int Offset, bool Global, string Name)>();
            Expected(type, 0, "", expected);
            var expectedAt = expected.ToDictionary(e => e.Offset, e => e);

            var seenLocal = new SortedSet<int>();
            var seenGlobal = new SortedSet<int>();
            foreach (var (start, _) in group)
            {
                int i = starts.IndexOf(start);
                uint limit = Math.Min(start + (uint)type.Size, i + 1 < starts.Count ? starts[i + 1] : uint.MaxValue);
                for (uint o = start; o < limit; o += 4)
                {
                    if (local.Contains(o)) seenLocal.Add((int)(o - start));
                    if (global.Contains(o)) seenGlobal.Add((int)(o - start));
                }
            }

            var unexpected = seenLocal.Where(o => !expectedAt.TryGetValue(o, out var e) || e.Global).Select(o => $"local@{o}")
                .Concat(seenGlobal.Where(o => !expectedAt.TryGetValue(o, out var e) || !e.Global).Select(o => $"global@{o}")).ToList();
            string status = unexpected.Count == 0 ? "ok" : "MISMATCH";
            Console.WriteLine($"- {group.Key} ×{group.Count()} (size {type.Size}): {status}; fixups seen at local [{string.Join(",", seenLocal)}] global [{string.Join(",", seenGlobal)}]");
            if (unexpected.Count > 0)
            {
                Console.WriteLine($"    unexpected: {string.Join(", ", unexpected)}");
                Console.WriteLine($"    registry: {string.Join(", ", expected.Select(e => $"{e.Name}@{e.Offset}{(e.Global ? "g" : "l")}"))}");
            }
        }
    }

    /// <summary>
    /// The offsets a registry type's members put fixups at: global for pointers, local for arrays and strings.
    /// </summary>
    private static void Expected(HavokType type, int baseOffset, string prefix, List<(int, bool, string)> into)
    {
        foreach (var f in type.Fields)
        {
            int o = baseOffset + f.Offset;
            string n = prefix + f.Name;
            var t = f.Type;
            switch (t.Kind)
            {
                case HavokType.TypeKind.Pointer: into.Add((o, true, n)); break;
                case HavokType.TypeKind.String: into.Add((o, false, n)); break;
                case HavokType.TypeKind.Array when t.Name is "hkArray" or "hkSimpleArray": into.Add((o, false, n)); break;
                case HavokType.TypeKind.Array when t.Name == "T[N]" && t.SubType is { Kind: HavokType.TypeKind.Pointer or HavokType.TypeKind.String } pe:
                    for (int i = 0; i < (t.Format >> 8); i++) into.Add((o + i * 8, pe.Kind == HavokType.TypeKind.Pointer, $"{n}[{i}]"));
                    break;
                case HavokType.TypeKind.Array when t.Name == "T[N]" && t.SubType is { Kind: HavokType.TypeKind.Record } e:
                    for (int i = 0; i < (t.Format >> 8); i++) Expected(e, o + i * e.Size, $"{n}[{i}].", into);
                    break;
                case HavokType.TypeKind.Record: Expected(t, o, n + ".", into); break;
            }
        }
    }
}
