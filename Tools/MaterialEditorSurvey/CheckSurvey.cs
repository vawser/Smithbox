using SoulsFormats;
using StudioCore.Editors.MaterialEditor;

/// <summary>
/// check GAME: runs the Material Editor's MaterialChecks over every vanilla material. Vanilla files are what
/// the checks call valid, so any finding here is a check that is wrong or too strict.
/// </summary>
public static class CheckSurvey
{
    public static void Run(GameMaterials g)
    {
        Console.WriteLine("== check (MaterialChecks over vanilla)");

        var files = 0;
        var byCode = new Dictionary<string, List<string>>();

        foreach (var b in g.Binders.Where(b => b.Selected))
        {
            foreach (var (file, data, _) in GameMaterials.Files(b))
            {
                object material;
                try { material = b.Kind == MaterialKind.MTD ? MTD.Read(data) : MATBIN.Read(data); }
                catch { continue; }

                files++;

                foreach (var f in MaterialChecks.Check(material))
                {
                    var code = $"{f.Severity} {f.Code}";
                    if (!byCode.TryGetValue(code, out var list))
                        byCode[code] = list = new();
                    list.Add($"{GameMaterials.ShortName(file.Name)}: {string.Join(", ", f.Args)}");
                }
            }
        }

        Console.WriteLine($"  {files} materials, {byCode.Values.Sum(l => l.Count)} findings");

        foreach (var (code, list) in byCode.OrderBy(kv => kv.Key))
        {
            Console.WriteLine($"    {code}: {list.Count} in {list.Select(l => l.Split(':')[0]).Distinct().Count()} files");
            foreach (var example in list.Take(3))
                Console.WriteLine($"        e.g. {example}");
        }
    }
}
