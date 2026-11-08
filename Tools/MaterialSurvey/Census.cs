using StudioCore.Renderer;
using System.Globalization;
using System.Text;
using System.Text.Json;

/// <summary>
/// Totals the meshes of a game by shader, token and sampler role, and writes the census files.
/// </summary>
class Census
{
    readonly string _game;
    readonly Dictionary<string, MaterialDefinition> _definitions;
    readonly Dictionary<string, string> _matbinSource;
    readonly List<MeshUse> _meshes;
    readonly Dictionary<string, HashSet<string>> _placedMaps;

    public Census(string game, Dictionary<string, MaterialDefinition> definitions, Dictionary<string, string> matbinSource,
        List<MeshUse> meshes, Dictionary<string, HashSet<string>> placedMaps)
    {
        _game = game;
        _definitions = definitions;
        _matbinSource = matbinSource;
        _meshes = meshes;
        _placedMaps = placedMaps;
    }

    /// <summary>A file's name without folders or extensions: N:\...\M[A]_foo.matxml and foo.matbin become their stem</summary>
    public static string Key(string path)
    {
        var name = path.Replace('\\', '/');
        name = name[(name.LastIndexOf('/') + 1)..];
        var dot = name.IndexOf('.');
        return (dot >= 0 ? name[..dot] : name).ToLowerInvariant();
    }

    /// <summary>The same files <c>MaterialData.LoadShaderTable</c> reads</summary>
    public static MaterialShaderTable LoadShaderTable(string game)
    {
        var folder = Path.Combine("Assets", "Material Definitions");
        T Read<T>(string suffix) where T : new()
        {
            var path = Path.Combine(folder, $"{game}{suffix}.json");
            return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path)) ?? new T() : new T();
        }

        var table = Read<MaterialShaderTable>("");
        table.TextureGroups = Read<Dictionary<string, Dictionary<int, string>>>("_TextureGroups");
        table.TextureInputs = Read<Dictionary<string, Dictionary<int, MaterialTextureInput>>>("_TextureInputs");
        table.Blends = Read<Dictionary<string, MaterialBlendRecipe>>("_Blends");
        table.Details = Read<Dictionary<string, MaterialDetailRecipe>>("_Details");
        table.Magma = Read<Dictionary<string, MaterialMagmaRecipe>>("_Magma");
        table.Water = Read<Dictionary<string, MaterialWaterRecipe>>("_Water");
        table.Fur = Read<Dictionary<string, MaterialFurRecipe>>("_Fur");
        table.Metallic = Read<Dictionary<string, Dictionary<int, string>>>("_Metallic");
        Console.WriteLine($"Shader table: {table.Shaders.Count} entries, {table.TextureGroups.Count} UV group shaders, " +
            $"{table.Blends.Count} blends, {table.Details.Count} details, {table.Magma.Count} magma, {table.Water.Count} water, {table.Fur.Count} fur");
        return table;
    }

    /// <summary>
    /// Where the viewport sends a texture of this material, following the order of
    /// <c>FlverResource.ProcessMaterialTexture</c>. A copy for the census only: Phase 3 replaces it with
    /// the renderer's own decision, so the two can't drift. "Dropped" means it is bound nowhere.
    /// </summary>
    public static string Route(MaterialDefinition d, MaterialSampler s)
    {
        var type = s.Type;
        if (d.Deformer != MaterialDeformer.None && d.DeformerTextures.ContainsValue(s.Path) && s.Path != "") return "Deformer";
        if (d.Magma?.TextureType == type) return "Magma";
        if (d.Detail is { } overlay && (type == overlay.AlbedoType || type == overlay.NormalType)) return "Overlay";
        if (d.Water != null && d.SurfaceType != null && s.Role == "NormalMap") return "Water";
        if (d.Fur != null && s.Role is "AlbedoMap" or "NormalMap") return "Fur";

        var knownBlend = d.BlendWeight == MaterialBlendWeight.Recipe || (d.BlendWeight == MaterialBlendWeight.TexCoord1X && d.BlendLayers.Count == 2);
        if (knownBlend)
        {
            if (d.Blend != null)
            {
                if (s.Role is "AlbedoMap" or "NormalMap") return "BlendLayer";
                if (type == d.Blend.MaskType) return "BlendMask";
                if (type == d.Blend.DetailType) return "BlendDetailMask";
            }
            else
            {
                for (var i = 0; i < d.BlendLayers.Count; i++)
                {
                    if (d.BlendLayers[i].Albedo == type) return $"BlendLayer";
                    if (d.BlendLayers[i].Normal == type) return $"BlendLayer";
                }

                if (s.Role is "AlbedoMap" or "NormalMap") return "BlendLayer";
            }
        }

        if (type.Contains("DOLTexture", StringComparison.OrdinalIgnoreCase)) return "Lightmap";
        var samplerName = type[(type.LastIndexOf("Texture2D_", StringComparison.Ordinal) + 1)..];
        if (samplerName.Contains("Emissive", StringComparison.OrdinalIgnoreCase)) return "Emissive";

        // FlverResource.ChooseMetallic: the bottom layer's metallic map is drawn, the others are left out.
        // Chosen here by the material's own paths only, where the viewport also counts the model's
        if (s.Role == "MetallicMap")
        {
            var withOffset = d.MetallicOffsets.ContainsKey(type) ? "+Offset" : "";
            // Each known blend layer has its own, from MaterialDefinition.ResolveMetallic
            var knownLayers = d.BlendWeight == MaterialBlendWeight.Recipe || (d.BlendWeight == MaterialBlendWeight.TexCoord1X && d.BlendLayers.Count == 2);
            if (knownLayers && d.BlendLayers.Exists(l => l.Metallic != null))
            {
                var index = d.BlendLayers.FindIndex(l => l.Metallic == type);
                return index >= 0 ? $"MetallicLayer{index}{withOffset}" : "MetallicNoLayer";
            }

            var first = d.Samplers.Where(x => x.Role == "MetallicMap" && x.Path != "")
                .OrderBy(x => Math.Max(x.Suffix, 0)).ThenBy(x => x.Slot).FirstOrDefault() ?? s;
            return first.Type == type ? $"Metallic{withOffset}" : "MetallicOtherLayer";
        }

        var upper = samplerName.ToUpperInvariant();
        if (upper is "G_DIFFUSETEXTURE" or "G_DIFFUSE" || upper.Contains("ALBEDO")) return "Albedo";
        if (upper is "G_BUMPMAPTEXTURE" or "G_BUMPMAP" || upper.Contains("NORMAL")) return "Normal";
        if (upper is "G_SPECULARTEXTURE" or "G_SPECULAR" || upper.Contains("SPECULAR")) return "Specular";
        if (upper is "G_SHININESSTEXTURE" or "G_SHININESS" || upper.Contains("SHININESS")) return "Shininess";
        if (upper.Contains("BLENDMASK")) return "BlendMask";
        return "Dropped";
    }

    static string Features(MaterialDefinition d)
    {
        List<string> f = [];
        if (d.Blend != null) f.Add("BlendRecipe");
        else if (d.BlendWeight == MaterialBlendWeight.TexCoord1X) f.Add("BlendTexCoord1X");
        else if (d.BlendLayerCount > 1) f.Add($"Layers{d.BlendLayerCount}Unweighted");
        if (d.Detail != null) f.Add("Overlay");
        if (d.Magma != null) f.Add("Magma");
        if (d.Surface != MaterialSurface.Standard) f.Add(d.Surface.ToString());
        if (d.Deformer != MaterialDeformer.None) f.Add(d.Deformer.ToString());
        if (d.Samplers.Any(s => s.Scroll.Any(v => v != 0))) f.Add("Scroll");
        if (d.BlendMode != 0) f.Add($"BlendMode{d.BlendMode}");
        if (d.AlphaRef > 0) f.Add("AlphaTest");
        return string.Join(" ", f);
    }

    class Tally
    {
        public long Meshes, Triangles, Placed;
        public HashSet<string> Models = new(), Materials = new(), Maps = new(), Shaders = new();
        public Dictionary<string, long> Categories = new();
        public Dictionary<string, long> Routes = new();
        public (string model, long weight) Example;

        public void Add(MeshUse m, string material, string shader, IEnumerable<string> maps)
        {
            Meshes++;
            Triangles += m.Triangles;
            Placed += m.PlacedTriangles;
            Models.Add(m.Model);
            Materials.Add(material);
            Shaders.Add(shader);
            foreach (var map in maps) Maps.Add(map);
            Categories[m.Category] = Categories.GetValueOrDefault(m.Category) + 1;
            // The example is the model this is most of, by what is placed first, else by size
            var weight = m.PlacedTriangles > 0 ? m.PlacedTriangles * 1000 : m.Triangles;
            if (weight > Example.weight) Example = (m.Flver, weight);
        }
    }

    public void Write(string outDir, List<string> failures)
    {
        var shaders = new Dictionary<string, Tally>();
        var tokens = new Dictionary<string, Tally>();
        var roles = new Dictionary<string, Tally>();
        var dropped = new Dictionary<(string shader, string role, int slot), Tally>();
        var unmatched = new Dictionary<string, Tally>();
        var flverOnly = new Dictionary<(string shader, string type), Tally>();
        var routeTotals = new Dictionary<string, (long meshes, long placed)>();
        long totalTriangles = _meshes.Sum(m => (long)m.Triangles), totalPlaced = _meshes.Sum(m => m.PlacedTriangles);

        foreach (var m in _meshes)
        {
            var maps = _placedMaps.TryGetValue(m.Model, out var set) && m.Placements > 0 ? set : (IEnumerable<string>)[];
            if (!_definitions.TryGetValue(m.Material, out var d))
            {
                Get(unmatched, m.Material).Add(m, m.Material, "", maps);
                continue;
            }

            var shader = d.ShaderName;
            Get(shaders, shader).Add(m, d.Name, shader, maps);
            foreach (var token in d.ShaderTokens.Distinct()) Get(tokens, token).Add(m, d.Name, shader, maps);

            var flverTextures = m.Textures.Where(t => t.Type != "").GroupBy(t => t.Type).ToDictionary(g => g.Key, g => g.First().Path);
            // Each mesh counts once per role, route and dropped slot; the route counts are of textures
            HashSet<string> meshRoles = [], meshRoutes = [];
            HashSet<(string, int)> meshDropped = [];
            foreach (var s in d.Samplers)
            {
                // A texture counts if either the material or the model gives it a file
                var hasPath = s.Path != "" || (flverTextures.TryGetValue(s.Type, out var p) && p != "");
                if (!hasPath) continue;

                var route = Route(d, s);
                var role = Get(roles, s.Role);
                if (meshRoles.Add(s.Role)) role.Add(m, d.Name, shader, maps);
                role.Routes[route] = role.Routes.GetValueOrDefault(route) + 1;
                var shaderTally = shaders[shader];
                shaderTally.Routes[$"{s.Role}>{route}"] = shaderTally.Routes.GetValueOrDefault($"{s.Role}>{route}") + 1;
                var total = routeTotals.GetValueOrDefault(route);
                routeTotals[route] = (total.meshes + 1, total.placed + (meshRoutes.Add(route) ? m.PlacedTriangles : 0));
                if (route == "Dropped" && meshDropped.Add((s.Role, s.Slot))) Get(dropped, (shader, s.Role, s.Slot)).Add(m, d.Name, shader, maps);
            }

            foreach (var (type, _) in flverTextures)
                if (!d.Samplers.Exists(s => s.Type == type))
                    Get(flverOnly, (shader, type)).Add(m, d.Name, shader, maps);
        }

        string Share(long v, long of) => of == 0 ? "0" : (100.0 * v / of).ToString("F3", CultureInfo.InvariantCulture);
        string Cats(Tally t) => string.Join(" ", t.Categories.OrderByDescending(x => x.Value).Select(x => $"{x.Key}:{x.Value}"));
        string RoutesOf(Tally t) => string.Join(" ", t.Routes.OrderByDescending(x => x.Value).Select(x => $"{x.Key}:{x.Value}"));

        // Shaders
        var shaderRows = shaders.OrderByDescending(x => x.Value.Placed).ThenByDescending(x => x.Value.Triangles).ToList();
        var sb = new StringBuilder("Shader\tKnown\tFeatures\tPlacedTriangles\tPlacedShare%\tTriangles\tTriangleShare%\tMeshes\tModels\tMaterials\tMaps\tCategories\tTextureRoutes\tExampleModel\n");
        foreach (var (name, t) in shaderRows)
        {
            var d = _definitions[t.Materials.First()];
            sb.Append($"{name}\t{d.IsKnownShader}\t{Features(d)}\t{t.Placed}\t{Share(t.Placed, totalPlaced)}\t{t.Triangles}\t{Share(t.Triangles, totalTriangles)}\t");
            sb.Append($"{t.Meshes}\t{t.Models.Count}\t{t.Materials.Count}\t{t.Maps.Count}\t{Cats(t)}\t{RoutesOf(t)}\t{t.Example.model}\n");
        }
        File.WriteAllText(Path.Combine(outDir, $"{_game}_Shaders.tsv"), sb.ToString());

        // Tokens
        sb = new StringBuilder("Token\tShaders\tPlacedTriangles\tPlacedShare%\tTriangles\tTriangleShare%\tMeshes\tMaterials\tExampleModel\n");
        foreach (var (name, t) in tokens.OrderByDescending(x => x.Value.Placed).ThenByDescending(x => x.Value.Triangles))
            sb.Append($"{name}\t{t.Shaders.Count}\t{t.Placed}\t{Share(t.Placed, totalPlaced)}\t{t.Triangles}\t{Share(t.Triangles, totalTriangles)}\t{t.Meshes}\t{t.Materials.Count}\t{t.Example.model}\n");
        File.WriteAllText(Path.Combine(outDir, $"{_game}_Tokens.tsv"), sb.ToString());

        // Roles
        sb = new StringBuilder("Role\tRoutes(meshes)\tShaders\tMaterials\tMeshes\tPlacedTriangles\tPlacedShare%\tTriangles\tExampleModel\n");
        foreach (var (name, t) in roles.OrderByDescending(x => x.Value.Meshes))
            sb.Append($"{name}\t{RoutesOf(t)}\t{t.Shaders.Count}\t{t.Materials.Count}\t{t.Meshes}\t{t.Placed}\t{Share(t.Placed, totalPlaced)}\t{t.Triangles}\t{t.Example.model}\n");
        File.WriteAllText(Path.Combine(outDir, $"{_game}_Roles.tsv"), sb.ToString());

        // Dropped textures: what the game samples that the viewport binds nowhere
        sb = new StringBuilder("Shader\tRole\tSlot\tPlacedTriangles\tPlacedShare%\tTriangles\tMeshes\tMaterials\tExampleModel\n");
        foreach (var ((shader, role, slot), t) in dropped.OrderByDescending(x => x.Value.Placed).ThenByDescending(x => x.Value.Triangles))
            sb.Append($"{shader}\t{role}\t{slot}\t{t.Placed}\t{Share(t.Placed, totalPlaced)}\t{t.Triangles}\t{t.Meshes}\t{t.Materials.Count}\t{t.Example.model}\n");
        File.WriteAllText(Path.Combine(outDir, $"{_game}_Dropped.tsv"), sb.ToString());

        // Textures a model lists that its material has no sampler for
        sb = new StringBuilder("Shader\tFlverTextureType\tMeshes\tPlacedTriangles\tExampleModel\n");
        foreach (var ((shader, type), t) in flverOnly.OrderByDescending(x => x.Value.Meshes))
            sb.Append($"{shader}\t{type}\t{t.Meshes}\t{t.Placed}\t{t.Example.model}\n");
        File.WriteAllText(Path.Combine(outDir, $"{_game}_FlverOnlyTextures.tsv"), sb.ToString());

        // Summary
        var md = new StringBuilder();
        md.AppendLine($"# {_game} material census");
        md.AppendLine();
        md.AppendLine($"Generated {DateTime.Now:yyyy-MM-dd} by `Tools/MaterialSurvey`. Placed triangles = LOD0 triangles x MSB placements;");
        md.AppendLine("characters and parts count only where an MSB places them (enemies), so the unplaced columns cover the rest.");
        md.AppendLine();
        md.AppendLine("| | Count |");
        md.AppendLine("|---|---|");
        md.AppendLine($"| MATBINs in the game | {_definitions.Count} |");
        md.AppendLine($"| MATBINs used by a mesh | {shaders.Values.SelectMany(t => t.Materials).Distinct().Count()} |");
        md.AppendLine($"| Shaders used | {shaders.Count} ({shaders.Count(s => _definitions[s.Value.Materials.First()].IsKnownShader)} with a shader table entry) |");
        md.AppendLine($"| Meshes | {_meshes.Count} |");
        md.AppendLine($"| Triangles (LOD0, each model once) | {totalTriangles:N0} |");
        md.AppendLine($"| Placed triangles | {totalPlaced:N0} |");
        md.AppendLine($"| Meshes whose material is not in the MATBIN binders | {unmatched.Values.Sum(t => t.Meshes)} ({unmatched.Count} names) |");
        md.AppendLine($"| Read failures | {failures.Count} |");
        md.AppendLine();

        md.AppendLine("## How many shaders cover the game");
        md.AppendLine();
        md.AppendLine("| Share | Shaders by placed triangles | Shaders by triangles |");
        md.AppendLine("|---|---|---|");
        foreach (var target in new[] { 50, 80, 90, 95, 99, 100 })
            md.AppendLine($"| {target}% | {Needed(shaders.Values.Select(t => t.Placed), totalPlaced, target)} | {Needed(shaders.Values.Select(t => t.Triangles), totalTriangles, target)} |");
        md.AppendLine();

        md.AppendLine("## Where textures go");
        md.AppendLine();
        md.AppendLine("Every sampler with a file, routed as `FlverResource.ProcessMaterialTexture` would route it (census copy of that logic).");
        md.AppendLine();
        md.AppendLine("| Route | Textures | Placed triangles of meshes with one |");
        md.AppendLine("|---|---|---|");
        foreach (var (route, (n, placed)) in routeTotals.OrderByDescending(x => x.Value.meshes))
            md.AppendLine($"| {route} | {n} | {placed:N0} |");
        md.AppendLine();

        md.AppendLine("## Roles");
        md.AppendLine();
        md.AppendLine("| Role | Routes (mesh textures) | Shaders | Meshes |");
        md.AppendLine("|---|---|---|---|");
        foreach (var (name, t) in roles.OrderByDescending(x => x.Value.Meshes))
            md.AppendLine($"| {name} | {RoutesOf(t)} | {t.Shaders.Count} | {t.Meshes} |");
        md.AppendLine();

        md.AppendLine("## Top 40 shaders by placed triangles");
        md.AppendLine();
        md.AppendLine("| Shader | Known | Features | Placed % | Triangles % | Meshes | Dropped roles | Example |");
        md.AppendLine("|---|---|---|---|---|---|---|---|");
        foreach (var (name, t) in shaderRows.Take(40))
        {
            var d = _definitions[t.Materials.First()];
            var droppedRoles = string.Join(" ", t.Routes.Where(r => r.Key.EndsWith(">Dropped")).Select(r => r.Key[..r.Key.IndexOf('>')]).Distinct());
            md.AppendLine($"| `{name}` | {(d.IsKnownShader ? "yes" : "")} | {Features(d)} | {Share(t.Placed, totalPlaced)} | {Share(t.Triangles, totalTriangles)} | {t.Meshes} | {droppedRoles} | {t.Example.model} |");
        }
        md.AppendLine();

        md.AppendLine("## Top 20 dropped textures by placed triangles");
        md.AppendLine();
        md.AppendLine("| Shader | Role | Slot | Placed % | Meshes | Example |");
        md.AppendLine("|---|---|---|---|---|---|");
        foreach (var ((shader, role, slot), t) in dropped.OrderByDescending(x => x.Value.Placed).ThenByDescending(x => x.Value.Triangles).Take(20))
            md.AppendLine($"| `{shader}` | {role} | {slot} | {Share(t.Placed, totalPlaced)} | {t.Meshes} | {t.Example.model} |");
        md.AppendLine();

        if (unmatched.Count > 0)
        {
            md.AppendLine("## Materials not found (top 15)");
            md.AppendLine();
            foreach (var (name, t) in unmatched.OrderByDescending(x => x.Value.Meshes).Take(15))
                md.AppendLine($"- `{name}`: {t.Meshes} meshes, e.g. {t.Example.model}");
            md.AppendLine();
        }

        File.WriteAllText(Path.Combine(outDir, $"{_game}_Summary.md"), md.ToString());
        File.WriteAllLines(Path.Combine(outDir, $"{_game}_Failures.txt"), failures.OrderBy(x => x));
    }

    static int Needed(IEnumerable<long> values, long total, int percent)
    {
        if (total == 0) return 0;
        long sum = 0;
        var n = 0;
        foreach (var v in values.Where(v => v > 0).OrderByDescending(v => v))
        {
            sum += v;
            n++;
            if (sum * 100 >= total * percent) break;
        }
        return n;
    }

    static Tally Get<TKey>(Dictionary<TKey, Tally> map, TKey key)
    {
        if (!map.TryGetValue(key, out var t)) map[key] = t = new Tally();
        return t;
    }
}
