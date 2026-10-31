#nullable enable
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace StudioCore.Renderer;

/// <summary>
/// How a material moves its vertices. Filled in from the per-game shader table.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MaterialDeformer>))]
public enum MaterialDeformer
{
    None,

    /// <summary>Plays a baked vertex animation texture, one column per vertex and one row per frame</summary>
    Vat,

    /// <summary>Offsets vertices along a direction by a smoothed triangle wave</summary>
    Sway
}

/// <summary>
/// Where the blend weight of a layered material comes from.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<MaterialBlendWeight>))]
public enum MaterialBlendWeight
{
    /// <summary>The layer weights are not known for this shader</summary>
    Unknown,

    /// <summary>The first component of the TEXCOORD1 vertex attribute</summary>
    TexCoord1X
}

/// <summary>
/// The per-game table of what is known about each shader. The numbered parameters of a shader
/// (such as snp_0_float_0) only have a meaning within that shader, so they are looked up here.
/// </summary>
public class MaterialShaderTable
{
    public Dictionary<string, MaterialShaderEntry> Shaders { get; set; } = new();
}

public class MaterialShaderEntry
{
    public MaterialDeformer Deformer { get; set; }

    public MaterialBlendWeight BlendWeight { get; set; }

    /// <summary>Role (e.g. Amplitude) to parameter name, without the shader prefix</summary>
    public Dictionary<string, string> Params { get; set; } = new();

    /// <summary>Role (e.g. VatPosition) to the number in the sampler's Texture2D_N name</summary>
    public Dictionary<string, int> Textures { get; set; } = new();
}

public class MaterialSampler
{
    /// <summary>The full sampler name, which is what FLVER materials refer to it by</summary>
    public string Type { get; set; } = "";

    /// <summary>What the texture is for, e.g. AlbedoMap, NormalMap or BlendEdge</summary>
    public string Role { get; set; } = "";

    /// <summary>The N of Texture2D_N, or -1 if the name doesn't have one</summary>
    public int Slot { get; set; } = -1;

    /// <summary>The number that ends the name, or -1 if it doesn't have one</summary>
    public int Suffix { get; set; } = -1;

    public string Path { get; set; } = "";

    public float[] Scale { get; set; } = [];
}

/// <summary>
/// The textures of one blended layer. Either side can be missing.
/// </summary>
public class MaterialBlendLayer
{
    public string? Albedo { get; set; }
    public string? Normal { get; set; }
}

/// <summary>
/// One resolved material: everything the renderer needs from an MTD or MATBIN, and from the shader table.
/// Built once for each material, so that nothing else needs to know how those files are laid out.
/// </summary>
public class MaterialDefinition
{
    private const string AlbedoRole = "AlbedoMap";
    private const string NormalRole = "NormalMap";

    private static readonly Regex ShaderToken = new(@"\[([^\]]+)\]", RegexOptions.Compiled);
    private static readonly Regex BlendToken = new(@"^Mb([2-9])", RegexOptions.Compiled);
    private static readonly Regex SlotName = new(@"Texture2D_(\d+)_(.*)$", RegexOptions.Compiled);
    private static readonly Regex SuffixName = new(@"^(.*?)_(\d+)$", RegexOptions.Compiled);

    public string Name { get; set; } = "";

    /// <summary>MTD or MATBIN</summary>
    public string Source { get; set; } = "";

    public string ShaderName { get; set; } = "";

    /// <summary>The bracketed building blocks of the shader name, e.g. AMSN_V, Mb2 and Ov_N</summary>
    public List<string> ShaderTokens { get; set; } = new();

    /// <summary>Whether the shader table has an entry for this shader</summary>
    public bool IsKnownShader { get; set; }

    public int BlendMode { get; set; }
    public int AlphaRef { get; set; }
    public bool CullNone { get; set; }
    public bool DisableDepthWrite { get; set; }
    public float[] DiffuseColor { get; set; } = [1, 1, 1];

    /// <summary>All of the material's values, keyed by name without the shader prefix</summary>
    public Dictionary<string, float[]> Params { get; set; } = new();

    public List<MaterialSampler> Samplers { get; set; } = new();

    /// <summary>The number of layers the shader blends, or 1 if it doesn't</summary>
    public int BlendLayerCount { get; set; } = 1;

    public MaterialBlendWeight BlendWeight { get; set; }

    public List<MaterialBlendLayer> BlendLayers { get; set; } = new();

    public MaterialDeformer Deformer { get; set; }

    /// <summary>Role to value, for the roles of this shader's table entry that the material has a value for</summary>
    public Dictionary<string, float[]> DeformerParams { get; set; } = new();

    /// <summary>Role to texture path, for the roles of this shader's table entry that the material has a texture for</summary>
    public Dictionary<string, string> DeformerTextures { get; set; } = new();

    public static MaterialDefinition FromMatbin(string name, MATBIN matbin, MaterialShaderTable? table)
    {
        MaterialDefinition def = new() { Name = name, Source = "MATBIN" };

        foreach (MATBIN.Param param in matbin.Params)
        {
            def.Params[ShortParamName(param.Name)] = ToFloats(param.Value);
        }

        foreach (MATBIN.Sampler sampler in matbin.Samplers)
        {
            def.Samplers.Add(ParseSampler(sampler.Type, sampler.Path, [sampler.Unk14.X, sampler.Unk14.Y]));
        }

        def.Finish(matbin.ShaderPath, table);
        return def;
    }

    public static MaterialDefinition FromMtd(string name, MTD mtd, MaterialShaderTable? table)
    {
        MaterialDefinition def = new() { Name = name, Source = "MTD" };

        foreach (MTD.Param param in mtd.Params)
        {
            def.Params[ShortParamName(param.Name)] = ToFloats(param.Value);
        }

        foreach (MTD.Texture texture in mtd.Textures)
        {
            def.Samplers.Add(ParseSampler(texture.Type, texture.Path ?? "", []));
        }

        def.Finish(mtd.ShaderPath, table);
        return def;
    }

    private void Finish(string shaderPath, MaterialShaderTable? table)
    {
        // Shader paths are Windows network paths, so Path can't be relied on to split them on every platform
        ShaderName = Path.GetFileNameWithoutExtension(shaderPath.Split('\\', '/')[^1]);
        ShaderTokens = ShaderToken.Matches(ShaderName).Select(m => m.Groups[1].Value).ToList();

        BlendMode = (int)GetFirst("g_BlendMode", 0);
        AlphaRef = (int)GetFirst("g_AlphaRef", 0);
        CullNone = GetFirst("g_GX_PrimCullNone", 0) != 0;
        DisableDepthWrite = GetFirst("g_GX_bDisableDepthWrite", 0) != 0;

        if (Params.TryGetValue("g_DiffuseMapColor", out var color) && color.Length >= 3)
        {
            DiffuseColor = color[..3];
        }

        ResolveBlendLayers();

        if (table != null && table.Shaders.TryGetValue(ShaderName, out MaterialShaderEntry? entry))
        {
            ApplyShaderEntry(entry);
        }
    }

    private void ApplyShaderEntry(MaterialShaderEntry entry)
    {
        IsKnownShader = true;
        Deformer = entry.Deformer;
        BlendWeight = entry.BlendWeight;

        foreach ((var role, var param) in entry.Params)
        {
            if (Params.TryGetValue(param, out var value))
            {
                DeformerParams[role] = value;
            }
        }

        foreach ((var role, var slot) in entry.Textures)
        {
            MaterialSampler? sampler = Samplers.Find(x => x.Slot == slot);
            if (sampler != null)
            {
                DeformerTextures[role] = sampler.Path;
            }
        }
    }

    /// <summary>
    /// Works out which albedo and normal belong to each layer of a [MbN] shader.
    /// The numbers in the names can't be used as layer indices directly: depending on the shader they
    /// start at 0 or 1, and can even repeat (M[Flag][Mb2] has AlbedoMap_0 twice). A shader can also have
    /// normals that aren't layers, such as the overlay normal of [Ov_N], so the number of layers
    /// comes from the shader name, and normals are only taken if they are numbered.
    /// </summary>
    private void ResolveBlendLayers()
    {
        foreach (var token in ShaderTokens)
        {
            Match match = BlendToken.Match(token);
            if (match.Success)
            {
                BlendLayerCount = match.Groups[1].Value[0] - '0';
                break;
            }
        }

        if (BlendLayerCount < 2)
        {
            return;
        }

        List<MaterialSampler> albedos = OrderedLayerSamplers(AlbedoRole).Take(BlendLayerCount).ToList();
        List<MaterialSampler> normals = OrderedLayerSamplers(NormalRole).ToList();

        // Prefer the normal with the same number as its albedo. If none line up, go by position
        var matchBySuffix = albedos.Count > 0 && albedos.All(a => normals.Exists(n => n.Suffix == a.Suffix))
            && albedos.Select(a => a.Suffix).Distinct().Count() == albedos.Count;

        for (var i = 0; i < albedos.Count; i++)
        {
            MaterialSampler? normal = matchBySuffix
                ? normals.Find(n => n.Suffix == albedos[i].Suffix)
                : i < normals.Count ? normals[i] : null;

            BlendLayers.Add(new MaterialBlendLayer { Albedo = albedos[i].Type, Normal = normal?.Type });
        }
    }

    private IEnumerable<MaterialSampler> OrderedLayerSamplers(string role)
    {
        return Samplers.Where(x => x.Role == role && x.Suffix >= 0).OrderBy(x => x.Suffix).ThenBy(x => x.Slot);
    }

    private float GetFirst(string name, float fallback)
    {
        return Params.TryGetValue(name, out var value) && value.Length > 0 ? value[0] : fallback;
    }

    /// <summary>
    /// Strips the shader prefix: M_Flag__Mb2__snp_0_float_20 becomes snp_0_float_20.
    /// </summary>
    private static string ShortParamName(string name)
    {
        var index = name.IndexOf("snp_", StringComparison.Ordinal);
        return index > 0 ? name[index..] : name;
    }

    private static MaterialSampler ParseSampler(string type, string path, float[] scale)
    {
        MaterialSampler sampler = new() { Type = type, Path = path, Scale = scale };

        var role = type;

        Match slot = SlotName.Match(type);
        if (slot.Success)
        {
            sampler.Slot = int.Parse(slot.Groups[1].Value);
            role = slot.Groups[2].Value;
        }

        if (role.StartsWith("GSBlendMap", StringComparison.Ordinal))
        {
            role = role["GSBlendMap".Length..];
        }

        role = role.TrimStart('_');

        Match suffix = SuffixName.Match(role);
        if (suffix.Success)
        {
            role = suffix.Groups[1].Value;
            sampler.Suffix = int.Parse(suffix.Groups[2].Value);
        }

        sampler.Role = role;
        return sampler;
    }

    private static float[] ToFloats(object? value)
    {
        return value switch
        {
            bool b => [b ? 1 : 0],
            int i => [i],
            float f => [f],
            int[] ints => ints.Select(x => (float)x).ToArray(),
            float[] floats => floats,
            _ => []
        };
    }
}
