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

        if (list.OfType<IHavokPhysicsSystem>().Any())
            return list.OfType<IHavokPhysicsSystem>().SelectMany(s => s.Bodies).Where(b => b != null);

        // The hkp games (DES, DS1, DS1R, DS2S): their rigid bodies aren't held in a system view
        return list.OfType<IHavokBody>();
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

        // No mesh: the hkp rigid bodies of an object (DES, DS1, DS1R, DS2S), each its primitive shapes placed by the
        // body's transform. Map collision always has its mesh, so it is read as before.
        if (result.Count == 0)
        {
            foreach (IHavokBody body in list.OfType<IHavokBody>())
            {
                var triangles = new List<Vector3>();
                try { AppendTriangles(body.Shape, BodyTransform(body), triangles); }
                catch (Exception e) { onError?.Invoke(body.Shape, e); continue; }
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
                if (shape is IHavokExternShapes externs)
                    foreach (var (child, local) in externs.ExternShapes) AppendTriangles(child, local * transform, triangles);
                return true;
            case IHavokMeshStorageShape storages:
                foreach (var storage in storages.MeshStorages) AppendTriangles(storage, transform, triangles);
                return true;
            case IHavokBox box:
                AppendBox(box.HalfExtents, transform, triangles);
                return true;
            case IHavokCylinder cylinder:
                AppendCylinder(cylinder.A, cylinder.B, cylinder.Radius, transform, triangles, caps: false);
                return true;
            case IHavokCapsule capsule:
                AppendCylinder(capsule.A, capsule.B, capsule.Radius, transform, triangles, caps: true);
                return true;
            case IHavokSphere sphere:
                AppendCylinder(Vector3.Zero, Vector3.Zero, sphere.Radius, transform, triangles, caps: true);
                return true;
            case IHavokConvexPoints points:
                AppendHull(points.Points, transform, triangles);
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

    #region Primitives

    private static void Add(List<Vector3> triangles, Matrix4x4 transform, Vector3 a, Vector3 b, Vector3 c)
    {
        triangles.Add(Vector3.Transform(a, transform));
        triangles.Add(Vector3.Transform(b, transform));
        triangles.Add(Vector3.Transform(c, transform));
    }

    /// <summary>A box about the origin: 12 triangles, wound outward.</summary>
    public static void AppendBox(Vector3 half, Matrix4x4 transform, List<Vector3> triangles)
    {
        Vector3 C(int x, int y, int z) => new(x * half.X, y * half.Y, z * half.Z);

        // Each face by its outward axis and two in-plane axes, as a quad of two triangles
        foreach (var (n, u, v) in new[]
        {
            (new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1)),
            (new Vector3(-1, 0, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0)),
            (new Vector3(0, 1, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0)),
            (new Vector3(0, -1, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 1)),
            (new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(0, 1, 0)),
            (new Vector3(0, 0, -1), new Vector3(0, 1, 0), new Vector3(1, 0, 0)),
        })
        {
            var p00 = (n - u - v) * half;
            var p10 = (n + u - v) * half;
            var p11 = (n + u + v) * half;
            var p01 = (n - u + v) * half;
            Add(triangles, transform, p00, p10, p11);
            Add(triangles, transform, p00, p11, p01);
        }
    }

    /// <summary>
    /// A cylinder from a to b (16 sides), or with caps a capsule (half spheres on the ends, 4 rings each); a and b equal
    /// with caps is a sphere.
    /// </summary>
    public static void AppendCylinder(Vector3 a, Vector3 b, float radius, Matrix4x4 transform, List<Vector3> triangles, bool caps)
    {
        const int sides = 16;
        const int rings = 4;

        if (radius <= 0)
            return;

        var axis = b - a;
        var length = axis.Length();
        var dir = length > 1e-6f ? axis / length : Vector3.UnitY;
        var helper = MathF.Abs(dir.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitX;
        var u = Vector3.Normalize(Vector3.Cross(dir, helper));
        var w = Vector3.Cross(dir, u);

        Vector3 Ring(int i) => u * MathF.Cos(i * MathF.Tau / sides) + w * MathF.Sin(i * MathF.Tau / sides);

        // The side
        if (length > 1e-6f)
        {
            for (var i = 0; i < sides; i++)
            {
                var r0 = Ring(i) * radius;
                var r1 = Ring(i + 1) * radius;
                Add(triangles, transform, a + r0, b + r1, b + r0);
                Add(triangles, transform, a + r0, a + r1, b + r1);
            }
        }

        if (!caps)
        {
            // Flat ends
            for (var i = 0; i < sides; i++)
            {
                Add(triangles, transform, a, a + Ring(i + 1) * radius, a + Ring(i) * radius);
                Add(triangles, transform, b, b + Ring(i) * radius, b + Ring(i + 1) * radius);
            }

            return;
        }

        // Half spheres: rings from the equator to the pole, on each end
        foreach (var (centre, pole) in new[] { (b, dir), (a, -dir) })
        {
            for (var k = 0; k < rings; k++)
            {
                var t0 = k * MathF.PI / 2 / rings;
                var t1 = (k + 1) * MathF.PI / 2 / rings;

                for (var i = 0; i < sides; i++)
                {
                    Vector3 P(float t, int j) => centre + (Ring(j) * MathF.Cos(t) + pole * MathF.Sin(t)) * radius;
                    Add(triangles, transform, P(t0, i), P(t0, i + 1), P(t1, i + 1));
                    Add(triangles, transform, P(t0, i), P(t1, i + 1), P(t1, i));
                }
            }
        }
    }

    /// <summary>The convex hull of a point set, as triangles (incremental hull; duplicate and inner points are dropped).</summary>
    public static void AppendHull(IReadOnlyList<Vector3> points, Matrix4x4 transform, List<Vector3> triangles)
    {
        foreach (var (a, b, c) in ConvexHull(points))
            Add(triangles, transform, points[a], points[b], points[c]);
    }

    /// <summary>
    /// The faces of the convex hull of a point set (indices into it, wound outward). Incremental: a starting
    /// tetrahedron, then each point outside the hull replaces the faces it sees, joined to the edges around them.
    /// Fewer than 4 points, or points all in a plane, give no faces.
    /// </summary>
    public static List<(int A, int B, int C)> ConvexHull(IReadOnlyList<Vector3> points)
    {
        var faces = new List<(int A, int B, int C)>();
        var n = points.Count;

        if (n < 4)
            return faces;

        var min = points[0];
        var max = points[0];
        foreach (var p in points) { min = Vector3.Min(min, p); max = Vector3.Max(max, p); }
        var eps = MathF.Max(1e-6f, (max - min).Length() * 1e-5f);

        // The starting tetrahedron: two far points, the point furthest from their line, the one furthest from that plane
        int i0 = 0, i1 = -1, i2 = -1, i3 = -1;
        var best = 0f;
        for (var i = 1; i < n; i++) { var d = Vector3.DistanceSquared(points[i], points[i0]); if (d > best) { best = d; i1 = i; } }
        if (i1 < 0 || best < eps * eps) return faces;

        best = 0;
        var line = Vector3.Normalize(points[i1] - points[i0]);
        for (var i = 0; i < n; i++)
        {
            var v = points[i] - points[i0];
            var d = (v - line * Vector3.Dot(v, line)).LengthSquared();
            if (d > best) { best = d; i2 = i; }
        }
        if (i2 < 0 || best < eps * eps) return faces;

        best = 0;
        var normal = Vector3.Normalize(Vector3.Cross(points[i1] - points[i0], points[i2] - points[i0]));
        for (var i = 0; i < n; i++)
        {
            var d = MathF.Abs(Vector3.Dot(points[i] - points[i0], normal));
            if (d > best) { best = d; i3 = i; }
        }
        if (i3 < 0 || best < eps) return faces;

        var centroid = (points[i0] + points[i1] + points[i2] + points[i3]) / 4;

        // A face facing away from the centroid
        (int, int, int) Outward(int a, int b, int c)
        {
            var nrm = Vector3.Cross(points[b] - points[a], points[c] - points[a]);
            return Vector3.Dot(nrm, points[a] - centroid) >= 0 ? (a, b, c) : (a, c, b);
        }

        faces.Add(Outward(i0, i1, i2));
        faces.Add(Outward(i0, i1, i3));
        faces.Add(Outward(i0, i2, i3));
        faces.Add(Outward(i1, i2, i3));

        bool Sees((int A, int B, int C) f, Vector3 p)
        {
            var a = points[f.A];
            var nrm = Vector3.Cross(points[f.B] - a, points[f.C] - a);
            var length = nrm.Length();
            return length > 1e-12f && Vector3.Dot(nrm / length, p - a) > eps;
        }

        for (var i = 0; i < n; i++)
        {
            if (i == i0 || i == i1 || i == i2 || i == i3)
                continue;

            var p = points[i];
            var visible = new List<int>();

            for (var f = 0; f < faces.Count; f++)
                if (Sees(faces[f], p))
                    visible.Add(f);

            if (visible.Count == 0)
                continue;

            // The horizon: edges of visible faces whose twin (reversed) edge isn't on a visible face
            var edges = new HashSet<(int, int)>();
            foreach (var f in visible)
            {
                var (a, b, c) = faces[f];
                edges.Add((a, b));
                edges.Add((b, c));
                edges.Add((c, a));
            }

            var horizon = edges.Where(e => !edges.Contains((e.Item2, e.Item1))).ToList();

            for (var k = visible.Count - 1; k >= 0; k--)
                faces.RemoveAt(visible[k]);

            // Kept faces and the new ones keep their winding: an edge (a, b) of a removed face becomes (a, b, i)
            foreach (var (a, b) in horizon)
                faces.Add((a, b, i));
        }

        return faces;
    }

    #endregion

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
