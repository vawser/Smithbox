using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the detection of editor shortcuts
/// </summary>
public class TextShortcuts
{
    private TextEditorScreen Screen;
    private TextPropertyDecorator Decorator;
    private TextSelectionManager Selection;
    private ActionManager EditorActionManager;

    public TextShortcuts(TextEditorScreen screen)
    {
        EditorActionManager = screen.EditorActionManager;
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    public void Monitor()
    {
        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            EditorActionManager.RedoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
        {
            EditorActionManager.RedoAction();
        }

        // Create
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_CreateNewEntry))
        {
            Screen.EntryCreationModal.ShowModal = true;
        }

        // Duplicate
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            Screen.ActionHandler.DuplicateEntries();
        }

        // Delete
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            Screen.ActionHandler.DeleteEntries();
        }

        // Focus Selected Entry
        if (InputTracker.GetKeyDown(KeyBindings.Current.TEXT_FocusSelectedEntry))
        {
            Screen.Selection.FocusFmgEntrySelection = true;
        }
    }

    /// <summary>
    /// Select All in the FMG Entry list
    /// </summary>
    public void HandleSelectAll()
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        // Select All
        if (Selection.CurrentSelectionContext is TextSelectionContext.FmgEntry)
        {
            if (InputTracker.GetKey(KeyBindings.Current.TEXT_SelectAll))
            {
                Selection.FmgEntryMultiselect.StoredEntries.Clear();
                for (int j = 0; j < Selection.SelectedFmg.Entries.Count; j++)
                {
                    var tEntry = Selection.SelectedFmg.Entries[j];

                    if (editor.Filters.IsFmgEntryFilterMatch(tEntry))
                    {
                        Selection.FmgEntryMultiselect.StoredEntries.Add(j, tEntry);
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
        // Copy Entry Contents
        if (Selection.CurrentSelectionContext is TextSelectionContext.FmgEntry)
        {
            if (InputTracker.GetKey(KeyBindings.Current.TEXT_CopyEntryContents))
            {
                Screen.ActionHandler.CopyEntryTextToClipboard(CFG.Current.TextEditor_TextCopy_IncludeID);
            }
        }
    }
}