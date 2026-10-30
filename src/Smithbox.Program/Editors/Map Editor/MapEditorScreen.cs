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

    public MapEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new(this, project);

        ResourceList = new();

        CommandQueue = new MapCommandQueue(this, project);
        Shortcuts = new MapShortcuts(this, project);
    }

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
                // Undo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
                {
                    if (activeView.ViewportActionManager.CanUndo())
                    {
                        activeView.ViewportActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}##undoAllAction"))
                {
                    if (activeView.ViewportActionManager.CanUndo())
                    {
                        activeView.ViewportActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
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

                    CFG.Current.LastSceneFilter = CFG.Current.Viewport_Filter_Preset_1.Filters;
                    activeView.DelayPicking();
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_2.Name))
                {
                    activeView.ViewportHandler.ActiveViewport.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_2.Filters;

                    CFG.Current.LastSceneFilter = CFG.Current.Viewport_Filter_Preset_2.Filters;
                    activeView.DelayPicking();
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_3.Name))
                {
                    activeView.ViewportHandler.ActiveViewport.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_3.Filters;

                    CFG.Current.LastSceneFilter = CFG.Current.Viewport_Filter_Preset_3.Filters;
                    activeView.DelayPicking();
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_4.Name))
                {
                    activeView.ViewportHandler.ActiveViewport.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_4.Filters;

                    CFG.Current.LastSceneFilter = CFG.Current.Viewport_Filter_Preset_4.Filters;
                    activeView.DelayPicking();
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_5.Name))
                {
                    activeView.ViewportHandler.ActiveViewport.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_5.Filters;

                    CFG.Current.LastSceneFilter = CFG.Current.Viewport_Filter_Preset_5.Filters;
                    activeView.DelayPicking();
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_6.Name))
                {
                    activeView.ViewportHandler.ActiveViewport.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_6.Filters;

                    CFG.Current.LastSceneFilter = CFG.Current.Viewport_Filter_Preset_6.Filters;
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

        // Low Collision
        if (ImGui.MenuItem($"{LOC.Get("MAP_Collision_Header_Low")}##toggleLowCollision"))
        {
            activeView.HavokCollisionBank.VisibleCollisionType = HavokCollisionType.Low;
            CFG.Current.CurrentHavokCollisionType = HavokCollisionType.Low;

            activeView.HavokCollisionBank.RefreshCollision();
            activeView.DelayPicking();
        }
        GUI.Tooltip(LOC.Get("MAP_Collision_Header_Low_TT"));
        GUI.ShowActiveStatus(activeView.HavokCollisionBank.VisibleCollisionType == HavokCollisionType.Low);

        // High Collision
        if (ImGui.MenuItem($"{LOC.Get("MAP_Collision_Header_High")}##toggleHighCollision"))
        {
            activeView.HavokCollisionBank.VisibleCollisionType = HavokCollisionType.High;
            CFG.Current.CurrentHavokCollisionType = HavokCollisionType.High;

            activeView.HavokCollisionBank.RefreshCollision();
            activeView.DelayPicking();
        }
        GUI.Tooltip(LOC.Get("MAP_Collision_Header_High_TT"));
        GUI.ShowActiveStatus(activeView.HavokCollisionBank.VisibleCollisionType == HavokCollisionType.High);

        if (Project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            // Fall Protection Collision
            if (ImGui.MenuItem($"{LOC.Get("MAP_Collision_Header_Fall_Protection")}##toggleFallProtectionCollision"))
            {
                activeView.HavokCollisionBank.VisibleCollisionType = HavokCollisionType.FallProtection;
                CFG.Current.CurrentHavokCollisionType = HavokCollisionType.FallProtection;

                activeView.HavokCollisionBank.RefreshCollision();
                activeView.DelayPicking();
            }
            GUI.Tooltip(LOC.Get("MAP_Collision_Header_Fall_Protection_TT"));
            GUI.ShowActiveStatus(activeView.HavokCollisionBank.VisibleCollisionType == HavokCollisionType.FallProtection);
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
                if (ImGui.MenuItem($"{LOC.Get("MAP_Options_Action_Unload_Current")}##unloadCurrentAction"))
                {
                    DialogResult result = PlatformUtils.Instance.MessageBox(
                        LOC.Get("MAP_Options_Dialog_Unload_Current"), 
                        LOC.Get("MAP_Options_Dialog_Confirm"), 
                        MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        activeView.Universe.UnloadMap(activeView.Selection.SelectedMapID);
                    }
                }
                GUI.Tooltip(LOC.Get("MAP_Options_Action_Unload_Current_TT"));

                // Unload All
                if (ImGui.MenuItem($"{LOC.Get("MAP_Options_Action_Unload_All")}##unloadAllAction"))
                {
                    DialogResult result = PlatformUtils.Instance.MessageBox(
                        LOC.Get("MAP_Options_Dialog_Unload_All"),
                        LOC.Get("MAP_Options_Dialog_Confirm"), 
                        MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        activeView.Universe.UnloadAllMaps();
                    }
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
        }
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

    public void Save(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        try
        {
            // Saves the modified files of all loaded maps, including their collision and navmesh binders
            if (autoSave && CFG.Current.MapEditor_Allow_Automatic_Save || !autoSave)
            {
                activeView.Universe.SaveAllMaps();
            }
        }
        catch (SavingFailedException e)
        {
            HandleSaveException(e);
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
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

            DialogResult result = PlatformUtils.Instance.MessageBox(
                LOC.Get("MAP_SaveException_Error_Log", eRef.Message),
                LOC.Get("MAP_SaveException_Error_Log_Title"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Error);
            if (result == DialogResult.Yes)
            {
                foreach (var entry in Project.Locator.MapFiles.Entries)
                {
                    var currentContainer = activeView.Selection.GetMapContainerFromMapID(entry.Filename);

                    if (currentContainer != null)
                    {
                        foreach (Entity obj in currentContainer.Objects)
                        {
                            if (obj.WrappedObject == eRef.Referrer)
                            {
                                activeView.ViewportSelection.ClearSelection();
                                activeView.ViewportSelection.AddSelection(obj);

                                activeView.FrameAction.ApplyViewportFrame();

                                return;
                            }
                        }
                    }
                }

                Smithbox.LogError(this, LOC.Get("MAP_SaveException_Error_MapEntity", eRef.Referrer.Name));
            }
        }
        else
        {
            Smithbox.Log(this, e.Message,
                LogLevel.Error, LogPriority.High, e.Wrapped);
        }
    }
}
