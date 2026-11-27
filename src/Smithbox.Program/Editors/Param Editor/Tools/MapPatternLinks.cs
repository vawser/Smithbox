using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Lookups shared by the Param Editor's map pattern tools (Map Placement panel, Map Pattern tool): how a lot result's
/// attach point and small base resolve to maps, and the commands that show them in the Map Editor.
/// Claude/Documentation/Map Variations (NR) Analysis.md P2/P3; data rules from Param Fields (NR).md §3X–3Y.
/// - attachId → SmallBaseAndSpotAttachPoint row: areaNo/gridXNo/gridZNo/mapVariantNo give the m60 tile
///   (mAA_XX_ZZ_V0, V the Shifting Earth variant), attachPointEntityId the tile's SmallBaseAttach region.
/// - smallBaseMapId AABB → the MSB mAA_BB_00_00, and the SmallBaseMapVariationParam row of that ID.
/// </summary>
public static class MapPatternLinks
{
    public const string AttachPointParam = "SmallBaseAndSpotAttachPoint";
    public const string SmallBaseParam = "SmallBaseMapVariationParam";
    public const string LotSiteParam = "LotResultSmallBaseAndSpot";
    public const string LotPlayAreaParam = "LotResultPlayAreaParam";
    public const string LotFlagParam = "LotResultMapPatternFlag";
    public const string ManualPatternParam = "ManualMapPattern";
    public const string ScenarioPlacementParam = "ScenarioPlacementParam";

    public static bool HasFields(Param.Row row, params string[] fields) =>
        row != null && fields.All(f => CharacterComposition.HasField(row, f));

    public static int GetInt(Param.Row row, string field) => CharacterComposition.GetValue(row, field);

    public static Param GetParam(ProjectEntry project, string name) =>
        project.Handler.ParamData?.PrimaryBank?.Params?.GetValueOrDefault(name);

    /// <summary>The first row of a param with an ID, or null</summary>
    public static Param.Row FindRow(ProjectEntry project, string param, int id) =>
        GetParam(project, param)?.Rows.FirstOrDefault(r => r.ID == id);

    public static string RowLabel(Param.Row row) =>
        row == null ? "" : string.IsNullOrWhiteSpace(row.Name) ? $"{row.ID}" : $"{row.ID} {row.Name}";

    /// <summary>The MSB of a small-base ID (AABB → mAA_BB_00_00)</summary>
    public static string SmallBaseMap(int id) => id <= 0 ? null : $"m{id / 100:00}_{id % 100:00}_00_00";

    /// <summary>
    /// The small m60 tile an attach point row names: mAA_XX_ZZ_V0, V its Shifting Earth variant. The region itself
    /// is usually in the large tile covering it (<see cref="NrAttachPointLocator"/>).
    /// </summary>
    public static string AttachTile(ProjectEntry project, Param.Row attachRow)
    {
        if (!TryGetAttachGrid(attachRow, out var area, out var x, out var z, out var variant))
            return null;

        return NrAttachPointLocator.Candidates(area, x, z, variant)[0];
    }

    public static bool TryGetAttachGrid(Param.Row attachRow, out int area, out int x, out int z, out int variant)
    {
        area = x = z = variant = 0;

        if (!HasFields(attachRow, "areaNo", "gridXNo", "gridZNo"))
            return false;

        area = GetInt(attachRow, "areaNo");
        x = GetInt(attachRow, "gridXNo");
        z = GetInt(attachRow, "gridZNo");
        variant = HasFields(attachRow, "mapVariantNo") ? GetInt(attachRow, "mapVariantNo") : 0;
        return true;
    }

    /// <summary>The candidate maps of an attach point row that the project has, in search order</summary>
    public static List<string> AttachCandidates(ProjectEntry project, Param.Row attachRow)
    {
        if (!TryGetAttachGrid(attachRow, out var area, out var x, out var z, out var variant))
            return new List<string>();

        return NrAttachPointLocator.Candidates(area, x, z, variant).Where(m => MapExists(project, m)).ToList();
    }

    /// <summary>Searches the candidate maps for the attach point's region (reads MSBs: on a click only)</summary>
    public static NrAttachPointLocator.Location LocateAttach(ProjectEntry project, Param.Row attachRow)
    {
        if (!TryGetAttachGrid(attachRow, out var area, out var x, out var z, out var variant))
            return null;

        return NrAttachPointLocator.Locate(project, area, x, z, variant, AttachEntityId(attachRow));
    }

    public static bool MapExists(ProjectEntry project, string mapId) =>
        mapId != null && project.Locator?.MapFiles?.Entries?.Any(e => string.Equals(e.Filename, mapId, StringComparison.OrdinalIgnoreCase)) == true;

    /// <summary>The attach point's region entity ID, or 0</summary>
    public static uint AttachEntityId(Param.Row attachRow) =>
        HasFields(attachRow, "attachPointEntityId") ? unchecked((uint)GetInt(attachRow, "attachPointEntityId")) : 0;

    /// <summary>
    /// Finds the attach point's SmallBaseAttach region, then loads its map in the Map Editor and selects it. With no
    /// region in any candidate map, loads the small tile the row names (where the row's position is measured) and
    /// says so in the log.
    /// </summary>
    public static void ViewAttachPoint(ProjectEntry project, Param.Row attachRow)
    {
        var location = LocateAttach(project, attachRow);

        if (location != null)
        {
            EditorCommandQueue.AddCommand($"map/load/{location.MapID}");
            EditorCommandQueue.AddCommand($"map/select/{location.MapID}/{location.RegionName}/{nameof(MsbEntityType.Region)}");
            return;
        }

        var tile = AttachTile(project, attachRow);
        Smithbox.Log(typeof(MapPatternLinks), LOC.Get("PARAM_MapPlacement_No_Region_Log", AttachEntityId(attachRow), tile));

        if (MapExists(project, tile))
            EditorCommandQueue.AddCommand($"map/load/{tile}");
    }

    /// <summary>
    /// Shows a small base at its attach point in the Map Editor: finds the attach region, then loads the region's map
    /// and the base, moves the base there and sets the Session Preview to the variation. With no region, logs why.
    /// </summary>
    public static void PreviewInPlace(ProjectEntry project, Param.Row attachRow, string baseMap, int variation)
    {
        var location = LocateAttach(project, attachRow);

        if (location == null || baseMap == null)
        {
            Smithbox.LogError(typeof(MapPatternLinks), LOC.Get("PARAM_MapPlacement_No_Region_Log", AttachEntityId(attachRow), AttachTile(project, attachRow)));
            return;
        }

        EditorCommandQueue.AddCommand($"map/placebase/{baseMap}/{location.MapID}/{location.RegionName}");
        EditorCommandQueue.AddCommand($"map/sessionpreview/{baseMap}/{variation}");
    }

    /// <summary>Loads a small base in the Map Editor with the Session Preview set to a variation</summary>
    public static void OpenBase(string mapId, int variation)
    {
        if (mapId == null)
            return;

        EditorCommandQueue.AddCommand($"map/load/{mapId}");
        EditorCommandQueue.AddCommand($"map/sessionpreview/{mapId}/{variation}");
    }

    /// <summary>The name of a project enum option (Param Enums), or null</summary>
    public static string EnumName(ProjectEntry project, string enumKey, int value)
    {
        var entry = project.Handler.ParamData?.Enums?.List.FirstOrDefault(e => e.Key == enumKey);
        var name = entry?.Options.FirstOrDefault(o => o.Key == value.ToString())?.GetName();

        return string.IsNullOrEmpty(name) ? null : name;
    }

    /// <summary>"value (name)" when the enum names the value, else the value</summary>
    public static string EnumLabel(ProjectEntry project, string enumKey, int value)
    {
        var name = EnumName(project, enumKey, value);
        return name == null ? $"{value}" : $"{value} ({name})";
    }

    /// <summary>
    /// The pattern IDs used by the lot result params, with a name from the first row that has one
    /// </summary>
    public static SortedDictionary<int, string> GetPatterns(ProjectEntry project)
    {
        var patterns = new SortedDictionary<int, string>();

        foreach (var paramName in new[] { LotPlayAreaParam, LotFlagParam, LotSiteParam })
        {
            var param = GetParam(project, paramName);

            if (param == null || param.Rows.Count == 0 || !HasFields(param.Rows.First(), "patternId"))
                continue;

            foreach (var row in param.Rows)
            {
                var id = GetInt(row, "patternId");

                if (!patterns.TryGetValue(id, out var name) || string.IsNullOrWhiteSpace(name))
                    patterns[id] = row.Name ?? "";
            }
        }

        return patterns;
    }

    /// <summary>The rows of a param whose patternId is this one, or an empty list</summary>
    public static List<Param.Row> PatternRows(ProjectEntry project, string paramName, int patternId)
    {
        var param = GetParam(project, paramName);

        if (param == null || param.Rows.Count == 0 || !HasFields(param.Rows.First(), "patternId"))
            return new List<Param.Row>();

        return param.Rows.Where(r => GetInt(r, "patternId") == patternId).ToList();
    }
}
