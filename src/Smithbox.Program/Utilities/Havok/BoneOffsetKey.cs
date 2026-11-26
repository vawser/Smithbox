#nullable enable
using System.Numerics;

namespace StudioCore.Utilities;

/// <summary>
///     An offset key on one bone of an <see cref="EditableClip" />, at a whole frame: a rotation and a translation in
///     the bone's parent space, put on top of the clip's own pose (<see cref="EditableClip.ApplyOffset" />).
/// </summary>
public readonly record struct BoneOffsetKey(int Frame, Quaternion Rotation, Vector3 Translation)
{
    public static BoneOffsetKey Identity(int frame) => new(frame, Quaternion.Identity, Vector3.Zero);

    public bool IsIdentity => Translation == Vector3.Zero &&
                              System.Math.Abs(System.Math.Abs(Quaternion.Dot(Rotation, Quaternion.Identity)) - 1) < 1e-7f;
}

/// <summary>How an offset goes from one key to the next</summary>
public enum BoneOffsetEase
{
    /// <summary>At an even rate</summary>
    Linear,

    /// <summary>Starting and ending gently (smoothstep), so a bone eases into and out of its offset</summary>
    Smooth
}
