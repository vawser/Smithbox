using Hexa.NET.ImGui;
using Havok.Shared;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Editing in the viewport (M3). While Edit in Viewport is on and the tool is shown, the tool takes the viewport's
/// clicks. Every displayed collision's points can be picked, without selecting the collision first: the cursor over a
/// handle (see <see cref="EdgeHandles"/>) highlights it; a click picks it, which makes its collision the one the tool
/// edits, selects its entry and puts the gizmo on it; dragging it slides it over that collision under the cursor. A
/// click on a ledge or wall selects its entry. Every move is one <see cref="EdgeEditAction"/>.
/// </summary>
public partial class EdgeEditorTool
{
    private bool Editing;

    // The ImGui frame the tool was last drawn: editing stops when the tool isn't, so the clicks aren't kept from
    // selecting once it is closed
    private int LastDrawnFrame = -10;

    public EdgeGizmoTarget GizmoTarget { get; }

    // The picked handle, by its kind and its first point, so it is found again after the file changes. It belongs to
    // the tool's current collision part.
    private EdgeHandleKind? PickedKind;
    private EdgePointRef PickedPoint;

    // What is under the cursor, of any collision
    private EdgeHandle Hovered;
    private MsbEntity HoveredPart;
    private int HoveredSegment = -1;

    // A press on a handle, which becomes a drag once the mouse moves
    private bool Pressed;
    private bool Dragging;
    private Vector2 PressMouse;
    private float PressDistance;
    private EDGE DragBefore;

    // The file before a gizmo drag
    private EDGE GizmoBefore;

    private const float DragStartPixels = 4.0f;

    // The marker drawn over the handle under the cursor
    private DebugPrimitiveRenderableProxy HoverMesh;
    private RenderScene HoverScene;

    // The triangles of each collision, for dragging and placing points over them; only ones that were loaded are kept
    private readonly Dictionary<string, (object Document, List<WallGeometry> Geometry)> Surfaces = new(StringComparer.OrdinalIgnoreCase);

    // Each collision's file and handles, kept until the file's version changes, so picking over every displayed
    // collision doesn't copy every file each frame
    private sealed record CachedHandles(int Version, EDGE Edge, List<EdgeHandle> Handles);
    private readonly Dictionary<string, CachedHandles> HandleCache = new(StringComparer.OrdinalIgnoreCase);

    private record EdgeContext(MsbEntity Part, MapContainer Map, string MapId, string ModelName, EDGE Edge, List<EdgeHandle> Handles);

    /// <summary>
    /// The file and handles of a collision part, from the cache. The file must only be read: edits go through
    /// <see cref="EdgeHandles.Move"/>, which copies it.
    /// </summary>
    private CachedHandles GetHandles(MsbEntity part)
    {
        if (part?.Container is not MapContainer map)
            return null;

        var modelName = part.GetPartModelName();
        var version = View.EdgeBank.GetVersion(map.Name, modelName);

        if (version < 0)
            return null;

        var key = EdgeBank.GetFileKey(map.Name, modelName);

        if (HandleCache.TryGetValue(key, out var cached) && cached.Version == version)
            return cached;

        var edge = View.EdgeBank.GetEdge(map.Name, modelName);

        if (edge == null)
            return null;

        cached = new CachedHandles(version, edge, EdgeHandles.Build(edge));
        HandleCache[key] = cached;

        return cached;
    }

    /// <summary>
    /// The collision part and file being edited: the tool's current collision part, with an EDGE file read.
    /// </summary>
    private bool TryGetContext(out EdgeContext context)
    {
        context = null;

        var part = GetCurrentPart();

        if (part == null || part.Container is not MapContainer map)
            return false;

        var cached = GetHandles(part);

        if (cached == null)
            return false;

        context = new EdgeContext(part, map, map.Name, part.GetPartModelName(), cached.Edge, cached.Handles);
        return true;
    }

    /// <summary>True while the tool takes the viewport's clicks.</summary>
    public bool IsEditing => Editing && ImGui.GetFrameCount() - LastDrawnFrame <= 2 && View.EdgeBank.Visible;

    private void SelectEntry(int index)
    {
        if (SelectedIndex != index)
        {
            SelectedIndex = index;
            Buffer = null;
        }
    }

    #region Viewport

    /// <summary>
    /// Handles the mouse while editing. Called each frame by the active viewport; returns true while the clicks are
    /// taken, so they don't select.
    /// </summary>
    public bool HandleViewportInput(VulkanViewport viewport, Ray ray, bool canPick)
    {
        if (!IsEditing)
        {
            EndDrag(null);
            SetHover(null, null, -1, null);
            EndDrawing();
            return false;
        }

        // Adding grapples and drawing chains (M4, EdgeEditorTool.Authoring)
        if (Mode != EdgeEditMode.Select)
        {
            EndDrag(null);
            return HandleAuthoringInput(viewport, ray, canPick);
        }

        EndDrawing();

        var mouse = InputManager.MousePosition;
        var direction = Vector3.Normalize(ray.Direction);

        // A press on a handle: a drag once the mouse moves, ended by letting go
        if (Pressed)
        {
            TryGetContext(out var context);

            if (context != null && InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action))
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
                EndDrag(context);
            }

            if (context != null && TryGetContext(out var after))
            {
                SetHover(GetPicked(after.Handles), after.Part, -1, viewport.RenderScene);
            }

            return true;
        }

        var hit = PickAll(ray.Origin, direction);

        SetHover(hit.Handle, hit.Part, hit.Segment, viewport.RenderScene);

        if (canPick && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
        {
            if (hit.Handle != null)
            {
                SetActivePart(hit.Part);

                PickedKind = hit.Handle.Kind;
                PickedPoint = hit.Handle.Points[0];
                SelectEntry(hit.Handle.Entry);

                Pressed = true;
                Dragging = false;
                PressMouse = mouse;
                PressDistance = hit.Distance;
                DragBefore = GetHandles(hit.Part) is { } cached ? EdgeBank.Copy(cached.Edge) : null;
            }
            else if (hit.Segment >= 0)
            {
                SetActivePart(hit.Part);

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

    private record struct PickHit(MsbEntity Part, EdgeHandle Handle, int Segment, float Distance);

    /// <summary>
    /// The nearest handle under the ray over every displayed collision, or else the nearest ledge or wall segment.
    /// Handles win over segments, so a chain's points can be picked where its segments meet.
    /// </summary>
    private PickHit PickAll(Vector3 origin, Vector3 direction)
    {
        var pointRadius = MathF.Max(CFG.Current.Viewport_Edge_Point_Size * 1.6f, 0.08f);
        var segmentRadius = MathF.Max(CFG.Current.Viewport_Edge_Segment_Radius * 3.0f, 0.08f);

        PickHit handleHit = new(null, null, -1, float.MaxValue);
        PickHit segmentHit = new(null, null, -1, float.MaxValue);

        foreach (var part in View.EdgeBank.GetEditableParts())
        {
            var cached = GetHandles(part);

            if (cached == null)
                continue;

            var world = part.GetWorldMatrix();

            foreach (var handle in cached.Handles)
            {
                var centre = Vector3.Transform(handle.Position, world);
                var radius = handle.Kind is EdgeHandleKind.Pull ? pointRadius * 0.8f : pointRadius;

                if (EdgeHandles.RaySphere(origin, direction, centre, radius) is float t && t < handleHit.Distance)
                {
                    handleHit = new PickHit(part, handle, -1, t);
                }
            }

            // Segments only matter while no handle is hit
            if (handleHit.Handle != null)
                continue;

            for (var i = 0; i < cached.Edge.Edges.Count; i++)
            {
                var entry = cached.Edge.Edges[i];

                if (entry.Type is EDGE.EdgeType.Grapple || entry.V1 == entry.V2)
                    continue;

                var a = Vector3.Transform(entry.V1, world);
                var b = Vector3.Transform(entry.V2, world);

                if (EdgeHandles.RaySegment(origin, direction, a, b, segmentRadius) is float t && t < segmentHit.Distance)
                {
                    segmentHit = new PickHit(part, null, i, t);
                }
            }
        }

        return handleHit.Handle != null ? handleHit : segmentHit;
    }

    private EdgeHandle GetPicked(List<EdgeHandle> handles)
    {
        return PickedKind is EdgeHandleKind kind ? EdgeHandles.Find(handles, kind, PickedPoint) : null;
    }

    /// <summary>
    /// Moves the picked handle to the collision under the cursor, shown straight away; the move is recorded when the
    /// drag ends. Without the collision's triangles, the handle stays at the distance it was picked at.
    /// </summary>
    private void DragTo(EdgeContext context, Ray ray)
    {
        var handle = GetPicked(context.Handles);
        var world = context.Part.GetWorldMatrix();

        if (handle == null || !Matrix4x4.Invert(world, out var toPart))
            return;

        var origin = Vector3.Transform(ray.Origin, toPart);
        var direction = Vector3.TransformNormal(ray.Direction, toPart);
        var length = direction.Length();

        if (length < 1e-8f)
            return;

        direction /= length;

        Vector3 local;

        if (RayCastSurface(context, new Ray(origin, direction), out var hit))
        {
            local = hit;
        }
        else
        {
            // The press distance is in world units: the part's scale turns it into its own
            var worldPoint = ray.Origin + Vector3.Normalize(ray.Direction) * PressDistance;
            local = Vector3.Transform(worldPoint, toPart);
        }

        Preview(context, EdgeHandles.Move(context.Edge, handle, local));
    }

    /// <summary>Records a drag that moved anything as one action, and lets go of the press.</summary>
    private void EndDrag(EdgeContext context)
    {
        if (Pressed && Dragging && context != null && DragBefore != null)
        {
            var after = View.EdgeBank.GetEdge(context.MapId, context.ModelName);

            if (after != null && !SameFile(DragBefore, after))
            {
                Execute(context.Map, context.ModelName, DragBefore, after,
                    LOC.Get("MAP_History_Edge_Move", context.ModelName));
            }
        }

        Pressed = false;
        Dragging = false;
        DragBefore = null;
    }

    private void Preview(EdgeContext context, EDGE moved)
    {
        View.EdgeBank.SetEdge(context.MapId, context.ModelName, moved);
        View.EdgeBank.RebuildModel(context.MapId, context.ModelName);
    }

    private static bool SameFile(EDGE a, EDGE b) => a.Write().AsSpan().SequenceEqual(b.Write());

    #endregion

    #region Surface

    /// <summary>
    /// The nearest point of the current collision under a ray in the part's space. The high collision is used, which
    /// EDGE points lie on, or the low one if only it is loaded.
    /// </summary>
    private bool RayCastSurface(EdgeContext context, Ray ray, out Vector3 point)
        => RayCastSurface(context.MapId, context.ModelName, ray, out point);

    private bool RayCastSurface(string mapId, string modelName, Ray ray, out Vector3 point)
    {
        point = default;

        var surface = GetSurface(mapId, modelName);

        if (surface == null)
            return false;

        var nearest = float.MaxValue;

        foreach (var geometry in surface)
        {
            if (geometry.RayCast(ray, out _, out var distance) && distance < nearest)
            {
                nearest = distance;
            }
        }

        if (nearest == float.MaxValue)
            return false;

        point = ray.Origin + ray.Direction * nearest;
        return true;
    }

    private List<WallGeometry> GetSurface(EdgeContext context) => GetSurface(context.MapId, context.ModelName);

    private List<WallGeometry> GetSurface(string mapId, string modelName)
    {
        if (string.IsNullOrEmpty(modelName) || modelName.Length < 2)
            return null;

        var number = modelName.Substring(1);

        foreach (var type in new[] { "h", "l" })
        {
            // e.g. h10_00_00_00_000100.hkx.dcx for h000100
            var key = $"{type}{mapId.Substring(1)}_{number}.hkx.dcx";
            var document = View.HavokCollisionBank.GetTagDocument(key);

            if (document == null)
                continue;

            // Kept while the collision bank holds the same document; a reloaded or edited collision is built again
            if (Surfaces.TryGetValue(key, out var cached) && ReferenceEquals(cached.Document, document))
                return cached.Geometry;

            var geometry = CollisionGeometry.Bodies(document.Objects)
                .Where(b => b.Shape is IHavokCompressedMesh)
                .Select(b => WallGeometry.Build((IHavokCompressedMesh)b.Shape, b))
                .ToList();

            if (geometry.Count == 0)
                continue;

            // Collisions of maps unloaded since aren't looked up again; dropped once many are kept
            if (Surfaces.Count >= 64)
                Surfaces.Clear();

            Surfaces[key] = (document, geometry);
            return geometry;
        }

        // Not loaded yet: tried again later
        return null;
    }

    #endregion

    #region Gizmo

    /// <summary>Where the gizmo sits: on the picked handle, in the world.</summary>
    public bool TryGetGizmoPosition(out Vector3 position)
    {
        position = default;

        if (!IsEditing || Mode != EdgeEditMode.Select || Pressed || PickedKind == null || !TryGetContext(out var context))
            return false;

        var handle = GetPicked(context.Handles);

        if (handle == null)
            return false;

        position = Vector3.Transform(handle.Position, context.Part.GetWorldMatrix());
        return true;
    }

    /// <summary>
    /// Moves the picked handle to a point in the world for the gizmo: shown while it is dragged, recorded as one
    /// action when it is let go.
    /// </summary>
    public void GizmoMove(Vector3 position, bool commit)
    {
        if (!TryGetContext(out var context) || !Matrix4x4.Invert(context.Part.GetWorldMatrix(), out var toPart))
            return;

        var handle = GetPicked(context.Handles);

        if (handle == null)
            return;

        GizmoBefore ??= EdgeBank.Copy(context.Edge);

        var moved = EdgeHandles.Move(context.Edge, handle, Vector3.Transform(position, toPart));

        Preview(context, moved);

        if (commit)
        {
            if (!SameFile(GizmoBefore, moved))
            {
                Execute(context.Map, context.ModelName, GizmoBefore, moved,
                    LOC.Get("MAP_History_Edge_Move", context.ModelName));
            }

            GizmoBefore = null;
        }
    }

    #endregion

    #region Hover

    private void SetHover(EdgeHandle handle, MsbEntity part, int segment, RenderScene scene)
    {
        Hovered = handle;
        HoveredPart = part;
        HoveredSegment = segment;

        if (scene != HoverScene)
        {
            HoverMesh?.Dispose();
            HoverMesh = null;
            HoverScene = scene;
        }

        if (handle == null || part == null || scene == null)
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
        var position = Vector3.Transform(handle.Position, part.GetWorldMatrix());

        HoverMesh.World = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(position);
        HoverMesh.Visible = true;
    }

    private string DescribeHandle(EdgeHandle handle)
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

    /// <summary>The Edit in Viewport toggle and what the cursor is over. Shown whether or not a collision is chosen.</summary>
    private void DisplayViewportEditing()
    {
        LastDrawnFrame = ImGui.GetFrameCount();

        // Viewport
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_EdgeEditor_Viewport_Header"), LOC.Get("MAP_EdgeEditor_Viewport_Header_TT"));

        GUI.MultiButtonInput("edgeEditorEditToggle",
            "edgeEditorEditToggleButton",
            Editing ? LOC.Get("MAP_EdgeEditor_Stop_Editing") : LOC.Get("MAP_EdgeEditor_Start_Editing"),
            LOC.Get("MAP_EdgeEditor_Editing_TT"),
            () =>
            {
                Editing = !Editing;
                PickedKind = null;
                EndDrag(null);
                EndDrawing();
            });

        if (!Editing)
            return;

        DisplayModes();

        if (!View.EdgeBank.Visible)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_EdgeEditor_Editing_Needs_Points"));
        }
        else if (TryGetContext(out var context) && GetSurface(context) == null)
        {
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_No_Surface"));
        }

        if (Mode != EdgeEditMode.Select)
        {
            DisplayAuthoringStatus();
        }
        else if (Hovered != null && HoveredPart != null)
        {
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Hovered", DescribeHandle(Hovered), HoveredPart.Name));
        }
        else if (HoveredSegment >= 0 && HoveredPart != null)
        {
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Hovered_Segment", HoveredSegment, HoveredPart.Name));
        }
        else
        {
            // Always draw the line, so the panel below doesn't jump as the cursor moves on and off handles
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Hovered_None"));
        }
    }

    /// <summary>The picked handle of the current collision, shown with its file.</summary>
    private void DisplayPicked()
    {
        if (Editing && TryGetContext(out var context) && GetPicked(context.Handles) is { } picked)
        {
            GUI.WrappedText(LOC.Get("MAP_EdgeEditor_Picked", DescribeHandle(picked)));
        }
    }

    #endregion
}

/// <summary>
/// The gizmo on the EDGE Editor's picked handle. Only moves: a point has no rotation or scale.
/// </summary>
public class EdgeGizmoTarget : IGizmoTarget
{
    private readonly EdgeEditorTool Tool;

    public EdgeGizmoTarget(EdgeEditorTool tool)
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
