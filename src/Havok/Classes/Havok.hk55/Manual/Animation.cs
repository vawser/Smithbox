using System.Numerics;
using Havok.Shared;

namespace Havok.hk55;

// The cross-version animation views (Havok.Shared/Animation.cs), so skeletons and clips of every SDK read through one
// path. Hand-written: the classes are generated as partial for them.

public partial class hkaSkeleton : IHavokSkeleton
{
    string? IHavokSkeleton.Name => m_name;

    int IHavokSkeleton.BoneCount => m_bones.Count;

    string? IHavokSkeleton.GetBoneName(int bone) => m_bones[bone]?.m_name;

    short IHavokSkeleton.GetParent(int bone) => bone < m_parentIndices.Count ? m_parentIndices[bone] : (short)-1;

    HavokQsTransform IHavokSkeleton.GetReferencePose(int bone) => AnimationViews.ToShared(m_referencePose[bone]);
}

public partial class hkaAnimationBinding : IHavokAnimationBinding
{
    IHavokAnimation? IHavokAnimationBinding.Animation => m_animation as IHavokAnimation;

    IReadOnlyList<short> IHavokAnimationBinding.TransformTrackToBoneIndices => m_transformTrackToBoneIndices;

    int IHavokAnimationBinding.BlendHint => (int)m_blendHint;
}

public partial class hkaInterleavedSkeletalAnimation : IHavokInterleavedAnimation
{
    float IHavokAnimation.Duration => m_duration;

    int IHavokAnimation.TransformTrackCount => AnimationViews.TransformTracks(this);

    int IHavokInterleavedAnimation.TransformCount => m_transforms.Count;

    HavokQsTransform IHavokInterleavedAnimation.GetTransform(int index) => AnimationViews.ToShared(m_transforms[index]);
}

public partial class hkaDefaultAnimatedReferenceFrame : IHavokReferenceFrame
{
    Vector4 IHavokReferenceFrame.Up => m_up;

    Vector4 IHavokReferenceFrame.Forward => m_forward;

    float IHavokReferenceFrame.Duration => m_duration;

    int IHavokReferenceFrame.SampleCount => m_referenceFrameSamples.Count;

    Vector4 IHavokReferenceFrame.GetSample(int index) => m_referenceFrameSamples[index];
}

public partial class hkaWaveletSkeletalAnimation : IHavokQuantizedAnimation
{
    float IHavokAnimation.Duration => m_duration;

    int IHavokAnimation.TransformTrackCount => AnimationViews.TransformTracks(this);

    int IHavokQuantizedAnimation.PoseCount => m_numberOfPoses;

    HavokQsTransform[] IHavokQuantizedAnimation.DecodePoses() => HavokQuantizedAnimationCodec.Decode(new HavokQuantizedLayout
    {
        Codec = HavokQuantizedCodec.Wavelet,
        TransformTracks = AnimationViews.TransformTracks(this),
        FloatTracks = m_numberOfFloatTracks,
        Poses = m_numberOfPoses,
        BlockSize = m_blockSize,
        Preserved = m_qFormat.m_preserved,
        DynamicCount = (int)m_qFormat.m_numD,
        OffsetIdx = (int)m_qFormat.m_offsetIdx,
        ScaleIdx = (int)m_qFormat.m_scaleIdx,
        BitWidthIdx = (int)m_qFormat.m_bitWidthIdx,
        StaticMaskIdx = (int)m_staticMaskIdx,
        StaticDofsIdx = (int)m_staticDOFsIdx,
        QuantizedDataIdx = (int)m_quantizedDataIdx,
        QuantizedDataSize = (int)m_quantizedDataSize,
        BlockIndexIdx = (int)m_blockIndexIdx,
        BlockIndexSize = (int)m_blockIndexSize,
        Data = m_dataBuffer.ToArray()
    });
}

public partial class hkaDeltaCompressedSkeletalAnimation : IHavokQuantizedAnimation
{
    float IHavokAnimation.Duration => m_duration;

    int IHavokAnimation.TransformTrackCount => AnimationViews.TransformTracks(this);

    int IHavokQuantizedAnimation.PoseCount => m_numberOfPoses;

    HavokQsTransform[] IHavokQuantizedAnimation.DecodePoses() => HavokQuantizedAnimationCodec.Decode(new HavokQuantizedLayout
    {
        Codec = HavokQuantizedCodec.Delta,
        TransformTracks = AnimationViews.TransformTracks(this),
        FloatTracks = m_numberOfFloatTracks,
        Poses = m_numberOfPoses,
        BlockSize = m_blockSize,
        Preserved = m_qFormat.m_preserved,
        DynamicCount = (int)m_qFormat.m_numD,
        OffsetIdx = (int)m_qFormat.m_offsetIdx,
        ScaleIdx = (int)m_qFormat.m_scaleIdx,
        BitWidthIdx = (int)m_qFormat.m_bitWidthIdx,
        StaticMaskIdx = (int)m_staticMaskIdx,
        StaticDofsIdx = (int)m_staticDOFsIdx,
        QuantizedDataIdx = (int)m_quantizedDataIdx,
        QuantizedDataSize = (int)m_quantizedDataSize,
        Data = m_dataBuffer.ToArray()
    });
}

internal static class AnimationViews
{
    // Havok 5.1 files (46 DES clips) leave numberOfTransformTracks at 0 and give the count in numberOfTracks
    public static int TransformTracks(hkaSkeletalAnimation animation) =>
        animation.m_numberOfTransformTracks > 0 ? animation.m_numberOfTransformTracks : animation.m_numberOfTracks;

    public static HavokQsTransform ToShared(hkQsTransform t) =>
        new(new Vector3(t.m_translation.X, t.m_translation.Y, t.m_translation.Z), t.m_rotation,
            new Vector3(t.m_scale.X, t.m_scale.Y, t.m_scale.Z));
}
