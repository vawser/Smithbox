using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>What a click in the viewport does while editing EDGE points.</summary>
public enum EdgeEditMode
{
    [Display(Name = "MAP_EdgeEditor_Mode_Select")]
    Select,
    [Display(Name = "MAP_EdgeEditor_Mode_AddGrapple")]
    AddGrapple,
    [Display(Name = "MAP_EdgeEditor_Mode_DrawHang")]
    DrawHang,
    [Display(Name = "MAP_EdgeEditor_Mode_DrawHug")]
    DrawHug
}

/// <summary>
/// Authoring in the viewport (M4): adding grapple points and drawing ledges and walls on any shown collision, with the
/// rules of <see cref="EdgeAuthoring"/>. New points go on the collision under the cursor (its triangles must be loaded)
/// or on a displayed point, and are stored relative to the collision part they belong to. A collision without an EDGE
/// file gets one with its first point. Every click is one <see cref="EdgeEditAction"/>.
///
/// Drawing: the first click starts a chain (or, on a chain end of the same type, continues that chain); each further
/// click adds a segment; a click on another chain's opposite end joins the two and stops. Escape, Enter or Finish
/// stops drawing.
/// </summary>
public partial class EdgeEditorTool
{
    private EdgeEditMode Mode = EdgeEditMode.Select;

    // The chain being drawn: its collision part, and either the first point (before the first segment) or the chain end
    // drawn from, by its point (V2 of the last segment from a tail, V1 of the first from a head)
    private MsbEntity DrawPart;
    private Vector3? DrawStart;
    private EdgePointRef? DrawEnd;

    // Why the last click did nothing, shown until the next one that does
    private string AuthoringMessage;

    // What the cursor is over this frame
    private MsbEntity TargetPart;

    // The new segment from the drawing end to the cursor, and the point being placed
    private DebugPrimitiveRenderableProxy PreviewLine;
    private DebugPrimitiveRenderableProxy PreviewPoint;
    private RenderScene PreviewScene;

    private bool IsDrawing => DrawPart != null;

    private EDGE.EdgeType DrawType => Mode == EdgeEditMode.DrawHang ? EDGE.EdgeType.Hang : EDGE.EdgeType.Hug;

    private record struct AuthoringTarget(MsbEntity Part, Vector3 World, EdgeHandle Handle, MsbEntity HandlePart);

    #region Input

    private bool HandleAuthoringInput(VulkanViewport viewport, Ray ray, bool canPick)
    {
        var direction = Vector3.Normalize(ray.Direction);
        var target = FindTarget(ray.Origin, direction);

        TargetPart = target.Part;

        // A displayed point under the cursor is snapped to, so it is outlined
        SetHover(target.Handle, target.HandlePart, -1, viewport.RenderScene);

        if (IsDrawing && DrawPart.Parent == null)
        {
            EndDrawing();
        }

        if (IsDrawing && (InputManager.IsKeyPressed(Key.Escape) || InputManager.IsKeyPressed(Key.Enter)))
        {
            EndDrawing();
        }

        UpdatePreview(viewport.RenderScene, target);

        if (canPick && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
        {
            if (Mode == EdgeEditMode.AddGrapple)
                PlaceGrapple(target);
            else
                DrawClick(target);
        }

        return true;
    }

    /// <summary>
    /// Where a click would put a point: a displayed point under the cursor if it is in front, else the nearest shown
    /// collision under it.
    /// </summary>
    private AuthoringTarget FindTarget(Vector3 origin, Vector3 direction)
    {
        var pick = PickAll(origin, direction);
        var surface = RayCastCollisions(origin, direction, out var surfacePart, out var surfaceDistance);

        if (pick.Handle != null && pick.Part != null && pick.Distance <= surfaceDistance + 0.05f)
        {
            var world = Vector3.Transform(pick.Handle.Position, pick.Part.GetWorldMatrix());
            return new AuthoringTarget(pick.Part, world, pick.Handle, pick.Part);
        }

        return surfacePart != null ? new AuthoringTarget(surfacePart, surface, null, null) : default;
    }

    /// <summary>The nearest point of any shown collision under a world ray, in the world.</summary>
    private Vector3 RayCastCollisions(Vector3 origin, Vector3 direction, out MsbEntity part, out float distance)
    {
        part = null;
        distance = float.MaxValue;
        var nearest = Vector3.Zero;
        var worldRay = new Ray(origin, direction);

        foreach (var collision in View.EdgeBank.GetCollisionParts())
        {
            if (collision.Container is not MapContainer map || !worldRay.Intersects(collision.RenderSceneMesh.GetBounds()))
                continue;

            var world = collision.GetWorldMatrix();

            if (!Matrix4x4.Invert(world, out var toPart))
                continue;

            var localDirection = Vector3.TransformNormal(direction, toPart);

            if (localDirection.LengthSquared() < 1e-16f)
                continue;

            var localRay = new Ray(Vector3.Transform(origin, toPart), Vector3.Normalize(localDirection));

            if (!RayCastSurface(map.Name, collision.GetPartModelName(), localRay, out var local))
                continue;

            var hit = Vector3.Transform(local, world);
            var d = Vector3.Distance(origin, hit);

            if (d < distance)
            {
                distance = d;
                nearest = hit;
                part = collision;
            }
        }

        return nearest;
    }

    #endregion

    #region Placing

    /// <summary>A collision part's file as it is, and the one to add to: a new empty file when it has none.</summary>
    private bool TryGetTargetFile(MsbEntity part, out MapContainer map, out string modelName, out EDGE before, out EDGE file)
    {
        map = part?.Container as MapContainer;
        modelName = part?.GetPartModelName();
        before = null;
        file = null;

        if (map == null || string.IsNullOrEmpty(modelName) || modelName.Length < 2)
            return false;

        if (!View.EdgeBank.EnsureMapLoaded(map.Name))
        {
            AuthoringMessage = LOC.Get("MAP_EdgeEditor_No_Binder", map.Name);
            return false;
        }

        before = View.EdgeBank.GetEdge(map.Name, modelName);
        file = before ?? EdgeAuthoring.CreateFile(modelName);

        return file != null;
    }

    private static Vector3 ToPart(MsbEntity part, Vector3 world)
    {
        return Matrix4x4.Invert(part.GetWorldMatrix(), out var toPart) ? Vector3.Transform(world, toPart) : world;
    }

    /// <summary>The world's up in a part's space, for grapple directions and pull points.</summary>
    private static Vector3 PartUp(MsbEntity part)
    {
        if (!Matrix4x4.Invert(part.GetWorldMatrix(), out var toPart))
            return Vector3.UnitY;

        var up = Vector3.TransformNormal(Vector3.UnitY, toPart);

        return up.LengthSquared() < 1e-12f ? Vector3.UnitY : Vector3.Normalize(up);
    }

    private void PlaceGrapple(AuthoringTarget target)
    {
        if (target.Part == null)
        {
            AuthoringMessage = LOC.Get("MAP_EdgeEditor_No_Target");
            return;
        }

        if (!TryGetTargetFile(target.Part, out var map, out var modelName, out var before, out var file))
            return;

        var after = EdgeAuthoring.AddGrapple(file, ToPart(target.Part, target.World), PartUp(target.Part), out var index);

        Execute(map, modelName, before, after, LOC.Get("MAP_History_Edge_Add_Grapple", modelName));

        SetActivePart(target.Part);
        SelectEntry(index);
        PickedKind = EdgeHandleKind.Grapple;
        PickedPoint = new EdgePointRef(index, 1);
        AuthoringMessage = null;
    }

    private void DrawClick(AuthoringTarget target)
    {
        if (target.Part == null)
        {
            AuthoringMessage = LOC.Get("MAP_EdgeEditor_No_Target");
            return;
        }

        if (!IsDrawing)
        {
            StartDrawing(target);
            return;
        }

        if (!TryGetTargetFile(DrawPart, out var map, out var modelName, out var before, out var file))
        {
            EndDrawing();
            return;
        }

        var local = ToPart(DrawPart, target.World);

        // The first segment: a new chain
        if (DrawStart is Vector3 start)
        {
            if (Vector3.Distance(start, local) < 1e-4f)
                return;

            var created = EdgeAuthoring.AddChain(file, DrawType, start, local, out var created_index);

            if (created == null)
                return;

            Execute(map, modelName, before, created, LOC.Get("MAP_History_Edge_Add_Chain", LOC.Get($"MAP_EdgeEditor_Type_{DrawType}"), modelName));

            DrawStart = null;
            DrawEnd = new EdgePointRef(created_index, 2);
            SelectEntry(created_index);
            AuthoringMessage = null;
            return;
        }

        var end = GetDrawEndHandle();

        if (end == null || before == null)
        {
            // The chain changed under it, e.g. by an undo
            EndDrawing();
            AuthoringMessage = LOC.Get("MAP_EdgeEditor_Drawing_Lost");
            return;
        }

        var fromTail = EdgeAuthoring.GetChainEnd(end) == ChainEnd.Tail;

        // Onto the other end of a chain of the same type: join them and stop
        if (target.Handle != null && target.HandlePart == DrawPart && target.Handle.Kind == EdgeHandleKind.Vertex
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
                    Execute(map, modelName, before, joined, LOC.Get("MAP_History_Edge_Join", modelName));
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

        Execute(map, modelName, before, extended, LOC.Get("MAP_History_Edge_Extend", modelName));

        DrawEnd = new EdgePointRef(index, fromTail ? 2 : 1);
        SelectEntry(index);
        AuthoringMessage = null;
    }

    /// <summary>
    /// The first click of a chain: a chain end of the type being drawn is continued, anything else is the first point.
    /// </summary>
    private void StartDrawing(AuthoringTarget target)
    {
        if (!TryGetTargetFile(target.Part, out _, out _, out var before, out _))
            return;

        SetActivePart(target.Part);
        DrawPart = target.Part;
        DrawStart = null;
        DrawEnd = null;
        AuthoringMessage = null;

        if (target.Handle != null && target.HandlePart == target.Part && before != null
            && target.Handle.Entry < before.Edges.Count && before.Edges[target.Handle.Entry].Type == DrawType
            && EdgeAuthoring.GetChainEnd(target.Handle) != ChainEnd.None)
        {
            DrawEnd = target.Handle.Points[0];
            SelectEntry(target.Handle.Entry);
            return;
        }

        DrawStart = ToPart(target.Part, target.World);
    }

    private EdgeHandle GetDrawEndHandle()
    {
        if (DrawEnd is not EdgePointRef point || GetHandles(DrawPart) is not { } cached)
            return null;

        var handle = EdgeHandles.Find(cached.Handles, EdgeHandleKind.Vertex, point);

        return handle != null && EdgeAuthoring.GetChainEnd(handle) != ChainEnd.None ? handle : null;
    }

    private void EndDrawing()
    {
        DrawPart = null;
        DrawStart = null;
        DrawEnd = null;

        if (PreviewLine != null) PreviewLine.Visible = false;
        if (PreviewPoint != null) PreviewPoint.Visible = false;
    }

    #endregion

    #region Preview

    /// <summary>Where the drawing is from, in the world: the first point, or the chain end.</summary>
    private Vector3? GetDrawFromWorld()
    {
        if (!IsDrawing)
            return null;

        var world = DrawPart.GetWorldMatrix();

        if (DrawStart is Vector3 start)
            return Vector3.Transform(start, world);

        return GetDrawEndHandle() is { } end ? Vector3.Transform(end.Position, world) : null;
    }

    private void UpdatePreview(RenderScene scene, AuthoringTarget target)
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
        var from = GetDrawFromWorld();

        // The point being placed: the grapple under the cursor, or where the chain is drawn from
        Vector3? point = Mode == EdgeEditMode.AddGrapple
            ? (target.Part != null ? target.World : null)
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

        if (from is Vector3 f && target.Part != null && Vector3.Distance(f, target.World) > 1e-3f)
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
        if (ImGui.BeginCombo("##edgeEditorMode", LOC.Get(Mode.GetDisplayName())))
        {
            foreach (var value in Enum.GetValues<EdgeEditMode>())
            {
                if (ImGui.Selectable($"{LOC.Get(value.GetDisplayName())}##edgeEditorMode{value}", value == Mode) && value != Mode)
                {
                    Mode = value;
                    PickedKind = null;
                    AuthoringMessage = null;
                    EndDrag(null);
                    EndDrawing();
                }
                GUI.Tooltip(LOC.Get($"MAP_EdgeEditor_Mode_{value}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_EdgeEditor_Mode_TT"));

        GUI.WrappedText(LOC.Get($"MAP_EdgeEditor_Mode_{Mode}_TT"));
    }

    /// <summary>What a click does next, the collision under the cursor, and why the last click did nothing.</summary>
    private void DisplayAuthoringStatus()
    {
        if (Mode != EdgeEditMode.AddGrapple)
        {
            var key = !IsDrawing ? "MAP_EdgeEditor_Drawing_Start"
                : DrawStart != null ? "MAP_EdgeEditor_Drawing_First"
                : "MAP_EdgeEditor_Drawing_Next";

            GUI.WrappedText(LOC.Get(key));

            if (IsDrawing)
            {
                GUI.MultiButtonInput("edgeEditorDrawing",
                    "edgeEditorFinishDrawing", LOC.Get("MAP_EdgeEditor_Finish_Drawing"), LOC.Get("MAP_EdgeEditor_Finish_Drawing_TT"),
                    EndDrawing);
            }
        }

        if (AuthoringMessage != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, AuthoringMessage);
        }

        // Always drawn, so the panel doesn't jump
        GUI.WrappedText(TargetPart != null
            ? LOC.Get("MAP_EdgeEditor_Target", TargetPart.Name)
            : LOC.Get("MAP_EdgeEditor_Hovered_None"));
    }

    /// <summary>
    /// The authoring actions of the selected entry and picked point, under the entry actions: split a segment, delete
    /// its chain, delete the picked point, give or take a grapple's pull point.
    /// </summary>
    private void DisplayAuthoringActions(MsbEntity part, MapContainer map, string modelName, EDGE edge)
    {
        var index = SelectedIndex;
        var hasEntry = index >= 0 && index < edge.Edges.Count;
        var isSegment = hasEntry && EdgeAuthoring.IsSegment(edge.Edges[index]);
        var isGrapple = hasEntry && edge.Edges[index].Type == EDGE.EdgeType.Grapple;
        var hasPull = isGrapple && edge.Edges[index].V3 != Vector3.Zero;

        GUI.ConditionalMultiButtonInput("edgeEditorChainActions",
            "edgeEditorSplit", LOC.Get("MAP_EdgeEditor_Split"), LOC.Get("MAP_EdgeEditor_Split_TT"),
            () =>
            {
                var after = EdgeAuthoring.Split(edge, index);

                if (after != null)
                {
                    Execute(map, modelName, edge, after, LOC.Get("MAP_History_Edge_Split", index, modelName));
                    SelectEntry(index + 1);
                }
            }, isSegment && edge.Edges[index].V1 != edge.Edges[index].V2,

            "edgeEditorDeleteChain", LOC.Get("MAP_EdgeEditor_Delete_Chain"), LOC.Get("MAP_EdgeEditor_Delete_Chain_TT"),
            () =>
            {
                var after = EdgeAuthoring.DeleteChain(edge, index);

                if (after != null)
                {
                    Execute(map, modelName, edge, after, LOC.Get("MAP_History_Edge_Delete_Chain", edge.Edges[index].Unk30, modelName));
                    SelectEntry(Math.Min(index, after.Edges.Count - 1));
                    PickedKind = null;
                }
            }, isSegment);

        var picked = Editing && Mode == EdgeEditMode.Select && TryGetContext(out var context) && context.Part == part
            ? GetPicked(context.Handles)
            : null;

        GUI.ConditionalMultiButtonInput("edgeEditorPointActions",
            "edgeEditorDeletePoint", LOC.Get("MAP_EdgeEditor_Delete_Point"), LOC.Get("MAP_EdgeEditor_Delete_Point_TT"),
            () =>
            {
                var after = EdgeAuthoring.DeletePoint(edge, picked);

                if (after != null)
                {
                    Execute(map, modelName, edge, after, LOC.Get("MAP_History_Edge_Delete_Point", modelName));
                    SelectEntry(Math.Min(picked.Entry, after.Edges.Count - 1));
                    PickedKind = null;
                }
            }, picked != null && (picked.Kind != EdgeHandleKind.Vertex || picked.Points.Count <= 2),

            "edgeEditorPull", LOC.Get(hasPull ? "MAP_EdgeEditor_Remove_Pull" : "MAP_EdgeEditor_Add_Pull"),
            LOC.Get(hasPull ? "MAP_EdgeEditor_Remove_Pull_TT" : "MAP_EdgeEditor_Add_Pull_TT"),
            () =>
            {
                var after = EdgeAuthoring.SetPullPoint(edge, index, !hasPull, PartUp(part));

                if (after != null)
                {
                    Execute(map, modelName, edge, after,
                        LOC.Get(hasPull ? "MAP_History_Edge_Remove_Pull" : "MAP_History_Edge_Add_Pull", index, modelName));

                    if (hasPull && PickedKind == EdgeHandleKind.Pull)
                        PickedKind = null;
                }
            }, isGrapple);
    }

    /// <summary>A collision without an EDGE file: make an empty one, with the collision's number as its ID.</summary>
    private void DisplayCreateFile(MapContainer map, string modelName)
    {
        GUI.ConditionalMultiButtonInput("edgeEditorCreateFile",
            "edgeEditorCreate", LOC.Get("MAP_EdgeEditor_Create_File"), LOC.Get("MAP_EdgeEditor_Create_File_TT"),
            () =>
            {
                var file = EdgeAuthoring.CreateFile(modelName);

                if (file != null)
                {
                    Execute(map, modelName, null, file, LOC.Get("MAP_History_Edge_Create_File", modelName));
                }
            }, EdgeAuthoring.CreateFile(modelName) != null);
    }

    #endregion
}
