using System.Runtime.InteropServices;

namespace StudioCore.Scene;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct Material
{
    public ushort colorTex;
    public ushort colorTex2;
    public ushort normalTex;
    public ushort normalTex2;
    public ushort specTex;
    public ushort specTex2;
    public ushort emissiveTex;
    public ushort shininessTex;
    public ushort shininessTex2;
    public ushort blendMaskTex;
    public ushort lightmapTex;
    public ushort lightmapTex2;
    public fixed ushort padding[4];
}
