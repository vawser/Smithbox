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
