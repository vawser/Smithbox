#nullable enable
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// Moves a cut: every key of its offset.sib event by the same translation and rotation (degrees), so a placement that
/// moves over the cut keeps its motion. The keys are the MQB's own transforms.
/// </summary>
public class CutscenePlacementAction : EditorAction
{
    private readonly CutsceneFile File;
    private readonly List<MQB.Transform> Keys;
    private readonly (Vector3 Translation, Vector3 Rotation)[] Before;
    private readonly (Vector3 Translation, Vector3 Rotation)[] After;
    private readonly bool WasDirty;

    public CutscenePlacementAction(CutsceneFile file, List<MQB.Transform> keys,
        (Vector3, Vector3)[] before, (Vector3, Vector3)[] after)
    {
        File = file;
        Keys = keys;
        Before = before;
        After = after;
        WasDirty = file.IsDirty;
    }

    public static (Vector3, Vector3)[] Snapshot(List<MQB.Transform> keys) =>
        keys.Select(k => (k.Translation, k.Rotation)).ToArray();

    /// <summary>The keys of a snapshot, moved and turned by the same amount</summary>
    public static (Vector3, Vector3)[] Shifted((Vector3 Translation, Vector3 Rotation)[] from, Vector3 translation, Vector3 rotation) =>
        from.Select(k => (k.Translation + translation, k.Rotation + rotation)).ToArray();

    public static void Apply(List<MQB.Transform> keys, (Vector3 Translation, Vector3 Rotation)[] values)
    {
        for (var i = 0; i < keys.Count && i < values.Length; i++)
        {
            keys[i].Translation = values[i].Translation;
            keys[i].Rotation = values[i].Rotation;
        }
    }

    public override ActionEvent Execute()
    {
        Apply(Keys, After);
        File.IsDirty = true;
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(Keys, Before);
        File.IsDirty = WasDirty;
        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// The viewport gizmo on the selected cut's placement: translation and rotation (no scale). A drag shows live and
/// becomes one undoable <see cref="CutscenePlacementAction" /> when it ends.
/// </summary>
public class CutscenePlacementGizmoTarget : IGizmoTarget
{
    private readonly CutsceneEditorScreen Editor;

    // The keys as they were when the drag started
    private (Vector3, Vector3)[]? DragStart;
    private CutscenePlacement? DragPlacement;

    public CutscenePlacementGizmoTarget(CutsceneEditorScreen editor)
    {
        Editor = editor;
    }

    private CutscenePlacement? Placement => Editor.SelectedPlacement is { OffsetEvent: not null, Keys.Count: > 0 } p ? p : null;

    public bool HasTarget => Placement != null && Editor.SelectedFile != null;

    public bool AllowScale => false;

    public Transform GetTransform()
    {
        var placement = Placement;
        if (placement == null)
            return Transform.Default;

        var key = placement.Keys[0];
        return new Transform(key.Translation, key.Rotation * (MathF.PI / 180f), Vector3.One);
    }

    private (Vector3 Translation, Vector3 Rotation) Delta(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        return mode switch
        {
            GizmoState.GizmosMode.Translate => (current.Position - original.Position, Vector3.Zero),
            GizmoState.GizmosMode.Rotate => (Vector3.Zero, (current.EulerRotation - original.EulerRotation) * (180f / MathF.PI)),
            _ => (Vector3.Zero, Vector3.Zero)
        };
    }

    public void Preview(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        var placement = Placement;
        if (placement == null)
            return;

        if (DragStart == null || DragPlacement != placement)
        {
            DragStart = CutscenePlacementAction.Snapshot(placement.Keys);
            DragPlacement = placement;
        }

        var (t, r) = Delta(original, current, mode);
        CutscenePlacementAction.Apply(placement.Keys, CutscenePlacementAction.Shifted(DragStart, t, r));
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        var placement = Placement;
        if (placement == null || Editor.SelectedFile == null)
        {
            DragStart = null;
            return;
        }

        var before = DragPlacement == placement && DragStart != null ? DragStart : CutscenePlacementAction.Snapshot(placement.Keys);
        var (t, r) = Delta(original, current, mode);
        var after = CutscenePlacementAction.Shifted(before, t, r);

        DragStart = null;
        DragPlacement = null;

        if (t == Vector3.Zero && r == Vector3.Zero)
        {
            CutscenePlacementAction.Apply(placement.Keys, before);
            return;
        }

        Editor.ActionManager.ExecuteAction(new CutscenePlacementAction(Editor.SelectedFile, placement.Keys, before, after));
    }
}
