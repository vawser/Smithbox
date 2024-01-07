using SoulsFormats;
using StudioCore.Havok;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore;

public class NewHavokAnimation_SplineCompressed : NewHavokAnimation
{
    // Index into array = hkx bone index, result = transform track index.
    private readonly int[] HkxBoneIndexToTransformTrackMap;

    private readonly int[] TransformTrackIndexToHkxBoneMap;
    public int BlockCount = 1;
    public int NumFramesPerBlock = 255;
    public List<SplineCompressedAnimation.TransformTrack[]> Tracks;

    public NewHavokAnimation_SplineCompressed(NewAnimSkeleton skeleton,
        HKX.HKADefaultAnimatedReferenceFrame refFrame, HKX.HKAAnimationBinding binding,
        HKX.HKASplineCompressedAnimation anim)
        : base(skeleton, refFrame, binding)
    {
        Duration = anim.Duration; // Math.Max(anim.Duration, anim.FrameDuration * anim.FrameCount);
        FrameCount = anim.FrameCount;

        FrameDuration = anim.FrameDuration;

        BlockCount = anim.BlockCount;
        NumFramesPerBlock = anim.FramesPerBlock - 1;

        HkxBoneIndexToTransformTrackMap = new int[skeleton.HkxSkeleton.Count];
        TransformTrackIndexToHkxBoneMap = new int[binding.TransformTrackToBoneIndices.Size];

        for (var i = 0; i < binding.TransformTrackToBoneIndices.Size; i++)
        {
            TransformTrackIndexToHkxBoneMap[i] = -1;
        }

        for (var i = 0; i < skeleton.HkxSkeleton.Count; i++)
        {
            HkxBoneIndexToTransformTrackMap[i] = -1;
        }

        for (var i = 0; i < binding.TransformTrackToBoneIndices.Size; i++)
        {
            var boneIndex = binding.TransformTrackToBoneIndices[i].data;
            if (boneIndex >= 0)
            {
                HkxBoneIndexToTransformTrackMap[boneIndex] = i;
            }

            TransformTrackIndexToHkxBoneMap[i] = boneIndex;
        }

        Tracks = SplineCompressedAnimation.ReadSplineCompressedAnimByteBlock(
            false, anim.GetData(), anim.TransformTrackCount, anim.BlockCount);
    }

    private int CurrentBlock => (int)(CurrentFrame % FrameCount / NumFramesPerBlock);

    private NewBlendableTransform GetTransformOnSpecificBlockAndFrame(int transformIndex, int block, float frame)
    {
        frame = frame % FrameCount % NumFramesPerBlock;

        NewBlendableTransform result = NewBlendableTransform.Identity;
        SplineCompressedAnimation.TransformTrack track = Tracks[block][transformIndex];
        HKX.Transform skeleTransform =
            Skeleton.OriginalHavokSkeleton.Transforms[TransformTrackIndexToHkxBoneMap[transformIndex]];

        //result.Scale.X = track.SplineScale?.ChannelX == null
        //    ? (IsAdditiveBlend ? 1 : track.StaticScale.X) : track.SplineScale.GetValueX(frame);
        //result.Scale.Y = track.SplineScale?.ChannelY == null
        //    ? (IsAdditiveBlend ? 1 : track.StaticScale.Y) : track.SplineScale.GetValueY(frame);
        //result.Scale.Z = track.SplineScale?.ChannelZ == null
        //    ? (IsAdditiveBlend ? 1 : track.StaticScale.Z) : track.SplineScale.GetValueZ(frame);

        if (track.SplineScale != null)
        {
            result.Scale.X = track.SplineScale.GetValueX(frame)
                             ?? (IsAdditiveBlend ? 1 : skeleTransform.Scale.Vector.X);

            result.Scale.Y = track.SplineScale.GetValueY(frame)
                             ?? (IsAdditiveBlend ? 1 : skeleTransform.Scale.Vector.Y);

            result.Scale.Z = track.SplineScale.GetValueZ(frame)
                             ?? (IsAdditiveBlend ? 1 : skeleTransform.Scale.Vector.Z);
        }
        else
        {
            if (track.Mask.ScaleTypes.Contains(SplineCompressedAnimation.FlagOffset.StaticX))
            {
                result.Scale.X = track.StaticScale.X;
            }
            else
            {
                result.Scale.X = IsAdditiveBlend ? 1 : skeleTransform.Scale.Vector.X;
            }

            if (track.Mask.ScaleTypes.Contains(SplineCompressedAnimation.FlagOffset.StaticY))
            {
                result.Scale.Y = track.StaticScale.Y;
            }
            else
            {
                result.Scale.Y = IsAdditiveBlend ? 1 : skeleTransform.Scale.Vector.Y;
            }

            if (track.Mask.ScaleTypes.Contains(SplineCompressedAnimation.FlagOffset.StaticZ))
            {
                result.Scale.Z = track.StaticScale.Z;
            }
            else
            {
                result.Scale.Z = IsAdditiveBlend ? 1 : skeleTransform.Scale.Vector.Z;
            }
        }

        if (IsAdditiveBlend)
        {
            result.Scale.X *= skeleTransform.Scale.Vector.X;
            result.Scale.Y *= skeleTransform.Scale.Vector.Y;
            result.Scale.Z *= skeleTransform.Scale.Vector.Z;
        }

        //if (result.Scale.LengthSquared() > (Vector3.One * 1.1f).LengthSquared())
        //{
        //    Console.WriteLine(":fatoof:");
        //}

        if (track.SplineRotation != null) //track.HasSplineRotation)
        {
            result.Rotation = track.SplineRotation.GetValue(frame);
        }
        else if (track.HasStaticRotation)
        {
            // We actually need static rotation or Gael hands become unbent among others
            result.Rotation = track.StaticRotation;
        }

        //result.Rotation = IsAdditiveBlend ? Quaternion.Identity : new Quaternion(
        //    skeleTransform.Rotation.Vector.X,
        //    skeleTransform.Rotation.Vector.Y,
        //    skeleTransform.Rotation.Vector.Z,
        //    skeleTransform.Rotation.Vector.W);
        if (IsAdditiveBlend)
        {
            result.Rotation = new Quaternion(
                skeleTransform.Rotation.Vector.X,
                skeleTransform.Rotation.Vector.Y,
                skeleTransform.Rotation.Vector.Z,
                skeleTransform.Rotation.Vector.W) * result.Rotation;
        }

        if (track.SplinePosition != null)
        {
            result.Translation.X = track.SplinePosition.GetValueX(frame)
                                   ?? (IsAdditiveBlend ? 0 : skeleTransform.Position.Vector.X);

            result.Translation.Y = track.SplinePosition.GetValueY(frame)
                                   ?? (IsAdditiveBlend ? 0 : skeleTransform.Position.Vector.Y);

            result.Translation.Z = track.SplinePosition.GetValueZ(frame)
                                   ?? (IsAdditiveBlend ? 0 : skeleTransform.Position.Vector.Z);
        }
        else
        {
            if (track.Mask.PositionTypes.Contains(SplineCompressedAnimation.FlagOffset.StaticX))
            {
                result.Translation.X = track.StaticPosition.X;
            }
            else
            {
                result.Translation.X = IsAdditiveBlend ? 0 : skeleTransform.Position.Vector.X;
            }

            if (track.Mask.PositionTypes.Contains(SplineCompressedAnimation.FlagOffset.StaticY))
            {
                result.Translation.Y = track.StaticPosition.Y;
            }
            else
            {
                result.Translation.Y = IsAdditiveBlend ? 0 : skeleTransform.Position.Vector.Y;
            }

            if (track.Mask.PositionTypes.Contains(SplineCompressedAnimation.FlagOffset.StaticZ))
            {
                result.Translation.Z = track.StaticPosition.Z;
            }
            else
            {
                result.Translation.Z = IsAdditiveBlend ? 0 : skeleTransform.Position.Vector.Z;
            }
        }

        //result.Translation.X = track.SplinePosition?.GetValueX(frame) ?? (IsAdditiveBlend ? 0 : track.StaticPosition.X);
        //result.Translation.Y = track.SplinePosition?.GetValueY(frame) ?? (IsAdditiveBlend ? 0 : track.StaticPosition.Y);
        //result.Translation.Z = track.SplinePosition?.GetValueZ(frame) ?? (IsAdditiveBlend ? 0 : track.StaticPosition.Z);

        //if (!IsAdditiveBlend && (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX) &&
        //    !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticX)))
        //{
        //    result.Translation.X = skeleTransform.Position.Vector.X;
        //}

        //if (!IsAdditiveBlend && (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY) &&
        //    !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticY)))
        //{
        //    result.Translation.Y = skeleTransform.Position.Vector.Y;
        //}

        //if (!IsAdditiveBlend && (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ) &&
        //    !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticZ)))
        //{
        //    result.Translation.Z = skeleTransform.Position.Vector.Z;
        //}

        if (IsAdditiveBlend)
        {
            result.Translation.X += skeleTransform.Position.Vector.X;
            result.Translation.Y += skeleTransform.Position.Vector.Y;
            result.Translation.Z += skeleTransform.Position.Vector.Z;
        }

        return result;
    }

    public override NewBlendableTransform GetBlendableTransformOnCurrentFrame(int hkxBoneIndex)
    {
        var track = HkxBoneIndexToTransformTrackMap[hkxBoneIndex];

        if (track == -1)
        {
            HKX.Transform skeleTransform = Skeleton.OriginalHavokSkeleton.Transforms[hkxBoneIndex];

            NewBlendableTransform defaultBoneTransformation = new();

            defaultBoneTransformation.Scale.X = skeleTransform.Scale.Vector.X;
            defaultBoneTransformation.Scale.Y = skeleTransform.Scale.Vector.Y;
            defaultBoneTransformation.Scale.Z = skeleTransform.Scale.Vector.Z;

            defaultBoneTransformation.Rotation = new Quaternion(
                skeleTransform.Rotation.Vector.X,
                skeleTransform.Rotation.Vector.Y,
                skeleTransform.Rotation.Vector.Z,
                skeleTransform.Rotation.Vector.W);

            defaultBoneTransformation.Translation.X = skeleTransform.Position.Vector.X;
            defaultBoneTransformation.Translation.Y = skeleTransform.Position.Vector.Y;
            defaultBoneTransformation.Translation.Z = skeleTransform.Position.Vector.Z;

            return defaultBoneTransformation;
        }

        var frame = CurrentFrame % FrameCount % NumFramesPerBlock;

        if (frame >= FrameCount - 1)
        {
            NewBlendableTransform currentFrame = GetTransformOnSpecificBlockAndFrame(track,
                CurrentBlock, (float)Math.Floor(frame));
            NewBlendableTransform nextFrame = GetTransformOnSpecificBlockAndFrame(track, 0, 0);
            currentFrame = NewBlendableTransform.Lerp(frame % 1, currentFrame, nextFrame);
            return currentFrame;
        }
        // Regular frame
        else
        {
            NewBlendableTransform currentFrame = GetTransformOnSpecificBlockAndFrame(track,
                CurrentBlock, frame);
            return currentFrame;
        }
    }
}
