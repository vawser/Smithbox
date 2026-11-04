using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

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
}
