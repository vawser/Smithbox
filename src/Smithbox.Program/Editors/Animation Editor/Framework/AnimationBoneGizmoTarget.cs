#nullable enable
using StudioCore.Application;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The viewport gizmo on the selected bone, at the playing frame (rounded to a whole frame): it sits where the bone is
/// in model space (the character plays in place, so model space is the viewport's), and moves or turns it. The pose is
/// taken back into the bone's parent space as an offset key at that frame (Animation Editor Analysis.md §4.7). A drag
/// shows live and is one undoable <see cref="AnimationClipEditAction" /> when it ends. Playback is paused while
/// dragging, so the frame keyed is the one shown.
/// <para>
/// With the root motion selected (<see cref="AnimationEditorView.RootBone" />), the gizmo sits where the root motion
/// has taken the character by that frame (the root path's arrow), and a move or a turn about the clip's up axis is
/// keyed on the root (<see cref="EditableClip.RootTrack" />).
/// </para>
/// </summary>
public class AnimationBoneGizmoTarget : IGizmoTarget
{
    private readonly AnimationEditorView View;

    // The clip as it was when the drag started (and whether it was an edit), the clip source and the frame
    private EditableClip? DragStart;
    private bool DragStartEdited;
    private AnimationClipSource? DragSource;
    private int DragFrame = -1;
    private int DragTrack = int.MinValue;

    public AnimationBoneGizmoTarget(AnimationEditorView view)
    {
        View = view;
    }

    private (AnimationClipSource Source, EditableClip Clip, HavokSkeletonData Skeleton, int Bone, int Track, int Frame)? Target
    {
        get
        {
            if (View.Bank.Character is not { } character || View.SelectedClip is not { } source ||
                View.Bank.Get(source) is not { } clip || !View.ViewportWindow.HasClip)
                return null;

            var skeleton = character.Skeleton;
            var frame = View.ViewportWindow.CurrentWholeFrame;

            if (View.RootSelected)
                return (source, clip, skeleton, AnimationEditorView.RootBone, EditableClip.RootTrack, frame);

            var bone = View.SelectedBone;
            if (bone < 0 || bone >= skeleton.BoneCount)
                return null;

            var track = clip.BoneToTrack(skeleton.BoneCount)[bone];
            if (track < 0)
                return null;

            return (source, clip, skeleton, bone, track, frame);
        }
    }

    public bool HasTarget => Target != null;

    public bool AllowScale => false;

    public Transform GetTransform()
    {
        if (Target is not { } target)
            return Transform.Default;

        if (target.Track == EditableClip.RootTrack)
        {
            // Where the root is at the frame, turned as it is there (the root path's arrow)
            var sample = target.Clip.RootSampleAt(target.Frame);
            return new Transform(new Vector3(sample.X, sample.Y, sample.Z),
                Quaternion.CreateFromAxisAngle(target.Clip.UpAxis, sample.W), Vector3.One);
        }

        var pose = target.Clip.ModelPose(target.Skeleton, target.Frame);
        var p = pose[target.Bone];

        // Where the bone is drawn: moved along the root motion with Travel on
        var root = View.ViewportWindow.RootWorld;
        return new Transform(Vector3.Transform(p.Position, root),
            Quaternion.Concatenate(p.Rotation, Quaternion.CreateFromRotationMatrix(root)), Vector3.One);
    }

    /// <summary>The gizmo's transform taken back from where the character is drawn into its model space</summary>
    private Transform ToModel(Transform current, int track)
    {
        // The root's gizmo is in the root path's space already
        var root = View.ViewportWindow.RootWorld;
        if (track == EditableClip.RootTrack || root.IsIdentity || !Matrix4x4.Invert(root, out var toModel))
            return current;

        var rotation = Quaternion.Concatenate(current.Rotation, Quaternion.Inverse(Quaternion.CreateFromRotationMatrix(root)));
        return new Transform(Vector3.Transform(current.Position, toModel), Quaternion.Normalize(rotation), current.Scale);
    }

    public void Preview(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        if (Target is not { } target)
            return;

        if (DragStart == null || DragSource != target.Source || DragTrack != target.Track)
        {
            DragStart = target.Clip;
            DragStartEdited = View.Bank.IsEdited(target.Source);
            DragSource = target.Source;
            DragFrame = target.Frame;
            DragTrack = target.Track;

            // The frame keyed is the frame shown
            View.ViewportWindow.SeekFrame(DragFrame);
        }

        // Shown straight away, without a step in the history: the step is made when the drag ends
        View.Bank.SetEdited(DragSource, WithOthers(DragStart, Keyed(DragStart, target.Skeleton, target.Bone, DragTrack, DragFrame,
            ToModel(current, DragTrack), mode), target.Skeleton, DragTrack, DragFrame));
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        var start = DragStart;
        var startEdited = DragStartEdited;
        var source = DragSource;
        var frame = DragFrame;
        var track = DragTrack;
        DragStart = null;
        DragSource = null;
        DragFrame = -1;
        DragTrack = int.MinValue;

        if (Target is not { } target)
            return;

        // A click with no drag never previewed
        if (start == null || source != target.Source || track != target.Track)
        {
            start = target.Clip;
            startEdited = View.Bank.IsEdited(target.Source);
            source = target.Source;
            frame = target.Frame;
            track = target.Track;
        }

        var keyed = WithOthers(start, Keyed(start, target.Skeleton, target.Bone, track, frame, ToModel(current, track), mode),
            target.Skeleton, track, frame);
        var several = track != EditableClip.RootTrack && View.OtherSelectedBones.Count > 0;

        // The bank goes back to the clip before the drag, so the step's undo has it
        View.Bank.SetEdited(source, startEdited ? start : null);

        if (keyed == null)
            return;

        var rotate = mode == GizmoState.GizmosMode.Rotate;
        var name = track == EditableClip.RootTrack
            ? LOC.Get(rotate ? "ANIM_Action_Turn_Root" : "ANIM_Action_Move_Root", frame)
            : LOC.Get(rotate ? "ANIM_Action_Rotate_Bone" : "ANIM_Action_Move_Bone",
                several ? LOC.Get("ANIM_Bones_Several", target.Skeleton.Names[target.Bone], View.OtherSelectedBones.Count)
                    : target.Skeleton.Names[target.Bone], frame);

        View.Execute(new AnimationClipEditAction(View.Bank, source, keyed, name));
    }

    /// <summary>
    /// With several bones selected (W10): the change the drag made to the primary bone's offset at the frame (a turn
    /// and a move, in its parent's space) is made to each other selected bone's offset too, in its own parent's space.
    /// So turning three spine bones together curls the spine, each by the same angle. Bones the clip doesn't animate
    /// are passed over. The root motion is keyed on its own.
    /// </summary>
    private EditableClip? WithOthers(EditableClip start, EditableClip? keyed, HavokSkeletonData skeleton, int track, int frame)
    {
        if (keyed == null || track == EditableClip.RootTrack || View.OtherSelectedBones.Count == 0)
            return keyed;

        var before = start.OffsetAt(track, frame);
        var after = keyed.OffsetAt(track, frame);
        var turn = Quaternion.Normalize(after.Rotation * Quaternion.Inverse(before.Rotation));
        var move = after.Translation - before.Translation;

        var boneToTrack = start.BoneToTrack(skeleton.BoneCount);
        foreach (var bone in View.OtherSelectedBones)
        {
            var other = bone >= 0 && bone < boneToTrack.Length ? boneToTrack[bone] : -1;
            if (other < 0 || other == track)
                continue;

            var old = start.OffsetAt(other, frame);
            keyed = keyed.WithKey(other, new BoneOffsetKey(frame, Quaternion.Normalize(turn * old.Rotation), old.Translation + move));
        }

        return keyed;
    }

    /// <summary>
    /// The clip with the key at <paramref name="frame" /> set so the bone has the gizmo's pose there: the new local
    /// transform (from the gizmo's model-space pose and its parent's, which the key doesn't move) as an offset from
    /// the clip's own local transform. A translate drag changes only the key's move, a rotate drag only its turn. For
    /// the root, the gizmo's position and its turn about the up axis.
    /// </summary>
    private static EditableClip? Keyed(EditableClip start, HavokSkeletonData skeleton, int bone, int track, int frame,
        Transform current, GizmoState.GizmosMode mode)
    {
        if (track == EditableClip.RootTrack)
        {
            return mode switch
            {
                GizmoState.GizmosMode.Rotate => start.KeyedRoot(frame, null, EditableClip.TwistAngle(current.Rotation, start.UpAxis)),
                GizmoState.GizmosMode.Translate => start.KeyedRoot(frame, current.Position, null),
                _ => null
            };
        }

        return mode switch
        {
            GizmoState.GizmosMode.Rotate => start.KeyedToModelPose(skeleton, bone, track, frame, null, current.Rotation),
            GizmoState.GizmosMode.Translate => start.KeyedToModelPose(skeleton, bone, track, frame, current.Position, null),
            _ => null
        };
    }
}
