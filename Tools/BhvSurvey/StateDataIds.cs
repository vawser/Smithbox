using SoulsFormats;
public static class StateDataIds
{
    public static void Run(string root, int version)
    {
        var st = new SortedDictionary<string, (int Id, int Idx, int All)>();
        foreach (var path in Directory.GetFiles(root, "*.bhv", SearchOption.AllDirectories))
        {
            var bhv = BHV.Read(File.ReadAllBytes(path));
            if (bhv.Version != version) continue;
            var ids = bhv.States.Select(s => (int)s.ID).ToHashSet();
            foreach (var s in bhv.States.Where(s => s.Data.Length >= 2))
            {
                var key = $"{bhv.Kind} kind{s.Kind,-4} len{s.Data.Length:X}";
                var e = st.GetValueOrDefault(key);
                for (int i = 0; i + 2 <= s.Data.Length; i += 2)
                {
                    int v = BitConverter.ToInt16(s.Data, i);
                    if (v < 0) continue;
                    e.All++; if (ids.Contains(v)) e.Id++; if (v < bhv.States.Count) e.Idx++;
                }
                st[key] = e;
            }
        }
        foreach (var (k, e) in st.Where(x => x.Value.All >= 20).OrderByDescending(x => x.Value.All).Take(40))
            Console.WriteLine($"{k,-28} s16>=0: {e.All,-6} isStateID {e.Id / (double)e.All:P0}  <count {e.Idx / (double)e.All:P0}");
    }
}
