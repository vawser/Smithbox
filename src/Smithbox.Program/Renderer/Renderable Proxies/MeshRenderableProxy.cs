#nullable enable
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     Render proxy for a single static mesh
/// </summary>
public class MeshRenderableProxy : RenderableProxy, IMeshProviderEventListener
{
    /// <summary>
    ///     Name of the resource this renders, if it came from a pool
    /// </summary>
    public string Name;

    public readonly MeshProvider _meshProvider;

    public readonly ModelMarkerType _placeholderType;
    public readonly MeshRenderables _renderablesSet;

    public readonly List<MeshRenderableProxy> _submeshes = new();

    // Submeshes are created/removed on the resource loader thread while the UI and render
    // threads read them, so every mutation happens under this lock and readers use snapshots.
    private readonly object _submeshLock = new();
    private bool _providerReleased;

    public RenderFilter _drawfilter = RenderFilter.All;

    public DrawGroup _drawgroups = new();
    public ResourceSet? _perObjectResourceSet;
    public Pipeline? _pickingPipeline;

    public Pipeline? _pipeline;

    public int _renderable = -1;

    public bool _renderOutline;

    public WeakReference<ISelectable>? _selectable;
    public Pipeline? _selectedPipeline;
    public int _selectionOutlineRenderable = -1;
    public Shader[]? _shaders;

    public bool _visible = true;

    public Matrix4x4 _world = Matrix4x4.Identity;
    public GPUBufferAllocator.GPUBufferHandle? _worldBuffer;

    private ModelAnimator? _animator;

    private Matrix4x4 _attachTransform = Matrix4x4.Identity;

    private Matrix4x4[]? _instanceTransforms;
    private BoundingBox _instanceBounds;

    // Instance count of _worldBuffer, 0 for the single slot of a non-instanced mesh
    private int _worldBufferInstances;

    public event Action<MeshRenderableProxy>? MeshAvailabilityChanged;

    public MeshRenderableProxy(MeshRenderables renderables, MeshProvider provider, ModelMarkerType placeholderType = ModelMarkerType.None, bool autoregister = true, string name = "", int? submeshIndex = null)
    {
        // Must be known before AddEventListener: an already-loaded provider calls OnProviderAvailable
        // from inside this constructor, and only the requested child should be built then.
        if (submeshIndex.HasValue)
        {
            _requestedSubmeshIndex = submeshIndex;
            _restrictToSingleSubmesh = true;
        }

        Name = string.IsNullOrEmpty(name) ? provider.ResourceHandle?.Name ?? "" : name;
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

        if (clone._restrictToSingleSubmesh && clone._requestedSubmeshIndex.HasValue)
        {
            RequestSubmesh(clone._requestedSubmeshIndex.Value);
        }
    }

    public IReadOnlyList<MeshRenderableProxy> Submeshes => _submeshes;

    public IResourceHandle ResourceHandle => _meshProvider.ResourceHandle;

    public override bool AutoRegister
    {
        get => _autoregister;
        set
        {
            _autoregister = value;
            foreach (MeshRenderableProxy c in SnapshotSubmeshes())
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
            foreach (MeshRenderableProxy sm in SnapshotSubmeshes())
            {
                sm.World = _world;
            }
        }
    }

    /// <summary>
    ///     Drives this mesh from an animator instead of its static pose. Null returns it to the static pose.
    /// </summary>
    public ModelAnimator? Animator
    {
        get => _animator;
        set
        {
            _animator = value;
            ScheduleRenderableUpdate();
            foreach (MeshRenderableProxy sm in SnapshotSubmeshes())
            {
                sm.Animator = value;
            }
        }
    }

    /// <summary>
    ///     Places this mesh within the model it is attached to, such as a weapon on a hand bone.
    ///     This sits between the mesh and <see cref="World" />, so moving the model leaves it in place.
    /// </summary>
    public Matrix4x4 AttachTransform
    {
        get => _attachTransform;
        set
        {
            _attachTransform = value;
            ScheduleRenderableUpdate();
            foreach (MeshRenderableProxy sm in SnapshotSubmeshes())
            {
                sm.AttachTransform = value;
            }
        }
    }

    /// <summary>
    ///     Draws this mesh once per transform with a single instanced draw, such as grass on a model.
    ///     Each transform sits between the mesh and <see cref="World" />. Null draws the mesh once.
    /// </summary>
    public Matrix4x4[]? InstanceTransforms
    {
        get => _instanceTransforms;
        set
        {
            _instanceTransforms = value;

            if (value is { Length: > 0 })
            {
                Vector3 min = value[0].Translation;
                Vector3 max = min;

                foreach (Matrix4x4 transform in value)
                {
                    min = Vector3.Min(min, transform.Translation);
                    max = Vector3.Max(max, transform.Translation);
                }

                _instanceBounds = new BoundingBox(min, max);
            }

            if (_registered)
            {
                ScheduleRenderableConstruction();
            }

            foreach (MeshRenderableProxy sm in SnapshotSubmeshes())
            {
                sm.InstanceTransforms = value;
            }
        }
    }

    public override bool Visible
    {
        get => _visible;
        set
        {
            _visible = value;
            ApplyVisibilityToRenderables();

            foreach (MeshRenderableProxy sm in SnapshotSubmeshes())
            {
                sm.Visible = value;
            }
        }
    }

    /// <summary>
    ///     Pushes the current visibility to the renderables this proxy owns, including the selection outline
    ///     renderable, which otherwise stays visible when the mesh itself is hidden.
    /// </summary>
    private void ApplyVisibilityToRenderables()
    {
        var visible = _visible;

        var renderable = _renderable;
        if (renderable != -1)
        {
            var baseVisible = visible;

            if (CFG.Current.Viewport_Enable_Selection_Outline &&
                !_meshProvider.SelectedRenderBaseMesh && _renderOutline)
            {
                baseVisible = false;
            }

            _renderablesSet.cVisible[renderable]._visible = baseVisible;
        }

        var outlineRenderable = _selectionOutlineRenderable;
        if (outlineRenderable != -1)
        {
            _renderablesSet.cVisible[outlineRenderable]._visible = visible;
        }
    }

    private MeshRenderableProxy[] SnapshotSubmeshes()
    {
        lock (_submeshLock)
        {
            return _submeshes.Count == 0 ? Array.Empty<MeshRenderableProxy>() : _submeshes.ToArray();
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

            foreach (MeshRenderableProxy sm in SnapshotSubmeshes())
            {
                sm.DrawFilter = _drawfilter;
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

            foreach (MeshRenderableProxy sm in SnapshotSubmeshes())
            {
                sm.DrawGroups = _drawgroups;
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

            foreach (MeshRenderableProxy child in SnapshotSubmeshes())
            {
                child.RenderSelectionOutline = value;
            }
        }
    }

    public void OnProviderAvailable()
    {
        // A released (disposed) proxy is still weakly listed by its provider until collected. It must not
        // build children: they would be registered with the scene, show the mesh, and belong to nobody.
        if (_providerReleased)
        {
            return;
        }

        var needsPlaceholder = _placeholderType != ModelMarkerType.None;

        // When a single submesh was requested build only that one, instead of building every child
        // and disposing the rest (which is wasteful and races with their queued construction).
        var onlyIndex = -1;
        if (_restrictToSingleSubmesh && _requestedSubmeshIndex.HasValue &&
            _requestedSubmeshIndex.Value >= 0 && _requestedSubmeshIndex.Value < _meshProvider.ChildCount)
        {
            onlyIndex = _requestedSubmeshIndex.Value;
        }

        _submeshesFiltered = onlyIndex != -1;

        for (var i = 0; i < _meshProvider.ChildCount; i++)
        {
            if (onlyIndex != -1 && i != onlyIndex)
            {
                continue;
            }

            MeshRenderableProxy child = new(_renderablesSet, _meshProvider.GetChildProvider(i),
                ModelMarkerType.None, AutoRegister);
            child.World = _world;
            child.Visible = _visible;
            child.DrawFilter = _drawfilter;
            child.DrawGroups = _drawgroups;
            child.Animator = _animator;
            child.AttachTransform = _attachTransform;
            child.InstanceTransforms = _instanceTransforms;

            // Children are created once the provider has loaded (async), which can be after the
            // outline was requested on this proxy. Without this they never get the selection outline.
            if (_renderOutline)
            {
                child.RenderSelectionOutline = true;
            }

            lock (_submeshLock)
            {
                _submeshes.Add(child);
            }

            ISelectable? sel = null;
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

        if (_restrictToSingleSubmesh)
        {
            ApplySubmeshSelection();
        }

        // Visible may have been changed by another thread while the children were being created (before they
        // were in the list, so the change never reached them). Re-apply the latest value now they are listed.
        var latestVisible = _visible;
        foreach (MeshRenderableProxy child in SnapshotSubmeshes())
        {
            child.Visible = latestVisible;
        }

        if (_meshProvider.HasMeshData())
        {
            ScheduleRenderableConstruction();
        }

        MeshAvailabilityChanged?.Invoke(this);
    }

    public void OnProviderUnavailable()
    {
        if (_providerReleased)
        {
            return;
        }

        if (_meshProvider.IsAtomic())
        {
            MeshRenderableProxy[] old;
            lock (_submeshLock)
            {
                old = _submeshes.ToArray();
                _submeshes.Clear();
            }

            foreach (MeshRenderableProxy curSubmesh in old)
            {
                curSubmesh.UnregisterAndRelease();
            }
        }
    }
    public bool HasRenderableGeometry()
    {
        if (_meshProvider != null && _meshProvider.IsAvailable() && _meshProvider.IndexCount > 0)
            return true;

        foreach (MeshRenderableProxy sm in SnapshotSubmeshes())
        {
            if (sm.HasRenderableGeometry())
                return true;
        }

        return false;
    }

    /// <summary>
    ///     True once all of the geometry of this mesh has been built into renderables, so that it will be drawn.
    /// </summary>
    public bool AreRenderablesConstructed()
    {
        var hasOwnGeometry = _meshProvider != null && _meshProvider.IsAvailable() && _meshProvider.HasMeshData() &&
                             _meshProvider.IndexCount > 0;
        var constructed = hasOwnGeometry && _renderable != -1;

        if (hasOwnGeometry && !constructed)
        {
            return false;
        }

        foreach (MeshRenderableProxy sm in SnapshotSubmeshes())
        {
            if (!sm.HasRenderableGeometry())
            {
                continue;
            }

            if (!sm.AreRenderablesConstructed())
            {
                return false;
            }

            constructed = true;
        }

        return constructed;
    }

    public override BoundingBox GetBounds()
    {
        return BoundingBox.Transform(GetLocalBounds(), _world);
    }

    public override BoundingBox GetFramingBounds()
    {
        return BoundingBox.Transform(GetLocalBounds(), _world);
    }

    public override BoundingBox GetLocalBounds()
    {
        if (_meshProvider.IsAvailable() && _meshProvider.HasMeshData() && _meshProvider.TryLock())
        {
            _meshProvider.Unlock();
            return BoundingBox.Transform(_meshProvider.Bounds, _meshProvider.ObjectTransform * _attachTransform);
        }

        MeshRenderableProxy[] subs = SnapshotSubmeshes();
        BoundingBox b = subs.Length > 0 ? subs[0].GetLocalBounds() : new BoundingBox();
        foreach (MeshRenderableProxy c in subs)
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

        foreach (MeshRenderableProxy c in SnapshotSubmeshes())
        {
            c.Register();
        }

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

        foreach (MeshRenderableProxy c in SnapshotSubmeshes())
        {
            c.UnregisterWithScene();
        }
    }

    public override void UnregisterAndRelease()
    {
        if (_registered)
        {
            UnregisterWithScene();
        }

        foreach (MeshRenderableProxy c in SnapshotSubmeshes())
        {
            c.UnregisterAndRelease();
        }

        // Dispose() runs this twice (override + base); releasing the provider twice under-counts shared providers
        if (_meshProvider != null && !_providerReleased)
        {
            _providerReleased = true;
            _meshProvider.RemoveEventListener(this);
            _meshProvider.Release();
        }

        if (_worldBuffer != null)
        {
            _worldBuffer.Dispose();
            _worldBuffer = null;
        }

        _worldBufferInstances = 0;
    }

    public override unsafe void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
    {
        // If we were unregistered before construction time, don't construct now
        if (!_registered)
        {
            return;
        }

        foreach (MeshRenderableProxy c in SnapshotSubmeshes())
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
        if (!EnsureWorldBuffer())
        {
            return;
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

        if(_renderOutline)
        {
            res = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, _meshProvider.ShaderName + (_meshProvider.UseSelectedShader ? "_selected" : "")).ToTuple();
        }

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
            _worldBufferInstances > 0
                ? SceneRenderer.GrassInstanceAllocator.Value.BackingBuffer
                : SceneRenderer.UniformBufferAllocator.BackingBuffer));

        // Build default pipeline
        GraphicsPipelineDescription pipelineDescription = new();
        pipelineDescription.BlendState = _meshProvider.BlendState;
        pipelineDescription.DepthStencilState = _meshProvider.DepthWrite
            ? DepthStencilStateDescription.DepthOnlyGreaterEqual
            : DepthStencilStateDescription.DepthOnlyGreaterEqualRead;
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
            projViewLayout, mainPerObjectLayout, SceneRenderer.GlobalTexturePool.GetLayout(),
            SceneRenderer.GlobalCubeTexturePool.GetLayout(), SceneRenderer.MaterialBufferAllocator.GetLayout(),
            SamplerSet.SamplersLayout, pickingResultLayout, SceneRenderer.BoneBufferAllocator.GetLayout()
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
        var indexStart = geombuffer.IAllocationStart / (_meshProvider.Is32Bit ? 4u : 2u) +
                         (uint)_meshProvider.IndexOffset;
        meshcomp._indirectArgs.FirstInstance = _worldBuffer.AllocationStart / (uint)sizeof(InstanceData);
        meshcomp._indirectArgs.VertexOffset = (int)(geombuffer.VAllocationStart / _meshProvider.VertexSize);
        meshcomp._indirectArgs.InstanceCount = (uint)Math.Max(_worldBufferInstances, 1);
        meshcomp._indirectArgs.FirstIndex = indexStart;
        meshcomp._indirectArgs.IndexCount = (uint)_meshProvider.IndexCount;

        // Rest of draw parameters
        meshcomp._indexFormat = _meshProvider.Is32Bit ? VkIndexType.Uint32 : VkIndexType.Uint16;
        meshcomp._objectResourceSet = _perObjectResourceSet;
        meshcomp._bufferIndex = geombuffer.BufferIndex;

        // Instantiate renderable
        BoundingBox bounds = GetWorldBounds();
        _renderable = _renderablesSet.CreateMesh(ref bounds, ref meshcomp);
        _renderablesSet.cRenderKeys[_renderable] = GetRenderKey(0.0f);
        _renderablesSet.cBackToFront[_renderable] = _meshProvider.IsTransparent;

        // Pipelines
        _renderablesSet.cPipelines[_renderable] = _pipeline;
        _renderablesSet.cSelectionPipelines[_renderable] = _pickingPipeline;

        // Update instance data
        FillWorldBuffer(gd, cl);

        // Selectable
        _renderablesSet.cSelectables[_renderable] = _selectable;

        // Visible
        _renderablesSet.cVisible[_renderable]._visible = _visible;

        _renderablesSet.cSceneVis[_renderable]._renderFilter = _drawfilter;
        _renderablesSet.cSceneVis[_renderable]._drawGroup = _drawgroups;

        if (_renderOutline && CFG.Current.Viewport_Enable_Selection_Outline)
        {
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyGreaterEqual;
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                _meshProvider.SelectedUseBackface
                    ? _meshProvider.CullMode == VkCullModeFlags.Front ? VkCullModeFlags.Back : VkCullModeFlags.Front
                    : _meshProvider.CullMode,
                _meshProvider.FillMode,
                _meshProvider.FrontFace,
                true,
                false);

            // A mesh that deforms needs its outline to deform with it, which only the flver shader can do
            var outlineShader = _meshProvider.ShaderName.Contains("_deform") ? @"FlverShader\FlverShader_deform_outline" : "SimpleFlver_Outline";
            Tuple<Shader, Shader> s = StaticResourceCache.GetShaders(gd, gd.ResourceFactory, outlineShader).ToTuple();
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

        // The outline renderable is created visible; make it follow the proxy's visibility
        ApplyVisibilityToRenderables();
    }

    public override unsafe void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
    {
        if (!_meshProvider.IsAvailable() || !_meshProvider.HasMeshData())
        {
            _meshProvider.Unlock();
            return;
        }

        if (!EnsureWorldBuffer())
        {
            return;
        }

        FillWorldBuffer(gd, cl);

        if (_renderable != -1)
        {
            _renderablesSet.cBounds[_renderable] = GetWorldBounds();
        }

        if (_selectionOutlineRenderable != -1)
        {
            _renderablesSet.cBounds[_selectionOutlineRenderable] = GetWorldBounds();
        }
    }

    /// <summary>
    ///     Makes sure the world buffer fits the current instances. Instanced meshes use a fixed budget, as
    ///     renderables keep the resource set of the buffer they were built with, so it can't grow under them.
    ///     False if the budget has run out, in which case the mesh isn't drawn.
    /// </summary>
    private unsafe bool EnsureWorldBuffer()
    {
        var instances = _instanceTransforms?.Length ?? 0;

        if (_worldBuffer != null && _worldBufferInstances == instances)
        {
            return true;
        }

        _worldBuffer?.Dispose();

        _worldBuffer = instances > 0
            ? SceneRenderer.GrassInstanceAllocator.Value.TryAllocate((uint)(sizeof(InstanceData) * instances), sizeof(InstanceData))
            : SceneRenderer.UniformBufferAllocator.Allocate((uint)sizeof(InstanceData), sizeof(InstanceData));

        _worldBufferInstances = instances;

        return _worldBuffer != null;
    }

    private void FillWorldBuffer(GraphicsDevice gd, CommandList cl)
    {
        InstanceData dat = CreateInstanceData();

        if (_worldBufferInstances == 0)
        {
            _worldBuffer!.FillBuffer(gd, cl, ref dat);
            return;
        }

        var instances = new InstanceData[_worldBufferInstances];
        Matrix4x4 attachedWorld = _attachTransform * _world;

        for (var i = 0; i < instances.Length; i++)
        {
            instances[i] = dat;
            instances[i].WorldMatrix = _meshProvider.ObjectTransform * _instanceTransforms![i] * attachedWorld;
        }

        _worldBuffer!.FillBuffer(gd, cl, instances);
    }

    private InstanceData CreateInstanceData()
    {
        InstanceData dat = new();
        dat.WorldMatrix = _meshProvider.ObjectTransform * _attachTransform * _world;
        dat.MaterialID = _meshProvider.MaterialIndex;

        if (_meshProvider.BoneBuffer != null)
        {
            dat.BoneStartIndex = _meshProvider.BoneBuffer.AllocationStart / 64;
        }
        else
        {
            dat.BoneStartIndex = 0;
        }

        if (_animator != null && _meshProvider is FlverSubmeshProvider flverMesh &&
            _meshProvider.GeometryBuffer != null)
        {
            var vertexOffset = _meshProvider.GeometryBuffer.VAllocationStart / _meshProvider.VertexSize;

            if (_animator.TryGetMeshState(flverMesh.MeshIndex, vertexOffset, out var boneStart, out var skinReference,
                    out Matrix4x4? transform))
            {
                dat.BoneStartIndex = boneStart;
                dat.SkinReference = skinReference;

                if (transform != null)
                {
                    dat.WorldMatrix = transform.Value * _attachTransform * _world;
                }
            }
        }

        dat.EntityID = GetPackedEntityID(_renderablesSet.RenderableSystemIndex, _renderable);
        return dat;
    }

    private BoundingBox GetWorldBounds()
    {
        if (_instanceTransforms is { Length: > 0 })
        {
            // Instances are scaled up to 200%, and the mesh can be turned any way around its position
            var margin = (_meshProvider.Bounds.Max - _meshProvider.Bounds.Min).Length() * 2f;
            Vector3 extent = new(margin);
            BoundingBox grown = new(_instanceBounds.Min - extent, _instanceBounds.Max + extent);

            return BoundingBox.Transform(grown, _attachTransform * _world);
        }

        BoundingBox bounds = BoundingBox.Transform(_meshProvider.Bounds, _meshProvider.ObjectTransform * _attachTransform * _world);

        if (_animator != null)
        {
            // Culling cannot see the vertex shader, and limbs can swing well outside the rest pose
            Vector3 margin = (bounds.Max - bounds.Min) * 1.5f + Vector3.One;
            bounds = new BoundingBox(bounds.Min - margin, bounds.Max + margin);
        }

        return bounds;
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

        foreach (MeshRenderableProxy c in SnapshotSubmeshes())
        {
            c.DestroyRenderables();
        }
    }

    public override void SetSelectable(ISelectable sel)
    {
        _selectable = new WeakReference<ISelectable>(sel);
        if (_renderable != -1)
        {
            _renderablesSet.cSelectables[_renderable] = _selectable;
        }

        foreach (MeshRenderableProxy child in SnapshotSubmeshes())
        {
            child.SetSelectable(sel);
        }
    }

    public RenderKey GetRenderKey(float distance)
    {
        var code = _pipeline != null ? (ulong)(uint)_pipeline.GetHashCode() : 0;
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

        // Instanced meshes use another resource set, so mixed in with the rest they would split it into
        // a batch at every change, which can be enough to run out of batches
        var instanced = _worldBufferInstances > 0 ? 1ul << 63 : 0;

        return new RenderKey(instanced | (code << 41 & ~(1ul << 63)) | index << 40 |
                             ((ulong)(_renderablesSet.cDrawParameters[_renderable]._bufferIndex & 0xFF) << 32) +
                              cameraDistanceInt);
    }

    // Creating a renderable is separate from finding the resource: find the resource in a ResourcePool (or build a
    // RawMeshProvider from your own data), then pass the handle or provider to one of these.

    /// <summary>
    ///     Renders any mesh source: a provider made from a pool resource (MeshProviderCache) or from raw data (RawMeshProvider).
    /// </summary>
    public static MeshRenderableProxy FromProvider(RenderScene scene, MeshProvider provider,
        ModelMarkerType modelType = ModelMarkerType.None)
    {
        return new MeshRenderableProxy(scene.OpaqueRenderables, provider, modelType);
    }

    /// <summary>
    ///     Renders a model, collision or navmesh resource found with <see cref="ResourcePool.Find{T}" />.
    ///     The resource doesn't have to be loaded yet; the renderable appears when it is.
    /// </summary>
    public static MeshRenderableProxy FromHandle(RenderScene scene, IResourceHandle handle,
        ModelMarkerType modelType = ModelMarkerType.None)
    {
        return FromProvider(scene, MeshProviderCache.GetProvider(handle), modelType);
    }

    public static MeshRenderableProxy MeshRenderableFromFlverResource(RenderScene scene,
        ResourceHandle<FlverResource> handle, ModelMarkerType modelType, string uid, IEnumerable<int>? masks)
    {
        return FromProvider(scene, MeshProviderCache.GetFlverMeshProvider(handle, masks, uid), modelType);
    }

    /// <summary>
    ///     Renders a single submesh of a model. Only that submesh's child proxy is ever created.
    /// </summary>
    public static MeshRenderableProxy MeshRenderableFromFlverSubmesh(RenderScene scene,
        ResourceHandle<FlverResource> handle, ModelMarkerType modelType, string uid, IEnumerable<int>? masks,
        int submeshIndex)
    {
        return new MeshRenderableProxy(scene.OpaqueRenderables,
            MeshProviderCache.GetFlverMeshProvider(handle, masks, uid), modelType, true, "", submeshIndex);
    }

    public static MeshRenderableProxy MeshRenderableFromCollisionResource(RenderScene scene,
        ResourceHandle<HavokCollisionResource> handle, ModelMarkerType modelType)
    {
        return FromProvider(scene, MeshProviderCache.GetCollisionMeshProvider(handle), modelType);
    }

    public static MeshRenderableProxy MeshRenderableFromNVMResource(RenderScene scene,
        ResourceHandle<NVMNavmeshResource> handle, ModelMarkerType modelType)
    {
        return FromProvider(scene, MeshProviderCache.GetNVMMeshProvider(handle), modelType);
    }

    public static MeshRenderableProxy MeshRenderableFromHavokNavmeshResource(RenderScene scene,
        ResourceHandle<HavokNavmeshResource> handle, ModelMarkerType modelType, bool temp = false)
    {
        return FromProvider(scene, MeshProviderCache.GetHavokNavMeshProvider(handle, temp), modelType);
    }


    private int? _requestedSubmeshIndex = null;
    private bool _restrictToSingleSubmesh = false;

    // True when OnProviderAvailable built only the requested child, so _submeshes no longer maps 1:1 to
    // the provider's child indices and there is nothing left to trim.
    private bool _submeshesFiltered = false;

    public void RequestSubmesh(int index)
    {
        _requestedSubmeshIndex = index;
        _restrictToSingleSubmesh = true;

        // If already loaded, apply immediately
        if (_submeshes.Count > 0)
        {
            ApplySubmeshSelection();
        }
    }

    private void ApplySubmeshSelection()
    {
        if (!_requestedSubmeshIndex.HasValue || _submeshesFiltered)
            return;

        int index = _requestedSubmeshIndex.Value;

        List<MeshRenderableProxy> removed = new();

        lock (_submeshLock)
        {
            if (index < 0 || index >= _submeshes.Count)
                return;

            for (int i = _submeshes.Count - 1; i >= 0; i--)
            {
                if (i == index)
                    continue;

                removed.Add(_submeshes[i]);
                _submeshes.RemoveAt(i);
            }
        }

        foreach (MeshRenderableProxy sub in removed)
        {
            sub.Dispose();
        }
    }

    public override void Dispose()
    {
        UnregisterAndRelease();
        base.Dispose();
    }
}
