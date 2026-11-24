using StudioCore.Application;

namespace StudioCore.Renderer;

/// <summary>How the viewports draw the scene, <see cref="ViewportSettings.Viewport_Render_Mode" /></summary>
public enum ViewportRenderMode
{
    /// <summary>Cheap lighting: surfaces are lit by the viewport's sun and the environment map alone, so their
    /// normal, specular and metallic maps read, and the map lighting, GI, shadows, lights, fog and screen effects are
    /// off whatever the Rendering tool says</summary>
    Editor = 0,

    /// <summary>The Rendering tool's settings decide the lighting and effects</summary>
    Game = 1
}

/// <summary>What the textured image shows, <see cref="ViewportSettings.Viewport_Texturing_Mode" /></summary>
public enum ViewportTexturingMode
{
    Diffuse = 0,

    /// <summary>The Normals surface view in place of the image</summary>
    Normal = 1
}

/// <summary>
///     The one place the render mode is read: the pipeline and its passes ask here instead of reading the feature
///     switches on their own, so Editor mode turns them all off together and Game mode leaves them as set.
/// </summary>
public static class RenderMode
{
    public static ViewportRenderMode Current => (ViewportRenderMode)RenderSettings.Viewport.Viewport_Render_Mode;

    public static bool IsGame => Current == ViewportRenderMode.Game;

    /// <summary>Whether the screen effects run: Game mode with them switched on</summary>
    public static bool PostEffects => IsGame && RenderSettings.Viewport.Viewport_Post_Effects;

    /// <summary>
    ///     Whether Game mode lights the Map Editor's maps with their BTL lights alone, for the selected project's game
    ///     (<see cref="GparamProfile.MapLightsOnly" />). The pipelines it applies to say so in
    ///     <see cref="SceneRenderPipeline.MapLightsOnly" />.
    /// </summary>
    public static bool MapLightsOnly => IsGame && GparamProfile.Current.MapLightsOnly;

    public static ViewportTexturingMode Texturing => (ViewportTexturingMode)RenderSettings.Viewport.Viewport_Texturing_Mode;

    /// <summary>
    ///     The lighting debug view value the flver shader takes for Editor mode, see SceneParam.LightingDebugView. It
    ///     lights the whole surface by the view's sun and the environment map, without shadows or the light loops.
    /// </summary>
    public const uint FullbrightDebugView = 4;

    /// <summary>
    ///     Editor mode for Texturing (Normal). The shader treats it as <see cref="FullbrightDebugView" />, as both
    ///     write the normal-mapped normals.
    /// </summary>
    public const uint FullbrightSurfaceDebugView = 5;

    /// <summary>The fullbright debug view value for this frame</summary>
    public static uint FullbrightView => Texturing == ViewportTexturingMode.Normal
        ? FullbrightSurfaceDebugView
        : FullbrightDebugView;
}
