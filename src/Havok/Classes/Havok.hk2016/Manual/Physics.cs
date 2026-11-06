using System.Numerics;
using Havok.Shared;

namespace Havok.hk2016;

// The cross-version collision and navigation views (Havok.Shared/Physics.cs); CollisionGeometry turns them into
// triangles for every SDK alike. Hand-written: the classes are generated as partial for them.

public partial class hknpPhysicsSceneData : IHavokPhysicsScene
{
    IEnumerable<IHavokPhysicsSystem> IHavokPhysicsScene.Systems => m_systemDatas.Where(s => s != null).Cast<IHavokPhysicsSystem>();
}

public partial class hknpPhysicsSystemData : IHavokPhysicsSystem
{
    IEnumerable<IHavokBody> IHavokPhysicsSystem.Bodies => m_bodyCinfos.Where(b => b != null).Cast<IHavokBody>();
}

public partial class hknpBodyCinfo : IHavokBody
{
    object? IHavokBody.Shape => m_shape;

    Vector3 IHavokBody.Position => new(m_position.X, m_position.Y, m_position.Z);

    Quaternion IHavokBody.Orientation => m_orientation;
}

public partial class hknpCompressedMeshShape : IHavokCompressedMesh
{
    private hknpCompressedMeshShapeTree? Tree => (m_data as hknpCompressedMeshShapeData)?.m_meshTree;

    int IHavokCompressedMesh.SectionCount => Tree?.m_sections.Count ?? 0;

    // hk_2014/2016 sections pack their counts: (first shared vertex index << 8 | packed vertex count), and
    // (first primitive << 8 | primitive count)
    HavokMeshSection IHavokCompressedMesh.GetSection(int index)
    {
        var s = Tree!.m_sections[index];
        return new(new Vector3(s.m_codecParms[0], s.m_codecParms[1], s.m_codecParms[2]),
            new Vector3(s.m_codecParms[3], s.m_codecParms[4], s.m_codecParms[5]),
            (int)s.m_firstPackedVertex, (int)(s.m_sharedVertices.m_data & 0xFF), (int)(s.m_sharedVertices.m_data >> 8),
            (int)(s.m_primitives.m_data >> 8), (int)(s.m_primitives.m_data & 0xFF));
    }

    int IHavokCompressedMesh.PrimitiveCount => Tree?.m_primitives.Count ?? 0;

    (byte A, byte B, byte C, byte D) IHavokCompressedMesh.GetPrimitive(int index)
    {
        byte[] i = Tree!.m_primitives[index].m_indices;
        return (i[0], i[1], i[2], i[3]);
    }

    IReadOnlyList<uint> IHavokCompressedMesh.PackedVertices => Tree?.m_packedVertices ?? (IReadOnlyList<uint>)Array.Empty<uint>();

    IReadOnlyList<ulong> IHavokCompressedMesh.SharedVertices => Tree?.m_sharedVertices ?? (IReadOnlyList<ulong>)Array.Empty<ulong>();

    IReadOnlyList<ushort> IHavokCompressedMesh.SharedVerticesIndex => Tree?.m_sharedVerticesIndex ?? (IReadOnlyList<ushort>)Array.Empty<ushort>();

    Vector3 IHavokCompressedMesh.DomainMin => Tree is { } t ? new(t.m_domain.m_min.X, t.m_domain.m_min.Y, t.m_domain.m_min.Z) : default;

    Vector3 IHavokCompressedMesh.DomainMax => Tree is { } t ? new(t.m_domain.m_max.X, t.m_domain.m_max.Y, t.m_domain.m_max.Z) : default;

    int IHavokCompressedMesh.BitsPerKey => (int)(Tree?.m_bitsPerKey ?? 0);
}

public partial class hknpExternMeshShape : IHavokTriangleMesh
{
    private hkGeometry? Geometry => (m_geometry as hknpDefaultExternMeshShapeGeometry)?.m_geometry;

    int IHavokTriangleMesh.VertexCount => Geometry?.m_vertices.Count ?? 0;

    Vector3 IHavokTriangleMesh.GetVertex(int index)
    {
        Vector4 v = Geometry!.m_vertices[index];
        return new Vector3(v.X, v.Y, v.Z);
    }

    int IHavokTriangleMesh.TriangleCount => Geometry?.m_triangles.Count ?? 0;

    (int A, int B, int C) IHavokTriangleMesh.GetTriangle(int index)
    {
        var t = Geometry!.m_triangles[index];
        return (t.m_a, t.m_b, t.m_c);
    }
}

public partial class hkaiNavMesh : IHavokNavMesh
{
    IReadOnlyList<Vector4> IHavokNavMesh.Vertices => m_vertices;

    int IHavokNavMesh.FaceCount => m_faces.Count;

    (int StartEdge, int EdgeCount, int Cluster) IHavokNavMesh.GetFace(int index) =>
        (m_faces[index].m_startEdgeIndex, m_faces[index].m_numEdges, m_faces[index].m_clusterIndex);

    int IHavokNavMesh.GetEdgeStart(int edge) => m_edges[edge].m_a;
}

public partial class hkaiDirectedGraphExplicitCost : IHavokDirectedGraph
{
    IReadOnlyList<Vector4> IHavokDirectedGraph.Positions => m_positions;

    int IHavokDirectedGraph.NodeCount => m_nodes.Count;

    (int StartEdge, int EdgeCount) IHavokDirectedGraph.GetNode(int index) => (m_nodes[index].m_startEdgeIndex, m_nodes[index].m_numEdges);

    int IHavokDirectedGraph.GetEdgeTarget(int edge) => (int)m_edges[edge].m_target;
}

public partial class hknpConvexPolytopeShape : IHavokConvexPolygons
{
    int IHavokConvexPolygons.VertexCount => m_vertices.Count;

    Vector3 IHavokConvexPolygons.GetVertex(int index) => new(m_vertices[index].X, m_vertices[index].Y, m_vertices[index].Z);

    int IHavokConvexPolygons.FaceCount => m_faces.Count;

    (int FirstIndex, int IndexCount) IHavokConvexPolygons.GetFace(int index) => (m_faces[index].m_firstIndex, m_faces[index].m_numIndices);

    int IHavokConvexPolygons.GetIndex(int index) => m_indices[index];
}
