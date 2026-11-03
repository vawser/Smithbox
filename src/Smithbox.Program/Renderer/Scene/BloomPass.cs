using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BloomParam
{
    /// <summary>x: mode, y: threshold, z: knee, w: luminance</summary>
    public Vector4 Settings;

    /// <summary>x: saturation control, y: radius scale, z: alpha, w: 1 / level count</summary>
    public Vector4 Shape;

    /// <summary>xy: the size of a texel of the texture filtered, zw: the size of the target drawn</summary>
    public Vector4 Sizes;
}

/// <summary>
///     The GPARAM Bloom group (Glare fields): what is over the threshold is blurred down a chain of
///     <see cref="GparamBloom.NumLevels" /> half-size levels and back up, then added to the scene. Each level going up
///     is its own texture, as the up pass reads the level below and the down level beside it; Veldrid discards a
///     framebuffer's contents when it is bound, so adding into a level with blending would add onto nothing.
///     See Bloom.frag for each step.
/// </summary>
public class BloomPass : PostProcessPass
{
    private const int MaxLevels = 8;

    private const int ModePrefilter = 0;
    private const int ModeDownsample = 1;
    private const int ModeUpsample = 2;
    private const int ModeComposite = 3;

    private static ResourceLayout Layout;
    private static Pipeline Pipeline;
    private static Sampler LinearClamp;

    private readonly AutoExposurePass AutoExposure;

    // Level i is half of level i - 1, the first half the scene
    private readonly List<Texture> Down = new();
    private readonly List<Framebuffer> DownFramebuffers = new();

    // Up[i] is Down[i] plus the blurred Up[i + 1] (Down for the last level), so Up[0] holds every level's sum
    private readonly List<Texture> Up = new();
    private readonly List<Framebuffer> UpFramebuffers = new();

    private readonly DeviceBuffer[] ParamBuffers = new DeviceBuffer[MaxLevels * 2];
    private readonly List<Step> Steps = new();

    // What the steps' resource sets were made for
    private Texture BoundSource;
    private Texture BoundDestination;
    private DeviceBuffer BoundExposureState;
    private (uint Width, uint Height, int Levels) BoundSize;

    public BloomPass(AutoExposurePass autoExposure)
    {
        AutoExposure = autoExposure;
    }

    public override string Name => "Bloom";

    public override bool IsActive(GparamLighting lighting)
    {
        return CFG.Current.Viewport_Post_Bloom && lighting is { PostEffects.Bloom.Enabled: true };
    }

    public override unsafe void Run(GraphicsDevice gd, CommandList cl, PostProcessFrame frame)
    {
        var bloom = frame.Effects.Bloom;
        var levels = Math.Clamp(bloom.NumLevels, 1, MaxLevels);

        EnsureResources(gd, frame, levels);

        var knee = Math.Max(bloom.Threshold, 1e-3f) * 0.5f * Math.Max(bloom.Smooth, 0.0f);
        var settings = new Vector4(0.0f, bloom.Threshold, knee, bloom.Luminance);
        var shape = new Vector4(bloom.SaturateControl, Math.Max(bloom.GaussianRadiusScale, 0.0f), bloom.Alpha,
            1.0f / levels);

        // Every step's parameters first: each update is a copy, which can't happen inside the render passes below
        for (var i = 0; i < Steps.Count; i++)
        {
            var step = Steps[i];
            var param = new BloomParam
            {
                Settings = settings with { X = step.Mode },
                Shape = shape,
                Sizes = new Vector4(1.0f / step.FilteredWidth, 1.0f / step.FilteredHeight, step.Width, step.Height)
            };
            cl.UpdateBuffer(ParamBuffers[i], 0, ref param, (uint)sizeof(BloomParam));
        }

        cl.Barrier(VkPipelineStageFlags2.Transfer, VkAccessFlags2.TransferWrite,
            VkPipelineStageFlags2.FragmentShader, VkAccessFlags2.UniformRead);

        foreach (var step in Steps)
        {
            cl.SetFramebuffer(step.Framebuffer);
            cl.SetFullViewport(0);
            cl.SetPipeline(Pipeline);
            cl.SetGraphicsResourceSet(0, step.ResourceSet);
            cl.Draw(3, 1, 0, 0);
        }
    }

    private static (uint Width, uint Height) LevelSize(uint width, uint height, int level)
    {
        return (Math.Max(width >> (level + 1), 1u), Math.Max(height >> (level + 1), 1u));
    }

    private unsafe void EnsureResources(GraphicsDevice gd, PostProcessFrame frame, int levels)
    {
        ResourceFactory factory = gd.ResourceFactory;

        Layout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("BloomParams", VkDescriptorType.UniformBuffer,
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
            (Shader vs, Shader fs) = StaticResourceCache.GetShaders(gd, factory, "Bloom");

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
            Pipeline.Name = "Bloom";
        }

        for (var i = 0; i < ParamBuffers.Length; i++)
        {
            ParamBuffers[i] ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(BloomParam),
                VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));
        }

        var target = frame.Target;
        var size = (target.Width, target.Height, levels);
        var exposureState = AutoExposure is { Applied: true } ? AutoExposure.StateBuffer : AutoExposurePass.GetNeutralState(gd);

        if (size != BoundSize)
        {
            CreateLevels(gd, target.Width, target.Height, levels);
        }
        else if (ReferenceEquals(BoundSource, frame.Source) && ReferenceEquals(BoundDestination, frame.Destination) &&
                 ReferenceEquals(BoundExposureState, exposureState))
        {
            return;
        }

        foreach (var step in Steps)
        {
            SceneRenderer.DisposeAfterFramesInFlight(step.ResourceSet);
        }

        Steps.Clear();

        void AddStep(int mode, Framebuffer framebuffer, uint width, uint height, Texture a, Texture b, Texture filtered)
        {
            var index = Steps.Count;
            var set = factory.CreateResourceSet(new ResourceSetDescription(Layout, ParamBuffers[index], a, b,
                LinearClamp, exposureState));
            Steps.Add(new Step(mode, framebuffer, width, height, filtered.Width, filtered.Height, set));
        }

        AddStep(ModePrefilter, DownFramebuffers[0], Down[0].Width, Down[0].Height, frame.Source, frame.Source, frame.Source);

        for (var i = 1; i < levels; i++)
        {
            AddStep(ModeDownsample, DownFramebuffers[i], Down[i].Width, Down[i].Height, Down[i - 1], Down[i - 1], Down[i - 1]);
        }

        var below = Down[levels - 1];

        for (var i = levels - 2; i >= 0; i--)
        {
            AddStep(ModeUpsample, UpFramebuffers[i], Up[i].Width, Up[i].Height, Down[i], below, below);
            below = Up[i];
        }

        AddStep(ModeComposite, frame.DestinationFramebuffer, frame.Destination.Width, frame.Destination.Height,
            frame.Source, below, below);

        BoundSource = frame.Source;
        BoundDestination = frame.Destination;
        BoundExposureState = exposureState;
    }

    private void CreateLevels(GraphicsDevice gd, uint width, uint height, int levels)
    {
        // A frame that is still executing can be reading the old levels
        foreach (var framebuffer in DownFramebuffers)
            SceneRenderer.DisposeAfterFramesInFlight(framebuffer);
        foreach (var texture in Down)
            SceneRenderer.DisposeAfterFramesInFlight(texture);
        foreach (var framebuffer in UpFramebuffers)
            SceneRenderer.DisposeAfterFramesInFlight(framebuffer);
        foreach (var texture in Up)
            SceneRenderer.DisposeAfterFramesInFlight(texture);

        Down.Clear();
        DownFramebuffers.Clear();
        Up.Clear();
        UpFramebuffers.Clear();

        Texture CreateLevel(uint w, uint h, string name)
        {
            TextureDescription description = TextureDescription.Texture2D(w, h, 1, 1, SceneTarget.ColorFormat,
                VkImageUsageFlags.ColorAttachment | VkImageUsageFlags.Sampled,
                VkImageCreateFlags.None, VkImageTiling.Optimal, VkSampleCountFlags.Count1);

            var texture = gd.ResourceFactory.CreateTexture(ref description);
            texture.Name = name;
            return texture;
        }

        for (var i = 0; i < levels; i++)
        {
            var (w, h) = LevelSize(width, height, i);

            var down = CreateLevel(w, h, $"Bloom Down {i}");
            Down.Add(down);
            DownFramebuffers.Add(gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, down)));

            if (i < levels - 1)
            {
                var up = CreateLevel(w, h, $"Bloom Up {i}");
                Up.Add(up);
                UpFramebuffers.Add(gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(null, up)));
            }
        }

        BoundSize = (width, height, levels);
    }

    /// <summary>One draw of the chain: where it goes, the size of the texture it filters, and what it reads</summary>
    private readonly record struct Step(int Mode, Framebuffer Framebuffer, uint Width, uint Height,
        uint FilteredWidth, uint FilteredHeight, ResourceSet ResourceSet);
}
