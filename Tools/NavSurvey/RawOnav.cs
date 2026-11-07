// ONAV probes: the relocation table, what each pointer points at, and the header fields
static class RawOnav
{
    public static void Map(string file)
    {
        var b = File.ReadAllBytes(file);
        int relocCount = Raw.I(b, 0xC);
        int table = (int)Raw.L(b, 0x28);
        Console.WriteLine($"{Path.GetFileName(file)} len 0x{b.Length:X} relocs {relocCount} table 0x{table:X} tableEnd 0x{table + relocCount * 8:X}");
        for (int i = 0; i < relocCount; i++)
        {
            int site = (int)Raw.L(b, table + i * 8);
            Console.WriteLine($"  site 0x{site:X} -> 0x{Raw.L(b, site):X}");
        }
        Console.Write("  hdr:");
        for (int o = 0x30; o < 0x74; o += 4) Console.Write($" {o:X}={Raw.I(b, o):X}");
        Console.WriteLine();
    }

    public static void Survey(string dir)
    {
        foreach (var path in Directory.GetFiles(dir, "*.onav"))
        {
            var b = File.ReadAllBytes(path);
            Raw.Count("ver", $"{b[4]},{b[5]},{b[6]},{b[7]}");
            Raw.Count("+08", Raw.I(b, 8));
            int relocCount = Raw.I(b, 0xC);
            long table = Raw.L(b, 0x28);
            Raw.Count("tableEnd==len", table + relocCount * 8 == b.Length);
            Raw.Count("p10==p18", Raw.L(b, 0x10) == Raw.L(b, 0x18));
            Raw.Count("p18==p20", Raw.L(b, 0x18) == Raw.L(b, 0x20));
            Raw.Count("p20 to table", (table - Raw.L(b, 0x20)) / 8 + " ptrs, objCount " + Raw.I(b, 0x34));
            for (int o = 0x30; o < 0x50; o += 4) if (o != 0x34) Raw.Count($"hdr+{o:X}", Raw.I(b, o));
            int gx = Raw.I(b, 0x68), gy = Raw.I(b, 0x6C), gz = Raw.I(b, 0x70);
            Raw.Count("grid product", gx * gy * gz);
            int n = Raw.I(b, 0x34);
            long arr = Raw.L(b, 0x20);
            var sizes = new List<long>();
            for (int i = 0; i < n; i++)
            {
                long obj = Raw.L(b, (int)arr + i * 8);
                long next = i + 1 < n ? Raw.L(b, (int)arr + (i + 1) * 8) : Raw.L(b, 0x10);
                sizes.Add(next - obj);
                Raw.Count("obj span", next - obj);
                int ob = (int)obj;
                Raw.Count("obj+00 u16 a==i", BitConverter.ToUInt16(b, ob) == i);
                Raw.Count("obj+02 u16 b", BitConverter.ToUInt16(b, ob + 2));
                Raw.Count("obj+10", Raw.I(b, ob + 0x10));
                Raw.Count("obj+18", Raw.I(b, ob + 0x18));
                for (int o = 0x28; o < 0x40; o += 4) Raw.Count($"obj+{o:X}", Raw.I(b, ob + o));
                Raw.Count("obj+240 ptr - obj", Raw.L(b, ob + 0x240) - obj);
            }
        }
        Raw.Print(14);
    }
}
