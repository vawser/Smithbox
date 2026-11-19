using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StudioCore.Editors.Viewport;
using System;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     Captures a viewport's tone mapped scene (without overlays, gizmos or ImGui) into a PNG, for the remote control.
///     A request is recorded while the viewport is drawn (<see cref="Record" />, from its overlay setup action), and read
///     back on the next frame, once the frame's commands have been submitted (<see cref="TryRead" />).
/// </summary>
public class ViewportCapture
{
    public enum CaptureState
    {
        Idle,
        Requested,
        Recorded
    }

    public volatile CaptureState State = CaptureState.Idle;

    // One capture per viewport, and the viewport whose scene a tone map pass draws, for the hook on ToneMapPass.Draw
    private static readonly ConditionalWeakTable<VulkanViewport, ViewportCapture> Captures = new();
    private static readonly ConditionalWeakTable<ToneMapPass, VulkanViewport> Owners = new();

    public static ViewportCapture For(VulkanViewport viewport)
    {
        if (viewport.ViewPipeline != null)
            Owners.AddOrUpdate(viewport.ViewPipeline.ToneMap, viewport);

        return Captures.GetValue(viewport, _ => new ViewportCapture());
    }

    public static bool TryGetOwner(ToneMapPass pass, out VulkanViewport viewport, out ViewportCapture capture)
    {
        capture = null;
        return Owners.TryGetValue(pass, out viewport) && Captures.TryGetValue(viewport, out capture);
    }

    // A tone map of its own, so its parameters don't share a buffer with the one drawn to the swapchain
    private readonly ToneMapPass ToneMap = new();

    private Texture Image;
    private Texture Staging;
    private Framebuffer Framebuffer;

    public void Request()
    {
        State = CaptureState.Requested;
    }

    public void Cancel()
    {
        State = CaptureState.Idle;
    }

    /// <summary>
    ///     Tone maps <paramref name="source" /> into the capture image and copies it to the staging texture. Binds its
    ///     own framebuffer, so the caller has to bind its own again afterwards.
    /// </summary>
    public void Record(GraphicsDevice gd, CommandList cl, Texture source, uint width, uint height,
        GparamLighting lighting, AutoExposurePass autoExposure, bool passThrough)
    {
        if (State != CaptureState.Requested || source == null)
        {
            return;
        }

        EnsureImage(gd, Math.Max(width, 1u), Math.Max(height, 1u));

        cl.SetFramebuffer(Framebuffer);
        cl.SetFullViewport(0);
        cl.ClearColorTarget(0, RgbaFloat.Black);
        ToneMap.Draw(gd, cl, source, FxaaPass.ImageOutputs, Vector2.Zero, lighting, autoExposure, passThrough);
        cl.CopyTexture(Image, Staging);

        State = CaptureState.Recorded;
    }

    /// <summary>
    ///     Writes the recorded image to <paramref name="path" />. Waits for the device, so it is only for occasional use.
    /// </summary>
    public bool TryRead(GraphicsDevice gd, string path, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (State != CaptureState.Recorded || Staging == null)
        {
            return false;
        }

        gd.WaitForIdle();

        width = (int)Staging.Width;
        height = (int)Staging.Height;

        var pixels = new byte[width * height * 4];
        MappedResource mapped = gd.Map(Staging, MapMode.Read, 0);

        try
        {
            var rowBytes = width * 4;

            for (var y = 0; y < height; y++)
            {
                Marshal.Copy(mapped.Data + (nint)((long)y * mapped.RowPitch), pixels, y * rowBytes, rowBytes);
            }
        }
        finally
        {
            gd.Unmap(Staging, 0);
            State = CaptureState.Idle;
        }

        // The alpha is whatever the tone map left, which a PNG viewer would show as transparency
        for (var i = 3; i < pixels.Length; i += 4)
        {
            pixels[i] = 255;
        }

        using var image = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(pixels, width, height);

        var folder = System.IO.Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(folder))
        {
            System.IO.Directory.CreateDirectory(folder);
        }

        image.SaveAsPng(path);

        return true;
    }

    private void EnsureImage(GraphicsDevice gd, uint width, uint height)
    {
        if (Image != null && Image.Width == width && Image.Height == height)
        {
            return;
        }

        if (Image != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(Framebuffer);
            SceneRenderer.DisposeAfterFramesInFlight(Image);
            SceneRenderer.DisposeAfterFramesInFlight(Staging);
        }

        ResourceFactory factory = gd.ResourceFactory;

        TextureDescription imageDescription = TextureDescription.Texture2D(width, height, 1, 1, FxaaPass.ImageFormat,
            VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferSrc,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        Image = factory.CreateTexture(ref imageDescription);
        Image.Name = "Viewport Capture";
        Framebuffer = factory.CreateFramebuffer(new FramebufferDescription(null, Image));

        TextureDescription stagingDescription = TextureDescription.Texture2D(width, height, 1, 1, FxaaPass.ImageFormat,
            VkImageUsageFlags.None, VkImageCreateFlags.None, VkImageTiling.Linear);

        Staging = factory.CreateTexture(ref stagingDescription);
        Staging.Name = "Viewport Capture Staging";
    }
}
