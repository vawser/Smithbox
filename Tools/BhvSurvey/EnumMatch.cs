using SoulsFormats;
using System.Text;
using System.Text.RegularExpressions;

// Matches each u8 offset of state bodies (per layout) against AC6 TDF u8 enums: observed values must all be in the enum.
public static class EnumMatch
{
    public static void Run(string root, string tdfDir, int version)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var sjis = Encoding.GetEncoding(932);
        var enums = new Dictionary<string, HashSet<long>>();
        foreach (var f in Directory.GetFiles(tdfDir, "*.tdf"))
        {
            var text = sjis.GetString(File.ReadAllBytes(f));
            var lines = text.Split('\n');
            if (lines.Length < 3 || !lines[1].Contains("\"u8\"")) continue;
            var vals = new HashSet<long>();
            foreach (var l in lines.Skip(2)) { var m = Regex.Match(l, @",""(-?\d+)"""); if (m.Success) vals.Add(long.Parse(m.Groups[1].Value)); }
            if (vals.Count >= 4) enums[Path.GetFileNameWithoutExtension(f)] = vals;
        }
        if (Environment.GetEnvironmentVariable("BHV_CONDS") == "1")
        {
            var byType = new Dictionary<int, List<byte[]>>();
            foreach (var path in Directory.GetFiles(root, "*.bhv", SearchOption.AllDirectories))
            {
                var bhv = BHV.Read(File.ReadAllBytes(path));
                if (bhv.Version != version) continue;
                foreach (var c in bhv.States.SelectMany(s => s.Transitions).SelectMany(t => t.Conditions))
                { if (!byType.TryGetValue(c.Type, out var l)) byType[c.Type] = l = new(); l.Add(c.Data); }
            }
            foreach (var (type, datas) in byType.OrderBy(k => k.Key))
            {
                int len = datas.GroupBy(d => d.Length).OrderByDescending(g => g.Count()).First().Key;
                var ds = datas.Where(d => d.Length == len).ToList();
                var parts = new List<string>();
                for (int o = 0; o < len; o++)
                {
                    var obs = ds.Select(b => (long)b[o]).Where(v => v != 0 && v != 255).Distinct().ToHashSet();
                    if (obs.Count < 3) continue;
                    var fit = enums.Where(e => obs.IsSubsetOf(e.Value)).Select(e => (e.Key, Cover: obs.Count / (double)e.Value.Count)).OrderByDescending(x => x.Cover).FirstOrDefault();
                    if (fit.Key != null && fit.Cover >= 0.15) parts.Add($"+{o:X} {fit.Key}({fit.Cover:P0},n={obs.Count})");
                }
                Console.WriteLine($"cond {type,3} n={datas.Count,-6} len={len:X} {string.Join("  ", parts)}");
            }
            return;
        }
        foreach (var layout in new[] { ("A weapon", 0x40), ("B base", 0xE0) })
        {
            var bodies = new List<byte[]>();
            foreach (var path in Directory.GetFiles(root, "*.bhv", SearchOption.AllDirectories))
            {
                var bhv = BHV.Read(File.ReadAllBytes(path));
                if (bhv.Version == version && bhv.Preamble.Length == layout.Item2) bodies.AddRange(bhv.States.Select(s => s.Body));
            }
            Console.WriteLine($"== layout {layout.Item1}: {bodies.Count} states");
            for (int o = 0; o < bodies[0].Length; o++)
            {
                var obs = bodies.Select(b => (long)b[o]).Where(v => v != 0 && v != 255).Distinct().ToHashSet();
                if (obs.Count < 3) continue;
                var fits = enums.Where(e => obs.IsSubsetOf(e.Value)).Select(e => (e.Key, Cover: obs.Count / (double)e.Value.Count))
                    .OrderByDescending(x => x.Cover).Take(4).ToList();
                if (fits.Count > 0)
                    Console.WriteLine($"  {o + 0x10:X3} n={obs.Count,-3} {string.Join("  ", fits.Select(x => $"{x.Key}({x.Cover:P0})"))}   vals {string.Join(",", obs.OrderBy(x => x).Take(14))}");
            }
        }
    }
}
