using Hexa.NET.ImGui;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.MapEditor;

public class CompassTool : IResourceEventListener
{
    private MapEditorView View;
    public ProjectEntry Project;

    private Task _loadingTask;

    public CompassTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        RegisterCompassTexture();
    }

    public void RegisterCompassTexture()
    {
        ResourceManager.AddResourceListener<TextureResource>("smithbox/compass/compass", this, AccessLevel.AccessGPUOptimizedOnly);

        ResourceManager.ScheduleCompassRefresh();
    }

    public void DisplayCompass()
    {
        if (!CFG.Current.DisplayCompass)
            return;

        ViewportCamera camera = null;
        VulkanViewport vulkanViewport = null;

        var curViewport = View.ViewportHandler.ActiveViewport;
        if (curViewport.Viewport is VulkanViewport vp)
        {
            vulkanViewport = vp;
            camera = vp.ViewportCamera;
        }

        var resHandle = GetImageTextureHandle("smithbox/compass/compass");

        if (resHandle != null)
        {
            TextureResource texRes = resHandle.Get();

            if (texRes != null)
            {
                var imageSize = GetDisplaySize(texRes);
                var textureId = new ImTextureID(texRes.GPUTexture.TexHandle);

                var originalCursorPos = ImGui.GetCursorScreenPos();

                if (vulkanViewport != null)
                {
                    var compassPadding = (float)CFG.Current.CompassOffset;

                    // Top-right corner of the viewport.
                    var position = new Vector2(
                        vulkanViewport.X + vulkanViewport.Width - imageSize.X - compassPadding,
                        vulkanViewport.Y + compassPadding);

                    ImGui.SetCursorScreenPos(position);
                }

                if (camera != null)
                {
                    float cameraYaw = camera.CameraTransform.EulerRotation.Y;

                    float northOffset = (float)CFG.Current.CompassNorthOffset * (float)Math.PI / 180.0f;
                    float compassRotation = -cameraYaw + northOffset;

                    DrawRotatedImage(textureId, imageSize, compassRotation);
                }
                else
                {
                    ImGui.Image(textureId, imageSize);
                }

                ImGui.SetCursorScreenPos(originalCursorPos);
            }
        }
    }

    private void DrawRotatedImage(ImTextureID textureId, Vector2 size, float angle)
    {
        var drawList = ImGui.GetWindowDrawList();
        var topLeft = ImGui.GetCursorScreenPos();
        var center = topLeft + size * 0.5f;
        var half = size * 0.5f;

        // Corners relative to center, in UV order: top-left, top-right, bottom-right, bottom-left.
        Span<Vector2> corners = stackalloc Vector2[4]
        {
            new Vector2(-half.X, -half.Y),
            new Vector2(half.X, -half.Y),
            new Vector2(half.X, half.Y),
            new Vector2(-half.X, half.Y),
        };

        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        for (int i = 0; i < 4; i++)
        {
            var c = corners[i];
            var rotated = new Vector2(
                c.X * cos - c.Y * sin,
                c.X * sin + c.Y * cos);
            corners[i] = center + rotated;
        }

        drawList.AddImageQuad(
            textureId,
            corners[0], corners[1], corners[2], corners[3],
            new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1));

        ImGui.Dummy(size);
    }

    private Vector2 GetDisplaySize(TextureResource texRes)
    {
        Vector2 size = new Vector2(0, 0);

        if (texRes.GPUTexture != null)
        {
            var width = texRes.GPUTexture.Width;
            var height = texRes.GPUTexture.Height;

            if (height != 0 && width != 0)
            {
                float scale = (float)CFG.Current.CompassSize / MathF.Max(width, height);
                size = new Vector2(width * scale, height * scale);
            }
        }

        return size;
    }

    public ResourceHandle<TextureResource> GetImageTextureHandle(string path)
    {
        var virtName = $@"{path}".ToLower();

        var resources = ResourceManager.GetResourceDatabase();

        if (resources.ContainsKey(virtName))
        {
            return (ResourceHandle<TextureResource>)resources[virtName];
        }

        return null;
    }

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
    }
}