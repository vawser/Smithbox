using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.AnimEditor;

public class AnimEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public AnimViewHandler ViewHandler;

    public AnimCommandQueue CommandQueue;
    public AnimShortcuts Shortcuts;

    public AnimToolWindow ToolMenu;

    public ResourceLoadWindow LoadingModal;
    public ResourceListWindow ResourceList;

    public AnimEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new AnimViewHandler(this, project);

        CommandQueue = new AnimCommandQueue(this, project);
        Shortcuts = new AnimShortcuts(this, project);

        LoadingModal = new();
        ResourceList = new();

        ToolMenu = new AnimToolWindow(this, project);
    }

    public string EditorName => "Animation Editor";
    public string CommandEndpoint => "anim";
    public string SaveType => "Animation";
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

        var dsid = ImGui.GetID("DockSpace_AnimEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0));

        ViewHandler.HandleViews();

        var activeView = ViewHandler.ActiveView;

        if (activeView != null)
        {
            ToolMenu.Display();

            LoadingModal.DisplayWindow(activeView.ViewportWindow.Viewport.Width, activeView.ViewportWindow.Viewport.Height);

            if (CFG.Current.Interface_AnimEditor_ResourceList)
            {
                ResourceList.DisplayWindow("animResourceList", activeView.Universe, CFG.Current.Interface_ModelEditor_ScreenshotMode);
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
                if (ImGui.MenuItem($"TAE"))
                {
                    CFG.Current.AnimEditor_ManualSave_IncludeTAE = !CFG.Current.AnimEditor_ManualSave_IncludeTAE;
                }
                UIHelper.Tooltip("If enabled, the time act container files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.AnimEditor_ManualSave_IncludeTAE);

                if (ImGui.MenuItem($"BEH"))
                {
                    CFG.Current.AnimEditor_ManualSave_IncludeBEH = !CFG.Current.AnimEditor_ManualSave_IncludeBEH;
                }
                UIHelper.Tooltip("If enabled, the behavior container files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.AnimEditor_ManualSave_IncludeBEH);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the manual saving process.");

            if (ImGui.BeginMenu("Output on Automatic Save"))
            {
                if (ImGui.MenuItem($"TAE"))
                {
                    CFG.Current.AnimEditor_AutomaticSave_IncludeTAE = !CFG.Current.AnimEditor_AutomaticSave_IncludeTAE;
                }
                UIHelper.Tooltip("If enabled, the time act container files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.AnimEditor_AutomaticSave_IncludeTAE);

                if (ImGui.MenuItem($"BEH"))
                {
                    CFG.Current.AnimEditor_AutomaticSave_IncludeBEH = !CFG.Current.AnimEditor_AutomaticSave_IncludeBEH;
                }
                UIHelper.Tooltip("If enabled, the behavior container files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.AnimEditor_AutomaticSave_IncludeBEH);

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
                CFG.Current.Interface_AnimEditor_ToolWindow = !CFG.Current.Interface_AnimEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_AnimEditor_ToolWindow);

            if (ImGui.MenuItem("Resource List"))
            {
                CFG.Current.Interface_AnimEditor_ResourceList = !CFG.Current.Interface_AnimEditor_ResourceList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_AnimEditor_ResourceList);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

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
        if (ViewHandler.ViewToClose != null)
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

        if (activeView.IsBehaviorView())
        {
            var behaviorView = activeView.GetBehaviorView();

            if (!autoSave && CFG.Current.AnimEditor_ManualSave_IncludeBEH ||
            autoSave && CFG.Current.AnimEditor_AutomaticSave_IncludeBEH)
            {
                if (behaviorView.Selection.SelectedFile != null)
                {
                    behaviorView.Selection.SelectedFile.Save();
                }
            }
        }

        if (activeView.IsTimeActView())
        {
            if (!autoSave && CFG.Current.AnimEditor_ManualSave_IncludeTAE ||
            autoSave && CFG.Current.AnimEditor_AutomaticSave_IncludeTAE)
            {

            }
        }


        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
}
