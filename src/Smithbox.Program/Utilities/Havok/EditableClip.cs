#nullable enable
using Havok;
using Havok.Shared;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities;

/// <summary>
///     A character clip held as one local pose per whole frame (by transform track, in the binding's order), its float
///     tracks and its root motion, for editing (<see cref="EditableClipOperations" />) and writing back into an HKX
///     file (<see cref="Write" />). See Claude/Documentation/Animation Editor Analysis.md §4.1.
///     <para>
///         Vanilla clips keep no authored keys (their splines are fitted per block), so a clip is edited as frames.
///         Between frames it is sampled by lerp and slerp, which is what the encoder fits.
///     </para>
///     <para>
///         Annotation tracks aren't held: the writer keeps the old clip's. No vanilla character clip has annotations
///         (ER, NR and DB have no annotation tracks; AC6, SDT and DS3 have only empty ones), so their times can't go stale.
///     </para>
/// </summary>
public sealed class EditableClip
{
    public int TrackCount;
    public int FloatTrackCount;
    public float FrameDuration = 1f / 30f;
    public HavokBlendMode BlendMode;

    /// <summary>[frame][transform track]: the local transform</summary>
    public HavokTransformData[][] Frames = Array.Empty<HavokTransformData[]>();

    /// <summary>[frame][float track]</summary>
    public float[][] Floats = Array.Empty<float[]>();

    /// <summary>
    ///     The root motion, one sample per frame: XYZ where the root is (model space), W how far it has turned about
    ///     <see cref="Up" /> (radians, adding up). Null when the clip has none.
    /// </summary>
    public Vector4[]? RootMotion;

    public Vector4 Up = new(0, 1, 0, 0);
    public Vector4 Forward = new(0, 0, 1, 0);

    /// <summary>The binding's track to bone map (empty: track i is bone i)</summary>
    public short[] TrackToBone = Array.Empty<short>();

    /// <summary>The binding's float track to float slot map (empty: track i is slot i)</summary>
    public short[] FloatTrackToSlot = Array.Empty<short>();

    /// <summary>
    ///     Offset keys by transform track, each list sorted by frame (Animation Editor Analysis.md §4.7). They are kept
    ///     apart from <see cref="Frames" /> until <see cref="Baked" />, which playback, saving and the whole-clip
    ///     operations use. A clip is not changed once made: an edit makes a new clip (<see cref="WithKey" />).
    /// </summary>
    public Dictionary<int, List<BoneOffsetKey>> Offsets = new();

    public BoneOffsetEase OffsetEase = BoneOffsetEase.Smooth;

    public bool HasOffsets => Offsets.Count > 0;

    // The clip with its offsets applied, made once
    private EditableClip? _baked;

    public int FrameCount => Frames.Length;

    public float Duration => Math.Max(FrameCount - 1, 0) * FrameDuration;

    public int BoneOf(int track) =>
        TrackToBone.Length == 0 ? track : track < TrackToBone.Length ? TrackToBone[track] : -1;

    /// <summary>
    ///     Reads the first animation of an HKX file (DCX or not). Null when the file has no animation this reader can
    ///     decode.
    /// </summary>
    /// <param name="compendium">The type compendium of the binder the file is from, for a tagfile that uses one</param>
    public static EditableClip? Read(ProjectType game, byte[] data, HavokCompendium? compendium,
        HavokSkeletonData skeleton)
    {
        var raw = Unpack(data);
        var clip = HavokAnimationReader.ReadClip(game, raw, compendium, skeleton);
        if (clip == null)
            return null;

        var document = Havok.Files.HavokFile.Read(raw, HavokAnimationReader.ProfileOf(game), compendium);
        var binding = document.ObjectsOf<IHavokAnimationBinding>().FirstOrDefault();
        var animation = binding?.Animation;
        if (binding == null || animation == null)
            return null;

        var result = new EditableClip
        {
            TrackCount = binding.TransformTrackToBoneIndices.Count > 0
                ? binding.TransformTrackToBoneIndices.Count
                : animation.TransformTrackCount,
            FloatTrackCount = binding.FloatTrackToFloatSlotIndices.Count > 0
                ? binding.FloatTrackToFloatSlotIndices.Count
                : animation.FloatTrackCount,
            FrameDuration = clip.FrameDuration > 0 ? clip.FrameDuration : 1f / 30f,
            BlendMode = clip.BlendMode,
            TrackToBone = binding.TransformTrackToBoneIndices.ToArray(),
            FloatTrackToSlot = binding.FloatTrackToFloatSlotIndices.ToArray()
        };

        var frameCount = Math.Max(clip.FrameCount, 2);
        result.Frames = new HavokTransformData[frameCount][];
        result.Floats = new float[frameCount][];

        for (var f = 0; f < frameCount; f++)
        {
            var pose = new HavokTransformData[result.TrackCount];
            for (var t = 0; t < result.TrackCount; t++)
            {
                var x = clip.Sample(t, f);
                pose[t] = new HavokTransformData { Translation = x.Translation, Rotation = x.Rotation, Scale = x.Scale };
            }

            result.Frames[f] = pose;

            var floats = new float[result.FloatTrackCount];
            for (var t = 0; t < result.FloatTrackCount; t++)
            {
                // A float track without data at a frame keeps its slot's reference value
                var slot = result.FloatTrackToSlot.Length == 0 ? t : t < result.FloatTrackToSlot.Length ? result.FloatTrackToSlot[t] : -1;
                floats[t] = clip.SampleFloat(t, f)
                            ?? (slot >= 0 && slot < skeleton.ReferenceFloats.Length ? skeleton.ReferenceFloats[slot] : 0);
            }

            result.Floats[f] = floats;
        }

        if (document.ObjectsOf<IHavokReferenceFrame>().FirstOrDefault() is { SampleCount: > 0 } frame &&
            clip.RootMotion != null)
        {
            result.Up = frame.Up;
            result.Forward = frame.Forward;
            result.RootMotion = new Vector4[frameCount];
            for (var f = 0; f < frameCount; f++)
            {
                result.RootMotion[f] = clip.RootMotion.SampleRaw(f * result.FrameDuration);
            }
        }

        return result;
    }

    /// <summary>A track's local transform at a frame, lerped and slerped between whole frames</summary>
    public HavokTransformData Sample(int track, float frame)
    {
        if (FrameCount == 0)
            return new HavokTransformData { Rotation = Quaternion.Identity, Scale = Vector3.One };

        frame = Math.Clamp(frame, 0, FrameCount - 1);
        var first = (int)frame;
        var second = Math.Min(first + 1, FrameCount - 1);
        var weight = frame - first;
        var a = Frames[first][track];
        if (weight <= 0 || first == second)
            return a;

        return Blend(a, Frames[second][track], weight);
    }

    public float SampleFloat(int track, float frame)
    {
        frame = Math.Clamp(frame, 0, FrameCount - 1);
        var first = (int)frame;
        var second = Math.Min(first + 1, FrameCount - 1);
        return Floats[first][track] + (Floats[second][track] - Floats[first][track]) * (frame - first);
    }

    /// <summary>
    ///     Writes the clip in place of the animation of an HKX file and returns the new file (not DCX compressed). The
    ///     rest of the file (binding, annotation tracks, property bag) is kept, so it binds to the skeleton as before.
    /// </summary>
    /// <param name="baseFile">The file whose animation is replaced (DCX or not), normally the clip's own source</param>
    public byte[] Write(ProjectType game, byte[] baseFile, HavokCompendium? compendium, out HavokClipWriteResult result,
        HavokClipFormat format = HavokClipFormat.Auto, SplineEncodeSettings? settings = null)
    {
        if (FrameCount < 2)
            throw new InvalidOperationException("A clip needs at least two frames.");

        if (HasOffsets)
            return Baked().Write(game, baseFile, compendium, out result, format, settings);

        var document = Havok.Files.HavokFile.Read(Unpack(baseFile), HavokAnimationReader.ProfileOf(game), compendium);

        HavokReferenceFrameContent? referenceFrame = RootMotion == null
            ? null
            : new HavokReferenceFrameContent
            {
                Up = Up,
                Forward = Forward,
                Duration = Duration,
                Samples = RootMotion.ToArray()
            };

        HavokTransformData SampleTransform(int track, float frame) => Sample(track, frame);

        // A character clip is stored as vanilla character clips are (THREECOMP40 rotations), unless asked otherwise
        result = HavokClipWriter.ReplaceClip(document, null, FrameCount, TrackCount, FloatTrackCount, SampleTransform,
            FloatTrackCount > 0 ? SampleFloat : null, FrameDuration, format,
            settings ?? SplineEncodeSettings.ForCharacterClips(), referenceFrame);

        return Havok.Files.HavokFile.Write(document);
    }

    /// <summary>
    ///     The clip for <see cref="ModelAnimator" />, so an edit plays before it is saved: its frames as an interleaved
    ///     clip, with its root motion.
    /// </summary>
    public HavokClip ToPlaybackClip(HavokSkeletonData skeleton)
    {
        if (HasOffsets)
            return Baked().ToPlaybackClip(skeleton);

        var transforms = new HavokTransformData[FrameCount * TrackCount];
        for (var f = 0; f < FrameCount; f++)
            Array.Copy(Frames[f], 0, transforms, f * TrackCount, TrackCount);

        var floats = new float[FrameCount * FloatTrackCount];
        for (var f = 0; f < FrameCount; f++)
            Array.Copy(Floats[f], 0, floats, f * FloatTrackCount, FloatTrackCount);

        var clip = new HavokInterleavedClip(transforms, TrackCount, Duration, TrackToBone, skeleton.BoneCount, BlendMode,
            floats, FloatTrackCount, FloatTrackToSlot, skeleton.FloatSlotCount);

        if (RootMotion is { Length: > 0 } motion)
            clip.RootMotion = new HavokRootMotion(motion.ToArray(), Duration, new Vector3(Up.X, Up.Y, Up.Z));

        return clip;
    }

    public EditableClip Clone()
    {
        var copy = (EditableClip)MemberwiseClone();
        copy.Frames = Frames.Select(p => p.ToArray()).ToArray();
        copy.Floats = Floats.Select(p => p.ToArray()).ToArray();
        copy.RootMotion = RootMotion?.ToArray();
        copy.TrackToBone = TrackToBone.ToArray();
        copy.FloatTrackToSlot = FloatTrackToSlot.ToArray();
        copy.Offsets = Offsets.ToDictionary(e => e.Key, e => e.Value.ToList());
        copy._baked = null;
        return copy;
    }

    // ---- offset keys

    /// <summary>
    ///     The clip with every offset key applied to its frames, and no keys of its own. Made once per clip; a clip
    ///     without keys is its own.
    /// </summary>
    public EditableClip Baked()
    {
        if (!HasOffsets)
            return this;

        if (_baked != null)
            return _baked;

        var baked = Clone();
        baked.Offsets = new Dictionary<int, List<BoneOffsetKey>>();
        foreach (var track in Offsets.Keys)
        {
            for (var f = 0; f < FrameCount; f++)
                baked.Frames[f][track] = ApplyOffset(Frames[f][track], OffsetAt(track, f));
        }

        _baked = baked;
        return baked;
    }

    /// <summary>
    ///     The offset of a track at a frame: slerped / lerped between the keys around it (eased as
    ///     <see cref="OffsetEase" />), held before the first key and after the last. Identity without keys.
    /// </summary>
    public BoneOffsetKey OffsetAt(int track, float frame)
    {
        if (!Offsets.TryGetValue(track, out var keys) || keys.Count == 0)
            return BoneOffsetKey.Identity(0);

        if (frame <= keys[0].Frame)
            return keys[0];

        if (frame >= keys[^1].Frame)
            return keys[^1];

        var next = keys.FindIndex(k => k.Frame >= frame);
        var a = keys[next - 1];
        var b = keys[next];
        var t = (frame - a.Frame) / (b.Frame - a.Frame);
        if (OffsetEase == BoneOffsetEase.Smooth)
            t = t * t * (3 - 2 * t);

        var rb = Quaternion.Dot(a.Rotation, b.Rotation) < 0 ? -b.Rotation : b.Rotation;
        return new BoneOffsetKey(-1, Quaternion.Normalize(Quaternion.Slerp(a.Rotation, rb, t)),
            Vector3.Lerp(a.Translation, b.Translation, t));
    }

    /// <summary>A local transform with an offset: rotated in its parent's space, then moved</summary>
    public static HavokTransformData ApplyOffset(HavokTransformData local, BoneOffsetKey offset)
    {
        return new HavokTransformData
        {
            Translation = local.Translation + offset.Translation,
            Rotation = Quaternion.Normalize(offset.Rotation * local.Rotation),
            Scale = local.Scale
        };
    }

    /// <summary>A copy with this key (replacing one at the same frame)</summary>
    public EditableClip WithKey(int track, BoneOffsetKey key)
    {
        var copy = Clone();
        if (!copy.Offsets.TryGetValue(track, out var keys))
            copy.Offsets[track] = keys = new List<BoneOffsetKey>();

        keys.RemoveAll(k => k.Frame == key.Frame);
        keys.Add(key);
        keys.Sort((a, b) => a.Frame.CompareTo(b.Frame));
        return copy;
    }

    /// <summary>A copy without the key at a frame (and without the track's list once it is empty)</summary>
    public EditableClip WithoutKey(int track, int frame)
    {
        var copy = Clone();
        if (copy.Offsets.TryGetValue(track, out var keys))
        {
            keys.RemoveAll(k => k.Frame == frame);
            if (keys.Count == 0)
                copy.Offsets.Remove(track);
        }

        return copy;
    }

    /// <summary>A copy without any key on a track, or on every track when <paramref name="track" /> is null</summary>
    public EditableClip WithoutOffsets(int? track)
    {
        var copy = Clone();
        if (track is { } t)
            copy.Offsets.Remove(t);
        else
            copy.Offsets.Clear();
        return copy;
    }

    /// <summary>
    ///     A copy whose key at <paramref name="frame" /> puts a bone at a model-space position and / or rotation there
    ///     (null keeps the key's own): the bone's new local transform, from that pose and its parent's (which the key
    ///     doesn't move), as an offset from the clip's own local transform at that frame. The viewport gizmo's edit.
    /// </summary>
    public EditableClip KeyedToModelPose(HavokSkeletonData skeleton, int bone, int track, int frame, Vector3? position,
        Quaternion? rotation)
    {
        var pose = ModelPose(skeleton, frame);
        var parent = skeleton.Parents[bone];
        var hasParent = parent >= 0 && parent < skeleton.BoneCount && parent != bone;
        var parentRotation = hasParent ? pose[parent].Rotation : Quaternion.Identity;
        var parentPosition = hasParent ? pose[parent].Position : Vector3.Zero;
        var parentScale = hasParent ? pose[parent].Scale : Vector3.One;
        var toParent = Quaternion.Inverse(parentRotation);

        // The bone's own local transform at the frame, without its keys, and the offset it has there now
        var local = Sample(track, frame);
        var existing = OffsetAt(track, frame);
        var offsetRotation = existing.Rotation;
        var offsetTranslation = existing.Translation;

        if (rotation is { } r)
        {
            var newLocal = Quaternion.Normalize(toParent * r);
            offsetRotation = Quaternion.Normalize(newLocal * Quaternion.Inverse(local.Rotation));
        }

        if (position is { } p)
        {
            var inParent = Vector3.Transform(p - parentPosition, toParent);
            var newLocal = new Vector3(Divide(inParent.X, parentScale.X), Divide(inParent.Y, parentScale.Y),
                Divide(inParent.Z, parentScale.Z));
            offsetTranslation = newLocal - local.Translation;
        }

        return WithKey(track, new BoneOffsetKey(frame, offsetRotation, offsetTranslation));
    }

    private static float Divide(float a, float b) => MathF.Abs(b) > 1e-8f ? a / b : a;

    public EditableClip WithEase(BoneOffsetEase ease)
    {
        var copy = Clone();
        copy.OffsetEase = ease;
        return copy;
    }

    // ---- model space

    /// <summary>
    ///     Every bone's model-space position, rotation and scale at a frame of the clip with its offsets (bones the
    ///     clip has no track for at their reference pose)
    /// </summary>
    public (Vector3 Position, Quaternion Rotation, Vector3 Scale)[] ModelPose(HavokSkeletonData skeleton, float frame)
    {
        var clip = Baked();
        var boneToTrack = BoneToTrack(skeleton.BoneCount);
        return ModelSpace(skeleton, b => boneToTrack[b] >= 0 ? clip.Sample(boneToTrack[b], frame) : skeleton.ReferencePose[b]);
    }

    /// <summary>The track of each bone of a skeleton, -1 for a bone the clip doesn't animate</summary>
    public int[] BoneToTrack(int boneCount)
    {
        var boneToTrack = Enumerable.Repeat(-1, boneCount).ToArray();
        for (var t = 0; t < TrackCount; t++)
        {
            var bone = BoneOf(t);
            if (bone >= 0 && bone < boneCount)
                boneToTrack[bone] = t;
        }

        return boneToTrack;
    }

    /// <summary>Model-space transforms from local ones, down the skeleton's parents</summary>
    public static (Vector3 Position, Quaternion Rotation, Vector3 Scale)[] ModelSpace(HavokSkeletonData skeleton,
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

    /// <summary>A copy with other frames (and root motion), keeping the clip's tracks, maps and settings</summary>
    public EditableClip WithFrames(HavokTransformData[][] frames, float[][] floats, Vector4[]? rootMotion)
    {
        var copy = Clone();
        copy.Frames = frames;
        copy.Floats = floats;
        copy.RootMotion = rootMotion;
        return copy;
    }

    public static HavokTransformData Blend(HavokTransformData a, HavokTransformData b, float weight)
    {
        var rb = b.Rotation;
        // Same hemisphere, so the slerp takes the short way
        if (Quaternion.Dot(a.Rotation, rb) < 0)
            rb = -rb;

        return new HavokTransformData
        {
            Translation = Vector3.Lerp(a.Translation, b.Translation, weight),
            Rotation = Quaternion.Normalize(Quaternion.Slerp(a.Rotation, rb, weight)),
            Scale = Vector3.Lerp(a.Scale, b.Scale, weight)
        };
    }

    private static byte[] Unpack(byte[] data) => DCX.Is(data) ? DCX.Decompress(data, out _) : data;
}
