using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor.Core;

public class MapListView : Actions.Viewport.IActionEventHandler
{
    private MapEditorScreen Editor;
    private IViewport Viewport;

    private ViewportActionManager EditorActionManager;
    private ViewportSelection Selection;
    private EditorFocusManager FocusManager;

    private string ImguiID;

    public List<string> MapIDs = new();
    public Dictionary<string, MapContentView> ContentViews = new();
    public bool SetupContentViews = false;

    public string SearchBarText = "";

    public string SelectedMap = "";

    private bool DisplayChaliceDungeons = true;

    public MapListView(MapEditorScreen screen)
    {
        Editor = screen;

        EditorActionManager = screen.EditorActionManager;
        Selection = screen.Selection;
        Viewport = screen.MapViewportView.Viewport;
        FocusManager = screen.FocusManager;
    }

    public void OnProjectChanged()
    {
        MapIDs = new();
        ContentViews = new();
        SetupContentViews = false;
    }

    /// <summary>
    /// Handles the update for each frame
    /// </summary>
    public void OnGui()
    {
        var scale = DPI.GetUIScale();

        if (UI.Current.Interface_MapEditor_MapList)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            // Map List
            if (ImGui.Begin($@"Map List##mapIdList"))
            {
                FocusManager.SwitchWindowContext(MapEditorContext.MapIdList);

                // World Map
                Editor.WorldMapView.DisplayWorldMapButton();
                Editor.WorldMapView.DisplayWorldMap();

                DisplaySearchbar();
                ImGui.SameLine();
                DisplayUnloadAllButton();
                if (Editor.Project.ProjectType is ProjectType.BB)
                {
                    ImGui.SameLine();
                    DisplayChaliceToggleButton();
                }

                ImGui.Separator();

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

                // Display List of Maps
                if (SetupContentViews)
                {
                    DisplayMapList(MapContentLoadState.Loaded);

                    ImGui.BeginChild($"mapListSection");
                    DisplayMapList(MapContentLoadState.Unloaded);
                    ImGui.EndChild();
                }
            }

            ImGui.End();
            ImGui.PopStyleColor();
        }

        if (UI.Current.Interface_MapEditor_MapContents)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            // Map Contents
            if (ImGui.Begin($@"Map Contents##mapContentsPanel"))
            {
                FocusManager.SwitchWindowContext(MapEditorContext.MapContents);

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

        Selection.ClearGotoTarget();
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
            UIHelper.Tooltip("Filter the map list entries.");
        }
    }

    /// <summary>
    /// Handles the unload all button
    /// </summary>
    private void DisplayUnloadAllButton()
    {
        if (ImGui.Button($"{ForkAwesome.MinusSquareO}"))
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
        if (ImGui.Button($"{ForkAwesome.Adjust}"))
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
        foreach (var entry in MapIDs)
        {
            MapContentView curView = null;

            // Skip entry if it isn't valid for the searchbar input
            if(!SearchFilters.IsMapSearchMatch(SearchBarText, entry, AliasUtils.GetMapNameAlias(Editor.Project, entry), AliasUtils.GetMapTags(Editor.Project, entry)) && loadType == MapContentLoadState.Unloaded)
            {
                continue;
            }

            if(!DisplayChaliceDungeons)
            {
                if (entry.Contains("m29_"))
                    continue;
            }

            if (ContentViews.ContainsKey(entry))
            {
                curView = ContentViews[entry];
            }

            // Display map ID entry
            if (curView != null && curView.ContentLoadState == loadType)
            {
                if (ImGui.Selectable($"##mapListEntry{entry}", entry == SelectedMap, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (loadType == MapContentLoadState.Loaded)
                    {
                        SelectedMap = entry;
                    }

                    if (CFG.Current.MapEditor_Enable_Map_Load_on_Double_Click)
                    {
                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
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
                }

                var mapId = curView.MapID;
                var displayedName = $"{mapId}";

                if (CFG.Current.MapEditor_MapObjectList_ShowMapNames)
                {
                    var mapName = AliasUtils.GetMapNameAlias(Editor.Project, curView.MapID);
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
