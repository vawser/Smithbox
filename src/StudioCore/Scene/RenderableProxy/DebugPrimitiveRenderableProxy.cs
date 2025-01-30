#nullable enable
using StudioCore;
using StudioCore.Resource;
using StudioCore.Scene.DebugPrimitives;
using StudioCore.Scene.Enums;
using StudioCore.Scene.Framework;
using StudioCore.Scene.Interfaces;
using StudioCore.Scene.Structs;
using StudioCore.Scene.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Scene.RenderableProxy;

public class DebugPrimitiveRenderableProxy : RenderableProxy
{
    private static DbgPrimSolidBox? _regionSolidBox;
    private static DbgPrimWireBox? _regionBox;
    private static DbgPrimWireCylinder? _regionCylinder;
    private static DbgPrimWireSphere? _regionSphere;
    private static DbgPrimWireSphere? _regionPoint;
    private static DbgPrimWireSphere? _dmyPoint;
    private static DbgPrimWireSphereForwardUp? _dmySphereFwdUp;
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

    public bool _hasColorVariance;
    public Color _highlightedColor = Color.Gray;
    public Color _initialColor = Color.Empty;
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
        pipelineDescription.BlendState = BlendStateDescription.SingleAlphaBlend;
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
        var indexStart = geombuffer.IAllocationStart / (_debugPrimitive.Is32Bit ? 4u : 2u) +
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
        if (_renderable != -1)
        {
            _renderablesSet.cVisible[_renderable]._visible = _visible;
            _renderablesSet.cSceneVis[_renderable]._renderFilter = _drawfilter;
            _renderablesSet.cSceneVis[_renderable]._drawGroup = _drawgroups;
        }
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

        if(_debugPrimitive == null)
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

    /// <summary>
    ///     These are initialized explicitly to ensure these meshes are created at startup time so that they don't share
    ///     vertex buffer memory with dynamically allocated resources and cause the megabuffers to not be freed.
    /// </summary>
    public static void InitializeDebugMeshes()
    {
        _regionSolidBox = new DbgPrimSolidBox(
            Transform.Default, 
            new Vector3(-0.5f, 0.0f, -0.5f),
            new Vector3(0.5f, 1.0f, 0.5f), 
            Color.Blue);

        _regionBox = new DbgPrimWireBox(
            Transform.Default, 
            new Vector3(-0.5f, 0.0f, -0.5f),
            new Vector3(0.5f, 1.0f, 0.5f), 
            Color.Blue);

        _regionCylinder = new DbgPrimWireCylinder(
            Transform.Default, 
            1.0f, 
            1.0f, 
            12, 
            Color.Blue);

        _regionSphere = new DbgPrimWireSphere(
            Transform.Default, 
            1.0f,
            Color.Blue);

        _regionPoint = new DbgPrimWireSphere(
            Transform.Default, 
            1.0f, 
            Color.Yellow, 
            1, 
            4);

        _dmyPoint = new DbgPrimWireSphere(
            Transform.Default, 
            0.05f, 
            Color.Yellow, 
            1, 
            4);

        _dmySphereFwdUp = new DbgPrimWireSphereForwardUp(
            Transform.Default, 
            0.05f, 
            Color.Yellow, 
            Color.Blue,
            Color.White, 
            1, 
            4);

        _jointSphere = new DbgPrimWireSphere(
            Transform.Default, 
            0.05f, 
            Color.Blue, 
            6, 
            6);

        _modelMarkerChr = new DbgPrimWireSpheroidWithArrow(
            Transform.Default, 
            0.9f, 
            Color.Firebrick, 
            4, 
            10, 
            true);

        _modelMarkerObj = new DbgPrimWireWallBox(
            Transform.Default, 
            new Vector3(-1.5f, 0.0f, -0.75f),
            new Vector3(1.5f, 2.5f, 0.75f), 
            Color.Firebrick);

        _modelMarkerPlayer = new DbgPrimWireSpheroidWithArrow(
            Transform.Default, 
            0.75f, 
            Color.Firebrick, 
            1, 
            6, 
            true);

        _modelMarkerOther = new DbgPrimWireWallBox(
            Transform.Default,
            new Vector3(-0.3f, 0.0f, -0.3f),
            new Vector3(0.3f, 1.8f, 0.3f), 
            Color.Firebrick);

        _pointLight = new DbgPrimWireSphere(
            Transform.Default, 
            1.0f, 
            Color.Yellow, 
            6, 
            6);

        _spotLight = new DbgPrimWireSpotLight(
            Transform.Default, 
            1.0f, 
            1.0f, 
            Color.Yellow);

        _directionalLight = new DbgPrimWireSpheroidWithArrow(
            Transform.Default, 
            5.0f, 
            Color.Yellow, 
            4, 
            2, 
            false, 
            true);
    }


    public static DebugPrimitiveRenderableProxy GetSolidBoxRegionProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _regionSolidBox);
        r.BaseColor = ColorHelper.GetAlphaRenderableColor(CFG.Current.GFX_Renderable_Box_BaseColor);
        r.HighlightedColor = ColorHelper.GetAlphaRenderableColor(CFG.Current.GFX_Renderable_Box_HighlightColor);
        //ColorHelper.ApplyColorVariance(r);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetBoxRegionProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _regionBox);
        r.BaseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_Box_BaseColor);
        r.HighlightedColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_Box_HighlightColor);
        ColorHelper.ApplyColorVariance(r);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetCylinderRegionProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _regionCylinder);
        r.BaseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_Cylinder_BaseColor);
        r.HighlightedColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_Cylinder_HighlightColor);
        ColorHelper.ApplyColorVariance(r);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetSphereRegionProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _regionSphere);
        r.BaseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_Sphere_BaseColor);
        r.HighlightedColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_Sphere_HighlightColor);
        ColorHelper.ApplyColorVariance(r);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetPointRegionProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _regionPoint);
        r.BaseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_Point_BaseColor);
        r.HighlightedColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_Point_HighlightColor);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetDummyPolyRegionProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OverlayRenderables, _dmyPoint);
        r.BaseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_DummyPoly_BaseColor);
        r.HighlightedColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_DummyPoly_HighlightColor);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetDummyPolyForwardUpProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _dmySphereFwdUp);
        r.BaseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_DummyPoly_BaseColor);
        r.HighlightedColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_DummyPoly_HighlightColor);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetBonePointProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OverlayRenderables, _jointSphere);
        r.BaseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_BonePoint_BaseColor);
        r.HighlightedColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_BonePoint_HighlightColor);
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
                baseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor);
                selectColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor);
                break;
            case ModelMarkerType.Object:
                prim = _modelMarkerObj;
                baseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor);
                selectColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor);
                break;
            case ModelMarkerType.Player:
                prim = _modelMarkerPlayer;
                baseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor);
                selectColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor);
                break;
            case ModelMarkerType.Other:
            default:
                prim = _modelMarkerOther;
                baseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor);
                selectColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor);
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
        r.BaseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_PointLight_BaseColor);
        r.HighlightedColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_PointLight_HighlightColor);
        ColorHelper.ApplyColorVariance(r);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetSpotLightProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _spotLight);
        r.BaseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_SpotLight_BaseColor);
        r.HighlightedColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_SpotLight_HighlightColor);
        ColorHelper.ApplyColorVariance(r);
        return r;
    }

    public static DebugPrimitiveRenderableProxy GetDirectionalLightProxy(RenderScene scene)
    {
        DebugPrimitiveRenderableProxy r = new(scene.OpaqueRenderables, _directionalLight);
        r.BaseColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_DirectionalLight_BaseColor);
        r.HighlightedColor = ColorHelper.GetRenderableColor(CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor);
        return r;
    }
}
