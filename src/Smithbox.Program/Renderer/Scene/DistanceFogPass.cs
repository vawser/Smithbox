using StudioCore.Application;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>Laid out as DistanceFog.frag's fogParams</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DistanceFogParam
{
    /// <summary>x: begin, y: end (meters along the camera's axis), z: the density at the end, w: unused</summary>
    public Vector4 Range;

    /// <summary>rgb: the fog's light, in the target's units (the exposure in), w: unused</summary>
    public Vector4 Color;
}

/// <summary>
///     The older games' classic distance fog (<see cref="GparamDistanceFog" />, Demon's Souls' FogBank): a fullscreen
///     draw that blends each pixel towards the fog's colour by how far it is, from none at Begin to MaxDensity at End.
///     Drawn where the scene's depth is copied after its opaque draws, after the volumetric fog, so like it the fog is
///     under the transparent draws. Pixels with nothing drawn (the viewport's background) are left as they are.
///     A reading of the bank's fields, not the game's shader.
/// </summary>
public class DistanceFogPass
{
    private static ResourceLayout Layout;
    private static Pipeline DrawPipeline;
    private static Sampler PointClamp;

    private DeviceBuffer ParamBuffer;
    private ResourceSet DrawSet;

    /// <summary>Releases this pass's own buffer and set; the layout, pipeline and sampler are shared</summary>
    public void Dispose()
    {
        SceneRenderer.DisposeAllAfterFramesInFlight(DrawSet, ParamBuffer);
        DrawSet = null;
        ParamBuffer = null;
    }

    private DistanceFogParam Params;
    private bool Pending;

    /// <summary>Whether the fog was laid over the scene in the last frame drawn</summary>
    public bool Drawn { get; private set; }

    public static bool IsActive(GparamLighting lighting)
    {
        return RenderMode.PostEffects && CFG.Current.Viewport_Post_Distance_Fog &&
               lighting is { PostEffects.DistanceFog: { Enabled: true, MaxDensity: > 0.0f } };
    }

    /// <summary>This frame's parameters from the map's lighting, or the fog left out of the frame if it isn't active</summary>
    public void Prepare(GparamLighting lighting)
    {
        if (!IsActive(lighting))
        {
            Pending = false;
            Drawn = false;
            return;
        }

        var fog = lighting.PostEffects.DistanceFog;
        var exposure = lighting.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale;

        var begin = fog.Begin;
        var end = Math.Max(fog.End, begin + 0.01f);

        Params = new DistanceFogParam
        {
            Range = new Vector4(begin, end, Math.Clamp(fog.MaxDensity, 0.0f, 1.0f), 0.0f),
            Color = new Vector4(Vector3.Max(new Vector3(fog.Color.X, fog.Color.Y, fog.Color.Z) * fog.Color.W * exposure,
                Vector3.Zero), 0.0f)
        };

        Pending = true;
    }

    /// <summary>
    ///     Lays the fog over <paramref name="pipeline" />'s target, bound with its opaque draws done and their depth
    ///     copied (<see cref="SceneRenderPipeline.CaptureSceneDepth" />)
    /// </summary>
    public unsafe void Apply(GraphicsDevice gd, CommandList cl, SceneRenderPipeline pipeline)
    {
        if (!Pending)
        {
            Drawn = false;
            return;
        }

        Pending = false;

        var target = pipeline.Target;

        if (target.Width == 0 || target.Height == 0)
        {
            Drawn = false;
            return;
        }

        EnsureResources(gd);

        cl.UpdateBuffer(ParamBuffer, 0, ref Params, (uint)sizeof(DistanceFogParam));

        cl.Barrier(VkPipelineStageFlags2.Transfer, VkAccessFlags2.TransferWrite,
            VkPipelineStageFlags2.FragmentShader, VkAccessFlags2.UniformRead);

        cl.SetPipeline(DrawPipeline);
        cl.SetGraphicsResourceSet(0, pipeline.SceneParamResourceSet);
        SceneRenderer.GlobalTexturePool.BindTexturePool(cl, 1);
        cl.SetGraphicsResourceSet(2, DrawSet);
        cl.Draw(3, 1, 0, 0);

        Drawn = true;
    }

    private unsafe void EnsureResources(GraphicsDevice gd)
    {
        ResourceFactory factory = gd.ResourceFactory;

        Layout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("FogParams", VkDescriptorType.UniformBuffer, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("PointClamp", VkDescriptorType.Sampler, VkShaderStageFlags.Fragment)));

        if (PointClamp == null)
        {
            SamplerDescription description = SamplerDescription.Point;
            description.AddressModeU = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeV = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeW = VkSamplerAddressMode.ClampToEdge;
            PointClamp = factory.CreateSampler(ref description);
        }

        if (DrawPipeline == null)
        {
            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, factory, "DistanceFog");

            // scene x (1 - fog) + the fog's light x fog, as the volumetric fog blends; the target's alpha is kept
            var blend = SceneTarget.WithSurface(new BlendStateDescription(RgbaFloat.Black, new BlendAttachmentDescription(true,
                VkBlendFactor.One, VkBlendFactor.SrcAlpha, VkBlendOp.Add,
                VkBlendFactor.Zero, VkBlendFactor.One, VkBlendOp.Add)));

            ResourceLayout sceneParamLayout = StaticResourceCache.GetResourceLayout(factory,
                StaticResourceCache.SceneParamLayoutDescription);

            GraphicsPipelineDescription description = new(
                blend,
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(VkCullModeFlags.None, VkPolygonMode.Fill, VkFrontFace.Clockwise, true, false),
                VkPrimitiveTopology.TriangleList,
                new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), new[] { vs, fs },
                    ShaderHelper.GetSpecializations(gd)),
                new[] { sceneParamLayout, SceneRenderer.GlobalTexturePool.GetLayout(), Layout },
                SceneTarget.Outputs);

            DrawPipeline = factory.CreateGraphicsPipeline(ref description);
            DrawPipeline.Name = "Distance Fog";
        }

        ParamBuffer ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(DistanceFogParam),
            VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));

        DrawSet ??= factory.CreateResourceSet(new ResourceSetDescription(Layout, ParamBuffer, PointClamp));
    }
}
