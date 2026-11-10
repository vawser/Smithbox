using Havok.Shared;

namespace StudioCore.Editors.MapEditor;

// New Havok objects for the Navigation tool's edits, made through the editable views of Havok.Shared (NavMeshEdit.cs),
// so an edit adds objects of whichever class set the file was read into. Kept free of the rest of Smithbox, so
// Tools/NavSurvey can link it.

public static class NavHavok
{
    /// <summary>A new edge of the mesh's class set, not yet added.</summary>
    public static IHavokNavEdge NewEdge(this IHavokNavMeshEdit mesh, int a, int b, uint oppositeEdge, uint oppositeFace,
        HavokNavEdgeFlags flags, float userEdgeCost = 0)
    {
        var edge = mesh.m_edges.New();
        edge.m_a = a;
        edge.m_b = b;
        edge.m_oppositeEdge = oppositeEdge;
        edge.m_oppositeFace = oppositeFace;
        edge.m_flags = flags;
        edge.m_userEdgeCost = userEdgeCost;
        return edge;
    }

    /// <summary>A new face of the mesh's class set, not yet added.</summary>
    public static IHavokNavFace NewFace(this IHavokNavMeshEdit mesh, int startEdgeIndex, short numEdges, int startUserEdgeIndex,
        short numUserEdges, short clusterIndex, ushort padding)
    {
        var face = mesh.m_faces.New();
        face.m_startEdgeIndex = startEdgeIndex;
        face.m_numEdges = numEdges;
        face.m_startUserEdgeIndex = startUserEdgeIndex;
        face.m_numUserEdges = numUserEdges;
        face.m_clusterIndex = clusterIndex;
        face.m_padding = padding;
        return face;
    }

    /// <summary>A new edge clearance entry of the cache's class set, not yet added.</summary>
    public static IHavokNavMcp NewMcp(this IHavokNavClearanceCache cache, byte interpolant, byte clearance)
    {
        var mcp = cache.m_mcpData.New();
        mcp.m_interpolant = interpolant;
        mcp.m_clearance = clearance;
        return mcp;
    }

    /// <summary>Sets a box from its corners.</summary>
    public static void Set(this IHavokAabb aabb, System.Numerics.Vector4 min, System.Numerics.Vector4 max)
    {
        aabb.m_min = min;
        aabb.m_max = max;
    }
}
