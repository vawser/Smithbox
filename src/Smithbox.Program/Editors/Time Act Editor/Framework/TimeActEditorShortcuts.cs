using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActEditorShortcuts
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActEditorShortcuts(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (!FocusManager.IsInTimeActEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_TimeActEditor_ToolWindow = !CFG.Current.Interface_TimeActEditor_ToolWindow;
        }

        if (activeView == null)
            return;

        if (ImGui.IsAnyItemActive() || activeView.ViewportWindow.ViewportUsingKeyboard)
            return;

        // Save
        if (InputManager.IsPressed(KeybindID.Save))
        {
            Editor.Save();
        }

        // Undo
        if (activeView.ViewportActionManager.CanUndo())
        {
            if (InputManager.IsPressed(KeybindID.Undo))
            {
                activeView.ViewportActionManager.UndoAction();
            }

            if (InputManager.IsPressedOrRepeated(KeybindID.Undo_Repeat))
            {
                activeView.ViewportActionManager.UndoAction();
            }
        }

        // Redo
        if (activeView.ViewportActionManager.CanRedo())
        {
            if (InputManager.IsPressed(KeybindID.Redo))
            {
                activeView.ViewportActionManager.RedoAction();
            }

            if (InputManager.IsPressedOrRepeated(KeybindID.Redo_Repeat))
            {
                activeView.ViewportActionManager.RedoAction();
            }
        }
    }
}
