using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using StudioCore.ViewportNS;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor.Core;

public class MapListView : Actions.Viewport.IActionEventHandler
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    private string ImguiID = "MapListView";

    public Dictionary<string, MapContentView> ContentViews = new();
    public bool SetupContentViews = false;

    public string SearchBarText = "";

    private bool DisplayChaliceDungeons = true;

    public MapListView(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }


    /// <summary>
    /// Handles the update for each frame
    /// </summary>
    public void OnGui()
    {
        var scale = DPI.UIScale();

        if (CFG.Current.Interface_MapEditor_MapList)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            // Map List
            if (ImGui.Begin($@"Map List##mapIdList", ImGuiWindowFlags.MenuBar))
            {
                Editor.FocusManager.SwitchWindowContext(MapEditorContext.MapIdList);

                // Setup the Content Views
                if (!SetupContentViews)
                {
                    SetupContentViews = true;

                    foreach (var entry in Project.MapData.MapFiles.Entries)
                    {
                        var newView = new MapContentView(Editor, Project, entry);

                        if (!ContentViews.ContainsKey(newView.MapID))
                        {
                            ContentViews.Add(newView.MapID, newView);
                        }
                    }
                }

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("Maps"))
                    {
                        if (ImGui.MenuItem("Unload Current"))
                        {
                            DialogResult result = PlatformUtils.Instance.MessageBox("Unload current?", "Confirm",
                                        MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                foreach (var entry in Editor.MapListView.ContentViews)
                                {
                                    if (entry.Value.MapID == Editor.Selection.SelectedMapID)
                                    {
                                        if (entry.Value.ContentLoadState == MapContentLoadState.Loaded)
                                            entry.Value.Unload();
                                    }
                                }
                            }
                        }
                        UIHelper.Tooltip("Unload the currently loaded and selected map.");

                        if (ImGui.MenuItem("Unload All"))
                        {
                            DialogResult result = PlatformUtils.Instance.MessageBox("Unload all maps?", "Confirm",
                                        MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                foreach (var entry in Editor.MapListView.ContentViews)
                                {
                                    if (entry.Value.ContentLoadState == MapContentLoadState.Loaded)
                                        entry.Value.Unload();
                                }

                                Editor.Universe.UnloadAllMaps();
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                                GC.Collect();
                            }
                        }
                        UIHelper.Tooltip("Unload all loaded maps.");

                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("List Filters"))
                    {
                        if (ImGui.BeginMenu("Select"))
                        {
                            Editor.MapListFilterTool.SelectionMenu();
                            ImGui.EndMenu();
                        }
                        UIHelper.Tooltip("Select an existing list filter to apply to the map list.");

                        if (ImGui.MenuItem("Clear"))
                        {
                            Editor.MapListFilterTool.Clear();
                        }
                        UIHelper.Tooltip("Clear the current list filter, resetting the filtering of the map list.");

                        ImGui.Separator();

                        if (ImGui.BeginMenu("Create"))
                        {
                            Editor.MapListFilterTool.CreationMenu();
                            ImGui.EndMenu();
                        }
                        UIHelper.Tooltip("Create a new list filter. The filter terms support regular expressions.");

                        if (ImGui.BeginMenu("Edit"))
                        {
                            Editor.MapListFilterTool.EditMenu();
                            ImGui.EndMenu();
                        }
                        UIHelper.Tooltip("Edit an existing list filter.");

                        if (ImGui.BeginMenu("Delete"))
                        {
                            Editor.MapListFilterTool.DeleteMenu();
                            ImGui.EndMenu();
                        }
                        UIHelper.Tooltip("Delete an existing list filter.");

                        ImGui.EndMenu();
                    }
                    UIHelper.Tooltip("Select a list filter to narrow the map list down to a pre-defined set of maps.");

                    if (Project.ProjectType is ProjectType.ER or ProjectType.NR)
                    {
                        if (ImGui.MenuItem("World Map"))
                        {
                            Editor.WorldMapTool.DisplayMenuOption();
                        }
                        UIHelper.Tooltip($"Open a world map with a visual representation of the map tiles.\nShortcut: {KeyBindings.Current.MAP_ToggleWorldMap.HintText}");
                    }

                    ImGui.EndMenuBar();
                }

                DisplaySearchbar();

                if (Editor.Project.ProjectType is ProjectType.BB)
                {
                    ImGui.SameLine();
                    DisplayChaliceToggleButton();
                }

                // Display List of Maps
                if (SetupContentViews)
                {
                    ImGui.BeginChild($"mapListSection");
                    DisplayMapList(MapContentLoadState.Loaded);
                    DisplayMapList(MapContentLoadState.Unloaded);
                    ImGui.EndChild();
                }
            }

            ImGui.End();
            ImGui.PopStyleColor();
        }

        if (CFG.Current.Interface_MapEditor_MapContents)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            // Map Contents
            if (ImGui.Begin($@"Map Contents##mapContentsPanel", ImGuiWindowFlags.MenuBar))
            {
                Editor.FocusManager.SwitchWindowContext(MapEditorContext.MapContents);

                if (Editor.Selection.SelectedMapView != null)
                {
                    Editor.Selection.SelectedMapView.OnGui();
                }
            }

            ImGui.End();
            ImGui.PopStyleColor(1);
        }

        Editor.MapListFilterTool.Update();

        Editor.ViewportSelection.ClearGotoTarget();

    }

    private string _lastSearchText = "";
    private HashSet<string> _cachedSearchMatches = new HashSet<string>();
    private Dictionary<string, string> _cachedMapNameAliases = new Dictionary<string, string>();
    private Dictionary<string, List<string>> _cachedMapTags = new Dictionary<string, List<string>>();
    private bool _updateMapList = true;

    public void UpdateMapList(List<string> mapList)
    {
        SearchBarText = string.Join("|", mapList);
        _lastSearchText = SearchBarText;
        _updateMapList = true;
    }

    /// <summary>
    /// Handles the display of the searchbar
    /// </summary>
    public void DisplaySearchbar()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (CFG.Current.MapEditor_MapObjectList_ShowMapIdSearch)
        {
            DPI.ApplyInputWidth(windowWidth * 0.75f);
            ImGui.InputText($"##mapListSearch_{ImguiID}", ref SearchBarText, 255);
            if(ImGui.IsItemEdited())
            {
                if (_lastSearchText != SearchBarText)
                {
                    _lastSearchText = SearchBarText;
                    _updateMapList = true;
                }
            }
            UIHelper.Tooltip("Filter the map list entries.");
        }

        if (_updateMapList)
        {
            _updateMapList = false;

            _cachedSearchMatches.Clear();
            _cachedMapNameAliases.Clear();
            _cachedMapTags.Clear();

            foreach (var entry in Project.MapData.MapFiles.Entries)
            {
                var mapID = entry.Filename;

                var nameAlias = AliasUtils.GetMapNameAlias(Editor.Project, mapID);
                var tags = AliasUtils.GetMapTags(Editor.Project, mapID);

                _cachedMapNameAliases[mapID] = nameAlias;
                _cachedMapTags[mapID] = tags;

                bool isMatch = SearchFilters.IsMapSearchMatch(_lastSearchText, mapID, nameAlias, tags);

                if (isMatch || _lastSearchText == "")
                {
                    _cachedSearchMatches.Add(mapID);
                }
            }
        }
    }

    /// <summary>
    /// Handles the chalice MSB toggle button
    /// </summary>
    private void DisplayChaliceToggleButton()
    {
        if (ImGui.Button($"{Icons.Adjust}", DPI.IconButtonSize))
        {
            DisplayChaliceDungeons = !DisplayChaliceDungeons;
        }
        UIHelper.Tooltip("Toggles the display of chalice dungeon maps within the map list.");
    }

    /// <summary>
    /// Handles the two types of map ID list
    /// </summary>
    private void DisplayMapList(MapContentLoadState loadType)
    {
        var filteredEntries = new List<MapContentView>();

        foreach (var entry in ContentViews)
        {
            if (!_cachedSearchMatches.Contains(entry.Value.MapID) && loadType == MapContentLoadState.Unloaded)
            {
                continue;
            }

            if (!DisplayChaliceDungeons && entry.Value.MapID.Contains("m29_"))
            {
                continue;
            }

            if (Editor.MapListFilterTool.CurrentFilter != null)
            {
                var matchType = Editor.MapListFilterTool.CurrentFilter.Type;

                var appliedFilters = Editor.MapListFilterTool.CurrentFilter.Entries;

                var curMapName = entry.Value.MapID;

                var add = true;

                if (matchType == FilterType.AND)
                {
                    foreach (var filter in appliedFilters)
                    {
                        var match = Regex.Match(curMapName, filter);
                        if (!match.Success)
                        {
                            add = false;
                        }
                    }
                }
                else if(matchType == FilterType.OR)
                {
                    add = false;

                    foreach (var filter in appliedFilters)
                    {
                        if (filter == "")
                            continue;

                        var match = Regex.Match(curMapName, filter);
                        if (match.Success)
                        {
                            add = true;
                        }
                    }
                }

                if (add)
                {
                    if (entry.Value.ContentLoadState == loadType)
                    {
                        filteredEntries.Add(entry.Value);
                    }
                }
            }
            else
            {
                if (entry.Value.ContentLoadState == loadType)
                {
                    filteredEntries.Add(entry.Value);
                }
            }
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredEntries.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var curView = filteredEntries[i];

                if (ImGui.Selectable($"##mapListEntry{curView.MapID}", curView.MapID == Editor.Selection.SelectedMapID, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (loadType == MapContentLoadState.Loaded)
                    {
                        Editor.Selection.SelectedMapID = curView.MapID;
                        Editor.Selection.SelectedMapView = curView;
                    }

                    if (CFG.Current.MapEditor_Enable_Map_Load_on_Double_Click && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (loadType == MapContentLoadState.Loaded)
                        {
                            curView.Unload();
                            Editor.Selection.SelectedMapID = "";
                            Editor.Selection.SelectedMapView = null;
                        }
                        else
                        {
                            curView.Load(true);
                            Editor.Selection.SelectedMapID = curView.MapID;
                            Editor.Selection.SelectedMapView = curView;
                        }
                    }
                }

                var mapId = curView.MapID;
                var displayedName = $"{mapId}";

                if (CFG.Current.MapEditor_MapObjectList_ShowMapNames)
                {
                    var mapName = _cachedMapNameAliases.TryGetValue(curView.MapID, out var cachedName) ? cachedName : "";
                    displayedName = $"{mapId}: {mapName}";
                }

                if (loadType == MapContentLoadState.Loaded)
                {
                    UIHelper.DisplayColoredAlias(displayedName, UI.Current.ImGui_AliasName_Text);
                }
                else
                {
                    UIHelper.DisplayColoredAlias(displayedName, UI.Current.ImGui_Default_Text_Color);
                }

                // Context Menu
                DisplayContextMenu(curView.MapID, curView);
            }
        }

        clipper.End();

        if (loadType == MapContentLoadState.Loaded)
            ImGui.Separator();
    }

    /// <summary>
    /// Handles the context menu for a map ID list entry
    /// </summary>
    private void DisplayContextMenu(string entry, MapContentView curView)
    {
        if (ImGui.BeginPopupContextItem($@"mapListEntryContext_{entry}"))
        {
            // Unloaded Map
            if (curView.ContentLoadState is MapContentLoadState.Unloaded)
            {
                // Load Map
                if (ImGui.Selectable("Load Map"))
                {
                    curView.Load(true);
                    Editor.Selection.SelectedMapID = curView.MapID;
                    Editor.Selection.SelectedMapView = curView;
                }
            }

            // Loaded Map
            if (curView.ContentLoadState is MapContentLoadState.Loaded)
            {
                var targetContainer = Editor.GetMapContainerFromMapID(curView.MapID);

                if (targetContainer != null)
                {
                    // Save Map
                    if (ImGui.Selectable("Save Map"))
                    {
                        try
                        {
                            Editor.Universe.SaveMap(targetContainer);
                        }
                        catch (SavingFailedException e)
                        {
                            Editor.HandleSaveException(e);
                        }
                    }

                    // Unload Map
                    if (ImGui.Selectable("Unload Map"))
                    {
                        curView.Unload();
                        Editor.Selection.SelectedMapID = "";
                        Editor.Selection.SelectedMapView = null;
                    }
                }
            }

            // ER: Load Related Maps
            if (Editor.Project.ProjectType is ProjectType.ER)
            {
                if (entry.StartsWith("m60") || entry.StartsWith("m61"))
                {
                    if (ImGui.Selectable("Load Related Maps"))
                    {
                        curView.Load(true);
                        Editor.Universe.LoadRelatedMapsER(entry);
                    }
                }
            }

            ImGui.Separator();

            // Utils
            if (ImGui.Selectable("Copy Map ID"))
            {
                PlatformUtils.Instance.SetClipboardText(entry);
            }
            if (ImGui.Selectable("Copy Map Name"))
            {
                var mapName = AliasUtils.GetMapNameAlias(Editor.Project, entry);
                PlatformUtils.Instance.SetClipboardText(mapName);
            }
            if (Editor.GlobalSearchTool.IsOpen)
            {
                if (ImGui.Selectable("Add to Map Filter"))
                {
                    Editor.GlobalSearchTool.AddMapFilterInput(entry);
                }
            }

            ImGui.EndPopup();
        }
    }

    public void OnActionEvent(Actions.Viewport.ActionEvent evt)
    {
        if (evt.HasFlag(Actions.Viewport.ActionEvent.ObjectAddedRemoved))
        {
            Editor.EntityTypeCache.InvalidateCache();
        }
    }

    public void TriggerMapLoad(string mapId)
    {
        if(ContentViews.ContainsKey(mapId))
        {
            var curView = ContentViews[mapId];

            curView.Load(true);
        }
    }

    public void TriggerMapUnload(string mapId)
    {
        if (ContentViews.ContainsKey(mapId))
        {
            var curView = ContentViews[mapId];

            curView.Unload();
        }
    }
}
