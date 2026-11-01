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

    /// <summary>Squares either side of the center of a <see cref="DbgPrimGridPlane" /></summary>
    public float GridCells;

    public fixed int pad[2];

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