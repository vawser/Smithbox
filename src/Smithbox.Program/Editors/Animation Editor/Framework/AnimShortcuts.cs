using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class AnimShortcuts
{
    public AnimEditorScreen Editor;
    public ProjectEntry Project;

    public AnimShortcuts(AnimEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (!FocusManager.IsInAnimEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_AnimEditor_ToolWindow = !CFG.Current.Interface_AnimEditor_ToolWindow;
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

        GizmoState.OnShortcut();
    }
}
