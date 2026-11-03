using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Renderer;

/// <summary>
///     The "renderer" for a scene. This pipeline is instantiated for every real or virtual viewport, and will
///     render a scene into an internally maintained framebuffer.
/// </summary>
public class SceneRenderPipeline
{
    private readonly SceneRenderer.RenderQueue _overlayQueue;
    private readonly SceneRenderer.RenderQueue _renderQueue;
    private readonly RenderScene Scene;

    // Scene Parameters
    public DeviceBuffer SceneParamBuffer { get; }
    public ResourceSet SceneParamResourceSet { get; }

    public SceneParam SceneParams;

    // Picking Results
    public DeviceBuffer PickingResultsBuffer { get; }
    public ResourceSet PickingResultResourceSet { get; }
    public DeviceBuffer PickingResultReadbackBuffer { get; }
    public bool PickingResultsReady { get; set; }
    private int _pickingEntity { get; set; }

    private bool _pickingEnabled;

    public PickingResult PickingResult;

    public Vector3 Eye { get; private set; }

    public float CPURenderTime => _renderQueue.CPURenderTime;

    /// <summary>
    ///     Number of renderables submitted by the last <see cref="RenderScene" />, up until the queue is executed.
    /// </summary>
    public int QueuedDrawCount => _renderQueue.Count;

    public uint EnvMapTexture = 0;

    /// <summary>
    ///     Whether models are lit by the irradiance volumes of <see cref="SceneRenderer.IrradianceVolumes" />. Only the
    ///     Map Editor's viewports are, as the table holds the volumes of its loaded maps.
    /// </summary>
    public bool UseIrradianceVolumes = false;

    /// <summary>
    ///     Whether the sun and the exposure come from <see cref="SceneRenderer.MapLighting" /> instead of the viewport's
    ///     own settings. Only the Map Editor's viewports do, as it holds the lighting of their loaded maps.
    /// </summary>
    public bool UseMapLighting = false;

    /// <summary>The map lighting this pipeline is lit with this frame, or null if it uses its own settings</summary>
    public GparamLighting ActiveMapLighting => UseMapLighting && CFG.Current.Viewport_Map_Lighting
        ? SceneRenderer.MapLighting
        : null;

    /// <summary>
    ///     The viewport's background with map lighting: the GPARAM's BG color, which is in the game's units like its
    ///     lights, so it takes their exposure and the shaders' gamma (a square root) to reach the screen. Null to keep
    ///     the background color of the settings.
    /// </summary>
    public Vector4? MapBackgroundColor
    {
        get
        {
            var lighting = ActiveMapLighting;

            if (lighting == null || !CFG.Current.Viewport_Map_Lighting_Background)
                return null;

            var bg = lighting.BackgroundColor;
            var exposure = lighting.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale;
            var linear = Vector3.Max(new Vector3(bg.X, bg.Y, bg.Z) * bg.W * exposure, Vector3.Zero);
            var display = Vector3.Min(Vector3.SquareRoot(linear), Vector3.One);

            return new Vector4(display, 1.0f);
        }
    }

    /// <summary>
    ///     A light that follows the camera, handy for looking at dark interiors. Off by default.
    ///     Its position is set every frame; the rest can be changed freely.
    /// </summary>
    public SceneLight CameraLight { get; } = new()
    {
        Type = SceneLightType.Point,
        Enabled = false,
        Intensity = 8.0f,
        Range = 25.0f
    };

    public unsafe SceneRenderPipeline(RenderScene scene, GraphicsDevice device, int width, int height)
    {
        Scene = scene;

        ResourceFactory factory = device.ResourceFactory;

        // Setup scene param uniform buffer.
        SceneParamBuffer = factory.CreateBuffer(
            new BufferDescription( (uint)sizeof(SceneParam), VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0)
        );

        SceneParams = new SceneParam();
        SceneParams.Projection = ViewportUtils.CreatePerspective(device, true, CFG.Current.Viewport_Camera_FOV * (float)Math.PI / 180.0f, width / (float)height, 0.1f, 2000.0f);
        SceneParams.View = Matrix4x4.CreateLookAt(new Vector3(0.0f, 2.0f, 0.0f), new Vector3(1.0f, 2.0f, 0.0f), Vector3.UnitY);
        SceneParams.EyePosition = new Vector4(0.0f, 2.0f, 0.0f, 0.0f);
        SceneParams.LightDirection = new Vector4(1.0f, -0.5f, 0.0f, 0.0f);
        SceneParams.EnvMap = EnvMapTexture;
        SceneParams.CursorPosition[0] = 0;
        SceneParams.CursorPosition[1] = 0;
        SceneParams.CursorPosition[2] = 0;
        SceneParams.CursorPosition[3] = 0;
        SceneParams.AmbientLightMult = 1.0f;
        SceneParams.DirectLightMult = 1.0f;
        SceneParams.IndirectLightMult = 1.0f;
        SceneParams.EmissiveMapMult = 1.0f;
        SceneParams.SceneBrightness = 1.0f;
        SceneParams.SimpleFlver_Brightness = 1.0f;
        SceneParams.SimpleFlver_Saturation = 0.5f;
        SceneParams.SelectionColor = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
        SceneParams.OutlineColor = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
        SceneParams.EnableDithering = 1;
        SceneParams.EnableTinting = 1;
        SceneParams.DitherOpacity = 0.5f;
        SceneParams.SunColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        SceneParams.SunSpecColor = SceneParams.SunColor;
        SceneParams.LightCount = 0;
        SceneParams.CollisionOpacity = 1.0f;

        device.UpdateBuffer(SceneParamBuffer, 0, ref SceneParams, (uint)sizeof(SceneParam));

        ResourceLayout sceneParamLayout = StaticResourceCache.GetResourceLayout(device.ResourceFactory,StaticResourceCache.SceneParamLayoutDescription);

        SceneParamResourceSet = StaticResourceCache.GetResourceSet(device.ResourceFactory, new ResourceSetDescription(sceneParamLayout, SceneParamBuffer));

        // Setup picking uniform buffer
        PickingResultsBuffer = factory.CreateBuffer(
            new BufferDescription(
                (uint)sizeof(PickingResult),
                VkBufferUsageFlags.StorageBuffer | VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                0,
                (uint)sizeof(PickingResult)
            ));

        PickingResult = new PickingResult();
        PickingResult.depth = 0; // int.MaxValue;
        PickingResult.entityID = ulong.MaxValue;
        device.UpdateBuffer(PickingResultsBuffer, 0, ref PickingResult, (uint)sizeof(PickingResult));
        ResourceLayout pickingResultLayout = StaticResourceCache.GetResourceLayout(
            device.ResourceFactory,
            StaticResourceCache.PickingResultDescription);
        PickingResultResourceSet = StaticResourceCache.GetResourceSet(device.ResourceFactory,
            new ResourceSetDescription(pickingResultLayout,
                PickingResultsBuffer));

        PickingResultReadbackBuffer = factory.CreateBuffer(
            new BufferDescription(
                (uint)sizeof(PickingResult),
                VkBufferUsageFlags.TransferDst,
                VmaMemoryUsage.Auto,
                VmaAllocationCreateFlags.Mapped));
        device.UpdateBuffer(PickingResultReadbackBuffer, 0, ref PickingResult, (uint)sizeof(PickingResult));

        _renderQueue = new SceneRenderer.RenderQueue("Viewport Render", device, this) { CapturesSceneDepth = true };
        SceneRenderer.RegisterRenderQueue(_renderQueue);

        _overlayQueue = new SceneRenderer.RenderQueue("Overlay Render", device, this);
        SceneRenderer.RegisterRenderQueue(_overlayQueue);
    }

    /// <summary>
    ///     Renders this pipeline to its own framebuffer instead of the swapchain.
    /// </summary>
    public void SetOffscreen()
    {
        _renderQueue.IsOffscreen = true;
        _overlayQueue.IsOffscreen = true;

        // Scene depth comes from the swapchain, which has nothing to do with what is drawn here
        _renderQueue.CapturesSceneDepth = false;
    }

    public void SetViewportSetupAction(Action<GraphicsDevice, CommandList> action)
    {
        _renderQueue.SetPredrawSetupAction(action);
    }

    public void SetOverlayViewportSetupAction(Action<GraphicsDevice, CommandList> action)
    {
        _overlayQueue.SetPredrawSetupAction(action);
    }

    public unsafe void UpdateSceneParameters(Matrix4x4 proj, Matrix4x4 view, Vector3 eye, int cursorx, int cursory)
    {
        var selectionColor = new Vector4(
            CFG.Current.Viewport_Selection_Tint_Color.X, 
            CFG.Current.Viewport_Selection_Tint_Color.Y, 
            CFG.Current.Viewport_Selection_Tint_Color.Z, 
            CFG.Current.Viewport_Selection_Tint_Strength);

        if(!CFG.Current.Viewport_Enable_Texturing)
        {
            selectionColor = new Vector4(
            CFG.Current.Viewport_Untextured_Selection_Tint_Color.X,
            CFG.Current.Viewport_Untextured_Selection_Tint_Color.Y,
            CFG.Current.Viewport_Untextured_Selection_Tint_Color.Z,
            CFG.Current.Viewport_Selection_Tint_Strength);
        }

        var outlineColor = new Vector4(
            CFG.Current.Viewport_Selection_Outline_Color.X,
            CFG.Current.Viewport_Selection_Outline_Color.Y,
            CFG.Current.Viewport_Selection_Outline_Color.Z,
            1.0f);

        uint enableDithering = 1;
        if (!CFG.Current.Viewport_Enable_Selection_Dithering)
            enableDithering = 0;

        uint enableTinting = 1;
        if (!CFG.Current.Viewport_Enable_Selection_Tint)
            enableTinting = 0;

        Eye = eye;

        // Lights are gathered here, where the scene can't change under us, and copied in below
        List<SceneLightData> lights = new(SceneLightList.MaxLights);
        if (CameraLight.Enabled)
        {
            CameraLight.Position = eye;
            lights.Add(CameraLight.ToGpu());
        }

        Scene.Lights.CollectEnabled(lights);

        var materialTime = MaterialClock.Time;
        Vector4 wind = MaterialClock.Wind;
        var sceneDepth = _renderQueue.CapturesSceneDepth ? SceneDepth.TextureIndex : 0;

        CollisionTransparency.Refresh();
        var collisionOpacity = Math.Clamp(CFG.Current.Viewport_Collision_Opacity, 0.0f, 1.0f);
        var navVolumeSliceHeight = NavVolumeSlice.Height;
        var navVolumeSliceFlags = NavVolumeSlice.Flags;
        var useIrradianceVolumes = UseIrradianceVolumes && CFG.Current.Viewport_GI_Enable;
        var irradianceExposure = CFG.Current.Viewport_GI_Intensity *
            (CFG.Current.Viewport_GI_Auto_Exposure ? SceneRenderer.IrradianceVolumes.AutoExposure : 1.0f);
        var irradianceFlags = CFG.Current.Viewport_GI_Normalize_Reflections ? 1u : 0u;

        // The map's GPARAM lighting is in the game's units, as the volumes' light is, so both take the GPARAM's
        // exposure instead of the volumes' auto exposure. The viewport's own sun settings are left as they are,
        // so they come back when map lighting is switched off.
        var mapLighting = ActiveMapLighting;
        var mapLightDirection = Vector4.Zero;
        var mapSunColor = Vector4.Zero;
        var mapSunSpecColor = Vector4.Zero;
        var mapHemiUp = Vector4.Zero;
        var mapHemiDown = Vector4.Zero;
        var mapEnvMapScale = Vector4.One;
        var mapEnvMapSaturate = 1.0f;
        var mapLightingFlags = 0u;

        if (mapLighting != null)
        {
            var exposure = mapLighting.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale;
            var sun = mapLighting.Sun;

            mapLightDirection = new Vector4(sun.Direction, 0.0f);
            mapSunColor = new Vector4(sun.Diffuse.X, sun.Diffuse.Y, sun.Diffuse.Z, sun.Diffuse.W * exposure);
            mapSunSpecColor = new Vector4(sun.Specular.X, sun.Specular.Y, sun.Specular.Z, sun.Specular.W * exposure);
            irradianceExposure = CFG.Current.Viewport_GI_Intensity * exposure;

            // The shader takes these as plain rgb, so the intensity and exposure are folded in here
            mapHemiUp = new Vector4(new Vector3(mapLighting.HemiUp.X, mapLighting.HemiUp.Y, mapLighting.HemiUp.Z) * mapLighting.HemiUp.W * exposure, 1.0f);
            mapHemiDown = new Vector4(new Vector3(mapLighting.HemiDown.X, mapLighting.HemiDown.Y, mapLighting.HemiDown.Z) * mapLighting.HemiDown.W * exposure, 1.0f);
            mapEnvMapScale = new Vector4(new Vector3(mapLighting.EnvMapScale.X, mapLighting.EnvMapScale.Y, mapLighting.EnvMapScale.Z) * mapLighting.EnvMapScale.W, 1.0f);
            mapEnvMapSaturate = mapLighting.EnvMapSaturate;

            mapLightingFlags = 1u | (CFG.Current.Viewport_Map_Lighting_Hemi_Inside_Volumes ? 2u : 0u);
        }

        SceneRenderer.AddBackgroundUploadTask((d, cl) =>
        {
            SceneParams.Projection = proj;
            SceneParams.View = view;
            SceneParams.EyePosition = new Vector4(eye, 0.0f);
            SceneParams.EnvMap = EnvMapTexture;
            SceneParams.CursorPosition[0] = cursorx;
            SceneParams.CursorPosition[1] = cursory;

            SceneParams.SimpleFlver_Brightness = CFG.Current.Viewport_Flat_Model_Brightness;
            SceneParams.SimpleFlver_Saturation = CFG.Current.Viewport_Flat_Model_Saturation;
            SceneParams.SelectionColor = selectionColor;
            SceneParams.OutlineColor = outlineColor;
            SceneParams.EnableDithering = enableDithering;
            SceneParams.EnableTinting = enableTinting;
            SceneParams.DitherOpacity = CFG.Current.Viewport_Selection_Dither_Opacity;
            SceneParams.MaterialTime = materialTime;
            SceneParams.WindPeriod = MaterialClock.WindPeriod;
            SceneParams.MaterialFeatures = (uint)MaterialClock.Features;
            SceneParams.SceneDepthTexture = sceneDepth;
            SceneParams.Wind = wind;
            SceneParams.CollisionOpacity = collisionOpacity;
            SceneParams.NavVolumeSliceHeight = navVolumeSliceHeight;
            SceneParams.NavVolumeSliceFlags = navVolumeSliceFlags;

            // Read here, on the upload, so the count only covers entries whose data is already on the GPU
            SceneParams.IrradianceVolumeStart = SceneRenderer.IrradianceVolumes.Start;
            SceneParams.IrradianceVolumeCount = useIrradianceVolumes ? SceneRenderer.IrradianceVolumes.Count : 0;
            SceneParams.IrradianceVolumeExposure = irradianceExposure;
            SceneParams.IrradianceVolumeFlags = irradianceFlags;

            SceneParams.LightCount = (uint)lights.Count;
            fixed (float* destination = SceneParams.Lights)
            {
                var gpuLights = (SceneLightData*)destination;
                for (var i = 0; i < lights.Count; i++)
                {
                    gpuLights[i] = lights[i];
                }
            }

            SceneParams.MapLightingFlags = mapLightingFlags;
            SceneParams.SunSpecColor = SceneParams.SunColor;
            SceneParams.HemiUp = mapHemiUp;
            SceneParams.HemiDown = mapHemiDown;
            SceneParams.EnvMapScale = mapEnvMapScale;
            SceneParams.EnvMapSaturate = mapEnvMapSaturate;

            if (mapLighting == null)
            {
                cl.UpdateBuffer(SceneParamBuffer, 0, ref SceneParams, (uint)sizeof(SceneParam));
                return;
            }

            // A copy, so the viewport's own sun stays in SceneParams for its sliders
            var uploaded = SceneParams;
            uploaded.LightDirection = mapLightDirection;
            uploaded.SunColor = mapSunColor;
            uploaded.SunSpecColor = mapSunSpecColor;

            cl.UpdateBuffer(SceneParamBuffer, 0, ref uploaded, (uint)sizeof(SceneParam));
        });
    }

    public void BindResources(CommandList cl)
    {
        cl.SetGraphicsResourceSet(0, SceneParamResourceSet);
        cl.SetGraphicsResourceSet(6, PickingResultResourceSet);
    }

    public void RenderScene(BoundingFrustum frustum)
    {
        Scene.Render(_renderQueue, _overlayQueue, frustum, this);
    }

    /// <summary>
    /// Triggered by mouse click: pick out entities from viewport
    /// </summary>
    public unsafe void CreateAsyncPickingRequest(bool selectPrimitives = false)
    {
        if (_pickingEnabled)
        {
            return;
        }

        _pickingEnabled = true;

        if (selectPrimitives)
        {
            Scene.SendPrimitivePickingRequest(_overlayQueue);
        }
        else
        {
            Scene.SendGPUPickingRequest(_renderQueue, _overlayQueue);
        }

        SceneRenderer.AddAsyncReadback(PickingResultReadbackBuffer, PickingResultsBuffer, d =>
        {
            MappedResourceView<PickingResult> result = d.Map<PickingResult>(PickingResultReadbackBuffer, MapMode.Read);

            var results = new Span<PickingResult>(result.MappedResource.Data.ToPointer(), 1);

            _pickingEntity = results[0].entityID != ulong.MaxValue ? (int)results[0].entityID : -1;

            PickingResultsReady = true;
            _pickingEnabled = false;

            SceneRenderer.AddBackgroundUploadTask((d, cl) =>
            {
                cl.UpdateBuffer(PickingResultsBuffer, 0, ref PickingResult, (uint)sizeof(PickingResult));
            });
        });
    }

    public ISelectable GetSelection()
    {
        if (!PickingResultsReady)
        {
            throw new Exception(LOC.Get("REND_Picking_Results_Not_Ready"));
        }

        PickingResultsReady = false;

        if (_pickingEntity == -1)
        {
            return null;
        }

        var renderableSystemIndex = (uint)_pickingEntity >> 30;
        WeakReference<ISelectable> sel;
        if (renderableSystemIndex == 0)
        {
            // TODO: Logging?
            if ((_pickingEntity & 0x3FFFFFFF) >= Scene.OpaqueRenderables.cSelectables.Length)
            {
                return null;
            }

            sel = Scene.OpaqueRenderables.cSelectables[_pickingEntity & 0x3FFFFFFF];
        }
        else
        {
            if ((_pickingEntity & 0x3FFFFFFF) >= Scene.OverlayRenderables.cSelectables.Length)
            {
                return null;
            }

            sel = Scene.OverlayRenderables.cSelectables[_pickingEntity & 0x3FFFFFFF];
        }

        ISelectable selected;
        if (sel != null && sel.TryGetTarget(out selected))
        {
            return selected;
        }

        return null;
    }
}
