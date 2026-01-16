using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

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
            }

            if (InputManager.IsPressedOrRepeated(InputAction.Undo_Repeat))
            {
                Editor.EditorActionManager.UndoAction();
            }
        }

        // Redo
        if (Editor.EditorActionManager.CanRedo())
        {
            if (InputManager.IsPressed(InputAction.Redo))
            {
                Editor.EditorActionManager.RedoAction();
            }

            if (InputManager.IsPressedOrRepeated(InputAction.Redo_Repeat))
            {
                Editor.EditorActionManager.RedoAction();
            }
        }

        if (FocusManager.IsFocus(EditorFocusContext.TextEditor_EntryList))
        {
            // Create
            if (InputManager.IsPressed(InputAction.TextEditor_Create_New_Entry))
            {
                Editor.EntryCreationModal.ShowModal = true;
            }

            // TODO: remove this if we add Copy/Paste functionality
            // Configurable Duplicate
            if (InputManager.IsPressed(InputAction.TextEditor_Configurable_Duplicate))
            {
                ImGui.OpenPopup("textDuplicatePopup");
            }

            // Standard Duplicate
            if (InputManager.IsPressed(InputAction.Duplicate))
            {
                Editor.ActionHandler.DuplicateEntries();
            }

            // Delete
            if (InputManager.IsPressed(InputAction.Delete))
            {
                Editor.ActionHandler.DeleteEntries();
            }
        }

        // Focus Selected Entry
        if (InputManager.IsPressed(InputAction.Jump))
        {
            Editor.Selection.FocusFmgEntrySelection = true;
        }
    }

    /// <summary>
    /// Select All in the FMG Entry list
    /// </summary>
    public void HandleSelectAll()
    {
        var multiselect = Editor.Selection.FmgEntryMultiselect;

        if (Editor.Selection.SelectedFmgWrapper == null)
            return;

        var fmg = Editor.Selection.SelectedFmgWrapper.File;

        // Select All
        if (FocusManager.IsFocus(EditorFocusContext.TextEditor_EntryList))
        {
            if (InputManager.IsPressed(InputAction.SelectAll))
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
        if (FocusManager.IsFocus(EditorFocusContext.TextEditor_EntryList))
        {
            if (InputManager.IsPressed(InputAction.Copy))
            {
                Editor.ActionHandler.CopyEntryTextToClipboard(CFG.Current.TextEditor_TextCopy_IncludeID);
            }
        }
    }
}