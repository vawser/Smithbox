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

/// <summary>
/// A compressed mesh's extern shapes (hknp 2018/2019): convex shapes placed with the mesh, such as the boxes and
/// cylinders of some AC6 assets, which the mesh's own primitives don't hold.
/// </summary>
public interface IHavokExternShapes
{
    IEnumerable<(object Shape, Matrix4x4 Transform)> ExternShapes { get; }
}

/// <summary>hkpBoxShape: a box about the origin, its half extents grown by the convex radius.</summary>
public interface IHavokBox
{
    Vector3 HalfExtents { get; }
}

/// <summary>hkpCylinderShape: a cylinder between two points.</summary>
public interface IHavokCylinder
{
    Vector3 A { get; }

    Vector3 B { get; }

    float Radius { get; }
}

/// <summary>hkpCapsuleShape: the points within a radius of a segment.</summary>
public interface IHavokCapsule
{
    Vector3 A { get; }

    Vector3 B { get; }

    float Radius { get; }
}

/// <summary>hkpSphereShape: a sphere about the origin.</summary>
public interface IHavokSphere
{
    float Radius { get; }
}

/// <summary>hkpConvexVerticesShape: a convex shape given as its points; the hull is made from them.</summary>
public interface IHavokConvexPoints
{
    IReadOnlyList<Vector3> Points { get; }
}

/// <summary>Helpers for the primitive views of each SDK.</summary>
public static class HavokViewMath
{
    /// <summary>
    /// An hkTransform as read (rotation rows, translation in the fourth row), with its fourth column cleared: the
    /// reader keeps the vectors' w there, and a zero M44 drops the translation when matrices are combined.
    /// </summary>
    public static Matrix4x4 Clean(Matrix4x4 m)
    {
        m.M14 = 0;
        m.M24 = 0;
        m.M34 = 0;
        m.M44 = 1;
        return m;
    }

    /// <summary>An hkQsTransform as a matrix: scale (none if zero), then rotation, then translation.</summary>
    public static Matrix4x4 QsTransform(Vector4 translation, Quaternion rotation, Vector4 scale)
    {
        var s = new Vector3(scale.X, scale.Y, scale.Z);
        if (s == Vector3.Zero) s = Vector3.One;
        if (rotation.LengthSquared() < 1e-6f) rotation = Quaternion.Identity;
        return Matrix4x4.CreateScale(s) * Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(rotation))
            * Matrix4x4.CreateTranslation(translation.X, translation.Y, translation.Z);
    }

    /// <summary>The rotation of an hkTransform as read.</summary>
    public static Quaternion Rotation(Matrix4x4 m)
    {
        var clean = Clean(m);
        clean.Translation = Vector3.Zero;
        var q = Quaternion.CreateFromRotationMatrix(clean);
        return float.IsFinite(q.X) && q.LengthSquared() > 1e-6f ? Quaternion.Normalize(q) : Quaternion.Identity;
    }

    /// <summary>The points of hkpConvexVerticesShape's rotated vertices: blocks of four, x, y and z in rows.</summary>
    public static List<Vector3> FourTransposedPoints(IEnumerable<Matrix4x4> blocks, int count)
    {
        var points = new List<Vector3>(count);

        foreach (var m in blocks)
        {
            for (var k = 0; k < 4 && points.Count < count; k++)
                points.Add(new Vector3(m[0, k], m[1, k], m[2, k]));
        }

        return points;
    }

    /// <summary>As above, from 5.5's blocks of three vectors (x, y, z of four points each).</summary>
    public static List<Vector3> FourVectorsPoints(IEnumerable<(Vector4 X, Vector4 Y, Vector4 Z)> blocks, int count)
    {
        var points = new List<Vector3>(count);

        foreach (var (x, y, z) in blocks)
        {
            for (var k = 0; k < 4 && points.Count < count; k++)
                points.Add(new Vector3(Get(x, k), Get(y, k), Get(z, k)));
        }

        return points;
    }

    private static float Get(Vector4 v, int i) => i switch { 0 => v.X, 1 => v.Y, 2 => v.Z, _ => v.W };
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
