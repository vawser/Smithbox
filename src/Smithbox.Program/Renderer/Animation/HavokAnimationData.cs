#nullable enable
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
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
///     Reads skeletons and clips from either hkx container: packfiles (DES, DS1, DS3, BB)
///     and tagfiles (DS1R, SDT, ER, NR, AC6).
/// </summary>
public static class HavokAnimationReader
{
    public static HavokSkeletonData ReadSkeleton(ProjectType game, byte[] data, byte[]? compendium)
    {
        data = Decompress(data);

        if (HavokTagfile.IsTagfile(data))
        {
            HavokTagfile tag = HavokTagfile.Read(data, ReadCompendium(compendium));
            foreach (HavokTagfile.TagObject skeleton in tag.FindObjects("hkaSkeleton"))
            {
                HavokTagfile.TagObject[] bones = skeleton.GetObjectArray("bones");
                var names = new string[bones.Length];
                for (var i = 0; i < names.Length; i++)
                {
                    names[i] = bones[i].GetString("name");
                }

                return new HavokSkeletonData
                {
                    Names = names,
                    Parents = skeleton.GetInt16Array("parentIndices"),
                    ReferencePose = skeleton.GetTransformArray("referencePose")
                };
            }

            throw new InvalidOperationException("Tagfile has no skeleton.");
        }

        HKX hkx = HKX.Read(data, GetVariation(game));
        foreach (HKX.HKXObject obj in hkx.DataSection.Objects)
        {
            if (obj is HKX.HKASkeleton skeleton)
            {
                var count = (int)skeleton.Bones.Size;
                HavokSkeletonData result = new()
                {
                    Names = new string[count],
                    Parents = new short[count],
                    ReferencePose = new HavokTransformData[count]
                };

                for (var i = 0; i < count; i++)
                {
                    result.Names[i] = skeleton.Bones[i].Name.GetString();
                    result.Parents[i] = skeleton.ParentIndices[i].data;
                    result.ReferencePose[i] = ToTransform(skeleton.Transforms[i]);
                }

                return result;
            }
        }

        throw new InvalidOperationException("Packfile has no skeleton.");
    }

    /// <summary>
    ///     Returns null when the file has no clip this reader understands.
    /// </summary>
    public static HavokClip? ReadClip(ProjectType game, byte[] data, byte[]? compendium, HavokSkeletonData skeleton)
    {
        data = Decompress(data);

        return HavokTagfile.IsTagfile(data)
            ? ReadTagfileClip(data, compendium, skeleton)
            : ReadPackfileClip(game, data, skeleton);
    }

    private static HavokClip? ReadTagfileClip(byte[] data, byte[]? compendium, HavokSkeletonData skeleton)
    {
        HavokTagfile tag = HavokTagfile.Read(data, ReadCompendium(compendium));

        short[] trackToBone = Array.Empty<short>();
        HavokBlendMode blendMode = HavokBlendMode.Normal;

        foreach (HavokTagfile.TagObject binding in tag.FindObjects("hkaAnimationBinding"))
        {
            trackToBone = binding.GetInt16Array("transformTrackToBoneIndices");
            blendMode = ToBlendMode(binding.GetByte("blendHint"));
            break;
        }

        foreach (HavokTagfile.TagObject anim in tag.FindObjects("hkaSplineCompressedAnimation"))
        {
            return new HavokSplineClip(anim.GetByteArray("data"), false,
                anim.GetInt32("numberOfTransformTracks"), anim.GetInt32("numBlocks"),
                anim.GetInt32("maxFramesPerBlock"), anim.GetInt32("numFrames"), anim.GetSingle("duration"),
                anim.GetSingle("frameDuration"), trackToBone, skeleton.BoneCount, blendMode);
        }

        foreach (HavokTagfile.TagObject anim in tag.FindObjects("hkaInterleavedUncompressedAnimation"))
        {
            return new HavokInterleavedClip(anim.GetTransformArray("transforms"),
                anim.GetInt32("numberOfTransformTracks"), anim.GetSingle("duration"), trackToBone,
                skeleton.BoneCount, blendMode);
        }

        return null;
    }

    private static HavokClip? ReadPackfileClip(ProjectType game, byte[] data, HavokSkeletonData skeleton)
    {
        HKX hkx = HKX.Read(data, GetVariation(game));

        HKX.HKASplineCompressedAnimation? spline = null;
        HKX.HKAInterleavedUncompressedAnimation? interleaved = null;
        HKX.HKAAnimationBinding? binding = null;

        foreach (HKX.HKXObject obj in hkx.DataSection.Objects)
        {
            switch (obj)
            {
                case HKX.HKASplineCompressedAnimation s:
                    spline = s;
                    break;
                case HKX.HKAInterleavedUncompressedAnimation i:
                    interleaved = i;
                    break;
                case HKX.HKAAnimationBinding b:
                    binding = b;
                    break;
            }
        }

        short[] trackToBone = Array.Empty<short>();
        if (binding != null)
        {
            trackToBone = new short[binding.TransformTrackToBoneIndices.Size];
            for (var i = 0; i < trackToBone.Length; i++)
            {
                trackToBone[i] = binding.TransformTrackToBoneIndices[i].data;
            }
        }

        HavokBlendMode blendMode = ToBlendMode((byte)(binding?.BlendHint ?? HKX.AnimationBlendHint.NORMAL));

        if (spline != null)
        {
            return new HavokSplineClip(spline.GetData(), hkx.Header.Endian == 0, spline.TransformTrackCount,
                spline.BlockCount, spline.FramesPerBlock, spline.FrameCount, spline.Duration, spline.FrameDuration,
                trackToBone, skeleton.BoneCount, blendMode);
        }

        if (interleaved != null)
        {
            var transforms = new HavokTransformData[interleaved.Transforms.Size];
            for (var i = 0; i < transforms.Length; i++)
            {
                transforms[i] = ToTransform(interleaved.Transforms[i]);
            }

            return new HavokInterleavedClip(transforms, interleaved.TransformTrackCount, interleaved.Duration,
                trackToBone, skeleton.BoneCount, blendMode);
        }

        return null;
    }

    private static HavokBlendMode ToBlendMode(byte blendHint)
    {
        return blendHint switch
        {
            1 => HavokBlendMode.AdditiveParentSpace,
            2 => HavokBlendMode.AdditiveChildSpace,
            _ => HavokBlendMode.Normal
        };
    }

    private static HavokTransformData ToTransform(HKX.Transform t)
    {
        return new HavokTransformData
        {
            Translation = new Vector3(t.Position.Vector.X, t.Position.Vector.Y, t.Position.Vector.Z),
            Rotation = new Quaternion(t.Rotation.Vector.X, t.Rotation.Vector.Y, t.Rotation.Vector.Z,
                t.Rotation.Vector.W),
            Scale = new Vector3(t.Scale.Vector.X, t.Scale.Vector.Y, t.Scale.Vector.Z)
        };
    }

    private static byte[] Decompress(byte[] data)
    {
        return DCX.Is(data) ? DCX.Decompress(data).ToArray() : data;
    }

    private static List<HavokTagfile.TagType>? ReadCompendium(byte[]? compendium)
    {
        return compendium != null ? HavokTagfile.ReadCompendium(Decompress(compendium)) : null;
    }

    private static HKX.HKXVariation GetVariation(ProjectType game)
    {
        return game switch
        {
            ProjectType.DES => HKX.HKXVariation.HKXDeS,
            ProjectType.DS1 or ProjectType.DS1R => HKX.HKXVariation.HKXDS1,
            ProjectType.BB => HKX.HKXVariation.HKXBloodBorne,
            _ => HKX.HKXVariation.HKXDS3
        };
    }
}
