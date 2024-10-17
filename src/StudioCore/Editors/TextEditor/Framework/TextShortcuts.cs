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
}