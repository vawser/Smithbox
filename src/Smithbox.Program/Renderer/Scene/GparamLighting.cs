using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Renderer;

/// <summary>
/// A directional light of a GPARAM LightSet: the sun, Light1, or one of the local lights
/// </summary>
public class GparamDirectionalLight
{
    /// <summary>Radians: x is the elevation, negative above the horizon, y the yaw</summary>
    public Vector2 Angle = new(-0.785398f, 1.570796f);

    /// <summary>Color (rgb) and intensity (w), in the game's units</summary>
    public Vector4 Diffuse = Vector4.Zero;

    /// <summary>Color (rgb) and intensity (w) of the light's highlights</summary>
    public Vector4 Specular = Vector4.Zero;

    /// <summary>The sun and Light1 are always on, at their intensity; the local lights have a switch</summary>
    public bool Enabled = true;

    /// <summary>Light1 only: the angle turns with the camera</summary>
    public bool FollowCamera = false;

    /// <summary>The direction the light travels in, see <see cref="GparamLighting.AngleToDirection" /></summary>
    public Vector3 Direction => GparamLighting.AngleToDirection(Angle);
}

/// <summary>
/// The lighting of a map at one time of day, from its GPARAMs: the LightSet, Tone Map, EnvironmentConfig and
/// Setting Info groups. Colors keep the game's convention of rgb with an intensity in w.
/// A value that isn't found keeps these defaults, which are those of row 0 in Elden Ring's mapbase at t4.
/// See Documentation/Full GI/Gparam Lighting.md.
/// </summary>
public class GparamLighting
{
    /// <summary>The map's GPARAM the values were looked up in first, before mapbase and default</summary>
    public string File = "";

    /// <summary>The LightSet value ID: 0 for the scene, 100 for models</summary>
    public int LightSetId;

    /// <summary>The TimeOfDay the values were interpolated at</summary>
    public float Hours;

    // LightSet
    public GparamDirectionalLight Sun = new() { Diffuse = new(1, 1, 1, 0.5f), Specular = new(1, 1, 1, 0.5f) };
    public GparamDirectionalLight Light1 = new();
    public GparamDirectionalLight LocalLight1 = new() { Enabled = false };
    public GparamDirectionalLight LocalLight2 = new() { Enabled = false };
    public Vector4 HemiUp = new(0.501961f, 0.501961f, 0.501961f, 0.1f);
    public Vector4 HemiDown = new(0.501961f, 0.501961f, 0.501961f, 0.1f);

    // Tone Map
    public float Exposure = 3.5f;
    public bool AutoExposure = false;
    public float MiddleGray = -7.0f;

    // EnvironmentConfig
    public float EnvMapHue = 0.0f;
    public float EnvMapSaturate = 1.0f;
    public Vector4 EnvMapShift = Vector4.One;
    public Vector4 EnvMapScale = Vector4.One;

    // Setting Info
    public Vector4 BackgroundColor = new(0.5f, 0.5f, 0.5f, 0.5f);

    /// <summary>
    /// Where each value came from, as "file #id", keyed by the GPARAM field key. Fields left at their default
    /// have no entry.
    /// </summary>
    public Dictionary<string, string> Sources = new(StringComparer.Ordinal);

    /// <summary>
    /// The direction a light with this GPARAM angle travels in: negative elevations point down, as all the game's
    /// files have them. Checked against the game in the Map Editor (Gparam Lighting.md, Phase 2).
    /// </summary>
    public static Vector3 AngleToDirection(Vector2 angle)
    {
        return new Vector3(
            MathF.Cos(angle.X) * MathF.Sin(angle.Y),
            MathF.Sin(angle.X),
            MathF.Cos(angle.X) * MathF.Cos(angle.Y));
    }
}
