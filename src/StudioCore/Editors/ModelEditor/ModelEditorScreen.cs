using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Core;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Interface;
using StudioCore.Resource;
using StudioCore.Scene;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Viewport = StudioCore.Interface.Viewport;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorScreen : EditorScreen
{
    public ViewportActionManager EditorActionManager = new();

    public ViewportSelection _selection = new();

    public ModelUniverse _universe;

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

    public ModelShortcuts EditorShortcuts;
    public ModelCommandQueue CommandQueue;
    public EditorFocusManager FocusManager;
    public ModelAssetCopyManager AssetCopyManager;

    public FileSelectionView FileSelection;
    public InternalFileSelectionView InternalFileSelection;
    public FlverDataSelectionView FlverDataSelection;
    public ModelPropertyView ModelPropertyEditor;

    public GxDescriptorBank GxItemDescriptors;

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

        _universe = new ModelUniverse(RenderScene, _selection);

        // Order matters here as classes may fill references via Screen composition
        ViewportManager = new ModelViewportManager(this, Viewport);
        Selection = new ModelSelectionManager(this);
        ToolView = new ModelToolView(this);
        ResManager = new ModelResourceManager(this, Viewport);
        ContextMenu = new ModelContextMenu(this);
        Decorator = new ModelPropertyDecorator(this);
        CommandQueue = new ModelCommandQueue(this);
        GxItemDescriptors = new GxDescriptorBank(this);

        ActionHandler = new ModelActionHandler(this);
        Filters = new ModelFilters(this);
        ToolMenubar = new ModelToolMenubar(this);

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

    public void EditDropdown()
    {
        if (!CFG.Current.EnableEditor_FLVER)
            return;

        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"Undo All"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Create", KeyBindings.Current.CORE_CreateNewEntry.HintText))
            {
                ActionHandler.CreateHandler();
            }
            UIHelper.ShowHoverTooltip($"Adds new entry based on current selection in Model Hierarchy.");

            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                ActionHandler.DuplicateHandler();
            }
            UIHelper.ShowHoverTooltip($"Duplicates current selection in Model Hierarchy.");

            if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                ActionHandler.DeleteHandler();
            }
            UIHelper.ShowHoverTooltip($"Deletes current selection in Model Hierarchy.");

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void ViewDropdown()
    {
        if (!CFG.Current.EnableEditor_FLVER)
            return;

        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Viewport"))
            {
                UI.Current.Interface_Editor_Viewport = !UI.Current.Interface_Editor_Viewport;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_Editor_Viewport);

            if (ImGui.MenuItem("Model Hierarchy"))
            {
                UI.Current.Interface_ModelEditor_ModelHierarchy = !UI.Current.Interface_ModelEditor_ModelHierarchy;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_ModelHierarchy);

            if (ImGui.MenuItem("Properties"))
            {
                UI.Current.Interface_ModelEditor_Properties = !UI.Current.Interface_ModelEditor_Properties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_Properties);

            if (ImGui.MenuItem("Asset Browser"))
            {
                UI.Current.Interface_ModelEditor_AssetBrowser = !UI.Current.Interface_ModelEditor_AssetBrowser;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_AssetBrowser);

            if (ImGui.MenuItem("Tool Window"))
            {
                UI.Current.Interface_ModelEditor_ToolConfigurationWindow = !UI.Current.Interface_ModelEditor_ToolConfigurationWindow;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_ToolConfigurationWindow);

            if (ImGui.MenuItem("Profiling"))
            {
                UI.Current.Interface_Editor_Profiling = !UI.Current.Interface_Editor_Profiling;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_Editor_Profiling);

            if (ImGui.MenuItem("Resource List"))
            {
                UI.Current.Interface_ModelEditor_ResourceList = !UI.Current.Interface_ModelEditor_ResourceList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_ResourceList);

            if (ImGui.MenuItem("Viewport Grid"))
            {
                UI.Current.Interface_ModelEditor_Viewport_Grid = !UI.Current.Interface_ModelEditor_Viewport_Grid;
                CFG.Current.ModelEditor_Viewport_RegenerateMapGrid = true;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ModelEditor_Viewport_Grid);

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }
    /// <summary>
    /// Handle the editor menubar
    /// </summary>
    public void EditorUniqueDropdowns()
    {
        if (!CFG.Current.EnableEditor_FLVER)
            return;

        ToolMenubar.DisplayMenu();

        ImGui.Separator();

        if (ImGui.BeginMenu("Filters", RenderScene != null && Viewport != null))
        {
            ModelContainer container = _universe.LoadedModelContainer;

            if (ImGui.MenuItem("Meshes"))
            {
                CFG.Current.ModelEditor_ViewMeshes = !CFG.Current.ModelEditor_ViewMeshes;

                if (container != null)
                {
                    foreach (var entry in container.Mesh_RootNode.Children)
                    {
                        entry.EditorVisible = CFG.Current.ModelEditor_ViewMeshes;
                    }
                }
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ViewMeshes);
            UIHelper.ShowHoverTooltip("Only applies on model reload.");

            if (ImGui.MenuItem("Dummy Polygons"))
            {
                CFG.Current.ModelEditor_ViewDummyPolys = !CFG.Current.ModelEditor_ViewDummyPolys;

                if (container != null)
                {
                    foreach (var entry in container.DummyPoly_RootNode.Children)
                    {
                        entry.EditorVisible = CFG.Current.ModelEditor_ViewDummyPolys;
                    }
                }
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ViewDummyPolys);

            if (ImGui.MenuItem("Bones"))
            {
                CFG.Current.ModelEditor_ViewBones = !CFG.Current.ModelEditor_ViewBones;

                if (container != null)
                {
                    foreach (var entry in container.Bone_RootNode.Children)
                    {
                        entry.EditorVisible = CFG.Current.ModelEditor_ViewBones;
                    }
                }
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ViewBones);

            // Collision
            if (Smithbox.ProjectType is ProjectType.ER)
            {
                // High
                if (ImGui.MenuItem("Collision (High)"))
                {
                    CFG.Current.ModelEditor_ViewHighCollision = !CFG.Current.ModelEditor_ViewHighCollision;
                }
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ViewHighCollision);
                UIHelper.ShowHoverTooltip("Only applies on model reload.");

                // Low
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
            if (ImGui.BeginMenu("Mode"))
            {
                if (ImGui.MenuItem("Translate", KeyBindings.Current.VIEWPORT_GizmoTranslationMode.HintText))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Translate;
                }
                UIHelper.ShowHoverTooltip($"Set the gizmo to Translation mode.");

                if (ImGui.MenuItem("Rotate", KeyBindings.Current.VIEWPORT_GizmoRotationMode.HintText))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Rotate;
                }
                UIHelper.ShowHoverTooltip($"Set the gizmo to Rotation mode.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Space"))
            {
                if (ImGui.MenuItem("Local", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.Local;
                }
                UIHelper.ShowHoverTooltip($"Place the gizmo origin based on the selection's local position.");

                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.World;
                }
                UIHelper.ShowHoverTooltip($"Place the gizmo origin based on the selection's world position.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Origin"))
            {
                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.World;
                }
                UIHelper.ShowHoverTooltip($"Orient the gizmo origin based on the world position.");

                if (ImGui.MenuItem("Bounding Box", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.BoundingBox;
                }
                UIHelper.ShowHoverTooltip($"Orient the gizmo origin based on the bounding box.");

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
        if (!CFG.Current.EnableEditor_FLVER)
            return;

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

        if (UI.Current.Interface_ModelEditor_ToolConfigurationWindow)
        {
            ToolView.OnGui();
        }

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
        if (!CFG.Current.EnableEditor_FLVER)
            return;

        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            Selection.OnProjectChanged();
            ResManager.OnProjectChanged();

            FileSelection.OnProjectChanged();
            InternalFileSelection.OnProjectChanged();
            FlverDataSelection.OnProjectChanged();

            ToolView.OnProjectChanged();
            ToolMenubar.OnProjectChanged();
        }

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
        if (!CFG.Current.EnableEditor_FLVER)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (Smithbox.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor is not supported for DES.", LogLevel.Warning);
            return;
        }

        ResManager.SaveModel();
    }

    public void SaveAll()
    {
        if (!CFG.Current.EnableEditor_FLVER)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (Smithbox.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor saving is not supported for DES.", LogLevel.Warning);
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
        if (!CFG.Current.EnableEditor_FLVER)
            return;

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
        if (!CFG.Current.EnableEditor_FLVER)
            return;

        if (Viewport != null)
        {
            Viewport.Draw(device, cl);
        }
    }

}
