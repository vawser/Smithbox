using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using Veldrid;

namespace StudioCore.Editors.TextureViewer;

public class TexShortcuts
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexShortcuts(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_TextureViewer_ToolWindow = !CFG.Current.Interface_TextureViewer_ToolWindow;
        }

        if (activeView == null)
            return;

        if (FocusManager.IsInTextureViewer())
        {
            // Save
            if (InputManager.IsPressed(KeybindID.Save))
            {
                Editor.Save();
            }

            // Undo / Redo
            if (activeView.ActionManager.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
            {
                activeView.ActionManager.UndoAction();
            }

            if (activeView.ActionManager.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
            {
                activeView.ActionManager.RedoAction();
            }
        }

        if (InputManager.IsPressed(KeybindID.TextureViewer_Export_Texture))
        {
            activeView.ToolView.TextureExport.ExportTextureHandler();
        }

        if (InputManager.HasCtrlDown())
        {
            activeView.ZoomState.HandleZoom();
        }

        if (InputManager.IsPressed(KeybindID.TextureViewer_Reset_Zoom_Level))
        {
            activeView.ZoomState.ZoomReset();
        }
    }
}
