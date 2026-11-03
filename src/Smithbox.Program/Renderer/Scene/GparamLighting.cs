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

    /// <summary>
    /// The model set (LightSet ID 100) that characters are lit with, resolved with the scene's for the viewports;
    /// null on the model set itself and when resolved alone
    /// </summary>
    public GparamLighting Models;

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
    /// The TimeOfDay keys of the game's seven timezones (CutsceneGparamTimeParam: morning, noon A, noon B, evening,
    /// night, deep night A, deep night B), at which Elden Ring keys its lighting. Elden Ring's seven irradiance
    /// volume bakes are these in order: bake N is lit as key N (Gparam Lighting.md, Phase 5).
    /// </summary>
    public static readonly float[] TimeKeys = { 0.0f, 4.0f, 8.0f, 12.0f, 15.0f, 21.0f, 22.5f };

    /// <summary>The localization keys of the timezones' names, in the order of <see cref="TimeKeys" /></summary>
    public static readonly string[] TimezoneNames =
    {
        "MAP_Lighting_Timezone_Morning", "MAP_Lighting_Timezone_Noon_A", "MAP_Lighting_Timezone_Noon_B",
        "MAP_Lighting_Timezone_Evening", "MAP_Lighting_Timezone_Night", "MAP_Lighting_Timezone_Deep_Night_A",
        "MAP_Lighting_Timezone_Deep_Night_B"
    };

    /// <summary>
    /// The timezones either side of a time, and how far it is from the first to the second, wrapping from the last
    /// round to the first: the bakes to blend at that time
    /// </summary>
    public static (int From, int To, float Ratio) TimezonesAt(float hours)
    {
        hours = ((hours % 24.0f) + 24.0f) % 24.0f;

        var from = TimeKeys.Length - 1;

        for (var i = TimeKeys.Length - 1; i >= 0; i--)
        {
            if (TimeKeys[i] <= hours)
            {
                from = i;
                break;
            }
        }

        var to = (from + 1) % TimeKeys.Length;
        var span = ((TimeKeys[to] - TimeKeys[from]) % 24.0f + 24.0f) % 24.0f;
        var ratio = span > 0.0f ? (((hours - TimeKeys[from]) % 24.0f + 24.0f) % 24.0f) / span : 0.0f;

        return (from, to, Math.Clamp(ratio, 0.0f, 1.0f));
    }

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
