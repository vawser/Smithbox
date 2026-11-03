using StudioCore.Application;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     The sun's cascaded shadow maps for one <see cref="SceneRenderPipeline" />. The camera's view is cut into up to
///     four slices by distance, as the GPARAM's Shadows group splits it, and each is drawn from the sun into a
///     quarter of one depth map. Each cascade is a render queue of its own, with a copy of the scene parameters that
///     has the sun's view, so the meshes' shaders draw it as they are, see c_shadowPass in FlverShader.frag.
/// </summary>
public class ShadowCascades
{
    public const int MaxCascades = 4;

    /// <summary>Specialization constant of FlverShader.frag that draws depth alone, for the shadow map</summary>
    public const uint ShadowPassConstant = 98;

    public const VkFormat DepthFormat = VkFormat.D32Sfloat;

    public static readonly OutputDescription Outputs = new(new OutputAttachmentDescription(DepthFormat));

    // Meshes this far beyond a cascade towards the sun can still cast into it
    private const float CasterReach = 1000.0f;

    private readonly GraphicsDevice Device;
    private readonly SceneRenderer.RenderQueue[] Queues = new SceneRenderer.RenderQueue[MaxCascades];
    private readonly DeviceBuffer[] ParamBuffers = new DeviceBuffer[MaxCascades];
    private readonly Matrix4x4[] Views = new Matrix4x4[MaxCascades];
    private readonly Matrix4x4[] Projections = new Matrix4x4[MaxCascades];
    private readonly BoundingFrustum[] Frustums = new BoundingFrustum[MaxCascades];

    private Texture Map;
    private Framebuffer MapFramebuffer;
    private TexturePool.TextureHandle Handle;

    /// <summary>How many cascades are drawn this frame, 0 when there are no shadows</summary>
    public int ActiveCount { get; private set; }

    /// <summary>The size of one cascade's square in the map, in texels</summary>
    public uint Resolution { get; private set; }

    /// <summary>What the shaders read the cascades with, filled by <see cref="Update" /></summary>
    public ShadowParameters Parameters;

    public struct ShadowParameters
    {
        public Matrix4x4 Matrix0, Matrix1, Matrix2, Matrix3;
        public Vector4 Splits, TexelSizes, DepthScales;
        public float FadeStart, FadeEnd, DepthBias, SlopeBias, CascadeFade;
    }

    public unsafe ShadowCascades(GraphicsDevice device, SceneRenderPipeline pipeline)
    {
        Device = device;

        ResourceLayout sceneParamLayout = StaticResourceCache.GetResourceLayout(device.ResourceFactory,
            StaticResourceCache.SceneParamLayoutDescription);

        for (var i = 0; i < MaxCascades; i++)
        {
            ParamBuffers[i] = device.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(SceneParam),
                VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));

            var cascade = i;
            Queues[i] = new SceneRenderer.RenderQueue($"Shadow Cascade {i}", device, pipeline)
            {
                IsOffscreen = true,
                SceneParamSet = device.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(sceneParamLayout, ParamBuffers[i]))
            };
            Queues[i].SetPredrawSetupAction((d, cl) => BeginCascade(cl, cascade));
            SceneRenderer.RegisterRenderQueue(Queues[i]);
        }
    }

    /// <summary>The shadow map's index in the global texture pool, or 0 if there are no shadows this frame</summary>
    public uint TextureIndex => ActiveCount > 0 ? Handle?.TexHandle ?? 0 : 0;

    /// <summary>The shadow map itself, for passes that bind it directly, or null if there are no shadows this frame</summary>
    public Texture MapTexture => ActiveCount > 0 ? Map : null;

    /// <summary>The whole map's size in texels: two cascades to a side</summary>
    public float MapSize => Resolution * 2.0f;

    /// <summary>
    ///     Places the cascades over the camera's view for this frame. <paramref name="lightTravel" /> is the
    ///     direction the sun's light travels in. With <paramref name="enabled" /> false, nothing is drawn.
    /// </summary>
    public void Update(bool enabled, Matrix4x4 view, Matrix4x4 projection, Vector3 lightTravel,
        GparamShadow settings)
    {
        ActiveCount = 0;

        // A Cascade Count of 0 switches the shadows off, as the GPARAMs' menu render IDs do
        if (!enabled || settings.CascadeCount <= 0 || lightTravel.LengthSquared() < 1e-8f ||
            !Matrix4x4.Invert(view, out Matrix4x4 cameraWorld))
        {
            return;
        }

        Resolution = (uint)Math.Clamp(CFG.Current.Viewport_Shadow_Resolution, 256, 8192);
        lightTravel = Vector3.Normalize(lightTravel);

        var count = Math.Clamp(settings.CascadeCount, 1, MaxCascades);
        var far = Math.Max(settings.FarDistance, settings.Near + 1.0f);

        // Where each cascade starts and ends along the camera's view; a split past the last cascade is left out
        Span<float> ends = stackalloc float[MaxCascades];
        ends[0] = settings.CascadeSplits.X;
        ends[1] = settings.CascadeSplits.Y;
        ends[2] = settings.CascadeSplits.Z;
        ends[count - 1] = far;

        for (var i = 0; i < count; i++)
        {
            ends[i] = Math.Clamp(ends[i], settings.Near + 0.01f, far);
        }

        // The camera's frustum at one meter of depth: perspective widens with depth, orthographic doesn't
        var perspective = MathF.Abs(projection.M34) > 0.5f;
        var halfX = 1.0f / MathF.Max(MathF.Abs(projection.M11), 1e-6f);
        var halfY = 1.0f / MathF.Max(MathF.Abs(projection.M22), 1e-6f);

        // Up for the sun's view, unless it shines straight down that way
        Vector3 up = MathF.Abs(lightTravel.Y) > 0.99f ? Vector3.UnitZ : Vector3.UnitY;
        Vector3 right = Vector3.Normalize(Vector3.Cross(up, lightTravel));
        up = Vector3.Cross(lightTravel, right);

        var start = settings.Near;
        Span<Vector3> corners = stackalloc Vector3[8];

        for (var i = 0; i < count; i++)
        {
            var end = ends[i];

            // A sphere around the slice keeps the cascade the same size however the camera turns, so its
            // texels stay put as it does
            Vector3 center = Vector3.Zero;
            var c = 0;

            for (var d = 0; d < 2; d++)
            {
                var depth = d == 0 ? start : end;
                var sx = perspective ? halfX * depth : halfX;
                var sy = perspective ? halfY * depth : halfY;

                for (var y = -1; y <= 1; y += 2)
                for (var x = -1; x <= 1; x += 2)
                {
                    corners[c] = Vector3.Transform(new Vector3(x * sx, y * sy, depth), cameraWorld);
                    center += corners[c];
                    c++;
                }
            }

            center /= 8.0f;

            var radius = 0.0f;
            foreach (Vector3 corner in corners)
            {
                radius = MathF.Max(radius, Vector3.Distance(corner, center));
            }

            // Rounded, so float error doesn't change the texel size from frame to frame
            radius = MathF.Ceiling(radius * 16.0f) / 16.0f;

            Matrix4x4 lightView = new(
                right.X, up.X, lightTravel.X, 0.0f,
                right.Y, up.Y, lightTravel.Y, 0.0f,
                right.Z, up.Z, lightTravel.Z, 0.0f,
                -Vector3.Dot(right, center), -Vector3.Dot(up, center), -Vector3.Dot(lightTravel, center), 1.0f);

            // Depth runs from behind the slice, where casters towards the sun are, to its far side, reversed as
            // the viewport's is so that 1 is nearest the sun
            var zNear = -(radius + CasterReach);
            var zFar = radius;
            var depthRange = zFar - zNear;

            Matrix4x4 lightProjection = new(
                1.0f / radius, 0.0f, 0.0f, 0.0f,
                0.0f, (Device.IsClipSpaceYInverted ? -1.0f : 1.0f) / radius, 0.0f, 0.0f,
                0.0f, 0.0f, -1.0f / depthRange, 0.0f,
                0.0f, 0.0f, zFar / depthRange, 1.0f);

            if (settings.AlignTexel)
            {
                // Moves the cascade by under a texel, so the world's origin lands on a texel corner
                Vector4 origin = Vector4.Transform(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), lightView * lightProjection);
                var half = Resolution * 0.5f;
                lightProjection.M41 += (MathF.Round(origin.X * half) - origin.X * half) / half;
                lightProjection.M42 += (MathF.Round(origin.Y * half) - origin.Y * half) / half;
            }

            Views[i] = lightView;
            Projections[i] = lightProjection;
            Frustums[i] = new BoundingFrustum(lightView * lightProjection);

            // With the standard clip Y, Veldrid flips the viewport, so clip space +Y is drawn at the top of the
            // cascade's square: the shaders find a point's texel with the matrix flipped to match
            Matrix4x4 lookup = lightView * lightProjection;
            if (!Device.IsClipSpaceYInverted)
            {
                lookup *= Matrix4x4.CreateScale(1.0f, -1.0f, 1.0f);
            }

            SetCascade(i, lookup, end, 2.0f * radius / Resolution, 1.0f / depthRange);
            start = end;
        }

        var fadeStart = Math.Min(settings.FarFadeStart, far);
        var biasScale = CFG.Current.Viewport_Shadow_Bias_Scale;

        Parameters.FadeStart = fadeStart;
        Parameters.FadeEnd = Math.Max(far, fadeStart + 0.01f);
        Parameters.DepthBias = settings.DepthBias * biasScale;
        Parameters.SlopeBias = settings.SlopeScaledDepthBias * biasScale;
        Parameters.CascadeFade = Math.Clamp(settings.CascadeFadeRangeScale, 0.0f, 0.5f);

        ActiveCount = count;
    }

    private void SetCascade(int i, Matrix4x4 matrix, float end, float texelSize, float depthScale)
    {
        switch (i)
        {
            case 0: Parameters.Matrix0 = matrix; Parameters.Splits.X = end; Parameters.TexelSizes.X = texelSize; Parameters.DepthScales.X = depthScale; break;
            case 1: Parameters.Matrix1 = matrix; Parameters.Splits.Y = end; Parameters.TexelSizes.Y = texelSize; Parameters.DepthScales.Y = depthScale; break;
            case 2: Parameters.Matrix2 = matrix; Parameters.Splits.Z = end; Parameters.TexelSizes.Z = texelSize; Parameters.DepthScales.Z = depthScale; break;
            case 3: Parameters.Matrix3 = matrix; Parameters.Splits.W = end; Parameters.TexelSizes.W = texelSize; Parameters.DepthScales.W = depthScale; break;
        }
    }

    /// <summary>
    ///     Puts the shadow casters each cascade can see in its queue. A cascade that isn't drawn this frame gets
    ///     none, so its queue does nothing.
    /// </summary>
    public void Submit(MeshRenderables renderables, RenderFilter filter, DrawGroup displayGroup)
    {
        for (var i = 0; i < MaxCascades; i++)
        {
            if (i < ActiveCount)
            {
                renderables.SubmitShadowCasters(Queues[i], Frustums[i], filter, displayGroup);
            }

            Queues[i].SetDrawParameters(renderables.cDrawParameters, renderables.cShadowPipelines);
        }
    }

    /// <summary>
    ///     Uploads each cascade's scene parameters: the viewport's, so that wind and animation move the casters as
    ///     they do in the view, but seen from the sun
    /// </summary>
    public unsafe void Upload(CommandList cl, SceneParam sceneParams)
    {
        for (var i = 0; i < ActiveCount; i++)
        {
            SceneParam cascade = sceneParams;
            cascade.View = Views[i];
            cascade.Projection = Projections[i];
            cascade.ShadowTexture = 0;

            cl.UpdateBuffer(ParamBuffers[i], 0, ref cascade, (uint)sizeof(SceneParam));
        }
    }

    private void BeginCascade(CommandList cl, int cascade)
    {
        // A queue draws what was put in it the frame before, so a cascade can still have draws after the shadows
        // are switched off or the cascade count drops. Once the map exists it is always bound, so those land in
        // it; before then, no frame has put anything in the queues.
        if (Map == null && ActiveCount == 0)
        {
            return;
        }

        if (cascade == 0)
        {
            if (ActiveCount > 0)
            {
                EnsureMap();
            }

            cl.SetFramebuffer(MapFramebuffer);
            cl.ClearDepthStencil(0);
        }

        var x = (uint)(cascade % 2) * Resolution;
        var y = (uint)(cascade / 2) * Resolution;

        cl.SetViewport(0, new Viewport(x, y, Resolution, Resolution, 0.0f, 1.0f));
        cl.SetScissorRect(0, x, y, Resolution, Resolution);
    }

    private void EnsureMap()
    {
        var size = Resolution * 2;

        if (Map != null && Map.Width == size)
        {
            return;
        }

        Handle ??= SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();

        if (MapFramebuffer != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(MapFramebuffer);
        }

        TextureDescription description = TextureDescription.Texture2D(size, size, 1, 1, DepthFormat,
            VkImageUsageFlags.DepthStencilAttachment | VkImageUsageFlags.Sampled,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        Map = Device.ResourceFactory.CreateTexture(ref description);
        Map.Name = "Shadow Map";
        MapFramebuffer = Device.ResourceFactory.CreateFramebuffer(new FramebufferDescription(Map));

        // The pool retires the texture it held
        Handle?.FillWithGPUTexture(Map);
    }
}
