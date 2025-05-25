using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Credit to RunDevelopment for this code
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct GXValue
{
    [FieldOffset(0)]
    public int I;
    [FieldOffset(0)]
    public float F;

    public GXValue(int i) : this()
    {
        I = i;
    }
    public GXValue(float f) : this()
    {
        F = f;
    }

    public byte[] GetBytes() => BitConverter.GetBytes(I);

    public override string ToString()
    {
        if (I == 0) return "0";
        return I.ToString(CultureInfo.InvariantCulture) + "/" + F.ToString(CultureInfo.InvariantCulture);
    }
}