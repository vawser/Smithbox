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

        if (activeView.ViewportHandler.ActiveViewport.Viewport == null)
            return;

        if (activeView.ViewportHandler.ActiveViewport.Viewport.ViewportCamera.RightMousePressed)
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

        // Undo and Redo belong to a text field being typed in, not to the map
        var typing = ImGui.GetIO().WantTextInput;

        // Undo. Modifiers are matched as "at least these", so Undo Repeat (Ctrl+Alt+Z) also matches Undo (Ctrl+Z):
        // the more specific binding is checked first, and only one of the two acts in a frame
        if (!typing && activeView.ViewportActionManager.CanUndo())
        {
            if (InputManager.IsPressedOrRepeated(KeybindID.Undo_Repeat))
            {
                activeView.ViewportActionManager.UndoAction();
            }
            else if (InputManager.IsPressed(KeybindID.Undo) && !InputManager.IsDown(KeybindID.Undo_Repeat))
            {
                activeView.ViewportActionManager.UndoAction();
            }
        }

        // Redo, the same way
        if (!typing && activeView.ViewportActionManager.CanRedo())
        {
            if (InputManager.IsPressedOrRepeated(KeybindID.Redo_Repeat))
            {
                activeView.ViewportActionManager.RedoAction();
            }
            else if (InputManager.IsPressed(KeybindID.Redo) && !InputManager.IsDown(KeybindID.Redo_Repeat))
            {
                activeView.ViewportActionManager.RedoAction();
            }
        }

        // The Map Data window holds input fields, so don't treat typing in them as actions
        var isMapDataFocus = FocusManager.IsFocus(EditorFocusContext.MapEditor_MapData) && !ImGui.GetIO().WantTextInput;

        // Actions
        if (FocusManager.IsFocus(EditorFocusContext.MapEditor_Viewport) || FocusManager.IsFocus(EditorFocusContext.MapEditor_ContentTree) || isMapDataFocus)
        {
            if (InputManager.IsPressed(KeybindID.Copy))
            {
                activeView.DuplicateToMapAction.OnCopy(activeView);
            }

            if (InputManager.IsPressed(KeybindID.Paste))
            {
                activeView.DuplicateToMapAction.OnPaste(activeView);
            }

            activeView.CreateAction.OnShortcut();
            activeView.DuplicateAction.OnShortcut();
            activeView.DeleteAction.OnShortcut();
            activeView.DuplicateToMapAction.OnShortcut();
            activeView.TranslateAction.OnShortcut();
            activeView.RotateAction.OnShortcut();
            activeView.ScrambleAction.OnShortcut();
            activeView.ReplicateAction.OnShortcut();
            activeView.GameVisibilityAction.OnShortcut();
            activeView.EditorVisibilityAction.OnShortcut();
            activeView.SelectCollisionRefAction.OnShortcut();
            activeView.GotoAction.OnShortcut();
            activeView.FrameAction.OnShortcut();
            activeView.SelectAllAction.OnShortcut();

            if (InputManager.IsPressed(KeybindID.MapEditor_Deselect_All))
            {
                activeView.ViewportSelection.ClearSelection();
            }
        }

        if (FocusManager.IsFocus(EditorFocusContext.MapEditor_ContentTree) || isMapDataFocus)
        {
            activeView.ReorderAction.OnShortcut();
        }

        if (FocusManager.IsFocus(EditorFocusContext.MapEditor_Viewport))
        {
            activeView.PullToCameraAction.OnShortcut();
            activeView.AdjustToGridAction.OnShortcut();
        }

        activeView.SelectionOutlineAction.OnShortcut();
        activeView.EntityInfoAction.OnShortcut();

        // Tools
        activeView.DisplayGroupTool.OnShortcut();
        activeView.PrefabTool.OnShortcut();
        activeView.PatrolDrawManager.OnShortcut();

        GizmoState.OnShortcut();
    }
}
