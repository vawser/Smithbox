using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>A face of an NGP mesh under the cursor (map space).</summary>
public sealed record NgpHit(NgpMesh Mesh, int Face, float Distance, Vector3 Point);

/// <summary>
/// The NGP mode's overlay (Phase G0): faces filled by their flags (or one part of them), inner and open edges, the gates'
/// edges and points, and the gate graph's links, as a <see cref="NavOverlay"/> for the shared renderer. Layers reused:
/// ConnectedInMap for gate edges, GateNodes for gate points, GateCosts for gate links, GateSelected for the chosen
/// gate. Renderer-free, so NavSurvey builds it from the game files.
/// </summary>
public static class NgpOverlayBuilder
{
    /// <param name="include">Which meshes to draw; null for all.</param>
    /// <param name="selectedGate">The gate drawn over everything (Gates tab), or -1.</param>
    /// <param name="vertices">A mesh's vertices as edited (Edit tab), or null for its own.</param>
    public static NavOverlay Build(NgpMap map, ISet<NavOverlayLayer> layers, NgpColorBy colorBy,
        Func<NgpMesh, bool> include = null, int selectedGate = -1, Func<NgpMesh, IReadOnlyList<Vector3>> vertices = null)
    {
        var overlay = new NavOverlay { MapId = map.MapId, Set = NavmeshSet.N };
        var lift = new Vector3(0, NavigationOverlayBuilder.Lift, 0);
        var ngp = map.Ngp;

        // Edges a gate uses, drawn as gate edges rather than open edges
        var gateEdges = new HashSet<(int Mesh, int Edge)>();
        foreach (var r in ngp.GateEdgeRefs)
            gateEdges.Add((NGP.Ids.MeshOf(r), NGP.Ids.IndexOf(r)));

        foreach (var nm in map.Meshes)
        {
            if (include != null && !include(nm))
                continue;

            var mesh = nm.Mesh;
            var placed = (vertices?.Invoke(nm) ?? mesh.Vertices).Select(v => map.Place(v) + lift).ToArray();

            if (layers.Contains(NavOverlayLayer.Faces))
            {
                for (int f = 0; f < mesh.Faces.Count; f++)
                {
                    var face = mesh.Faces[f];
                    if (!NgpGeometry.InRange(face, placed.Length))
                        continue;

                    uint flags = f < mesh.FaceFlags.Count ? mesh.FaceFlags[f] : 0;
                    var batch = overlay.Batch(NavOverlayLayer.Faces, NgpFlags.Key(flags, colorBy));
                    batch.Triangles.Add((placed[face.V1], placed[face.V2], placed[face.V3]));
                    batch.FaceCount++;
                }
            }

            bool inner = layers.Contains(NavOverlayLayer.InnerEdges);
            bool open = layers.Contains(NavOverlayLayer.OpenEdges);
            bool gates = layers.Contains(NavOverlayLayer.ConnectedInMap);

            if (inner || open || gates)
            {
                for (int e = 0; e < mesh.Edges.Count; e++)
                {
                    var edge = mesh.Edges[e];
                    if (edge.VertexA < 0 || edge.VertexA >= placed.Length || edge.VertexB < 0 || edge.VertexB >= placed.Length)
                        continue;

                    var line = (placed[edge.VertexA], placed[edge.VertexB]);

                    if (gateEdges.Contains((nm.Index, e)))
                    {
                        if (gates)
                            overlay.Batch(NavOverlayLayer.ConnectedInMap).Lines.Add(line);
                    }
                    else if (edge.Face1 == NGP.Ids.None)
                    {
                        if (open)
                            overlay.Batch(NavOverlayLayer.OpenEdges).Lines.Add(line);
                    }
                    else if (inner)
                    {
                        overlay.Batch(NavOverlayLayer.InnerEdges).Lines.Add(line);
                    }
                }
            }
        }

        // Gates and their links, for the gates of the drawn meshes
        bool Drawn(int mesh) => include == null || (mesh >= 0 && mesh < map.Meshes.Count && include(map.Meshes[mesh]));

        for (int g = 0; g < ngp.Gates.Count; g++)
        {
            var gate = ngp.Gates[g];
            int ma = NGP.Ids.MeshOf(gate.MeshAId), mb = NGP.Ids.MeshOf(gate.MeshBId);
            if (!Drawn(ma) && !Drawn(mb))
                continue;

            var point = map.Place(gate.Position) + lift * 4;

            if (g == selectedGate)
            {
                Cross(overlay.Batch(NavOverlayLayer.GateSelected), point, NavigationOverlayBuilder.GateSize * 1.5f);
                foreach (var line in GateEdgeLines(map, g, lift * 2))
                    overlay.Batch(NavOverlayLayer.GateSelected).Lines.Add(line);
            }
            else if (layers.Contains(NavOverlayLayer.GateNodes))
            {
                Cross(overlay.Batch(NavOverlayLayer.GateNodes), point, NavigationOverlayBuilder.GateSize);
            }

            // Each link once (from the lower gate), or every link of the chosen gate
            for (int k = 0; k < gate.LinkCount; k++)
            {
                int li = gate.LinkStart + k;
                if (li < 0 || li >= ngp.GateLinks.Count)
                    continue;

                int target = ngp.GateLinks[li].TargetGate;
                if (target < 0 || target >= ngp.Gates.Count)
                    continue;

                var to = map.Place(ngp.Gates[target].Position) + lift * 4;

                if (g == selectedGate || target == selectedGate)
                    overlay.Batch(NavOverlayLayer.GateSelected).Lines.Add((point, to));
                else if (target > g && layers.Contains(NavOverlayLayer.GateCosts))
                    overlay.Batch(NavOverlayLayer.GateCosts).Lines.Add((point, to));
            }
        }

        return overlay;
    }

    /// <summary>A gate's side-A edges, placed: the boundary it opens.</summary>
    public static IEnumerable<(Vector3 A, Vector3 B)> GateEdgeLines(NgpMap map, int gate, Vector3 lift)
    {
        var ngp = map.Ngp;
        var g = ngp.Gates[gate];

        for (int k = 0; k < g.EdgeCount; k++)
        {
            int ci = g.SideAEdgeStart + k;
            if (ci < 0 || ci >= ngp.GateEdgeRefs.Count)
                continue;

            uint r = ngp.GateEdgeRefs[ci];
            int m = NGP.Ids.MeshOf(r), e = NGP.Ids.IndexOf(r);
            if (m >= ngp.Meshes.Count || e >= ngp.Meshes[m].Edges.Count)
                continue;

            var mesh = ngp.Meshes[m];
            var edge = mesh.Edges[e];
            if (edge.VertexA < 0 || edge.VertexA >= mesh.Vertices.Count || edge.VertexB < 0 || edge.VertexB >= mesh.Vertices.Count)
                continue;

            yield return (map.Place(mesh.Vertices[edge.VertexA]) + lift, map.Place(mesh.Vertices[edge.VertexB]) + lift);
        }
    }

    /// <summary>The outline of one face, drawn over everything (the picked face).</summary>
    public static NavOverlay Highlight(NgpMap map, NgpHit hit, IReadOnlyList<Vector3> vertices = null)
    {
        var overlay = new NavOverlay { MapId = map.MapId, Set = NavmeshSet.N };
        var lift = new Vector3(0, NavigationOverlayBuilder.Lift * 2, 0);
        var mesh = hit.Mesh.Mesh;
        var v = vertices ?? mesh.Vertices;

        if (hit.Face < 0 || hit.Face >= mesh.Faces.Count || !NgpGeometry.InRange(mesh.Faces[hit.Face], v.Count))
            return overlay;

        var face = mesh.Faces[hit.Face];
        Outline(overlay.Batch(NavOverlayLayer.Highlight), map.Place(v[face.V1]) + lift, map.Place(v[face.V2]) + lift, map.Place(v[face.V3]) + lift);
        return overlay;
    }

    /// <summary>The nearest face a ray in map space hits, or null.</summary>
    public static NgpHit Pick(NgpMap map, Vector3 origin, Vector3 direction, Func<NgpMesh, bool> include = null,
        Func<NgpMesh, IReadOnlyList<Vector3>> vertices = null)
    {
        if (!Matrix4x4.Invert(map.World, out var inverse))
            return null;

        var localOrigin = Vector3.Transform(origin, inverse);
        var localDirection = Vector3.TransformNormal(direction, inverse);
        NgpHit best = null;

        foreach (var nm in map.Meshes)
        {
            if (include != null && !include(nm))
                continue;

            var mesh = nm.Mesh;
            var edited = vertices?.Invoke(nm);
            var v = edited ?? mesh.Vertices;

            // The mesh's bounding box first (a map has up to 115 meshes); edited vertices may lie outside the stored one
            if (edited == null && !RayBox(localOrigin, localDirection, mesh.BoundingBoxMin - new Vector3(0.1f), mesh.BoundingBoxMax + new Vector3(0.1f)))
                continue;

            for (int f = 0; f < mesh.Faces.Count; f++)
            {
                var face = mesh.Faces[f];
                if (!NgpGeometry.InRange(face, v.Count))
                    continue;

                if (!NavigationOverlayBuilder.RayTriangle(localOrigin, localDirection, v[face.V1], v[face.V2], v[face.V3], out float t))
                    continue;

                var point = map.Place(localOrigin + localDirection * t);
                float distance = Vector3.Distance(origin, point);

                if (best == null || distance < best.Distance)
                    best = new NgpHit(nm, f, distance, point);
            }
        }

        return best;
    }

    /// <summary>The gate nearest a ray, within the given distance of it, or -1.</summary>
    public static int PickGate(NgpMap map, Vector3 origin, Vector3 direction, float within)
    {
        var dir = Vector3.Normalize(direction);
        int best = -1;
        float bestAlong = float.MaxValue;

        for (int g = 0; g < map.Ngp.Gates.Count; g++)
        {
            var p = map.Place(map.Ngp.Gates[g].Position);
            float along = Vector3.Dot(p - origin, dir);
            if (along < 0)
                continue;

            if (Vector3.Distance(origin + dir * along, p) <= within && along < bestAlong)
            {
                bestAlong = along;
                best = g;
            }
        }

        return best;
    }

    private static bool RayBox(Vector3 origin, Vector3 direction, Vector3 min, Vector3 max)
    {
        float tMin = float.NegativeInfinity, tMax = float.PositiveInfinity;

        for (int i = 0; i < 3; i++)
        {
            float o = i == 0 ? origin.X : i == 1 ? origin.Y : origin.Z;
            float d = i == 0 ? direction.X : i == 1 ? direction.Y : direction.Z;
            float lo = i == 0 ? min.X : i == 1 ? min.Y : min.Z;
            float hi = i == 0 ? max.X : i == 1 ? max.Y : max.Z;

            if (MathF.Abs(d) < 1e-9f)
            {
                if (o < lo || o > hi)
                    return false;
                continue;
            }

            float t1 = (lo - o) / d, t2 = (hi - o) / d;
            if (t1 > t2)
                (t1, t2) = (t2, t1);

            tMin = MathF.Max(tMin, t1);
            tMax = MathF.Min(tMax, t2);
            if (tMin > tMax)
                return false;
        }

        return tMax >= 0;
    }

    private static void Outline(NavOverlayBatch batch, Vector3 a, Vector3 b, Vector3 c)
    {
        batch.Lines.Add((a, b));
        batch.Lines.Add((b, c));
        batch.Lines.Add((c, a));
    }

    private static void Cross(NavOverlayBatch batch, Vector3 p, float size)
    {
        float h = size / 2;
        batch.Lines.Add((p - new Vector3(h, 0, 0), p + new Vector3(h, 0, 0)));
        batch.Lines.Add((p - new Vector3(0, 0, h), p + new Vector3(0, 0, h)));
        batch.Lines.Add((p, p + new Vector3(0, size, 0)));
    }
}
