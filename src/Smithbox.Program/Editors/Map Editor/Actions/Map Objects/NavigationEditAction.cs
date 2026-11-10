using Havok.Shared;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

/// <summary>A face's face data before and after an edit.</summary>
public record NavFaceDataChange(NavmeshFile File, int Face, int Before, int After);

/// <summary>An edge's flags and edge data before and after an edit.</summary>
public record NavEdgeChange(NavmeshFile File, int Edge, int FlagsBefore, int FlagsAfter, int DataBefore, int DataAfter);

/// <summary>
/// Edits of the Navigation tool that change values of a map's navmesh files but no indices: face data painted or typed,
/// and user edge flags and data. Connections stay valid, as no face or edge is renumbered. The edits are made to the
/// navmesh bank's Havok documents, which the map's save writes back (MapFileType.Navmesh).
/// </summary>
public class NavigationEditAction : ViewportAction
{
    private readonly MapContainer Map;
    private readonly IReadOnlyList<NavFaceDataChange> Faces;
    private readonly IReadOnlyList<NavEdgeChange> Edges;
    private readonly Action Changed;

    /// <param name="changed">Called after each apply, e.g. to redraw the overlay.</param>
    public NavigationEditAction(MapContainer map, IReadOnlyList<NavFaceDataChange> faces, IReadOnlyList<NavEdgeChange> edges, Action changed)
    {
        Map = map;
        Faces = faces ?? Array.Empty<NavFaceDataChange>();
        Edges = edges ?? Array.Empty<NavEdgeChange>();
        Changed = changed;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var change in Faces)
            ApplyFace(change, true);

        foreach (var change in Edges)
            ApplyEdge(change, true);

        Finish();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = Edges.Count - 1; i >= 0; i--)
            ApplyEdge(Edges[i], false);

        for (int i = Faces.Count - 1; i >= 0; i--)
            ApplyFace(Faces[i], false);

        Finish();
        return ActionEvent.NoEvent;
    }

    /// <summary>Sets a face's data to its value after, or before, the edit.</summary>
    public static void ApplyFace(NavFaceDataChange change, bool after)
    {
        var mesh = change.File.Mesh;
        int index = change.Face * Math.Max(1, mesh.m_faceDataStriding);

        if (index < mesh.m_faceData.Count)
            mesh.m_faceData[index] = after ? change.After : change.Before;
    }

    /// <summary>Sets an edge's flags and data to their values after, or before, the edit.</summary>
    public static void ApplyEdge(NavEdgeChange change, bool after)
    {
        var mesh = change.File.Mesh;

        if (change.Edge >= mesh.m_edges.Count)
            return;

        mesh.m_edges[change.Edge].m_flags = (HavokNavEdgeFlags)(after ? change.FlagsAfter : change.FlagsBefore);

        int index = change.Edge * Math.Max(1, mesh.m_edgeDataStriding);
        if (mesh.m_edgeDataStriding > 0 && index < mesh.m_edgeData.Count)
            mesh.m_edgeData[index] = after ? change.DataAfter : change.DataBefore;
    }

    private void Finish()
    {
        Map?.MarkDirty(MapFileType.Navmesh);
        Changed?.Invoke();
    }
}
