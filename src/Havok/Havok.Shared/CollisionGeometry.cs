using System.Numerics;

namespace Havok.Shared;

/// <summary>
/// Turns the collision views (Physics.cs) into world-space triangles, the same way for every SDK. It's the union of what
/// Smithbox's per-library builders handled: compound, compressed, extern and convex polytope (or hull) shapes. A
/// primitive whose vertex can't be resolved is dropped, and a body without a rotation is placed unrotated.
/// </summary>
public static class CollisionGeometry
{
    /// <summary>The bodies of a file: those of its first physics scene, else of its loose physics systems (BB).</summary>
    public static IEnumerable<IHavokBody> Bodies(IEnumerable<object> objects)
    {
        var list = objects as IReadOnlyCollection<object> ?? objects.ToList();
        if (list.OfType<IHavokPhysicsScene>().FirstOrDefault() is { } scene)
            return scene.Systems.Where(s => s != null).SelectMany(s => s.Bodies).Where(b => b != null);

        return list.OfType<IHavokPhysicsSystem>().SelectMany(s => s.Bodies).Where(b => b != null);
    }

    /// <summary>
    /// A file's collision as triangle lists in world space, one per submesh: a body each for the hknp games, a mesh
    /// storage each (untransformed, as before) for the hkp games. Submeshes without triangles are left out. A body
    /// that fails to decode is passed to <paramref name="onError" /> and skipped.
    /// </summary>
    public static List<List<Vector3>> Submeshes(IEnumerable<object> objects, Action<object?, Exception>? onError = null)
    {
        var list = objects as IReadOnlyCollection<object> ?? objects.ToList();
        var result = new List<List<Vector3>>();

        if (list.Any(o => o is IHavokPhysicsScene or IHavokPhysicsSystem))
        {
            foreach (IHavokBody body in Bodies(list))
            {
                var triangles = new List<Vector3>();
                try { AppendTriangles(body.Shape, BodyTransform(body), triangles); }
                catch (Exception e) { onError?.Invoke(body.Shape, e); continue; }
                if (triangles.Count > 0) result.Add(triangles);
            }
            return result;
        }

        foreach (IHavokMeshStorageShape shape in list.OfType<IHavokMeshStorageShape>())
        {
            foreach (IHavokTriangleMesh storage in shape.MeshStorages)
            {
                var triangles = new List<Vector3>();
                try { AppendTriangles(storage, Matrix4x4.Identity, triangles); }
                catch (Exception e) { onError?.Invoke(shape, e); continue; }
                if (triangles.Count > 0) result.Add(triangles);
            }
        }
        return result;
    }

    /// <summary>
    /// A navmesh's faces as a triangle list: each face is a convex polygon, fanned around its first edge's vertex.
    /// </summary>
    public static List<Vector3> NavMeshTriangles(IHavokNavMesh mesh)
    {
        var triangles = new List<Vector3>();
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            var (start, count, _) = mesh.GetFace(f);
            for (int t = 0; t < count - 2; t++)
            {
                int end = t + 2 >= count ? start : start + t + 2;
                triangles.Add(AsVector3(mesh.Vertices[mesh.GetEdgeStart(start)]));
                triangles.Add(AsVector3(mesh.Vertices[mesh.GetEdgeStart(start + t + 1)]));
                triangles.Add(AsVector3(mesh.Vertices[mesh.GetEdgeStart(end)]));
            }
        }
        return triangles;
    }

    /// <summary>A cost graph's edges as line segments (pairs of points), from each node to each of its targets.</summary>
    public static List<Vector3> GraphSegments(IHavokDirectedGraph graph)
    {
        var points = new List<Vector3>();
        for (int n = 0; n < graph.NodeCount; n++)
        {
            var (start, count) = graph.GetNode(n);
            for (int e = 0; e < count; e++)
            {
                points.Add(AsVector3(graph.Positions[n]));
                points.Add(AsVector3(graph.Positions[graph.GetEdgeTarget(start + e)]));
            }
        }
        return points;
    }

    private static Vector3 AsVector3(Vector4 v) => new(v.X, v.Y, v.Z);

    /// <summary>A body's world transform. Bodies without a rotation leave it zeroed.</summary>
    public static Matrix4x4 BodyTransform(IHavokBody body)
    {
        Quaternion orientation = body.Orientation;
        if (orientation.LengthSquared() < 1e-6f) orientation = Quaternion.Identity;
        return Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(body.Position);
    }

    /// <summary>
    /// Appends a shape's triangles to a triangle list (three vertices a triangle, in order), transformed.
    /// Returns false if no view describes the shape.
    /// </summary>
    public static bool AppendTriangles(object? shape, Matrix4x4 transform, List<Vector3> triangles)
    {
        switch (shape)
        {
            case IHavokCompoundShape compound:
                foreach (var (child, local) in compound.Instances) AppendTriangles(child, local * transform, triangles);
                return true;
            case IHavokCompressedMesh mesh:
                foreach (var (_, a, b, c) in CompressedTriangles(mesh, keyed: false))
                {
                    triangles.Add(Vector3.Transform(a, transform));
                    triangles.Add(Vector3.Transform(b, transform));
                    triangles.Add(Vector3.Transform(c, transform));
                }
                return true;
            case IHavokTriangleMesh mesh:
                for (int i = 0; i < mesh.TriangleCount; i++)
                {
                    var (a, b, c) = mesh.GetTriangle(i);
                    if (!Valid(a, mesh.VertexCount) || !Valid(b, mesh.VertexCount) || !Valid(c, mesh.VertexCount)) continue;
                    triangles.Add(Vector3.Transform(mesh.GetVertex(a), transform));
                    triangles.Add(Vector3.Transform(mesh.GetVertex(b), transform));
                    triangles.Add(Vector3.Transform(mesh.GetVertex(c), transform));
                }
                return true;
            case IHavokConvexPolygons hull:
                // each face is a convex polygon: a fan around its first vertex
                for (int f = 0; f < hull.FaceCount; f++)
                {
                    var (first, count) = hull.GetFace(f);
                    for (int i = 1; i < count - 1; i++)
                    {
                        int a = hull.GetIndex(first), b = hull.GetIndex(first + i), c = hull.GetIndex(first + i + 1);
                        if (!Valid(a, hull.VertexCount) || !Valid(b, hull.VertexCount) || !Valid(c, hull.VertexCount)) continue;
                        triangles.Add(Vector3.Transform(hull.GetVertex(a), transform));
                        triangles.Add(Vector3.Transform(hull.GetVertex(b), transform));
                        triangles.Add(Vector3.Transform(hull.GetVertex(c), transform));
                    }
                }
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// A compressed mesh's triangles in local space, with their shape keys. A quad gives two triangles: its first,
    /// second and third vertices (key half 0), then its first, third and fourth (half 1). Marker primitives (DEAD)
    /// and primitives with a vertex that isn't stored are skipped.
    /// </summary>
    public static IEnumerable<(uint Key, Vector3 A, Vector3 B, Vector3 C)> CompressedTriangles(IHavokCompressedMesh mesh, bool keyed = true)
    {
        if (mesh.PackedVertices.Count == 0 && mesh.SharedVertices.Count == 0) yield break;

        for (int s = 0; s < mesh.SectionCount; s++)
        {
            HavokMeshSection section = mesh.GetSection(s);
            for (int p = 0; p < section.PrimitiveCount; p++)
            {
                int primitiveIndex = section.FirstPrimitive + p;
                if (primitiveIndex >= mesh.PrimitiveCount) break;
                var (i0, i1, i2, i3) = mesh.GetPrimitive(primitiveIndex);
                if (i0 == 0xDE && i1 == 0xAD && i2 == 0xDE && i3 == 0xAD) continue;

                bool quad = i2 != i3;
                if (!TryVertex(mesh, section, i0, out Vector3 a) || !TryVertex(mesh, section, i1, out Vector3 b) ||
                    !TryVertex(mesh, section, i2, out Vector3 c)) continue;
                Vector3 d = default;
                if (quad && !TryVertex(mesh, section, i3, out d)) continue;

                yield return (keyed ? ShapeKey(mesh.BitsPerKey, s, p, 0) : 0, a, b, c);
                if (quad) yield return (keyed ? ShapeKey(mesh.BitsPerKey, s, p, 1) : 0, a, c, d);
            }
        }
    }

    /// <summary>
    /// A triangle's shape key: section, primitive within it and quad half, filled from the top bit down, with the
    /// unused bits set.
    /// </summary>
    public static uint ShapeKey(int bitsPerKey, int section, int primitive, int quadHalf)
    {
        uint key = (uint)((section << 8) | (primitive << 1) | quadHalf);
        int unused = 32 - bitsPerKey;
        return (key << unused) | (uint)((1L << unused) - 1);
    }

    public static Vector3 DecompressPackedVertex(uint vertex, Vector3 scale, Vector3 offset) => new(
        (vertex & 0x7FF) * scale.X + offset.X,
        ((vertex >> 11) & 0x7FF) * scale.Y + offset.Y,
        ((vertex >> 22) & 0x3FF) * scale.Z + offset.Z);

    public static Vector3 DecompressSharedVertex(ulong vertex, Vector3 min, Vector3 max)
    {
        float sx = (max.X - min.X) / ((1 << 21) - 1);
        float sy = (max.Y - min.Y) / ((1 << 21) - 1);
        float sz = (max.Z - min.Z) / ((1 << 22) - 1);
        return new Vector3((vertex & 0x1FFFFF) * sx + min.X, ((vertex >> 21) & 0x1FFFFF) * sy + min.Y, ((vertex >> 42) & 0x3FFFFF) * sz + min.Z);
    }

    private static bool TryVertex(IHavokCompressedMesh mesh, HavokMeshSection section, byte index, out Vector3 vertex)
    {
        vertex = default;
        if (index < section.PackedVertexCount)
        {
            int packed = section.FirstPackedVertex + index;
            if (packed >= mesh.PackedVertices.Count) return false;
            vertex = DecompressPackedVertex(mesh.PackedVertices[packed], section.Scale, section.Offset);
            return true;
        }

        int sharedIndexIndex = section.FirstSharedVertexIndex + index - section.PackedVertexCount;
        if (sharedIndexIndex < 0 || sharedIndexIndex >= mesh.SharedVerticesIndex.Count) return false;
        int shared = mesh.SharedVerticesIndex[sharedIndexIndex];
        if (shared >= mesh.SharedVertices.Count) return false;
        vertex = DecompressSharedVertex(mesh.SharedVertices[shared], mesh.DomainMin, mesh.DomainMax);
        return true;
    }

    private static bool Valid(int index, int count) => index >= 0 && index < count;
}
