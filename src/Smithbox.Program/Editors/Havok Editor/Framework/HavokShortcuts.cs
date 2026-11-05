using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokShortcuts
{
    public HavokEditorScreen Editor;
    public ProjectEntry Project;

    public HavokShortcuts(HavokEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (!FocusManager.IsInHavokEditor())
            return;

        // Saves every binder with unsaved changes (marked * in the binder list)
        if (InputManager.IsPressed(KeybindID.Save))
        {
            Editor.SaveAll();
        }

        // While a text box has focus, its keys belong to the text box:
        // Delete, Copy, Paste and Undo would otherwise act on the files or the action history
        if (ImGui.GetIO().WantTextInput)
            return;

        if (activeView != null)
        {
            // Undo
            if (activeView.ActionManager.CanUndo())
            {
                if (InputManager.IsPressed(KeybindID.Undo))
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
            }

            if (FocusManager.IsFocus(EditorFocusContext.HavokEditor_CategoryList))
            {
                activeView.CategoryView.Shortcuts();
            }

            if (FocusManager.IsFocus(EditorFocusContext.HavokEditor_BinderList))
            {
                activeView.BinderView.Shortcuts();
            }

            if (FocusManager.IsFocus(EditorFocusContext.HavokEditor_FileList))
            {
                activeView.FileView.Shortcuts();
            }

            if (FocusManager.IsFocus(EditorFocusContext.HavokEditor_Properties))
            {
                activeView.PropertyView.Shortcuts();
            }

            if (FocusManager.IsFocus(EditorFocusContext.HavokEditor_Tools))
            {
                activeView.Tools.Shortcuts();
            }
        }
    }
}
