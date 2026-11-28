#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities;

/// <summary>
///     One layer of offset keys on an <see cref="EditableClip" /> (Animation Editor Analysis.md §13.3 item 7). Layers
///     are put on the clip's frames in order, the first one nearest the frames: each one's offset at a frame, scaled by
///     its <see cref="Weight" />, on top of the pose the layers before it give. A muted layer, a layer left out by
///     another's <see cref="Solo" />, or a bone outside its <see cref="Mask" /> adds nothing.
/// </summary>
public sealed class OffsetLayer
{
    public string Name = "Base";

    /// <summary>How much of the layer's offsets is used: 0 none, 1 all, between a part (above 1 more)</summary>
    public float Weight = 1;

    public bool Mute;

    /// <summary>With any layer soloed, only soloed layers are used</summary>
    public bool Solo;

    /// <summary>
    ///     The tracks the layer acts on (<see cref="EditableClip.RootTrack" /> for the root motion); null for all.
    ///     Keys on other tracks are kept but do nothing.
    /// </summary>
    public HashSet<int>? Mask;

    /// <summary>Offset keys by transform track, each list sorted by frame</summary>
    public Dictionary<int, List<BoneOffsetKey>> Keys = new();

    public bool HasKeys => Keys.Count > 0;

    public int KeyCount => Keys.Sum(k => k.Value.Count);

    public OffsetLayer Copy() => new()
    {
        Name = Name,
        Weight = Weight,
        Mute = Mute,
        Solo = Solo,
        Mask = Mask == null ? null : new HashSet<int>(Mask),
        Keys = Keys.ToDictionary(e => e.Key, e => e.Value.ToList())
    };

    /// <summary>An offset scaled by a weight: the turn that far from none (slerp, past 1 beyond it), the move times it</summary>
    public static BoneOffsetKey Scaled(BoneOffsetKey offset, float weight)
    {
        if (weight == 1)
            return offset;

        var rotation = offset.Rotation.W < 0 ? -offset.Rotation : offset.Rotation;
        return new BoneOffsetKey(offset.Frame, Quaternion.Normalize(Quaternion.Slerp(Quaternion.Identity, rotation, weight)),
            offset.Translation * weight, offset.Ease);
    }
}
