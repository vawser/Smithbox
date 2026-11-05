using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Linq;

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
        var activeView = Editor.ViewHandler.ActiveView;

        // Save: every container with unsaved changes (marked *), not just the selected one.
        // Global Replace, Language Sync and Import change many containers at once.
        if (InputManager.IsPressed(KeybindID.Save))
        {
            Editor.SaveAll();
        }

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_TextEditor_ToolWindow = !CFG.Current.Interface_TextEditor_ToolWindow;
        }

        // While a text box has keyboard focus (search bar, entry text, ID input),
        // its keys belong to the text box: Delete, Ctrl+Z, Ctrl+D and Insert
        // must not also act on the entry list.
        if (ImGui.GetIO().WantTextInput)
            return;

        if (activeView != null)
        {
            // Undo / Redo: each selects the entry it changed afterwards
            if (activeView.ActionManager.CanUndo())
            {
                if (InputManager.IsPressed(KeybindID.Undo))
                {
                    activeView.UndoAndReveal();
                }

                if (InputManager.IsPressedOrRepeated(KeybindID.Undo_Repeat))
                {
                    activeView.UndoAndReveal();
                }
            }

            if (activeView.ActionManager.CanRedo())
            {
                if (InputManager.IsPressed(KeybindID.Redo))
                {
                    activeView.RedoAndReveal();
                }

                if (InputManager.IsPressedOrRepeated(KeybindID.Redo_Repeat))
                {
                    activeView.RedoAndReveal();
                }
            }

            if (FocusManager.IsFocus(EditorFocusContext.TextEditor_EntryList))
            {
                // Create
                if (InputManager.IsPressed(KeybindID.TextEditor_Create_New_Entry))
                {
                    activeView.TextEntryCreator.ShowModal = true;
                }

                // TODO: remove this if we add Copy/Paste functionality
                // Configurable Duplicate
                if (InputManager.IsPressed(KeybindID.TextEditor_Configurable_Duplicate))
                {
                    activeView.TextDuplicatePopup.OpenRequested = true;
                }

                // Standard Duplicate
                if (InputManager.IsPressed(KeybindID.Duplicate))
                {
                    activeView.ActionHandler.DuplicateEntries();
                }

                // Delete
                if (InputManager.IsPressed(KeybindID.Delete))
                {
                    activeView.ActionHandler.DeleteEntries();
                }

                // Focus Selected Entry
                if (InputManager.IsPressed(KeybindID.Jump))
                {
                    activeView.TextEntryList.FocusSelection = true;
                }
            }
        }
    }

    /// <summary>
    /// Select All in the FMG Entry list
    /// </summary>
    public void HandleSelectAll()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView != null)
        {
            var multiselect = activeView.Selection.FmgEntryMultiselect;

            if (activeView.Selection.SelectedFmgWrapper == null)
                return;

            var fmg = activeView.Selection.SelectedFmgWrapper.File;

            multiselect.StoredEntries.Clear();

            if (fmg != null)
            {
                for (int j = 0; j < fmg.Entries.Count; j++)
                {
                    var tEntry = fmg.Entries[j];

                    // Same check as the list itself, so hidden null entries aren't selected
                    if (activeView.TextEntryList.IsEntryVisible(tEntry))
                    {
                        multiselect.StoredEntries.Add(j, tEntry);
                    }
                }

                // Give Delete/Duplicate a primary selection to work from
                if (multiselect.StoredEntries.Count > 0 && activeView.Selection._selectedFmgEntryIndex == -1)
                {
                    var first = multiselect.StoredEntries.First();
                    activeView.Selection._selectedFmgEntryIndex = first.Key;
                    activeView.Selection._selectedFmgEntry = first.Value;
                }
            }
        }
    }

    /// <summary>
    /// Copy Entry Text in the FMG Entry list
    /// </summary>
    public void HandleCopyEntryText()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView != null)
        {
            activeView.ActionHandler.CopyEntryTextToClipboard(CFG.Current.TextEditor_Text_Clipboard_Include_ID);
        }
    }
}