using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Main interface for the MSB Editor.
/// </summary>
public class MapEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public MapViewHandler ViewHandler;

    public MapCommandQueue CommandQueue;
    public MapShortcuts Shortcuts;

    public ResourceListTool ResourceList;

    /// <summary>
    /// The copied map objects, shared by every view so a copy in one can be pasted in another
    /// </summary>
    public List<MsbEntity> Clipboard = new();

    /// <summary>
    /// The world matrix of each copied object's parents when it was copied, for Keep World Position on paste
    /// </summary>
    public Dictionary<MsbEntity, Matrix4x4> ClipboardParentWorlds = new();

    /// <summary>
    /// The problems that stopped the last save, for the user to fix or save past
    /// </summary>
    public MapSaveCheckWindow SaveCheckWindow;

    public MapEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new(this, project);

        ResourceList = new();

        CommandQueue = new MapCommandQueue(this, project);
        Shortcuts = new MapShortcuts(this, project);

        SaveCheckWindow = new MapSaveCheckWindow(this, project);
        VanillaCompare = new MapVanillaCompare(project);
    }

    /// <summary>
    /// The loaded maps compared with their vanilla MSBs, for the Map Contents highlighting, Properties and the
    /// Vanilla Compare tool
    /// </summary>
    public MapVanillaCompare VanillaCompare;

    public string EditorName => "Visual Map Editor";
    public string CommandEndpoint => "map";
    public string SaveType => "Maps";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();

        var activeView = ViewHandler.ActiveView;

        Shortcuts.Monitor();
        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            if(activeView != null)
            {
                activeView.ToolView.DisplayDropdown();
            }

            OptionsMenu();

            if(Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            {
                ImGui.Separator();

                ImGui.Text(LOC.Get("SYS_OpenGL_Viewport_Blocked"));
            }

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_MapEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ref GUI.DockGroup_MapEditor);

        ViewHandler.HandleViews(dsid);

        SaveCheckWindow.Display();
    }

    public void FileMenu()
    {
        var activeView = ViewHandler.ActiveView;

        // File
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            // Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##saveAction", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_SaveAll_TT"));

            ImGui.Separator();

            // Allow Unmodified Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save")}##allowUnmodifiedSave"))
            {
                CFG.Current.MapEditor_Allow_Unmodified_Save = !CFG.Current.MapEditor_Allow_Unmodified_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.MapEditor_Allow_Unmodified_Save);

            // Allow Automatic Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                CFG.Current.MapEditor_Allow_Automatic_Save = !CFG.Current.MapEditor_Allow_Automatic_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.MapEditor_Allow_Automatic_Save);

            ImGui.Separator();

            // Check Maps Before Saving
            if (ImGui.MenuItem($"{LOC.Get("MAP_SaveCheck_Menu_Enabled")}##saveCheckEnabled"))
            {
                CFG.Current.MapEditor_SaveCheck_Enabled = !CFG.Current.MapEditor_SaveCheck_Enabled;
            }
            GUI.Tooltip(LOC.Get("MAP_SaveCheck_Menu_Enabled_TT"));
            GUI.ShowActiveStatus(CFG.Current.MapEditor_SaveCheck_Enabled);

            // Stop on Warnings
            if (ImGui.MenuItem($"{LOC.Get("MAP_SaveCheck_Menu_Stop_On_Warnings")}##saveCheckWarnings", "", false,
                CFG.Current.MapEditor_SaveCheck_Enabled))
            {
                CFG.Current.MapEditor_SaveCheck_Stop_On_Warnings = !CFG.Current.MapEditor_SaveCheck_Stop_On_Warnings;
            }
            GUI.Tooltip(LOC.Get("MAP_SaveCheck_Menu_Stop_On_Warnings_TT"));
            GUI.ShowActiveStatus(CFG.Current.MapEditor_SaveCheck_Stop_On_Warnings);

            // Show the last report
            if (ImGui.MenuItem($"{LOC.Get("MAP_SaveCheck_Menu_Check_Now")}##saveCheckNow"))
            {
                var maps = Project.Handler.MapData.PrimaryBank.Maps.Values
                    .Select(w => w.MapContainer)
                    .Where(m => m != null)
                    .ToList();

                SaveCheckWindow.Open(MapCheck.Run(Project, maps), maps);
            }
            GUI.Tooltip(LOC.Get("MAP_SaveCheck_Menu_Check_Now_TT"));

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        // Edit
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##editMenuHeader"))
        {
            if (activeView != null)
            {
                var canUndo = activeView.ViewportActionManager.CanUndo();
                var canRedo = activeView.ViewportActionManager.CanRedo();

                // Undo (greyed out when there is nothing to undo)
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}", false, canUndo))
                {
                    if (activeView.ViewportActionManager.CanUndo())
                    {
                        activeView.ViewportActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}##undoAllAction", "", false, canUndo))
                {
                    if (activeView.ViewportActionManager.CanUndo())
                    {
                        activeView.ViewportActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}", false, canRedo))
                {
                    if (activeView.ViewportActionManager.CanRedo())
                    {
                        activeView.ViewportActionManager.RedoAction();
                    }
                }

                ImGui.Separator();

                activeView.DuplicateAction.OnMenu();
                activeView.DeleteAction.OnMenu();
                activeView.TranslateAction.OnMenu();
                activeView.RotateAction.OnMenu();
                activeView.ScrambleAction.OnMenu();
                activeView.ReplicateAction.OnMenu();

                ImGui.Separator();

                activeView.CreateAction.OnMenu();
                activeView.DuplicateToMapAction.OnMenu();
                activeView.MoveToMapAction.OnMenu();

                ImGui.Separator();

                activeView.GotoAction.OnMenu();
                activeView.FrameAction.OnMenu();
                activeView.PullToCameraAction.OnMenu();
                activeView.SnapToMeshAction.OnMenu();

                ImGui.Separator();

                activeView.ReorderAction.OnMenu();

                ImGui.Separator();

                activeView.EditorVisibilityAction.OnMenu();
                activeView.GameVisibilityAction.OnMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            // Individual Windows
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Options_EditorDisplay_Action_Windows")}##windows"))
            {
                CFG.Current.Interface_MapEditor_EditorDisplayType = EditorDisplayType.Windows;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Options_EditorDisplay_Action_Windows_TT"));
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_EditorDisplayType == EditorDisplayType.Windows);

            // Tabbed Panels
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Options_EditorDisplay_Action_TabbedPanels")}##tabbedPanels"))
            {
                CFG.Current.Interface_MapEditor_EditorDisplayType = EditorDisplayType.TabbedPanels;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Options_EditorDisplay_Action_TabbedPanels_TT"));
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_EditorDisplayType == EditorDisplayType.TabbedPanels);

            ImGui.Separator();

            // Tools
            if (ImGui.MenuItem($"{LOC.Get("MAP_View_Toggle_Tools")}##toolsToggle"))
            {
                CFG.Current.Interface_MapEditor_ToolWindow = !CFG.Current.Interface_MapEditor_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_ToolWindow);

            // Map Information
            if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
            {
                if (ImGui.MenuItem($"{LOC.Get("MAP_MapInfo_Window_Title")}##mapInformationToggle"))
                {
                    CFG.Current.Interface_MapEditor_MapInformation = !CFG.Current.Interface_MapEditor_MapInformation;
                }
                GUI.Tooltip(LOC.Get("MAP_MapInfo_Open_TT", InputManager.GetHint(KeybindID.MapEditor_Toggle_World_Map_Menu)));
                GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_MapInformation);
            }

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.Separator();

            var activeView = ViewHandler.ActiveView;

            if(activeView != null)
            {
                activeView.ViewportHandler.DisplayMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void FilterMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        var validViewportState = activeView.ViewportHandler.ActiveViewport.RenderScene != null &&
             activeView.ViewportHandler.ActiveViewport.Viewport != null;

        // General Filters
        if (ImGui.BeginMenu($"{LOC.Get("MAP_Filter_Header")}##filtersMenuHeader", validViewportState))
        {
            activeView.BasicFilters.Display();

            ImGui.Separator();

            // Filter Presets
            if (ImGui.BeginMenu($"{LOC.Get("MAP_Filter_Header_FilterPresets")}##filterPresetsMenuHeader"))
            {
                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_1.Name))
                {
                    activeView.ViewportHandler.ActiveViewport.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_1.Filters;

                    activeView.BasicFilters.StoreFilterState();
                    activeView.DelayPicking();
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_2.Name))
                {
                    activeView.ViewportHandler.ActiveViewport.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_2.Filters;

                    activeView.BasicFilters.StoreFilterState();
                    activeView.DelayPicking();
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_3.Name))
                {
                    activeView.ViewportHandler.ActiveViewport.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_3.Filters;

                    activeView.BasicFilters.StoreFilterState();
                    activeView.DelayPicking();
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_4.Name))
                {
                    activeView.ViewportHandler.ActiveViewport.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_4.Filters;

                    activeView.BasicFilters.StoreFilterState();
                    activeView.DelayPicking();
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_5.Name))
                {
                    activeView.ViewportHandler.ActiveViewport.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_5.Filters;

                    activeView.BasicFilters.StoreFilterState();
                    activeView.DelayPicking();
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_6.Name))
                {
                    activeView.ViewportHandler.ActiveViewport.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_6.Filters;

                    activeView.BasicFilters.StoreFilterState();
                    activeView.DelayPicking();
                }

                ImGui.EndMenu();
            }

            // Region Filters
            if (ImGui.BeginMenu($"{LOC.Get("MAP_Filter_Header_Region_Visibility")}##regionVisibilityHeader", validViewportState))
            {
                activeView.RegionFilters.DisplayOptions();

                ImGui.EndMenu();
            }

            // Collision Filters
            if (ImGui.BeginMenu($"{LOC.Get("MAP_Filter_Header_Collision_Visibility")}##collisionVisibilityHeader", validViewportState))
            {
                CollisionMenu();

                ImGui.EndMenu();
            }

            // Patrol Routes
            if (ImGui.BeginMenu($"{LOC.Get("MAP_Filter_Header_Patrol_Route_Visibility")}##patrolRouteVisibilityHeader", validViewportState))
            {
                if (activeView.Project.Descriptor.ProjectType != ProjectType.DS2S && activeView.Project.Descriptor.ProjectType != ProjectType.DS2)
                {
                    // Display
                    if (ImGui.MenuItem($"{LOC.Get("MAP_Filter_Header_Patrol_Route_Visibility_Action_Display")}##displayPatrolRoutes"))
                    {
                        activeView.PatrolDrawManager.Generate();
                        activeView.DelayPicking();
                    }
                    GUI.Tooltip(LOC.Get("MAP_Filter_Header_Patrol_Route_Visibility_Action_Display_TT"));

                    // Clear
                    if (ImGui.MenuItem($"{LOC.Get("MAP_Filter_Header_Patrol_Route_Visibility_Action_Clear")}##clearPatrolRoutes"))
                    {
                        activeView.PatrolDrawManager.Clear();
                        activeView.DelayPicking();
                    }
                    GUI.Tooltip(LOC.Get("MAP_Filter_Header_Patrol_Route_Visibility_Action_Clear_TT"));
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void CollisionMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        var validViewportState = activeView.ViewportHandler.ActiveViewport.RenderScene != null &&
            activeView.ViewportHandler.ActiveViewport.Viewport != null;

        var bank = activeView.HavokCollisionBank;

        // Each type is toggled on its own, so any combination of them can be displayed
        foreach (var type in bank.GetSupportedCollisionTypes())
        {
            var (header, id) = type switch
            {
                HavokCollisionType.High => ("MAP_Collision_Header_High", "toggleHighCollision"),
                HavokCollisionType.FallProtection => ("MAP_Collision_Header_Fall_Protection", "toggleFallProtectionCollision"),
                _ => ("MAP_Collision_Header_Low", "toggleLowCollision")
            };

            if (ImGui.MenuItem($"{LOC.Get(header)}##{id}"))
            {
                bank.ToggleCollisionType(type);
                activeView.DelayPicking();
            }
            GUI.Tooltip(LOC.Get($"{header}_TT"));
            GUI.ShowActiveStatus(bank.IsCollisionTypeVisible(type));
        }
    }



    public void OptionsMenu()
    {
        var activeView = ViewHandler.ActiveView;

        // Options
        if (ImGui.BeginMenu($"{LOC.Get("MAP_Options_Header_Options")}##optionsHeader"))
        {
            // Map List
            if (ImGui.BeginMenu($"{LOC.Get("MAP_Options_Header_MapList")}##mapListHeader"))
            {
                // Unload Current
                // Asks only if the map has unsaved changes, offering to save them
                if (ImGui.MenuItem($"{LOC.Get("MAP_Options_Action_Unload_Current")}##unloadCurrentAction", "", false,
                    !string.IsNullOrEmpty(activeView?.Selection.SelectedMapID)))
                {
                    activeView.Universe.RequestUnloadMap(activeView.Selection.SelectedMapID);
                }
                GUI.Tooltip(LOC.Get("MAP_Options_Action_Unload_Current_TT"));

                // Unload All
                if (ImGui.MenuItem($"{LOC.Get("MAP_Options_Action_Unload_All")}##unloadAllAction"))
                {
                    activeView?.Universe.RequestUnloadAllMaps();
                }
                GUI.Tooltip(LOC.Get("MAP_Options_Action_Unload_All_TT"));

                // List Filters
                if (ImGui.BeginMenu($"{LOC.Get("MAP_Options_Header_List_Filters")}##listFiltersHeader"))
                {
                    activeView.MapListFilterTool.DisplayMenu();
                    ImGui.EndMenu();
                }
                GUI.Tooltip(LOC.Get("MAP_Options_Header_List_Filters_TT"));

                ImGui.EndMenu();
            }

            // Content Display
            if (ImGui.BeginMenu($"{LOC.Get("MAP_Options_Header_Content_Display")}##contentDisplayHeader"))
            {
                // Tree
                if (ImGui.MenuItem($"{LOC.Get("MAP_Options_ContentDisplay_Action_Tree")}##treeAction"))
                {
                    activeView.MapContentView.ContentViewType = MapContentViewType.ObjectType;
                }
                GUI.Tooltip(LOC.Get("MAP_Options_ContentDisplay_Action_Tree_TT"));
                GUI.ShowActiveStatus(activeView.MapContentView.ContentViewType == MapContentViewType.ObjectType);

                // Flat
                if (ImGui.MenuItem($"{LOC.Get("MAP_Options_ContentDisplay_Action_Flat")}##flatAction"))
                {
                    activeView.MapContentView.ContentViewType = MapContentViewType.Flat;
                }
                GUI.Tooltip(LOC.Get("MAP_Options_ContentDisplay_Action_Flat_TT"));
                GUI.ShowActiveStatus(activeView.MapContentView.ContentViewType == MapContentViewType.Flat);

                ImGui.Separator();

                // Highlight Changes from Vanilla
                if (ImGui.MenuItem($"{LOC.Get("MAP_Vanilla_Highlight")}##vanillaHighlight"))
                {
                    CFG.Current.MapEditor_Vanilla_Highlight = !CFG.Current.MapEditor_Vanilla_Highlight;
                }
                GUI.Tooltip(LOC.Get("MAP_Vanilla_Highlight_TT"));
                GUI.ShowActiveStatus(CFG.Current.MapEditor_Vanilla_Highlight);

                ImGui.EndMenu();
            }

            if (activeView.LightAtlasBank.CanUse())
            {
                // Light Atlases
                if (ImGui.BeginMenu($"{LOC.Get("MAP_Options_LightAtlas_Header")}##lightAtlasesHeader"))
                {
                    // Automatic Adjust
                    if (ImGui.MenuItem($"{LOC.Get("MAP_Options_LightAtlas_Automatic_Adjust_Toggle")}##autoAdjustAction"))
                    {
                        CFG.Current.MapEditor_LightAtlas_AutomaticAdjust = !CFG.Current.MapEditor_LightAtlas_AutomaticAdjust;
                    }
                    GUI.Tooltip(LOC.Get("MAP_Options_LightAtlas_Automatic_Adjust_Toggle_TT"));
                    GUI.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticAdjust);

                    // Automatic Add
                    if (ImGui.MenuItem($"{LOC.Get("MAP_Options_LightAtlas_Automatic_Add_Toggle")}##autoAddAction"))
                    {
                        CFG.Current.MapEditor_LightAtlas_AutomaticAdd = !CFG.Current.MapEditor_LightAtlas_AutomaticAdd;
                    }
                    GUI.Tooltip(LOC.Get("MAP_Options_LightAtlas_Automatic_Add_Toggle_TT"));
                    GUI.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticAdd);

                    // Automatic Delete
                    if (ImGui.MenuItem($"{LOC.Get("MAP_Options_LightAtlas_Automatic_Delete_Toggle")}##autoDeleteAction"))
                    {
                        CFG.Current.MapEditor_LightAtlas_AutomaticDelete = !CFG.Current.MapEditor_LightAtlas_AutomaticDelete;
                    }
                    GUI.Tooltip(LOC.Get("MAP_Options_LightAtlas_Automatic_Delete_Toggle_TT"));
                    GUI.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticDelete);

                    ImGui.EndMenu();
                }
            }

            ImGui.EndMenu();
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (ViewHandler.ViewToClose == null)
        {
            foreach (var view in ViewHandler.Views)
            {
                if (view == null || view.ShowMapData)
                    continue;

                foreach(var viewport in view.ViewportHandler.Viewports)
                {
                    if (viewport == null)
                        continue;

                    if (viewport.Viewport is VulkanViewport vulkanViewport)
                    {
                        if (vulkanViewport.Visible)
                        {
                            vulkanViewport.Draw(device, cl);
                        }
                    }
                }
            }
        }

        // Done here so we don't mutate the list during drawing
        if(ViewHandler.ViewToClose != null)
        {
            ViewHandler.RemoveView(ViewHandler.ViewToClose);

            ViewHandler.ViewToClose = null;
        }
    }

    public void Update(float dt)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        activeView.ViewportWindow.Update(dt);

        // The irradiance volumes of every loaded map, shared by all views, with the bakes of the menu or of the
        // map lighting's time of day
        var loadedMaps = activeView.Project.Handler.MapData.PrimaryBank.Maps.Values
            .Select(w => w.MapContainer)
            .Where(m => m != null && m.LoadState == MapContentLoadState.Loaded)
            .ToList();

        var (bake, blendBake, blendRatio) = activeView.IrradianceVolumeBank.ResolveBakes(loadedMaps);

        activeView.IrradianceVolumeBank.UpdateGpuTable(loadedMaps, bake, blendBake, blendRatio);

        // The sun and exposure of the loaded maps' GPARAMs, shared by all views; for Demon's Souls, of their DrawParam
        SceneRenderer.MapLighting = activeView.DrawParamLightingBank.CanUse()
            ? activeView.DrawParamLightingBank.ResolveForViewport(loadedMaps)
            : activeView.GparamLightingBank.ResolveForViewport(loadedMaps);

        // Demon's Souls' parts lit by the LightBank rows of their own LightIDs, as lighting sets 1 onwards of each view
        // (SceneRenderPipeline.SceneLightingBuffer), named by each part's instance (InstanceData.LightingSet)
        SceneRenderer.MapLightSets = activeView.DrawParamLightingBank.CanUse()
            ? activeView.DrawParamLightingBank.UpdatePartLighting(loadedMaps, SceneRenderer.MapLighting)
            : Array.Empty<GparamLighting>();

        // DS3's lightmaps of the map pieces, with the lighting's Dominant Light Map values
        activeView.Project.Handler.MapData.DolLightmapBank?.Update(loadedMaps, SceneRenderer.MapLighting);

        // The fog volumes follow the same GPARAM file, weather and time of day
        activeView.FogVolumeBank.Update(loadedMaps);

        // Throw any exceptions that ocurred during async map loading, in any view.
        // Cleared before throwing so a handled exception is not rethrown every frame.
        foreach (var view in ViewHandler.Views)
        {
            var loadException = view?.Universe.LoadMapExceptions;

            if (loadException != null)
            {
                view.Universe.LoadMapExceptions = null;
                loadException.Throw();
            }

            view?.HavokCollisionBank.UpdateObjectCollision(view.ViewportHandler.ActiveViewport.RenderScene.DrawFilter);
            view?.PrefabTool.Update();
            view?.CreateAction.MassCreate.Update();
        }

        // The bank is shared by all views, so grass stays built while any of them displays it
        var showGrass = ViewHandler.Views.Any(view => view != null &&
            view.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Grass));

        ViewHandler.ActiveView?.GrassBank.Update(showGrass);

        // Same as the above
        var showEdgePoints = ViewHandler.Views.Any(view => view != null &&
            view.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.EdgePoint));

        ViewHandler.ActiveView?.EdgeBank.Update(showEdgePoints);

        // Same as the above
        var showWalls = ViewHandler.Views.Any(view => view != null &&
            view.ViewportHandler.ActiveViewport.RenderScene.DrawFilter.HasFlag(RenderFilter.Wall));

        ViewHandler.ActiveView?.WallBank.Update(showWalls);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        activeView.ViewportWindow.EditorResized(window, device);
    }

    public bool InputCaptured()
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return false;

        return activeView.ViewportWindow.InputCaptured();
    }

    /// <param name="autoSave">An autosave: skipped unless allowed, and it opens no windows</param>
    /// <param name="skipCheck">Save without the save check, as Save Anyway does</param>
    public void Save(bool autoSave = false, bool skipCheck = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        // The save check: problems in the maps about to be written are listed in the Save Check window, and
        // nothing is saved if any of them stops the save. Autosaves keep their old behaviour (U10).
        if (!autoSave && !skipCheck && CFG.Current.MapEditor_SaveCheck_Enabled)
        {
            var maps = GetMapsToSave();
            var issues = MapCheck.Run(Project, maps);

            if (MapCheck.StopsSave(issues, CFG.Current.MapEditor_SaveCheck_Stop_On_Warnings))
            {
                SaveCheckWindow.Open(issues, maps);
                Smithbox.Log(this, LOC.Get("MAP_SaveCheck_Log_Stopped", issues.Count(i => i.Severity is MapCheckSeverity.Error),
                    issues.Count(i => i.Severity is MapCheckSeverity.Warning)), LogLevel.Warning);
                return;
            }

            // Nothing stops this save, so an open report is out of date
            SaveCheckWindow.Close();
        }

        try
        {
            // Saves the modified files of all loaded maps, including their collision and navmesh binders
            if (autoSave && CFG.Current.MapEditor_Allow_Automatic_Save || !autoSave)
            {
                activeView.Universe.SaveAllMaps();

                // Load list, entry file list and parts token ID list edits are kept per view, so each view's are saved
                foreach (var view in ViewHandler.Views)
                {
                    view.LoadListTool.SaveModified();
                    view.EntryFileListTool.SaveModified();
                    view.PartsTokenIdListTool.SaveModified();
                    view.WorldMapTool.MapInfo.Bank.SaveModified();
                }
            }
        }
        catch (SavingFailedException e)
        {
            if (autoSave)
            {
                // No dialog for an autosave: it would reappear every interval until the problem is fixed.
                // The map is marked in the Map List instead (MapContainer.SaveError).
                Smithbox.Log(this, LOC.Get("MAP_Autosave_Failed", e.Message), LogLevel.Error, LogPriority.Normal, e.Wrapped);
            }
            else
            {
                HandleSaveException(e);
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }


    public List<string> GetUnsavedFiles()
    {
        var files = new List<string>();

        var bank = Project.Handler.MapData?.PrimaryBank;

        if (bank != null)
        {
            files.AddRange(bank.Maps.Values
                .Select(w => w.MapContainer)
                .Where(m => m != null && m.HasUnsavedChanges)
                .Select(m => m.Name));
        }

        // Kept per view, like the save that writes them
        foreach (var view in ViewHandler.Views)
        {
            if (view == null)
                continue;

            if (view.LoadListTool?.HasModifiedFiles == true)
                files.Add(LOC.Get("UNSAVED_Map_Load_List"));

            if (view.EntryFileListTool?.HasModifiedFiles == true)
                files.Add(LOC.Get("UNSAVED_Map_Entry_File_List"));

            if (view.PartsTokenIdListTool?.HasModifiedFiles == true)
                files.Add(LOC.Get("UNSAVED_Map_Parts_Token_ID_List"));
        }

        return files.Distinct().ToList();
    }

    public void SaveUnsaved()
    {
        // From the unsaved changes prompt when exiting or closing the project, which can't wait for the Save
        // Check window; a failure still shows the save-failure dialog
        Save(skipCheck: true);
    }

    /// <summary>
    /// The loaded maps whose MSB a save will write: those with MSB edits, or all of them when unmodified saves
    /// are allowed. Shared by every view, like the save.
    /// </summary>
    public List<MapContainer> GetMapsToSave()
    {
        return Project.Handler.MapData.PrimaryBank.Maps.Values
            .Select(w => w.MapContainer)
            .Where(m => m != null && m.LoadState is MapContentLoadState.Loaded &&
                (CFG.Current.MapEditor_Allow_Unmodified_Save || m.IsDirty(MapFileType.MSB)))
            .ToList();
    }

    public void HandleSaveException(SavingFailedException e)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (e.Wrapped is MSB.MissingReferenceException eRef)
        {
            Smithbox.Log(this, e.Message,
                LogLevel.Error, LogPriority.Normal, e.Wrapped);

            // The object holding the reference, and the fields that hold the missing name
            Entity referrer = Project.Handler.MapData.PrimaryBank.Maps.Values
                .Select(w => w.MapContainer)
                .Where(m => m != null)
                .SelectMany(m => m.Objects)
                .FirstOrDefault(o => o.WrappedObject == eRef.Referrer);

            if (referrer == null)
            {
                Smithbox.LogError(this, LOC.Get("MAP_SaveException_Error_MapEntity", eRef.Referrer.Name));
                return;
            }

            var slots = MsbReferences.GetSlots(referrer).Where(s => s.Value == eRef.ReferreeName).ToList();
            var fields = slots.Count > 0 ? string.Join(", ", slots.Select(s => s.FieldName)) : "?";

            DialogResult result = PlatformUtils.Instance.MessageBox(
                LOC.Get("MAP_SaveException_Missing_Reference", referrer.Name, fields, eRef.ReferreeName),
                LOC.Get("MAP_SaveException_Error_Log_Title"),
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Error);

            // The object's undo history and selection belong to the view that loaded its map
            var ownerView = (referrer.Container as MapContainer)?.OwnerView ?? activeView;

            if (result == DialogResult.Yes && slots.Count > 0)
            {
                ownerView.ViewportActionManager.ExecuteAction(new ClearReferencesAction(slots));
                Smithbox.Log(this, LOC.Get("MAP_SaveException_Reference_Cleared", referrer.Name, fields));
            }
            else if (result == DialogResult.No || (result == DialogResult.Yes && slots.Count == 0))
            {
                ownerView.ViewportSelection.ClearSelection();
                ownerView.ViewportSelection.AddSelection(referrer);

                ownerView.FrameAction.ApplyViewportFrame();
            }
        }
        else
        {
            Smithbox.Log(this, e.Message,
                LogLevel.Error, LogPriority.High, e.Wrapped);
        }
    }
}
