using SoulsFormats;
using System;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Box maths for the Forbidden Region Editor, on SoulsFormats and System.Numerics only, so
/// Claude/Tools/MfrSurvey can compile this file and check it against the game's boxes (mode "edit").
/// An entry's matrix rows are its axes and then its centre (<see cref="MFR.Entry.Transform"/>), in map space.
/// </summary>
public static class MfrBoxMath
{
    /// <summary>
    /// True when the box's axes are mirrored (left-handed). A quaternion can't hold a mirror, so
    /// <see cref="GetRotation"/> and <see cref="SetRotation"/> flip the Z axis around it.
    /// </summary>
    public static bool IsMirrored(MFR.Entry entry)
    {
        return Vector3.Dot(Vector3.Cross(entry.AxisX, entry.AxisY), entry.AxisZ) < 0;
    }

    /// <summary>The box's rotation, with the Z axis flipped back first if the box is mirrored.</summary>
    public static Quaternion GetRotation(MFR.Entry entry)
    {
        var z = IsMirrored(entry) ? -entry.AxisZ : entry.AxisZ;

        var m = new Matrix4x4(
            entry.AxisX.X, entry.AxisX.Y, entry.AxisX.Z, 0,
            entry.AxisY.X, entry.AxisY.Y, entry.AxisY.Z, 0,
            z.X, z.Y, z.Z, 0,
            0, 0, 0, 1);

        return Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(m));
    }

    /// <summary>Sets the box's axes from a rotation, keeping it mirrored if it was.</summary>
    public static void SetRotation(MFR.Entry entry, Quaternion rotation, bool mirrored)
    {
        var m = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(rotation));

        entry.AxisX = new Vector3(m.M11, m.M12, m.M13);
        entry.AxisY = new Vector3(m.M21, m.M22, m.M23);

        var z = new Vector3(m.M31, m.M32, m.M33);
        entry.AxisZ = mirrored ? -z : z;
    }

    /// <summary>
    /// The unit box drawn for an entry (-0.5..0.5 in X and Z, 0..1 in Y, as the box region primitive) sized and
    /// placed in map space.
    /// </summary>
    public static Matrix4x4 GetDrawMatrix(MFR.Entry entry)
    {
        var h = entry.HalfExtents;

        return Matrix4x4.CreateScale(h * 2)
            * Matrix4x4.CreateTranslation(0, -h.Y, 0)
            * entry.Transform;
    }

    /// <summary>
    /// Where a ray (in map space) enters the box, as a distance along the direction, or null if it misses.
    /// A ray starting inside the box hits at 0.
    /// </summary>
    public static float? RayHit(MFR.Entry entry, Vector3 origin, Vector3 direction)
    {
        // Into the box's frame: its axes are unit length, so dot products give local coordinates
        var d = origin - entry.Center;
        Vector3 o = new(Vector3.Dot(d, entry.AxisX), Vector3.Dot(d, entry.AxisY), Vector3.Dot(d, entry.AxisZ));
        Vector3 v = new(Vector3.Dot(direction, entry.AxisX), Vector3.Dot(direction, entry.AxisY), Vector3.Dot(direction, entry.AxisZ));
        var h = entry.HalfExtents;

        float tMin = float.NegativeInfinity, tMax = float.PositiveInfinity;

        for (var i = 0; i < 3; i++)
        {
            float oi = i == 0 ? o.X : i == 1 ? o.Y : o.Z;
            float vi = i == 0 ? v.X : i == 1 ? v.Y : v.Z;
            float hi = i == 0 ? h.X : i == 1 ? h.Y : h.Z;

            if (MathF.Abs(vi) < 1e-9f)
            {
                if (oi < -hi || oi > hi)
                    return null;
                continue;
            }

            var t1 = (-hi - oi) / vi;
            var t2 = (hi - oi) / vi;

            if (t1 > t2)
                (t1, t2) = (t2, t1);

            tMin = MathF.Max(tMin, t1);
            tMax = MathF.Min(tMax, t2);

            if (tMin > tMax)
                return null;
        }

        if (tMax < 0)
            return null;

        return MathF.Max(tMin, 0);
    }

    /// <summary>An axis-aligned box spanning min..max (map space), of at least the given half size.</summary>
    public static MFR.Entry FromBounds(Vector3 min, Vector3 max, MFR.EntryType type, float minHalf = 0.05f)
    {
        var half = Vector3.Max((max - min) / 2, new Vector3(minHalf));

        return new MFR.Entry
        {
            Type = type,
            HalfExtents = half,
            Center = (min + max) / 2
        };
    }

    /// <summary>A copy of an entry (its padding bytes included).</summary>
    public static MFR.Entry Copy(MFR.Entry entry)
    {
        var copy = new MFR.Entry
        {
            Cell = entry.Cell,
            Type = entry.Type,
            HalfExtents = entry.HalfExtents,
            HalfExtentsW = entry.HalfExtentsW,
            AxisX = entry.AxisX,
            AxisY = entry.AxisY,
            AxisZ = entry.AxisZ,
            Center = entry.Center
        };

        entry.Padding05.CopyTo(copy.Padding05, 0);

        return copy;
    }
}
