using System.Numerics;

namespace StudioCore.Renderer;

/// <summary>
/// How a fog volume is drawn: the values of its row in the "FogVolume ParamEditor" group of a GPARAM. A volume
/// whose row isn't found uses these defaults, which are those of row 0 in Elden Ring's mapbase.
/// </summary>
public class FogVolumeSettings
{
    public static readonly FogVolumeSettings Default = new();

    // Draw Area
    public float NearFadeMinDistance = 0.0f;
    public float NearFadeMaxDistance = 0.0f;
    public float FarFadeMinDistance = 0.0f;
    public float FarFadeMaxDistance = 0.0f;
    public float EdgeFadeDistance = 0.25f;
    public float EdgeFadeExponent = 2.5f;
    public float HeightFallOff = 0.0f;
    public float FadeDistanceForHeightMap = 0.0f;
    public float MaxInjectingDepth = 0.0f;

    // Ray Marching
    public uint MaxSampleCount = 16;

    // Light
    public Vector2 DirectionalLightAngle = new(-0.785398f, 1.570796f);
    public Vector4 DirectionalLightColor = Vector4.One;
    public Vector4 DirectionalLightOppositeColor = Vector4.One;
    public Vector4 AmbientColor = new(0.0f, 0.0f, 0.0f, 1.0f);
    public bool LightOverwrite = false;
    public float AttenuationIntensity = 0.25f;
    public float AttenuationOffset = 0.125f;
    public uint OctaveNumPerlinNoise = 3;
    public float ColorMapTextureIntensity = 1.0f;
    public Vector2 ColorMapTextureUVTiling = Vector2.One;

    // Density
    public float DensityIntensity = 0.01f;

    // Perlin Noise
    public float PerlinNoiseFrequency = 0.1f;
    public float PerlinNoiseLacunarity = 4.0f;
    public float PerlinNoiseGain = 0.5f;
    public float PerlinNoiseOffset = 0.0f;

    // Scroll
    public Vector3 ScrollSpeed = Vector3.Zero;
    public float ScrollDetailScale = 1.0f;

    /// <summary>
    /// The GPARAM the values came from, or null if the defaults are used
    /// </summary>
    public string Source;
}
