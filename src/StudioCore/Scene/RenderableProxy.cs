#nullable enable
using StudioCore.DebugPrimitives;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Scene;

/// <summary>
///     Model marker type for meshes that may not be visible in the editor (c0000, fogwalls, etc)
/// </summary>
public enum ModelMarkerType
{
    Enemy,
    Object,
    Player,
    Other,
    None
}

/// <summary>
///     Proxy class that represents a connection between the logical scene
///     heirarchy and the actual underlying renderable representation. Used to handle
///     things like renderable construction, selection, hiding/showing, etc
/// </summary>
public abstract class RenderableProxy : Renderer.IRendererUpdatable, IDisposable
{
    protected bool _autoregister = true;

    protected bool _registered;
    private bool disposedValue;

    public abstract bool RenderSelectionOutline { set; get; }

    public abstract Matrix4x4 World { get; set; }

    public abstract bool Visible { get; set; }

    public abstract RenderFilter DrawFilter { get; set; }
    public abstract DrawGroup DrawGroups { get; set; }
    public virtual bool AutoRegister { get => _autoregister; set => _autoregister = value; }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public abstract void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline sp);
    public abstract void DestroyRenderables();
    public abstract void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline sp);

    public abstract void SetSelectable(ISelectable sel);

    public abstract BoundingBox GetBounds();

    public abstract BoundingBox GetLocalBounds();

    internal void ScheduleRenderableConstruction()
    {
        Renderer.AddLowPriorityBackgroundUploadTask((gd, cl) =>
        {
            Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, @"Renderable construction");
            ConstructRenderables(gd, cl, null);
            Tracy.TracyCZoneEnd(ctx);
        });
    }

    internal void ScheduleRenderableUpdate()
    {
        Renderer.AddLowPriorityBackgroundUploadTask((gd, cl) =>
        {
            Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, @"Renderable update");
            UpdateRenderables(gd, cl, null);
            Tracy.TracyCZoneEnd(ctx);
        });
    }

    public virtual void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        ScheduleRenderableConstruction();
    }

    public virtual void UnregisterWithScene()
    {
        if (_registered)
        {
            _registered = false;
            DestroyRenderables();
        }
    }

    public virtual void UnregisterAndRelease()
    {
        if (_registered)
        {
            UnregisterWithScene();
        }
        /*if (Resource != null)
        {
            Resource.Release();
        }
        Resource = null;
        Created = false;
        Submeshes = null;*/
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            UnregisterAndRelease();
            disposedValue = true;
        }
    }

    protected uint GetPackedEntityID(int system, int index)
    {
        return ((uint)system << 30) | ((uint)index & 0x3FFFFFFF);
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~RenderableProxy()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(false);
    }
}

/// <summary>
///     Render proxy for a single static mesh
/// </summary>
public class MeshRenderableProxy : RenderableProxy, IMeshProviderEventListener
{
    /// <summary>
    ///     List of assets starting IDs that will always receive a model marker.
    ///     Contains speedtree AEGs, which currently do not render in DSMS.
    ///     Can be removed when speedtree rendering is functional.
    /// </summary>
    private static readonly HashSet<string> _speedtreeAegNames = new()
    {
        "AEG801",
        "AEG805",
        "AEG810",
        "AEG811",
        "AEG813",
        "AEG814",
        "AEG815",
        "AEG816"
    };

    private readonly MeshProvider _meshProvider;

    private readonly ModelMarkerType _placeholderType;
    private readonly MeshRenderables _renderablesSet;

    private readonly List<MeshRenderableProxy> _submeshes = new();

    private RenderFilter _drawfilter = RenderFilter.All;

    private DrawGroup _drawgroups = new();
    protected ResourceSet _perObjectResourceSet;
    protected Pipeline _pickingPipeline;

    protected Pipeline _pipeline;
    private RenderableProxy? _placeholderProxy;

    private int _renderable = -1;

    private bool _renderOutline;

    private WeakReference<ISelectable> _selectable;
    protected Pipeline _selectedPipeline;
    private int _selectionOutlineRenderable = -1;
    protected Shader[] _shaders;

    private bool _visible = true;

    private Matrix4x4 _world = Matrix4x4.Identity;
    protected GPUBufferAllocator.GPUBufferHandle? _worldBuffer;

    public MeshRenderableProxy(
        MeshRenderables renderables,
        MeshProvider provider,
        ModelMarkerType placeholderType = ModelMarkerType.None,
        bool autoregister = true)
    {
        AutoRegister = autoregister;
        _registered = AutoRegister;
        _renderablesSet = renderables;
        _meshProvider = provider;
        _placeholderType = placeholderType;
        _meshProvider.AddEventListener(this);
        _meshProvider.Acquire();
        if (autoregister)
        {
            ScheduleRenderableConstruction();
        }
    }

    public MeshRenderableProxy(MeshRenderableProxy clone) :
        this(clone._renderablesSet, clone._meshProvider, clone._placeholderType)
    {
        DrawFilter = clone._drawfilter;
    }

    public IReadOnlyList<MeshRenderableProxy> Submeshes => _submeshes;

    public IResourceHandle ResourceHandle => _meshProvider.ResourceHandle;

    public override bool AutoRegister
    {
        get => _autoregister;
        set
        {
            _autoregister = value;
            foreach (MeshRenderableProxy c in _submeshes)
            {
                c.AutoRegister = value;
            }
        }
    }

    public override Matrix4x4 World
    {
        get => _world;
        set
        {
            _world = value;
            ScheduleRenderableUpdate();
            foreach (MeshRenderableProxy sm in _submeshes)
            {
                sm.World = _world;
            }

            if (_placeholderProxy != null)
            {
                _placeholderProxy.World = value;
            }
        }
    }

    public override bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            if (_renderable != -1)
            {
                _renderablesSet.cVisible[_renderable]._visible = value;
                if (!_meshProvider.SelectedRenderBaseMesh && _renderOutline)
                {
                    _renderablesSet.cVisible[_renderable]._visible = false;
                }
            }

            foreach (MeshRenderableProxy sm in _submeshes)
            {
                sm.Visible = _visible;
            }

            if (_placeholderProxy != null)
            {
                _placeholderProxy.Visible = _visible;
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

            foreach (MeshRenderableProxy sm in _submeshes)
            {
                sm.DrawFilter = _drawfilter;
            }

            if (_placeholderProxy != null)
            {
                _placeholderProxy.DrawFilter = value;
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

            foreach (MeshRenderableProxy sm in _submeshes)
            {
                sm.DrawGroups = _drawgroups;
            }

            if (_placeholderProxy != null)
            {
                _placeholderProxy.DrawGroups = value;
            }
        }
    }

    public override bool RenderSelectionOutline
    {
        get => _renderOutline;
        set
        {
            _renderOutline = value;
            if (_registered)
            {
                ScheduleRenderableConstruction();
            }

            foreach (MeshRenderableProxy child in _submeshes)
            {
                child.RenderSelectionOutline = value;
            }

            if (_placeholderProxy != null)
            {
                _placeholderProxy.RenderSelectionOutline = value;
            }
        }
    }

    public void OnProviderAvailable()
    {
        var needsPlaceholder = _placeholderType != ModelMarkerType.None;
        for (var i = 0; i < _meshProvider.ChildCount; i++)
        {
            MeshRenderableProxy child = new(_renderablesSet, _meshProvider.GetChildProvider(i),
                ModelMarkerType.None, AutoRegister);
            child.World = _world;
            child.Visible = _visible;
            child.DrawFilter = _drawfilter;
            child.DrawGroups = _drawgroups;
            _submeshes.Add(child);
            ISelectable sel = null;
            if (_selectable != null)
            {
                _selectable.TryGetTarget(out sel);
                if (sel != null)
                {
                    child.SetSelectable(sel);
                }
            }

            if (child._meshProvider != null && child._meshProvider.IsAvailable() &&
                child._meshProvider.IndexCount > 0)
            {
                needsPlaceholder = false;
            }
        }

        if (_meshProvider.HasMeshData())
        {
            ScheduleRenderableConstruction();
            if (_meshProvider != null && _meshProvider.IndexCount > 0)
            {
                needsPlaceholder = false;
            }
        }

        if (_placeholderType != ModelMarkerType.None)
        {
            if (_meshProvider is FlverMeshProvider fProvider)
            {
                if (_speedtreeAegNames.FirstOrDefault(n => fProvider.MeshName.ToUpper().StartsWith(n)) != null)
                {
                    if (fProvider.MeshName.ToUpper() != "AEG801_224")
                    {
                        // Non-rendering speedtree
                        needsPlaceholder = true;
                    }
                }
            }

            if (needsPlaceholder)
            {
                _placeholderProxy =
                    DebugPrimitiveRenderableProxy.GetModelMarkerProxy(_renderablesSet, _placeholderType);
                _placeholderProxy.World = World;
                _placeholderProxy.Visible = Visible;
                _placeholderProxy.DrawFilter = _drawfilter;
                _placeholderProxy.DrawGroups = _drawgroups;
                if (_selectable != null)
                {
                    _selectable.TryGetTarget(out ISelectable? sel);
                    if (sel != null)
                    {
                        _placeholderProxy.SetSelectable(sel);
                    }
                }

                if (_registered)
                {
                    _placeholderProxy.Register();
                }
            }
        }
    }

    public void OnProviderUnavailable()
    {
        if (_meshProvider.IsAtomic())
        {
            foreach (MeshRenderableProxy c in _submeshes)
            {
                c.UnregisterAndRelease();
            }

            _submeshes.Clear();
        }

        if (_placeholderProxy != null)
        {
            _placeholderProxy.Dispose();
            _placeholderProxy = null;
        }
    }

    public override BoundingBox GetBounds()
    {
        return BoundingBox.Transform(GetLocalBounds(), _world);
    }

    public override BoundingBox GetLocalBounds()
    {
        if (_meshProvider.IsAvailable() && _meshProvider.HasMeshData() && _meshProvider.TryLock())
        {
            _meshProvider.Unlock();
            return BoundingBox.Transform(_meshProvider.Bounds, _meshProvider.ObjectTransform);
        }

        BoundingBox b = _submeshes.Count > 0 ? _submeshes[0].GetLocalBounds() : new BoundingBox();
        foreach (MeshRenderableProxy c in _submeshes)
        {
            b = BoundingBox.Combine(b, c.GetLocalBounds());
        }

        return b;
    }

    public override void Register()
    {
        if (_registered)
        {
            return;
        }

        foreach (MeshRenderableProxy c in _submeshes)
        {
            c.Register();
        }

        _placeholderProxy?.Register();
        _registered = true;
        ScheduleRenderableConstruction();
    }

    public override void UnregisterWithScene()
    {
        if (_registered)
        {
            _registered = false;
            DestroyRenderables();
        }

        foreach (MeshRenderableProxy c in _submeshes)
        {
            c.UnregisterWithScene();
        }

        _placeholderProxy?.UnregisterWithScene();
    }

    public override void UnregisterAndRelease()
    {
        if (_registered)
        {
            UnregisterWithScene();
        }

        foreach (MeshRenderableProxy c in _submeshes)
        {
            c.UnregisterAndRelease();
        }

        _placeholderProxy?.UnregisterAndRelease();

        if (_meshProvider != null)
        {
            _meshProvider.Release();
        }

        if (_worldBuffer != null)
        {
            _worldBuffer.Dispose();
            _worldBuffer = null;
        }
    }

    public override unsafe void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline sp)
    {
        // If we were unregistered before construction time, don't construct now
        if (!_registered)
        {
            return;
        }

        foreach (MeshRenderableProxy c in _submeshes)
        {
            if (c._registered)
            {
                c.ScheduleRenderableConstruction();
            }
        }

        if (_renderable != -1)
        {
            _renderablesSet.RemoveRenderable(_renderable);
            _renderable = -1;
        }

        if (_selectionOutlineRenderable != -1)
        {
            _renderablesSet.RemoveRenderable(_selectionOutlineRenderable);
            _selectionOutlineRenderable = -1;
        }

        if (!_meshProvider.IsAvailable() || !_meshProvider.HasMeshData())
        {
            return;
        }

        if (_meshProvider.GeometryBuffer == null)
        {
            return;
        }

        if (_meshProvider.GeometryBuffer.AllocStatus !=
            VertexIndexBufferAllocator.VertexIndexBuffer.Status.Resident)
        {
            ScheduleRenderableConstruction();
            return;
        }

        ResourceFactory? factory = gd.ResourceFactory;
        if (_worldBuffer == null)
        {
            _worldBuffer =
                Renderer.UniformBufferAllocator.Allocate((uint)sizeof(InstanceData), sizeof(InstanceData));
        }

        // Construct pipeline
        ResourceLayout projViewCombinedLayout = StaticResourceCache.GetResourceLayout(
            gd.ResourceFactory,
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ViewProjection", VkDescriptorType.UniformBuffer,
                    VkShaderStageFlags.Vertex)));

        ResourceLayout worldLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory,
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "World",
                    VkDescriptorType.UniformBufferDynamic,
                    VkShaderStageFlags.Vertex,
                    VkDescriptorBindingFlags.None)));


        VertexLayoutDescription[] mainVertexLayouts = { _meshProvider.LayoutDescription };

        Tuple<Shader, Shader> res = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, _meshProvider.ShaderName)
            .ToTuple();
        _shaders = new[] { res.Item1, res.Item2 };

        ResourceLayout projViewLayout = StaticResourceCache.GetResourceLayout(
            gd.ResourceFactory,
            StaticResourceCache.SceneParamLayoutDescription);

        ResourceLayout pickingResultLayout = StaticResourceCache.GetResourceLayout(
            gd.ResourceFactory,
            StaticResourceCache.PickingResultDescription);

        ResourceLayout mainPerObjectLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory,
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "WorldBuffer",
                    VkDescriptorType.StorageBuffer,
                    VkShaderStageFlags.Vertex | VkShaderStageFlags.Fragment,
                    VkDescriptorBindingFlags.None)));

        ResourceLayout texLayout = StaticResourceCache.GetResourceLayout(gd.ResourceFactory,
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "globalTextures",
                    VkDescriptorType.SampledImage,
                    VkShaderStageFlags.Vertex | VkShaderStageFlags.Fragment,
                    VkDescriptorBindingFlags.None)));

        _perObjectResourceSet = StaticResourceCache.GetResourceSet(factory, new ResourceSetDescription(
            mainPerObjectLayout,
            Renderer.UniformBufferAllocator._backingBuffer));

        // Build default pipeline
        GraphicsPipelineDescription pipelineDescription = new();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
        pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyGreaterEqual;
        pipelineDescription.RasterizerState = new RasterizerStateDescription(
            _meshProvider.CullMode,
            _meshProvider.FillMode,
            _meshProvider.FrontFace,
            true,
            false);
        pipelineDescription.PrimitiveTopology = _meshProvider.Topology;
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            mainVertexLayouts,
            _shaders, _meshProvider.SpecializationConstants);
        pipelineDescription.ResourceLayouts = new[]
        {
            projViewLayout, mainPerObjectLayout, Renderer.GlobalTexturePool.GetLayout(),
            Renderer.GlobalCubeTexturePool.GetLayout(), Renderer.MaterialBufferAllocator.GetLayout(),
            SamplerSet.SamplersLayout, pickingResultLayout, Renderer.BoneBufferAllocator.GetLayout()
        };
        pipelineDescription.Outputs = gd.SwapchainFramebuffer.OutputDescription;
        _pipeline = StaticResourceCache.GetPipeline(factory, ref pipelineDescription);

        // Build picking pipeline
        var pickingSpecializationConstants =
            new SpecializationConstant[_meshProvider.SpecializationConstants.Length + 1];
        Array.Copy(_meshProvider.SpecializationConstants, pickingSpecializationConstants,
            _meshProvider.SpecializationConstants.Length);
        pickingSpecializationConstants[pickingSpecializationConstants.Length - 1] =
            new SpecializationConstant(99, true);
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            mainVertexLayouts,
            _shaders, pickingSpecializationConstants);
        _pickingPipeline = StaticResourceCache.GetPipeline(factory, ref pipelineDescription);

        // Create draw call arguments
        MeshDrawParametersComponent meshcomp = new();
        VertexIndexBufferAllocator.VertexIndexBufferHandle? geombuffer = _meshProvider.GeometryBuffer;
        var indexStart = (geombuffer.IAllocationStart / (_meshProvider.Is32Bit ? 4u : 2u)) +
                         (uint)_meshProvider.IndexOffset;
        meshcomp._indirectArgs.FirstInstance = _worldBuffer.AllocationStart / (uint)sizeof(InstanceData);
        meshcomp._indirectArgs.VertexOffset = (int)(geombuffer.VAllocationStart / _meshProvider.VertexSize);
        meshcomp._indirectArgs.InstanceCount = 1;
        meshcomp._indirectArgs.FirstIndex = indexStart;
        meshcomp._indirectArgs.IndexCount = (uint)_meshProvider.IndexCount;

        // Rest of draw parameters
        meshcomp._indexFormat = _meshProvider.Is32Bit ? VkIndexType.Uint32 : VkIndexType.Uint16;
        meshcomp._objectResourceSet = _perObjectResourceSet;
        meshcomp._bufferIndex = geombuffer.BufferIndex;

        // Instantiate renderable
        BoundingBox bounds = BoundingBox.Transform(_meshProvider.Bounds, _meshProvider.ObjectTransform * _world);
        _renderable = _renderablesSet.CreateMesh(ref bounds, ref meshcomp);
        _renderablesSet.cRenderKeys[_renderable] = GetRenderKey(0.0f);

        // Pipelines
        _renderablesSet.cPipelines[_renderable] = _pipeline;
        _renderablesSet.cSelectionPipelines[_renderable] = _pickingPipeline;

        // Update instance data
        InstanceData dat = new();
        dat.WorldMatrix = _meshProvider.ObjectTransform * _world;
        dat.MaterialID = _meshProvider.MaterialIndex;
        if (_meshProvider.BoneBuffer != null)
        {
            dat.BoneStartIndex = _meshProvider.BoneBuffer.AllocationStart / 64;
        }
        else
        {
            dat.BoneStartIndex = 0;
        }

        dat.EntityID = GetPackedEntityID(_renderablesSet.RenderableSystemIndex, _renderable);
        _worldBuffer.FillBuffer(gd, cl, ref dat);

        // Selectable
        _renderablesSet.cSelectables[_renderable] = _selectable;

        // Visible
        _renderablesSet.cVisible[_renderable]._visible = _visible;
        if (!_meshProvider.SelectedRenderBaseMesh && _renderOutline)
        {
            _renderablesSet.cVisible[_renderable]._visible = false;
        }

        _renderablesSet.cSceneVis[_renderable]._renderFilter = _drawfilter;
        _renderablesSet.cSceneVis[_renderable]._drawGroup = _drawgroups;

        // Build mesh for selection outline
        if (_renderOutline)
        {
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                _meshProvider.SelectedUseBackface
                    ? _meshProvider.CullMode == VkCullModeFlags.Front ? VkCullModeFlags.Back : VkCullModeFlags.Front
                    : _meshProvider.CullMode,
                _meshProvider.FillMode,
                _meshProvider.FrontFace,
                true,
                false);

            Tuple<Shader, Shader> s = StaticResourceCache.GetShaders(gd, gd.ResourceFactory,
                _meshProvider.ShaderName + (_meshProvider.UseSelectedShader ? "_selected" : "")).ToTuple();
            _shaders = new[] { s.Item1, s.Item2 };
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                mainVertexLayouts,
                _shaders, _meshProvider.SpecializationConstants);
            _selectedPipeline = StaticResourceCache.GetPipeline(factory, ref pipelineDescription);

            _selectionOutlineRenderable = _renderablesSet.CreateMesh(ref bounds, ref meshcomp);
            _renderablesSet.cRenderKeys[_selectionOutlineRenderable] = GetRenderKey(0.0f);

            // Pipelines
            _renderablesSet.cPipelines[_selectionOutlineRenderable] = _selectedPipeline;
            _renderablesSet.cSelectionPipelines[_selectionOutlineRenderable] = _selectedPipeline;

            // Selectable
            _renderablesSet.cSelectables[_selectionOutlineRenderable] = _selectable;
        }
    }

    public override unsafe void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline sp)
    {
        if (!_meshProvider.IsAvailable() || !_meshProvider.HasMeshData())
        {
            _meshProvider.Unlock();
            return;
        }

        InstanceData dat = new();
        dat.WorldMatrix = _meshProvider.ObjectTransform * _world;
        dat.MaterialID = _meshProvider.MaterialIndex;
        if (_meshProvider.BoneBuffer != null)
        {
            dat.BoneStartIndex = _meshProvider.BoneBuffer.AllocationStart / 64;
        }
        else
        {
            dat.BoneStartIndex = 0;
        }

        dat.EntityID = GetPackedEntityID(_renderablesSet.RenderableSystemIndex, _renderable);
        if (_worldBuffer == null)
        {
            _worldBuffer =
                Renderer.UniformBufferAllocator.Allocate((uint)sizeof(InstanceData), sizeof(InstanceData));
        }

        _worldBuffer.FillBuffer(gd, cl, ref dat);

        if (_renderable != -1)
        {
            _renderablesSet.cBounds[_renderable] =
                BoundingBox.Transform(_meshProvider.Bounds, _meshProvider.ObjectTransform * _world);
        }

        if (_selectionOutlineRenderable != -1)
        {
            _renderablesSet.cBounds[_selectionOutlineRenderable] =
                BoundingBox.Transform(_meshProvider.Bounds, _meshProvider.ObjectTransform * _world);
        }
    }

    public override void DestroyRenderables()
    {
        if (_renderable != -1)
        {
            _renderablesSet.RemoveRenderable(_renderable);
            _renderable = -1;
        }

        if (_selectionOutlineRenderable != -1)
        {
            _renderablesSet.RemoveRenderable(_selectionOutlineRenderable);
            _selectionOutlineRenderable = -1;
        }

        foreach (MeshRenderableProxy c in _submeshes)
        {
            c.DestroyRenderables();
        }

        _placeholderProxy?.DestroyRenderables();
    }

    public override void SetSelectable(ISelectable sel)
    {
        _selectable = new WeakReference<ISelectable>(sel);
        if (_renderable != -1)
        {
            _renderablesSet.cSelectables[_renderable] = _selectable;
        }

        foreach (MeshRenderableProxy child in _submeshes)
        {
            child.SetSelectable(sel);
        }

        _placeholderProxy?.SetSelectable(sel);
    }

    public RenderKey GetRenderKey(float distance)
    {
        var code = _pipeline != null ? (ulong)_pipeline.GetHashCode() : 0;
        ulong index = 0;

        var cameraDistanceInt = (uint)Math.Min(uint.MaxValue, distance * 1000f);

        if (_meshProvider.IsAvailable())
        {
            if (_meshProvider.TryLock())
            {
                index = _meshProvider.Is32Bit ? 1u : 0;
                _meshProvider.Unlock();
            }
        }

        return new RenderKey((code << 41) | (index << 40) |
                             (((ulong)(_renderablesSet.cDrawParameters[_renderable]._bufferIndex & 0xFF) << 32) +
                              cameraDistanceInt));
    }

    public static MeshRenderableProxy MeshRenderableFromFlverResource(
        RenderScene scene, string virtualPath, ModelMarkerType modelType)
    {
        MeshRenderableProxy renderable = new(scene.OpaqueRenderables,
            MeshProviderCache.GetFlverMeshProvider(virtualPath), modelType);
        return renderable;
    }

    public static MeshRenderableProxy MeshRenderableFromCollisionResource(
        RenderScene scene, string virtualPath, ModelMarkerType modelType)
    {
        MeshRenderableProxy renderable = new(scene.OpaqueRenderables,
            MeshProviderCache.GetCollisionMeshProvider(virtualPath), modelType);
        return renderable;
    }

    public static MeshRenderableProxy MeshRenderableFromNVMResource(
        RenderScene scene, string virtualPath, ModelMarkerType modelType)
    {
        MeshRenderableProxy renderable = new(scene.OpaqueRenderables,
            MeshProviderCache.GetNVMMeshProvider(virtualPath), modelType);
        return renderable;
    }

    public static MeshRenderableProxy MeshRenderableFromHavokNavmeshResource(
        RenderScene scene, string virtualPath, ModelMarkerType modelType, bool temp = false)
    {
        MeshRenderableProxy renderable = new(scene.OpaqueRenderables,
            MeshProviderCache.GetHavokNavMeshProvider(virtualPath, temp), modelType);
        return renderable;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct DbgMaterial
{
    private struct _color
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
    }

    private _color _Color;
    public fixed int pad[3];

    public Color Color
    {
        set
        {
            _Color.r = value.R;
            _Color.g = value.G;
            _Color.b = value.B;
            _Color.a = value.A;
        }
    }
}

public class DebugPrimitiveRenderableProxy : RenderableProxy
{
    private static float _colorHueIncrement;

    private static DbgPrimWireBox? _regionBox;
    private static DbgPrimWireCylinder? _regionCylinder;
    private static DbgPrimWireSphere? _regionSphere;
    private static DbgPrimWireSphere? _regionPoint;
    private static DbgPrimWireSphere? _dmyPoint;
    private static DbgPrimWireSphere? _jointSphere;
    private static DbgPrimWireSpheroidWithArrow? _modelMarkerChr;
    private static DbgPrimWireWallBox? _modelMarkerObj;
    private static DbgPrimWireSpheroidWithArrow? _modelMarkerPlayer;
    private static DbgPrimWireWallBox? _modelMarkerOther;
    private static DbgPrimWireSphere? _pointLight;
    private static DbgPrimWireSpotLight? _spotLight;
    private static DbgPrimWireSpheroidWithArrow? _directionalLight;

    private readonly IDbgPrim? _debugPrimitive;

    private readonly MeshRenderables _renderablesSet;
    private Color _baseColor = Color.Gray;

    private RenderFilter _drawfilter = RenderFilter.All;

    private DrawGroup _drawgroups = new();

    private bool _hasColorVariance;
    private Color _highlightedColor = Color.Gray;
    private Color _initialColor = Color.Empty;
    protected GPUBufferAllocator.GPUBufferHandle _materialBuffer;

    private bool _overdraw;
    protected ResourceSet _perObjectResourceSet;
    protected Pipeline _pickingPipeline;

    protected Pipeline _pipeline;

    private int _renderable = -1;

    private bool _renderOutline;
    private WeakReference<ISelectable> _selectable;
    protected Shader[] _shaders;

    private bool _visible = true;

    private Matrix4x4 _world = Matrix4x4.Identity;
    protected GPUBufferAllocator.GPUBufferHandle _worldBuffer;

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
        if (clone._hasColorVariance)
        {
            ApplyColorVariance(this);
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

    public override Matrix4x4 World
    {
        get => _world;
        set
        {
            _world = value;
            ScheduleRenderableUpdate();
        }
    }

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

    public override BoundingBox GetBounds()
    {
        return BoundingBox.Transform(_debugPrimitive.Bounds, _world);
    }

    public override BoundingBox GetLocalBounds()
    {
        return _debugPrimitive.Bounds;
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

    public override unsafe void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline sp)
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

        if (_debugPrimitive.GeometryBuffer.AllocStatus !=
            VertexIndexBufferAllocator.VertexIndexBuffer.Status.Resident)
        {
            ScheduleRenderableConstruction();
            return;
        }

        ResourceFactory? factory = gd.ResourceFactory;
        if (_worldBuffer == null)
        {
            _worldBuffer =
                Renderer.UniformBufferAllocator.Allocate((uint)sizeof(InstanceData), sizeof(InstanceData));
        }

        if (_materialBuffer == null)
        {
            _materialBuffer =
                Renderer.MaterialBufferAllocator.Allocate((uint)sizeof(DbgMaterial), sizeof(DbgMaterial));
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

        _perObjectResourceSet = StaticResourceCache.GetResourceSet(factory, new ResourceSetDescription(
            mainPerObjectLayout,
            Renderer.UniformBufferAllocator._backingBuffer));

        // Build default pipeline
        GraphicsPipelineDescription pipelineDescription = new();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
        pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyGreaterEqual;
        pipelineDescription.RasterizerState = new RasterizerStateDescription(
            _debugPrimitive.CullMode,
            _debugPrimitive.FillMode,
            _debugPrimitive.FrontFace,
            true,
            false);
        pipelineDescription.PrimitiveTopology = _debugPrimitive.Topology;
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            mainVertexLayouts,
            _shaders, _debugPrimitive.SpecializationConstants);
        pipelineDescription.ResourceLayouts = new[]
        {
            projViewLayout, mainPerObjectLayout, Renderer.GlobalTexturePool.GetLayout(),
            Renderer.GlobalCubeTexturePool.GetLayout(), Renderer.MaterialBufferAllocator.GetLayout(),
            SamplerSet.SamplersLayout, pickingResultLayout, Renderer.BoneBufferAllocator.GetLayout()
        };
        pipelineDescription.Outputs = gd.SwapchainFramebuffer.OutputDescription;
        _pipeline = StaticResourceCache.GetPipeline(factory, ref pipelineDescription);

        // Build picking pipeline
        var pickingSpecializationConstants =
            new SpecializationConstant[_debugPrimitive.SpecializationConstants.Length + 1];
        Array.Copy(_debugPrimitive.SpecializationConstants, pickingSpecializationConstants,
            _debugPrimitive.SpecializationConstants.Length);
        pickingSpecializationConstants[pickingSpecializationConstants.Length - 1] =
            new SpecializationConstant(99, true);
        pipelineDescription.ShaderSet = new ShaderSetDescription(
            mainVertexLayouts,
            _shaders, pickingSpecializationConstants);
        _pickingPipeline = StaticResourceCache.GetPipeline(factory, ref pipelineDescription);

        // Create draw call arguments
        MeshDrawParametersComponent meshcomp = new();
        VertexIndexBufferAllocator.VertexIndexBufferHandle? geombuffer = _debugPrimitive.GeometryBuffer;
        var indexStart = (geombuffer.IAllocationStart / (_debugPrimitive.Is32Bit ? 4u : 2u)) +
                         (uint)_debugPrimitive.IndexOffset;
        meshcomp._indirectArgs.FirstInstance = _worldBuffer.AllocationStart / (uint)sizeof(InstanceData);
        meshcomp._indirectArgs.VertexOffset = (int)(geombuffer.VAllocationStart / _debugPrimitive.VertexSize);
        meshcomp._indirectArgs.InstanceCount = 1;
        meshcomp._indirectArgs.FirstIndex = indexStart;
        meshcomp._indirectArgs.IndexCount = geombuffer.IAllocationSize / (_debugPrimitive.Is32Bit ? 4u : 2u);

        // Rest of draw parameters
        meshcomp._indexFormat = _debugPrimitive.Is32Bit ? VkIndexType.Uint32 : VkIndexType.Uint16;
        meshcomp._objectResourceSet = _perObjectResourceSet;
        meshcomp._bufferIndex = geombuffer.BufferIndex;

        // Instantiate renderable
        BoundingBox bounds = BoundingBox.Transform(_debugPrimitive.Bounds, _world);
        _renderable = _renderablesSet.CreateMesh(ref bounds, ref meshcomp);
        _renderablesSet.cRenderKeys[_renderable] = GetRenderKey(0.0f);

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
        colmat.Color = _renderOutline ? HighlightedColor : BaseColor;
        _materialBuffer.FillBuffer(gd, cl, ref colmat);

        // Selectable
        _renderablesSet.cSelectables[_renderable] = _selectable;

        // Visible
        _renderablesSet.cVisible[_renderable]._visible = _visible;
        _renderablesSet.cSceneVis[_renderable]._renderFilter = _drawfilter;
        _renderablesSet.cSceneVis[_renderable]._drawGroup = _drawgroups;
    }

    public override unsafe void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline sp)
    {
        if (_materialBuffer == null)
        {
            _materialBuffer =
                Renderer.MaterialBufferAllocator.Allocate((uint)sizeof(DbgMaterial), sizeof(DbgMaterial));
        }

        InstanceData dat = new();
        dat.WorldMatrix = _world;
        dat.MaterialID = _materialBuffer.AllocationStart / (uint)sizeof(DbgMaterial);
        dat.EntityID = GetPackedEntityID(_renderablesSet.RenderableSystemIndex, _renderable);
        if (_worldBuffer == null)
        {
            _worldBuffer =
                Renderer.UniformBufferAllocator.Allocate((uint)sizeof(InstanceData), sizeof(InstanceData));
        }

        _worldBuffer.FillBuffer(gd, cl, ref dat);

        DbgMaterial colmat = new();
        colmat.Color = _renderOutline ? HighlightedColor : BaseColor;
        _materialBuffer.FillBuffer(gd, cl, ref colmat);

        if (_renderable != -1)
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

        var code = _pipeline != null ? (ulong)_pipeline.GetHashCode() : 0;

        var cameraDistanceInt = (uint)Math.Min(uint.MaxValue, distance * 1000f);
        ulong index = _debugPrimitive.Is32Bit ? 1u : 0;

        return new RenderKey((code << 41) | (index << 40) |
                             (((ulong)(_renderablesSet.cDrawParameters[_renderable]._bufferIndex & 0xFF) << 32) +
                              cameraDistanceInt));
    }

    /// <summary>
    ///     These are initialized explicitly to ensure these meshes are created at startup time so that they don't share
    ///     vertex buffer memory with dynamically allocated resources and cause the megabuffers to not be freed.
    /// </summary>
    public static void InitializeDebugMeshes()
    {
        _regionBox = new DbgPrimWireBox(Transform.Default, new Vector3(-0.5f, 0.0f, -0.5f),
            new Vector3(0.5f, 1.0f, 0.5f), Color.Blue);
        _regionCylinder = new DbgPrimWireCylinder(Transform.Default, 1.0f, 1.0f, 12, Color.Blue);
        _regionSphere = new DbgPrimWireSphere(Transform.Default, 1.0f, Color.Blue);
        _regionPoint = new DbgPrimWireSphere(Transform.Default, 1.0f, Color.Yellow, 1, 4);
        _dmyPoint = new DbgPrimWireSphere(Transform.Default, 0.05f, Color.Yellow, 1, 4);
        _jointSphere = new DbgPrimWireSphere(Transform.Default, 0.05f, Color.Blue, 6, 6);
        _modelMarkerChr = new DbgPrimWireSpheroidWithArrow(Transform.Default, .9f, Color.Firebrick, 4, 10, true);
        _modelMarkerObj = new DbgPrimWireWallBox(Transform.Default, new Vector3(-1.5f, 0.0f, -0.75f),
            new Vector3(1.5f, 2.5f, 0.75f), Color.Firebrick);
        _modelMarkerPlayer =
            new DbgPrimWireSpheroidWithArrow(Transform.Default, 0.75f, Color.Firebrick, 1, 6, true);
        _modelMarkerOther = new DbgPrimWireWallBox(Transform.Default, new Vector3(-0.3f, 0.0f, -0.3f),
            new Vector3(0.3f, 1.8f, 0.3f), Color.Firebrick);
        _pointLight = new DbgPrimWireSphere(Transform.Default, 1.0f, Color.Yellow, 6, 6);
        _spotLight = new DbgPrimWireSpotLight(Transform.Default, 1.0f, 1.0f, Color.Yellow);
        _directionalLight =
            new DbgPrimWireSpheroidWithArrow(Transform.Default, 5.0f, Color.Yellow, 4, 2, false, true);
    }

    private static Color GetRenderableColor(Vector3 color)
    {
        return Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }

    public static DebugPrimitiveRenderableProxy GetBoxRegionProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _regionBox);
        r.BaseColor = GetRenderableColor(CFG.Current.GFX_Renderable_Box_BaseColor);
        r.HighlightedColor = GetRenderableColor(CFG.Current.GFX_Renderable_Box_HighlightColor);
        ApplyColorVariance(r);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetCylinderRegionProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _regionCylinder);
        r.BaseColor = GetRenderableColor(CFG.Current.GFX_Renderable_Cylinder_BaseColor);
        r.HighlightedColor = GetRenderableColor(CFG.Current.GFX_Renderable_Cylinder_HighlightColor);
        ApplyColorVariance(r);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetSphereRegionProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _regionSphere);
        r.BaseColor = GetRenderableColor(CFG.Current.GFX_Renderable_Sphere_BaseColor);
        r.HighlightedColor = GetRenderableColor(CFG.Current.GFX_Renderable_Sphere_HighlightColor);
        ApplyColorVariance(r);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetPointRegionProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _regionPoint);
        r.BaseColor = GetRenderableColor(CFG.Current.GFX_Renderable_Point_BaseColor);
        r.HighlightedColor = GetRenderableColor(CFG.Current.GFX_Renderable_Point_HighlightColor);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetDummyPolyRegionProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OverlayRenderables, _dmyPoint);
        r.BaseColor = GetRenderableColor(CFG.Current.GFX_Renderable_DummyPoly_BaseColor);
        r.HighlightedColor = GetRenderableColor(CFG.Current.GFX_Renderable_DummyPoly_HighlightColor);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetBonePointProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OverlayRenderables, _jointSphere);
        r.BaseColor = GetRenderableColor(CFG.Current.GFX_Renderable_BonePoint_BaseColor);
        r.HighlightedColor = GetRenderableColor(CFG.Current.GFX_Renderable_BonePoint_HighlightColor);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetModelMarkerProxy(MeshRenderables renderables,
        ModelMarkerType type)
    {
        // Model markers are used as placeholders for meshes that would not otherwise render in the editor
        IDbgPrim? prim;
        Color baseColor;
        Color selectColor;

        switch (type)
        {
            case ModelMarkerType.Enemy:
                prim = _modelMarkerChr;
                baseColor = GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor);
                selectColor = GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor);
                break;
            case ModelMarkerType.Object:
                prim = _modelMarkerObj;
                baseColor = GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor);
                selectColor = GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor);
                break;
            case ModelMarkerType.Player:
                prim = _modelMarkerPlayer;
                baseColor = GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor);
                selectColor = GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor);
                break;
            case ModelMarkerType.Other:
            default:
                prim = _modelMarkerOther;
                baseColor = GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor);
                selectColor = GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor);
                break;
        }

        DebugPrimitiveRenderableProxy r = new(renderables, prim, false);
        r.BaseColor = baseColor;
        r.HighlightedColor = selectColor;

        return r;
    }

    public static DebugPrimitiveRenderableProxy GetPointLightProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _pointLight);
        r.BaseColor = GetRenderableColor(CFG.Current.GFX_Renderable_PointLight_BaseColor);
        r.HighlightedColor = GetRenderableColor(CFG.Current.GFX_Renderable_PointLight_HighlightColor);
        ApplyColorVariance(r);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetSpotLightProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _spotLight);
        r.BaseColor = GetRenderableColor(CFG.Current.GFX_Renderable_SpotLight_BaseColor);
        r.HighlightedColor = GetRenderableColor(CFG.Current.GFX_Renderable_SpotLight_HighlightColor);
        ApplyColorVariance(r);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetDirectionalLightProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _directionalLight);
        r.BaseColor = GetRenderableColor(CFG.Current.GFX_Renderable_DirectionalLight_BaseColor);
        r.HighlightedColor = GetRenderableColor(CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor);
        return r;
    }

    private static void ApplyColorVariance(DebugPrimitiveRenderableProxy rend)
    {
        // Determines how much color varies per-increment.
        const float incrementModifier = 0.721f;

        rend._hasColorVariance = true;

        Vector3 hsv = Utils.ColorToHSV(rend._initialColor);
        var range = 360.0f * CFG.Current.GFX_Wireframe_Color_Variance / 2;
        _colorHueIncrement += range * incrementModifier;
        if (_colorHueIncrement > range)
        {
            _colorHueIncrement -= range * 2;
        }

        hsv.X += _colorHueIncrement;
        if (hsv.X > 360.0f)
        {
            hsv.X -= 360.0f;
        }
        else if (hsv.X < 0.0f)
        {
            hsv.X += 360.0f;
        }

        rend.BaseColor = Utils.ColorFromHSV(hsv);
    }
}

public class SkeletonBoneRenderableProxy : RenderableProxy
{
    /// <summary>
    ///     Renderable for the actual bone
    /// </summary>
    private readonly DebugPrimitiveRenderableProxy _bonePointRenderable;

    /// <summary>
    ///     Renderables for the bones to child joints
    /// </summary>
    private readonly List<DebugPrimitiveRenderableProxy> _boneRenderables = new();

    /// <summary>
    ///     Child renderables that this bone is connected to
    /// </summary>
    private List<SkeletonBoneRenderableProxy> _childBones = new();

    private RenderFilter _drawfilter = RenderFilter.All;

    private DrawGroup _drawgroups = new();

    private bool _renderOutline;

    private WeakReference<ISelectable> _selectable;
    private bool _visible = true;

    private Matrix4x4 _world = Matrix4x4.Identity;

    public SkeletonBoneRenderableProxy(RenderScene scene)
    {
        _bonePointRenderable = DebugPrimitiveRenderableProxy.GetBonePointProxy(scene);
        ScheduleRenderableConstruction();
        AutoRegister = true;
        _registered = true;
    }

    public override bool AutoRegister
    {
        get => _autoregister;
        set
        {
            _autoregister = value;
            _bonePointRenderable.AutoRegister = value;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.AutoRegister = value;
            }
        }
    }

    public override bool RenderSelectionOutline
    {
        get => _renderOutline;
        set
        {
            _renderOutline = value;
            _bonePointRenderable.RenderSelectionOutline = _renderOutline;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.RenderSelectionOutline = value;
            }
        }
    }

    public override Matrix4x4 World
    {
        get => _world;
        set
        {
            _world = value;
            ScheduleRenderableUpdate();
            _bonePointRenderable.World = _world;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.World = _world;
            }
        }
    }

    public override bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            _bonePointRenderable.Visible = value;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.Visible = _visible;
            }
        }
    }

    public override RenderFilter DrawFilter
    {
        get => _drawfilter;
        set
        {
            _drawfilter = value;
            _bonePointRenderable.DrawFilter = value;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.DrawFilter = _drawfilter;
            }
        }
    }

    public override DrawGroup DrawGroups
    {
        get => _drawgroups;
        set
        {
            _drawgroups = value;
            _bonePointRenderable.DrawGroups = _drawgroups;
            foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
            {
                c.DrawGroups = _drawgroups;
            }
        }
    }

    public override void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline sp)
    {
        _bonePointRenderable.ScheduleRenderableConstruction();
        foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
        {
            c.ScheduleRenderableConstruction();
        }
    }

    public override void DestroyRenderables()
    {
        _bonePointRenderable.DestroyRenderables();
        foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
        {
            c.DestroyRenderables();
        }
    }

    public override BoundingBox GetBounds()
    {
        return BoundingBox.Transform(GetLocalBounds(), _world);
    }

    public override BoundingBox GetLocalBounds()
    {
        BoundingBox b = _bonePointRenderable.GetLocalBounds();
        foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
        {
            b = BoundingBox.Combine(b, c.GetLocalBounds());
        }

        return b;
    }

    public override void SetSelectable(ISelectable sel)
    {
        _selectable = new WeakReference<ISelectable>(sel);
        _bonePointRenderable.SetSelectable(sel);
        foreach (DebugPrimitiveRenderableProxy c in _boneRenderables)
        {
            c.SetSelectable(sel);
        }
    }

    public override void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline sp)
    {
    }
}
