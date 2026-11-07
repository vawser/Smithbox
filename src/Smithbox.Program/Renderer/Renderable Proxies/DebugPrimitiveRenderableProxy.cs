#nullable enable
using StudioCore.Editors.Common;
using System;
using System.Drawing;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

public class DebugPrimitiveRenderableProxy : RenderableProxy
{
    private static readonly Color FocusGhostColor = Color.FromArgb(90, 128, 128, 128);

    private IDbgPrim? _debugPrimitive;

    private readonly MeshRenderables _renderablesSet;
    private Color _baseColor = Color.Gray;
    private float _gridCells;
    private int _gridMajorInterval;
    private float _gridFadeDistance;
    private Vector2 _gridOffset;
    private float _gridLineWidth = 1f;
    private bool _gridAxisLines;

    private RenderFilter _drawfilter = RenderFilter.All;

    private DrawGroup _drawgroups = new();

    public bool _hasColorVariance;
    public Color _highlightedColor = Color.Gray;
    public Color _initialColor = Color.Empty;
    protected GPUBufferAllocator.GPUBufferHandle? _materialBuffer;

    private bool _overdraw;
    private bool _pickable = true;
    private bool _translucent;
    private bool _occludedOnly;
    protected Pipeline? _pickingPipeline;

    protected Pipeline? _pipeline;

    private int _renderable = -1;

    private bool _renderOutline;
    private bool _renderFocusGhost;
    private WeakReference<ISelectable>? _selectable;
    protected Shader[]? _shaders;

    private bool _visible = true;

    private Matrix4x4 _world = Matrix4x4.Identity;
    protected GPUBufferAllocator.GPUBufferHandle? _worldBuffer;

    public DebugPrimitiveRenderableProxy(MeshRenderables renderables, IDbgPrim? prim, bool autoregister = true)
    {
        _renderablesSet = renderables;
        _debugPrimitive = prim;
        if (autoregister)
        {
            ScheduleRenderableConstruction();
            AutoRegister = true;
            _registered = true;
        }
    }

    public DebugPrimitiveRenderableProxy(DebugPrimitiveRenderableProxy clone) : this(clone._renderablesSet,
        clone._debugPrimitive)
    {
        _drawfilter = clone.DrawFilter;
        _initialColor = clone._initialColor;
        _baseColor = clone.BaseColor;
        _highlightedColor = clone._highlightedColor;
        _pickable = clone._pickable;
        _translucent = clone._translucent;
        _occludedOnly = clone._occludedOnly;
        if (clone._hasColorVariance)
        {
            ColorHelper.ApplyColorVariance(this);
        }
    }

    public Color BaseColor
    {
        get => _baseColor;
        set
        {
            _baseColor = value;
            if (_initialColor == Color.Empty)
            {
                _initialColor = value;
            }

            ScheduleRenderableUpdate();
        }
    }

    public Color HighlightedColor
    {
        get => _highlightedColor;
        set
        {
            _highlightedColor = value;
            ScheduleRenderableUpdate();
        }
    }

    /// <summary>
    /// Squares either side of the center of a <see cref="DbgPrimGridPlane" />.
    /// </summary>
    public float GridCells
    {
        get => _gridCells;
        set
        {
            if (_gridCells == value)
            {
                return;
            }

            _gridCells = value;
            ScheduleRenderableUpdate();
        }
    }

    /// <summary>
    /// Squares between the major lines of a <see cref="DbgPrimGridPlane" />, 0 or 1 for none.
    /// </summary>
    public int GridMajorInterval
    {
        get => _gridMajorInterval;
        set
        {
            // Packed into 16 bits of the grid flags
            value = Math.Clamp(value, 0, ushort.MaxValue);

            if (_gridMajorInterval == value)
            {
                return;
            }

            _gridMajorInterval = value;
            ScheduleRenderableUpdate();
        }
    }

    /// <summary>
    /// Where the center of a <see cref="DbgPrimGridPlane" />'s quad is, in whole squares from the grid's origin
    /// along its X and Z. Set when the grid follows the camera.
    /// </summary>
    public Vector2 GridOffset
    {
        get => _gridOffset;
        set
        {
            if (_gridOffset == value)
            {
                return;
            }

            _gridOffset = value;
            ScheduleRenderableUpdate();
        }
    }

    /// <summary>
    /// Scales the line widths of a <see cref="DbgPrimGridPlane" />, 1 for the default widths.
    /// </summary>
    public float GridLineWidth
    {
        get => _gridLineWidth;
        set
        {
            if (_gridLineWidth == value)
            {
                return;
            }

            _gridLineWidth = value;
            ScheduleRenderableUpdate();
        }
    }

    /// <summary>
    /// Distance from the camera where a <see cref="DbgPrimGridPlane" /> has faded out, 0 for no fade.
    /// </summary>
    public float GridFadeDistance
    {
        get => _gridFadeDistance;
        set
        {
            if (_gridFadeDistance == value)
            {
                return;
            }

            _gridFadeDistance = value;
            ScheduleRenderableUpdate();
        }
    }

    /// <summary>
    /// Draws the axis lines through the center of a <see cref="DbgPrimGridPlane" />.
    /// </summary>
    public bool GridAxisLines
    {
        get => _gridAxisLines;
        set
        {
            if (_gridAxisLines == value)
            {
                return;
            }

            _gridAxisLines = value;
            ScheduleRenderableUpdate();
        }
    }

    private uint GridFlags => (_gridAxisLines ? 1u : 0u) | ((uint)_gridMajorInterval << 16);

    public override Matrix4x4 World
    {
        get => _world;
        set
        {
            _world = value;
            ScheduleRenderableUpdate();
        }
    }

    /// <summary>
    ///     True once the primitive's geometry is resident and its renderable has been built, so that it will be drawn.
    /// </summary>
    public bool IsRenderableConstructed => _renderable != -1;

    public override bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            if (_renderable != -1)
            {
                _renderablesSet.cVisible[_renderable]._visible = value;
            }
        }
    }

    public override RenderFilter DrawFilter
    {
        get => _drawfilter;
        set
        {
            _drawfilter = value;
            if (_renderable != -1)
            {
                _renderablesSet.cSceneVis[_renderable]._renderFilter = value;
            }
        }
    }

    public override DrawGroup DrawGroups
    {
        get => _drawgroups;
        set
        {
            _drawgroups = value;
            if (_renderable != -1)
            {
                _renderablesSet.cSceneVis[_renderable]._drawGroup = value;
            }
        }
    }

    public override bool RenderSelectionOutline
    {
        get => _renderOutline;
        set
        {
            var old = _renderOutline;
            _renderOutline = value;
            if (_registered && old != _renderOutline)
            {
                ScheduleRenderableUpdate();
            }
        }
    }

    public override bool RenderFocusGhost
    {
        get => _renderFocusGhost;
        set
        {
            var old = _renderFocusGhost;
            _renderFocusGhost = value;
            if (_registered && old != _renderFocusGhost)
            {
                ScheduleRenderableUpdate();
            }
        }
    }

    /// <summary>
    /// Primitives are already alpha blended, so all they need for focus is another color.
    /// </summary>
    private Color CurrentColor => _renderOutline ? HighlightedColor : _renderFocusGhost ? FocusGhostColor : BaseColor;

    public bool RenderOverlay
    {
        get => _overdraw;
        set
        {
            var old = _overdraw;
            _overdraw = true;
            if (_registered && _overdraw != old)
            {
                ScheduleRenderableConstruction();
            }
        }
    }

    /// <summary>
    /// Translucent primitives don't write depth, so they can't hide what is behind them, and are drawn
    /// back to front.
    /// </summary>
    public bool RenderTranslucent
    {
        get => _translucent;
        set
        {
            var old = _translucent;
            _translucent = value;
            if (_registered && _translucent != old)
            {
                ScheduleRenderableConstruction();
            }
        }
    }

    /// <summary>
    /// Draws only the parts of this primitive that are hidden behind other surfaces, and doesn't write depth.
    /// Paired with a normal copy of the primitive, this shows what is behind geometry, usually more faintly.
    /// </summary>
    public bool RenderOccludedOnly
    {
        get => _occludedOnly;
        set
        {
            var old = _occludedOnly;
            _occludedOnly = value;
            if (_registered && _occludedOnly != old)
            {
                ScheduleRenderableConstruction();
            }
        }
    }

    /// <summary>
    /// Whether this primitive can be clicked in the viewport. If not, clicks reach whatever is behind it.
    /// </summary>
    public bool Pickable
    {
        get => _pickable;
        set
        {
            var old = _pickable;
            _pickable = value;
            if (_registered && _pickable != old)
            {
                ScheduleRenderableConstruction();
            }
        }
    }

    public override BoundingBox GetBounds()
    {
        if (_debugPrimitive == null)
            return new BoundingBox();

        return BoundingBox.Transform(_debugPrimitive.Bounds, _world);
    }

    public override BoundingBox GetLocalBounds()
    {
        if (_debugPrimitive == null)
            return new BoundingBox();

        return _debugPrimitive.Bounds;
    }

    public override BoundingBox GetFramingBounds()
    {
        if (_debugPrimitive == null)
            return new BoundingBox();

        return _debugPrimitive.FramingBounds;
    }

    public override void UnregisterAndRelease()
    {
        if (_registered)
        {
            UnregisterWithScene();
        }

        if (_worldBuffer != null)
        {
            _worldBuffer.Dispose();
            _worldBuffer = null;
        }

        if (_materialBuffer != null)
        {
            _materialBuffer.Dispose();
            _materialBuffer = null;
        }
    }

    public override unsafe void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
    {
        // If we were unregistered before construction time, don't construct now
        if (!_registered)
        {
            return;
        }

        if (_renderable != -1)
        {
            _renderablesSet.RemoveRenderable(_renderable);
            _renderable = -1;
        }

        if (_debugPrimitive == null)
            return;


        if (_debugPrimitive.GeometryBuffer == null)
            return;

        if (_debugPrimitive.GeometryBuffer.AllocStatus !=
            VertexIndexBufferAllocator.VertexIndexBuffer.Status.Resident)
        {
            ScheduleRenderableConstruction();
            return;
        }

        ResourceFactory? factory = gd.ResourceFactory;
        _worldBuffer ??= SceneRenderer.TryAllocateInstance();

        if (_worldBuffer == null)
        {
            return;
        }

        if (_materialBuffer == null)
        {
            _materialBuffer =
                SceneRenderer.MaterialBufferAllocator.Allocate((uint)sizeof(DbgMaterial), sizeof(DbgMaterial));
        }

        // Construct pipeline
        ResourceLayout projViewCombinedLayout = StaticResourceCache.GetResourceLayout(
            gd.ResourceFactory,
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ViewProjection", VkDescriptorType.UniformBuffer,
                    VkShaderStageFlags.Vertex)));

        ResourceLayout worldLayout = StaticResourceCache.GetResourceLayout(
            gd.ResourceFactory,
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "World",
                    VkDescriptorType.UniformBufferDynamic,
                    VkShaderStageFlags.Vertex,
                    VkDescriptorBindingFlags.None)));


        VertexLayoutDescription[] mainVertexLayouts = { _debugPrimitive.LayoutDescription };

        Tuple<Shader, Shader> res = StaticResourceCache
            .GetShaders(gd, gd.ResourceFactory, _debugPrimitive.ShaderName).ToTuple();
        _shaders = new[] { res.Item1, res.Item2 };

        ResourceLayout projViewLayout = StaticResourceCache.GetResourceLayout(
            gd.ResourceFactory,
            StaticResourceCache.SceneParamLayoutDescription);

        ResourceLayout pickingResultLayout = StaticResourceCache.GetResourceLayout(
            gd.ResourceFactory,
            StaticResourceCache.PickingResultDescription);

        ResourceLayout mainPerObjectLayout = StaticResourceCache.GetResourceLayout(
            gd.ResourceFactory,
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "WorldBuffer",
                    VkDescriptorType.StorageBuffer,
                    VkShaderStageFlags.Vertex | VkShaderStageFlags.Fragment,
                    VkDescriptorBindingFlags.None)));

        ResourceLayout texLayout = StaticResourceCache.GetResourceLayout(
            gd.ResourceFactory,
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "globalTextures",
                    VkDescriptorType.SampledImage,
                    VkShaderStageFlags.Vertex | VkShaderStageFlags.Fragment,
                    VkDescriptorBindingFlags.None)));

        // Build default pipeline
        GraphicsPipelineDescription pipelineDescription = new();

        pipelineDescription.BlendState = SceneTarget.BlendFor(_renderablesSet, BlendStateDescription.SingleAlphaBlend);

        // The depth is reversed, so a hidden fragment is one with less depth than what is already drawn
        pipelineDescription.DepthStencilState = _occludedOnly
            ? new DepthStencilStateDescription(true, false, VkCompareOp.Less)
            : _translucent
                ? DepthStencilStateDescription.DepthOnlyGreaterEqualRead
                : DepthStencilStateDescription.DepthOnlyGreaterEqual;

        pipelineDescription.RasterizerState = new RasterizerStateDescription(
            _debugPrimitive.CullMode,
            _debugPrimitive.FillMode,
            _debugPrimitive.FrontFace,
            true,
            false)
        {
            DepthBiasConstant = _debugPrimitive.DepthBiasConstant,
            DepthBiasSlope = _debugPrimitive.DepthBiasSlope
        };

        pipelineDescription.PrimitiveTopology = _debugPrimitive.Topology;

        // Primitives in the scene go in the HDR target, where the shaders output linear light; overlays don't
        SpecializationConstant[] specializationConstants =
            SceneTarget.Specialize(_debugPrimitive.SpecializationConstants, _renderablesSet);

        pipelineDescription.ShaderSet = new ShaderSetDescription(
            mainVertexLayouts,
            _shaders, specializationConstants);

        pipelineDescription.ResourceLayouts = new[]
        {
            projViewLayout, mainPerObjectLayout, SceneRenderer.GlobalTexturePool.GetLayout(),
            SceneRenderer.GlobalCubeTexturePool.GetLayout(), SceneRenderer.MaterialBufferAllocator.GetLayout(),
            SamplerSet.SamplersLayout, pickingResultLayout, SceneRenderer.BoneBufferAllocator.GetLayout()
        };

        pipelineDescription.Outputs = SceneTarget.OutputsFor(gd, _renderablesSet);
        _pipeline = StaticResourceCache.GetPipeline(factory, ref pipelineDescription);

        // Build picking pipeline. Unpickable primitives keep drawing during a picking
        // frame, but with the default pipeline so they never write to the picking buffer.
        _pickingPipeline = _pipeline;
        if (_pickable)
        {
            var pickingSpecializationConstants =
                new SpecializationConstant[specializationConstants.Length + 1];
            Array.Copy(specializationConstants, pickingSpecializationConstants,
                specializationConstants.Length);
            pickingSpecializationConstants[pickingSpecializationConstants.Length - 1] =
                new SpecializationConstant(99, true);
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                mainVertexLayouts,
                _shaders, pickingSpecializationConstants);
            _pickingPipeline = StaticResourceCache.GetPipeline(factory, ref pipelineDescription);
        }

        // Create draw call arguments
        MeshDrawParametersComponent meshcomp = new();
        VertexIndexBufferAllocator.VertexIndexBufferHandle? geombuffer = _debugPrimitive.GeometryBuffer;
        var indexStart = geombuffer.IAllocationStart / (_debugPrimitive.Is32Bit ? 4u : 2u) +
                         (uint)_debugPrimitive.IndexOffset;
        meshcomp._indirectArgs.FirstInstance = _worldBuffer.AllocationStart / (uint)sizeof(InstanceData);
        meshcomp._indirectArgs.VertexOffset = (int)(geombuffer.VAllocationStart / _debugPrimitive.VertexSize);
        meshcomp._indirectArgs.InstanceCount = 1;
        meshcomp._indirectArgs.FirstIndex = indexStart;
        meshcomp._indirectArgs.IndexCount = geombuffer.IAllocationSize / (_debugPrimitive.Is32Bit ? 4u : 2u);

        // Rest of draw parameters
        meshcomp._indexFormat = _debugPrimitive.Is32Bit ? VkIndexType.Uint32 : VkIndexType.Uint16;
        meshcomp._objectBuffer = SceneRenderer.UniformBufferAllocator;
        meshcomp._objectLayout = mainPerObjectLayout;
        meshcomp._bufferIndex = geombuffer.BufferIndex;

        // Instantiate renderable
        BoundingBox bounds = BoundingBox.Transform(_debugPrimitive.Bounds, _world);
        _renderable = _renderablesSet.CreateMesh(ref bounds, ref meshcomp);
        _renderablesSet.cRenderKeys[_renderable] = GetRenderKey(0.0f);
        _renderablesSet.cBackToFront[_renderable] = _translucent;

        // Pipelines
        _renderablesSet.cPipelines[_renderable] = _pipeline;
        _renderablesSet.cSelectionPipelines[_renderable] = _pickingPipeline;

        // Update instance data
        InstanceData dat = new();
        dat.WorldMatrix = _world;
        dat.MaterialID = _materialBuffer.AllocationStart / (uint)sizeof(DbgMaterial);
        dat.EntityID = GetPackedEntityID(_renderablesSet.RenderableSystemIndex, _renderable);
        _worldBuffer.FillBuffer(gd, cl, ref dat);

        // Update material data
        DbgMaterial colmat = new();
        colmat.Color = CurrentColor;
        colmat.GridCells = _gridCells;
        colmat.GridFadeDistance = _gridFadeDistance;
        colmat.GridOffsetX = _gridOffset.X;
        colmat.GridOffsetZ = _gridOffset.Y;
        colmat.GridLineWidth = _gridLineWidth;
        colmat.GridFlags = GridFlags;
        _materialBuffer.FillBuffer(gd, cl, ref colmat);

        // Selectable
        _renderablesSet.cSelectables[_renderable] = _selectable;

        // Visible
        if (_renderable != -1)
        {
            _renderablesSet.cVisible[_renderable]._visible = _visible;
            _renderablesSet.cSceneVis[_renderable]._renderFilter = _drawfilter;
            _renderablesSet.cSceneVis[_renderable]._drawGroup = _drawgroups;
        }
    }

    public override unsafe void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
    {
        if (_materialBuffer == null)
        {
            _materialBuffer =
                SceneRenderer.MaterialBufferAllocator.Allocate((uint)sizeof(DbgMaterial), sizeof(DbgMaterial));
        }

        InstanceData dat = new();
        dat.WorldMatrix = _world;
        dat.MaterialID = _materialBuffer.AllocationStart / (uint)sizeof(DbgMaterial);
        dat.EntityID = GetPackedEntityID(_renderablesSet.RenderableSystemIndex, _renderable);
        _worldBuffer ??= SceneRenderer.TryAllocateInstance();

        if (_worldBuffer == null)
        {
            return;
        }

        _worldBuffer.FillBuffer(gd, cl, ref dat);

        DbgMaterial colmat = new();
        colmat.Color = CurrentColor;
        colmat.GridCells = _gridCells;
        colmat.GridFadeDistance = _gridFadeDistance;
        colmat.GridOffsetX = _gridOffset.X;
        colmat.GridOffsetZ = _gridOffset.Y;
        colmat.GridLineWidth = _gridLineWidth;
        colmat.GridFlags = GridFlags;

        _materialBuffer.FillBuffer(gd, cl, ref colmat);

        if (_renderable != -1 && _debugPrimitive != null)
        {
            _renderablesSet.cBounds[_renderable] = BoundingBox.Transform(_debugPrimitive.Bounds, _world);
        }
    }

    public override void DestroyRenderables()
    {
        if (_renderable != -1)
        {
            _renderablesSet.RemoveRenderable(_renderable);
            _renderable = -1;
        }
    }

    public override void SetSelectable(ISelectable sel)
    {
        _selectable = new WeakReference<ISelectable>(sel);
        if (_renderable != -1)
        {
            _renderablesSet.cSelectables[_renderable] = _selectable;
        }
    }

    public RenderKey GetRenderKey(float distance)
    {
        // Overlays are always rendered last
        if (_overdraw)
        {
            return new RenderKey(ulong.MaxValue);
        }

        if (_debugPrimitive == null)
        {
            return new RenderKey(ulong.MaxValue);
        }

        var code = _pipeline != null ? (ulong)_pipeline.GetHashCode() : 0;

        var cameraDistanceInt = (uint)Math.Min(uint.MaxValue, distance * 1000f);
        ulong index = _debugPrimitive.Is32Bit ? 1u : 0;

        return new RenderKey(code << 41 | index << 40 |
                             ((ulong)(_renderablesSet.cDrawParameters[_renderable]._bufferIndex & 0xFF) << 32) +
                              cameraDistanceInt);
    }
}
