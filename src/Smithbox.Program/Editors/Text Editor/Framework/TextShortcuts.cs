using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the detection of editor shortcuts
/// </summary>
public class TextShortcuts
{
    private TextEditorScreen Editor;
    private ProjectEntry Project;

    public TextShortcuts(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
        {
            Editor.Save();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SaveAll))
        {
            Editor.SaveAll();
        }

        if (Editor.EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            Editor.EditorActionManager.UndoAction();
        }

        if (Editor.EditorActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
        {
            Editor.EditorActionManager.UndoAction();
        }

        if (Editor.EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            Editor.EditorActionManager.RedoAction();
        }

        if (Editor.EditorActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
        {
            Editor.EditorActionManager.RedoAction();
        }

        if (Editor.Selection.CurrentWindowContext is TextEditorContext.FmgEntry)
        {
            // Create
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_CreateNewEntry))
            {
                Editor.EntryCreationModal.ShowModal = true;
            }

            // Configurable Duplicate
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntryPopup))
            {
                ImGui.OpenPopup("textDuplicatePopup");
            }

            // Standard Duplicate
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
            {
                Editor.ActionHandler.DuplicateEntries();
            }

            // Delete
            if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
            {
                Editor.ActionHandler.DeleteEntries();
            }
        }

        // Focus Selected Entry
        if (InputTracker.GetKeyDown(KeyBindings.Current.TEXT_FocusSelectedEntry))
        {
            Editor.Selection.FocusFmgEntrySelection = true;
        }
    }

    /// <summary>
    /// Select All in the FMG Entry list
    /// </summary>
    public void HandleSelectAll()
    {
        var selectionContext = Editor.Selection.CurrentWindowContext;
        var multiselect = Editor.Selection.FmgEntryMultiselect;

        if (Editor.Selection.SelectedFmgWrapper == null)
            return;

        var fmg = Editor.Selection.SelectedFmgWrapper.File;

        // Select All
        if (selectionContext is TextEditorContext.FmgEntry)
        {
            if (InputTracker.GetKey(KeyBindings.Current.TEXT_SelectAll))
            {
                multiselect.StoredEntries.Clear();

                if (fmg != null)
                {
                    for (int j = 0; j < fmg.Entries.Count; j++)
                    {
                        var tEntry = fmg.Entries[j];

                        if (Editor.Filters.IsFmgEntryFilterMatch(tEntry))
                        {
                            multiselect.StoredEntries.Add(j, tEntry);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Copy Entry Text in the FMG Entry list
    /// </summary>
    public void HandleCopyEntryText()
    {
        var selectionContext = Editor.Selection.CurrentWindowContext;

        // Copy Entry Contents
        if (selectionContext is TextEditorContext.FmgEntry)
        {
            if (InputTracker.GetKey(KeyBindings.Current.TEXT_CopyEntryContents))
            {
                Editor.ActionHandler.CopyEntryTextToClipboard(CFG.Current.TextEditor_TextCopy_IncludeID);
            }
        }
    }
}