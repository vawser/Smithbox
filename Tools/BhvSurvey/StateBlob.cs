// Statistics for state blob element size vs. state fields.
public static class StateBlob
{
    public static void Run(string root)
    {
        var st = new SortedDictionary<string, int>();
        foreach (var path in Directory.GetFiles(root, "*.bhv", SearchOption.AllDirectories))
        {
            var b = File.ReadAllBytes(path);
            int I(int a) => BitConverter.ToInt32(b, a);
            short S(int a) => BitConverter.ToInt16(b, a);
            short ver = S(0); int ss = ver >= 10 ? 0xA0 : 0x80;
            int n = I(0x24), so = 0x20 + I(0x20);
            var starts = new SortedSet<int>();
            for (int i = 0; i < n; i++) { int s = so + i * ss; if (I(s + 12) > 0) starts.Add(I(s + 8)); starts.Add(I(s + 4)); }
            starts.Add(I(0x28)); starts.Add(I(0x30));
            for (int i = 0; i < n; i++)
            {
                int s = so + i * ss; int cnt = S(s + 2), off = I(s + 4);
                if (cnt == 0) continue;
                int next = starts.Where(x => x > off).DefaultIfEmpty(b.Length - 0x20).Min();
                int size = next - off;
                string k = $"v{ver} u2={cnt:X} size={size:X}";
                st[k] = st.GetValueOrDefault(k) + 1;
            }
        }
        foreach (var (k, v) in st.OrderBy(k => k.Key)) Console.WriteLine($"{v,6} {k}");
    }
}
