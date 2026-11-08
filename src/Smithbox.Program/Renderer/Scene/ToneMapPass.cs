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
    Filmic = 2,

    /// <summary>
    ///     DS3's and Bloodborne's Yebis filmic table (ToneMap_WriteFilmicToneMappingTablePS): Hejl and Burgess-Dawson's
    ///     curve, whose output is already display encoded, so no square root follows
    /// </summary>
    YebisFilmic = 3
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

    /// <summary>x: 1 to grade, y: the first LUT's slot, z: the second's, w: how far towards it</summary>
    public Vector4 Grading;

    /// <summary>The colour correction (<see cref="GparamToneCorrection" />): xyz the brightness, w the saturation</summary>
    public Vector4 CorrectionA;

    /// <summary>xyz the contrast, w the hue rotation in degrees. All 1 (and hue 0) leaves the colour as it is.</summary>
    public Vector4 CorrectionB;
}

/// <summary>
///     Takes a <see cref="SceneTarget" /> to a display framebuffer: the exposure, a tone mapping curve, and the
///     square root the shaders used to finish with as their gamma, then the map's color grading LUT (see
///     <see cref="ColorGradingLuts" />). One per <see cref="SceneRenderPipeline" />, as each reads its own target; the
///     pipelines are shared between those drawing to the same outputs, and the LUT atlas between them all.
/// </summary>
public class ToneMapPass
{
    private static readonly Dictionary<OutputDescription, Pipeline> Pipelines = new();
    private static ResourceLayout Layout;

    // The LUTs on the GPU, made from the last ColorGradingLuts drawn with, and a 1 x 1 texture bound without them
    private static Texture LutAtlas;
    private static ColorGradingLuts LutAtlasSource;
    private static Texture NoLuts;
    private static Sampler LutSampler;

    private DeviceBuffer ParamBuffer;
    private ResourceSet ResourceSet;
    // The texture ResourceSet reads: the scene target's color, or the post-processing chain's output. A resized
    // target or chain makes new textures, so the reference tells when the set is stale.
    private Texture BoundSource;
    private DeviceBuffer BoundExposureState;
    private Texture BoundLuts;
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
        // filmic contrast; YebisFilmicMode is DS3 and BB's Yebis-ToneType 2. The others shift colors in ways that aren't
        // known, so are left clipped.
        return lighting.ToneMapMode switch
        {
            8 => (ToneMapOperator.Reinhard,
                Math.Max(lighting.ReinhardContrast * lighting.ReinhardWhiteBalance, 1e-3f)),
            7 => (ToneMapOperator.Filmic, whitePoint),
            GparamLighting.YebisFilmicMode => (ToneMapOperator.YebisFilmic, whitePoint),
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

        // The LUTs either side of the time of day; with only one found, both are it
        var grading = lighting?.PostEffects.ColorGrading;
        var grade = !passThrough && RenderMode.PostEffects && CFG.Current.Viewport_Post_Color_Grading &&
                    grading is { Enabled: true, Luts: not null } && (grading.Slot >= 0 || grading.NextSlot >= 0);
        var slot = grade ? grading.Slot >= 0 ? grading.Slot : grading.NextSlot : 0;
        var nextSlot = grade ? grading.NextSlot >= 0 ? grading.NextSlot : slot : 0;

        EnsureResources(gd, source, applyAutoExposure ? autoExposure.StateBuffer : AutoExposurePass.GetNeutralState(gd),
            grade ? GetLutAtlas(gd, grading.Luts) : GetNoLuts(gd));

        var (op, whitePoint) = passThrough ? (ToneMapOperator.Clamp, 1.0f) : Resolve(lighting);
        var exposure = passThrough ? 1.0f : Math.Max(CFG.Current.Viewport_ToneMap_Exposure, 0.0f);

        Params.Settings = new Vector4(exposure, (float)op, whitePoint, applyAutoExposure ? 1.0f : 0.0f);
        Params.Offset = new Vector4(offset, 0.0f, 0.0f);
        Params.Grading = grade
            ? new Vector4(1.0f, slot, nextSlot, Math.Clamp(grading.Blend, 0.0f, 1.0f))
            : Vector4.Zero;

        // The older games' colour correction (Demon's Souls' ToneCorrectBank); the identity otherwise
        var correction = lighting?.ToneCorrection;
        var correct = !passThrough && RenderMode.PostEffects && CFG.Current.Viewport_Post_Tone_Correction &&
                      correction is { Enabled: true };
        Params.CorrectionA = correct ? new Vector4(correction.Brightness, correction.Saturation) : Vector4.One;
        Params.CorrectionB = correct ? new Vector4(correction.Contrast, correction.Hue) : new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
        cl.UpdateBuffer(ParamBuffer, 0, ref Params, (uint)sizeof(ToneMapParam));

        cl.SetPipeline(GetPipeline(gd, outputs));
        cl.SetGraphicsResourceSet(0, ResourceSet);
        cl.Draw(3, 1, 0, 0);
    }

    private unsafe void EnsureResources(GraphicsDevice gd, Texture source, DeviceBuffer exposureState, Texture luts)
    {
        ResourceFactory factory = gd.ResourceFactory;

        ParamBuffer ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(ToneMapParam),
            VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));

        if (ReferenceEquals(BoundSource, source) && ReferenceEquals(BoundExposureState, exposureState) &&
            ReferenceEquals(BoundLuts, luts) && ResourceSet != null)
        {
            return;
        }

        if (ResourceSet != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(ResourceSet);
        }

        ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(GetLayout(factory), ParamBuffer, source,
            gd.PointSampler, exposureState, luts, LutSampler));
        BoundSource = source;
        BoundExposureState = exposureState;
        BoundLuts = luts;
    }

    private static ResourceLayout GetLayout(ResourceFactory factory)
    {
        Layout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ToneMapParams", VkDescriptorType.UniformBuffer,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("SceneColor", VkDescriptorType.SampledImage,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("SceneSampler", VkDescriptorType.Sampler,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("ExposureState", VkDescriptorType.StorageBuffer,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("LutAtlas", VkDescriptorType.SampledImage,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("LutSampler", VkDescriptorType.Sampler,
                VkShaderStageFlags.Fragment)));

        return Layout;
    }

    private static Sampler GetLutSampler(ResourceFactory factory)
    {
        if (LutSampler == null)
        {
            SamplerDescription description = SamplerDescription.Linear;
            description.AddressModeU = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeV = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeW = VkSamplerAddressMode.ClampToEdge;
            LutSampler = factory.CreateSampler(ref description);
        }

        return LutSampler;
    }

    /// <summary>The atlas of <paramref name="luts" /> on the GPU, uploaded again only when they are another set</summary>
    private static Texture GetLutAtlas(GraphicsDevice gd, ColorGradingLuts luts)
    {
        GetLutSampler(gd.ResourceFactory);

        if (LutAtlas != null && ReferenceEquals(LutAtlasSource, luts))
        {
            return LutAtlas;
        }

        // A frame that is still executing can be reading the old atlas
        if (LutAtlas != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(LutAtlas);
        }

        LutAtlas = CreateLutTexture(gd, luts.Atlas, (uint)luts.Width, ColorGradingLuts.Height, "Color Grading LUTs");
        LutAtlasSource = luts;

        return LutAtlas;
    }

    private static Texture GetNoLuts(GraphicsDevice gd)
    {
        GetLutSampler(gd.ResourceFactory);
        return NoLuts ??= CreateLutTexture(gd, new byte[] { 255, 255, 255, 255 }, 1, 1, "No Color Grading LUTs");
    }

    private static unsafe Texture CreateLutTexture(GraphicsDevice gd, byte[] pixels, uint width, uint height, string name)
    {
        TextureDescription description = TextureDescription.Texture2D(width, height, 1, 1, VkFormat.R8G8B8A8Unorm,
            VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst, VkImageCreateFlags.None, VkImageTiling.Optimal,
            VkSampleCountFlags.Count1);

        var texture = gd.ResourceFactory.CreateTexture(ref description);
        texture.Name = name;

        fixed (byte* data = pixels)
        {
            gd.UpdateTexture(texture, (IntPtr)data, (uint)pixels.Length, 0, 0, 0, width, height, 1, 0, 0);
        }

        return texture;
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
            new[] { GetLayout(gd.ResourceFactory) },
            outputs);

        pipeline = gd.ResourceFactory.CreateGraphicsPipeline(ref description);
        pipeline.Name = "ToneMap";
        Pipelines[outputs] = pipeline;

        return pipeline;
    }
}
