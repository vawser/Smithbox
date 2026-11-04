using System.Text.RegularExpressions;

public static class Evidence
{
    // evidence <template> <trace> : the trace's lines for each unnamed field
    public static void Run(string template, string traceFile)
    {
        var t = TemplateReader.Read(template);
        var blocks = new Dictionary<int, List<string>>();
        List<string> cur = null;
        foreach (var line in File.ReadAllLines(traceFile))
        {
            var m = Regex.Match(line, @"^## (\d+) ");
            if (m.Success) { blocks[int.Parse(m.Groups[1].Value)] = cur = new(); continue; }
            cur?.Add(line);
        }
        foreach (var ev in t.Values.OrderBy(e => e.ID))
        {
            var fields = ev.Fields.Where(f => f.Name == null && f.Assert == null).ToList();
            if (fields.Count == 0) continue;
            Console.WriteLine($"{ev.ID} {ev.Name}");
            blocks.TryGetValue(ev.ID, out var b);
            b ??= new();
            foreach (var f in fields)
            {
                var key = $"+{f.Offset:X2}";
                var lines = new List<string>();
                var i = b.FindIndex(l => l.StartsWith($"  {key} "));
                if (i >= 0)
                {
                    lines.Add(Regex.Match(b[i], @"^  \+\w+ (\S+)").Groups[1].Value);
                    for (var k = i + 1; k < b.Count && b[k].StartsWith("        "); k++) lines.Add(b[k].Trim());
                }
                lines.AddRange(b.Where(l => l.Contains("effect:") && (l.Contains($"field {key}") || Regex.IsMatch(l, $@"a\d={Regex.Escape(key)}\b"))).Select(l => l.Replace("effect: ", "").Trim()).Distinct().Take(4));
                Console.WriteLine($"   {key} {f.Type,-4} {(lines.Count == 0 ? "NOT READ" : string.Join(" | ", lines.Take(6)))}");
            }
        }
    }
}
