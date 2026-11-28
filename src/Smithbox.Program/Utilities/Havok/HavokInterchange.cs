#nullable enable
using StudioCore.Renderer;
using StudioCore.Utilities.Fbx;
using StudioCore.Utilities.Interchange;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities;

/// <summary>
///     Havok skeletons and editable clips as interchange data (<see cref="InterchangeSkeleton" />,
///     <see cref="InterchangeClip" />), in game space. The file writers mirror them into their own space.
///     See Claude/Documentation/Animation Editor Analysis.md §14.3.
/// </summary>
public static class HavokInterchange
{
    public static InterchangeSkeleton Skeleton(HavokSkeletonData skeleton)
    {
        // A reference pose with scale is written as FBX locals that give the viewport's worlds (see FbxLocals)
        var rest = ScaleMatters(skeleton, b => skeleton.ReferencePose[b])
            ? FbxLocals(skeleton, ViewportWorlds(skeleton, b => skeleton.ReferencePose[b]))
            : skeleton.ReferencePose.Select(Transform).ToArray();

        var result = new InterchangeSkeleton { Name = skeleton.Name };
        for (var b = 0; b < skeleton.BoneCount; b++)
        {
            result.Joints.Add(new InterchangeJoint
            {
                Name = skeleton.Names[b],
                Parent = ParentOf(skeleton, b),
                Rest = rest[b],
            });
        }

        return result;
    }

    private static int ParentOf(HavokSkeletonData skeleton, int b)
    {
        var parent = skeleton.Parents[b];
        return parent >= 0 && parent < skeleton.BoneCount && parent != b ? parent : -1;
    }

    // Scale this far from 1 counts as scale: compressed clips hold 1 to about 1e-5, and 1e-4 moves children by under
    // 0.1 mm per metre of bone, below the export's own error
    private const float ScaleTolerance = 1e-4f;

    private static bool IsScaled(HavokTransformData t) =>
        MathF.Abs(t.Scale.X - 1) > ScaleTolerance || MathF.Abs(t.Scale.Y - 1) > ScaleTolerance ||
        MathF.Abs(t.Scale.Z - 1) > ScaleTolerance;

    /// <summary>
    ///     Whether any bone with children is scaled: only then do FBX's matrix inheritance and the viewport's
    ///     composition differ (a leaf's own scale comes out the same in both)
    /// </summary>
    private static bool ScaleMatters(HavokSkeletonData skeleton, Func<int, HavokTransformData> local)
    {
        var parents = new HashSet<int>(Enumerable.Range(0, skeleton.BoneCount).Select(b => ParentOf(skeleton, b)).Where(p => p >= 0));
        return parents.Any(p => IsScaled(local(p)));
    }

    // The smallest scale a world matrix gets, so a bone scaled to 0 (the games hide parts that way) stays invertible
    private const float MinScale = 1e-4f;

    private static Vector3 Clamped(Vector3 s) => new(
        MathF.Abs(s.X) < MinScale ? MathF.CopySign(MinScale, s.X == 0 ? 1 : s.X) : s.X,
        MathF.Abs(s.Y) < MinScale ? MathF.CopySign(MinScale, s.Y == 0 ? 1 : s.Y) : s.Y,
        MathF.Abs(s.Z) < MinScale ? MathF.CopySign(MinScale, s.Z == 0 ? 1 : s.Z) : s.Z);

    /// <summary>
    ///     Every bone's world matrix as the viewport poses it (ModelRig.Apply): rotation and translation down the
    ///     hierarchy without scale, scale multiplied down on its own, the world being Scale(accumulated) · RT. A
    ///     parent's scale doesn't move or shear its children, unlike plain matrix inheritance (FBX's).
    /// </summary>
    public static Matrix4x4[] ViewportWorlds(HavokSkeletonData skeleton, Func<int, HavokTransformData> local)
    {
        var count = skeleton.BoneCount;
        var rt = new Matrix4x4[count];
        var scale = new Vector3[count];
        var world = new Matrix4x4[count];
        foreach (var b in ModelRig.SortParentsFirst(skeleton.Parents.Select(p => (int)p).ToArray()))
        {
            if (b < 0 || b >= count)
                continue;
            var l = local(b);
            var m = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(l.Rotation)) * Matrix4x4.CreateTranslation(l.Translation);
            var s = l.Scale;
            var parent = ParentOf(skeleton, b);
            if (parent >= 0)
            {
                m *= rt[parent];
                s *= scale[parent];
            }

            rt[b] = m;
            scale[b] = s;
            world[b] = Matrix4x4.CreateScale(Clamped(s)) * m;
        }

        return world;
    }

    /// <summary>
    ///     FBX local transforms that give these worlds under FBX's matrix inheritance: world · parent world⁻¹, decomposed.
    ///     Exact for uniform scale (and for no scale, where it equals the Havok locals).
    /// </summary>
    private static InterchangeTransform[] FbxLocals(HavokSkeletonData skeleton, Matrix4x4[] world)
    {
        var result = new InterchangeTransform[skeleton.BoneCount];
        for (var b = 0; b < skeleton.BoneCount; b++)
        {
            var parent = ParentOf(skeleton, b);
            var local = world[b];
            if (parent >= 0 && Matrix4x4.Invert(world[parent], out var inverse))
                local = world[b] * inverse;
            Matrix4x4.Decompose(local, out var s, out var r, out var t);
            result[b] = new InterchangeTransform(t, Quaternion.Normalize(r), s);
        }

        return result;
    }

    /// <summary>
    ///     The inverse of <see cref="ViewportWorlds" /> for one bone: its local transform from its world and its parent's
    ///     (the rotation-translation parts divided, the accumulated scales divided)
    /// </summary>
    private static HavokTransformData ViewportLocal(Matrix4x4 world, Matrix4x4? parentWorld)
    {
        Matrix4x4.Decompose(world, out var s, out var r, out var t);
        var rt = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(r)) * Matrix4x4.CreateTranslation(t);
        if (parentWorld is { } pw)
        {
            Matrix4x4.Decompose(pw, out var ps, out var pr, out var pt);
            var prt = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(pr)) * Matrix4x4.CreateTranslation(pt);
            if (Matrix4x4.Invert(prt, out var inverse))
                rt *= inverse;
            s = new Vector3(ps.X != 0 ? s.X / ps.X : s.X, ps.Y != 0 ? s.Y / ps.Y : s.Y, ps.Z != 0 ? s.Z / ps.Z : s.Z);
        }

        Matrix4x4.Decompose(rt, out _, out var lr, out var lt);
        return new HavokTransformData { Translation = lt, Rotation = Quaternion.Normalize(lr), Scale = s };
    }

    /// <summary>
    ///     A clip as it plays (keys on every layer baked in) on the skeleton it is bound to. An additive clip is put on
    ///     the reference pose the way the viewport plays it, so the file shows the motion; <paramref name="notes" /> says so.
    ///     Float tracks aren't carried yet (F4).
    /// </summary>
    public static InterchangeClip Clip(EditableClip clip, HavokSkeletonData skeleton, string name, List<string> notes)
    {
        var baked = clip.Baked();
        var boneToTrack = baked.BoneToTrack(skeleton.BoneCount);
        var frames = baked.FrameCount;

        var result = new InterchangeClip
        {
            Name = name,
            FrameRate = baked.FrameDuration > 0 ? 1.0 / baked.FrameDuration : 30,
            FrameCount = frames,
            Tracks = new InterchangeTransform[]?[skeleton.BoneCount],
        };

        HavokTransformData LocalAt(int b, int f) => boneToTrack[b] >= 0
            ? OnReference(baked.Frames[f][boneToTrack[b]], skeleton.ReferencePose[b], baked.BlendMode)
            : skeleton.ReferencePose[b];

        // With scale on any bone, FBX's matrix inheritance would let a scaled bone move and shear its children, which
        // the viewport doesn't: every bone is then written from its viewport world (untracked ones too, as their
        // FBX locals change with their parents' scale)
        var scaled = Enumerable.Range(0, frames).Any(f => ScaleMatters(skeleton, b => LocalAt(b, f)));
        if (scaled)
        {
            for (var b = 0; b < skeleton.BoneCount; b++)
                result.Tracks[b] = new InterchangeTransform[frames];
            for (var f = 0; f < frames; f++)
            {
                var frame = f;
                var locals = FbxLocals(skeleton, ViewportWorlds(skeleton, b => LocalAt(b, frame)));
                for (var b = 0; b < skeleton.BoneCount; b++)
                    result.Tracks[b]![f] = locals[b];
            }

            notes.Add("bone scale written as the viewport shows it");
        }
        else
        {
            for (var b = 0; b < skeleton.BoneCount; b++)
            {
                if (boneToTrack[b] < 0)
                    continue;
                var samples = new InterchangeTransform[frames];
                for (var f = 0; f < frames; f++)
                    samples[f] = Transform(LocalAt(b, f));
                result.Tracks[b] = samples;
            }
        }

        if (baked.BlendMode != HavokBlendMode.Normal)
            notes.Add($"additive ({baked.BlendMode}): exported on the reference pose");
        // Float tracks by slot name (the slot's index when the skeleton names none)
        for (var k = 0; k < baked.FloatTrackCount; k++)
            result.Floats[FloatSlotName(baked, skeleton, k)] = Enumerable.Range(0, frames).Select(f => baked.Floats[f][k]).ToArray();

        if (baked.RootMotion is { Length: > 0 } motion)
        {
            var up = baked.UpAxis;
            result.RootMotion = new InterchangeTransform[frames];
            for (var f = 0; f < frames; f++)
            {
                var s = motion[Math.Min(f, motion.Length - 1)];
                result.RootMotion[f] = new InterchangeTransform(new Vector3(s.X, s.Y, s.Z),
                    Quaternion.CreateFromAxisAngle(up, s.W), Vector3.One);
            }
        }

        result.Comment = string.Join("; ",
            $"Smithbox clip={name}",
            $"skeleton={skeleton.Name}",
            $"blend={baked.BlendMode}",
            $"fps={result.FrameRate.ToString("0.###", CultureInfo.InvariantCulture)}",
            $"tracks={baked.TrackCount}");
        return result;
    }

    /// <summary>What an import matched and changed, for the tool's report</summary>
    public sealed class ImportReport
    {
        public int Tracks;
        public int Matched;
        public int MatchedStill;
        public List<string> Unmatched = new();
        public int FileJointsUnused;
        public int Frames;
        public bool RootMotionFromFile;
        public string? RootMotionFromBone;
        public int FloatTracksFromFile;
        public List<string> Notes = new();
    }

    /// <summary>
    ///     A clip made from an imported take on the clip being replaced (or copied for a new ID): the template's binding,
    ///     frame rate, blend mode and float tracks, frames from the file's poses (matched by bone name, in model space,
    ///     so the file's rest pose and bone orientations don't matter), and no offset keys (they're baked into the
    ///     template first, D17). Bones the file doesn't have follow <paramref name="missing" />.
    /// </summary>
    public static EditableClip FromImport(EditableClip template, HavokSkeletonData skeleton, FbxImportedTake take,
        ImportRootMotion rootMotion, ImportMissingBones missing, ImportReport report, string? rootBone = null)
    {
        var baseClip = template.Baked();
        var frames = take.FrameCount;
        var boneCount = skeleton.BoneCount;
        var boneToTrack = baseClip.BoneToTrack(boneCount);

        // The file's poses by bone
        var fromFile = new Matrix4x4[]?[boneCount];
        for (var b = 0; b < boneCount; b++)
            fromFile[b] = take.WorldOf(skeleton.Names[b]);

        report.Tracks = baseClip.TrackCount;
        report.Frames = frames;
        for (var t = 0; t < baseClip.TrackCount; t++)
        {
            var b = baseClip.BoneOf(t);
            if (b < 0 || b >= boneCount)
                continue;
            if (fromFile[b] != null)
            {
                report.Matched++;
                if (!take.IsAnimated(skeleton.Names[b]))
                    report.MatchedStill++;
            }
            else
            {
                report.Unmatched.Add(skeleton.Names[b]);
            }
        }

        report.FileJointsUnused = take.JointNames.Count(n =>
            !FbxAnimationImport.IsLeafBone(n) && Array.IndexOf(skeleton.Names, n) < 0);

        var result = baseClip.Clone();
        result.Layers = new List<OffsetLayer> { new() { Name = baseClip.Layers[0].Name } };
        result.ActiveLayer = 0;
        result.Frames = new HavokTransformData[frames][];
        result.Floats = new float[frames][];

        var previous = new Quaternion[baseClip.TrackCount];
        var world = new Matrix4x4[boneCount];
        var done = new bool[boneCount];
        for (var f = 0; f < frames; f++)
        {
            var tf = Math.Min(f, Math.Max(baseClip.FrameCount - 1, 0));
            Array.Clear(done);

            // Model space: the file's pose for matched bones, else the clip's (or the rest pose) down from the parent
            Matrix4x4 Resolve(int b)
            {
                if (done[b])
                    return world[b];
                done[b] = true;

                if (fromFile[b] is { } file)
                    return world[b] = file[f];

                var track = boneToTrack[b];
                var local = missing == ImportMissingBones.KeepClip && track >= 0 && baseClip.FrameCount > 0
                    ? OnReference(baseClip.Frames[tf][track], skeleton.ReferencePose[b], baseClip.BlendMode)
                    : skeleton.ReferencePose[b];

                // Down from the parent as the viewport poses bones (ViewportWorlds): rotation-translation chained,
                // scale accumulated apart
                var rt = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(local.Rotation)) * Matrix4x4.CreateTranslation(local.Translation);
                var s = local.Scale;
                var parent = ParentOf(skeleton, b);
                if (parent >= 0)
                {
                    Matrix4x4.Decompose(Resolve(parent), out var ps, out var pr, out var pt);
                    rt *= Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(pr)) * Matrix4x4.CreateTranslation(pt);
                    s *= ps;
                }

                return world[b] = Matrix4x4.CreateScale(Clamped(s)) * rt;
            }

            for (var b = 0; b < boneCount; b++)
                Resolve(b);

            var row = new HavokTransformData[baseClip.TrackCount];
            for (var t = 0; t < baseClip.TrackCount; t++)
            {
                var b = baseClip.BoneOf(t);
                if (b < 0 || b >= boneCount)
                {
                    row[t] = baseClip.FrameCount > 0 ? baseClip.Frames[tf][t] : default;
                    continue;
                }

                // The local transform that gives this world as the viewport poses bones (scale divided apart)
                var parent = ParentOf(skeleton, b);
                var pose = ViewportLocal(world[b], parent >= 0 ? world[parent] : null);

                // Same hemisphere as the previous frame, so frames interpolate the short way
                if (f > 0 && Quaternion.Dot(pose.Rotation, previous[t]) < 0)
                    pose.Rotation = -pose.Rotation;
                previous[t] = pose.Rotation;

                row[t] = FromReference(pose, skeleton.ReferencePose[b], baseClip.BlendMode);
            }

            result.Frames[f] = row;
            result.Floats[f] = baseClip.FrameCount > 0 && baseClip.FloatTrackCount > 0
                ? baseClip.Floats[tf].ToArray()
                : new float[baseClip.FloatTrackCount];
        }

        // Root motion
        switch (rootMotion)
        {
            case ImportRootMotion.FromNode when take.AnimatedRootMotion != null:
                result.RootMotion = RootMotionSamples(take.AnimatedRootMotion, baseClip.UpAxis);
                report.RootMotionFromFile = true;
                break;
            case ImportRootMotion.FromNode:
            case ImportRootMotion.Keep:
                result.RootMotion = baseClip.RootMotion is { Length: > 0 } motion
                    ? Enumerable.Range(0, frames).Select(f => motion[Math.Min(f, motion.Length - 1)]).ToArray()
                    : null;
                if (rootMotion == ImportRootMotion.FromNode && result.RootMotion != null)
                    report.Notes.Add("the file has no root motion node: the clip's root motion kept");
                break;
            case ImportRootMotion.FromBone:
                result = WithRootFromBone(result, skeleton, rootBone, baseClip.RootMotion != null, report);
                break;
            default:
                // Zero samples rather than none when the clip had root motion: Export keeps the anibnd's root motion
                // for a clip without any (as Retarget does)
                result.RootMotion = baseClip.RootMotion != null ? new Vector4[frames] : null;
                break;
        }

        // Float tracks from the file where it has them by slot name, else the clip's
        var floatsFromFile = 0;
        for (var k = 0; k < baseClip.FloatTrackCount; k++)
        {
            if (!take.Floats.TryGetValue(FloatSlotName(baseClip, skeleton, k), out var values) || values.Length == 0)
                continue;
            floatsFromFile++;
            for (var f = 0; f < frames; f++)
                result.Floats[f][k] = values[Math.Min(f, values.Length - 1)];
        }

        report.FloatTracksFromFile = floatsFromFile;
        if (baseClip.BlendMode != HavokBlendMode.Normal)
            report.Notes.Add($"written back as additive ({baseClip.BlendMode}) against the reference pose");
        if (frames != baseClip.FrameCount && baseClip.FloatTrackCount > floatsFromFile)
            report.Notes.Add("float tracks not in the file kept from the clip (held at its last frame)");

        return result;
    }

    /// <summary>
    ///     Root motion taken out of a bone of a newly made clip (EditableClipOperations.AddRootMotionFromBone: along the
    ///     ground and the turn, as the Root Motion tool's defaults). The bone must be one root motion can come from
    ///     (animated, with no animated parents); null or another: the first such bone (Master).
    /// </summary>
    private static EditableClip WithRootFromBone(EditableClip clip, HavokSkeletonData skeleton, string? boneName,
        bool hadRootMotion, ImportReport report)
    {
        clip.RootMotion = null;
        var sources = EditableClipOperations.RootMotionSourceBones(clip, skeleton);
        var bone = boneName != null ? Array.IndexOf(skeleton.Names, boneName) : -1;
        if (!sources.Contains(bone))
        {
            if (boneName != null && sources.Count > 0)
                report.Notes.Add($"root motion can't come from {boneName}: {skeleton.Names[sources[0]]} used");
            bone = sources.Count > 0 ? sources[0] : -1;
        }

        if (bone < 0)
        {
            report.Notes.Add("no bone root motion can come from: none added");
            clip.RootMotion = hadRootMotion ? new Vector4[clip.FrameCount] : null;
            return clip;
        }

        report.RootMotionFromBone = skeleton.Names[bone];
        return EditableClipOperations.AddRootMotionFromBone(clip, skeleton, bone, false, true);
    }

    /// <summary>
    ///     Root motion samples from the RootMotion node's matrices: where it is, and how far it has turned about
    ///     <paramref name="up" /> (radians, unwrapped so turns add up past a half turn)
    /// </summary>
    public static Vector4[] RootMotionSamples(Matrix4x4[] node, Vector3 up)
    {
        var samples = new Vector4[node.Length];
        float turn = 0, lastTwist = 0;
        for (var f = 0; f < node.Length; f++)
        {
            Matrix4x4.Decompose(node[f], out _, out var r, out var p);
            var twist = EditableClip.TwistAngle(r, up);
            if (f == 0)
            {
                turn = twist;
            }
            else
            {
                var d = twist - lastTwist;
                if (d > MathF.PI) d -= 2 * MathF.PI;
                else if (d < -MathF.PI) d += 2 * MathF.PI;
                turn += d;
            }

            lastTwist = twist;
            samples[f] = new Vector4(p, turn);
        }

        return samples;
    }

    /// <summary>The name a float track is written under: its slot's name, or "float{slot}"</summary>
    public static string FloatSlotName(EditableClip clip, HavokSkeletonData skeleton, int track)
    {
        var slot = clip.FloatTrackToSlot.Length == 0 ? track : track < clip.FloatTrackToSlot.Length ? clip.FloatTrackToSlot[track] : -1;
        return slot >= 0 && slot < skeleton.FloatSlotCount && skeleton.FloatSlots[slot].Length > 0
            ? skeleton.FloatSlots[slot]
            : $"float{Math.Max(slot, track)}";
    }

    /// <summary>A matrix without its scale: rotation and translation only</summary>
    private static Matrix4x4 Rigid(Matrix4x4 m)
    {
        Matrix4x4.Decompose(m, out _, out var r, out var t);
        return Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(r)) * Matrix4x4.CreateTranslation(t);
    }

    private static HavokTransformData Local(Matrix4x4 world, Matrix4x4? parentWorld)
    {
        var local = world;
        if (parentWorld is { } p && Matrix4x4.Invert(p, out var inverse))
            local = world * inverse;
        Matrix4x4.Decompose(local, out _, out var r, out var t);
        return new HavokTransformData { Translation = t, Rotation = Quaternion.Normalize(r), Scale = Vector3.One };
    }

    /// <summary>
    ///     The file's own skeleton and its take as a Havok skeleton and clip (one track per joint, at the take's frame
    ///     rate), for <see cref="SkeletonRetarget" />. Scale is dropped (joints in model space keep position and rotation
    ///     only), so a scaled armature node (centimetre rigs) doesn't scale the bones. Null when the file has no joints.
    /// </summary>
    public static (HavokSkeletonData Skeleton, EditableClip Clip)? SourceFromImport(FbxImportedTake take)
    {
        var joints = take.Joints;
        if (joints.Count == 0)
            return null;

        var index = joints.Select((n, i) => (n, i)).ToDictionary(p => p.n, p => p.i, StringComparer.Ordinal);
        var parents = joints.Select(n => take.JointParent[n] is { } p ? (short)index[p] : (short)-1).ToArray();
        var rest = joints.Select(n => Rigid(take.Rest[n])).ToArray();

        var skeleton = new HavokSkeletonData
        {
            Name = "FBX",
            Names = joints.ToArray(),
            Parents = parents,
            ReferencePose = joints.Select((_, j) => Local(rest[j], parents[j] >= 0 ? rest[parents[j]] : null)).ToArray(),
        };

        var frames = take.FrameCount;
        var clip = new EditableClip
        {
            TrackCount = joints.Count,
            FloatTrackCount = 0,
            FrameDuration = (float)(1 / take.FrameRate),
            BlendMode = HavokBlendMode.Normal,
            Frames = new HavokTransformData[frames][],
            Floats = Enumerable.Range(0, frames).Select(_ => Array.Empty<float>()).ToArray(),
        };

        var previous = new Quaternion[joints.Count];
        for (var f = 0; f < frames; f++)
        {
            var world = joints.Select(n => Rigid(take.World[n][f])).ToArray();
            var row = new HavokTransformData[joints.Count];
            for (var j = 0; j < joints.Count; j++)
            {
                row[j] = Local(world[j], parents[j] >= 0 ? world[parents[j]] : null);
                if (f > 0 && Quaternion.Dot(row[j].Rotation, previous[j]) < 0)
                    row[j].Rotation = -row[j].Rotation;
                previous[j] = row[j].Rotation;
            }

            clip.Frames[f] = row;
        }

        if (take.AnimatedRootMotion != null)
            clip.RootMotion = RootMotionSamples(take.AnimatedRootMotion, Vector3.UnitY);
        return (skeleton, clip);
    }

    /// <summary>
    ///     A take of another skeleton's file moved onto the character (F3): <see cref="SkeletonRetarget.Retarget" /> of
    ///     the file's skeleton and clip onto the template's binding, then the import's root motion choice. The result
    ///     is a normal clip (Retarget's), at the template's frame rate when the take was sampled at it.
    /// </summary>
    public static EditableClip RetargetImport(EditableClip template, HavokSkeletonData target,
        HavokSkeletonData source, EditableClip sourceClip, RetargetMap map, RetargetSettings settings,
        ImportRootMotion rootMotion, ImportReport report, string? rootBone = null)
    {
        var baseClip = template.Baked();
        settings.RootMotion = rootMotion == ImportRootMotion.FromNode && sourceClip.RootMotion != null;
        var result = SkeletonRetarget.Retarget(sourceClip, source, target, map, baseClip, settings);
        var frames = result.FrameCount;

        if (rootMotion == ImportRootMotion.Keep && baseClip.RootMotion is { Length: > 0 } motion)
            result.RootMotion = Enumerable.Range(0, frames).Select(f => motion[Math.Min(f, motion.Length - 1)]).ToArray();
        else if (rootMotion == ImportRootMotion.FromNode && sourceClip.RootMotion == null && baseClip.RootMotion is { Length: > 0 } kept)
        {
            result.RootMotion = Enumerable.Range(0, frames).Select(f => kept[Math.Min(f, kept.Length - 1)]).ToArray();
            report.Notes.Add("the file has no root motion node: the clip's root motion kept");
        }
        else if (rootMotion == ImportRootMotion.FromBone)
        {
            result = WithRootFromBone(result, target, rootBone, baseClip.RootMotion != null, report);
        }

        report.RootMotionFromFile = settings.RootMotion;
        report.Tracks = baseClip.TrackCount;
        report.Frames = frames;
        var boneToTrack = baseClip.BoneToTrack(target.BoneCount);
        for (var b = 0; b < target.BoneCount; b++)
        {
            if (boneToTrack[b] < 0)
                continue;
            if (map.SourceOf[b] >= 0)
                report.Matched++;
            else
                report.Unmatched.Add(target.Names[b]);
        }

        report.FileJointsUnused = source.BoneCount - map.SourceOf.Where(s => s >= 0).Distinct().Count();
        if (baseClip.BlendMode != HavokBlendMode.Normal)
            report.Notes.Add($"the clip was additive ({baseClip.BlendMode}); the retargeted clip is a normal one");
        return result;
    }

    /// <summary>The inverse of <see cref="OnReference" />: an absolute local pose as the clip's blend mode stores it</summary>
    private static HavokTransformData FromReference(HavokTransformData pose, HavokTransformData reference,
        HavokBlendMode mode)
    {
        static Vector3 Divide(Vector3 a, Vector3 b) => new(b.X != 0 ? a.X / b.X : a.X, b.Y != 0 ? a.Y / b.Y : a.Y,
            b.Z != 0 ? a.Z / b.Z : a.Z);

        var referenceInverse = Quaternion.Inverse(Quaternion.Normalize(reference.Rotation));
        return mode switch
        {
            HavokBlendMode.AdditiveChildSpace => new HavokTransformData
            {
                Translation = pose.Translation - reference.Translation,
                Rotation = Quaternion.Normalize(referenceInverse * pose.Rotation),
                Scale = Divide(pose.Scale, reference.Scale),
            },
            HavokBlendMode.AdditiveParentSpace => new HavokTransformData
            {
                Translation = pose.Translation - reference.Translation,
                Rotation = Quaternion.Normalize(pose.Rotation * referenceInverse),
                Scale = Divide(pose.Scale, reference.Scale),
            },
            _ => pose,
        };
    }

    /// <summary>A sampled local transform as the viewport plays it (ModelRig): additive clips on the reference pose.</summary>
    private static HavokTransformData OnReference(HavokTransformData sampled, HavokTransformData reference,
        HavokBlendMode mode) => mode switch
    {
        HavokBlendMode.AdditiveChildSpace => new HavokTransformData
        {
            Translation = reference.Translation + sampled.Translation,
            Rotation = reference.Rotation * sampled.Rotation,
            Scale = reference.Scale * sampled.Scale,
        },
        HavokBlendMode.AdditiveParentSpace => new HavokTransformData
        {
            Translation = reference.Translation + sampled.Translation,
            Rotation = sampled.Rotation * reference.Rotation,
            Scale = reference.Scale * sampled.Scale,
        },
        _ => sampled,
    };

    private static InterchangeTransform Transform(HavokTransformData t) =>
        new(t.Translation, Quaternion.Normalize(t.Rotation), t.Scale);
}
