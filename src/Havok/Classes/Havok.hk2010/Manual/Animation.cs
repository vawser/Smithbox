using System.Numerics;
using Havok.Shared;

namespace Havok.hk2010;

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

public partial class hkaSplineCompressedAnimation : IHavokSplineAnimation
{
    float IHavokAnimation.Duration => m_duration;

    int IHavokAnimation.TransformTrackCount => m_numberOfTransformTracks;

    int IHavokSplineAnimation.FrameCount => m_numFrames;

    int IHavokSplineAnimation.BlockCount => m_numBlocks;

    int IHavokSplineAnimation.MaxFramesPerBlock => m_maxFramesPerBlock;

    float IHavokSplineAnimation.FrameDuration => m_frameDuration;

    IReadOnlyList<byte> IHavokSplineAnimation.Data => m_data;
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
