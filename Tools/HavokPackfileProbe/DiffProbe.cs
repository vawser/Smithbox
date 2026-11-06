using System.Text;
using Havok;
using Havok.Formats.Packfile;
using SoulsFormats;

/// <summary>
/// --diff: reads a packfile with the unified engine (the class set its SDK needs), writes it, and compares the two:
/// each section's part sizes, the first differing byte of each part, and the object of the data section it falls in.
/// </summary>
public static class DiffProbe
{
    public static void Run(byte[] original, string game, string name)
    {
        string sdk = Encoding.ASCII.GetString(original, 0x28, 16).TrimEnd('\0', '\xFF');
        var profile = HavokProfile.For(game, sdk)!;
        var classSet = profile.Sdk switch
        {
            HavokSdk.Hk2010 => Havok.hk2010.ClassSet.For(profile),
            HavokSdk.Hk5x => Havok.hk55.ClassSet.For(profile),
            _ => Havok.hk2014.ClassSet.For(profile)
        };
        var serializer = new HavokPackfileSerializer(classSet);
        byte[] written = serializer.Write(serializer.Read(original));
        Console.WriteLine($"{name} ({sdk}): original {original.Length} bytes, written {written.Length}");
        var ga = Globals(original); var gb = Globals(written);
        Console.WriteLine($"  global fixups: {GlobalRows(original).Count} rows in the original, {GlobalRows(written).Count} written");
        foreach (var row in GlobalRows(original).GroupBy(r => r).Where(g => g.Count() > 1).Take(5)) Console.WriteLine($"  original row repeated {row.Count()}x: {row.Key}");
        var objectsA = Parse(original).Objects(original, 2);
        string In(uint at) => objectsA.Where(o => o.Key <= at).OrderByDescending(o => o.Key).Select(o => $"{o.Value}x{o.Key:X}+{at - o.Key}").FirstOrDefault() ?? "?";
        foreach (var src in ga.Keys.Except(gb.Keys).Take(10)) Console.WriteLine($"  global fixup 0x{src:X} ({In(src)}) -> 0x{ga[src]:X} ({In(ga[src])}) is not written");
        foreach (var src in gb.Keys.Except(ga.Keys).Take(10)) Console.WriteLine($"  global fixup 0x{src:X} -> 0x{gb[src]:X} is new");

        var a = Parse(original);
        var b = Parse(written);
        int header = Math.Min(a.SectionsStart, b.SectionsStart);
        int h = First(original, written, 0, header);
        if (h >= 0) Console.WriteLine($"  header differs at 0x{h:X}");

        for (int i = 0; i < Math.Min(a.Sections.Count, b.Sections.Count); i++)
        {
            var (sa, sb) = (a.Sections[i], b.Sections[i]);
            Console.WriteLine($"  {sa.Tag}: data {sa.Local}/{sb.Local}, local {sa.Global - sa.Local}/{sb.Global - sb.Local}, " +
                              $"global {sa.Virtual - sa.Global}/{sb.Virtual - sb.Global}, virtual {sa.Exports - sa.Virtual}/{sb.Exports - sb.Virtual}");
            foreach (var (part, from, to) in new[] { ("data", 0u, Math.Min(sa.Local, sb.Local)), ("local", sa.Local, sa.Global), ("global", sa.Global, sa.Virtual), ("virtual", sa.Virtual, sa.Exports) })
            {
                uint offB = part switch { "data" => 0u, "local" => sb.Local, "global" => sb.Global, _ => sb.Virtual };
                int len = (int)Math.Min(to - from, (part switch { "data" => sb.Local, "local" => sb.Global, "global" => sb.Virtual, _ => sb.Exports }) - offB);
                int d = First(original, written, (int)(sa.Start + from), len, (int)(sb.Start + offB));
                if (d < 0) continue;
                string where = "";
                if (part == "data")
                {
                    var obj = a.Objects(original, i).Where(o => o.Key <= d).OrderByDescending(o => o.Key).FirstOrDefault();
                    if (obj.Value != null) where = $" ({obj.Value} at 0x{obj.Key:X}, +{d - obj.Key})";
                }
                Console.WriteLine($"    {part} differs at +0x{d:X}{where}: {Convert.ToHexString(original, (int)(sa.Start + from + d), Math.Min(16, len - d))} vs {Convert.ToHexString(written, (int)(sb.Start + offB + d), Math.Min(16, len - d))}");
            }
        }
    }

    private static List<string> GlobalRows(byte[] data)
    {
        var l = Parse(data);
        var s = l.Sections[2];
        var rows = new List<string>();
        for (uint q = s.Start + s.Global; q + 12 <= s.Start + s.Virtual; q += 12)
            rows.Add($"{l.Reader.GetUInt32(q):X}:{l.Reader.GetUInt32(q + 4)}:{l.Reader.GetUInt32(q + 8):X}");
        return rows;
    }

    private static Dictionary<uint, uint> Globals(byte[] data)
    {
        var l = Parse(data);
        var s = l.Sections[2];
        var result = new Dictionary<uint, uint>();
        for (uint q = s.Start + s.Global; q + 12 <= s.Start + s.Virtual; q += 12)
            if (l.Reader.GetUInt32(q) != 0xFFFFFFFF) result[l.Reader.GetUInt32(q)] = l.Reader.GetUInt32(q + 8);
        return result;
    }

    private static int First(byte[] a, byte[] b, int start, int length, int startB = -1)
    {
        if (startB < 0) startB = start;
        for (int i = 0; i < length; i++)
            if (start + i >= a.Length || startB + i >= b.Length || a[start + i] != b[startB + i]) return i;
        return -1;
    }

    private sealed record Section(string Tag, uint Start, uint Local, uint Global, uint Virtual, uint Exports);

    private sealed record Layout(int SectionsStart, List<Section> Sections, BinaryReaderEx Reader)
    {
        public Dictionary<uint, string> Objects(byte[] data, int sectionIndex)
        {
            var s = Sections[sectionIndex];
            var names = Sections[0];
            var result = new Dictionary<uint, string>();
            for (uint q = s.Start + s.Virtual; q + 12 <= s.Start + s.Exports; q += 12)
            {
                if (Reader.GetUInt32(q) == 0xFFFFFFFF) continue;
                int p = (int)(names.Start + Reader.GetUInt32(q + 8));
                result[Reader.GetUInt32(q)] = Encoding.ASCII.GetString(data, p, Array.IndexOf(data, (byte)0, p) - p);
            }
            return result;
        }
    }

    private static Layout Parse(byte[] data)
    {
        var br = new BinaryReaderEx(data[0x11] == 0, data);
        int version = br.GetInt32(12);
        int start = version >= 0x0B ? 0x40 + br.GetInt16(0x3E) : 0x40;
        int size = version >= 0x0B ? 0x40 : 0x30;
        var sections = Enumerable.Range(0, br.GetInt32(0x14)).Select(i =>
        {
            int h = start + i * size;
            return new Section(br.GetASCII(h, 19).TrimEnd('\0'), br.GetUInt32(h + 20), br.GetUInt32(h + 24),
                br.GetUInt32(h + 28), br.GetUInt32(h + 32), br.GetUInt32(h + 36));
        }).ToList();
        return new Layout(start, sections, br);
    }
}

public static class GlobalDump
{
    /// <summary>The data section's global fixups whose target isn't the data section.</summary>
    public static void Run(byte[] data)
    {
        var br = new BinaryReaderEx(data[0x11] == 0, data);
        int version = br.GetInt32(12);
        int start = version >= 0x0B ? 0x40 + br.GetInt16(0x3E) : 0x40;
        int size = version >= 0x0B ? 0x40 : 0x30;
        int count = br.GetInt32(0x14);
        for (int i = 0; i < count; i++)
        {
            int h = start + i * size;
            string tag = br.GetASCII(h, 19).TrimEnd('\0');
            uint s = br.GetUInt32(h + 20), g = br.GetUInt32(h + 28), v = br.GetUInt32(h + 32), e = br.GetUInt32(h + 36);
            var objects = new HashSet<uint>();
            for (uint q = s + v; q + 12 <= s + e; q += 12) if (br.GetUInt32(q) != 0xFFFFFFFF) objects.Add(br.GetUInt32(q));
            for (uint q = s + g; q + 12 <= s + v; q += 12)
            {
                if (br.GetUInt32(q) == 0xFFFFFFFF) continue;
                uint section = br.GetUInt32(q + 4);
                if (section == i && !objects.Contains(br.GetUInt32(q + 8)))
                    Console.WriteLine($"  {tag} global 0x{br.GetUInt32(q):X} -> 0x{br.GetUInt32(q + 8):X}, not an object start");
                if (section != i) Console.WriteLine($"  {tag} global 0x{br.GetUInt32(q):X} -> section {section} 0x{br.GetUInt32(q + 8):X}");
            }
        }
    }
}
