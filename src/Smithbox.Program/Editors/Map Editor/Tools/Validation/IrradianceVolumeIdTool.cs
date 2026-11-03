using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;
using static StudioCore.Editors.MapEditor.IrradianceVolumeBank;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Lists the EnvMapEffectBoxes of the loaded maps with the irradiance volume each is linked to, found as the
/// game finds it (<see cref="IrradianceVolumeBank.ResolveVolumeId" />), and points out boxes the game skips.
/// In ER and NR it can also fill the ID property of boxes that leave it at -1, as every vanilla box does, with
/// the ID their name gives. That changes nothing in game, as the property then holds the same ID, but keeps the
/// link if the box is renamed. One undo reverts it.
/// </summary>
public class IrradianceVolumeIdTool
{
    private MapEditorView View;
    private ProjectEntry Project;

    /// <summary>
    /// Set to open the tool's tab the next time the tool window is drawn
    /// </summary>
    public bool FocusRequested;

    public IrradianceVolumeIdTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private IrradianceVolumeBank Bank => View.IrradianceVolumeBank;

    private IEnumerable<MapContainer> LoadedMaps()
    {
        return Project.Handler.MapData.PrimaryBank.Maps.Values
            .Select(w => w.MapContainer)
            .Where(m => m != null && m.LoadState == MapContentLoadState.Loaded)
            .OrderBy(m => m.Name);
    }

    private static IEnumerable<Entity> Boxes(MapContainer map)
    {
        return map.Regions.Where(e => GetVolumeIdProperty(e.WrappedObject) != null);
    }

    /// <summary>
    /// The boxes of a map whose ID property is -1 and whose name gives an ID
    /// </summary>
    private List<(Entity Entity, short Id)> GetFillable(MapContainer map)
    {
        var fillable = new List<(Entity, short)>();

        if (!Bank.HasNameFallback())
            return fillable;

        foreach (var entity in Boxes(map))
        {
            if (GetVolumeId(entity.WrappedObject) >= 0)
                continue;

            if (ReadIdFromName((IMsbRegion)entity.WrappedObject, out var id) == NameIssue.None)
                fillable.Add((entity, id));
        }

        return fillable;
    }

    public void OnToolWindow()
    {
        if (!Bank.CanUse())
            return;

        GUI.WrappedText(LOC.Get(Bank.HasNameFallback() ? "MAP_GI_Tool_Description" : "MAP_GI_Tool_Description_AC6"));
        GUI.Spacer();

        var maps = LoadedMaps().ToList();

        if (maps.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_GI_Tool_No_Maps"));
            return;
        }

        foreach (var map in maps)
        {
            DisplayMap(map);
        }
    }

    private void DisplayMap(MapContainer map)
    {
        var summary = Bank.Summarise(map);
        var volumes = Bank.GetMap(map.Name);

        GUI.SimpleHeader(map.Name, "");
        GUI.WrappedText(LOC.Get("MAP_GI_Tool_Map_Summary",
            volumes?.Infos.Count ?? 0, summary.Boxes, summary.Linked, summary.Unlinked, summary.LinkedWithoutVolume,
            summary.SkippedByGame));

        var fillable = GetFillable(map);

        if (fillable.Count > 0)
        {
            if (ImGui.Button($"{LOC.Get("MAP_GI_Tool_Fill", fillable.Count)}##fill_{map.Name}"))
            {
                Fill(map, fillable);
            }
            GUI.Tooltip(LOC.Get("MAP_GI_Tool_Fill_TT"));
        }

        if (ImGui.BeginTable($"##boxes_{map.Name}", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn(LOC.Get("MAP_GI_Tool_Column_Box"));
            ImGui.TableSetupColumn(LOC.Get("MAP_GI_Tool_Column_Property"));
            ImGui.TableSetupColumn(LOC.Get("MAP_GI_Tool_Column_Id"));
            ImGui.TableSetupColumn(LOC.Get("MAP_GI_Tool_Column_Status"));
            ImGui.TableHeadersRow();

            foreach (var entity in Boxes(map))
            {
                var region = (IMsbRegion)entity.WrappedObject;
                var property = GetVolumeId(region) ?? -1;
                var id = Bank.ResolveVolumeId(region, out var source);

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{region.Name}##box_{map.Name}_{entity.GetHashCode()}"))
                {
                    View.ViewportSelection.ClearSelection();
                    View.ViewportSelection.AddSelection(entity);
                }

                ImGui.TableNextColumn();
                ImGui.Text(property.ToString());

                ImGui.TableNextColumn();
                ImGui.Text(id >= 0 ? id.ToString() : "-");

                ImGui.TableNextColumn();
                ImGui.TextWrapped(GetStatus(region, id, source, volumes));
            }

            ImGui.EndTable();
        }

        GUI.Spacer();
    }

    private string GetStatus(IMsbRegion region, short id, LinkSource source, MapVolumes volumes)
    {
        if (Bank.HasNameFallback())
        {
            var issue = ReadIdFromName(region, out _);

            if (issue == NameIssue.NotBox)
                return LOC.Get("MAP_GI_Tool_Status_Skipped_Shape");

            if (issue == NameIssue.NameLength)
                return LOC.Get("MAP_GI_Tool_Status_Skipped_Name");
        }

        if (id < 0)
            return LOC.Get("MAP_GI_Tool_Status_Unlinked");

        var hasVolume = volumes != null && volumes.HasVolume(id);

        var status = source == LinkSource.Name
            ? LOC.Get(hasVolume ? "MAP_GI_Tool_Status_Linked_Name" : "MAP_GI_Tool_Status_Linked_Name_No_Volume")
            : LOC.Get(hasVolume ? "MAP_GI_Tool_Status_Linked_Property" : "MAP_GI_Tool_Status_Linked_Property_No_Volume");

        // The game doesn't check the name's last three characters are digits
        if (source == LinkSource.Name && ReadIdFromName(region, out _) == NameIssue.NotDigits)
            status += " " + LOC.Get("MAP_GI_Tool_Status_Not_Digits");

        return status;
    }

    /// <summary>
    /// Write the IDs as one action, so a single undo reverts them all
    /// </summary>
    private void Fill(MapContainer map, List<(Entity Entity, short Id)> fillable)
    {
        var actions = new List<ViewportAction>();

        foreach (var (entity, id) in fillable)
        {
            var prop = GetVolumeIdProperty(entity.WrappedObject);
            actions.Add(new PropChangeAction(entity, prop, entity.WrappedObject, id));
        }

        var action = new ViewportCompoundAction(actions);
        (map.OwnerView ?? View).ViewportActionManager.ExecuteAction(action);

        Smithbox.Log(this, LOC.Get("MAP_GI_Tool_Fill_Log", fillable.Count, map.Name));
    }
}
