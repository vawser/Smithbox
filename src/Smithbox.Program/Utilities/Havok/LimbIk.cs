#nullable enable
using StudioCore.Renderer;
using System;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities;

/// <summary>
///     A limb for <see cref="LimbIk" />: the end bone (hand, foot), its parent (forearm, calf) and grandparent (upper
///     arm, thigh), each with its track in the clip
/// </summary>
public readonly record struct IkChain(int Upper, int Lower, int End, int UpperTrack, int LowerTrack, int EndTrack);

/// <summary>
///     Two-bone IK (Animation Editor Analysis.md §10, P8): the upper and lower bones of a limb turned so its end bone
///     reaches a model-space point. The knee or elbow stays on the side it points to now: the new joint lies in the
///     plane of the reach and the joint's current direction, at the angle the bone lengths give (law of cosines), and
///     each bone is turned the shortest way onto its new direction, so it keeps its twist. The end bone keeps its
///     model-space rotation (a planted foot stays flat), or turns with the limb.
///     <para>
///         In vanilla ER skeletons the chains are direct parents (L_Thigh → L_Calf → L_Foot, L_UpperArm → L_Forearm →
///         L_Hand); twist, knee and elbow helpers hang off them as siblings, so they follow.
///     </para>
/// </summary>
public static class LimbIk
{
    private const float Epsilon = 1e-5f;

    /// <summary>
    ///     The limb ending at a bone: its parent and grandparent, all three animated by the clip and with a length.
    ///     Null otherwise (a bone near the top of the skeleton, or one the clip doesn't animate).
    /// </summary>
    public static IkChain? ChainOf(EditableClip clip, HavokSkeletonData skeleton, int end)
    {
        if (end < 0 || end >= skeleton.BoneCount)
            return null;

        var lower = Parent(skeleton, end);
        var upper = lower >= 0 ? Parent(skeleton, lower) : -1;
        if (upper < 0)
            return null;

        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);
        if (boneToTrack[upper] < 0 || boneToTrack[lower] < 0 || boneToTrack[end] < 0)
            return null;

        // Both bones need a length, or there is no limb to bend
        var rest = EditableClip.ModelSpace(skeleton, b => skeleton.ReferencePose[b]);
        if ((rest[lower].Position - rest[upper].Position).Length() < 1e-3f ||
            (rest[end].Position - rest[lower].Position).Length() < 1e-3f)
            return null;

        return new IkChain(upper, lower, end, boneToTrack[upper], boneToTrack[lower], boneToTrack[end]);
    }

    /// <summary>
    ///     The model-space rotations of the chain's bones so the end bone is at <paramref name="target" /> (or as near as
    ///     the limb reaches, fully stretched). <paramref name="endRotation" />: the end bone's model rotation to keep, or
    ///     null for it to turn with the limb.
    /// </summary>
    public static (Quaternion Upper, Quaternion Lower, Quaternion End) Solve(
        (Vector3 Position, Quaternion Rotation, Vector3 Scale)[] pose, IkChain chain, Vector3 target, Quaternion? endRotation)
    {
        var a = pose[chain.Upper].Position;
        var b = pose[chain.Lower].Position;
        var c = pose[chain.End].Position;
        var upperRotation = pose[chain.Upper].Rotation;
        var lowerRotation = pose[chain.Lower].Rotation;
        var endModel = pose[chain.End].Rotation;

        var upperLength = (b - a).Length();
        var lowerLength = (c - b).Length();
        var toTarget = target - a;
        if (toTarget.Length() < Epsilon || upperLength < Epsilon || lowerLength < Epsilon)
            return (upperRotation, lowerRotation, endRotation ?? endModel);

        // How far the end is from the upper joint: within what the bones reach (not quite straight, so the bend side holds)
        var reach = Math.Clamp(toTarget.Length(), Math.Abs(upperLength - lowerLength) + Epsilon,
            (upperLength + lowerLength) * 0.9999f);
        var direction = Vector3.Normalize(toTarget);

        // The side the joint goes to: square to the reach about the limb's hinge (the normal of its bend), the same way
        // round as now, so an elbow or knee keeps bending the way it bends. Taking the joint's own offset off the new
        // reach line instead bent it backwards when the reach swung past it (ER c0000, 4 of 80 reaches).
        var joint = b - a;
        var hinge = Vector3.Cross(b - a, c - b);
        var side = Vector3.Cross(hinge, direction);
        if (hinge.LengthSquared() > Epsilon * Epsilon && side.LengthSquared() > Epsilon * Epsilon)
        {
            var now = Vector3.Cross(hinge, c - a);
            if (Vector3.Dot(now, joint) < 0)
                side = -side;
        }
        else
        {
            // A straight limb, or a reach along the hinge: the joint's offset off the reach line, else any square side
            side = joint - Vector3.Dot(joint, direction) * direction;
            if (side.LengthSquared() < Epsilon * Epsilon)
                side = Vector3.Cross(direction, MathF.Abs(direction.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitX);
        }
        side = Vector3.Normalize(side);

        var cos = Math.Clamp((upperLength * upperLength + reach * reach - lowerLength * lowerLength) / (2 * upperLength * reach), -1f, 1f);
        var sin = MathF.Sqrt(1 - cos * cos);
        var newJoint = a + upperLength * (cos * direction + sin * side);
        var newEnd = a + reach * direction;

        var turnUpper = Between(b - a, newJoint - a);
        var lowerAfter = Vector3.Transform(c - b, turnUpper);
        var turnLower = Between(lowerAfter, newEnd - newJoint);

        var newUpper = Quaternion.Normalize(turnUpper * upperRotation);
        var newLower = Quaternion.Normalize(turnLower * turnUpper * lowerRotation);
        var newEndRotation = endRotation ?? Quaternion.Normalize(turnLower * turnUpper * endModel);
        return (newUpper, newLower, newEndRotation);
    }

    /// <summary>
    ///     The chain's new local rotations from its new model rotations, with the upper bone's parent as the pose has
    ///     it (IK doesn't move it)
    /// </summary>
    private static (Quaternion Upper, Quaternion Lower, Quaternion End) ToLocal(HavokSkeletonData skeleton,
        (Vector3 Position, Quaternion Rotation, Vector3 Scale)[] pose, IkChain chain,
        (Quaternion Upper, Quaternion Lower, Quaternion End) model)
    {
        var parent = Parent(skeleton, chain.Upper);
        var parentRotation = parent >= 0 ? pose[parent].Rotation : Quaternion.Identity;
        return (Quaternion.Normalize(Quaternion.Inverse(parentRotation) * model.Upper),
            Quaternion.Normalize(Quaternion.Inverse(model.Upper) * model.Lower),
            Quaternion.Normalize(Quaternion.Inverse(model.Lower) * model.End));
    }

    /// <summary>
    ///     A copy with keys at <paramref name="frame" /> on the chain's three bones that put the end bone at a
    ///     model-space point (the gizmo's IK drag). Each key keeps its move; its turn is what takes the clip's own local
    ///     rotation to the solved one. <paramref name="keepEnd" />: the end bone keeps the model rotation it has there.
    /// </summary>
    public static EditableClip KeyedReach(EditableClip clip, HavokSkeletonData skeleton, IkChain chain, int frame,
        Vector3 target, bool keepEnd)
    {
        var pose = clip.ModelPose(skeleton, frame);
        var model = Solve(pose, chain, target, keepEnd ? pose[chain.End].Rotation : null);
        var local = ToLocal(skeleton, pose, chain, model);

        var result = clip;
        foreach (var (track, rotation) in new[] { (chain.UpperTrack, local.Upper), (chain.LowerTrack, local.Lower), (chain.EndTrack, local.End) })
        {
            var own = clip.Sample(track, frame).Rotation;
            var existing = clip.OffsetAt(track, frame);
            result = result.WithKey(track, new BoneOffsetKey(frame, Quaternion.Normalize(rotation * Quaternion.Inverse(own)),
                existing.Translation));
        }

        return result;
    }

    /// <summary>
    ///     Pins the chain's end bone where it is on <paramref name="first" /> frame, in the world (along the root motion,
    ///     as the game moves the character), through <paramref name="last" />, easing in and out over
    ///     <paramref name="blend" /> frames either side; with <paramref name="keepRotation" /> it also keeps its turn
    ///     there. For feet that slide while standing (after a retarget or a speed change). The solved pose is written
    ///     into the clip's own frames under its offset keys (<c>frame = key⁻¹ · solved</c>), so every key stays and the
    ///     clip plays the solved pose with them.
    /// </summary>
    public static EditableClip Plant(EditableClip clip, HavokSkeletonData skeleton, IkChain chain, int first, int last,
        int blend, bool keepRotation)
    {
        if (clip.FrameCount == 0)
            return clip;

        first = Math.Clamp(first, 0, clip.FrameCount - 1);
        last = Math.Clamp(last, first, clip.FrameCount - 1);
        blend = Math.Max(blend, 0);

        var pinPose = clip.ModelPose(skeleton, first);
        var pinRoot = clip.RootMatrix(first);
        var pinWorld = Vector3.Transform(pinPose[chain.End].Position, pinRoot);
        var pinWorldRotation = Quaternion.Normalize(Quaternion.CreateFromRotationMatrix(pinRoot) * pinPose[chain.End].Rotation);

        var frames = clip.Frames.Select(f => f.ToArray()).ToArray();
        for (var f = Math.Max(0, first - blend); f <= Math.Min(clip.FrameCount - 1, last + blend); f++)
        {
            var weight = f < first ? Smooth((f - (first - blend - 1)) / (float)(blend + 1))
                : f > last ? Smooth(((last + blend + 1) - f) / (float)(blend + 1))
                : 1f;
            if (weight <= 0)
                continue;

            // The pin seen from where the root motion has the character on this frame
            var root = clip.RootMatrix(f);
            Matrix4x4.Invert(root, out var toModel);
            var rootRotation = Quaternion.CreateFromRotationMatrix(root);

            var pose = clip.ModelPose(skeleton, f);
            var pinned = Vector3.Transform(pinWorld, toModel);
            var target = Vector3.Lerp(pose[chain.End].Position, pinned, weight);

            Quaternion? endRotation = null;
            if (keepRotation)
            {
                var pinnedRotation = Quaternion.Normalize(Quaternion.Inverse(rootRotation) * pinWorldRotation);
                endRotation = Slerp(pose[chain.End].Rotation, pinnedRotation, weight);
            }

            var local = ToLocal(skeleton, pose, chain, Solve(pose, chain, target, endRotation));
            SetUnderKeys(clip, frames, chain.UpperTrack, f, local.Upper);
            SetUnderKeys(clip, frames, chain.LowerTrack, f, local.Lower);
            SetUnderKeys(clip, frames, chain.EndTrack, f, local.End);
        }

        return clip.WithFrames(frames, clip.Floats.Select(f => f.ToArray()).ToArray(), clip.RootMotion?.ToArray());
    }

    /// <summary>A track's frame set so that, with the track's offset there, it plays <paramref name="rotation" /></summary>
    private static void SetUnderKeys(EditableClip clip, HavokTransformData[][] frames, int track, int frame, Quaternion rotation)
    {
        var offset = clip.OffsetAt(track, frame).Rotation;
        frames[frame][track].Rotation = Quaternion.Normalize(Quaternion.Inverse(offset) * rotation);
    }

    private static float Smooth(float t)
    {
        t = Math.Clamp(t, 0, 1);
        return t * t * (3 - 2 * t);
    }

    private static Quaternion Slerp(Quaternion a, Quaternion b, float t)
    {
        if (Quaternion.Dot(a, b) < 0)
            b = -b;
        return Quaternion.Normalize(Quaternion.Slerp(a, b, t));
    }

    /// <summary>The shortest turn taking direction <paramref name="from" /> onto <paramref name="to" /></summary>
    public static Quaternion Between(Vector3 from, Vector3 to)
    {
        if (from.LengthSquared() < Epsilon * Epsilon || to.LengthSquared() < Epsilon * Epsilon)
            return Quaternion.Identity;

        from = Vector3.Normalize(from);
        to = Vector3.Normalize(to);
        var dot = Vector3.Dot(from, to);
        if (dot > 1 - 1e-7f)
            return Quaternion.Identity;

        if (dot < -1 + 1e-6f)
        {
            // Opposite: half a turn about any axis square to them
            var axis = Vector3.Cross(from, MathF.Abs(from.X) < 0.9f ? Vector3.UnitX : Vector3.UnitY);
            return Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), MathF.PI);
        }

        var cross = Vector3.Cross(from, to);
        return Quaternion.Normalize(new Quaternion(cross, 1 + dot));
    }

    private static int Parent(HavokSkeletonData skeleton, int bone)
    {
        var p = skeleton.Parents[bone];
        return p >= 0 && p < skeleton.BoneCount && p != bone ? p : -1;
    }
}
