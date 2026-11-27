#nullable enable
using System;
using System.Numerics;

namespace StudioCore.Utilities;

/// <summary>
///     Havok cloth relates bones and vertices to triangle frames built from the mesh (Character Scale Plan.md §4.6).
///     A frame's axes a_i are unit vectors, edge vectors (length¹) or the cross product of two edges (length²): power
///     k_i. A stored matrix M = R · A⁻¹ (row vectors: a bone R in the frame, or with R = I the frame's inverse) then
///     changes under a uniform scale s as column i × s^-k_i and translation component i × s^(1 - k_i). The 3x3's
///     inverse has rows a_i · Rᵀ, which keep the a_i's lengths and cross products, so k is read from the matrix itself.
/// </summary>
public static class ClothFrame
{
    private const float Tolerance = 2e-3f;

    /// <summary>The power of length of each frame axis (0 unit, 1 edge, 2 cross product), or null if singular</summary>
    public static int[]? AxisPowers(Matrix4x4 m)
    {
        var a = new Matrix4x4(m.M11, m.M12, m.M13, 0, m.M21, m.M22, m.M23, 0, m.M31, m.M32, m.M33, 0, 0, 0, 0, 1);
        if (!Matrix4x4.Invert(a, out var inv))
            return null;

        var rows = new[]
        {
            new Vector3(inv.M11, inv.M12, inv.M13),
            new Vector3(inv.M21, inv.M22, inv.M23),
            new Vector3(inv.M31, inv.M32, inv.M33)
        };
        if (!float.IsFinite(rows[0].X + rows[1].Y + rows[2].Z))
            return null;

        var k = new int[3];
        for (var i = 0; i < 3; i++)
        {
            var r = rows[i];
            var length = r.Length();
            if (MathF.Abs(length - 1f) < Tolerance)
            {
                k[i] = 0;
                continue;
            }

            var cross = Vector3.Cross(rows[(i + 1) % 3], rows[(i + 2) % 3]);
            var near = MathF.Min((r - cross).Length(), (r + cross).Length());
            k[i] = near < Tolerance * length ? 2 : 1;
        }

        return k;
    }

    /// <summary>A frame-relative matrix under a uniform scale s, given its axis powers</summary>
    public static Matrix4x4 ScaleFrame(Matrix4x4 m, int[] k, float s)
    {
        float Col(int i) => MathF.Pow(s, -k[i]);
        float T(int i) => MathF.Pow(s, 1 - k[i]);

        m.M11 *= Col(0); m.M21 *= Col(0); m.M31 *= Col(0);
        m.M12 *= Col(1); m.M22 *= Col(1); m.M32 *= Col(1);
        m.M13 *= Col(2); m.M23 *= Col(2); m.M33 *= Col(2);
        m.M41 *= T(0);
        m.M42 *= T(1);
        m.M43 *= T(2);
        return m;
    }
}
