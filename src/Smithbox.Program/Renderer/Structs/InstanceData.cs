using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InstanceData
{
    public Matrix4x4 WorldMatrix;
    public uint MaterialID;
    public uint BoneStartIndex;

    // How an animated mesh fetches its bones, see GetBoneMatrix in the vertex shaders
    public uint SkinReference;
    public uint EntityID;
}
