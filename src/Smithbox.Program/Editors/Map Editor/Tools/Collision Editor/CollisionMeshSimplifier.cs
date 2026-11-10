using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Reduces the detail of a collision mesh while keeping its shape: edges are collapsed cheapest first, each costed by
/// the squared distance its merged vertex would move from the planes of the faces around it (Garland and Heckbert's
/// quadric error). Flat areas collapse almost entirely, while creases, slopes and steps keep their vertices.
///
/// The mesh must be welded, as only shared edges connect the triangles. Winding and each triangle's material are kept.
/// Open edges (the seams with neighbouring collision) and borders between materials are held in place, either exactly
/// (locked) or by a heavy penalty for moving off them.
/// </summary>
public static class CollisionMeshSimplifier
{
    public sealed class Settings
    {
        /// <summary>The share of the triangles to keep, from 0 to 1. Stops early if the error limit is reached.</summary>
        public float TargetRatio = 0.25f;

        /// <summary>How far, in metres, the surface may move from where it was. Collapses past it aren't made.</summary>
        public float MaxError = 0.25f;

        /// <summary>
        /// True: vertices on open edges and material borders never move. False: they may, but at a heavy cost for
        /// leaving the border's line.
        /// </summary>
        public bool LockBorders = true;
    }

    // Moving off a border is this many times as costly as moving off a face, when borders aren't locked
    private const double BorderWeight = 10.0;

    // A collapse that turns a triangle further than this (the cosine between its old and new normals) is refused
    private const double MinNormalDot = 0.2;

    /// <summary>
    /// The simplified mesh. The inputs aren't changed; a mesh that can't be reduced comes back as a copy.
    /// </summary>
    public static (List<Vector3> Vertices, List<int> Indices, List<uint> Materials) Simplify(
        IReadOnlyList<Vector3> vertices, IReadOnlyList<int> indices, IReadOnlyList<uint> materials, Settings settings)
    {
        var mesh = new Mesh(vertices, indices, materials, settings);
        mesh.Run();
        return mesh.Build();
    }

    /// <summary>A symmetric 4x4 quadric, stored as its 10 distinct entries.</summary>
    private struct Quadric
    {
        public double A00, A01, A02, A03, A11, A12, A13, A22, A23, A33;

        public static Quadric FromPlane(double a, double b, double c, double d, double weight)
        {
            return new Quadric
            {
                A00 = weight * a * a, A01 = weight * a * b, A02 = weight * a * c, A03 = weight * a * d,
                A11 = weight * b * b, A12 = weight * b * c, A13 = weight * b * d,
                A22 = weight * c * c, A23 = weight * c * d,
                A33 = weight * d * d
            };
        }

        public void Add(in Quadric q)
        {
            A00 += q.A00; A01 += q.A01; A02 += q.A02; A03 += q.A03;
            A11 += q.A11; A12 += q.A12; A13 += q.A13;
            A22 += q.A22; A23 += q.A23;
            A33 += q.A33;
        }

        public static Quadric Sum(in Quadric a, in Quadric b)
        {
            var q = a;
            q.Add(b);
            return q;
        }

        public readonly double Error(double x, double y, double z)
        {
            return A00 * x * x + 2 * A01 * x * y + 2 * A02 * x * z + 2 * A03 * x
                 + A11 * y * y + 2 * A12 * y * z + 2 * A13 * y
                 + A22 * z * z + 2 * A23 * z
                 + A33;
        }

        /// <summary>The position of least error, if the quadric has one (it doesn't on a flat or straight patch).</summary>
        public readonly bool TryOptimal(out Vector3 position)
        {
            var det = A00 * (A11 * A22 - A12 * A12) - A01 * (A01 * A22 - A12 * A02) + A02 * (A01 * A12 - A11 * A02);

            // Relative to the matrix's own size, so the test doesn't depend on the mesh's scale or plane count
            var scale = Math.Max(A00 + A11 + A22, 1e-12);

            if (Math.Abs(det) < 1e-9 * scale * scale * scale)
            {
                position = default;
                return false;
            }

            var inv = 1.0 / det;

            var x = -inv * (A03 * (A11 * A22 - A12 * A12) - A13 * (A01 * A22 - A02 * A12) + A23 * (A01 * A12 - A02 * A11));
            var y = -inv * (A00 * (A13 * A22 - A23 * A12) - A01 * (A03 * A22 - A23 * A02) + A02 * (A03 * A12 - A13 * A02));
            var z = -inv * (A00 * (A11 * A23 - A12 * A13) - A01 * (A01 * A23 - A02 * A13) + A03 * (A01 * A12 - A11 * A02));

            position = new Vector3((float)x, (float)y, (float)z);
            return float.IsFinite(position.X) && float.IsFinite(position.Y) && float.IsFinite(position.Z);
        }
    }

    private sealed class Mesh
    {
        private readonly Settings Options;

        private readonly Vector3[] Positions;
        private readonly Quadric[] Quadrics;
        private readonly bool[] Locked;
        private readonly bool[] Removed;
        private readonly int[] Version;
        private readonly List<int>[] VertexTriangles;

        private readonly int[] Triangles;
        private readonly uint[] Materials;
        private readonly bool[] Deleted;

        private int LiveTriangles;
        private readonly int TargetTriangles;
        private readonly double MaxCost;

        private readonly PriorityQueue<(int A, int B, int VersionA, int VersionB, Vector3 Position), double> Queue = new();

        // Reused by the collapse checks
        private readonly HashSet<int> NeighboursA = new();
        private readonly HashSet<int> NeighboursB = new();

        public Mesh(IReadOnlyList<Vector3> vertices, IReadOnlyList<int> indices, IReadOnlyList<uint> materials, Settings settings)
        {
            Options = settings;

            Positions = new Vector3[vertices.Count];
            for (var i = 0; i < vertices.Count; i++)
                Positions[i] = vertices[i];

            var triangleCount = indices.Count / 3;
            Triangles = new int[triangleCount * 3];
            Materials = new uint[triangleCount];
            Deleted = new bool[triangleCount];

            for (var i = 0; i < triangleCount * 3; i++)
                Triangles[i] = indices[i];

            for (var t = 0; t < triangleCount; t++)
                Materials[t] = t < materials.Count ? materials[t] : 0;

            Quadrics = new Quadric[Positions.Length];
            Locked = new bool[Positions.Length];
            Removed = new bool[Positions.Length];
            Version = new int[Positions.Length];
            VertexTriangles = new List<int>[Positions.Length];

            for (var v = 0; v < Positions.Length; v++)
                VertexTriangles[v] = new List<int>(6);

            LiveTriangles = triangleCount;
            TargetTriangles = Math.Max(1, (int)MathF.Ceiling(triangleCount * Math.Clamp(settings.TargetRatio, 0f, 1f)));
            MaxCost = (double)settings.MaxError * settings.MaxError;

            for (var t = 0; t < triangleCount; t++)
            {
                var (a, b, c) = (Triangles[t * 3], Triangles[t * 3 + 1], Triangles[t * 3 + 2]);

                // A triangle that is already a line or point is dropped
                if (a == b || b == c || c == a)
                {
                    Deleted[t] = true;
                    LiveTriangles--;
                    continue;
                }

                VertexTriangles[a].Add(t);
                VertexTriangles[b].Add(t);
                VertexTriangles[c].Add(t);

                // Each face's plane, unweighted, so a vertex's error is the summed squared distance to its faces
                var normal = Vector3.Cross(Positions[b] - Positions[a], Positions[c] - Positions[a]);

                if (normal.LengthSquared() < 1e-20f)
                    continue;

                normal = Vector3.Normalize(normal);

                var plane = Quadric.FromPlane(normal.X, normal.Y, normal.Z, -Vector3.Dot(normal, Positions[a]), 1.0);
                Quadrics[a].Add(plane);
                Quadrics[b].Add(plane);
                Quadrics[c].Add(plane);
            }

            AddBorders();
        }

        /// <summary>
        /// Finds the edges with a triangle on one side only (open edges) or with different materials on each side, and
        /// locks their vertices or adds the planes that hold them to the border's line.
        /// </summary>
        private void AddBorders()
        {
            // Each undirected edge, with the first triangle seen on it and how many share it
            var edges = new Dictionary<(int, int), (int Triangle, int Count, bool MaterialBorder)>();

            for (var t = 0; t < Materials.Length; t++)
            {
                if (Deleted[t])
                    continue;

                for (var e = 0; e < 3; e++)
                {
                    var a = Triangles[t * 3 + e];
                    var b = Triangles[t * 3 + (e + 1) % 3];
                    var key = a < b ? (a, b) : (b, a);

                    if (edges.TryGetValue(key, out var seen))
                    {
                        edges[key] = (seen.Triangle, seen.Count + 1, seen.MaterialBorder || Materials[seen.Triangle] != Materials[t]);
                    }
                    else
                    {
                        edges[key] = (t, 1, false);
                    }
                }
            }

            foreach (var ((a, b), (triangle, count, materialBorder)) in edges)
            {
                // An edge shared by more than two triangles is a seam of another kind, held in the same way
                if (count == 2 && !materialBorder)
                    continue;

                if (Options.LockBorders)
                {
                    Locked[a] = true;
                    Locked[b] = true;
                    continue;
                }

                var p0 = Positions[Triangles[triangle * 3]];
                var faceNormal = Vector3.Cross(Positions[Triangles[triangle * 3 + 1]] - p0, Positions[Triangles[triangle * 3 + 2]] - p0);
                var edge = Positions[b] - Positions[a];

                // A plane through the edge, upright to its face: moving along the edge is free, moving off it is not
                var normal = Vector3.Cross(edge, faceNormal);

                if (normal.LengthSquared() < 1e-20f)
                    continue;

                normal = Vector3.Normalize(normal);

                var plane = Quadric.FromPlane(normal.X, normal.Y, normal.Z, -Vector3.Dot(normal, Positions[a]), BorderWeight);
                Quadrics[a].Add(plane);
                Quadrics[b].Add(plane);
            }
        }

        public void Run()
        {
            if (LiveTriangles <= TargetTriangles)
                return;

            for (var t = 0; t < Materials.Length; t++)
            {
                if (Deleted[t])
                    continue;

                for (var e = 0; e < 3; e++)
                {
                    var a = Triangles[t * 3 + e];
                    var b = Triangles[t * 3 + (e + 1) % 3];

                    // Each edge once (an open edge's only triangle may have it either way round, so it is pushed then)
                    if (a < b || IsOpenEdge(a, b))
                        PushEdge(a, b);
                }
            }

            while (LiveTriangles > TargetTriangles && Queue.TryDequeue(out var entry, out var cost))
            {
                // Everything left costs more than allowed
                if (cost > MaxCost)
                    break;

                var (a, b) = (entry.A, entry.B);

                // Stale: an end has been collapsed or moved since this was costed
                if (Removed[a] || Removed[b] || Version[a] != entry.VersionA || Version[b] != entry.VersionB)
                    continue;

                if (!CanCollapse(a, b, entry.Position))
                    continue;

                Collapse(a, b, entry.Position);
            }
        }

        private bool IsOpenEdge(int a, int b)
        {
            var shared = 0;

            foreach (var t in VertexTriangles[a])
            {
                if (!Deleted[t] && Contains(t, b))
                    shared++;
            }

            return shared == 1;
        }

        /// <summary>Costs the collapse of an edge and queues it, if its ends may be merged.</summary>
        private void PushEdge(int a, int b)
        {
            if (a == b || Removed[a] || Removed[b])
                return;

            if (Locked[a] && Locked[b])
                return;

            var q = Quadric.Sum(Quadrics[a], Quadrics[b]);
            var pa = Positions[a];
            var pb = Positions[b];

            Vector3 position;
            double cost;

            if (Locked[a])
            {
                position = pa;
                cost = q.Error(pa.X, pa.Y, pa.Z);
            }
            else if (Locked[b])
            {
                position = pb;
                cost = q.Error(pb.X, pb.Y, pb.Z);
            }
            else
            {
                // The best point, unless it lies far from the edge (a near-flat quadric can put it anywhere), else
                // the better of the ends and the middle
                var mid = (pa + pb) * 0.5f;
                var reach = Math.Max(Vector3.Distance(pa, pb), 1e-4f) * 2f;

                if (q.TryOptimal(out var optimal) && Vector3.Distance(optimal, mid) <= reach)
                {
                    position = optimal;
                    cost = q.Error(optimal.X, optimal.Y, optimal.Z);
                }
                else
                {
                    position = mid;
                    cost = q.Error(mid.X, mid.Y, mid.Z);

                    var costA = q.Error(pa.X, pa.Y, pa.Z);
                    if (costA < cost) { position = pa; cost = costA; }

                    var costB = q.Error(pb.X, pb.Y, pb.Z);
                    if (costB < cost) { position = pb; cost = costB; }
                }
            }

            Queue.Enqueue((a, b, Version[a], Version[b], position), Math.Max(cost, 0));
        }

        private bool Contains(int triangle, int vertex)
        {
            return Triangles[triangle * 3] == vertex || Triangles[triangle * 3 + 1] == vertex || Triangles[triangle * 3 + 2] == vertex;
        }

        private void GatherNeighbours(int vertex, HashSet<int> into)
        {
            into.Clear();

            foreach (var t in VertexTriangles[vertex])
            {
                if (Deleted[t])
                    continue;

                for (var i = 0; i < 3; i++)
                {
                    var v = Triangles[t * 3 + i];
                    if (v != vertex)
                        into.Add(v);
                }
            }
        }

        /// <summary>
        /// Whether merging a and b at the position keeps the mesh sound: the edge still exists, no fold or pinch is
        /// made (the ends share only the vertices across the edge's own triangles), and no triangle around them flips
        /// or is crushed flat.
        /// </summary>
        private bool CanCollapse(int a, int b, Vector3 position)
        {
            var edgeTriangles = 0;

            foreach (var t in VertexTriangles[a])
            {
                if (!Deleted[t] && Contains(t, b))
                    edgeTriangles++;
            }

            // The edge went away with an earlier collapse
            if (edgeTriangles == 0)
                return false;

            // The link condition: a shared neighbour that isn't the third corner of one of the edge's own triangles
            // would be pinched into a non-manifold fan
            GatherNeighbours(a, NeighboursA);
            GatherNeighbours(b, NeighboursB);

            var shared = 0;
            foreach (var v in NeighboursA)
            {
                if (NeighboursB.Contains(v))
                    shared++;
            }

            if (shared != edgeTriangles)
                return false;

            return !Flips(a, b, position) && !Flips(b, a, position);
        }

        /// <summary>Whether moving the vertex to the position flips or crushes a triangle of it that survives.</summary>
        private bool Flips(int vertex, int other, Vector3 position)
        {
            foreach (var t in VertexTriangles[vertex])
            {
                if (Deleted[t] || Contains(t, other))
                    continue;

                var i0 = Triangles[t * 3];
                var i1 = Triangles[t * 3 + 1];
                var i2 = Triangles[t * 3 + 2];

                var p0 = Positions[i0];
                var p1 = Positions[i1];
                var p2 = Positions[i2];

                var before = Vector3.Cross(p1 - p0, p2 - p0);

                if (i0 == vertex) p0 = position;
                else if (i1 == vertex) p1 = position;
                else p2 = position;

                var after = Vector3.Cross(p1 - p0, p2 - p0);

                var beforeLength = before.Length();
                var afterLength = after.Length();

                if (afterLength < 1e-10f || beforeLength < 1e-10f)
                    return true;

                if (Vector3.Dot(before, after) < MinNormalDot * beforeLength * afterLength)
                    return true;
            }

            return false;
        }

        /// <summary>Merges b into a at the position: the edge's triangles go, b's others turn to a.</summary>
        private void Collapse(int a, int b, Vector3 position)
        {
            foreach (var t in VertexTriangles[b])
            {
                if (Deleted[t])
                    continue;

                if (Contains(t, a))
                {
                    Deleted[t] = true;
                    LiveTriangles--;
                    continue;
                }

                for (var i = 0; i < 3; i++)
                {
                    if (Triangles[t * 3 + i] == b)
                        Triangles[t * 3 + i] = a;
                }

                VertexTriangles[a].Add(t);
            }

            VertexTriangles[b].Clear();
            VertexTriangles[a].RemoveAll(t => Deleted[t]);

            Positions[a] = position;
            Quadrics[a].Add(Quadrics[b]);
            Locked[a] |= Locked[b];
            Removed[b] = true;
            Version[a]++;

            // Every edge of a is costed again, as its place and quadric have changed
            GatherNeighbours(a, NeighboursA);

            foreach (var v in NeighboursA)
                PushEdge(a, v);
        }

        /// <summary>The surviving triangles, with their vertices packed.</summary>
        public (List<Vector3> Vertices, List<int> Indices, List<uint> Materials) Build()
        {
            var remap = new int[Positions.Length];
            Array.Fill(remap, -1);

            var vertices = new List<Vector3>();
            var indices = new List<int>(LiveTriangles * 3);
            var materials = new List<uint>(LiveTriangles);

            for (var t = 0; t < Materials.Length; t++)
            {
                if (Deleted[t])
                    continue;

                for (var i = 0; i < 3; i++)
                {
                    var v = Triangles[t * 3 + i];

                    if (remap[v] < 0)
                    {
                        remap[v] = vertices.Count;
                        vertices.Add(Positions[v]);
                    }

                    indices.Add(remap[v]);
                }

                materials.Add(Materials[t]);
            }

            return (vertices, indices, materials);
        }
    }
}
