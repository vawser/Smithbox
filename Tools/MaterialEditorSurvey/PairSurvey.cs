using SoulsFormats;

/// <summary>
/// pairs GAME: for the material names a game has as both an MTD and a MATBIN (ER/NR keep an MTD binder beside the
/// MATBINs), how the two compare: shader, param names, values, texture types and paths. Shows whether converting
/// one form to the other is defined by the game's own data, for the Material Editor plan's porting question.
/// </summary>
public static class PairSurvey
{
    public static void Run(GameMaterials g)
    {
        Console.WriteLine("== pairs (MTD vs MATBIN of the same name)");

        var mtds = new Dictionary<string, MTD>(StringComparer.OrdinalIgnoreCase);
        var matbins = new Dictionary<string, MATBIN>(StringComparer.OrdinalIgnoreCase);

        foreach (var b in g.Binders.Where(b => b.Selected))
        {
            foreach (var (file, data, _) in GameMaterials.Files(b))
            {
                try
                {
                    var name = GameMaterials.ShortName(file.Name);
                    if (b.Kind == MaterialKind.MTD) mtds[name] = MTD.Read(data);
                    else matbins[name] = MATBIN.Read(data);
                }
                catch { }
            }
        }

        var names = mtds.Keys.Where(matbins.ContainsKey).ToList();
        Console.WriteLine($"  {mtds.Count} MTDs, {matbins.Count} MATBINs, {names.Count} names in both");
        if (names.Count == 0)
            return;

        var sameShader = 0;
        var sameParamNames = 0;
        var mtdParamsInMatbin = 0;
        var mtdParamsTotal = 0;
        var sameValues = 0;
        var comparedValues = 0;
        var sameTextureTypes = 0;
        var samePaths = 0;
        var comparedPaths = 0;
        var matbinOnlyParams = new Dictionary<string, int>();
        var examples = new List<string>();

        string Stem(string p) => Path.GetFileNameWithoutExtension((p ?? "").Replace('\\', '/').Split('/')[^1]);
        string Short(string n) { var i = (n ?? "").IndexOf("snp_", StringComparison.Ordinal); return i > 0 ? n[i..] : n ?? ""; }
        string Text(object v) => v is Array a ? string.Join(",", a.Cast<object>()) : $"{v}";

        foreach (var name in names)
        {
            var mtd = mtds[name];
            var mb = matbins[name];

            if (string.Equals(Stem(mtd.ShaderPath), Stem(mb.ShaderPath), StringComparison.OrdinalIgnoreCase)) sameShader++;
            else if (examples.Count < 3) examples.Add($"{name}: shader {Stem(mtd.ShaderPath)} vs {Stem(mb.ShaderPath)}");

            var mtdNames = mtd.Params.Select(p => Short(p.Name)).ToHashSet(StringComparer.Ordinal);
            var mbNames = mb.Params.Select(p => Short(p.Name)).ToHashSet(StringComparer.Ordinal);
            if (mtdNames.SetEquals(mbNames)) sameParamNames++;

            foreach (var p in mtd.Params)
            {
                mtdParamsTotal++;
                var match = mb.Params.FirstOrDefault(q => Short(q.Name) == Short(p.Name));
                if (match == null) continue;
                mtdParamsInMatbin++;
                comparedValues++;
                if (Text(p.Value) == Text(match.Value)) sameValues++;
            }

            foreach (var q in mb.Params.Where(q => !mtdNames.Contains(Short(q.Name))))
                matbinOnlyParams[Short(q.Name)] = matbinOnlyParams.GetValueOrDefault(Short(q.Name)) + 1;

            var mtdTypes = mtd.Textures.Select(t => t.Type).ToHashSet(StringComparer.Ordinal);
            var mbTypes = mb.Samplers.Select(s => s.Type).ToHashSet(StringComparer.Ordinal);
            if (mtdTypes.SetEquals(mbTypes)) sameTextureTypes++;

            foreach (var t in mtd.Textures.Where(t => t.Extended))
            {
                var s = mb.Samplers.FirstOrDefault(x => x.Type == t.Type);
                if (s == null) continue;
                comparedPaths++;
                if (string.Equals(Stem(t.Path), Stem(s.Path), StringComparison.OrdinalIgnoreCase)) samePaths++;
            }
        }

        Console.WriteLine($"  same shader name: {sameShader}/{names.Count}");
        Console.WriteLine($"  same param names (shader prefix stripped): {sameParamNames}/{names.Count}; " +
            $"{mtdParamsInMatbin}/{mtdParamsTotal} MTD params have a MATBIN param of the name, {sameValues}/{comparedValues} of those with the same value");
        Console.WriteLine($"  same texture/sampler types: {sameTextureTypes}/{names.Count}; same texture name in {samePaths}/{comparedPaths} extended textures");
        Console.WriteLine($"  params only the MATBIN has, commonest: {string.Join(", ", matbinOnlyParams.OrderByDescending(kv => kv.Value).Take(6).Select(kv => $"{kv.Key} x{kv.Value}"))}");
        foreach (var e in examples) Console.WriteLine($"      e.g. {e}");
    }
}
