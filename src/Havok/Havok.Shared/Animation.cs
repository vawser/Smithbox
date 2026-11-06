using System.Numerics;

namespace Havok.Shared;

// Cross-version views of the animation classes. Every SDK's class set implements them in Manual partials, so code that
// plays skeletons and clips reads every game (packfile or tagfile, 32- or 64-bit) through one path.

/// <summary>A bone's local transform: hkQsTransform.</summary>
public readonly record struct HavokQsTransform(Vector3 Translation, Quaternion Rotation, Vector3 Scale);

/// <summary>hkaSkeleton.</summary>
public interface IHavokSkeleton
{
    string? Name { get; }

    int BoneCount { get; }

    string? GetBoneName(int bone);

    /// <summary>The parent bone's index, or -1 for a root.</summary>
    short GetParent(int bone);

    HavokQsTransform GetReferencePose(int bone);
}

/// <summary>hkaAnimationBinding: which bone each transform track drives.</summary>
public interface IHavokAnimationBinding
{
    IHavokAnimation? Animation { get; }

    /// <summary>Empty when the tracks are in bone order.</summary>
    IReadOnlyList<short> TransformTrackToBoneIndices { get; }

    /// <summary>hkaAnimationBinding::BlendHint: 0 normal, 1 additive (deprecated, parent space), 2 additive.</summary>
    int BlendHint { get; }
}

/// <summary>hkaAnimation (hkaSkeletalAnimation in Havok 5.x).</summary>
public interface IHavokAnimation
{
    float Duration { get; }

    int TransformTrackCount { get; }
}

/// <summary>hkaSplineCompressedAnimation.</summary>
public interface IHavokSplineAnimation : IHavokAnimation
{
    int FrameCount { get; }

    int BlockCount { get; }

    int MaxFramesPerBlock { get; }

    float FrameDuration { get; }

    /// <summary>The compressed blocks, in the byte order of the file they were read from.</summary>
    IReadOnlyList<byte> Data { get; }
}

/// <summary>hkaInterleavedUncompressedAnimation (hkaInterleavedSkeletalAnimation in Havok 5.x).</summary>
public interface IHavokInterleavedAnimation : IHavokAnimation
{
    /// <summary>Frame-major: frame f's track t is at f × <see cref="IHavokAnimation.TransformTrackCount" /> + t.</summary>
    int TransformCount { get; }

    HavokQsTransform GetTransform(int index);
}
