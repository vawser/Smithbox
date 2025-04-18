using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Scene;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct InstanceData
{
    public Matrix4x4 WorldMatrix;
    public uint MaterialID;
    public uint BoneStartIndex;
    public uint r2;
    public uint EntityID;
}
