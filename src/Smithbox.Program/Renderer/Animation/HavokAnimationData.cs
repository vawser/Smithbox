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
    public string[] Names = Array.Empty<string>();
    public short[] Parents = Array.Empty<short>();
    public HavokTransformData[] ReferencePose = Array.Empty<HavokTransformData>();

    public int BoneCount => Names.Length;
}

public enum HavokBlendMode
{
    Normal,

    // The clip is a delta applied on top of the reference pose
    AdditiveParentSpace,
    AdditiveChildSpace
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

    // Index = hkx bone index, value = transform track index or -1
    public int[] BoneToTrack = Array.Empty<int>();

    public abstract NewBlendableTransform Sample(int track, float frame);

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
    private readonly int _framesPerBlock;

    public HavokSplineClip(byte[] data, bool bigEndian, int trackCount, int blockCount, int framesPerBlock,
        int frameCount, float duration, float frameDuration, short[] trackToBone, int boneCount,
        HavokBlendMode blendMode)
    {
        Duration = duration;
        FrameCount = frameCount;
        FrameDuration = frameDuration;
        BlendMode = blendMode;
        _framesPerBlock = framesPerBlock;

        _blocks = SplineCompressedAnimation.ReadSplineCompressedAnimByteBlock(bigEndian, data, trackCount,
            blockCount);

        BuildBoneMap(trackToBone, boneCount, trackCount);
    }

    public override NewBlendableTransform Sample(int track, float frame)
    {
        // Each block covers maxFramesPerBlock frames, of which the last is shared with the next block
        var span = _framesPerBlock - 1;
        var block = span > 0 ? (int)(frame / span) : 0;
        block = Math.Clamp(block, 0, _blocks.Count - 1);

        var frameInBlock = Math.Max(0, frame - block * span);
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

        return result;
    }
}

public sealed class HavokInterleavedClip : HavokClip
{
    private readonly HavokTransformData[] _transforms;
    private readonly int _trackCount;

    public HavokInterleavedClip(HavokTransformData[] transforms, int trackCount, float duration,
        short[] trackToBone, int boneCount, HavokBlendMode blendMode)
    {
        _transforms = transforms;
        _trackCount = trackCount;

        Duration = duration;
        FrameCount = trackCount > 0 ? transforms.Length / trackCount : 0;
        FrameDuration = FrameCount > 1 ? duration / (FrameCount - 1) : duration;
        BlendMode = blendMode;

        BuildBoneMap(trackToBone, boneCount, trackCount);
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

    public static HavokSkeletonData ReadSkeleton(ProjectType game, byte[] data, HavokCompendium? compendium)
    {
        HavokDocument document = HavokFile.Read(Decompress(data), ProfileOf(game), compendium);
        IHavokSkeleton skeleton = document.ObjectsOf<IHavokSkeleton>().FirstOrDefault()
                                  ?? throw new InvalidOperationException("The file has no skeleton.");

        var count = skeleton.BoneCount;
        HavokSkeletonData result = new()
        {
            Names = new string[count],
            Parents = new short[count],
            ReferencePose = new HavokTransformData[count]
        };

        for (var i = 0; i < count; i++)
        {
            result.Names[i] = skeleton.GetBoneName(i) ?? "";
            result.Parents[i] = skeleton.GetParent(i);
            result.ReferencePose[i] = ToTransform(skeleton.GetReferencePose(i));
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
        short[] trackToBone = binding?.TransformTrackToBoneIndices.ToArray() ?? Array.Empty<short>();
        HavokBlendMode blendMode = ToBlendMode(binding?.BlendHint ?? 0);

        if (document.ObjectsOf<IHavokSplineAnimation>().FirstOrDefault() is { } spline)
        {
            return new HavokSplineClip(spline.Data as byte[] ?? spline.Data.ToArray(), document.Source.BigEndian,
                spline.TransformTrackCount, spline.BlockCount, spline.MaxFramesPerBlock, spline.FrameCount,
                spline.Duration, spline.FrameDuration, trackToBone, skeleton.BoneCount, blendMode);
        }

        if (document.ObjectsOf<IHavokInterleavedAnimation>().FirstOrDefault() is { } interleaved)
        {
            var transforms = new HavokTransformData[interleaved.TransformCount];
            for (var i = 0; i < transforms.Length; i++)
            {
                transforms[i] = ToTransform(interleaved.GetTransform(i));
            }

            return new HavokInterleavedClip(transforms, interleaved.TransformTrackCount, interleaved.Duration,
                trackToBone, skeleton.BoneCount, blendMode);
        }

        return null;
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
