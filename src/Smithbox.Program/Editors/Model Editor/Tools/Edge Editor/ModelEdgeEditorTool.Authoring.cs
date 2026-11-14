using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Authoring in the viewport, with the Map Editor EDGE Editor's modes and the rules of <see cref="EdgeAuthoring"/>:
/// adding grapple points and drawing ledges and walls on the model's surface (or on a displayed point), in the object's
/// space. An object without an EDGE file gets one with its first point. Every click is one
/// <see cref="ModelEdgeEditAction"/>.
/// <para>
/// New entries follow the game's object files (EdgeSurvey objects, <see cref="EdgeAuthoring.ObjectDefaults"/>), not its
/// map files: a grapple gets Variation 1, a pull point (Has Pull Point 1) and a sideways direction point, facing the
/// camera; ledges and walls Variation 0.
/// </para>
/// </summary>
public partial class ModelEdgeEditorTool
{
    private EdgeEditMode Mode = EdgeEditMode.Select;

    // The chain being drawn: either the first point (before the first segment) or the chain end drawn from
    private ModelWrapper DrawWrapper;
    private Vector3? DrawStart;
    private EdgePointRef? DrawEnd;

    // Why the last click did nothing, shown until the next one that does
    private string AuthoringMessage;

    // Whether the cursor is over something to place on this frame
    private bool HasTarget;

    // The new segment from the drawing end to the cursor, and the point being placed
    private DebugPrimitiveRenderableProxy PreviewLine;
    private DebugPrimitiveRenderableProxy PreviewPoint;
    private RenderScene PreviewScene;

    private bool IsDrawing => DrawWrapper != null;

    private EDGE.EdgeType DrawType => Mode == EdgeEditMode.DrawHang ? EDGE.EdgeType.Hang : EDGE.EdgeType.Hug;

    /// <summary>Where a click would put a point, in the viewport and in the EDGE's space, and the handle snapped to.</summary>
    private record struct AuthoringTarget(bool Valid, Vector3 World, Vector3 Local, EdgeHandle Handle, Vector3 ViewDirection);

    #region Input

    private bool HandleAuthoringInput(VulkanViewport viewport, Ray ray, bool canPick)
    {
        var wrapper = GetWrapper();

        if (wrapper == null || !wrapper.CanEditEdge)
        {
            EndDrawing();
            return true;
        }

        var space = GetSpace(wrapper);

        if (!Matrix4x4.Invert(space, out var toEdge))
            return true;

        var direction = Vector3.Normalize(ray.Direction);
        var target = FindTarget(wrapper, space, toEdge, ray.Origin, direction);

        HasTarget = target.Valid;

        // A displayed point under the cursor is snapped to, so it is outlined
        SetHover(target.Handle, -1, viewport.RenderScene, space);

        if (IsDrawing && DrawWrapper != wrapper)
        {
            EndDrawing();
        }

        if (IsDrawing && (InputManager.IsKeyPressed(Key.Escape) || InputManager.IsKeyPressed(Key.Enter)))
        {
            EndDrawing();
        }

        UpdatePreview(viewport.RenderScene, wrapper, space, target);

        if (canPick && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
        {
            if (Mode == EdgeEditMode.AddGrapple)
                PlaceGrapple(wrapper, target);
            else
                DrawClick(wrapper, target);
        }

        return true;
    }

    /// <summary>
    /// Where a click would put a point: a displayed point under the cursor if it is in front, else the model's surface.
    /// </summary>
    private AuthoringTarget FindTarget(ModelWrapper wrapper, Matrix4x4 space, Matrix4x4 toEdge, Vector3 origin, Vector3 direction)
    {
        var viewDirection = Vector3.TransformNormal(direction, toEdge);

        PickHit pick = default;

        if (wrapper.EDGE != null)
        {
            var handles = GetHandles(wrapper.EDGE);
            pick = Pick(new EdgeContext(wrapper, wrapper.EDGE, handles, space, toEdge), origin, direction);
        }

        var onSurface = RayCastSurface(wrapper, toEdge, origin, direction, out var local);
        var surfaceWorld = onSurface ? Vector3.Transform(local, space) : Vector3.Zero;
        var surfaceDistance = onSurface ? Vector3.Distance(origin, surfaceWorld) : float.MaxValue;

        if (pick.Handle != null && pick.Distance <= surfaceDistance + 0.05f)
        {
            var world = Vector3.Transform(pick.Handle.Position, space);
            return new AuthoringTarget(true, world, pick.Handle.Position, pick.Handle, viewDirection);
        }

        return onSurface ? new AuthoringTarget(true, surfaceWorld, local, null, viewDirection) : default;
    }

    #endregion

    #region Placing

    /// <summary>The world's up in the EDGE's space, for pull points and fixes.</summary>
    private static Vector3 EdgeUp(ModelWrapper wrapper)
    {
        if (!Matrix4x4.Invert(GetSpace(wrapper), out var toEdge))
            return Vector3.UnitY;

        var up = Vector3.TransformNormal(Vector3.UnitY, toEdge);

        return up.LengthSquared() < 1e-12f ? Vector3.UnitY : Vector3.Normalize(up);
    }

    /// <summary>
    /// The way an object grapple's direction point lies: level (202 of 255 in the game's object files), facing back along
    /// the view so it faces where it was placed from; along the object's Z when there is no view or it looks straight down.
    /// </summary>
    private static Vector3 ObjectGrappleDirection(Vector3 up, Vector3? viewDirection)
    {
        Vector3 Level(Vector3 v) => v - up * Vector3.Dot(v, up);

        if (viewDirection is Vector3 view)
        {
            var facing = Level(-view);

            if (facing.LengthSquared() > 1e-6f)
                return Vector3.Normalize(facing);
        }

        var z = Level(Vector3.UnitZ);

        return z.LengthSquared() > 1e-6f ? Vector3.Normalize(z) : Vector3.Normalize(Level(Vector3.UnitX));
    }

    /// <summary>The model's file as it is, and the one to add to: a new empty file when it has none.</summary>
    private static bool TryGetTargetFile(ModelWrapper wrapper, out EDGE file)
    {
        file = wrapper.EDGE ?? EdgeAuthoring.CreateFile(wrapper.Name);
        return file != null;
    }

    private void PlaceGrapple(ModelWrapper wrapper, AuthoringTarget target)
    {
        if (!target.Valid)
        {
            AuthoringMessage = LOC.Get("MODEL_EdgeEditor_No_Target");
            return;
        }

        if (!TryGetTargetFile(wrapper, out var file))
            return;

        var up = EdgeUp(wrapper);
        var direction = ObjectGrappleDirection(up, target.ViewDirection);

        var added = EdgeAuthoring.AddGrapple(file, target.Local, direction, out var index, EdgeAuthoring.ObjectDefaults.GrappleVariation);

        // Object grapples have a pull point (188 of 255), which the gizmo can then move
        var after = EdgeAuthoring.SetPullPoint(added, index, true, up) ?? added;

        Execute(wrapper, after, LOC.Get("MAP_History_Edge_Add_Grapple", wrapper.Name));

        SelectEntry(index);
        PickedKind = EdgeHandleKind.Grapple;
        PickedPoint = new EdgePointRef(index, 1);
        AuthoringMessage = null;
    }

    private void DrawClick(ModelWrapper wrapper, AuthoringTarget target)
    {
        if (!target.Valid)
        {
            AuthoringMessage = LOC.Get("MODEL_EdgeEditor_No_Target");
            return;
        }

        if (!IsDrawing)
        {
            StartDrawing(wrapper, target);
            return;
        }

        if (!TryGetTargetFile(wrapper, out var file))
        {
            EndDrawing();
            return;
        }

        var local = target.Local;

        // The first segment: a new chain
        if (DrawStart is Vector3 start)
        {
            if (Vector3.Distance(start, local) < 1e-4f)
                return;

            var created = EdgeAuthoring.AddChain(file, DrawType, start, local, out var createdIndex,
                EdgeAuthoring.ObjectDefaults.Variation(DrawType));

            if (created == null)
                return;

            Execute(wrapper, created, LOC.Get("MAP_History_Edge_Add_Chain", LOC.Get($"MAP_EdgeEditor_Type_{DrawType}"), wrapper.Name));

            DrawStart = null;
            DrawEnd = new EdgePointRef(createdIndex, 2);
            SelectEntry(createdIndex);
            AuthoringMessage = null;
            return;
        }

        var before = wrapper.EDGE;
        var end = GetDrawEndHandle(wrapper);

        if (end == null || before == null)
        {
            // The chain changed under it, e.g. by an undo
            EndDrawing();
            AuthoringMessage = LOC.Get("MAP_EdgeEditor_Drawing_Lost");
            return;
        }

        var fromTail = EdgeAuthoring.GetChainEnd(end) == ChainEnd.Tail;

        // Onto the other end of a chain of the same type: join them and stop
        if (target.Handle != null && target.Handle.Kind == EdgeHandleKind.Vertex
            && target.Handle.Entry < before.Edges.Count && before.Edges[target.Handle.Entry].Type == DrawType)
        {
            var other = EdgeAuthoring.GetChainEnd(target.Handle);

            if (other == (fromTail ? ChainEnd.Head : ChainEnd.Tail))
            {
                var joined = fromTail
                    ? EdgeAuthoring.Join(before, end, target.Handle, out var joinIndex)
                    : EdgeAuthoring.Join(before, target.Handle, end, out joinIndex);

                if (joined != null)
                {
                    Execute(wrapper, joined, LOC.Get("MAP_History_Edge_Join", wrapper.Name));
                    SelectEntry(joinIndex);
                    EndDrawing();
                    AuthoringMessage = null;
                    return;
                }
            }
        }

        var extended = EdgeAuthoring.Extend(before, end, local, out var index);

        if (extended == null)
            return;

        Execute(wrapper, extended, LOC.Get("MAP_History_Edge_Extend", wrapper.Name));

        DrawEnd = new EdgePointRef(index, fromTail ? 2 : 1);
        SelectEntry(index);
        AuthoringMessage = null;
    }

    /// <summary>
    /// The first click of a chain: a chain end of the type being drawn is continued, anything else is the first point.
    /// </summary>
    private void StartDrawing(ModelWrapper wrapper, AuthoringTarget target)
    {
        DrawWrapper = wrapper;
        DrawStart = null;
        DrawEnd = null;
        AuthoringMessage = null;

        var before = wrapper.EDGE;

        if (target.Handle != null && before != null
            && target.Handle.Entry < before.Edges.Count && before.Edges[target.Handle.Entry].Type == DrawType
            && EdgeAuthoring.GetChainEnd(target.Handle) != ChainEnd.None)
        {
            DrawEnd = target.Handle.Points[0];
            SelectEntry(target.Handle.Entry);
            return;
        }

        DrawStart = target.Local;
    }

    private EdgeHandle GetDrawEndHandle(ModelWrapper wrapper)
    {
        if (DrawEnd is not EdgePointRef point || wrapper.EDGE == null)
            return null;

        var handle = EdgeHandles.Find(GetHandles(wrapper.EDGE), EdgeHandleKind.Vertex, point);

        return handle != null && EdgeAuthoring.GetChainEnd(handle) != ChainEnd.None ? handle : null;
    }

    private void EndDrawing()
    {
        DrawWrapper = null;
        DrawStart = null;
        DrawEnd = null;

        if (PreviewLine != null) PreviewLine.Visible = false;
        if (PreviewPoint != null) PreviewPoint.Visible = false;
    }

    #endregion

    #region Preview

    /// <summary>Where the drawing is from, in the viewport: the first point, or the chain end.</summary>
    private Vector3? GetDrawFromWorld(ModelWrapper wrapper, Matrix4x4 space)
    {
        if (!IsDrawing)
            return null;

        if (DrawStart is Vector3 start)
            return Vector3.Transform(start, space);

        return GetDrawEndHandle(wrapper) is { } end ? Vector3.Transform(end.Position, space) : null;
    }

    private void UpdatePreview(RenderScene scene, ModelWrapper wrapper, Matrix4x4 space, AuthoringTarget target)
    {
        if (scene != PreviewScene)
        {
            PreviewLine?.Dispose();
            PreviewPoint?.Dispose();
            PreviewLine = null;
            PreviewPoint = null;
            PreviewScene = scene;
        }

        if (scene == null)
            return;

        var color = Mode switch
        {
            EdgeEditMode.AddGrapple => CFG.Current.Viewport_Edge_Grapple_Color,
            EdgeEditMode.DrawHang => CFG.Current.Viewport_Edge_Hang_Color,
            _ => CFG.Current.Viewport_Edge_Hug_Color
        };

        if (PreviewLine == null)
        {
            PreviewLine = RenderableHelper.GetEdgeMarkerWireProxy(scene, true, color, true);
            PreviewLine.DrawFilter = RenderFilter.EdgePoint;
        }

        if (PreviewPoint == null)
        {
            PreviewPoint = RenderableHelper.GetEdgeMarkerWireProxy(scene, false, color, true);
            PreviewPoint.DrawFilter = RenderFilter.EdgePoint;
        }

        var size = MathF.Max(CFG.Current.Viewport_Edge_Point_Size, 0.01f);
        var from = GetDrawFromWorld(wrapper, space);

        // The point being placed: the grapple under the cursor, or where the chain is drawn from
        Vector3? point = Mode == EdgeEditMode.AddGrapple
            ? (target.Valid ? target.World : null)
            : from;

        if (point is Vector3 p)
        {
            PreviewPoint.World = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(p);
            PreviewPoint.Visible = true;
        }
        else
        {
            PreviewPoint.Visible = false;
        }

        if (from is Vector3 f && target.Valid && Vector3.Distance(f, target.World) > 1e-3f)
        {
            PreviewLine.World = MsbEntity.GetEdgeSegmentTransform(f, target.World, MathF.Max(CFG.Current.Viewport_Edge_Segment_Radius, 0.01f));
            PreviewLine.Visible = true;
        }
        else
        {
            PreviewLine.Visible = false;
        }
    }

    #endregion

    #region Tool Window

    private void DisplayModes()
    {
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##modelEdgeEditorMode", LOC.Get(Mode.GetDisplayName())))
        {
            foreach (var value in Enum.GetValues<EdgeEditMode>())
            {
                if (ImGui.Selectable($"{LOC.Get(value.GetDisplayName())}##modelEdgeEditorMode{value}", value == Mode) && value != Mode)
                {
                    Mode = value;
                    PickedKind = null;
                    AuthoringMessage = null;
                    EndDrag();
                    EndDrawing();
                }
                GUI.Tooltip(LOC.Get($"MODEL_EdgeEditor_Mode_{value}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_EdgeEditor_Mode_TT"));

        GUI.WrappedText(LOC.Get($"MODEL_EdgeEditor_Mode_{Mode}_TT"));
    }

    /// <summary>What a click does next, whether the cursor is over the model, and why the last click did nothing.</summary>
    private void DisplayAuthoringStatus()
    {
        if (Mode != EdgeEditMode.AddGrapple)
        {
            var key = !IsDrawing ? "MODEL_EdgeEditor_Drawing_Start"
                : DrawStart != null ? "MAP_EdgeEditor_Drawing_First"
                : "MAP_EdgeEditor_Drawing_Next";

            GUI.WrappedText(LOC.Get(key));

            if (IsDrawing)
            {
                GUI.MultiButtonInput("modelEdgeEditorDrawing",
                    "modelEdgeEditorFinishDrawing", LOC.Get("MAP_EdgeEditor_Finish_Drawing"), LOC.Get("MAP_EdgeEditor_Finish_Drawing_TT"),
                    EndDrawing);
            }
        }

        if (AuthoringMessage != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, AuthoringMessage);
        }

        // Always drawn, so the panel doesn't jump
        GUI.WrappedText(LOC.Get(HasTarget ? "MODEL_EdgeEditor_Target" : "MAP_EdgeEditor_Hovered_None"));
    }

    /// <summary>
    /// The authoring actions of the selected entry and picked point, under the entry actions: split a segment, delete
    /// its chain, delete the picked point, give or take a grapple's pull point.
    /// </summary>
    private void DisplayAuthoringActions(ModelWrapper wrapper, EDGE edge)
    {
        var index = SelectedIndex;
        var hasEntry = index >= 0 && index < edge.Edges.Count;
        var isSegment = hasEntry && EdgeAuthoring.IsSegment(edge.Edges[index]);
        var isGrapple = hasEntry && edge.Edges[index].Type == EDGE.EdgeType.Grapple;
        var hasPull = isGrapple && edge.Edges[index].V3 != Vector3.Zero;

        GUI.ConditionalMultiButtonInput("modelEdgeEditorChainActions",
            "modelEdgeEditorSplit", LOC.Get("MAP_EdgeEditor_Split"), LOC.Get("MAP_EdgeEditor_Split_TT"),
            () =>
            {
                var after = EdgeAuthoring.Split(edge, index);

                if (after != null)
                {
                    Execute(wrapper, after, LOC.Get("MAP_History_Edge_Split", index, wrapper.Name));
                    SelectEntry(index + 1);
                }
            }, isSegment && edge.Edges[index].V1 != edge.Edges[index].V2,

            "modelEdgeEditorDeleteChain", LOC.Get("MAP_EdgeEditor_Delete_Chain"), LOC.Get("MAP_EdgeEditor_Delete_Chain_TT"),
            () =>
            {
                var chain = edge.Edges[index].Unk30;
                var after = EdgeAuthoring.DeleteChain(edge, index);

                if (after != null)
                {
                    Execute(wrapper, after, LOC.Get("MAP_History_Edge_Delete_Chain", chain, wrapper.Name));
                    SelectEntry(Math.Min(index, after.Edges.Count - 1));
                    PickedKind = null;
                }
            }, isSegment);

        var picked = Editing && Mode == EdgeEditMode.Select ? GetPicked(GetHandles(edge)) : null;

        GUI.ConditionalMultiButtonInput("modelEdgeEditorPointActions",
            "modelEdgeEditorDeletePoint", LOC.Get("MAP_EdgeEditor_Delete_Point"), LOC.Get("MAP_EdgeEditor_Delete_Point_TT"),
            () =>
            {
                var after = EdgeAuthoring.DeletePoint(edge, picked);

                if (after != null)
                {
                    Execute(wrapper, after, LOC.Get("MAP_History_Edge_Delete_Point", wrapper.Name));
                    SelectEntry(Math.Min(picked.Entry, after.Edges.Count - 1));
                    PickedKind = null;
                }
            }, picked != null && (picked.Kind != EdgeHandleKind.Vertex || picked.Points.Count <= 2),

            "modelEdgeEditorPull", LOC.Get(hasPull ? "MAP_EdgeEditor_Remove_Pull" : "MAP_EdgeEditor_Add_Pull"),
            LOC.Get(hasPull ? "MAP_EdgeEditor_Remove_Pull_TT" : "MODEL_EdgeEditor_Add_Pull_TT"),
            () =>
            {
                var after = EdgeAuthoring.SetPullPoint(edge, index, !hasPull, EdgeUp(wrapper));

                if (after != null)
                {
                    Execute(wrapper, after,
                        LOC.Get(hasPull ? "MAP_History_Edge_Remove_Pull" : "MAP_History_Edge_Add_Pull", index, wrapper.Name));

                    if (hasPull && PickedKind == EdgeHandleKind.Pull)
                        PickedKind = null;
                }
            }, isGrapple);
    }

    #endregion
}
