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
/// The skeleton drawn over the character: a line from each bone to its parent at the playing frame, the selected bone
/// (and the line to its parent) in its own colour with small axes, all drawn over the mesh. Built from the clip as it
/// is now (offset keys included), so an edit shows before it is saved. A new set of lines is uploaded hidden and only
/// replaces the drawn one once it is built, as the Cutscene Editor's lines are, so a playing pose doesn't flicker.
/// </summary>
public sealed class AnimationSkeletonOverlay
{
    private static readonly Color BoneColor = Color.FromArgb(255, 80, 200, 255);
    private static readonly Color SelectedColor = Color.FromArgb(255, 255, 200, 40);

    private readonly AnimationEditorView View;

    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)> Shown = new();
    private List<(DebugPrimitiveRenderableProxy Proxy, Action Release)>? Pending;
    private int PendingShownFrames;
    private const int OverlapFrames = 2;

    // What the drawn lines were made from
    private EditableClip? BuiltClip;
    private float BuiltFrame = -1;
    private int BuiltBone = -2;

    // A small sphere at each bone: one shared unit sphere, a proxy per bone made once per skeleton and moved each
    // pose (a world matrix change only updates the instance), sized from the skeleton's height
    private DbgPrimSolidSphere? JointSphere;
    private readonly List<DebugPrimitiveRenderableProxy> Joints = new();
    private HavokSkeletonData? JointsFor;
    private float JointRadius;
    private bool JointsShown;
    private float BuiltSize = -1;

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

        if (!CFG.Current.Interface.Interface_AnimationEditor_Skeleton || scene == null || character == null ||
            clip == null || animator?.CurrentClip == null)
        {
            if (BuiltClip != null)
                Clear();
            ShowJoints(false);
            return;
        }

        var joints = CFG.Current.Interface.Interface_AnimationEditor_Skeleton_Joints;
        if (joints)
            EnsureJoints(scene, character.Skeleton);
        if (joints != JointsShown)
        {
            ShowJoints(joints);
            BuiltClip = null;
        }

        // A new size places the joints again at once
        var jointSize = Math.Clamp(CFG.Current.Interface.Interface_AnimationEditor_Joint_Size, 0.05f, 10f);
        if (jointSize != BuiltSize)
        {
            BuiltSize = jointSize;
            BuiltClip = null;
        }

        var frame = MathF.Min(animator.Frame, clip.FrameCount - 1);
        if (clip == BuiltClip && Math.Abs(frame - BuiltFrame) < 1e-4f && View.SelectedBone == BuiltBone)
            return;

        BuiltClip = clip;
        BuiltFrame = frame;
        BuiltBone = View.SelectedBone;

        var skeleton = character.Skeleton;
        var pose = clip.ModelPose(skeleton, frame);
        var bones = new List<(Vector3, Vector3)>();
        var selected = new List<(Vector3, Vector3)>();

        for (var b = 0; b < skeleton.BoneCount; b++)
        {
            var parent = skeleton.Parents[b];
            if (parent < 0 || parent >= skeleton.BoneCount || parent == b)
                continue;

            (b == View.SelectedBone ? selected : bones).Add((pose[parent].Position, pose[b].Position));
        }

        if (View.SelectedBone is var sel && sel >= 0 && sel < skeleton.BoneCount)
        {
            var world = Matrix4x4.CreateFromQuaternion(pose[sel].Rotation) * Matrix4x4.CreateTranslation(pose[sel].Position);
            var o = world.Translation;
            const float size = 0.08f;
            selected.Add((o, Vector3.Transform(new Vector3(size, 0, 0), world)));
            selected.Add((o, Vector3.Transform(new Vector3(0, size, 0), world)));
            selected.Add((o, Vector3.Transform(new Vector3(0, 0, size), world)));
        }

        Show(scene, bones, selected);

        if (joints)
            PlaceJoints(pose);
    }

    // ---- joints

    private void EnsureJoints(RenderScene scene, HavokSkeletonData skeleton)
    {
        if (JointsFor == skeleton)
            return;

        DisposeJoints();
        JointsFor = skeleton;
        JointSphere ??= new DbgPrimSolidSphere(Transform.Default, 1f, BoneColor, 6, 8);

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
        }

        JointsShown = false;
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

            // Only on a change, as a colour change updates the instance
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
        if (JointsShown == shown)
            return;

        JointsShown = shown;
        foreach (var proxy in Joints)
            proxy.Visible = shown;
    }

    private void DisposeJoints()
    {
        foreach (var proxy in Joints)
            proxy.Dispose();

        Joints.Clear();
        JointsFor = null;
        JointsShown = false;
    }

    private void Show(RenderScene scene, List<(Vector3, Vector3)> bones, List<(Vector3, Vector3)> selected)
    {
        if (Pending != null)
            Release(Pending);

        var built = new List<(DebugPrimitiveRenderableProxy, Action)>();
        Add(scene, built, bones, BoneColor);
        Add(scene, built, selected, SelectedColor);

        Pending = built;
        PendingShownFrames = 0;
    }

    private static void Add(RenderScene scene, List<(DebugPrimitiveRenderableProxy, Action)> built,
        List<(Vector3, Vector3)> lines, Color color)
    {
        foreach (var chunk in lines.Chunk(DbgPrimLineBatch.MaxLines))
        {
            var primitive = new DbgPrimLineBatch(chunk);
            var proxy = new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, primitive)
            {
                Pickable = false,
                BaseColor = color,
                HighlightedColor = color,
                // The bones show through the character's mesh
                RenderOverlay = true,
                Visible = false,
            };
            built.Add((proxy, primitive.Release));
        }
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
            {
                if (!proxy.Visible)
                    proxy.Visible = true;
            }

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
        BuiltClip = null;
        BuiltFrame = -1;
        BuiltBone = -2;
        DisposeJoints();
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
