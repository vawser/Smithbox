using System.Numerics;

namespace Havok.Shared;

// Cross-version views of collision and navigation data. Every SDK's class set implements them in Manual partials, and
// CollisionGeometry turns them into triangles the same way for every game. They expose data, not algorithms, so the
// decoding lives in one place.

/// <summary>hknpPhysicsSceneData: the physics systems of a file.</summary>
public interface IHavokPhysicsScene
{
    IEnumerable<IHavokPhysicsSystem> Systems { get; }
}

/// <summary>hknpPhysicsSystemData: its bodies.</summary>
public interface IHavokPhysicsSystem
{
    IEnumerable<IHavokBody> Bodies { get; }
}

/// <summary>hknpBodyCinfo: a shape placed in the world.</summary>
public interface IHavokBody
{
    /// <summary>The body's shape (any of the shape views below, or a shape no view describes).</summary>
    object? Shape { get; }

    Vector3 Position { get; }

    /// <summary>As stored: bodies without a rotation can leave it zeroed.</summary>
    Quaternion Orientation { get; }
}

/// <summary>One section of a compressed mesh, with the counts the SDKs store in different ways normalised.</summary>
public readonly record struct HavokMeshSection(
    Vector3 Offset,
    Vector3 Scale,
    int FirstPackedVertex,
    int PackedVertexCount,
    int FirstSharedVertexIndex,
    int FirstPrimitive,
    int PrimitiveCount);

/// <summary>hknpCompressedMeshShape (and fsnpCustomParamCompressedMeshShape): the data of its mesh tree.</summary>
public interface IHavokCompressedMesh
{
    int SectionCount { get; }

    HavokMeshSection GetSection(int index);

    /// <summary>A primitive's four vertex indices: a triangle when the last two are equal, else a quad.</summary>
    (byte A, byte B, byte C, byte D) GetPrimitive(int index);

    int PrimitiveCount { get; }

    /// <summary>11/11/10-bit vertices, relative to their section's offset and scale.</summary>
    IReadOnlyList<uint> PackedVertices { get; }

    /// <summary>21/21/22-bit vertices, relative to the domain.</summary>
    IReadOnlyList<ulong> SharedVertices { get; }

    /// <summary>Where each section's shared vertices are, past its packed ones.</summary>
    IReadOnlyList<ushort> SharedVerticesIndex { get; }

    Vector3 DomainMin { get; }

    Vector3 DomainMax { get; }

    /// <summary>How many of a shape key's top bits are used.</summary>
    int BitsPerKey { get; }
}

/// <summary>A plain triangle list: hknpExternMeshShape's default geometry, an hkp mesh storage.</summary>
public interface IHavokTriangleMesh
{
    int VertexCount { get; }

    Vector3 GetVertex(int index);

    int TriangleCount { get; }

    (int A, int B, int C) GetTriangle(int index);
}

/// <summary>A convex hull stored as polygons: hknpConvexPolytopeShape, or hknpConvexShape's hull in 2019.</summary>
public interface IHavokConvexPolygons
{
    int VertexCount { get; }

    Vector3 GetVertex(int index);

    int FaceCount { get; }

    (int FirstIndex, int IndexCount) GetFace(int index);

    int GetIndex(int index);
}

/// <summary>hknpCompoundShape: shapes placed within one body.</summary>
public interface IHavokCompoundShape
{
    /// <summary>The used instances, with their local transforms (scale included).</summary>
    IEnumerable<(object Shape, Matrix4x4 Transform)> Instances { get; }
}

/// <summary>hkpStorageExtendedMeshShape: the older (hkp) games' mesh collision.</summary>
public interface IHavokMeshStorageShape
{
    IEnumerable<IHavokTriangleMesh> MeshStorages { get; }
}

/// <summary>hkaiNavMesh.</summary>
public interface IHavokNavMesh
{
    IReadOnlyList<Vector4> Vertices { get; }

    int FaceCount { get; }

    (int StartEdge, int EdgeCount, int Cluster) GetFace(int index);

    /// <summary>The vertex an edge starts at.</summary>
    int GetEdgeStart(int edge);
}

/// <summary>hkaiDirectedGraphExplicitCost: the navmesh's cost graph.</summary>
public interface IHavokDirectedGraph
{
    IReadOnlyList<Vector4> Positions { get; }

    int NodeCount { get; }

    (int StartEdge, int EdgeCount) GetNode(int index);

    int GetEdgeTarget(int edge);
}
