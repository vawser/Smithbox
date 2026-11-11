using SoulsFormats;
public static class Split
{
    public static void Run(string root)
    {
        var c = new SortedDictionary<string, int>();
        foreach (var path in Directory.GetFiles(root, "*.bhv", SearchOption.AllDirectories))
        {
            var bhv = BHV.Read(File.ReadAllBytes(path));
            string kind = Path.GetFileName(path) == "basenormal.bhv" ? "base" : "w";
            foreach (var s in bhv.States)
            {
                float a = BitConverter.ToSingle(s.Body, 0x24 - 0x10), b = BitConverter.ToSingle(s.Body, 0x40 - 0x10);
                string g = a == 1f ? "A" : b == 1f ? "B" : "?";
                var k = $"v{bhv.Version} {kind} layout {g}";
                c[k] = c.GetValueOrDefault(k) + 1;
            }
        }
        foreach (var (k, v) in c) Console.WriteLine($"{v,7} {k}");
    }
}
