// Annotated dump of one .bhv: states with their blob + transitions, conditions, and section offsets.
public static class Dump
{
    public static void Run(string path)
    {
        var b = File.ReadAllBytes(path);
        int I(int a) => BitConverter.ToInt32(b, a);
        short ver = BitConverter.ToInt16(b, 0);
        const int D0 = 0x20;
        int stateSize = ver >= 10 ? 0xA0 : 0x80;
        string Hex(int a, int n) => n <= 0 ? "" : BitConverter.ToString(b, a, Math.Min(n, b.Length - a)).Replace("-", " ");
        Console.WriteLine($"v{ver} len {b.Length:X} states@{I(0x20):X}x{I(0x24)} B@{I(0x28):X}x{I(0x2C)} C@{I(0x30):X}x{BitConverter.ToInt16(b,0x34)} sz{BitConverter.ToInt16(b,0x36):X} D@{I(0x38):X}x{I(0x3C)}");
        Console.WriteLine($"preamble: {Hex(0x40, D0 + I(0x20) - 0x40)}");
        int n = I(0x24);
        for (int i = 0; i < n; i++)
        {
            int s = D0 + I(0x20) + i * stateSize;
            int next = i + 1 < n ? I(s + stateSize + 4) : -1;
            Console.WriteLine($"state {i} @{s - D0:X}: id={BitConverter.ToInt16(b, s)} u2={BitConverter.ToInt16(b, s + 2)} blob@{I(s + 4):X} trans@{I(s + 8):X}x{I(s + 12)}  body {Hex(s + 16, stateSize - 16)}");
            for (int t = 0; t < I(s + 12); t++)
            {
                int ta = D0 + I(s + 8) + t * 0x20;
                Console.WriteLine($"   T{t} @{ta - D0:X} -> {I(ta)} conds@{I(ta + 4):X}x{I(ta + 8)} abb@{I(ta + 12):X}: {Hex(D0 + I(ta + 12), 0x20)}");
                for (int c = 0; c < I(ta + 8); c++)
                {
                    int ca = D0 + I(ta + 4) + c * 0x10;
                    Console.WriteLine($"      C{c} @{ca - D0:X} id={b[ca]} {b[ca + 2]}{b[ca + 3]} {b[ca + 8]}{b[ca + 11]} data@{I(ca + 4):X}: {Hex(D0 + I(ca + 4), 0x20)}");
                }
            }
        }
    }
}
