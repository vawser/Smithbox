using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
///     Per material table read by the flver shader. Every value is the index of a texture in the global texture pool.
///     The shader reads this as 12 32-bit words, two values to a word, so the order and size here
///     must match FlverShader.frag exactly: 24 values in six rows of four, 48 bytes.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct Material
{
    /// <summary>The most texture layers a material can blend</summary>
    public const int MaxLayers = 4;

    // Row 0: albedo, layers 0-3
    public ushort colorTex;
    public ushort colorTex2;
    public ushort colorTex3;
    public ushort colorTex4;

    // Row 1: normal, layers 0-3
    public ushort normalTex;
    public ushort normalTex2;
    public ushort normalTex3;
    public ushort normalTex4;

    // Row 2: specular, layers 0-3
    public ushort specTex;
    public ushort specTex2;
    public ushort specTex3;
    public ushort specTex4;

    // Row 3: shininess, layers 0-3
    public ushort shininessTex;
    public ushort shininessTex2;
    public ushort shininessTex3;
    public ushort shininessTex4;

    // Row 4: other textures
    public ushort blendMaskTex;
    public ushort emissiveTex;
    public ushort lightmapTex;
    public ushort lightmapTex2;

    // Row 5: the UV set (0 = first, 1 = second) that each layer's textures use
    public ushort uvSet;
    public ushort uvSet2;
    public ushort uvSet3;
    public ushort uvSet4;
}

/// <summary>
///     How a material moves its vertices. Read by the flver vertex shader as the entry straight after the
///     material's <see cref="Material" />, which is the only reason it has the same 48 byte size.
///     Only materials with a deformer allocate it, so static materials don't pay for it.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct MaterialDeform
{
    public float Amplitude;

    /// <summary>Frames a second for a vertex animation, cycles a second for sway</summary>
    public float Rate;

    private fixed float _padding[2];

    /// <summary>The direction of sway, unused by vertex animation</summary>
    public Vector4 Direction;

    /// <summary>Global texture index of the positions and normals of a vertex animation. 0 until they load</summary>
    public uint VatPositionTex;
    public uint VatNormalTex;

    private fixed uint _padding2[2];
}

/// <summary>
///     What is uploaded for a material with a deformer
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MaterialWithDeform
{
    public Material Material;
    public MaterialDeform Deform;
}
