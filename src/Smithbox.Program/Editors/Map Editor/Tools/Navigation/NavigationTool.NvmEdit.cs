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
/// The NVM mode's Edit tab: moves NVM vertices with the Havok mode's brush (Raise, Lower, Drag, Smooth; the same keys,
/// radius, step, interval and cursor). Other rooms' vertices under the brush follow too when linked, so gates between
/// navmeshes stay joined. Edits are drawn in the overlay until committed, which builds each edited NVM again (topology
/// kept, root box and box tree rebuilt the game's way), sets the edited rooms' MCP boxes from them, moves the MCG nodes
/// on moved gate triangles (scaling their edges' costs), and swaps it all in as one undoable action.
/// </summary>
public partial class NavigationTool
{
    private bool NvmVertexActive;
    private MapContainer NvmVertexMap;

    /// <summary>The working copies, by NVM file name (a file two parts load is edited once).</summary>
    private readonly Dictionary<string, NvmVertexCopy> NvmVertexCopies = new(StringComparer.OrdinalIgnoreCase);

    private sealed record NvmVertexHit(NvmVertexCopy Copy, int Triangle, int Vertex, Vector3 Point, float Distance);
    private sealed record NvmVertexMove(NvmVertexCopy Copy, List<(int Vertex, float Weight)> Falloff);

    private NvmVertexHit NvmVertexHover;
    private NvmVertexHit NvmVertexStroke;
    private CollisionBrushMode NvmVertexStrokeMode;
    private long NvmVertexNextApply;
    private List<NvmVertexMove> NvmVertexDragMoves;
    private float NvmVertexDragHeight;
    private Vector3 NvmVertexDragLast;
    private Vector3 NvmVertexDragAnchor;

    private bool IsNvmDragStroke => NvmVertexStroke != null && NvmVertexStrokeMode is CollisionBrushMode.Drag;

    private bool HasNvmVertexEdits => NvmVertexCopies.Values.Any(c => c.IsEdited);

    /// <summary>Changes with every move of any copy, so the overlay knows to be drawn again.</summary>
    private int NvmVertexRevision => NvmVertexCopies.Values.Sum(c => c.Revision) + NvmVertexCopies.Count * 7919;

    /// <summary>A room's vertices as edited, or null to draw its NVM's.</summary>
    private IReadOnlyList<Vector3> NvmEditedVertices(NvmRoom room)
    {
        if (room?.FileName == null || NvmContainer != NvmVertexMap || !NvmVertexCopies.TryGetValue(room.FileName, out var copy) || !copy.IsEdited)
            return null;

        return copy.Vertices;
    }

    #region Tab

    private void DisplayNvmEditTab()
    {
        if (!NvmShowOverlay)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Needs_Overlay"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Actions_Header"), LOC.Get("MAP_Navigation_Edit_Actions_Header_TT"));

        GUI.MultiButtonInput("navigationNvmEditToggle",
            "navigationNvmEditToggleButton",
            NvmVertexActive ? LOC.Get("MAP_Navigation_Edit_Stop") : LOC.Get("MAP_Navigation_Edit_Start"),
            NvmVertexActive ? LOC.Get("MAP_Navigation_Edit_Stop_TT") : LOC.Get("MAP_Navigation_Edit_Start_TT"),
            ToggleNvmVertexEdit);

        var hasEdits = HasNvmVertexEdits;

        GUI.ConditionalMultiButtonInput("navigationNvmEditActions",
            "navigationNvmEditCommit", LOC.Get("MAP_Navigation_Edit_Commit"), LOC.Get("MAP_Navigation_Nvm_Edit_Commit_TT"),
            CommitNvmVertexEdits, hasEdits,

            "navigationNvmEditDiscard", LOC.Get("MAP_Navigation_Edit_Discard"), LOC.Get("MAP_Navigation_Edit_Discard_TT"),
            DiscardNvmVertexEdits, hasEdits);

        // Linked Navmeshes
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Link_Header"), LOC.Get("MAP_Navigation_Edit_Link_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Edit_Link_Neighbours")}##navigationNvmEditLinkNeighbours", ref VertexLinkNeighbours);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Edit_Link_Neighbours_TT"));

        DisplayVertexBrushOptions();

        // Cursor Opacity
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Header"), LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##navigationNvmEditCursorOpacity", ref VertexCursorOpacity, 0f, 100f, "%.0f%%");
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Cursor_Opacity_Input_TT"));

        // Controls
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Edit_Controls_Header"), LOC.Get("MAP_Navigation_Edit_Controls_Header_TT"), ref ShowVertexControls);

        if (ShowVertexControls)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Control_Brush", InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action)));
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Control_Modes",
                InputManager.GetHint(Keybinds.KeybindID.MapEditor_NavigationEditor_Raise),
                InputManager.GetHint(Keybinds.KeybindID.MapEditor_NavigationEditor_Lower),
                InputManager.GetHint(Keybinds.KeybindID.MapEditor_NavigationEditor_Drag),
                InputManager.GetHint(Keybinds.KeybindID.MapEditor_NavigationEditor_Smooth)));
        }

        // Cursor
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Cursor_Header"), LOC.Get("MAP_Navigation_Edit_Cursor_Header_TT"));

        if (!NvmVertexActive)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_Inactive"));
        }
        else if (((IsNvmDragStroke ? NvmVertexStroke : null) ?? NvmVertexHover) is { } hit)
        {
            var position = IsNvmDragStroke ? NvmVertexDragAnchor : hit.Point;
            var moves = IsNvmDragStroke && NvmVertexDragMoves != null ? NvmVertexDragMoves : GetNvmVertexMoves(hit);

            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Edit_Cursor_Room", hit.Copy.Room.FileName, hit.Copy.Room.PartName));
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

        var edited = NvmVertexCopies.Values.Where(c => c.IsEdited).OrderBy(c => c.Room.FileName).ToList();

        if (edited.Count == 0)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Pending_None"));
        else if (NvmVertexMap != null && NvmVertexMap != NvmContainer)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_Navigation_Edit_Pending_Other_Map", NvmVertexMap.Name));

        foreach (var copy in edited)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Pending_File", copy.Room.FileName, copy.MovedCount));
    }

    private void ToggleNvmVertexEdit()
    {
        if (NvmVertexActive)
        {
            StopNvmVertexEdit();
            return;
        }

        // The Collision Editor's cursor takes the same keys
        View.CollisionEditorTool?.StopEditing();
        SetNvmClick(NvmClick.None);
        NvmVertexActive = true;
    }

    /// <summary>Hides the cursor; pending edits are kept, copies without edits are dropped.</summary>
    private void StopNvmVertexEdit()
    {
        if (!NvmVertexActive)
            return;

        NvmVertexActive = false;
        EndNvmVertexStroke();
        NvmVertexHover = null;
        DisposeVertexCursor();

        foreach (var name in NvmVertexCopies.Where(c => !c.Value.IsEdited).Select(c => c.Key).ToList())
            NvmVertexCopies.Remove(name);
    }

    private void DiscardNvmVertexEdits()
    {
        EndNvmVertexStroke();

        foreach (var copy in NvmVertexCopies.Values)
            copy.Reset();

        SetNvmMessage(LOC.Get("MAP_Navigation_Edit_Discarded"));
    }

    /// <summary>
    /// Builds each edited NVM again with its moved vertices, sets the edited rooms' MCP boxes, moves the MCG nodes on
    /// their gates, and swaps them in as one action.
    /// </summary>
    private void CommitNvmVertexEdits()
    {
        EndNvmVertexStroke();

        var map = NvmVertexMap;
        var edited = NvmVertexCopies.Values.Where(c => c.IsEdited).ToList();
        var data = map != null ? Nvm.GetMap(map) : null;

        if (data == null || edited.Count == 0)
            return;

        var files = new List<NavigationNvmEditAction.FileChange>();
        var roomsAfter = new Dictionary<NvmRoom, NVM>();
        var boxes = new List<(NvmRoom, NVM, NVM)>();
        int moved = 0;

        foreach (var copy in edited)
        {
            if (!data.Files.TryGetValue(copy.Room.FileName, out var before) || !ReferenceEquals(before, copy.Nvm))
                continue;

            NVM after;
            try
            {
                after = NvmEdit.WithVertices(before, copy.Vertices);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Edit_Commit_Failed", copy.Room.FileName), e);
                SetNvmMessage(LOC.Get("MAP_Navigation_Edit_Commit_Failed", copy.Room.FileName), true);
                return;
            }

            files.Add(new(copy.Room.FileName, before, after));
            moved += copy.MovedCount;

            // Every part that loads the file has its room follow
            foreach (var room in data.Rooms.Where(r => r.FileName != null && r.FileName.Equals(copy.Room.FileName, StringComparison.OrdinalIgnoreCase)))
            {
                roomsAfter[room] = after;
                boxes.Add((room, before, after));
            }
        }

        if (files.Count == 0)
            return;

        var mcp = NvmEdit.WithRoomBoxes(data, data.Mcp, boxes);
        var mcg = NvmEdit.WithNodesFollowing(data, data.Mcg, roomsAfter, out int nodes);

        View.ViewportActionManager.ExecuteAction(new NavigationNvmEditAction(map, Nvm, files, data.Mcp, mcp, data.Mcg, mcg, OnNvmEdited));

        // The copies belonged to the documents that went
        NvmVertexCopies.Clear();

        SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Edit_Committed", files.Count, moved, nodes, ReferenceEquals(mcp, data.Mcp) ? 0 : boxes.Count));
        Smithbox.Log(this, NvmMessage);
    }

    #endregion

    #region Viewport

    /// <summary>Called each frame: the brush while editing, and dropping copies of documents that went. True while editing.</summary>
    private bool UpdateNvmVertexEdit(Ray ray, bool canInteract)
    {
        // Copies whose NVM is no longer the bank's (an undo or redo) or whose map went
        if (NvmVertexCopies.Count > 0)
        {
            var data = NvmVertexMap != null && NvmVertexMap.LoadState == MapContentLoadState.Loaded ? Nvm.GetMap(NvmVertexMap) : null;

            foreach (var name in NvmVertexCopies.Keys.ToList())
            {
                if (data == null || !data.Files.TryGetValue(name, out var nvm) || !ReferenceEquals(nvm, NvmVertexCopies[name].Nvm))
                    NvmVertexCopies.Remove(name);
            }
        }

        if (NvmVertexActive)
            HandleNvmVertexBrush(ray, canInteract);

        UpdateNvmVertexCursor();
        return NvmVertexActive;
    }

    private void HandleNvmVertexBrush(Ray ray, bool canInteract)
    {
        var canEdit = NvmData != null && NvmContainer != null && (NvmVertexMap == null || NvmVertexMap == NvmContainer || !HasNvmVertexEdits);

        var typing = ImGui.GetIO().WantTextInput;
        var flying = InputManager.IsMouseDown(Veldrid.MouseButton.Right) || InputManager.IsMouseDown(Veldrid.MouseButton.Middle);
        var keysFree = canInteract && !typing && !flying && !InputManager.IsMouseDown(Veldrid.MouseButton.Left);
        var brushFree = canInteract && !typing && !flying;

        if (keysFree && NvmVertexStroke == null)
        {
            foreach (var (key, mode) in VertexBrushKeys)
            {
                if (InputManager.IsPressed(key))
                    VertexBrushMode = mode;
            }
        }

        if (NvmVertexStroke != null && (!brushFree || !canEdit || !InputManager.IsMouseDown(MousebindID.Viewport_Picking_Action)))
            EndNvmVertexStroke();

        if (!IsNvmDragStroke)
            NvmVertexHover = canInteract && canEdit ? NvmVertexRayCast(ray) : null;

        if (NvmVertexStroke != null)
            ContinueNvmVertexStroke(ray);
        else if (brushFree && NvmVertexHover != null && InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
            BeginNvmVertexStroke(NvmVertexHover);
    }

    /// <summary>The working copy of a room's NVM on the overlay's map, made the first time it is asked for.</summary>
    private NvmVertexCopy GetNvmVertexCopy(NvmRoom room)
    {
        if (room?.Nvm == null || room.FileName == null || room.Nvm.Triangles.Count == 0)
            return null;

        if (NvmVertexCopies.TryGetValue(room.FileName, out var existing) && ReferenceEquals(existing.Nvm, room.Nvm))
            return existing;

        // The copies are of one map: another map's without edits make way
        if (NvmVertexMap != null && NvmVertexMap != NvmContainer)
        {
            if (HasNvmVertexEdits)
                return null;

            NvmVertexCopies.Clear();
        }

        NvmVertexMap = NvmContainer;
        var copy = new NvmVertexCopy(room);
        NvmVertexCopies[room.FileName] = copy;
        return copy;
    }

    private bool IsNvmRoomDrawn(NvmRoom room)
        => !NvmOnlyPickedRoom || PickedNvmRoom(NvmData) is not NvmRoom picked || picked.Index == room.Index;

    private NvmVertexHit NvmVertexRayCast(Ray ray)
    {
        NvmVertexHit best = null;
        var direction = Vector3.Normalize(ray.Direction);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var room in NvmData.Rooms)
        {
            // A file two parts load is brushed where its first part places it
            if (room.FileName == null || !IsNvmRoomDrawn(room) || !seen.Add(room.FileName))
                continue;

            var copy = GetNvmVertexCopy(room);
            if (copy == null || !copy.RayCast(ray.Origin, direction, out var triangle, out var distance, out var point))
                continue;

            if (best == null || distance < best.Distance)
                best = new NvmVertexHit(copy, triangle, copy.NearestCorner(triangle, point), point, distance);
        }

        return best;
    }

    /// <summary>
    /// The vertices a brush at a hit moves: the hit NVM's within the radius (or the triangle's nearest corner alone if
    /// none is), and with neighbours linked, the other rooms' within it, by the same falloff.
    /// </summary>
    private List<NvmVertexMove> GetNvmVertexMoves(NvmVertexHit target)
    {
        var radius = Math.Max(0.01f, VertexRadius);
        var moves = new List<NvmVertexMove>();

        var own = target.Copy.Falloff(target.Point, 0f, radius);
        if (own.Count == 0)
            own.Add((target.Vertex, 1f));
        moves.Add(new NvmVertexMove(target.Copy, own));

        if (!VertexLinkNeighbours || NvmData == null)
            return moves;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { target.Copy.Room.FileName };

        foreach (var room in NvmData.Rooms)
        {
            if (room.FileName == null || !IsNvmRoomDrawn(room) || !seen.Add(room.FileName))
                continue;

            var copy = GetNvmVertexCopy(room);
            if (copy == null)
                continue;

            var falloff = copy.Falloff(target.Point, 0f, radius);
            if (falloff.Count > 0)
                moves.Add(new NvmVertexMove(copy, falloff));
        }

        return moves;
    }

    private void BeginNvmVertexStroke(NvmVertexHit hit)
    {
        NvmVertexStroke = hit;
        NvmVertexStrokeMode = VertexBrushMode;

        if (NvmVertexStrokeMode is CollisionBrushMode.Drag)
        {
            NvmVertexDragMoves = GetNvmVertexMoves(hit);
            NvmVertexDragHeight = hit.Point.Y;
            NvmVertexDragLast = hit.Point;
            NvmVertexDragAnchor = hit.Point;
            return;
        }

        ApplyNvmVertexBrush(hit);
        NvmVertexNextApply = Stopwatch.GetTimestamp() + VertexIntervalTicks();
    }

    private void ContinueNvmVertexStroke(Ray ray)
    {
        if (NvmVertexStrokeMode is CollisionBrushMode.Drag)
        {
            if (!IntersectVertexHeight(ray, NvmVertexDragHeight, out var point))
                return;

            var offset = point - NvmVertexDragLast;
            offset.Y = 0;

            if (offset.LengthSquared() < 1e-10f)
                return;

            foreach (var move in NvmVertexDragMoves)
                move.Copy.MoveWorld(move.Falloff, offset);

            NvmVertexDragLast = point;
            NvmVertexDragAnchor += offset;
            return;
        }

        var now = Stopwatch.GetTimestamp();
        if (now < NvmVertexNextApply)
            return;

        if (NvmVertexHover != null)
            ApplyNvmVertexBrush(NvmVertexHover);

        NvmVertexNextApply = now + VertexIntervalTicks();
    }

    private void EndNvmVertexStroke()
    {
        NvmVertexStroke = null;
        NvmVertexDragMoves = null;
    }

    private void ApplyNvmVertexBrush(NvmVertexHit hit)
    {
        var moves = GetNvmVertexMoves(hit);

        switch (NvmVertexStrokeMode)
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
                SmoothNvmVertices(moves);
                break;
        }
    }

    /// <summary>Pulls the brushed vertices towards their best-fit plane, as the Havok mode's Smooth brush.</summary>
    private void SmoothNvmVertices(List<NvmVertexMove> moves)
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

    /// <summary>The brush sphere at the hit, and a smaller sphere on the triangle's nearest corner.</summary>
    private void UpdateNvmVertexCursor()
    {
        var target = NvmVertexActive ? (IsNvmDragStroke ? NvmVertexStroke : null) ?? NvmVertexHover : null;

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
        var cursor = IsNvmDragStroke ? NvmVertexDragAnchor : target.Point;
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
