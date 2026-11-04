public static class LayoutDiff
{
    // layoutdiff <a> <b> : events in both whose field types/offsets differ, and events only in one
    public static void Run(string a, string b)
    {
        var ta = TemplateReader.Read(a); var tb = TemplateReader.Read(b);
        string Sig(TemplateEvent e) => string.Join(",", e.Fields.Select(f => $"{f.Offset:X}:{f.Type}"));
        foreach (var id in ta.Keys.Intersect(tb.Keys).OrderBy(x => x))
            if (Sig(ta[id]) != Sig(tb[id])) Console.WriteLine($"DIFF {id} {tb[id].Name}\n   a: {Sig(ta[id])}\n   b: {Sig(tb[id])}");
        Console.WriteLine("only in b: " + string.Join(" ", tb.Keys.Except(ta.Keys).OrderBy(x => x).Select(x => $"{x}:{tb[x].Name}")));
        Console.WriteLine("only in a: " + string.Join(" ", ta.Keys.Except(tb.Keys).OrderBy(x => x)));
    }
}
