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
        ImGui.DockSpace(dsid, new Vector2(0, 0), ref GUI.DockGroup_ModelEditor);

        ViewHandler.HandleViews(dsid);

        if (activeView != null)
        {
            if (activeView.ViewportWindow.Viewport != null)
            {
                LoadingModal.DisplayWindow(activeView.ViewportWindow.Viewport.Width, activeView.ViewportWindow.Viewport.Height);
            }
        }
    }

    public void FileMenu()
    {
        // File
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            // Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##saveAction", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }

            ImGui.Separator();

            // Manual Save Output
            if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Manual_Save_Output")}##manualSaveMenuHeader"))
            {
                // FLVER
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_FLVER_TT")}##manualToggle_flver"))
                {
                    CFG.Current.ModelEditor_ManualSave_IncludeFLVER = !CFG.Current.ModelEditor_ManualSave_IncludeFLVER;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_FLVER_TT"));
                GUI.ShowActiveStatus(CFG.Current.ModelEditor_ManualSave_IncludeFLVER);


                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Manual_Save_Output_TT"));

            // Automatic Save Output
            if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Auto_Save_Output")}##autoSaveMenuHeader"))
            {
                // FLVER
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_FLVER_TT")}##autoToggle_flver"))
                {
                    CFG.Current.ModelEditor_AutomaticSave_IncludeFLVER = !CFG.Current.ModelEditor_AutomaticSave_IncludeFLVER;
                }
                GUI.Tooltip(LOC.Get("EDITOR_SaveOutput_FLVER_TT"));
                GUI.ShowActiveStatus(CFG.Current.ModelEditor_AutomaticSave_IncludeFLVER);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Auto_Save_Output_TT"));


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

                // Actions
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
        // View
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            // Tools
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Editor_View_Tools")}##toolsToggle"))
            {
                CFG.Current.Interface_ModelEditor_ToolWindow = !CFG.Current.Interface_ModelEditor_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ToolWindow);

            // Hides the non-Viewport windows
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Editor_View_Screenshot_Mode")}##screenshotModeToggle"))
            {
                CFG.Current.Interface_ModelEditor_ScreenshotMode = !CFG.Current.Interface_ModelEditor_ScreenshotMode;
            }
            GUI.Tooltip("MODEL_Editor_View_Screenshot_Mode_TT");
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ScreenshotMode);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    public void OptionsMenu()
    {
        // Otpions
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Options")}##optionsMenuHeader"))
        {
            // Containers
            if (ImGui.BeginMenu($"{LOC.Get("MODEL_Editor_Option_Container_Header")}##containerMenuHeaer"))
            {
                // Include Alias in Search
                if (ImGui.MenuItem($"{LOC.Get("MODEL_Editor_Container_Include_Alias_in_Search")}##includeAliasInSearchToggle"))
                {
                    CFG.Current.ModelEditor_Containers_IncludeAliasInSearch = !CFG.Current.ModelEditor_Containers_IncludeAliasInSearch;
                }
                GUI.Tooltip(LOC.Get("MODEL_Editor_Container_Include_Alias_in_Search_TT"));
                GUI.ShowActiveStatus(CFG.Current.ModelEditor_Containers_IncludeAliasInSearch);

                ImGui.EndMenu();
            }

            // Files
            if (ImGui.BeginMenu($"{LOC.Get("MODEL_Editor_Option_Files_Header")}##filesMenuHeaer"))
            {
                // Auto-Select First Entries
                if (ImGui.MenuItem($"{LOC.Get("MODEL_Editor_Files_AutoSelect_First")}##autoSelectFirstFileToggle"))
                {
                    CFG.Current.ModelEditor_Files_AutoLoadFirstEntry = !CFG.Current.ModelEditor_Files_AutoLoadFirstEntry;
                }
                GUI.Tooltip(LOC.Get("MODEL_Editor_Files_AutoSelect_First_TT"));
                GUI.ShowActiveStatus(CFG.Current.ModelEditor_Files_AutoLoadFirstEntry);

                ImGui.EndMenu();
            }

            // Contents
            if (ImGui.BeginMenu($"{LOC.Get("MODEL_Editor_Option_Contents_Header")}##contentsMenuHeaer"))
            {
                // Display Node Name in Mesh Entry
                if (ImGui.MenuItem($"{LOC.Get("MODEL_Editor_Contents_Display_Node_Name_in_Mesh_Entry")}##nodeNameInMeshEntryToggle"))
                {
                    CFG.Current.ModelEditor_Contents_NodeNameInMeshEntry = !CFG.Current.ModelEditor_Contents_NodeNameInMeshEntry;
                }
                GUI.Tooltip($"{LOC.Get("MODEL_Editor_Contents_Display_Node_Name_in_Mesh_Entry_TT")}");
                GUI.ShowActiveStatus(CFG.Current.ModelEditor_Contents_NodeNameInMeshEntry);

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
            Smithbox.Log(this, LOC.Get("MODEL_Editor_Invalid_Save_Project_Type_DES"), LogLevel.Warning);
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
