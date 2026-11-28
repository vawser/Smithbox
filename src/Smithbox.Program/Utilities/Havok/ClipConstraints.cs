#nullable enable
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities;

/// <summary>
///     Constraints baked to a layer (Animation Editor Analysis.md §13.3 item 11, D14): each is solved on every frame of
///     a range and written as keys on a new layer on top of the clip, named after it, so it can be muted, weighted or
///     removed with the layer, and applied again after the motion under it changes.
///     <list type="bullet">
///         <item>Child-of: a bone keeps where it was relative to another bone on a reference frame (the off hand on
///         the weapon hand). A limb's end bone is reached with two-bone IK; any other bone is moved and turned itself.</item>
///         <item>Aim: a bone turns so the local axis that points nearest the target on the first frame points at it:
///         another bone's position plus an offset, or a point (model space).</item>
///     </list>
///     Over <c>blend</c> frames either side of the range the constraint eases in and out; zero keys just outside it
///     keep the layer from acting on the rest of the clip.
/// </summary>
public static class ClipConstraints
{
    /// <summary>The clip with the new layer (keying stays on the layer that was active) and how many frames were keyed</summary>
    public sealed record Result(EditableClip Clip, int Layer, int Frames);

    public static Result ChildOf(EditableClip clip, HavokSkeletonData skeleton, int bone, int parentBone, int reference,
        int first, int last, int blend, bool keepRotation, string layerName)
    {
        var end = clip.FrameCount - 1;
        reference = Math.Clamp(reference, 0, end);
        var refPose = clip.ModelPose(skeleton, reference);

        // Where the bone sits in the other bone's space on the reference frame
        var toParent = Quaternion.Inverse(refPose[parentBone].Rotation);
        var relPosition = Vector3.Transform(refPose[bone].Position - refPose[parentBone].Position, toParent);
        var relRotation = Quaternion.Normalize(toParent * refPose[bone].Rotation);

        var chain = LimbIk.ChainOf(clip, skeleton, bone);
        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);

        return Bake(clip, skeleton, first, last, blend, layerName, (layered, pose, f, weight, keys) =>
        {
            var targetPosition = pose[parentBone].Position + Vector3.Transform(relPosition, pose[parentBone].Rotation);
            var targetRotation = Quaternion.Normalize(pose[parentBone].Rotation * relRotation);
            targetPosition = Vector3.Lerp(pose[bone].Position, targetPosition, weight);
            targetRotation = Slerp(pose[bone].Rotation, targetRotation, weight);

            if (chain is { } limb)
            {
                var model = LimbIk.Solve(pose, limb, targetPosition, keepRotation ? targetRotation : null);
                var local = LimbIk.ToLocal(skeleton, pose, limb, model);
                Add(layered, keys, limb.UpperTrack, f, local.Upper, null);
                Add(layered, keys, limb.LowerTrack, f, local.Lower, null);
                Add(layered, keys, limb.EndTrack, f, local.End, null);
            }
            else if (boneToTrack[bone] is var track and >= 0)
            {
                // Not a limb's end: the bone itself goes there, in its parent's space
                var parent = LimbIk.Parent(skeleton, bone);
                var (pp, pr, ps) = parent >= 0 ? pose[parent] : (Vector3.Zero, Quaternion.Identity, Vector3.One);
                var inParent = Vector3.Transform(targetPosition - pp, Quaternion.Inverse(pr));
                var localPosition = new Vector3(Divide(inParent.X, ps.X), Divide(inParent.Y, ps.Y), Divide(inParent.Z, ps.Z));
                var localRotation = keepRotation ? Quaternion.Normalize(Quaternion.Inverse(pr) * targetRotation) : (Quaternion?)null;
                Add(layered, keys, track, f, localRotation, localPosition);
            }
        });
    }

    public static Result Aim(EditableClip clip, HavokSkeletonData skeleton, int bone, int targetBone, Vector3 offset,
        int first, int last, int blend, string layerName)
    {
        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);
        var track = boneToTrack[bone];
        var parent = LimbIk.Parent(skeleton, bone);

        Vector3 Target((Vector3 Position, Quaternion Rotation, Vector3 Scale)[] pose) =>
            (targetBone >= 0 ? pose[targetBone].Position : Vector3.Zero) + offset;

        // The bone's local axis that points nearest the target on the first frame
        var axis = AimAxis(clip, skeleton, bone, targetBone, offset, first);

        return Bake(clip, skeleton, first, last, blend, layerName, (layered, pose, f, weight, keys) =>
        {
            if (track < 0)
                return;

            var to = Target(pose) - pose[bone].Position;
            if (to.LengthSquared() < 1e-10f)
                return;

            var current = Vector3.Transform(axis, pose[bone].Rotation);
            var turn = Slerp(Quaternion.Identity, LimbIk.Between(current, Vector3.Normalize(to)), weight);
            var model = Quaternion.Normalize(turn * pose[bone].Rotation);
            var parentRotation = parent >= 0 ? pose[parent].Rotation : Quaternion.Identity;
            Add(layered, keys, track, f, Quaternion.Normalize(Quaternion.Inverse(parentRotation) * model), null);
        });
    }

    /// <summary>The local axis Aim uses for a bone and target on the first frame (for the UI and the checks)</summary>
    public static Vector3 AimAxis(EditableClip clip, HavokSkeletonData skeleton, int bone, int targetBone, Vector3 offset, int first)
    {
        var start = clip.ModelPose(skeleton, Math.Clamp(first, 0, clip.FrameCount - 1));
        var to = (targetBone >= 0 ? start[targetBone].Position : Vector3.Zero) + offset - start[bone].Position;
        if (to.LengthSquared() < 1e-10f)
            return Vector3.UnitZ;

        var direction = Vector3.Normalize(to);
        return new[] { Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY, -Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitZ }
            .OrderByDescending(a => Vector3.Dot(Vector3.Transform(a, start[bone].Rotation), direction)).First();
    }

    // ---- shared

    private delegate void Solver(EditableClip layered, (Vector3 Position, Quaternion Rotation, Vector3 Scale)[] pose, int frame,
        float weight, Dictionary<int, List<BoneOffsetKey>> keys);

    /// <summary>
    ///     A new layer on top, keyed by <paramref name="solve" /> on every frame of the range and its blend frames (each
    ///     against the clip's pose there, which the new layer doesn't change), with zero keys just outside; keying stays
    ///     on the layer that was active
    /// </summary>
    private static Result Bake(EditableClip clip, HavokSkeletonData skeleton, int first, int last, int blend, string layerName, Solver solve)
    {
        var end = clip.FrameCount - 1;
        first = Math.Clamp(first, 0, end);
        last = Math.Clamp(last, first, end);
        blend = Math.Max(blend, 0);

        var layered = clip.WithNewLayer(layerName);
        var keys = new Dictionary<int, List<BoneOffsetKey>>();
        var frames = 0;

        for (var f = Math.Max(0, first - blend); f <= Math.Min(end, last + blend); f++)
        {
            var weight = f < first ? Smooth((f - (first - blend - 1)) / (float)(blend + 1))
                : f > last ? Smooth((last + blend + 1 - f) / (float)(blend + 1))
                : 1f;

            solve(layered, clip.ModelPose(skeleton, f), f, weight, keys);
            frames++;
        }

        // Zero keys just outside, so the layer leaves the rest of the clip as it is
        foreach (var list in keys.Values)
        {
            var before = first - blend - 1;
            var after = last + blend + 1;
            if (before >= 0)
                list.Add(BoneOffsetKey.Identity(before));
            if (after <= end)
                list.Add(BoneOffsetKey.Identity(after));
            list.Sort((a, b) => a.Frame.CompareTo(b.Frame));
        }

        var result = layered.Clone();
        result.Layers[^1].Keys = keys;
        result.ActiveLayer = Math.Clamp(clip.ActiveLayer, 0, result.Layers.Count - 1);
        return new Result(result, result.Layers.Count - 1, frames);
    }

    /// <summary>The new layer's key at a frame that makes a track play this local rotation / position (null keeps it)</summary>
    private static void Add(EditableClip layered, Dictionary<int, List<BoneOffsetKey>> keys, int track, int frame,
        Quaternion? localRotation, Vector3? localPosition)
    {
        if (track < 0 || layered.ActiveKeyFor(track, frame, localRotation, localPosition) is not { } key)
            return;

        if (!keys.TryGetValue(track, out var list))
            keys[track] = list = new List<BoneOffsetKey>();
        list.Add(key with { Ease = BoneOffsetEase.Linear });
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

    private static float Divide(float a, float b) => MathF.Abs(b) > 1e-8f ? a / b : a;
}
