using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// The Map Placement section of the Fields window, for rows that place something in a map (by field presence):
/// - a site (attachId + smallBaseMapId + variationId: LotResultSmallBaseAndSpot, ManualMapPattern): the attach point
///   and its m60 tile (View in Map), the small base and variation (Open Base, with the Session Preview set to it), and
///   a warning when the scanned base has no map object at that variation;
/// - an attach point (attachPointEntityId + areaNo/gridXNo/gridZNo): its tile and region (View in Map);
/// - a scenario placement (placeScenarioId + conditionScenarioId + conditionScenarioStage): the map parts whose key
///   triple matches it (Used by).
/// Claude/Documentation/Map Variations (NR) Analysis.md P2, P5.
/// </summary>
public class MapPlacementPanel
{
    private readonly ParamEditorView View;

    public MapPlacementPanel(ParamEditorView view)
    {
        View = view;
    }

    private ProjectEntry Project => View.Project;

    public void Display(Param.Row row)
    {
        if (row == null)
            return;

        var isSite = MapPatternLinks.HasFields(row, "attachId", "smallBaseMapId", "variationId");
        var isAttach = MapPatternLinks.HasFields(row, "attachPointEntityId", "areaNo", "gridXNo", "gridZNo");
        var isScenario = MapPatternLinks.HasFields(row, "placeScenarioId", "conditionScenarioId", "conditionScenarioStage");

        if (!isSite && !isAttach && !isScenario)
            return;

        if (!ParamFieldSections.Header(ParamFieldSections.MapPlacementKey, LOC.Get("PARAM_MapPlacement_Header"),
                LOC.Get("PARAM_MapPlacement_Header_TT")))
            return;

        if (isSite)
            DisplaySite(row);

        if (isAttach)
            DisplayAttachPoint(row, row.ID);

        if (isScenario)
            DisplayScenario(row);

        ImGui.Spacing();
    }

    private void DisplaySite(Param.Row row)
    {
        var attachId = MapPatternLinks.GetInt(row, "attachId");
        var attachRow = MapPatternLinks.FindRow(Project, MapPatternLinks.AttachPointParam, attachId);

        // Attach point
        if (attachRow == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PARAM_MapPlacement_No_Attach_Row", attachId));
        }
        else
        {
            ImGui.TextUnformatted(LOC.Get("PARAM_MapPlacement_Attach", MapPatternLinks.RowLabel(attachRow)));
            DisplayAttachPoint(attachRow, row.ID);
        }

        // Small base
        var baseId = MapPatternLinks.GetInt(row, "smallBaseMapId");
        var variation = MapPatternLinks.GetInt(row, "variationId");
        var baseMap = MapPatternLinks.SmallBaseMap(baseId);
        var baseRow = MapPatternLinks.FindRow(Project, MapPatternLinks.SmallBaseParam, baseId);

        if (baseMap == null)
            return;

        var baseName = baseRow != null && !string.IsNullOrWhiteSpace(baseRow.Name) ? $" ({baseRow.Name})" : "";
        ImGui.TextUnformatted(LOC.Get("PARAM_MapPlacement_Base", baseMap, baseName, variation));

        if (!MapPatternLinks.MapExists(Project, baseMap))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PARAM_MapPlacement_No_Base_Map", baseMap));
            return;
        }

        if (ImGui.SmallButton($"{LOC.Get("PARAM_MapPlacement_Open_Base")}##mapPlacementOpenBase"))
            MapPatternLinks.OpenBase(baseMap, variation);
        GUI.Tooltip(LOC.Get("PARAM_MapPlacement_Open_Base_TT", variation));

        // The scanned base's map objects at this variation, when the maps have been scanned
        var index = NrPlacementIndex.For(Project);

        if (index.IsScanned && index.HasMap(baseMap))
        {
            var count = index.CountAtVariation(baseMap, variation);

            ImGui.SameLine();

            if (count == 0)
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PARAM_MapPlacement_Variation_Empty", variation));
            else
                ImGui.TextDisabled(LOC.Get("PARAM_MapPlacement_Variation_Count", count));

            GUI.Tooltip(LOC.Get("PARAM_MapPlacement_Variation_Count_TT", index.ScannedAt.ToString("HH:mm")));
        }
        else if (!index.IsScanning)
        {
            ImGui.SameLine();

            if (ImGui.SmallButton($"{LOC.Get("PARAM_RandomAppear_Scan")}##mapPlacementScan"))
                index.Scan();
            GUI.Tooltip(LOC.Get("PARAM_MapPlacement_Scan_TT"));
        }
    }

    private void DisplayAttachPoint(Param.Row attachRow, int idSuffix)
    {
        var tile = MapPatternLinks.AttachTile(Project, attachRow);
        var entityId = MapPatternLinks.AttachEntityId(attachRow);

        if (tile == null)
            return;

        ImGui.TextUnformatted(LOC.Get("PARAM_MapPlacement_Region", tile, entityId));

        // The region is usually in the large tile covering the small one: known once View in Map has searched
        var candidates = MapPatternLinks.AttachCandidates(Project, attachRow);

        if (candidates.Count == 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PARAM_MapPlacement_No_Tile", tile));
            return;
        }

        if (NrAttachPointLocator.TryGetCached(Project, entityId, out var location))
        {
            if (location != null)
                ImGui.TextDisabled(LOC.Get("PARAM_MapPlacement_Region_Found", location.RegionName, location.MapID));
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PARAM_MapPlacement_Region_Missing", string.Join(", ", candidates)));
        }

        if (ImGui.SmallButton($"{LOC.Get("PARAM_MapRef_Action_View_In_Map")}##mapPlacementViewAttach{idSuffix}_{attachRow.ID}"))
            MapPatternLinks.ViewAttachPoint(Project, attachRow);
        GUI.Tooltip(LOC.Get("PARAM_MapPlacement_View_Attach_TT", string.Join(", ", candidates)));
    }

    private void DisplayScenario(Param.Row row)
    {
        // CSScenarioPlacementManagerImp matches the part's Key 1 to placeScenarioId (+4), Key 2 to
        // conditionScenarioId (+6) and Step to conditionScenarioStage (+8): MSB Research.md §5.4
        var key0 = MapPatternLinks.GetInt(row, "placeScenarioId");
        var key1 = MapPatternLinks.GetInt(row, "conditionScenarioId");
        var step = MapPatternLinks.GetInt(row, "conditionScenarioStage");

        ImGui.TextUnformatted(LOC.Get("PARAM_MapPlacement_Scenario_Keys", key0, key1, step));
        GUI.Tooltip(LOC.Get("PARAM_MapPlacement_Scenario_Keys_TT"));

        PlacementUsersList.Display(Project, index => index.UsingScenario(key0, key1, step), "scenario",
            LOC.Get("PARAM_MapPlacement_Scenario_Used_By_TT"));
    }
}
