using SoulsFormats;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace StudioCore.Port;

// Assets/Game Port/DS3_ER/Materials.json: what the DS3 -> ER material port reads. Written by the genmaterials mode of
// Claude/Tools/Ds3ErPortSurvey from the ER game files and the rules of Material Mapping.md §3. Param values are kept as
// invariant strings so a param of any MATBIN type round-trips.

public sealed class PortConfig
{
    public string Generated { get; set; }
    public string Notes { get; set; }

    /// <summary>The DS3 MTD params whose values replace the template's (same names in ER).</summary>
    public List<string> CopiedParams { get; set; } = new();

    /// <summary>ER texture suffix per sampler role (A, N, M, B, E, O = overlay normal).</summary>
    public Dictionary<string, string> Suffixes { get; set; } = new();

    /// <summary>A DS3 material is metal when MetallicCoverage of its specular map has linear F0 luma above MetallicThreshold.</summary>
    public float MetallicThreshold { get; set; }
    public float MetallicCoverage { get; set; }

    /// <summary>The metallic map: linear F0 luma at Ramp[0] is 0, at Ramp[1] is 1 (smoothstep).</summary>
    public float[] MetallicRamp { get; set; } = [0.04f, 0.10f];

    /// <summary>Ported textures go in the first free AET group from this one.</summary>
    public int AetGroupFrom { get; set; }

    /// <summary>The shader of a material no rule matches.</summary>
    public string DefaultShader { get; set; }

    /// <summary>Ordered: the first rule that matches a DS3 MTD name gives its ER shader.</summary>
    public List<PortRule> Rules { get; set; } = new();

    /// <summary>FLVER header fields and skeleton set of an ER map piece.</summary>
    public ModelTemplate Model { get; set; }

    public Dictionary<string, ShaderTemplate> Shaders { get; set; } = new();
}

public sealed class PortRule
{
    public string Id { get; set; }
    /// <summary>Regex on the MTD base (the part up to the first ']', e.g. M[ARSN]).</summary>
    public string Base { get; set; }
    /// <summary>The _Nm blend layer count (0 = none); null = any.</summary>
    public int? Blend { get; set; }
    /// <summary>Tokens (lower case, after the base, split on '_') that must all be present / one of / none of.</summary>
    public List<string> All { get; set; }
    public List<string> Any { get; set; }
    public List<string> None { get; set; }
    /// <summary>null = either; true / false = the material is / isn't metal.</summary>
    public bool? Metallic { get; set; }
    public string Shader { get; set; }
    /// <summary>direct, open (an open question in Material Mapping.md) or handcheck.</summary>
    public string Status { get; set; }
    public string Note { get; set; }

    public bool Matches(MtdName name, bool metallic)
    {
        if (Base != null && !Regex.IsMatch(name.Base, Base, RegexOptions.IgnoreCase)) return false;
        if (Blend != null && Blend != name.Blend) return false;
        if (All != null && !All.All(name.Tokens.Contains)) return false;
        if (Any != null && !Any.Any(name.Tokens.Contains)) return false;
        if (None != null && None.Any(name.Tokens.Contains)) return false;
        if (Metallic != null && Metallic != metallic) return false;
        return true;
    }
}

/// <summary>A DS3 MTD name split for the rules: M[ARSN]_2m_l_edg -> base M[ARSN], blend 2, tokens l, edg.</summary>
public sealed record MtdName(string Name, string Base, int Blend, HashSet<string> Tokens)
{
    public static MtdName Parse(string name)
    {
        var end = name.IndexOf(']');
        var head = end < 0 ? name.Split('_')[0] : name[..(end + 1)];
        var tokens = name[head.Length..].Split('_', StringSplitOptions.RemoveEmptyEntries).Select(t => t.ToLowerInvariant()).ToList();
        var blend = 0;
        foreach (var t in tokens.ToList())
        {
            var m = Regex.Match(t, @"^(\d)m$");
            if (m.Success) { blend = int.Parse(m.Groups[1].Value); tokens.Remove(t); }
        }
        return new MtdName(name, head, blend, tokens.ToHashSet());
    }
}

public sealed class ShaderTemplate
{
    public string ShaderPath { get; set; }
    public int Matbins { get; set; }
    public string Representative { get; set; }
    /// <summary>The representative's name inside allmaterial.matbinbnd.dcx, for the ported MATBIN's binder name.</summary>
    public string BinderName { get; set; }
    public string SourcePath { get; set; }
    public List<ParamTemplate> Params { get; set; } = new();
    public List<SamplerTemplate> Samplers { get; set; } = new();
    public DonorTemplate Donor { get; set; }
}

public sealed class ParamTemplate
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
    public int Distinct { get; set; }
    public int DominantCount { get; set; }

    public MATBIN.Param ToParam()
    {
        var p = new MATBIN.Param { Name = Name };
        p.Type = Enum.Parse<MATBIN.ParamType>(Type);
        p.Value = Parse(p.Type, Value);
        return p;
    }

    public static string Format(object value) => value switch
    {
        null => "",
        bool b => b ? "true" : "false",
        int i => i.ToString(CultureInfo.InvariantCulture),
        float f => f.ToString("R", CultureInfo.InvariantCulture),
        int[] a => string.Join(",", a.Select(x => x.ToString(CultureInfo.InvariantCulture))),
        float[] a => string.Join(",", a.Select(x => x.ToString("R", CultureInfo.InvariantCulture))),
        _ => value.ToString()
    };

    public static object Parse(MATBIN.ParamType type, string text)
    {
        var parts = text.Split(',', StringSplitOptions.RemoveEmptyEntries);
        float F(int i) => float.Parse(parts[i], CultureInfo.InvariantCulture);
        int I(int i) => int.Parse(parts[i], CultureInfo.InvariantCulture);
        return type switch
        {
            MATBIN.ParamType.Bool => text == "true",
            MATBIN.ParamType.Int => I(0),
            MATBIN.ParamType.Int2 => new[] { I(0), I(1) },
            MATBIN.ParamType.Float => F(0),
            MATBIN.ParamType.Float2 => Enumerable.Range(0, 2).Select(F).ToArray(),
            MATBIN.ParamType.Float3 => Enumerable.Range(0, 3).Select(F).ToArray(),
            MATBIN.ParamType.Float4 => Enumerable.Range(0, 4).Select(F).ToArray(),
            MATBIN.ParamType.Float5 => Enumerable.Range(0, 5).Select(F).ToArray(),
            _ => throw new NotSupportedException(type.ToString())
        };
    }
}

public sealed class SamplerTemplate
{
    public string Type { get; set; }
    /// <summary>A albedo, N normal, M metallic, B blend mask, E emissive, O overlay normal, X other.</summary>
    public string Role { get; set; }
    public int Rank { get; set; }
    public float[] Unk14 { get; set; }
    /// <summary>The most common SysTex path, for a sampler no DS3 texture fills ("" if vanilla never has one).</summary>
    public string FallbackPath { get; set; }
    public string Paths { get; set; }
}

public sealed class DonorTemplate
{
    public string Model { get; set; }
    public string Material { get; set; }
    public int MaterialIndex { get; set; }
    public List<TextureSlot> Textures { get; set; } = new();
    public GxTemplate Gx { get; set; }
    public List<MemberTemplate> Layout { get; set; } = new();
    public int NormalW { get; set; }
}

public sealed class TextureSlot
{
    public string ParamName { get; set; }
    public float[] TilingScale { get; set; }
    public byte TilingTypeU { get; set; }
    public byte TilingTypeV { get; set; }
    public float Unk14 { get; set; }
    public float Unk18 { get; set; }
    public float Unk1C { get; set; }

    public FLVER2.Texture ToTexture() => new(ParamName, "", new(TilingScale[0], TilingScale[1]),
        (FLVER2.Texture.TilingType)TilingTypeU, (FLVER2.Texture.TilingType)TilingTypeV, Unk14, Unk18, Unk1C);
}

public sealed class GxTemplate
{
    public int TerminatorID { get; set; }
    public int TerminatorLength { get; set; }
    public List<GxItemTemplate> Items { get; set; } = new();

    public FLVER2.GXList ToList()
    {
        var list = new FLVER2.GXList { TerminatorID = TerminatorID, TerminatorLength = TerminatorLength };
        list.AddRange(Items.Select(i => new FLVER2.GXItem(i.ID, i.Unk04, Convert.FromBase64String(i.Data))));
        return list;
    }
}

public sealed class GxItemTemplate
{
    public string ID { get; set; }
    public int Unk04 { get; set; }
    public string Data { get; set; }
}

public sealed class MemberTemplate
{
    public string Semantic { get; set; }
    public int Index { get; set; }
    public string Type { get; set; }
    public int Stream { get; set; }
    public short SpecialModifier { get; set; }

    public FLVER.LayoutMember ToMember() => new(Enum.Parse<FLVER.LayoutType>(Type), Enum.Parse<FLVER.LayoutSemantic>(Semantic), Index, Stream, SpecialModifier);
}

public sealed class ModelTemplate
{
    public string Donor { get; set; }
    /// <summary>FLVER2.FLVERHeader's scalar properties (not the bounding box), by name.</summary>
    public Dictionary<string, string> Header { get; set; } = new();
    public List<short[]> BaseSkeleton { get; set; } = new();
    public List<short[]> AllSkeletons { get; set; } = new();

    public void Apply(FLVER2 flver)
    {
        foreach (var p in typeof(FLVER2.FLVERHeader).GetProperties().Where(p => p.CanWrite && IsScalar(p.PropertyType)))
            if (Header.TryGetValue(p.Name, out var v))
                p.SetValue(flver.Header, Convert.ChangeType(v, p.PropertyType, CultureInfo.InvariantCulture));

        flver.Skeletons = new FLVER2.SkeletonSet
        {
            BaseSkeleton = BaseSkeleton.Select(ToBone).ToList(),
            AllSkeletons = AllSkeletons.Select(ToBone).ToList()
        };
    }

    private static bool IsScalar(Type t) => t == typeof(int) || t == typeof(bool) || t == typeof(byte) || t == typeof(short);

    private static FLVER2.SkeletonSet.Bone ToBone(short[] b) => new(b[4])
    {
        ParentIndex = b[0], FirstChildIndex = b[1], NextSiblingIndex = b[2], PreviousSiblingIndex = b[3]
    };
}

/// <summary>
/// Assets/Game Port/DS3_ER/HitMaterials.json: the collision surface remap of the analysis §8.8. A collision material ID
/// is surface (id % 100, a HitMtrlParam row) + property (id / 100, kept). Surfaces not listed keep their ID.
/// </summary>
public sealed class HitMaterialConfig
{
    public string Notes { get; set; }
    /// <summary>DS3 surface -> ER surface.</summary>
    public Dictionary<int, int> Remap { get; set; } = new();
    /// <summary>
    /// DS3 surfaces that need a new ER row (no fall damage), with the ER row they are mapped to until such rows are
    /// written; listed in the port report.
    /// </summary>
    public Dictionary<int, int> NeedsNewRow { get; set; } = new();

    public uint Map(uint id, out bool needsRow)
    {
        var surface = (int)(id % 100);
        needsRow = NeedsNewRow.TryGetValue(surface, out var row);
        var target = needsRow ? row : Remap.TryGetValue(surface, out var r) ? r : surface;
        return (uint)(id / 100 * 100 + target);
    }
}

/// <summary>
/// .smithbox/port/DS3_ER.json in the output project: the names a previous run gave, so a re-run reuses them (the same
/// AET group and ids, MATBIN names, model numbers) instead of taking new ones.
/// </summary>
public sealed class PortManifest
{
    public int AetGroup { get; set; } = -1;
    /// <summary>AET container key (DS3 base name, or albedo|specular for a metal material) -> container id.</summary>
    public Dictionary<string, int> AetIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>MATBIN identity -> MATBIN name.</summary>
    public Dictionary<string, string> Matbins { get; set; } = new();
    /// <summary>"{targetMap}|{ds3 model}" -> ER model number (map pieces m, collisions h).</summary>
    public Dictionary<string, string> Models { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Sampler roles and layer ranks, the same for DS3 MTD/FLVER texture names and ER MATBIN sampler types, so a DS3
/// texture finds the ER sampler of the same (role, rank). Layers that hold an albedo are ranked in order (ER blends
/// number them 1..3, DS3 0..2); a normal on a layer without an albedo, or DS3's detail bump, is the overlay normal O;
/// blend masks are B. DS3's specular colour R becomes metallic M (the converted texture).
/// </summary>
public static class PortRoles
{
    /// <summary>
    /// A sampler's role: A albedo, R specular colour, M metallic, S shininess, N normal, D detail normal, L lightmap,
    /// E emissive, B blend mask, H height, X other.
    /// </summary>
    public static string Role(string type)
    {
        var t = type.ToLowerInvariant();
        if (t.Contains("lightmap") || t.Contains("doltexture")) return "L";
        if (t.Contains("detail")) return "D";
        if (t.Contains("diffuse") || t.Contains("albedo")) return "A";
        if (t.Contains("specular") || t.Contains("reflectance")) return "R";
        if (t.Contains("metallic")) return "M";
        if (t.Contains("shininess")) return "S";
        if (t.Contains("bumpmap") || t.Contains("normal")) return "N";
        if (t.Contains("emissive")) return "E";
        if (t.Contains("mask") || t.Contains("blend")) return "B";
        if (t.Contains("height") || t.Contains("displacement") || t.Contains("parallax")) return "H";
        return "X";
    }

    /// <summary>ER "AlbedoMap_1" is layer 1; DS3 "g_DiffuseTexture2" is layer 1, and no number is 0.</summary>
    public static int Layer(string type)
    {
        var m = Regex.Match(type, @"(_?)(\d+)$");
        if (!m.Success) return 0;
        var n = int.Parse(m.Groups[2].Value);
        return m.Groups[1].Value == "_" ? n : Math.Max(0, n - 1);
    }

    public static List<(string Type, string Role, int Rank)> Rank(IEnumerable<string> types)
    {
        var list = types.Select(t => (Type: t, Role: Role(t), Layer: Layer(t))).ToList();
        var albedoLayers = list.Where(x => x.Role == "A").Select(x => x.Layer).Distinct().OrderBy(l => l).ToList();
        var result = new List<(string, string, int)>();
        var masks = 0;
        foreach (var (type, role, layer) in list)
        {
            var r = role == "R" ? "M" : role;
            var rank = albedoLayers.IndexOf(layer);
            if (r == "D" || (r == "N" && rank < 0 && albedoLayers.Count > 0)) { r = "O"; rank = 0; }
            else if (r == "B") rank = masks++;
            else if (rank < 0) rank = albedoLayers.Count == 0 ? layer : 0;
            result.Add((type, r, rank));
        }
        return result;
    }
}

[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(PortConfig))]
[JsonSerializable(typeof(HitMaterialConfig))]
[JsonSerializable(typeof(PortManifest))]
internal partial class PortJsonContext : JsonSerializerContext
{
}
