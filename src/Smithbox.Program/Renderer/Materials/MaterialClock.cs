using System;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Renderer;

/// <summary>
/// The time that animated materials run on. Every viewport shares it, so that they stay in step.
/// Pausing holds the materials on their current frame rather than resetting them.
/// </summary>
public static class MaterialClock
{
    public const float WindPeriod = 4.0f;

    // The steady and wave parts of the wind at a strength of 1.
    // The game gets these from its own wind sources, which aren't in anything loaded here
    private const float WindSteadyForce = 2.0f;
    private const float WindWaveForce = 6.0f;

    private static readonly Stopwatch Clock = Stopwatch.StartNew();

    /// <summary>
    /// The one wind in the scene: xy is the world XZ direction it blows in, z its
    /// steady force and w the force of its waves.
    /// </summary>
    public static Vector4 Wind
    {
        get
        {
            var angle = CFG.Current.Viewport_Wind_Direction * MathF.PI / 180.0f;
            var strength = CFG.Current.Viewport_Wind_Strength;

            return new Vector4(MathF.Sin(angle), MathF.Cos(angle),
                WindSteadyForce * strength, WindWaveForce * strength);
        }
    }

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
