using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// Grid movement mode: while a grid is set here, the viewport gizmo of that editor moves the grid
/// instead of the selection. One grid at most per editor.
/// </summary>
public static class ViewportGridMovement
{
    private static readonly ViewportGridType?[] MovingGrids = new ViewportGridType?[3];

    public static ViewportGridType? GetMovingGrid(ViewportGridEditor editor)
    {
        return MovingGrids[(int)editor];
    }

    public static bool IsMoving(ViewportGridEditor editor, ViewportGridType grid)
    {
        return MovingGrids[(int)editor] == grid;
    }

    public static void Start(ViewportGridEditor editor, ViewportGridType grid)
    {
        MovingGrids[(int)editor] = grid;
    }

    public static void Stop(ViewportGridEditor editor)
    {
        MovingGrids[(int)editor] = null;
    }

    /// <summary>
    /// The grid's transform. Used to draw the grid, move it and snap to it, so all three agree.
    /// </summary>
    public static Transform GetGridTransform(ViewportGridSettings grid)
    {
        return new Transform(
            grid.Position.X, grid.Position.Y, grid.Position.Z,
            Utils.DegToRadians(grid.Rotation.X),
            Utils.DegToRadians(grid.Rotation.Y),
            Utils.DegToRadians(grid.Rotation.Z));
    }

    public static void SetGridPosition(ViewportGridSettings grid, Vector3 position)
    {
        grid.Position = position;
    }

    /// <summary>
    /// Stores a rotation as the grid's euler degrees. <see cref="Transform.QuaternionToEuler" /> is the
    /// inverse of the Y * Z * X order the grid is drawn with, so the grid doesn't jump.
    /// </summary>
    public static void SetGridRotation(ViewportGridSettings grid, Quaternion rotation)
    {
        var euler = Transform.QuaternionToEuler(Quaternion.Normalize(rotation));

        grid.Rotation.X = Utils.RadiansToDeg(euler.X);
        grid.Rotation.Y = Utils.RadiansToDeg(euler.Y);
        grid.Rotation.Z = Utils.RadiansToDeg(euler.Z);
    }
}

/// <summary>
/// Gizmo target for grid movement mode. Edits the grid's settings directly, so moves aren't undoable.
/// </summary>
public class ViewportGridGizmoTarget : IGizmoTarget
{
    private readonly ViewportGridEditor Editor;

    public ViewportGridGizmoTarget(ViewportGridEditor editor)
    {
        Editor = editor;
    }

    public bool HasTarget
    {
        get
        {
            var grid = ViewportGridMovement.GetMovingGrid(Editor);

            return grid != null && ViewportGridConfig.Get(Editor, grid.Value).Display;
        }
    }

    public bool AllowScale => false;

    public Transform GetTransform()
    {
        var grid = ViewportGridMovement.GetMovingGrid(Editor);
        if (grid == null)
        {
            return Transform.Default;
        }

        // Always in the grid's own space, so the arrows slide the grid along its plane
        return ViewportGridMovement.GetGridTransform(ViewportGridConfig.Get(Editor, grid.Value));
    }

    public void Preview(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        var grid = ViewportGridMovement.GetMovingGrid(Editor);
        if (grid == null)
        {
            return;
        }

        var config = ViewportGridConfig.Get(Editor, grid.Value);

        // Only write the part being dragged, so translating never rounds the stored rotation
        if (mode == GizmoState.GizmosMode.Translate)
        {
            ViewportGridMovement.SetGridPosition(config, current.Position);
        }
        else if (mode == GizmoState.GizmosMode.Rotate)
        {
            ViewportGridMovement.SetGridRotation(config, current.Rotation);
        }
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        Preview(original, current, mode);
    }
}
