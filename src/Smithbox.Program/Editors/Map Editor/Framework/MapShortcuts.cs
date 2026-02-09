using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using Veldrid;

namespace StudioCore.Editors.MapEditor;

public class MapShortcuts
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public MapShortcuts(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        if (!FocusManager.IsInMapEditor())
            return;

        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if(activeView.ViewportWindow.ViewportUsingKeyboard &&
            ImGui.IsAnyItemActive())
            return;

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_MapEditor_ToolWindow = !CFG.Current.Interface_MapEditor_ToolWindow;
        }

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

        // Actions
        activeView.CreateAction.OnShortcut();
        activeView.DuplicateAction.OnShortcut();
        activeView.DeleteAction.OnShortcut();
        activeView.DuplicateToMapAction.OnShortcut();
        activeView.RotateAction.OnShortcut();
        activeView.ScrambleAction.OnShortcut();
        activeView.ReplicateAction.OnShortcut();
        activeView.RenderTypeAction.OnShortcut();
        activeView.ReorderAction.OnShortcut();
        activeView.GotoAction.OnShortcut();
        activeView.GameVisibilityAction.OnShortcut();
        activeView.FrameAction.OnShortcut();
        activeView.PullToCameraAction.OnShortcut();
        activeView.EditorVisibilityAction.OnShortcut();
        activeView.SelectionOutlineAction.OnShortcut();
        activeView.AdjustToGridAction.OnShortcut();
        activeView.SelectAllAction.OnShortcut();
        activeView.EntityInfoAction.OnShortcut();
        activeView.SelectCollisionRefAction.OnShortcut();

        // Tools
        activeView.MassEditTool.OnShortcut();
        activeView.DisplayGroupTool.OnShortcut();
        activeView.PrefabTool.OnShortcut();
        activeView.SelectionGroupTool.OnShortcut();
        activeView.RotationIncrementTool.OnShortcut();
        activeView.PositionIncrementTool.OnShortcut();
        activeView.PatrolDrawManager.OnShortcut();

        GizmoState.OnShortcut();
    }
}
