#nullable enable
using StudioCore.Editors.Common;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
/// One Model particle's 96 byte instance slot, as FxModel.vert reads it: the world matrix's rows (with the
/// submesh's own transform in it), the colour, and the uv offset, sprite frame and material index (uint bits).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct FxModelGpu
{
    public Vector4 Row0, Row1, Row2, Row3;
    public Vector4 Color;
    public Vector4 Uv;

    public const int Size = 96;

    public static FxModelGpu From(in Matrix4x4 world, Vector4 color, Vector2 uvOffset, float frame, uint material) => new()
    {
        Row0 = new Vector4(world.M11, world.M12, world.M13, world.M14),
        Row1 = new Vector4(world.M21, world.M22, world.M23, world.M24),
        Row2 = new Vector4(world.M31, world.M32, world.M33, world.M34),
        Row3 = new Vector4(world.M41, world.M42, world.M43, world.M44),
        Color = color,
        Uv = new Vector4(uvOffset.X, uvOffset.Y, frame, BitConverter.UInt32BitsToSingle(material)),
    };
}

/// <summary>A Model batch's constants (std140, 64 bytes), as fxModelMaterial in FxModel.frag.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct FxModelMaterial
{
    public uint Albedo, Pad0, Pad1, Pad2;
    public uint Columns, TotalFrames, Flags, Pad3;
    public uint BlendMode, Pad4, Pad5, Pad6;
    public Vector4 Tuning; // x: brightness

    public const uint FlagLit = 1;
}

/// <summary>
/// What an <see cref="FxModelRenderableProxy"/> draws of a FLVER submesh: its geometry, its first face set (as the
/// FLVER renderer draws), its vertex layout, culling and transform, and whether its material is opaque.
/// </summary>
public sealed class FxModelSubmesh
{
    public VertexIndexBufferAllocator.VertexIndexBufferHandle GeomBuffer = null!;
    public VertexLayoutDescription Layout;
    public MeshLayoutType LayoutType;
    public uint VertexSize;
    public int IndexOffset, IndexCount;
    public bool Is32Bit, IsStrip, CullBack, Opaque;
    public Matrix4x4 LocalTransform = Matrix4x4.Identity;
    public BoundingBox Bounds;
}

/// <summary>
/// The Model particles of one appearance on one FLVER submesh (FX Editor Plan §18): one instanced draw of the
/// submesh's own geometry with FxModel.vert/frag, coloured by the effect. As the billboard batches, it draws into
/// the HDR scene target with a pipeline per blend mode (and layout, topology and culling). Translucent batches sort
/// back to front and don't write depth; Normal blend on an opaque material writes it, so a solid model hides itself.
/// </summary>
public class FxModelRenderableProxy : RenderableProxy
{
    private readonly MeshRenderables _renderablesSet;
    private readonly FxModelSubmesh _submesh;

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
    private FxModelGpu[]? _pendingData;
    private int _pendingCount;
    private BoundingBox _pendingBounds;
    private bool _updateScheduled;
    private int _count;

    private FxModelMaterial _material;
    private bool _materialDirty = true;
    private int _blendMode = 2;

    public unsafe FxModelRenderableProxy(MeshRenderables renderables, FxModelSubmesh submesh)
    {
        _renderablesSet = renderables;
        _submesh = submesh;
        _materialBuffer = SceneRenderer.MaterialBufferAllocator.Allocate((uint)sizeof(FxModelMaterial), sizeof(FxModelMaterial));

        ScheduleRenderableConstruction();
        AutoRegister = true;
        _registered = true;
    }

    public FxModelSubmesh Submesh => _submesh;

    /// <summary>The material slot this batch's particles name in their instance data.</summary>
    public unsafe uint MaterialIndex => _materialBuffer == null ? 0 : _materialBuffer.AllocationStart / (uint)sizeof(FxModelMaterial);

    /// <summary>The batch's constants. A new blend mode needs another pipeline, so the renderable is rebuilt.</summary>
    public void SetMaterial(FxModelMaterial material)
    {
        lock (_lock)
        {
            bool newBlend = material.BlendMode != (uint)_blendMode;
            bool changed = newBlend || !material.Equals(_material);
            _material = material;
            if (!changed)
                return;
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
    public void SetParticles(FxModelGpu[] data, int count, BoundingBox bounds)
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

    // Normal blend (2, 6) on an opaque material is drawn as a solid model
    private bool Solid => _blendMode is 2 or 6 && _submesh.Opaque;

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

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<(int Blend, MeshLayoutType Layout, bool Strip, bool Cull, bool Solid, int System), Pipeline> _pipelines = new();

    /// <summary>The blend modes, as the billboards' (FxBillboardRenderableProxy.BlendFor).</summary>
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
        var key = (_blendMode, _submesh.LayoutType, _submesh.IsStrip, _submesh.CullBack, Solid, _renderablesSet.RenderableSystemIndex);
        var layout = _submesh.Layout;
        return _pipelines.GetOrAdd(key, _ =>
        {
            ResourceLayout sceneParamLayout = StaticResourceCache.GetResourceLayout(factory, StaticResourceCache.SceneParamLayoutDescription);
            ResourceLayout pickingResultLayout = StaticResourceCache.GetResourceLayout(factory, StaticResourceCache.PickingResultDescription);
            ResourceLayout perObjectLayout = PerObjectLayout(factory);

            var shaders = StaticResourceCache.GetShaders(gd, factory, "FxModel");

            GraphicsPipelineDescription description = new();
            description.BlendState = SceneTarget.BlendFor(_renderablesSet,
                new BlendStateDescription(RgbaFloat.Black, BlendFor(key.Item1)));

            description.DepthStencilState = key.Item5
                ? DepthStencilStateDescription.DepthOnlyGreaterEqual
                : DepthStencilStateDescription.DepthOnlyGreaterEqualRead;

            description.RasterizerState = new RasterizerStateDescription(
                key.Item4 ? VkCullModeFlags.Back : VkCullModeFlags.None, VkPolygonMode.Fill,
                VkFrontFace.CounterClockwise, true, false);

            description.PrimitiveTopology = key.Item3 ? VkPrimitiveTopology.TriangleStrip : VkPrimitiveTopology.TriangleList;
            description.ShaderSet = new ShaderSetDescription(
                new[] { layout },
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

        var geombuffer = _submesh.GeomBuffer;
        if (geombuffer == null || geombuffer.AllocStatus != VertexIndexBufferAllocator.VertexIndexBuffer.Status.Resident)
        {
            ScheduleRenderableConstruction();
            return;
        }

        ResourceFactory factory = gd.ResourceFactory;
        EnsureCapacity(16);
        if (_instances == null)
            return;

        var pipeline = GetPipeline(gd, factory);

        MeshDrawParametersComponent meshcomp = new();
        meshcomp._indirectArgs.FirstInstance = _instances.AllocationStart / FxModelGpu.Size;
        meshcomp._indirectArgs.VertexOffset = (int)(geombuffer.VAllocationStart / _submesh.VertexSize);
        meshcomp._indirectArgs.InstanceCount = (uint)_count;
        meshcomp._indirectArgs.FirstIndex = geombuffer.IAllocationStart / (_submesh.Is32Bit ? 4u : 2u) + (uint)_submesh.IndexOffset;
        meshcomp._indirectArgs.IndexCount = (uint)_submesh.IndexCount;
        meshcomp._indexFormat = _submesh.Is32Bit ? VkIndexType.Uint32 : VkIndexType.Uint16;
        meshcomp._objectBuffer = SceneRenderer.UniformBufferAllocator;
        meshcomp._objectLayout = PerObjectLayout(factory);
        meshcomp._bufferIndex = geombuffer.BufferIndex;

        var bounds = _bounds;
        _renderable = _renderablesSet.CreateMesh(ref bounds, ref meshcomp);
        _renderablesSet.cRenderKeys[_renderable] = new RenderKey(Solid ? 0ul : RenderKey.BackToFrontStart);
        _renderablesSet.cBackToFront[_renderable] = !Solid;

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

        int capacity = Math.Max(16, _capacity);
        while (capacity < count)
            capacity *= 2;

        // 96 byte aligned, so the draw's first instance can be the start / 96 (see FxModel.vert)
        var handle = SceneRenderer.UniformBufferAllocator.TryAllocate((uint)(capacity * FxModelGpu.Size), FxModelGpu.Size);
        if (handle == null)
            return;

        if (_instances != null)
            SceneRenderer.DisposeAfterFramesInFlight(_instances);
        _instances = handle;
        _capacity = capacity;

        if (_renderable != -1)
            _renderablesSet.cDrawParameters[_renderable]._indirectArgs.FirstInstance = _instances.AllocationStart / FxModelGpu.Size;
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
        FxModelGpu[]? data;
        int count;
        BoundingBox bounds;
        FxModelMaterial material;
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
