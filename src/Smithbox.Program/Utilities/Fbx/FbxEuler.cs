#nullable enable
using System;
using System.Numerics;

namespace StudioCore.Utilities.Fbx;

/// <summary>
///     FBX's Euler XYZ rotation order (<c>RotationOrder</c> 0, the default): degrees (x, y, z), applied X first, then
///     Y, then Z, so the rotation is Rz · Ry · Rx on column vectors. Computed in double precision.
/// </summary>
public static class FbxEuler
{
    private const double Rad = 180.0 / Math.PI;

    /// <summary>The XYZ angles (degrees) of a rotation; Y in [-90, 90].</summary>
    public static Vector3 ToXyzDegrees(Quaternion rotation)
    {
        var q = Quaternion.Normalize(rotation);
        double x = q.X, y = q.Y, z = q.Z, w = q.W;

        // Column-vector rotation matrix entries used below
        var r01 = 2 * (x * y - z * w);
        var r02 = 2 * (x * z + y * w);
        var r11 = 1 - 2 * (x * x + z * z);
        var r12 = 2 * (y * z - x * w);
        var r20 = 2 * (x * z - y * w);
        var r21 = 2 * (y * z + x * w);
        var r22 = 1 - 2 * (x * x + y * y);

        // X first; then Y and Z from R · Rx(-x) = Rz(z) · Ry(y), so whatever rounding X has near gimbal lock (where
        // it comes from tiny entries) is made up for by Y and Z, and the rotation stays exact (Mike Day's method).
        var ax = Math.Atan2(r21, r22);
        var (sx, cx) = Math.SinCos(ax);
        var ay = Math.Atan2(-r20, sx * r21 + cx * r22);
        var az = Math.Atan2(sx * r02 - cx * r01, cx * r11 - sx * r12);

        return new Vector3((float)(ax * Rad), (float)(ay * Rad), (float)(az * Rad));
    }

    /// <summary>The rotation of XYZ angles (degrees).</summary>
    public static Quaternion FromXyzDegrees(Vector3 degrees)
    {
        var qx = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(degrees.X / Rad));
        var qy = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(degrees.Y / Rad));
        var qz = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(degrees.Z / Rad));

        // X applied first: qz * qy * qx
        return Quaternion.Normalize(qz * qy * qx);
    }

    /// <summary>
    ///     The rotation of Euler angles (degrees) in an FBX <c>RotationOrder</c>: 0 XYZ, 1 XZY, 2 YZX, 3 YXZ, 4 ZXY,
    ///     5 ZYX, 6 spheric (read as XYZ). The first axis named is applied first.
    /// </summary>
    public static Quaternion FromDegrees(Vector3 degrees, int order)
    {
        var qx = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)(degrees.X / Rad));
        var qy = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)(degrees.Y / Rad));
        var qz = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)(degrees.Z / Rad));
        var q = order switch
        {
            1 => qy * qz * qx,
            2 => qx * qz * qy,
            3 => qz * qx * qy,
            4 => qy * qx * qz,
            5 => qx * qy * qz,
            _ => qz * qy * qx,
        };
        return Quaternion.Normalize(q);
    }

    /// <summary>
    ///     The XYZ angles of a rotation closest to the previous frame's angles: of the two equivalent sets, with whole
    ///     turns added, the one that moves least. Keeps curves free of ±180° jumps, so DCC tools interpolate them
    ///     between frames without spinning.
    /// </summary>
    public static Vector3 ToXyzDegrees(Quaternion rotation, Vector3 previous)
    {
        var a = ToXyzDegrees(rotation);
        var b = new Vector3(a.X + 180, 180 - a.Y, a.Z + 180);

        a = Near(a, previous);
        b = Near(b, previous);
        return Vector3.DistanceSquared(a, previous) <= Vector3.DistanceSquared(b, previous) ? a : b;
    }

    private static Vector3 Near(Vector3 v, Vector3 to) => new(Near(v.X, to.X), Near(v.Y, to.Y), Near(v.Z, to.Z));

    private static float Near(float v, float to) => v + 360f * MathF.Round((to - v) / 360f);
}
