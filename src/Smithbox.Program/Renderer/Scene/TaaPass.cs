using StudioCore.Application;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>Laid out as Taa.frag's TaaParams</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TaaParam
{
    public Matrix4x4 ClipToWorld;
    public Matrix4x4 PreviousViewProjection;

    /// <summary>x: feedback weight, y: 1 if there is a history, z: sharpen intensity, w: sharpen range</summary>
    public Vector4 Settings;

    /// <summary>xyz: FBWeightBorderDepth(0-2)</summary>
    public Vector4 Borders;

    /// <summary>FBWeightCancelByDepth(0-3)</summary>
    public Vector4 Cancel;

    /// <summary>xyz: projection [2][2], [3][2] and [2][3] as the shader indexes it, w: scene depth texture index</summary>
    public Vector4 Depth;

    /// <summary>xy: the scene's size</summary>
    public Vector4 Sizes;
}

/// <summary>
///     The GPARAM Anti-Alias-Editor group's TAA, Anti-Alias Type 5, which Elden Ring's mapbase uses: the scene is drawn
///     a sub-pixel off each frame (<see cref="JitterFor" />, JitterCount Halton offsets, which
///     <see cref="SceneRenderPipeline" /> puts in the projection) and each frame is blended into a history of the
///     ones before. Replaces FXAA where it runs. After SSAO and SSR, so their noise is smoothed too, and before auto
///     exposure and bloom. Only the viewports that run the screen effects jitter, see
///     <see cref="SceneRenderPipeline.AllowTaa" />. See Taa.frag for how the values are read.
/// </summary>
public class TaaPass : PostProcessPass
{
    /// <summary>The Anti-Alias Type that is TAA</summary>
    public const int TaaType = 5;

    private static readonly GparamAntiAlias Defaults = new();

    private static ResourceLayout Layout;
    private static Pipeline Pipeline;
    private static Sampler LinearClamp;

    private readonly SceneRenderPipeline Owner;

    private Texture History;
    private DeviceBuffer ParamBuffer;
    private ResourceSet ResourceSet;

    // What the resource set was made for
    private Texture BoundSource;
    private Texture BoundHistory;

    // The last frame's unjittered view and projection, and whether the history holds it
    private Matrix4x4 PreviousViewProjection;
    private bool HasHistory;
    private bool RanLastFrame;
    private bool RanThisFrame;

    public TaaPass(SceneRenderPipeline owner)
    {
        Owner = owner;
    }

    public override string Name => "TAA";

    /// <summary>Whether the lighting's Anti-Alias group asks for TAA, as far as the viewport's switches allow it</summary>
    public static bool Wants(GparamLighting lighting)
    {
        var aa = lighting?.PostEffects.AntiAlias ?? Defaults;
        return CFG.Current.Viewport_Post_Effects && CFG.Current.Viewport_Post_Taa && aa.Enabled && aa.Type == TaaType;
    }

    /// <summary>
    ///     The sub-pixel offset of frame <paramref name="frame" />, in pixels from the centre, -0.5 to 0.5: the Halton
    ///     (2, 3) sequence over <paramref name="count" /> frames
    /// </summary>
    public static Vector2 JitterFor(long frame, int count)
    {
        var index = (int)(frame % Math.Max(count, 1)) + 1;
        return new Vector2(Halton(index, 2) - 0.5f, Halton(index, 3) - 0.5f);
    }

    private static float Halton(int index, int radix)
    {
        var result = 0.0f;
        var fraction = 1.0f / radix;

        while (index > 0)
        {
            result += index % radix * fraction;
            index /= radix;
            fraction /= radix;
        }

        return result;
    }

    public override void BeginFrame()
    {
        RanLastFrame = RanThisFrame;
        RanThisFrame = false;
    }

    public override bool IsActive(GparamLighting lighting)
    {
        return Owner.AllowTaa && Wants(lighting);
    }

    public override unsafe void Run(GraphicsDevice gd, CommandList cl, PostProcessFrame frame)
    {
        var aa = frame.Effects?.AntiAlias ?? Defaults;
        var target = frame.Target;

        var resized = EnsureResources(gd, frame);

        var scene = Owner.SceneParams;
        var projection = scene.Projection;
        var viewProjection = scene.View * Owner.UnjitteredProjection;
        Matrix4x4.Invert(viewProjection, out var clipToWorld);

        // A history from another size, or from before TAA was off, would smear what is no longer there
        var useHistory = HasHistory && RanLastFrame && !resized;

        var param = new TaaParam
        {
            ClipToWorld = clipToWorld,
            PreviousViewProjection = PreviousViewProjection,
            Settings = new Vector4(aa.FeedbackWeight, useHistory ? 1.0f : 0.0f, Math.Max(aa.SharpenIntensity, 0.0f),
                Math.Max(Math.Abs(aa.SharpenRangeMin), Math.Abs(aa.SharpenRangeMax))),
            Borders = new Vector4(aa.BorderDepth, 0.0f),
            Cancel = aa.CancelByDepth,
            // GLSL reads the matrix as uploaded, so its [c][r] is M(c+1)(r+1) here
            Depth = new Vector4(projection.M33, projection.M43, projection.M34, Owner.SceneDepth.TextureIndex),
            Sizes = new Vector4(target.Width, target.Height, 0.0f, 0.0f)
        };

        cl.UpdateBuffer(ParamBuffer, 0, ref param, (uint)sizeof(TaaParam));
        cl.Barrier(VkPipelineStageFlags2.Transfer, VkAccessFlags2.TransferWrite,
            VkPipelineStageFlags2.FragmentShader, VkAccessFlags2.UniformRead);

        cl.SetFramebuffer(frame.DestinationFramebuffer);
        cl.SetFullViewport(0);
        cl.SetPipeline(Pipeline);
        cl.SetGraphicsResourceSet(0, ResourceSet);
        SceneRenderer.GlobalTexturePool.BindTexturePool(cl, 1);
        cl.Draw(3, 1, 0, 0);

        // This frame is next frame's history. The copy ends the render pass, and leaves both textures readable.
        cl.CopyTexture(frame.Destination, 0, 0, 0, 0, 0, History, 0, 0, 0, 0, 0, target.Width, target.Height, 1, 1);

        PreviousViewProjection = viewProjection;
        HasHistory = true;
        RanThisFrame = true;
    }

    /// <summary>True if the history was made anew, so holds nothing yet</summary>
    private unsafe bool EnsureResources(GraphicsDevice gd, PostProcessFrame frame)
    {
        ResourceFactory factory = gd.ResourceFactory;

        Layout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("TaaParams", VkDescriptorType.UniformBuffer, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("Current", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
            new ResourceLayoutElementDescription("History", VkDescriptorType.SampledImage, VkShaderStageFlags.Fragment),
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
            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, factory, "Taa");

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
            Pipeline.Name = "TAA";
        }

        ParamBuffer ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(TaaParam),
            VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));

        var target = frame.Target;
        var resized = false;

        if (History == null || History.Width != target.Width || History.Height != target.Height)
        {
            // A frame that is still executing can be reading the old history
            if (History != null)
            {
                SceneRenderer.DisposeAfterFramesInFlight(History);
            }

            // Filled from this frame's result before anything reads it, so it starts out cleared to nothing
            TextureDescription description = TextureDescription.Texture2D(target.Width, target.Height, 1, 1,
                SceneTarget.ColorFormat, VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst,
                VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);
            History = factory.CreateTexture(ref description);
            History.Name = "TAA History";

            HasHistory = false;
            resized = true;
        }

        // Until the history has been written it isn't in a layout that can be read; the source stands in, and the
        // shader is told not to read it
        var historyBinding = HasHistory ? History : frame.Source;

        if (ReferenceEquals(BoundSource, frame.Source) && ReferenceEquals(BoundHistory, historyBinding) &&
            ResourceSet != null)
        {
            return resized;
        }

        if (ResourceSet != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(ResourceSet);
        }

        ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(Layout, ParamBuffer, frame.Source,
            historyBinding, LinearClamp));
        BoundSource = frame.Source;
        BoundHistory = historyBinding;

        return resized;
    }
}
