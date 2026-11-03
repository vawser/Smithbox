using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FxaaParam
{
    /// <summary>x: alpha, y: reduce min, z: reduce mul, w: span max</summary>
    public Vector4 Settings;

    /// <summary>xy: where the image's first pixel is drawn in the framebuffer</summary>
    public Vector4 Offset;
}

/// <summary>
///     FXAA from the GPARAM Anti-Alias group, after the tone map, as FXAA works on display values. While it is active
///     the tone map draws into <see cref="Framebuffer" />, an 8 bit image the size of the scene, and this draws that to
///     the screen. Used whatever AntiAliasType asks for until there is a TAA (Plan.md, Phase 9). Without map lighting
///     it uses Elden Ring's mapbase values.
/// </summary>
public class FxaaPass
{
    public const VkFormat ImageFormat = VkFormat.R8G8B8A8Unorm;

    /// <summary>The outputs of <see cref="Framebuffer" />, which the tone map draws to</summary>
    public static readonly OutputDescription ImageOutputs = new(null, new OutputAttachmentDescription(ImageFormat));

    private static readonly GparamAntiAlias DefaultSettings = new();

    private static readonly Dictionary<OutputDescription, Pipeline> Pipelines = new();
    private static ResourceLayout Layout;
    private static Sampler LinearClamp;

    private Texture Image;
    private DeviceBuffer ParamBuffer;
    private ResourceSet ResourceSet;
    private FxaaParam Params;

    public Framebuffer Framebuffer { get; private set; }

    public bool IsActive(GparamLighting lighting)
    {
        if (!CFG.Current.Viewport_Post_Effects || !CFG.Current.Viewport_Post_Fxaa)
        {
            return false;
        }

        // An Anti-Alias Type of TAA has TaaPass instead, where it runs
        if (TaaPass.Wants(lighting))
        {
            return false;
        }

        return (lighting?.PostEffects.AntiAlias ?? DefaultSettings).Enabled;
    }

    /// <summary>Binds <see cref="Framebuffer" />, the size of <paramref name="target" />, for the tone map to draw into</summary>
    public void BeginImage(GraphicsDevice gd, CommandList cl, SceneTarget target)
    {
        EnsureImage(gd, target.Width, target.Height);

        cl.SetFramebuffer(Framebuffer);
        cl.SetFullViewport(0);
    }

    /// <summary>
    ///     Draws the image into the bound framebuffer, whose outputs are <paramref name="outputs" />, at
    ///     <paramref name="offset" />. The viewport must already cover where it goes.
    /// </summary>
    public unsafe void Draw(GraphicsDevice gd, CommandList cl, OutputDescription outputs, Vector2 offset,
        GparamLighting lighting)
    {
        if (Image == null)
        {
            return;
        }

        var settings = lighting?.PostEffects.AntiAlias ?? DefaultSettings;

        Params.Settings = new Vector4(settings.FxaaAlpha, Math.Max(settings.FxaaReduceMin, 1e-6f),
            settings.FxaaReduceMul, Math.Max(settings.FxaaSpanMax, 0.0f));
        Params.Offset = new Vector4(offset, 0.0f, 0.0f);
        cl.UpdateBuffer(ParamBuffer, 0, ref Params, (uint)sizeof(FxaaParam));

        cl.SetPipeline(GetPipeline(gd, outputs));
        cl.SetGraphicsResourceSet(0, ResourceSet);
        cl.Draw(3, 1, 0, 0);
    }

    private unsafe void EnsureImage(GraphicsDevice gd, uint width, uint height)
    {
        ResourceFactory factory = gd.ResourceFactory;

        Layout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("FxaaParams", VkDescriptorType.UniformBuffer,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Image", VkDescriptorType.SampledImage,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("LinearClamp", VkDescriptorType.Sampler,
                VkShaderStageFlags.Fragment)));

        if (LinearClamp == null)
        {
            SamplerDescription description = SamplerDescription.Linear;
            description.AddressModeU = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeV = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeW = VkSamplerAddressMode.ClampToEdge;
            LinearClamp = factory.CreateSampler(ref description);
        }

        ParamBuffer ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(FxaaParam),
            VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));

        width = Math.Max(width, 1u);
        height = Math.Max(height, 1u);

        if (Image != null && Image.Width == width && Image.Height == height)
        {
            return;
        }

        // A frame that is still executing can be reading the old image
        if (Image != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(ResourceSet);
            SceneRenderer.DisposeAfterFramesInFlight(Framebuffer);
            SceneRenderer.DisposeAfterFramesInFlight(Image);
        }

        TextureDescription imageDescription = TextureDescription.Texture2D(width, height, 1, 1, ImageFormat,
            VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.Sampled,
            VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

        Image = factory.CreateTexture(ref imageDescription);
        Image.Name = "FXAA Image";
        Framebuffer = factory.CreateFramebuffer(new FramebufferDescription(null, Image));
        ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(Layout, ParamBuffer, Image, LinearClamp));
    }

    private static Pipeline GetPipeline(GraphicsDevice gd, OutputDescription outputs)
    {
        if (Pipelines.TryGetValue(outputs, out Pipeline pipeline))
        {
            return pipeline;
        }

        (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "Fxaa");

        GraphicsPipelineDescription description = new(
            BlendStateDescription.SingleOverrideBlend,
            DepthStencilStateDescription.Disabled,
            new RasterizerStateDescription(VkCullModeFlags.None, VkPolygonMode.Fill, VkFrontFace.Clockwise, true, false),
            VkPrimitiveTopology.TriangleList,
            new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), new[] { vs, fs },
                ShaderHelper.GetSpecializations(gd)),
            new[] { Layout },
            outputs);

        pipeline = gd.ResourceFactory.CreateGraphicsPipeline(ref description);
        pipeline.Name = "FXAA";
        Pipelines[outputs] = pipeline;

        return pipeline;
    }
}
