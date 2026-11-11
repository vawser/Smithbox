// x64 exception directory: RUNTIME_FUNCTION entries give function [begin, end) ranges.
public static class Pdata
{
    static List<(ulong Begin, ulong End)> funcs;

    public static List<(ulong Begin, ulong End)> All
    {
        get
        {
            if (funcs != null) return funcs;
            funcs = new();
            var img = Exe.Image;
            int pe = BitConverter.ToInt32(img, 0x3C);
            int opt = pe + 24;
            int dd = opt + 112; // PE32+ data directories
            uint rva = BitConverter.ToUInt32(img, dd + 3 * 8), size = BitConverter.ToUInt32(img, dd + 3 * 8 + 4);
            long fo = Exe.FileOffset(Exe.ImageBase + rva);
            for (long i = 0; i + 12 <= size; i += 12)
            {
                uint b = BitConverter.ToUInt32(img, (int)(fo + i)), e = BitConverter.ToUInt32(img, (int)(fo + i + 4));
                if (b == 0) break;
                funcs.Add((Exe.ImageBase + b, Exe.ImageBase + e));
            }
            funcs.Sort();
            return funcs;
        }
    }

    public static (ulong Begin, ulong End)? Containing(ulong va)
    {
        var l = All; int lo = 0, hi = l.Count - 1;
        while (lo <= hi)
        {
            int m = (lo + hi) / 2;
            if (va < l[m].Begin) hi = m - 1; else if (va >= l[m].End) lo = m + 1; else return l[m];
        }
        return null;
    }
}
