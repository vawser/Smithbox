using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public ModelViewHandler ViewHandler;

    public ModelCommandQueue CommandQueue;
    public ModelShortcuts Shortcuts;

    public ModelToolWindow ToolMenu;

    public ResourceLoadWindow LoadingModal;
    public ResourceListWindow ResourceList;

    public ModelEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new ModelViewHandler(this, project);

        CommandQueue = new ModelCommandQueue(this, project);
        Shortcuts = new ModelShortcuts(this, project);

        LoadingModal = new();
        ResourceList = new();

        ToolMenu = new ModelToolWindow(this, project);
    }

    public string EditorName => "Model Editor";
    public string CommandEndpoint => "model";
    public string SaveType => "Models";
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
            ToolMenu.OnMenubar();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_ModelEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0));

        ViewHandler.HandleViews();

        var activeView = ViewHandler.ActiveView;

        if (activeView != null)
        {
            ToolMenu.Display();

            LoadingModal.DisplayWindow(activeView.ViewportWindow.Viewport.Width, activeView.ViewportWindow.Viewport.Height);

            if (CFG.Current.Interface_ModelEditor_ResourceList)
            {
                ResourceList.DisplayWindow("modelResourceList", activeView.Universe);
            }
        }
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{InputManager.GetHint(KeybindID.Save)}"))
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
        var activeView = ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Edit"))
        {
            if (activeView != null)
            {
                // Undo
                if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(KeybindID.Undo)}  /  {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"Undo All"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)}  /  {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanRedo())
                    {
                        activeView.ActionManager.RedoAction();
                    }
                }

                ImGui.Separator();

                // Actions
                ToolMenu.CreateAction.OnMenu();
                ToolMenu.DuplicateAction.OnMenu();
                ToolMenu.DeleteAction.OnMenu();

                ImGui.Separator();

                ToolMenu.FrameAction.OnMenu();
                ToolMenu.GotoAction.OnMenu();
                ToolMenu.PullToCameraAction.OnMenu();

                ImGui.Separator();

                ToolMenu.ReorderAction.OnMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Tools"))
            {
                CFG.Current.Interface_ModelEditor_ToolWindow = !CFG.Current.Interface_ModelEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ToolWindow);

            if (ImGui.MenuItem("Resource List"))
            {
                CFG.Current.Interface_ModelEditor_ResourceList = !CFG.Current.Interface_ModelEditor_ResourceList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ResourceList);

            if (ImGui.MenuItem("Screenshot Mode"))
            {
                CFG.Current.Interface_ModelEditor_ScreenshotMode = !CFG.Current.Interface_ModelEditor_ScreenshotMode;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ScreenshotMode);


            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        foreach (var view in ViewHandler.ModelViews)
        {
            if (view.ViewportWindow.Viewport is VulkanViewport vulkanViewport)
            {
                if (vulkanViewport.Visible)
                {
                    view.ViewportWindow.Draw(device, cl);
                }
            }
        }
    }

    public void Update(float dt)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        activeView.ViewportWindow.Update(dt);
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

        if (Project.Descriptor.ProjectType == ProjectType.DES)
        {
            TaskLogs.AddLog("Model Editor is not supported for DES.", LogLevel.Warning);
            return;
        }

        if (activeView == null)
            return;

        if (!autoSave && CFG.Current.ModelEditor_ManualSave_IncludeFLVER ||
        autoSave && CFG.Current.ModelEditor_AutomaticSave_IncludeFLVER)
        {
            if (activeView.Selection.SelectedModelWrapper != null)
            {
                activeView.Selection.SelectedModelWrapper.Save();
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
}
