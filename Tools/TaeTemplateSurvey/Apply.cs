using System.Xml;

/// <summary>
/// Applies a rename spec to a template, keeping its formatting. Lines:
///   E id Name                          rename an event
///   F id offset Name [type]            rename a field at an offset, optionally retyping it (same size)
///   S id offset type:Name type:=0 ...  split a field into several (":=0" asserts 0, ":" leaves it unnamed)
///   N id offset value Name             add or rename an enum entry of a field
/// '#' starts a comment.
/// </summary>
public static class Apply
{
    public static void Run(string templatePath, string specPath)
    {
        var doc = new XmlDocument { PreserveWhitespace = true };
        doc.Load(templatePath);
        var events = doc.DocumentElement.SelectNodes("action").Cast<XmlElement>().ToDictionary(e => int.Parse(e.GetAttribute("id")));
        var errors = 0;

        foreach (var raw in File.ReadAllLines(specPath))
        {
            var line = raw.Split('#')[0].Trim();
            if (line.Length == 0) continue;
            var p = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            try
            {
                var ev = events[int.Parse(p[1])];

                switch (p[0])
                {
                    case "E":
                        ev.SetAttribute("name", string.Join(' ', p.Skip(2)));
                        break;
                    case "F":
                    {
                        var f = FieldAt(ev, Convert.ToInt32(p[2], 16));
                        if (p.Length > 4)
                        {
                            if (TemplateReader.SizeOf(p[4], f) != TemplateReader.SizeOf(f.Name, f)) throw new Exception($"retype changes the size of +{p[2]}");
                            f = Retype(f, p[4]);
                        }
                        f.SetAttribute("name", p[3]);
                        break;
                    }
                    case "S":
                    {
                        var f = FieldAt(ev, Convert.ToInt32(p[2], 16));
                        var oldSize = TemplateReader.SizeOf(f.Name, f);
                        var parts = p.Skip(3).Select(x => x.Split(':', 2)).ToList();
                        if (parts.Sum(x => TemplateReader.SizeOf(x[0], null)) != oldSize) throw new Exception($"split of +{p[2]} changes the size");
                        var indent = f.PreviousSibling is XmlWhitespace ws ? ws.Value : "\n    ";
                        XmlNode after = f;
                        for (var i = 0; i < parts.Count; i++)
                        {
                            var n = doc.CreateElement(parts[i][0]);
                            if (parts[i].Length > 1 && parts[i][1] == "=0") n.SetAttribute("assert", "0");
                            else if (parts[i].Length > 1 && parts[i][1].Length > 0) n.SetAttribute("name", parts[i][1]);
                            if (i == 0)
                            {
                                foreach (XmlNode c in f.ChildNodes.Cast<XmlNode>().ToList()) n.AppendChild(c);
                                ev.ReplaceChild(n, f);
                            }
                            else
                            {
                                ev.InsertAfter(doc.CreateWhitespace(indent), after);
                                ev.InsertAfter(n, after.NextSibling);
                            }
                            after = n;
                        }
                        break;
                    }
                    case "N":
                    {
                        var f = FieldAt(ev, Convert.ToInt32(p[2], 16));
                        var value = p[3];
                        var name = string.Join(' ', p.Skip(4));
                        var entry = f.SelectNodes("entry").Cast<XmlElement>().FirstOrDefault(e => e.GetAttribute("value") == value);
                        if (entry == null)
                        {
                            var indent = f.HasChildNodes && f.LastChild is XmlWhitespace lw ? lw.Value : null;
                            var fieldIndent = f.PreviousSibling is XmlWhitespace fw ? fw.Value : "\n    ";
                            entry = doc.CreateElement("entry");
                            entry.SetAttribute("name", name);
                            entry.SetAttribute("value", value);
                            if (indent == null)
                            {
                                f.AppendChild(doc.CreateWhitespace(fieldIndent + "  "));
                                f.AppendChild(entry);
                                f.AppendChild(doc.CreateWhitespace(fieldIndent));
                            }
                            else
                            {
                                f.InsertBefore(doc.CreateWhitespace(indent + "  "), f.LastChild);
                                f.InsertBefore(entry, f.LastChild);
                            }
                        }
                        else entry.SetAttribute("name", name);
                        break;
                    }
                    default:
                        throw new Exception("unknown op");
                }
            }
            catch (Exception e)
            {
                errors++;
                Console.WriteLine($"FAILED: {raw} ({e.Message})");
            }
        }

        doc.Save(templatePath);
        Console.WriteLine($"applied, {errors} failed");
    }

    static XmlElement FieldAt(XmlElement ev, int offset)
    {
        var o = 0;
        foreach (var f in ev.ChildNodes.OfType<XmlElement>())
        {
            if (o == offset) return f;
            o += TemplateReader.SizeOf(f.Name, f);
            if (o > offset) break;
        }
        throw new Exception($"no field starts at +{offset:X2}");
    }

    static XmlElement Retype(XmlElement f, string type)
    {
        if (f.Name == type) return f;
        var n = f.OwnerDocument.CreateElement(type);
        foreach (XmlAttribute a in f.Attributes) n.SetAttribute(a.Name, a.Value);
        foreach (XmlNode c in f.ChildNodes.Cast<XmlNode>().ToList()) n.AppendChild(c);
        f.ParentNode.ReplaceChild(n, f);
        return n;
    }
}
