using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.TextureViewer;
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
        if (InputTracker.GetKeyDown(KeyBindings.Current.TEXTURE_ExportTexture))
        {
            Editor.Tools.ExportTextureHandler();
        }

        if (InputTracker.GetKey(Key.LControl))
        {
            Editor.ViewerZoom.HandleZoom();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.TEXTURE_ResetZoomLevel))
        {
            Editor.ViewerZoom.ZoomReset();
        }
    }
}
