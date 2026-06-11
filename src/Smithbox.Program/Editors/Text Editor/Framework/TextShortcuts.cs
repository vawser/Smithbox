using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

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

        // Save
        if (InputManager.IsPressed(KeybindID.Save))
        {
            Editor.Save();
        }

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_TextEditor_ToolWindow = !CFG.Current.Interface_TextEditor_ToolWindow;
        }

        if (activeView != null)
        {
            // Undo
            if (activeView.ActionManager.CanUndo())
            {
                if (InputManager.IsPressed(KeybindID.Undo))
                {
                    activeView.ActionManager.UndoAction();
                }

                if (InputManager.IsPressedOrRepeated(KeybindID.Undo_Repeat))
                {
                    activeView.ActionManager.UndoAction();
                }
            }

            // Redo
            if (activeView.ActionManager.CanRedo())
            {
                if (InputManager.IsPressed(KeybindID.Redo))
                {
                    activeView.ActionManager.RedoAction();
                }

                if (InputManager.IsPressedOrRepeated(KeybindID.Redo_Repeat))
                {
                    activeView.ActionManager.RedoAction();
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
                    ImGui.OpenPopup("textDuplicatePopup");
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
                    activeView.Selection.FocusFmgEntrySelection = true;
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

            // Select All
            if (FocusManager.IsFocus(EditorFocusContext.TextEditor_EntryList))
            {
                if (InputManager.IsPressed(KeybindID.SelectAll))
                {
                    multiselect.StoredEntries.Clear();

                    if (fmg != null)
                    {
                        for (int j = 0; j < fmg.Entries.Count; j++)
                        {
                            var tEntry = fmg.Entries[j];

                            var input = activeView.TextEntryList.EntryListFilter;
                            var exactMatch = activeView.TextEntryList.ExactEntryListFilter;

                            var isMatch = EditorFilters.IsMatch(
                                input, tEntry.ID.ToString(), exactMatch, tEntry.Text);

                            // Ignore normal match if a special conditional commands has been used
                            if (activeView.TextEntryList.UsedMatchCommands(input))
                            {
                                isMatch = activeView.TextEntryList.HandleMatchCommands(input, tEntry);
                            }

                            if (isMatch)
                            {
                                multiselect.StoredEntries.Add(j, tEntry);
                            }
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
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView != null)
        {
            // Copy Entry Contents
            if (FocusManager.IsFocus(EditorFocusContext.TextEditor_EntryList))
            {
                if (InputManager.IsPressed(KeybindID.Copy))
                {
                    activeView.ActionHandler.CopyEntryTextToClipboard(CFG.Current.TextEditor_Text_Clipboard_Include_ID);
                }
            }
        }
    }
}