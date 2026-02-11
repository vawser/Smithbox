using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;

namespace StudioCore.Editors.ModelEditor;

public class ModelShortcuts
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public ModelShortcuts(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (!FocusManager.IsInModelEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_ModelEditor_ToolWindow = !CFG.Current.Interface_ModelEditor_ToolWindow;
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

        // Actions
        activeView.CreateAction.OnShortcut();
        activeView.DuplicateAction.OnShortcut();
        activeView.DeleteAction.OnShortcut();
        activeView.FrameAction.OnShortcut();
        activeView.GotoAction.OnShortcut();
        activeView.PullToCameraAction.OnShortcut();
        activeView.ReorderAction.OnShortcut();

        GizmoState.OnShortcut();
    }
}
