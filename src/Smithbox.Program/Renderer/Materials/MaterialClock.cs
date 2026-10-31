using System;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Renderer;

/// <summary>
/// The material features that can be switched off from the viewport. These are read by the
/// flver shaders every frame, so they apply at once. The values must match FlverShader.vert and .frag.
/// </summary>
[Flags]
public enum MaterialFeatures : uint
{
    None = 0,
    Wind = 1,
    VertexAnimation = 2,
    Sway = 4,
    Birds = 8,
    LayerBlending = 16,
    Overlays = 32,
    Magma = 64,
    Emissive = 128,
    UvScroll = 256,
    Unlit = 512,
    Water = 1024,
    Fur = 2048
}

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
    /// The features that are switched on.
    /// </summary>
    public static MaterialFeatures Features
    {
        get
        {
            var current = MaterialFeatures.None;

            void Add(bool enabled, MaterialFeatures feature)
            {
                if (enabled)
                {
                    current |= feature;
                }
            }

            Add(CFG.Current.Viewport_Material_Wind, MaterialFeatures.Wind);
            Add(CFG.Current.Viewport_Material_Vertex_Animation, MaterialFeatures.VertexAnimation);
            Add(CFG.Current.Viewport_Material_Sway, MaterialFeatures.Sway);
            Add(CFG.Current.Viewport_Material_Birds, MaterialFeatures.Birds);
            Add(CFG.Current.Viewport_Material_Layer_Blending, MaterialFeatures.LayerBlending);
            Add(CFG.Current.Viewport_Material_Overlays, MaterialFeatures.Overlays);
            Add(CFG.Current.Viewport_Material_Magma, MaterialFeatures.Magma);
            Add(CFG.Current.Viewport_Material_Emissive, MaterialFeatures.Emissive);
            Add(CFG.Current.Viewport_Material_UV_Scroll, MaterialFeatures.UvScroll);
            Add(CFG.Current.Viewport_Material_Unlit, MaterialFeatures.Unlit);
            Add(CFG.Current.Viewport_Material_Water, MaterialFeatures.Water);
            Add(CFG.Current.Viewport_Material_Fur, MaterialFeatures.Fur);

            return current;
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
