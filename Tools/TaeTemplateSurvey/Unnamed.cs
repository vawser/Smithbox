public static class Unnamed
{
    public static void Run(string template, string cache)
    {
        var t = TemplateReader.Read(template);
        var used = Cache.Read(cache).GroupBy(e => e.Type).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var ev in t.Values.OrderBy(e => e.ID))
        {
            var fields = ev.Fields.Where(f => f.Name == null && f.Assert == null).ToList();
            if (fields.Count == 0) continue;
            used.TryGetValue(ev.ID, out var evs);
            Console.WriteLine($"{ev.ID} {ev.Name} (uses {evs?.Count ?? 0})");
            foreach (var f in fields)
            {
                var vals = evs?.Where(e => e.Bytes.Length >= f.Offset + f.Size).Select(e => Report.Decode(f.Type, e.Bytes, f.Offset, f.Size)).GroupBy(v => v).OrderByDescending(g => g.Count()).Take(6).Select(g => $"{g.Key}x{g.Count()}");
                Console.WriteLine($"   +{f.Offset:X2} {f.Type,-4} {string.Join(" ", vals ?? Enumerable.Empty<string>())}");
            }
        }
    }
}
