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

    public ResourceLoadWindow LoadingModal;

    public ModelEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new ModelViewHandler(this, project);

        CommandQueue = new ModelCommandQueue(this, project);
        Shortcuts = new ModelShortcuts(this, project);

        LoadingModal = new();
    }

    public string EditorName => "Model Editor";
    public string CommandEndpoint => "model";
    public string SaveType => "Models";
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

        var dsid = ImGui.GetID("DockSpace_ModelEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ref UIHelper.DockGroup_ModelEditor);

        ViewHandler.HandleViews(dsid);

        if (activeView != null)
        {
            LoadingModal.DisplayWindow(activeView.ViewportWindow.Viewport.Width, activeView.ViewportWindow.Viewport.Height);
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
                if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)}  /  {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
                {
                    if (activeView.ViewportActionManager.CanRedo())
                    {
                        activeView.ViewportActionManager.RedoAction();
                    }
                }

                ImGui.Separator();

                // Actions
                activeView.CreateAction.OnMenu();
                activeView.DuplicateAction.OnMenu();
                activeView.DeleteAction.OnMenu();

                ImGui.Separator();

                activeView.FrameAction.OnMenu();
                activeView.GotoAction.OnMenu();
                activeView.PullToCameraAction.OnMenu();

                ImGui.Separator();

                activeView.ReorderAction.OnMenu();
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

            // Hides the non-Viewport windows
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

    public void OptionsMenu()
    {
        if (ImGui.BeginMenu("Options"))
        {
            if (ImGui.BeginMenu("Containers"))
            {
                if (ImGui.MenuItem("Include Alias in Search"))
                {
                    CFG.Current.ModelEditor_Containers_IncludeAliasInSearch = !CFG.Current.ModelEditor_Containers_IncludeAliasInSearch;
                }
                UIHelper.Tooltip($"If enabled, when filtering the source list, alias will be included. Can be slower than normal.");
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_Containers_IncludeAliasInSearch);

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Files"))
            {
                if (ImGui.MenuItem("Auto-Select First Entries"))
                {
                    CFG.Current.ModelEditor_Files_AutoLoadFirstEntry = !CFG.Current.ModelEditor_Files_AutoLoadFirstEntry;
                }
                UIHelper.Tooltip($"If enabled, the first entry in the list will be loaded automatically.");
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_Files_AutoLoadFirstEntry);

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Contents"))
            {
                if (ImGui.MenuItem("Display Node Name in Mesh Entry"))
                {
                    CFG.Current.ModelEditor_Contents_NodeNameInMeshEntry = !CFG.Current.ModelEditor_Contents_NodeNameInMeshEntry;
                }
                UIHelper.Tooltip($"If enabled, the linked node name is displayed in the mesh entry name.");
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_Contents_NodeNameInMeshEntry);

                ImGui.EndMenu();
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

        if (Project.Descriptor.ProjectType == ProjectType.DES)
        {
            Smithbox.Log(this, "Model Editor is not supported for DES.", LogLevel.Warning);
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
