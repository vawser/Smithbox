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
    /// <summary>The length of a step as a fraction of the ray through the box: 0.1 is 10 steps, at most MaxSampleCount</summary>
    public float SteppingDistance = 0.1f;
    /// <summary>The march stops once less than this much light gets through</summary>
    public float SuspendedTransmittance = 0.001f;
    // Read, not drawn: the game lights the fog every LightProcessingInterval steps, which only saves time
    public uint LightProcessingInterval = 4;
    public float LightProcessingIntervalThreshold = 0.5f;

    // Light
    // Read, not drawn: the sun's shadow on the fog, and Smithbox has no shadow map
    public float ShadowingIntensity = 1.0f;
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
    /// The first GPARAM with the volume's row, which the values are looked up in before the others, or null if the
    /// defaults are used
    /// </summary>
    public string Source;
}
