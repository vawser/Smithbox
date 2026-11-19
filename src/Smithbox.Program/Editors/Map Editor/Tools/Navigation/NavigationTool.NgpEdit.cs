using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The NGP mode's Edit tab (Phase G3): moves NGP vertices with the Havok and NVM modes' brush (Raise, Lower, Drag,
/// Smooth; the same keys, radius, step, interval and cursor). Other meshes' vertices under the brush follow too when
/// linked, so the edges gates use stay joined. Edits are drawn in the overlay until committed, which builds the NGP again
/// with the moved vertices (NgpEdit.WithVertices: topology kept; the moved meshes' bounding boxes and trees, and the
/// map's gates, links and IDs rebuilt) and swaps it in as one undoable action.
/// </summary>
public partial class NavigationTool
{
    private bool NgpVertexActive;
    private MapContainer NgpVertexMap;

    /// <summary>The working copies, by mesh index.</summary>
    private readonly Dictionary<int, NgpVertexCopy> NgpVertexCopies = new();

    private sealed record NgpVertexHit(NgpVertexCopy Copy, int Face, int Vertex, Vector3 Point, float Distance);
    private sealed record NgpVertexMove(NgpVertexCopy Copy, List<(int Vertex, float Weight)> Falloff);

    private NgpVertexHit NgpVertexHover;
    private NgpVertexHit NgpVertexStroke;
    private CollisionBrushMode NgpVertexStrokeMode;
    private long NgpVertexNextApply;
    private List<NgpVertexMove> NgpVertexDragMoves;
    private float NgpVertexDragHeight;
    private Vector3 NgpVertexDragLast;
    private Vector3 NgpVertexDragAnchor;
    private int NgpOverlayRevision = -1;

    private bool IsNgpDragStroke => NgpVertexStroke != null && NgpVertexStrokeMode is CollisionBrushMode.Drag;

    private bool HasNgpVertexEdits => NgpVertexCopies.Values.Any(c => c.IsEdited);

    /// <summary>Changes with every move of any copy, so the overlay knows to be drawn again.</summary>
    private int NgpVertexRevision => NgpVertexCopies.Values.Sum(c => c.Revision) + NgpVertexCopies.Count * 7919;

    /// <summary>A mesh's vertices as edited, or null to draw its own.</summary>
    private IReadOnlyList<Vector3> NgpEditedVertices(NgpMesh mesh)
    {
        if (mesh == null || NgpContainer != NgpVertexMap || !NgpVertexCopies.TryGetValue(mesh.Index, out var copy) || !copy.IsEdited)
            return null;

        return copy.Vertices;
    }

    #region Tab

    private void DisplayNgpEditTab()
    {
        if (!NgpShowOverlay)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Needs_Overlay"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Actions_Header"), LOC.Get("MAP_Navigation_Edit_Actions_Header_TT"));

        GUI.MultiButtonInput("navigationNgpEditToggle",
            "navigationNgpEditToggleButton",
            NgpVertexActive ? LOC.Get("MAP_Navigation_Edit_Stop") : LOC.Get("MAP_Navigation_Edit_Start"),
            NgpVertexActive ? LOC.Get("MAP_Navigation_Edit_Stop_TT") : LOC.Get("MAP_Navigation_Edit_Start_TT"),
            ToggleNgpVertexEdit);

        var hasEdits = HasNgpVertexEdits;

        GUI.ConditionalMultiButtonInput("navigationNgpEditActions",
            "navigationNgpEditCommit", LOC.Get("MAP_Navigation_Edit_Commit"), LOC.Get("MAP_Navigation_Ngp_Edit_Commit_TT"),
            CommitNgpVertexEdits, hasEdits,

            "navigationNgpEditDiscard", LOC.Get("MAP_Navigation_Edit_Discard"), LOC.Get("MAP_Navigation_Edit_Discard_TT"),
            DiscardNgpVertexEdits, hasEdits);

        // Linked Navmeshes
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Link_Header"), LOC.Get("MAP_Navigation_Edit_Link_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Edit_Link_Neighbours")}##navigationNgpEditLinkNeighbours", ref VertexLinkNeighbours);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Edit_Link_Neighbours_TT"));

        DisplayVertexBrushOptions();

        // Cursor Opacity
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Header"), LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##navigationNgpEditCursorOpacity", ref VertexCursorOpacity, 0f, 100f, "%.0f%%");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Input_TT"));

        // Controls
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Edit_Controls_Header"), LOC.Get("MAP_Navigation_Edit_Controls_Header_TT"), ref ShowVertexControls);

        if (ShowVertexControls)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Control_Brush", InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action)));
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Control_Modes",
                InputManager.GetHint(KeybindID.MapEditor_NavigationEditor_Raise),
                InputManager.GetHint(KeybindID.MapEditor_NavigationEditor_Lower),
                InputManager.GetHint(KeybindID.MapEditor_NavigationEditor_Drag),
                InputManager.GetHint(KeybindID.MapEditor_NavigationEditor_Smooth)));
        }

        // Cursor
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Cursor_Header"), LOC.Get("MAP_Navigation_Edit_Cursor_Header_TT"));

        if (!NgpVertexActive)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_Inactive"));
        }
        else if (((IsNgpDragStroke ? NgpVertexStroke : null) ?? NgpVertexHover) is { } hit)
        {
            var position = IsNgpDragStroke ? NgpVertexDragAnchor : hit.Point;
            var moves = IsNgpDragStroke && NgpVertexDragMoves != null ? NgpVertexDragMoves : GetNgpVertexMoves(hit);

            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Edit_Cursor_Mesh", hit.Copy.Mesh.Index, hit.Copy.Mesh.Label));
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_Point", $"{position.X:0.###}, {position.Y:0.###}, {position.Z:0.###}"));
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_Moves", moves.Sum(m => m.Falloff.Count), moves.Count, $"{VertexRadius:0.###}"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_None"));
        }

        // Pending Edits
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Pending_Header"), LOC.Get("MAP_Navigation_Edit_Pending_Header_TT"));

        var edited = NgpVertexCopies.Values.Where(c => c.IsEdited).OrderBy(c => c.Mesh.Index).ToList();

        if (edited.Count == 0)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Pending_None"));
        else if (NgpVertexMap != null && NgpVertexMap != NgpContainer)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_Navigation_Edit_Pending_Other_Map", NgpVertexMap.Name));

        foreach (var copy in edited)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Edit_Pending_Mesh", copy.Mesh.Index, copy.Mesh.Label, copy.MovedCount));
    }

    private void ToggleNgpVertexEdit()
    {
        if (NgpVertexActive)
        {
            StopNgpVertexEdit();
            return;
        }

        // The Collision Editor's cursor takes the same keys
        View.CollisionEditorTool?.StopEditing();
        SetNgpClick(NgpClick.None);
        NgpVertexActive = true;
    }

    /// <summary>Hides the cursor; pending edits are kept, copies without edits are dropped.</summary>
    private void StopNgpVertexEdit()
    {
        if (!NgpVertexActive)
            return;

        NgpVertexActive = false;
        EndNgpVertexStroke();
        NgpVertexHover = null;
        DisposeVertexCursor();

        foreach (var mesh in NgpVertexCopies.Where(c => !c.Value.IsEdited).Select(c => c.Key).ToList())
            NgpVertexCopies.Remove(mesh);
    }

    private void DiscardNgpVertexEdits()
    {
        EndNgpVertexStroke();

        foreach (var copy in NgpVertexCopies.Values)
            copy.Reset();

        SetNgpMessage(LOC.Get("MAP_Navigation_Edit_Discarded"));
    }

    /// <summary>
    /// Builds the NGP again with the moved vertices (bounding boxes, trees, gates, links and IDs following) and swaps it
    /// in as one action. Says so when gates went or came, which happens when one side of a gate moved without the other.
    /// </summary>
    private void CommitNgpVertexEdits()
    {
        EndNgpVertexStroke();

        var map = NgpVertexMap;
        var data = map != null ? Ngp.GetMap(map) : null;
        var edited = NgpVertexCopies.Values.Where(c => c.IsEdited).ToList();

        if (data == null || edited.Count == 0)
            return;

        // The copies must be of the document the bank holds (an undo replaces it)
        if (edited.Any(c => c.Mesh.Index >= data.Meshes.Count || !ReferenceEquals(data.Meshes[c.Mesh.Index].Mesh, c.Data)))
        {
            NgpVertexCopies.Clear();
            return;
        }

        NGP after;
        try
        {
            after = NgpEdit.WithVertices(data.Ngp, edited.ToDictionary(c => c.Mesh.Index, c => (IReadOnlyList<Vector3>)c.Vertices));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Edit_Commit_Failed", data.FileName), e);
            SetNgpMessage(LOC.Get("MAP_Navigation_Edit_Commit_Failed", data.FileName), true);
            return;
        }

        int moved = edited.Sum(c => c.MovedCount);
        int gatesBefore = data.Ngp.Gates.Count, gatesAfter = after.Gates.Count;

        View.ViewportActionManager.ExecuteAction(new NavigationNgpEditAction(map, Ngp, data.Ngp, after, OnNgpEdited));

        // The copies belonged to the document that went
        NgpVertexCopies.Clear();

        if (gatesAfter < gatesBefore)
            SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Edit_Committed_Gates_Lost", edited.Count, moved, gatesBefore, gatesAfter), true);
        else
            SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Edit_Committed", edited.Count, moved, gatesBefore, gatesAfter));

        Smithbox.Log(this, NgpMessage);
    }

    #endregion

    #region Viewport

    /// <summary>Called each frame: the brush while editing, and dropping copies of documents that went. True while editing.</summary>
    private bool UpdateNgpVertexEdit(Ray ray, bool canInteract)
    {
        // Copies whose mesh is no longer the bank's (an undo or redo) or whose map went
        if (NgpVertexCopies.Count > 0)
        {
            var data = NgpVertexMap != null && NgpVertexMap.LoadState == MapContentLoadState.Loaded ? Ngp.GetMap(NgpVertexMap) : null;

            foreach (var mesh in NgpVertexCopies.Keys.ToList())
            {
                if (data == null || mesh >= data.Meshes.Count || !ReferenceEquals(data.Meshes[mesh].Mesh, NgpVertexCopies[mesh].Data))
                    NgpVertexCopies.Remove(mesh);
            }
        }

        if (NgpVertexActive)
            HandleNgpVertexBrush(ray, canInteract);

        UpdateNgpVertexCursor();
        return NgpVertexActive;
    }

    private void HandleNgpVertexBrush(Ray ray, bool canInteract)
    {
        var canEdit = NgpData != null && NgpContainer != null && (NgpVertexMap == null || NgpVertexMap == NgpContainer || !HasNgpVertexEdits);

        var typing = ImGui.GetIO().WantTextInput;
        var flying = InputManager.IsMouseDown(Veldrid.MouseButton.Right) || InputManager.IsMouseDown(Veldrid.MouseButton.Middle);
        var keysFree = canInteract && !typing && !flying && !InputManager.IsMouseDown(Veldrid.MouseButton.Left);
        var brushFree = canInteract && !typing && !flying;

        if (keysFree && NgpVertexStroke == null)
        {
            foreach (var (key, mode) in VertexBrushKeys)
            {
                if (InputManager.IsPressed(key))
                    VertexBrushMode = mode;
            }
        }

        if (NgpVertexStroke != null && (!brushFree || !canEdit || !InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action)))
            EndNgpVertexStroke();

        if (!IsNgpDragStroke)
            NgpVertexHover = canInteract && canEdit ? NgpVertexRayCast(ray) : null;

        if (NgpVertexStroke != null)
            ContinueNgpVertexStroke(ray);
        else if (brushFree && NgpVertexHover != null && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
            BeginNgpVertexStroke(NgpVertexHover);
    }

    /// <summary>The working copy of a mesh on the overlay's map, made the first time it is asked for.</summary>
    private NgpVertexCopy GetNgpVertexCopy(NgpMesh mesh)
    {
        if (mesh?.Mesh == null || mesh.Mesh.Faces.Count == 0)
            return null;

        if (NgpVertexCopies.TryGetValue(mesh.Index, out var existing) && ReferenceEquals(existing.Data, mesh.Mesh))
            return existing;

        // The copies are of one map: another map's without edits make way
        if (NgpVertexMap != null && NgpVertexMap != NgpContainer)
        {
            if (HasNgpVertexEdits)
                return null;

            NgpVertexCopies.Clear();
        }

        NgpVertexMap = NgpContainer;
        var copy = new NgpVertexCopy(mesh, NgpData.World);
        NgpVertexCopies[mesh.Index] = copy;
        return copy;
    }

    private bool IsNgpMeshDrawn(NgpMesh mesh)
        => !NgpOnlyPickedMesh || PickedNgpMesh(NgpData) is not NgpMesh picked || picked.Index == mesh.Index;

    private NgpVertexHit NgpVertexRayCast(Ray ray)
    {
        NgpVertexHit best = null;
        var direction = Vector3.Normalize(ray.Direction);

        // The nearest face first (bounding boxes skip most meshes), then the working copy of its mesh
        var pick = NgpOverlayBuilder.Pick(NgpData, ray.Origin, direction, IsNgpMeshDrawn, NgpEditedVertices);
        if (pick == null)
            return null;

        var copy = GetNgpVertexCopy(pick.Mesh);
        if (copy != null && copy.RayCast(ray.Origin, direction, out var face, out var distance, out var point))
            best = new NgpVertexHit(copy, face, copy.NearestCorner(face, point), point, distance);

        return best;
    }

    /// <summary>
    /// The vertices a brush at a hit moves: the hit mesh's within the radius (or the face's nearest corner alone if none
    /// is), and with neighbours linked, the other meshes' within it, by the same falloff.
    /// </summary>
    private List<NgpVertexMove> GetNgpVertexMoves(NgpVertexHit target)
    {
        var radius = Math.Max(0.01f, VertexRadius);
        var moves = new List<NgpVertexMove>();

        var own = target.Copy.Falloff(target.Point, 0f, radius);
        if (own.Count == 0)
            own.Add((target.Vertex, 1f));
        moves.Add(new NgpVertexMove(target.Copy, own));

        if (!VertexLinkNeighbours || NgpData == null)
            return moves;

        var reach = new Vector3(radius + 0.1f);

        foreach (var mesh in NgpData.Meshes)
        {
            if (mesh.Index == target.Copy.Mesh.Index || !IsNgpMeshDrawn(mesh))
                continue;

            // Only meshes whose (edited) bounds come within the radius
            var existing = NgpVertexCopies.TryGetValue(mesh.Index, out var c) && ReferenceEquals(c.Data, mesh.Mesh) ? c : null;
            if (existing == null)
            {
                var min = NgpData.Place(mesh.Mesh.BoundingBoxMin) - reach;
                var max = NgpData.Place(mesh.Mesh.BoundingBoxMax) + reach;
                if (target.Point.X < min.X || target.Point.Y < min.Y || target.Point.Z < min.Z || target.Point.X > max.X || target.Point.Y > max.Y || target.Point.Z > max.Z)
                    continue;
            }

            var copy = existing ?? GetNgpVertexCopy(mesh);
            if (copy == null)
                continue;

            var falloff = copy.Falloff(target.Point, 0f, radius);
            if (falloff.Count > 0)
                moves.Add(new NgpVertexMove(copy, falloff));
        }

        return moves;
    }

    private void BeginNgpVertexStroke(NgpVertexHit hit)
    {
        NgpVertexStroke = hit;
        NgpVertexStrokeMode = VertexBrushMode;

        if (NgpVertexStrokeMode is CollisionBrushMode.Drag)
        {
            NgpVertexDragMoves = GetNgpVertexMoves(hit);
            NgpVertexDragHeight = hit.Point.Y;
            NgpVertexDragLast = hit.Point;
            NgpVertexDragAnchor = hit.Point;
            return;
        }

        ApplyNgpVertexBrush(hit);
        NgpVertexNextApply = Stopwatch.GetTimestamp() + VertexIntervalTicks();
    }

    private void ContinueNgpVertexStroke(Ray ray)
    {
        if (NgpVertexStrokeMode is CollisionBrushMode.Drag)
        {
            if (!IntersectVertexHeight(ray, NgpVertexDragHeight, out var point))
                return;

            var offset = point - NgpVertexDragLast;
            offset.Y = 0;

            if (offset.LengthSquared() < 1e-10f)
                return;

            foreach (var move in NgpVertexDragMoves)
                move.Copy.MoveWorld(move.Falloff, offset);

            NgpVertexDragLast = point;
            NgpVertexDragAnchor += offset;
            return;
        }

        var now = Stopwatch.GetTimestamp();
        if (now < NgpVertexNextApply)
            return;

        if (NgpVertexHover != null)
            ApplyNgpVertexBrush(NgpVertexHover);

        NgpVertexNextApply = now + VertexIntervalTicks();
    }

    private void EndNgpVertexStroke()
    {
        NgpVertexStroke = null;
        NgpVertexDragMoves = null;
    }

    private void ApplyNgpVertexBrush(NgpVertexHit hit)
    {
        var moves = GetNgpVertexMoves(hit);

        switch (NgpVertexStrokeMode)
        {
            case CollisionBrushMode.Raise:
                foreach (var move in moves)
                    move.Copy.MoveWorld(move.Falloff, new Vector3(0, VertexStep, 0));
                break;

            case CollisionBrushMode.Lower:
                foreach (var move in moves)
                    move.Copy.MoveWorld(move.Falloff, new Vector3(0, -VertexStep, 0));
                break;

            case CollisionBrushMode.Smooth:
                SmoothNgpVertices(moves);
                break;
        }
    }

    /// <summary>Pulls the brushed vertices towards their best-fit plane, as the other modes' Smooth brush.</summary>
    private void SmoothNgpVertices(List<NgpVertexMove> moves)
    {
        var points = new List<(Vector3 Position, float Weight)>();

        foreach (var move in moves)
        {
            foreach (var (v, weight) in move.Falloff)
                points.Add((move.Copy.WorldVertex(v), weight));
        }

        if (!CollisionEditorTool.FitPlane(points, out var center, out var normal))
            return;

        var strength = Math.Clamp(VertexSmoothStrength, 0f, 1f);

        foreach (var move in moves)
        {
            var offsets = new List<(int, Vector3)>(move.Falloff.Count);

            foreach (var (v, weight) in move.Falloff)
            {
                var distance = Vector3.Dot(move.Copy.WorldVertex(v) - center, normal);
                if (MathF.Abs(distance) >= 1e-6f)
                    offsets.Add((v, -normal * (distance * weight * strength)));
            }

            move.Copy.OffsetWorld(offsets);
        }
    }

    /// <summary>The brush sphere at the hit, and a smaller sphere on the face's nearest corner.</summary>
    private void UpdateNgpVertexCursor()
    {
        var target = NgpVertexActive ? (IsNgpDragStroke ? NgpVertexStroke : null) ?? NgpVertexHover : null;

        if (target == null)
        {
            SetVertexProxyVisible(VertexCursorWire, false);
            SetVertexProxyVisible(VertexCursorFill, false);
            SetVertexProxyVisible(VertexMarker, false);
            return;
        }

        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;
        if (scene == null)
            return;

        var cursorColor = ToVertexColor(CFG.Current.MapEditor_CollisionEditor_Cursor_Color);

        VertexCursorWire ??= RenderableHelper.GetDetectionSphereProxy(scene, cursorColor);
        VertexCursorFill ??= RenderableHelper.GetDetectionSphereFillProxy(scene, cursorColor);
        VertexMarker ??= new DebugPrimitiveRenderableProxy(scene.OpaqueRenderables, RenderableHelper._placementOrb)
        {
            Pickable = false,
            RenderOverlay = true,
            BaseColor = ToVertexColor(CFG.Current.MapEditor_CollisionEditor_Vertex_Color),
            HighlightedColor = ToVertexColor(CFG.Current.MapEditor_CollisionEditor_Vertex_Color)
        };

        var fill = ColorHelper.GetTransparencyColor(cursorColor, VertexCursorOpacity);
        if (VertexCursorFill.BaseColor != fill)
        {
            VertexCursorFill.BaseColor = fill;
            VertexCursorFill.HighlightedColor = fill;
        }

        var size = Math.Max(0.01f, VertexRadius);
        var cursor = IsNgpDragStroke ? NgpVertexDragAnchor : target.Point;
        var cursorWorld = Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(cursor);
        var markerSize = MathF.Min(size * 0.6f, VertexMaxMarkerSize);

        VertexCursorWire.World = cursorWorld;
        VertexCursorFill.World = cursorWorld;
        VertexMarker.World = Matrix4x4.CreateScale(markerSize / VertexSphereRadius) * Matrix4x4.CreateTranslation(target.Copy.WorldVertex(target.Vertex));

        SetVertexProxyVisible(VertexCursorWire, true);
        SetVertexProxyVisible(VertexCursorFill, true);
        SetVertexProxyVisible(VertexMarker, true);
    }

    #endregion
}
