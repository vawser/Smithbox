using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

public class MapListView : IActionEventHandler
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    private string ImguiID = "MapListView";

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
                Editor.FocusManager.SwitchMapEditorContext(MapEditorContext.MapIdList);

                DisplayMenubar();
                DisplaySearchbar();

                if (Editor.Project.ProjectType is ProjectType.BB)
                {
                    ImGui.SameLine();
                    DisplayChaliceToggleButton();
                }

                ImGui.BeginChild($"mapListSection");
                DisplayMapList(MapContentLoadState.Loaded);
                DisplayMapList(MapContentLoadState.Unloaded);
                ImGui.EndChild();
            }

            ImGui.End();
            ImGui.PopStyleColor();
        }

        Editor.MapListFilterTool.Update();

    }

    public void DisplayMenubar()
    {
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
                        Editor.Universe.UnloadMap(Editor.Selection.SelectedMapID);
                    }
                }
                UIHelper.Tooltip("Unload the currently loaded and selected map.");

                if (ImGui.MenuItem("Unload All"))
                {
                    DialogResult result = PlatformUtils.Instance.MessageBox("Unload all maps?", "Confirm", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        Editor.Universe.UnloadAllMaps();
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

                var nameAlias = AliasHelper.GetMapNameAlias(Editor.Project, mapID);
                var tags = AliasHelper.GetMapTags(Editor.Project, mapID);

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
        var filteredEntries = new List<MapWrapper>();

        foreach (var entry in Project.MapData.PrimaryBank.Maps)
        {
            var wrapper = entry.Value;

            if (!_cachedSearchMatches.Contains(wrapper.Name) && loadType == MapContentLoadState.Unloaded)
            {
                continue;
            }

            if (!DisplayChaliceDungeons && wrapper.Name.Contains("m29_"))
            {
                continue;
            }

            if (Editor.MapListFilterTool.CurrentFilter != null)
            {
                var matchType = Editor.MapListFilterTool.CurrentFilter.Type;

                var appliedFilters = Editor.MapListFilterTool.CurrentFilter.Entries;

                var curMapName = wrapper.Name;

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
                    if (loadType is MapContentLoadState.Loaded)
                    {
                        if (wrapper.MapContainer != null && 
                            wrapper.MapContainer.LoadState is MapContentLoadState.Loaded)
                        {
                            filteredEntries.Add(entry.Value);
                        }
                    }
                    else if (loadType is MapContentLoadState.Unloaded)
                    {
                        if (wrapper.MapContainer == null || 
                            wrapper.MapContainer.LoadState is MapContentLoadState.Unloaded)
                        {
                            filteredEntries.Add(entry.Value);
                        }
                    }
                }
            }
            else
            {
                if (loadType is MapContentLoadState.Loaded)
                {
                    if (wrapper.MapContainer != null &&
                        wrapper.MapContainer.LoadState is MapContentLoadState.Loaded)
                    {
                        filteredEntries.Add(entry.Value);
                    }
                }
                else if (loadType is MapContentLoadState.Unloaded)
                {
                    if (wrapper.MapContainer == null ||
                        wrapper.MapContainer.LoadState is MapContentLoadState.Unloaded)
                    {
                        filteredEntries.Add(entry.Value);
                    }
                }
            }
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredEntries.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var curWrapper = filteredEntries[i];

                if (ImGui.Selectable($"##mapListEntry{curWrapper.Name}", curWrapper.Name == Editor.Selection.SelectedMapID, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (loadType == MapContentLoadState.Loaded)
                    {
                        Editor.Selection.SelectedMapID = curWrapper.Name;
                        Editor.Selection.SelectedMapContainer = curWrapper.MapContainer;
                    }

                    if (CFG.Current.MapEditor_Enable_Map_Load_on_Double_Click && 
                        ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (loadType == MapContentLoadState.Loaded)
                        {
                            Editor.Universe.UnloadMap(curWrapper.Name);
                        }
                        else
                        {
                            Editor.Universe.LoadMap(curWrapper.Name, true);
                        }
                    }
                }

                var mapId = curWrapper.Name;
                var displayedName = $"{mapId}";

                if (CFG.Current.MapEditor_MapObjectList_ShowMapNames)
                {
                    var mapName = _cachedMapNameAliases.TryGetValue(mapId, out var cachedName) ? cachedName : "";
                    displayedName = $"{mapId}: {mapName}";
                }

                if (loadType == MapContentLoadState.Loaded)
                {
                    UIHelper.DisplayColoredAlias(displayedName, UI.Current.ImGui_AliasName_Text, CFG.Current.Interface_MapEditor_WrapAliasDisplay);
                }
                else
                {
                    UIHelper.DisplayColoredAlias(displayedName, UI.Current.ImGui_Default_Text_Color, CFG.Current.Interface_MapEditor_WrapAliasDisplay);
                }

                // Context Menu
                DisplayContextMenu(curWrapper);
            }
        }

        clipper.End();

        if (loadType == MapContentLoadState.Loaded)
            ImGui.Separator();
    }

    /// <summary>
    /// Handles the context menu for a map ID list entry
    /// </summary>
    private void DisplayContextMenu(MapWrapper mapWrapper)
    {
        if (ImGui.BeginPopupContextItem($@"mapListEntryContext_{mapWrapper.Name}"))
        {
            // Unloaded Map
            if (mapWrapper.MapContainer == null ||
                mapWrapper.MapContainer.LoadState is MapContentLoadState.Unloaded)
            {
                // Load Map
                if (ImGui.Selectable("Load Map"))
                {
                    Editor.Universe.LoadMap(mapWrapper.Name, true);
                }
            }

            // Loaded Map
            if (mapWrapper.MapContainer != null &&
                mapWrapper.MapContainer.LoadState is MapContentLoadState.Loaded)
            {
                // Save Map
                if (ImGui.Selectable("Save Map"))
                {
                    try
                    {
                        Editor.Universe.SaveMap(mapWrapper.MapContainer);
                    }
                    catch (SavingFailedException e)
                    {
                        Editor.HandleSaveException(e);
                    }
                }

                // Unload Map
                if (ImGui.Selectable("Unload Map"))
                {
                    Editor.Universe.UnloadMap(mapWrapper.Name);
                }
            }

            // ER: Load Related Maps
            if (Editor.Project.ProjectType is ProjectType.ER)
            {
                if (mapWrapper.Name.StartsWith("m60") || mapWrapper.Name.StartsWith("m61"))
                {
                    if (ImGui.Selectable("Load Related Maps"))
                    {
                        Editor.Universe.LoadMap(mapWrapper.Name, true);
                        Editor.Universe.LoadRelatedMapsER(mapWrapper.Name);
                    }
                }
            }

            ImGui.Separator();

            // Utils
            if (ImGui.Selectable("Copy Map ID"))
            {
                PlatformUtils.Instance.SetClipboardText(mapWrapper.Name);
            }
            if (ImGui.Selectable("Copy Map Name"))
            {
                var mapName = AliasHelper.GetMapNameAlias(Editor.Project, mapWrapper.Name);
                PlatformUtils.Instance.SetClipboardText(mapName);
            }
            if (Editor.GlobalSearchTool.IsOpen)
            {
                if (ImGui.Selectable("Add to Map Filter"))
                {
                    Editor.GlobalSearchTool.AddMapFilterInput(mapWrapper.Name);
                }
            }

            ImGui.EndPopup();
        }
    }

    public void OnActionEvent(ActionEvent evt)
    {
        if (evt.HasFlag(ActionEvent.ObjectAddedRemoved))
        {
            Editor.EntityTypeCache.InvalidateCache();
        }
    }
}
