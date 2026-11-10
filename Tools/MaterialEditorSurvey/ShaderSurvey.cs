using SoulsFormats;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Renderer;
using System.Text.Json;

/// <summary>
/// shaders GAME: how much the Material Editor's MaterialShaderInfo explains of vanilla materials, and whether
/// the rules F2 would apply (a sampler for every slot the shader samples; params every material of a shader has)
/// hold in vanilla.
/// </summary>
public static class ShaderSurvey
{
    /// <summary>The same files MaterialData.LoadShaderTable reads</summary>
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
        table.UvSets = Read<Dictionary<string, Dictionary<int, string>>>("_UvSets");
        table.MergeBlends(Read<Dictionary<string, MaterialBlendRecipe>>("_GeneratedBlends"));
        table.MergeOverlays(Read<Dictionary<string, MaterialDetailRecipe>>("_Overlays"));
        table.MergeDetailStacks(Read<Dictionary<string, MaterialDetailRecipe>>("_DetailStacks"));
        table.MergeColorAdjust(Read<Dictionary<string, MaterialColorAdjustRecipe>>("_ColorAdjust"));
        table.EmissiveMasks = Read<Dictionary<string, MaterialEmissiveMask>>("_EmissiveMasks");
        table.MergeFurMasks(Read<Dictionary<string, MaterialFurMask>>("_FurMasks"));
        return table;
    }

    private record Mat(string Name, string ShaderPath, List<(string name, string type, object value)> Params, List<(string type, int slot)> Samplers, int StaticSwitch);

    public static void Run(GameMaterials g)
    {
        Console.WriteLine("== shaders (F2 coverage)");

        var table = LoadShaderTable(g.Name == "DB_NT" ? "DB_NT" : g.Name);
        Console.WriteLine($"  table: {table.Shaders.Count} shader entries, {table.TextureGroups.Count} with UV groups, {table.TextureInputs.Count} texture inputs, {table.Blends.Count} blends, {table.Details.Count} details");

        var mats = new List<Mat>();
        foreach (var b in g.Binders.Where(b => b.Selected))
        {
            foreach (var (file, data, _) in GameMaterials.Files(b))
            {
                try
                {
                    if (b.Kind == MaterialKind.MATBIN)
                    {
                        var m = MATBIN.Read(data);
                        var sw = m.Params.FirstOrDefault(p => p.Name == "GXFT_SATStaticSwitch")?.Value is int i ? i : 0;
                        mats.Add(new(GameMaterials.ShortName(file.Name), m.ShaderPath,
                            m.Params.Select(p => (p.Name, p.Type.ToString(), p.Value)).ToList(),
                            m.Samplers.Select(s => (s.Type, MaterialShaderInfo.SlotOf(s.Type))).ToList(), sw));
                    }
                    else
                    {
                        var m = MTD.Read(data);
                        mats.Add(new(GameMaterials.ShortName(file.Name), m.ShaderPath,
                            m.Params.Select(p => (p.Name, p.Type.ToString(), p.Value)).ToList(),
                            m.Textures.Select(t => (t.Type, MaterialShaderInfo.SlotOf(t.Type))).ToList(), 0));
                    }
                }
                catch { }
            }
        }

        // Coverage
        var known = 0;
        var paramTotal = 0;
        var paramLabelled = 0;
        var unlabelled = new Dictionary<string, int>();
        var samplerTotal = 0;
        var samplerWithSlot = 0;
        var samplerGrouped = 0;
        var samplerUsed = 0;
        var missingSlotMats = 0;
        var missingSlotExamples = new List<string>();
        var groupedShaders = 0;

        foreach (var m in mats)
        {
            var info = MaterialShaderInfo.Build(table, m.ShaderPath, m.StaticSwitch);
            if (info.Known) known++;

            foreach (var (name, _, _) in m.Params)
            {
                paramTotal++;
                if (info.UsesOfParam(name).Count > 0)
                {
                    paramLabelled++;
                }
                else
                {
                    var shortName = MaterialShaderInfo.ShortParamName(name);
                    unlabelled[shortName] = unlabelled.GetValueOrDefault(shortName) + 1;
                }
            }

            foreach (var (_, slot) in m.Samplers)
            {
                samplerTotal++;
                if (slot >= 0) samplerWithSlot++;
                if (info.SlotGroups.ContainsKey(slot)) samplerGrouped++;
                if (info.UsesOfSlot(slot).Count > 0 || info.ParamUses.Values.Any(u => u.Any(x => x.Slot == slot))) samplerUsed++;
            }

            if (info.SlotGroups.Count > 0)
            {
                groupedShaders++;
                var have = m.Samplers.Select(s => s.slot).ToHashSet();
                var missing = info.SlotGroups.Keys.Where(k => !have.Contains(k)).OrderBy(k => k).ToList();
                if (missing.Count > 0)
                {
                    missingSlotMats++;
                    if (missingSlotExamples.Count < 4)
                        missingSlotExamples.Add($"{m.Name} ({info.ShaderName}): no sampler for slot {string.Join(",", missing)}");
                }
            }
        }

        Console.WriteLine($"  {mats.Count} materials, {known} with a shader the table knows ({Pct(known, mats.Count)})");
        Console.WriteLine($"  params: {paramLabelled}/{paramTotal} get a use ({Pct(paramLabelled, paramTotal)}); commonest without one: " +
            string.Join(", ", unlabelled.OrderByDescending(kv => kv.Value).Take(8).Select(kv => $"{kv.Key} x{kv.Value}")));
        Console.WriteLine($"  samplers: {samplerTotal}, {samplerWithSlot} with a slot number, {samplerGrouped} in a UV group, {samplerUsed} with a use");
        Console.WriteLine($"  slot check: of {groupedShaders} materials whose shader lists its slots, {missingSlotMats} lack a sampler for one of them");
        foreach (var e in missingSlotExamples) Console.WriteLine($"      e.g. {e}");

        // Templates: per shader path, the params and samplers every / most of its materials have
        var byShader = mats.GroupBy(m => m.ShaderPath, StringComparer.OrdinalIgnoreCase).ToList();
        var multi = byShader.Where(s => s.Count() >= 2).ToList();
        var sameParams = multi.Count(s => s.Select(m => string.Join("|", m.Params.Select(p => p.name).OrderBy(x => x))).Distinct().Count() == 1);
        var sameSamplers = multi.Count(s => s.Select(m => string.Join("|", m.Samplers.Select(p => p.type).OrderBy(x => x))).Distinct().Count() == 1);
        var sameTypes = byShader.Sum(s => s.SelectMany(m => m.Params).GroupBy(p => p.name).Count(gr => gr.Select(p => p.type).Distinct().Count() > 1));

        // Materials missing a param or sampler that all other materials of their shader have (the "Add missing" rule)
        var lacking = 0;
        var lackingExamples = new List<string>();
        foreach (var s in multi)
        {
            var list = s.ToList();
            foreach (var m in list)
            {
                var others = list.Where(o => o != m).ToList();
                var commonParams = others.Select(o => o.Params.Select(p => p.name).ToHashSet()).Aggregate((a, b) => { a.IntersectWith(b); return a; });
                var commonSamplers = others.Select(o => o.Samplers.Select(p => p.type).ToHashSet()).Aggregate((a, b) => { a.IntersectWith(b); return a; });
                var missP = commonParams.Except(m.Params.Select(p => p.name)).ToList();
                var missS = commonSamplers.Except(m.Samplers.Select(p => p.type)).ToList();
                if (missP.Count + missS.Count > 0)
                {
                    lacking++;
                    if (lackingExamples.Count < 4)
                        lackingExamples.Add($"{m.Name}: {missP.Count} params, {missS.Count} samplers (e.g. {string.Join(", ", missP.Concat(missS).Take(2))}) of {others.Count} others");
                }
            }
        }

        Console.WriteLine($"  templates: {byShader.Count} shader paths, {multi.Count} used by 2+ materials; " +
            $"{sameParams} of those have the same param names in every material, {sameSamplers} the same samplers; " +
            $"{sameTypes} (shader, param) pairs with more than one type");
        Console.WriteLine($"  'add missing' rule (what every other material of the shader has): {lacking} materials would be offered something");
        foreach (var e in lackingExamples) Console.WriteLine($"      e.g. {e}");
    }

    private static string Pct(int a, int b) => b == 0 ? "-" : $"{100.0 * a / b:0.0}%";
}
