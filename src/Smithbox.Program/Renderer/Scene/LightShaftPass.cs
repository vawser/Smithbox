using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>Laid out as LightShaft.frag's LightShaftParams</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LightShaftParam
{
    public Matrix4x4 ClipToWorld;

    /// <summary>x: mode, y: mask depth threshold, z: cos of the sky mask size angle, w: center power</summary>
    public Vector4 Settings;

    /// <summary>xy: the sun's point on screen (0-1), z: how far towards the parallel blur, w: fade</summary>
    public Vector4 Sun;

    /// <summary>xy: the parallel blur over the screen, z: the radial blur's reach, w: falloff</summary>
    public Vector4 Blur;

    /// <summary>rgb: Sky Multiply Color x w, w: sky color threshold</summary>
    public Vector4 Multiply;

    /// <summary>rgb: Sky Add Color x w with its share of the exposure, w: saturate</summary>
    public Vector4 Add;

    /// <summary>xyz: projection [2][2], [3][2] and [2][3] as the shader indexes it, w: scene depth texture index</summary>
    public Vector4 Depth;

    /// <summary>xyz: the direction towards the sun, w: this blur pass's share of the whole length</summary>
    public Vector4 ToSun;

    public Vector4 Eye;

    /// <summary>zw: the size of the target drawn</summary>
    public Vector4 Sizes;
}

/// <summary>
///     The GPARAM LightShaft[Yebis] group: rays fanning out from the sun where it shows through. The sky around the
///     sun is masked at half size, blurred towards the sun ShaftBlurCount times, and added to the scene. The sun is
///     the LightSet's when EnableRefLight is set (Elden Ring's files point at bank 160000000, taken to be the scene's
///     LightSet), and the group's own Angle otherwise. Runs before bloom, so the brightest shafts glow too. See
///     LightShaft.frag for how the values are read, which is a reading of them, not the game's code.
/// </summary>
public class LightShaftPass : PostProcessPass
{
    private const int ModeMask = 0;
    private const int ModeBlur = 1;
    private const int ModeComposite = 2;

    private const int MaxBlurPasses = 4;
    private const int Taps = 16;

    private static ResourceLayout Layout;
    private static Pipeline Pipeline;
    private static Sampler LinearClamp;

    private readonly SceneRenderPipeline Owner;
    private readonly AutoExposurePass AutoExposure;

    // Half size: the mask, then the blur passes back and forth between the two
    private readonly Texture[] Half = new Texture[2];
    private readonly Framebuffer[] HalfFramebuffers = new Framebuffer[2];
    private Texture Mask;
    private Framebuffer MaskFramebuffer;

    private readonly DeviceBuffer[] ParamBuffers = new DeviceBuffer[MaxBlurPasses + 2];
    private readonly List<Step> Steps = new();

    public override void Dispose()
    {
        // A step's framebuffer can be the chain's, so only its set is this pass's
        foreach (var step in Steps)
            SceneRenderer.DisposeAllAfterFramesInFlight(step.ResourceSet);

        Steps.Clear();

        SceneRenderer.DisposeAllAfterFramesInFlight(ParamBuffers);
        SceneRenderer.DisposeAllAfterFramesInFlight(HalfFramebuffers);
        SceneRenderer.DisposeAllAfterFramesInFlight(Half);
        SceneRenderer.DisposeAllAfterFramesInFlight(MaskFramebuffer, Mask);

        Array.Clear(ParamBuffers);
        Array.Clear(HalfFramebuffers);
        Array.Clear(Half);
        MaskFramebuffer = null;
        Mask = null;

        BoundSource = null;
        BoundDestination = null;
        BoundExposureState = null;
    }

    // What the steps' resource sets were made for
    private Texture BoundSource;
    private Texture BoundDestination;
    private DeviceBuffer BoundExposureState;
    private (uint Width, uint Height, int Passes) BoundSize;

    public LightShaftPass(SceneRenderPipeline owner, AutoExposurePass autoExposure)
    {
        Owner = owner;
        AutoExposure = autoExposure;
    }

    public override string Name => "Light Shafts";

    public override bool IsActive(GparamLighting lighting)
    {
        // The shafts are traced from the eye, which an orthographic view hasn't got
        return CFG.Current.Viewport_Post_Light_Shafts && lighting is { PostEffects.LightShaft.Enabled: true } &&
               !Owner.IsOrthographic;
    }

    public override unsafe void Run(GraphicsDevice gd, CommandList cl, PostProcessFrame frame)
    {
        var shaft = frame.Effects.LightShaft;
        var lighting = frame.Lighting;
        var passes = Math.Clamp(shaft.BlurCount, 1, MaxBlurPasses);

        EnsureResources(gd, frame, passes);

        var scene = Owner.SceneParams;
        var view = scene.View;
        var projection = scene.Projection;
        var eye = new Vector3(scene.EyePosition.X, scene.EyePosition.Y, scene.EyePosition.Z);
        var viewProjection = view * projection;
        Matrix4x4.Invert(viewProjection, out var clipToWorld);

        var travel = shaft.EnableRefLight ? lighting.Sun.Direction : GparamLighting.AngleToDirection(shaft.Angle);
        var toSun = travel.LengthSquared() > 1e-10f ? Vector3.Normalize(-travel) : Vector3.UnitY;

        // The camera's forward and the sun's angle from it
        Matrix4x4.Invert(view, out var cameraWorld);
        var forward = Vector3.Normalize(Vector3.TransformNormal(-Vector3.UnitZ, cameraWorld));
        var angle = MathF.Acos(Math.Clamp(Vector3.Dot(forward, toSun), -1.0f, 1.0f));

        var fade = 1.0f - SmoothStep(shaft.FadeOutBeginAngle, shaft.FadeOutEndAngle, angle);
        var parallel = SmoothStep(shaft.ParallelBeginAngle, shaft.ParallelEndAngle, angle);

        // The sun's point on screen; behind the camera there is none, and the blur is parallel
        var sunScreen = ToScreen(gd, viewProjection, eye + toSun * 1000.0f, out var inFront);
        if (!inFront)
        {
            parallel = 1.0f;
        }

        // Across the screen in the sun's direction: from a point ahead of the camera, a little towards the sun
        var ahead = ToScreen(gd, viewProjection, eye + forward, out _);
        var aside = ToScreen(gd, viewProjection, eye + forward + toSun * 0.01f, out _);
        var across = aside - ahead;
        across = across.LengthSquared() > 1e-12f ? Vector2.Normalize(across) : Vector2.UnitY;
        var parallelBlur = across * Math.Max(shaft.BlurRangeParallel, 0.0f);

        var exposure = lighting.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale;
        var addExposure = 1.0f + (exposure - 1.0f) * Math.Clamp(shaft.ExposureInfluenceAddColor, 0.0f, 1.0f);

        var multiply = shaft.SkyMultiplyColor;
        var add = shaft.SkyAddColor;

        var baseParam = new LightShaftParam
        {
            ClipToWorld = clipToWorld,
            Settings = new Vector4(0.0f, shaft.MaskDepthThreshold, MathF.Cos(Math.Clamp(shaft.SkyMaskSizeAngle, 1e-3f, MathF.PI)),
                shaft.SkyMaskCenterPower),
            Sun = new Vector4(sunScreen, parallel, fade),
            Blur = new Vector4(parallelBlur, Math.Clamp(shaft.BlurRange * 0.25f, 0.0f, 1.0f), Math.Max(shaft.BlurFalloff, 0.0f)),
            Multiply = new Vector4(Vector3.Max(new Vector3(multiply.X, multiply.Y, multiply.Z) * multiply.W, Vector3.Zero),
                Math.Max(shaft.SkyColorThreshold, 0.0f)),
            Add = new Vector4(Vector3.Max(new Vector3(add.X, add.Y, add.Z) * add.W * addExposure, Vector3.Zero),
                shaft.SkyColorSaturate),
            // GLSL reads the matrix as uploaded, so its [c][r] is M(c+1)(r+1) here
            Depth = new Vector4(projection.M33, projection.M43, projection.M34, Owner.SceneDepth.TextureIndex),
            Eye = new Vector4(eye, 1.0f)
        };

        // Every step's parameters first: each update is a copy, which can't happen inside the render passes below
        var share = 1.0f;

        for (var i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];
            var param = baseParam;
            param.Settings.X = step.Mode;
            param.ToSun = new Vector4(toSun, step.Mode == ModeBlur ? share : 1.0f);
            param.Sizes = new Vector4(0.0f, 0.0f, step.Width, step.Height);
            cl.UpdateBuffer(ParamBuffers[i], 0, ref param, (uint)sizeof(LightShaftParam));

            // Each blur pass fills in between the taps of the one before
            if (step.Mode == ModeBlur)
            {
                share /= Taps;
            }
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

    }

    private static float SmoothStep(float edge0, float edge1, float x)
    {
        if (edge1 <= edge0)
        {
            return x >= edge0 ? 1.0f : 0.0f;
        }

        var t = Math.Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
        return t * t * (3.0f - 2.0f * t);
    }

    /// <summary>
    ///     Where a world point lands on the target, 0-1 from its top left as gl_FragCoord runs. With the standard clip
    ///     Y, Veldrid flips the viewport, so clip +Y is the top row.
    /// </summary>
    private static Vector2 ToScreen(GraphicsDevice gd, Matrix4x4 viewProjection, Vector3 world, out bool inFront)
    {
        var clip = Vector4.Transform(new Vector4(world, 1.0f), viewProjection);
        inFront = clip.W > 1e-6f;

        var w = inFront ? clip.W : 1e-6f;
        var ndc = new Vector2(clip.X / w, clip.Y / w);

        return new Vector2(ndc.X * 0.5f + 0.5f, gd.IsClipSpaceYInverted ? ndc.Y * 0.5f + 0.5f : 0.5f - ndc.Y * 0.5f);
    }

    private unsafe void EnsureResources(GraphicsDevice gd, PostProcessFrame frame, int passes)
    {
        ResourceFactory factory = gd.ResourceFactory;

        Layout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("LightShaftParams", VkDescriptorType.UniformBuffer,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("TextureA", VkDescriptorType.SampledImage,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("TextureB", VkDescriptorType.SampledImage,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("LinearClamp", VkDescriptorType.Sampler,
                VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("ExposureState", VkDescriptorType.StorageBuffer,
                VkShaderStageFlags.Fragment)));

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
            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, factory, "LightShaft");

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
            Pipeline.Name = "Light Shaft";
        }

        for (var i = 0; i < ParamBuffers.Length; i++)
        {
            ParamBuffers[i] ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(LightShaftParam),
                VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));
        }

        var target = frame.Target;
        var size = (target.Width, target.Height, passes);
        var exposureState = AutoExposure is { Applied: true } ? AutoExposure.StateBuffer : AutoExposurePass.GetNeutralState(gd);

        if ((size.Width, size.Height) != (BoundSize.Width, BoundSize.Height))
        {
            CreateTextures(gd, target.Width, target.Height);
        }
        else if (size.passes == BoundSize.Passes && ReferenceEquals(BoundSource, frame.Source) &&
                 ReferenceEquals(BoundDestination, frame.Destination) && ReferenceEquals(BoundExposureState, exposureState))
        {
            return;
        }

        foreach (var step in Steps)
        {
            SceneRenderer.DisposeAfterFramesInFlight(step.ResourceSet);
        }

        Steps.Clear();

        void AddStep(int mode, Framebuffer framebuffer, Texture drawn, Texture a, Texture b)
        {
            var index = Steps.Count;
            var set = factory.CreateResourceSet(new ResourceSetDescription(Layout, ParamBuffers[index], a, b,
                LinearClamp, exposureState));
            Steps.Add(new Step(mode, framebuffer, drawn.Width, drawn.Height, set));
        }

        AddStep(ModeMask, MaskFramebuffer, Mask, frame.Source, frame.Source);

        var read = Mask;

        for (var i = 0; i < passes; i++)
        {
            AddStep(ModeBlur, HalfFramebuffers[i % 2], Half[i % 2], read, read);
            read = Half[i % 2];
        }

        AddStep(ModeComposite, frame.DestinationFramebuffer, frame.Destination, frame.Source, read);

        BoundSource = frame.Source;
        BoundDestination = frame.Destination;
        BoundExposureState = exposureState;
        BoundSize = size;
    }

    private void CreateTextures(GraphicsDevice gd, uint width, uint height)
    {
        // A frame that is still executing can be reading the old textures
        if (MaskFramebuffer != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(MaskFramebuffer);
            SceneRenderer.DisposeAfterFramesInFlight(Mask);

            for (var i = 0; i < Half.Length; i++)
            {
                SceneRenderer.DisposeAfterFramesInFlight(HalfFramebuffers[i]);
                SceneRenderer.DisposeAfterFramesInFlight(Half[i]);
            }
        }

        var w = Math.Max(width / 2, 1u);
        var h = Math.Max(height / 2, 1u);

        Texture Create(string name)
        {
            TextureDescription description = TextureDescription.Texture2D(w, h, 1, 1, SceneTarget.ColorFormat,
                VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.Sampled,
                VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

            var texture = gd.ResourceFactory.CreateTexture(ref description);
            texture.Name = name;
            return texture;
        }

        Mask = Create("Light Shaft Mask");
        MaskFramebuffer = gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, Mask));

        for (var i = 0; i < Half.Length; i++)
        {
            Half[i] = Create($"Light Shaft Blur {i}");
            HalfFramebuffers[i] = gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, Half[i]));
        }
    }

    /// <summary>One draw: where it goes, its size, and what it reads</summary>
    private readonly record struct Step(int Mode, Framebuffer Framebuffer, uint Width, uint Height, ResourceSet ResourceSet);
}
