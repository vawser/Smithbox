using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
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
        Editor.ToolMenu.CreateAction.OnShortcut();
        Editor.ToolMenu.DuplicateAction.OnShortcut();
        Editor.ToolMenu.DeleteAction.OnShortcut();
        Editor.ToolMenu.FrameAction.OnShortcut();
        Editor.ToolMenu.GotoAction.OnShortcut();
        Editor.ToolMenu.PullToCameraAction.OnShortcut();
        Editor.ToolMenu.ReorderAction.OnShortcut();

        // Cycle Gizmo Translation Mode
        if (InputManager.IsPressed(KeybindID.Cycle_Gizmo_Translation_Mode))
        {
            Gizmos.Mode = Gizmos.GizmosMode.Translate;
        }

        // Cycle Gizmo Rotation Mode
        if (InputManager.IsPressed(KeybindID.Cycle_Gizmo_Rotation_Mode))
        {
            Gizmos.Mode = Gizmos.GizmosMode.Rotate;
        }

        // Cycle Gizmo Origin Mode
        if (InputManager.IsPressed(KeybindID.Cycle_Gizmo_Origin_Mode))
        {
            if (Gizmos.Origin == Gizmos.GizmosOrigin.World)
            {
                Gizmos.Origin = Gizmos.GizmosOrigin.BoundingBox;
            }
            else if (Gizmos.Origin == Gizmos.GizmosOrigin.BoundingBox)
            {
                Gizmos.Origin = Gizmos.GizmosOrigin.World;
            }
        }

        // Cycle Gizmo Space Mode
        if (InputManager.IsPressed(KeybindID.Cycle_Gizmo_Space_Mode))
        {
            if (Gizmos.Space == Gizmos.GizmosSpace.Local)
            {
                Gizmos.Space = Gizmos.GizmosSpace.World;
            }
            else if (Gizmos.Space == Gizmos.GizmosSpace.World)
            {
                Gizmos.Space = Gizmos.GizmosSpace.Local;
            }
        }
    }
}
