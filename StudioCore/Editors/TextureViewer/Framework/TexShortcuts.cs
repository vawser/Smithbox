using StudioCore.Configuration;
using StudioCore.Editors.TextureViewer.Tools;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Editors.TextureViewer;

public class TexShortcuts
{
    private TextureViewerScreen Screen;
    public TexTools Tools;
    public TexViewerZoom ViewerZoom;

    public TexShortcuts(TextureViewerScreen screen)
    {
        Screen = screen;
        Tools = screen.Tools;
        ViewerZoom = screen.ViewerZoom;
    }

    public void Monitor()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.TEXTURE_ExportTexture))
        {
            Tools.ExportTextureHandler();
        }

        if (InputTracker.GetKey(Key.LControl))
        {
            ViewerZoom.HandleZoom();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.TEXTURE_ResetZoomLevel))
        {
            ViewerZoom.ZoomReset();
        }
    }
}
