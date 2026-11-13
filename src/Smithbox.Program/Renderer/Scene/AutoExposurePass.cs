using StudioCore.Application;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AutoExposureParam
{
    /// <summary>x: middle gray, y: adaption min, z: adaption max, w: center weight</summary>
    public Vector4 Exposure;

    /// <summary>x: seconds since the last frame, y: light adapt speed, z: dark adapt speed, w: GPARAM exposure</summary>
    public Vector4 Adapt;

    /// <summary>x: 1 to jump straight to the target</summary>
    public Vector4 Control;
}

/// <summary>The state AutoExposure.comp keeps between frames, std430</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AutoExposureState
{
    /// <summary>The adjustment the tone map applies on top of the GPARAM exposure, in EV</summary>
    public float Ev;

    public float TargetEv;

    /// <summary>The scene's measured mean log2 luminance, in game units</summary>
    public float MeanLog;

    public float Valid;
}

/// <summary>
///     The GPARAM Tone Map group's auto exposure (Enable AutoExposure, Middle Gray, Adaption Min/Max, the adapt
///     speeds and Center Weight), which Elden Ring's map files all switch on. It measures the scene and moves an EV
///     adjustment towards the target from frame to frame, in <see cref="StateBuffer" />, which
///     <see cref="ToneMapPass" /> and <see cref="BloomPass" /> read on the GPU. It doesn't change the image itself.
///     See AutoExposure.comp for the formula, which is a reading of the values, not the game's code.
/// </summary>
public class AutoExposurePass : PostProcessPass
{
    private static ResourceLayout Layout;
    private static ComputePass Compute;

    private DeviceBuffer ParamBuffer;
    private DeviceBuffer ReadbackBuffer;
    private ResourceSet ResourceSet;

    public override void Dispose()
    {
        SceneRenderer.DisposeAllAfterFramesInFlight(ResourceSet, ParamBuffer);

        // A readback of the state may still be queued, so these are kept longer
        if (StateBuffer != null)
            SceneRenderer.DisposeAfterFramesInFlight(StateBuffer, SceneRenderer.ReadbackReleaseFrames);

        if (ReadbackBuffer != null)
            SceneRenderer.DisposeAfterFramesInFlight(ReadbackBuffer, SceneRenderer.ReadbackReleaseFrames);

        ResourceSet = null;
        ParamBuffer = null;
        StateBuffer = null;
        ReadbackBuffer = null;
        BoundSource = null;
    }
    private Texture BoundSource;
    private AutoExposureParam Params;

    private readonly Stopwatch Clock = new();
    private bool WasActive;
    private bool ReadbackPending;

    public override string Name => "Auto Exposure";

    public override bool WritesImage => false;

    /// <summary>The state the shader keeps, valid once <see cref="Applied" /> has been true</summary>
    public DeviceBuffer StateBuffer { get; private set; }

    /// <summary>Whether the pass ran this frame, so the tone map should apply the adjustment</summary>
    public bool Applied { get; private set; }

    /// <summary>The state as last read back to the CPU, a few frames old, for the menu</summary>
    public AutoExposureState LastState { get; private set; }

    private static DeviceBuffer NeutralState;

    /// <summary>A state of 0 EV, bound by the passes that read the state when auto exposure didn't run</summary>
    public static unsafe DeviceBuffer GetNeutralState(GraphicsDevice gd)
    {
        if (NeutralState != null)
        {
            return NeutralState;
        }

        NeutralState = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)sizeof(AutoExposureState),
            VkBufferUsageFlags.StorageBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0,
            (uint)sizeof(AutoExposureState)));
        NeutralState.Name = "Auto Exposure Neutral State";

        var neutral = new AutoExposureState();
        gd.UpdateBuffer(NeutralState, 0, ref neutral, (uint)sizeof(AutoExposureState));

        return NeutralState;
    }

    public override void BeginFrame()
    {
        Applied = false;
    }

    public override bool IsActive(GparamLighting lighting)
    {
        var active = CFG.Current.Viewport_Post_Auto_Exposure && lighting is { AutoExposure: true };

        if (!active)
        {
            WasActive = false;
        }

        return active;
    }

    public override unsafe void Run(GraphicsDevice gd, CommandList cl, PostProcessFrame frame)
    {
        EnsureResources(gd, frame.Source);

        var lighting = frame.Lighting;
        var settings = frame.Effects.AutoExposure;

        var seconds = Clock.IsRunning ? (float)Clock.Elapsed.TotalSeconds : 0.0f;
        Clock.Restart();

        Params.Exposure = new Vector4(lighting.MiddleGray, settings.AdaptionMin, settings.AdaptionMax, settings.CenterWeight);
        Params.Adapt = new Vector4(Math.Clamp(seconds, 0.0f, 0.5f), settings.LightAdaptSpeed, settings.DarkAdaptSpeed,
            lighting.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale);
        Params.Control = new Vector4(WasActive ? 0.0f : 1.0f, 0.0f, 0.0f, 0.0f);
        // Copied through a staging buffer, which also ends the scene's render pass, so the barriers below are
        // outside one
        cl.UpdateBuffer(ParamBuffer, 0, ref Params, (uint)sizeof(AutoExposureParam));

        // The parameters' copy has to land, and the last frame's tone map may still be reading the state
        cl.Barrier(VkPipelineStageFlags2.Transfer | VkPipelineStageFlags2.FragmentShader,
            VkAccessFlags2.TransferWrite | VkAccessFlags2.ShaderRead,
            VkPipelineStageFlags2.ComputeShader,
            VkAccessFlags2.UniformRead | VkAccessFlags2.ShaderRead | VkAccessFlags2.ShaderWrite);

        Compute.Dispatch(gd, cl, new[] { ResourceSet }, Compute.GroupSize.X, Compute.GroupSize.Y);

        cl.Barrier(VkPipelineStageFlags2.ComputeShader, VkAccessFlags2.ShaderWrite,
            VkPipelineStageFlags2.FragmentShader | VkPipelineStageFlags2.Transfer,
            VkAccessFlags2.ShaderRead | VkAccessFlags2.TransferRead);

        WasActive = true;
        Applied = true;

        RequestReadback();
    }

    private void RequestReadback()
    {
        if (ReadbackPending)
        {
            return;
        }

        ReadbackPending = true;

        SceneRenderer.AddAsyncReadback(ReadbackBuffer, StateBuffer, d =>
        {
            unsafe
            {
                MappedResourceView<AutoExposureState> result = d.Map<AutoExposureState>(ReadbackBuffer, MapMode.Read);
                LastState = new Span<AutoExposureState>(result.MappedResource.Data.ToPointer(), 1)[0];
            }

            ReadbackPending = false;
        });
    }

    private unsafe void EnsureResources(GraphicsDevice gd, Texture source)
    {
        ResourceFactory factory = gd.ResourceFactory;

        Layout ??= factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("AutoExposureParams", VkDescriptorType.UniformBuffer,
                VkShaderStageFlags.Compute),
            new ResourceLayoutElementDescription("SceneColor", VkDescriptorType.SampledImage,
                VkShaderStageFlags.Compute),
            new ResourceLayoutElementDescription("SceneSampler", VkDescriptorType.Sampler,
                VkShaderStageFlags.Compute),
            new ResourceLayoutElementDescription("ExposureState", VkDescriptorType.StorageBuffer,
                VkShaderStageFlags.Compute)));

        Compute ??= new ComputePass("AutoExposure", new[] { Layout }, 16, 16);

        ParamBuffer ??= factory.CreateBuffer(new BufferDescription((uint)sizeof(AutoExposureParam),
            VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));

        if (StateBuffer == null)
        {
            StateBuffer = factory.CreateBuffer(new BufferDescription((uint)sizeof(AutoExposureState),
                VkBufferUsageFlags.StorageBuffer | VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto, 0, (uint)sizeof(AutoExposureState)));
            StateBuffer.Name = "Auto Exposure State";

            var initial = new AutoExposureState();
            gd.UpdateBuffer(StateBuffer, 0, ref initial, (uint)sizeof(AutoExposureState));

            ReadbackBuffer = factory.CreateBuffer(new BufferDescription((uint)sizeof(AutoExposureState),
                VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, VmaAllocationCreateFlags.Mapped));
        }

        if (ReferenceEquals(BoundSource, source) && ResourceSet != null)
        {
            return;
        }

        if (ResourceSet != null)
        {
            SceneRenderer.DisposeAfterFramesInFlight(ResourceSet);
        }

        ResourceSet = factory.CreateResourceSet(new ResourceSetDescription(Layout, ParamBuffer, source,
            gd.LinearSampler, StateBuffer));
        BoundSource = source;
    }
}
