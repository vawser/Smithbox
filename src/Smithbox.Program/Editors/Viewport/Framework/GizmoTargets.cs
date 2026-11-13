using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// What the viewport gizmo moves. The gizmo does the drag maths; the target supplies the
/// starting transform and applies the result.
/// </summary>
public interface IGizmoTarget
{
    /// <summary>
    /// True when there is something to move, so the gizmo is shown.
    /// </summary>
    bool HasTarget { get; }

    /// <summary>
    /// True if the scale gizmo can be used on this target.
    /// </summary>
    bool AllowScale { get; }

    /// <summary>
    /// The gizmo's origin and orientation.
    /// </summary>
    Transform GetTransform();

    /// <summary>
    /// Called every frame of a drag with the gizmo's start and current transforms.
    /// </summary>
    void Preview(Transform original, Transform current, GizmoState.GizmosMode mode);

    /// <summary>
    /// Called once when the drag ends.
    /// </summary>
    void Commit(Transform original, Transform current, GizmoState.GizmosMode mode);
}

/// <summary>
/// The default gizmo target: the viewport's selected entities.
/// </summary>
public class SelectionGizmoTarget : IGizmoTarget
{
    private readonly VulkanViewport Viewport;

    public SelectionGizmoTarget(VulkanViewport viewport)
    {
        Viewport = viewport;
    }

    public bool HasTarget => Viewport.ViewportSelection.IsFilteredSelection<Entity>(o => o.HasTransform);

    public bool AllowScale => true;

    public Transform GetTransform()
    {
        return GetSelectionTransform(true);
    }

    /// <summary>
    /// The selection's gizmo origin. With useGizmoSpace false, the rotation is always the selection's own.
    /// </summary>
    public Transform GetSelectionTransform(bool useGizmoSpace)
    {
        Transform transform;

        if (Viewport.ViewportSelection.IsSingleFilteredSelection<Entity>(o => o.HasTransform))
        {
            var sel = Viewport.ViewportSelection.GetSingleFilteredSelection<Entity>(o => o.HasTransform);
            transform = sel.GetRootLocalTransform();
            if (GizmoState.Origin == GizmoState.GizmosOrigin.BoundingBox && sel.RenderSceneMesh != null)
            {
                transform.Position = sel.RenderSceneMesh.GetBounds().GetCenter();
            }
        }
        else
        {
            // Average the positions of the selections and use rotation of first
            Vector3 accumPos = Vector3.Zero;
            HashSet<Entity> sels = Viewport.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform);
            foreach (Entity sel in sels)
            {
                if (GizmoState.Origin == GizmoState.GizmosOrigin.BoundingBox && sel.RenderSceneMesh != null)
                {
                    accumPos += sel.RenderSceneMesh.GetBounds().GetCenter();
                }
                else
                {
                    accumPos += sel.GetRootLocalTransform().Position;
                }
            }

            transform = new Transform(
                accumPos / sels.Count(),
                sels.First().GetRootLocalTransform().EulerRotation
            );
        }

        if (useGizmoSpace && GizmoState.Space == GizmoState.GizmosSpace.World)
        {
            transform.Rotation = Quaternion.Identity;
        }

        return transform;
    }

    public void Preview(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        foreach (Entity sel in GetMovedSelection())
        {
            sel.SetTemporaryTransform(sel.ProjectGizmoDelta(original, current));
        }
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in GetMovedSelection())
        {
            sel.ClearTemporaryTransform(false);

            var includeScale = mode == GizmoState.GizmosMode.Scale;

            actlist.Add(sel.GetUpdateTransformAction(sel.ProjectGizmoDelta(original, current), includeScale));
        }

        ViewportCompoundAction action = new(actlist);
        Viewport.ActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// The selected objects the drag moves itself. An object whose parent (or a parent of that) is also selected
    /// already follows its parent, so moving it as well would move it twice.
    /// </summary>
    private List<Entity> GetMovedSelection()
    {
        HashSet<Entity> selection = Viewport.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform);

        return selection.Where(sel =>
        {
            for (Entity p = sel.Parent; p != null; p = p.Parent)
            {
                if (selection.Contains(p))
                    return false;
            }

            return true;
        }).ToList();
    }
}
