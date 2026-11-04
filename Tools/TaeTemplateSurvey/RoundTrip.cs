using SoulsFormats;

public static class RoundTrip
{
    // roundtrip <cache> <template>: applies the template to every event, with asserts, and writes it back
    public static void Run(string cacheFile, string templateFile)
    {
        var bank = TAE.Template.ReadXMLFile(templateFile)[0];
        var fails = new Dictionary<int, (int Count, string First)>();
        var events = Cache.Read(cacheFile);

        foreach (var e in events)
        {
            if (!bank.TryGetValue(e.Type, out var t)) continue;
            string error = null;

            try
            {
                var ev = new TAE.Event(e.Start, e.End, e.Type, 0, (byte[])e.Bytes.Clone(), false);
                ev.ApplyTemplate(false, t);
                var size = t.GetAllParametersByteCount();
                var back = ev.GetParameterBytes(false);

                for (var i = 0; i < Math.Max(size, e.Bytes.Length); i++)
                {
                    var o = i < e.Bytes.Length ? e.Bytes[i] : (byte)0;
                    var n = i < back.Length ? back[i] : (byte)0;
                    if (o != n) { error = $"byte {i:X} {o:X2} -> {n:X2}"; break; }
                }
            }
            catch (Exception ex)
            {
                error = ex.Message.Split('\n')[0];
            }

            if (error != null)
                fails[e.Type] = fails.TryGetValue(e.Type, out var f) ? (f.Count + 1, f.First) : (1, error);
        }

        Console.WriteLine($"{events.Count} events, {fails.Values.Sum(f => f.Count)} failed");
        foreach (var (type, f) in fails.OrderBy(f => f.Key))
            Console.WriteLine($"  {type} {bank[type].Name}: {f.Count} ({f.First})");
    }
}
