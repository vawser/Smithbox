using SoulsFormats;

// Per-offset value profile of BHV state bodies (one version), for matching fields to enums.
public static class Fields
{
    public static void Run(string root, int version, string what)
    {
        var bodies = new List<byte[]>();
        foreach (var path in Directory.GetFiles(root, "*.bhv", SearchOption.AllDirectories))
        {
            var bhv = BHV.Read(File.ReadAllBytes(path));
            if (bhv.Version != version) continue;
            if (what == "body") bodies.AddRange(bhv.States.Select(s => s.Body));
            else if (what == "bodyB") { if (bhv.Preamble.Length == 0xE0) bodies.AddRange(bhv.States.Select(s => s.Body)); }
            else if (what == "bodyA") { if (bhv.Preamble.Length == 0x40) bodies.AddRange(bhv.States.Select(s => s.Body)); }
            else if (what == "params3") bodies.AddRange(bhv.States.SelectMany(s => s.Transitions).Where(t => t.Params[1] == 3).Select(t => t.Params));
            else if (what == "params4") bodies.AddRange(bhv.States.SelectMany(s => s.Transitions).Where(t => t.Params[1] == 4).Select(t => t.Params));
            else if (what == "B") bodies.AddRange(bhv.Animations);
            else if (what == "BW") { if (bhv.Kind == BHVKind.Weapon) bodies.AddRange(bhv.Animations); }
            else if (what == "BB") { if (bhv.Kind == BHVKind.Base) bodies.AddRange(bhv.Animations); }
            else if (what == "DA") bodies.AddRange(bhv.StructDs.SelectMany(d => d.StructDAs));
            else if (what == "pre") bodies.Add(bhv.Preamble);
        }
        bodies = bodies.Where(b => b.Length == bodies.GroupBy(x => x.Length).OrderByDescending(g => g.Count()).First().Key).ToList();
        Console.WriteLine($"{bodies.Count} records of {bodies[0].Length:X}");
        int len = bodies[0].Length;
        for (int o = 0; o < len; o += 4)
        {
            var i32 = bodies.Select(b => BitConverter.ToInt32(b, o)).ToList();
            var f = bodies.Select(b => BitConverter.ToSingle(b, o)).ToList();
            bool floaty = f.All(x => x == 0 || (Math.Abs(x) > 1e-4 && Math.Abs(x) < 1e6 && !float.IsNaN(x)));
            string Desc(IEnumerable<long> vals)
            {
                var g = vals.GroupBy(v => v).OrderByDescending(x => x.Count()).ToList();
                return $"n={g.Count,-5} [{vals.Min()}..{vals.Max()}] " + string.Join(" ", g.Take(8).Select(x => $"{x.Key}:{x.Count()}"));
            }
            Console.WriteLine($"{o + 0x10:X3} i32 {Desc(i32.Select(x => (long)x))}" + (floaty && i32.Any(x => x != 0 && x != -1) ? $"   f: {string.Join(" ", f.GroupBy(x => x).OrderByDescending(x => x.Count()).Take(6).Select(x => $"{x.Key:0.###}:{x.Count()}"))}" : ""));
            for (int h = 0; h < 4; h += 2)
                Console.WriteLine($"   +{h} s16 {Desc(bodies.Select(b => (long)BitConverter.ToInt16(b, o + h)))}");
            for (int k = 0; k < 4; k++)
                Console.WriteLine($"   +{k} u8  {Desc(bodies.Select(b => (long)b[o + k]))}");
        }
    }
}
