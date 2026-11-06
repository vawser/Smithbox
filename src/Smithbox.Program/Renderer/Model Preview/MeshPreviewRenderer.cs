#nullable enable
using StudioCore.Application;
using StudioCore.Editors.Viewport;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public enum MeshPreviewState
{
    /// <summary>The preview hasn't been rendered yet.</summary>
    Pending,

    /// <summary>The preview can be displayed.</summary>
    Ready,

    /// <summary>There is nothing in the mesh to see, or it couldn't be rendered.</summary>
    Unavailable
}

/// <summary>
///     A preview of a mesh that isn't a loaded model, such as generated collision, asked for with
///     <see cref="MeshPreviewRenderer.Request" />. Dispose it once it is no longer displayed, which includes when it is
///     replaced by a preview of a changed mesh.
/// </summary>
public sealed class MeshPreview : IDisposable
{
    internal readonly Vector3[] Positions;
    internal readonly int[] Indices;
    internal readonly Color Color;

    internal TexturePool.TextureHandle? Texture;

    internal MeshPreview(Vector3[] positions, int[] indices, Color color)
    {
        Positions = positions;
        Indices = indices;
        Color = color;
    }

    public MeshPreviewState State { get; internal set; } = MeshPreviewState.Pending;

    /// <summary>The slot of the preview in the global texture pool, when it is ready.</summary>
    public uint TextureHandle => Texture?.TexHandle ?? 0;

    public int TriangleCount => Indices.Length / 3;

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        MeshPreviewRenderer.Release(this);
    }
}

/// <summary>
///     Renders meshes given as triangles into textures, in the style of the model previews
///     (<see cref="ModelPreviewRenderer" />): the same camera, background and tone map, with the mesh drawn flat
///     shaded with its triangle edges, as collision is drawn in the viewports.
///     <para>
///         There is only one renderer, as the render queues of its pipeline last for the life of the program. It works
///         through the previews asked for one at a time; <see cref="Draw" /> moves it along once per frame.
///     </para>
/// </summary>
public sealed class MeshPreviewRenderer
{
    /// <summary>The size of a preview's texture, the same as a model preview's.</summary>
    public const uint Resolution = ModelPreviewRenderer.Resolution;

    private const float FieldOfView = MathF.PI / 3.0f;

    // From above and to the side, like the model previews, but steeper: generated collision is often flat, and
    // would be a sliver from the model previews' angle
    private static readonly Quaternion ViewRotation = Quaternion.CreateFromYawPitchRoll(-0.6f, 0.7f, 0.0f);

    // Gives up on a mesh after this many frames
    private const int BuildTimeout = 600;

    // More than the frames in flight, which may still be using a mesh or texture after it has been replaced
    private const int RetireFrames = 4;

    private static MeshPreviewRenderer? _instance;

    // What has been asked for and not rendered yet, in the order it was asked
    private static readonly List<MeshPreview> _queue = new();

    // Textures of released previews, kept until the frames that displayed them are finished
    private static readonly List<Retired<TexturePool.TextureHandle>> _retiredTextures = new();

    private readonly GraphicsDevice Device;
    private readonly RenderScene Scene = new();
    private readonly SceneRenderPipeline Pipeline;

    // Rendering happens in here, and is copied to the preview's own texture
    private readonly Texture ColorTarget;
    private readonly Texture DepthTarget;
    private readonly Framebuffer Framebuffer;

    // Meshes that have been rendered, kept until the frames that drew them are finished
    private readonly List<Retired<MeshRenderableProxy>> _retiredMeshes = new();

    private Stage CurrentStage = Stage.Idle;
    private MeshPreview? Current;
    private MeshRenderableProxy? Mesh;

    private TexturePool.TextureHandle? Preview;
    private Texture? PreviewTexture;

    // Set for the frame in which the queue has the mesh's draws, read by the queue's setup actions
    private bool CaptureThisFrame;

    private int FramesInStage;

    private enum Stage
    {
        Idle,
        Building,
        Capture,
        Publish
    }

    private sealed class Retired<T>(T item)
    {
        public T Item { get; } = item;
        public int Age { get; set; }
    }

    /// <summary>
    ///     Previews are rendered with the Vulkan renderer, so the compatibility renderer can't have them.
    /// </summary>
    public static bool IsSupported => ModelPreviewCache.IsSupported;

    /// <summary>
    ///     Asks for a preview of a triangle list. The data is copied, so the lists can be changed afterwards.
    /// </summary>
    /// <param name="positions">Vertex positions</param>
    /// <param name="indices">Three indices into <paramref name="positions" /> per triangle</param>
    public static MeshPreview Request(IReadOnlyList<Vector3> positions, IReadOnlyList<int> indices, Color color)
    {
        var preview = new MeshPreview([.. positions], [.. indices], color);

        if (!IsSupported || preview.Indices.Length < 3 || preview.Indices.Length % 3 != 0)
        {
            preview.State = MeshPreviewState.Unavailable;
            return preview;
        }

        _queue.Add(preview);

        return preview;
    }

    /// <summary>
    ///     Does the render work for this frame. Called with the rest of the scene drawing, before the frame's render
    ///     queues are executed.
    /// </summary>
    public static void Draw()
    {
        RetireTextures();

        if (_instance == null)
        {
            if (_queue.Count == 0)
                return;

            _instance = new MeshPreviewRenderer(Smithbox.Instance._context.Device);
        }

        _instance.Advance();
    }

    internal static void Release(MeshPreview preview)
    {
        _queue.Remove(preview);

        if (_instance?.Current == preview)
        {
            _instance.Cancel();
        }

        if (preview.Texture != null)
        {
            _retiredTextures.Add(new Retired<TexturePool.TextureHandle>(preview.Texture));
            preview.Texture = null;
        }
    }

    private static void RetireTextures()
    {
        for (var i = _retiredTextures.Count - 1; i >= 0; i--)
        {
            Retired<TexturePool.TextureHandle> retired = _retiredTextures[i];

            if (++retired.Age < RetireFrames)
                continue;

            _retiredTextures.RemoveAt(i);
            retired.Item.Dispose();
        }
    }

    private MeshPreviewRenderer(GraphicsDevice device)
    {
        Device = device;

        OutputDescription outputs = device.SwapchainFramebuffer.OutputDescription;

        // The mesh is drawn into the pipeline's HDR target, then tone mapped into this, which has the swapchain's
        // formats so that it can be copied into a preview that ImGui draws
        TextureDescription colorDescription = TextureDescription.Texture2D(Resolution, Resolution, 1, 1,
            outputs.ColorAttachments[0].Format, VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.TransferSrc,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        TextureDescription depthDescription = TextureDescription.Texture2D(Resolution, Resolution, 1, 1,
            outputs.DepthAttachment!.Value.Format, VkImageUsageFlags.DepthStencilAttachment,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        ColorTarget = device.ResourceFactory.CreateTexture(ref colorDescription);
        DepthTarget = device.ResourceFactory.CreateTexture(ref depthDescription);
        Framebuffer = device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(DepthTarget, ColorTarget));

        Pipeline = new SceneRenderPipeline(Scene, device, (int)Resolution, (int)Resolution, shadows: false);
        Pipeline.SetOffscreen();

        Pipeline.SetViewportSetupAction((d, cl) =>
        {
            Pipeline.Target.EnsureSize(d, Resolution, Resolution);

            if (CaptureThisFrame)
            {
                Pipeline.Target.BeginScene(cl, Pipeline.BackgroundColor);
            }
            else
            {
                cl.SetFramebuffer(Pipeline.Target.Framebuffer);
            }
        });

        // The overlay queue runs after the mesh has been drawn, and this scene has no overlays of its own
        Pipeline.SetOverlayViewportSetupAction((d, cl) =>
        {
            cl.SetFramebuffer(Framebuffer);

            if (CaptureThisFrame && PreviewTexture != null)
            {
                CaptureThisFrame = false;
                cl.SetFullViewport(0);
                Pipeline.ToneMap.Draw(d, cl, Pipeline.Target.Color, Framebuffer.OutputDescription, Vector2.Zero, null,
                    passThrough: !RenderMode.IsGame);
                cl.CopyTexture(ColorTarget, PreviewTexture);
            }
        });
    }

    private void Advance()
    {
        RetireMeshes();

        if (CurrentStage == Stage.Idle)
        {
            Begin();
        }

        FramesInStage++;

        switch (CurrentStage)
        {
            case Stage.Building:
                AdvanceBuilding();
                break;

            case Stage.Capture:
                Capture();
                break;

            case Stage.Publish:
                Publish();
                break;
        }
    }

    private void Begin()
    {
        if (_queue.Count == 0)
            return;

        Current = _queue[0];
        _queue.RemoveAt(0);

        try
        {
            RawMeshProvider provider = RawMeshProvider.FromTriangles(Current.Positions, Current.Indices, Current.Color);
            Mesh = MeshRenderableProxy.FromProvider(Scene, provider);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("REND_MeshPreview_Build_Failed"), e);
            Fail();
            return;
        }

        SetStage(Stage.Building);
    }

    private void AdvanceBuilding()
    {
        if (FramesInStage > BuildTimeout)
        {
            Fail();
            return;
        }

        // The renderable is only constructed once the geometry has been uploaded
        if (Mesh == null || !Mesh.AreRenderablesConstructed())
            return;

        BoundingBox bounds = Mesh.GetBounds();

        // Slightly larger than the mesh so that it doesn't touch the edges
        var radius = Vector3.Distance(bounds.Max, bounds.Min) * 0.5f * 1.05f;

        if (radius <= 0.0f || float.IsNaN(radius) || float.IsInfinity(radius))
        {
            Fail();
            return;
        }

        var distance = radius / MathF.Sin(FieldOfView * 0.5f);
        Vector3 direction = Vector3.Transform(Vector3.UnitZ, ViewRotation);
        Vector3 eye = bounds.GetCenter() - direction * distance;

        Transform camera = new(eye, ViewRotation, Vector3.One);

        Matrix4x4 view = camera.CameraViewMatrixLH;
        Matrix4x4 projection = ViewportUtils.CreatePerspective(Device, true, FieldOfView, 1.0f,
            MathF.Max(distance - radius, distance * 0.01f), distance + radius);

        Pipeline.UpdateSceneParameters(projection, view, eye, 0, 0);
        Pipeline.RenderScene(new BoundingFrustum(view * projection));

        // Nothing to draw leaves the render pass that clears the framebuffer unstarted, so there is nothing to copy
        if (Pipeline.QueuedDrawCount == 0)
        {
            Fail();
            return;
        }

        SetStage(Stage.Capture);
    }

    private void Capture()
    {
        // A queue encodes its draws in one frame and submits them in the next, which is this one
        Preview = SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();

        if (Preview == null)
        {
            Fail();
            return;
        }

        TextureDescription description = TextureDescription.Texture2D(Resolution, Resolution, 1, 1, ColorTarget.Format,
            VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst, VkImageCreateFlags.None, VkImageTiling.Optimal,
            VkSampleCountFlags.Count1);

        PreviewTexture = Device.ResourceFactory.CreateTexture(ref description);
        PreviewTexture.Name = "Mesh Preview";

        Preview.FillWithGPUTexture(PreviewTexture);

        CaptureThisFrame = true;

        SetStage(Stage.Publish);
    }

    private void Publish()
    {
        if (Current != null && Preview != null)
        {
            Current.Texture = Preview;
            Current.State = MeshPreviewState.Ready;
        }

        Preview = null;
        PreviewTexture = null;

        FinishMesh();
    }

    /// <summary>
    ///     Abandons the preview in progress.
    /// </summary>
    private void Cancel()
    {
        CaptureThisFrame = false;

        // The capture may already be queued, so the texture has to outlast the frames in flight
        if (Preview != null)
        {
            _retiredTextures.Add(new Retired<TexturePool.TextureHandle>(Preview));
        }

        Preview = null;
        PreviewTexture = null;

        FinishMesh();
    }

    private void Fail()
    {
        if (Current != null)
        {
            Current.State = MeshPreviewState.Unavailable;
        }

        Cancel();
    }

    private void FinishMesh()
    {
        if (Mesh != null)
        {
            // Hidden now, so the next mesh is drawn alone, but its geometry has to outlast any frames that drew it
            Mesh.Visible = false;
            _retiredMeshes.Add(new Retired<MeshRenderableProxy>(Mesh));
        }

        Mesh = null;
        Current = null;
        CurrentStage = Stage.Idle;
    }

    private void RetireMeshes()
    {
        for (var i = _retiredMeshes.Count - 1; i >= 0; i--)
        {
            Retired<MeshRenderableProxy> retired = _retiredMeshes[i];

            if (++retired.Age < RetireFrames)
                continue;

            _retiredMeshes.RemoveAt(i);

            // Releases the raw mesh provider, which frees its geometry
            retired.Item.Dispose();
        }
    }

    private void SetStage(Stage stage)
    {
        CurrentStage = stage;
        FramesInStage = 0;
    }
}
