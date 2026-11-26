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

    private readonly AnimationEditorView View;

    // The root path's batch: the one drawn and the one being built
    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> Shown = new();
    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)>? Pending;
    private int PendingShownFrames;
    private const int OverlapFrames = 2;
    private EditableClip? PathClip;

    // What the skeleton was last placed at
    private EditableClip? BuiltClip;
    private float BuiltFrame = -1;
    private int BuiltBone = -2;
    private float BuiltSize = -1;
    private Matrix4x4 BuiltRoot = Matrix4x4.Identity;
    private bool BuiltSkeleton;
    private bool BuiltPath;
    private bool BuiltJoints;

    // Made once per skeleton: a sphere per joint (one shared unit sphere, sized from the skeleton's height), a line per
    // bone, and the extra lines (the selected bone's 3 axes, the root path's 3-line facing arrow), all on shared shapes
    private DbgPrimSolidSphere? JointSphere;
    private DbgPrimLineBatch? UnitLine;
    private readonly List<DebugPrimitiveRenderableProxy> Joints = new();
    private readonly List<DebugPrimitiveRenderableProxy> BoneLines = new();
    private readonly List<DebugPrimitiveRenderableProxy> AxisLines = new();
    private readonly List<DebugPrimitiveRenderableProxy> ArrowLines = new();
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
        SwapPending();

        var scene = View.RenderScene;
        var character = View.Bank.Character;
        var clip = View.SelectedClip is { } source ? View.Bank.Get(source) : null;
        var animator = View.ViewportWindow.Animator;

        var showSkeleton = CFG.Current.Interface.Interface_AnimationEditor_Skeleton;
        var showPath = CFG.Current.Interface.Interface_AnimationEditor_Root_Path && clip?.RootMotion != null;

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

        if (clip == BuiltClip && Math.Abs(frame - BuiltFrame) < 1e-4f && View.SelectedBone == BuiltBone &&
            root == BuiltRoot && showSkeleton == BuiltSkeleton && showPath == BuiltPath && joints == BuiltJoints &&
            jointSize == BuiltSize)
            return;

        BuiltClip = clip;
        BuiltFrame = frame;
        BuiltBone = View.SelectedBone;
        BuiltRoot = root;
        BuiltSkeleton = showSkeleton;
        BuiltPath = showPath;
        BuiltJoints = joints;
        BuiltSize = jointSize;

        UpdatePath(scene, showPath ? clip : null);
        PlaceArrow(showPath ? clip : null, frame);

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
                b == selected ? SelectedColor : BoneColor, valid);
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

    // ---- root path

    /// <summary>
    /// The root's path over the clip (a line through every frame's root position, where the character starts at the
    /// origin) and a mark at every second. Built again only when the clip changes.
    /// </summary>
    private void UpdatePath(RenderScene scene, EditableClip? clip)
    {
        if (clip == PathClip)
            return;

        PathClip = clip;
        if (Pending != null)
            Release(Pending);
        Pending = null;

        if (clip == null)
        {
            Release(Shown);
            return;
        }

        var motion = clip.RootMotion!;
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

        var built = new List<(DebugPrimitiveRenderableProxy, Action)>();
        foreach (var chunk in lines.Chunk(DbgPrimLineBatch.MaxLines))
        {
            var primitive = new DbgPrimLineBatch(chunk);
            built.Add((NewLineProxy(scene, primitive, PathColor), primitive.Release));
        }

        Pending = built;
        PendingShownFrames = 0;
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
        PlaceLine(ArrowLines[0], here, here + facing, PathColor, true);
        PlaceLine(ArrowLines[1], here + facing, here + facing * 0.7f + Vector3.Cross(up, facing) * 0.25f, PathColor, true);
        PlaceLine(ArrowLines[2], here + facing, here + facing * 0.7f - Vector3.Cross(up, facing) * 0.25f, PathColor, true);
    }

    private void SwapPending()
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
            var selected = b == View.SelectedBone;
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

        Joints.Clear();
        BoneLines.Clear();
        AxisLines.Clear();
        ArrowLines.Clear();
        MadeFor = null;
    }

    public void Clear()
    {
        Release(Shown);
        if (Pending != null)
            Release(Pending);
        Pending = null;
        PendingShownFrames = 0;
        PathClip = null;
        BuiltClip = null;
        BuiltFrame = -1;
        BuiltBone = -2;
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
