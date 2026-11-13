using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
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

    /// <summary>
    /// The problems that stopped the last save, for the user to fix or save past
    /// </summary>
    public ModelSaveCheckWindow SaveCheckWindow;

    public ModelEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new ModelViewHandler(this, project);

        CommandQueue = new ModelCommandQueue(this, project);
        Shortcuts = new ModelShortcuts(this, project);

        SaveCheckWindow = new ModelSaveCheckWindow(this, project);
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

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            {
                ImGui.Separator();

                ImGui.Text(LOC.Get("SYS_OpenGL_Viewport_Blocked"));
            }

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_ModelEdit");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ref GUI.DockGroup_ModelEditor);

        ViewHandler.HandleViews(dsid);

        SaveCheckWindow.Display();
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
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_Save_TT"));

            // Save All: also covers the models that were edited and then unloaded, so show how many there are
            var unsavedCount = Project.Handler.ModelData?.PrimaryBank?.Models.Values
                .Sum(e => e.Models.Count(m => m.IsDirty)) ?? 0;

            var saveAllLabel = unsavedCount > 0
                ? $"{LOC.Get("EDITOR_Menubar_Action_SaveAll")} {LOC.Get("MODEL_Editor_Unsaved_Count", unsavedCount)}"
                : LOC.Get("EDITOR_Menubar_Action_SaveAll");

            if (ImGui.MenuItem($"{saveAllLabel}##saveAllAction"))
            {
                SaveAll();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_SaveAll_TT"));

            ImGui.Separator();

            // Allow Unmodified Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save")}##allowUnmodifiedSave"))
            {
                CFG.Current.ModelEditor_Allow_Unmodified_Save = !CFG.Current.ModelEditor_Allow_Unmodified_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_Allow_Unmodified_Save);

            // Allow Automatic Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                CFG.Current.ModelEditor_Allow_Automatic_Save = !CFG.Current.ModelEditor_Allow_Automatic_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_Allow_Automatic_Save);

            ImGui.Separator();

            // Check Models Before Saving
            if (ImGui.MenuItem($"{LOC.Get("MODEL_SaveCheck_Menu_Enabled")}##saveCheckEnabled"))
            {
                CFG.Current.ModelEditor_SaveCheck_Enabled = !CFG.Current.ModelEditor_SaveCheck_Enabled;
            }
            GUI.Tooltip(LOC.Get("MODEL_SaveCheck_Menu_Enabled_TT"));
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_SaveCheck_Enabled);

            // Stop on Warnings
            if (ImGui.MenuItem($"{LOC.Get("MODEL_SaveCheck_Menu_Stop_On_Warnings")}##saveCheckWarnings", "", false,
                CFG.Current.ModelEditor_SaveCheck_Enabled))
            {
                CFG.Current.ModelEditor_SaveCheck_Stop_On_Warnings = !CFG.Current.ModelEditor_SaveCheck_Stop_On_Warnings;
            }
            GUI.Tooltip(LOC.Get("MODEL_SaveCheck_Menu_Stop_On_Warnings_TT"));
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_SaveCheck_Stop_On_Warnings);

            // Check the loaded model now, without saving
            var loaded = ViewHandler.ActiveView?.Selection.SelectedModelWrapper;

            if (ImGui.MenuItem($"{LOC.Get("MODEL_SaveCheck_Menu_Check_Now")}##saveCheckNow", "", false, loaded?.FLVER != null))
            {
                var models = new List<ModelWrapper> { loaded };

                SaveCheckWindow.Open(ModelCheck.Run(Project, models), models, null);
            }
            GUI.Tooltip(LOC.Get("MODEL_SaveCheck_Menu_Check_Now_TT"));

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Runs the save check over the models a save is about to write. Returns false, and opens the Save Check window,
    /// if the save should stop. Autosaves and disabled checks always pass.
    /// </summary>
    private bool PassesSaveCheck(List<ModelWrapper> models, bool autoSave, bool skipCheck, Action saveWithoutCheck)
    {
        if (autoSave || skipCheck || !CFG.Current.ModelEditor_SaveCheck_Enabled || models.Count == 0)
            return true;

        var issues = ModelCheck.Run(Project, models);

        if (ModelCheck.StopsSave(issues, CFG.Current.ModelEditor_SaveCheck_Stop_On_Warnings))
        {
            SaveCheckWindow.Open(issues, models, saveWithoutCheck);

            Smithbox.Log(this, LOC.Get("MODEL_SaveCheck_Log_Stopped",
                issues.Count(i => i.Severity is ModelCheckSeverity.Error),
                issues.Count(i => i.Severity is ModelCheckSeverity.Warning)), LogLevel.Warning);

            return false;
        }

        // Nothing stops this save, so an open report is out of date
        SaveCheckWindow.Close();

        return true;
    }

    /// <summary>The models Save All writes: every modified one, loaded or not</summary>
    private List<ModelWrapper> GetUnsavedModels()
    {
        return Project.Handler.ModelData?.PrimaryBank?.Models.Values
            .SelectMany(c => c.Models)
            .Where(m => m.IsDirty)
            .ToList() ?? new();
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
                if (activeView.ViewportActionManager.PeekUndoAction() is { } undoNext)
                {
                    GUI.Tooltip(LOC.Get("MODEL_History_Tool_Undo_TT", ModelHistory.Describe(undoNext)));
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
                if (activeView.ViewportActionManager.PeekRedoAction() is { } redoNext)
                {
                    GUI.Tooltip(LOC.Get("MODEL_History_Tool_Redo_TT", ModelHistory.Describe(redoNext)));
                }

                // History: opens the History tool
                if (ImGui.MenuItem($"{LOC.Get("MODEL_History_Menu")}##historyAction"))
                {
                    CFG.Current.Interface_ModelEditor_ToolWindow = true;
                    CFG.Current.Interface_ModelEditor_Tool_History = true;
                    activeView.HistoryTool.FocusOnNextDraw = true;
                }
                GUI.Tooltip(LOC.Get("MODEL_History_Menu_TT"));

                ImGui.Separator();

                // Actions
                activeView.AddAction.OnMenu();
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
            // Individual Windows
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Options_EditorDisplay_Action_Windows")}##windows"))
            {
                CFG.Current.Interface_ModelEditor_EditorDisplayType = EditorDisplayType.Windows;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Options_EditorDisplay_Action_Windows_TT"));
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_EditorDisplayType == EditorDisplayType.Windows);

            // Tabbed Panels
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Options_EditorDisplay_Action_TabbedPanels")}##tabbedPanels"))
            {
                CFG.Current.Interface_ModelEditor_EditorDisplayType = EditorDisplayType.TabbedPanels;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Options_EditorDisplay_Action_TabbedPanels_TT"));
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_EditorDisplayType == EditorDisplayType.TabbedPanels);

            ImGui.Separator();

            // Tools
            if (ImGui.MenuItem($"{LOC.Get("MODEL_Editor_View_Tools")}##toolsToggle"))
            {
                CFG.Current.Interface_ModelEditor_ToolWindow = !CFG.Current.Interface_ModelEditor_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_ToolWindow);

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

            // Viewport
            if (ImGui.BeginMenu($"{LOC.Get("MODEL_Editor_Option_Viewport_Header")}##viewportMenuHeader"))
            {
                // Live Preview
                if (ImGui.MenuItem($"{LOC.Get("MODEL_Editor_Viewport_Live_Preview")}##livePreviewToggle"))
                {
                    CFG.Current.ModelEditor_Viewport_Live_Preview = !CFG.Current.ModelEditor_Viewport_Live_Preview;
                }
                GUI.Tooltip(LOC.Get("MODEL_Editor_Viewport_Live_Preview_TT"));
                GUI.ShowActiveStatus(CFG.Current.ModelEditor_Viewport_Live_Preview);

                // Refresh Viewport
                if (ImGui.MenuItem($"{LOC.Get("MODEL_Editor_Viewport_Refresh")}##refreshViewport", false,
                    ViewHandler.ActiveView?.Selection.SelectedModelWrapper?.Container != null))
                {
                    ViewHandler.ActiveView?.RefreshViewportFromEdits();
                }
                GUI.Tooltip(LOC.Get("MODEL_Editor_Viewport_Refresh_TT"));

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

        // The map lighting and irradiance volume the viewports borrow, loaded once a model is, as reading the map's
        // volumes and GPARAM on the editor's first frame froze it
        if (activeView.Universe.HasLoadedModel)
            Project.Handler.ViewportEnvironment.Update();
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

    public void Save(bool autoSave = false, bool skipCheck = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (autoSave && CFG.Current.ModelEditor_Allow_Automatic_Save || !autoSave)
        {
            var wrapper = activeView.Selection.SelectedModelWrapper;

            if (wrapper != null)
            {
                // Only a model the save will write is checked
                var willWrite = wrapper.IsDirty || CFG.Current.ModelEditor_Allow_Unmodified_Save;

                if (willWrite && !PassesSaveCheck(new List<ModelWrapper> { wrapper }, autoSave, skipCheck,
                    () => SaveModelWithoutCheck(wrapper)))
                {
                    return;
                }

                wrapper.Save();
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public List<string> GetUnsavedFiles()
    {
        var bank = Project.Handler.ModelData?.PrimaryBank;

        if (bank?.Models == null)
            return new();

        return bank.Models.Values
            .SelectMany(c => c.Models)
            .Where(m => m.IsDirty)
            .Select(m => m.Name)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Saves the model the Save Check window stopped, whether or not it is still the loaded one
    /// </summary>
    private void SaveModelWithoutCheck(ModelWrapper wrapper)
    {
        wrapper.Save();

        Smithbox.Instance.SaveConfiguration();
    }

    /// <summary>
    /// Called when closing with unsaved changes, so it isn't stopped by the check, as the Map Editor's isn't
    /// </summary>
    public void SaveUnsaved()
    {
        SaveAll(skipCheck: true);
    }

    /// <summary>
    /// Save all modified models, including those that are not currently loaded
    /// </summary>
    public void SaveAll(bool autoSave = false, bool skipCheck = false)
    {
        if (autoSave && CFG.Current.ModelEditor_Allow_Automatic_Save || !autoSave)
        {
            if (!PassesSaveCheck(GetUnsavedModels(), autoSave, skipCheck, () => SaveAll(skipCheck: true)))
                return;

            Project.Handler.ModelData.PrimaryBank.SaveAllModels();
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
}
