#nullable enable
using StudioCore;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Resource;
using StudioCore.Scene.Enums;
using StudioCore.Scene.Helpers;
using StudioCore.Scene.Interfaces;
using StudioCore.Scene.Meshes;
using StudioCore.Scene.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Scene.Framework;

/// <summary>
///     Render proxy for a single static mesh
/// </summary>
public class MeshRenderableProxy : RenderableProxy, IMeshProviderEventListener
{
    public string VirtPath;

    public readonly MeshProvider _meshProvider;

    public readonly ModelMarkerType _placeholderType;
    public readonly MeshRenderables _renderablesSet;

    public readonly List<MeshRenderableProxy> _submeshes = new();

    public RenderFilter _drawfilter = RenderFilter.All;

    public DrawGroup _drawgroups = new();
    public ResourceSet? _perObjectResourceSet;
    public Pipeline? _pickingPipeline;

    public Pipeline? _pipeline;
    public RenderableProxy? _placeholderProxy;

    public int _renderable = -1;

    public bool _renderOutline;

    public WeakReference<ISelectable>? _selectable;
    public Pipeline? _selectedPipeline;
    public int _selectionOutlineRenderable = -1;
    public Shader[]? _shaders;

    public bool _visible = true;

    public Matrix4x4 _world = Matrix4x4.Identity;
    public GPUBufferAllocator.GPUBufferHandle? _worldBuffer;

    public MeshRenderableProxy(MeshRenderables renderables, MeshProvider provider, ModelMarkerType placeholderType = ModelMarkerType.None, bool autoregister = true, string virtPath = "")
    {
        VirtPath = virtPath;
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
        var useTreePlaceholder = false;
        var useBushPlaceholder = false;

        for (var i = 0; i < _meshProvider.ChildCount; i++)
        {
            MeshRenderableProxy child = new(_renderablesSet, _meshProvider.GetChildProvider(i),
                ModelMarkerType.None, AutoRegister);
            child.World = _world;
            child.Visible = _visible;
            child.DrawFilter = _drawfilter;
            child.DrawGroups = _drawgroups;
            _submeshes.Add(child);
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
            // Speed Tree asset
            var speedTreeType = RenderableHelper.IsSpeedTreeAsset(_meshProvider);
            if (speedTreeType != RenderableHelper.SpeedTreeType.None)
            {
                needsPlaceholder = true;
                if (speedTreeType == RenderableHelper.SpeedTreeType.Tree)
                    useTreePlaceholder = true;
                else
                    useBushPlaceholder = true;
            }

            if (needsPlaceholder)
            {
                _placeholderProxy =
                    RenderableHelper.GetModelMarkerProxy(_renderablesSet, _placeholderType);

                if(useTreePlaceholder)
                {
                    _placeholderProxy =
                    RenderableHelper.GetTreeProxy(_renderablesSet);
                    _placeholderProxy.DrawFilter = RenderFilter.SpeedTree;
                }
                else if (useBushPlaceholder)
                {
                    _placeholderProxy =
                        RenderableHelper.GetBushProxy(_renderablesSet);
                    _placeholderProxy.DrawFilter = RenderFilter.SpeedTree;
                }
                else
                {
                    _placeholderProxy.DrawFilter = _drawfilter;
                }

                _placeholderProxy.World = World;
                _placeholderProxy.Visible = Visible;
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

    public override BoundingBox GetFramingBounds()
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

    public override unsafe void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
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
        var indexStart = geombuffer.IAllocationStart / (_meshProvider.Is32Bit ? 4u : 2u) +
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
        if (_renderOutline && CFG.Current.Viewport_Enable_Selection_Outline)
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

    public override unsafe void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
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

        return new RenderKey(code << 41 | index << 40 |
                             ((ulong)(_renderablesSet.cDrawParameters[_renderable]._bufferIndex & 0xFF) << 32) +
                              cameraDistanceInt);
    }

    public static MeshRenderableProxy MeshRenderableFromFlverResource(RenderScene scene, string virtualPath, ModelMarkerType modelType, IEnumerable<int>? masks)
    {
        var meshProvider = MeshProviderCache.GetFlverMeshProvider(virtualPath, masks);

        MeshRenderableProxy renderable = new(scene.OpaqueRenderables, meshProvider, modelType);

        return renderable;
    }

    public static MeshRenderableProxy MeshRenderableFromCollisionResource(RenderScene scene, string virtualPath, ModelMarkerType modelType, string virtPath = "")
    {
        var meshProvider = MeshProviderCache.GetCollisionMeshProvider(virtualPath);

        MeshRenderableProxy renderable = new(scene.OpaqueRenderables, meshProvider, modelType, true, virtPath);

        return renderable;
    }

    public static MeshRenderableProxy MeshRenderableFromNVMResource(RenderScene scene, string virtualPath, ModelMarkerType modelType)
    {
        var meshProvider = MeshProviderCache.GetNVMMeshProvider(virtualPath);

        MeshRenderableProxy renderable = new(scene.OpaqueRenderables, meshProvider, modelType);

        return renderable;
    }

    public static MeshRenderableProxy MeshRenderableFromHavokNavmeshResource(RenderScene scene, string virtualPath, ModelMarkerType modelType, bool temp = false)
    {
        var meshProvider = MeshProviderCache.GetHavokNavMeshProvider(virtualPath, temp);

        MeshRenderableProxy renderable = new(scene.OpaqueRenderables, meshProvider, modelType);

        return renderable;
    }
}
