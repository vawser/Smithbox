using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Linq;

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
        var activeView = Editor._activeView;
        var paramData = Editor.Project.Handler.ParamData;
        var primaryBank = Editor.Project.Handler.ParamData.PrimaryBank;
        var activeParamExists = Editor._activeView.Selection.ActiveParamExists();
        var rowSelectionExists = Editor._activeView.Selection.RowSelectionExists();
        var activeParam = Editor._activeView.Selection.GetActiveParam();

        if (!Editor._isShortcutPopupOpen && !Editor._isMEditPopupOpen && !Editor._isStatisticPopupOpen && !Editor._isSearchBarActive)
        {
            // Save
            if (InputManager.IsPressed(InputAction.Save))
            {
                Editor.Save();
            }

            // Undo
            if (Editor.EditorActionManager.CanUndo())
            {
                if (InputManager.IsPressed(InputAction.Undo))
                {
                    Editor.EditorActionManager.UndoAction();
                    paramData.RefreshParamDifferenceCacheTask();
                }

                if (InputManager.IsPressedOrRepeated(InputAction.Undo_Repeat))
                {
                    Editor.EditorActionManager.UndoAction();
                    paramData.RefreshParamDifferenceCacheTask();
                }
            }

            // Redo
            if (Editor.EditorActionManager.CanRedo())
            {
                if (InputManager.IsPressed(InputAction.Redo))
                {
                    Editor.EditorActionManager.RedoAction();
                    paramData.RefreshParamDifferenceCacheTask();
                }

                if (InputManager.IsPressedOrRepeated(InputAction.Redo_Repeat))
                {
                    Editor.EditorActionManager.RedoAction();
                    paramData.RefreshParamDifferenceCacheTask();
                }
            }

            // Select All
            if (!ImGui.IsAnyItemActive() && activeParamExists)
            {
                if (InputManager.IsPressed(InputAction.SelectAll))
                {
                    primaryBank.ClipboardParam = activeView.Selection.GetActiveParam();

                    foreach (Param.Row row in UICache.GetCached(
                        Editor, (activeView.ViewIndex, activeView.Selection.GetActiveParam()),
                        () => Editor.MassEditHandler.rse.Search((primaryBank, primaryBank.Params[activeView.Selection.GetActiveParam()]),
                        activeView.Selection.GetCurrentRowSearchString(), true, true)))
                    {
                        if (activeView.TableGroupView.IsInTableGroupMode(activeParam))
                        {
                            if (row.ID == activeView.TableGroupView.CurrentTableGroup)
                            {
                                activeView.Selection.AddRowToSelection(row);
                            }
                        }
                        else
                        {
                            activeView.Selection.AddRowToSelection(row);
                        }
                    }
                }
            }

            // Copy
            if (!ImGui.IsAnyItemActive() && rowSelectionExists)
            {
                if (InputManager.IsPressed(InputAction.Copy))
                {
                    Editor.CopySelectionToClipboard();
                }
            }

            // Paste
            if (primaryBank.ClipboardRows.Count > 00 && primaryBank.ClipboardParam == activeParam && !ImGui.IsAnyItemActive())
            {
                if (InputManager.IsPressed(InputAction.Paste))
                {
                    ImGui.OpenPopup("ctrlVPopup");
                }
            }

            // Duplicate
            if (!ImGui.IsAnyItemActive() && rowSelectionExists)
            {
                if (InputManager.IsPressed(InputAction.Duplicate))
                {
                    if (!FocusManager.IsFocus(EditorFocusContext.ParamEditor_TableList))
                    {
                        Editor.ParamToolView.DuplicateRow();
                    }
                }
            }

            // Delete
            if (!ImGui.IsAnyItemActive() && rowSelectionExists)
            {
                if (InputManager.IsPressed(InputAction.Delete))
                {
                    if (!FocusManager.IsFocus(EditorFocusContext.ParamEditor_TableList))
                    {
                        Editor.DeleteSelection();
                    }
                }
            }

            // Go to Row
            if (!ImGui.IsAnyItemActive() && rowSelectionExists)
            {
                if (InputManager.IsPressed(InputAction.Jump))
                {
                    Editor.GotoSelectedRow = true;
                }
            }
        }

        // Sort Rows
        if (InputManager.IsPressed(InputAction.ParamEditor_RowList_Sort_Rows))
        {
            Editor.ParamToolView.SortRows();
        }

        // Execute Mass Editor
        if (InputManager.IsPressed(InputAction.ParamEditor_Apply_Mass_Edit))
        {
            Editor.MassEditHandler.ApplyMassEdit(Editor.MassEditHandler.CurrentInput);
        }

        // View Mass Edit
        if (InputManager.IsPressed(InputAction.ParamEditor_View_Mass_Edit))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditRegex");
        }

        // Import CSV
        if (!ImGui.IsAnyItemActive() && activeParamExists)
        {
            if (InputManager.IsPressed(InputAction.ParamEditor_Import_CSV))
            {
                EditorCommandQueue.AddCommand(@"param/menu/massEditCSVImport");
            }

            if (InputManager.IsPressed(InputAction.ParamEditor_Export_CSV))
            {
                EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/{ParamUpgradeRowGetType.AllRows}");
            }

            if (rowSelectionExists)
            {
                if (InputManager.IsPressed(InputAction.ParamEditor_Export_CSV_Names))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/2");
                }

                if (InputManager.IsPressed(InputAction.ParamEditor_Export_CSV_Param))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/massEditCSVExport/0");
                }

                if (InputManager.IsPressed(InputAction.ParamEditor_Export_CSV_All_Rows))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/AllRows");
                }

                if (InputManager.IsPressed(InputAction.ParamEditor_Export_CSV_Modified_Rows))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/ModifiedRows");
                }

                if (InputManager.IsPressed(InputAction.ParamEditor_Export_CSV_Selected_Rows))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/SelectedRows");
                }
            }
        }

        if (Editor.ParamReloader.CanReloadMemoryParams(primaryBank))
        {
            // Reload All Params
            if (InputManager.IsPressed(InputAction.ParamEditor_Reload_All_Params))
            {
                Editor.ParamReloader.ReloadMemoryParams(primaryBank, primaryBank.Params.Keys.ToArray());
            }

            // Reload Selected Param
            if (activeParamExists)
            {
                if (InputManager.IsPressed(InputAction.ParamEditor_Reload_Selected_Param))
                {
                    Editor.ParamReloader.ReloadMemoryParam(primaryBank, activeParam);
                }
            }
        }
    }
}
