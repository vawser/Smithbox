using StudioCore.Application;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>Laid out as Ssr.frag's SsrParams</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SsrParam
{
    public Matrix4x4 ClipToWorld;
    public Matrix4x4 ViewProjection;

    /// <summary>x: mode, y: shininess reject, z: Fresnel reject, w: fade to viewer start</summary>
    public Vector4 Settings;

    /// <summary>x: step min in the scene's pixels, y: step max, z: ray length, w: crack detect factor</summary>
    public Vector4 Trace;

    /// <summary>x: mip scale, y: mip offset, z: the last mip, w: depth reject threshold</summary>
    public Vector4 Mip;

    /// <summary>xyz: projection [2][2], [3][2] and [2][3] as the shader indexes it, w: scene depth texture index</summary>
    public Vector4 Depth;

    public Vector4 Eye;

    /// <summary>xy: the scene's size, zw: the size of the target drawn</summary>
    public Vector4 Sizes;

    /// <summary>x: the viewport's scale on the reflections</summary>
    public Vector4 Strength;
}

/// <summary>
///     The GPARAM SSR ParamEditor group: glossy surfaces reflect what is on screen. The image is copied into a texture
///     with mips, so rough surfaces read it blurred; each glossy pixel's reflected ray is marched across the screen at
///     half size; and the composite swaps the environment's reflection the flver shader put in (the Reflection
///     attachment) for the traced one wherever the ray hit. Where it missed, the environment's stays. After SSAO and
///     before auto exposure. Elden Ring's maps take mapbase's values; without map lighting, mapbase's are used too.
///     A file with SSR Enable off has none. See Ssr.frag for how the values are read.
/// </summary>
public class SsrPass : PostProcessPass
{
    private const int ModeTrace = 0;
    private const int ModeComposite = 1;
    private const uint MaxMips = 8;

    /// <summary>The height the step sizes in pixels are taken to be for</summary>
    private const float ReferenceHeight = 1080.0f;

    // Off when there is no GPARAM in the games before Sekiro, which have no SSR
    private static GparamSsr Defaults => GparamProfile.Current.Defaults.Ssr;

    private static ResourceLayout Layout;
    private static Pipeline Pipeline;
    private static Sampler LinearClamp;

    private readonly SceneRenderPipeline Owner;

    private Texture ColorMips;
    private Texture Traced;
    private Framebuffer TracedFramebuffer;

    private readonly DeviceBuffer[] ParamBuffers = new DeviceBuffer[2];
    private ResourceSet TraceSet;
    private ResourceSet CompositeSet;

    // What the resource sets were made for
    private Texture BoundSource;
    private Texture BoundDestination;
    private SceneTarget BoundTarget;
    private int BoundTargetVersion = -1;

    public SsrPass(SceneRenderPipeline owner)
    {
        Owner = owner;
    }

    public override string Name => "SSR";

    /// <summary>The half size trace (color x weight, weight), for the debug view, or null</summary>
    public Texture Result => Traced;

    /// <summary>Whether <see cref="Result" /> was filled this frame</summary>
    public bool Ran { get; private set; }

    public override void BeginFrame()
    {
        Ran = false;
    }

    public override bool IsActive(GparamLighting lighting)
    {
        return CFG.Current.Viewport_Post_Ssr && (lighting?.PostEffects.Ssr ?? Defaults).Enabled;
    }

    public override unsafe void Run(GraphicsDevice gd, CommandList cl, PostProcessFrame frame)
    {
        var ssr = frame.Effects?.Ssr ?? Defaults;
        var target = frame.Target;

        EnsureResources(gd, frame);

        // The image as it is now, with mips for the rough surfaces. The copy and the mips are outside any render pass.
        cl.CopyTexture(frame.Source, 0, 0, 0, 0, 0, ColorMips, 0, 0, 0, 0, 0, target.Width, target.Height, 1, 1);
        cl.GenerateMipmaps(ColorMips);

        var scene = Owner.SceneParams;
        var view = scene.View;
        var projection = scene.Projection;
        var viewProjection = view * projection;
        Matrix4x4.Invert(viewProjection, out var clipToWorld);
        var pixelScale = target.Height / ReferenceHeight;

        var baseParam = new SsrParam
        {
            ClipToWorld = clipToWorld,
            ViewProjection = viewProjection,
            Settings = new Vector4(0.0f, ssr.ShininessRejectThreshold, ssr.FresnelRejectThreshold, ssr.FadeToViewerStart),
            Trace = new Vector4(Math.Max(ssr.StepScaleMin, 0.5f) * pixelScale,
                Math.Max(ssr.StepScaleMax, ssr.StepScaleMin) * pixelScale, Math.Max(ssr.DepthFadeRange, 0.1f),
                Math.Max(ssr.CrackDetectFactor, 1.0f)),
            Mip = new Vector4(ssr.MipScale, ssr.MipOffset, ColorMips.MipLevels - 1, ssr.DepthRejectThreshold),
            // GLSL reads the matrix as uploaded, so its [c][r] is M(c+1)(r+1) here
            Depth = new Vector4(projection.M33, projection.M43, projection.M34, Owner.SceneDepth.TextureIndex),
            Eye = scene.EyePosition,
            Strength = new Vector4(Math.Max(CFG.Current.Viewport_Post_Ssr_Strength, 0.0f), 0.0f, 0.0f, 0.0f)
        };

        var trace = baseParam;
        trace.Settings.X = ModeTrace;
        trace.Sizes = new Vector4(target.Width, target.Height, Traced.Width, Traced.Height);
        cl.UpdateBuffer(ParamBuffers[0], 0, ref trace, (uint)sizeof(SsrParam));

        var composite = baseParam;
        composite.Settings.X = ModeComposite;
        composite.Sizes = new Vector4(target.Width, target.Height, target.Width, target.Height);
        cl.UpdateBuffer(ParamBuffers[1], 0, ref composite, (uint)sizeof(SsrParam));

        cl.Barrier(VkPipelineStageFlags2.Transfer, VkAccessFlags2.TransferWrite,
            VkPipelineStageFlags2.FragmentShader, VkAccessFlags2.UniformRead | VkAccessFlags2.ShaderRead);

        cl.SetFramebuffer(TracedFramebuffer);
        cl.SetFullViewport(0);
        cl.SetPipeline(Pipeline);
        cl.SetGraphicsResourceSet(0, TraceSet);
        SceneRenderer.GlobalTexturePool.BindTexturePool(cl, 1);
        cl.Draw(3, 1, 0, 0);

        cl.SetFramebuffer(frame.DestinationFramebuffer);
        cl.SetFullViewport(0);
        cl.SetPipeline(Pipeline);
        cl.SetGraphicsResourceSet(0, CompositeSet);
        SceneRenderer.GlobalTexturePool.BindTexturePool(cl, 1);
        cl.Draw(3, 1, 0, 0);

        Ran = true;
    }

    private unsafe void EnsureResources(GraphicsDevice gd, PostProcessFrame frame)
    {
        ResourceFactory factory = gd.ResourceFactory;

        Layout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("SsrParams", VkDescriptorType.UniformBuffer, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("ColorMips", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Surface", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Reflection", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Scene", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Traced", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("LinearClamp", VkDescriptorType.Sampler, VkShaderStageFlags.Fragment)));

        if (LinearClamp == null)
        {
            SamplerDescription description = SamplerDescription.Linear;
            description.AddressModeU = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeV = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeW = VkSamplerAddressMode.ClampToEdge;
            LinearClamp = factory.CreateSampler(ref description);
        }

        if (Pipeline == null)
        {
            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, factory, "Ssr");

            GraphicsPipelineDescription description = new(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(VkCullModeFlags.None, VkPolygonMode.Fill, VkFrontFace.Clockwise, true, false),
                VkPrimitiveTopology.TriangleList,
                new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), new[] { vs, fs },
                    ShaderHelper.GetSpecializations(gd)),
                new[] { Layout, SceneRenderer.GlobalTexturePool.GetLayout() },
                PostProcessChain.Outputs);

            Pipeline = factory.CreateGraphicsPipeline(ref description);
            Pipeline.Name = "SSR";
        }

        for (var i = 0; i < ParamBuffers.Length; i++)
        {
            ParamBuffers[i] ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(SsrParam),
                VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));
        }

        var target = frame.Target;

        if (ColorMips == null || ColorMips.Width != target.Width || ColorMips.Height != target.Height)
        {
            CreateTextures(gd, target.Width, target.Height);
        }
        else if (ReferenceEquals(BoundSource, frame.Source) && ReferenceEquals(BoundDestination, frame.Destination) &&
                 ReferenceEquals(BoundTarget, target) && BoundTargetVersion == target.Version)
        {
            return;
        }

        if (TraceSet != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(TraceSet);
            SceneRenderer.DisposeAfterFramesInFlight(CompositeSet);
        }

        // The trace reads no trace and puts no image together; textures already drawn this frame stand in, as one
        // never drawn isn't in a layout that can be read
        TraceSet = factory.CreateResourceSet(new ResourceSetDescription(Layout, ParamBuffers[0], ColorMips,
            target.Surface, target.Reflection, frame.Source, target.Reflection, LinearClamp));
        CompositeSet = factory.CreateResourceSet(new ResourceSetDescription(Layout, ParamBuffers[1], ColorMips,
            target.Surface, target.Reflection, frame.Source, Traced, LinearClamp));

        BoundSource = frame.Source;
        BoundDestination = frame.Destination;
        BoundTarget = target;
        BoundTargetVersion = target.Version;
    }

    private void CreateTextures(GraphicsDevice gd, uint width, uint height)
    {
        // A frame that is still executing can be reading the old textures
        if (ColorMips != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(ColorMips);
            SceneRenderer.DisposeAfterFramesInFlight(TracedFramebuffer);
            SceneRenderer.DisposeAfterFramesInFlight(Traced);
        }

        var mips = Math.Min(MaxMips, (uint)Math.Floor(Math.Log2(Math.Max(width, height))) + 1);

        TextureDescription mipDescription = TextureDescription.Texture2D(width, height, mips, 1, SceneTarget.ColorFormat,
            VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferSrc | VkImageUsageFlags.TransferDst,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);
        ColorMips = gd.ResourceFactory.CreateTexture(ref mipDescription);
        ColorMips.Name = "SSR Color Mips";

        TextureDescription traceDescription = TextureDescription.Texture2D(Math.Max(width / 2, 1u),
            Math.Max(height / 2, 1u), 1, 1, SceneTarget.ColorFormat,
            VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.Sampled,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);
        Traced = gd.ResourceFactory.CreateTexture(ref traceDescription);
        Traced.Name = "SSR Trace";
        TracedFramebuffer = gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, Traced));
    }
}
