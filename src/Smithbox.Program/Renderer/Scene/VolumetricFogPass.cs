using StudioCore.Application;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>Laid out as VolumetricFog.comp's fogParams</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VolumetricFogParam
{
    /// <summary>The inverse of the camera's view and projection</summary>
    public Matrix4x4 ClipToWorld;

    /// <summary>x: cells across, y: cells down, z: slices, w: steps through each slice</summary>
    public Vector4 Volume;

    /// <summary>x: near depth, y: far depth, z: log(far / near), w: fog end depth</summary>
    public Vector4 DepthRange;

    /// <summary>x: fixed density, y: height fog density scale, z: distribution, w: height offset</summary>
    public Vector4 Density;

    /// <summary>x: how much more light the fog takes away than it scatters</summary>
    public Vector4 Extinction;

    /// <summary>rgb: sky light with its intensity and the exposure, w: height attenuation</summary>
    public Vector4 Sky;

    /// <summary>rgb: the sun's light looking towards it, w: height attenuation</summary>
    public Vector4 Direct;

    /// <summary>rgb: the sun's light looking away from it, w: ScatteringG</summary>
    public Vector4 Opposite;

    /// <summary>x: sky start distance, y: sun start distance, z: local light scale, w: local light range scale</summary>
    public Vector4 Start;

    /// <summary>xyz: the direction the sun's light travels, w: 1 to shadow it</summary>
    public Vector4 Sun;

    /// <summary>x: 1 to add the map's lights, y: 1 for the analytic fog past the far depth, zw: target size</summary>
    public Vector4 Flags;
}

/// <summary>
///     The GPARAM VolumetricFog group, which is Elden Ring's distance and height fog (its Fog group has no fields).
///     A compute pass fills a froxel volume over the camera's view (VolumeWidth across, VolumeDepth slices out to
///     FarDepth) with the light the fog scatters and lets through, lit by the sky, the sun through its shadow
///     cascades, and the map's lights; then a fullscreen draw lays it over the scene. Both run where the scene's depth
///     is copied after its opaque draws (<see cref="SceneRenderPipeline.CaptureSceneDepth" />), so the fog is under
///     the transparent draws, which aren't fogged yet. See VolumetricFog.comp for how the values are read, which is
///     a reading of them, not the game's code.
/// </summary>
public class VolumetricFogPass
{
    private const int MaxVolumeSize = 256;
    private const int MaxSlices = 128;

    private static ResourceLayout ComputeLayout;
    private static ResourceLayout DrawLayout;
    private static ComputePass Compute;
    private static Pipeline DrawPipeline;
    private static Sampler LinearClamp;
    private static Sampler Point;
    private static Texture NoShadowMap;

    private DeviceBuffer ParamBuffer;
    private Texture Volume;
    private ResourceSet ComputeSet;
    private ResourceSet DrawSet;

    // What the resource sets were made for
    private Texture BoundShadowMap;
    private DeviceBuffer BoundLightBuffer;

    private VolumetricFogParam Params;
    private bool Pending;

    public string Name => "Volumetric Fog";

    /// <summary>Whether the fog was laid over the scene in the last frame drawn</summary>
    public bool Drawn { get; private set; }

    /// <summary>The volume's cells across, down and its slices, as last drawn</summary>
    public (int Width, int Height, int Depth) VolumeSize { get; private set; }

    public static bool IsActive(GparamLighting lighting)
    {
        return CFG.Current.Viewport_Post_Effects && CFG.Current.Viewport_Post_Volumetric_Fog &&
               lighting is { PostEffects.VolumetricFog.Enabled: true };
    }

    /// <summary>
    ///     Works out this frame's parameters from the camera and the map's lighting, or leaves the fog out of the
    ///     frame if it isn't active. <paramref name="sunTravel" /> is the direction the sun's light travels in.
    /// </summary>
    public void Prepare(Matrix4x4 view, Matrix4x4 projection, GparamLighting lighting, Vector3 sunTravel)
    {
        if (!IsActive(lighting) || !Matrix4x4.Invert(view * projection, out var clipToWorld))
        {
            Pending = false;
            Drawn = false;
            return;
        }

        var fog = lighting.PostEffects.VolumetricFog;
        var exposure = lighting.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale;

        // Square cells: the target's aspect is the projection's
        var aspect = MathF.Abs(projection.M11) > 1e-6f ? MathF.Abs(projection.M22 / projection.M11) : 1.0f;
        var width = Math.Clamp(fog.VolumeWidth, 8, MaxVolumeSize);
        var height = Math.Clamp((int)MathF.Round(width / Math.Max(aspect, 1e-3f)), 4, MaxVolumeSize);
        var slices = Math.Clamp(fog.VolumeDepth, 4, MaxSlices);

        var near = Math.Max(fog.NearDepth, 0.01f);
        var far = Math.Max(fog.FarDepth, near * 1.01f);
        var absorption = (fog.AbsorptionScale.X + fog.AbsorptionScale.Y + fog.AbsorptionScale.Z) / 3.0f;

        Params = new VolumetricFogParam
        {
            ClipToWorld = clipToWorld,
            Volume = new Vector4(width, height, slices, Math.Clamp(fog.SampleCount, 1, 16)),
            DepthRange = new Vector4(near, far, MathF.Log(far / near), Math.Max(fog.FogEndDepth, far)),
            Density = new Vector4(Math.Max(fog.FixedDensity, 0.0f), Math.Max(fog.HeightFogDensityScale, 0.0f),
                fog.HeightFogDistribution, fog.HeightFogOffset),
            Extinction = new Vector4(Math.Max(absorption, 0.0f), 0.0f, 0.0f, 0.0f),
            Sky = new Vector4(Light(fog.SkyColor, exposure), fog.SkyHeightAtten),
            Direct = new Vector4(Light(fog.DirectColor, exposure), fog.DirHeightAtten),
            Opposite = new Vector4(Light(fog.OppositeColor, exposure), Math.Clamp(fog.ScatteringG, -0.95f, 0.95f)),
            Start = new Vector4(fog.SkyStartDistance, fog.DirStartDistance, fog.LocalLightScale, fog.LocalLightDistScale),
            Sun = new Vector4(sunTravel, CFG.Current.Viewport_Post_Fog_Shadows ? 1.0f : 0.0f),
            Flags = new Vector4(CFG.Current.Viewport_Post_Fog_Local_Lights ? 1.0f : 0.0f,
                fog.EnableAnalyticalFog ? 1.0f : 0.0f, 0.0f, 0.0f)
        };

        Pending = true;
    }

    /// <summary>A GPARAM color, rgb times its intensity in w and the exposure</summary>
    private static Vector3 Light(Vector4 color, float exposure)
    {
        return Vector3.Max(new Vector3(color.X, color.Y, color.Z) * color.W * exposure, Vector3.Zero);
    }

    /// <summary>
    ///     Fills the volume and lays it over <paramref name="pipeline" />'s target, which is bound with its opaque
    ///     draws done and their depth copied. The dispatch ends the target's render pass; the draw after it carries
    ///     on with what the target holds.
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

        var size = ((int)Params.Volume.X, (int)Params.Volume.Y, (int)Params.Volume.Z);
        EnsureResources(gd, pipeline, size);

        Params.Flags.Z = target.Width;
        Params.Flags.W = target.Height;
        cl.UpdateBuffer(ParamBuffer, 0, ref Params, (uint)sizeof(VolumetricFogParam));

        // The parameters' copy has to land, and the last frame's draw may still be reading the volume
        cl.Barrier(VkPipelineStageFlags2.Transfer | VkPipelineStageFlags2.FragmentShader,
            VkAccessFlags2.TransferWrite | VkAccessFlags2.ShaderRead,
            VkPipelineStageFlags2.ComputeShader | VkPipelineStageFlags2.FragmentShader,
            VkAccessFlags2.UniformRead | VkAccessFlags2.ShaderWrite);

        Compute.Dispatch(gd, cl, new[] { ComputeSet }, (uint)size.Item1, (uint)size.Item2);

        cl.Barrier(VkPipelineStageFlags2.ComputeShader, VkAccessFlags2.ShaderWrite,
            VkPipelineStageFlags2.FragmentShader, VkAccessFlags2.ShaderRead);

        cl.SetPipeline(DrawPipeline);
        cl.SetGraphicsResourceSet(0, pipeline.SceneParamResourceSet);
        SceneRenderer.GlobalTexturePool.BindTexturePool(cl, 1);
        cl.SetGraphicsResourceSet(2, DrawSet);
        cl.Draw(3, 1, 0, 0);

        VolumeSize = size;
        Drawn = true;
    }

    private unsafe void EnsureResources(GraphicsDevice gd, SceneRenderPipeline pipeline, (int, int, int) size)
    {
        ResourceFactory factory = gd.ResourceFactory;

        ComputeLayout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("FogParams", VkDescriptorType.UniformBuffer, VkShaderStageFlags.Compute),
            new ResourceLayoutElementDescription("SceneParams", VkDescriptorType.UniformBuffer, VkShaderStageFlags.Compute),
            new ResourceLayoutElementDescription("ShadowMap", VkDescriptorType.SampledImage, VkShaderStageFlags.Compute),
            new ResourceLayoutElementDescription("PointSampler", VkDescriptorType.Sampler, VkShaderStageFlags.Compute),
            new ResourceLayoutElementDescription("MapLights", VkDescriptorType.StorageBuffer, VkShaderStageFlags.Compute),
            new ResourceLayoutElementDescription("Volume", VkDescriptorType.StorageImage, VkShaderStageFlags.Compute)));

        DrawLayout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("FogParams", VkDescriptorType.UniformBuffer, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Volume", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("LinearClamp", VkDescriptorType.Sampler, VkShaderStageFlags.Fragment)));

        Compute ??= new ComputePass("VolumetricFog", new[] { ComputeLayout }, 8, 8);

        if (LinearClamp == null)
        {
            SamplerDescription description = SamplerDescription.Linear;
            description.AddressModeU = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeV = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeW = VkSamplerAddressMode.ClampToEdge;
            LinearClamp = factory.CreateSampler(ref description);

            description = SamplerDescription.Point;
            description.AddressModeU = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeV = VkSamplerAddressMode.ClampToEdge;
            description.AddressModeW = VkSamplerAddressMode.ClampToEdge;
            Point = factory.CreateSampler(ref description);
        }

        if (NoShadowMap == null)
        {
            TextureDescription description = TextureDescription.Texture2D(1, 1, 1, 1, VkFormat.R32Sfloat,
                VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst, VkImageCreateFlags.None,
                VkImageTiling.Optimal, VkSampleCountFlags.Count1);
            NoShadowMap = factory.CreateTexture(ref description);
            NoShadowMap.Name = "Volumetric Fog No Shadow Map";
        }

        if (DrawPipeline == null)
        {
            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, factory, "VolumetricFog");

            // scene x the fog's alpha (what it lets through) + the fog's light; the target's alpha is kept
            var blend = new BlendStateDescription(RgbaFloat.Black, new BlendAttachmentDescription(true,
                VkBlendFactor.One, VkBlendFactor.SrcAlpha, VkBlendOp.Add,
                VkBlendFactor.Zero, VkBlendFactor.One, VkBlendOp.Add));

            ResourceLayout sceneParamLayout = StaticResourceCache.GetResourceLayout(factory,
                StaticResourceCache.SceneParamLayoutDescription);

            GraphicsPipelineDescription description = new(
                blend,
                DepthStencilStateDescription.Disabled,
                new RasterizerStateDescription(VkCullModeFlags.None, VkPolygonMode.Fill, VkFrontFace.Clockwise, true, false),
                VkPrimitiveTopology.TriangleList,
                new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), new[] { vs, fs },
                    ShaderHelper.GetSpecializations(gd)),
                new[] { sceneParamLayout, SceneRenderer.GlobalTexturePool.GetLayout(), DrawLayout },
                SceneTarget.Outputs);

            DrawPipeline = factory.CreateGraphicsPipeline(ref description);
            DrawPipeline.Name = "Volumetric Fog";
        }

        ParamBuffer ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(VolumetricFogParam),
            VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));

        var recreate = false;

        if (Volume == null || Volume.Width != size.Item1 || Volume.Height != size.Item2 || Volume.Depth != size.Item3)
        {
            // A frame that is still executing can be reading the old volume
            if (Volume != null)
            {
                SceneRenderer.DisposeAfterFramesInFlight(Volume);
            }

            TextureDescription description = TextureDescription.Texture3D((uint)size.Item1, (uint)size.Item2,
                (uint)size.Item3, 1, VkFormat.R16G16B16A16Sfloat, VkImageUsageFlags.Storage | VkImageUsageFlags.Sampled,
                VkImageCreateFlags.None, VkImageTiling.Optimal);
            Volume = factory.CreateTexture(ref description);
            Volume.Name = "Volumetric Fog Volume";
            recreate = true;
        }

        var shadowMap = pipeline.Shadows?.MapTexture ?? NoShadowMap;
        var lightBuffer = SceneRenderer.MaterialBufferAllocator.BackingBuffer;

        if (!recreate && ComputeSet != null && ReferenceEquals(BoundShadowMap, shadowMap) &&
            ReferenceEquals(BoundLightBuffer, lightBuffer))
        {
            return;
        }

        if (ComputeSet != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(ComputeSet);
            SceneRenderer.DisposeAfterFramesInFlight(DrawSet);
        }

        ComputeSet = factory.CreateResourceSet(new ResourceSetDescription(ComputeLayout, ParamBuffer,
            pipeline.SceneParamBuffer, shadowMap, Point, lightBuffer, Volume));
        DrawSet = factory.CreateResourceSet(new ResourceSetDescription(DrawLayout, ParamBuffer, Volume, LinearClamp));

        BoundShadowMap = shadowMap;
        BoundLightBuffer = lightBuffer;
    }
}
