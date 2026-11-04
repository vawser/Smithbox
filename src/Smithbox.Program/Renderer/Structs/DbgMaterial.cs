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

    /// <summary>Distance from the camera where a <see cref="DbgPrimGridPlane" /> has faded out, 0 for no fade</summary>
    public float GridFadeDistance;

    /// <summary>
    /// Flags of a <see cref="DbgPrimGridPlane" />. Bit 0: draw the axis lines through its center.
    /// Bits 16-31: squares between major lines, 0 or 1 for none.
    /// </summary>
    public uint GridFlags;

    /// <summary>
    /// Where the center of a <see cref="DbgPrimGridPlane" />'s quad is, in squares from the grid's origin. Non-zero
    /// when the grid follows the camera, so its lines stay where the grid's origin puts them.
    /// </summary>
    public float GridOffsetX;

    /// <inheritdoc cref="GridOffsetX" />
    public float GridOffsetZ;

    /// <summary>Scales the line widths of a <see cref="DbgPrimGridPlane" />, 1 for the default widths</summary>
    public float GridLineWidth;

    // Keeps the size a multiple of 16 bytes, the stride of the shaders' material arrays. 32 bytes in all, see
    // DebugWire.frag and ViewportGrid.frag.
    private uint pad;

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