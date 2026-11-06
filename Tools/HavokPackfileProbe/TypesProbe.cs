using System.Text;
using SoulsFormats;

/// <summary>
/// --types: prints a packfile's __types__ section (the hkClass reflection old packfiles carry): its objects by class,
/// and the first objects of each reflection class in hex with their fixups, to work out the hkClass layouts of an SDK.
/// </summary>
public static class TypesProbe
{
    public static void Run(byte[] data, string name)
    {
        bool bigEndian = data[0x11] == 0;
        var br = new BinaryReaderEx(bigEndian, data);
        int version = br.GetInt32(12);
        int pointerSize = data[0x10];
        int sectionCount = br.GetInt32(0x14);
        int sectionsStart = version >= 0x0B ? 0x40 + br.GetInt16(0x3E) : 0x40;
        int headerSize = version >= 0x0B ? 0x40 : 0x30;
        Console.WriteLine($"{name}: version {version}, {pointerSize * 8}-bit {(bigEndian ? "BE" : "LE")}, " +
                          $"{sectionCount} sections, SDK \"{br.GetASCII(0x28, 16).TrimEnd('\0')}\"");

        (string Tag, uint Start, uint Local, uint Global, uint Virtual, uint Exports) Section(int i)
        {
            int h = sectionsStart + i * headerSize;
            return (br.GetASCII(h, 19).TrimEnd('\0'), br.GetUInt32(h + 20), br.GetUInt32(h + 24), br.GetUInt32(h + 28),
                br.GetUInt32(h + 32), br.GetUInt32(h + 36));
        }

        var names = Section(0);
        int typesIndex = Enumerable.Range(0, sectionCount).First(i => Section(i).Tag == "__types__");
        var types = Section(typesIndex);
        Console.WriteLine($"__types__: {types.Local} bytes of data");

        string CStr(uint abs)
        {
            int end = Array.IndexOf(data, (byte)0, (int)abs);
            return Encoding.ASCII.GetString(data, (int)abs, end - (int)abs);
        }

        var local = new Dictionary<uint, uint>();
        for (uint q = types.Start + types.Local; q + 8 <= types.Start + types.Global; q += 8)
            if (br.GetUInt32(q) != 0xFFFFFFFF) local[br.GetUInt32(q)] = br.GetUInt32(q + 4);
        var global = new Dictionary<uint, (uint, uint)>();
        for (uint q = types.Start + types.Global; q + 12 <= types.Start + types.Virtual; q += 12)
            if (br.GetUInt32(q) != 0xFFFFFFFF) global[br.GetUInt32(q)] = (br.GetUInt32(q + 4), br.GetUInt32(q + 8));
        var objects = new SortedDictionary<uint, string>();
        for (uint q = types.Start + types.Virtual; q + 12 <= types.Start + types.Exports; q += 12)
            if (br.GetUInt32(q) != 0xFFFFFFFF) objects[br.GetUInt32(q)] = CStr(names.Start + br.GetUInt32(q + 8));

        foreach (var g in objects.GroupBy(o => o.Value)) Console.WriteLine($"  {g.Key}: {g.Count()}");

        var starts = objects.Keys.ToList();
        foreach (var cls in new[] { "hkClass", "hkClassMember", "hkClassEnum", "hkClassEnumItem" })
        {
            foreach (var start in objects.Where(o => o.Value == cls).Select(o => o.Key).Take(2))
            {
                int idx = starts.IndexOf(start);
                uint end = idx + 1 < starts.Count ? starts[idx + 1] : types.Local;
                Console.WriteLine($"{cls} @0x{start:X} (next object at +{end - start}):");
                for (uint o = 0; o < Math.Min(end - start, 96); o += 16)
                    Console.WriteLine($"  +{o,3}: {Convert.ToHexString(data, (int)(types.Start + start + o), 16)}");
                for (uint o = start; o < Math.Min(end, start + 96); o += 4)
                {
                    if (local.TryGetValue(o, out uint dst))
                        Console.WriteLine($"    local  +{o - start,3} -> 0x{dst:X} \"{CStr(types.Start + dst)}\"");
                    if (global.TryGetValue(o, out var gd))
                        Console.WriteLine($"    global +{o - start,3} -> section {gd.Item1} 0x{gd.Item2:X}" +
                                          (gd.Item1 == typesIndex && objects.TryGetValue(gd.Item2, out var t) ? $" ({t})" : ""));
                }
            }
        }
    }
}
