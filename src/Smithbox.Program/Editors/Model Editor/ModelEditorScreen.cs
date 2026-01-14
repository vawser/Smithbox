using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    /// <summary>
    /// Lock variable used to handle pauses to the Update() function.
    /// </summary>
    private static readonly object _lock_PauseUpdate = new();
    private bool GCNeedsCollection;
    private bool _PauseUpdate;

    public ViewportActionManager EditorActionManager = new();
    public ModelActionHandler ActionHandler;
    public ViewportSelection ViewportSelection = new();
    public ModelSelection Selection;
    public ModelUniverse Universe;
    public ModelEntityTypeCache EntityTypeCache;
    public EditorFocusManager FocusManager;
    public ModelPropertyCache ModelPropertyCache = new();
    public ModelCommandQueue CommandQueue;
    public ModelShortcuts Shortcuts;

    public ModelViewportView ModelViewportView;
    public ModelSourceView ModelSourceView;
    public ModelSelectView ModelSelectView;
    public ModelContentView ModelContentView;
    public ModelPropertyView ModelPropertyView;
    public ModelToolWindow ModelToolView;

    public ModelViewportFilters ViewportFilters;

    public ModelGridConfiguration ModelGridTool;
    public ModelInsightView ModelInsightTool;
    public ModelInstanceFinder ModelInstanceFinder;
    public ModelMaskToggler ModelMaskToggler;

    public CreateAction CreateAction;
    public DuplicateAction DuplicateAction;
    public DeleteAction DeleteAction;
    public FrameAction FrameAction;
    public GotoAction GotoAction;
    public PullToCameraAction PullToCameraAction;
    public ReorderAction ReorderAction;

    public ModelEditorScreen(ProjectEntry project)
    {
        Project = project;

        ModelViewportView = new ModelViewportView(this, project);
        ModelViewportView.Setup();

        Universe = new ModelUniverse(this, project);
        FocusManager = new EditorFocusManager(this);
        EntityTypeCache = new(this, project);

        Selection = new(this, project);

        // Core Views
        ModelSourceView = new ModelSourceView(this, project);
        ModelSelectView = new ModelSelectView(this, project);
        ModelContentView = new ModelContentView(this, project);
        ModelPropertyView = new ModelPropertyView(this, project);
        ModelToolView = new ModelToolWindow(this, project);

        ViewportFilters = new ModelViewportFilters(this, project);

        ActionHandler = new ModelActionHandler(this, project);
        CommandQueue = new ModelCommandQueue(this, project);
        Shortcuts = new ModelShortcuts(this, project);

        ModelGridTool = new ModelGridConfiguration(this, Project);
        ModelInsightTool = new ModelInsightView(this, Project);
        ModelInstanceFinder = new ModelInstanceFinder(this, Project);
        ModelMaskToggler = new ModelMaskToggler(this, Project);

        CreateAction = new CreateAction(this, Project);
        DuplicateAction = new DuplicateAction(this, Project);
        DeleteAction = new DeleteAction(this, Project);
        FrameAction = new FrameAction(this, Project);
        GotoAction = new GotoAction(this, Project);
        PullToCameraAction = new PullToCameraAction(this, Project);
        ReorderAction = new ReorderAction(this, Project);

        ModelInsightHelper.Setup(this, Project);

        FocusManager.SetDefaultFocusElement("Properties##modeleditprop");

        EditorActionManager.AddEventHandler(ModelContentView);
    }
    private bool PauseUpdate
    {
        get
        {
            lock (_lock_PauseUpdate)
            {
                return _PauseUpdate;
            }
        }
        set
        {
            lock (_lock_PauseUpdate)
            {
                _PauseUpdate = value;
            }
        }
    }

    public string EditorName => "Model Editor";
    public string CommandEndpoint => "model";
    public string SaveType => "Models";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] initcmd)
    {
        if (Project.IsInitializing)
            return;

        var scale = DPI.UIScale();

        // Docking setup
        //var vp = ImGui.GetMainViewport();
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 0.0f);
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse |
                                 ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        flags |= ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking;
        flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
        flags |= ImGuiWindowFlags.NoBackground;
        //ImGui.Begin("DockSpace_MapEdit", flags);
        ImGui.PopStyleVar(4);
        var dsid = ImGui.GetID("DockSpace_ModelEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0));

        Shortcuts.Monitor();
        CommandQueue.Parse(initcmd);

        // Action OnGUI

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300, 500) * scale, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowPos(new Vector2(20, 20) * scale, ImGuiCond.FirstUseEver);

        Vector3 clear_color = new(114f / 255f, 144f / 255f, 154f / 255f);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolMenu();

            ImGui.EndMenuBar();
        }

        ModelViewportView.OnGui();
        ModelSourceView.OnGui();
        ModelSelectView.OnGui();
        ModelContentView.OnGui();
        ModelToolView.OnGui();

        if (Smithbox.FirstFrame)
        {
            ImGui.SetNextWindowFocus();
        }

        if (ModelPropertyView.Focus)
        {
            ModelPropertyView.Focus = false;
            ImGui.SetNextWindowFocus();
        }

        ModelPropertyView.OnGui();

        ResourceLoadWindow.DisplayWindow(ModelViewportView.Viewport.Width, ModelViewportView.Viewport.Height);

        if (CFG.Current.Interface_MapEditor_ResourceList)
        {
            ResourceListWindow.DisplayWindow("modelResourceList", this);
        }

        ImGui.PopStyleColor(1);

        FocusManager.OnFocus();
    }

    public void OnDefocus()
    {
        FocusManager.ResetFocus();
    }

    public void Update(float dt)
    {
        if (Project.IsInitializing)
            return;

        if (GCNeedsCollection)
        {
            GC.Collect();
            GCNeedsCollection = false;
        }

        if (PauseUpdate)
        {
            return;
        }

        ModelViewportView.Update(dt);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        ModelViewportView.EditorResized(window, device);
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{KeyBindings.Current.CORE_Save.HintText}"))
            {
                Save();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Output on Manual Save"))
            {
                if (ImGui.MenuItem($"FLVER"))
                {
                    CFG.Current.ModelEditor_ManualSave_IncludeFLVER = !CFG.Current.ModelEditor_ManualSave_IncludeFLVER;
                }
                UIHelper.Tooltip("If enabled, the model container files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ManualSave_IncludeFLVER);


                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the manual saving process.");

            if (ImGui.BeginMenu("Output on Automatic Save"))
            {
                if (ImGui.MenuItem($"FLVER"))
                {
                    CFG.Current.ModelEditor_AutomaticSave_IncludeFLVER = !CFG.Current.ModelEditor_AutomaticSave_IncludeFLVER;
                }
                UIHelper.Tooltip("If enabled, the model container files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_AutomaticSave_IncludeFLVER);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the automatic saving process.");


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

            // Actions
            CreateAction.OnMenu();
            DuplicateAction.OnMenu();
            DeleteAction.OnMenu();

            ImGui.Separator();

            FrameAction.OnMenu();
            GotoAction.OnMenu();
            PullToCameraAction.OnMenu();

            ImGui.Separator();

            ReorderAction.OnMenu();

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

            if (ImGui.MenuItem("Source List"))
            {
                CFG.Current.Interface_ModelEditor_ModelSourceList = !CFG.Current.Interface_ModelEditor_ModelSourceList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ModelSourceList);

            if (ImGui.MenuItem("Model List"))
            {
                CFG.Current.Interface_ModelEditor_ModelSelectList = !CFG.Current.Interface_ModelEditor_ModelSelectList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ModelSelectList);

            if (ImGui.MenuItem("Model Contents"))
            {
                CFG.Current.Interface_ModelEditor_ModelContents = !CFG.Current.Interface_ModelEditor_ModelContents;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ModelContents);

            if (ImGui.MenuItem("Model Properties"))
            {
                CFG.Current.Interface_ModelEditor_Properties = !CFG.Current.Interface_ModelEditor_Properties;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Properties);

            if (ImGui.MenuItem("Resource List"))
            {
                CFG.Current.Interface_ModelEditor_ResourceList = !CFG.Current.Interface_ModelEditor_ResourceList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ResourceList);

            ImGui.EndMenu();
        }
    }

    public void ToolMenu()
    {
        ModelToolView.OnMenubar();
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Project.IsInitializing)
            return;

        if (ModelViewportView.Viewport != null)
        {
            ModelViewportView.Draw(device, cl);
        }
    }

    public bool InputCaptured()
    {
        return ModelViewportView.InputCaptured();
    }

    public void Save(bool autoSave = false)
    {
        if (Project.Descriptor.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor is not supported for DES.", LogLevel.Warning);
            return;
        }

        if (!autoSave && CFG.Current.ModelEditor_ManualSave_IncludeFLVER ||
            autoSave && CFG.Current.ModelEditor_AutomaticSave_IncludeFLVER)
        {
            if (Selection.SelectedModelWrapper != null)
            {
                Selection.SelectedModelWrapper.Save();
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    /// <summary>
    /// Re-assigns the drawable meshes for Dummy/Node objects when the sizing changes via the Viewport menu
    /// </summary>
    public void UpdateDisplayNodes()
    {
        var wrapper = Selection.SelectedModelWrapper;
        if (wrapper != null)
        {
            var container = wrapper.Container;

            if (container != null)
            {
                foreach (var obj in container.Dummies)
                {
                    obj.RenderSceneMesh.Dispose();
                    container.AssignDummyDrawable(obj, wrapper);
                }

                foreach (var obj in container.Nodes)
                {
                    obj.RenderSceneMesh.Dispose();
                    container.AssignNodeDrawable(obj, wrapper);
                }
            }
        }
    }
}
