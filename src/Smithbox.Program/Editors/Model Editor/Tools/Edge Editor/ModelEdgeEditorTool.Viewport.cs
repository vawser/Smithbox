using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Editing in the viewport, as the Map Editor's EDGE Editor does it. While Edit in Viewport is on and the tool is
/// shown, the tool takes the viewport's clicks: the handle under the cursor (see <see cref="EdgeHandles"/>) is
/// outlined; a click picks it, selects its entry and puts the gizmo on it; dragging it slides it over the model's
/// surface under the cursor (<see cref="ModelEdgeSurface"/>). A click on a ledge or wall selects its entry. Every move
/// is one <see cref="ModelEdgeEditAction"/>.
/// <para>
/// The markers are drawn every frame the view is drawn (<see cref="UpdateMarkers"/>), whether or not the tool is.
/// </para>
/// </summary>
public partial class ModelEdgeEditorTool
{
    private bool Editing;

    // The ImGui frame the tool was last drawn: editing stops when the tool isn't, so the clicks aren't kept from
    // selecting once it is closed
    private int LastDrawnFrame = -10;

    public ModelEdgeGizmoTarget GizmoTarget { get; }

    private readonly ModelEdgeMarkers Markers = new();

    // The picked handle, by its kind and its first point, so it is found again after the file changes
    private EdgeHandleKind? PickedKind;
    private EdgePointRef PickedPoint;

    // What is under the cursor
    private EdgeHandle Hovered;
    private int HoveredSegment = -1;

    // A press on a handle, which becomes a drag once the mouse moves
    private bool Pressed;
    private bool Dragging;
    private Vector2 PressMouse;
    private float PressDistance;
    private ModelWrapper DragWrapper;
    private EDGE DragBefore;

    // The file before a gizmo drag
    private EDGE GizmoBefore;

    private const float DragStartPixels = 4.0f;

    // The marker drawn over the handle under the cursor
    private DebugPrimitiveRenderableProxy HoverMesh;
    private RenderScene HoverScene;

    // The surface points are placed on: the loaded model's drawn FLVER triangles, built when first needed
    private ModelEdgeSurface Surface;
    private FLVER2 SurfaceFlver;
    private bool SurfaceStale = true;

    // The handles of the file, kept until its contents change
    private int HandlesKey;
    private List<EdgeHandle> HandlesCache;

    private record EdgeContext(ModelWrapper Wrapper, EDGE Edge, List<EdgeHandle> Handles, Matrix4x4 Space, Matrix4x4 ToEdge);

    /// <summary>True while the tool takes the viewport's clicks.</summary>
    public bool IsEditing => Editing && ImGui.GetFrameCount() - LastDrawnFrame <= 2 && CFG.Current.ModelEditor_DisplayEdgePoints;

    private List<EdgeHandle> GetHandles(EDGE edge)
    {
        var key = ModelEdgeMarkers.ContentFingerprint(edge);

        if (HandlesCache == null || key != HandlesKey)
        {
            HandlesCache = EdgeHandles.Build(edge);
            HandlesKey = key;
        }

        return HandlesCache;
    }

    /// <summary>The loaded model and its editable EDGE file, with the matrices into and out of the EDGE's space.</summary>
    private bool TryGetContext(out EdgeContext context)
    {
        context = null;

        var wrapper = GetWrapper();

        if (wrapper?.EDGE == null || !wrapper.CanEditEdge)
            return false;

        var space = GetSpace(wrapper);

        if (!Matrix4x4.Invert(space, out var toEdge))
            return false;

        context = new EdgeContext(wrapper, wrapper.EDGE, GetHandles(wrapper.EDGE), space, toEdge);
        return true;
    }

    #region Markers

    /// <summary>
    /// Draws the loaded model's EDGE markers. Called each frame by the view's viewport.
    /// </summary>
    public void UpdateMarkers()
    {
        var wrapper = CanUse() ? GetWrapper() : null;

        View.RenderScene.SetDrawFilter(RenderFilter.EdgePoint, CFG.Current.ModelEditor_DisplayEdgePoints);

        if (wrapper?.EDGE == null)
        {
            Markers.Clear();
            SetHover(null, -1, null, Matrix4x4.Identity);
            return;
        }

        // The selected entry is only drawn as selected while the tool shows it
        var selected = ImGui.GetFrameCount() - LastDrawnFrame <= 2 && wrapper == SelectedWrapper ? SelectedIndex : -1;

        Markers.Update(View.RenderScene, wrapper.EDGE, selected, GetSpace(wrapper));
    }

    #endregion

    #region Viewport

    /// <summary>
    /// Handles the mouse while editing. Called each frame by the active viewport; returns true while the clicks are
    /// taken, so they don't select.
    /// </summary>
    public bool HandleViewportInput(VulkanViewport viewport, Ray ray, bool canPick)
    {
        if (!IsEditing)
        {
            EndDrag();
            SetHover(null, -1, null, Matrix4x4.Identity);
            EndDrawing();
            return false;
        }

        // Adding grapples and drawing chains (ModelEdgeEditorTool.Authoring)
        if (Mode != EdgeEditMode.Select)
        {
            EndDrag();
            return HandleAuthoringInput(viewport, ray, canPick);
        }

        EndDrawing();

        if (!TryGetContext(out var context))
        {
            EndDrag();
            SetHover(null, -1, null, Matrix4x4.Identity);
            return true;
        }

        var mouse = InputManager.MousePosition;
        var direction = Vector3.Normalize(ray.Direction);

        // A press on a handle: a drag once the mouse moves, ended by letting go
        if (Pressed)
        {
            if (InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action) && context.Wrapper == DragWrapper)
            {
                if (!Dragging && Vector2.Distance(mouse, PressMouse) > DragStartPixels)
                {
                    Dragging = true;
                }

                if (Dragging)
                {
                    DragTo(context, ray);
                }
            }
            else
            {
                EndDrag();
            }

            if (TryGetContext(out var after))
            {
                SetHover(GetPicked(after.Handles), -1, viewport.RenderScene, after.Space);
            }

            return true;
        }

        var hit = Pick(context, ray.Origin, direction);

        SetHover(hit.Handle, hit.Segment, viewport.RenderScene, context.Space);

        if (canPick && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
        {
            if (hit.Handle != null)
            {
                PickedKind = hit.Handle.Kind;
                PickedPoint = hit.Handle.Points[0];
                SelectEntry(hit.Handle.Entry);

                Pressed = true;
                Dragging = false;
                PressMouse = mouse;
                PressDistance = hit.Distance;
                DragWrapper = context.Wrapper;
                DragBefore = EdgeAuthoring.Copy(context.Edge);
            }
            else if (hit.Segment >= 0)
            {
                PickedKind = null;
                SelectEntry(hit.Segment);
            }
            else
            {
                PickedKind = null;
            }
        }

        return true;
    }

    private record struct PickHit(EdgeHandle Handle, int Segment, float Distance);

    /// <summary>
    /// The nearest handle under a world ray, or else the nearest ledge or wall segment. Handles win over segments, so a
    /// chain's points can be picked where its segments meet.
    /// </summary>
    private PickHit Pick(EdgeContext context, Vector3 origin, Vector3 direction)
    {
        var pointRadius = MathF.Max(CFG.Current.Viewport_Edge_Point_Size * 1.6f, 0.04f);
        var segmentRadius = MathF.Max(CFG.Current.Viewport_Edge_Segment_Radius * 3.0f, 0.04f);

        PickHit handleHit = new(null, -1, float.MaxValue);
        PickHit segmentHit = new(null, -1, float.MaxValue);

        foreach (var handle in context.Handles)
        {
            var centre = Vector3.Transform(handle.Position, context.Space);
            var radius = handle.Kind is EdgeHandleKind.Pull ? pointRadius * 0.8f : pointRadius;

            if (EdgeHandles.RaySphere(origin, direction, centre, radius) is float t && t < handleHit.Distance)
            {
                handleHit = new PickHit(handle, -1, t);
            }
        }

        if (handleHit.Handle != null)
            return handleHit;

        for (var i = 0; i < context.Edge.Edges.Count; i++)
        {
            var entry = context.Edge.Edges[i];

            if (entry.Type is EDGE.EdgeType.Grapple || entry.V1 == entry.V2)
                continue;

            var a = Vector3.Transform(entry.V1, context.Space);
            var b = Vector3.Transform(entry.V2, context.Space);

            if (EdgeHandles.RaySegment(origin, direction, a, b, segmentRadius) is float t && t < segmentHit.Distance)
            {
                segmentHit = new PickHit(null, i, t);
            }
        }

        return segmentHit;
    }

    private EdgeHandle GetPicked(List<EdgeHandle> handles)
    {
        return PickedKind is EdgeHandleKind kind ? EdgeHandles.Find(handles, kind, PickedPoint) : null;
    }

    /// <summary>
    /// Moves the picked handle to the model's surface under the cursor, shown straight away; the move is recorded when
    /// the drag ends. Off the surface, the handle stays at the distance it was picked at.
    /// </summary>
    private void DragTo(EdgeContext context, Ray ray)
    {
        var handle = GetPicked(context.Handles);

        if (handle == null)
            return;

        Vector3 local;

        if (RayCastSurface(context.Wrapper, context.ToEdge, ray.Origin, Vector3.Normalize(ray.Direction), out var hit))
        {
            local = hit;
        }
        else
        {
            var worldPoint = ray.Origin + Vector3.Normalize(ray.Direction) * PressDistance;
            local = Vector3.Transform(worldPoint, context.ToEdge);
        }

        Preview(context.Edge, EdgeHandles.Move(context.Edge, handle, local));
    }

    /// <summary>
    /// Shows a moved copy of the file by writing its values into the file's own entries (a move never adds or removes
    /// one), so the markers follow; the action at the end of the drag records it.
    /// </summary>
    private static void Preview(EDGE edge, EDGE moved) => ModelEdgeFileState.PreviewInto(edge, moved);

    /// <summary>
    /// Records a drag that moved anything as one action, and lets go of the press: the file is put back as it was before
    /// the drag, then the action moves it, so undo returns it there.
    /// </summary>
    private void EndDrag()
    {
        if (Pressed && Dragging && DragWrapper?.EDGE != null && DragBefore != null)
        {
            var file = DragWrapper.EDGE;
            var moved = EdgeAuthoring.Copy(file);

            if (!SameFile(DragBefore, moved))
            {
                Preview(file, DragBefore);
                Execute(DragWrapper, moved, LOC.Get("MAP_History_Edge_Move", DragWrapper.Name));
            }
        }

        Pressed = false;
        Dragging = false;
        DragWrapper = null;
        DragBefore = null;
    }

    private static bool SameFile(EDGE a, EDGE b) => a.Write().AsSpan().SequenceEqual(b.Write());

    #endregion

    #region Surface

    /// <summary>
    /// The nearest point of the model's drawn surface under a world ray, in the EDGE's space. False when the ray misses
    /// it, or when there is no FLVER to place on.
    /// </summary>
    private bool RayCastSurface(ModelWrapper wrapper, Matrix4x4 toEdge, Vector3 origin, Vector3 direction, out Vector3 point)
    {
        point = default;

        var surface = GetSurface(wrapper);

        if (surface == null)
            return false;

        // The display rotation only turns the model, so distances stay the same in its space
        var localOrigin = Vector3.Transform(origin, toEdge);
        var localDirection = Vector3.TransformNormal(direction, toEdge);
        var length = localDirection.Length();

        if (length < 1e-8f)
            return false;

        localDirection /= length;

        if (!surface.RayCast(localOrigin, localDirection, out var distance))
            return false;

        point = localOrigin + localDirection * distance;
        return true;
    }

    private ModelEdgeSurface GetSurface(ModelWrapper wrapper)
    {
        var flver = wrapper?.Container?.Flver;

        if (flver == null)
            return null;

        if (Surface == null || SurfaceStale || !ReferenceEquals(SurfaceFlver, flver))
        {
            Surface = ModelEdgeSurface.Build(flver, Project.Descriptor.ProjectType);
            SurfaceFlver = flver;
            SurfaceStale = false;
        }

        return Surface.TriangleCount > 0 ? Surface : null;
    }

    #endregion

    #region Gizmo

    /// <summary>Where the gizmo sits: on the picked handle, in the viewport's space.</summary>
    public bool TryGetGizmoPosition(out Vector3 position)
    {
        position = default;

        if (!IsEditing || Mode != EdgeEditMode.Select || Pressed || PickedKind == null || !TryGetContext(out var context))
            return false;

        var handle = GetPicked(context.Handles);

        if (handle == null)
            return false;

        position = Vector3.Transform(handle.Position, context.Space);
        return true;
    }

    /// <summary>
    /// Moves the picked handle to a point in the viewport for the gizmo: shown while it is dragged, recorded as one
    /// action when it is let go.
    /// </summary>
    public void GizmoMove(Vector3 position, bool commit)
    {
        if (!TryGetContext(out var context))
            return;

        var handle = GetPicked(context.Handles);

        if (handle == null)
            return;

        GizmoBefore ??= EdgeAuthoring.Copy(context.Edge);

        var moved = EdgeHandles.Move(context.Edge, handle, Vector3.Transform(position, context.ToEdge));

        Preview(context.Edge, moved);

        if (commit)
        {
            if (!SameFile(GizmoBefore, moved))
            {
                Preview(context.Edge, GizmoBefore);
                Execute(context.Wrapper, moved, LOC.Get("MAP_History_Edge_Move", context.Wrapper.Name));
            }

            GizmoBefore = null;
        }
    }

    #endregion

    #region Hover

    private void SetHover(EdgeHandle handle, int segment, RenderScene scene, Matrix4x4 space)
    {
        Hovered = handle;
        HoveredSegment = segment;

        if (scene != HoverScene)
        {
            HoverMesh?.Dispose();
            HoverMesh = null;
            HoverScene = scene;
        }

        if (handle == null || scene == null)
        {
            if (HoverMesh != null)
                HoverMesh.Visible = false;

            return;
        }

        if (HoverMesh == null)
        {
            HoverMesh = RenderableHelper.GetEdgeMarkerWireProxy(scene, false, CFG.Current.Viewport_Edge_Hover_Color, true);
            HoverMesh.DrawFilter = RenderFilter.EdgePoint;
        }

        var size = MathF.Max(CFG.Current.Viewport_Edge_Point_Size, 0.01f) * (handle.Kind is EdgeHandleKind.Pull ? 1.2f : 1.9f);
        var position = Vector3.Transform(handle.Position, space);

        HoverMesh.World = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(position);
        HoverMesh.Visible = true;
    }

    private static string DescribeHandle(EdgeHandle handle)
    {
        return handle.Kind switch
        {
            EdgeHandleKind.Grapple => LOC.Get("MAP_EdgeEditor_Handle_Grapple", handle.Entry),
            EdgeHandleKind.Pull => LOC.Get("MAP_EdgeEditor_Handle_Pull", handle.Entry),
            _ => LOC.Get("MAP_EdgeEditor_Handle_Vertex", string.Join(", ", handle.Points.Select(p => p.Entry).Distinct()))
        };
    }

    #endregion

    #region Tool Window

    /// <summary>The Edit in Viewport toggle, the mode, and what the cursor is over. Null file: placing creates one.</summary>
    private void DisplayViewportEditing(ModelWrapper wrapper, EDGE edge)
    {
        LastDrawnFrame = ImGui.GetFrameCount();

        // Viewport
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Viewport_Header"), LOC.Get("MODEL_EdgeEditor_Viewport_Header_TT"));

        GUI.MultiButtonInput("modelEdgeEditorEditToggle",
            "modelEdgeEditorEditToggleButton",
            Editing ? LOC.Get("MAP_EdgeEditor_Stop_Editing") : LOC.Get("MAP_EdgeEditor_Start_Editing"),
            LOC.Get("MODEL_EdgeEditor_Editing_TT"),
            () =>
            {
                Editing = !Editing;
                PickedKind = null;
                EndDrag();
                EndDrawing();
            });

        if (!Editing)
            return;

        DisplayModes();

        if (!CFG.Current.ModelEditor_DisplayEdgePoints)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_EdgeEditor_Editing_Needs_Points"));
        }
        else if (GetSurface(wrapper) == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_EdgeEditor_No_Surface"));
        }

        if (Mode != EdgeEditMode.Select)
        {
            DisplayAuthoringStatus();
        }
        else if (edge == null)
        {
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Hovered_None"));
        }
        else if (Hovered != null)
        {
            GUI.WrappedText(LOC.Get("MODEL_EdgeEditor_Hovered", DescribeHandle(Hovered)));
        }
        else if (HoveredSegment >= 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_EdgeEditor_Hovered_Segment", HoveredSegment));
        }
        else
        {
            // Always draw the line, so the panel below doesn't jump as the cursor moves on and off handles
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Hovered_None"));
        }

        if (edge != null && Mode == EdgeEditMode.Select && GetPicked(GetHandles(edge)) is { } picked)
        {
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Picked", DescribeHandle(picked)));
        }
    }

    #endregion
}

/// <summary>
/// The gizmo on the Model Editor EDGE Editor's picked handle. Only moves: a point has no rotation or scale.
/// </summary>
public class ModelEdgeGizmoTarget : IGizmoTarget
{
    private readonly ModelEdgeEditorTool Tool;

    public ModelEdgeGizmoTarget(ModelEdgeEditorTool tool)
    {
        Tool = tool;
    }

    public bool HasTarget => Tool.TryGetGizmoPosition(out _);

    public bool AllowScale => false;

    public Transform GetTransform()
    {
        return Tool.TryGetGizmoPosition(out var position)
            ? new Transform(position, Quaternion.Identity, Vector3.One)
            : Transform.Default;
    }

    public void Preview(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        if (mode == GizmoState.GizmosMode.Translate)
            Tool.GizmoMove(current.Position, false);
    }

    public void Commit(Transform original, Transform current, GizmoState.GizmosMode mode)
    {
        if (mode == GizmoState.GizmosMode.Translate)
            Tool.GizmoMove(current.Position, true);
    }
}
