using System.Numerics;
using Havok.Shared;

namespace Havok.hk2018;

// The cross-version animation views (Havok.Shared/Animation.cs), so skeletons and clips of every SDK read through one
// path. Hand-written: the classes are generated as partial for them.

public partial class hkaSkeleton : IHavokSkeleton
{
    string? IHavokSkeleton.Name => m_name;

    int IHavokSkeleton.BoneCount => m_bones.Count;

    string? IHavokSkeleton.GetBoneName(int bone) => m_bones[bone]?.m_name;

    short IHavokSkeleton.GetParent(int bone) => bone < m_parentIndices.Count ? m_parentIndices[bone] : (short)-1;

    HavokQsTransform IHavokSkeleton.GetReferencePose(int bone) => AnimationViews.ToShared(m_referencePose[bone]);

    int IHavokSkeleton.FloatSlotCount => m_floatSlots.Count;

    string? IHavokSkeleton.GetFloatSlotName(int slot) => m_floatSlots[slot];

    float IHavokSkeleton.GetReferenceFloat(int slot) => slot < m_referenceFloats.Count ? m_referenceFloats[slot] : 0;
}

public partial class hkaAnimationBinding : IHavokAnimationBinding
{
    IHavokAnimation? IHavokAnimationBinding.Animation => m_animation as IHavokAnimation;

    IReadOnlyList<short> IHavokAnimationBinding.TransformTrackToBoneIndices => m_transformTrackToBoneIndices;

    int IHavokAnimationBinding.BlendHint => (int)m_blendHint;

    string? IHavokAnimationBinding.OriginalSkeletonName => m_originalSkeletonName;

    IReadOnlyList<short> IHavokAnimationBinding.FloatTrackToFloatSlotIndices => m_floatTrackToFloatSlotIndices;
}

public partial class hkaSplineCompressedAnimation : IHavokSplineAnimation
{
    float IHavokAnimation.Duration => m_duration;

    int IHavokAnimation.TransformTrackCount => m_numberOfTransformTracks;

    int IHavokSplineAnimation.FrameCount => m_numFrames;

    int IHavokSplineAnimation.BlockCount => m_numBlocks;

    int IHavokSplineAnimation.MaxFramesPerBlock => m_maxFramesPerBlock;

    float IHavokSplineAnimation.FrameDuration => m_frameDuration;

    IReadOnlyList<byte> IHavokSplineAnimation.Data => m_data;

    int IHavokAnimation.FloatTrackCount => m_numberOfFloatTracks;

    IReadOnlyList<uint> IHavokSplineAnimation.BlockOffsets => m_blockOffsets;

    IReadOnlyList<uint> IHavokSplineAnimation.FloatBlockOffsets => m_floatBlockOffsets;
}

public partial class hkaInterleavedUncompressedAnimation : IHavokInterleavedAnimation
{
    float IHavokAnimation.Duration => m_duration;

    int IHavokAnimation.TransformTrackCount => m_numberOfTransformTracks;

    int IHavokInterleavedAnimation.TransformCount => m_transforms.Count;

    HavokQsTransform IHavokInterleavedAnimation.GetTransform(int index) => AnimationViews.ToShared(m_transforms[index]);

    int IHavokAnimation.FloatTrackCount => m_numberOfFloatTracks;

    int IHavokInterleavedAnimation.FloatCount => m_floats.Count;

    float IHavokInterleavedAnimation.GetFloat(int index) => m_floats[index];
}

public partial class hkaDefaultAnimatedReferenceFrame : IHavokReferenceFrame
{
    Vector4 IHavokReferenceFrame.Up => m_up;

    Vector4 IHavokReferenceFrame.Forward => m_forward;

    float IHavokReferenceFrame.Duration => m_duration;

    int IHavokReferenceFrame.SampleCount => m_referenceFrameSamples.Count;

    Vector4 IHavokReferenceFrame.GetSample(int index) => m_referenceFrameSamples[index];
}

internal static class AnimationViews
{
    public static HavokQsTransform ToShared(hkQsTransform t) =>
        new(new Vector3(t.m_translation.X, t.m_translation.Y, t.m_translation.Z), t.m_rotation,
            new Vector3(t.m_scale.X, t.m_scale.Y, t.m_scale.Z));
}

// Made partial by hand in Autogen: add hkaQuantizedAnimation to the generator's partial list on the next regeneration
public partial class hkaQuantizedAnimation : IHavokQuantizedAnimation
{
    float IHavokAnimation.Duration => m_duration;

    int IHavokAnimation.TransformTrackCount => m_numberOfTransformTracks;

    int IHavokAnimation.FloatTrackCount => m_numberOfFloatTracks;

    int IHavokQuantizedAnimation.PoseCount => HavokQuantizedAnimation2014Codec.ReadLayout(m_data.ToArray()).Frames;

    HavokQsTransform[] IHavokQuantizedAnimation.DecodePoses() => HavokQuantizedAnimation2014Codec.Decode(m_data.ToArray()).Poses;

    HavokQsTransform[] IHavokQuantizedAnimation.DecodePoses(Func<int, HavokQsTransform>? referencePose) =>
        HavokQuantizedAnimation2014Codec.Decode(m_data.ToArray(), referencePose).Poses;

    float[] IHavokQuantizedAnimation.DecodeFloats() => HavokQuantizedAnimation2014Codec.Decode(m_data.ToArray()).Floats;
}
