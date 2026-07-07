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

    public ResourceLoadWindow LoadingModal;
    public ResourceListTool ResourceList;

    public MapEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new(this, project);

        LoadingModal = new();
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

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_MapEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ref GUI.DockGroup_MapEditor);

        ViewHandler.HandleViews(dsid);

        if (activeView != null)
        {
            var curViewport = activeView.ViewportHandler.ActiveViewport;

            if (curViewport.Viewport != null)
            {
                LoadingModal.DisplayWindow(curViewport.Viewport.Width, curViewport.Viewport.Height);
            }
        }
    }

    public void FileMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Output on Manual Save"))
            {
                if (ImGui.MenuItem($"MSB"))
                {
                    CFG.Current.MapEditor_ManualSave_IncludeMSB = !CFG.Current.MapEditor_ManualSave_IncludeMSB;
                }
                GUI.Tooltip("If enabled, the map files are outputted on save.");
                GUI.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeMSB);

                if (ImGui.MenuItem($"BTL"))
                {
                    CFG.Current.MapEditor_ManualSave_IncludeBTL = !CFG.Current.MapEditor_ManualSave_IncludeBTL;
                }
                GUI.Tooltip("If enabled, the light files are outputted on save.");
                GUI.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeBTL);

                if (activeView != null)
                {
                    if (activeView.AutoInvadeBank.CanUse())
                    {
                        if (ImGui.MenuItem($"AIP"))
                        {
                            CFG.Current.MapEditor_ManualSave_IncludeAIP = !CFG.Current.MapEditor_ManualSave_IncludeAIP;
                        }
                        GUI.Tooltip("If enabled, the auto invade point files are outputted on save.");
                        GUI.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeAIP);
                    }

                    if (activeView.HavokNavmeshBank.CanUse())
                    {
                        if (ImGui.MenuItem($"NVA"))
                        {
                            CFG.Current.MapEditor_ManualSave_IncludeNVA = !CFG.Current.MapEditor_ManualSave_IncludeNVA;
                        }
                        GUI.Tooltip("If enabled, the navmesh configuration files are outputted on save.");
                        GUI.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeNVA);
                    }

                    if (activeView.LightAtlasBank.CanUse())
                    {
                        if (ImGui.MenuItem($"BTAB"))
                        {
                            CFG.Current.MapEditor_ManualSave_IncludeBTAB = !CFG.Current.MapEditor_ManualSave_IncludeBTAB;
                        }
                        GUI.Tooltip("If enabled, the light atlas files are outputted on save.");
                        GUI.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeBTAB);
                    }

                    if (activeView.LightProbeBank.CanUse())
                    {
                        if (ImGui.MenuItem($"BTPB"))
                        {
                            CFG.Current.MapEditor_ManualSave_IncludeBTPB = !CFG.Current.MapEditor_ManualSave_IncludeBTPB;
                        }
                        GUI.Tooltip("If enabled, the light probe files are outputted on save.");
                        GUI.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeBTPB);
                    }
                }

                ImGui.EndMenu();
            }
            GUI.Tooltip("Determines which files are outputted during the manual saving process.");

            if (ImGui.BeginMenu("Output on Automatic Save"))
            {
                if (ImGui.MenuItem($"MSB"))
                {
                    CFG.Current.MapEditor_AutomaticSave_IncludeMSB = !CFG.Current.MapEditor_AutomaticSave_IncludeMSB;
                }
                GUI.Tooltip("If enabled, the map files are outputted on save.");
                GUI.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeMSB);

                if (ImGui.MenuItem($"BTL"))
                {
                    CFG.Current.MapEditor_AutomaticSave_IncludeBTL = !CFG.Current.MapEditor_AutomaticSave_IncludeBTL;
                }
                GUI.Tooltip("If enabled, the light files are outputted on save.");
                GUI.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeBTL);

                if (activeView != null)
                {
                    if (activeView.AutoInvadeBank.CanUse())
                    {
                        if (ImGui.MenuItem($"AIP"))
                        {
                            CFG.Current.MapEditor_AutomaticSave_IncludeAIP = !CFG.Current.MapEditor_AutomaticSave_IncludeAIP;
                        }
                        GUI.Tooltip("If enabled, the auto invade point files are outputted on save.");
                        GUI.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeAIP);
                    }

                    if (activeView.HavokNavmeshBank.CanUse())
                    {
                        if (ImGui.MenuItem($"NVA"))
                        {
                            CFG.Current.MapEditor_AutomaticSave_IncludeNVA = !CFG.Current.MapEditor_AutomaticSave_IncludeNVA;
                        }
                        GUI.Tooltip("If enabled, the navmesh configuration files are outputted on save.");
                        GUI.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeNVA);
                    }

                    if (activeView.LightAtlasBank.CanUse())
                    {
                        if (ImGui.MenuItem($"BTAB"))
                        {
                            CFG.Current.MapEditor_AutomaticSave_IncludeBTAB = !CFG.Current.MapEditor_AutomaticSave_IncludeBTAB;
                        }
                        GUI.Tooltip("If enabled, the light atlas files are outputted on save.");
                        GUI.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeBTAB);
                    }

                    if (activeView.LightProbeBank.CanUse())
                    {
                        if (ImGui.MenuItem($"BTPB"))
                        {
                            CFG.Current.MapEditor_AutomaticSave_IncludeBTPB = !CFG.Current.MapEditor_AutomaticSave_IncludeBTPB;
                        }
                        GUI.Tooltip("If enabled, the light probe files are outputted on save.");
                        GUI.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeBTPB);
                    }
                }

                ImGui.EndMenu();
            }
            GUI.Tooltip("Determines which files are outputted during the automatic saving process.");

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Edit"))
        {
            if (activeView != null)
            {
                // Undo
                if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
                {
                    if (activeView.ViewportActionManager.CanUndo())
                    {
                        activeView.ViewportActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"Undo All"))
                {
                    if (activeView.ViewportActionManager.CanUndo())
                    {
                        activeView.ViewportActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
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
                activeView.RenderTypeAction.OnMenu();

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
        // Dropdown: View
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Tools"))
            {
                CFG.Current.Interface_MapEditor_ToolWindow = !CFG.Current.Interface_MapEditor_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_ToolWindow);

            if (ImGui.MenuItem("Resource List"))
            {
                CFG.Current.Interface_MapEditor_ResourceList = !CFG.Current.Interface_MapEditor_ResourceList;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_ResourceList);

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
        if (ImGui.BeginMenu("Filters", validViewportState))
        {
            activeView.BasicFilters.Display();

            ImGui.Separator();

            if (ImGui.BeginMenu("Filter Presets"))
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
            if (ImGui.BeginMenu("Region Visibility", validViewportState))
            {
                activeView.RegionFilters.DisplayOptions();

                ImGui.EndMenu();
            }

            // Collision Filters
            if (ImGui.BeginMenu("Collision Visibility", validViewportState))
            {
                CollisionMenu();

                ImGui.EndMenu();
            }

            // Patrol Routes
            if (ImGui.BeginMenu("Patrol Route Visibility", validViewportState))
            {
                if (activeView.Project.Descriptor.ProjectType != ProjectType.DS2S && activeView.Project.Descriptor.ProjectType != ProjectType.DS2)
                {
                    if (ImGui.MenuItem("Display"))
                    {
                        activeView.PatrolDrawManager.Generate();
                        activeView.DelayPicking();
                    }
                    GUI.Tooltip("Display the connections between patrol route nodes.");

                    if (ImGui.MenuItem("Clear"))
                    {
                        activeView.PatrolDrawManager.Clear();
                        activeView.DelayPicking();
                    }
                    GUI.Tooltip("Clear the display of connections between patrol route nodes.");
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

        if (ImGui.MenuItem("Low"))
        {
            activeView.HavokCollisionBank.VisibleCollisionType = HavokCollisionType.Low;
            CFG.Current.CurrentHavokCollisionType = HavokCollisionType.Low;

            activeView.HavokCollisionBank.RefreshCollision();
            activeView.DelayPicking();
        }
        GUI.Tooltip("Visible collision will use the low-detail mesh.\nUsed for standard collision.");
        GUI.ShowActiveStatus(activeView.HavokCollisionBank.VisibleCollisionType == HavokCollisionType.Low);

        if (ImGui.MenuItem("High"))
        {
            activeView.HavokCollisionBank.VisibleCollisionType = HavokCollisionType.High;
            CFG.Current.CurrentHavokCollisionType = HavokCollisionType.High;

            activeView.HavokCollisionBank.RefreshCollision();
            activeView.DelayPicking();
        }
        GUI.Tooltip("Visible collision will use the high-detail mesh.\nUsed for IK.");
        GUI.ShowActiveStatus(activeView.HavokCollisionBank.VisibleCollisionType == HavokCollisionType.High);

        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            if (ImGui.MenuItem("Fall Protection"))
            {
                activeView.HavokCollisionBank.VisibleCollisionType = HavokCollisionType.FallProtection;
                CFG.Current.CurrentHavokCollisionType = HavokCollisionType.FallProtection;

                activeView.HavokCollisionBank.RefreshCollision();
                activeView.DelayPicking();
            }
            GUI.Tooltip("Visible collision will use the fall-protection mesh.\nUsed for enemy fall protection.");
            GUI.ShowActiveStatus(activeView.HavokCollisionBank.VisibleCollisionType == HavokCollisionType.FallProtection);
        }
    }



    public void OptionsMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Options"))
        {
            if (ImGui.BeginMenu("Map List"))
            {
                if (ImGui.MenuItem("Unload Current"))
                {
                    DialogResult result = PlatformUtils.Instance.MessageBox("Unload current?", "Confirm", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        activeView.Universe.UnloadMap(activeView.Selection.SelectedMapID);
                    }
                }
                GUI.Tooltip("Unload the currently loaded and selected map.");

                if (ImGui.MenuItem("Unload All"))
                {
                    DialogResult result = PlatformUtils.Instance.MessageBox("Unload all maps?", "Confirm", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        activeView.Universe.UnloadAllMaps();
                    }
                }
                GUI.Tooltip("Unload all loaded maps.");

                if (ImGui.BeginMenu("List Filters"))
                {
                    if (ImGui.BeginMenu("Select"))
                    {
                        activeView.MapListFilterTool.SelectionMenu();
                        ImGui.EndMenu();
                    }
                    GUI.Tooltip("Select an existing list filter to apply to the map list.");

                    if (ImGui.MenuItem("Clear"))
                    {
                        activeView.MapListFilterTool.Clear();
                    }
                    GUI.Tooltip("Clear the current list filter, resetting the filtering of the map list.");

                    ImGui.Separator();

                    if (ImGui.BeginMenu("Create"))
                    {
                        activeView.MapListFilterTool.CreationMenu();
                        ImGui.EndMenu();
                    }
                    GUI.Tooltip("Create a new list filter. The filter terms support regular expressions.");

                    if (ImGui.BeginMenu("Edit"))
                    {
                        activeView.MapListFilterTool.EditMenu();
                        ImGui.EndMenu();
                    }
                    GUI.Tooltip("Edit an existing list filter.");

                    if (ImGui.BeginMenu("Delete"))
                    {
                        activeView.MapListFilterTool.DeleteMenu();
                        ImGui.EndMenu();
                    }
                    GUI.Tooltip("Delete an existing list filter.");

                    ImGui.EndMenu();
                }
                GUI.Tooltip("Select a list filter to narrow the map list down to a pre-defined set of maps.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Contents"))
            {
                if (ImGui.BeginMenu("Content Display"))
                {
                    if (ImGui.MenuItem("Tree"))
                    {
                        activeView.MapContentView.ContentViewType = MapContentViewType.ObjectType;
                    }
                    GUI.Tooltip("Display the content in the object type tree form.");
                    GUI.ShowActiveStatus(activeView.MapContentView.ContentViewType == MapContentViewType.ObjectType);

                    if (ImGui.MenuItem("Flat"))
                    {
                        activeView.MapContentView.ContentViewType = MapContentViewType.Flat;
                    }
                    GUI.Tooltip("Display the content in the flat form.");
                    GUI.ShowActiveStatus(activeView.MapContentView.ContentViewType == MapContentViewType.Flat);

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Name Display"))
                {
                    var curType = CFG.Current.MapEditor_MapObjectName_DisplayType;

                    if (ImGui.MenuItem("Internal"))
                    {
                        CFG.Current.MapEditor_MapObjectName_DisplayType = MapObjectNameDisplayType.Internal;
                    }
                    GUI.Tooltip("Display the internal map object name only.");
                    GUI.ShowActiveStatus(curType == MapObjectNameDisplayType.Internal);

                    if (ImGui.MenuItem("Internal + Text"))
                    {
                        CFG.Current.MapEditor_MapObjectName_DisplayType = MapObjectNameDisplayType.Internal_FMG;
                    }
                    GUI.Tooltip("Display the internal map object name with the associated FMG name as the alias.");
                    GUI.ShowActiveStatus(curType == MapObjectNameDisplayType.Internal_FMG);

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            //if (activeView.LightAtlasBank.CanUse())
            //{
            //    if (ImGui.BeginMenu("Light Atlases"))
            //    {
            //        if (ImGui.BeginMenu("Light Atlases"))
            //        {
            //            if (ImGui.MenuItem("Automatically adjust entries"))
            //            {
            //                CFG.Current.MapEditor_LightAtlas_AutomaticAdjust = !CFG.Current.MapEditor_LightAtlas_AutomaticAdjust;
            //            }
            //            UIHelper.Tooltip("If enabled, when a part is renamed, if a light atlas entry points to it, the name reference within the entry is updated to the new name.");
            //            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticAdjust);


            //            if (ImGui.MenuItem("Automatically add entries"))
            //            {
            //                CFG.Current.MapEditor_LightAtlas_AutomaticAdd = !CFG.Current.MapEditor_LightAtlas_AutomaticAdd;
            //            }
            //            UIHelper.Tooltip("If enabled, when new parts are duplicated, the a new light atlas entry pointing to the newly duplicated part is created (deriving the other properties from the source part).");
            //            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticAdd);

            //            if (ImGui.MenuItem("Automatically delete entries"))
            //            {
            //                CFG.Current.MapEditor_LightAtlas_AutomaticDelete = !CFG.Current.MapEditor_LightAtlas_AutomaticDelete;
            //            }
            //            UIHelper.Tooltip("If enabled, when parts are deleted, if there is a light atlas entry pointing to that part, the entry is deleted.");
            //            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_LightAtlas_AutomaticDelete);

            //            ImGui.EndMenu();
            //        }

            //        ImGui.EndMenu();
            //    }
            //}

            ImGui.EndMenu();
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (ViewHandler.ViewToClose == null)
        {
            foreach (var view in ViewHandler.Views)
            {
                if (view == null)
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

        // Throw any exceptions that ocurred during async map loading.
        if (activeView.Universe.LoadMapExceptions != null)
        {
            activeView.Universe.LoadMapExceptions.Throw();
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
            // NOTE: perhaps this should only save the loaded map for the active view (currently does all loaded maps)
            activeView.Universe.SaveAllMaps(autoSave);
        }
        catch (SavingFailedException e)
        {
            HandleSaveException(e);
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }


    public void ReloadUniverse()
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        activeView.Universe.UnloadAllMaps();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        activeView.CreateAction.PopulateClassNames();
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

            DialogResult result = PlatformUtils.Instance.MessageBox($"{eRef.Message}\nSelect referring map entity?",
                "Failed to save map",
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

                Smithbox.Log(this, $"Unable to find map entity \"{eRef.Referrer.Name}\"",
                    LogLevel.Error, LogPriority.High);
            }
        }
        else
        {
            Smithbox.Log(this, e.Message,
                LogLevel.Error, LogPriority.High, e.Wrapped);
        }
    }
}
