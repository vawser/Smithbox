#nullable enable
using Havok;
using Havok.Shared;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities;

/// <summary>
///     A character clip held as one local pose per whole frame (by transform track, in the binding's order), its float
///     tracks and its root motion, for editing (<see cref="EditableClipOperations" />) and writing back into an HKX
///     file (<see cref="Write" />). See Claude/Documentation/Animation Editor Analysis.md §4.1.
///     <para>
///         Vanilla clips keep no authored keys (their splines are fitted per block), so a clip is edited as frames.
///         Between frames it is sampled by lerp and slerp, which is what the encoder fits.
///     </para>
///     <para>
///         Annotation tracks aren't held: the writer keeps the old clip's. No vanilla character clip has annotations
///         (ER, NR and DB have no annotation tracks; AC6, SDT and DS3 have only empty ones), so their times can't go stale.
///     </para>
/// </summary>
public sealed class EditableClip
{
    public int TrackCount;
    public int FloatTrackCount;
    public float FrameDuration = 1f / 30f;
    public HavokBlendMode BlendMode;

    /// <summary>[frame][transform track]: the local transform</summary>
    public HavokTransformData[][] Frames = Array.Empty<HavokTransformData[]>();

    /// <summary>[frame][float track]</summary>
    public float[][] Floats = Array.Empty<float[]>();

    /// <summary>
    ///     The root motion, one sample per frame: XYZ where the root is (model space), W how far it has turned about
    ///     <see cref="Up" /> (radians, adding up). Null when the clip has none.
    /// </summary>
    public Vector4[]? RootMotion;

    public Vector4 Up = new(0, 1, 0, 0);
    public Vector4 Forward = new(0, 0, 1, 0);

    /// <summary>The binding's track to bone map (empty: track i is bone i)</summary>
    public short[] TrackToBone = Array.Empty<short>();

    /// <summary>The binding's float track to float slot map (empty: track i is slot i)</summary>
    public short[] FloatTrackToSlot = Array.Empty<short>();

    /// <summary>
    ///     The layers of offset keys, in the order they are put on the frames (§13.3 item 7); always at least one. They
    ///     are kept apart from <see cref="Frames" /> until <see cref="Baked" />, which playback, saving and the
    ///     whole-clip operations use. A clip is not changed once made: an edit makes a new clip (<see cref="WithKey" />).
    /// </summary>
    public List<OffsetLayer> Layers = new() { new OffsetLayer() };

    /// <summary>The layer keying acts on (<see cref="Offsets" />, <see cref="WithKey" />, <see cref="OffsetAt" />)</summary>
    public int ActiveLayer;

    public OffsetLayer Active => Layers[Math.Clamp(ActiveLayer, 0, Layers.Count - 1)];

    /// <summary>
    ///     The active layer's offset keys by transform track, each list sorted by frame (Animation Editor Analysis.md
    ///     §4.7). Everything that keys works on these; <see cref="Baked" /> puts every layer on.
    /// </summary>
    public Dictionary<int, List<BoneOffsetKey>> Offsets
    {
        get => Active.Keys;
        set => Active.Keys = value;
    }

    /// <summary>
    ///     The <see cref="Offsets" /> entry of the root motion's keys: a key's Translation moves the root (model space)
    ///     and its Rotation, a turn about <see cref="UpAxis" />, turns it. Baked into <see cref="RootMotion" />, which is
    ///     made (from zero) for a clip that has none.
    /// </summary>
    public const int RootTrack = -1;

    /// <summary>The ease of keys without their own (<see cref="BoneOffsetKey.Ease" />): the clip's default</summary>
    public BoneOffsetEase OffsetEase = BoneOffsetEase.Smooth;

    /// <summary>Whether any layer has keys (so playing or saving the clip bakes them)</summary>
    public bool HasOffsets => Layers.Any(l => l.HasKeys);

    /// <summary>Whether any layer has root motion keys</summary>
    public bool HasRootKeys => Layers.Any(l => l.Keys.ContainsKey(RootTrack));

    /// <summary>Every layer's keys, for counts</summary>
    public int KeyCountAllLayers => Layers.Sum(l => l.KeyCount);

    /// <summary>How much layer <paramref name="layer" /> acts on a track: 0 when muted, soloed out or masked out</summary>
    public float LayerWeight(int layer, int track)
    {
        var l = Layers[layer];
        if (l.Mute || l.Solo == false && Layers.Any(x => x.Solo) || l.Mask != null && !l.Mask.Contains(track))
            return 0;

        return l.Weight;
    }

    /// <summary>
    ///     Layers <paramref name="from" /> to <paramref name="to" /> (both included) at a frame as one offset: their
    ///     weighted turns one on the other (a later layer's outermost), their moves added. Identity for none.
    /// </summary>
    public BoneOffsetKey CompositeOffset(int track, float frame, int from = 0, int to = int.MaxValue)
    {
        var rotation = Quaternion.Identity;
        var translation = Vector3.Zero;
        for (var i = Math.Max(from, 0); i <= Math.Min(to, Layers.Count - 1); i++)
        {
            var weight = LayerWeight(i, track);
            if (weight == 0 || !Layers[i].Keys.ContainsKey(track))
                continue;

            var offset = OffsetLayer.Scaled(LayerOffsetAt(i, track, frame), weight);
            rotation = Quaternion.Normalize(offset.Rotation * rotation);
            translation += offset.Translation;
        }

        return new BoneOffsetKey(-1, rotation, translation);
    }

    /// <summary>The weight the active layer acts on a track with (0: its keys there do nothing)</summary>
    public float ActiveWeight(int track) => LayerWeight(Math.Clamp(ActiveLayer, 0, Layers.Count - 1), track);

    /// <summary>The root motion as it plays: with the root keys, or as the clip has it</summary>
    public Vector4[]? EffectiveRootMotion => HasRootKeys ? Baked().RootMotion : RootMotion;

    // The clip with its offsets applied, made once
    private EditableClip? _baked;

    public int FrameCount => Frames.Length;

    public float Duration => Math.Max(FrameCount - 1, 0) * FrameDuration;

    public int BoneOf(int track) =>
        TrackToBone.Length == 0 ? track : track < TrackToBone.Length ? TrackToBone[track] : -1;

    /// <summary>
    ///     Reads the first animation of an HKX file (DCX or not). Null when the file has no animation this reader can
    ///     decode.
    /// </summary>
    /// <param name="compendium">The type compendium of the binder the file is from, for a tagfile that uses one</param>
    public static EditableClip? Read(ProjectType game, byte[] data, HavokCompendium? compendium,
        HavokSkeletonData skeleton)
    {
        var raw = Unpack(data);
        var clip = HavokAnimationReader.ReadClip(game, raw, compendium, skeleton);
        if (clip == null)
            return null;

        var document = Havok.Files.HavokFile.Read(raw, HavokAnimationReader.ProfileOf(game), compendium);
        var binding = document.ObjectsOf<IHavokAnimationBinding>().FirstOrDefault();
        var animation = binding?.Animation;
        if (binding == null || animation == null)
            return null;

        var result = new EditableClip
        {
            TrackCount = binding.TransformTrackToBoneIndices.Count > 0
                ? binding.TransformTrackToBoneIndices.Count
                : animation.TransformTrackCount,
            FloatTrackCount = binding.FloatTrackToFloatSlotIndices.Count > 0
                ? binding.FloatTrackToFloatSlotIndices.Count
                : animation.FloatTrackCount,
            FrameDuration = clip.FrameDuration > 0 ? clip.FrameDuration : 1f / 30f,
            BlendMode = clip.BlendMode,
            TrackToBone = binding.TransformTrackToBoneIndices.ToArray(),
            FloatTrackToSlot = binding.FloatTrackToFloatSlotIndices.ToArray()
        };

        var frameCount = Math.Max(clip.FrameCount, 2);
        result.Frames = new HavokTransformData[frameCount][];
        result.Floats = new float[frameCount][];

        for (var f = 0; f < frameCount; f++)
        {
            var pose = new HavokTransformData[result.TrackCount];
            for (var t = 0; t < result.TrackCount; t++)
            {
                var x = clip.Sample(t, f);
                pose[t] = new HavokTransformData { Translation = x.Translation, Rotation = x.Rotation, Scale = x.Scale };
            }

            result.Frames[f] = pose;

            var floats = new float[result.FloatTrackCount];
            for (var t = 0; t < result.FloatTrackCount; t++)
            {
                // A float track without data at a frame keeps its slot's reference value
                var slot = result.FloatTrackToSlot.Length == 0 ? t : t < result.FloatTrackToSlot.Length ? result.FloatTrackToSlot[t] : -1;
                floats[t] = clip.SampleFloat(t, f)
                            ?? (slot >= 0 && slot < skeleton.ReferenceFloats.Length ? skeleton.ReferenceFloats[slot] : 0);
            }

            result.Floats[f] = floats;
        }

        if (document.ObjectsOf<IHavokReferenceFrame>().FirstOrDefault() is { SampleCount: > 0 } frame &&
            clip.RootMotion != null)
        {
            result.Up = frame.Up;
            result.Forward = frame.Forward;
            result.RootMotion = new Vector4[frameCount];
            for (var f = 0; f < frameCount; f++)
            {
                result.RootMotion[f] = clip.RootMotion.SampleRaw(f * result.FrameDuration);
            }
        }

        return result;
    }

    /// <summary>A track's local transform at a frame, lerped and slerped between whole frames</summary>
    public HavokTransformData Sample(int track, float frame)
    {
        if (FrameCount == 0)
            return new HavokTransformData { Rotation = Quaternion.Identity, Scale = Vector3.One };

        frame = Math.Clamp(frame, 0, FrameCount - 1);
        var first = (int)frame;
        var second = Math.Min(first + 1, FrameCount - 1);
        var weight = frame - first;
        var a = Frames[first][track];
        if (weight <= 0 || first == second)
            return a;

        return Blend(a, Frames[second][track], weight);
    }

    public float SampleFloat(int track, float frame)
    {
        frame = Math.Clamp(frame, 0, FrameCount - 1);
        var first = (int)frame;
        var second = Math.Min(first + 1, FrameCount - 1);
        return Floats[first][track] + (Floats[second][track] - Floats[first][track]) * (frame - first);
    }

    /// <summary>
    ///     Writes the clip in place of the animation of an HKX file and returns the new file (not DCX compressed). The
    ///     rest of the file (binding, annotation tracks, property bag) is kept, so it binds to the skeleton as before.
    /// </summary>
    /// <param name="baseFile">The file whose animation is replaced (DCX or not), normally the clip's own source</param>
    public byte[] Write(ProjectType game, byte[] baseFile, HavokCompendium? compendium, out HavokClipWriteResult result,
        HavokClipFormat format = HavokClipFormat.Auto, SplineEncodeSettings? settings = null)
    {
        if (FrameCount < 2)
            throw new InvalidOperationException("A clip needs at least two frames.");

        if (HasOffsets)
            return Baked().Write(game, baseFile, compendium, out result, format, settings);

        var document = Havok.Files.HavokFile.Read(Unpack(baseFile), HavokAnimationReader.ProfileOf(game), compendium);

        HavokReferenceFrameContent? referenceFrame = RootMotion == null
            ? null
            : new HavokReferenceFrameContent
            {
                Up = Up,
                Forward = Forward,
                Duration = Duration,
                Samples = RootMotion.ToArray()
            };

        HavokTransformData SampleTransform(int track, float frame) => Sample(track, frame);

        // A character clip is stored as vanilla character clips are (THREECOMP40 rotations), unless asked otherwise
        result = HavokClipWriter.ReplaceClip(document, null, FrameCount, TrackCount, FloatTrackCount, SampleTransform,
            FloatTrackCount > 0 ? SampleFloat : null, FrameDuration, format,
            settings ?? SplineEncodeSettings.ForCharacterClips(), referenceFrame);

        return Havok.Files.HavokFile.Write(document);
    }

    /// <summary>
    ///     The clip for <see cref="ModelAnimator" />, so an edit plays before it is saved: its frames as an interleaved
    ///     clip, with its root motion.
    /// </summary>
    public HavokClip ToPlaybackClip(HavokSkeletonData skeleton)
    {
        if (HasOffsets)
            return Baked().ToPlaybackClip(skeleton);

        var transforms = new HavokTransformData[FrameCount * TrackCount];
        for (var f = 0; f < FrameCount; f++)
            Array.Copy(Frames[f], 0, transforms, f * TrackCount, TrackCount);

        var floats = new float[FrameCount * FloatTrackCount];
        for (var f = 0; f < FrameCount; f++)
            Array.Copy(Floats[f], 0, floats, f * FloatTrackCount, FloatTrackCount);

        var clip = new HavokInterleavedClip(transforms, TrackCount, Duration, TrackToBone, skeleton.BoneCount, BlendMode,
            floats, FloatTrackCount, FloatTrackToSlot, skeleton.FloatSlotCount);

        if (RootMotion is { Length: > 0 } motion)
            clip.RootMotion = new HavokRootMotion(motion.ToArray(), Duration, new Vector3(Up.X, Up.Y, Up.Z));

        return clip;
    }

    public EditableClip Clone()
    {
        var copy = (EditableClip)MemberwiseClone();
        copy.Frames = Frames.Select(p => p.ToArray()).ToArray();
        copy.Floats = Floats.Select(p => p.ToArray()).ToArray();
        copy.RootMotion = RootMotion?.ToArray();
        copy.TrackToBone = TrackToBone.ToArray();
        copy.FloatTrackToSlot = FloatTrackToSlot.ToArray();
        copy.Layers = Layers.Select(l => l.Copy()).ToList();
        copy._baked = null;
        return copy;
    }

    // ---- layers (each a new clip, as every edit is)

    /// <summary>A copy with a new empty layer on top, made active</summary>
    public EditableClip WithNewLayer(string name)
    {
        var copy = Clone();
        copy.Layers.Add(new OffsetLayer { Name = name });
        copy.ActiveLayer = copy.Layers.Count - 1;
        return copy;
    }

    /// <summary>A copy with a layer's copy right above it, made active</summary>
    public EditableClip WithDuplicatedLayer(int layer, string name)
    {
        var copy = Clone();
        var duplicate = copy.Layers[layer].Copy();
        duplicate.Name = name;
        copy.Layers.Insert(layer + 1, duplicate);
        copy.ActiveLayer = layer + 1;
        return copy;
    }

    /// <summary>A copy without a layer (the last one left is emptied instead)</summary>
    public EditableClip WithoutLayer(int layer)
    {
        var copy = Clone();
        if (copy.Layers.Count == 1)
            copy.Layers[0] = new OffsetLayer { Name = copy.Layers[0].Name };
        else
            copy.Layers.RemoveAt(layer);

        copy.ActiveLayer = Math.Clamp(ActiveLayer > layer ? ActiveLayer - 1 : ActiveLayer, 0, copy.Layers.Count - 1);
        return copy;
    }

    /// <summary>A copy with a layer moved up (+1) or down (−1) the order, the active one following it</summary>
    public EditableClip WithLayerMoved(int layer, int by)
    {
        var to = Math.Clamp(layer + by, 0, Layers.Count - 1);
        var copy = Clone();
        if (to == layer)
            return copy;

        var moved = copy.Layers[layer];
        copy.Layers.RemoveAt(layer);
        copy.Layers.Insert(to, moved);
        copy.ActiveLayer = ActiveLayer == layer ? to : ActiveLayer == to ? layer : ActiveLayer;
        return copy;
    }

    /// <summary>A copy with a layer changed by <paramref name="change" /> (name, weight, mute, solo, mask)</summary>
    public EditableClip WithLayer(int layer, Action<OffsetLayer> change)
    {
        var copy = Clone();
        change(copy.Layers[layer]);
        return copy;
    }

    public EditableClip WithActiveLayer(int layer)
    {
        var copy = Clone();
        copy.ActiveLayer = Math.Clamp(layer, 0, copy.Layers.Count - 1);
        return copy;
    }

    /// <summary>The index of the layer of that name (ignoring case), -1 for none</summary>
    public int LayerNamed(string name) => Layers.FindIndex(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    // ---- offset keys

    /// <summary>
    ///     The clip with every offset key applied to its frames, and no keys of its own. Made once per clip; a clip
    ///     without keys is its own.
    /// </summary>
    public EditableClip Baked()
    {
        if (!HasOffsets)
            return this;

        if (_baked != null)
            return _baked;

        var baked = Clone();
        baked.Layers = new List<OffsetLayer> { new() { Name = Layers[0].Name } };
        baked.ActiveLayer = 0;
        foreach (var track in Layers.SelectMany(l => l.Keys.Keys).Distinct())
        {
            if (track == RootTrack)
            {
                baked.RootMotion = RootWithKeys();
                continue;
            }

            for (var f = 0; f < FrameCount; f++)
                baked.Frames[f][track] = ApplyOffset(Frames[f][track], CompositeOffset(track, f));
        }

        _baked = baked;
        return baked;
    }

    /// <summary>
    ///     The clip with only its root keys folded into its root motion, keeping the bones' keys: for the root motion
    ///     operations, which work on the root motion as it plays
    /// </summary>
    public EditableClip BakedRoot()
    {
        if (!HasRootKeys)
            return this;

        var copy = Clone();
        copy.RootMotion = RootWithKeys();
        foreach (var layer in copy.Layers)
            layer.Keys.Remove(RootTrack);
        return copy;
    }

    /// <summary>Each frame's root motion sample with every layer's root keys' move and turn added</summary>
    private Vector4[] RootWithKeys()
    {
        var result = new Vector4[FrameCount];
        for (var f = 0; f < FrameCount; f++)
        {
            var sample = RootMotion != null && f < RootMotion.Length ? RootMotion[f] : Vector4.Zero;
            var (move, turn) = RootKeysAt(f, -1);
            result[f] = sample + new Vector4(move, turn);
        }

        return result;
    }

    /// <summary>
    ///     The root keys' move and turn (radians about <see cref="UpAxis" />) at a frame, every layer's weighted and
    ///     added, but for <paramref name="except" />. Turns are added as angles, so several make more than half a turn.
    /// </summary>
    private (Vector3 Move, float Turn) RootKeysAt(float frame, int except)
    {
        var up = UpAxis;
        var move = Vector3.Zero;
        var turn = 0f;
        for (var i = 0; i < Layers.Count; i++)
        {
            var weight = LayerWeight(i, RootTrack);
            if (i == except || weight == 0 || !Layers[i].Keys.ContainsKey(RootTrack))
                continue;

            var offset = LayerOffsetAt(i, RootTrack, frame);
            move += offset.Translation * weight;
            turn += TwistAngle(offset.Rotation, up) * weight;
        }

        return (move, turn);
    }

    /// <summary>The signed angle (radians) a rotation turns about an axis: its twist, the rest dropped</summary>
    public static float TwistAngle(Quaternion rotation, Vector3 axis)
    {
        var q = Quaternion.Normalize(rotation);
        var along = Vector3.Dot(new Vector3(q.X, q.Y, q.Z), axis);
        var angle = 2 * MathF.Atan2(along, q.W);

        // The shorter way round
        if (angle > MathF.PI)
            angle -= 2 * MathF.PI;
        else if (angle < -MathF.PI)
            angle += 2 * MathF.PI;

        return angle;
    }

    /// <summary>
    ///     A copy whose root key at <paramref name="frame" /> puts the root at a model-space position and / or turn
    ///     (radians about <see cref="UpAxis" />) there; null keeps the key's own. The offset is from the clip's own root
    ///     motion at that frame (zero for a clip without any). The viewport gizmo's edit of the root.
    /// </summary>
    public EditableClip KeyedRoot(int frame, Vector3? position, float? turn)
    {
        // The active layer's key is what is left after the root motion and the other layers, undone by its weight
        var weight = ActiveWeight(RootTrack);
        if (MathF.Abs(weight) < 1e-4f)
            return this;

        var sample = RootMotion != null && frame >= 0 && frame < RootMotion.Length ? RootMotion[frame] : Vector4.Zero;
        var (otherMove, otherTurn) = RootKeysAt(frame, ActiveLayer);
        var existing = OffsetAt(RootTrack, frame);
        var move = position is { } p ? (p - new Vector3(sample.X, sample.Y, sample.Z) - otherMove) / weight : existing.Translation;
        var rotation = turn is { } t ? Quaternion.CreateFromAxisAngle(UpAxis, (t - sample.W - otherTurn) / weight) : existing.Rotation;
        return WithKey(RootTrack, new BoneOffsetKey(frame, Quaternion.Normalize(rotation), move));
    }

    /// <summary>
    ///     The active layer's offset of a track at a frame: slerped / lerped between the keys around it (eased as
    ///     each next key's ease), held before the first key and after the last. Identity without keys. Not weighted:
    ///     the key values as typed and shown.
    /// </summary>
    public BoneOffsetKey OffsetAt(int track, float frame) => LayerOffsetAt(Math.Clamp(ActiveLayer, 0, Layers.Count - 1), track, frame);

    /// <summary>A layer's offset of a track at a frame, as <see cref="OffsetAt" /> (not weighted)</summary>
    public BoneOffsetKey LayerOffsetAt(int layer, int track, float frame)
    {
        if (!Layers[layer].Keys.TryGetValue(track, out var keys) || keys.Count == 0)
            return BoneOffsetKey.Identity(0);

        if (frame <= keys[0].Frame)
            return keys[0];

        if (frame >= keys[^1].Frame)
            return keys[^1];

        var next = keys.FindIndex(k => k.Frame >= frame);
        var a = keys[next - 1];
        var b = keys[next];
        // The ease into the next key: its own, else the clip's
        var ease = b.Ease ?? OffsetEase;
        if (ease == BoneOffsetEase.Hold)
            return new BoneOffsetKey(-1, a.Rotation, a.Translation);

        var t = (frame - a.Frame) / (b.Frame - a.Frame);
        if (ease == BoneOffsetEase.Smooth)
            t = t * t * (3 - 2 * t);

        var rb = Quaternion.Dot(a.Rotation, b.Rotation) < 0 ? -b.Rotation : b.Rotation;
        return new BoneOffsetKey(-1, Quaternion.Normalize(Quaternion.Slerp(a.Rotation, rb, t)),
            Vector3.Lerp(a.Translation, b.Translation, t));
    }

    /// <summary>A local transform with an offset: rotated in its parent's space, then moved</summary>
    public static HavokTransformData ApplyOffset(HavokTransformData local, BoneOffsetKey offset)
    {
        return new HavokTransformData
        {
            Translation = local.Translation + offset.Translation,
            Rotation = Quaternion.Normalize(offset.Rotation * local.Rotation),
            Scale = local.Scale
        };
    }

    /// <summary>
    ///     A copy with this key (replacing one at the same frame). A key without its own ease takes the ease of the
    ///     key it replaces, so moving a bone at a key keeps how it eases in.
    /// </summary>
    public EditableClip WithKey(int track, BoneOffsetKey key)
    {
        var copy = Clone();
        if (!copy.Offsets.TryGetValue(track, out var keys))
            copy.Offsets[track] = keys = new List<BoneOffsetKey>();

        var replaced = keys.FindIndex(k => k.Frame == key.Frame);
        if (key.Ease == null && replaced >= 0)
            key = key with { Ease = keys[replaced].Ease };

        keys.RemoveAll(k => k.Frame == key.Frame);
        keys.Add(key);
        keys.Sort((a, b) => a.Frame.CompareTo(b.Frame));
        return copy;
    }

    /// <summary>A copy without the key at a frame (and without the track's list once it is empty)</summary>
    public EditableClip WithoutKey(int track, int frame)
    {
        var copy = Clone();
        if (copy.Offsets.TryGetValue(track, out var keys))
        {
            keys.RemoveAll(k => k.Frame == frame);
            if (keys.Count == 0)
                copy.Offsets.Remove(track);
        }

        return copy;
    }

    /// <summary>A copy without any key on a track, or on every track when <paramref name="track" /> is null</summary>
    public EditableClip WithoutOffsets(int? track)
    {
        var copy = Clone();
        if (track is { } t)
            copy.Offsets.Remove(t);
        else
            copy.Offsets.Clear();
        return copy;
    }

    /// <summary>
    ///     A copy whose key at <paramref name="frame" /> puts a bone at a model-space position and / or rotation there
    ///     (null keeps the key's own): the bone's new local transform, from that pose and its parent's (which the key
    ///     doesn't move), as an offset from the clip's own local transform at that frame. The viewport gizmo's edit.
    /// </summary>
    public EditableClip KeyedToModelPose(HavokSkeletonData skeleton, int bone, int track, int frame, Vector3? position,
        Quaternion? rotation)
    {
        var pose = ModelPose(skeleton, frame);
        var parent = skeleton.Parents[bone];
        var hasParent = parent >= 0 && parent < skeleton.BoneCount && parent != bone;
        var parentRotation = hasParent ? pose[parent].Rotation : Quaternion.Identity;
        var parentPosition = hasParent ? pose[parent].Position : Vector3.Zero;
        var parentScale = hasParent ? pose[parent].Scale : Vector3.One;
        var toParent = Quaternion.Inverse(parentRotation);

        Quaternion? localRotation = rotation is { } r ? Quaternion.Normalize(toParent * r) : null;
        Vector3? localTranslation = null;
        if (position is { } p)
        {
            var inParent = Vector3.Transform(p - parentPosition, toParent);
            localTranslation = new Vector3(Divide(inParent.X, parentScale.X), Divide(inParent.Y, parentScale.Y),
                Divide(inParent.Z, parentScale.Z));
        }

        return ActiveKeyFor(track, frame, localRotation, localTranslation) is { } key ? WithKey(track, key) : this;
    }

    /// <summary>
    ///     The active layer's key at a frame that makes a track play this local rotation and / or translation (null
    ///     keeps the key's own), through the layers below it (on the frames) and above it, and undoing its weight.
    ///     Null when the active layer doesn't act on the track (muted, soloed out, masked out, weight 0).
    /// </summary>
    public BoneOffsetKey? ActiveKeyFor(int track, int frame, Quaternion? localRotation, Vector3? localTranslation)
    {
        var active = Math.Clamp(ActiveLayer, 0, Layers.Count - 1);
        var weight = LayerWeight(active, track);
        if (MathF.Abs(weight) < 1e-4f)
            return null;

        // local = above · (active scaled) · below, where below is the frames with the earlier layers on
        var below = ApplyOffset(Sample(track, frame), CompositeOffset(track, frame, 0, active - 1));
        var above = CompositeOffset(track, frame, active + 1);
        var existing = OffsetAt(track, frame);

        var rotationKey = existing.Rotation;
        if (localRotation is { } r)
        {
            var scaled = Quaternion.Normalize(Quaternion.Inverse(above.Rotation) * r * Quaternion.Inverse(below.Rotation));
            rotationKey = OffsetLayer.Scaled(new BoneOffsetKey(frame, scaled, Vector3.Zero), 1 / weight).Rotation;
        }

        var translationKey = existing.Translation;
        if (localTranslation is { } t)
            translationKey = (t - below.Translation - above.Translation) / weight;

        return new BoneOffsetKey(frame, rotationKey, translationKey);
    }

    private static float Divide(float a, float b) => MathF.Abs(b) > 1e-8f ? a / b : a;

    /// <summary>A copy whose keys without their own ease use this one</summary>
    public EditableClip WithEase(BoneOffsetEase ease)
    {
        var copy = Clone();
        copy.OffsetEase = ease;
        return copy;
    }

    /// <summary>A copy with these keys' own ease set (null: back to the clip's); keys that don't exist are passed over</summary>
    public EditableClip WithKeyEase(IEnumerable<(int Track, int Frame)> keys, BoneOffsetEase? ease)
    {
        var copy = Clone();
        foreach (var (track, frame) in keys)
        {
            if (copy.Offsets.TryGetValue(track, out var list) && list.FindIndex(k => k.Frame == frame) is var i and >= 0)
                list[i] = list[i] with { Ease = ease };
        }

        return copy;
    }

    /// <summary>The ease a key goes in with: its own, else the clip's</summary>
    public BoneOffsetEase EaseOf(BoneOffsetKey key) => key.Ease ?? OffsetEase;

    // ---- root motion

    /// <summary>The axis the root turns about (normalized; +Y without root motion)</summary>
    public Vector3 UpAxis
    {
        get
        {
            var up = new Vector3(Up.X, Up.Y, Up.Z);
            return up.LengthSquared() > 1e-8f ? Vector3.Normalize(up) : Vector3.UnitY;
        }
    }

    /// <summary>The root motion sample at a frame as it plays (root keys included), lerped between frames (zero without root motion)</summary>
    public Vector4 RootSampleAt(float frame)
    {
        if (EffectiveRootMotion is not { Length: > 0 } motion)
            return Vector4.Zero;

        frame = Math.Clamp(frame, 0, motion.Length - 1);
        var first = (int)frame;
        var second = Math.Min(first + 1, motion.Length - 1);
        return Vector4.Lerp(motion[first], motion[second], frame - first);
    }

    /// <summary>
    ///     Where the root motion has taken the character by a frame, as HavokRootMotion.Sample places it: turned about
    ///     <see cref="UpAxis" />, then moved. Identity without root motion.
    /// </summary>
    public Matrix4x4 RootMatrix(float frame)
    {
        if (EffectiveRootMotion == null)
            return Matrix4x4.Identity;

        var s = RootSampleAt(frame);
        return Matrix4x4.CreateFromAxisAngle(UpAxis, s.W) * Matrix4x4.CreateTranslation(s.X, s.Y, s.Z);
    }

    // ---- model space

    /// <summary>
    ///     Every bone's model-space position, rotation and scale at a frame of the clip with its offsets (bones the
    ///     clip has no track for at their reference pose)
    /// </summary>
    public (Vector3 Position, Quaternion Rotation, Vector3 Scale)[] ModelPose(HavokSkeletonData skeleton, float frame)
    {
        var clip = Baked();
        var boneToTrack = BoneToTrack(skeleton.BoneCount);
        return ModelSpace(skeleton, b => boneToTrack[b] >= 0 ? clip.Sample(boneToTrack[b], frame) : skeleton.ReferencePose[b]);
    }

    /// <summary>The track of each bone of a skeleton, -1 for a bone the clip doesn't animate</summary>
    public int[] BoneToTrack(int boneCount)
    {
        var boneToTrack = Enumerable.Repeat(-1, boneCount).ToArray();
        for (var t = 0; t < TrackCount; t++)
        {
            var bone = BoneOf(t);
            if (bone >= 0 && bone < boneCount)
                boneToTrack[bone] = t;
        }

        return boneToTrack;
    }

    /// <summary>Model-space transforms from local ones, down the skeleton's parents</summary>
    public static (Vector3 Position, Quaternion Rotation, Vector3 Scale)[] ModelSpace(HavokSkeletonData skeleton,
        Func<int, HavokTransformData> local)
    {
        var result = new (Vector3 Position, Quaternion Rotation, Vector3 Scale)[skeleton.BoneCount];
        var done = new bool[skeleton.BoneCount];

        void Resolve(int b)
        {
            if (done[b]) return;
            var x = local(b);
            var parent = skeleton.Parents[b];
            if (parent < 0 || parent >= skeleton.BoneCount || parent == b)
            {
                result[b] = (x.Translation, Quaternion.Normalize(x.Rotation), x.Scale);
            }
            else
            {
                Resolve(parent);
                var p = result[parent];
                result[b] = (p.Position + Vector3.Transform(x.Translation * p.Scale, p.Rotation),
                    Quaternion.Normalize(p.Rotation * x.Rotation), x.Scale);
            }

            done[b] = true;
        }

        for (var b = 0; b < skeleton.BoneCount; b++)
            Resolve(b);

        return result;
    }

    /// <summary>A copy with other frames (and root motion), keeping the clip's tracks, maps and settings</summary>
    public EditableClip WithFrames(HavokTransformData[][] frames, float[][] floats, Vector4[]? rootMotion)
    {
        var copy = Clone();
        copy.Frames = frames;
        copy.Floats = floats;
        copy.RootMotion = rootMotion;
        return copy;
    }

    public static HavokTransformData Blend(HavokTransformData a, HavokTransformData b, float weight)
    {
        var rb = b.Rotation;
        // Same hemisphere, so the slerp takes the short way
        if (Quaternion.Dot(a.Rotation, rb) < 0)
            rb = -rb;

        return new HavokTransformData
        {
            Translation = Vector3.Lerp(a.Translation, b.Translation, weight),
            Rotation = Quaternion.Normalize(Quaternion.Slerp(a.Rotation, rb, weight)),
            Scale = Vector3.Lerp(a.Scale, b.Scale, weight)
        };
    }

    private static byte[] Unpack(byte[] data) => DCX.Is(data) ? DCX.Decompress(data, out _) : data;
}
