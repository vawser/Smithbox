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
        foreach (Entity sel in Viewport.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform))
        {
            sel.SetTemporaryTransform(ProjectTransformDelta(sel, original, current));
        }
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in Viewport.ViewportSelection.GetFilteredSelection<Entity>(o => o.HasTransform))
        {
            sel.ClearTemporaryTransform(false);

            var includeScale = mode == GizmoState.GizmosMode.Scale;

            actlist.Add(sel.GetUpdateTransformAction(ProjectTransformDelta(sel, original, current), includeScale));
        }

        ViewportCompoundAction action = new(actlist);
        Viewport.ActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// Calculate position of selection as its being moved
    /// </summary>
    private static Transform ProjectTransformDelta(Entity sel, Transform original, Transform current)
    {
        Transform objt = sel.GetLocalTransform();
        Transform rootT = sel.GetRootTransform();

        Quaternion deltaRot = current.Rotation * Quaternion.Inverse(original.Rotation);

        Vector3 deltaPos = Vector3.Transform(
            current.Position - original.Position,
            Quaternion.Inverse(rootT.Rotation));

        objt.Rotation = deltaRot * objt.Rotation;

        // TODO: fix rotation gizmo being wrong due to root object transform node rotation
        Vector3 rotateCenterOffset = Vector3.Transform(rootT.Position, rootT.Rotation);

        Vector3 posdif = objt.Position - original.Position + rotateCenterOffset;

        posdif = Vector3.Transform(
            Vector3.Transform(posdif, Quaternion.Conjugate(original.Rotation)),
            current.Rotation);

        objt.Position = original.Position + posdif;
        objt.Position += deltaPos - rotateCenterOffset;

        Vector3 deltaScale = current.Scale / original.Scale;
        objt.Scale *= deltaScale;

        return objt;
    }
}
