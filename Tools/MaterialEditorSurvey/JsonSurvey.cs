using SoulsFormats;
using StudioCore.Editors.MaterialEditor;

/// <summary>
/// json GAME: every vanilla material through MaterialJson (to text, back to a material) must give the same fields,
/// keys included (recomputed on import), and the result must write.
/// </summary>
public static class JsonSurvey
{
    public static void Run(GameMaterials g)
    {
        Console.WriteLine("== json (F9 export / import)");

        var total = 0;
        var same = 0;
        var failures = new List<string>();
        var bytes = 0L;
        var entries = 0;
        var entriesSame = 0;

        foreach (var b in g.Binders.Where(b => b.Selected))
        {
            foreach (var (file, data, _) in GameMaterials.Files(b))
            {
                object material;
                try { material = b.Kind == MaterialKind.MTD ? MTD.Read(data) : MATBIN.Read(data); }
                catch { continue; }

                total++;
                var name = GameMaterials.ShortName(file.Name);

                try
                {
                    var text = MaterialJson.From(name, material).ToText();
                    bytes += text.Length;

                    var back = MaterialJson.FromText(text)?.ToMaterial(out var error);
                    if (back == null)
                    {
                        if (failures.Count < 5) failures.Add($"{name}: not read back");
                        continue;
                    }

                    // Writes, and every field (MATBIN keys too) is what the game's file has
                    _ = back is MTD m ? m.Write() : ((MATBIN)back).Write();

                    if (Same(material, back))
                        same++;
                    else if (failures.Count < 5)
                        failures.Add($"{name}: fields differ");

                    // Each entry through the clipboard form: copied, pasted back, the same entry (keys too)
                    foreach (var (entry, _) in MaterialQueryMatcher.Entries(material))
                    {
                        entries++;
                        var copied = MaterialJson.FromEntry(entry).ToText();
                        var pasted = MaterialJson.ToEntry(MaterialJson.EntryJson.FromText(copied));

                        var keySame = (entry, pasted) switch
                        {
                            (MATBIN.Param p1, MATBIN.Param p2) => p1.Key == p2.Key,
                            (MATBIN.Sampler s1, MATBIN.Sampler s2) => s1.Key == s2.Key,
                            _ => true
                        };

                        if (pasted != null && keySame && MaterialJson.FromEntry(pasted).ToText() == copied)
                            entriesSame++;
                        else if (failures.Count < 5)
                            failures.Add($"{name}: entry {MaterialJson.FromEntry(entry).List} differs");
                    }
                }
                catch (Exception e)
                {
                    if (failures.Count < 5) failures.Add($"{name}: {e.GetType().Name} {e.Message}");
                }
            }
        }

        Console.WriteLine($"  {total} materials: {same} come back with every field the same ({bytes / Math.Max(total, 1)} chars of JSON each on average)");
        Console.WriteLine($"  {entries} entries: {entriesSame} come back the same through copy and paste");
        foreach (var f in failures) Console.WriteLine($"      e.g. {f}");
    }

    private static bool Same(object a, object b) => (a, b) switch
    {
        (MTD x, MTD y) => Inspect.Dump(x) == Inspect.Dump(y),
        (MATBIN x, MATBIN y) => Inspect.Dump(x) == Inspect.Dump(y),
        _ => false
    };
}
