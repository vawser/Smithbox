#nullable enable
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The viewport's translate gizmo on the AI simulation's target character: dragging moves it on the ground, live, and
/// it keeps facing the character. The simulation reads the distance from where the characters are, so a move counts
/// straight away (an act re-checks it as it plays; the next act is chosen with it).
/// </summary>
public class LuaTargetGizmo : IGizmoTarget
{
    private readonly LuaViewer Viewer;

    private Vector3 DragStart;
    private bool Dragging;

    public LuaTargetGizmo(LuaViewer viewer)
    {
        Viewer = viewer;
    }

    public bool HasTarget => Viewer.Target != null;

    public bool AllowScale => false;

    public Transform GetTransform() =>
        Viewer.Target is { } t ? new Transform(t.Position, Vector3.Zero, Vector3.One) : Transform.Default;

    public void Preview(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        if (mode != GizmoState.GizmosMode.Translate || Viewer.Target is not { } target)
            return;
        if (!Dragging)
        {
            DragStart = target.Position;
            Dragging = true;
        }
        Move(DragStart + (current.Position - original.Position));
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        if (mode == GizmoState.GizmosMode.Translate && Viewer.Target != null)
            Move((Dragging ? DragStart : original.Position) + (current.Position - original.Position));
        Dragging = false;
    }

    /// <summary>Puts the target at a point on the ground, facing the character</summary>
    private void Move(Vector3 position)
    {
        Viewer.MoveTargetTo(position with { Y = 0 });
    }
}
