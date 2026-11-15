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

    // Float slots: named scalar channels beside the bones, such as a cutscene camera's FOV (camNNNN_FOV).
    // Defaults for SDKs whose skeletons have none.

    int FloatSlotCount => 0;

    string? GetFloatSlotName(int slot) => null;

    /// <summary>The slot's value when no float track drives it.</summary>
    float GetReferenceFloat(int slot) => 0;
}

/// <summary>hkaAnimationBinding: which bone each transform track drives.</summary>
public interface IHavokAnimationBinding
{
    IHavokAnimation? Animation { get; }

    /// <summary>Empty when the tracks are in bone order.</summary>
    IReadOnlyList<short> TransformTrackToBoneIndices { get; }

    /// <summary>hkaAnimationBinding::BlendHint: 0 normal, 1 additive (deprecated, parent space), 2 additive.</summary>
    int BlendHint { get; }

    /// <summary>
    /// The name of the skeleton the clip was made for. A file holding several skeletons and clips, such as a cutscene
    /// cut, pairs them by this name.
    /// </summary>
    string? OriginalSkeletonName => null;

    /// <summary>Which float slot each float track drives. Empty when the tracks are in slot order.</summary>
    IReadOnlyList<short> FloatTrackToFloatSlotIndices => [];
}

/// <summary>hkaAnimation (hkaSkeletalAnimation in Havok 5.x).</summary>
public interface IHavokAnimation
{
    float Duration { get; }

    int TransformTrackCount { get; }

    int FloatTrackCount => 0;
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

    /// <summary>Where each block starts in <see cref="Data" />. Empty for SDKs that don't store them.</summary>
    IReadOnlyList<uint> BlockOffsets => [];

    /// <summary>
    /// Where each block's float track data starts, from the start of the block. The block begins with one 4-byte mask
    /// per transform track, then one mask byte per float track: 0x01 a static float, 0x10 a spline, 0x02 16-bit
    /// (rather than 8-bit) spline values.
    /// </summary>
    IReadOnlyList<uint> FloatBlockOffsets => [];
}

/// <summary>
/// hkaDefaultAnimatedReferenceFrame: the root motion of a clip, which the game moves the character by rather than the
/// bones. Each sample is the offset from the start of the clip, with the turn about <see cref="Up" /> (radians) in W.
/// </summary>
public interface IHavokReferenceFrame
{
    Vector4 Up { get; }

    Vector4 Forward { get; }

    float Duration { get; }

    /// <summary>Spread evenly over <see cref="Duration" />, the first at 0 and the last at its end.</summary>
    int SampleCount { get; }

    Vector4 GetSample(int index);
}

/// <summary>
/// hkaWaveletSkeletalAnimation and hkaDeltaCompressedSkeletalAnimation (Havok 5.x): clips quantized into blocks, which
/// <see cref="HavokQuantizedAnimationCodec" /> decodes into whole poses.
/// </summary>
public interface IHavokQuantizedAnimation : IHavokAnimation
{
    /// <summary>The number of poses (frames), the first at time 0 and the last at the end of the clip.</summary>
    int PoseCount { get; }

    /// <summary>Frame-major like <see cref="IHavokInterleavedAnimation" />: frame f's track t is at f × tracks + t.</summary>
    HavokQsTransform[] DecodePoses();

    /// <summary>
    /// As <see cref="DecodePoses()" />, for codecs that leave out the components equal to the reference pose
    /// (hkaQuantizedAnimation): those come from <paramref name="referencePose" />, by transform track.
    /// </summary>
    HavokQsTransform[] DecodePoses(Func<int, HavokQsTransform>? referencePose) => DecodePoses();

    /// <summary>The float tracks, frame-major like the poses. Empty for codecs without them.</summary>
    float[] DecodeFloats() => [];
}

/// <summary>hkaInterleavedUncompressedAnimation (hkaInterleavedSkeletalAnimation in Havok 5.x).</summary>
public interface IHavokInterleavedAnimation : IHavokAnimation
{
    /// <summary>Frame-major: frame f's track t is at f × <see cref="IHavokAnimation.TransformTrackCount" /> + t.</summary>
    int TransformCount { get; }

    HavokQsTransform GetTransform(int index);

    /// <summary>Frame-major like the transforms: frame f's float track t is at f × float tracks + t.</summary>
    int FloatCount => 0;

    float GetFloat(int index) => 0;
}

/// <summary>
/// The content of a new animation for a binding, for <see cref="IHavokAnimationContainer.ReplaceAnimation" />: what
/// differs between clips. The rest of the animation (annotation tracks, extracted motion) is kept from the one it
/// replaces.
/// </summary>
public abstract class HavokAnimationContent
{
    public float Duration;
    public int TransformTrackCount;
    public int FloatTrackCount;
}

/// <summary>hkaSplineCompressedAnimation's fields, as an encoder gives them.</summary>
public sealed class HavokSplineContent : HavokAnimationContent
{
    public int FrameCount;
    public int BlockCount;
    public int MaxFramesPerBlock;
    public int MaskAndQuantizationSize;
    public float BlockDuration;
    public float BlockInverseDuration;
    public float FrameDuration;
    public byte[] Data = [];
    public uint[] BlockOffsets = [];
    public uint[] FloatBlockOffsets = [];
}

/// <summary>hkaInterleavedUncompressedAnimation's fields: every frame of every track, frame-major.</summary>
public sealed class HavokInterleavedContent : HavokAnimationContent
{
    public HavokQsTransform[] Transforms = [];
    public float[] Floats = [];
}

/// <summary>hkaAnimationContainer, for editing which animation a binding plays.</summary>
public interface IHavokAnimationContainer
{
    IReadOnlyList<IHavokAnimationBinding> Bindings { get; }

    /// <summary>
    /// Gives the binding a new animation of the content's kind, in place of its old one both in the binding and in the
    /// container's animation list. The old animation's annotation tracks, extracted motion and property bag carry
    /// over; every other object in the document is left as it was.
    /// </summary>
    void ReplaceAnimation(IHavokAnimationBinding binding, HavokAnimationContent content);
}