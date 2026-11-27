using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The map-pattern sites seen from the Map Editor: which LotResultSmallBaseAndSpot rows put a small base at a
/// SmallBaseAttach region (by its entity ID, through SmallBaseAndSpotAttachPoint), and which regions a pattern uses.
/// Built from the param bank on first use and again when the params' row counts or objects change.
/// </summary>
public static class NrPatternSites
{
    public sealed record Site(int PatternId, int SiteRowId, int AttachRowId, uint AttachEntityId, string BaseMap,
        string BaseName, int Variation);

    private sealed class Index
    {
        public object SiteParam;
        public object AttachParam;
        public int SiteRows;
        public int AttachRows;

        public readonly Dictionary<uint, List<Site>> ByEntity = new();
        public readonly Dictionary<int, List<Site>> ByPattern = new();
        public readonly Dictionary<string, List<Site>> ByBase = new(StringComparer.OrdinalIgnoreCase);
    }

    private static readonly ConditionalWeakTable<ProjectEntry, Index> Indices = new();

    /// <summary>Whether the project has the params (by their fields)</summary>
    public static bool IsAvailable(ProjectEntry project) => Get(project) != null;

    /// <summary>The sites placed at a region, by its entity ID</summary>
    public static IReadOnlyList<Site> AtRegion(ProjectEntry project, uint entityId) =>
        Get(project)?.ByEntity.GetValueOrDefault(entityId) ?? (IReadOnlyList<Site>)Array.Empty<Site>();

    /// <summary>The sites that place a small-base map</summary>
    public static IReadOnlyList<Site> OfBase(ProjectEntry project, string baseMap) =>
        Get(project)?.ByBase.GetValueOrDefault(baseMap) ?? (IReadOnlyList<Site>)Array.Empty<Site>();

    /// <summary>The sites of a pattern</summary>
    public static IReadOnlyList<Site> InPattern(ProjectEntry project, int patternId) =>
        Get(project)?.ByPattern.GetValueOrDefault(patternId) ?? (IReadOnlyList<Site>)Array.Empty<Site>();

    private static Index Get(ProjectEntry project)
    {
        var sites = MapPatternLinks.GetParam(project, MapPatternLinks.LotSiteParam);
        var attach = MapPatternLinks.GetParam(project, MapPatternLinks.AttachPointParam);

        if (sites == null || attach == null || sites.Rows.Count == 0 || attach.Rows.Count == 0 ||
            !MapPatternLinks.HasFields(sites.Rows.First(), "patternId", "attachId", "smallBaseMapId", "variationId") ||
            !MapPatternLinks.HasFields(attach.Rows.First(), "attachPointEntityId"))
            return null;

        if (Indices.TryGetValue(project, out var cached) && cached.SiteParam == sites && cached.AttachParam == attach &&
            cached.SiteRows == sites.Rows.Count && cached.AttachRows == attach.Rows.Count)
            return cached;

        var index = new Index { SiteParam = sites, AttachParam = attach, SiteRows = sites.Rows.Count, AttachRows = attach.Rows.Count };

        // Attach row ID → region entity ID (the first row of an ID)
        var attachEntity = new Dictionary<int, uint>();

        foreach (var row in attach.Rows)
            attachEntity.TryAdd(row.ID, MapPatternLinks.AttachEntityId(row));

        var baseNames = new Dictionary<int, string>();

        foreach (var row in sites.Rows)
        {
            var attachId = MapPatternLinks.GetInt(row, "attachId");
            var baseId = MapPatternLinks.GetInt(row, "smallBaseMapId");

            if (!baseNames.TryGetValue(baseId, out var baseName))
            {
                baseName = MapPatternLinks.FindRow(project, MapPatternLinks.SmallBaseParam, baseId)?.Name ?? "";
                baseNames[baseId] = baseName;
            }

            var site = new Site(MapPatternLinks.GetInt(row, "patternId"), row.ID, attachId,
                attachEntity.GetValueOrDefault(attachId), MapPatternLinks.SmallBaseMap(baseId), baseName,
                MapPatternLinks.GetInt(row, "variationId"));

            if (site.AttachEntityId != 0)
            {
                if (!index.ByEntity.TryGetValue(site.AttachEntityId, out var list))
                    index.ByEntity[site.AttachEntityId] = list = new();

                list.Add(site);
            }

            if (!index.ByPattern.TryGetValue(site.PatternId, out var patternList))
                index.ByPattern[site.PatternId] = patternList = new();

            patternList.Add(site);

            if (site.BaseMap != null)
            {
                if (!index.ByBase.TryGetValue(site.BaseMap, out var baseList))
                    index.ByBase[site.BaseMap] = baseList = new();

                baseList.Add(site);
            }
        }

        Indices.AddOrUpdate(project, index);
        return index;
    }
}
