#nullable enable
using StudioCore.Editors.Common;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>The quad every billboard particle is drawn with: corners -0.5 to 0.5 in x and y.</summary>
public class DbgPrimFxQuad : DbgPrimSolid
{
    private static DbgPrimFxQuad? _shared;

    /// <summary>One quad for all billboard batches; its geometry uploads once.</summary>
    public static DbgPrimFxQuad Shared => _shared ??= new DbgPrimFxQuad();

    public override BoundingBox Bounds => new(new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0));

    private DbgPrimFxQuad()
    {
        AddQuad(new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0), Color.White);

        SceneRenderer.AddBackgroundUploadTask((d, cl) => UpdatePerFrameResources(d, cl, null));
    }
}

/// <summary>
/// One particle's two 96 byte instance slots, as FxBillboard.vert reads them. A: centre + z rotation; size, frame,
/// material (uint bits); colour; x/y rotation, thresholds (packUnorm2x16 bits), depth offset; layer 1 uv; layer 2
/// uv. B: layer 3 uv; layer colours; node right; node up.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct FxBillboardGpu
{
    public Vector4 A0, A1, A2, A3, A4, A5;
    public Vector4 B0, B1, B2, B3, B4, B5;

    public const int Size = 192;
}

/// <summary>A batch's constants (std140, 64 bytes), as fxMaterial in the billboard shaders.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct FxBillboardMaterial
{
    public uint Texture1, Texture2, Texture3, LayerCount;
    public uint Columns, TotalFrames, Flags, Orientation;
    public uint BlendMode, Pad0, Pad1, Pad2;
    public Vector4 Tuning; // x: brightness, y: depth fade distance

    public const uint FlagInterpolate = 1, FlagDepthBlend = 2, FlagPremultiply = 4, FlagMultiTexture = 8,
        FlagRotateBeforeScale = 16;
}

/// <summary>
/// A batch of billboard particles of one appearance (FX Editor Plan, phase V3): one instanced draw of
/// <see cref="DbgPrimFxQuad"/>, the particles sorted back to front, the batch itself sorted with the other
/// translucent draws by its centre. The FX Editor hands it new particles each frame with <see cref="SetParticles"/>.
/// It draws into the HDR scene target with depth testing and no depth writes, in a pipeline per blend mode.
/// </summary>
public class FxBillboardRenderableProxy : RenderableProxy
{
    private readonly MeshRenderables _renderablesSet;
    private readonly DbgPrimFxQuad _quad = DbgPrimFxQuad.Shared;

    private GPUBufferAllocator.GPUBufferHandle? _instances;
    private int _capacity;
    private GPUBufferAllocator.GPUBufferHandle? _materialBuffer;

    private int _renderable = -1;
    private bool _visible = true;
    private RenderFilter _drawfilter = RenderFilter.All;
    private DrawGroup _drawgroups = new();
    private BoundingBox _bounds = new(Vector3.Zero, Vector3.Zero);

    // Set on the main thread, taken on the render thread
    private readonly object _lock = new();
    private FxBillboardGpu[]? _pendingData;
    private int _pendingCount;
    private BoundingBox _pendingBounds;
    private bool _updateScheduled;
    private int _count;

    private FxBillboardMaterial _material;
    private bool _materialDirty = true;
    private int _blendMode = 2;

    public unsafe FxBillboardRenderableProxy(MeshRenderables renderables)
    {
        _renderablesSet = renderables;

        // Made now, so the particles can name the material before the renderable is built
        _materialBuffer = SceneRenderer.MaterialBufferAllocator.Allocate((uint)sizeof(FxBillboardMaterial), sizeof(FxBillboardMaterial));

        ScheduleRenderableConstruction();
        AutoRegister = true;
        _registered = true;
    }

    /// <summary>The material slot this batch's particles name in their instance data.</summary>
    public unsafe uint MaterialIndex => _materialBuffer == null ? 0 : _materialBuffer.AllocationStart / (uint)sizeof(FxBillboardMaterial);

    public bool IsConstructed => _renderable != -1;

    /// <summary>The batch's constants. A new blend mode needs another pipeline, so the renderable is rebuilt.</summary>
    public void SetMaterial(FxBillboardMaterial material)
    {
        lock (_lock)
        {
            bool newBlend = material.BlendMode != (uint)_blendMode;
            _material = material;
            _materialDirty = true;
            if (newBlend)
            {
                _blendMode = (int)material.BlendMode;
                if (_renderable != -1)
                    ScheduleRenderableConstruction();
            }
        }
        RequestUpdate();
    }

    /// <summary>
    /// The particles to draw, sorted back to front, with the material index already in each. The array is kept
    /// until the render thread has uploaded it, so pass a new one each time.
    /// </summary>
    public void SetParticles(FxBillboardGpu[] data, int count, BoundingBox bounds)
    {
        lock (_lock)
        {
            _pendingData = data;
            _pendingCount = count;
            _pendingBounds = bounds;
        }
        RequestUpdate();
    }

    private void RequestUpdate()
    {
        lock (_lock)
        {
            if (_updateScheduled)
                return;
            _updateScheduled = true;
        }
        ScheduleRenderableUpdate();
    }

    public override bool RenderSelectionOutline { get; set; }

    public override Matrix4x4 World { get => Matrix4x4.Identity; set { } }

    public override bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            if (_renderable != -1)
                _renderablesSet.cVisible[_renderable]._visible = value && _count > 0;
        }
    }

    public override RenderFilter DrawFilter
    {
        get => _drawfilter;
        set
        {
            _drawfilter = value;
            if (_renderable != -1)
                _renderablesSet.cSceneVis[_renderable]._renderFilter = value;
        }
    }

    public override DrawGroup DrawGroups
    {
        get => _drawgroups;
        set
        {
            _drawgroups = value;
            if (_renderable != -1)
                _renderablesSet.cSceneVis[_renderable]._drawGroup = value;
        }
    }

    public override BoundingBox GetBounds() => _bounds;
    public override BoundingBox GetLocalBounds() => _bounds;
    public override BoundingBox GetFramingBounds() => _bounds;

    public override void SetSelectable(ISelectable sel) { }

    public override void UnregisterAndRelease()
    {
        if (_registered)
            UnregisterWithScene();

        if (_instances != null)
            SceneRenderer.DisposeAfterFramesInFlight(_instances);
        _instances = null;
        _capacity = 0;

        if (_materialBuffer != null)
            SceneRenderer.DisposeAfterFramesInFlight(_materialBuffer);
        _materialBuffer = null;
    }

    // ---------------------------------------------------------------- pipelines

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<(int Blend, int System), Pipeline> _pipelines = new();

    /// <summary>
    /// The blend modes: Normal (2, 6) source-over; Add (4, 0, 7) adds colour × alpha; Subtract (5) takes it away;
    /// Multiply (3) multiplies the background (the shader moves transparent parts to white); Source (1) replaces it.
    /// </summary>
    private static BlendAttachmentDescription BlendFor(int mode) => mode switch
    {
        1 => BlendAttachmentDescription.OverrideBlend,
        3 => new BlendAttachmentDescription(true, VkBlendFactor.DstColor, VkBlendFactor.Zero, VkBlendOp.Add,
            VkBlendFactor.Zero, VkBlendFactor.One, VkBlendOp.Add),
        4 or 0 or 7 => new BlendAttachmentDescription(true, VkBlendFactor.SrcAlpha, VkBlendFactor.One, VkBlendOp.Add,
            VkBlendFactor.Zero, VkBlendFactor.One, VkBlendOp.Add),
        5 => new BlendAttachmentDescription(true, VkBlendFactor.SrcAlpha, VkBlendFactor.One, VkBlendOp.ReverseSubtract,
            VkBlendFactor.Zero, VkBlendFactor.One, VkBlendOp.Add),
        _ => new BlendAttachmentDescription(true, VkBlendFactor.SrcAlpha, VkBlendFactor.OneMinusSrcAlpha, VkBlendOp.Add,
            VkBlendFactor.Zero, VkBlendFactor.One, VkBlendOp.Add),
    };

    private Pipeline GetPipeline(GraphicsDevice gd, ResourceFactory factory)
    {
        return _pipelines.GetOrAdd((_blendMode, _renderablesSet.RenderableSystemIndex), _ =>
        {
            ResourceLayout sceneParamLayout = StaticResourceCache.GetResourceLayout(factory, StaticResourceCache.SceneParamLayoutDescription);
            ResourceLayout pickingResultLayout = StaticResourceCache.GetResourceLayout(factory, StaticResourceCache.PickingResultDescription);
            ResourceLayout perObjectLayout = PerObjectLayout(factory);

            var shaders = StaticResourceCache.GetShaders(gd, factory, "FxBillboard");

            GraphicsPipelineDescription description = new();
            description.BlendState = SceneTarget.BlendFor(_renderablesSet,
                new BlendStateDescription(RgbaFloat.Black, BlendFor(_blendMode)));

            // Tested against the scene, never written: particles don't hide each other or what's behind them
            description.DepthStencilState = DepthStencilStateDescription.DepthOnlyGreaterEqualRead;

            description.RasterizerState = new RasterizerStateDescription(
                VkCullModeFlags.None, VkPolygonMode.Fill, VkFrontFace.CounterClockwise, true, false);

            description.PrimitiveTopology = _quad.Topology;
            description.ShaderSet = new ShaderSetDescription(
                new[] { _quad.LayoutDescription },
                new[] { shaders.vs, shaders.fs },
                SceneTarget.Specialize(Array.Empty<SpecializationConstant>(), _renderablesSet));

            description.ResourceLayouts = new[]
            {
                sceneParamLayout, perObjectLayout, SceneRenderer.GlobalTexturePool.GetLayout(),
                SceneRenderer.GlobalCubeTexturePool.GetLayout(), SceneRenderer.MaterialBufferAllocator.GetLayout(),
                SamplerSet.SamplersLayout, pickingResultLayout, SceneRenderer.BoneBufferAllocator.GetLayout()
            };

            description.Outputs = SceneTarget.OutputsFor(gd, _renderablesSet);
            return StaticResourceCache.GetPipeline(factory, ref description);
        });
    }

    private static ResourceLayout PerObjectLayout(ResourceFactory factory) =>
        StaticResourceCache.GetResourceLayout(factory, new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("WorldBuffer", VkDescriptorType.StorageBuffer,
                VkShaderStageFlags.Vertex | VkShaderStageFlags.Fragment, VkDescriptorBindingFlags.None)));

    // ---------------------------------------------------------------- construction and upload

    public override unsafe void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
    {
        if (!_registered)
            return;

        if (_renderable != -1)
        {
            _renderablesSet.RemoveRenderable(_renderable);
            _renderable = -1;
        }

        if (_quad.GeometryBuffer == null ||
            _quad.GeometryBuffer.AllocStatus != VertexIndexBufferAllocator.VertexIndexBuffer.Status.Resident)
        {
            ScheduleRenderableConstruction();
            return;
        }

        ResourceFactory factory = gd.ResourceFactory;
        EnsureCapacity(64);
        if (_instances == null)
            return;

        var pipeline = GetPipeline(gd, factory);

        MeshDrawParametersComponent meshcomp = new();
        var geombuffer = _quad.GeometryBuffer;
        meshcomp._indirectArgs.FirstInstance = _instances.AllocationStart / FxBillboardGpu.Size;
        meshcomp._indirectArgs.VertexOffset = (int)(geombuffer.VAllocationStart / _quad.VertexSize);
        meshcomp._indirectArgs.InstanceCount = (uint)_count;
        meshcomp._indirectArgs.FirstIndex = geombuffer.IAllocationStart / 2u;
        meshcomp._indirectArgs.IndexCount = geombuffer.IAllocationSize / 2u;
        meshcomp._indexFormat = VkIndexType.Uint16;
        meshcomp._objectBuffer = SceneRenderer.UniformBufferAllocator;
        meshcomp._objectLayout = PerObjectLayout(factory);
        meshcomp._bufferIndex = geombuffer.BufferIndex;

        var bounds = _bounds;
        _renderable = _renderablesSet.CreateMesh(ref bounds, ref meshcomp);
        _renderablesSet.cRenderKeys[_renderable] = new RenderKey(RenderKey.BackToFrontStart);
        _renderablesSet.cBackToFront[_renderable] = true;

        // Particles can't be picked: picking frames draw them the same way, writing no ids
        _renderablesSet.cPipelines[_renderable] = pipeline;
        _renderablesSet.cSelectionPipelines[_renderable] = pipeline;
        _renderablesSet.cSelectables[_renderable] = null!;

        _renderablesSet.cVisible[_renderable]._visible = _visible && _count > 0;
        _renderablesSet.cSceneVis[_renderable]._renderFilter = _drawfilter;
        _renderablesSet.cSceneVis[_renderable]._drawGroup = _drawgroups;

        _materialDirty = true;
        Upload(gd, cl);
    }

    /// <summary>Makes room for <paramref name="count"/> particles, growing by doubling. The old range goes once no frame uses it.</summary>
    private void EnsureCapacity(int count)
    {
        if (count <= _capacity && _instances != null)
            return;

        int capacity = Math.Max(64, _capacity);
        while (capacity < count)
            capacity *= 2;

        // 192 byte aligned, so the draw's first instance can be the start / 192 (see FxBillboard.vert)
        var handle = SceneRenderer.UniformBufferAllocator.TryAllocate((uint)(capacity * FxBillboardGpu.Size), FxBillboardGpu.Size);
        if (handle == null)
            return;

        if (_instances != null)
            SceneRenderer.DisposeAfterFramesInFlight(_instances);
        _instances = handle;
        _capacity = capacity;

        if (_renderable != -1)
            _renderablesSet.cDrawParameters[_renderable]._indirectArgs.FirstInstance = _instances.AllocationStart / FxBillboardGpu.Size;
    }

    public override void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
    {
        if (_renderable == -1)
        {
            lock (_lock)
                _updateScheduled = false;
            return;
        }
        Upload(gd, cl);
    }

    private void Upload(GraphicsDevice gd, CommandList cl)
    {
        FxBillboardGpu[]? data;
        int count;
        BoundingBox bounds;
        FxBillboardMaterial material;
        bool materialDirty;

        lock (_lock)
        {
            _updateScheduled = false;
            data = _pendingData;
            count = _pendingCount;
            bounds = _pendingBounds;
            _pendingData = null;
            material = _material;
            materialDirty = _materialDirty;
            _materialDirty = false;
        }

        if (materialDirty && _materialBuffer != null)
            _materialBuffer.FillBuffer(gd, cl, ref material);

        if (data == null)
            return;

        EnsureCapacity(count);
        if (_instances == null)
            return;

        count = Math.Min(count, _capacity);
        if (count > 0)
        {
            // Only what's used: the staging copy is the whole range, but the draw reads just count particles
            if (data.Length > _capacity)
                Array.Resize(ref data, _capacity);
            _instances.FillBuffer(gd, cl, data);
        }

        _count = count;
        _bounds = bounds;
        _renderablesSet.cDrawParameters[_renderable]._indirectArgs.InstanceCount = (uint)count;
        _renderablesSet.cBounds[_renderable] = bounds;
        _renderablesSet.cVisible[_renderable]._visible = _visible && count > 0;
    }

    public override void DestroyRenderables()
    {
        if (_renderable != -1)
        {
            _renderablesSet.RemoveRenderable(_renderable);
            _renderable = -1;
        }
    }
}
