using Havok.Shared;
using StudioCore.Application;
using System;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Going to a Check issue (Navigation Editor UX Analysis, R6): a click on an issue's row draws its navmesh's set, picks
/// its face, chooses its gate or selects its model, and frames it in the viewport. The issue only holds a NameID, so
/// the navmesh is found again in the drawn data once the overlay is up to date.
/// </summary>
public partial class NavigationTool
{
    /// <summary>The issue last gone to, drawn as the selected row.</summary>
    private NavIssue GoToIssue;

    /// <summary>Set by a click: the go-to waits until the overlay of the issue's set is drawn.</summary>
    private bool GoToPending;

    /// <summary>What the last go-to did, or why it couldn't.</summary>
    private string GoToMessage;
    private bool GoToMessageIsError;

    /// <summary>Whether a row can be gone to: it names a navmesh of the checked map.</summary>
    private bool CanGoTo(NavIssue issue)
        => issue.NameID != null && Current != null && issue.MapId == Current.MapId;

    private void StartGoTo(NavIssue issue)
    {
        GoToIssue = issue;
        GoToMessage = null;

        var entry = Current.Map.FindNavmesh((int)issue.NameID.Value);
        if (entry == null)
        {
            GoToMessage = LOC.Get("MAP_Navigation_Issue_Go_To_Missing", issue.NameID.Value);
            GoToMessageIsError = true;
            return;
        }

        // Draw the issue's set; the go-to itself waits for that overlay
        var set = NavigationMap.SetOf(entry, Bank.Profile);
        if (!ShowOverlay)
        {
            ShowOverlay = true;
            OverlayDirty = true;
        }
        if (OverlaySet != set)
        {
            OverlaySet = set;
            OverlayDirty = true;
        }

        GoToPending = true;
    }

    /// <summary>Called by the Check tab each frame: finishes a go-to once the overlay is drawn.</summary>
    private void UpdateGoTo()
    {
        if (!GoToPending || OverlayDirty || OverlayNavigation == null || OverlayContainer == null || GoToIssue == null)
            return;

        if (OverlayNavigation.MapId != GoToIssue.MapId)
        {
            GoToPending = false;
            return;
        }

        GoToPending = false;
        var issue = GoToIssue;
        var entry = OverlayNavigation.FindNavmesh((int)issue.NameID.Value);

        if (entry == null)
        {
            GoToMessage = LOC.Get("MAP_Navigation_Issue_Go_To_Missing", issue.NameID.Value);
            GoToMessageIsError = true;
            return;
        }

        var world = EntryWorld(OverlayContainer, entry);
        var file = OverlayNavigation.FileOf(entry);
        var mesh = file?.Mesh;
        GoToMessageIsError = false;

        // A gate: choose it for the Gates tab, and frame its point
        if (issue.Gate is int gate && gate >= 0 && gate < entry.GateNodes.Count)
        {
            ChooseGates(entry, gate);
            var at = Vector3.Transform(entry.GateNodes[gate].Position, MapWorld(OverlayContainer));
            Frame(new BoundingBox(at, at), 2f);
            GoToMessage = LOC.Get("MAP_Navigation_Issue_Go_To_Gate", gate, issue.NameID.Value);
            return;
        }

        // A face: pick it, and frame it
        if (issue.Face is int face && mesh != null && face >= 0 && face < mesh.m_faces.Count)
        {
            var box = FaceBounds(mesh, face, world);
            SetPicked(new NavFaceHit(entry, file, face, 0, box.GetCenter()));
            Frame(box, 2f);
            GoToMessage = LOC.Get("MAP_Navigation_Issue_Go_To_Face", face, file.Name);
            return;
        }

        // Otherwise the navmesh: select its model, and frame the whole mesh
        SelectedModelID = entry.ModelID;
        if (OverlayOnlySelectedModel)
            OverlayDirty = true;

        if (mesh != null && mesh.m_vertices.Count > 0)
            Frame(MeshBounds(mesh, world), 5f);

        GoToMessage = LOC.Get("MAP_Navigation_Issue_Go_To_Navmesh", issue.NameID.Value, entry.ModelID);
    }

    private void Frame(BoundingBox box, float distance)
        => View.ViewportHandler.ActiveViewport?.Viewport.FrameBox(box, Vector3.Zero, distance);

    private static BoundingBox FaceBounds(IHavokNavMeshEdit mesh, int face, Matrix4x4 world)
    {
        var f = mesh.m_faces[face];
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        for (int k = f.m_startEdgeIndex; k < f.m_startEdgeIndex + f.m_numEdges; k++)
        {
            var v = mesh.m_vertices[(int)mesh.m_edges[k].m_a];
            var p = Vector3.Transform(new Vector3(v.X, v.Y, v.Z), world);
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        return new BoundingBox(min, max);
    }

    private static BoundingBox MeshBounds(IHavokNavMeshEdit mesh, Matrix4x4 world)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var v in mesh.m_vertices)
        {
            var p = Vector3.Transform(new Vector3(v.X, v.Y, v.Z), world);
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        return new BoundingBox(min, max);
    }
}
