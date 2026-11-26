#nullable enable
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Utilities;

/// <summary>How a target bone found its source bone (Animation Editor Analysis.md §9)</summary>
public enum RetargetMatch
{
    /// <summary>No source bone: the bone stays at its reference pose</summary>
    None = 0,

    /// <summary>The same name (ignoring case)</summary>
    Exact = 1,

    /// <summary>The same name once spaces, underscores and the side marker are put alike (L Thigh = L_Thigh)</summary>
    Name = 2,

    /// <summary>The mirror bone is matched, and the mirror of its source bone is this one's</summary>
    Mirror = 3,

    /// <summary>Picked by hand (kept by Auto Map)</summary>
    Manual = 4
}

/// <summary>Which source bone each target bone follows</summary>
public sealed class RetargetMap
{
    /// <summary>Per target bone: the source bone, -1 for none</summary>
    public int[] SourceOf;

    public RetargetMatch[] Kind;

    public RetargetMap(int targetBones)
    {
        SourceOf = Enumerable.Repeat(-1, targetBones).ToArray();
        Kind = new RetargetMatch[targetBones];
    }

    public int MappedCount => SourceOf.Count(s => s >= 0);

    public int Count(RetargetMatch kind) => Kind.Count(k => k == kind);

    public RetargetMap Clone()
    {
        var copy = new RetargetMap(SourceOf.Length);
        SourceOf.CopyTo(copy.SourceOf, 0);
        Kind.CopyTo(copy.Kind, 0);
        return copy;
    }
}

/// <summary>Settings of one retarget</summary>
public sealed class RetargetSettings
{
    /// <summary>How much the source's moves (translations, root motion) are scaled: the target's size over the source's</summary>
    public float Scale = 1;

    /// <summary>Whether the source's root motion is carried over (scaled); otherwise the clip has none</summary>
    public bool RootMotion = true;

    /// <summary>
    ///     Added to the position of the topmost animated bone (Master) on every frame, in model space (Y up), so the whole
    ///     pose sits higher, lower or further along. For sources that hold Master off where the target needs it.
    /// </summary>
    public Vector3 MasterOffset = Vector3.Zero;

    /// <summary>
    ///     Per clip: also move Master up or down so it is at the target's reference height on the first frame (a clip
    ///     that starts in the air is pulled down to the ground, so it is off by default)
    /// </summary>
    public bool LevelFirstFrame;
}

/// <summary>
///     Moves a clip of one skeleton onto another (Animation Editor Analysis.md §9). Bones are matched by name
///     (<see cref="AutoMap" />), then each matched target bone takes its source bone's model-space turn away from the
///     source's reference pose, on the target's own reference pose: <c>R_t = (R_s · R_s,ref⁻¹) · R_t,ref</c>. That doesn't
///     depend on how either skeleton's bones are axed, and a target with fewer bones in a chain (two spine bones for
///     three) still puts the bones after it where they face in the source. Bones keep the target's own lengths; only
///     the source's moves away from its reference pose (pelvis bob, Master's travel) are carried over, scaled.
/// </summary>
public static class SkeletonRetarget
{
    private static readonly Regex BipPrefix = new(@"^bip0*\d+[ _]?", RegexOptions.IgnoreCase);
    private static readonly Regex SidePrefix = new(@"^(l|r|left|right)[ _\-.](.+)$", RegexOptions.IgnoreCase);
    private static readonly Regex SideSuffix = new(@"^(.+)[ _\-.](l|r|left|right)$", RegexOptions.IgnoreCase);
    private static readonly Regex Separators = new(@"[ _\-.]", RegexOptions.None);

    /// <summary>
    ///     A bone name put alike for matching: lower case, a Bip01 prefix dropped, the side (L_ / _L / Left ) moved to
    ///     the front as "l:" or "r:", spaces, underscores, dashes and dots taken out. "L Thigh", "L_Thigh", "Thigh_L"
    ///     are all "l:thigh"; "Spine1" is ":spine1".
    /// </summary>
    public static string NameKey(string name)
    {
        var s = BipPrefix.Replace(name.Trim(), "");
        var side = "";
        if (SidePrefix.Match(s) is { Success: true } p)
        {
            side = p.Groups[1].Value[..1].ToLowerInvariant();
            s = p.Groups[2].Value;
        }
        else if (SideSuffix.Match(s) is { Success: true } q)
        {
            side = q.Groups[2].Value[..1].ToLowerInvariant();
            s = q.Groups[1].Value;
        }

        return side + ":" + Separators.Replace(s, "").ToLowerInvariant();
    }

    /// <summary>The key of the bone on the other side (l: ↔ r:), the same key for a centre bone</summary>
    private static string MirrorKey(string key) =>
        key.StartsWith("l:") ? "r:" + key[2..] : key.StartsWith("r:") ? "l:" + key[2..] : key;

    /// <summary>
    ///     Matches every target bone it can to a source bone, each source bone used once: hand-picked ones first
    ///     (<paramref name="manual" />: target bone name → source bone name, "" to leave it unmatched), then the same name,
    ///     then the same <see cref="NameKey" />, then by mirror (<see cref="RetargetMatch" />).
    /// </summary>
    public static RetargetMap AutoMap(HavokSkeletonData source, HavokSkeletonData target,
        IReadOnlyDictionary<string, string>? manual = null)
    {
        var map = new RetargetMap(target.BoneCount);
        var used = new bool[source.BoneCount];
        var fixedNone = new bool[target.BoneCount];

        void Set(int t, int s, RetargetMatch kind)
        {
            map.SourceOf[t] = s;
            map.Kind[t] = kind;
            used[s] = true;
        }

        var sourceByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var sourceByKey = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var s = 0; s < source.BoneCount; s++)
        {
            sourceByName.TryAdd(source.Names[s], s);
            sourceByKey.TryAdd(NameKey(source.Names[s]), s);
        }

        if (manual != null)
        {
            for (var t = 0; t < target.BoneCount; t++)
            {
                if (!manual.TryGetValue(target.Names[t], out var from))
                    continue;

                if (from.Length == 0)
                {
                    fixedNone[t] = true;
                    map.Kind[t] = RetargetMatch.Manual;
                }
                else if (sourceByName.TryGetValue(from, out var s))
                {
                    // A hand-picked source bone may be picked for several target bones
                    Set(t, s, RetargetMatch.Manual);
                }
            }
        }

        bool Open(int t) => map.SourceOf[t] < 0 && !fixedNone[t];

        for (var t = 0; t < target.BoneCount; t++)
        {
            if (Open(t) && sourceByName.TryGetValue(target.Names[t], out var s) && !used[s])
                Set(t, s, RetargetMatch.Exact);
        }

        var targetKeys = target.Names.Select(NameKey).ToArray();
        for (var t = 0; t < target.BoneCount; t++)
        {
            if (Open(t) && sourceByKey.TryGetValue(targetKeys[t], out var s) && !used[s])
                Set(t, s, RetargetMatch.Name);
        }

        // Mirror: the bone on the other side is matched (by hand, usually); this one takes the other side of its source
        // bone. No matching by place in the hierarchy: tried against ER (2026-11-26) it paired unrelated bones (Saddle
        // with Spine1_Mantle, L_Scapula with L_ShoulderArmor, cloth bones with mantles), and a wrong match is worse than
        // none.
        var targetByKey = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var t = 0; t < target.BoneCount; t++)
            targetByKey.TryAdd(targetKeys[t], t);

        for (var t = 0; t < target.BoneCount; t++)
        {
            var mirrorKey = MirrorKey(targetKeys[t]);
            if (Open(t) && mirrorKey != targetKeys[t] && targetByKey.TryGetValue(mirrorKey, out var tm) &&
                map.SourceOf[tm] is var sm and >= 0 &&
                sourceByKey.TryGetValue(MirrorKey(NameKey(source.Names[sm])), out var s) && s != sm && !used[s])
            {
                Set(t, s, RetargetMatch.Mirror);
            }
        }

        return map;
    }

    /// <summary>
    ///     The target's size over the source's: the height of the topmost matched bone that stands off the ground in
    ///     both (normally Root or Pelvis), else of the whole skeletons; 1 when neither has a height.
    /// </summary>
    public static float AutoScale(HavokSkeletonData source, HavokSkeletonData target, RetargetMap map)
    {
        var sourceRest = RestModel(source);
        var targetRest = RestModel(target);
        var sourceHeight = sourceRest.Max(p => p.Position.Y);
        var targetHeight = targetRest.Max(p => p.Position.Y);

        var best = -1;
        var bestDepth = int.MaxValue;
        for (var t = 0; t < target.BoneCount; t++)
        {
            var s = map.SourceOf[t];
            if (s < 0 || targetRest[t].Position.Y < 0.05f * targetHeight || sourceRest[s].Position.Y < 0.05f * sourceHeight)
                continue;

            var depth = Depth(target, t);
            if (depth < bestDepth)
            {
                best = t;
                bestDepth = depth;
            }
        }

        if (best >= 0)
            return targetRest[best].Position.Y / sourceRest[map.SourceOf[best]].Position.Y;

        return sourceHeight > 1e-4f && targetHeight > 1e-4f ? targetHeight / sourceHeight : 1;
    }

    private static int Depth(HavokSkeletonData skeleton, int bone)
    {
        var depth = 0;
        for (var b = bone; b >= 0 && b < skeleton.BoneCount && skeleton.Parents[b] != b && depth < 1000; b = skeleton.Parents[b])
            depth++;
        return depth;
    }

    private static (Vector3 Position, Quaternion Rotation, Vector3 Scale)[] RestModel(HavokSkeletonData skeleton) =>
        EditableClip.ModelSpace(skeleton, b => skeleton.ReferencePose[b]);

    /// <summary>
    ///     The target bones whose source bone the template's binding can't carry: matched, but the template has no track
    ///     for them, so they stay at their reference pose in the result
    /// </summary>
    public static List<int> UntrackedMatches(RetargetMap map, EditableClip template, int targetBones)
    {
        var boneToTrack = template.BoneToTrack(targetBones);
        return Enumerable.Range(0, targetBones).Where(t => map.SourceOf[t] >= 0 && boneToTrack[t] < 0).ToList();
    }

    /// <summary>
    ///     The source clip (its keys baked in) moved onto the target skeleton, as a clip with the template's binding
    ///     (its tracks and float tracks, which saving writes through) and the source's frames and frame duration. Target
    ///     bones the map leaves out, and bones the template has no track for, are at their reference pose; float tracks
    ///     at their slots' reference values. Root motion: the source's, moves scaled, with its up and forward; or, when
    ///     it isn't carried over, none (zero samples when the template has root motion, which saving would otherwise
    ///     keep). Additive source clips are refused (their frames are deltas, not poses).
    /// </summary>
    public static EditableClip Retarget(EditableClip sourceClip, HavokSkeletonData source, HavokSkeletonData target,
        RetargetMap map, EditableClip template, RetargetSettings settings)
    {
        if (sourceClip.BlendMode != HavokBlendMode.Normal)
            throw new InvalidOperationException("An additive clip can't be retargeted.");

        var clip = sourceClip.Baked();
        var frameCount = clip.FrameCount;
        var scale = settings.Scale;

        var sourceRest = RestModel(source);
        var targetRest = RestModel(target);
        var sourceBoneToTrack = clip.BoneToTrack(source.BoneCount);
        var targetBoneToTrack = template.BoneToTrack(target.BoneCount);
        var order = ParentFirst(target);

        // Per target bone that follows a source bone: the turn taking a source parent-space move into target parent space
        var moveInto = new Quaternion[target.BoneCount];
        var moveScale = new Vector3[target.BoneCount];
        var follows = new int[target.BoneCount];
        for (var t = 0; t < target.BoneCount; t++)
        {
            var s = targetBoneToTrack[t] >= 0 ? map.SourceOf[t] : -1;
            follows[t] = s;
            if (s < 0)
                continue;

            var sp = Parent(source, s);
            var tp = Parent(target, t);
            var sourceParent = sp >= 0 ? sourceRest[sp].Rotation : Quaternion.Identity;
            var targetParent = tp >= 0 ? targetRest[tp].Rotation : Quaternion.Identity;
            moveInto[t] = Quaternion.Normalize(Quaternion.Inverse(targetParent) * sourceParent);

            // The parents' scales: a source move in model units, then in the target parent's units
            var sourceScale = sp >= 0 ? sourceRest[sp].Scale : Vector3.One;
            var targetScale = tp >= 0 ? targetRest[tp].Scale : Vector3.One;
            moveScale[t] = new Vector3(Divide(sourceScale.X, targetScale.X), Divide(sourceScale.Y, targetScale.Y),
                Divide(sourceScale.Z, targetScale.Z));
        }

        var frames = new HavokTransformData[frameCount][];
        var targetModel = new Quaternion[target.BoneCount];
        for (var f = 0; f < frameCount; f++)
        {
            var sourcePose = clip.ModelPose(source, f);
            var pose = new HavokTransformData[template.TrackCount];
            var local = new HavokTransformData[target.BoneCount];

            foreach (var t in order)
            {
                var rest = target.ReferencePose[t];
                var tp = Parent(target, t);
                var parentModel = tp >= 0 ? targetModel[tp] : Quaternion.Identity;
                var s = follows[t];

                if (s < 0)
                {
                    local[t] = rest;
                }
                else
                {
                    var turn = sourcePose[s].Rotation * Quaternion.Inverse(sourceRest[s].Rotation);
                    var model = Quaternion.Normalize(turn * targetRest[t].Rotation);

                    var sourceLocal = sourceBoneToTrack[s] >= 0 ? clip.Frames[f][sourceBoneToTrack[s]] : source.ReferencePose[s];
                    var move = sourceLocal.Translation - source.ReferencePose[s].Translation;
                    var moved = Vector3.Transform(move, moveInto[t]) * moveScale[t] * scale;

                    local[t] = new HavokTransformData
                    {
                        Translation = rest.Translation + moved,
                        Rotation = Quaternion.Normalize(Quaternion.Inverse(parentModel) * model),
                        Scale = rest.Scale
                    };
                }

                targetModel[t] = Quaternion.Normalize(parentModel * local[t].Rotation);
            }

            for (var track = 0; track < template.TrackCount; track++)
            {
                var bone = template.BoneOf(track);
                pose[track] = bone >= 0 && bone < target.BoneCount
                    ? local[bone]
                    : new HavokTransformData { Rotation = Quaternion.Identity, Scale = Vector3.One };
            }

            frames[f] = pose;
        }

        ApplyMasterOffset(frames, target, targetRest, targetBoneToTrack, settings);

        var floats = new float[frameCount][];
        for (var f = 0; f < frameCount; f++)
        {
            var values = new float[template.FloatTrackCount];
            for (var track = 0; track < values.Length; track++)
            {
                var slot = template.FloatTrackToSlot.Length == 0 ? track : track < template.FloatTrackToSlot.Length ? template.FloatTrackToSlot[track] : -1;
                values[track] = slot >= 0 && slot < target.ReferenceFloats.Length ? target.ReferenceFloats[slot] : 0;
            }

            floats[f] = values;
        }

        Vector4[]? rootMotion = null;
        var sourceRoot = clip.EffectiveRootMotion;
        if (settings.RootMotion && sourceRoot is { Length: > 0 })
        {
            rootMotion = new Vector4[frameCount];
            for (var f = 0; f < frameCount; f++)
            {
                var r = sourceRoot[Math.Min(f, sourceRoot.Length - 1)];
                rootMotion[f] = new Vector4(r.X * scale, r.Y * scale, r.Z * scale, r.W);
            }
        }
        else if (template.RootMotion != null)
        {
            rootMotion = new Vector4[frameCount];
        }

        var result = template.WithoutOffsets(null).WithFrames(frames, floats, rootMotion);
        result.FrameDuration = clip.FrameDuration;
        result.BlendMode = HavokBlendMode.Normal;
        if (sourceRoot != null && settings.RootMotion)
        {
            result.Up = clip.Up;
            result.Forward = clip.Forward;
        }

        return result;
    }

    /// <summary>
    ///     The bones <see cref="RetargetSettings.MasterOffset" /> moves: each bone the template animates whose parents it
    ///     doesn't (Master, in every sampled FromSoftware skeleton). Moving them moves the whole pose.
    /// </summary>
    public static List<int> MasterBones(EditableClip template, HavokSkeletonData target) =>
        TopTracked(target, template.BoneToTrack(target.BoneCount));

    private static List<int> TopTracked(HavokSkeletonData target, int[] boneToTrack)
    {
        var result = new List<int>();
        for (var b = 0; b < target.BoneCount; b++)
        {
            if (boneToTrack[b] < 0)
                continue;

            var top = true;
            var depth = 0;
            for (var p = Parent(target, b); p >= 0 && depth++ < target.BoneCount; p = Parent(target, p))
            {
                if (boneToTrack[p] >= 0)
                {
                    top = false;
                    break;
                }
            }

            if (top)
                result.Add(b);
        }

        return result;
    }

    /// <summary>
    ///     Moves the <see cref="MasterBones" /> by the settings' offset (model space), plus, with
    ///     <see cref="RetargetSettings.LevelFirstFrame" />, the height that puts each at its reference height on the first
    ///     frame. Their parents aren't animated, so they sit at rest, and a model-space move is a fixed parent-space one.
    /// </summary>
    private static void ApplyMasterOffset(HavokTransformData[][] frames, HavokSkeletonData target,
        (Vector3 Position, Quaternion Rotation, Vector3 Scale)[] targetRest, int[] boneToTrack, RetargetSettings settings)
    {
        if (frames.Length == 0 || (settings.MasterOffset == Vector3.Zero && !settings.LevelFirstFrame))
            return;

        foreach (var b in TopTracked(target, boneToTrack))
        {
            var track = boneToTrack[b];
            var tp = Parent(target, b);
            var parentPosition = tp >= 0 ? targetRest[tp].Position : Vector3.Zero;
            var parentRotation = tp >= 0 ? targetRest[tp].Rotation : Quaternion.Identity;
            var parentScale = tp >= 0 ? targetRest[tp].Scale : Vector3.One;

            var offset = settings.MasterOffset;
            if (settings.LevelFirstFrame)
            {
                var first = parentPosition + Vector3.Transform(frames[0][track].Translation * parentScale, parentRotation);
                offset.Y += targetRest[b].Position.Y - first.Y;
            }

            var inParent = Vector3.Transform(offset, Quaternion.Inverse(parentRotation));
            var local = new Vector3(Divide(inParent.X, parentScale.X), Divide(inParent.Y, parentScale.Y),
                Divide(inParent.Z, parentScale.Z));

            foreach (var pose in frames)
                pose[track].Translation += local;
        }
    }

    private static int Parent(HavokSkeletonData skeleton, int bone)
    {
        var p = skeleton.Parents[bone];
        return p >= 0 && p < skeleton.BoneCount && p != bone ? p : -1;
    }

    /// <summary>The bones in an order where each comes after its parent</summary>
    private static List<int> ParentFirst(HavokSkeletonData skeleton)
    {
        var order = new List<int>(skeleton.BoneCount);
        var done = new bool[skeleton.BoneCount];

        void Visit(int b, int depth)
        {
            if (done[b] || depth > skeleton.BoneCount)
                return;
            var p = Parent(skeleton, b);
            if (p >= 0)
                Visit(p, depth + 1);
            if (done[b])
                return;
            done[b] = true;
            order.Add(b);
        }

        for (var b = 0; b < skeleton.BoneCount; b++)
            Visit(b, 0);

        return order;
    }

    private static float Divide(float a, float b) => MathF.Abs(b) > 1e-8f ? a / b : a;
}
