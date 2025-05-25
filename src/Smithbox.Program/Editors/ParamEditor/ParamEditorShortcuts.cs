using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using Octokit;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Editors.ParamEditor.MassEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamEditorShortcuts
{
    private ParamEditorScreen Editor;

    public ParamEditorShortcuts(ParamEditorScreen Editor)
    {
        this.Editor = Editor;
    }

    public void Shortcuts()
    {
        if (!Editor._isShortcutPopupOpen && !Editor._isMEditPopupOpen && !Editor._isStatisticPopupOpen && !Editor._isSearchBarActive)
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
            {
                Editor.Save();
            }

            // Undo
            if (Editor.EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
            {
                Editor.EditorActionManager.UndoAction();
                Editor.Project.ParamData.RefreshParamDifferenceCacheTask();
            }

            // Undo (contant)
            if (Editor.EditorActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
            {
                Editor.EditorActionManager.UndoAction();
                Editor.Project.ParamData.RefreshParamDifferenceCacheTask();
            }

            // Redo
            if (Editor.EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
            {
                Editor.EditorActionManager.RedoAction();
                Editor.Project.ParamData.RefreshParamDifferenceCacheTask();
            }

            // Redo (constant)
            if (Editor.EditorActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
            {
                Editor.EditorActionManager.RedoAction();
                Editor.Project.ParamData.RefreshParamDifferenceCacheTask();
            }

            // Select All
            if (!ImGui.IsAnyItemActive() && Editor._activeView.Selection.ActiveParamExists() && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SelectAll))
            {
                Editor.Project.ParamData.PrimaryBank.ClipboardParam = Editor._activeView.Selection.GetActiveParam();

                foreach (Param.Row row in UICache.GetCached(Editor, (Editor._activeView.ViewIndex, Editor._activeView.Selection.GetActiveParam()),
                    () => Editor.MassEditHandler.rse.Search((Editor.Project.ParamData.PrimaryBank, Editor.Project.ParamData.PrimaryBank.Params[Editor._activeView.Selection.GetActiveParam()]),
                    Editor._activeView.Selection.GetCurrentRowSearchString(), true, true)))
                {
                    Editor._activeView.Selection.AddRowToSelection(row);
                }
            }

            // Copy
            if (!ImGui.IsAnyItemActive() && Editor._activeView.Selection.RowSelectionExists() && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CopyToClipboard))
            {
                Editor.CopySelectionToClipboard();
            }

            // Paste
            if (Editor.Project.ParamData.PrimaryBank.ClipboardRows.Count > 00 && Editor.Project.ParamData.PrimaryBank.ClipboardParam == Editor._activeView.Selection.GetActiveParam() && !ImGui.IsAnyItemActive() && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_PasteClipboard))
            {
                ImGui.OpenPopup("ctrlVPopup");
            }

            // Duplicate
            if (!ImGui.IsAnyItemActive() && Editor._activeView.Selection.RowSelectionExists() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
            {
                Editor.ParamTools.DuplicateRow();
            }

            // Delete
            if (!ImGui.IsAnyItemActive() && Editor._activeView.Selection.RowSelectionExists() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
            {
                Editor.DeleteSelection();
            }

            // Go to Row
            if (!ImGui.IsAnyItemActive() && Editor._activeView.Selection.RowSelectionExists() && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_GoToSelectedRow))
            {
                Editor.GotoSelectedRow = true;
            }

            // Copy Row ID
            if (!ImGui.IsAnyItemActive() && Editor._activeView.Selection.RowSelectionExists() && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CopyId))
            {
                Editor.ParamTools.CopyRowDetails();
            }

            // Copy Row ID and Name
            if (!ImGui.IsAnyItemActive() && Editor._activeView.Selection.RowSelectionExists() && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CopyIdAndName))
            {
                Editor.ParamTools.CopyRowDetails(true);
            }
        }

        // Create Param Pin Group
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateParamPinGroup))
        {
            Editor.PinGroupHandler.SetAutoGroupName("Param");
            Editor.PinGroupHandler.CreateParamGroup();
        }

        // Create Row Pin Group
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateRowPinGroup))
        {
            Editor.PinGroupHandler.SetAutoGroupName("Row");
            Editor.PinGroupHandler.CreateRowGroup();
        }

        // Create Field Pin Group
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateFieldPinGroup))
        {
            Editor.PinGroupHandler.SetAutoGroupName("Field");
            Editor.PinGroupHandler.CreateFieldGroup();
        }

        // Clear current Pinned Params
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedParams))
        {
            Editor.Project.PinnedParams = new();
        }

        // Clear current Pinned Rows
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedRows))
        {
            Editor.Project.PinnedRows = new();
        }

        // Clear current Pinned Fields
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedFields))
        {
            Editor.Project.PinnedFields = new();
        }

        // Show only Pinned Params
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_OnlyShowPinnedParams))
        {
            CFG.Current.Param_PinGroups_ShowOnlyPinnedParams = !CFG.Current.Param_PinGroups_ShowOnlyPinnedParams;
        }

        // Show only Pinned Rows
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_OnlyShowPinnedRows))
        {
            CFG.Current.Param_PinGroups_ShowOnlyPinnedRows = !CFG.Current.Param_PinGroups_ShowOnlyPinnedRows;
        }

        // Show only Pinned Fields
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_OnlyShowPinnedFields))
        {
            CFG.Current.Param_PinGroups_ShowOnlyPinnedFields = !CFG.Current.Param_PinGroups_ShowOnlyPinnedFields;
        }

        // Sort Rows
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SortRows))
        {
            Editor.ParamTools.SortRows();
        }

        // Execute Mass Editor
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExecuteMassEdit))
        {
            Editor.MassEditHandler.ApplyMassEdit(Editor.MassEditHandler.CurrentInput);
        }

        // View Mass Edit
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ViewMassEdit))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditRegex");
        }

        // Import CSV
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ImportCSV))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditCSVImport");
        }

        // Export CSV
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV))
        {
            EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/{ParamBank.RowGetType.AllRows}");
        }

        // Export CSV: Names
        if (!ImGui.IsAnyItemActive() && 
            Editor._activeView.Selection.RowSelectionExists() && 
            InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV_Names))
        {
            EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/2");
        }

        // Export CSV: Param
        if (!ImGui.IsAnyItemActive() &&
            Editor._activeView.Selection.RowSelectionExists() &&
            InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV_Param))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditCSVExport/0");
        }

        // Export CSV: All Rows
        if (!ImGui.IsAnyItemActive() &&
            Editor._activeView.Selection.RowSelectionExists() &&
            InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV_AllRows))
        {
            EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/AllRows");
        }

        // Export CSV: Modified Rows
        if (!ImGui.IsAnyItemActive() &&
            Editor._activeView.Selection.RowSelectionExists() &&
            InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV_ModifiedRows))
        {
            EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/ModifiedRows");
        }

        // Export CSV: Selected Rows
        if (!ImGui.IsAnyItemActive() &&
            Editor._activeView.Selection.RowSelectionExists() &&
            InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV_SelectedRows))
        {
            EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/SelectedRows");
        }

        //Hot Reload shortcut keys
        if (Editor.ParamReloader.CanReloadMemoryParams(Editor.Project.ParamData.PrimaryBank))
        {
            // Reload all PArams
            if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ReloadAllParams))
            {
                Editor.ParamReloader.ReloadMemoryParams(Editor.Project.ParamData.PrimaryBank, Editor.Project.ParamData.PrimaryBank.Params.Keys.ToArray());
            }
            // Reload Current Param
            else if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ReloadParam) && Editor._activeView.Selection.GetActiveParam() != null)
            {
                Editor.ParamReloader.ReloadMemoryParam(Editor.Project.ParamData.PrimaryBank, Editor._activeView.Selection.GetActiveParam());
            }
        }
    }
}
