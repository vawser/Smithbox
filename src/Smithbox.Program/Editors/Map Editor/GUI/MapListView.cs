using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

public class MapListView : IActionEventHandler
{
    public MapEditorView View;
    public ProjectEntry Project;

    public string SearchBarText = "";

    private bool DisplayChaliceDungeons = true;
    private bool DisplayAssetMsbs = true;

    public MapListView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        GUI.TitleHeader("Map List", "");

        DisplaySearchbar();

        DisplayLoadStatus();

        UpdateListSelection();

        ImGui.BeginChild("MapListSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayMapList(MapContentLoadState.Loaded);
        DisplayMapList(MapContentLoadState.Unloaded);

        ImGui.EndChild();
    }

    private HashSet<string> _cachedSearchMatches = new HashSet<string>();
    private Dictionary<string, string> _cachedMapNameAliases = new Dictionary<string, string>();
    private bool _updateMapList = true;

    private string FileListFilter = "";
    private bool ExactFileListFilter = false;
    private bool _lastExactFileListFilter = false;

    // The World Map pre-filter, as a set for the per-map lookup
    private HashSet<string> FilteredMapList = new();

    // The filtered entries of each list, rebuilt only when one of their inputs changes rather than every frame
    private readonly Dictionary<MapContentLoadState, List<MapWrapper>> _filteredEntries = new();
    private readonly Dictionary<MapContentLoadState, int> _filteredEntriesKey = new();
    private int _listVersion;

    public void ClearMapList()
    {
        FilteredMapList.Clear();
        _listVersion++;
    }

    public void UpdateMapList(List<string> mapList)
    {
        FilteredMapList = new HashSet<string>(mapList ?? new List<string>());
        _listVersion++;
    }

    /// <summary>
    /// Handles the display of the searchbar
    /// </summary>
    public void DisplaySearchbar()
    {
        ImGui.BeginChild($"framedListFilter_mapEditor_ListView", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("mapEditor_ListView", ref FileListFilter, ref ExactFileListFilter);
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            _updateMapList = true;
        }

        // The exact-match checkbox is drawn before the text input, so its change is caught by value
        if (ExactFileListFilter != _lastExactFileListFilter)
        {
            _lastExactFileListFilter = ExactFileListFilter;
            _updateMapList = true;
        }
        GUI.Tooltip("Filter the map list entries.");

        ImGui.SameLine();
        View.MapListFilterTool.DisplayHeaderButton();

        ImGui.SameLine();
        if (ImGui.Button($"{(View.ShowMapData ? Icons.Cube : Icons.Table)}##mapDataToggle", DPI.IconButtonSize))
        {
            View.ToggleMapData();
        }
        GUI.Tooltip(View.ShowMapData
            ? "Hide the Map Data window and show the viewport."
            : "Hide the viewport and show the Map Data window, which lists the MSB data of the selected map.");

        if (View.Project.Descriptor.ProjectType is ProjectType.BB)
        {
            ImGui.SameLine();
            DisplayChaliceToggleButton();
        }

        if (View.Project.Descriptor.ProjectType is ProjectType.DB or ProjectType.DB_NT)
        {
            ImGui.SameLine();
            DisplayAssetToggleButton();
        }

        if (View.Project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.NR)
        {
            ImGui.SameLine();
            if (ImGui.Button($"{Icons.Map}", DPI.IconButtonSize))
            {
                View.WorldMapTool.ToggleWindow();
            }
            GUI.Tooltip(LOC.Get("MAP_MapInfo_Open_TT", InputManager.GetHint(KeybindID.MapEditor_Toggle_World_Map_Menu)));

            // Tile outlines in the viewport
            View.TileOutlineTool.DisplayHeaderButton();

            if(FilteredMapList.Count > 0)
            {
                ImGui.SameLine();
                if (ImGui.Button($"{Icons.MapMarker}", DPI.IconButtonSize))
                {
                    ClearMapList();
                }
                GUI.Tooltip($"Clear the pre-filtered map list set by a World Map click.");
            }
        }

        if (_updateMapList)
        {
            _updateMapList = false;
            _listVersion++;

            _cachedSearchMatches.Clear();
            _cachedMapNameAliases.Clear();

            foreach (var entry in Project.Locator.MapFiles.Entries)
            {
                var mapID = entry.Filename;

                var nameAlias = AliasHelper.GetMapNameAlias(View.Project, mapID);

                _cachedMapNameAliases[mapID] = nameAlias;

                var isMatch = EditorFilters.IsMatch(FileListFilter, mapID, ExactFileListFilter, nameAlias, true, true);

                if (isMatch)
                {
                    _cachedSearchMatches.Add(mapID);
                }
            }
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Handles the chalice MSB toggle button
    /// </summary>
    private void DisplayChaliceToggleButton()
    {
        if (ImGui.Button($"{Icons.Adjust}", DPI.IconButtonSize))
        {
            DisplayChaliceDungeons = !DisplayChaliceDungeons;
            _listVersion++;
        }
        GUI.Tooltip("Toggles the display of chalice dungeon maps within the map list.");
    }

    private void DisplayAssetToggleButton()
    {
        if (ImGui.Button($"{Icons.Adjust}", DPI.IconButtonSize))
        {
            DisplayAssetMsbs = !DisplayAssetMsbs;
            _listVersion++;
        }
        GUI.Tooltip("Toggles the display of asset MSBs within the map list.");
    }

    /// <summary>
    /// Handles the two types of map ID list
    /// </summary>
    private void DisplayMapList(MapContentLoadState loadType)
    {
        var filteredEntries = GetFilteredEntries(loadType);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredEntries.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var curWrapper = filteredEntries[i];

                if (ImGui.Selectable($"##mapListEntry{curWrapper.Name}", _selectedMaps.Contains(curWrapper.Name), ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        // The first click of the pair has already set the selection (and left a multi-selection
                        // standing), so the double-click acts on all of it
                        _pendingCollapse = null;
                        _selectedMaps.Add(curWrapper.Name);

                        if (CFG.Current.MapEditor_Map_List_Enable_Load_on_Double_Click)
                        {
                            OnDoubleClick(curWrapper, loadType);
                        }
                    }
                    else
                    {
                        OnClick(curWrapper, loadType);
                    }
                }

                var mapId = curWrapper.Name;
                var displayedName = $"{mapId}";

                if (CFG.Current.MapEditor_Map_List_Display_Map_Aliases)
                {
                    var mapName = _cachedMapNameAliases.TryGetValue(mapId, out var cachedName) ? cachedName : "";
                    displayedName = $"{mapId}: {mapName}";
                }

                // Mark maps with unsaved changes
                if (curWrapper.MapContainer != null && curWrapper.MapContainer.HasUnsavedChanges)
                {
                    displayedName = $"{displayedName}*";
                }

                // Mark maps that are loading, or waiting for their turn to load
                var loadMarker = GetLoadMarker(mapId);
                if (loadMarker != null)
                {
                    displayedName = $"{displayedName} {loadMarker}";
                }

                var saveError = curWrapper.MapContainer?.SaveError;

                if (saveError != null)
                {
                    // A failed save (an autosave, which shows no dialog) stays marked until the map saves
                    GUI.DisplayColoredAlias(displayedName, UI.Current.ImGui_Invalid_Text_Color, CFG.Current.Interface_Alias_Wordwrap_Map_Editor);
                    GUI.Tooltip(LOC.Get("MAP_List_Save_Failed_TT", saveError));
                }
                else if (loadType == MapContentLoadState.Loaded)
                {
                    GUI.DisplayColoredAlias(displayedName, UI.Current.ImGui_AliasName_Text, CFG.Current.Interface_Alias_Wordwrap_Map_Editor);
                }
                else
                {
                    GUI.DisplayColoredAlias(displayedName, UI.Current.ImGui_Default_Text_Color, CFG.Current.Interface_Alias_Wordwrap_Map_Editor);
                }

                // Context Menu
                DisplayContextMenu(curWrapper);
            }
        }

        clipper.End();

        if (loadType == MapContentLoadState.Loaded)
            ImGui.Separator();
    }

    // The maps selected in the list, by ID. The active map (View.Selection.SelectedMapID) is separate: it is one
    // loaded map, and follows the last loaded map clicked here.
    private readonly HashSet<string> _selectedMaps = new();

    // Where a Shift+click range starts
    private string _selectionAnchor;

    // The active map last seen, so a change made elsewhere (a viewport click, a load) can be followed
    private string _lastActiveMap;

    // A plain click on one map of a multi-selection collapses the selection to it only once the double-click time
    // has passed, so that a double-click acts on the whole selection
    private string _pendingCollapse;
    private double _pendingCollapseTime;

    /// <summary>
    /// Runs once a frame, before the lists are drawn
    /// </summary>
    private void UpdateListSelection()
    {
        if (_pendingCollapse != null &&
            ImGui.GetTime() - _pendingCollapseTime > ImGui.GetIO().MouseDoubleClickTime)
        {
            _selectedMaps.Clear();
            _selectedMaps.Add(_pendingCollapse);
            _pendingCollapse = null;
        }

        // The active map was changed elsewhere: the selection follows it, unless it is already part of it
        // (as when one of several selected maps finishes loading)
        var active = View.Selection.SelectedMapID;

        if (active != _lastActiveMap)
        {
            _lastActiveMap = active;

            if (!string.IsNullOrEmpty(active) && !_selectedMaps.Contains(active))
            {
                _selectedMaps.Clear();
                _selectedMaps.Add(active);
                _selectionAnchor = active;
                _pendingCollapse = null;
            }
        }
    }

    /// <summary>
    /// Click: select only this map. Ctrl+click: add or remove it. Shift+click: select the range from the last
    /// clicked map (added to the selection with Ctrl held).
    /// </summary>
    private void OnClick(MapWrapper wrapper, MapContentLoadState loadType)
    {
        var io = ImGui.GetIO();
        var name = wrapper.Name;

        _pendingCollapse = null;

        if (io.KeyShift && _selectionAnchor != null && SelectRange(_selectionAnchor, name, io.KeyCtrl))
        {
            // The anchor stays, so the range can be changed with another Shift+click
        }
        else if (io.KeyCtrl)
        {
            if (!_selectedMaps.Remove(name))
                _selectedMaps.Add(name);

            _selectionAnchor = name;
        }
        else
        {
            if (_selectedMaps.Count > 1 && _selectedMaps.Contains(name))
            {
                _pendingCollapse = name;
                _pendingCollapseTime = ImGui.GetTime();
            }
            else
            {
                _selectedMaps.Clear();
                _selectedMaps.Add(name);
            }

            _selectionAnchor = name;
        }

        // A loaded map clicked becomes the active map, as before (not when Ctrl+click took it out of the selection)
        if (loadType == MapContentLoadState.Loaded && _selectedMaps.Contains(name))
        {
            View.Selection.SelectedMapID = name;
            View.Selection.SelectedMapContainer = wrapper.MapContainer;
            _lastActiveMap = name;
        }
    }

    /// <summary>
    /// Selects the maps from one to the other, in the order shown (loaded maps, then unloaded maps). False if
    /// either is no longer in the list.
    /// </summary>
    private bool SelectRange(string from, string to, bool add)
    {
        var visible = GetVisibleMaps();

        var start = visible.FindIndex(e => e.Name == from);
        var end = visible.FindIndex(e => e.Name == to);

        if (start < 0 || end < 0)
            return false;

        if (!add)
            _selectedMaps.Clear();

        for (int i = Math.Min(start, end); i <= Math.Max(start, end); i++)
        {
            _selectedMaps.Add(visible[i].Name);
        }

        return true;
    }

    /// <summary>
    /// Every unloaded selected map is loaded, the clicked one made active once loaded (or the first, if the
    /// clicked map is already loaded, in which case it is framed). Unloading a loaded map drops its collision,
    /// navmesh and models, which are slow to load again, so that is left to the context menu.
    /// </summary>
    private void OnDoubleClick(MapWrapper wrapper, MapContentLoadState loadType)
    {
        LoadMaps(GetSelectedMaps(wrapper), wrapper.Name);

        if (loadType == MapContentLoadState.Loaded && wrapper.MapContainer != null)
        {
            View.ViewportWindow.FrameMap(wrapper.MapContainer);
        }
    }

    /// <summary>
    /// The maps shown, in the order shown
    /// </summary>
    private List<MapWrapper> GetVisibleMaps()
    {
        var visible = new List<MapWrapper>(GetFilteredEntries(MapContentLoadState.Loaded));
        visible.AddRange(GetFilteredEntries(MapContentLoadState.Unloaded));
        return visible;
    }

    /// <summary>
    /// The selected maps the list shows (a map hidden by the search or a filter is left out, so an action never
    /// reaches a map the user can't see), in the order shown. Always includes the clicked map.
    /// </summary>
    private List<MapWrapper> GetSelectedMaps(MapWrapper clicked)
    {
        var selected = GetVisibleMaps().Where(e => _selectedMaps.Contains(e.Name)).ToList();

        if (!selected.Contains(clicked))
            selected.Insert(0, clicked);

        return selected;
    }

    private static bool IsLoaded(MapWrapper wrapper)
    {
        return wrapper.MapContainer != null && wrapper.MapContainer.LoadState is MapContentLoadState.Loaded;
    }

    /// <summary>
    /// Loads the unloaded maps. One is made the active map once loaded: the preferred map if it is among them,
    /// otherwise the first. That one starts first; the rest queue behind it.
    /// </summary>
    private void LoadMaps(List<MapWrapper> maps, string preferred)
    {
        var toLoad = maps.Where(e => !IsLoaded(e)).Select(e => e.Name).ToList();

        if (toLoad.Count == 0)
            return;

        var selectTarget = toLoad.Contains(preferred) ? preferred : toLoad[0];

        View.Universe.LoadMap(selectTarget, true);

        foreach (var mapID in toLoad)
        {
            if (mapID != selectTarget)
                View.Universe.LoadMap(mapID, false);
        }
    }

    /// <summary>
    /// "Loading m10_00_00_00 (2 queued)" while this view is loading maps
    /// </summary>
    private void DisplayLoadStatus()
    {
        var loading = View.Universe.LoadingMaps;
        var queued = View.Universe.QueuedLoadCount;

        if (loading.Count == 0 && queued == 0)
            return;

        // Several maps (Load Related Maps, or loads queued behind each other): one line with the count, and the
        // names on hover, rather than a list of every map
        var (done, total) = View.Universe.LoadProgress;

        if (total > 1)
        {
            var fraction = Math.Clamp(done / (float)total, 0f, 1f);

            ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("MAP_List_Loading_Batch", total, done));

            if (ImGui.IsItemHovered())
            {
                var loadingNames = loading.Count > 0 ? string.Join(", ", loading.OrderBy(e => e)) : "-";

                GUI.Tooltip(LOC.Get("MAP_List_Loading_Batch_TT", loadingNames, queued));
            }

            return;
        }

        var names = loading.Count > 0 ? string.Join(", ", loading.OrderBy(e => e)) : "";
        var text = queued > 0
            ? LOC.Get("MAP_List_Loading_Status_Queued", names, queued)
            : LOC.Get("MAP_List_Loading_Status", names);

        ImGui.TextColored(UI.Current.ImGui_AliasName_Text, text);
    }

    /// <summary>
    /// "(Loading…)" or "(Queued)" for a map being loaded by, or waiting in the load queue of, any view
    /// </summary>
    private string GetLoadMarker(string mapID)
    {
        foreach (var view in View.Editor.ViewHandler.Views)
        {
            if (view == null)
                continue;

            if (view.Universe.IsMapLoading(mapID))
                return LOC.Get("MAP_List_Loading_Marker");

            if (view.Universe.IsMapQueued(mapID))
                return LOC.Get("MAP_List_Queued_Marker");
        }

        return null;
    }

    /// <summary>
    /// The entries of the list, rebuilt only when the search, the filters, the toggles or the set of loaded maps
    /// change. Running every filter pattern over every map was otherwise done twice a frame.
    /// </summary>
    private List<MapWrapper> GetFilteredEntries(MapContentLoadState loadType)
    {
        var key = HashCode.Combine(_listVersion, View.MapListFilterTool.FilterVersion, GetLoadedMapsSignature());

        if (_filteredEntries.TryGetValue(loadType, out var cached) &&
            _filteredEntriesKey.TryGetValue(loadType, out var cachedKey) && cachedKey == key)
        {
            return cached;
        }

        var entries = BuildFilteredEntries(loadType);

        _filteredEntries[loadType] = entries;
        _filteredEntriesKey[loadType] = key;

        return entries;
    }

    /// <summary>
    /// Changes whenever a map is loaded or unloaded
    /// </summary>
    private int GetLoadedMapsSignature()
    {
        var hash = 0;
        var count = 0;

        foreach (var wrapper in Project.Handler.MapData.PrimaryBank.Maps.Values)
        {
            if (wrapper.MapContainer != null && wrapper.MapContainer.LoadState is MapContentLoadState.Loaded)
            {
                // Order-independent, so the dictionary's order doesn't matter
                hash ^= wrapper.Name.GetHashCode();
                count++;
            }
        }

        return HashCode.Combine(hash, count);
    }

    private List<MapWrapper> BuildFilteredEntries(MapContentLoadState loadType)
    {
        var filteredEntries = new List<MapWrapper>();

        // NOTE: this currently displays loaded maps across multiple views, perhaps it should be view-specific
        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            var wrapper = entry.Value;

            if(FilteredMapList.Count > 0)
            {
                if (!FilteredMapList.Contains(wrapper.Name))
                    continue;
            }

            if (!_cachedSearchMatches.Contains(wrapper.Name) && loadType == MapContentLoadState.Unloaded)
            {
                continue;
            }

            if (!DisplayChaliceDungeons && wrapper.Name.Contains("m29_"))
            {
                continue;
            }

            if (!DisplayAssetMsbs && wrapper.Name.Contains("aep"))
            {
                continue;
            }

            if (View.MapListFilterTool.CurrentFilter != null)
            {
                var matchType = View.MapListFilterTool.CurrentFilter.Type;

                var appliedFilters = View.MapListFilterTool.CurrentFilter.Entries;

                var curMapName = wrapper.Name;

                var add = true;

                if (matchType == FilterType.AND)
                {
                    foreach (var filter in appliedFilters)
                    {
                        if (!IsFilterMatch(filter, curMapName))
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

                        if (IsFilterMatch(filter, curMapName))
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

        return filteredEntries;
    }

    // The map list filter patterns, compiled once each; null for a pattern that is not a valid regex
    private readonly Dictionary<string, Regex> _filterRegexCache = new();

    /// <summary>
    /// Whether the map ID matches a map list filter pattern. The patterns are user-written and run for every
    /// map every frame, so an invalid one is reported once and treated as matching nothing, rather than
    /// throwing each frame.
    /// </summary>
    private bool IsFilterMatch(string pattern, string mapName)
    {
        if (pattern == null)
            return false;

        if (!_filterRegexCache.TryGetValue(pattern, out var regex))
        {
            try
            {
                regex = new Regex(pattern);
            }
            catch (ArgumentException e)
            {
                regex = null;
                Smithbox.LogError<MapListView>($"The map list filter pattern \"{pattern}\" is not a valid regular expression: {e.Message}");
            }

            _filterRegexCache[pattern] = regex;
        }

        return regex != null && regex.IsMatch(mapName);
    }

    /// <summary>
    /// Handles the context menu for a map ID list entry
    /// </summary>
    private void DisplayContextMenu(MapWrapper mapWrapper)
    {
        if (ImGui.BeginPopupContextItem($@"mapListEntryContext_{mapWrapper.Name}"))
        {
            // Right-clicking a map outside the selection selects just it, so the menu never acts on maps the
            // user didn't mean
            if (ImGui.IsWindowAppearing() && !_selectedMaps.Contains(mapWrapper.Name))
            {
                _selectedMaps.Clear();
                _selectedMaps.Add(mapWrapper.Name);
                _selectionAnchor = mapWrapper.Name;
                _pendingCollapse = null;
            }

            var targets = GetSelectedMaps(mapWrapper);
            var unloaded = targets.Where(e => !IsLoaded(e)).ToList();
            var loaded = targets.Where(IsLoaded).ToList();

            // Unloaded maps
            if (unloaded.Count > 0)
            {
                // Load Map
                if (ImGui.Selectable(unloaded.Count == 1 ? "Load Map" : LOC.Get("MAP_List_Load_Maps", unloaded.Count)))
                {
                    LoadMaps(unloaded, mapWrapper.Name);
                }
            }

            // Loaded maps
            if (loaded.Count > 0)
            {
                // Save Map
                if (ImGui.Selectable(loaded.Count == 1 ? "Save Map" : LOC.Get("MAP_List_Save_Maps", loaded.Count)))
                {
                    foreach (var map in loaded)
                    {
                        try
                        {
                            View.Universe.SaveMap(map.MapContainer);
                        }
                        catch (SavingFailedException e)
                        {
                            // The save error dialog is shown once, and the maps after it are left unsaved
                            View.Editor.HandleSaveException(e);
                            break;
                        }
                    }
                }

                // Unload Map
                if (ImGui.Selectable(loaded.Count == 1 ? "Unload Map" : LOC.Get("MAP_List_Unload_Maps", loaded.Count)))
                {
                    View.Universe.RequestUnloadMaps(loaded.Select(e => e.Name).ToList());
                }

                // How much undo history goes with them: the history is held by the view that loaded each map.
                // Clicking Unload above unloads them in this same frame, leaving no container, so skip those.
                if (ImGui.IsItemHovered())
                {
                    var dropped = 0;

                    foreach (var map in loaded)
                    {
                        if (map.MapContainer is { } hoveredMap)
                        {
                            var manager = (hoveredMap.OwnerView ?? View).ViewportActionManager;
                            dropped += manager.CountActionsTouching(map.Name);
                        }
                    }

                    GUI.Tooltip(dropped > 0
                        ? LOC.Get("MAP_History_Unload_Map_TT", dropped)
                        : LOC.Get("MAP_History_Unload_Map_None_TT"));
                }
            }

            // Paste into the right-clicked map, when something is copied (and the map is still loaded after the
            // items above). It targets one map, so it stays with the map clicked.
            if (IsLoaded(mapWrapper))
            {
                View.DuplicateToMapAction.OnPasteIntoMapContext(mapWrapper.MapContainer);
            }

            // ER: Load Related Maps
            if (View.Project.Descriptor.ProjectType is ProjectType.ER)
            {
                var overworld = targets.Where(e => e.Name.StartsWith("m60") || e.Name.StartsWith("m61")).ToList();

                if (overworld.Count > 0)
                {
                    if (ImGui.Selectable("Load Related Maps"))
                    {
                        LoadMaps(overworld, mapWrapper.Name);

                        foreach (var map in overworld)
                        {
                            View.Universe.LoadRelatedMapsER(map.Name);
                        }
                    }
                }
            }

            // Tile maps (ER, NR): Load Adjacent
            if (View.TileOutlineTool.IsSupported)
            {
                DisplayLoadAdjacentMenu(targets);
            }

            ImGui.Separator();

            // Utils: with several maps, one per line
            if (ImGui.Selectable(targets.Count == 1 ? "Copy Map ID" : LOC.Get("MAP_List_Copy_Map_IDs", targets.Count)))
            {
                PlatformUtils.Instance.SetClipboardText(string.Join("\n", targets.Select(e => e.Name)));
            }
            if (ImGui.Selectable(targets.Count == 1 ? "Copy Map Name" : LOC.Get("MAP_List_Copy_Map_Names", targets.Count)))
            {
                var mapNames = targets.Select(e => AliasHelper.GetMapNameAlias(View.Project, e.Name));
                PlatformUtils.Instance.SetClipboardText(string.Join("\n", mapNames));
            }
            if (ImGui.Selectable("Add to Map Filter"))
            {
                foreach (var map in targets)
                {
                    View.GlobalSearchTool.TargetMap(map.Name);
                }
            }

            ImGui.EndPopup();
        }
    }

    // The eight neighbours of a tile, as (localization key, X step, Z step). +X is east and +Z is north: the tile
    // grid indices in the map ID grow that way, as MapConnections_ER/NR place them.
    private static readonly (string Key, int DX, int DZ)[] AdjacentDirections =
    {
        ("MAP_List_Load_Adjacent_North", 0, 1),
        ("MAP_List_Load_Adjacent_North_East", 1, 1),
        ("MAP_List_Load_Adjacent_East", 1, 0),
        ("MAP_List_Load_Adjacent_South_East", 1, -1),
        ("MAP_List_Load_Adjacent_South", 0, -1),
        ("MAP_List_Load_Adjacent_South_West", -1, -1),
        ("MAP_List_Load_Adjacent_West", -1, 0),
        ("MAP_List_Load_Adjacent_North_West", -1, 1),
    };

    /// <summary>
    /// Load Adjacent: the neighbouring tiles of the same size and variant, in one direction or all eight, for every
    /// selected tile map. A direction is disabled when none of its tiles exist or all are loaded. The active map is
    /// left as it is.
    /// </summary>
    private void DisplayLoadAdjacentMenu(List<MapWrapper> targets)
    {
        var tiles = targets.Where(e => MapTileOutlineTool.GetTileScale(e.Name) >= 0).ToList();

        if (tiles.Count == 0)
            return;

        if (!ImGui.BeginMenu(LOC.Get("MAP_List_Load_Adjacent")))
            return;

        var all = new List<string>();

        foreach (var (key, dx, dz) in AdjacentDirections)
        {
            var toLoad = GetAdjacentToLoad(tiles, dx, dz);
            all.AddRange(toLoad);

            if (ImGui.MenuItem(LOC.Get(key), "", false, toLoad.Count > 0))
            {
                LoadAdjacent(toLoad);
            }
            GUI.Tooltip(toLoad.Count > 0
                ? LOC.Get("MAP_List_Load_Adjacent_TT", string.Join("\n", toLoad))
                : LOC.Get("MAP_List_Load_Adjacent_None_TT"));
        }

        ImGui.Separator();

        all = all.Distinct().ToList();

        if (ImGui.MenuItem(LOC.Get("MAP_List_Load_Adjacent_All"), "", false, all.Count > 0))
        {
            LoadAdjacent(all);
        }
        GUI.Tooltip(all.Count > 0
            ? LOC.Get("MAP_List_Load_Adjacent_TT", string.Join("\n", all))
            : LOC.Get("MAP_List_Load_Adjacent_None_TT"));

        ImGui.EndMenu();
    }

    /// <summary>
    /// The neighbours one step along (dx, dz) of each tile that exist in the project and aren't loaded or loading
    /// </summary>
    private List<string> GetAdjacentToLoad(List<MapWrapper> tiles, int dx, int dz)
    {
        var result = new List<string>();

        foreach (var tile in tiles)
        {
            if (!MapConnectionsUtil.TryParseMap(tile.Name, out var parts))
                continue;

            var x = parts[1] + dx;
            var z = parts[2] + dz;

            if (x <= 0 || z <= 0 || x > 99 || z > 99)
                continue;

            var neighbour = MapConnectionsUtil.FormatMap(new[] { parts[0], (byte)x, (byte)z, parts[3] });

            if (result.Contains(neighbour))
                continue;

            var wrapper = Project.Handler.MapData.PrimaryBank.Maps.Values.FirstOrDefault(e => e.Name == neighbour);

            if (wrapper == null || IsLoaded(wrapper) || View.Universe.IsMapLoading(neighbour) || View.Universe.IsMapQueued(neighbour))
                continue;

            result.Add(neighbour);
        }

        return result;
    }

    private void LoadAdjacent(List<string> mapIDs)
    {
        foreach (var mapID in mapIDs)
        {
            View.Universe.LoadMap(mapID, false);
        }
    }

    public void OnActionEvent(ActionEvent evt)
    {
        if (evt.HasFlag(ActionEvent.ObjectAddedRemoved))
        {
            View.EntityTypeCache.InvalidateCache();
            View.MapDataView.EntryView.Invalidate();
        }
    }
}
