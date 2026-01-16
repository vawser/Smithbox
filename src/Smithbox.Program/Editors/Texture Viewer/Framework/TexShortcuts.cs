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
        if (InputManager.IsPressed(KeybindID.TextureViewer_Export_Texture))
        {
            Editor.Tools.ExportTextureHandler();
        }

        if (InputManager.HasCtrlDown())
        {
            Editor.ViewerZoom.HandleZoom();
        }

        if (InputManager.IsPressed(KeybindID.TextureViewer_Reset_Zoom_Level))
        {
            Editor.ViewerZoom.ZoomReset();
        }
    }
}
