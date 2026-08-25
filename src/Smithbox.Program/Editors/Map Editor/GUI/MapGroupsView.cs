using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace StudioCore.Editors.MapEditor;

public class MapGroupsView
{
    public MapEditorView View;
    public ProjectEntry Project;

    public string ImguiID = "MapGroupsView";

    public string CurrentMapGroupEntryGUID = "";
    public MapGroupEntry CurrentMapGroupEntry = null;

    private string MapGroupsFilter = "";
    private bool ExactMapGroupsFilter = false;

    public MapGroupsView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        Shortcuts();

        GUI.SimpleHeader("Groups", "Store groups of map objects as selections here. You can then easily re-select or manipulate them.\nGroups are specific to each project and to each map.");

        DisplayHeader();

        ImGui.BeginChild("MapGroups", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayMapGroupsList();

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        var map = View.Selection.SelectedMapContainer;

        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedListFilter_mapGroupsList", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("mapEditor_MapGroupsFilter",
            ref MapGroupsFilter, ref ExactMapGroupsFilter);

        // Name Auto Adjust
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.LightbulbO}##toggleNameAutoAdjust"))
        {
            CFG.Current.MapEditor_MapContentGroup_AutoAdjustName = !CFG.Current.MapEditor_MapContentGroup_AutoAdjustName;
        }

        var nameAutoAdjustMode = "Map object names are not automatically updated when changed.";
        if (CFG.Current.MapEditor_MapContentGroup_AutoAdjustName)
            nameAutoAdjustMode = "Map object names are automatically updated when changed.";

        GUI.Tooltip($"Determines if the map object names stored within a group are automatically updated to the new name if the map object's name is edited.\nCurrent Mode: {nameAutoAdjustMode}");

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

    public void DisplayMapGroupsList()
    {
        var mapID = View.Selection.SelectedMapID;

        ImGui.BeginChild($"mapGroupsList_{ImguiID}");

        MapGroups curGroup = null;

        if(mapID != null && Project.Handler.MapData.MapGroupsList.List.ContainsKey(mapID))
        {
            curGroup = Project.Handler.MapData.MapGroupsList.List[mapID];

            for (int i = 0; i < curGroup.Groups.Count; i++)
            {
                var curGroupEntry = curGroup.Groups[i];

                DisplayMapGroupEntry(curGroupEntry, i);
            }
        }
        else if(mapID == null)
        {
            GUI.WrappedText("No map has been loaded yet.");
        }
        else 
        {
            GUI.WrappedText("No groups exist yet.");
        }

        ImGui.EndChild();
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

        if(selected)
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
            if(curEntry.Objects.Any(e => e == c.Name))
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
            var mapContainer = View.Selection.SelectedMapContainer;
            var btlParent = mapContainer.BTLParents.FirstOrDefault();

            EntDuplicateAction action = new(View, mapObjects, mapContainer, btlParent, false, true);
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
                List<string> nameList = new();
                
                var line = "";
                for(int i = 0; i < curEntry.Objects.Count; i++)
                {
                    var name = curEntry.Objects[i];

                    if (i % 6 == 0)
                    {
                        line = $"{line} |";
                        nameList.Add(line);
                        line = "";
                    }
                    else
                    {
                        line = $"{line} | {name}";
                    }
                }

                foreach (var entry in nameList)
                {
                    ImGui.Text(entry);
                }

                ImGui.EndMenu();
            }
            GUI.Tooltip("Display the contents of this group.");


            ImGui.EndPopup();
        }
    }

    public void ContentsContextMenu(Entity ent)
    {
        if (ImGui.Selectable("Create New Map Contents Group"))
        {
            var curSelection = View.ViewportSelection.GetEntitySelection();

            CreateMapGroupEntry(curSelection);
        }
        GUI.Tooltip("Create a new map contents group from the current selection.");
    }

    public string CreateMapGroupEntry(List<MsbEntity> selection)
    {
        var objectNames = new List<string>();

        foreach(var entry in selection)
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

            if(mapGroups.Groups.Any(e => e.GUID.ToString() == guid))
            {
                var targetGroup = mapGroups.Groups.FirstOrDefault(e  => e.GUID.ToString() == guid);
                if(targetGroup != null)
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
        var mapID = View.Selection.SelectedMapID;

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
