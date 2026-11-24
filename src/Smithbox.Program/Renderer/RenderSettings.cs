using StudioCore.Application;
using System;

namespace StudioCore.Renderer;

/// <summary>
///     The settings the renderer reads: the viewport and graphics sections. The renderer reads them here rather than
///     from <see cref="CFG"/>, so it only depends on the settings it uses, and can be given its own (as tests do).
/// </summary>
/// <remarks>
///     Smithbox hands over the loaded configuration's sections once at start-up (<see cref="Use"/>), before the
///     graphics context is made. The sections stay the same objects for the rest of the run (resetting to defaults
///     copies into them), so changes made in the Preferences reach the renderer straight away. Until then, and in
///     tests, these are fresh sections with the default values.
/// </remarks>
public static class RenderSettings
{
    public static ViewportSettings Viewport { get; private set; } = new();

    public static GraphicsSettings Graphics { get; private set; } = new();

    public static void Use(ViewportSettings viewport, GraphicsSettings graphics)
    {
        Viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));
        Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
    }
}
