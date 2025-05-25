using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TexViewerZoom
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexViewerZoom(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    private Vector2 zoomFactor = new Vector2(1.0f, 1.0f);
    private float zoomFactorStep = 0.1f;

    public void HandleZoom()
    {
        var delta = InputTracker.GetMouseWheelDelta();

        if (delta > 0)
        {
            ZoomIn();
        }
        if (delta < 0)
        {
            ZoomOut();
        }
    }

    public void ZoomIn()
    {
        zoomFactor.X = zoomFactor.X + zoomFactorStep;
        zoomFactor.Y = zoomFactor.Y + zoomFactorStep;

        if (zoomFactor.X > 10.0f)
        {
            zoomFactor.X = 10.0f;
        }
        if (zoomFactor.Y > 10.0f)
        {
            zoomFactor.Y = 10.0f;
        }
    }
    public void ZoomOut()
    {
        zoomFactor.X = zoomFactor.X - zoomFactorStep;
        zoomFactor.Y = zoomFactor.Y - zoomFactorStep;

        if (zoomFactor.X < 0.1f)
        {
            zoomFactor.X = 0.1f;
        }
        if (zoomFactor.Y < 0.1f)
        {
            zoomFactor.Y = 0.1f;
        }
    }
    public void ZoomReset()
    {
        zoomFactor = new Vector2(1.0f, 1.0f);
    }

    public float GetZoomFactorWidth()
    {
        return zoomFactor.X;
    }
    public float GetZoomFactorHeight()
    {
        return zoomFactor.Y;
    }
}
