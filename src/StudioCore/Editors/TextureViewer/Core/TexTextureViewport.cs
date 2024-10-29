using ImGuiNET;
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

namespace StudioCore.Editors.TextureViewer;

public class TexTextureViewport
{
    private TextureViewerScreen Screen;
    private TexViewSelection Selection;
    private TexViewerZoom ViewerZoom;

    public TexTextureViewport(TextureViewerScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ViewerZoom = screen.ViewerZoom;
    }

    // <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Viewer##TextureViewer", ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);
        Selection.SwitchWindowContext(TextureViewerContext.TextureViewport);

        Selection.TextureViewWindowPosition = ImGui.GetWindowPos();
        Selection.TextureViewScrollPosition = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());

        ResourceHandle<TextureResource> resHandle = GetImageTextureHandle(Selection._selectedTextureKey, Selection._selectedTexture, Selection._selectedAssetDescription);

        if (resHandle != null)
        {
            TextureResource texRes = resHandle.Get();

            Selection.CurrentTextureInView = texRes;
            Selection.CurrentTextureName = Selection._selectedTextureKey;

            if (texRes != null)
            {
                IntPtr handle = (nint)texRes.GPUTexture.TexHandle;
                Vector2 size = GetImageSize(texRes, true);

                ImGui.Image(handle, size);
            }
        }

        ImGui.End();
    }

    /// <summary>
    /// Get the resource manager handle for the passed texture key
    /// </summary>
    public ResourceHandle<TextureResource> GetImageTextureHandle(string key, TPF.Texture texture, ResourceDescriptor desc)
    {
        if (texture != null)
        {
            var path = desc.AssetVirtualPath;

            if (desc.AssetArchiveVirtualPath != null)
            {
                path = desc.AssetArchiveVirtualPath;
            }
            var virtName = $@"{path}/{key}".ToLower();

            var resources = ResourceManager.GetResourceDatabase();

            if (resources.ContainsKey(virtName))
            {
                return (ResourceHandle<TextureResource>)resources[virtName];
            }
        }

        return null;
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
                    size = new Vector2((Width * ViewerZoom.GetZoomFactorWidth()), (Height * ViewerZoom.GetZoomFactorHeight()));
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
        relativePos.X = relativePos.X / ViewerZoom.GetZoomFactorWidth();
        relativePos.Y = relativePos.Y / ViewerZoom.GetZoomFactorHeight();

        return relativePos;
    }
}
