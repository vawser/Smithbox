#nullable enable
using Havok;
using Havok.Shared;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using System;
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
        return copy;
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
