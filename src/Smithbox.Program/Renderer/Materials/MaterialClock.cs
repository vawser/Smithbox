using System.Diagnostics;

namespace StudioCore.Renderer;

/// <summary>
/// The time that animated materials run on. Every viewport shares it, so that they stay in step.
/// Pausing holds the materials on their current frame rather than resetting them.
/// </summary>
public static class MaterialClock
{
    private static readonly Stopwatch Clock = Stopwatch.StartNew();

    /// <summary>
    /// Seconds of animation so far.
    /// </summary>
    public static float Time
    {
        get
        {
            if (CFG.Current.Viewport_Enable_Material_Animation)
            {
                Clock.Start();
            }
            else
            {
                Clock.Stop();
            }

            return (float)Clock.Elapsed.TotalSeconds;
        }
    }
}
