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
    private readonly GraphicsDevice Device;

    /// <summary>The settings of the sun's shadows without map lighting: Elden Ring's mapbase</summary>
    private static readonly GparamShadow DefaultShadow = new();

    /// <summary>The HDR target the scene is drawn into, before <see cref="ToneMap" /> takes it to the screen</summary>
    public SceneTarget Target { get; } = new();

    public ToneMapPass ToneMap { get; } = new();

    /// <summary>The screen effects run on <see cref="Target" /> before <see cref="ToneMap" /> reads it</summary>
    public PostProcessChain PostProcess { get; } = new();

    /// <summary>The first of <see cref="PostProcess" />'s passes, whose adjustment the tone map and bloom read</summary>
    public AutoExposurePass AutoExposure { get; } = new();

    /// <summary>The debug view of the scene target's surface attachments, the last of <see cref="PostProcess" />'s passes</summary>
    public SurfaceViewPass SurfaceView { get; }

    /// <summary>The ambient occlusion, the first of <see cref="PostProcess" />'s passes</summary>
    public SsaoPass Ssao { get; }

    /// <summary>The anti-aliasing after the tone map, see <see cref="FxaaPass" /></summary>
    public FxaaPass Fxaa { get; } = new();

    /// <summary>The copy of <see cref="Target" />'s depth that transparent draws read</summary>
    public SceneDepth SceneDepth { get; } = new();

    /// <summary>The GPARAM's distance and height fog, laid over the scene when its depth is copied</summary>
    public VolumetricFogPass Fog { get; } = new();

    /// <summary>The sun's shadow maps, or null for a pipeline drawn without shadows</summary>
    public ShadowCascades Shadows { get; }

    // Scene Parameters
    public DeviceBuffer SceneParamBuffer { get; }
    public ResourceSet SceneParamResourceSet { get; }
    private DeviceBuffer OverlayParamBuffer { get; }
    private ResourceSet OverlayParamResourceSet { get; }

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
    ///     Whether models are lit by the irradiance volumes of <see cref="VolumeTable" />
    /// </summary>
    public bool UseIrradianceVolumes = false;

    /// <summary>
    ///     The irradiance volumes this pipeline lights with: the Map Editor's loaded maps in
    ///     <see cref="SceneRenderer.IrradianceVolumes" /> by default, or the borrowed bake of the Model and Time Act
    ///     Editors' viewport environment
    /// </summary>
    public IrradianceVolumeTable VolumeTable;

    /// <summary>
    ///     Whether the sun and the exposure come from <see cref="MapLightingSource" /> instead of the viewport's own
    ///     settings
    /// </summary>
    public bool UseMapLighting = false;

    /// <summary>
    ///     The GPARAM lighting this pipeline is lit with under <see cref="UseMapLighting" />: the Map Editor's loaded
    ///     maps in <see cref="SceneRenderer.MapLighting" /> by default, or the default map of the Model and Time Act
    ///     Editors' viewport environment
    /// </summary>
    public Func<GparamLighting> MapLightingSource;

    /// <summary>
    ///     Whether models are lit by the map's lights in <see cref="SceneRenderer.MapLights" />. Only the Map Editor's
    ///     viewports are, as the table holds the BTL lights of its loaded maps.
    /// </summary>
    public bool UseMapLights = false;

    /// <summary>The map lighting this pipeline is lit with this frame, or null if it uses its own settings</summary>
    public GparamLighting ActiveMapLighting => UseMapLighting && CFG.Current.Viewport_Map_Lighting
        ? MapLightingSource != null ? MapLightingSource() : SceneRenderer.MapLighting
        : null;

    /// <summary>The irradiance volume table this pipeline reads, see <see cref="VolumeTable" /></summary>
    public IrradianceVolumeTable ActiveVolumeTable => VolumeTable ?? SceneRenderer.IrradianceVolumes;

    /// <summary>
    ///     The viewport's background as linear light, which <see cref="Target" /> is cleared to. With map lighting it is
    ///     the GPARAM's BG color, which is in the game's units like its lights, so it takes their exposure; otherwise
    ///     the background color of the settings, which is a display value, so is squared to undo the tone map's gamma.
    /// </summary>
    public RgbaFloat BackgroundColor
    {
        get
        {
            var lighting = ActiveMapLighting;

            if (lighting == null || !CFG.Current.Viewport_Map_Lighting_Background)
            {
                var display = CFG.Current.Viewport_Bg_Color;
                return new RgbaFloat(display.X * display.X, display.Y * display.Y, display.Z * display.Z, 1.0f);
            }

            var bg = lighting.BackgroundColor;
            var exposure = lighting.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale;
            var linear = Vector3.Max(new Vector3(bg.X, bg.Y, bg.Z) * bg.W * exposure, Vector3.Zero);

            return new RgbaFloat(linear.X, linear.Y, linear.Z, 1.0f);
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

    /// <param name="shadows">Whether the sun casts shadows in this pipeline's scene</param>
    public unsafe SceneRenderPipeline(RenderScene scene, GraphicsDevice device, int width, int height,
        bool shadows = true)
    {
        Scene = scene;
        Device = device;

        Ssao = new SsaoPass(this);
        SurfaceView = new SurfaceViewPass(Ssao);

        // SSAO darkens the scene itself, so it goes before the auto exposure measures it. Auto exposure next, so
        // the passes after it see this frame's adjustment.
        PostProcess.Add(Ssao);
        PostProcess.Add(AutoExposure);
        PostProcess.Add(new LightShaftPass(this, AutoExposure));
        PostProcess.Add(new BloomPass(AutoExposure));

        // Last, as it replaces the image
        PostProcess.Add(SurfaceView);

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

        // The overlays' copy, which differs in the cursor: they are drawn on the swapchain, and the scene in its target
        OverlayParamBuffer = factory.CreateBuffer(
            new BufferDescription((uint)sizeof(SceneParam), VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst, VmaMemoryUsage.Auto, 0));
        device.UpdateBuffer(OverlayParamBuffer, 0, ref SceneParams, (uint)sizeof(SceneParam));
        OverlayParamResourceSet = factory.CreateResourceSet(new ResourceSetDescription(sceneParamLayout, OverlayParamBuffer));

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

        // The queues run in the order they are registered: the shadow maps have to be drawn before the scene that
        // reads them, and all of a pipeline's queues have to be together, as binding another framebuffer in between
        // would discard its target
        if (shadows)
        {
            Shadows = new ShadowCascades(device, this);
        }

        // The scene goes into the pipeline's own HDR target, on the offscreen list, so it is drawn before the
        // overlay queue tone maps it onto the swapchain
        _renderQueue = new SceneRenderer.RenderQueue("Viewport Render", device, this)
        {
            CapturesSceneDepth = true,
            IsOffscreen = true
        };
        SceneRenderer.RegisterRenderQueue(_renderQueue);

        _overlayQueue = new SceneRenderer.RenderQueue("Overlay Render", device, this)
        {
            SceneParamSet = OverlayParamResourceSet
        };
        SceneRenderer.RegisterRenderQueue(_overlayQueue);
    }

    /// <summary>
    ///     Draws this pipeline's overlays on the offscreen list too, for a pipeline that doesn't reach the swapchain.
    /// </summary>
    public void SetOffscreen()
    {
        _overlayQueue.IsOffscreen = true;
    }

    /// <summary>
    ///     Copies <see cref="Target" />'s depth for the transparent draws that follow, see <see cref="SceneDepth" />,
    ///     and lays the fog over the opaque draws, which reads it
    /// </summary>
    public void CaptureSceneDepth(CommandList cl)
    {
        SceneDepth.Capture(Device, cl, Target.Depth);
        Fog.Apply(Device, cl, this);
    }

    public void SetViewportSetupAction(Action<GraphicsDevice, CommandList> action)
    {
        _renderQueue.SetPredrawSetupAction(action);
    }

    public void SetOverlayViewportSetupAction(Action<GraphicsDevice, CommandList> action)
    {
        _overlayQueue.SetPredrawSetupAction(action);
    }

    /// <param name="cursorx">The cursor in the pixels of the framebuffer the overlays are drawn to</param>
    /// <param name="targetx">Where <see cref="Target" />'s first pixel is in that framebuffer, so the scene's picking
    /// can take the cursor in its own pixels</param>
    public unsafe void UpdateSceneParameters(Matrix4x4 proj, Matrix4x4 view, Vector3 eye, int cursorx, int cursory,
        int targetx = 0, int targety = 0)
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
        var shadowStrength = Math.Clamp(CFG.Current.Viewport_Shadow_Strength, 0.0f, 1.0f);

        CollisionTransparency.Refresh();
        var collisionOpacity = Math.Clamp(CFG.Current.Viewport_Collision_Opacity, 0.0f, 1.0f);
        var navVolumeSliceHeight = NavVolumeSlice.Height;
        var navVolumeSliceFlags = NavVolumeSlice.Flags;
        var volumeTable = ActiveVolumeTable;
        var useIrradianceVolumes = UseIrradianceVolumes && CFG.Current.Viewport_GI_Enable;
        var irradianceExposure = CFG.Current.Viewport_GI_Intensity *
            (CFG.Current.Viewport_GI_Auto_Exposure ? volumeTable.AutoExposure : 1.0f);
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
        MapDirectionalLightData[] mapModelLights = null;
        var mapModelHemiUp = Vector4.Zero;
        var mapModelHemiDown = Vector4.Zero;

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

            // Characters take the model set: its own sun, the fill lights, and its hemisphere
            var models = mapLighting.Models;

            if (models != null && CFG.Current.Viewport_Map_Lighting_Model_Lights)
            {
                // Light1 can turn with the camera, by the camera's yaw on the GPARAM angles' scale
                var cameraYaw = 0.0f;

                if (Matrix4x4.Invert(view, out var cameraWorld))
                {
                    var forward = Vector3.TransformNormal(-Vector3.UnitZ, cameraWorld);
                    cameraYaw = MathF.Atan2(forward.X, forward.Z);
                }

                mapModelLights = new[]
                {
                    ToGpu(models.Sun, exposure, cameraYaw),
                    ToGpu(models.Light1, exposure, cameraYaw),
                    ToGpu(models.LocalLight1, exposure, cameraYaw),
                    ToGpu(models.LocalLight2, exposure, cameraYaw)
                };

                mapModelHemiUp = new Vector4(new Vector3(models.HemiUp.X, models.HemiUp.Y, models.HemiUp.Z) * models.HemiUp.W * exposure, 1.0f);
                mapModelHemiDown = new Vector4(new Vector3(models.HemiDown.X, models.HemiDown.Y, models.HemiDown.Z) * models.HemiDown.W * exposure, 1.0f);

                mapLightingFlags |= 4u;
            }
        }

        // The cascades follow the sun the shaders light with, and the GPARAM's Shadows group
        var sunDirection = mapLighting != null ? mapLightDirection : SceneParams.LightDirection;
        var sunIntensity = mapLighting != null ? mapSunColor.W : SceneParams.SunColor.W;
        var castShadows = CFG.Current.Viewport_Shadows && sunIntensity > 0.0f;

        Shadows?.Update(castShadows, view, proj, new Vector3(sunDirection.X, sunDirection.Y, sunDirection.Z),
            mapLighting?.Shadow ?? DefaultShadow);

        Fog.Prepare(view, proj, mapLighting, new Vector3(sunDirection.X, sunDirection.Y, sunDirection.Z));

        var shadowParams = Shadows?.Parameters ?? default;
        var shadowCount = (uint)(Shadows?.ActiveCount ?? 0);
        var shadowTexture = Shadows?.TextureIndex ?? 0;
        var shadowMapSize = Shadows?.MapSize ?? 0.0f;

        SceneRenderer.AddBackgroundUploadTask((d, cl) =>
        {
            SceneParams.Projection = proj;
            SceneParams.View = view;
            SceneParams.EyePosition = new Vector4(eye, 0.0f);
            SceneParams.EnvMap = EnvMapTexture;
            SceneParams.CursorPosition[0] = cursorx - targetx;
            SceneParams.CursorPosition[1] = cursory - targety;

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
            SceneParams.IrradianceVolumeStart = volumeTable.Start;
            SceneParams.IrradianceVolumeCount = useIrradianceVolumes ? volumeTable.Count : 0;
            SceneParams.IrradianceVolumeExposure = irradianceExposure;
            SceneParams.IrradianceVolumeFlags = irradianceFlags;

            // As with the volumes, read here so the count only covers lights already on the GPU
            SceneParams.MapLightStart = SceneRenderer.MapLights.Start;
            SceneParams.MapLightCount = UseMapLights ? SceneRenderer.MapLights.Count : 0;

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
            SceneParams.ModelLight0 = mapModelLights?[0] ?? default;
            SceneParams.ModelLight1 = mapModelLights?[1] ?? default;
            SceneParams.ModelLight2 = mapModelLights?[2] ?? default;
            SceneParams.ModelLight3 = mapModelLights?[3] ?? default;
            SceneParams.ModelHemiUp = mapModelHemiUp;
            SceneParams.ModelHemiDown = mapModelHemiDown;

            SceneParams.ShadowMatrix0 = shadowParams.Matrix0;
            SceneParams.ShadowMatrix1 = shadowParams.Matrix1;
            SceneParams.ShadowMatrix2 = shadowParams.Matrix2;
            SceneParams.ShadowMatrix3 = shadowParams.Matrix3;
            SceneParams.ShadowSplits = shadowParams.Splits;
            SceneParams.ShadowTexelSizes = shadowParams.TexelSizes;
            SceneParams.ShadowDepthScales = shadowParams.DepthScales;
            SceneParams.ShadowTexture = shadowTexture;
            SceneParams.ShadowCascadeCount = shadowTexture != 0 ? shadowCount : 0;
            SceneParams.ShadowFadeStart = shadowParams.FadeStart;
            SceneParams.ShadowFadeEnd = shadowParams.FadeEnd;
            SceneParams.ShadowDepthBias = shadowParams.DepthBias;
            SceneParams.ShadowSlopeBias = shadowParams.SlopeBias;
            SceneParams.ShadowCascadeFade = shadowParams.CascadeFade;
            SceneParams.ShadowStrength = shadowStrength;
            SceneParams.ShadowMapSize = shadowMapSize;

            // A copy, so the viewport's own sun stays in SceneParams for its sliders
            var uploaded = SceneParams;

            if (mapLighting != null)
            {
                uploaded.LightDirection = mapLightDirection;
                uploaded.SunColor = mapSunColor;
                uploaded.SunSpecColor = mapSunSpecColor;
            }

            cl.UpdateBuffer(SceneParamBuffer, 0, ref uploaded, (uint)sizeof(SceneParam));
            Shadows?.Upload(cl, uploaded);

            var overlay = uploaded;
            overlay.CursorPosition[0] = cursorx;
            overlay.CursorPosition[1] = cursory;
            cl.UpdateBuffer(OverlayParamBuffer, 0, ref overlay, (uint)sizeof(SceneParam));
        });
    }

    /// <summary>
    ///     A GPARAM directional light as the shaders read it, at the exposure. A light that is off gets an intensity
    ///     of 0; one that follows the camera turns by <paramref name="cameraYaw" />.
    /// </summary>
    private static MapDirectionalLightData ToGpu(GparamDirectionalLight light, float exposure, float cameraYaw)
    {
        var intensity = light.Enabled ? exposure : 0.0f;
        var angle = light.FollowCamera ? new Vector2(light.Angle.X, light.Angle.Y + cameraYaw) : light.Angle;

        return new MapDirectionalLightData
        {
            Direction = new Vector4(GparamLighting.AngleToDirection(angle), 0.0f),
            Color = new Vector4(light.Diffuse.X, light.Diffuse.Y, light.Diffuse.Z, light.Diffuse.W * intensity),
            SpecColor = new Vector4(light.Specular.X, light.Specular.Y, light.Specular.Z, light.Specular.W * intensity)
        };
    }

    /// <param name="sceneParams">The scene parameters to draw with, or null for this pipeline's own</param>
    public void BindResources(CommandList cl, ResourceSet sceneParams = null)
    {
        cl.SetGraphicsResourceSet(0, sceneParams ?? SceneParamResourceSet);
        cl.SetGraphicsResourceSet(6, PickingResultResourceSet);
    }

    public void RenderScene(BoundingFrustum frustum)
    {
        Scene.Render(_renderQueue, _overlayQueue, frustum, this);
        Shadows?.Submit(Scene.OpaqueRenderables, Scene.DrawFilter, Scene.DisplayGroup);
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
