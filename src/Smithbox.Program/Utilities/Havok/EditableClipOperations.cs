#nullable enable
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities;

/// <summary>
///     Whole-clip edits on an <see cref="EditableClip" /> (Animation Editor Analysis.md P1, kind A). Each returns a
///     new clip and leaves its input as it was. Root motion is edited with the poses: a clip that starts later starts
///     its root at zero again, a reversed clip walks its path backwards from where it ended, a spliced clip carries on
///     from where the first left the character.
/// </summary>
public static class EditableClipOperations
{
    /// <summary>The frames from <paramref name="first" /> to <paramref name="last" />, both kept</summary>
    public static EditableClip Trim(EditableClip clip, int first, int last)
    {
        first = Math.Clamp(first, 0, clip.FrameCount - 1);
        last = Math.Clamp(last, 0, clip.FrameCount - 1);
        if (last - first < 1)
            throw new ArgumentException("A trimmed clip needs at least two frames.");

        var count = last - first + 1;
        var frames = clip.Frames.Skip(first).Take(count).Select(p => p.ToArray()).ToArray();
        var floats = clip.Floats.Skip(first).Take(count).Select(p => p.ToArray()).ToArray();

        Vector4[]? root = null;
        if (clip.RootMotion is { } motion)
        {
            var up = Up(clip);
            var start = motion[first];
            root = Enumerable.Range(first, count).Select(f => Relative(start, motion[f], up)).ToArray();
        }

        return clip.WithFrames(frames, floats, root);
    }

    /// <summary>
    ///     Plays the clip over a new number of frames (at the same frame rate), so faster when there are fewer. The
    ///     root motion covers the same path in the new time.
    /// </summary>
    public static EditableClip Retime(EditableClip clip, int frameCount)
    {
        if (frameCount < 2)
            throw new ArgumentException("A clip needs at least two frames.");

        var scale = (float)(clip.FrameCount - 1) / (frameCount - 1);
        var frames = new HavokTransformData[frameCount][];
        var floats = new float[frameCount][];
        var root = clip.RootMotion != null ? new Vector4[frameCount] : null;

        for (var f = 0; f < frameCount; f++)
        {
            var at = f * scale;
            frames[f] = Enumerable.Range(0, clip.TrackCount).Select(t => clip.Sample(t, at)).ToArray();
            floats[f] = Enumerable.Range(0, clip.FloatTrackCount).Select(t => clip.SampleFloat(t, at)).ToArray();
            if (root != null)
                root[f] = SampleRoot(clip.RootMotion!, at);
        }

        return clip.WithFrames(frames, floats, root);
    }

    /// <summary>
    ///     The largest angle (degrees) a track turns between two frames. A retime to fewer frames multiplies it; past
    ///     180 degrees between the new frames a bone turns the short way round instead (a fast spin goes backwards),
    ///     so warn before <see cref="Retime" /> when this times the speed-up passes 180. Vanilla ER has clips at 138.
    /// </summary>
    public static float LargestRotationStep(EditableClip clip)
    {
        var largest = 0f;
        for (var f = 1; f < clip.FrameCount; f++)
        {
            for (var t = 0; t < clip.TrackCount; t++)
            {
                var a = Quaternion.Normalize(clip.Frames[f - 1][t].Rotation);
                var b = Quaternion.Normalize(clip.Frames[f][t].Rotation);
                var chord = MathF.Min((a - b).Length(), (a + b).Length());
                largest = MathF.Max(largest, 4 * MathF.Asin(MathF.Min(1, chord / 2)) * 180 / MathF.PI);
            }
        }

        return largest;
    }

    /// <summary>
    ///     Scales the clip's root motion: how far it travels along the ground, how far up or down, and how far it
    ///     turns, each by its own factor (0 removes that part: vertical 0 flattens the motion). The poses are left as
    ///     they are, so feet slide a little when the ground distance changes much.
    /// </summary>
    public static EditableClip ScaleRootMotion(EditableClip clip, float ground, float vertical, float turn)
    {
        if (clip.RootMotion is not { } motion)
            throw new ArgumentException("The clip has no root motion.");

        var up = clip.UpAxis;
        var scaled = motion.Select(s =>
        {
            var p = new Vector3(s.X, s.Y, s.Z);
            var upward = up * Vector3.Dot(p, up);
            var along = p - upward;
            var result = along * ground + upward * vertical;
            return new Vector4(result, s.W * turn);
        }).ToArray();

        return clip.WithFrames(clip.Frames.Select(p => p.ToArray()).ToArray(), clip.Floats.Select(p => p.ToArray()).ToArray(), scaled);
    }

    /// <summary>Retime by a speed: 2 plays twice as fast (half the frames), 0.5 half as fast</summary>
    public static EditableClip Speed(EditableClip clip, float speed)
    {
        if (speed <= 0)
            throw new ArgumentException("The speed must be above 0.");

        return Retime(clip, Math.Max((int)MathF.Round((clip.FrameCount - 1) / speed) + 1, 2));
    }

    /// <summary>The frames in reverse order; the root goes back along its path from where the clip ended</summary>
    public static EditableClip Reverse(EditableClip clip)
    {
        var frames = clip.Frames.Reverse().Select(p => p.ToArray()).ToArray();
        var floats = clip.Floats.Reverse().Select(p => p.ToArray()).ToArray();

        Vector4[]? root = null;
        if (clip.RootMotion is { } motion)
        {
            var up = Up(clip);
            var end = motion[^1];
            root = motion.Reverse().Select(s => Relative(end, s, up)).ToArray();
        }

        return clip.WithFrames(frames, floats, root);
    }

    /// <summary>
    ///     <paramref name="first" /> then <paramref name="second" />. With <paramref name="blendFrames" /> above 0,
    ///     the last frames of the first clip crossfade into the first frames of the second over that many frames
    ///     (the clips overlap there). The second clip's root motion carries on from where the first left the root.
    ///     Both clips must bind the same tracks (as two clips of one character do).
    /// </summary>
    public static EditableClip Splice(EditableClip first, EditableClip second, int blendFrames)
    {
        if (first.TrackCount != second.TrackCount || !first.TrackToBone.SequenceEqual(second.TrackToBone))
            throw new ArgumentException("The clips don't animate the same bones in the same order.");

        if (first.FloatTrackCount != second.FloatTrackCount)
            throw new ArgumentException("The clips don't have the same float tracks.");

        if (first.BlendMode != second.BlendMode)
            throw new ArgumentException("One clip is additive and the other isn't.");

        blendFrames = Math.Clamp(blendFrames, 0, Math.Min(first.FrameCount, second.FrameCount) - 1);

        // Frame of the result where the second clip's frame 0 plays
        var offset = first.FrameCount - blendFrames;
        var count = offset + second.FrameCount;

        var frames = new HavokTransformData[count][];
        var floats = new float[count][];

        var hasRoot = first.RootMotion != null || second.RootMotion != null;
        var root = hasRoot ? new Vector4[count] : null;
        var up = Up(first);
        var firstRoot = first.RootMotion ?? new Vector4[first.FrameCount];
        var secondRoot = second.RootMotion ?? new Vector4[second.FrameCount];

        for (var f = 0; f < count; f++)
        {
            var inFirst = f < first.FrameCount;
            var inSecond = f >= offset;
            var j = f - offset;

            if (inFirst && inSecond)
            {
                // Towards the second clip over the overlap, never wholly either at its ends
                var weight = (float)(j + 1) / (blendFrames + 1);
                frames[f] = Enumerable.Range(0, first.TrackCount)
                    .Select(t => EditableClip.Blend(first.Frames[f][t], second.Frames[j][t], weight)).ToArray();
                floats[f] = Enumerable.Range(0, first.FloatTrackCount)
                    .Select(t => first.Floats[f][t] + (second.Floats[j][t] - first.Floats[f][t]) * weight).ToArray();
            }
            else if (inFirst)
            {
                frames[f] = first.Frames[f].ToArray();
                floats[f] = first.Floats[f].ToArray();
            }
            else
            {
                frames[f] = second.Frames[j].ToArray();
                floats[f] = second.Floats[j].ToArray();
            }
        }

        // The root moves by each clip's own steps (how far it goes and turns from one frame to the next, seen from
        // where it is), the steps crossfaded over the overlap and added up. Blending the steps rather than the
        // positions keeps the path whole: two clips moving differently over the overlap would otherwise pull the
        // root back to the second clip's path when the blend ends.
        if (root != null)
        {
            root[0] = firstRoot[0];
            for (var f = 1; f < count; f++)
            {
                var j = f - offset;
                Vector4? stepFirst = f < first.FrameCount ? Relative(firstRoot[f - 1], firstRoot[f], up) : null;
                // The second clip has no step into its frame 0: it starts where the first clip has the root
                Vector4? stepSecond = j >= 1 && j < second.FrameCount ? Relative(secondRoot[j - 1], secondRoot[j], up) : null;

                Vector4 step;
                if (stepFirst is { } a && stepSecond is { } b)
                    step = Vector4.Lerp(a, b, (float)(j + 1) / (blendFrames + 1));
                else
                    step = stepFirst ?? stepSecond ?? Vector4.Zero;

                root[f] = Compose(root[f - 1], step, up);
            }
        }

        return first.WithFrames(frames, floats, root);
    }

    /// <summary>
    ///     The clip as its mirror image across the character's side-to-side plane (the plane of its root motion's up
    ///     and forward). Each bone takes its mirror bone's motion (<see cref="MirrorBoneName" />, or itself): the
    ///     model-space rotation the mirror bone has away from its reference pose, mirrored, applied to this bone's own
    ///     reference pose, and the mirror bone's position, mirrored. So it holds for any rest pose, as long as the
    ///     skeleton itself is symmetric. The root turns the other way. Not for additive clips.
    /// </summary>
    public static EditableClip Mirror(EditableClip clip, HavokSkeletonData skeleton, Func<string, string>? mirrorName = null)
    {
        if (clip.BlendMode != HavokBlendMode.Normal)
            throw new ArgumentException("An additive clip can't be mirrored.");

        mirrorName ??= MirrorBoneName;
        var boneCount = skeleton.BoneCount;
        var up = Up(clip);
        var forward = new Vector3(clip.Forward.X, clip.Forward.Y, clip.Forward.Z);
        var side = Vector3.Cross(up, forward.LengthSquared() > 1e-8f ? Vector3.Normalize(forward) : Vector3.UnitZ);
        side = side.LengthSquared() > 1e-8f ? Vector3.Normalize(side) : Vector3.UnitX;

        // Mirror bone of each bone, by name; a name without a match (or a centre bone) is its own
        var byName = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var b = 0; b < boneCount; b++)
            byName.TryAdd(skeleton.Names[b], b);
        var mirror = new int[boneCount];
        for (var b = 0; b < boneCount; b++)
            mirror[b] = byName.TryGetValue(mirrorName(skeleton.Names[b]), out var m) ? m : b;

        var boneToTrack = Enumerable.Repeat(-1, boneCount).ToArray();
        for (var t = 0; t < clip.TrackCount; t++)
        {
            var bone = clip.BoneOf(t);
            if (bone >= 0 && bone < boneCount)
                boneToTrack[bone] = t;
        }

        var rest = ModelSpace(skeleton, b => skeleton.ReferencePose[b]);

        var frames = new HavokTransformData[clip.FrameCount][];
        for (var f = 0; f < clip.FrameCount; f++)
        {
            var frame = f;
            var pose = ModelSpace(skeleton, b => boneToTrack[b] >= 0 ? clip.Frames[frame][boneToTrack[b]] : skeleton.ReferencePose[b]);

            var mirrored = new (Vector3 Position, Quaternion Rotation, Vector3 Scale)[boneCount];
            for (var b = 0; b < boneCount; b++)
            {
                var s = mirror[b];
                var delta = pose[s].Rotation * Quaternion.Inverse(rest[s].Rotation);
                mirrored[b] = (Reflect(pose[s].Position, side),
                    Quaternion.Normalize(ReflectRotation(delta, side) * rest[b].Rotation),
                    pose[s].Scale);
            }

            // Back to local transforms, for the tracks the clip has
            var local = new HavokTransformData[clip.TrackCount];
            for (var t = 0; t < clip.TrackCount; t++)
            {
                var b = clip.BoneOf(t);
                if (b < 0 || b >= boneCount)
                {
                    local[t] = clip.Frames[f][t];
                    continue;
                }

                var parent = skeleton.Parents[b];
                var sourceScale = boneToTrack[mirror[b]] >= 0
                    ? clip.Frames[f][boneToTrack[mirror[b]]].Scale
                    : skeleton.ReferencePose[mirror[b]].Scale;

                if (parent < 0)
                {
                    local[t] = new HavokTransformData
                    {
                        Translation = mirrored[b].Position,
                        Rotation = mirrored[b].Rotation,
                        Scale = sourceScale
                    };
                    continue;
                }

                var inverseParent = Quaternion.Inverse(mirrored[parent].Rotation);
                var offset = Vector3.Transform(mirrored[b].Position - mirrored[parent].Position, inverseParent);
                var parentScale = mirrored[parent].Scale;
                local[t] = new HavokTransformData
                {
                    Translation = new Vector3(
                        SafeDivide(offset.X, parentScale.X), SafeDivide(offset.Y, parentScale.Y), SafeDivide(offset.Z, parentScale.Z)),
                    Rotation = Quaternion.Normalize(inverseParent * mirrored[b].Rotation),
                    Scale = sourceScale
                };
            }

            frames[f] = local;
        }

        Vector4[]? root = clip.RootMotion?.Select(s =>
        {
            var p = Reflect(new Vector3(s.X, s.Y, s.Z), side);
            return new Vector4(p, -s.W);
        }).ToArray();

        return clip.WithFrames(frames, clip.Floats.Select(p => p.ToArray()).ToArray(), root);
    }

    /// <summary>
    ///     The bones whose motion <see cref="Mirror" /> can't carry over: each has a track in the clip but its mirror
    ///     bone has none, so the mirror bone can't take its motion (a clip only animates the bones its binding lists)
    ///     and it is left at the mirror bone's reference pose. Partial clips of AC6 and SDT have these (up to 30 bones);
    ///     ER, NR, DB and DS3 clips sampled had none. Empty when the mirror loses nothing.
    /// </summary>
    public static IReadOnlyList<string> MirrorUnpairedBones(EditableClip clip, HavokSkeletonData skeleton,
        Func<string, string>? mirrorName = null)
    {
        mirrorName ??= MirrorBoneName;
        var tracked = new HashSet<int>();
        for (var t = 0; t < clip.TrackCount; t++)
        {
            var bone = clip.BoneOf(t);
            if (bone >= 0 && bone < skeleton.BoneCount)
                tracked.Add(bone);
        }

        var byName = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var b = 0; b < skeleton.BoneCount; b++)
            byName.TryAdd(skeleton.Names[b], b);

        return tracked
            .Where(b => byName.TryGetValue(mirrorName(skeleton.Names[b]), out var m) && m != b && !tracked.Contains(m))
            .OrderBy(b => b)
            .Select(b => skeleton.Names[b])
            .ToList();
    }

    /// <summary>
    ///     The name of a bone's mirror bone: L_/R_ (and l_/r_) at the start, _L/_R at the end, Left/Right anywhere.
    ///     A name with none of these is its own mirror.
    /// </summary>
    public static string MirrorBoneName(string name)
    {
        if (name.StartsWith("L_", StringComparison.Ordinal)) return "R_" + name[2..];
        if (name.StartsWith("R_", StringComparison.Ordinal)) return "L_" + name[2..];
        if (name.StartsWith("l_", StringComparison.Ordinal)) return "r_" + name[2..];
        if (name.StartsWith("r_", StringComparison.Ordinal)) return "l_" + name[2..];
        if (name.EndsWith("_L", StringComparison.Ordinal)) return name[..^2] + "_R";
        if (name.EndsWith("_R", StringComparison.Ordinal)) return name[..^2] + "_L";
        if (name.Contains("Left", StringComparison.Ordinal)) return name.Replace("Left", "Right", StringComparison.Ordinal);
        if (name.Contains("Right", StringComparison.Ordinal)) return name.Replace("Right", "Left", StringComparison.Ordinal);
        return name;
    }

    // Root motion samples are (position, turn about up): a point x of the character is at Rot(up, W) x + position.

    /// <summary>Sample <paramref name="b" /> seen from <paramref name="a" />: inverse(a) then b</summary>
    private static Vector4 Relative(Vector4 a, Vector4 b, Vector3 up)
    {
        var inverse = Quaternion.CreateFromAxisAngle(up, -a.W);
        var p = Vector3.Transform(new Vector3(b.X - a.X, b.Y - a.Y, b.Z - a.Z), inverse);
        return new Vector4(p, b.W - a.W);
    }

    /// <summary><paramref name="b" /> carried on from <paramref name="a" />: b, then a</summary>
    private static Vector4 Compose(Vector4 a, Vector4 b, Vector3 up)
    {
        var p = Vector3.Transform(new Vector3(b.X, b.Y, b.Z), Quaternion.CreateFromAxisAngle(up, a.W));
        return new Vector4(p.X + a.X, p.Y + a.Y, p.Z + a.Z, a.W + b.W);
    }

    private static Vector4 SampleRoot(Vector4[] samples, float frame)
    {
        frame = Math.Clamp(frame, 0, samples.Length - 1);
        var first = (int)frame;
        var second = Math.Min(first + 1, samples.Length - 1);
        return Vector4.Lerp(samples[first], samples[second], frame - first);
    }

    private static Vector3 Up(EditableClip clip)
    {
        var up = new Vector3(clip.Up.X, clip.Up.Y, clip.Up.Z);
        return up.LengthSquared() > 1e-8f ? Vector3.Normalize(up) : Vector3.UnitY;
    }

    private static (Vector3 Position, Quaternion Rotation, Vector3 Scale)[] ModelSpace(HavokSkeletonData skeleton,
        Func<int, HavokTransformData> local)
    {
        var result = new (Vector3 Position, Quaternion Rotation, Vector3 Scale)[skeleton.BoneCount];
        var done = new bool[skeleton.BoneCount];

        void Resolve(int b)
        {
            if (done[b]) return;
            var x = local(b);
            var parent = skeleton.Parents[b];
            if (parent < 0 || parent >= skeleton.BoneCount || parent == b)
            {
                result[b] = (x.Translation, Quaternion.Normalize(x.Rotation), x.Scale);
            }
            else
            {
                Resolve(parent);
                var p = result[parent];
                result[b] = (p.Position + Vector3.Transform(x.Translation * p.Scale, p.Rotation),
                    Quaternion.Normalize(p.Rotation * x.Rotation), x.Scale);
            }

            done[b] = true;
        }

        for (var b = 0; b < skeleton.BoneCount; b++)
            Resolve(b);

        return result;
    }

    private static Vector3 Reflect(Vector3 p, Vector3 normal) => p - 2 * Vector3.Dot(p, normal) * normal;

    /// <summary>
    ///     A rotation seen in a mirror: about the reflected axis, the other way round. For a unit quaternion (v, w)
    ///     that is (-reflect(v), w).
    /// </summary>
    private static Quaternion ReflectRotation(Quaternion q, Vector3 normal)
    {
        var v = -Reflect(new Vector3(q.X, q.Y, q.Z), normal);
        return new Quaternion(v, q.W);
    }

    private static float SafeDivide(float a, float b) => MathF.Abs(b) > 1e-8f ? a / b : a;
}
