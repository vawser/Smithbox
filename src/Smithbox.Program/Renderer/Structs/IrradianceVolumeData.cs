using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
/// One irradiance volume as FlverShader.frag reads it from the material buffer, see
/// <see cref="IrradianceVolumeTable" />. 128 bytes, every member a multiple of 16, so std140 and std430 agree.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IrradianceVolumeData
{
    public const int Size = 128;

    /// <summary>From world space to the volume's box, which spans -1 to 1 on each axis as in the game</summary>
    public Matrix4x4 WorldToLocal;

    /// <summary>xyz: half the size of the box in metres, w: 1 / the distance the volume fades in over</summary>
    public Vector4 ExtentsFade;

    /// <summary>xyz: the voxel counts of the textures, w: the scale of the diffuse light</summary>
    public Vector4 VoxelCountsScale;

    /// <summary>The indices of the X, Y, Z and W textures in the volume texture pool</summary>
    public uint TextureX;
    public uint TextureY;
    public uint TextureZ;
    public uint TextureW;

    /// <summary>x: volume ID, y: bake, z: priority, w: unused. For debugging.</summary>
    public Vector4 Info;
}
