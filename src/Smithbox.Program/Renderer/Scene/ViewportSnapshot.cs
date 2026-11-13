using StudioCore.Application;
using System;
using System.Numerics;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     The pixels of a viewport snapshot: 8 bit RGBA, rows top to bottom, as the screen shows the scene after the tone
///     map (and FXAA when it is on), without the overlays drawn on the swapchain after it.
/// </summary>
public sealed record SnapshotPixels(int Width, int Height, byte[] Rgba);

/// <summary>
///     Takes one viewport's tone mapped scene off the GPU, for the render snapshots that check a renderer change
///     against a baseline (Documentation/Shader Family Split Plan.md, S1). <see cref="Request" /> asks for the next
///     frame; the viewport calls <see cref="Record" /> where it tone maps that frame, which draws the image again into
///     a target of its own and copies it to a staging texture, and <see cref="Update" /> every frame, which reads it
///     once the frames in flight are done with it.
/// </summary>
public class ViewportSnapshot
{
    // The image the tone map (or FXAA) draws into, as FxaaPass's own does
    private Texture Image;
    private Framebuffer Framebuffer;
    private Texture Staging;

    // Asked for, not yet drawn
    private Action<SnapshotPixels> PendingCallback;

    // Drawn and copied, read once FramesUntilRead reaches 0
    private Action<SnapshotPixels> ReadCallback;
    private int FramesUntilRead;

    /// <summary>Whether the next frame the viewport draws is to be taken</summary>
    public bool Pending => PendingCallback != null;

    /// <summary>Whether a snapshot is asked for or on its way back</summary>
    public bool Busy => PendingCallback != null || ReadCallback != null;

    /// <summary>
    ///     Asks for the next frame. <paramref name="onDone" /> is called on the main thread with its pixels, or with null
    ///     if they couldn't be read. A request made while one is busy replaces the one not yet drawn.
    /// </summary>
    public void Request(Action<SnapshotPixels> onDone)
    {
        PendingCallback = onDone;
    }

    /// <summary>
    ///     Draws the frame into the snapshot image and copies it to the staging texture. Called where the viewport tone
    ///     maps the scene, with what it tone maps: <paramref name="source" /> is the screen effects' result, and with
    ///     <paramref name="fxaa" /> the FXAA image has already been filled by the tone map.
    /// </summary>
    public void Record(GraphicsDevice gd, CommandList cl, SceneRenderPipeline pipeline, Texture source,
        GparamLighting lighting, bool passThrough, bool fxaa)
    {
        if (PendingCallback == null || (!fxaa && source == null))
            return;

        EnsureImages(gd, pipeline.Target.Width, pipeline.Target.Height);

        cl.SetFramebuffer(Framebuffer);
        cl.SetFullViewport(0);

        if (fxaa)
        {
            pipeline.Fxaa.Draw(gd, cl, FxaaPass.ImageOutputs, Vector2.Zero, lighting);
        }
        else
        {
            pipeline.ToneMap.Draw(gd, cl, source, FxaaPass.ImageOutputs, Vector2.Zero, lighting,
                pipeline.AutoExposure, passThrough);
        }

        cl.CopyTexture(Image, Staging);

        ReadCallback = PendingCallback;
        PendingCallback = null;

        // As the async readbacks wait for a buffer, so this frame and those in flight before it are done
        FramesUntilRead = SceneRenderer.ReadbackReleaseFrames;
    }

    /// <summary>Reads a recorded snapshot once its frame is done. Called every frame.</summary>
    public unsafe void Update(GraphicsDevice gd)
    {
        if (ReadCallback == null)
            return;

        if (--FramesUntilRead > 0)
            return;

        var callback = ReadCallback;
        ReadCallback = null;

        SnapshotPixels pixels = null;

        try
        {
            var width = (int)Staging.Width;
            var height = (int)Staging.Height;
            var rgba = new byte[width * height * 4];

            MappedResource mapped = gd.Map(Staging, MapMode.Read);

            try
            {
                var rowBytes = width * 4;

                for (var y = 0; y < height; y++)
                {
                    var row = new ReadOnlySpan<byte>((byte*)mapped.Data + (long)y * mapped.RowPitch, rowBytes);
                    row.CopyTo(rgba.AsSpan(y * rowBytes, rowBytes));
                }
            }
            finally
            {
                gd.Unmap(Staging);
            }

            pixels = new SnapshotPixels(width, height, rgba);
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(ViewportSnapshot), "Failed to read the viewport snapshot", e);
        }

        callback(pixels);
    }

    /// <summary>Drops a request not yet drawn; one on its way back still arrives</summary>
    public void Cancel()
    {
        PendingCallback = null;
    }

    public void Dispose()
    {
        SceneRenderer.DisposeAllAfterFramesInFlight(Framebuffer, Image, Staging);
        Framebuffer = null;
        Image = null;
        Staging = null;
        PendingCallback = null;
        ReadCallback = null;
    }

    private void EnsureImages(GraphicsDevice gd, uint width, uint height)
    {
        width = Math.Max(width, 1u);
        height = Math.Max(height, 1u);

        if (Image != null && Image.Width == width && Image.Height == height)
            return;

        // Only replaced between snapshots, when no frame reads them, but a frame in flight could still copy
        if (Image != null)
        {
            SceneRenderer.DisposeAllAfterFramesInFlight(Framebuffer, Image, Staging);
        }

        ResourceFactory factory = gd.ResourceFactory;

        Image = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, FxaaPass.ImageFormat,
            VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.Sampled,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1));
        Image.Name = "Viewport Snapshot";

        Framebuffer = factory.CreateFramebuffer(new FramebufferDescription(null, Image));

        // A linear texture is a mappable staging one (see Texture's constructor)
        var staging = new TextureDescription
        {
            Width = width,
            Height = height,
            Depth = 1,
            MipLevels = 1,
            ArrayLayers = 1,
            Format = FxaaPass.ImageFormat,
            Type = VkImageType.Image2D,
            SampleCount = VkSampleCountFlags.Count1,
            Usage = VkImageUsageFlags.None,
            CreateFlags = VkImageCreateFlags.None,
            Tiling = VkImageTiling.Linear
        };

        Staging = factory.CreateTexture(staging);
    }
}
