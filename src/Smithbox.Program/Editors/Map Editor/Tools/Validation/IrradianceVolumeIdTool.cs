using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Lists the EnvMapEffectBoxes of the loaded maps with their irradiance volume links, and in ER and NR offers to
/// fill the volume ID property of boxes that leave it at -1, as every vanilla box does.
/// The game reads those boxes' IDs from their names, so filling the property with the same ID changes nothing in
/// game, but makes the link survive a rename. This is the only place Smithbox reads a box's name for its volume,
/// and only as the source of that one-off, undoable write.
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

    /// <summary>
    /// Why the game's name rule gives no ID for a box
    /// </summary>
    public enum NameIssue
    {
        None,
        NotBox,
        NameLength,
        NotDigits
    }

    /// <summary>
    /// The ID the game reads from an ER/NR box's name when its ID property is -1: it loads only Box shapes with
    /// names of exactly 10 characters (e.g. Env_Box203), and takes characters 7 to 9 as a decimal number
    /// </summary>
    public static NameIssue ReadIdFromName(IMsbRegion region, out short id)
    {
        id = -1;

        if (region.Shape is not MSB.Shape.Box)
            return NameIssue.NotBox;

        var name = region.Name;

        if (name == null || name.Length != 10)
            return NameIssue.NameLength;

        // The game doesn't check these are digits, but anything else gives an ID that is meaningless to fill
        if (!char.IsAsciiDigit(name[7]) || !char.IsAsciiDigit(name[8]) || !char.IsAsciiDigit(name[9]))
            return NameIssue.NotDigits;

        id = (short)((name[7] - '0') * 100 + (name[8] - '0') * 10 + (name[9] - '0'));

        return NameIssue.None;
    }

    private IEnumerable<MapContainer> LoadedMaps()
    {
        return Project.Handler.MapData.PrimaryBank.Maps.Values
            .Select(w => w.MapContainer)
            .Where(m => m != null && m.LoadState == MapContentLoadState.Loaded)
            .OrderBy(m => m.Name);
    }

    private static IEnumerable<Entity> Boxes(MapContainer map)
    {
        return map.Regions.Where(e => IrradianceVolumeBank.GetVolumeIdProperty(e.WrappedObject) != null);
    }

    /// <summary>
    /// The boxes of a map whose ID property is -1 and whose name gives the game an ID
    /// </summary>
    private List<(Entity Entity, short Id)> GetFillable(MapContainer map)
    {
        var fillable = new List<(Entity, short)>();

        if (!Bank.HasNameFallback())
            return fillable;

        foreach (var entity in Boxes(map))
        {
            if (IrradianceVolumeBank.GetVolumeId(entity.WrappedObject) >= 0)
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
            volumes?.Infos.Count ?? 0, summary.Boxes, summary.Linked, summary.Unlinked, summary.LinkedWithoutVolume));

        var fillable = GetFillable(map);

        if (fillable.Count > 0)
        {
            if (ImGui.Button($"{LOC.Get("MAP_GI_Tool_Fill", fillable.Count)}##fill_{map.Name}"))
            {
                Fill(map, fillable);
            }
            GUI.Tooltip(LOC.Get("MAP_GI_Tool_Fill_TT"));
        }

        if (ImGui.BeginTable($"##boxes_{map.Name}", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn(LOC.Get("MAP_GI_Tool_Column_Box"));
            ImGui.TableSetupColumn(LOC.Get("MAP_GI_Tool_Column_Id"));
            ImGui.TableSetupColumn(LOC.Get("MAP_GI_Tool_Column_Status"));
            ImGui.TableHeadersRow();

            foreach (var entity in Boxes(map))
            {
                var region = (IMsbRegion)entity.WrappedObject;
                var id = IrradianceVolumeBank.GetVolumeId(region) ?? -1;

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{region.Name}##box_{map.Name}_{entity.GetHashCode()}"))
                {
                    View.ViewportSelection.ClearSelection();
                    View.ViewportSelection.AddSelection(entity);
                }

                ImGui.TableNextColumn();
                ImGui.Text(id.ToString());

                ImGui.TableNextColumn();
                ImGui.TextWrapped(GetStatus(region, id, volumes));
            }

            ImGui.EndTable();
        }

        GUI.Spacer();
    }

    private string GetStatus(IMsbRegion region, short id, IrradianceVolumeBank.MapVolumes volumes)
    {
        var issue = NameIssue.None;
        short nameId = -1;

        if (Bank.HasNameFallback())
            issue = ReadIdFromName(region, out nameId);

        if (issue is NameIssue.NotBox or NameIssue.NameLength)
        {
            return LOC.Get(issue == NameIssue.NotBox ? "MAP_GI_Tool_Status_Skipped_Shape" : "MAP_GI_Tool_Status_Skipped_Name");
        }

        if (id >= 0)
        {
            return volumes != null && volumes.HasVolume(id)
                ? LOC.Get("MAP_GI_Tool_Status_Linked")
                : LOC.Get("MAP_GI_Tool_Status_No_Volume");
        }

        if (!Bank.HasNameFallback())
            return LOC.Get("MAP_GI_Tool_Status_Unlinked");

        if (issue == NameIssue.NotDigits)
            return LOC.Get("MAP_GI_Tool_Status_Not_Digits");

        return volumes != null && volumes.HasVolume(nameId)
            ? LOC.Get("MAP_GI_Tool_Status_Fillable", nameId)
            : LOC.Get("MAP_GI_Tool_Status_Fillable_No_Volume", nameId);
    }

    /// <summary>
    /// Write the IDs as one action, so a single undo reverts them all
    /// </summary>
    private void Fill(MapContainer map, List<(Entity Entity, short Id)> fillable)
    {
        var actions = new List<ViewportAction>();

        foreach (var (entity, id) in fillable)
        {
            var prop = IrradianceVolumeBank.GetVolumeIdProperty(entity.WrappedObject);
            actions.Add(new PropChangeAction(entity, prop, entity.WrappedObject, id));
        }

        var action = new ViewportCompoundAction(actions);
        (map.OwnerView ?? View).ViewportActionManager.ExecuteAction(action);

        Smithbox.Log(this, LOC.Get("MAP_GI_Tool_Fill_Log", fillable.Count, map.Name));
    }
}
