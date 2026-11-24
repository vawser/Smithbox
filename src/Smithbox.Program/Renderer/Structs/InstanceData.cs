using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
///     One drawn instance, as the shaders read it from <c>instanceData</c> in Shaders/InstanceData.glsl (set 1,
///     binding 0, std140): 96 bytes, every member in whole 16 byte rows.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InstanceData
{
    public Matrix4x4 WorldMatrix;
    public uint MaterialID;
    public uint BoneStartIndex;

    /// <summary>
    ///     In <see cref="SkinReference" /> of a static instance: the low bits are its entry in
    ///     <see cref="SceneRenderer.Lightmaps" />. Animated instances use 1 or the high bit, so the two never meet.
    /// </summary>
    public const uint LightmapFlag = 0x40000000;

    /// <summary>
    ///     With <see cref="LightmapFlag" />: a lightmapped material whose part found no lightmap, for the lighting debug
    ///     view; lit as if it had none
    /// </summary>
    public const uint MissingLightmap = 0x3FFFFFFF;

    /// <summary>The bits of <see cref="SkinReference" /> that say what it holds</summary>
    public const uint SkinReferenceKindMask = 0xC0000000;

    // How an animated mesh fetches its bones, see GetBoneMatrix in the vertex shaders; for a static one, its
    // lightmap entry with LightmapFlag, or 0
    public uint SkinReference;
    public uint EntityID;

    /// <summary>
    ///     The lighting set the instance is lit by: an entry of its view's
    ///     <see cref="SceneRenderPipeline.SceneLightingBuffer" />, 0 for the view's own lighting. Demon's Souls'
    ///     parts take the LightBank row of their LightID (<see cref="MeshRenderableProxy.LightSet" />).
    /// </summary>
    public uint LightingSet;

    /// <summary>
    ///     How faded the instance is, 1 - opacity, as the bits of a float: 0, the default, is fully opaque. Only read by
    ///     the flver shader, which multiplies its alpha by the opacity (<see cref="MeshRenderableProxy.Opacity" />).
    /// </summary>
    public uint Fade;

    /// <summary>
    ///     A colour the flver shader multiplies the albedo (and its alpha) by, as half floats: R and G in
    ///     <see cref="TintRG" />, B and A in <see cref="TintBA" />. Both 0, the default, is no tint. An effect's
    ///     RichModel particles take their colour this way (<see cref="MeshRenderableProxy.Tint" />).
    /// </summary>
    public uint TintRG;
    public uint TintBA;

    /// <summary>Packs a tint for <see cref="TintRG" /> and <see cref="TintBA" />; null is no tint.</summary>
    public static (uint RG, uint BA) PackTint(Vector4? tint)
    {
        if (tint is not { } t)
            return (0u, 0u);
        static uint Half(float v) => BitConverter.HalfToUInt16Bits((Half)v);
        uint rg = Half(t.X) | (Half(t.Y) << 16);
        uint ba = Half(t.Z) | (Half(t.W) << 16);
        // An all-zero tint would read as none: keep the alpha's lowest bit so it still blacks the model out
        if (rg == 0 && ba == 0)
            ba = 1;
        return (rg, ba);
    }
}
