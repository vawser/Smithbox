#nullable enable
using Havok;
using Havok.Files;
using Havok.Shared;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Renderer;

public struct HavokTransformData
{
    public Vector3 Translation;
    public Quaternion Rotation;
    public Vector3 Scale;

    public NewBlendableTransform ToBlendable()
    {
        return new NewBlendableTransform { Translation = Translation, Rotation = Rotation, Scale = Scale };
    }
}

/// <summary>
///     The bones of an hkaSkeleton, independent of the container they were read from.
/// </summary>
public sealed class HavokSkeletonData
{
    // The skeleton's own name. In a cutscene cut it is the name of the MQB resource it animates (cam0010, c4750_0000).
    public string Name = "";

    public string[] Names = Array.Empty<string>();
    public short[] Parents = Array.Empty<short>();
    public HavokTransformData[] ReferencePose = Array.Empty<HavokTransformData>();

    // Named scalar channels beside the bones, such as camNNNN_FOV, and their values when no track drives them
    public string[] FloatSlots = Array.Empty<string>();
    public float[] ReferenceFloats = Array.Empty<float>();

    public int BoneCount => Names.Length;
    public int FloatSlotCount => FloatSlots.Length;
}

/// <summary>
///     One skeleton of a multi-skeleton file (a cutscene cut) and the clip bound to it, if any.
/// </summary>
public sealed record HavokSceneTrack(HavokSkeletonData Skeleton, HavokClip? Clip);

public enum HavokBlendMode
{
    Normal,

    // The clip is a delta applied on top of the reference pose
    AdditiveParentSpace,
    AdditiveChildSpace
}

/// <summary>
///     The root motion of a clip (hkaDefaultAnimatedReferenceFrame): how far the game moves the whole character
///     over the clip, apart from what its bones do. It's in the model space of the skeleton.
/// </summary>
public sealed class HavokRootMotion
{
    private readonly Vector4[] _samples;
    private readonly float _duration;
    private readonly Vector3 _up;

    public HavokRootMotion(Vector4[] samples, float duration, Vector3 up)
    {
        _samples = samples;
        _duration = duration;
        _up = up.LengthSquared() > 1e-8f ? Vector3.Normalize(up) : Vector3.UnitY;
    }

    public int SampleCount => _samples.Length;

    /// <summary>
    ///     Where the root is at a sample, for drawing the path it takes.
    /// </summary>
    public Vector3 GetSamplePosition(int index)
    {
        return GetSamplePosition(index, HavokRootMotionScale.One);
    }

    /// <summary>
    ///     Where the root is at a sample, with its motion scaled as <see cref="Sample(float, HavokRootMotionScale)" /> does.
    /// </summary>
    public Vector3 GetSamplePosition(int index, HavokRootMotionScale scale)
    {
        Vector4 sample = _samples[index];
        return scale.ScaleTranslation(new Vector3(sample.X, sample.Y, sample.Z), _up);
    }

    /// <summary>
    ///     How far the root has moved and turned since the start of the clip, at a time within it.
    /// </summary>
    public Matrix4x4 Sample(float time)
    {
        return Sample(time, HavokRootMotionScale.One);
    }

    /// <summary>
    ///     How far the root has moved and turned since the start of the clip, with the motion along the ground,
    ///     the motion along up and the turn each scaled, to preview how a change of speed would look.
    /// </summary>
    public Matrix4x4 Sample(float time, HavokRootMotionScale scale)
    {
        if (_samples.Length == 0)
        {
            return Matrix4x4.Identity;
        }

        // The samples are spread evenly over the clip, the first at its start and the last at its end
        var position = _samples.Length > 1 && _duration > 0
            ? Math.Clamp(time / _duration, 0, 1) * (_samples.Length - 1)
            : 0;

        var first = Math.Min((int)position, _samples.Length - 1);
        var second = Math.Min(first + 1, _samples.Length - 1);

        // The turn in W keeps adding up rather than wrapping, so it lerps like the rest
        Vector4 sample = Vector4.Lerp(_samples[first], _samples[second], position - first);

        return Matrix4x4.CreateFromAxisAngle(_up, sample.W * scale.Turn) *
               Matrix4x4.CreateTranslation(scale.ScaleTranslation(new Vector3(sample.X, sample.Y, sample.Z), _up));
    }
}

/// <summary>
///     How much of a clip's root motion is applied: along the ground, along up, and of the turn. 1 is as the clip has it.
/// </summary>
public readonly record struct HavokRootMotionScale(float Ground, float Up, float Turn)
{
    public static readonly HavokRootMotionScale One = new(1, 1, 1);

    public bool IsOne => Ground == 1 && Up == 1 && Turn == 1;

    public Vector3 ScaleTranslation(Vector3 translation, Vector3 up)
    {
        if (Ground == 1 && Up == 1)
        {
            return translation;
        }

        Vector3 vertical = up * Vector3.Dot(translation, up);
        return (translation - vertical) * Ground + vertical * Up;
    }
}

/// <summary>
///     One decoded animation. Sampling returns the local transform of a transform track.
/// </summary>
public abstract class HavokClip
{
    public float Duration;
    public float FrameDuration;
    public int FrameCount;
    public HavokBlendMode BlendMode;

    // Null when the clip doesn't move the character, or has no samples
    public HavokRootMotion? RootMotion;

    // Index = hkx bone index, value = transform track index or -1
    public int[] BoneToTrack = Array.Empty<int>();

    // Index = skeleton float slot, value = float track index or -1
    public int[] SlotToFloatTrack = Array.Empty<int>();

    public abstract NewBlendableTransform Sample(int track, float frame);

    /// <summary>
    ///     A float track's value at a frame, or null when the track has no data there and its slot keeps the
    ///     reference value.
    /// </summary>
    public virtual float? SampleFloat(int track, float frame) => null;

    /// <summary>
    ///     A skeleton float slot's value at a frame: its track's, else the skeleton's reference value.
    /// </summary>
    public float SampleSlot(HavokSkeletonData skeleton, int slot, float frame)
    {
        var track = slot < SlotToFloatTrack.Length ? SlotToFloatTrack[slot] : -1;
        var value = track >= 0 ? SampleFloat(track, frame) : null;
        return value ?? (slot < skeleton.ReferenceFloats.Length ? skeleton.ReferenceFloats[slot] : 0);
    }

    protected void BuildFloatSlotMap(short[] trackToSlot, int slotCount, int trackCount)
    {
        SlotToFloatTrack = new int[slotCount];
        Array.Fill(SlotToFloatTrack, -1);

        for (var track = 0; track < trackCount; track++)
        {
            // Unbound tracks are in slot order
            int slot = trackToSlot.Length == 0 ? track : track < trackToSlot.Length ? trackToSlot[track] : -1;
            if (slot >= 0 && slot < slotCount)
            {
                SlotToFloatTrack[slot] = track;
            }
        }
    }

    protected void BuildBoneMap(short[] trackToBone, int boneCount, int trackCount)
    {
        BoneToTrack = new int[boneCount];
        Array.Fill(BoneToTrack, -1);

        // Unbound clips use the tracks in bone order
        if (trackToBone.Length == 0)
        {
            for (var i = 0; i < Math.Min(boneCount, trackCount); i++)
            {
                BoneToTrack[i] = i;
            }

            return;
        }

        for (var track = 0; track < trackToBone.Length; track++)
        {
            int bone = trackToBone[track];
            if (bone >= 0 && bone < boneCount)
            {
                BoneToTrack[bone] = track;
            }
        }
    }
}

public sealed class HavokSplineClip : HavokClip
{
    private readonly List<SplineCompressedAnimation.TransformTrack[]> _blocks;
    private readonly List<SplineCompressedAnimation.FloatTrack[]> _floatBlocks;
    private readonly int _framesPerBlock;

    public HavokSplineClip(byte[] data, bool bigEndian, int trackCount, int blockCount, int framesPerBlock,
        int frameCount, float duration, float frameDuration, short[] trackToBone, int boneCount,
        HavokBlendMode blendMode, IReadOnlyList<uint>? blockOffsets = null, int floatTrackCount = 0,
        IReadOnlyList<uint>? floatBlockOffsets = null, short[]? floatTrackToSlot = null, int floatSlotCount = 0)
    {
        Duration = duration;
        FrameCount = frameCount;
        FrameDuration = frameDuration;
        BlendMode = blendMode;
        _framesPerBlock = framesPerBlock;

        _blocks = SplineCompressedAnimation.ReadSplineCompressedAnimByteBlock(bigEndian, data, trackCount,
            blockCount, blockOffsets, floatTrackCount);
        _floatBlocks = SplineCompressedAnimation.ReadSplineCompressedFloatBlocks(bigEndian, data, trackCount,
            floatTrackCount, blockOffsets, floatBlockOffsets);

        BuildBoneMap(trackToBone, boneCount, trackCount);
        BuildFloatSlotMap(floatTrackToSlot ?? Array.Empty<short>(), floatSlotCount,
            _floatBlocks.Count > 0 ? floatTrackCount : 0);
    }

    // Each block covers maxFramesPerBlock frames, of which the last is shared with the next block
    private int BlockOf(float frame, int blockCount, out float frameInBlock)
    {
        var span = _framesPerBlock - 1;
        var block = span > 0 ? (int)(frame / span) : 0;
        block = Math.Clamp(block, 0, blockCount - 1);
        frameInBlock = Math.Max(0, frame - block * span);
        return block;
    }

    public override float? SampleFloat(int track, float frame)
    {
        if (_floatBlocks.Count == 0)
        {
            return null;
        }

        var block = BlockOf(frame, _floatBlocks.Count, out var frameInBlock);
        return _floatBlocks[block][track].GetValue(frameInBlock);
    }

    public override NewBlendableTransform Sample(int track, float frame)
    {
        var block = BlockOf(frame, _blocks.Count, out var frameInBlock);
        SplineCompressedAnimation.TransformTrack t = _blocks[block][track];

        NewBlendableTransform result = NewBlendableTransform.Identity;

        if (t.SplineScale != null)
        {
            result.Scale.X = t.SplineScale.GetValueX(frameInBlock) ?? 1;
            result.Scale.Y = t.SplineScale.GetValueY(frameInBlock) ?? 1;
            result.Scale.Z = t.SplineScale.GetValueZ(frameInBlock) ?? 1;
        }
        else
        {
            result.Scale = t.StaticScale;
        }

        if (t.SplineRotation != null)
        {
            result.Rotation = t.SplineRotation.GetValue(frameInBlock);
        }
        else if (t.HasStaticRotation)
        {
            result.Rotation = t.StaticRotation;
        }

        if (t.SplinePosition != null)
        {
            result.Translation.X = t.SplinePosition.GetValueX(frameInBlock) ?? 0;
            result.Translation.Y = t.SplinePosition.GetValueY(frameInBlock) ?? 0;
            result.Translation.Z = t.SplinePosition.GetValueZ(frameInBlock) ?? 0;
        }
        else
        {
            result.Translation = t.StaticPosition;
        }

        // Some vanilla clips store NaN keys on bones scaled to nothing (ER s12_05_0020, c8905 wing support bones).
        // The game draws nothing there; here one NaN would spread through the bone matrices, so fall back.
        if (!float.IsFinite(result.Rotation.X + result.Rotation.Y + result.Rotation.Z + result.Rotation.W))
        {
            result.Rotation = Quaternion.Identity;
        }

        if (!float.IsFinite(result.Translation.X + result.Translation.Y + result.Translation.Z))
        {
            result.Translation = Vector3.Zero;
        }

        if (!float.IsFinite(result.Scale.X + result.Scale.Y + result.Scale.Z))
        {
            result.Scale = Vector3.One;
        }

        return result;
    }
}

public sealed class HavokInterleavedClip : HavokClip
{
    private readonly HavokTransformData[] _transforms;
    private readonly int _trackCount;
    private readonly float[] _floats;
    private readonly int _floatTrackCount;

    public HavokInterleavedClip(HavokTransformData[] transforms, int trackCount, float duration,
        short[] trackToBone, int boneCount, HavokBlendMode blendMode, float[]? floats = null,
        int floatTrackCount = 0, short[]? floatTrackToSlot = null, int floatSlotCount = 0)
    {
        _transforms = transforms;
        _trackCount = trackCount;

        Duration = duration;
        FrameCount = trackCount > 0 ? transforms.Length / trackCount : 0;
        FrameDuration = FrameCount > 1 ? duration / (FrameCount - 1) : duration;
        BlendMode = blendMode;

        // Frame-major like the transforms; ignored unless there is a value per track for every frame
        _floats = floats ?? Array.Empty<float>();
        _floatTrackCount = floatTrackCount > 0 && _floats.Length >= FrameCount * floatTrackCount ? floatTrackCount : 0;

        BuildBoneMap(trackToBone, boneCount, trackCount);
        BuildFloatSlotMap(floatTrackToSlot ?? Array.Empty<short>(), floatSlotCount, _floatTrackCount);
    }

    public override float? SampleFloat(int track, float frame)
    {
        if (_floatTrackCount == 0 || FrameCount == 0)
        {
            return null;
        }

        var first = Math.Clamp((int)frame, 0, FrameCount - 1);
        var second = Math.Min(first + 1, FrameCount - 1);
        var t = Math.Clamp(frame - first, 0, 1);

        var a = _floats[first * _floatTrackCount + track];
        var b = _floats[second * _floatTrackCount + track];
        return a + (b - a) * t;
    }

    public override NewBlendableTransform Sample(int track, float frame)
    {
        var first = Math.Clamp((int)frame, 0, FrameCount - 1);
        var second = Math.Min(first + 1, FrameCount - 1);

        return NewBlendableTransform.Lerp(frame - first,
            _transforms[first * _trackCount + track].ToBlendable(),
            _transforms[second * _trackCount + track].ToBlendable());
    }
}

/// <summary>
///     Reads skeletons and clips through the unified Havok library (Havok.Files), whatever the container
///     (packfile or tagfile) and SDK: the Havok.Shared animation views are the same for every game.
/// </summary>
public static class HavokAnimationReader
{
    public static HavokProfile ProfileOf(ProjectType game)
    {
        return HavokProfile.FromGame(game.ToString())
               ?? throw new NotSupportedException($"{game} has no Havok profile.");
    }

    /// <summary>
    ///     Parses a binder's type compendium once, for the tagfiles in the binder that refer to it.
    /// </summary>
    public static HavokCompendium? ReadCompendium(ProjectType game, byte[]? compendium)
    {
        return compendium != null ? HavokFile.ReadCompendium(Decompress(compendium), ProfileOf(game)) : null;
    }

    /// <summary>
    ///     Returns null when the file has no skeleton: the anibnd skeleton of DS1R c3290 is an empty animation container.
    /// </summary>
    public static HavokSkeletonData? ReadSkeleton(ProjectType game, byte[] data, HavokCompendium? compendium)
    {
        HavokDocument document = HavokFile.Read(Decompress(data), ProfileOf(game), compendium);
        IHavokSkeleton? skeleton = document.ObjectsOf<IHavokSkeleton>().FirstOrDefault();

        return skeleton != null ? ToSkeletonData(skeleton) : null;
    }

    /// <summary>
    ///     The bone skeleton of a ragdoll file (the hkx of a chrbnd), which also holds the coarser ragdoll skeleton
    ///     that maps onto it: the one with the most bones is the one animations are made for. Null when it has none.
    /// </summary>
    public static HavokSkeletonData? ReadRagdollBoneSkeleton(ProjectType game, byte[] data)
    {
        HavokDocument document = HavokFile.Read(Decompress(data), ProfileOf(game));
        IHavokSkeleton? skeleton = document.ObjectsOf<IHavokSkeleton>().MaxBy(s => s.BoneCount);

        return skeleton != null ? ToSkeletonData(skeleton) : null;
    }

    /// <summary>
    ///     Every skeleton in a file with the clip bound to it, paired by the binding's original skeleton name. A cutscene
    ///     cut keeps one per actor and camera in a single hkaAnimationContainer. A skeleton without a binding comes
    ///     back with a null clip, and so does one whose clip this reader doesn't understand.
    /// </summary>
    public static List<HavokSceneTrack> ReadScene(ProjectType game, byte[] data, HavokCompendium? compendium)
    {
        HavokDocument document = HavokFile.Read(Decompress(data), ProfileOf(game), compendium);
        List<IHavokSkeleton> skeletons = document.ObjectsOf<IHavokSkeleton>().ToList();
        List<IHavokAnimationBinding> bindings = document.ObjectsOf<IHavokAnimationBinding>().ToList();

        List<HavokSceneTrack> result = new();
        foreach (IHavokSkeleton skeleton in skeletons)
        {
            HavokSkeletonData skeletonData = ToSkeletonData(skeleton);

            IHavokAnimationBinding? binding = bindings.FirstOrDefault(b =>
                string.Equals(b.OriginalSkeletonName, skeleton.Name, StringComparison.Ordinal));

            // A file with one of each needn't name the skeleton
            if (binding == null && skeletons.Count == 1 && bindings.Count == 1)
            {
                binding = bindings[0];
            }

            HavokClip? clip = binding?.Animation is { } animation
                ? BuildClip(animation, binding, skeletonData, document.Source.BigEndian)
                : null;

            result.Add(new HavokSceneTrack(skeletonData, clip));
        }

        return result;
    }

    /// <summary>
    ///     Returns null when the file has no clip this reader understands.
    /// </summary>
    public static HavokClip? ReadClip(ProjectType game, byte[] data, HavokCompendium? compendium,
        HavokSkeletonData skeleton)
    {
        HavokDocument document = HavokFile.Read(Decompress(data), ProfileOf(game), compendium);

        IHavokAnimationBinding? binding = document.ObjectsOf<IHavokAnimationBinding>().FirstOrDefault();

        IHavokAnimation? animation = (IHavokAnimation?)document.ObjectsOf<IHavokSplineAnimation>().FirstOrDefault()
                                     ?? (IHavokAnimation?)document.ObjectsOf<IHavokInterleavedAnimation>().FirstOrDefault()
                                     ?? document.ObjectsOf<IHavokQuantizedAnimation>().FirstOrDefault();

        HavokClip? clip = animation != null
            ? BuildClip(animation, binding, skeleton, document.Source.BigEndian)
            : null;

        if (clip != null && document.ObjectsOf<IHavokReferenceFrame>().FirstOrDefault() is { SampleCount: > 0 } frame)
        {
            var samples = new Vector4[frame.SampleCount];
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] = frame.GetSample(i);
            }

            // The reference frame spans the clip, so the clip's duration stands in if it gives none
            var duration = frame.Duration > 0 ? frame.Duration : clip.Duration;

            clip.RootMotion = new HavokRootMotion(samples, duration, new Vector3(frame.Up.X, frame.Up.Y, frame.Up.Z));
        }

        return clip;
    }

    private static HavokSkeletonData ToSkeletonData(IHavokSkeleton skeleton)
    {
        var count = skeleton.BoneCount;
        var slots = skeleton.FloatSlotCount;
        HavokSkeletonData result = new()
        {
            Name = skeleton.Name ?? "",
            Names = new string[count],
            Parents = new short[count],
            ReferencePose = new HavokTransformData[count],
            FloatSlots = new string[slots],
            ReferenceFloats = new float[slots]
        };

        for (var i = 0; i < count; i++)
        {
            result.Names[i] = skeleton.GetBoneName(i) ?? "";
            result.Parents[i] = skeleton.GetParent(i);
            result.ReferencePose[i] = ToTransform(skeleton.GetReferencePose(i));
        }

        for (var i = 0; i < slots; i++)
        {
            result.FloatSlots[i] = skeleton.GetFloatSlotName(i) ?? "";
            result.ReferenceFloats[i] = skeleton.GetReferenceFloat(i);
        }

        return result;
    }

    /// <summary>
    ///     A playable clip from one animation and the binding that maps it onto the skeleton. Null when the animation
    ///     is of a kind this reader doesn't understand.
    /// </summary>
    private static HavokClip? BuildClip(IHavokAnimation animation, IHavokAnimationBinding? binding,
        HavokSkeletonData skeleton, bool bigEndian)
    {
        short[] trackToBone = binding?.TransformTrackToBoneIndices.ToArray() ?? Array.Empty<short>();
        short[] floatTrackToSlot = binding?.FloatTrackToFloatSlotIndices.ToArray() ?? Array.Empty<short>();
        HavokBlendMode blendMode = ToBlendMode(binding?.BlendHint ?? 0);

        switch (animation)
        {
            case IHavokSplineAnimation spline:
                return new HavokSplineClip(spline.Data as byte[] ?? spline.Data.ToArray(), bigEndian,
                    spline.TransformTrackCount, spline.BlockCount, spline.MaxFramesPerBlock, spline.FrameCount,
                    spline.Duration, spline.FrameDuration, trackToBone, skeleton.BoneCount, blendMode,
                    spline.BlockOffsets, spline.FloatTrackCount, spline.FloatBlockOffsets, floatTrackToSlot,
                    skeleton.FloatSlotCount);

            case IHavokInterleavedAnimation interleaved:
            {
                var transforms = new HavokTransformData[interleaved.TransformCount];
                for (var i = 0; i < transforms.Length; i++)
                {
                    transforms[i] = ToTransform(interleaved.GetTransform(i));
                }

                var floats = new float[interleaved.FloatCount];
                for (var i = 0; i < floats.Length; i++)
                {
                    floats[i] = interleaved.GetFloat(i);
                }

                return NewInterleavedClip(transforms, interleaved.TransformTrackCount, interleaved.Duration,
                    trackToBone, skeleton.BoneCount, blendMode, floats, interleaved.FloatTrackCount, floatTrackToSlot,
                    skeleton.FloatSlotCount);
            }

            case IHavokQuantizedAnimation quantized:
            {
                // Decoded once into whole poses, then played like an interleaved clip. hkaQuantizedAnimation leaves
                // out the components equal to the reference pose, which comes from the bone each track drives.
                HavokQsTransform ReferenceOf(int track)
                {
                    int bone = trackToBone.Length == 0 ? track : track < trackToBone.Length ? trackToBone[track] : -1;
                    if (bone < 0 || bone >= skeleton.ReferencePose.Length)
                    {
                        return new HavokQsTransform(Vector3.Zero, Quaternion.Identity, Vector3.One);
                    }

                    HavokTransformData r = skeleton.ReferencePose[bone];
                    return new HavokQsTransform(r.Translation, r.Rotation, r.Scale);
                }

                HavokQsTransform[] poses = quantized.DecodePoses(ReferenceOf);
                var transforms = new HavokTransformData[poses.Length];
                for (var i = 0; i < transforms.Length; i++)
                {
                    transforms[i] = ToTransform(poses[i]);
                }

                float[] floats = quantized.FloatTrackCount > 0 ? quantized.DecodeFloats() : Array.Empty<float>();

                return NewInterleavedClip(transforms, quantized.TransformTrackCount, quantized.Duration,
                    trackToBone, skeleton.BoneCount, blendMode, floats, quantized.FloatTrackCount,
                    floatTrackToSlot, skeleton.FloatSlotCount);
            }

            default:
                return null;
        }
    }

    private static HavokClip? NewInterleavedClip(HavokTransformData[] transforms, int trackCount, float duration,
        short[] trackToBone, int boneCount, HavokBlendMode blendMode, float[]? floats = null,
        int floatTrackCount = 0, short[]? floatTrackToSlot = null, int floatSlotCount = 0)
    {
        // A clip without a whole frame has nothing to sample
        if (trackCount <= 0 || transforms.Length < trackCount)
        {
            return null;
        }

        return new HavokInterleavedClip(transforms, trackCount, duration, trackToBone, boneCount, blendMode,
            floats, floatTrackCount, floatTrackToSlot, floatSlotCount);
    }

    private static HavokBlendMode ToBlendMode(int blendHint)
    {
        return blendHint switch
        {
            1 => HavokBlendMode.AdditiveParentSpace,
            2 => HavokBlendMode.AdditiveChildSpace,
            _ => HavokBlendMode.Normal
        };
    }

    private static HavokTransformData ToTransform(HavokQsTransform t)
    {
        return new HavokTransformData { Translation = t.Translation, Rotation = t.Rotation, Scale = t.Scale };
    }

    private static byte[] Decompress(byte[] data)
    {
        return DCX.Is(data) ? DCX.Decompress(data).ToArray() : data;
    }
}
