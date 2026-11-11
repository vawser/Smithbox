using StudioCore.Application;
using System;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Where each game keeps its map textures, for the Texture Transfer tool (Claude/Documentation/Map Texture Transfer.md).
/// DS1/DS1R/BB/DS3/SDT: four binders per area, map/mAA/mAA_0000..0003.tpfbhd, shared by every map of the area.
/// DS2/DS2S: one binder per map, model/map/tAA_BB_CC_DD.tpfbhd. The lightmap binders (gi_env) and DS2's _low twins
/// (BND4 files under a .tpfbhd name) are not map texture binders here. DES and the asset games aren't supported.
/// </summary>
public static class MapTextureTransferProfile
{
    private static readonly Regex AreaBinder = new(@"^/map/m\d\d/(m\d\d)_\d{4}\.tpfbhd$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex MapBinder = new(@"^/model/map/t(\d\d_\d\d_\d\d_\d\d)\.tpfbhd$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex PathArea = new(@"[\\/]map[\\/](m\d\d(?:_\d\d_\d\d_\d\d)?)[\\/]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static bool IsSupported(ProjectType type) => type is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB
        or ProjectType.DS3 or ProjectType.SDT or ProjectType.DS2 or ProjectType.DS2S;

    public static bool IsPerMap(ProjectType type) => type is ProjectType.DS2 or ProjectType.DS2S;

    /// <summary>The key binders are shared under: the area (m30) or, in DS2, the map (m10_02_00_00)</summary>
    public static string KeyOfMap(ProjectType type, string mapId)
    {
        if (string.IsNullOrEmpty(mapId) || mapId.Length < 3)
            return null;

        return IsPerMap(type) ? mapId.ToLowerInvariant() : mapId[..3].ToLowerInvariant();
    }

    /// <summary>The key of a map texture binder, or null if the path isn't one</summary>
    public static string KeyOfBinder(ProjectType type, string path)
    {
        path = Normalize(path);

        if (IsPerMap(type))
        {
            var m = MapBinder.Match(path);
            return m.Success ? $"m{m.Groups[1].Value}".ToLowerInvariant() : null;
        }

        var a = AreaBinder.Match(path);
        return a.Success ? a.Groups[1].Value.ToLowerInvariant() : null;
    }

    /// <summary>
    /// The key a texture path names: ...\map\m30\tex\... is m30 (DS2: ...\map\m10_02_00_00\tex\... is that map).
    /// The surveyed games hold a texture in the area its path names in all but a handful of cases.
    /// </summary>
    public static string KeyOfTexturePath(ProjectType type, string texturePath)
    {
        var m = PathArea.Match(texturePath ?? "");
        if (!m.Success)
            return null;

        var id = m.Groups[1].Value.ToLowerInvariant();

        if (IsPerMap(type))
            return id.Length == 12 ? id : null;

        return id[..3];
    }

    /// <summary>A texture or file name without folders and extensions, lower case: ...\tex\m30_brick_a.tif is m30_brick_a</summary>
    public static string Stem(string name)
    {
        var file = (name ?? "").Replace('\\', '/');
        var slash = file.LastIndexOf('/');
        if (slash >= 0)
            file = file[(slash + 1)..];

        var dot = file.IndexOf('.');
        if (dot >= 0)
            file = file[..dot];

        return file.ToLowerInvariant();
    }

    /// <summary>The low detail twin the area binders keep beside a texture (DS3/BB/SDT): name_l</summary>
    public static string LowDetailStem(string stem) => stem + "_l";

    public static string Normalize(string path)
    {
        var p = (path ?? "").Replace('\\', '/');
        return p.StartsWith('/') ? p : "/" + p;
    }

    public static bool SamePath(string a, string b) => string.Equals(Normalize(a), Normalize(b), StringComparison.OrdinalIgnoreCase);
}
