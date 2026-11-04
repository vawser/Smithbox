using StudioCore.Application;

namespace StudioCore.Renderer;

/// <summary>How the viewports draw the scene, <see cref="CFG.Viewport_Render_Mode" /></summary>
public enum ViewportRenderMode
{
    /// <summary>Fullbright: surfaces show their albedo unlit, and the map lighting, GI, shadows, fog and screen
    /// effects are off whatever the Rendering tool says</summary>
    Editor = 0,

    /// <summary>The Rendering tool's settings decide the lighting and effects</summary>
    Game = 1
}

/// <summary>What the textured image shows, <see cref="CFG.Viewport_Texturing_Mode" /></summary>
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
    public static ViewportRenderMode Current => (ViewportRenderMode)CFG.Current.Viewport_Render_Mode;

    public static bool IsGame => Current == ViewportRenderMode.Game;

    /// <summary>Whether the screen effects run: Game mode with them switched on</summary>
    public static bool PostEffects => IsGame && CFG.Current.Viewport_Post_Effects;

    public static ViewportTexturingMode Texturing => (ViewportTexturingMode)CFG.Current.Viewport_Texturing_Mode;

    /// <summary>The lighting debug view value the flver shader takes for fullbright, see SceneParam.LightingDebugView</summary>
    public const uint FullbrightDebugView = 4;
}
