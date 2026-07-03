using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamEditorScreen : EditorScreen
{
    public string EditorName => "Param Editor";
    public string CommandEndpoint => "param";
    public string SaveType => "Params";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public ParamViewHandler ViewHandler;
    public ParamCommandQueue CommandQueue;
    public ParamClipboard Clipboard;
    public ParamStatisticsMenu StatisticsMenu;
    public ParamShortcuts Shortcuts;

    public ParamPasteMenu PasteMenu;
    public ParamToolMenu ToolMenu;

    private bool ImportRowNamesPrompted = false;

    public ParamEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new(this, project);
        CommandQueue = new(this, project);
        Clipboard = new(this, project);
        StatisticsMenu = new(this, project);
        Shortcuts = new(this, project);

        ToolMenu = new(this, project);

        PasteMenu = new(this, project);

        Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
    }

    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();

        if (!ImportRowNamesPrompted)
        {
            ImportRowNamesPrompted = true;
            ImportRowNamesPrompt();
        }

        Shortcuts.Shortcuts();

        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            ToolMenu.DisplayMenu();
            ToolMenu.ParamUpgrader.ParamUpgradeWarning();

            ImGui.EndMenuBar();
        }

        var activeView = ViewHandler.ActiveView;

        if (activeView != null)
        {
            activeView.MassEdit.PopupMenu.Display();
        }

        PasteMenu.Display();
        StatisticsMenu.Display();
        ToolMenu.ParamComparisonTool.HandleReportModal();

        if (CFG.Current.ParamEditor_Enable_Compact_Mode)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(1.0f, 1.0f) * scale);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 1.0f) * scale);
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5.0f, 1.0f) * scale);
        }
        else
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,
                style.ItemSpacing * scale - new Vector2(3.5f, 0f) * scale);
        }

        // Views
        var dsid = ImGui.GetID("DockSpace_ParamView");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref UIHelper.DockGroup_ParamEditor);

        ViewHandler.HandleViews(dsid);

        if (ViewHandler.ActiveView != null)
        {
            ToolMenu.Draw();
        }

        if (CFG.Current.ParamEditor_Enable_Compact_Mode)
        {
            ImGui.PopStyleVar(3);
        }
        else
        {
            ImGui.PopStyleVar();
        }
    }

    #region Row Name Import
    private void ImportRowNamesPrompt()
    {
        if (!Project.Descriptor.ImportedParamRowNames)
        {
            var languages = Project.Handler.ParamData.ParamAnnotationLanguages;
            var curLanguage = languages.Languages.FirstOrDefault(e => e.Name == CFG.Current.ParamEditor_Annotation_Language);

            var displayName = LOC.Get(curLanguage.Key);

            var dialog = PlatformUtils.Instance.MessageBox(
                LOC.Get("PARAM_RowName_Dialog_Prompt", displayName),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (dialog is DialogResult.OK)
            {
                RowNameHelper.ImportRowNames(Project, Project.Handler.ParamData.PrimaryBank, CFG.Current.ParamEditor_Import_Language);
            }

            Project.Descriptor.ImportedParamRowNames = true;

            Smithbox.Orchestrator.SaveProject(Project);
        }

    }

    #endregion

    #region Menubar
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
                // PARAM
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_PARAM")}##manualToggle_param"))
                {
                    CFG.Current.ParamEditor_ManualSave_IncludePARAM = !CFG.Current.ParamEditor_ManualSave_IncludePARAM;
                }
                UIHelper.Tooltip(LOC.Get("EDITOR_SaveOutput_PARAM_TT"));
                UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_ManualSave_IncludePARAM);


                ImGui.EndMenu();
            }
            UIHelper.Tooltip(LOC.Get("EDITOR_Menubar_Manual_Save_Output_TT"));

            // Automatic Save Output
            if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Auto_Save_Output")}##autoSaveMenuHeader"))
            {
                // PARAM
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_SaveOutput_PARAM")}##autoToggle_param"))
                {
                    CFG.Current.ParamEditor_AutomaticSave_IncludePARAM = !CFG.Current.ParamEditor_AutomaticSave_IncludePARAM;
                }
                UIHelper.Tooltip(LOC.Get("EDITOR_SaveOutput_FMG_TT"));
                UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_AutomaticSave_IncludePARAM);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip(LOC.Get("EDITOR_Menubar_Auto_Save_Output_TT"));

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        // Edit
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##editMenuHeader"))
        {
            // Undo
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
            {
                if (ActionManager.CanUndo())
                {
                    ActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}##undoAllAction"))
            {
                if (ActionManager.CanUndo())
                {
                    ActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
            {
                if (ActionManager.CanRedo())
                {
                    ActionManager.RedoAction();
                }
            }

            // Param Row
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_Menubar_Header_Param_Row")}##paramRowMenuHeader"))
            {
                // Duplicate
                if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Action_Duplicate")}##duplicateAction", InputManager.GetHint(KeybindID.Duplicate)))
                {
                    ParamRowDuplicate.ApplyDuplicate(activeView);
                }
                UIHelper.Tooltip(LOC.Get("PARAM_Menubar_Action_Duplicate_TT"));

                // Duplicate to Commutative Param
                if (ImGui.BeginMenu($"{LOC.Get("PARAM_Menubar_Action_Duplicate_to_Commutative_Param")}##commutativeDuplicateAction", ParamRowDuplicate.IsCommutativeParam(activeView)))
                {
                    ParamRowDuplicate.ApplyCommutativeDuplicate(activeView);

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip(LOC.Get("PARAM_Menubar_Action_Duplicate_to_Commutative_Param_TT"));

                // Delete
                if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Action_Delete")}##deleteAction", InputManager.GetHint(KeybindID.Delete)))
                {
                    ParamRowDelete.ApplyDelete(activeView);
                }
                UIHelper.Tooltip(LOC.Get("PARAM_Menubar_Action_Delete_TT"));

                // Copy
                if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Action_Copy")}##copyAction", InputManager.GetHint(KeybindID.Copy)))
                {
                    Clipboard.CopySelectionToClipboard(activeView);
                }
                UIHelper.Tooltip(LOC.Get("PARAM_Menubar_Action_Copy_TT"));

                // Paste
                if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Action_Paste")}##pasteAction", InputManager.GetHint(KeybindID.Paste)))
                {
                    if (Project.Handler.ParamData.PrimaryBank.ClipboardRows.Any())
                    {
                        EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
                    }
                }
                UIHelper.Tooltip(LOC.Get("PARAM_Menubar_Action_Paste_TT"));

                // Jump
                if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Action_Jump")}##jumpAction", InputManager.GetHint(KeybindID.Jump)))
                {
                    if (activeView.Selection.RowSelectionExists())
                    {
                        activeView.JumpToSelectedRow = true;
                    }
                }
                UIHelper.Tooltip(LOC.Get("PARAM_Menubar_Action_Jump_TT"));

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_View_Editor")}##editorViewToggle"))
            {
                CFG.Current.Interface_ParamEditor_Table = !CFG.Current.Interface_ParamEditor_Table;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Table);

            if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Tool_Window")}##toolsViewToggle"))
            {
                CFG.Current.Interface_ParamEditor_ToolWindow = !CFG.Current.Interface_ParamEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    #endregion

    #region Save
    public bool SaveLock = false;
    public async void Save(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        await Task.Yield();

        if (!SaveLock)
        {
            SaveLock = true;

            if (CFG.Current.ParamEditor_ManualSave_IncludePARAM)
            {
                activeView.ParamTableWindow.WriteTableGroupNames();

                Task<bool> paramSaveTask = SaveParams();
                Task.WaitAll(paramSaveTask);
            }

            SaveLock = false;
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Param_Save_in_Progress"));
        }
    }

    public async Task<bool> SaveParams()
    {
        var activeView = ViewHandler.ActiveView;

        try
        {
            await activeView.GetPrimaryBank().Save();
            Smithbox.Log(this, LOC.Get("PARAM_Params_Saved"));
        }
        catch (SavingFailedException e)
        {
            Smithbox.LogError(this, e.Message, e.Wrapped);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, e.Message, e);
        }

        return true;
    }

    public async void SaveAll()
    {
        await Task.Yield();

        Save();
    }
    #endregion
}
