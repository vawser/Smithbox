using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Properties panel section of a region that map patterns attach small bases to (by data: some
/// LotResultSmallBaseAndSpot row's attach point has the region's entity ID): the bases and variations placed here,
/// how many patterns pick each, and Preview, which shows the base in place (<see cref="NrSmallBasePlacement"/>) with
/// the Session Preview at that variation. Analysis doc M4.
/// </summary>
public static class PropInfo_Region_SmallBaseAttach
{
    public static void Display(MapEditorView view, Entity firstEnt)
    {
        if (firstEnt is not MsbEntity { Type: MsbEntityType.Region } || firstEnt.Container is not MapContainer map)
            return;

        var entityId = MsbEntityIds.GetUInt(firstEnt.WrappedObject);

        if (entityId == 0)
            return;

        var sites = NrPatternSites.AtRegion(view.Project, entityId);

        if (sites.Count == 0)
            return;

        var groups = sites
            .GroupBy(s => (s.BaseMap, s.Variation))
            .OrderByDescending(g => g.Select(s => s.PatternId).Distinct().Count())
            .ToList();

        if (!MapPropertySections.Header(MapPropertySections.SmallBasesKey, LOC.Get("MAP_SmallBase_Header"),
                LOC.Get("MAP_SmallBase_Header_TT"), groups.Count))
            return;

        var placed = view.SmallBasePlacement.Placements;

        for (var i = 0; i < groups.Count; i++)
        {
            var (baseMap, variation) = groups[i].Key;
            var first = groups[i].First();
            var patterns = groups[i].Select(s => s.PatternId).Distinct().Count();
            var name = string.IsNullOrWhiteSpace(first.BaseName) ? "" : $" {first.BaseName}";

            ImGui.TextUnformatted(LOC.Get("MAP_SmallBase_Entry", baseMap, name, variation, patterns));

            ImGui.SameLine();

            var isHere = placed.TryGetValue(baseMap, out var placement) &&
                placement.AttachMapID == map.Name && placement.RegionName == firstEnt.Name;

            if (isHere)
            {
                if (ImGui.SmallButton($"{LOC.Get("MAP_SmallBase_Reset")}##smallBaseReset{i}"))
                    view.SmallBasePlacement.Reset(baseMap);
                GUI.Tooltip(LOC.Get("MAP_SmallBase_Reset_TT"));
            }
            else
            {
                if (ImGui.SmallButton($"{LOC.Get("MAP_SmallBase_Preview")}##smallBasePreview{i}"))
                {
                    EditorCommandQueue.AddCommand($"map/placebase/{baseMap}/{map.Name}/{firstEnt.Name}");
                    EditorCommandQueue.AddCommand($"map/sessionpreview/{baseMap}/{variation}");
                }
                GUI.Tooltip(LOC.Get("MAP_SmallBase_Preview_TT", variation));
            }
        }

        ImGui.Spacing();
    }
}
