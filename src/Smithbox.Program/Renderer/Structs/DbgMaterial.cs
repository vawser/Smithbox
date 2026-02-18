#nullable enable
using System.Drawing;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct DbgMaterial
{
    private struct _color
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
    }

    private _color _Color;
    public fixed int pad[3];

    public Color Color
    {
        set
        {
            _Color.r = value.R;
            _Color.g = value.G;
            _Color.b = value.B;
            _Color.a = value.A;
        }
    }
}