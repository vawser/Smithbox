using System;
using System.Drawing;
using System.Numerics;

namespace StudioCore.DebugPrimitives;

public class DbgPrimSolidBone : DbgPrimSolid
{
    public DbgPrimSolidBone(bool isHkx, string name, Transform location, Quaternion rotation, float thickness,
        float length, Color color)
    {
        Category = isHkx ? DbgPrimCategory.HkxBone : DbgPrimCategory.FlverBone;

        NameColor = color;
        Name = name;

        thickness = Math.Min(length / 2, thickness);

        Vector3 start = Vector3.Zero;
        Vector3 a = (Vector3.UnitY * thickness) + (Vector3.UnitX * thickness);
        Vector3 b = (Vector3.UnitZ * thickness) + (Vector3.UnitX * thickness);
        Vector3 c = (-Vector3.UnitY * thickness) + (Vector3.UnitX * thickness);
        Vector3 d = (-Vector3.UnitZ * thickness) + (Vector3.UnitX * thickness);
        Vector3 end = Vector3.UnitX * length;

        start = Vector3.Transform(start, Matrix4x4.CreateFromQuaternion(rotation));
        a = Vector3.Transform(a, Matrix4x4.CreateFromQuaternion(rotation));
        b = Vector3.Transform(b, Matrix4x4.CreateFromQuaternion(rotation));
        c = Vector3.Transform(c, Matrix4x4.CreateFromQuaternion(rotation));
        d = Vector3.Transform(d, Matrix4x4.CreateFromQuaternion(rotation));
        end = Vector3.Transform(end, Matrix4x4.CreateFromQuaternion(rotation));

        AddTri(start, a, b, color);
        AddTri(start, b, c, color);
        AddTri(start, c, d, color);
        AddTri(start, d, a, color);

        AddTri(end, b, a, color);
        AddTri(end, c, b, color);
        AddTri(end, d, c, color);
        AddTri(end, a, d, color);
    }
}
