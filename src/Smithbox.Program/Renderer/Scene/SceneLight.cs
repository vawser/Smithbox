#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

public enum SceneLightType
{
    /// <summary>Parallel light with no position, like the sun</summary>
    Directional = 0,

    /// <summary>Light that shines in every direction from a position</summary>
    Point = 1,

    /// <summary>Light that shines in a cone from a position</summary>
    Spot = 2
}

/// <summary>
///     A light in a <see cref="RenderScene" />, in world space. The scene's sun (see
///     <see cref="SceneParam.LightDirection" />) is always present; these are in addition to it.
///     Lights can be changed at any time, they are read once per frame.
/// </summary>
public class SceneLight
{
    public const float MinConeSeparationDegrees = 0.5f;

    public SceneLightType Type { get; set; } = SceneLightType.Point;

    public bool Enabled { get; set; } = true;

    /// <summary>World position. Not used by directional lights.</summary>
    public Vector3 Position { get; set; }

    /// <summary>The direction the light travels. Used by directional and spot lights.</summary>
    public Vector3 Direction { get; set; } = -Vector3.UnitY;

    /// <summary>Linear RGB color, each channel 0 to 1</summary>
    public Vector3 Color { get; set; } = Vector3.One;

    /// <summary>Multiplier on the color. The sun is 1.</summary>
    public float Intensity { get; set; } = 1.0f;

    /// <summary>Distance at which a point or spot light has faded to nothing. 0 means it never does.</summary>
    public float Range { get; set; } = 20.0f;

    /// <summary>
    ///     Fades from full strength at the light to nothing at <see cref="Range" />, as game lights are
    ///     authored, instead of falling off with the square of the distance.
    /// </summary>
    public bool FadeAcrossRange { get; set; }

    /// <summary>
    ///     With <see cref="FadeAcrossRange" />: the distance out to which the light is at full strength, before it
    ///     fades to nothing at <see cref="Range" /> (Demon's Souls' PointLightBank dwindleBegin). 0 fades from the light.
    /// </summary>
    public float FullStrengthRange { get; set; }

    /// <summary>
    ///     With <see cref="FadeAcrossRange" />: fade in a straight line rather than with the square of the fade
    ///     (Demon's Souls' point lights, which dwindle from dwindleBegin to dwindleEnd)
    /// </summary>
    public bool LinearFade { get; set; }

    /// <summary>Spot lights: angle from the center of the cone within which the light is at full strength</summary>
    public float InnerConeAngleDegrees { get; set; } = 20.0f;

    /// <summary>Spot lights: angle from the center of the cone at which the light has faded to nothing</summary>
    public float OuterConeAngleDegrees { get; set; } = 35.0f;

    /// <summary>
    ///     Linear RGB color of the light's highlights, as BTL lights give it. Null for the same as <see cref="Color" />.
    ///     Only lights in <see cref="SceneLightTable" /> have their own; the scene params' lights use the color.
    /// </summary>
    public Vector3? SpecularColor { get; set; }

    /// <summary>Multiplier on <see cref="SpecularColor" />. Null for the same as <see cref="Intensity" />.</summary>
    public float? SpecularIntensity { get; set; }

    /// <summary>The light with its highlights, as <see cref="SceneLightTable" /> holds it</summary>
    internal MapLightData ToMapGpu()
    {
        return new MapLightData
        {
            Light = ToGpu(),
            Specular = new Vector4(SpecularColor ?? Color, Math.Max(SpecularIntensity ?? Intensity, 0.0f))
        };
    }

    internal SceneLightData ToGpu()
    {
        Vector3 direction = Direction.LengthSquared() > 1e-10f ? Vector3.Normalize(Direction) : -Vector3.UnitY;

        var inner = Math.Clamp(InnerConeAngleDegrees, 0.0f, 89.0f);
        var outer = Math.Clamp(OuterConeAngleDegrees, inner + MinConeSeparationDegrees, 90.0f);
        const float toRadians = MathF.PI / 180.0f;

        return new SceneLightData
        {
            PositionRange = new Vector4(Position, Math.Max(Range, 0.0f)),
            DirectionType = new Vector4(direction, (float)Type),
            ColorIntensity = new Vector4(Color, Math.Max(Intensity, 0.0f)),
            Spot = new Vector4(MathF.Cos(inner * toRadians), MathF.Cos(outer * toRadians),
                FadeAcrossRange ? (LinearFade ? 2.0f : 1.0f) : 0.0f, Math.Clamp(FullStrengthRange, 0.0f, Math.Max(Range, 0.0f)))
        };
    }
}

/// <summary>
///     A light as the shaders read it. Must match sceneLight in FlverShader.frag: four vec4s.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SceneLightData
{
    public const int FloatCount = 16;

    public Vector4 PositionRange;
    public Vector4 DirectionType;
    public Vector4 ColorIntensity;
    public Vector4 Spot;
}

/// <summary>
///     A light of <see cref="SceneLightTable" /> as the shaders read it, with its own highlights. Must match mapLight in
///     FlverShader.frag: a sceneLight and a vec4, 80 bytes. Kept apart from <see cref="SceneLightData" /> so the scene
///     params' light array, which every shader's copy of them lays out, is unchanged.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MapLightData
{
    public const int Size = 80;

    public SceneLightData Light;

    /// <summary>rgb: color of the highlights, a: their intensity</summary>
    public Vector4 Specular;
}

/// <summary>
///     The lights of a scene. Safe to change from any thread.
/// </summary>
public sealed class SceneLightList : IEnumerable<SceneLight>
{
    /// <summary>The most lights that are drawn. Lights beyond this are ignored.</summary>
    public const int MaxLights = 16;

    private readonly List<SceneLight> _lights = new();
    private readonly object _lock = new();

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _lights.Count;
            }
        }
    }

    public void Add(SceneLight light)
    {
        lock (_lock)
        {
            if (!_lights.Contains(light))
            {
                _lights.Add(light);
            }
        }
    }

    public bool Remove(SceneLight light)
    {
        lock (_lock)
        {
            return _lights.Remove(light);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _lights.Clear();
        }
    }

    /// <summary>
    ///     Appends the enabled lights to <paramref name="destination" />, up to the maximum.
    /// </summary>
    internal void CollectEnabled(List<SceneLightData> destination)
    {
        lock (_lock)
        {
            foreach (SceneLight light in _lights)
            {
                if (destination.Count >= MaxLights)
                {
                    return;
                }

                if (light.Enabled)
                {
                    destination.Add(light.ToGpu());
                }
            }
        }
    }

    public IEnumerator<SceneLight> GetEnumerator()
    {
        List<SceneLight> snapshot;
        lock (_lock)
        {
            snapshot = new List<SceneLight>(_lights);
        }

        return snapshot.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
