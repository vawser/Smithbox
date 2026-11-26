#nullable enable
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The skeleton drawn over the character: a line from each bone to its parent at the playing frame, a sphere at each
/// joint, the selected bone (and the line to its parent) in its own colour with small axes, all drawn over the mesh.
/// Built from the clip as it is now (offset keys included), so an edit shows before it is saved.
/// <para>
/// The lines and joints are made once per skeleton and moved each pose: every line is one shared unit line (0,0,0) →
/// (0,0,1) stretched from parent to child by its proxy's world matrix, and a world matrix change only updates the
/// instance, so the skeleton follows every frame of playback. The root path, which only changes with the clip, is a
/// line batch, uploaded hidden and shown once built so it doesn't flicker.
/// </para>
/// </summary>
public sealed class AnimationSkeletonOverlay
{
    private static readonly Color BoneColor = Color.FromArgb(255, 80, 200, 255);
    private static readonly Color SelectedColor = Color.FromArgb(255, 255, 200, 40);
    private static readonly Color PathColor = Color.FromArgb(255, 90, 230, 110);
    private static readonly Color GhostColor = Color.FromArgb(255, 210, 120, 255);

    private readonly AnimationEditorView View;

    // Line batches that change with the clip: the root path, and the selected bone's trail (W8)
    private readonly SwappedBatch RootPath = new();
    private readonly SwappedBatch BoneTrail = new();
    private const int OverlapFrames = 2;

    // What the skeleton was last placed at
    private EditableClip? BuiltClip;
    private float BuiltFrame = -1;
    private int BuiltBone = int.MinValue;
    private float BuiltSize = -1;
    private Matrix4x4 BuiltRoot = Matrix4x4.Identity;
    private bool BuiltSkeleton;
    private bool BuiltPath;
    private bool BuiltJoints;
    private EditableClip? BuiltGhost;
    private bool BuiltTravel;

    // Made once per skeleton: a sphere per joint (one shared unit sphere, sized from the skeleton's height), a line per
    // bone, and the extra lines (the selected bone's 3 axes, the root path's 3-line facing arrow), all on shared shapes
    private DbgPrimSolidSphere? JointSphere;
    private DbgPrimLineBatch? UnitLine;
    private readonly List<DebugPrimitiveRenderableProxy> Joints = new();
    private readonly List<DebugPrimitiveRenderableProxy> BoneLines = new();
    private readonly List<DebugPrimitiveRenderableProxy> AxisLines = new();
    private readonly List<DebugPrimitiveRenderableProxy> ArrowLines = new();

    // The comparison ghost (W5): the clip compared with, at the same frame
    private GhostSkeleton? CompareGhost;

    // The onion skin (W6): the playing clip some frames before (first half) and after (second half) the playing one
    private readonly List<GhostSkeleton> OnionBefore = new();
    private readonly List<GhostSkeleton> OnionAfter = new();
    private (bool On, int Count, int Step) BuiltOnion;

    private RenderScene? MadeIn;
    private HavokSkeletonData? MadeFor;
    private float JointRadius;

    /// <summary>The radius the joint spheres are drawn at, before the selected bone's enlargement (for picking)</summary>
    public float DrawnJointRadius => JointRadius * Math.Max(BuiltSize, 0.05f);

    public AnimationSkeletonOverlay(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>Called every frame, after the viewport's update (which sets the pose)</summary>
    public void Update()
    {
        RootPath.Swap();
        BoneTrail.Swap();

        var scene = View.RenderScene;
        var character = View.Bank.Character;
        var clip = View.SelectedClip is { } source ? View.Bank.Get(source) : null;
        var animator = View.ViewportWindow.Animator;

        var showSkeleton = CFG.Current.Interface.Interface_AnimationEditor_Skeleton;
        // The path is shown while the root motion is selected, as its gizmo sits on it
        var showPath = (CFG.Current.Interface.Interface_AnimationEditor_Root_Path || View.RootSelected) &&
                       clip?.EffectiveRootMotion != null;

        if (!(showSkeleton || showPath) || scene == null || character == null || clip == null || animator?.CurrentClip == null)
        {
            if (BuiltClip != null || MadeFor != null)
                Clear();
            return;
        }

        var skeleton = character.Skeleton;
        EnsureSkeleton(scene, skeleton);

        var joints = showSkeleton && CFG.Current.Interface.Interface_AnimationEditor_Skeleton_Joints;
        var jointSize = Math.Clamp(CFG.Current.Interface.Interface_AnimationEditor_Joint_Size, 0.05f, 10f);
        var frame = MathF.Min(animator.Frame, clip.FrameCount - 1);
        var root = View.ViewportWindow.RootWorld;

        // Every frame, as a new path waits for the one being shown
        UpdatePath(scene, showPath ? clip : null);

        // The clip compared with, unless it is the playing one itself (an unedited clip against its anibnd)
        var compare = CFG.Current.Interface.Interface_AnimationEditor_Compare;
        var ghost = showSkeleton && compare != AnimationCompareSource.None && View.SelectedClip is { } playing
            ? View.Bank.GetCompare(playing, compare)
            : null;
        if (ghost == clip)
            ghost = null;
        var travel = CFG.Current.Interface.Interface_AnimationEditor_Apply_Root_Motion;

        // Every frame too, as the trail follows a drag the way the path does (the root has its path instead)
        var showTrail = showSkeleton && CFG.Current.Interface.Interface_AnimationEditor_Bone_Trail && !View.RootSelected;
        UpdateTrail(scene, skeleton, showTrail ? clip : null, View.SelectedBone, travel);

        var onion =(showSkeleton && CFG.Current.Interface.Interface_AnimationEditor_Onion_Skin,
            Math.Clamp(CFG.Current.Interface.Interface_AnimationEditor_Onion_Count, 1, MaxOnion),
            Math.Clamp(CFG.Current.Interface.Interface_AnimationEditor_Onion_Step, 1, 100));

        if (clip == BuiltClip && Math.Abs(frame - BuiltFrame) < 1e-4f && View.SelectionVersion == BuiltBone &&
            root == BuiltRoot && showSkeleton == BuiltSkeleton && showPath == BuiltPath && joints == BuiltJoints &&
            jointSize == BuiltSize && ghost == BuiltGhost && travel == BuiltTravel && onion == BuiltOnion)
            return;

        BuiltGhost = ghost;
        BuiltTravel = travel;
        BuiltOnion = onion;

        BuiltClip = clip;
        BuiltFrame = frame;
        BuiltBone = View.SelectionVersion;
        BuiltRoot = root;
        BuiltSkeleton = showSkeleton;
        BuiltPath = showPath;
        BuiltJoints = joints;
        BuiltSize = jointSize;

        PlaceArrow(showPath ? clip : null, frame);
        PlaceCompare(skeleton, ghost, frame, travel, joints);
        PlaceOnion(skeleton, onion.Item1 ? clip : null, frame, travel, onion.Item2, onion.Item3);

        if (!showSkeleton)
        {
            foreach (var line in BoneLines.Concat(AxisLines))
                SetVisible(line, false);
            ShowJoints(false);
            return;
        }

        // In the space the character is drawn in: moved along its root motion with Travel on
        var pose = clip.ModelPose(skeleton, frame);
        for (var b = 0; b < pose.Length; b++)
            pose[b].Position = Vector3.Transform(pose[b].Position, root);

        var selected = View.SelectedBone;
        for (var b = 0; b < BoneLines.Count; b++)
        {
            var parent = b < skeleton.Parents.Length ? skeleton.Parents[b] : -1;
            var valid = b < pose.Length && parent >= 0 && parent < pose.Length && parent != b;
            PlaceLine(BoneLines[b], valid ? pose[parent].Position : default, valid ? pose[b].Position : default,
                View.IsBoneSelected(b) ? SelectedColor : BoneColor, valid);
        }

        // The selected bone's axes
        var hasSelected = selected >= 0 && selected < pose.Length;
        if (hasSelected)
        {
            var rotation = Quaternion.Concatenate(pose[selected].Rotation, Quaternion.CreateFromRotationMatrix(root));
            var world = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(pose[selected].Position);
            const float size = 0.08f;
            PlaceLine(AxisLines[0], world.Translation, Vector3.Transform(new Vector3(size, 0, 0), world), SelectedColor, true);
            PlaceLine(AxisLines[1], world.Translation, Vector3.Transform(new Vector3(0, size, 0), world), SelectedColor, true);
            PlaceLine(AxisLines[2], world.Translation, Vector3.Transform(new Vector3(0, 0, size), world), SelectedColor, true);
        }
        else
        {
            foreach (var line in AxisLines)
                SetVisible(line, false);
        }

        if (joints)
            PlaceJoints(pose);
        ShowJoints(joints);
    }

    // ---- ghost

    /// <summary>
    /// The compared clip's skeleton at the same frame (its last, if it is shorter), so a retime shows as the two
    /// drifting apart. With Travel on it travels along its own root motion, as the character does along the edited
    /// clip's. Hidden without a clip to compare with.
    /// </summary>
    private void PlaceCompare(HavokSkeletonData skeleton, EditableClip? ghost, float frame, bool travel, bool joints)
    {
        if (ghost == null)
        {
            CompareGhost?.Hide();
            return;
        }

        CompareGhost ??= new GhostSkeleton(MadeIn!, skeleton.BoneCount, JointSphere!, UnitLine!, true);
        CompareGhost.Place(skeleton, ghost, MathF.Min(frame, ghost.FrameCount - 1), travel, GhostColor,
            joints ? JointRadius * 0.7f * BuiltSize : 0);
    }

    /// <summary>Ghosts on each side of the playing frame at most (the menu's slider goes this far)</summary>
    public const int MaxOnion = 4;

    /// <summary>
    /// The onion skin: the playing clip (edits included) at every Step frames before and after the playing frame, up to
    /// Count on each side, lines only. Red-orange before and green after, fainter further away, the convention of
    /// animation tools. A ghost whose frame falls outside the clip is hidden (no wrap, so a loop's seam reads as an end).
    /// </summary>
    private void PlaceOnion(HavokSkeletonData skeleton, EditableClip? clip, float frame, bool travel, int count, int step)
    {
        for (var k = 1; k <= MaxOnion; k++)
        {
            PlaceOnionSide(OnionBefore, k, clip, frame - k * step, k <= count, travel, skeleton, OnionBeforeColor);
            PlaceOnionSide(OnionAfter, k, clip, frame + k * step, k <= count, travel, skeleton, OnionAfterColor);
        }
    }

    private static readonly Color OnionBeforeColor = Color.FromArgb(255, 255, 120, 90);
    private static readonly Color OnionAfterColor = Color.FromArgb(255, 110, 235, 140);

    private void PlaceOnionSide(List<GhostSkeleton> side, int k, EditableClip? clip, float at, bool wanted, bool travel,
        HavokSkeletonData skeleton, Color color)
    {
        var shown = wanted && clip != null && at >= 0 && at <= clip.FrameCount - 1;
        if (!shown)
        {
            if (k <= side.Count)
                side[k - 1].Hide();
            return;
        }

        while (side.Count < k)
            side.Add(new GhostSkeleton(MadeIn!, skeleton.BoneCount, JointSphere!, UnitLine!, false));

        // Fainter further from the playing frame
        var fade = 1f - 0.22f * (k - 1);
        var faded = Color.FromArgb(255, (int)(color.R * fade), (int)(color.G * fade), (int)(color.B * fade));
        side[k - 1].Place(skeleton, clip!, at, travel, faded, 0);
    }

    /// <summary>
    /// A skeleton drawn in one colour with proxies of its own (a line per bone, and a joint sphere per bone when asked
    /// for) on the overlay's shared unit line and sphere, so moving it each pose only updates instances
    /// </summary>
    private sealed class GhostSkeleton
    {
        private readonly List<DebugPrimitiveRenderableProxy> Lines = new();
        private readonly List<DebugPrimitiveRenderableProxy> Joints = new();

        public GhostSkeleton(RenderScene scene, int bones, DbgPrimSolidSphere sphere, DbgPrimLineBatch line, bool withJoints)
        {
            for (var b = 0; b < bones; b++)
            {
                Lines.Add(NewLineProxy(scene, line, GhostColor));
                if (withJoints)
                {
                    Joints.Add(new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, sphere)
                    {
                        Pickable = false,
                        BaseColor = GhostColor,
                        HighlightedColor = GhostColor,
                        RenderOverlay = true,
                        Visible = false
                    });
                }
            }
        }

        /// <summary>The clip's pose at a frame, along its own root motion with Travel on; joints at the radius (0: none)</summary>
        public void Place(HavokSkeletonData skeleton, EditableClip clip, float frame, bool travel, Color color, float jointRadius)
        {
            var root = travel && clip.EffectiveRootMotion != null ? clip.RootMatrix(frame) : Matrix4x4.Identity;
            var pose = clip.ModelPose(skeleton, frame);
            for (var b = 0; b < pose.Length; b++)
                pose[b].Position = Vector3.Transform(pose[b].Position, root);

            for (var b = 0; b < Lines.Count; b++)
            {
                var parent = b < skeleton.Parents.Length ? skeleton.Parents[b] : -1;
                var valid = b < pose.Length && parent >= 0 && parent < pose.Length && parent != b;
                PlaceLine(Lines[b], valid ? pose[parent].Position : default, valid ? pose[b].Position : default, color, valid);
            }

            for (var b = 0; b < Joints.Count; b++)
            {
                var shown = jointRadius > 0 && b < pose.Length;
                if (shown)
                {
                    var world = Matrix4x4.CreateScale(jointRadius) * Matrix4x4.CreateTranslation(pose[b].Position);
                    if (Joints[b].World != world)
                        Joints[b].World = world;
                    if (Joints[b].BaseColor != color)
                    {
                        Joints[b].BaseColor = color;
                        Joints[b].HighlightedColor = color;
                    }
                }

                SetVisible(Joints[b], shown);
            }
        }

        public void Hide()
        {
            foreach (var proxy in Lines.Concat(Joints))
                SetVisible(proxy, false);
        }

        public void Dispose()
        {
            foreach (var proxy in Lines.Concat(Joints))
                proxy.Dispose();
            Lines.Clear();
            Joints.Clear();
        }
    }

    // ---- root path

    /// <summary>
    /// The root's path over the clip (a line through every frame's root position, where the character starts at the
    /// origin) and a mark at every second. Built again only when the clip changes.
    /// </summary>
    private void UpdatePath(RenderScene scene, EditableClip? clip)
    {
        RootPath.Set(scene, clip, () => PathLines(clip!), PathColor);
    }

    private static List<(Vector3, Vector3)> PathLines(EditableClip clip)
    {
        var motion = clip.EffectiveRootMotion!;
        Vector3 At(int f) => new(motion[f].X, motion[f].Y, motion[f].Z);

        var lines = new List<(Vector3, Vector3)>();
        for (var f = 1; f < motion.Length; f++)
            lines.Add((At(f - 1), At(f)));

        var up = clip.UpAxis;
        var side = Vector3.Normalize(Vector3.Cross(up, MathF.Abs(up.X) < 0.9f ? Vector3.UnitX : Vector3.UnitZ));
        var perSecond = clip.FrameDuration > 0 ? Math.Max(1, (int)MathF.Round(1 / clip.FrameDuration)) : 30;
        for (var f = 0; f < motion.Length; f += perSecond)
        {
            var p = At(f);
            lines.Add((p - side * 0.05f, p + side * 0.05f));
        }

        return lines;
    }

    // ---- bone trail

    private static readonly Color TrailColor = Color.FromArgb(255, 255, 160, 60);

    /// <summary>
    /// The selected bone's path over the whole clip (W8): a line through its position at every frame, in the space the
    /// character is drawn in (along the root motion with Travel on), and a small cross at each of its keys. Shows the
    /// arc a swing makes and where it pops. Built again only when the clip, the bone or Travel changes; while a gizmo
    /// drag makes a new clip each frame, the trail follows once the batch being shown is in.
    /// </summary>
    private void UpdateTrail(RenderScene scene, HavokSkeletonData skeleton, EditableClip? clip, int bone, bool travel)
    {
        if (clip == null || bone < 0 || bone >= skeleton.BoneCount)
        {
            BoneTrail.Set(scene, null, () => new(), TrailColor);
            return;
        }

        BoneTrail.Set(scene, (clip, bone, travel), () => TrailLines(skeleton, clip, bone, travel), TrailColor);
    }

    private static List<(Vector3, Vector3)> TrailLines(HavokSkeletonData skeleton, EditableClip clip, int bone, bool travel)
    {
        var points = new Vector3[clip.FrameCount];
        for (var f = 0; f < clip.FrameCount; f++)
        {
            var pose = clip.ModelPose(skeleton, f);
            var p = bone < pose.Length ? pose[bone].Position : Vector3.Zero;
            points[f] = travel && clip.EffectiveRootMotion != null ? Vector3.Transform(p, clip.RootMatrix(f)) : p;
        }

        var lines = new List<(Vector3, Vector3)>();
        for (var f = 1; f < points.Length; f++)
            lines.Add((points[f - 1], points[f]));

        // The bone's keys (its track's), as a small cross
        var track = clip.BoneToTrack(skeleton.BoneCount)[bone];
        if (track >= 0 && clip.Offsets.TryGetValue(track, out var keys))
        {
            const float s = 0.015f;
            foreach (var key in keys)
            {
                if (key.Frame < 0 || key.Frame >= points.Length)
                    continue;

                var p = points[key.Frame];
                lines.Add((p - new Vector3(s, 0, 0), p + new Vector3(s, 0, 0)));
                lines.Add((p - new Vector3(0, s, 0), p + new Vector3(0, s, 0)));
                lines.Add((p - new Vector3(0, 0, s), p + new Vector3(0, 0, s)));
            }
        }

        return lines;
    }

    /// <summary>
    /// A line batch that is built again when what it shows changes, uploaded hidden and swapped in once built, so it
    /// doesn't flicker. While one is waiting to be shown a newer one isn't started (a drag makes a new clip every frame;
    /// replacing the batch each time would never show one); the newest is taken once it is in.
    /// </summary>
    private sealed class SwappedBatch
    {
        private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> Shown = new();
        private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)>? Pending;
        private int PendingShownFrames;
        private object? Key;

        /// <summary>Shows the lines of <paramref name="key" /> (null: none), building them only when it changes</summary>
        public void Set(RenderScene scene, object? key, Func<List<(Vector3, Vector3)>> build, Color color)
        {
            if (Equals(key, Key))
                return;

            if (key != null && Pending != null)
                return;

            Key = key;
            if (Pending != null)
                Release(Pending);
            Pending = null;

            if (key == null)
            {
                Release(Shown);
                return;
            }

            var built = new List<(DebugPrimitiveRenderableProxy, Action)>();
            foreach (var chunk in build().Chunk(DbgPrimLineBatch.MaxLines))
            {
                var primitive = new DbgPrimLineBatch(chunk);
                built.Add((NewLineProxy(scene, primitive, color), primitive.Release));
            }

            Pending = built;
            PendingShownFrames = 0;
        }

        /// <summary>Called every frame: shows a built batch, then drops the old one a couple of frames later</summary>
        public void Swap()
        {
            if (Pending == null)
                return;

            if (PendingShownFrames >= OverlapFrames || Pending.Count == 0)
            {
                Release(Shown);
                Shown = Pending;
                Pending = null;
                PendingShownFrames = 0;
            }
            else if (PendingShownFrames > 0 || Pending.All(p => p.Proxy.IsRenderableConstructed))
            {
                foreach (var (proxy, _) in Pending)
                    SetVisible(proxy, true);

                PendingShownFrames++;
            }
        }

        public void Clear()
        {
            Release(Shown);
            if (Pending != null)
                Release(Pending);
            Pending = null;
            PendingShownFrames = 0;
            Key = null;
        }
    }

    /// <summary>
    /// At the playing frame, a short arrow along where the character faces, turned by the root's turn. Characters face
    /// -Z (they travel along -Z), the opposite of the reference frame's stored forward (+Z in every vanilla clip).
    /// </summary>
    private void PlaceArrow(EditableClip? clip, float frame)
    {
        if (clip == null)
        {
            foreach (var line in ArrowLines)
                SetVisible(line, false);
            return;
        }

        var up = clip.UpAxis;
        var root = clip.RootMatrix(frame);
        var here = root.Translation;
        var facing = Vector3.TransformNormal(-Vector3.UnitZ, root) * 0.3f;
        var color = View.RootSelected ? SelectedColor : PathColor;
        PlaceLine(ArrowLines[0], here, here + facing, color, true);
        PlaceLine(ArrowLines[1], here + facing, here + facing * 0.7f + Vector3.Cross(up, facing) * 0.25f, color, true);
        PlaceLine(ArrowLines[2], here + facing, here + facing * 0.7f - Vector3.Cross(up, facing) * 0.25f, color, true);
    }

    // ---- skeleton

    private void EnsureSkeleton(RenderScene scene, HavokSkeletonData skeleton)
    {
        if (MadeFor == skeleton)
            return;

        DisposeSkeleton();
        MadeFor = skeleton;
        JointSphere ??= new DbgPrimSolidSphere(Transform.Default, 1f, BoneColor, 6, 8);
        UnitLine ??= new DbgPrimLineBatch(new[] { (Vector3.Zero, Vector3.UnitZ) });

        // A hundredth of the skeleton's height at rest, so a boss's joints read as a person's do
        var rest = EditableClip.ModelSpace(skeleton, b => skeleton.ReferencePose[b]);
        var min = rest.Length > 0 ? rest.Min(p => p.Position.Y) : 0;
        var max = rest.Length > 0 ? rest.Max(p => p.Position.Y) : 1;
        JointRadius = Math.Clamp((max - min) * 0.01f, 0.004f, 0.1f);

        for (var b = 0; b < skeleton.BoneCount; b++)
        {
            Joints.Add(new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, JointSphere)
            {
                Pickable = false,
                BaseColor = BoneColor,
                HighlightedColor = BoneColor,
                RenderOverlay = true,
                Visible = false
            });

            BoneLines.Add(NewLineProxy(scene, UnitLine, BoneColor));
        }

        // The ghosts are made when first shown, in this scene
        MadeIn = scene;

        for (var i = 0; i < 3; i++)
        {
            AxisLines.Add(NewLineProxy(scene, UnitLine, SelectedColor));
            ArrowLines.Add(NewLineProxy(scene, UnitLine, PathColor));
        }
    }

    private static DebugPrimitiveRenderableProxy NewLineProxy(RenderScene scene, DbgPrimLineBatch primitive, Color color)
    {
        return new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
        {
            Pickable = false,
            BaseColor = color,
            HighlightedColor = color,
            // The bones show through the character's mesh
            RenderOverlay = true,
            Visible = false
        };
    }

    /// <summary>
    /// Stretches a unit line proxy from a to b: its world matrix maps (0,0,1) to b - a and the origin to a, with the other
    /// two axes any unit perpendiculars (so the matrix stays invertible). Hidden when not shown or too short to draw.
    /// </summary>
    private static void PlaceLine(DebugPrimitiveRenderableProxy proxy, Vector3 a, Vector3 b, Color color, bool shown)
    {
        var d = b - a;
        var length = d.Length();
        shown &= length > 1e-6f;

        if (shown)
        {
            var z = d / length;
            var helper = MathF.Abs(z.Y) < 0.99f ? Vector3.UnitY : Vector3.UnitX;
            var x = Vector3.Normalize(Vector3.Cross(helper, z));
            var y = Vector3.Cross(z, x);
            var world = new Matrix4x4(
                x.X, x.Y, x.Z, 0,
                y.X, y.Y, y.Z, 0,
                d.X, d.Y, d.Z, 0,
                a.X, a.Y, a.Z, 1);

            if (proxy.World != world)
                proxy.World = world;

            // Only on a change, as a colour change updates the instance
            if (proxy.BaseColor != color)
            {
                proxy.BaseColor = color;
                proxy.HighlightedColor = color;
            }
        }

        SetVisible(proxy, shown);
    }

    private static void SetVisible(DebugPrimitiveRenderableProxy proxy, bool visible)
    {
        if (proxy.Visible != visible)
            proxy.Visible = visible;
    }

    private void PlaceJoints((Vector3 Position, Quaternion Rotation, Vector3 Scale)[] pose)
    {
        for (var b = 0; b < Joints.Count && b < pose.Length; b++)
        {
            var selected = View.IsBoneSelected(b);
            var radius = (selected ? JointRadius * 1.6f : JointRadius) * BuiltSize;
            var world = Matrix4x4.CreateScale(radius) * Matrix4x4.CreateTranslation(pose[b].Position);
            var proxy = Joints[b];

            if (proxy.World != world)
                proxy.World = world;

            var color = selected ? SelectedColor : BoneColor;
            if (proxy.BaseColor != color)
            {
                proxy.BaseColor = color;
                proxy.HighlightedColor = color;
            }
        }
    }

    private void ShowJoints(bool shown)
    {
        foreach (var proxy in Joints)
            SetVisible(proxy, shown);
    }

    private void DisposeSkeleton()
    {
        foreach (var proxy in Joints.Concat(BoneLines).Concat(AxisLines).Concat(ArrowLines))
            proxy.Dispose();

        CompareGhost?.Dispose();
        CompareGhost = null;
        foreach (var ghost in OnionBefore.Concat(OnionAfter))
            ghost.Dispose();
        OnionBefore.Clear();
        OnionAfter.Clear();
        BuiltOnion = default;

        Joints.Clear();
        BoneLines.Clear();
        AxisLines.Clear();
        ArrowLines.Clear();
        MadeFor = null;
    }

    public void Clear()
    {
        RootPath.Clear();
        BoneTrail.Clear();
        BuiltClip = null;
        BuiltFrame = -1;
        BuiltBone = int.MinValue;
        BuiltGhost = null;
        DisposeSkeleton();
    }

    private static void Release(List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> parts)
    {
        foreach (var (proxy, release) in parts)
        {
            proxy.Dispose();
            release();
        }

        parts.Clear();
    }
}
