public static class SizeCheck
{
    // sizecheck <before> <after>: events whose size changed, and a parse with the editor's own reader
    public static void Run(string before, string after)
    {
        var a = TemplateReader.Read(before);
        var b = TemplateReader.Read(after);
        foreach (var id in a.Keys.Union(b.Keys).OrderBy(x => x))
        {
            var sa = a.TryGetValue(id, out var ea) ? ea.Size : -1;
            var sb = b.TryGetValue(id, out var eb) ? eb.Size : -1;
            if (sa != sb) System.Console.WriteLine($"{id} {eb?.Name}: 0x{sa:X} -> 0x{sb:X}");
        }
        var t = SoulsFormats.TAE.Template.ReadXMLFile(after);
        System.Console.WriteLine($"SoulsFormats read {t[0].Count} events");
        foreach (var ev in t[0].Values)
            ev.GetDefaultBytes(false);
        System.Console.WriteLine("default bytes ok");
    }
}
