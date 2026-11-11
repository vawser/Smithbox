using SoulsFormats;
using StudioCore.Editors.MaterialEditor;
using System.Diagnostics;

/// <summary>
/// query GAME: checks the F8 search and mass edit rules over vanilla. Every param value written as text and Set back
/// must give the same value, of the same type and length; a few searches are timed over every material.
/// </summary>
public static class QuerySurvey
{
    public static void Run(GameMaterials g)
    {
        Console.WriteLine("== query (F8 search / mass edit rules)");

        var materials = new List<(string name, object material)>();
        foreach (var b in g.Binders.Where(b => b.Selected))
        {
            foreach (var (file, data, _) in GameMaterials.Files(b))
            {
                try { materials.Add((GameMaterials.ShortName(file.Name), b.Kind == MaterialKind.MTD ? MTD.Read(data) : MATBIN.Read(data))); }
                catch { }
            }
        }

        // Set(ValueText(v)) == v
        var values = 0;
        var same = 0;
        var failures = new List<string>();

        foreach (var (name, material) in materials)
        {
            foreach (var (entry, _) in MaterialQueryMatcher.Entries(material))
            {
                var value = entry switch { MTD.Param p => p.Value, MATBIN.Param p => p.Value, _ => null };
                if (value == null) continue;

                values++;
                var text = MaterialQueryMatcher.ValueText(value);

                if (MaterialValueEdit.TryApply(value, MaterialValueEdit.Operation.Set, text, out var back) &&
                    back.GetType() == value.GetType() &&
                    MaterialQueryMatcher.ValueText(back) == text &&
                    (value is not Array a || ((Array)back).Length == a.Length))
                {
                    same++;
                }
                else if (failures.Count < 5)
                {
                    failures.Add($"{name}: {value.GetType().Name} '{text}'");
                }
            }
        }

        Console.WriteLine($"  {materials.Count} materials, {values} param values: {same} come back the same through Set");
        foreach (var f in failures) Console.WriteLine($"      e.g. {f}");

        // Timings of whole-game searches, as the tool runs them on the main thread
        Time(materials, "entry name snp_0_color", new MaterialQuery("", "", "snp_0_color", "", "", MaterialComparison.Contains, false));
        Time(materials, "value > 10", new MaterialQuery("", "", "", "10", "", MaterialComparison.GreaterThan, false));
        Time(materials, "path contains _a.tif", new MaterialQuery("", "", "", "", "_a.tif", MaterialComparison.Contains, false));
        Time(materials, "shader [Sway] (materials)", new MaterialQuery("", "[Sway]", "", "", "", MaterialComparison.Contains, false));
    }

    private static void Time(List<(string name, object material)> materials, string label, MaterialQuery query)
    {
        var timer = Stopwatch.StartNew();
        var hits = 0;

        foreach (var (name, material) in materials)
        {
            if (!MaterialQueryMatcher.MatchMaterial(query, name, material))
                continue;

            if (!query.MatchesEntries)
            {
                hits++;
                continue;
            }

            hits += MaterialQueryMatcher.Entries(material).Count(e => MaterialQueryMatcher.MatchEntry(query, e.entry));
        }

        Console.WriteLine($"  search '{label}': {hits} results in {timer.Elapsed.TotalMilliseconds:0} ms");
    }
}
