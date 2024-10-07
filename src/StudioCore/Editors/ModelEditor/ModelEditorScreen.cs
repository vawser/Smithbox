using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Resource;
using StudioCore.Scene;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Viewport = StudioCore.Interface.Viewport;
using StudioCore.Configuration;
using StudioCore.MsbEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Core.Project;
using StudioCore.Interface;
using System.Xml;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Editors.ModelEditor.Core;
using StudioCore.Editors.ModelEditor.Core.Properties;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorScreen : EditorScreen
{
    public MapEditor.ViewportActionManager EditorActionManager = new();

    public ViewportSelection _selection = new();

    public Universe _universe;

    public Rectangle Rect;
    public RenderScene RenderScene;
    public IViewport Viewport;

    public bool ViewportUsingKeyboard;
    public Sdl2Window Window;

    public ModelSelectionManager Selection;
    public ModelContextMenu ContextMenu;
    public ModelPropertyDecorator Decorator;

    public ModelResourceManager ResManager;
    public ModelViewportManager ViewportManager;

    public ModelActionHandler ActionHandler;
    public ModelFilters Filters;

    public ModelToolView ToolView;
    public ModelToolMenubar ToolMenubar;
    public ModelActionMenubar ActionMenubar;

    public ModelShortcuts EditorShortcuts;
    public ModelCommandQueue CommandQueue;
    public EditorFocusManager FocusManager;
    public ModelAssetCopyManager AssetCopyManager;

    public FileSelectionView FileSelection;
    public InternalFileSelectionView InternalFileSelection;
    public FlverDataSelectionView FlverDataSelection;
    public ModelPropertyView ModelPropertyEditor;

    public ModelEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        Rect = window.Bounds;
        Window = window;

        if (device != null)
        {
            RenderScene = new RenderScene();
            Viewport = new Viewport(ViewportType.ModelEditor, "Modeleditvp", device, RenderScene, EditorActionManager, _selection, Rect.Width, Rect.Height);
        }
        else
        {
            Viewport = new NullViewport(ViewportType.ModelEditor, "Modeleditvp", EditorActionManager, _selection, Rect.Width, Rect.Height);
        }

        _universe = new Universe(RenderScene, _selection);

        // Order matters here as classes may fill references via Screen composition
        ToolView = new ModelToolView(this);
        ViewportManager = new ModelViewportManager(this, Viewport);
        Selection = new ModelSelectionManager(this);
        ResManager = new ModelResourceManager(this, Viewport);
        ContextMenu = new ModelContextMenu(this);
        Decorator = new ModelPropertyDecorator(this);
        CommandQueue = new ModelCommandQueue(this);

        ActionHandler = new ModelActionHandler(this);
        Filters = new ModelFilters(this);
        ToolMenubar = new ModelToolMenubar(this);
        ActionMenubar = new ModelActionMenubar(this);

        EditorShortcuts = new ModelShortcuts(this);
        AssetCopyManager = new ModelAssetCopyManager(this);
        FocusManager = new EditorFocusManager(this);
        FocusManager.SetDefaultFocusElement("Properties##ModelEditorProperties");

        FileSelection = new FileSelectionView(this);
        InternalFileSelection = new InternalFileSelectionView(this);
        FlverDataSelection = new FlverDataSelectionView(this);
        ModelPropertyEditor = new ModelPropertyView(this);
    }

    public string EditorName => "Model Editor";
    public string CommandEndpoint => "model";
    public string SaveType => "Models";

    /// <summary>
    /// Handle the editor menubar
    /// </summary>
    public void DrawEditorMenu()
    {
        ImGui.Separator();

        if (ImGui.BeginMenu("Edit"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo All", "", false,
                    EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}", false,
                    EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        ActionMenubar.DisplayMenu();

        ImGui.Separator();

        ToolMenubar.DisplayMenu();

        ImGui.Separator();

        if (ImGui.BeginMenu("View"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Viewport"))
            {
                UI.Current.Interface_Editor_Viewport = !UI.Current.Interface_Editor_Viewport;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_Editor_Viewport);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Model Hierarchy"))
            {
                UI.Current.Interface_ModelEditor_ModelHierarchy = !UI.Current.Interface_ModelEditor_ModelHierarchy;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_ModelHierarchy);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Properties"))
            {
                UI.Current.Interface_ModelEditor_Properties = !UI.Current.Interface_ModelEditor_Properties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_Properties);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Asset Browser"))
            {
                UI.Current.Interface_ModelEditor_AssetBrowser = !UI.Current.Interface_ModelEditor_AssetBrowser;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_AssetBrowser);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Tool Window"))
            {
                UI.Current.Interface_ModelEditor_ToolConfigurationWindow = !UI.Current.Interface_ModelEditor_ToolConfigurationWindow;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_ToolConfigurationWindow);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Profiling"))
            {
                UI.Current.Interface_Editor_Profiling = !UI.Current.Interface_Editor_Profiling;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_Editor_Profiling);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Resource List"))
            {
                UI.Current.Interface_ModelEditor_ResourceList = !UI.Current.Interface_ModelEditor_ResourceList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_ResourceList);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Viewport Grid"))
            {
                UI.Current.Interface_ModelEditor_Viewport_Grid = !UI.Current.Interface_ModelEditor_Viewport_Grid;
                CFG.Current.ModelEditor_Viewport_RegenerateMapGrid = true;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_Viewport_Grid);

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Filters", RenderScene != null && Viewport != null))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Meshes"))
            {
                CFG.Current.ModelEditor_ViewMeshes = !CFG.Current.ModelEditor_ViewMeshes;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ViewMeshes);
            UIHelper.ShowHoverTooltip("Only applies on model reload.");

            UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Dummy Polygons"))
            {
                CFG.Current.ModelEditor_ViewDummyPolys = !CFG.Current.ModelEditor_ViewDummyPolys;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ViewDummyPolys);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
            if (ImGui.MenuItem("Bones"))
            {
                CFG.Current.ModelEditor_ViewBones = !CFG.Current.ModelEditor_ViewBones;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ViewBones);

            // Collision
            if (Smithbox.ProjectType is ProjectType.ER)
            {
                // High
                UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
                if (ImGui.MenuItem("Collision (High)"))
                {
                    CFG.Current.ModelEditor_ViewHighCollision = !CFG.Current.ModelEditor_ViewHighCollision;
                }
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ViewHighCollision);
                UIHelper.ShowHoverTooltip("Only applies on model reload.");

                // Low
                UIHelper.ShowMenuIcon($"{ForkAwesome.Eye}");
                if (ImGui.MenuItem("Collision (Low)"))
                {
                    CFG.Current.ModelEditor_ViewLowCollision = !CFG.Current.ModelEditor_ViewLowCollision;
                }
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ViewLowCollision);
                UIHelper.ShowHoverTooltip("Only applies on model reload.");
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Viewport"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.LightbulbO}");
            if (ImGui.BeginMenu("Scene Lighting"))
            {
                Viewport.SceneParamsGui();
                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Gizmos"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Compass}");
            if (ImGui.BeginMenu("Mode"))
            {
                if (ImGui.MenuItem("Translate", KeyBindings.Current.VIEWPORT_GizmoTranslationMode.HintText,
                        Gizmos.Mode == Gizmos.GizmosMode.Translate))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Translate;
                }

                if (ImGui.MenuItem("Rotate", KeyBindings.Current.VIEWPORT_GizmoRotationMode.HintText,
                        Gizmos.Mode == Gizmos.GizmosMode.Rotate))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Rotate;
                }

                ImGui.EndMenu();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Cube}");
            if (ImGui.BeginMenu("Space"))
            {
                if (ImGui.MenuItem("Local", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText,
                        Gizmos.Space == Gizmos.GizmosSpace.Local))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.Local;
                }

                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText,
                        Gizmos.Space == Gizmos.GizmosSpace.World))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.World;
                }

                ImGui.EndMenu();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Cubes}");
            if (ImGui.BeginMenu("Origin"))
            {
                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText,
                        Gizmos.Origin == Gizmos.GizmosOrigin.World))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.World;
                }

                if (ImGui.MenuItem("Bounding Box", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText,
                        Gizmos.Origin == Gizmos.GizmosOrigin.BoundingBox))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.BoundingBox;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// The editor main loop
    /// </summary>
    public void OnGUI(string[] initcmd)
    {
        var scale = DPI.GetUIScale();

        // Docking setup
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);
        var dsid = ImGui.GetID("DockSpace_ModelEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0));

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300, 500) * scale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(20, 20) * scale, ImGuiCond.FirstUseEver);

        EditorShortcuts.Monitor();
        CommandQueue.Parse(initcmd);

        Viewport.OnGui();

        FileSelection.Display();
        InternalFileSelection.Display();
        FlverDataSelection.Display();
        ModelPropertyEditor.Display();

        ResManager.UpdateModelContainer();

        ResourceLoadWindow.DisplayWindow(Viewport.Width, Viewport.Height);

        if (UI.Current.Interface_ModelEditor_ResourceList)
        {
            ResourceListWindow.DisplayWindow("modelResourceList");
        }

        FocusManager.OnFocus();

        ImGui.PopStyleColor(1);
    }

    /// <summary>
    /// Handle the editor state on project change
    /// </summary>
    public void OnProjectChanged()
    {
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            Selection.OnProjectChanged();

            FileSelection.OnProjectChanged();
            InternalFileSelection.OnProjectChanged();
            FlverDataSelection.OnProjectChanged();

            ToolView.OnProjectChanged();
            ToolMenubar.OnProjectChanged();
            ActionMenubar.OnProjectChanged();
            ViewportManager.OnProjectChanged();
        }

        ResManager.OnProjectChange();
        _universe.UnloadAll(true);
    }

    /// <summary>
    /// Handle the editor defocus state
    /// </summary>
    public void OnDefocus()
    {
        FocusManager.ResetFocus();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (Smithbox.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor is not supported for DES.");
            return;
        }

        ResManager.SaveModel();
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (Smithbox.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor saving is not supported for DES.");
            return;
        }

        Save(); // Just call save.
    }

    public bool InputCaptured()
    {
        return Viewport.ViewportSelected;
    }

    public void Update(float dt)
    {
        ViewportUsingKeyboard = Viewport.Update(Window, dt);

        if (ResManager._loadingTask != null && ResManager._loadingTask.IsCompleted)
        {
            ResManager._loadingTask = null;
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Window = window;
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Viewport != null)
        {
            Viewport.Draw(device, cl);
        }
    }

}
