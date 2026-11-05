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
                View.WorldMapTool.DisplayMenuOption();
            }
            GUI.Tooltip($"Open a world map with a visual representation of the map tiles.\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Toggle_World_Map_Menu)}");

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

                if (ImGui.Selectable($"##mapListEntry{curWrapper.Name}", curWrapper.Name == View.Selection.SelectedMapID, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (loadType == MapContentLoadState.Loaded)
                    {
                        View.Selection.SelectedMapID = curWrapper.Name;
                        View.Selection.SelectedMapContainer = curWrapper.MapContainer;
                    }

                    if (CFG.Current.MapEditor_Map_List_Enable_Load_on_Double_Click &&
                        ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (loadType == MapContentLoadState.Loaded)
                        {
                            View.Universe.RequestUnloadMap(curWrapper.Name);
                        }
                        else
                        {
                            View.Universe.LoadMap(curWrapper.Name, true);
                        }
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

    /// <summary>
    /// "Loading m10_00_00_00 (2 queued)" while this view is loading maps
    /// </summary>
    private void DisplayLoadStatus()
    {
        var loading = View.Universe.LoadingMaps;
        var queued = View.Universe.QueuedLoadCount;

        if (loading.Count == 0 && queued == 0)
            return;

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
            // Unloaded Map
            if (mapWrapper.MapContainer == null ||
                mapWrapper.MapContainer.LoadState is MapContentLoadState.Unloaded)
            {
                // Load Map
                if (ImGui.Selectable("Load Map"))
                {
                    View.Universe.LoadMap(mapWrapper.Name, true);
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
                        View.Universe.SaveMap(mapWrapper.MapContainer);
                    }
                    catch (SavingFailedException e)
                    {
                        View.Editor.HandleSaveException(e);
                    }
                }

                // Unload Map
                if (ImGui.Selectable("Unload Map"))
                {
                    View.Universe.RequestUnloadMap(mapWrapper.Name);
                }
            }

            // ER: Load Related Maps
            if (View.Project.Descriptor.ProjectType is ProjectType.ER)
            {
                if (mapWrapper.Name.StartsWith("m60") || mapWrapper.Name.StartsWith("m61"))
                {
                    if (ImGui.Selectable("Load Related Maps"))
                    {
                        View.Universe.LoadMap(mapWrapper.Name, true);
                        View.Universe.LoadRelatedMapsER(mapWrapper.Name);
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
                var mapName = AliasHelper.GetMapNameAlias(View.Project, mapWrapper.Name);
                PlatformUtils.Instance.SetClipboardText(mapName);
            }
            if (ImGui.Selectable("Add to Map Filter"))
            {
                View.GlobalSearchTool.AddMapFilterInput(mapWrapper.Name);
            }

            ImGui.EndPopup();
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
