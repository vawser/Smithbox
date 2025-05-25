using Hexa.NET.ImGui;
using StudioCore.Resource.Types;
using StudioCore.Resource;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Resource.Locators;
using SoulsFormats;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.Core;

namespace StudioCore.Editors.TextureViewer;

public class TexDisplayView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexDisplayView(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Viewer##TextureViewer", ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        Editor.Selection.SwitchWindowContext(TextureViewerContext.TextureDisplay);

        Editor.Selection.TextureViewWindowPosition = ImGui.GetWindowPos();
        Editor.Selection.TextureViewScrollPosition = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());

        if (Editor.Selection.ViewerTextureResource != null)
        {
            TextureResource texRes = Editor.Selection.ViewerTextureResource;

            if (texRes != null)
            {
                Vector2 size = GetImageSize(texRes, true);

                var textureId = new ImTextureID(texRes.GPUTexture.TexHandle);
                ImGui.Image(textureId, size);
            }
        }

        ImGui.End();
    }

    /// <summary>
    /// Get the image resolution for the passed texture resource.
    /// </summary>
    public Vector2 GetImageSize(TextureResource texRes, bool includeZoomFactor)
    {
        Vector2 size = new Vector2(0, 0);

        if (texRes.GPUTexture != null)
        {
            var Width = texRes.GPUTexture.Width;
            var Height = texRes.GPUTexture.Height;

            if (Height != 0 && Width != 0)
            {
                if (includeZoomFactor)
                {
                    size = new Vector2(
                        (Width * Editor.ViewerZoom.GetZoomFactorWidth()), 
                        (Height * Editor.ViewerZoom.GetZoomFactorHeight()));
                }
                else
                {
                    size = new Vector2(Width, Height);
                }
            }
        }

        return size;
    }

    /// <summary>
    /// Match the cursor position to an icon position from the shoebox layout file
    /// </summary>
    public (string, bool) MatchMousePosToIcon(SubTexture entry, Vector2 relativePos)
    {
        var cursorPos = relativePos;

        var SubTexName = entry.Name.Replace(".png", "");

        var success = false;

        float Xmin = float.Parse(entry.X);
        float Xmax = (Xmin + float.Parse(entry.Width));
        float Ymin = float.Parse(entry.Y);
        float Ymax = (Ymin + float.Parse(entry.Height));

        if (cursorPos.X > Xmin && cursorPos.X < Xmax && cursorPos.Y > Ymin && cursorPos.Y < Ymax)
        {
            success = true;
        }

        return (SubTexName, success);
    }

    /// <summary>
    /// Get the relative position of the cursor in relation to the loaded image. 
    /// </summary>
    public Vector2 GetRelativePosition(Vector2 imageSize, Vector2 windowPos, Vector2 scrollPos)
    {
        Vector2 relativePos = new Vector2(0, 0);

        var fixedX = 3;
        var fixedY = 24;
        var cursorPos = ImGui.GetMousePos();

        // Account for window position and scroll
        relativePos.X = cursorPos.X - ((windowPos.X + fixedX) - scrollPos.X);
        relativePos.Y = cursorPos.Y - ((windowPos.Y + fixedY) - scrollPos.Y);

        // Account for zoom
        relativePos.X = relativePos.X / Editor.ViewerZoom.GetZoomFactorWidth();
        relativePos.Y = relativePos.Y / Editor.ViewerZoom.GetZoomFactorHeight();

        return relativePos;
    }
}
