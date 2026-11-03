using StudioCore.Application;
using System;
using System.Numerics;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>What <see cref="SurfaceViewPass" /> shows in place of the image</summary>
public enum SurfaceView
{
    None = 0,
    Normals = 1,
    Roughness = 2,
    Specular = 3,
    Indirect = 4,
    AmbientOcclusion = 5
}

/// <summary>
///     A debug view of the scene target's surface attachments (<see cref="SceneTarget.Surface" /> and
///     <see cref="SceneTarget.Indirect" />), drawn last in the chain in place of the image, so what SSAO and SSR read
///     can be checked by eye. Normals, roughness and specular are shown as they are, with the tone map passed through
///     (<see cref="ShowsDisplayValues" />); the indirect light is tone mapped as the scene is.
/// </summary>
public class SurfaceViewPass : PostProcessPass
{
    private static ResourceLayout Layout;
    private static Pipeline Pipeline;

    private DeviceBuffer ParamBuffer;
    private ResourceSet ResourceSet;
    private int BoundVersion = -1;
    private SceneTarget BoundTarget;
    private Texture BoundOcclusion;

    private readonly SsaoPass Ssao;

    /// <param name="ssao">The pipeline's SSAO, whose visibility the ambient occlusion view shows</param>
    public SurfaceViewPass(SsaoPass ssao)
    {
        Ssao = ssao;
    }

    public override string Name => "Surface View";

    /// <summary>Whether the image this frame is a debug view the tone map should pass through as it is</summary>
    public bool ShowsDisplayValues { get; private set; }

    public override void BeginFrame()
    {
        ShowsDisplayValues = false;
    }

    public override bool IsActive(GparamLighting lighting)
    {
        return (SurfaceView)CFG.Current.Viewport_Post_Surface_View != SurfaceView.None;
    }

    public override unsafe void Run(GraphicsDevice gd, CommandList cl, PostProcessFrame frame)
    {
        var view = (SurfaceView)CFG.Current.Viewport_Post_Surface_View;

        EnsureResources(gd, frame.Target);

        var settings = new Vector4((float)view, Ssao.Ran ? 1.0f : 0.0f, 0.0f, 0.0f);
        cl.UpdateBuffer(ParamBuffer, 0, ref settings, (uint)sizeof(Vector4));
        cl.Barrier(VkPipelineStageFlags2.Transfer, VkAccessFlags2.TransferWrite,
            VkPipelineStageFlags2.FragmentShader, VkAccessFlags2.UniformRead);

        cl.SetFramebuffer(frame.DestinationFramebuffer);
        cl.SetFullViewport(0);
        cl.SetPipeline(Pipeline);
        cl.SetGraphicsResourceSet(0, ResourceSet);
        cl.Draw(3, 1, 0, 0);

        ShowsDisplayValues = view != SurfaceView.Indirect;
    }

    private unsafe void EnsureResources(GraphicsDevice gd, SceneTarget target)
    {
        ResourceFactory factory = gd.ResourceFactory;

        Layout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("SurfaceViewParams", VkDescriptorType.UniformBuffer,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Surface", VkDescriptorType.SampledImage,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Indirect", VkDescriptorType.SampledImage,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("PointSampler", VkDescriptorType.Sampler,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Occlusion", VkDescriptorType.SampledImage,
                VkShaderStageFlags.Fragment)));

        if (Pipeline == null)
        {
            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, factory, "SurfaceView");

            GraphicsPipelineDescription description = new(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(VkCullModeFlags.None, VkPolygonMode.Fill, VkFrontFace.Clockwise, true, false),
                VkPrimitiveTopology.TriangleList,
                new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), new[] { vs, fs },
                    ShaderHelper.GetSpecializations(gd)),
                new[] { Layout },
                PostProcessChain.Outputs);

            Pipeline = factory.CreateGraphicsPipeline(ref description);
            Pipeline.Name = "Surface View";
        }

        ParamBuffer ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(Vector4),
            VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));

        // Before SSAO first runs there is no visibility; the indirect light stands in, and isn't shown
        var occlusion = Ssao.Visibility ?? target.Indirect;

        if (ResourceSet != null && ReferenceEquals(BoundTarget, target) && BoundVersion == target.Version &&
            ReferenceEquals(BoundOcclusion, occlusion))
        {
            return;
        }

        if (ResourceSet != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(ResourceSet);
        }

        ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(Layout, ParamBuffer, target.Surface,
            target.Indirect, gd.PointSampler, occlusion));
        BoundTarget = target;
        BoundVersion = target.Version;
        BoundOcclusion = occlusion;
    }
}
