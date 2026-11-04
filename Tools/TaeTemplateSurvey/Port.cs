using System.Text.RegularExpressions;
using System.Xml;

/// <summary>
/// Ports event definitions from one game's template to another's. An event is ported only when the second game's exe
/// reads no parameter that the first's doesn't (the same offsets and widths, or a subset where the trace stopped short), from each game's trace. The ported definition
/// keeps the target's enum entries that the source doesn't list, and its event name when the source's is a placeholder.
///   port <srcTemplate> <srcTrace> <dstTemplate> <dstTrace> [skip ids,...]
/// </summary>
public static class Port
{
    static Dictionary<int, SortedSet<string>> Reads(string trace)
    {
        var result = new Dictionary<int, SortedSet<string>>();
        SortedSet<string> cur = null;
        foreach (var line in File.ReadAllLines(trace))
        {
            var h = Regex.Match(line, @"^## (\d+) ");
            if (h.Success) { result[int.Parse(h.Groups[1].Value)] = cur = new(); continue; }
            var r = Regex.Match(line, @"^  \+([0-9A-F]+) (.*?)  @");
            if (r.Success && cur != null)
            {
                var off = Convert.ToInt32(r.Groups[1].Value, 16);
                if (off >= 0x100) continue;
                // the width of each read; vector loads copy whole blocks, so they're kept as they are
                foreach (Match m in Regex.Matches(r.Groups[2].Value, @"(\w+):(\w+?)(\[idx\])?x\d+"))
                    cur.Add($"{off:X2}:{m.Groups[2].Value.Replace("Int", "").Replace("U", "")}");
            }
        }
        return result;
    }

    static bool IsPlaceholder(string name) =>
        name == null || Regex.IsMatch(name, @"^(Event\d+|Unk.*|.*_\d{3,}$|.*Unk\d+.*|\[.*)", RegexOptions.IgnoreCase);

    public static void Run(string srcPath, string srcTrace, string dstPath, string dstTrace, string skip)
    {
        var skipIds = (skip ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();
        var srcReads = Reads(srcTrace);
        var dstReads = Reads(dstTrace);

        var src = new XmlDocument { PreserveWhitespace = true }; src.Load(srcPath);
        var dst = new XmlDocument { PreserveWhitespace = true }; dst.Load(dstPath);
        var srcEvents = src.DocumentElement.SelectNodes("action").Cast<XmlElement>().ToDictionary(e => int.Parse(e.GetAttribute("id")));
        var dstEvents = dst.DocumentElement.SelectNodes("action").Cast<XmlElement>().ToDictionary(e => int.Parse(e.GetAttribute("id")));

        var ported = new List<int>();
        var differ = new List<string>();

        foreach (var (id, d) in dstEvents.OrderBy(e => e.Key))
        {
            if (!srcEvents.TryGetValue(id, out var s) || skipIds.Contains(id)) continue;
            srcReads.TryGetValue(id, out var sr); dstReads.TryGetValue(id, out var dr);
            sr ??= new(); dr ??= new();

            if (!dr.IsSubsetOf(sr))
            {
                differ.Add($"{id} {d.GetAttribute("name")}: only ER [{string.Join(" ", sr.Except(dr))}] only NR [{string.Join(" ", dr.Except(sr))}]");
                continue;
            }

            var copy = (XmlElement)dst.ImportNode(s, true);

            // keep the target's name when the source's is still a placeholder
            var dstName = d.GetAttribute("name");
            if (IsPlaceholder(copy.GetAttribute("name")) && !IsPlaceholder(dstName)) copy.SetAttribute("name", dstName);

            // keep enum entries the source doesn't have, from the field at the same offset
            var dFields = FieldsByOffset(d);
            foreach (var (off, f) in FieldsByOffset(copy))
            {
                if (!dFields.TryGetValue(off, out var df) || df.Name != f.Name || df.GetAttribute("name") != f.GetAttribute("name")) continue;
                var have = f.SelectNodes("entry").Cast<XmlElement>().Select(e => e.GetAttribute("value")).ToHashSet();
                foreach (var e in df.SelectNodes("entry").Cast<XmlElement>().Where(e => !have.Contains(e.GetAttribute("value"))))
                {
                    var indent = f.LastChild is XmlWhitespace w ? w.Value : null;
                    if (indent == null) { f.AppendChild(dst.CreateWhitespace("\n      ")); f.AppendChild(dst.ImportNode(e, true)); f.AppendChild(dst.CreateWhitespace("\n    ")); }
                    else { f.InsertBefore(dst.CreateWhitespace(indent + "  "), f.LastChild); f.InsertBefore(dst.ImportNode(e, true), f.LastChild); }
                }
            }

            d.ParentNode.ReplaceChild(copy, d);
            ported.Add(id);
        }

        dst.Save(dstPath);
        Console.WriteLine($"ported {ported.Count}: {string.Join(" ", ported)}");
        Console.WriteLine($"reads differ ({differ.Count}):");
        differ.ForEach(x => Console.WriteLine("  " + x));
    }

    static Dictionary<int, XmlElement> FieldsByOffset(XmlElement ev)
    {
        var r = new Dictionary<int, XmlElement>();
        var o = 0;
        foreach (var f in ev.ChildNodes.OfType<XmlElement>())
        {
            r[o] = f;
            o += TemplateReader.SizeOf(f.Name, f);
        }
        return r;
    }
}
