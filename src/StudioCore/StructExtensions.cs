using System.Numerics;

namespace StudioCore;

public static class StructExtensions
{
    public static Vector3 XYZ(this Vector4 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }
}
