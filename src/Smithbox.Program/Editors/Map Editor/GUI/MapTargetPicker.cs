using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The "target map" combo shared by the tools that act on a chosen loaded map (Create, Duplicate to Map,
/// Move to Map, Prefab, Entity ID Check, Rename Map Objects). Besides each loaded map, it offers "Active Map",
/// which follows the map the content tree shows (the map of the last selected object, or the one picked in
/// the Map List). Resolve the choice with MapSelection.ResolveTargetMap.
/// </summary>
public static class MapTargetPicker
{
    public const string ActiveMapID = "<active>";

    /// <summary>
    /// The choice that follows the active map
    /// </summary>
    public static (string, ObjectContainer) ActiveMap => (ActiveMapID, null);

    /// <summary>
    /// No map chosen
    /// </summary>
    public static (string, ObjectContainer) None => ("None", null);

    public static bool IsActiveMap((string MapID, ObjectContainer Container) target) => target.MapID == ActiveMapID;

    /// <summary>
    /// Draws the combo. Returns true if the choice changed.
    /// </summary>
    public static bool Display(MapEditorView view, string id, ref (string, ObjectContainer) target, bool allowActiveMap = true)
    {
        var changed = false;

        GUI.SetInputWidth();
        if (ImGui.BeginCombo($"##{id}", GetPreview(view, target)))
        {
            if (allowActiveMap)
            {
                if (ImGui.Selectable(GetActiveMapLabel(view), IsActiveMap(target)))
                {
                    target = ActiveMap;
                    changed = true;
                }
                GUI.Tooltip(LOC.Get("MAP_Target_Picker_Active_Map_TT"));

                ImGui.Separator();
            }

            foreach (var entry in view.Project.Handler.MapData.PrimaryBank.Maps)
            {
                var map = entry.Value.MapContainer;

                if (map == null)
                    continue;

                var mapID = entry.Key.Filename;

                if (ImGui.Selectable(GetMapLabel(view, mapID), !IsActiveMap(target) && target.Item1 == mapID))
                {
                    target = (mapID, map);
                    changed = true;
                }
            }

            ImGui.EndCombo();
        }

        return changed;
    }

    private static string GetPreview(MapEditorView view, (string MapID, ObjectContainer Container) target)
    {
        if (IsActiveMap(target))
            return GetActiveMapLabel(view);

        // A map that has been unloaded since it was chosen
        if (target.Container == null || view.Selection.ResolveTargetMap(target) == null)
            return LOC.Get("MAP_Target_Picker_None");

        return GetMapLabel(view, target.MapID);
    }

    private static string GetActiveMapLabel(MapEditorView view)
    {
        var active = view.Selection.SelectedMapContainer;

        return active != null
            ? LOC.Get("MAP_Target_Picker_Active_Map", active.Name)
            : LOC.Get("MAP_Target_Picker_Active_Map_None");
    }

    private static string GetMapLabel(MapEditorView view, string mapID)
    {
        var alias = AliasHelper.GetMapNameAlias(view.Project, mapID);

        return string.IsNullOrEmpty(alias) ? mapID : $"{mapID}: {alias}";
    }
}
