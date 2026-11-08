using System;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     What the rain can reach, for one <see cref="SceneRenderPipeline" />: a depth map of the opaque scene seen
///     straight down over a square round the camera, so FlverShader can tell open ground from ground under a roof
///     when it wets surfaces. Drawn with the meshes' shadow pipelines, as <see cref="ShadowCascades" /> are.
///     <para>
///         Map geometry is static, so the map is only drawn again when the camera has moved far enough or the casters
///         have changed, and is kept in between. A queue draws what was put in it a frame later than it was put there
///         (or in the same frame), so a redraw puts the casters in for three frames and clears and draws the map in
///         the middle two, which have the casters whichever it is. Draws on the frames either side go to a 1 x 1
///         sink instead, as binding the map's framebuffer at all discards what it holds.
///     </para>
/// </summary>
public class RainOcclusion
{
    /// <summary>The side of the square the map covers, in meters, centred on the camera</summary>
    public const float Extent = 256.0f;

    /// <summary>The map's side in texels</summary>
    public const uint Resolution = 2048;

    /// <summary>How far the camera moves from where the map was drawn before it is drawn again</summary>
    public const float RedrawDistance = 32.0f;

    // How far above and below the camera the map reaches
    private const float HeightRange = 1000.0f;

    // Frames a redraw puts the casters in the queue for
    private const int RedrawFrames = 3;

    private readonly GraphicsDevice Device;
    private readonly SceneRenderer.RenderQueue Queue;
    private readonly DeviceBuffer ParamBuffer;

    private Texture Map;
    private Framebuffer MapFramebuffer;
    private Texture Sink;
    private Framebuffer SinkFramebuffer;
    private TexturePool.TextureHandle Handle;

    // The view the map is being drawn with, and the one its contents were drawn with
    private Matrix4x4 PendingView;
    private Matrix4x4 PendingProjection;
    private Matrix4x4 PendingLookup;
    private Vector3 PendingCenter;

    private Vector3 DrawnCenter;
    private int DrawnSignature;
    private bool HasStarted;

    // Which frame of a redraw this is, from 0, or -1 for none
    private int RedrawPhase = -1;

    // Frames left in which the queue can still hold casters from a redraw
    private int StrayFrames;

    // Set by Update for this frame
    private bool SubmitThisFrame;
    private bool ClearThisFrame;

    /// <summary>Whether the map is wanted this frame, see <see cref="Update" /></summary>
    public bool Active { get; private set; }

    /// <summary>Whether the map holds a drawn view, which <see cref="Lookup" /> finds texels in</summary>
    public bool HasContent { get; private set; }

    /// <summary>World to the map's texture space, as the shaders read it: xy -1..1 across, z the depth</summary>
    public Matrix4x4 Lookup { get; private set; } = Matrix4x4.Identity;

    /// <summary>The map's index in the global texture pool, or 0 when it isn't used this frame</summary>
    public uint TextureIndex => Active && HasContent ? Handle?.TexHandle ?? 0 : 0;

    /// <summary>Meters per texel</summary>
    public static float TexelSize => Extent / Resolution;

    /// <summary>The map's depth per meter of height</summary>
    public static float DepthScale => 1.0f / (2.0f * HeightRange);

    /// <summary>Where the map was last drawn round, for the menu</summary>
    public Vector3 Center => DrawnCenter;

    /// <summary>How many times the map has been drawn, for the menu</summary>
    public int DrawCount { get; private set; }

    public unsafe RainOcclusion(GraphicsDevice device, SceneRenderPipeline pipeline)
    {
        Device = device;

        ResourceLayout sceneParamLayout = StaticResourceCache.GetResourceLayout(device.ResourceFactory,
            StaticResourceCache.SceneParamLayoutDescription);

        ParamBuffer = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(SceneParam),
            VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));

        Queue = new SceneRenderer.RenderQueue("Rain Occlusion", device, pipeline)
        {
            IsOffscreen = true,
            SceneParamSet = device.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(sceneParamLayout, ParamBuffer, pipeline.SceneLightingBuffer))
        };
        Queue.SetPredrawSetupAction((d, cl) => Begin(cl));
        SceneRenderer.RegisterRenderQueue(Queue);
    }

    /// <summary>
    ///     Decides whether the map is drawn this frame. <paramref name="enabled" /> is whether anything is wet;
    ///     <paramref name="signature" /> changes whenever the casters do, see
    ///     <see cref="MeshRenderables.ShadowCasterSignature" />.
    /// </summary>
    public void Update(bool enabled, Vector3 eye, int signature)
    {
        StrayFrames = Math.Max(StrayFrames - 1, 0);
        SubmitThisFrame = false;
        ClearThisFrame = false;
        Active = enabled;

        if (!enabled)
        {
            // A redraw that was cut short leaves the map as it was, which the next one replaces
            RedrawPhase = -1;
            return;
        }

        if (RedrawPhase < 0 && (!HasStarted || signature != DrawnSignature ||
            Vector3.Distance(eye, DrawnCenter) > RedrawDistance))
        {
            StartRedraw(eye);
            DrawnSignature = signature;
            HasStarted = true;
        }

        if (RedrawPhase < 0)
        {
            return;
        }

        SubmitThisFrame = true;
        ClearThisFrame = RedrawPhase >= 1;
        StrayFrames = 2;

        // From the first frame the map is cleared and drawn again, it holds the new view
        if (RedrawPhase == 1)
        {
            Lookup = PendingLookup;
            DrawnCenter = PendingCenter;
            HasContent = true;
            DrawCount++;
        }

        RedrawPhase = RedrawPhase + 1 < RedrawFrames ? RedrawPhase + 1 : -1;
    }

    private void StartRedraw(Vector3 eye)
    {
        // Snapped to the texel grid, so the texels lie in the same place in the world each time it is drawn
        var texel = TexelSize;
        var center = new Vector3(MathF.Round(eye.X / texel) * texel, MathF.Round(eye.Y),
            MathF.Round(eye.Z / texel) * texel);

        // Looking down -Y, with +X to the right and +Z up the map, as ShadowCascades builds a sun's view
        Vector3 travel = -Vector3.UnitY;
        Vector3 right = Vector3.UnitX;
        Vector3 up = Vector3.UnitZ;

        Matrix4x4 view = new(
            right.X, up.X, travel.X, 0.0f,
            right.Y, up.Y, travel.Y, 0.0f,
            right.Z, up.Z, travel.Z, 0.0f,
            -Vector3.Dot(right, center), -Vector3.Dot(up, center), -Vector3.Dot(travel, center), 1.0f);

        // Reversed depth as the viewport's: 1 at the top, HeightRange above the camera
        var half = Extent * 0.5f;
        var zNear = -HeightRange;
        var zFar = HeightRange;
        var depthRange = zFar - zNear;

        Matrix4x4 projection = new(
            1.0f / half, 0.0f, 0.0f, 0.0f,
            0.0f, (Device.IsClipSpaceYInverted ? -1.0f : 1.0f) / half, 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f / depthRange, 0.0f,
            0.0f, 0.0f, zFar / depthRange, 1.0f);

        // With the standard clip Y, Veldrid flips the viewport, so the lookup is flipped to match, as the cascades'
        Matrix4x4 lookup = view * projection;
        if (!Device.IsClipSpaceYInverted)
        {
            lookup *= Matrix4x4.CreateScale(1.0f, -1.0f, 1.0f);
        }

        PendingView = view;
        PendingProjection = projection;
        PendingLookup = lookup;
        PendingCenter = center;
        RedrawPhase = 0;
    }

    /// <summary>Puts the casters under the map's square in the queue, on the frames of a redraw</summary>
    public void Submit(MeshRenderables renderables, RenderFilter filter, DrawGroup displayGroup)
    {
        if (SubmitThisFrame)
        {
            renderables.SubmitShadowCasters(Queue, new BoundingFrustum(PendingView * PendingProjection), filter,
                displayGroup);
        }

        Queue.SetDrawParameters(renderables.cDrawParameters, renderables.cShadowPipelines);
    }

    /// <summary>Uploads the map's scene parameters while it is being drawn: the viewport's, seen from above</summary>
    public unsafe void Upload(CommandList cl, SceneParam sceneParams)
    {
        if (!SubmitThisFrame)
        {
            return;
        }

        SceneParam rain = sceneParams;
        rain.View = PendingView;
        rain.Projection = PendingProjection;
        rain.ShadowTexture = 0;
        rain.RainTexture = 0;

        cl.UpdateBuffer(ParamBuffer, 0, ref rain, (uint)sizeof(SceneParam));
    }

    private void Begin(CommandList cl)
    {
        if (ClearThisFrame)
        {
            EnsureMap();

            cl.SetFramebuffer(MapFramebuffer);
            cl.ClearDepthStencil(0);
            cl.SetViewport(0, new Viewport(0, 0, Resolution, Resolution, 0.0f, 1.0f));
            cl.SetScissorRect(0, 0, 0, Resolution, Resolution);
            return;
        }

        // Casters can still be in the queue from a redraw's first or last frame
        if (StrayFrames > 0)
        {
            EnsureSink();

            cl.SetFramebuffer(SinkFramebuffer);
            cl.SetViewport(0, new Viewport(0, 0, 1, 1, 0.0f, 1.0f));
            cl.SetScissorRect(0, 0, 0, 1, 1);
        }
    }

    private void EnsureMap()
    {
        if (Map != null)
        {
            return;
        }

        Handle ??= SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();

        TextureDescription description = TextureDescription.Texture2D(Resolution, Resolution, 1, 1,
            ShadowCascades.DepthFormat, VkImageUsageFlags.DepthStencilAttachment | VkImageUsageFlags.Sampled,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        Map = Device.ResourceFactory.CreateTexture(ref description);
        Map.Name = "Rain Occlusion Map";
        MapFramebuffer = Device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(Map));

        Handle?.FillWithGPUTexture(Map);
    }

    private void EnsureSink()
    {
        if (Sink != null)
        {
            return;
        }

        TextureDescription description = TextureDescription.Texture2D(1, 1, 1, 1, ShadowCascades.DepthFormat,
            VkImageUsageFlags.DepthStencilAttachment, VkImageCreateFlags.None, VkImageTiling.Optimal,
            VkSampleCountFlags.Count1);

        Sink = Device.ResourceFactory.CreateTexture(ref description);
        Sink.Name = "Rain Occlusion Sink";
        SinkFramebuffer = Device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(Sink));
    }
}
