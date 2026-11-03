using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>The tone mapping curves of ToneMap.frag</summary>
public enum ToneMapOperator
{
    /// <summary>Clipped at 1, which is how the viewport looked before it had an HDR target</summary>
    Clamp = 0,

    /// <summary>Extended Reinhard on luminance, reaching white at the white point</summary>
    Reinhard = 1,

    /// <summary>The ACES filmic curve, as fitted by Narkowicz</summary>
    Filmic = 2
}

/// <summary>The viewport setting for tone mapping, CFG.Viewport_ToneMap</summary>
public enum ToneMapSetting
{
    /// <summary>The map's GPARAM Tone Map group, or <see cref="ToneMapOperator.Clamp" /> without map lighting</summary>
    Gparam = 0,
    Clamp = 1,
    Reinhard = 2,
    Filmic = 3
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ToneMapParam
{
    /// <summary>x: exposure, y: <see cref="ToneMapOperator" />, z: white point, w: 1 to apply the auto exposure</summary>
    public Vector4 Settings;

    /// <summary>xy: where the scene target's first pixel is drawn in the framebuffer</summary>
    public Vector4 Offset;
}

/// <summary>
///     Takes a <see cref="SceneTarget" /> to a display framebuffer: the exposure, a tone mapping curve, and the
///     square root the shaders used to finish with as their gamma. One per <see cref="SceneRenderPipeline" />, as each
///     reads its own target; the pipelines are shared between those drawing to the same outputs.
/// </summary>
public class ToneMapPass
{
    private static readonly Dictionary<OutputDescription, Pipeline> Pipelines = new();
    private static ResourceLayout Layout;

    private DeviceBuffer ParamBuffer;
    private ResourceSet ResourceSet;
    // The texture ResourceSet reads: the scene target's color, or the post-processing chain's output. A resized
    // target or chain makes new textures, so the reference tells when the set is stale.
    private Texture BoundSource;
    private DeviceBuffer BoundExposureState;
    private ToneMapParam Params;

    /// <summary>
    ///     The curve, white point and exposure for a pipeline lit with <paramref name="lighting" />, or by its own
    ///     settings when that is null. GPARAM exposure is already in the target, see <see cref="SceneRenderPipeline" />.
    /// </summary>
    public static (ToneMapOperator Operator, float WhitePoint) Resolve(GparamLighting lighting)
    {
        var whitePoint = Math.Max(CFG.Current.Viewport_ToneMap_White_Point, 1e-3f);

        switch ((ToneMapSetting)CFG.Current.Viewport_ToneMap)
        {
            case ToneMapSetting.Clamp:
                return (ToneMapOperator.Clamp, whitePoint);
            case ToneMapSetting.Reinhard:
                return (ToneMapOperator.Reinhard, whitePoint);
            case ToneMapSetting.Filmic:
                return (ToneMapOperator.Filmic, whitePoint);
        }

        if (lighting == null)
        {
            return (ToneMapOperator.Clamp, whitePoint);
        }

        // ToneMapMode 8 is "Reinhard Contrast and White Balance", which every Elden Ring GPARAM uses, and 7 the
        // filmic contrast. The others shift colors in ways that aren't known, so are left clipped.
        return lighting.ToneMapMode switch
        {
            8 => (ToneMapOperator.Reinhard,
                Math.Max(lighting.ReinhardContrast * lighting.ReinhardWhiteBalance, 1e-3f)),
            7 => (ToneMapOperator.Filmic, whitePoint),
            _ => (ToneMapOperator.Clamp, whitePoint)
        };
    }

    /// <summary>
    ///     Draws <paramref name="source" /> into the bound framebuffer, whose outputs are <paramref name="outputs" />,
    ///     at <paramref name="offset" />. The viewport must already cover where it goes.
    /// </summary>
    /// <param name="source">The linear light to tone map: the scene target's color, or what
    /// <see cref="PostProcessChain.Run" /> returned for it</param>
    /// <param name="autoExposure">The pipeline's auto exposure, whose adjustment is applied if it ran this frame</param>
    /// <param name="passThrough">Whether the source is a debug view of display values, squared, which is shown as
    /// it is: no exposure and no curve, just the square root</param>
    public unsafe void Draw(GraphicsDevice gd, CommandList cl, Texture source, OutputDescription outputs,
        Vector2 offset, GparamLighting lighting, AutoExposurePass autoExposure = null, bool passThrough = false)
    {
        if (source == null)
        {
            return;
        }

        var applyAutoExposure = !passThrough && autoExposure is { Applied: true };
        EnsureResources(gd, source, applyAutoExposure ? autoExposure.StateBuffer : AutoExposurePass.GetNeutralState(gd));

        var (op, whitePoint) = passThrough ? (ToneMapOperator.Clamp, 1.0f) : Resolve(lighting);
        var exposure = passThrough ? 1.0f : Math.Max(CFG.Current.Viewport_ToneMap_Exposure, 0.0f);

        Params.Settings = new Vector4(exposure, (float)op, whitePoint, applyAutoExposure ? 1.0f : 0.0f);
        Params.Offset = new Vector4(offset, 0.0f, 0.0f);
        cl.UpdateBuffer(ParamBuffer, 0, ref Params, (uint)sizeof(ToneMapParam));

        cl.SetPipeline(GetPipeline(gd, outputs));
        cl.SetGraphicsResourceSet(0, ResourceSet);
        cl.Draw(3, 1, 0, 0);
    }

    private unsafe void EnsureResources(GraphicsDevice gd, Texture source, DeviceBuffer exposureState)
    {
        ResourceFactory factory = gd.ResourceFactory;

        Layout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ToneMapParams", VkDescriptorType.UniformBuffer,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("SceneColor", VkDescriptorType.SampledImage,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("SceneSampler", VkDescriptorType.Sampler,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("ExposureState", VkDescriptorType.StorageBuffer,
                VkShaderStageFlags.Fragment)));

        ParamBuffer ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(ToneMapParam),
            VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));

        if (ReferenceEquals(BoundSource, source) && ReferenceEquals(BoundExposureState, exposureState) &&
            ResourceSet != null)
        {
            return;
        }

        if (ResourceSet != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(ResourceSet);
        }

        ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(Layout, ParamBuffer, source,
            gd.PointSampler, exposureState));
        BoundSource = source;
        BoundExposureState = exposureState;
    }

    private static Pipeline GetPipeline(GraphicsDevice gd, OutputDescription outputs)
    {
        if (Pipelines.TryGetValue(outputs, out Pipeline pipeline))
        {
            return pipeline;
        }

        (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, "ToneMap");

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
        pipeline.Name = "ToneMap";
        Pipelines[outputs] = pipeline;

        return pipeline;
    }
}
