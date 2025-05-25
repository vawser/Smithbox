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
    private TextEditorScreen Editor;
    private TextPropertyDecorator Decorator;
    private TextSelectionManager Selection;
    private ActionManager EditorActionManager;

    public TextShortcuts(TextEditorScreen screen)
    {
        EditorActionManager = screen.EditorActionManager;
        Editor = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
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
            Editor.EntryCreationModal.ShowModal = true;
        }

        // Duplicate
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            Editor.ActionHandler.DuplicateEntries();
        }

        // Delete
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            Editor.ActionHandler.DeleteEntries();
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
        var selectionContext = Selection.CurrentWindowContext;
        var multiselect = Selection.FmgEntryMultiselect;

        if (Selection.SelectedFmgWrapper == null)
            return;

        var fmg = Selection.SelectedFmgWrapper.File;

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
        var selectionContext = Selection.CurrentWindowContext;

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