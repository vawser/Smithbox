﻿using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using StudioCore.Editors.ParamEditor;
using StudioCore.MsbEditor;
using StudioCore.Editor;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.Resource.Locators;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Tools.WorldMap;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using System.ComponentModel;

namespace StudioCore.Editors.MapEditor.Core;

public class MapListView : Actions.Viewport.IActionEventHandler
{
    private MapEditorScreen Screen;
    private IViewport Viewport;

    private ViewportActionManager EditorActionManager;
    private Universe Universe;
    private ViewportSelection Selection;
    private EditorFocusManager FocusManager;

    private string ImguiID;

    public List<string> MapIDs = new();
    public Dictionary<string, MapContentView> ContentViews = new();
    public bool SetupContentViews = false;

    private string SearchBarText = "";

    private string SelectedMap = "";

    private bool _chaliceLoadError;
    private string _chaliceMapID = "m29_";

    public MapListView(MapEditorScreen screen)
    {
        Screen = screen;

        EditorActionManager = screen.EditorActionManager;
        Universe = screen.Universe;
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

        if (!UI.Current.Interface_MapEditor_MapList)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        // Wait for DS2 params to finish loading
        if (Smithbox.ProjectType is ProjectType.DS2S || Smithbox.ProjectType is ProjectType.DS2)
        {
            if (ParamBank.PrimaryBank.IsLoadingParams)
            {
                return;
            }
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        // Map List
        if (ImGui.Begin($@"Map List##mapIdList"))
        {
            FocusManager.SwitchWindowContext(MapEditorContext.MapIdList);

            // World Map
            Screen.WorldMapView.DisplayWorldMapButton();
            Screen.WorldMapView.DisplayWorldMap();

            DisplaySearchbar();
            ImGui.SameLine();
            DisplayUnloadAllButton();

            ImGui.Separator();

            // Setup the Content Views
            if (Universe.LoadedObjectContainers.Count > 0 && !SetupContentViews)
            {
                SetupContentViews = true;

                var maps = Universe.LoadedObjectContainers
                    .Where(k => k.Key is not null)
                    .OrderBy(k => k.Key);

                foreach(var entry in maps)
                {
                    var newView = new MapContentView(Screen, entry.Key, entry.Value);

                    if(!ContentViews.ContainsKey(newView.MapID))
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

                // TODO: check this
                if (Smithbox.ProjectType == ProjectType.BB)
                {
                    ChaliceDungeonImportButton();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor();

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
            UIHelper.ShowHoverTooltip("Filter the map list entries.");
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
                foreach (var entry in Screen.MapListView.ContentViews)
                {
                    if(entry.Value.ContentLoadState == MapContentLoadState.Loaded)
                        entry.Value.Unload();
                }

                Universe.UnloadAllMaps();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
        UIHelper.ShowHoverTooltip("Unload all currently loaded maps.");
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
            if(!SearchFilters.IsMapSearchMatch(SearchBarText, entry, AliasUtils.GetMapNameAlias(entry), AliasUtils.GetMapTags(entry)) && loadType == MapContentLoadState.Unloaded)
            {
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
                    var mapName = AliasUtils.GetMapNameAlias(curView.MapID);
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
            if(curView.Container == null)
            {
                // Load Map
                if (ImGui.Selectable("Load Map"))
                {
                    curView.Load(true);
                }
            }

            // Loaded Map
            if (curView.Container is MapContainer m)
            {
                // Save Map
                if (ImGui.Selectable("Save Map"))
                {
                    try
                    {
                        Universe.SaveMap(m);
                    }
                    catch (SavingFailedException e)
                    {
                        Screen.HandleSaveException(e);
                    }
                }

                // Unload Map
                if (ImGui.Selectable("Unload Map"))
                {
                    curView.Unload();
                }

                // ER: Load Related Maps
                if (Universe.GameType is ProjectType.ER)
                {
                    if (entry.StartsWith("m60") || entry.StartsWith("m61"))
                    {
                        if (ImGui.Selectable("Load Related Maps"))
                        {
                            curView.Load(true);
                            Universe.LoadRelatedMapsER(entry, Universe.LoadedObjectContainers);
                        }
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
                var mapName = AliasUtils.GetMapNameAlias(entry);
                PlatformUtils.Instance.SetClipboardText(mapName);
            }
            if (Screen.MapQueryView.IsOpen)
            {
                if (ImGui.Selectable("Add to Map Filter"))
                {
                    Screen.MapQueryView.AddMapFilterInput(entry);
                }
            }

            ImGui.EndPopup();
        }
    }

    public void OnActionEvent(Actions.Viewport.ActionEvent evt)
    {
        if (evt.HasFlag(Actions.Viewport.ActionEvent.ObjectAddedRemoved))
        {
            Screen.EntityTypeCache.InvalidateCache();
        }
    }

    private bool DisplayChaliceDungeonSection = true;

    private void ChaliceDungeonImportButton()
    {
        ImGui.Separator();

        if (ImGui.Selectable("Toggle Chalice Dungeon Panel"))
        {
            DisplayChaliceDungeonSection = !DisplayChaliceDungeonSection;
        }

        ImGui.Separator();

        if (DisplayChaliceDungeonSection)
        {
            var width = ImGui.GetWindowWidth() * 0.95f;

            ImGui.Indent(5f);
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Chalice ID (m29_xx_xx_xx): ");
            var pname = _chaliceMapID;

            if (_chaliceLoadError)
            {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_ErrorInput_Background);
            }

            ImGui.SetNextItemWidth(width);
            if (ImGui.InputText("##chalicename", ref pname, 12))
            {
                _chaliceMapID = pname;
            }

            if (_chaliceLoadError)
            {
                ImGui.PopStyleColor();
            }

            if (ImGui.Button("Load", new Vector2(width, 24)))
            {
                if (!Universe.LoadMap(_chaliceMapID))
                {
                    _chaliceLoadError = true;
                }
                else
                {
                    ImGui.CloseCurrentPopup();
                    _chaliceLoadError = false;
                    _chaliceMapID = "m29_";
                }
            }
            ImGui.Unindent(5f);
        }
    }
}