using StudioCore.Renderer;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// What the FX display (<see cref="FxDisplay"/>) needs from the editor showing it: the project's effects, the scene
/// to draw into and that scene's viewport pipeline. The FX Editor, the Cutscene Editor and any other editor with a
/// viewport can host one; nothing in the display knows which editor it is in.
/// </summary>
public interface IFxDisplayHost
{
    ProjectEntry Project { get; }

    /// <summary>The project's effects (schema and bank). Null when the project has no FX data.</summary>
    FxData Data { get; }

    RenderScene RenderScene { get; }

    /// <summary>The viewport's pipeline, for the scene colour copy screen effects draw with. May be null.</summary>
    SceneRenderPipeline Pipeline { get; }

    /// <summary>What kinds of particles are drawn, and how bright. Read every frame.</summary>
    FxDisplaySettings Settings { get; }
}

/// <summary>Which parts of an effect the display draws.</summary>
public sealed class FxDisplaySettings
{
    public bool Billboards = true;
    public bool ScreenEffects = true;
    public bool LensFlares = true;
    public bool GpuParticles = true;
    public bool Models = true;
    public bool Lights = true;
    public float BillboardBrightness = 1;
    public float LightBrightness = 1;

    /// <summary>The FX Editor's Viewport Rendering choices, which other editors follow too.</summary>
    public FxDisplaySettings ReadFxEditorConfig()
    {
        var c = CFG.Current;
        Billboards = c.FxEditor_Viewport_Display_Billboards;
        ScreenEffects = c.FxEditor_Viewport_Display_Screen_Effects;
        LensFlares = c.FxEditor_Viewport_Display_Lens_Flares;
        GpuParticles = c.FxEditor_Viewport_Display_Gpu_Particles;
        Models = c.FxEditor_Viewport_Display_Models;
        Lights = c.FxEditor_Viewport_Display_Lights;
        BillboardBrightness = c.FxEditor_Viewport_Billboard_Brightness;
        LightBrightness = c.FxEditor_Viewport_Light_Brightness;
        return this;
    }
}

/// <summary>
/// An <see cref="IFxDisplayHost"/> from delegates, so an editor hosts a display without implementing the interface.
/// The settings follow the FX Editor's Viewport Rendering choices unless a settings function is given.
/// </summary>
public sealed class FxDisplayHost : IFxDisplayHost
{
    private readonly Func<RenderScene> GetScene;
    private readonly Func<SceneRenderPipeline> GetPipeline;
    private readonly Func<FxDisplaySettings> GetSettings;
    private readonly FxDisplaySettings Shared = new();

    public ProjectEntry Project { get; }

    public FxData Data => Project.Handler?.FxData;

    public RenderScene RenderScene => GetScene();

    public SceneRenderPipeline Pipeline => GetPipeline?.Invoke();

    public FxDisplaySettings Settings => GetSettings?.Invoke() ?? Shared.ReadFxEditorConfig();

    public FxDisplayHost(ProjectEntry project, Func<RenderScene> scene, Func<SceneRenderPipeline> pipeline,
        Func<FxDisplaySettings> settings = null)
    {
        Project = project;
        GetScene = scene;
        GetPipeline = pipeline;
        GetSettings = settings;
    }

    /// <summary>The pipeline of a viewport, for <paramref name="pipeline"/>: null for one that isn't a Vulkan viewport.</summary>
    public static SceneRenderPipeline PipelineOf(StudioCore.Editors.Viewport.IViewport viewport) =>
        (viewport as StudioCore.Editors.Viewport.VulkanViewport)?.ViewPipeline;
}
