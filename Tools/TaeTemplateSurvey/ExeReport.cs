using System.Text;

/// <summary>
/// Traces each event type from the dispatchers and sets the reads the game makes against the template's fields.
/// The functions a field's value is passed to are matched across events, so a named field can name another.
/// </summary>
public static class ExeReport
{
    public static void Run(string exe, ulong[] dispatchers, string templateFile, string cacheFile, string outFile, int only = -1)
    {
        ExeScan.Load(exe);
        ParamTrace.Init();

        var template = TemplateReader.Read(templateFile);
        var used = Cache.Read(cacheFile).GroupBy(e => e.Type).ToDictionary(g => g.Key, g => g.ToList());
        var types = template.Keys.Union(used.Keys).Where(t => only < 0 || t == only).OrderBy(t => t).ToList();
        var sb = new StringBuilder();
        var results = new Dictionary<int, ParamTrace.Result>();

        foreach (var type in types)
        {
            var r = new ParamTrace.Result();
            foreach (var d in dispatchers)
            {
                var one = ParamTrace.Trace(d, type, new() { ["RDX"] = new ParamTrace.Tag(ParamTrace.Kind.Ctx, 0) });
                r.Accesses.AddRange(one.Accesses); r.CtxAccesses.AddRange(one.CtxAccesses);
                r.Notes.UnionWith(one.Notes); r.Functions.UnionWith(one.Functions); r.Steps += one.Steps;
                r.Sinks.AddRange(one.Sinks); r.Consts.AddRange(one.Consts);
            }
            results[type] = r;
        }

        // the named fields that reach each sink
        var sinkNames = new Dictionary<string, HashSet<string>>();
        foreach (var (type, r) in results)
        {
            if (!template.TryGetValue(type, out var t)) continue;
            foreach (var (off, sink) in r.Sinks.Distinct())
            {
                var f = t.Fields.FirstOrDefault(x => x.Offset == off);
                if (f?.Name == null) continue;
                if (!sinkNames.TryGetValue(sink, out var set)) sinkNames[sink] = set = new();
                set.Add($"{type}:{f.Name}");
            }
        }

        foreach (var type in types)
        {
            var r = results[type];
            template.TryGetValue(type, out var t);
            used.TryGetValue(type, out var evs);

            sb.AppendLine($"## {type} {t?.Name} uses={evs?.Count ?? 0} functions={r.Functions.Count} steps={r.Steps}");

            var reads = r.Accesses.Where(a => a.Offset >= 0 && a.Offset < 0x100)
                .GroupBy(a => a.Offset).OrderBy(g => g.Key).ToList();

            foreach (var g in reads)
            {
                var hows = g.GroupBy(a => a.How).Select(h => $"{h.Key}x{h.Count()}");
                var field = t?.Fields.FirstOrDefault(f => g.Key >= f.Offset && g.Key < f.Offset + f.Size);
                var sizes = g.Select(a => a.Size).Distinct().ToList();
                var flag = "";

                if (field == null) flag = "  <== NO FIELD";
                else
                {
                    if (field.Offset != g.Key) flag += $"  <== MID-FIELD of +{field.Offset:X2}";
                    if (!sizes.Contains(field.Size) && !sizes.All(s => s >= 16)) flag += $"  <== SIZE {string.Join("/", sizes)} vs {field.Size}";
                    if (field.Name == null) flag += "  <== UNNAMED";
                    if (field.Assert != null) flag += $"  <== ASSERTED {field.Assert}";
                    var isFloat = g.Any(a => a.How.Contains("Float32") && !a.How.Contains("Packed"));
                    if (isFloat && field.Type != "f32" && field.Type != "f32grad") flag += $"  <== FLOAT, template {field.Type}";
                    var isInt = g.Any(a => a.How.StartsWith("movzx") || a.How.StartsWith("movsx") || (a.How.StartsWith("cmp") && !a.How.Contains("Float")));
                    if (isInt && field.Type == "f32") flag += $"  <== INT, template f32";
                    var signed = g.Any(a => a.How.StartsWith("movsx"));
                    var unsigned = g.Any(a => a.How.StartsWith("movzx"));
                    if (signed && !unsigned && field.Type is "u8" or "u16" or "u32") flag += "  <== SIGNED";
                    if (g.Any(a => a.Write)) flag += "  <== WRITTEN";
                }

                var fieldText = field == null ? "" : $"{field.Type} {field.Name ?? "<unnamed>"}";
                sb.AppendLine($"  +{g.Key:X2} {string.Join(" ", hows)}  @{string.Join(",", g.Select(a => $"{a.Where:X}").Distinct().Take(3))}  [{fieldText}]{flag}");

                var consts = r.Consts.Where(c => c.Offset == g.Key).Select(c => c.Const).Distinct().ToList();
                if (consts.Count > 0) sb.AppendLine($"        compared to: {string.Join(",", consts.Take(40))}");

                foreach (var sink in r.Sinks.Where(s => s.Offset == g.Key).Select(s => s.Sink).Distinct().Take(6))
                {
                    var others = sinkNames.TryGetValue(sink, out var set) ? set.Where(n => !n.StartsWith($"{type}:")).Take(6).ToList() : new();
                    sb.AppendLine($"        -> {sink}{(others.Count > 0 ? "   shared with " + string.Join(", ", others) : "")}");
                }
            }

            var ctx = r.CtxAccesses.Where(a => a.Offset != 0 && a.Offset != 8).Select(a => $"+{a.Offset:X}").Distinct();
            if (ctx.Any()) sb.AppendLine($"  ctx: {string.Join(" ", ctx)}");

            foreach (var n in r.Notes.Take(8)) sb.AppendLine($"  note: {n}");

            // template fields the game never read
            if (t != null)
            {
                var unread = t.Fields.Where(f => f.Assert == null && !r.Accesses.Any(a => a.Offset < f.Offset + f.Size && a.Offset + Math.Max(a.Size, 1) > f.Offset))
                    .Select(f => $"+{f.Offset:X2} {f.Name ?? "<unnamed>"}");
                if (unread.Any()) sb.AppendLine($"  unread: {string.Join(", ", unread)}");
            }

            sb.AppendLine();
        }

        File.WriteAllText(outFile, sb.ToString());
        Console.WriteLine($"wrote {outFile}");
    }
}
