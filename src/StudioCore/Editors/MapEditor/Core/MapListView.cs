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

namespace StudioCore.Editors.MapEditor.Core;

public class MapListView : Actions.Viewport.IActionEventHandler
{
    private MapEditorScreen Editor;
    private IViewport Viewport;

    private string ImguiID = "MapListView";

    public List<string> MapIDs = new();
    public Dictionary<string, MapContentView> ContentViews = new();
    public bool SetupContentViews = false;

    public string SearchBarText = "";

    public string SelectedMap = "";

    private bool DisplayChaliceDungeons = true;

    public MapListView(MapEditorScreen screen)
    {
        Editor = screen;
        Viewport = screen.MapViewportView.Viewport;
    }


    /// <summary>
    /// Handles the update for each frame
    /// </summary>
    public void OnGui()
    {
        var scale = DPI.GetUIScale();

        if (CFG.Current.Interface_MapEditor_MapList)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            // Map List
            if (ImGui.Begin($@"Map List##mapIdList"))
            {
                Editor.FocusManager.SwitchWindowContext(MapEditorContext.MapIdList);

                // Setup the Content Views
                if (Editor.Universe.GetMapContainerCount() > 0 && !SetupContentViews)
                {
                    SetupContentViews = true;

                    foreach (var entry in Editor.Universe.GetMapContainerList())
                    {
                        var newView = new MapContentView(Editor, entry.Key, entry.Value);

                        if (!ContentViews.ContainsKey(newView.MapID))
                        {
                            MapIDs.Add(newView.MapID);
                            ContentViews.Add(newView.MapID, newView);
                        }
                    }
                }

                DisplaySearchbar();

                ImGui.SameLine();

                DisplayUnloadAllButton();

                if (Editor.Project.ProjectType is ProjectType.BB)
                {
                    ImGui.SameLine();
                    DisplayChaliceToggleButton();
                }

                ImGui.Separator();

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
            if (ImGui.Begin($@"Map Contents##mapContentsPanel"))
            {
                Editor.FocusManager.SwitchWindowContext(MapEditorContext.MapContents);

                if (ContentViews.Count > 0)
                {
                    foreach (var entry in ContentViews)
                    {
                        if (entry.Key == SelectedMap)
                        {
                            entry.Value.OnGui();
                        }
                    }
                }
            }

            ImGui.End();
            ImGui.PopStyleColor(1);
        }

        Editor.Selection.ClearGotoTarget();
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
        var width = ImGui.GetWindowWidth();

        if (CFG.Current.MapEditor_MapObjectList_ShowMapIdSearch)
        {
            ImGui.SetNextItemWidth(width * 0.75f);
            ImGui.InputText($"##mapListSearch_{ImguiID}", ref SearchBarText, 255);
            if(ImGui.IsItemDeactivatedAfterEdit())
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

            foreach (var entry in MapIDs)
            {
                var nameAlias = AliasUtils.GetMapNameAlias(Editor.Project, entry);
                var tags = AliasUtils.GetMapTags(Editor.Project, entry);

                _cachedMapNameAliases[entry] = nameAlias;
                _cachedMapTags[entry] = tags;

                bool isMatch = SearchFilters.IsMapSearchMatch(_lastSearchText, entry, nameAlias, tags);

                if (isMatch || _lastSearchText == "")
                {
                    _cachedSearchMatches.Add(entry);
                }
            }
        }
    }

    /// <summary>
    /// Handles the unload all button
    /// </summary>
    private void DisplayUnloadAllButton()
    {
        if (ImGui.Button($"{Icons.MinusSquareO}"))
        {
            DialogResult result = PlatformUtils.Instance.MessageBox("Unload all maps?", "Confirm",
                        MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                foreach (var entry in Editor.MapListView.ContentViews)
                {
                    if(entry.Value.ContentLoadState == MapContentLoadState.Loaded)
                        entry.Value.Unload();
                }

                Editor.Universe.UnloadAllMaps();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
        UIHelper.Tooltip("Unload all currently loaded maps.");
    }

    /// <summary>
    /// Handles the chalice MSB toggle button
    /// </summary>
    private void DisplayChaliceToggleButton()
    {
        if (ImGui.Button($"{Icons.Adjust}"))
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
        var filteredEntries = new List<string>();
        foreach (var entry in MapIDs)
        {
            if (!_cachedSearchMatches.Contains(entry) && loadType == MapContentLoadState.Unloaded)
            {
                continue;
            }

            if (!DisplayChaliceDungeons && entry.Contains("m29_"))
            {
                continue;
            }

            if (ContentViews.TryGetValue(entry, out var curView) && curView.ContentLoadState == loadType)
            {
                filteredEntries.Add(entry);
            }
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredEntries.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var entry = filteredEntries[i];
                var curView = ContentViews[entry];

                if (ImGui.Selectable($"##mapListEntry{entry}", entry == SelectedMap, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (loadType == MapContentLoadState.Loaded)
                    {
                        SelectedMap = entry;
                    }

                    if (CFG.Current.MapEditor_Enable_Map_Load_on_Double_Click && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (loadType == MapContentLoadState.Loaded)
                        {
                            curView.Unload();
                            SelectedMap = "";
                        }
                        else
                        {
                            curView.Load(true);
                            SelectedMap = entry;
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
                DisplayContextMenu(entry, curView);
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
                }
            }

            // Loaded Map
            if (curView.ContentLoadState is MapContentLoadState.Loaded)
            {
                if (curView.Container is MapContainer m)
                {
                    // Save Map
                    if (ImGui.Selectable("Save Map"))
                    {
                        try
                        {
                            Editor.Universe.SaveMap(m);
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
            if (Editor.MapQueryView.IsOpen)
            {
                if (ImGui.Selectable("Add to Map Filter"))
                {
                    Editor.MapQueryView.AddMapFilterInput(entry);
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

    public void SignalLoad(string mapId)
    {
        if(ContentViews.ContainsKey(mapId))
        {
            var curView = ContentViews[mapId];

            curView.Load(true);
        }
    }

    public void SignalUnload(string mapId)
    {
        if (ContentViews.ContainsKey(mapId))
        {
            var curView = ContentViews[mapId];

            curView.Unload();
        }
    }
}
