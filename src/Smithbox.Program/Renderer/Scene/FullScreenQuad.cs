using StudioCore.Application;
using StudioCore.Editors.Viewport;
using StudioCore.Utilities;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public class FullScreenQuad
{
    private static readonly ushort[] s_quadIndices = { 0, 1, 2, 0, 2, 3 };
    private DisposeCollector _disposeCollector;
    private DeviceBuffer _ib;
    private Pipeline _pipeline;
    private DeviceBuffer _vb;

    // Quad Parameters
    public DeviceBuffer QuadParamBuffer { get; private set; }
    public ResourceSet QuadParamResourceSet { get; private set; }
    private ResourceLayout _quadParamLayout;
    public QuadParam QuadParams;

    public unsafe void CreateDeviceObjects(GraphicsDevice gd, CommandList cl)
    {
        DisposeCollectorResourceFactory factory = new(gd.ResourceFactory);
        _disposeCollector = factory.DisposeCollector;

        // Setup QuadParam uniform buffer
        QuadParamBuffer = factory.CreateBuffer(
            new BufferDescription(
                (uint)sizeof(QuadParam),
                VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                0));

        var color = CFG.Current.Viewport_Bg_Color;

        QuadParams = new QuadParam
        {
            Color = new Vector4(color.X, color.Y, color.Z, 1.0f)
        };

        cl.UpdateBuffer(QuadParamBuffer, 0, ref QuadParams, (uint)sizeof(QuadParam));

        // Setup resource layout for QuadParam
        _quadParamLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription(
                "QuadParams",
                VkDescriptorType.UniformBuffer,
                VkShaderStageFlags.Fragment)));

        QuadParamResourceSet = factory.CreateResourceSet(
            new ResourceSetDescription(_quadParamLayout, QuadParamBuffer));

        (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "FullScreenQuad");

        GraphicsPipelineDescription pd = new(
            new BlendStateDescription(
                RgbaFloat.Black,
                BlendAttachmentDescription.OverrideBlend),
            new DepthStencilStateDescription(true, true, VkCompareOp.Always),
            new RasterizerStateDescription(VkCullModeFlags.Back, VkPolygonMode.Fill, VkFrontFace.Clockwise, true, false),
            VkPrimitiveTopology.TriangleList,
            new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VkFormat.R32G32Sfloat),
                        new VertexElementDescription("TexCoords", VkFormat.R32G32Sfloat))
                },
                new[] { vs, fs },
                ShaderHelper.GetSpecializations(gd)),
            new ResourceLayout[] { _quadParamLayout },
            gd.SwapchainFramebuffer.OutputDescription);

        _pipeline = factory.CreateGraphicsPipeline(ref pd);

        var verts = ViewportUtils.GetFullScreenQuadVerts(gd);

        _vb = factory.CreateBuffer(
            new BufferDescription(
                (uint)verts.Length * sizeof(float),
                VkBufferUsageFlags.VertexBuffer | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                0));

        cl.UpdateBuffer(_vb, 0, verts);

        _ib = factory.CreateBuffer(
            new BufferDescription(
                (uint)s_quadIndices.Length * sizeof(ushort),
                VkBufferUsageFlags.IndexBuffer | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                0));

        cl.UpdateBuffer(_ib, 0, s_quadIndices);

        cl.Barrier(VkPipelineStageFlags2.Transfer,
            VkAccessFlags2.TransferWrite,
            VkPipelineStageFlags2.VertexInput | VkPipelineStageFlags2.IndexInput,
            VkAccessFlags2.VertexAttributeRead);
    }

    public unsafe void UpdateQuadParams(GraphicsDevice gd, Vector4 color)
    {
        QuadParams.Color = color;
        gd.UpdateBuffer(QuadParamBuffer, 0, ref QuadParams, (uint)sizeof(QuadParam));
    }

    public void DestroyDeviceObjects()
    {
        _disposeCollector.DisposeAll();
    }

    public void Render(GraphicsDevice gd, CommandList cl)
    {
        var color = CFG.Current.Viewport_Bg_Color;

        UpdateQuadParams(gd, new Vector4(color.X, color.Y, color.Z, 1.0f));

        cl.SetPipeline(_pipeline);
        cl.SetGraphicsResourceSet(0, QuadParamResourceSet);
        cl.SetVertexBuffer(0, _vb);
        cl.SetIndexBuffer(_ib, VkIndexType.Uint16);
        cl.DrawIndexed(6, 1, 0, 0, 0);
    }
}
