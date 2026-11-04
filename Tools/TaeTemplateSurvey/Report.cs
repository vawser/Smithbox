using System.Text;

/// <summary>
/// Compares the events of a game against a template: the sizes the game uses, the values of each field,
/// asserts the game breaks, enum values the template doesn't list, and data past the end of the template.
/// </summary>
public static class Report
{
    public static void Run(string cacheFile, string templateFile, string outFile)
    {
        var events = Cache.Read(cacheFile);
        var template = TemplateReader.Read(templateFile);
        var sb = new StringBuilder();
        var summary = new StringBuilder();

        summary.AppendLine($"# {events.Count} events, {events.Select(e => e.Type).Distinct().Count()} types, template has {template.Count}");
        summary.AppendLine();

        foreach (var group in events.GroupBy(e => e.Type).OrderBy(g => g.Key))
        {
            var list = group.ToList();
            template.TryGetValue(group.Key, out var t);
            var sizes = list.GroupBy(e => e.Bytes.Length).OrderByDescending(g => g.Count()).ToList();
            var maxSize = sizes.Max(s => s.Key);

            sb.AppendLine($"## {group.Key} {t?.Name ?? "(NOT IN TEMPLATE)"}");
            sb.AppendLine($"count={list.Count} files={list.Select(e => e.Source).Distinct().Count()} sizes={string.Join(",", sizes.Select(s => $"0x{s.Key:X}x{s.Count()}"))} templateSize=0x{t?.Size ?? 0:X}");

            var issues = new List<string>();

            if (t == null)
                issues.Add("missing");

            if (t != null)
            {
                foreach (var f in t.Fields)
                {
                    var values = list.Where(e => e.Bytes.Length >= f.Offset + f.Size).Select(e => Decode(f.Type, e.Bytes, f.Offset, f.Size)).ToList();
                    var hist = values.GroupBy(v => v).OrderByDescending(g => g.Count()).ToList();
                    var flags = new List<string>();

                    if (f.Assert != null)
                    {
                        var bad = hist.Where(h => !SameValue(h.Key, f.Assert)).ToList();
                        if (bad.Count > 0)
                        {
                            flags.Add($"ASSERT {f.Assert} BROKEN by {bad.Sum(b => b.Count())}");
                            issues.Add($"assert@{f.Offset:X2}");
                        }
                    }

                    if (f.Enum.Count > 0)
                    {
                        var unknown = hist.Where(h => long.TryParse(h.Key, out var v) && !f.Enum.ContainsKey(v)).Select(h => $"{h.Key}x{h.Count()}").ToList();
                        if (unknown.Count > 0)
                        {
                            flags.Add($"UNLISTED ENUM {string.Join(" ", unknown.Take(12))}");
                            issues.Add($"enum@{f.Offset:X2}");
                        }
                    }

                    if (f.Type is "s32" or "u32" or "x32" && values.Count > 0)
                    {
                        var floaty = list.Count(e => e.Bytes.Length >= f.Offset + 4 && LooksFloat(BitConverter.ToUInt32(e.Bytes, f.Offset)));
                        if (floaty > values.Count / 4 && floaty > 0)
                        {
                            flags.Add($"LOOKS FLOAT {floaty}/{values.Count}");
                            issues.Add($"float@{f.Offset:X2}");
                        }
                    }

                    if (f.Type == "b" && hist.Any(h => h.Key != "0" && h.Key != "1"))
                    {
                        flags.Add("BOOL HAS OTHER VALUES");
                        issues.Add($"bool@{f.Offset:X2}");
                    }

                    sb.AppendLine($"  +{f.Offset:X2} {f.Type,-4} {f.Name ?? "<unnamed>",-36} distinct={hist.Count,-5} {string.Join(" ", hist.Take(10).Select(h => $"{h.Key}x{h.Count()}"))} {string.Join(" ", flags)}");
                }
            }

            // Anything past the template
            var start = t?.Size ?? 0;

            if (maxSize > start)
            {
                var anyNonZero = false;

                for (var o = start; o < maxSize; o++)
                {
                    if (list.Any(e => e.Bytes.Length > o && e.Bytes[o] != 0))
                        anyNonZero = true;
                }

                if (anyNonZero)
                    issues.Add($"tail {start:X}..{maxSize:X} has data");

                sb.AppendLine($"  -- past template ({start:X}..{maxSize:X}) {(anyNonZero ? "HAS DATA" : "all zero")}");

                if (anyNonZero)
                {
                    for (var o = start & ~3; o + 4 <= maxSize; o += 4)
                    {
                        var vals = list.Where(e => e.Bytes.Length >= o + 4).Select(e => BitConverter.ToUInt32(e.Bytes, o)).ToList();
                        var nz = vals.Count(v => v != 0);
                        if (nz == 0) continue;
                        var floaty = vals.Count(LooksFloat);
                        var hist = vals.GroupBy(v => v).OrderByDescending(g => g.Count()).Take(10);
                        sb.AppendLine($"  +{o:X2} slot nonzero={nz}/{vals.Count} float={floaty} {string.Join(" ", hist.Select(h => $"{Slot(h.Key)}x{h.Count()}"))}");
                    }
                }
            }

            sb.AppendLine();

            summary.AppendLine($"{group.Key,5} {t?.Name ?? "-",-40} n={list.Count,-7} sizes={string.Join(",", sizes.Select(s => $"0x{s.Key:X}"))} tpl=0x{t?.Size ?? 0:X} {string.Join(" ", issues)}");
        }

        summary.AppendLine();
        summary.AppendLine("# Template events not seen in the game");
        foreach (var t in template.Values.Where(t => !events.Any(e => e.Type == t.ID)))
            summary.AppendLine($"{t.ID,5} {t.Name}");

        File.WriteAllText(outFile, summary + "\n\n" + sb);
        Console.WriteLine(summary);
    }

    public static string Slot(uint v)
    {
        if (LooksFloat(v))
            return $"{BitConverter.UInt32BitsToSingle(v):0.###}f";
        return ((int)v).ToString();
    }

    public static bool LooksFloat(uint v)
    {
        if (v == 0) return false;
        var f = BitConverter.UInt32BitsToSingle(v);
        if (float.IsNaN(f) || float.IsInfinity(f)) return v == 0x7F7FFFFF;
        var a = Math.Abs(f);
        return a >= 1e-4f && a <= 1e7f;
    }

    static bool SameValue(string value, string assert)
    {
        if (value == assert) return true;
        if (double.TryParse(value, out var a) && double.TryParse(assert, out var b)) return a == b;
        if (assert.Equals("false", StringComparison.OrdinalIgnoreCase)) return value == "0";
        if (assert.Equals("true", StringComparison.OrdinalIgnoreCase)) return value == "1";
        return false;
    }

    public static string Decode(string type, byte[] b, int o, int size) => type switch
    {
        "b" or "u8" or "x8" => b[o].ToString(),
        "s8" => ((sbyte)b[o]).ToString(),
        "u16" or "x16" => BitConverter.ToUInt16(b, o).ToString(),
        "s16" => BitConverter.ToInt16(b, o).ToString(),
        "u32" or "x32" => BitConverter.ToUInt32(b, o).ToString(),
        "s32" => BitConverter.ToInt32(b, o).ToString(),
        "f32" => BitConverter.ToSingle(b, o).ToString("0.####"),
        "f32grad" => $"{BitConverter.ToSingle(b, o):0.###}>{BitConverter.ToSingle(b, o + 4):0.###}",
        "f64" => BitConverter.ToDouble(b, o).ToString("0.####"),
        "u64" or "x64" => BitConverter.ToUInt64(b, o).ToString(),
        "s64" => BitConverter.ToInt64(b, o).ToString(),
        _ => Convert.ToHexString(b, o, size)
    };
}
