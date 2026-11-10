public static class StateBlobExamples
{
    public static void Run(string root, int wantVer, int wantU2, int max)
    {
        int shown = 0;
        foreach (var path in Directory.GetFiles(root, "*.bhv", SearchOption.AllDirectories))
        {
            var b = File.ReadAllBytes(path);
            int I(int a) => BitConverter.ToInt32(b, a);
            short S(int a) => BitConverter.ToInt16(b, a);
            short ver = S(0); if (ver != wantVer) continue;
            int ss = ver >= 10 ? 0xA0 : 0x80;
            int n = I(0x24), so = 0x20 + I(0x20);
            var starts = new SortedSet<int>();
            for (int i = 0; i < n; i++) { int s = so + i * ss; if (I(s + 12) > 0) starts.Add(I(s + 8)); starts.Add(I(s + 4)); }
            starts.Add(I(0x28)); starts.Add(I(0x30));
            for (int i = 0; i < n; i++)
            {
                int s = so + i * ss; int u2 = S(s + 2), off = I(s + 4);
                if (u2 != wantU2) continue;
                int next = starts.Where(x => x > off).DefaultIfEmpty(b.Length - 0x20).Min();
                Console.WriteLine($"{Path.GetFileName(Path.GetDirectoryName(path))}/{Path.GetFileName(path)} st{i} id={S(s)} size={next - off:X}: {BitConverter.ToString(b, 0x20 + off, Math.Min(next - off, 0x60)).Replace("-", " ")}");
                Console.WriteLine($"     body: {BitConverter.ToString(b, s + 16, ss - 16).Replace("-", " ")}");
                if (++shown >= max) return;
            }
        }
    }
}
