using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>What the Edit tab's cursor does while editing.</summary>
public enum NavEditTool
{
    [Display(Name = "MAP_Navigation_Edit_Tool_Brush")]
    Brush,
    [Display(Name = "MAP_Navigation_Edit_Tool_Add_Face")]
    AddFace,
    [Display(Name = "MAP_Navigation_Edit_Tool_Remove_Face")]
    RemoveFace,
}

/// <summary>
/// The Edit tab's Add Face tool, in both modes: three clicks in the viewport place the corners of a new triangle. A
/// click near a vertex of the navmesh takes that vertex (so the face joins the faces on its edges), elsewhere it puts a
/// new vertex where the cursor meets the drawn navmesh, or the height of the last corner. The first corner must be on a
/// navmesh, which the face is added to. The third click adds it as one undoable action (NavFaceAdd for the Havok games,
/// NvmFaceAdd for the NVM ones); the map's save writes it.
/// </summary>
public partial class NavigationTool
{
    private NavEditTool EditTool = NavEditTool.Brush;

    /// <summary>How near the cursor's ray a vertex must pass for a corner to take it, in metres.</summary>
    private float FaceSnapDistance = 0.5f;

    /// <summary>A placed (or hovered) corner: a vertex of the target (or -1 for a new one), and where it is in the viewport.</summary>
    private sealed record FaceCornerPick(int Vertex, Vector3 World);

    private readonly List<FaceCornerPick> FaceCorners = new();
    private FaceCornerPick FaceHover;

    // The file the corners are placed on: a Havok working copy or an NVM one
    private NavVertexFile FaceFile;
    private NvmVertexCopy FaceNvm;

    // The vertices a corner may take in the Havok target (not those of user edges), found once per file
    private NavVertexFile FaceVerticesFor;
    private HashSet<int> FaceVertexSet;

    private string FaceMessage;
    private bool FaceMessageIsError;

    private DebugPrimitiveRenderableProxy[] FaceOrbs;
    private readonly List<DebugPrimitiveRenderableProxy> FaceLines = new();
    private static DbgPrimLineBatch FaceLinePrim;

    private bool IsBrushTool => EditTool == NavEditTool.Brush;
    private bool IsFaceTool => EditTool == NavEditTool.AddFace;
    private bool IsRemoveTool => EditTool == NavEditTool.RemoveFace;

    /// <summary>The header's text for the Edit tab's mode while it is on.</summary>
    private string EditModeKey => EditTool switch
    {
        NavEditTool.AddFace => "MAP_Navigation_Mode_Face_Add",
        NavEditTool.RemoveFace => "MAP_Navigation_Mode_Face_Remove",
        _ => "MAP_Navigation_Mode_Vertex_Edit",
    };

    #region Tab

    /// <summary>The Tool group: Brush or Add Face. Changing it drops a stroke or the corners placed so far.</summary>
    private void DisplayEditToolChoice()
    {
        // Tool
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Edit_Tool_Header"), LOC.Get("MAP_Navigation_Edit_Tool_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##navigationEditTool", LOC.Get(EditTool.GetDisplayName())))
        {
            foreach (var tool in Enum.GetValues<NavEditTool>())
            {
                if (ImGui.Selectable($"{LOC.Get(tool.GetDisplayName())}##navigationEditTool{tool}", tool == EditTool) && tool != EditTool)
                {
                    EditTool = tool;
                    EndVertexStroke();
                    EndNvmVertexStroke();
                    VertexHover = null;
                    NvmVertexHover = null;
                    ClearFaceCorners();
                }
                GUI.Tooltip(LOC.Get($"MAP_Navigation_Edit_Tool_{tool}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Edit_Tool_Input_TT"));

        GUI.WrappedText(LOC.Get($"MAP_Navigation_Edit_Tool_{EditTool}_TT"));
    }

    /// <summary>The Add Face groups: snapping, the corners placed, what the last add did, and the controls.</summary>
    private void DisplayFaceAddOptions(bool active)
    {
        // Snap Distance
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Face_Snap_Header"), LOC.Get("MAP_Navigation_Face_Snap_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##navigationFaceSnap", ref FaceSnapDistance))
            FaceSnapDistance = Math.Max(0f, FaceSnapDistance);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Face_Snap_Input_TT"));

        // Corners
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Face_Corners_Header"), LOC.Get("MAP_Navigation_Face_Corners_Header_TT"));

        if (!active)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_Inactive"));
        }
        else
        {
            var target = FaceFile?.File.Name ?? FaceNvm?.Room.FileName;

            if (target != null)
                GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Target", target));

            GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Placed", FaceCorners.Count));

            for (int i = 0; i < FaceCorners.Count; i++)
            {
                var c = FaceCorners[i];
                var at = $"{c.World.X:0.###}, {c.World.Y:0.###}, {c.World.Z:0.###}";
                GUI.WrappedText(c.Vertex >= 0
                    ? LOC.Get("MAP_Navigation_Face_Corner_Vertex", i + 1, c.Vertex, at)
                    : LOC.Get("MAP_Navigation_Face_Corner_New", i + 1, at));
            }

            if (FaceHover != null)
                GUI.WrappedText(FaceHover.Vertex >= 0
                    ? LOC.Get("MAP_Navigation_Face_Hover_Vertex", FaceHover.Vertex)
                    : LOC.Get("MAP_Navigation_Face_Hover_New"));
            else
                GUI.WrappedText(LOC.Get(FaceCorners.Count == 0 ? "MAP_Navigation_Face_Hover_None_First" : "MAP_Navigation_Face_Hover_None"));
        }

        var hasCorners = FaceCorners.Count > 0;

        GUI.ConditionalMultiButtonInput("navigationFaceActions",
            "navigationFaceUndoCorner", LOC.Get("MAP_Navigation_Face_Undo_Corner"), LOC.Get("MAP_Navigation_Face_Undo_Corner_TT"),
            RemoveLastFaceCorner, hasCorners,

            "navigationFaceClear", LOC.Get("MAP_Navigation_Face_Clear"), LOC.Get("MAP_Navigation_Face_Clear_TT"),
            ClearFaceCorners, hasCorners);

        if (FaceMessage != null)
        {
            if (FaceMessageIsError)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, FaceMessage);
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, FaceMessage);
        }

        // Controls
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Edit_Controls_Header"), LOC.Get("MAP_Navigation_Edit_Controls_Header_TT"),
            ref ShowVertexControls);

        if (ShowVertexControls)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Control_Click", InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action)));
    }

    private void RemoveLastFaceCorner()
    {
        if (FaceCorners.Count > 0)
            FaceCorners.RemoveAt(FaceCorners.Count - 1);

        if (FaceCorners.Count == 0)
        {
            FaceFile = null;
            FaceNvm = null;
        }
    }

    private void ClearFaceCorners()
    {
        FaceCorners.Clear();
        FaceHover = null;
        ClearFaceRemoveHover();
        FaceFile = null;
        FaceNvm = null;
    }

    private void SetFaceMessage(string message, bool error = false)
    {
        FaceMessage = message;
        FaceMessageIsError = error;

        if (error)
            Smithbox.LogError(this, message);
        else
            Smithbox.Log(this, message);
    }

    #endregion

    #region Havok

    /// <summary>Places the corners on the overlay's navmeshes, and adds the face on the third.</summary>
    private void HandleFaceAdd(Ray ray, bool canInteract)
    {
        VertexHover = null;

        var canEdit = OverlayNavigation != null && OverlayContainer != null && (VertexMap == null || VertexMap == OverlayContainer);

        // The target's document went (an undo, redo or another edit): its corners went with it
        if (FaceFile != null && !VertexFiles.ContainsValue(FaceFile))
            ClearFaceCorners();

        var flying = InputManager.IsMouseDown(Veldrid.MouseButton.Right) || InputManager.IsMouseDown(Veldrid.MouseButton.Middle);

        if (!canInteract || !canEdit || flying || ImGui.GetIO().WantTextInput)
        {
            FaceHover = null;
            return;
        }

        var hit = VertexRayCast(ray);
        var target = FaceFile ?? hit?.File;

        FaceHover = target == null ? null : PickFaceCorner(ray, hit?.Point, target.Vertices.Length, v => target.WorldVertex(v), HavokFaceVertices(target));

        if (FaceHover == null || !InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
            return;

        if (FaceCorners.Count == 0)
        {
            // A face added to a file with brush edits waiting would drop them, as it gives the file a new document
            if (target.IsEdited)
            {
                SetFaceMessage(LOC.Get("MAP_Navigation_Face_Error_Pending", target.File.Name), true);
                return;
            }

            FaceFile = target;
        }

        FaceCorners.Add(FaceHover);

        if (FaceCorners.Count == 3)
            CommitHavokFace();
    }

    private HashSet<int> HavokFaceVertices(NavVertexFile file)
    {
        if (!ReferenceEquals(FaceVerticesFor, file))
        {
            FaceVerticesFor = file;
            FaceVertexSet = NavFaceAdd.FaceVertices(file.Mesh);
        }

        return FaceVertexSet;
    }

    /// <summary>
    /// Adds the triangle to the target file: the NVA entries of the file get its new face count (and the running face
    /// data indices follow), connections naming the user edges that moved up follow them, and the file's document is
    /// swapped, all as one undoable action. A refused face keeps the first two corners, so the third can be placed again.
    /// </summary>
    private void CommitHavokFace()
    {
        var file = FaceFile;
        var navigation = OverlayNavigation;
        var map = OverlayContainer;

        if (file == null || navigation == null || map == null || !Matrix4x4.Invert(file.World, out var toMesh))
        {
            ClearFaceCorners();
            return;
        }

        var corners = FaceCorners.Select(c => c.Vertex >= 0
            ? NavFaceCorner.Existing(c.Vertex, file.Vertices[c.Vertex])
            : NavFaceCorner.New(Vector3.Transform(c.World, toMesh))).ToList();

        var entry = file.Entry;
        var connected = ConnectedEdges(navigation, entry);
        var before = new NavModelState(navigation);

        NavFaceEditResult result;
        try
        {
            result = NavFaceAdd.Build(file.File.Document, corners, connected.Contains);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Face_Failed", file.File.Name), e);
            result = new NavFaceEditResult { Error = "MAP_Navigation_Face_Error_File" };
        }

        if (!result.Ok)
        {
            SetFaceMessage(LOC.Get(result.Error), true);
            FaceCorners.RemoveAt(FaceCorners.Count - 1);
            return;
        }

        var changes = new NavRemapResult { NvaChanged = true };
        int faces = result.Face + 1;

        foreach (var e in navigation.Nva.Navmeshes)
        {
            if (string.Equals(navigation.FileNameOf(e), file.File.Name, StringComparison.OrdinalIgnoreCase))
                e.FaceCount = faces;
        }

        if (NavUserEdges.ConnectionsNameEdgesFrom(navigation, entry.NameID, result.FirstEdge))
        {
            var remap = NavigationRemap.RemapNavmesh(navigation, entry.NameID, f => f, result.EdgeMap);
            foreach (var nvc in remap.NvcsChanged)
                changes.NvcsChanged.Add(nvc);
        }

        NavModelEdit.RecomputeFaceDataIndex(navigation.Nva, navigation.Profile);

        var after = new NavModelState(navigation);
        ModelEditMap = map;
        View.ViewportActionManager.ExecuteAction(new NavigationModelEditAction(map, Bank, before, after,
            new[] { new NavFileChange(file.File.Name, file.File.Document, result.Document) }, changes, OnModelEdited));

        SetFaceMessage(LOC.Get(result.GraphRebuilt ? "MAP_Navigation_Face_Added_Graph" : "MAP_Navigation_Face_Added",
            file.File.Name, result.Face, result.Joined, result.NewVertices));
        ClearFaceCorners();
    }

    /// <summary>The edges of an entry's mesh that a connection names, on this map's side (NVA, or NVC).</summary>
    private static HashSet<int> ConnectedEdges(NavigationMap map, NVA.Navmesh entry)
    {
        var edges = new HashSet<int>();

        foreach (var view in NavigationRemap.Connections(map))
        {
            bool sideA = !view.IsCrossMap || view.Nvc.MapA == map.MapId;
            bool sideB = !view.IsCrossMap || view.Nvc.MapB == map.MapId;

            if (sideA && view.MainNameID == entry.NameID)
                edges.Add(view.Connection.EdgeIndex);
            if (sideB && view.TargetNameID == entry.NameID)
                edges.Add(view.Connection.OppositeEdgeIndex);
        }

        return edges;
    }

    #endregion

    #region NVM

    private void HandleNvmFaceAdd(Ray ray, bool canInteract)
    {
        NvmVertexHover = null;

        var canEdit = NvmData != null && NvmContainer != null && (NvmVertexMap == null || NvmVertexMap == NvmContainer || !HasNvmVertexEdits);

        if (FaceNvm != null && !NvmVertexCopies.ContainsValue(FaceNvm))
            ClearFaceCorners();

        var flying = InputManager.IsMouseDown(Veldrid.MouseButton.Right) || InputManager.IsMouseDown(Veldrid.MouseButton.Middle);

        if (!canInteract || !canEdit || flying || ImGui.GetIO().WantTextInput)
        {
            FaceHover = null;
            return;
        }

        var hit = NvmVertexRayCast(ray);
        var target = FaceNvm ?? hit?.Copy;

        FaceHover = target == null ? null : PickFaceCorner(ray, hit?.Point, target.Vertices.Length, v => target.WorldVertex(v), null);

        if (FaceHover == null || !InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
            return;

        if (FaceCorners.Count == 0)
        {
            if (target.IsEdited)
            {
                SetFaceMessage(LOC.Get("MAP_Navigation_Face_Error_Pending", target.Room.FileName), true);
                return;
            }

            FaceNvm = target;
        }

        FaceCorners.Add(FaceHover);

        if (FaceCorners.Count == 3)
            CommitNvmFace();
    }

    /// <summary>
    /// Adds the triangle to the target NVM: every room that loads the file follows, their MCP room boxes are set from its
    /// new root box, and it is all swapped in as one undoable action. The MCG names no triangle the add moves.
    /// </summary>
    private void CommitNvmFace()
    {
        var copy = FaceNvm;
        var map = NvmVertexMap ?? NvmContainer;
        var data = map != null ? Nvm.GetMap(map) : null;

        if (copy == null || data == null || !data.Files.TryGetValue(copy.Room.FileName, out var before) || !ReferenceEquals(before, copy.Nvm)
            || !Matrix4x4.Invert(copy.World, out var toPart))
        {
            ClearFaceCorners();
            return;
        }

        var corners = FaceCorners.Select(c => c.Vertex >= 0
            ? NavFaceCorner.Existing(c.Vertex, before.Vertices[c.Vertex])
            : NavFaceCorner.New(Vector3.Transform(c.World, toPart))).ToList();

        NavFaceEditResult result;
        try
        {
            result = NvmFaceAdd.Build(before, corners);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Face_Failed", copy.Room.FileName), e);
            result = new NavFaceEditResult { Error = "MAP_Navigation_Face_Error_File" };
        }

        if (!result.Ok)
        {
            SetFaceMessage(LOC.Get(result.Error), true);
            FaceCorners.RemoveAt(FaceCorners.Count - 1);
            return;
        }

        var boxes = data.Rooms.Where(r => r.FileName != null && r.FileName.Equals(copy.Room.FileName, StringComparison.OrdinalIgnoreCase))
            .Select(r => (r, before, result.Nvm)).ToList();
        var mcp = NvmEdit.WithRoomBoxes(data, data.Mcp, boxes);

        var files = new List<NavigationNvmEditAction.FileChange> { new(copy.Room.FileName, before, result.Nvm) };
        View.ViewportActionManager.ExecuteAction(new NavigationNvmEditAction(map, Nvm, files, data.Mcp, mcp, data.Mcg, data.Mcg, OnNvmEdited));

        // The copies belonged to the documents that went
        NvmVertexCopies.Clear();

        SetFaceMessage(LOC.Get("MAP_Navigation_Face_Added", copy.Room.FileName, result.Face, result.Joined, result.NewVertices));
        ClearFaceCorners();
    }

    #endregion

    #region Shared

    /// <summary>
    /// Where a corner would go: the target's vertex nearest the cursor's ray within the snap distance (only those in
    /// <paramref name="allowed"/>, when given), else a new vertex where the ray meets a drawn navmesh, else where it
    /// meets the height of the last corner. Null when none of those is found (the first corner must be on a navmesh).
    /// </summary>
    private FaceCornerPick PickFaceCorner(Ray ray, Vector3? onNavmesh, int vertexCount, Func<int, Vector3> worldVertex, HashSet<int> allowed)
    {
        var origin = ray.Origin;
        var direction = Vector3.Normalize(ray.Direction);

        int best = -1;
        float bestDistance = FaceSnapDistance;
        float bestAlong = float.MaxValue;

        if (FaceSnapDistance > 0)
        {
            for (int v = 0; v < vertexCount; v++)
            {
                if (allowed != null && !allowed.Contains(v))
                    continue;

                var p = worldVertex(v);
                float along = Vector3.Dot(p - origin, direction);
                if (along <= 0)
                    continue;

                float distance = Vector3.Distance(origin + direction * along, p);

                // The nearest to the ray, and of two as near, the nearer to the camera
                if (distance < bestDistance - 1e-4f || (distance <= bestDistance + 1e-4f && along < bestAlong))
                {
                    best = v;
                    bestDistance = distance;
                    bestAlong = along;
                }
            }
        }

        // Of a snap and a navmesh hit, the one nearer the camera (a vertex behind a wall isn't taken)
        if (best >= 0 && (onNavmesh == null || bestAlong <= Vector3.Distance(origin, onNavmesh.Value) + FaceSnapDistance))
            return new FaceCornerPick(best, worldVertex(best));

        if (onNavmesh is Vector3 point)
            return new FaceCornerPick(-1, point);

        if (FaceCorners.Count > 0 && IntersectVertexHeight(ray, FaceCorners[^1].World.Y, out var level))
            return new FaceCornerPick(-1, level);

        return null;
    }

    /// <summary>
    /// Add Face: a sphere on each corner placed and on the hovered one, and lines from each to the next. Remove Face: the
    /// outline of the face under the cursor.
    /// </summary>
    private void UpdateFaceAddDrawing(bool active)
    {
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        if (!active || IsBrushTool || scene == null)
        {
            HideFaceAddDrawing();
            return;
        }

        var cornerColor = ToVertexColor(CFG.Current.MapEditor_CollisionEditor_Vertex_Color);
        var hoverColor = ToVertexColor(CFG.Current.MapEditor_CollisionEditor_Cursor_Color);

        FaceLinePrim ??= new DbgPrimLineBatch(new List<(Vector3, Vector3)> { (Vector3.Zero, Vector3.UnitX) });
        FaceOrbs ??= Enumerable.Range(0, 4).Select(_ => MakeFaceProxy(scene, RenderableHelper._placementOrb, cornerColor)).ToArray();

        // Remove Face draws no corners
        var points = IsFaceTool ? FaceCorners.Select(c => c.World).ToList() : new List<Vector3>();
        if (IsFaceTool && FaceHover != null)
            points.Add(FaceHover.World);

        for (int i = 0; i < FaceOrbs.Length; i++)
        {
            var orb = FaceOrbs[i];

            if (i >= points.Count)
            {
                SetVertexProxyVisible(orb, false);
                continue;
            }

            var hovered = i == FaceCorners.Count;
            var snapped = (hovered ? FaceHover : FaceCorners[i]).Vertex >= 0;

            // A taken vertex is drawn larger than a new one
            var size = (snapped ? 1.5f : 1f) * VertexMaxMarkerSize;
            orb.World = Matrix4x4.CreateScale(size / VertexSphereRadius) * Matrix4x4.CreateTranslation(points[i]);
            SetFaceProxyColor(orb, hovered ? hoverColor : cornerColor);
            SetVertexProxyVisible(orb, true);
        }

        var segments = new List<(Vector3, Vector3)>();
        var loop = IsRemoveTool ? FaceRemoveOutline : points;
        for (int i = 0; i + 1 < loop.Count; i++)
            segments.Add((loop[i], loop[i + 1]));
        if (loop.Count >= 3 && (IsRemoveTool || loop.Count == 3))
            segments.Add((loop[^1], loop[0]));

        while (FaceLines.Count < segments.Count)
            FaceLines.Add(MakeFaceProxy(scene, FaceLinePrim, hoverColor));

        for (int i = 0; i < FaceLines.Count; i++)
        {
            var line = FaceLines[i];

            if (i >= segments.Count || Vector3.DistanceSquared(segments[i].Item1, segments[i].Item2) < 1e-10f)
            {
                SetVertexProxyVisible(line, false);
                continue;
            }

            line.World = FaceLineWorld(segments[i].Item1, segments[i].Item2);
            SetVertexProxyVisible(line, true);
        }
    }

    private static DebugPrimitiveRenderableProxy MakeFaceProxy(RenderScene scene, IDbgPrim prim, Color color)
        => new(scene.OpaqueRenderables, prim)
        {
            Pickable = false,
            RenderOverlay = true,
            BaseColor = color,
            HighlightedColor = color,
            Visible = false,
        };

    private static void SetFaceProxyColor(DebugPrimitiveRenderableProxy proxy, Color color)
    {
        if (proxy.BaseColor != color)
        {
            proxy.BaseColor = color;
            proxy.HighlightedColor = color;
        }
    }

    /// <summary>The world matrix that takes the unit line along X onto the line from a to b (as the Collision Editor's).</summary>
    private static Matrix4x4 FaceLineWorld(Vector3 a, Vector3 b)
    {
        var x = b - a;
        var direction = Vector3.Normalize(x);
        var helper = MathF.Abs(direction.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitX;
        var y = Vector3.Normalize(Vector3.Cross(direction, helper));
        var z = Vector3.Cross(direction, y);

        return new Matrix4x4(
            x.X, x.Y, x.Z, 0,
            y.X, y.Y, y.Z, 0,
            z.X, z.Y, z.Z, 0,
            a.X, a.Y, a.Z, 1);
    }

    private void HideFaceAddDrawing()
    {
        foreach (var proxy in FaceOrbs ?? Array.Empty<DebugPrimitiveRenderableProxy>())
            SetVertexProxyVisible(proxy, false);

        foreach (var proxy in FaceLines)
            SetVertexProxyVisible(proxy, false);
    }

    private void DisposeFaceAddDrawing()
    {
        foreach (var proxy in FaceOrbs ?? Array.Empty<DebugPrimitiveRenderableProxy>())
            proxy.Dispose();

        foreach (var proxy in FaceLines)
            proxy.Dispose();

        FaceOrbs = null;
        FaceLines.Clear();
    }

    #endregion
}
