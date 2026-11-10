#nullable enable
using StudioCore.Renderer;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Where a material's param or texture slot is used, as far as the renderer's shader table knows.
/// <see cref="Section"/> is a fixed code (Deformer, TextureInput, Blend, Detail, Magma, Water, Fur, Metallic,
/// EmissiveMask, Engine, Tiling) the editor localizes; <see cref="Detail"/> is the field it fills, as the table
/// names it (Layers[0].Sharpness, Speed, Color).
/// </summary>
public record MaterialShaderUse(string Section, string Detail, int? Slot = null);

/// <summary>
/// What the shader table (Assets/Material Definitions/{GAME}*.json, generated from the game's compiled shaders)
/// says about one shader: which params feed which part of it, and what each texture slot is for. Built the way
/// MaterialDefinition looks entries up (shader name, or shader name@static switch), so what it shows is what the
/// viewport uses. No editor dependencies, so the survey tool can link it.
/// </summary>
public class MaterialShaderInfo
{
    public string ShaderName { get; private set; } = "";

    /// <summary>Whether any section of the table has an entry for this shader</summary>
    public bool Known { get; private set; }

    public MaterialDeformer Deformer { get; private set; }

    /// <summary>Short param name (snp_0_float_5) to its uses</summary>
    public Dictionary<string, List<MaterialShaderUse>> ParamUses { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Texture slot (the N of Texture2D_N) to its uses</summary>
    public Dictionary<int, List<MaterialShaderUse>> SlotUses { get; } = new();

    /// <summary>Texture slot to its UV group, whose tiling is the group's {group}_CommonUV-UVParam</summary>
    public Dictionary<int, string> SlotGroups { get; } = new();

    /// <summary>Texture slot to the mesh UV set it samples</summary>
    public Dictionary<int, string> SlotUvSets { get; } = new();

    private static readonly Regex ParamToken = new(@"snp_\d+_[A-Za-z]+\d*_\d+", RegexOptions.Compiled);
    private static readonly Regex SlotName = new(@"Texture2D_(\d+)_", RegexOptions.Compiled);
    private static readonly Regex GroupTiling = new(@"^(group_\d+)_CommonUV-UVParam$", RegexOptions.Compiled);

    // Int fields of the recipes that hold a texture slot
    private static readonly HashSet<string> SlotFields = new()
    {
        "Albedo", "Normal", "Mask", "DetailMask", "Texture", "Slot", "Cut", "Masked",
        "BackAlbedo", "BackNormal", "StackWeightMap"
    };

    // Params the renderer reads by name, whatever the shader, and what for
    private static readonly Dictionary<string, string> EngineParams = new(StringComparer.Ordinal)
    {
        ["g_BlendMode"] = "BlendMode",
        ["g_AlphaRef"] = "AlphaRef",
        ["g_GX_PrimCullNone"] = "CullNone",
        ["g_GX_bDisableDepthWrite"] = "DisableDepthWrite",
        ["g_GX_bMeshDecal"] = "MeshDecal",
        ["g_DiffuseMapColor"] = "DiffuseColor",
        ["GXFT_SATStaticSwitch"] = "StaticSwitch",
        ["g_TexScroll_0"] = "TexScroll",
        ["g_LightingType"] = "LightingType",
    };

    /// <summary>
    /// The shader's name from its path, as MaterialDefinition takes it
    /// </summary>
    public static string ShaderNameOf(string shaderPath) =>
        Path.GetFileNameWithoutExtension((shaderPath ?? "").Split('\\', '/')[^1]);

    /// <summary>
    /// Strips the shader prefix: M_Flag__Mb2__snp_0_float_20 becomes snp_0_float_20, as MaterialDefinition does
    /// </summary>
    public static string ShortParamName(string name)
    {
        name ??= "";
        var index = name.IndexOf("snp_", StringComparison.Ordinal);
        return index > 0 ? name[index..] : name;
    }

    /// <summary>
    /// The N of a sampler's Texture2D_N name, or -1
    /// </summary>
    public static int SlotOf(string samplerType)
    {
        var match = SlotName.Match(samplerType ?? "");
        return match.Success ? int.Parse(match.Groups[1].Value) : -1;
    }

    public static MaterialShaderInfo Build(MaterialShaderTable? table, string shaderPath, int staticSwitch)
    {
        var info = new MaterialShaderInfo { ShaderName = ShaderNameOf(shaderPath) };

        if (table == null)
            return info;

        var name = info.ShaderName;

        // The keys MaterialDefinition tries for the permutation sections, in its order
        var permutationKeys = new[] { $"{name}@{staticSwitch}", $"{name}@0", name };

        if (table.Shaders.TryGetValue(name, out var entry))
        {
            info.Known = true;
            info.Deformer = entry.Deformer;

            foreach (var (role, param) in entry.Params)
                info.AddParams(param, "Deformer", role);

            foreach (var (role, slot) in entry.Textures)
                info.AddSlot(slot, "Deformer", role);
        }

        foreach (var key in permutationKeys)
        {
            if (!table.TextureInputs.TryGetValue(key, out var inputs))
                continue;

            info.Known = true;
            foreach (var (slot, input) in inputs)
            {
                info.AddSlot(slot, "TextureInput", "");
                info.Walk(input, "TextureInput", "", slot);
            }
            break;
        }

        info.WalkFirst(table.Blends, permutationKeys, "Blend");
        info.WalkFirst(table.Details, permutationKeys, "Detail");
        info.WalkFirst(table.Magma, permutationKeys, "Magma");
        info.WalkFirst(table.EmissiveMasks, permutationKeys, "EmissiveMask");

        if (table.Water.TryGetValue(name, out var water))
        {
            info.Known = true;
            info.Walk(water, "Water", "", null);
        }

        if (table.Fur.TryGetValue(name, out var fur))
        {
            info.Known = true;
            info.Walk(fur, "Fur", "", null);
        }

        if (table.Metallic.TryGetValue(name, out var metallic))
        {
            info.Known = true;
            foreach (var (slot, offset) in metallic)
            {
                info.AddSlot(slot, "Metallic", "");
                info.AddParams(offset, "Metallic", "Offset", slot);
            }
        }

        if (table.TextureGroups.TryGetValue(name, out var groups))
        {
            info.Known = true;
            foreach (var (slot, group) in groups)
                info.SlotGroups[slot] = group;
        }

        if (table.UvSets.TryGetValue(name, out var uvSets))
        {
            foreach (var (slot, set) in uvSets)
                info.SlotUvSets[slot] = set;
        }

        return info;
    }

    /// <summary>
    /// What a param of a material with this shader is used for. Includes the params the renderer reads whatever
    /// the shader, and the tiling param of each UV group.
    /// </summary>
    public List<MaterialShaderUse> UsesOfParam(string paramName)
    {
        var uses = new List<MaterialShaderUse>();

        if (EngineParams.TryGetValue(paramName ?? "", out var engine))
            uses.Add(new("Engine", engine));

        var tiling = GroupTiling.Match(paramName ?? "");
        if (tiling.Success)
        {
            var slots = SlotGroups.Where(kv => kv.Value == tiling.Groups[1].Value).Select(kv => kv.Key).OrderBy(x => x).ToList();
            uses.Add(new("Tiling", string.Join(", ", slots)));
        }

        if (ParamUses.TryGetValue(ShortParamName(paramName ?? ""), out var found))
            uses.AddRange(found);

        return uses;
    }

    public List<MaterialShaderUse> UsesOfSlot(int slot) =>
        SlotUses.TryGetValue(slot, out var uses) ? uses : new();

    #region Walk
    private void WalkFirst<T>(Dictionary<string, T> section, string[] keys, string label)
    {
        foreach (var key in keys)
        {
            if (section.TryGetValue(key, out var recipe) && recipe != null)
            {
                Known = true;
                Walk(recipe, label, "", null);
                return;
            }
        }
    }

    /// <summary>
    /// Every param named in a recipe's text fields, and every slot in its slot fields, with the field's path
    /// </summary>
    private void Walk(object? obj, string section, string path, int? slot)
    {
        if (obj == null)
            return;

        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0)
                continue;

            var value = prop.GetValue(obj);
            var field = path == "" ? prop.Name : $"{path}.{prop.Name}";

            switch (value)
            {
                case null:
                    break;

                case string text:
                    AddParams(text, section, field, slot);
                    break;

                case int number when SlotFields.Contains(prop.Name):
                    AddSlot(number, section, field);
                    break;

                case IEnumerable<int> numbers when prop.Name.EndsWith("Slots", StringComparison.Ordinal):
                    foreach (var n in numbers)
                        AddSlot(n, section, field);
                    break;

                case IDictionary dictionary:
                    foreach (DictionaryEntry kv in dictionary)
                        Walk(kv.Value, section, $"{field}[{kv.Key}]", slot);
                    break;

                case IList list when value is not string:
                    for (var i = 0; i < list.Count; i++)
                        Walk(list[i], section, $"{field}[{i}]", slot);
                    break;

                default:
                    if (value.GetType().IsClass && value.GetType().Namespace == typeof(MaterialShaderTable).Namespace)
                        Walk(value, section, field, slot);
                    break;
            }
        }
    }

    private void AddParams(string text, string section, string detail, int? slot = null)
    {
        foreach (Match match in ParamToken.Matches(text ?? ""))
        {
            if (!ParamUses.TryGetValue(match.Value, out var uses))
                ParamUses[match.Value] = uses = new();

            var use = new MaterialShaderUse(section, detail, slot);
            if (!uses.Contains(use))
                uses.Add(use);
        }
    }

    private void AddSlot(int slot, string section, string detail)
    {
        if (!SlotUses.TryGetValue(slot, out var uses))
            SlotUses[slot] = uses = new();

        var use = new MaterialShaderUse(section, detail);
        if (detail != "" && !uses.Contains(use))
            uses.Add(use);
    }
    #endregion
}
