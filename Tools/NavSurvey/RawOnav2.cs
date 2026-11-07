// The pointer pattern inside each ONAV object, relative to the object
static class RawOnav2
{
    public static void Survey(string dir)
    {
        int shown = 0;
        foreach (var path in Directory.GetFiles(dir, "*.onav"))
        {
            var b = File.ReadAllBytes(path);
            int relocCount = Raw.I(b, 0xC);
            int table = (int)Raw.L(b, 0x28);
            var sites = Enumerable.Range(0, relocCount).Select(i => (int)Raw.L(b, table + i * 8)).ToList();
            Raw.Count("sites sorted except first3", sites.Skip(3).SequenceEqual(sites.Skip(3).OrderBy(x => x)));
            Raw.Count("first3", string.Join(",", sites.Take(3).Select(x => x.ToString("X"))));
            int n = Raw.I(b, 0x34);
            int arr = (int)Raw.L(b, 0x18);
            if (arr + n * 8 > b.Length) { Console.WriteLine($"{Path.GetFileName(path)}: n {n} arr {arr:X} len {b.Length:X} hdr30 {Raw.I(b,0x30)} 38 {Raw.I(b,0x38)}"); continue; }
            for (int i = 0; i < n; i++)
            {
                int obj = (int)Raw.L(b, arr + i * 8);
                int next = i + 1 < n ? (int)Raw.L(b, arr + (i + 1) * 8) : arr;
                var inner = sites.Where(s => s >= obj && s < next).Select(s => $"{s - obj:X}->{Raw.L(b, s) - obj:X}").ToList();
                var pat = $"span {next - obj:X}: {string.Join(" ", inner)}";
                Raw.Count("pattern", pat);
                if (pat.StartsWith("span 2C8") && shown++ < 1) Console.WriteLine($"{Path.GetFileName(path)} obj {i} @ {obj:X}");
            }
        }
        Raw.Print(30);
    }
}
