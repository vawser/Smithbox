using StudioCore.Application;
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

        if (activeView == null)
            return;

        if (InputManager.IsPressed(KeybindID.TextureViewer_Export_Texture))
        {
            Editor.ToolMenu.TextureExport.ExportTextureHandler();
        }

        if (InputManager.HasCtrlDown())
        {
            activeView.ViewerZoom.HandleZoom();
        }

        if (InputManager.IsPressed(KeybindID.TextureViewer_Reset_Zoom_Level))
        {
            activeView.ViewerZoom.ZoomReset();
        }
    }
}
