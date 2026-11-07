static class RawOnav4
{
    public static void Survey(string dir)
    {
        foreach (var path in Directory.GetFiles(dir, "*.onav"))
        {
            var b = File.ReadAllBytes(path);
            long a = Raw.L(b, 0x10), bb = Raw.L(b, 0x18), c = Raw.L(b, 0x20), t = Raw.L(b, 0x28);
            var u16 = Enumerable.Range(0, 16).Select(i => BitConverter.ToUInt16(b, 0x30 + i * 2)).ToArray();
            Raw.Count("u16s 30..4F", string.Join(",", u16));
            Raw.Count("B-A, C-B, T-C", $"{bb - a:X},{c - bb:X},{t - c:X}  u16[2]={u16[2]} u16[3]={u16[3]}");
            Raw.Count("(T-C)/8 == u16[2]", (t - c) / 8 == u16[2]);
            Raw.Count("(C-B)/8 == ?", $"{(c - bb) / 8} vs {u16[0]},{u16[1]},{u16[3]}");
            Raw.Count("ver byte5", $"{b[5]} u16[3]={u16[3]}");
        }
        Raw.Print(20);
    }
}
