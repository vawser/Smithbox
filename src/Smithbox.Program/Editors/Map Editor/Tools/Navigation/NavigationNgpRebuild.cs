using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The NGP mode's rebuild engine (Navigation.md (NGP (DS2, DS2S)), Phase G2): every table that follows from the meshes'
/// vertices, faces and face flags, so an edit only changes those and calls <see cref="Rebuild"/>.
///
/// Per mesh, only for the meshes an edit changed (the vanilla data breaks the edge rule in 144 meshes, which an untouched
/// mesh keeps byte for byte):
/// - the bounding box (the vertex bounds);
/// - the edges and the faces' half-edges, in face order: the first face to use a vertex pair makes the edge (face0,
///   side 0), the second fills face1, a third reuses side 1 as the vanilla files do;
/// - the split tree: X and Z alternating by depth, split at the mean of the faces' centroids (16,116 of 16,228 vanilla
///   inner nodes), left the faces reaching above it, right those reaching to it or below; a leaf at 16 faces or fewer,
///   at depth 10, or when a child would keep every face. The vanilla tool stops some nodes earlier by a rule not found,
///   so a rebuilt tree is valid but not byte-identical.
///
/// Map-wide, on every rebuild:
/// - gates: coincident (within <see cref="NgpValidator.GateTolerance"/> per end), opposite boundary edges of two meshes,
///   grouped into connected runs; the gates of two unchanged meshes are kept as the file has them (one vanilla gate,
///   in m50_35, joins edges 4 m apart and couldn't be found again); sorted by mesh A, mesh B, lowest side-A edge;
/// - their positions (the first side-A edge's middle), the edge refs (C), the meshes' gate lists (D), the links (B: each
///   side to every other gate of its mesh, ascending, side A first, cost FLT_MAX) and every ID.
///
/// Renderer-free, so Tools/NavSurvey checks it on the game files (`nvm GAME ngp-rebuild`).
/// </summary>
public static class NgpRebuild
{
    public const int LeafFaces = 16;
    public const int MaxDepth = 10;

    /// <summary>A copy of the NGP with the changed meshes' tables and the map-wide tables rebuilt.</summary>
    /// <param name="keepTopology">The changed meshes' faces are as they were (a vertex move): their edges and half-edges
    /// are kept, so the vanilla quirks stay; only the bounding box and tree follow the vertices.</param>
    public static NGP Rebuild(NGP source, IReadOnlyCollection<int> changedMeshes, bool keepTopology = false)
    {
        var ngp = NgpEdit.Copy(source);
        var changed = new HashSet<int>(changedMeshes ?? Array.Empty<int>());
        byte map = ngp.MapIndex;

        ngp.MapId = NGP.Ids.Map(map);

        for (int m = 0; m < ngp.Meshes.Count; m++)
        {
            ngp.Meshes[m].Id = NGP.Ids.Mesh(map, m);
            if (changed.Contains(m))
                RebuildMesh(ngp.Meshes[m], keepTopology);
        }

        RebuildGates(ngp, changed);
        return ngp;
    }

    #region Per mesh

    public static void RebuildMesh(NGP.Mesh mesh, bool keepTopology = false)
    {
        // Bounding box
        if (mesh.Vertices.Count > 0)
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            foreach (var v in mesh.Vertices)
            {
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }

            mesh.BoundingBoxMin = min;
            mesh.BoundingBoxMax = max;
        }

        while (mesh.FaceFlags.Count < mesh.Faces.Count)
            mesh.FaceFlags.Add(NgpFlags.DefaultLevel);
        if (mesh.FaceFlags.Count > mesh.Faces.Count)
            mesh.FaceFlags.RemoveRange(mesh.Faces.Count, mesh.FaceFlags.Count - mesh.Faces.Count);

        if (!keepTopology || mesh.Edges.Count == 0)
            BuildEdges(mesh);

        mesh.SplitTreeRoot = BuildTree(mesh);
    }

    /// <summary>The edges and half-edges from the faces, in face order.</summary>
    public static void BuildEdges(NGP.Mesh mesh)
    {
        var edges = new List<NGP.Edge>();
        var index = new Dictionary<(int, int), int>();

        for (int f = 0; f < mesh.Faces.Count; f++)
        {
            var face = mesh.Faces[f];
            short[] v = { face.V1, face.V2, face.V3 };
            var halves = new ushort[3];

            for (int k = 0; k < 3; k++)
            {
                short a = v[k], b = v[(k + 1) % 3];
                var key = (Math.Min(a, b), Math.Max(a, b));

                if (index.TryGetValue(key, out int e))
                {
                    var edge = edges[e];
                    if (edge.Face1 == NGP.Ids.None && edge.Face0 != f)
                        edge.Face1 = (short)f;

                    // A third face on the edge reuses side 1, as the vanilla files do
                    halves[k] = (ushort)(e << 1 | 1);
                }
                else
                {
                    e = edges.Count;
                    index[key] = e;
                    edges.Add(new NGP.Edge { VertexA = a, VertexB = b, Face0 = (short)f });
                    halves[k] = (ushort)(e << 1);
                }
            }

            face.HalfEdge1 = halves[0];
            face.HalfEdge2 = halves[1];
            face.HalfEdge3 = halves[2];
        }

        // Each edge's neighbours in its two faces
        for (int f = 0; f < mesh.Faces.Count; f++)
        {
            var face = mesh.Faces[f];
            ushort[] h = { face.HalfEdge1, face.HalfEdge2, face.HalfEdge3 };

            for (int k = 0; k < 3; k++)
            {
                var edge = edges[h[k] >> 1];
                short next = (short)(h[(k + 1) % 3] >> 1), prev = (short)(h[(k + 2) % 3] >> 1);

                if ((h[k] & 1) == 0 && edge.Face0 == f)
                {
                    edge.Face0Next = next;
                    edge.Face0Prev = prev;
                }
                else if ((h[k] & 1) == 1 && edge.Face1 == f)
                {
                    edge.Face1Next = next;
                    edge.Face1Prev = prev;
                }
            }
        }

        mesh.Edges = edges;
    }

    /// <summary>The split tree of the mesh's faces.</summary>
    public static NGP.SplitNode BuildTree(NGP.Mesh mesh)
    {
        int fc = mesh.Faces.Count;
        var centroids = new Vector3[fc];
        var min = new Vector3[fc];
        var max = new Vector3[fc];

        for (int f = 0; f < fc; f++)
        {
            var face = mesh.Faces[f];
            Vector3 a = mesh.Vertices[face.V1], b = mesh.Vertices[face.V2], c = mesh.Vertices[face.V3];
            centroids[f] = (a + b + c) / 3;
            min[f] = Vector3.Min(a, Vector3.Min(b, c));
            max[f] = Vector3.Max(a, Vector3.Max(b, c));
        }

        NGP.SplitNode Build(List<short> faces, int depth)
        {
            bool x = depth % 2 == 0;
            float Axis(Vector3 v) => x ? v.X : v.Z;

            double sum = 0;
            foreach (var f in faces)
                sum += Axis(centroids[f]);
            float split = faces.Count == 0 ? 0 : (float)(sum / faces.Count);

            var node = new NGP.SplitNode { Split = split };

            if (faces.Count > LeafFaces && depth < MaxDepth)
            {
                var left = faces.Where(f => Axis(max[f]) > split).ToList();
                var right = faces.Where(f => Axis(min[f]) <= split).ToList();

                if (left.Count > 0 && right.Count > 0 && left.Count < faces.Count && right.Count < faces.Count)
                {
                    node.Left = Build(left, depth + 1);
                    node.Right = Build(right, depth + 1);
                    return node;
                }
            }

            node.FaceIndices = faces.Count > 0 ? faces : null;
            return node;
        }

        return Build(Enumerable.Range(0, fc).Select(f => (short)f).ToList(), 0);
    }

    #endregion

    #region Gates

    private sealed class GateSpec
    {
        public int MeshA, MeshB;
        public List<int> EdgesA = new(), EdgesB = new();

        /// <summary>A kept gate's position as the file has it: the vanilla tool's midpoint rounds differently in 5 gates.</summary>
        public Vector3? Position;
    }

    private static void RebuildGates(NGP ngp, HashSet<int> changed)
    {
        byte map = ngp.MapIndex;
        int meshCount = ngp.Meshes.Count;
        var specs = new List<GateSpec>();

        // The gates of two unchanged meshes, kept as the file has them
        foreach (var gate in ngp.Gates)
        {
            int ma = NGP.Ids.MeshOf(gate.MeshAId), mb = NGP.Ids.MeshOf(gate.MeshBId);
            if (ma >= meshCount || mb >= meshCount || changed.Contains(ma) || changed.Contains(mb))
                continue;
            if (gate.SideAEdgeStart < 0 || gate.SideBEdgeStart + gate.EdgeCount > ngp.GateEdgeRefs.Count)
                continue;

            var spec = new GateSpec { MeshA = ma, MeshB = mb, Position = gate.Position };
            for (int k = 0; k < gate.EdgeCount; k++)
            {
                spec.EdgesA.Add(NGP.Ids.IndexOf(ngp.GateEdgeRefs[gate.SideAEdgeStart + k]));
                spec.EdgesB.Add(NGP.Ids.IndexOf(ngp.GateEdgeRefs[gate.SideBEdgeStart + k]));
            }
            specs.Add(spec);
        }

        // The gates of every pair with a changed mesh, found again from the geometry
        if (changed.Count > 0)
            specs.AddRange(FindGates(ngp, changed));

        specs = specs.OrderBy(s => s.MeshA).ThenBy(s => s.MeshB).ThenBy(s => s.EdgesA.Min()).ToList();

        // Gates, edge refs (C)
        ngp.Gates = new List<NGP.Gate>(specs.Count);
        ngp.GateEdgeRefs = new List<uint>();
        var gatesOfMesh = Enumerable.Range(0, meshCount).Select(_ => new List<int>()).ToList();

        for (int g = 0; g < specs.Count; g++)
        {
            var s = specs[g];
            var meshA = ngp.Meshes[s.MeshA];
            var first = meshA.Edges[s.EdgesA[0]];

            var gate = new NGP.Gate
            {
                Position = s.Position ?? (meshA.Vertices[first.VertexA] + meshA.Vertices[first.VertexB]) / 2,
                Id = NGP.Ids.Gate(map, s.MeshA, s.MeshB, g),
                EdgeCount = s.EdgesA.Count,
                SideAEdgeStart = (short)ngp.GateEdgeRefs.Count,
                SideBEdgeStart = (short)(ngp.GateEdgeRefs.Count + s.EdgesA.Count),
                MeshAId = NGP.Ids.Mesh(map, s.MeshA),
                MeshBId = NGP.Ids.Mesh(map, s.MeshB),
            };

            foreach (var e in s.EdgesA)
                ngp.GateEdgeRefs.Add(NGP.Ids.EdgeRef(map, s.MeshA, e));
            foreach (var e in s.EdgesB)
                ngp.GateEdgeRefs.Add(NGP.Ids.EdgeRef(map, s.MeshB, e));

            ngp.Gates.Add(gate);
            gatesOfMesh[s.MeshA].Add(g);
            gatesOfMesh[s.MeshB].Add(g);
        }

        // The meshes' gate lists (D)
        ngp.MeshGateIndices = new List<short>();
        for (int m = 0; m < meshCount; m++)
        {
            var mesh = ngp.Meshes[m];
            mesh.GateCount = (short)gatesOfMesh[m].Count;
            mesh.GateStart = gatesOfMesh[m].Count == 0 ? (short)-1 : (short)ngp.MeshGateIndices.Count;
            foreach (var g in gatesOfMesh[m])
                ngp.MeshGateIndices.Add((short)g);
        }

        // The links (B)
        ngp.GateLinks = new List<NGP.GateLink>();
        for (int g = 0; g < ngp.Gates.Count; g++)
        {
            var gate = ngp.Gates[g];
            var s = specs[g];
            int start = ngp.GateLinks.Count;

            foreach (var other in gatesOfMesh[s.MeshA].Where(o => o != g))
                ngp.GateLinks.Add(new NGP.GateLink { EdgeRef = ngp.GateEdgeRefs[gate.SideAEdgeStart], TargetGate = other, Cost = float.MaxValue });
            foreach (var other in gatesOfMesh[s.MeshB].Where(o => o != g))
                ngp.GateLinks.Add(new NGP.GateLink { EdgeRef = ngp.GateEdgeRefs[gate.SideBEdgeStart], TargetGate = other, Cost = float.MaxValue });

            gate.LinkCount = (short)(ngp.GateLinks.Count - start);
            gate.LinkStart = gate.LinkCount == 0 ? (short)-1 : (short)start;
        }
    }

    /// <summary>
    /// The gates between each pair of meshes with a changed one: their coincident, opposite boundary edges, paired
    /// closest first, grouped into runs that share a vertex on mesh A, each run listed along its chain.
    /// </summary>
    private static IEnumerable<GateSpec> FindGates(NGP ngp, HashSet<int> changed)
    {
        float tolerance = NgpValidator.GateTolerance;
        float q = tolerance;
        (int, int, int) Cell(Vector3 v) => ((int)MathF.Floor(v.X / q), (int)MathF.Floor(v.Y / q), (int)MathF.Floor(v.Z / q));

        // Every boundary edge, bucketed by its first end's cell
        var byCell = new Dictionary<(int, int, int), List<(int Mesh, int Edge)>>();
        for (int m = 0; m < ngp.Meshes.Count; m++)
        {
            var mesh = ngp.Meshes[m];
            for (int e = 0; e < mesh.Edges.Count; e++)
            {
                if (mesh.Edges[e].Face1 != NGP.Ids.None)
                    continue;

                var c = Cell(mesh.Vertices[mesh.Edges[e].VertexA]);
                if (!byCell.TryGetValue(c, out var list))
                    byCell[c] = list = new();
                list.Add((m, e));
            }
        }

        (Vector3 A, Vector3 B) Ends(int m, int e)
        {
            var mesh = ngp.Meshes[m];
            var edge = mesh.Edges[e];
            return (mesh.Vertices[edge.VertexA], mesh.Vertices[edge.VertexB]);
        }

        // Candidate pairs: an edge of mesh A and one of a higher mesh B, at least one of them changed, ends within the
        // tolerance (summed over both ends), either direction
        var candidates = new List<(int Ma, int Ea, int Mb, int Eb, float D)>();
        for (int m = 0; m < ngp.Meshes.Count; m++)
        {
            var mesh = ngp.Meshes[m];
            for (int e = 0; e < mesh.Edges.Count; e++)
            {
                if (mesh.Edges[e].Face1 != NGP.Ids.None)
                    continue;

                var (a, b) = Ends(m, e);

                // The other edge's first end is near this edge's B (opposite) or A (same direction)
                foreach (var end in new[] { b, a })
                {
                    var c = Cell(end);
                    for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (!byCell.TryGetValue((c.Item1 + dx, c.Item2 + dy, c.Item3 + dz), out var list))
                            continue;

                        foreach (var (om, oe) in list)
                        {
                            if (om <= m || (!changed.Contains(m) && !changed.Contains(om)))
                                continue;

                            var (oa, ob) = Ends(om, oe);
                            float opposite = Vector3.Distance(a, ob) + Vector3.Distance(b, oa);
                            float same = Vector3.Distance(a, oa) + Vector3.Distance(b, ob);
                            float d = MathF.Min(opposite, same);
                            if (d <= tolerance)
                                candidates.Add((m, e, om, oe, d));
                        }
                    }
                }
            }
        }

        // Each boundary edge in one pair, closest first
        var usedA = new HashSet<(int, int)>();
        var pairs = new List<(int Ma, int Ea, int Mb, int Eb)>();
        foreach (var c in candidates.Distinct().OrderBy(c => c.D).ThenBy(c => c.Ma).ThenBy(c => c.Ea).ThenBy(c => c.Mb).ThenBy(c => c.Eb))
        {
            if (usedA.Contains((c.Ma, c.Ea)) || usedA.Contains((c.Mb, c.Eb)))
                continue;

            usedA.Add((c.Ma, c.Ea));
            usedA.Add((c.Mb, c.Eb));
            pairs.Add((c.Ma, c.Ea, c.Mb, c.Eb));
        }

        // Runs per mesh pair: pairs whose mesh A edges share a vertex
        foreach (var group in pairs.GroupBy(p => (p.Ma, p.Mb)))
        {
            var meshA = ngp.Meshes[group.Key.Ma];
            var left = group.ToList();

            while (left.Count > 0)
            {
                var run = new List<(int Ma, int Ea, int Mb, int Eb)> { left[0] };
                left.RemoveAt(0);

                bool grew = true;
                while (grew)
                {
                    grew = false;
                    var verts = run.SelectMany(p => new[] { (int)meshA.Edges[p.Ea].VertexA, meshA.Edges[p.Ea].VertexB }).ToHashSet();
                    for (int i = left.Count - 1; i >= 0; i--)
                    {
                        var edge = meshA.Edges[left[i].Ea];
                        if (verts.Contains(edge.VertexA) || verts.Contains(edge.VertexB))
                        {
                            run.Add(left[i]);
                            left.RemoveAt(i);
                            grew = true;
                        }
                    }
                }

                var ordered = ChainOrder(meshA, run.Select(p => p.Ea).ToList());
                var byA = run.ToDictionary(p => p.Ea, p => p.Eb);

                var spec = new GateSpec { MeshA = group.Key.Ma, MeshB = group.Key.Mb };
                foreach (var ea in ordered)
                {
                    spec.EdgesA.Add(ea);
                    spec.EdgesB.Add(byA[ea]);
                }

                yield return spec;
            }
        }
    }

    /// <summary>A run's edges along their chain, from an end (the lowest-numbered one), or from the lowest edge of a loop.</summary>
    private static List<int> ChainOrder(NGP.Mesh mesh, List<int> run)
    {
        if (run.Count <= 1)
            return run;

        var byVertex = new Dictionary<int, List<int>>();
        foreach (var e in run)
        {
            foreach (int v in new[] { (int)mesh.Edges[e].VertexA, mesh.Edges[e].VertexB })
            {
                if (!byVertex.TryGetValue(v, out var list))
                    byVertex[v] = list = new();
                list.Add(e);
            }
        }

        var ends = run.Where(e => byVertex[mesh.Edges[e].VertexA].Count == 1 || byVertex[mesh.Edges[e].VertexB].Count == 1).ToList();
        int current = ends.Count > 0 ? ends.Min() : run.Min();

        var ordered = new List<int>();
        var seen = new HashSet<int>();

        while (current >= 0 && seen.Add(current))
        {
            ordered.Add(current);
            var edge = mesh.Edges[current];
            current = byVertex[edge.VertexA].Concat(byVertex[edge.VertexB]).Where(e => !seen.Contains(e)).DefaultIfEmpty(-1).Min();
        }

        // A run that isn't a simple path (a fork): the rest by index
        ordered.AddRange(run.Where(e => !seen.Contains(e)).OrderBy(e => e));
        return ordered;
    }

    #endregion
}
