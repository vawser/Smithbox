using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
/// One irradiance volume as FlverShader.frag reads it from the material buffer, see
/// <see cref="IrradianceVolumeTable" />. 256 bytes, every member a multiple of 16, so std140 and std430 agree,
/// and a power of two so the table's allocation is simple to align.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IrradianceVolumeData
{
    public const int Size = 256;

    /// <summary>xyz: the minimum of the box's world bounds, tested before anything else</summary>
    public Vector4 BoundsMin;

    /// <summary>xyz: the maximum of the box's world bounds</summary>
    public Vector4 BoundsMax;

    /// <summary>From world space to the volume's box, which spans -1 to 1 on each axis as in the game</summary>
    public Matrix4x4 WorldToLocal;

    /// <summary>xyz: half the size of the box in metres, w: 1 / the distance the volume fades in over</summary>
    public Vector4 ExtentsFade;

    /// <summary>xyz: the voxel counts of the textures, w: the scale of the diffuse light</summary>
    public Vector4 VoxelCountsScale;

    /// <summary>The indices of the X, Y, Z and W textures of the bake in the volume texture pool</summary>
    public uint Bake0X;
    public uint Bake0Y;
    public uint Bake0Z;
    public uint Bake0W;

    /// <summary>The same for the bake blended towards, the same as the first if there is none</summary>
    public uint Bake1X;
    public uint Bake1Y;
    public uint Bake1Z;
    public uint Bake1W;

    /// <summary>The IVINFO ShLuminance of each bake, which reflections are normalised by</summary>
    public Vector4 ShLuminance0;
    public Vector4 ShLuminance1;

    /// <summary>x: volume ID, y: bake, z: bake blended towards, w: how far towards it (0 to 1)</summary>
    public Vector4 Info;

    /// <summary>x: the scale of the reflections, yzw: unused</summary>
    public Vector4 Params;

    private Vector4 _reserved0;
    private Vector4 _reserved1;
}
