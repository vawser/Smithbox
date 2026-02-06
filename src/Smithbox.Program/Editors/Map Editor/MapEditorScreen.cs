using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
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

    public MapToolWindow ToolWindow;

    public ResourceLoadWindow LoadingModal;
    public ResourceListWindow ResourceList;

    public MapEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new(this, project);

        LoadingModal = new();
        ResourceList = new();

        CommandQueue = new MapCommandQueue(this, project);
        Shortcuts = new MapShortcuts(this, project);

        ToolWindow = new MapToolWindow(this, project);
    }

    public string EditorName => "Map Editor";
    public string CommandEndpoint => "map";
    public string SaveType => "Maps";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();

        Shortcuts.Monitor();
        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            ToolWindow.DisplayMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_MapEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0));

        ViewHandler.HandleViews();

        var activeView = ViewHandler.ActiveView;

        if (activeView != null)
        {
            ToolWindow.OnGui();

            LoadingModal.DisplayWindow(activeView.ViewportWindow.Viewport.Width, activeView.ViewportWindow.Viewport.Height);

            if (CFG.Current.Interface_MapEditor_ResourceList)
            {
                ResourceList.DisplayWindow("mapResourceList", activeView.Universe);
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
                UIHelper.Tooltip("If enabled, the map files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeMSB);

                if (ImGui.MenuItem($"BTL"))
                {
                    CFG.Current.MapEditor_ManualSave_IncludeBTL = !CFG.Current.MapEditor_ManualSave_IncludeBTL;
                }
                UIHelper.Tooltip("If enabled, the light files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeBTL);

                if (activeView != null)
                {
                    if (activeView.AutoInvadeBank.CanUse())
                    {
                        if (ImGui.MenuItem($"AIP"))
                        {
                            CFG.Current.MapEditor_ManualSave_IncludeAIP = !CFG.Current.MapEditor_ManualSave_IncludeAIP;
                        }
                        UIHelper.Tooltip("If enabled, the auto invade point files are outputted on save.");
                        UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeAIP);
                    }

                    if (activeView.HavokNavmeshBank.CanUse())
                    {
                        if (ImGui.MenuItem($"NVA"))
                        {
                            CFG.Current.MapEditor_ManualSave_IncludeNVA = !CFG.Current.MapEditor_ManualSave_IncludeNVA;
                        }
                        UIHelper.Tooltip("If enabled, the navmesh configuration files are outputted on save.");
                        UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeNVA);
                    }

                    if (activeView.LightAtlasBank.CanUse())
                    {
                        if (ImGui.MenuItem($"BTAB"))
                        {
                            CFG.Current.MapEditor_ManualSave_IncludeBTAB = !CFG.Current.MapEditor_ManualSave_IncludeBTAB;
                        }
                        UIHelper.Tooltip("If enabled, the light atlas files are outputted on save.");
                        UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeBTAB);
                    }

                    if (activeView.LightProbeBank.CanUse())
                    {
                        if (ImGui.MenuItem($"BTPB"))
                        {
                            CFG.Current.MapEditor_ManualSave_IncludeBTPB = !CFG.Current.MapEditor_ManualSave_IncludeBTPB;
                        }
                        UIHelper.Tooltip("If enabled, the light probe files are outputted on save.");
                        UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ManualSave_IncludeBTPB);
                    }
                }

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the manual saving process.");

            if (ImGui.BeginMenu("Output on Automatic Save"))
            {
                if (ImGui.MenuItem($"MSB"))
                {
                    CFG.Current.MapEditor_AutomaticSave_IncludeMSB = !CFG.Current.MapEditor_AutomaticSave_IncludeMSB;
                }
                UIHelper.Tooltip("If enabled, the map files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeMSB);

                if (ImGui.MenuItem($"BTL"))
                {
                    CFG.Current.MapEditor_AutomaticSave_IncludeBTL = !CFG.Current.MapEditor_AutomaticSave_IncludeBTL;
                }
                UIHelper.Tooltip("If enabled, the light files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeBTL);

                if (activeView != null)
                {
                    if (activeView.AutoInvadeBank.CanUse())
                    {
                        if (ImGui.MenuItem($"AIP"))
                        {
                            CFG.Current.MapEditor_AutomaticSave_IncludeAIP = !CFG.Current.MapEditor_AutomaticSave_IncludeAIP;
                        }
                        UIHelper.Tooltip("If enabled, the auto invade point files are outputted on save.");
                        UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeAIP);
                    }

                    if (activeView.HavokNavmeshBank.CanUse())
                    {
                        if (ImGui.MenuItem($"NVA"))
                        {
                            CFG.Current.MapEditor_AutomaticSave_IncludeNVA = !CFG.Current.MapEditor_AutomaticSave_IncludeNVA;
                        }
                        UIHelper.Tooltip("If enabled, the navmesh configuration files are outputted on save.");
                        UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeNVA);
                    }

                    if (activeView.LightAtlasBank.CanUse())
                    {
                        if (ImGui.MenuItem($"BTAB"))
                        {
                            CFG.Current.MapEditor_AutomaticSave_IncludeBTAB = !CFG.Current.MapEditor_AutomaticSave_IncludeBTAB;
                        }
                        UIHelper.Tooltip("If enabled, the light atlas files are outputted on save.");
                        UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeBTAB);
                    }

                    if (activeView.LightProbeBank.CanUse())
                    {
                        if (ImGui.MenuItem($"BTPB"))
                        {
                            CFG.Current.MapEditor_AutomaticSave_IncludeBTPB = !CFG.Current.MapEditor_AutomaticSave_IncludeBTPB;
                        }
                        UIHelper.Tooltip("If enabled, the light probe files are outputted on save.");
                        UIHelper.ShowActiveStatus(CFG.Current.MapEditor_AutomaticSave_IncludeBTPB);
                    }
                }

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the automatic saving process.");

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
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_ToolWindow);

            if (ImGui.MenuItem("Resource List"))
            {
                CFG.Current.Interface_MapEditor_ResourceList = !CFG.Current.Interface_MapEditor_ResourceList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_ResourceList);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    public void FilterMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        var validViewportState = activeView.RenderScene != null &&
             activeView.ViewportWindow.Viewport != null;

        // General Filters
        if (ImGui.BeginMenu("General Filters", validViewportState))
        {
            activeView.BasicFilters.Display();

            ImGui.Separator();

            if (ImGui.BeginMenu("Filter Presets"))
            {
                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_1.Name))
                {
                    activeView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_1.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_2.Name))
                {
                    activeView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_2.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_3.Name))
                {
                    activeView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_3.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_4.Name))
                {
                    activeView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_4.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_5.Name))
                {
                    activeView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_5.Filters;
                }

                if (ImGui.MenuItem(CFG.Current.Viewport_Filter_Preset_6.Name))
                {
                    activeView.RenderScene.DrawFilter = CFG.Current.Viewport_Filter_Preset_6.Filters;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        // Region Filters
        if (ImGui.BeginMenu("Region Filters", validViewportState))
        {
            activeView.RegionFilters.DisplayOptions();

            ImGui.EndMenu();
        }
    }

    public void CollisionMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        var validViewportState = activeView.RenderScene != null &&
            activeView.ViewportWindow.Viewport != null;

        if (ImGui.BeginMenu("Collision Type", validViewportState))
        {
            if (ImGui.MenuItem("Low"))
            {
                activeView.HavokCollisionBank.VisibleCollisionType = HavokCollisionType.Low;
                CFG.Current.CurrentHavokCollisionType = HavokCollisionType.Low;

                activeView.HavokCollisionBank.RefreshCollision();
            }
            UIHelper.Tooltip("Visible collision will use the low-detail mesh.\nUsed for standard collision.");
            UIHelper.ShowActiveStatus(activeView.HavokCollisionBank.VisibleCollisionType == HavokCollisionType.Low);

            if (ImGui.MenuItem("High"))
            {
                activeView.HavokCollisionBank.VisibleCollisionType = HavokCollisionType.High;
                CFG.Current.CurrentHavokCollisionType = HavokCollisionType.High;

                activeView.HavokCollisionBank.RefreshCollision();
            }
            UIHelper.Tooltip("Visible collision will use the high-detail mesh.\nUsed for IK.");
            UIHelper.ShowActiveStatus(activeView.HavokCollisionBank.VisibleCollisionType == HavokCollisionType.High);

            if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
            {
                if (ImGui.MenuItem("Fall Protection"))
                {
                    activeView.HavokCollisionBank.VisibleCollisionType = HavokCollisionType.FallProtection;
                    CFG.Current.CurrentHavokCollisionType = HavokCollisionType.FallProtection;

                    activeView.HavokCollisionBank.RefreshCollision();
                }
                UIHelper.Tooltip("Visible collision will use the fall-protection mesh.\nUsed for enemy fall protection.");
                UIHelper.ShowActiveStatus(activeView.HavokCollisionBank.VisibleCollisionType == HavokCollisionType.FallProtection);
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
                if (view == null)
                    continue;

                if (view.ViewportWindow.Viewport is VulkanViewport vulkanViewport)
                {
                    if (vulkanViewport.Visible)
                    {
                        view.ViewportWindow.Draw(device, cl);
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
            TaskLogs.AddLog(e.Message,
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

                TaskLogs.AddLog($"Unable to find map entity \"{eRef.Referrer.Name}\"",
                    LogLevel.Error, LogPriority.High);
            }
        }
        else
        {
            TaskLogs.AddLog(e.Message,
                LogLevel.Error, LogPriority.High, e.Wrapped);
        }
    }
}
