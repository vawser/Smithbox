using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;
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

        GUI.SimpleHeader("Groups", "");

        DisplaySearchbar();

        ImGui.BeginChild("MapGroups", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayMapGroupsList();

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

    public void DisplaySearchbar()
    {
        var map = View.Selection.SelectedMapContainer;

        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedListFilter_mapGroupsList", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("mapEditor_MapGroupsFilter",
            ref MapGroupsFilter, ref ExactMapGroupsFilter);

        ImGui.EndChild();
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

            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                FrameMapGroupEntry(curEntry);
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

            // Pull
            if (ImGui.Selectable("Pull to Camera"))
            {
                PullMapGroupEntry(curEntry);
            }

            // Duplicate
            if (ImGui.Selectable("Duplicate"))
            {
                DuplicateMapGroupEntry(curEntry);
            }

            // Delete
            if (ImGui.BeginMenu("Delete"))
            {
                if (ImGui.Selectable("Delete Group"))
                {
                    DeleteMapGroupEntry(curEntry.GUID);
                }
                if (ImGui.Selectable("Delete Group and Objects"))
                {
                    DeleteMapGroupEntryObjects(curEntry);
                    DeleteMapGroupEntry(curEntry.GUID);
                }
                if (ImGui.Selectable("Delete Objects"))
                {
                    DeleteMapGroupEntryObjects(curEntry);
                }

                ImGui.EndMenu();
            }

            // Visibility
            if (ImGui.BeginMenu("Visibility"))
            {
                if (ImGui.Selectable("Show"))
                {
                    ToggleMapGroupEntryVisibility(curEntry, true);
                }
                if (ImGui.Selectable("Hide"))
                {
                    ToggleMapGroupEntryVisibility(curEntry, false);
                }

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    public void ContentsContextMenu(Entity ent)
    {
        if (ImGui.Selectable("Create Map Group Entry"))
        {
            var curSelection = View.ViewportSelection.GetEntitySelection();

            CreateMapGroupEntry(curSelection);
        }
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
