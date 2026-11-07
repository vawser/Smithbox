static class RawOnav3
{
    // Every object's fields outside the bitmap, as hex/int/float columns
    public static void Objects(string file)
    {
        var b = File.ReadAllBytes(file);
        int n = (ushort)Raw.I(b, 0x34);
        int arr = (int)Raw.L(b, 0x18);
        Console.WriteLine($"{Path.GetFileName(file)} hdr34 {Raw.I(b, 0x34):X} objs {n}");
        for (int i = 0; i < n; i++)
        {
            int obj = (int)Raw.L(b, arr + i * 8);
            int next = i + 1 < n ? (int)Raw.L(b, arr + (i + 1) * 8) : arr;
            Console.WriteLine($" obj {i} @{obj:X} span {next - obj:X}");
            void Row(int from, int to)
            {
                for (int o = from; o < to; o += 4)
                {
                    int v = Raw.I(b, obj + o); float f = Raw.F(b, obj + o);
                    string fs = (Math.Abs(f) > 1e-6 && Math.Abs(f) < 1e7) ? f.ToString("0.#####") : "";
                    Console.WriteLine($"   +{o:X3} {v:X8} {v,11} {fs}");
                }
            }
            Row(0, 0x40);
            Console.WriteLine("   ... bitmap ...");
            Row(0x240, next - obj);
        }
    }
}
