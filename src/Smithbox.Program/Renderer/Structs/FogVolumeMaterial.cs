using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
/// The material of a fog volume, read by FogVolume.frag. Every member is 16 bytes or a multiple of it, so the
/// layout matches std140.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FogVolumeMaterial
{
    /// <summary>From world space to the unit box, which spans -0.5 to 0.5 on each axis</summary>
    public Matrix4x4 InverseWorld;

    /// <summary>xyz: the size of the box, w: density</summary>
    public Vector4 SizeDensity;

    /// <summary>x, y: near fade min and max distance, z, w: far fade min and max distance</summary>
    public Vector4 Fade;

    /// <summary>x: edge fade distance, y: edge fade exponent, z: height falloff, w: height map fade distance</summary>
    public Vector4 Shape;

    /// <summary>x: frequency, y: lacunarity, z: gain, w: offset</summary>
    public Vector4 Noise;

    /// <summary>xyz: scroll speed, w: detail scale</summary>
    public Vector4 Scroll;

    public Vector4 DirectionalLightColor;
    public Vector4 DirectionalLightOppositeColor;
    public Vector4 AmbientColor;

    /// <summary>x, y: directional light angle, z: 1 if the angle overrides the sun, w: color map intensity</summary>
    public Vector4 Light;

    /// <summary>x, y: color map tiling, z: attenuation intensity, w: attenuation offset</summary>
    public Vector4 ColorMap;

    /// <summary>x: height map texture, y: color map texture (0 for none), z: noise octaves, w: max sample count</summary>
    public uint HeightMapTexture;
    public uint ColorMapTexture;
    public uint Octaves;
    public uint MaxSampleCount;

    /// <summary>x: max injecting depth (0 for none), yzw: unused</summary>
    public Vector4 Misc;
}
