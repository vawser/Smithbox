using Andre.Formats;
using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

public class ParamShortcuts
{
    private ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ParamShortcuts(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Shortcuts()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (!FocusManager.IsInParamEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_ParamEditor_ToolWindow = !CFG.Current.Interface_ParamEditor_ToolWindow;
        }

        if (AllowContextualShortcuts(activeView))
        {
            // Save
            if (InputManager.IsPressed(KeybindID.Save))
            {
                Editor.Save();
            }

            // Undo
            if (InputManager.IsPressed(KeybindID.Undo))
            {
                if (Editor.ActionManager.CanUndo())
                {
                    Editor.ActionManager.UndoAction();
                    Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
                }
            }

            // Redo
            if (InputManager.IsPressed(KeybindID.Redo))
            {
                if (Editor.ActionManager.CanRedo())
                {
                    Editor.ActionManager.RedoAction();
                    Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
                }
            }

            // Select All
            if (InputManager.IsPressed(KeybindID.SelectAll))
            {
                if (activeView.Selection.ActiveParamExists())
                {
                    activeView.ParamRowWindow.SelectAll();
                }
            }

            // Copy
            if (InputManager.IsPressed(KeybindID.Copy))
            {
                if (activeView.Selection.RowSelectionExists())
                {
                    Editor.Clipboard.CopySelectionToClipboard(activeView);
                }
            }

            // Paste
            if (InputManager.IsPressed(KeybindID.Paste))
            {
                if (Editor.Clipboard.HasClipboardContents(activeView))
                {
                    Editor.PasteMenu.Open();
                }
            }

            // Duplicate
            if (InputManager.IsPressed(KeybindID.Duplicate))
            {
                if (activeView.Selection.RowSelectionExists())
                {
                    if (FocusManager.Focus is not EditorFocusContext.ParamEditor_TableList)
                    {
                        ParamRowDuplicate.ApplyDuplicate(activeView);
                    }
                }
            }

            // Delete
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                if (activeView.Selection.RowSelectionExists())
                {
                    if (FocusManager.Focus is not EditorFocusContext.ParamEditor_TableList)
                    {
                        ParamRowDelete.ApplyDelete(activeView);
                    }
                }
            }

            // Jump
            if (InputManager.IsPressed(KeybindID.Jump))
            {
                if (activeView.Selection.RowSelectionExists())
                {
                    activeView.JumpToSelectedRow = true;
                }
            }
        }

        // Sort Rows
        if (InputManager.IsPressed(KeybindID.ParamEditor_RowList_Sort_Rows))
        {
            ParamRowTools.SortRows(activeView);
        }

        // Apply Mass Edit
        if (InputManager.IsPressed(KeybindID.ParamEditor_Apply_Mass_Edit))
        {
            activeView.MassEdit.ApplyMassEdit(activeView.MassEdit.CurrentMassEditInput);
        }

        // Open Mass Edit Menu
        if (InputManager.IsPressed(KeybindID.ParamEditor_View_Mass_Edit))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditRegex");
        }

        // Import CSV
        if (!ImGui.IsAnyItemActive() && activeView.Selection.ActiveParamExists())
        {
            if (InputManager.IsPressed(KeybindID.ParamEditor_Import_CSV))
            {
                EditorCommandQueue.AddCommand(@"param/menu/massEditCSVImport");
            }

            if (InputManager.IsPressed(KeybindID.ParamEditor_Export_CSV))
            {
                EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/{ParamUpgradeRowGetType.AllRows}");
            }

            if (activeView.Selection.RowSelectionExists())
            {
                if (InputManager.IsPressed(KeybindID.ParamEditor_Export_CSV_Names))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/2");
                }

                if (InputManager.IsPressed(KeybindID.ParamEditor_Export_CSV_Param))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/massEditCSVExport/0");
                }

                if (InputManager.IsPressed(KeybindID.ParamEditor_Export_CSV_All_Rows))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/AllRows");
                }

                if (InputManager.IsPressed(KeybindID.ParamEditor_Export_CSV_Modified_Rows))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/ModifiedRows");
                }

                if (InputManager.IsPressed(KeybindID.ParamEditor_Export_CSV_Selected_Rows))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/SelectedRows");
                }
            }
        }

        // Reload All Params
        if (InputManager.IsPressed(KeybindID.ParamEditor_Reload_All_Params))
        {
            Editor.ToolMenu.ParamReloader.ReloadMemoryParams(
                activeView.GetPrimaryBank(),
                activeView.GetPrimaryBank().Params.Keys.ToArray());
        }

        // Reload Current Param
        if (InputManager.IsPressed(KeybindID.ParamEditor_Reload_Selected_Param))
        {
            if (activeView.Selection.GetActiveParam() != null)
            {
                Editor.ToolMenu.ParamReloader.ReloadMemoryParam(
                    activeView.GetPrimaryBank(),
                    activeView.Selection.GetActiveParam());
            }
        }
    }
    public bool AllowContextualShortcuts(ParamEditorView curView)
    {
        if (ImGui.IsAnyItemActive())
        {
            return false;
        }

        if (curView.MassEdit.DisplayMassEditPopup)
        {
            return false;
        }

        if (curView._isSearchBarActive)
        {
            return false;
        }

        if (Editor.StatisticsMenu._isStatisticPopupOpen)
        {
            return false;
        }

        if (Editor.PasteMenu.DisplayPasteMenu)
        {
            return false;
        }

        return true;
    }
}
