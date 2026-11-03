#nullable enable
using StudioCore.Editors.Common;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
/// Draws the fog of a fog volume: a box that marches through the fog between the camera and whatever is behind
/// it, read from the scene depth. It is drawn after everything opaque, without depth testing, so the fog is seen
/// in front of walls and from inside the box. It can't be clicked, as fog boxes are often bigger than the map.
/// </summary>
public class FogVolumeRenderableProxy : RenderableProxy, IResourceEventListener
{
    private const int HeightMapTag = 0;
    private const int ColorMapTag = 1;

    private readonly MeshRenderables _renderablesSet;
    private readonly IDbgPrim _box;

    private GPUBufferAllocator.GPUBufferHandle? _worldBuffer;
    private GPUBufferAllocator.GPUBufferHandle? _materialBuffer;
    private Pipeline? _pipeline;

    private int _renderable = -1;
    private bool _visible = true;
    private RenderFilter _drawfilter = RenderFilter.FogVolume;
    private DrawGroup _drawgroups = new();
    private Matrix4x4 _world = Matrix4x4.Identity;
    private WeakReference<ISelectable>? _selectable;

    private FogVolumeSettings _settings = FogVolumeSettings.Default;
    private readonly ResourceHandle<TextureResource>?[] _textures = new ResourceHandle<TextureResource>?[2];
    private string? _heightMapName;
    private string? _colorMapName;

    public FogVolumeRenderableProxy(MeshRenderables renderables, IDbgPrim box)
    {
        _renderablesSet = renderables;
        _box = box;

        ScheduleRenderableConstruction();
        AutoRegister = true;
        _registered = true;
    }

    /// <summary>
    /// The GPARAM settings the fog is drawn with
    /// </summary>
    public FogVolumeSettings Settings
    {
        get => _settings;
        set
        {
            _settings = value ?? FogVolumeSettings.Default;
            ScheduleRenderableUpdate();
        }
    }

    /// <summary>
    /// Sets the textures of the fog by name, e.g. AET822_009_1m. They are used once they are loaded.
    /// </summary>
    public void SetTextures(string? heightMap, string? colorMap)
    {
        if (!string.Equals(heightMap, _heightMapName, StringComparison.OrdinalIgnoreCase))
        {
            _heightMapName = heightMap;
            ReleaseTexture(HeightMapTag);
            Listen(heightMap, HeightMapTag);
        }

        if (!string.Equals(colorMap, _colorMapName, StringComparison.OrdinalIgnoreCase))
        {
            _colorMapName = colorMap;
            ReleaseTexture(ColorMapTag);
            Listen(colorMap, ColorMapTag);
        }

        ScheduleRenderableUpdate();
    }

    private void Listen(string? name, int tag)
    {
        if (string.IsNullOrEmpty(name))
            return;

        ResourceManager.DefaultPool.AddListener<TextureResource>(name, this, AccessLevel.AccessGPUOptimizedOnly, tag);
    }

    private void ReleaseTexture(int tag)
    {
        _textures[tag]?.Release();
        _textures[tag] = null;
    }

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        var name = tag == HeightMapTag ? _heightMapName : _colorMapName;
        var texHandle = (ResourceHandle<TextureResource>)handle;

        // The texture may have been changed since it was asked for
        if (name == null || ResourceName.Normalize(name) != ResourceName.Normalize(texHandle.Name ?? ""))
        {
            return;
        }

        texHandle.Acquire();
        _textures[tag]?.Release();
        _textures[tag] = texHandle;
        ScheduleRenderableUpdate();
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        ReleaseTexture(tag);
        ScheduleRenderableUpdate();
    }

    private uint GetTextureIndex(int tag)
    {
        var handle = _textures[tag];

        if (handle == null || !handle.IsLoaded)
            return 0;

        return handle.Get()?.GPUTexture?.TexHandle ?? 0;
    }

    public override bool RenderSelectionOutline { get; set; }

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

    public override BoundingBox GetBounds()
    {
        return BoundingBox.Transform(_box.Bounds, _world);
    }

    public override BoundingBox GetLocalBounds()
    {
        return _box.Bounds;
    }

    public override BoundingBox GetFramingBounds()
    {
        return BoundingBox.Transform(new BoundingBox(new Vector3(-0.5f), new Vector3(0.5f)), _world);
    }

    public override void SetSelectable(ISelectable sel)
    {
        _selectable = new WeakReference<ISelectable>(sel);
        if (_renderable != -1)
        {
            _renderablesSet.cSelectables[_renderable] = _selectable;
        }
    }

    public override void UnregisterAndRelease()
    {
        if (_registered)
        {
            UnregisterWithScene();
        }

        ReleaseTexture(HeightMapTag);
        ReleaseTexture(ColorMapTag);
        _heightMapName = null;
        _colorMapName = null;

        _worldBuffer?.Dispose();
        _worldBuffer = null;

        _materialBuffer?.Dispose();
        _materialBuffer = null;
    }

    public override unsafe void ConstructRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
    {
        // If we were unregistered before construction time, don't construct now
        if (!_registered)
            return;

        if (_renderable != -1)
        {
            _renderablesSet.RemoveRenderable(_renderable);
            _renderable = -1;
        }

        if (_box.GeometryBuffer == null)
            return;

        if (_box.GeometryBuffer.AllocStatus != VertexIndexBufferAllocator.VertexIndexBuffer.Status.Resident)
        {
            ScheduleRenderableConstruction();
            return;
        }

        ResourceFactory factory = gd.ResourceFactory;
        _worldBuffer ??= SceneRenderer.TryAllocateInstance();

        if (_worldBuffer == null)
            return;

        _materialBuffer ??= SceneRenderer.MaterialBufferAllocator.Allocate((uint)sizeof(FogVolumeMaterial), sizeof(FogVolumeMaterial));

        if (_pipeline == null)
        {
            ResourceLayout sceneParamLayout = StaticResourceCache.GetResourceLayout(factory, StaticResourceCache.SceneParamLayoutDescription);
            ResourceLayout pickingResultLayout = StaticResourceCache.GetResourceLayout(factory, StaticResourceCache.PickingResultDescription);
            ResourceLayout perObjectLayout = StaticResourceCache.GetResourceLayout(
                factory,
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription(
                        "WorldBuffer",
                        VkDescriptorType.StorageBuffer,
                        VkShaderStageFlags.Vertex | VkShaderStageFlags.Fragment,
                        VkDescriptorBindingFlags.None)));

            Tuple<Shader, Shader> shaders = StaticResourceCache.GetShaders(gd, factory, _box.ShaderName).ToTuple();

            GraphicsPipelineDescription pipelineDescription = new();
            pipelineDescription.BlendState = SceneTarget.BlendFor(_renderablesSet, BlendStateDescription.SingleAlphaBlend);

            // The shader finds what is in front of the fog from the scene depth itself
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.Disabled;

            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                VkCullModeFlags.None,
                VkPolygonMode.Fill,
                _box.FrontFace,
                false,
                false);

            pipelineDescription.PrimitiveTopology = _box.Topology;
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                new[] { _box.LayoutDescription },
                new[] { shaders.Item1, shaders.Item2 },
                SceneTarget.Specialize(_box.SpecializationConstants, _renderablesSet));

            pipelineDescription.ResourceLayouts = new[]
            {
                sceneParamLayout, perObjectLayout, SceneRenderer.GlobalTexturePool.GetLayout(),
                SceneRenderer.GlobalCubeTexturePool.GetLayout(), SceneRenderer.MaterialBufferAllocator.GetLayout(),
                SamplerSet.SamplersLayout, pickingResultLayout, SceneRenderer.BoneBufferAllocator.GetLayout()
            };

            pipelineDescription.Outputs = SceneTarget.OutputsFor(gd, _renderablesSet);
            _pipeline = StaticResourceCache.GetPipeline(factory, ref pipelineDescription);
        }

        ResourceLayout mainPerObjectLayout = StaticResourceCache.GetResourceLayout(
            factory,
            new ResourceLayoutDescription(
                new ResourceLayoutElementDescription(
                    "WorldBuffer",
                    VkDescriptorType.StorageBuffer,
                    VkShaderStageFlags.Vertex | VkShaderStageFlags.Fragment,
                    VkDescriptorBindingFlags.None)));

        // Create draw call arguments
        MeshDrawParametersComponent meshcomp = new();
        VertexIndexBufferAllocator.VertexIndexBufferHandle geombuffer = _box.GeometryBuffer;
        meshcomp._indirectArgs.FirstInstance = _worldBuffer.AllocationStart / (uint)sizeof(InstanceData);
        meshcomp._indirectArgs.VertexOffset = (int)(geombuffer.VAllocationStart / _box.VertexSize);
        meshcomp._indirectArgs.InstanceCount = 1;
        meshcomp._indirectArgs.FirstIndex = geombuffer.IAllocationStart / (_box.Is32Bit ? 4u : 2u) + (uint)_box.IndexOffset;
        meshcomp._indirectArgs.IndexCount = geombuffer.IAllocationSize / (_box.Is32Bit ? 4u : 2u);
        meshcomp._indexFormat = _box.Is32Bit ? VkIndexType.Uint32 : VkIndexType.Uint16;
        meshcomp._objectBuffer = SceneRenderer.UniformBufferAllocator;
        meshcomp._objectLayout = mainPerObjectLayout;
        meshcomp._bufferIndex = geombuffer.BufferIndex;

        BoundingBox bounds = GetBounds();
        _renderable = _renderablesSet.CreateMesh(ref bounds, ref meshcomp);
        _renderablesSet.cRenderKeys[_renderable] = new RenderKey(RenderKey.BackToFrontStart);
        _renderablesSet.cBackToFront[_renderable] = true;

        // Picking frames draw it the same way, so it never writes to the picking buffer
        _renderablesSet.cPipelines[_renderable] = _pipeline;
        _renderablesSet.cSelectionPipelines[_renderable] = _pipeline;
        _renderablesSet.cSelectables[_renderable] = _selectable;

        FillBuffers(gd, cl);

        _renderablesSet.cVisible[_renderable]._visible = _visible;
        _renderablesSet.cSceneVis[_renderable]._renderFilter = _drawfilter;
        _renderablesSet.cSceneVis[_renderable]._drawGroup = _drawgroups;
    }

    public override void UpdateRenderables(GraphicsDevice gd, CommandList cl, SceneRenderPipeline? sp)
    {
        if (_worldBuffer == null || _materialBuffer == null)
            return;

        FillBuffers(gd, cl);

        if (_renderable != -1)
        {
            _renderablesSet.cBounds[_renderable] = GetBounds();
        }
    }

    private unsafe void FillBuffers(GraphicsDevice gd, CommandList cl)
    {
        if (_worldBuffer == null || _materialBuffer == null)
            return;

        InstanceData dat = new();
        dat.WorldMatrix = _world;
        dat.MaterialID = _materialBuffer.AllocationStart / (uint)sizeof(FogVolumeMaterial);
        dat.EntityID = _renderable == -1 ? 0 : GetPackedEntityID(_renderablesSet.RenderableSystemIndex, _renderable);
        _worldBuffer.FillBuffer(gd, cl, ref dat);

        FogVolumeMaterial mat = BuildMaterial();
        _materialBuffer.FillBuffer(gd, cl, ref mat);
    }

    private FogVolumeMaterial BuildMaterial()
    {
        var s = _settings;

        Matrix4x4.Decompose(_world, out Vector3 size, out _, out _);

        if (!Matrix4x4.Invert(_world, out Matrix4x4 inverse))
        {
            inverse = Matrix4x4.Identity;
        }

        return new FogVolumeMaterial
        {
            InverseWorld = inverse,
            SizeDensity = new Vector4(size, s.DensityIntensity),
            Fade = new Vector4(s.NearFadeMinDistance, s.NearFadeMaxDistance, s.FarFadeMinDistance, s.FarFadeMaxDistance),
            Shape = new Vector4(s.EdgeFadeDistance, s.EdgeFadeExponent, s.HeightFallOff, s.FadeDistanceForHeightMap),
            Noise = new Vector4(s.PerlinNoiseFrequency, s.PerlinNoiseLacunarity, s.PerlinNoiseGain, s.PerlinNoiseOffset),
            Scroll = new Vector4(s.ScrollSpeed, s.ScrollDetailScale),
            DirectionalLightColor = s.DirectionalLightColor,
            DirectionalLightOppositeColor = s.DirectionalLightOppositeColor,
            AmbientColor = s.AmbientColor,
            Light = new Vector4(s.DirectionalLightAngle, s.LightOverwrite ? 1.0f : 0.0f, s.ColorMapTextureIntensity),
            ColorMap = new Vector4(s.ColorMapTextureUVTiling, s.AttenuationIntensity, s.AttenuationOffset),
            HeightMapTexture = GetTextureIndex(HeightMapTag),
            ColorMapTexture = GetTextureIndex(ColorMapTag),
            Octaves = s.OctaveNumPerlinNoise,
            MaxSampleCount = s.MaxSampleCount,
            Misc = new Vector4(s.MaxInjectingDepth, s.SteppingDistance, s.SuspendedTransmittance, 0.0f)
        };
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
