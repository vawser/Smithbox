using SoulsFormats;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The kinds of material the File List can filter by. A material can be several kinds at once.
/// </summary>
[Flags]
public enum MaterialKind
{
    None = 0,
    AlbedoOnly = 1 << 0,
    Metallic = 1 << 1,
    Emissive = 1 << 2,
    Scrolling = 1 << 3,
    Transparent = 1 << 4,
    Cutout = 1 << 5,
    Layered = 1 << 6,
    Overlay = 1 << 7,
    Unlit = 1 << 8,
    FurHair = 1 << 9,
    Water = 1 << 10,
    Snow = 1 << 11,
}

/// <summary>
/// Works out what kinds a material is, from the same field everywhere: the material's name and its shader's name
/// (both carry the FromSoftware naming tokens, M[AMSN_V][Mb2]_Emissive, P[ARSN]_e, C[A]_Glow), its texture types,
/// and its params (g_BlendMode, scroll params). One rule set for every game: a rule only fires where its field exists.
/// </summary>
public static class MaterialKinds
{
    /// <summary>The kinds in the order the filter lists them</summary>
    public static readonly MaterialKind[] All =
    {
        MaterialKind.AlbedoOnly, MaterialKind.Metallic, MaterialKind.Emissive, MaterialKind.Scrolling,
        MaterialKind.Transparent, MaterialKind.Cutout, MaterialKind.Layered, MaterialKind.Overlay,
        MaterialKind.Unlit, MaterialKind.FurHair, MaterialKind.Water, MaterialKind.Snow,
    };

    private static readonly Regex BracketToken = new(@"\[([^\]]*)\]", RegexOptions.Compiled);
    private static readonly char[] PartSeparators = { '_', '[', ']', ' ', '-', '.' };
    private static readonly Regex LayerBlendToken = new(@"^Mb\d", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    // The older games' suffixes (_2m, _m, _e, _em), only after an underscore: a bare M or C is the name's prefix
    private static readonly Regex LayerSuffix = new(@"_\d?m(?=[_\[\s]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex EdgeSuffix = new(@"_e(?=[_\[\s]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex EmissiveSuffix = new(@"_em(?=[_\[\s]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private sealed class Cached
    {
        public int Revision;
        public object Material;
        public MaterialKind Kinds;
    }

    // Worked out once per edit of a file, as the list filters every row each frame
    private static readonly ConditionalWeakTable<MaterialFileWrapper, Cached> Cache = new();

    /// <summary>
    /// The kinds of a material file, cached until it is next edited
    /// </summary>
    public static MaterialKind Of(MaterialFileWrapper file)
    {
        var material = MaterialOf(file);
        if (material == null)
            return MaterialKind.None;

        if (Cache.TryGetValue(file, out var cached) && cached.Revision == file.Revision && ReferenceEquals(cached.Material, material))
            return cached.Kinds;

        var kinds = Classify(file.Key, material);
        Cache.AddOrUpdate(file, new Cached { Revision = file.Revision, Material = material, Kinds = kinds });
        return kinds;
    }

    private static object MaterialOf(MaterialFileWrapper file) => file switch
    {
        MTDFileWrapper m => m.MTD,
        MATBINFileWrapper m => m.MATBIN,
        _ => null
    };

    /// <summary>
    /// The kinds of an MTD or MATBIN, given its key in the binder. No editor dependencies, so a survey tool can link it.
    /// </summary>
    public static MaterialKind Classify(string key, object material)
    {
        var shortName = Path.GetFileNameWithoutExtension((key ?? "").Split('\\', '/')[^1]);
        var shaderName = MaterialShaderInfo.ShaderNameOf(MaterialQueryMatcher.ShaderOf(material));
        var names = $"{shortName} {shaderName}";

        // [AMSN_V], [Mb2], [Ov_N]: the bracketed tokens
        var tokens = new List<string>();
        foreach (Match match in BracketToken.Matches(names))
            tokens.Add(match.Groups[1].Value);

        // The words between separators: Emissive, Glow, em, Alp, e, NoLight
        var parts = names.Split(PartSeparators, StringSplitOptions.RemoveEmptyEntries);

        var (textureTypes, parameters) = EntriesOf(material);
        var blendMode = BlendModeOf(parameters);

        var kinds = MaterialKind.None;

        // Albedo Only: an [A] (newer games) or [D] (DS1) material, or one whose every texture is an albedo
        var albedoCount = textureTypes.Count(IsAlbedo);
        if (tokens.Any(t => Is(t, "A") || Is(t, "D")) || textureTypes.Count > 0 && albedoCount == textureTypes.Count)
            kinds |= MaterialKind.AlbedoOnly;

        // Metallic: has a metallic map
        if (textureTypes.Any(t => Has(t, "Metallic")))
            kinds |= MaterialKind.Metallic;

        // Emissive: an emissive or glow shader (_em in older games), an emissive map, or emissive params
        if (Has(names, "Emissive") || Has(names, "Glow") || EmissiveSuffix.IsMatch(names) ||
            textureTypes.Any(t => Has(t, "Emissive")) || parameters.Any(p => Has(p.name, "Emissive")))
            kinds |= MaterialKind.Emissive;

        // Scrolling: a scroll shader, or a scroll param that isn't zero
        if (Has(names, "Scroll") || parameters.Any(p => Has(p.name, "Scroll") && IsNonZero(p.value)))
            kinds |= MaterialKind.Scrolling;

        // Transparent: a blended blend mode, or an alpha / translucent shader
        if (blendMode is { } mode && IsBlended(mode) ||
            parts.Any(p => Is(p, "Alp") || Is(p, "Alpha") || Has(p, "Transparen") || Has(p, "Translucen")))
            kinds |= MaterialKind.Transparent;

        // Cutout: the alpha-tested blend mode, or an edge shader (_e, _Edge)
        if (blendMode is MTD.BlendMode.TexEdge or MTD.BlendMode.LSTexEdge || EdgeSuffix.IsMatch(names) || parts.Any(p => Is(p, "Edge")))
            kinds |= MaterialKind.Cutout;

        // Layered: blended layers ([Mb2], [Multiple], [DetailBlend], _2m / _m) or more than one albedo
        if (tokens.Any(t => LayerBlendToken.IsMatch(t) || Has(t, "Multiple") || Has(t, "DetailBlend")) ||
            LayerSuffix.IsMatch(names) || albedoCount > 1)
            kinds |= MaterialKind.Layered;

        // Overlay: an overlay ([Ov_N], [Ov_AN]) or detail texture
        if (tokens.Any(t => t.StartsWith("Ov", StringComparison.OrdinalIgnoreCase)) ||
            textureTypes.Any(t => Has(t, "Detail")))
            kinds |= MaterialKind.Overlay;

        // Unlit
        if (Has(names, "NoLight"))
            kinds |= MaterialKind.Unlit;

        // Fur and hair
        if (parts.Any(p => p.StartsWith("Fur", StringComparison.OrdinalIgnoreCase) || Has(p, "Hair")))
            kinds |= MaterialKind.FurHair;

        // Water: a water shader or blend mode
        if (Has(names, "Water") || Has(names, "Ocean") ||
            blendMode is MTD.BlendMode.Water or MTD.BlendMode.WaterWave or MTD.BlendMode.LSWater)
            kinds |= MaterialKind.Water;

        if (Has(names, "Snow"))
            kinds |= MaterialKind.Snow;

        return kinds;
    }

    private static (List<string> textureTypes, List<(string name, object value)> parameters) EntriesOf(object material)
    {
        var textureTypes = new List<string>();
        var parameters = new List<(string, object)>();

        switch (material)
        {
            case MTD mtd:
                textureTypes.AddRange(mtd.Textures.Select(t => t.Type ?? ""));
                parameters.AddRange(mtd.Params.Select(p => (p.Name ?? "", p.Value)));
                break;

            case MATBIN matbin:
                textureTypes.AddRange(matbin.Samplers.Select(s => s.Type ?? ""));
                parameters.AddRange(matbin.Params.Select(p => (p.Name ?? "", p.Value)));
                break;
        }

        return (textureTypes, parameters);
    }

    private static MTD.BlendMode? BlendModeOf(List<(string name, object value)> parameters)
    {
        foreach (var (name, value) in parameters)
        {
            if (name.EndsWith("g_BlendMode", StringComparison.Ordinal) && value is int mode)
                return (MTD.BlendMode)mode;
        }

        return null;
    }

    private static bool IsBlended(MTD.BlendMode mode) => mode is not
        (MTD.BlendMode.Normal or MTD.BlendMode.TexEdge or MTD.BlendMode.LSNormal or MTD.BlendMode.LSTexEdge);

    private static bool IsAlbedo(string textureType) => Has(textureType, "Albedo") || Has(textureType, "Diffuse");

    private static bool IsNonZero(object value) => value switch
    {
        null => false,
        bool b => b,
        int i => i != 0,
        float f => f != 0,
        int[] ints => ints.Any(i => i != 0),
        float[] floats => floats.Any(f => f != 0),
        _ => true
    };

    private static bool Is(string text, string word) => string.Equals(text, word, StringComparison.OrdinalIgnoreCase);

    private static bool Has(string text, string word) => text.Contains(word, StringComparison.OrdinalIgnoreCase);
}
