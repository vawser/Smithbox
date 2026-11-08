using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
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

        // Groups
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MAP_SelectionGroups_Groups_Header"),
            LOC.Get("MAP_SelectionGroups_Groups_Header_TT"));

        EditorFilters.DisplayFramedListFilter("mapEditor_SelectionGroupsFilter",
            ref MapGroupsFilter, ref ExactMapGroupsFilter);

        ImGui.BeginChild("mapSelectionGroupsList", ImGuiChildFlags.Borders);

        DisplayMapGroupsList(mapID);

        ImGui.EndChild();
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

    private void DisplayMapGroupsList(string mapID)
    {
        if (!Project.Handler.MapData.MapGroupsList.List.TryGetValue(mapID, out var curGroup)
            || curGroup.Groups.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_SelectionGroups_No_Groups"));
            return;
        }

        for (int i = 0; i < curGroup.Groups.Count; i++)
        {
            DisplayMapGroupEntry(curGroup.Groups[i], i);
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

        if (selected)
        {
            DisplayMapGroupEntryContext(curEntry);
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

    public void DisplayMapGroupEntryContext(MapGroupEntry curEntry)
    {
        if (ImGui.BeginPopupContextItem($@"mapGroupContext_{curEntry.GUID}"))
        {
            // Name
            var curName = curEntry.Name;
            ImGui.InputText("##mapGroupEntryName", ref curName, 255);
            GUI.Tooltip("Edit the name of this group.");

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                curEntry.Name = curName;
                SaveMapGroups();
            }

            // Select
            if (ImGui.Selectable("Select"))
            {
                FrameMapGroupEntry(curEntry);
            }
            GUI.Tooltip("Select and frame the map object contents of this group.");

            // Pull
            if (ImGui.Selectable("Pull to Camera"))
            {
                PullMapGroupEntry(curEntry);
            }
            GUI.Tooltip("Pull the map object contents of this group to the camera.");

            // Duplicate
            if (ImGui.Selectable("Duplicate"))
            {
                DuplicateMapGroupEntry(curEntry);
            }
            GUI.Tooltip("Duplicate this group and its map object contents.");

            // Delete
            if (ImGui.BeginMenu("Delete"))
            {
                if (ImGui.Selectable("Delete Group"))
                {
                    DeleteMapGroupEntry(curEntry.GUID);
                }
                GUI.Tooltip("Delete the group (leaves the map objects as is).");

                if (ImGui.Selectable("Delete Group and Objects"))
                {
                    DeleteMapGroupEntryObjects(curEntry);
                    DeleteMapGroupEntry(curEntry.GUID);
                }
                GUI.Tooltip("Delete the map objects that belong to this group, and the group.");

                if (ImGui.Selectable("Delete Objects"))
                {
                    DeleteMapGroupEntryObjects(curEntry);
                }
                GUI.Tooltip("Delete the map objects that belong to this group.");

                ImGui.EndMenu();
            }

            // Visibility
            if (ImGui.BeginMenu("Visibility"))
            {
                if (ImGui.Selectable("Show"))
                {
                    ToggleMapGroupEntryVisibility(curEntry, true);
                }
                GUI.Tooltip("Show (in the viewport) all map objects that belong to this group.");

                if (ImGui.Selectable("Hide"))
                {
                    ToggleMapGroupEntryVisibility(curEntry, false);
                }
                GUI.Tooltip("Hide (in the viewport) all map objects that belong to this group.");

                ImGui.EndMenu();
            }

            // Contents
            if (ImGui.BeginMenu("Group Contents"))
            {
                // Add To
                if (ImGui.Selectable("Add Selection"))
                {
                    AddToMapGroupEntry(curEntry);
                }
                GUI.Tooltip("Add the currently selected map objects to this group (if they are not already present).");

                // Remove From
                if (ImGui.Selectable("Remove Selection"))
                {
                    RemoveFromMapGroupEntry(curEntry);
                }
                GUI.Tooltip("Remove the currently selected map objects from this group (if they are present).");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("List of Contents"))
            {
                // Six names per line
                for (int i = 0; i < curEntry.Objects.Count; i += 6)
                {
                    ImGui.Text(string.Join(" | ", curEntry.Objects.Skip(i).Take(6)));
                }

                ImGui.EndMenu();
            }
            GUI.Tooltip("Display the contents of this group.");

            ImGui.EndPopup();
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
