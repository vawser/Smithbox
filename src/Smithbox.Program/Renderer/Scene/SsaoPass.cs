using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>Laid out as Ssao.frag's SsaoParams</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SsaoParam
{
    public Matrix4x4 ClipToWorld;

    /// <summary>x: mode, y: Sample Offset in the scene's pixels, z: kernel scale min, w: max</summary>
    public Vector4 Settings;

    /// <summary>x: kernel scale base distance, y: AO scale, z: AO offset, w: depth reject rate</summary>
    public Vector4 Shape;

    /// <summary>x: fade start, y: fade range, z: precision depth, w: 1 to use the flvers' normals</summary>
    public Vector4 Fade;

    /// <summary>xyz: projection [2][2], [3][2] and [2][3] as the shader indexes it, w: scene depth texture index</summary>
    public Vector4 Depth;

    /// <summary>xyz: the camera, w: the projection's y scale</summary>
    public Vector4 Eye;

    /// <summary>xy: the scene's size, zw: the size of the target drawn</summary>
    public Vector4 Sizes;

    /// <summary>x: the viewport's scale on the occlusion</summary>
    public Vector4 Strength;
}

/// <summary>
///     The GPARAM SSAO Editor group: the scene's ambient light taken away where nearby geometry covers it. The
///     occlusion is found at half size from the scene depth and the Surface attachment's normals, blurred across and
///     down with the depth kept, and the composite takes the Indirect attachment's light times the occlusion off the
///     image, so sunlit faces keep their sun. First in the chain, so the auto exposure measures the darkened scene.
///     Elden Ring's maps take mapbase's values (none of m10, m30 or m60 have the group). Without map lighting
///     mapbase's are used too. The LightSet's SsaoWeight0-3, which would let it darken the directional lights, are
///     not used. See Ssao.frag for how the values are read.
/// </summary>
public class SsaoPass : PostProcessPass
{
    private const int ModeOcclusion = 0;
    private const int ModeBlurX = 1;
    private const int ModeBlurY = 2;
    private const int ModeComposite = 3;

    /// <summary>The height the kernel's pixel sizes are taken to be for</summary>
    private const float ReferenceHeight = 1080.0f;

    private static readonly GparamSsao Defaults = new();

    private static ResourceLayout Layout;
    private static Pipeline Pipeline;
    private static Sampler LinearClamp;

    private readonly SceneRenderPipeline Owner;

    // Half size: the occlusion goes into A, is blurred across into B, and down back into A
    private readonly Texture[] Half = new Texture[2];
    private readonly Framebuffer[] HalfFramebuffers = new Framebuffer[2];

    private readonly DeviceBuffer[] ParamBuffers = new DeviceBuffer[4];
    private readonly List<Step> Steps = new();

    // What the steps' resource sets were made for
    private Texture BoundSource;
    private Texture BoundDestination;
    private int BoundTargetVersion = -1;
    private SceneTarget BoundTarget;

    public SsaoPass(SceneRenderPipeline owner)
    {
        Owner = owner;
    }

    public override string Name => "SSAO";

    /// <summary>The blurred half size visibility (1 open, 0 covered), for the debug view, or null</summary>
    public Texture Visibility => Half[0];

    /// <summary>Whether <see cref="Visibility" /> was filled this frame</summary>
    public bool Ran { get; private set; }

    public override void BeginFrame()
    {
        Ran = false;
    }

    public override bool IsActive(GparamLighting lighting)
    {
        return CFG.Current.Viewport_Post_Ssao;
    }

    public override unsafe void Run(GraphicsDevice gd, CommandList cl, PostProcessFrame frame)
    {
        var ssao = frame.Effects?.Ssao ?? Defaults;
        var target = frame.Target;

        EnsureResources(gd, frame);

        var scene = Owner.SceneParams;
        var projection = scene.Projection;
        Matrix4x4.Invert(scene.View * projection, out var clipToWorld);

        var baseParam = new SsaoParam
        {
            ClipToWorld = clipToWorld,
            Settings = new Vector4(0.0f, Math.Max(ssao.SampleOffset, 0.0f) * target.Height / ReferenceHeight,
                Math.Max(ssao.KernelMin, 0.0f), Math.Max(ssao.KernelMax, ssao.KernelMin)),
            Shape = new Vector4(Math.Max(ssao.KernelBaseDistance, 1e-3f), ssao.Scale, ssao.Offset,
                Math.Max(ssao.DepthRejectRate, 1e-3f)),
            Fade = new Vector4(ssao.DepthFadeStart, ssao.DepthFadeRange, ssao.PrecisionDepth, ssao.UseNormal ? 1.0f : 0.0f),
            // GLSL reads the matrix as uploaded, so its [c][r] is M(c+1)(r+1) here
            Depth = new Vector4(projection.M33, projection.M43, projection.M34, Owner.SceneDepth.TextureIndex),
            Eye = new Vector4(scene.EyePosition.X, scene.EyePosition.Y, scene.EyePosition.Z, MathF.Abs(projection.M22)),
            Strength = new Vector4(Math.Max(CFG.Current.Viewport_Post_Ssao_Strength, 0.0f), 0.0f, 0.0f, 0.0f)
        };

        // Every step's parameters first: each update is a copy, which can't happen inside the render passes below
        for (var i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];
            var param = baseParam;
            param.Settings.X = step.Mode;
            param.Sizes = new Vector4(target.Width, target.Height, step.Width, step.Height);
            cl.UpdateBuffer(ParamBuffers[i], 0, ref param, (uint)sizeof(SsaoParam));
        }

        cl.Barrier(VkPipelineStageFlags2.Transfer, VkAccessFlags2.TransferWrite,
            VkPipelineStageFlags2.FragmentShader, VkAccessFlags2.UniformRead);

        foreach (var step in Steps)
        {
            cl.SetFramebuffer(step.Framebuffer);
            cl.SetFullViewport(0);
            cl.SetPipeline(Pipeline);
            cl.SetGraphicsResourceSet(0, step.ResourceSet);
            SceneRenderer.GlobalTexturePool.BindTexturePool(cl, 1);
            cl.Draw(3, 1, 0, 0);
        }

        Ran = true;
    }

    private unsafe void EnsureResources(GraphicsDevice gd, PostProcessFrame frame)
    {
        ResourceFactory factory = gd.ResourceFactory;

        Layout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("SsaoParams", VkDescriptorType.UniformBuffer, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Occlusion", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Surface", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Indirect", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Scene", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
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
            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, factory, "Ssao");

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
            Pipeline.Name = "SSAO";
        }

        for (var i = 0; i < ParamBuffers.Length; i++)
        {
            ParamBuffers[i] ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(SsaoParam),
                VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));
        }

        var target = frame.Target;
        var resized = Half[0] == null || Half[0].Width != Math.Max(target.Width / 2, 1u) ||
                      Half[0].Height != Math.Max(target.Height / 2, 1u);

        if (resized)
        {
            CreateTextures(gd, target.Width, target.Height);
        }
        else if (ReferenceEquals(BoundSource, frame.Source) && ReferenceEquals(BoundDestination, frame.Destination) &&
                 ReferenceEquals(BoundTarget, target) && BoundTargetVersion == target.Version)
        {
            return;
        }

        foreach (var step in Steps)
        {
            SceneRenderer.DisposeAfterFramesInFlight(step.ResourceSet);
        }

        Steps.Clear();

        void AddStep(int mode, Framebuffer framebuffer, Texture drawn, Texture occlusion)
        {
            var index = Steps.Count;
            var set = factory.CreateResourceSet(new ResourceSetDescription(Layout, ParamBuffers[index], occlusion,
                target.Surface, target.Indirect, frame.Source, LinearClamp));
            Steps.Add(new Step(mode, framebuffer, drawn.Width, drawn.Height, set));
        }

        // The occlusion step reads no occlusion. Something already drawn this frame is bound in its place, as B
        // hasn't been on the first frame, and a texture never drawn isn't in a layout that can be read.
        AddStep(ModeOcclusion, HalfFramebuffers[0], Half[0], target.Indirect);
        AddStep(ModeBlurX, HalfFramebuffers[1], Half[1], Half[0]);
        AddStep(ModeBlurY, HalfFramebuffers[0], Half[0], Half[1]);
        AddStep(ModeComposite, frame.DestinationFramebuffer, frame.Destination, Half[0]);

        BoundSource = frame.Source;
        BoundDestination = frame.Destination;
        BoundTarget = target;
        BoundTargetVersion = target.Version;
    }

    private void CreateTextures(GraphicsDevice gd, uint width, uint height)
    {
        // A frame that is still executing can be reading the old textures
        for (var i = 0; i < Half.Length; i++)
        {
            if (HalfFramebuffers[i] != null)
            {
                SceneRenderer.DisposeAfterFramesInFlight(HalfFramebuffers[i]);
                SceneRenderer.DisposeAfterFramesInFlight(Half[i]);
            }

            TextureDescription description = TextureDescription.Texture2D(Math.Max(width / 2, 1u),
                Math.Max(height / 2, 1u), 1, 1, SceneTarget.ColorFormat,
                VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.Sampled,
                VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

            Half[i] = gd.ResourceFactory.CreateTexture(ref description);
            Half[i].Name = $"SSAO {i}";
            HalfFramebuffers[i] = gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, Half[i]));
        }
    }

    /// <summary>One draw: where it goes, its size, and what it reads</summary>
    private readonly record struct Step(int Mode, Framebuffer Framebuffer, uint Width, uint Height, ResourceSet ResourceSet);
}
