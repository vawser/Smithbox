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
    public ParamFindMenu FindMenu;

    private bool ImportRowNamesPrompted = false;

    public ParamEditorScreen(ProjectEntry project)
    {
        Project = project;

        ActionManager.ExecutionGuard = GetSaveBlockReason;

        ViewHandler = new(this, project);
        CommandQueue = new(this, project);
        Clipboard = new(this, project);
        StatisticsMenu = new(this, project);
        Shortcuts = new(this, project);

        PasteMenu = new(this, project);
        FindMenu = new(this, project);

        Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
    }

    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();
        var activeView = ViewHandler.ActiveView;

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

            if (activeView != null)
            {
                activeView.ToolMenu.DisplayMenu();
                activeView.ToolMenu.ParamUpgrader.ParamUpgradeWarning();
            }

            ImGui.EndMenuBar();
        }


        if (activeView != null)
        {
            activeView.MassEdit.PopupMenu.Display();
        }

        PasteMenu.Display();
        FindMenu.Display();
        StatisticsMenu.Display();

        if (activeView != null)
        {
            activeView.ToolMenu.ParamComparisonTool.HandleReportModal();
        }

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
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_ParamEditor);

        ViewHandler.HandleViews(dsid);

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
            // Name the language the import will actually use (the Import language, not the Annotation one).
            // Fall back to the raw setting if it isn't in the list, rather than throwing.
            var importLanguage = CFG.Current.ParamEditor_Import_Language;
            var curLanguage = Project.Handler.ParamData.RowImportLanguages?.Options?
                .FirstOrDefault(e => e.Name == importLanguage);

            var displayName = curLanguage != null ? LOC.Get(curLanguage.Key) : importLanguage;

            var dialog = PlatformUtils.Instance.MessageBox(
                LOC.Get("PARAM_RowName_Dialog_Prompt", displayName),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (dialog is DialogResult.OK)
            {
                RowNameHelper.ImportRowNames(Project, Project.Handler.ParamData.PrimaryBank, importLanguage);
            }

            Project.Descriptor.ImportedParamRowNames = true;

            Smithbox.Orchestrator.SaveProject(Project);
        }

    }

    #endregion

    #region Undo / Redo
    // Used by both the Edit menu and the shortcuts, so both refresh the modified-row colours afterwards

    public void Undo()
    {
        if (!ActionManager.CanUndo())
            return;

        ActionManager.UndoAction();
        Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
    }

    public void UndoAll()
    {
        if (!ActionManager.CanUndo())
            return;

        // Reverts the whole session's edits in one go, so confirm first
        var dialog = PlatformUtils.Instance.MessageBox(
            LOC.Get("PARAM_Undo_All_Dialog_Prompt", ActionManager.UndoCount),
            LOC.Get("SYS_Warning_Header"),
            MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

        if (dialog is not DialogResult.OK)
            return;

        ActionManager.UndoAllAction();
        Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
    }

    /// <summary>
    /// A short description of an action for the Edit menu, e.g. "Undo: Delete Rows"
    /// </summary>
    public static string GetActionDisplayName(EditorAction action)
    {
        return action switch
        {
            null => "",
            PropertiesChangedAction => LOC.Get("PARAM_Action_Name_Edit_Value"),
            AddParamsAction => LOC.Get("PARAM_Action_Name_Add_Rows"),
            DeleteParamsAction => LOC.Get("PARAM_Action_Name_Delete_Rows"),
            ReorderRowAction => LOC.Get("PARAM_Action_Name_Reorder_Rows"),
            AddRowsToTableGroup => LOC.Get("PARAM_Action_Name_Add_Rows"),
            CompoundAction c when c.ContainsOnly<PropertiesChangedAction>() => LOC.Get("PARAM_Action_Name_Edit_Values"),
            CompoundAction => LOC.Get("PARAM_Action_Name_Multiple"),
            _ => action.GetType().Name.Replace("Action", "")
        };
    }

    private string GetUndoRedoLabel(string baseKey, EditorAction next)
    {
        var label = LOC.Get(baseKey);

        return next == null ? label : $"{label}: {GetActionDisplayName(next)}";
    }

    public void Redo()
    {
        if (!ActionManager.CanRedo())
            return;

        ActionManager.RedoAction();
        Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
    }
    #endregion

    #region Menubar
    public void FileMenu()
    {
        // File
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            // Save (disabled, and labelled as such, while a save is running)
            var saveLabel = IsSaving ? LOC.Get("PARAM_Menubar_Saving") : LOC.Get("EDITOR_Menubar_Action_Save");

            if (ImGui.MenuItem($"{saveLabel}##saveAction", $"{InputManager.GetHint(KeybindID.Save)}", false, !IsSaving))
            {
                Save();
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Action_SaveAll_TT"));

            ImGui.Separator();

            // Allow Unmodified Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save")}##allowUnmodifiedSave"))
            {
                CFG.Current.ParamEditor_Allow_Unmodified_Save = !CFG.Current.ParamEditor_Allow_Unmodified_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Unmodified_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.ParamEditor_Allow_Unmodified_Save);

            // Allow Automatic Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##allowAutomaticSave"))
            {
                CFG.Current.ParamEditor_Allow_Automatic_Save = !CFG.Current.ParamEditor_Allow_Automatic_Save;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
            GUI.ShowActiveStatus(CFG.Current.ParamEditor_Allow_Automatic_Save);

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        // Edit
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##editMenuHeader"))
        {
            // Undo / Redo name the action they'll act on, e.g. "Undo: Delete Rows"
            var undoLabel = GetUndoRedoLabel("EDITOR_Menubar_Action_Undo", ActionManager.PeekUndoAction());
            var redoLabel = GetUndoRedoLabel("EDITOR_Menubar_Action_Redo", ActionManager.PeekRedoAction());

            // Undo
            if (ImGui.MenuItem($"{undoLabel}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}", false, ActionManager.CanUndo()))
            {
                Undo();
            }

            // Undo All (asks for confirmation)
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}...##undoAllAction", "", false, ActionManager.CanUndo()))
            {
                UndoAll();
            }

            // Redo
            if (ImGui.MenuItem($"{redoLabel}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}", false, ActionManager.CanRedo()))
            {
                Redo();
            }

            // Row actions are only available while the row list is shown (not with a stay param open), and the
            // selection-based ones need a selection. Disabled items explain themselves better than a no-op or a crash.
            var rowsShown = activeView != null
                && activeView.Selection.ActiveParamExists()
                && activeView.Selection.ActiveStayParam == null;

            var hasRowSelection = rowsShown && activeView.Selection.RowSelectionExists();
            var hasClipboard = rowsShown && Project.Handler.ParamData.PrimaryBank.ClipboardRows.Count > 0;

            // Param Row
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_Menubar_Header_Param_Row")}##paramRowMenuHeader"))
            {
                // Duplicate
                if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Action_Duplicate")}##duplicateAction", InputManager.GetHint(KeybindID.Duplicate), false, hasRowSelection))
                {
                    ParamRowDuplicate.ApplyDuplicate(activeView);
                }
                GUI.Tooltip(LOC.Get("PARAM_Menubar_Action_Duplicate_TT"));

                // Duplicate to Commutative Param
                if (ImGui.BeginMenu($"{LOC.Get("PARAM_Menubar_Action_Duplicate_to_Commutative_Param")}##commutativeDuplicateAction", hasRowSelection && ParamRowDuplicate.IsCommutativeParam(activeView)))
                {
                    ParamRowDuplicate.ApplyCommutativeDuplicate(activeView);

                    ImGui.EndMenu();
                }
                GUI.Tooltip(LOC.Get("PARAM_Menubar_Action_Duplicate_to_Commutative_Param_TT"));

                // Delete
                if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Action_Delete")}##deleteAction", InputManager.GetHint(KeybindID.Delete), false, hasRowSelection))
                {
                    ParamRowDelete.ApplyDelete(activeView);
                }
                GUI.Tooltip(LOC.Get("PARAM_Menubar_Action_Delete_TT"));

                // Copy
                if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Action_Copy")}##copyAction", InputManager.GetHint(KeybindID.Copy), false, hasRowSelection))
                {
                    Clipboard.CopySelectionToClipboard(activeView);
                }
                GUI.Tooltip(LOC.Get("PARAM_Menubar_Action_Copy_TT"));

                // Paste
                if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Action_Paste")}##pasteAction", InputManager.GetHint(KeybindID.Paste), false, hasClipboard))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
                }
                GUI.Tooltip(LOC.Get("PARAM_Menubar_Action_Paste_TT"));

                // Jump
                if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Action_Jump")}##jumpAction", InputManager.GetHint(KeybindID.Jump), false, hasRowSelection))
                {
                    activeView.JumpToSelectedRow = true;
                }
                GUI.Tooltip(LOC.Get("PARAM_Menubar_Action_Jump_TT"));

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
            if (ImGui.MenuItem($"{LOC.Get("PARAM_Menubar_Tool_Window")}##toolsViewToggle"))
            {
                CFG.Current.Interface_ParamEditor_ToolWindow = !CFG.Current.Interface_ParamEditor_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ParamEditor_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    #endregion

    #region Save
    // 1 while a save is running. Set with Interlocked so two saves started in the same frame can't both run.
    private int _saveInProgress = 0;

    /// <summary>
    /// True while the params are being written. The write runs on a worker thread, so row-structure changes are
    /// refused until it finishes (see <see cref="GetSaveBlockReason"/>).
    /// </summary>
    public bool IsSaving => Volatile.Read(ref _saveInProgress) == 1;

    public async void Save(bool autoSave = false)
    {
        if (autoSave && !CFG.Current.ParamEditor_Allow_Automatic_Save)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _saveInProgress, 1, 0) != 0)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Param_Save_in_Progress"));
            return;
        }

        try
        {
            // Written here, on the UI thread, since the table names are edited from the UI
            ViewHandler.ActiveView?.ParamTableWindow.WriteTableGroupNames();

            // The bank write itself moves to a worker thread
            await SaveParams();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, e.Message, e);
        }
        finally
        {
            Volatile.Write(ref _saveInProgress, 0);
        }
    }

    public async Task<bool> SaveParams()
    {
        var bank = Project.Handler.ParamData.PrimaryBank;

        if (bank == null)
        {
            return false;
        }

        try
        {
            // The bank logs when the save is skipped because nothing has been modified
            Smithbox.Log(this, LOC.Get("PARAM_Params_Saving"));

            if (await bank.Save())
            {
                Smithbox.Log(this, LOC.Get("PARAM_Params_Saved"));

                // Which params had changes, so it's clear what this save wrote. Long lists are shortened.
                var modified = bank.LastSaveModifiedParams;

                if (modified.Count > 0)
                {
                    const int maxNames = 15;
                    var names = string.Join(", ", modified.Take(maxNames));

                    if (modified.Count > maxNames)
                    {
                        names += LOC.Get("PARAM_Params_Saved_More", modified.Count - maxNames);
                    }

                    Smithbox.Log(this, LOC.Get("PARAM_Params_Saved_Modified", modified.Count, names));
                }

                return true;
            }
        }
        catch (SavingFailedException e)
        {
            Smithbox.LogError(this, e.Message, e.Wrapped);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, e.Message, e);
        }

        return false;
    }

    /// <summary>
    /// Returns why an action can't run right now, or null if it can.
    /// While a save is running only field value changes are allowed: adding, removing or reordering rows would change
    /// a row list the save is iterating over on its worker thread. Field edits still keep their dirty mark, because the
    /// save only clears params that weren't edited after it started.
    /// </summary>
    public string GetSaveBlockReason(EditorAction action)
    {
        if (!IsSaving)
        {
            return null;
        }

        if (action is PropertiesChangedAction)
        {
            return null;
        }

        // e.g. a rename of several selected rows
        if (action is CompoundAction compound && compound.ContainsOnly<PropertiesChangedAction>())
        {
            return null;
        }

        return LOC.Get("PARAM_Action_Blocked_During_Save");
    }

    public async void SaveAll()
    {
        await Task.Yield();

        Save();
    }

    public List<string> GetUnsavedFiles()
    {
        var bank = Project.Handler.ParamData?.PrimaryBank;

        if (bank == null || !bank.IsDirty())
            return new();

        var names = (bank.Params?.Keys ?? Enumerable.Empty<string>())
            .Concat(bank.StayParams.Keys)
            .Where(bank.IsParamDirty)
            .Distinct()
            .OrderBy(e => e)
            .ToList();

        return names;
    }

    // Save sets IsSaving before its first await, so the prompt sees the save as running from this call on
    public void SaveUnsaved()
    {
        Save();
    }

    public bool IsSavingUnsaved => IsSaving;
    #endregion
}
