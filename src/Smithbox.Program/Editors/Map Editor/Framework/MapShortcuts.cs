using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using Veldrid;

namespace StudioCore.Editors.MapEditor;

public class MapShortcuts
{
    public MapEditorScreen Editor;

    public MapShortcuts(MapEditorScreen screen)
    {
        Editor = screen;
    }

    public void Monitor()
    {
        if (!FocusManager.IsInMapEditor())
            return;

        if(Editor.MapViewportView.ViewportUsingKeyboard &&
            ImGui.IsAnyItemActive())
            return;

        // Save
        if(InputManager.IsPressed(InputAction.Save))
        {
            Editor.Save();
        }

        // Undo
        if (Editor.EditorActionManager.CanUndo())
        {
            if (InputManager.IsPressed(InputAction.Undo))
            {
                Editor.EditorActionManager.UndoAction();
            }

            if (InputManager.IsPressedOrRepeated(InputAction.Undo_Repeat))
            {
                Editor.EditorActionManager.UndoAction();
            }
        }

        // Redo
        if (Editor.EditorActionManager.CanRedo())
        {
            if (InputManager.IsPressed(InputAction.Redo))
            {
                Editor.EditorActionManager.RedoAction();
            }

            if (InputManager.IsPressedOrRepeated(InputAction.Redo_Repeat))
            {
                Editor.EditorActionManager.RedoAction();
            }
        }

        // Cycle Gizmo Translation Mode
        if (InputManager.IsPressed(InputAction.Cycle_Gizmo_Translation_Mode))
        {
            Gizmos.Mode = Gizmos.GizmosMode.Translate;
        }

        // Cycle Gizmo Rotation Mode
        if (InputManager.IsPressed(InputAction.Cycle_Gizmo_Rotation_Mode))
        {
            Gizmos.Mode = Gizmos.GizmosMode.Rotate;
        }

        // Cycle Gizmo Origin Mode
        if (InputManager.IsPressed(InputAction.Cycle_Gizmo_Origin_Mode))
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
        if (InputManager.IsPressed(InputAction.Cycle_Gizmo_Space_Mode))
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

        // Actions
        Editor.CreateAction.OnShortcut();
        Editor.DuplicateAction.OnShortcut();
        Editor.DeleteAction.OnShortcut();
        Editor.DuplicateToMapAction.OnShortcut();
        Editor.RotateAction.OnShortcut();
        Editor.ScrambleAction.OnShortcut();
        Editor.ReplicateAction.OnShortcut();
        Editor.RenderTypeAction.OnShortcut();
        Editor.ReorderAction.OnShortcut();
        Editor.GotoAction.OnShortcut();
        Editor.GameVisibilityAction.OnShortcut();
        Editor.FrameAction.OnShortcut();
        Editor.PullToCameraAction.OnShortcut();
        Editor.EditorVisibilityAction.OnShortcut();
        Editor.SelectionOutlineAction.OnShortcut();
        Editor.AdjustToGridAction.OnShortcut();
        Editor.SelectAllAction.OnShortcut();
        Editor.EntityInfoAction.OnShortcut();

        // Tools
        Editor.MassEditTool.OnShortcut();
        Editor.DisplayGroupTool.OnShortcut();
        Editor.PrefabTool.OnShortcut();
        Editor.SelectionGroupTool.OnShortcut();
        Editor.RotationIncrementTool.OnShortcut();
        Editor.PositionIncrementTool.OnShortcut();
    }
}
