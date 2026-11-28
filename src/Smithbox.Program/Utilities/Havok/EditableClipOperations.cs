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

    /// <summary>
    ///     The bones root motion can be taken out of: bones the clip animates whose parents (all the way up) it doesn't,
    ///     so the bone's model-space pose is its own track on fixed parents. The skeleton's root (Master) first.
    /// </summary>
    public static List<int> RootMotionSourceBones(EditableClip clip, HavokSkeletonData skeleton)
    {
        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);
        var result = new List<int>();
        for (var b = 0; b < skeleton.BoneCount; b++)
        {
            if (boneToTrack[b] < 0)
                continue;

            var free = true;
            for (int p = skeleton.Parents[b], guard = 0; p >= 0 && p < skeleton.BoneCount && guard < skeleton.BoneCount; p = skeleton.Parents[p], guard++)
            {
                if (boneToTrack[p] >= 0)
                {
                    free = false;
                    break;
                }
            }

            if (free)
                result.Add(b);
        }

        return result;
    }

    /// <summary>
    ///     Gives a clip without root motion the travel of one of its bones (from <see cref="RootMotionSourceBones" />):
    ///     how far the bone moves along the ground from frame 0 (and up or down with <paramref name="vertical" />), and
    ///     how far it turns about up (with <paramref name="turn" />), become root motion, and the bone keeps only what
    ///     is left. With the root motion applied (in game, or Travel in the viewport) every bone is where it was.
    /// </summary>
    public static EditableClip AddRootMotionFromBone(EditableClip clip, HavokSkeletonData skeleton, int bone,
        bool vertical, bool turn)
    {
        if (clip.RootMotion != null)
            throw new ArgumentException("The clip already has root motion.");

        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);
        var track = bone >= 0 && bone < skeleton.BoneCount ? boneToTrack[bone] : -1;
        if (track < 0 || !RootMotionSourceBones(clip, skeleton).Contains(bone))
            throw new ArgumentException("Root motion can only come from an animated bone whose parents aren't animated.");

        var up = clip.UpAxis;

        // The bone's parents don't move, so their model-space pose is the reference pose's
        var rest = EditableClip.ModelSpace(skeleton, b => skeleton.ReferencePose[b]);
        var parent = skeleton.Parents[bone];
        var hasParent = parent >= 0 && parent < skeleton.BoneCount;
        var parentRotation = hasParent ? rest[parent].Rotation : Quaternion.Identity;
        var parentPosition = hasParent ? rest[parent].Position : Vector3.Zero;
        var parentScale = hasParent ? rest[parent].Scale : Vector3.One;
        var toParent = Quaternion.Inverse(parentRotation);

        (Vector3 Position, Quaternion Rotation) Model(HavokTransformData local) =>
            (parentPosition + Vector3.Transform(local.Translation * parentScale, parentRotation),
                Quaternion.Normalize(parentRotation * local.Rotation));

        var start = Model(clip.Frames[0][track]);
        var root = new Vector4[clip.FrameCount];
        var frames = clip.Frames.Select(p => p.ToArray()).ToArray();
        var previousTurn = 0f;

        for (var f = 0; f < clip.FrameCount; f++)
        {
            var model = Model(clip.Frames[f][track]);

            // Travel from frame 0, along the ground (and up with vertical)
            var moved = model.Position - start.Position;
            var upward = up * Vector3.Dot(moved, up);
            var translation = vertical ? moved : moved - upward;

            // Turn about up since frame 0 (the twist of the change in rotation), kept adding up across ±180
            var w = 0f;
            if (turn)
            {
                var twist = TwistAngle(Quaternion.Normalize(model.Rotation * Quaternion.Inverse(start.Rotation)), up);
                w = previousTurn + WrapAngle(twist - WrapAngle(previousTurn));
                previousTurn = w;
            }

            root[f] = new Vector4(translation, w);

            // What the bone keeps: its model pose with the root motion taken back off (R⁻¹ · M)
            var rootRotation = Quaternion.CreateFromAxisAngle(up, w);
            var inverseRoot = Quaternion.Inverse(rootRotation);
            var keptPosition = Vector3.Transform(model.Position - translation, inverseRoot);
            var keptRotation = Quaternion.Normalize(inverseRoot * model.Rotation);

            var inParent = Vector3.Transform(keptPosition - parentPosition, toParent);
            frames[f][track] = new HavokTransformData
            {
                Translation = new Vector3(SafeDivide(inParent.X, parentScale.X), SafeDivide(inParent.Y, parentScale.Y), SafeDivide(inParent.Z, parentScale.Z)),
                Rotation = Quaternion.Normalize(toParent * keptRotation),
                Scale = clip.Frames[f][track].Scale
            };
        }

        var result = clip.WithFrames(frames, clip.Floats.Select(p => p.ToArray()).ToArray(), root);
        result.Up = new Vector4(up, 0);
        return result;
    }

    /// <summary>
    ///     Gives a clip without root motion a made path: over the clip the character travels <paramref name="forward" />
    ///     (along where it faces, -Z), <paramref name="sideways" /> (to its right, -X: vanilla skeletons have their L_
    ///     bones on +X) and <paramref name="upward" />, and turns <paramref name="turnDegrees" /> (positive to the right:
    ///     a positive turn about +Y swings -Z towards -X), eased as asked. The poses don't change, so the clip should be
    ///     one made to travel (a step, a lunge) or the character slides.
    /// </summary>
    public static EditableClip AddRootMotionPath(EditableClip clip, float forward, float sideways, float upward,
        float turnDegrees, BoneOffsetEase ease)
    {
        if (clip.RootMotion != null)
            throw new ArgumentException("The clip already has root motion.");

        // Vanilla root motion: up +Y; characters face -Z (they travel mostly along -Z)
        var move = new Vector3(-sideways, upward, -forward);
        var turn = turnDegrees * MathF.PI / 180;
        var last = Math.Max(clip.FrameCount - 1, 1);

        var root = new Vector4[clip.FrameCount];
        for (var f = 0; f < clip.FrameCount; f++)
        {
            var t = (float)f / last;
            if (ease == BoneOffsetEase.Smooth)
                t = t * t * (3 - 2 * t);

            root[f] = new Vector4(move * t, turn * t);
        }

        var result = clip.WithFrames(clip.Frames.Select(p => p.ToArray()).ToArray(), clip.Floats.Select(p => p.ToArray()).ToArray(), root);
        result.Up = new Vector4(0, 1, 0, 0);
        result.Forward = new Vector4(0, 0, 1, 0);
        return result;
    }

    /// <summary>The angle (radians) a rotation turns about an axis: its twist in a swing-twist split</summary>
    private static float TwistAngle(Quaternion q, Vector3 axis)
    {
        var along = Vector3.Dot(new Vector3(q.X, q.Y, q.Z), axis);
        return 2 * MathF.Atan2(along, q.W);
    }

    private static float WrapAngle(float a)
    {
        while (a > MathF.PI) a -= 2 * MathF.PI;
        while (a < -MathF.PI) a += 2 * MathF.PI;
        return a;
    }

    // ---- smoothing (§13.3 item 8)

    /// <summary>The result of <see cref="Smooth" />: the clip and the largest change it made (degrees, units)</summary>
    public sealed record SmoothResult(EditableClip Clip, float LargestTurn, float LargestMove);

    /// <summary>
    ///     Smooths the base frames of <paramref name="tracks" /> (null: every track) from <paramref name="first" /> to
    ///     <paramref name="last" /> with a Gaussian of <paramref name="radius" /> frames either side (σ = radius / 2):
    ///     rotations averaged in one hemisphere and normalized, translations averaged; scale is kept. Where the range
    ///     doesn't reach an end of the clip, the smoothing fades in and out over <paramref name="radius" /> frames, so
    ///     the frames outside are met without a step. With <paramref name="root" /> the root motion is smoothed the same
    ///     way. Offset keys and layers are untouched (they sit on the frames).
    /// </summary>
    public static SmoothResult Smooth(EditableClip clip, IReadOnlyCollection<int>? tracks, int first, int last, int radius, bool root)
    {
        var end = clip.FrameCount - 1;
        first = Math.Clamp(first, 0, end);
        last = Math.Clamp(last, first, end);
        radius = Math.Clamp(radius, 1, 60);

        var sigma = radius / 2f;
        var kernel = Enumerable.Range(-radius, radius * 2 + 1).Select(k => MathF.Exp(-k * k / (2 * sigma * sigma))).ToArray();

        // How much of the smoothed value a frame takes: all of it, fading in and out at range edges inside the clip
        float Amount(int f)
        {
            var amount = 1f;
            if (first > 0)
                amount = Math.Min(amount, (f - first + 1) / (float)(radius + 1));
            if (last < end)
                amount = Math.Min(amount, (last - f + 1) / (float)(radius + 1));
            amount = Math.Clamp(amount, 0, 1);
            return amount * amount * (3 - 2 * amount);
        }

        var frames = clip.Frames.Select(p => p.ToArray()).ToArray();
        var largestTurn = 0f;
        var largestMove = 0f;

        foreach (var t in tracks ?? (IReadOnlyCollection<int>)Enumerable.Range(0, clip.TrackCount).ToList())
        {
            if (t < 0 || t >= clip.TrackCount)
                continue;

            for (var f = first; f <= last; f++)
            {
                var own = clip.Frames[f][t];
                var rotation = Vector4.Zero;
                var translation = Vector3.Zero;
                var total = 0f;
                for (var k = -radius; k <= radius; k++)
                {
                    var (q, p) = Reflected(clip, t, f + k, end);
                    if (Quaternion.Dot(q, own.Rotation) < 0)
                        q = -q;

                    var w = kernel[k + radius];
                    rotation += new Vector4(q.X, q.Y, q.Z, q.W) * w;
                    translation += p * w;
                    total += w;
                }

                var smoothed = new HavokTransformData
                {
                    Rotation = Quaternion.Normalize(new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W)),
                    Translation = translation / total,
                    Scale = own.Scale
                };

                var result = EditableClip.Blend(own, smoothed, Amount(f));
                result.Scale = own.Scale;
                frames[f][t] = result;

                largestTurn = Math.Max(largestTurn, AngleBetween(own.Rotation, result.Rotation));
                largestMove = Math.Max(largestMove, (own.Translation - result.Translation).Length());
            }
        }

        var rootMotion = clip.RootMotion?.ToArray();
        if (root && clip.RootMotion is { } motion)
        {
            for (var f = first; f <= last; f++)
            {
                var sum = Vector4.Zero;
                var total = 0f;
                for (var k = -radius; k <= radius; k++)
                {
                    // Past an end, the motion mirrored through that end's sample (as for the bones)
                    var i = f + k;
                    var sample = i < 0 ? 2 * motion[0] - motion[Math.Min(-i, end)]
                        : i > end ? 2 * motion[end] - motion[Math.Max(2 * end - i, 0)]
                        : motion[i];
                    sum += sample * kernel[k + radius];
                    total += kernel[k + radius];
                }

                rootMotion![f] = Vector4.Lerp(motion[f], sum / total, Amount(f));
                var moved = rootMotion[f] - motion[f];
                largestMove = Math.Max(largestMove, new Vector3(moved.X, moved.Y, moved.Z).Length());
            }
        }

        return new SmoothResult(clip.WithFrames(frames, clip.Floats.Select(p => p.ToArray()).ToArray(), rootMotion), largestTurn, largestMove);
    }

    /// <summary>
    ///     A track's frame for smoothing, past the clip's ends mirrored through the end frame (odd reflection:
    ///     <c>2·p₀ − pₖ</c>, <c>q₀·qₖ⁻¹·q₀</c>), so a steady motion carries on and an end frame keeps its pose
    /// </summary>
    private static (Quaternion Rotation, Vector3 Translation) Reflected(EditableClip clip, int track, int frame, int end)
    {
        if (frame >= 0 && frame <= end)
            return (clip.Frames[frame][track].Rotation, clip.Frames[frame][track].Translation);

        var pivot = frame < 0 ? 0 : end;
        var mirror = Math.Clamp(2 * pivot - frame, 0, end);
        var p = clip.Frames[pivot][track];
        var m = clip.Frames[mirror][track];
        var q = m.Rotation;
        if (Quaternion.Dot(q, p.Rotation) < 0)
            q = -q;

        return (Quaternion.Normalize(p.Rotation * Quaternion.Inverse(q) * p.Rotation), 2 * p.Translation - m.Translation);
    }

    /// <summary>The angle (degrees) between two rotations</summary>
    private static float AngleBetween(Quaternion a, Quaternion b)
    {
        var dot = MathF.Min(MathF.Abs(Quaternion.Dot(Quaternion.Normalize(a), Quaternion.Normalize(b))), 1);
        return 2 * MathF.Acos(dot) * 180 / MathF.PI;
    }

    // ---- range timing (§13.3 item 5): these keep the offset keys, moving them with the frames

    /// <summary>The result of a range time operation: the clip, the event time map, keys merged onto one frame</summary>
    public sealed record RangeResult(EditableClip Clip, TimeMap Events, int MergedKeys, int DroppedKeys);

    /// <summary>
    ///     Plays frames <paramref name="first" /> to <paramref name="last" /> over <paramref name="length" /> frames
    ///     instead of <c>last − first</c> (shorter is faster), the rest of the clip unchanged and the frames after the
    ///     range moved along. The base frames, float tracks and root motion are resampled; offset keys keep their
    ///     poses and move with their frames (keys squeezed onto one frame: the later one stays).
    /// </summary>
    public static RangeResult RetimeRange(EditableClip clip, int first, int last, int length)
    {
        var end = clip.FrameCount - 1;
        first = Math.Clamp(first, 0, end);
        last = Math.Clamp(last, 0, end);
        if (last - first < 1 || length < 1)
            throw new ArgumentException("A range retime needs a range of at least one frame and a length of at least one.");

        var span = last - first;
        var count = clip.FrameCount + length - span;

        // New frame → old time
        float OldAt(int g) => g <= first ? g : g <= first + length ? first + (g - first) * (float)span / length : g - length + span;

        // Old frame → new frame (for keys)
        float NewAt(int k) => k <= first ? k : k <= last ? first + (k - first) * (float)length / span : k + length - span;

        var result = Resampled(clip, count, OldAt);
        var merged = MovedKeys(result, clip, k => (int)MathF.Round(NewAt(k)), _ => false);

        var fd = clip.FrameDuration;
        var map = TimeMap.From([(0, 0), (first * fd, first * fd), (last * fd, (first + length) * fd),
            ((end + 1) * fd, (end + 1 + length - span) * fd)]) ?? TimeMap.Identity;
        return new RangeResult(result, map, merged, 0);
    }

    /// <summary>
    ///     Holds frame <paramref name="at" /> for <paramref name="count" /> more frames (the pose and the root stay
    ///     still), the frames after it moved along. Keys after the frame move with it.
    /// </summary>
    public static RangeResult HoldFrames(EditableClip clip, int at, int count)
    {
        var end = clip.FrameCount - 1;
        at = Math.Clamp(at, 0, end);
        if (count < 1)
            throw new ArgumentException("Hold at least one frame.");

        float OldAt(int g) => g <= at ? g : g <= at + count ? at : g - count;
        var result = Resampled(clip, clip.FrameCount + count, OldAt);
        var merged = MovedKeys(result, clip, k => k <= at ? k : k + count, _ => false);

        // Events up to the held frame stay, events from the next frame on move by the count
        var fd = clip.FrameDuration;
        var map = at < end
            ? TimeMap.From([(0, 0), (at * fd, at * fd), ((at + 1) * fd, (at + 1 + count) * fd), ((end + 1) * fd, (end + 1 + count) * fd)])
            : TimeMap.From([(0, 0), (end * fd, end * fd)]);
        return new RangeResult(result, map ?? TimeMap.Identity, merged, 0);
    }

    /// <summary>
    ///     Takes frames <paramref name="first" /> to <paramref name="last" /> out: frame <c>first − 1</c> is followed
    ///     by <c>last + 1</c>. Both have to be inside the clip (removing from an end is a trim). Keys on the removed
    ///     frames go; later ones move back.
    /// </summary>
    public static RangeResult RemoveFrames(EditableClip clip, int first, int last)
    {
        var end = clip.FrameCount - 1;
        if (first < 1 || last > end - 1 || last < first)
            throw new ArgumentException("Removed frames need a frame before and after them; remove from an end with Trim.");

        var removed = last - first + 1;
        float OldAt(int g) => g < first ? g : g + removed;
        var result = Resampled(clip, clip.FrameCount - removed, OldAt);
        var merged = MovedKeys(result, clip, k => k < first ? k : k - removed, k => k >= first && k <= last);

        var dropped = clip.Layers.Sum(l => l.Keys.Sum(o => o.Value.Count(k => k.Frame >= first && k.Frame <= last)));

        // Events in the removed frames fold onto the frame after them
        var fd = clip.FrameDuration;
        var map = TimeMap.From([(0, 0), ((first - 1) * fd, (first - 1) * fd), ((last + 1) * fd, first * fd),
            ((end + 1) * fd, (end + 1 - removed) * fd)]) ?? TimeMap.Identity;
        return new RangeResult(result, map, merged, dropped);
    }

    /// <summary>The clip's base frames, float tracks and root motion sampled at an old time for each new frame</summary>
    private static EditableClip Resampled(EditableClip clip, int count, Func<int, float> oldAt)
    {
        var frames = new HavokTransformData[count][];
        var floats = new float[count][];
        var root = clip.RootMotion != null ? new Vector4[count] : null;

        for (var g = 0; g < count; g++)
        {
            var at = Math.Clamp(oldAt(g), 0, clip.FrameCount - 1);
            frames[g] = Enumerable.Range(0, clip.TrackCount).Select(t => clip.Sample(t, at)).ToArray();
            floats[g] = Enumerable.Range(0, clip.FloatTrackCount).Select(t => clip.SampleFloat(t, at)).ToArray();
            if (root != null)
                root[g] = SampleRoot(clip.RootMotion!, at);
        }

        return clip.WithFrames(frames, floats, root);
    }

    /// <summary>
    ///     Every layer's keys of <paramref name="clip" /> at their new frames, into the same layers of
    ///     <paramref name="result" /> (<paramref name="drop" />ped ones left out). Keys landing on one frame keep the
    ///     later one; the count of those merged is returned.
    /// </summary>
    private static int MovedKeys(EditableClip result, EditableClip clip, Func<int, int> newFrame, Func<int, bool> drop)
    {
        var merged = 0;
        for (var i = 0; i < clip.Layers.Count && i < result.Layers.Count; i++)
            result.Layers[i].Keys = MovedLayerKeys(clip.Layers[i].Keys, newFrame, drop, ref merged);

        return merged;
    }

    private static Dictionary<int, List<BoneOffsetKey>> MovedLayerKeys(Dictionary<int, List<BoneOffsetKey>> source,
        Func<int, int> newFrame, Func<int, bool> drop, ref int merged)
    {
        var result = new Dictionary<int, List<BoneOffsetKey>>();
        foreach (var (track, keys) in source)
        {
            var moved = new List<BoneOffsetKey>();
            foreach (var key in keys.Where(k => !drop(k.Frame)))
            {
                var frame = newFrame(key.Frame);
                if (moved.Count > 0 && moved[^1].Frame == frame)
                {
                    moved.RemoveAt(moved.Count - 1);
                    merged++;
                }

                moved.Add(key with { Frame = frame });
            }

            if (moved.Count > 0)
                result[track] = moved;
        }

        return result;
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
    ///     A bone's offset keys put on another bone (its L/R pair, <see cref="MirrorBoneName" />) as their mirror image
    ///     (Animation Editor Analysis.md §8, W11), replacing that bone's keys. At each key's frame, the change the key
    ///     makes is taken into model space (a turn about the bone, a move in its parent's space), reflected across the
    ///     character's side-to-side plane (as <see cref="Mirror" />), and taken into the other bone's parent space at that
    ///     frame. So it holds whatever the two bones' local axes are, and mirroring the keys back gives the first ones
    ///     again. The clip's own motion is not mirrored: an asymmetric attack keeps its shape, with the same fix on the
    ///     other side. Null when the first bone has no keys or either bone has no track.
    /// </summary>
    public static EditableClip? MirrorKeys(EditableClip clip, HavokSkeletonData skeleton, int fromBone, int toBone)
    {
        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);
        if (fromBone < 0 || toBone < 0 || fromBone >= boneToTrack.Length || toBone >= boneToTrack.Length)
            return null;

        var from = boneToTrack[fromBone];
        var to = boneToTrack[toBone];
        if (from < 0 || to < 0 || from == to || !clip.Offsets.TryGetValue(from, out var keys) || keys.Count == 0)
            return null;

        var side = Side(clip);
        var parentFrom = skeleton.Parents[fromBone];
        var parentTo = skeleton.Parents[toBone];

        var mirrored = new List<BoneOffsetKey>();
        foreach (var key in keys)
        {
            // Parents' poses with their own keys; neither bone's keys move its parent
            var pose = clip.ModelPose(skeleton, key.Frame);
            var (pa, ra, sa) = parentFrom >= 0 && parentFrom < pose.Length ? pose[parentFrom] : (Vector3.Zero, Quaternion.Identity, Vector3.One);
            var (_, rb, sb) = parentTo >= 0 && parentTo < pose.Length ? pose[parentTo] : (Vector3.Zero, Quaternion.Identity, Vector3.One);

            var turn = ReflectRotation(ra * key.Rotation * Quaternion.Inverse(ra), side);
            var rotation = Quaternion.Normalize(Quaternion.Inverse(rb) * turn * rb);

            var move = Reflect(Vector3.Transform(key.Translation * sa, ra), side);
            var local = Vector3.Transform(move, Quaternion.Inverse(rb));
            var translation = new Vector3(SafeDivide(local.X, sb.X), SafeDivide(local.Y, sb.Y), SafeDivide(local.Z, sb.Z));

            mirrored.Add(new BoneOffsetKey(key.Frame, rotation, translation, key.Ease));
        }

        var result = clip.WithoutOffsets(to);
        foreach (var key in mirrored)
            result = result.WithKey(to, key);
        return result;
    }

    /// <summary>The normal of the character's side-to-side plane: up × forward of its root motion (X for a bare clip)</summary>
    private static Vector3 Side(EditableClip clip)
    {
        var up = Up(clip);
        var forward = new Vector3(clip.Forward.X, clip.Forward.Y, clip.Forward.Z);
        var side = Vector3.Cross(up, forward.LengthSquared() > 1e-8f ? Vector3.Normalize(forward) : Vector3.UnitZ);
        return side.LengthSquared() > 1e-8f ? Vector3.Normalize(side) : Vector3.UnitX;
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
