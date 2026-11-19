using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Checks a map's NGP (Phase G0), with the rules the game files hold (NGP Format.md; Tools/NgpSurvey strict, links,
/// gaterebuild). The vanilla files pass it without errors (NavSurvey `ngp validate`); its warnings are things a few
/// vanilla meshes do too, which the Check tab marks inherited. Issues name the mesh in Room and the face in Face, so the
/// Check tab can go to them.
/// </summary>
public static class NgpValidator
{
    /// <summary>How far apart a gate's two sides may be, summed over both ends (vanilla: all but 1 of 2,523 within 5 cm).</summary>
    public const float GateTolerance = 0.05f;

    /// <summary>The least slope a ladder face (type 0x20) should have, in degrees.</summary>
    public const float LadderMinSlope = 80f;

    /// <summary>The least slope a one-way drop face (type 0x40) should have, in degrees.</summary>
    public const float DropMinSlope = 20f;

    public static List<NavIssue> Validate(NgpMap map)
    {
        var issues = new List<NavIssue>();
        var ngp = map.Ngp;
        var file = map.FileName;

        void Add(NavIssueSeverity severity, NavIssueCode code, int? mesh, int? face, params object[] args)
            => issues.Add(new NavIssue(severity, code, file, args) { MapId = map.MapId, Room = mesh, Face = face });

        if (ngp.Version != map.Profile.Version)
            Add(NavIssueSeverity.Error, NavIssueCode.NgpVersion, null, null, (int)ngp.Version, (int)map.Profile.Version);

        if (map.PartCount != ngp.Meshes.Count)
            Add(NavIssueSeverity.Error, NavIssueCode.NgpPartCount, null, null, map.PartCount, ngp.Meshes.Count);

        if ((ngp.MapId & 0xFFFFFF) != 0xFFFFFF)
            Add(NavIssueSeverity.Error, NavIssueCode.NgpMapId, null, null, ngp.MapId.ToString("X8"));

        if (ngp.Meshes.Count > 128)
            Add(NavIssueSeverity.Error, NavIssueCode.NgpCountLimit, null, null, "-", "meshes", ngp.Meshes.Count, 128);

        if (ngp.Gates.Count > 1024)
            Add(NavIssueSeverity.Error, NavIssueCode.NgpCountLimit, null, null, "-", "gates", ngp.Gates.Count, 1024);

        for (int m = 0; m < ngp.Meshes.Count; m++)
            ValidateMesh(map, m, Add);

        ValidateGates(map, Add);
        return issues;
    }

    private delegate void Adder(NavIssueSeverity severity, NavIssueCode code, int? mesh, int? face, params object[] args);

    private static void ValidateMesh(NgpMap map, int m, Adder add)
    {
        var mesh = map.Ngp.Meshes[m];
        var label = map.Meshes[m].Label;
        int vc = mesh.Vertices.Count, fc = mesh.Faces.Count, ec = mesh.Edges.Count;

        if (mesh.Id != NGP.Ids.Mesh(map.Ngp.MapIndex, m))
            add(NavIssueSeverity.Error, NavIssueCode.NgpMeshId, m, null, label, mesh.Id.ToString("X8"), NGP.Ids.Mesh(map.Ngp.MapIndex, m).ToString("X8"));

        if (vc > 32767 || fc > 32767 || ec > 32767)
            add(NavIssueSeverity.Error, NavIssueCode.NgpCountLimit, m, null, label, "vertices/faces/edges", Math.Max(vc, Math.Max(fc, ec)), 32767);

        if (mesh.FaceFlags.Count != fc)
            add(NavIssueSeverity.Error, NavIssueCode.NgpFlagsCount, m, null, label, mesh.FaceFlags.Count, fc);

        // Bounding box = vertex bounds
        if (vc > 0)
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            foreach (var v in mesh.Vertices)
            {
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }

            if (Vector3.Distance(min, mesh.BoundingBoxMin) > 1e-3f || Vector3.Distance(max, mesh.BoundingBoxMax) > 1e-3f)
                add(NavIssueSeverity.Error, NavIssueCode.NgpBoundingBox, m, null, label);
        }

        // Faces, their half-edges and flags
        int sharedFaces = 0, firstShared = -1, unknownFlags = 0, firstUnknown = -1;
        for (int f = 0; f < fc; f++)
        {
            var face = mesh.Faces[f];
            if (!NgpGeometry.InRange(face, vc))
            {
                add(NavIssueSeverity.Error, NavIssueCode.NgpFaceRange, m, f, label, f, vc);
                continue;
            }

            if (f < mesh.FaceFlags.Count && (mesh.FaceFlags[f] & ~NgpFlags.KnownMask) != 0)
            {
                unknownFlags++;
                if (firstUnknown < 0) firstUnknown = f;
            }

            ushort[] halves = { face.HalfEdge1, face.HalfEdge2, face.HalfEdge3 };
            short[] verts = { face.V1, face.V2, face.V3 };
            bool bad = false;

            for (int k = 0; k < 3; k++)
            {
                int e = NgpGeometry.EdgeOf(halves[k]), side = NgpGeometry.SideOf(halves[k]);
                if (e >= ec)
                {
                    add(NavIssueSeverity.Error, NavIssueCode.NgpHalfEdgeRange, m, f, label, f, e, ec);
                    bad = false;
                    break;
                }

                var edge = mesh.Edges[e];
                short a = verts[k], b = verts[(k + 1) % 3];
                bool backToFace = (side == 0 ? edge.Face0 : edge.Face1) == f;
                bool ends = side == 0 ? edge.VertexA == a && edge.VertexB == b : edge.VertexA == b && edge.VertexB == a;
                if (!backToFace || !ends)
                    bad = true;
            }

            // An edge shared by three or more faces can't be stored: the extra faces reuse another face's half-edge.
            // 97 vanilla meshes do this (367 faces), so it is a warning
            if (bad)
            {
                sharedFaces++;
                if (firstShared < 0) firstShared = f;
            }
        }

        if (sharedFaces > 0)
            add(NavIssueSeverity.Warning, NavIssueCode.NgpHalfEdgeShared, m, firstShared, label, sharedFaces);

        if (unknownFlags > 0)
            add(NavIssueSeverity.Warning, NavIssueCode.NgpFlagsUnknown, m, firstUnknown, label, unknownFlags);

        // Ladders are vertical (all 745 vanilla ladder faces are 90°) and drops steep (95 % at 38° or more, 105 under 20°)
        int ladders = 0, firstLadder = -1, drops = 0, firstDrop = -1;
        for (int f = 0; f < fc && f < mesh.FaceFlags.Count; f++)
        {
            if (!NgpGeometry.InRange(mesh.Faces[f], vc))
                continue;

            uint type = NgpFlags.Type(mesh.FaceFlags[f]);
            if (type != 0x20 && type != 0x40)
                continue;

            float slope = NgpGeometry.Slope(mesh, f);
            if (slope < 0)
                continue;

            if (type == 0x20 && slope < LadderMinSlope)
            {
                ladders++;
                if (firstLadder < 0) firstLadder = f;
            }
            else if (type == 0x40 && slope < DropMinSlope)
            {
                drops++;
                if (firstDrop < 0) firstDrop = f;
            }
        }

        if (ladders > 0)
            add(NavIssueSeverity.Warning, NavIssueCode.NgpLadderNotVertical, m, firstLadder, label, ladders, LadderMinSlope);
        if (drops > 0)
            add(NavIssueSeverity.Warning, NavIssueCode.NgpDropFlat, m, firstDrop, label, drops, DropMinSlope);

        // Edges
        for (int e = 0; e < ec; e++)
        {
            var edge = mesh.Edges[e];
            bool verticesOk = edge.VertexA >= 0 && edge.VertexA < vc && edge.VertexB >= 0 && edge.VertexB < vc;
            bool facesOk = edge.Face0 >= 0 && edge.Face0 < fc && (edge.Face1 == NGP.Ids.None || edge.Face1 >= 0 && edge.Face1 < fc);
            if (!verticesOk || !facesOk)
            {
                add(NavIssueSeverity.Error, NavIssueCode.NgpEdgeRange, m, null, label, e);
                break;
            }
        }

        // The split tree lists every face, and only faces that exist
        var listed = new HashSet<int>();
        bool treeRange = false;
        Walk(mesh.SplitTreeRoot, node =>
        {
            if (node.FaceIndices == null)
                return;
            foreach (var f in node.FaceIndices)
            {
                if (f < 0 || f >= fc) treeRange = true;
                else listed.Add(f);
            }
        });

        if (treeRange)
            add(NavIssueSeverity.Error, NavIssueCode.NgpTreeRange, m, null, label);

        if (listed.Count < fc)
        {
            int first = Enumerable.Range(0, fc).First(f => !listed.Contains(f));
            add(NavIssueSeverity.Error, NavIssueCode.NgpTreeMissingFace, m, first, label, fc - listed.Count);
        }
    }

    private static void Walk(NGP.SplitNode node, Action<NGP.SplitNode> visit)
    {
        var stack = new Stack<NGP.SplitNode>();
        if (node != null)
            stack.Push(node);

        while (stack.Count > 0)
        {
            var n = stack.Pop();
            visit(n);
            if (n.Left != null) stack.Push(n.Left);
            if (n.Right != null) stack.Push(n.Right);
        }
    }

    private static void ValidateGates(NgpMap map, Adder add)
    {
        var ngp = map.Ngp;
        int meshCount = ngp.Meshes.Count;
        byte mapIndex = ngp.MapIndex;
        var gatesOfMesh = Enumerable.Range(0, meshCount).Select(_ => new List<int>()).ToList();
        var usedEdges = new HashSet<(int Mesh, int Edge)>();

        for (int g = 0; g < ngp.Gates.Count; g++)
        {
            var gate = ngp.Gates[g];
            int ma = NGP.Ids.MeshOf(gate.MeshAId), mb = NGP.Ids.MeshOf(gate.MeshBId);

            bool idsOk = ma < meshCount && mb < meshCount && gate.MeshAId == NGP.Ids.Mesh(mapIndex, ma) && gate.MeshBId == NGP.Ids.Mesh(mapIndex, mb) && ma < mb;
            if (!idsOk)
            {
                add(NavIssueSeverity.Error, NavIssueCode.NgpGateRange, null, null, g, gate.MeshAId.ToString("X8"), gate.MeshBId.ToString("X8"));
                continue;
            }

            gatesOfMesh[ma].Add(g);
            gatesOfMesh[mb].Add(g);

            if (gate.Id != NGP.Ids.Gate(mapIndex, ma, mb, g))
                add(NavIssueSeverity.Error, NavIssueCode.NgpGateId, ma, null, g, gate.Id.ToString("X8"), NGP.Ids.Gate(mapIndex, ma, mb, g).ToString("X8"));

            bool rangesOk = gate.EdgeCount > 0 && gate.SideAEdgeStart >= 0 && gate.SideBEdgeStart == gate.SideAEdgeStart + gate.EdgeCount
                && gate.SideBEdgeStart + gate.EdgeCount <= ngp.GateEdgeRefs.Count
                && (gate.LinkCount == 0 ? gate.LinkStart == -1 : gate.LinkStart >= 0 && gate.LinkStart + gate.LinkCount <= ngp.GateLinks.Count);
            if (!rangesOk)
            {
                add(NavIssueSeverity.Error, NavIssueCode.NgpGateRange, ma, null, g, gate.MeshAId.ToString("X8"), gate.MeshBId.ToString("X8"));
                continue;
            }

            // Each side's edge k is the same boundary edge seen from each mesh, reversed
            float worst = 0;
            bool edgesOk = true;
            for (int k = 0; k < gate.EdgeCount; k++)
            {
                uint ra = ngp.GateEdgeRefs[gate.SideAEdgeStart + k], rb = ngp.GateEdgeRefs[gate.SideBEdgeStart + k];
                if (!EdgeRefOk(ngp, ra, mapIndex, ma) || !EdgeRefOk(ngp, rb, mapIndex, mb))
                {
                    edgesOk = false;
                    break;
                }

                var meshA = ngp.Meshes[ma];
                var meshB = ngp.Meshes[mb];
                var ea = meshA.Edges[NGP.Ids.IndexOf(ra)];
                var eb = meshB.Edges[NGP.Ids.IndexOf(rb)];
                usedEdges.Add((ma, NGP.Ids.IndexOf(ra)));
                usedEdges.Add((mb, NGP.Ids.IndexOf(rb)));

                float reversed = Vector3.Distance(meshA.Vertices[ea.VertexA], meshB.Vertices[eb.VertexB]) + Vector3.Distance(meshA.Vertices[ea.VertexB], meshB.Vertices[eb.VertexA]);
                float same = Vector3.Distance(meshA.Vertices[ea.VertexA], meshB.Vertices[eb.VertexA]) + Vector3.Distance(meshA.Vertices[ea.VertexB], meshB.Vertices[eb.VertexB]);
                worst = Math.Max(worst, Math.Min(reversed, same));
            }

            if (!edgesOk)
            {
                add(NavIssueSeverity.Error, NavIssueCode.NgpGateEdge, ma, null, g);
                continue;
            }

            if (worst > GateTolerance)
                add(NavIssueSeverity.Warning, NavIssueCode.NgpGateSidesApart, ma, null, g, worst.ToString("0.00"));
        }

        // D: each mesh lists the gates touching it, ascending
        for (int m = 0; m < meshCount; m++)
        {
            var mesh = ngp.Meshes[m];
            var listed = new List<int>();
            bool rangeOk = mesh.GateCount == 0 ? mesh.GateStart == -1 : mesh.GateStart >= 0 && mesh.GateStart + mesh.GateCount <= ngp.MeshGateIndices.Count;
            if (rangeOk)
                for (int k = 0; k < mesh.GateCount; k++)
                    listed.Add(ngp.MeshGateIndices[mesh.GateStart + k]);

            if (!rangeOk || !listed.SequenceEqual(gatesOfMesh[m]))
                add(NavIssueSeverity.Error, NavIssueCode.NgpMeshGateList, m, null, map.Meshes[m].Label);
        }

        // B: each side links to every other gate of that side's mesh, ascending, side A first
        for (int g = 0; g < ngp.Gates.Count; g++)
        {
            var gate = ngp.Gates[g];
            int ma = NGP.Ids.MeshOf(gate.MeshAId), mb = NGP.Ids.MeshOf(gate.MeshBId);
            if (ma >= meshCount || mb >= meshCount || gate.LinkCount > 0 && (gate.LinkStart < 0 || gate.LinkStart + gate.LinkCount > ngp.GateLinks.Count)
                || gate.SideAEdgeStart < 0 || gate.SideBEdgeStart >= ngp.GateEdgeRefs.Count)
                continue;

            var expected = new List<(uint Edge, int Target)>();
            foreach (var other in gatesOfMesh[ma].Where(x => x != g))
                expected.Add((ngp.GateEdgeRefs[gate.SideAEdgeStart], other));
            foreach (var other in gatesOfMesh[mb].Where(x => x != g))
                expected.Add((ngp.GateEdgeRefs[gate.SideBEdgeStart], other));

            var actual = Enumerable.Range(0, gate.LinkCount).Select(k => ngp.GateLinks[gate.LinkStart + k]).Select(l => (l.EdgeRef, l.TargetGate)).ToList();
            if (!actual.SequenceEqual(expected))
                add(NavIssueSeverity.Error, NavIssueCode.NgpGateLinks, ma, null, g, actual.Count, expected.Count);
        }

        // Boundary edges two meshes share that no gate uses: enemies can't cross there
        foreach (var (ma, mb, count, mesh, face) in UnusedSharedEdges(map, usedEdges))
            add(NavIssueSeverity.Warning, NavIssueCode.NgpSharedEdgeNoGate, mesh, face, map.Meshes[ma].Label, map.Meshes[mb].Label, count);
    }

    private static bool EdgeRefOk(NGP ngp, uint r, byte mapIndex, int mesh)
    {
        int m = NGP.Ids.MeshOf(r), e = NGP.Ids.IndexOf(r);
        return r >> 24 == mapIndex && (r & 0x18000) == 0x8000 && m == mesh && e < ngp.Meshes[m].Edges.Count
            && ngp.Meshes[m].Edges[e].Face1 == NGP.Ids.None;
    }

    /// <summary>
    /// The pairs of meshes with coincident (within GateTolerance per end), opposite boundary edges that no gate uses:
    /// the count, and the first such edge's mesh and face.
    /// </summary>
    public static IEnumerable<(int MeshA, int MeshB, int Count, int Mesh, int Face)> UnusedSharedEdges(NgpMap map, HashSet<(int Mesh, int Edge)> used)
    {
        var ngp = map.Ngp;
        float q = GateTolerance;
        (int, int, int) Key(Vector3 v) => ((int)MathF.Round(v.X / q), (int)MathF.Round(v.Y / q), (int)MathF.Round(v.Z / q));

        var byEnds = new Dictionary<((int, int, int), (int, int, int)), List<(int Mesh, int Edge)>>();
        for (int m = 0; m < ngp.Meshes.Count; m++)
        {
            var mesh = ngp.Meshes[m];
            for (int e = 0; e < mesh.Edges.Count; e++)
            {
                var edge = mesh.Edges[e];
                if (edge.Face1 != NGP.Ids.None || used.Contains((m, e)))
                    continue;
                if (edge.VertexA < 0 || edge.VertexA >= mesh.Vertices.Count || edge.VertexB < 0 || edge.VertexB >= mesh.Vertices.Count)
                    continue;

                var a = Key(mesh.Vertices[edge.VertexA]);
                var b = Key(mesh.Vertices[edge.VertexB]);
                var key = a.CompareTo(b) < 0 ? (a, b) : (b, a);
                if (!byEnds.TryGetValue(key, out var list))
                    byEnds[key] = list = new();
                list.Add((m, e));
            }
        }

        var pairs = new Dictionary<(int, int), (int Count, int Mesh, int Face)>();
        foreach (var list in byEnds.Values)
        {
            for (int i = 0; i < list.Count; i++)
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].Mesh == list[j].Mesh)
                        continue;

                    var (x, y) = list[i].Mesh < list[j].Mesh ? (list[i], list[j]) : (list[j], list[i]);
                    var key = (x.Mesh, y.Mesh);
                    if (pairs.TryGetValue(key, out var p))
                        pairs[key] = (p.Count + 1, p.Mesh, p.Face);
                    else
                        pairs[key] = (1, x.Mesh, ngp.Meshes[x.Mesh].Edges[x.Edge].Face0);
                }
        }

        return pairs.OrderBy(p => p.Key).Select(p => (p.Key.Item1, p.Key.Item2, p.Value.Count, p.Value.Mesh, p.Value.Face));
    }
}
