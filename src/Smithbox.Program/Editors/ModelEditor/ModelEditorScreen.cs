using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Core;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Interface;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.ViewportNS;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorScreen : EditorScreen
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

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

    public HavokCollisionManager CollisionManager;

    // public GxDescriptorBank GxItemDescriptors;

    public ModelEditorScreen(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;

        Rect = baseEditor._context.Window.Bounds;
        Window = baseEditor._context.Window;

        if (baseEditor._context.Device != null)
        {
            RenderScene = new RenderScene();
            Viewport = new ViewportNS.Viewport(BaseEditor, null, this, ViewportType.ModelEditor, "Modeleditvp", Rect.Width, Rect.Height);
        }
        else
        {
            Viewport = new NullViewport(BaseEditor, null, this, ViewportType.ModelEditor, "Modeleditvp", Rect.Width, Rect.Height);
        }

        _universe = new ModelUniverse(this, RenderScene, _selection);

        // Order matters here as classes may fill references via Screen composition
        ViewportManager = new ModelViewportManager(this, Viewport);
        Selection = new ModelSelectionManager(this);
        ToolView = new ModelToolView(this);
        ResManager = new ModelResourceManager(this, Viewport);
        ContextMenu = new ModelContextMenu(this);
        Decorator = new ModelPropertyDecorator(this);
        CommandQueue = new ModelCommandQueue(this);
        // GxItemDescriptors = new GxDescriptorBank(this);

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

        CollisionManager = new(this, Project);
    }

    public string EditorName => "Model Editor";
    public string CommandEndpoint => "model";
    public string SaveType => "Models";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    /// <summary>
    /// The editor main loop
    /// </summary>
    public void OnGUI(string[] initcmd)
    {
        if (Project.IsInitializing)
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

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            ImGui.EndMenuBar();
        }

        EditorShortcuts.Monitor();
        CommandQueue.Parse(initcmd);

        Viewport.OnGui();

        FileSelection.Display();
        InternalFileSelection.Display();
        FlverDataSelection.Display();
        ModelPropertyEditor.Display();

        if (CFG.Current.Interface_ModelEditor_ToolWindow)
        {
            ToolView.OnGui();
        }

        ResourceLoadWindow.DisplayWindow(Viewport.Width, Viewport.Height);

        if (CFG.Current.Interface_ModelEditor_ResourceList)
        {
            ResourceListWindow.DisplayWindow("modelResourceList");
        }

        FocusManager.OnFocus();

        ImGui.PopStyleColor(1);
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{KeyBindings.Current.CORE_Save.HintText}"))
            {
                Save();
            }

            if (ImGui.MenuItem($"Save All", $"{KeyBindings.Current.CORE_SaveAll.HintText}"))
            {
                SaveAll();
            }

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
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
            UIHelper.Tooltip($"Adds new entry based on current selection in Model Hierarchy.");

            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                ActionHandler.DuplicateHandler();
            }
            UIHelper.Tooltip($"Duplicates current selection in Model Hierarchy.");

            if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                ActionHandler.DeleteHandler();
            }
            UIHelper.Tooltip($"Deletes current selection in Model Hierarchy.");

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Viewport"))
            {
                CFG.Current.Interface_Editor_Viewport = !CFG.Current.Interface_Editor_Viewport;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_Editor_Viewport);

            if (ImGui.MenuItem("Model Hierarchy"))
            {
                CFG.Current.Interface_ModelEditor_ModelHierarchy = !CFG.Current.Interface_ModelEditor_ModelHierarchy;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ModelHierarchy);

            if (ImGui.MenuItem("Properties"))
            {
                CFG.Current.Interface_ModelEditor_Properties = !CFG.Current.Interface_ModelEditor_Properties;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Properties);

            if (ImGui.MenuItem("Asset Browser"))
            {
                CFG.Current.Interface_ModelEditor_AssetBrowser = !CFG.Current.Interface_ModelEditor_AssetBrowser;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_AssetBrowser);

            if (ImGui.MenuItem("Tool Window"))
            {
                CFG.Current.Interface_ModelEditor_ToolWindow = !CFG.Current.Interface_ModelEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ToolWindow);

            if (ImGui.MenuItem("Resource List"))
            {
                CFG.Current.Interface_ModelEditor_ResourceList = !CFG.Current.Interface_ModelEditor_ResourceList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ResourceList);

            ImGui.EndMenu();
        }
    }

    public void FilterMenu()
    {
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
            UIHelper.Tooltip("Only applies on model reload.");

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
            if (Project.ProjectType is ProjectType.ER)
            {
                // High
                if (ImGui.MenuItem("Collision (High)"))
                {
                    CFG.Current.ModelEditor_ViewHighCollision = !CFG.Current.ModelEditor_ViewHighCollision;
                }
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ViewHighCollision);
                UIHelper.Tooltip("Only applies on model reload.");

                // Low
                if (ImGui.MenuItem("Collision (Low)"))
                {
                    CFG.Current.ModelEditor_ViewLowCollision = !CFG.Current.ModelEditor_ViewLowCollision;
                }
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ViewLowCollision);
                UIHelper.Tooltip("Only applies on model reload.");
            }

            ImGui.EndMenu();
        }
    }

    public void OnDefocus()
    {
        FocusManager.ResetFocus();
    }

    public void Save()
    {
        if (Project.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor is not supported for DES.", LogLevel.Warning);
            return;
        }

        ResManager.SaveModel();

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }

    public void SaveAll()
    {
        if (Project.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor saving is not supported for DES.", LogLevel.Warning);
            return;
        }

        Save(); // Just call save.

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }

    public bool InputCaptured()
    {
        return Viewport.IsViewportSelected;
    }

    public void Update(float dt)
    {
        if (Project.IsInitializing)
            return;

        ViewportUsingKeyboard = Viewport.Update(Window, dt);

        if (ResManager._loadingTask != null && ResManager._loadingTask.IsCompleted)
        {
            ResManager._loadingTask = null;
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        if (Project.IsInitializing)
            return;

        Window = window;
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.IsInitializing)
            return;

        if (Viewport != null)
        {
            Viewport.Draw(device, cl);
        }
    }

}
