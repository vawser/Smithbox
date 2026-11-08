using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Stored selections of map objects, per project and per map, shown as a section of the Tools window.
/// </summary>
public class SelectionGroupsTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    public string CurrentMapGroupEntryGUID = "";
    public MapGroupEntry CurrentMapGroupEntry = null;

    private string MapGroupsFilter = "";
    private bool ExactMapGroupsFilter = false;

    private bool ShowContentsList = true;

    public SelectionGroupsTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Called every frame by the view, so the shortcuts work while the section is hidden.
    /// </summary>
    public void Update()
    {
        Shortcuts();
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MAP_ToolWindow_Selection_Groups")}##mapSelectionGroupsSectionHeader"))
            return;

        ImGui.BeginChild("mapSelectionGroupsSection", ImGuiChildFlags.Borders);

        // The group shortcuts (duplicate, delete, deselect) act while the section is hovered
        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
        {
            FocusManager.SetFocus(EditorFocusContext.MapEditor_MapGroups);
        }

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MAP_SelectionGroups_Hint"));

        var mapID = View.Selection.SelectedMapID;

        if (mapID == null)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("MAP_SelectionGroups_No_Map"));
            return;
        }

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SelectionGroups_Options_Header"),
            LOC.Get("MAP_SelectionGroups_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_SelectionGroups_Auto_Adjust_Name")}##selectionGroupsAutoAdjustName",
            ref CFG.Current.MapEditor_MapContentGroup_AutoAdjustName);
        GUI.Tooltip(LOC.Get("MAP_SelectionGroups_Auto_Adjust_Name_TT"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SelectionGroups_Actions_Header"),
            LOC.Get("MAP_SelectionGroups_Actions_Header_TT"));

        var hasSelection = View.ViewportSelection.IsSelection();

        GUI.ConditionalMultiButtonInput("selectionGroupsActions",
            "selectionGroupsCreate", LOC.Get("MAP_SelectionGroups_Create"),
            LOC.Get("MAP_SelectionGroups_Create_TT", InputManager.GetHint(KeybindID.MapEditor_Create_Map_Group_Entry)),
            () => CreateMapGroupEntry(View.ViewportSelection.GetEntitySelection()), hasSelection);

        Project.Handler.MapData.MapGroupsList.List.TryGetValue(mapID, out var mapGroups);

        // A group picked in another map isn't shown for this one
        var curEntry = mapGroups != null && CurrentMapGroupEntry != null && mapGroups.Groups.Contains(CurrentMapGroupEntry)
            ? CurrentMapGroupEntry
            : null;

        // Groups list on the left, the selected group's actions on the right
        GUI.Spacer();

        var tableSize = new Vector2(0, ImGui.GetContentRegionAvail().Y);

        if (!ImGui.BeginTable("mapSelectionGroupsSplit", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV, tableSize))
            return;

        ImGui.TableSetupColumn("##groupsListColumn", ImGuiTableColumnFlags.WidthStretch, 0.45f);
        ImGui.TableSetupColumn("##groupActionsColumn", ImGuiTableColumnFlags.WidthStretch, 0.55f);

        ImGui.TableNextRow();

        // Groups
        ImGui.TableSetColumnIndex(0);

        ImGui.BeginChild("mapSelectionGroupsListColumn", new Vector2(0, ImGui.GetContentRegionAvail().Y));

        GUI.SimpleHeader(
            LOC.Get("MAP_SelectionGroups_Groups_Header"),
            LOC.Get("MAP_SelectionGroups_Groups_Header_TT"));

        EditorFilters.DisplayFramedListFilter("mapEditor_SelectionGroupsFilter",
            ref MapGroupsFilter, ref ExactMapGroupsFilter);

        ImGui.BeginChild("mapSelectionGroupsList", ImGuiChildFlags.Borders);

        DisplayMapGroupsList(mapGroups);

        ImGui.EndChild();

        ImGui.EndChild();

        // Group actions
        ImGui.TableSetColumnIndex(1);

        ImGui.BeginChild("mapSelectionGroupActionsColumn", new Vector2(0, ImGui.GetContentRegionAvail().Y));

        DisplayGroupActions(curEntry);

        ImGui.EndChild();

        ImGui.EndTable();
    }

    public void Shortcuts()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Create_Map_Group_Entry))
            {
                var curSelection = View.ViewportSelection.GetEntitySelection();

                CreateMapGroupEntry(curSelection);
            }
        }

        if (FocusManager.IsFocus(EditorFocusContext.MapEditor_MapGroups))
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Deselect_All))
            {
                CurrentMapGroupEntryGUID = "";
                CurrentMapGroupEntry = null;
            }

            if (InputManager.IsPressed(KeybindID.Duplicate))
            {
                if (CurrentMapGroupEntry != null)
                {
                    DuplicateMapGroupEntry(CurrentMapGroupEntry);
                }
            }

            if (InputManager.IsPressed(KeybindID.Delete))
            {
                if (CurrentMapGroupEntry != null)
                {
                    DeleteMapGroupEntryObjects(CurrentMapGroupEntry);
                    DeleteMapGroupEntry(CurrentMapGroupEntryGUID);
                }
            }
        }
    }

    private void DisplayMapGroupsList(MapGroups mapGroups)
    {
        if (mapGroups == null || mapGroups.Groups.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_SelectionGroups_No_Groups"));
            return;
        }

        for (int i = 0; i < mapGroups.Groups.Count; i++)
        {
            DisplayMapGroupEntry(mapGroups.Groups[i], i);
        }
    }

    public void DisplayMapGroupEntry(MapGroupEntry curEntry, int index)
    {
        var selected = CurrentMapGroupEntryGUID == curEntry.GUID;

        var isMatch = EditorFilters.IsMatch(MapGroupsFilter, curEntry.Name, ExactMapGroupsFilter);

        if (!isMatch)
            return;

        if (ImGui.Selectable($"{curEntry.Name}##{curEntry.GUID}_mapGroupEntry", selected, ImGuiSelectableFlags.AllowDoubleClick))
        {
            CurrentMapGroupEntry = curEntry;
            CurrentMapGroupEntryGUID = curEntry.GUID;

            // If shift is held, add the contents to the current selection
            // Useful for selecting multiple groups
            if (InputManager.HasShiftDown())
            {
                AddToSelectionMapGroupEntry(curEntry);
            }
            else
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    FrameMapGroupEntry(curEntry);
                }
            }
        }
    }

    public void SelectMapGroupEntry(MapGroupEntry curEntry)
    {
        var map = View.Selection.SelectedMapContainer;

        View.ViewportSelection.ClearSelection();

        foreach (MsbEntity c in map.Objects)
        {
            if (curEntry.Objects.Any(e => e == c.Name))
            {
                View.ViewportSelection.AddSelection(c);
            }
        }
    }

    public void AddToSelectionMapGroupEntry(MapGroupEntry curEntry)
    {
        var map = View.Selection.SelectedMapContainer;

        foreach (MsbEntity c in map.Objects)
        {
            if (curEntry.Objects.Any(e => e == c.Name))
            {
                View.ViewportSelection.AddSelection(c);
            }
        }
    }

    public void DuplicateMapGroupEntry(MapGroupEntry curEntry)
    {
        SelectMapGroupEntry(curEntry);

        if (View.ViewportSelection.IsSelection())
        {
            var mapObjects = View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();
            // The group's members can be in any loaded map, so each clone goes into its source's own map
            EntDuplicateAction action = new(
                view: View,
                objects: mapObjects,
                targetMap: null,
                targetBTL: null,
                duplicateToMap: false,
                createMapGroupEntry: true,
                incrementEntityID: CFG.Current.Toolbar_Duplicate_Increment_Entity_ID,
                incrementInstanceID: CFG.Current.Toolbar_Duplicate_Increment_InstanceID,
                incrementPartNames: CFG.Current.Toolbar_Duplicate_Increment_PartNames,
                clearEntityID: CFG.Current.Toolbar_Duplicate_Clear_Entity_ID,
                clearEntityGroupID: CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs,
                randomiseRotation: CFG.Current.Toolbar_Duplicate_Randomise_Rotation,
                randomRotationDir: CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Dir,
                randomRotationRange: CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Range,
                placeAtListEnd: CFG.Current.Toolbar_Duplicate_Place_at_List_End);

            View.ViewportActionManager.ExecuteAction(action);
        }
        else
        {
            Smithbox.LogError<DuplicateAction>("No object selected.");
        }

        View.DelayPicking();
    }

    /// <summary>
    /// The actions for the selected group, shown in the right column.
    /// </summary>
    private void DisplayGroupActions(MapGroupEntry curEntry)
    {
        if (curEntry == null)
        {
            GUI.WrappedText(LOC.Get("MAP_SelectionGroups_No_Group_Selected"));
            return;
        }

        var hasSelection = View.ViewportSelection.IsSelection();

        // Name
        GUI.SimpleHeader(
            LOC.Get("MAP_SelectionGroups_Name_Header"),
            LOC.Get("MAP_SelectionGroups_Name_Header_TT"));

        // The GUID in the ID keeps an unfinished edit from carrying over to another group
        var curName = curEntry.Name;
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText($"##selectionGroupName_{curEntry.GUID}", ref curName, 255);
        GUI.Tooltip(LOC.Get("MAP_SelectionGroups_Name_TT"));

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            curEntry.Name = curName;
            SaveMapGroups();
        }

        // Selection
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SelectionGroups_Selection_Header"),
            LOC.Get("MAP_SelectionGroups_Selection_Header_TT"));

        GUI.MultiButtonInput("selectionGroupSelection",
            "selectionGroupFrame", LOC.Get("MAP_SelectionGroups_Select"), LOC.Get("MAP_SelectionGroups_Select_TT"),
            () => FrameMapGroupEntry(curEntry),

            "selectionGroupAddToSelection", LOC.Get("MAP_SelectionGroups_Add_To_Selection"), LOC.Get("MAP_SelectionGroups_Add_To_Selection_TT"),
            () => AddToSelectionMapGroupEntry(curEntry));

        // Objects
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SelectionGroups_Objects_Header"),
            LOC.Get("MAP_SelectionGroups_Objects_Header_TT"));

        GUI.MultiButtonInput("selectionGroupObjects",
            "selectionGroupPull", LOC.Get("MAP_SelectionGroups_Pull_To_Camera"), LOC.Get("MAP_SelectionGroups_Pull_To_Camera_TT"),
            () => PullMapGroupEntry(curEntry),

            "selectionGroupDuplicate", LOC.Get("MAP_SelectionGroups_Duplicate"), LOC.Get("MAP_SelectionGroups_Duplicate_TT"),
            () => DuplicateMapGroupEntry(curEntry));

        // Contents
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SelectionGroups_Edit_Contents_Header"),
            LOC.Get("MAP_SelectionGroups_Edit_Contents_Header_TT"));

        GUI.ConditionalMultiButtonInput("selectionGroupContents",
            "selectionGroupAddObjects", LOC.Get("MAP_SelectionGroups_Add_Selected"), LOC.Get("MAP_SelectionGroups_Add_Selected_TT"),
            () => AddToMapGroupEntry(curEntry), hasSelection,

            "selectionGroupRemoveObjects", LOC.Get("MAP_SelectionGroups_Remove_Selected"), LOC.Get("MAP_SelectionGroups_Remove_Selected_TT"),
            () => RemoveFromMapGroupEntry(curEntry), hasSelection);

        // Visibility
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SelectionGroups_Visibility_Header"),
            LOC.Get("MAP_SelectionGroups_Visibility_Header_TT"));

        GUI.MultiButtonInput("selectionGroupVisibility",
            "selectionGroupShow", LOC.Get("MAP_SelectionGroups_Show"), LOC.Get("MAP_SelectionGroups_Show_TT"),
            () => ToggleMapGroupEntryVisibility(curEntry, true),

            "selectionGroupHide", LOC.Get("MAP_SelectionGroups_Hide"), LOC.Get("MAP_SelectionGroups_Hide_TT"),
            () => ToggleMapGroupEntryVisibility(curEntry, false));

        // Delete
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SelectionGroups_Delete_Header"),
            LOC.Get("MAP_SelectionGroups_Delete_Header_TT"));

        GUI.MultiButtonInput("selectionGroupDelete",
            "selectionGroupDeleteGroup", LOC.Get("MAP_SelectionGroups_Delete_Group"), LOC.Get("MAP_SelectionGroups_Delete_Group_TT"),
            () => DeleteMapGroupEntry(curEntry.GUID),

            "selectionGroupDeleteBoth", LOC.Get("MAP_SelectionGroups_Delete_Group_And_Objects"), LOC.Get("MAP_SelectionGroups_Delete_Group_And_Objects_TT"),
            () =>
            {
                DeleteMapGroupEntryObjects(curEntry);
                DeleteMapGroupEntry(curEntry.GUID);
            },

            "selectionGroupDeleteObjects", LOC.Get("MAP_SelectionGroups_Delete_Objects"), LOC.Get("MAP_SelectionGroups_Delete_Objects_TT"),
            () => DeleteMapGroupEntryObjects(curEntry));

        // Contents List
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("MAP_SelectionGroups_Contents_Header", curEntry.Objects.Count),
            LOC.Get("MAP_SelectionGroups_Contents_Header_TT"),
            ref ShowContentsList);

        if (ShowContentsList)
        {
            ImGui.BeginChild("selectionGroupContentsList", ImGuiChildFlags.Borders);

            if (curEntry.Objects.Count == 0)
            {
                GUI.WrappedText(LOC.Get("MAP_SelectionGroups_Contents_Empty"));
            }

            foreach (var name in curEntry.Objects)
            {
                ImGui.TextUnformatted(name);
            }

            ImGui.EndChild();
        }
    }

    /// <summary>
    /// Entry for the Map Contents context menu.
    /// </summary>
    public void ContentsContextMenu(Entity ent)
    {
        if (ImGui.Selectable(LOC.Get("MAP_SelectionGroups_Create_Context")))
        {
            var curSelection = View.ViewportSelection.GetEntitySelection();

            CreateMapGroupEntry(curSelection);
        }
        GUI.Tooltip(LOC.Get("MAP_SelectionGroups_Create_Context_TT"));
    }

    public string CreateMapGroupEntry(List<MsbEntity> selection)
    {
        var objectNames = new List<string>();

        foreach (var entry in selection)
        {
            objectNames.Add(entry.Name);
        }

        var mapID = View.Selection.SelectedMapID;

        var mapGroupEntry = new MapGroupEntry();
        mapGroupEntry.GUID = Guid.NewGuid().ToString();
        mapGroupEntry.Name = "Untitled";
        mapGroupEntry.Objects = objectNames;

        if (!Project.Handler.MapData.MapGroupsList.List.ContainsKey(mapID))
        {
            var mapGroups = new MapGroups();
            mapGroups.MapID = mapID;
            mapGroups.Groups = new()
            {
                mapGroupEntry
            };

            Project.Handler.MapData.MapGroupsList.List.Add(mapID, mapGroups);
        }
        else
        {
            var mapGroups = Project.Handler.MapData.MapGroupsList.List[mapID];
            mapGroups.Groups.Add(mapGroupEntry);
        }

        SaveMapGroups();

        return mapGroupEntry.GUID;
    }

    public void DeleteMapGroupEntry(string guid)
    {
        var mapID = View.Selection.SelectedMapID;

        if (Project.Handler.MapData.MapGroupsList.List.ContainsKey(mapID))
        {
            var mapGroups = Project.Handler.MapData.MapGroupsList.List[mapID];

            if (mapGroups.Groups.Any(e => e.GUID.ToString() == guid))
            {
                var targetGroup = mapGroups.Groups.FirstOrDefault(e => e.GUID.ToString() == guid);
                if (targetGroup != null)
                {
                    mapGroups.Groups.Remove(targetGroup);
                }

                if (CurrentMapGroupEntryGUID == guid)
                {
                    CurrentMapGroupEntryGUID = "";
                    CurrentMapGroupEntry = null;
                }
            }
        }

        SaveMapGroups();
    }

    public void ToggleMapGroupEntryVisibility(MapGroupEntry curEntry, bool visibleState)
    {
        var map = View.Selection.SelectedMapContainer;

        foreach (MsbEntity c in map.Objects)
        {
            if (curEntry.Objects.Any(e => e == c.Name))
            {
                c.EditorVisible = visibleState;
            }
        }
    }

    public void DeleteMapGroupEntryObjects(MapGroupEntry curEntry)
    {
        SelectMapGroupEntry(curEntry);

        View.DeleteAction.ApplyDelete();
    }

    public void FrameMapGroupEntry(MapGroupEntry curEntry)
    {
        SelectMapGroupEntry(curEntry);

        View.FrameAction.ApplyViewportFrame();
    }

    public void PullMapGroupEntry(MapGroupEntry curEntry)
    {
        SelectMapGroupEntry(curEntry);

        View.PullToCameraAction.ApplyMoveToCamera();
    }

    public void AddToMapGroupEntry(MapGroupEntry curEntry)
    {
        var newEntities = View.ViewportSelection.GetEntitySelection();

        foreach (var entry in newEntities)
        {
            if (!curEntry.Objects.Any(e => e == entry.Name))
            {
                curEntry.Objects.Add(entry.Name);
            }
        }

        SaveMapGroups();
    }

    public void RemoveFromMapGroupEntry(MapGroupEntry curEntry)
    {
        var newEntities = View.ViewportSelection.GetEntitySelection();

        foreach (var entry in newEntities)
        {
            if (curEntry.Objects.Any(e => e == entry.Name))
            {
                curEntry.Objects.Remove(entry.Name);
            }
        }

        SaveMapGroups();
    }

    public void UpdateMapGroupEntry(string oldName, string newName)
    {
        var existingEntry = Project.Handler.MapData.MapGroupsList.List
            .FirstOrDefault(e => e.Value.Groups
            .Any(e => e.Objects
            .Any(e => e == oldName)));

        if (existingEntry.Key == null)
            return;

        var existingGroup = existingEntry.Value.Groups
            .FirstOrDefault(e => e.Objects
            .Any(e => e == oldName));

        if (existingGroup == null)
            return;

        existingGroup.Objects.Remove(oldName);
        existingGroup.Objects.Add(newName);

        SaveMapGroups();
    }

    public void SaveMapGroups()
    {
        var projectFolder = Path.Combine(
            Project.Descriptor.ProjectPath,
            ".smithbox",
            "MSB");

        if (!Directory.Exists(projectFolder))
        {
            Directory.CreateDirectory(projectFolder);
        }

        var projectFile = Path.Combine(
            projectFolder,
            "Map Groups.json");

        string jsonString = JsonSerializer.Serialize(Project.Handler.MapData.MapGroupsList, MapEditorJsonSerializerContext.Default.MapGroupsList);

        try
        {
            var fs = new FileStream(projectFile, FileMode.Create);
            var data = Encoding.ASCII.GetBytes(jsonString);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Dispose();
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_Failed_Write_Map_Object_Selections", projectFile), ex);
        }
    }
}
