using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PickingResult
{
    public uint depth;
    private uint padding;
    public ulong entityID;
}
