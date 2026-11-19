using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

// The NGP mode's whole-map regeneration (Navigation Editor DS2 Plan, Phase G6): the Havok mode's Regenerate, for the
// .ngp file. One generation over the map's collision, pruned to what the seeds reach, split into the map's navmeshes
// (one per MSB navmesh part, so the MSB is not touched), with the face flags carried over, steep faces marked as drops,
// and the gates found again along the cuts. Renderer-free, so NavSurvey checks it on the game files.

/// <summary>What an NGP regeneration is built from. Every position is map-local (the NGP's own space).</summary>
public sealed class NgpRegenInput
{
    /// <summary>Walkable triangles, three points each, wound to face up: the map's collision parts, placed.</summary>
    public List<Vector3> Triangles { get; } = new();

    /// <summary>Per triangle: the NvmGroup bit of its collision part (its navmesh's NavmeshGroup bit), or -1.</summary>
    public List<int> Groups { get; } = new();

    /// <summary>Points paths start from: the map's generators and start points.</summary>
    public List<Vector3> Seeds { get; } = new();

    /// <summary>Per mesh (MSB navmesh part, in order): its NavmeshGroup bit, or -1.</summary>
    public List<int> MeshGroups { get; } = new();

    public NavGenParameters Parameters { get; set; } = NgpModelEdit.DefaultParameters;

    /// <summary>Also seed from the current navmesh's faces, so a vanilla map keeps what its navmesh covers.</summary>
    public bool SeedFromCurrent { get; set; } = true;

    /// <summary>Put each new face in the navmesh of the current face under it, so a vanilla map keeps its split.</summary>
    public bool KeepMeshes { get; set; } = true;

    /// <summary>Give each new face the level, and the area type (ground, event area, water…), of the current faces under it.</summary>
    public bool KeepFlags { get; set; } = true;

    /// <summary>Set ground faces at least <see cref="SteepSlope"/> steep to One-way drop, as vanilla drops are.</summary>
    public bool MarkSteep { get; set; } = true;

    public float SteepSlope { get; set; } = 50f;

    /// <summary>Per triangle: whether the faces over it are kept whatever reaches them (collision the user added).</summary>
    public List<bool> SeedTriangles { get; } = new();

    public void Add(IReadOnlyList<Vector3> triangles, int group, bool seed = false)
    {
        for (int t = 0; t + 2 < triangles.Count; t += 3)
        {
            Triangles.Add(triangles[t]);
            Triangles.Add(triangles[t + 1]);
            Triangles.Add(triangles[t + 2]);
            Groups.Add(group);
            SeedTriangles.Add(seed);
        }
    }
}

/// <summary>What an NGP regeneration made.</summary>
public sealed class NgpRegenResult
{
    public NGP Ngp;

    /// <summary>Null when it worked; else the LOC key of why not.</summary>
    public string Error;

    public int Generated, Kept, Seeds, SeedsPlaced;
    public int ByCurrent, ByCollision, BySpread, ByNearest, Dropped;

    /// <summary>Faces kept because they lie over a seed collision (one the user added), before spreading from them.</summary>
    public int SeededByCollision;
    public int FlagsKept, Steep;

    /// <summary>Per mesh: its new face count, or -1 when it got none and keeps its faces as they were.</summary>
    public List<int> MeshFaces = new();

    public int KeptAsBefore => MeshFaces.Count(c => c < 0);
}

public static class NgpRegenerate
{
    /// <summary>The types of strips the game makes small and the helpers make again (doors, ladders, ropes, drops): never carried over to a generated face.</summary>
    private static readonly HashSet<uint> StripTypes = new() { 0x08, 0x10, 0x18, 0x20, 0x28, 0x30, 0x40 };

    /// <summary>Faces of a set of meshes in an XZ grid, for "the face under this point".</summary>
    public sealed class FaceGrid
    {
        private const float Cell = 4f;
        private readonly Dictionary<(int, int), List<(int Mesh, int Face, Vector3 A, Vector3 B, Vector3 C)>> Cells = new();

        public void Add(int mesh, int face, Vector3 a, Vector3 b, Vector3 c)
        {
            float minX = MathF.Min(a.X, MathF.Min(b.X, c.X)), maxX = MathF.Max(a.X, MathF.Max(b.X, c.X));
            float minZ = MathF.Min(a.Z, MathF.Min(b.Z, c.Z)), maxZ = MathF.Max(a.Z, MathF.Max(b.Z, c.Z));
            for (int x = (int)MathF.Floor(minX / Cell); x <= (int)MathF.Floor(maxX / Cell); x++)
                for (int z = (int)MathF.Floor(minZ / Cell); z <= (int)MathF.Floor(maxZ / Cell); z++)
                {
                    if (!Cells.TryGetValue((x, z), out var list))
                        Cells[(x, z)] = list = new();
                    list.Add((mesh, face, a, b, c));
                }
        }

        /// <summary>The face over an XZ point whose height there is within <paramref name="below"/> under and <paramref name="above"/> over a height, nearest first.</summary>
        public (int Mesh, int Face, float Y)? At(Vector3 p, float below, float above)
        {
            if (!Cells.TryGetValue(((int)MathF.Floor(p.X / Cell), (int)MathF.Floor(p.Z / Cell)), out var list))
                return null;

            (int, int, float)? best = null;
            float bestDistance = float.MaxValue;
            var q = new Vector2(p.X, p.Z);
            foreach (var (mesh, face, a, b, c) in list)
            {
                if (!Height(a, b, c, q, out float y) || y < p.Y - below || y > p.Y + above)
                    continue;
                float d = MathF.Abs(y - p.Y);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = (mesh, face, y);
                }
            }
            return best;
        }

        private static bool Height(Vector3 a, Vector3 b, Vector3 c, Vector2 p, out float y)
        {
            Vector2 A = new(a.X, a.Z), B = new(b.X, b.Z), C = new(c.X, c.Z);
            float d = NgpWorkMesh.Orient(A, B, C);
            y = 0;
            if (MathF.Abs(d) < 1e-9f)
                return false;
            float wa = NgpWorkMesh.Orient(B, C, p) / d, wb = NgpWorkMesh.Orient(C, A, p) / d, wc = 1 - wa - wb;
            y = wa * a.Y + wb * b.Y + wc * c.Y;
            return wa >= -1e-4f && wb >= -1e-4f && wc >= -1e-4f;
        }
    }

    /// <summary>Steps 1-2: generates over the input; the whole mesh as vertices, faces and their neighbours across edges.</summary>
    private sealed class Whole
    {
        public List<Vector3> Vertices = new();
        public List<(int A, int B, int C)> Faces = new();
        public List<List<int>> Neighbours = new();

        public Vector3 Centre(int f) => (Vertices[Faces[f].A] + Vertices[Faces[f].B] + Vertices[Faces[f].C]) / 3;

        public static Whole From(List<List<Vector3>> polygons)
        {
            var whole = new Whole();
            var index = new Dictionary<Vector3, int>();
            int V(Vector3 p)
            {
                if (!index.TryGetValue(p, out var i))
                {
                    i = whole.Vertices.Count;
                    index[p] = i;
                    whole.Vertices.Add(p);
                }
                return i;
            }

            // Fanned, clockwise from above as the game's faces (NgpModelEdit.FromPolygons)
            foreach (var polygon in polygons)
            {
                if (polygon.Count < 3)
                    continue;
                var corners = polygon.Select(V).ToList();
                for (int k = 1; k + 1 < corners.Count; k++)
                {
                    int a = corners[0], b = corners[k], c = corners[k + 1];
                    if (a == b || b == c || a == c)
                        continue;
                    if (Vector3.Cross(whole.Vertices[b] - whole.Vertices[a], whole.Vertices[c] - whole.Vertices[a]).Y > 0)
                        (b, c) = (c, b);
                    whole.Faces.Add((a, b, c));
                }
            }

            var edges = new Dictionary<(int, int), List<int>>();
            for (int f = 0; f < whole.Faces.Count; f++)
            {
                var (a, b, c) = whole.Faces[f];
                foreach (var (u, w) in new[] { (a, b), (b, c), (c, a) })
                {
                    var key = (Math.Min(u, w), Math.Max(u, w));
                    if (!edges.TryGetValue(key, out var list))
                        edges[key] = list = new();
                    list.Add(f);
                }
            }

            for (int f = 0; f < whole.Faces.Count; f++)
                whole.Neighbours.Add(new List<int>());
            foreach (var list in edges.Values)
                foreach (var f in list)
                    foreach (var g in list)
                        if (f != g)
                            whole.Neighbours[f].Add(g);

            return whole;
        }
    }

    /// <summary>Generates the map's navmesh again; reads the current NGP only, so it can run in the background.</summary>
    public static NgpRegenResult Run(NGP current, NgpRegenInput input)
    {
        var result = new NgpRegenResult();

        if (input.Triangles.Count < 3)
        {
            result.Error = "MAP_Navigation_Ngp_Regen_Error_Input";
            return result;
        }

        // 1. One generation over all the collision
        var generated = NavigationGenerator.GeneratePolygons(input.Triangles, input.Parameters with { Links = false });
        if (!generated.Ok)
        {
            result.Error = generated.Error;
            return result;
        }

        var whole = Whole.From(generated.Polygons);
        result.Generated = whole.Faces.Count;

        var wholeGrid = new FaceGrid();
        for (int f = 0; f < whole.Faces.Count; f++)
            wholeGrid.Add(0, f, whole.Vertices[whole.Faces[f].A], whole.Vertices[whole.Faces[f].B], whole.Vertices[whole.Faces[f].C]);

        var currentGrid = new FaceGrid();
        for (int m = 0; m < current.Meshes.Count; m++)
        {
            var mesh = current.Meshes[m];
            for (int f = 0; f < mesh.Faces.Count; f++)
                if (NgpGeometry.InRange(mesh.Faces[f], mesh.Vertices.Count))
                {
                    var (a, b, c) = NgpGeometry.Corners(mesh, f);
                    currentGrid.Add(m, f, a, b, c);
                }
        }

        // 2. Pruned to the faces the seeds reach over shared edges: a seed stands on the face under it
        var seeds = input.Seeds.ToList();
        if (input.SeedFromCurrent)
        {
            foreach (var mesh in current.Meshes)
                for (int f = 0; f < mesh.Faces.Count; f += 5)
                    if (NgpGeometry.InRange(mesh.Faces[f], mesh.Vertices.Count))
                    {
                        var (a, b, c) = NgpGeometry.Corners(mesh, f);
                        seeds.Add((a + b + c) / 3);
                    }
        }
        result.Seeds = seeds.Count;

        var reached = new bool[whole.Faces.Count];
        var queue = new Queue<int>();
        foreach (var seed in seeds)
        {
            if (wholeGrid.At(seed, 1.5f, 1.0f) is not var (_, f, _))
                continue;
            result.SeedsPlaced++;
            if (!reached[f])
            {
                reached[f] = true;
                queue.Enqueue(f);
            }
        }

        // Every face over a seed collision (one the user added): a new area isn't joined to the current navmesh, so
        // nothing else would reach it
        if (input.SeedTriangles.Any(s => s))
        {
            var seedGrid = new FaceGrid();
            for (int t = 0; t * 3 + 2 < input.Triangles.Count; t++)
                if (input.SeedTriangles[t])
                    seedGrid.Add(0, t, input.Triangles[t * 3], input.Triangles[t * 3 + 1], input.Triangles[t * 3 + 2]);

            for (int f = 0; f < whole.Faces.Count; f++)
            {
                if (reached[f] || seedGrid.At(whole.Centre(f), 1.5f, 0.5f) == null)
                    continue;
                reached[f] = true;
                queue.Enqueue(f);
                result.SeededByCollision++;
            }
        }

        while (queue.Count > 0)
        {
            int f = queue.Dequeue();
            foreach (int g in whole.Neighbours[f])
                if (!reached[g])
                {
                    reached[g] = true;
                    queue.Enqueue(g);
                }
        }
        result.Kept = reached.Count(r => r);

        // 3. Labels: the navmesh under the face, else the navmesh of the collision under it (its group bit), else
        // spread from labelled neighbours
        var collisionGrid = new FaceGrid();
        for (int t = 0; t * 3 + 2 < input.Triangles.Count; t++)
            if (input.Groups[t] >= 0)
                collisionGrid.Add(input.Groups[t], t, input.Triangles[t * 3], input.Triangles[t * 3 + 1], input.Triangles[t * 3 + 2]);

        var byBit = new Dictionary<int, List<int>>();
        for (int m = 0; m < input.MeshGroups.Count && m < current.Meshes.Count; m++)
            if (input.MeshGroups[m] >= 0)
            {
                if (!byBit.TryGetValue(input.MeshGroups[m], out var list))
                    byBit[input.MeshGroups[m]] = list = new();
                list.Add(m);
            }

        // A bit several navmeshes share (bits are reused across a map): the one whose box is nearest
        int Nearest(List<int> meshes, Vector3 p) => meshes.OrderBy(m =>
        {
            var mesh = current.Meshes[m];
            var clamped = Vector3.Clamp(p, mesh.BoundingBoxMin, mesh.BoundingBoxMax);
            return Vector3.DistanceSquared(clamped, p);
        }).First();

        var label = new int[whole.Faces.Count];
        Array.Fill(label, -1);
        for (int f = 0; f < whole.Faces.Count; f++)
        {
            if (!reached[f])
                continue;
            var centre = whole.Centre(f);

            if (input.KeepMeshes && currentGrid.At(centre, 1.0f, 1.0f) is var (m, _, _))
            {
                label[f] = m;
                result.ByCurrent++;
            }
            else if (collisionGrid.At(centre, 1.5f, 0.5f) is var (bit, _, _) && byBit.TryGetValue(bit, out var meshes))
            {
                label[f] = Nearest(meshes, centre);
                result.ByCollision++;
            }
        }

        var spread = new Queue<int>(Enumerable.Range(0, whole.Faces.Count).Where(f => label[f] >= 0));
        while (spread.Count > 0)
        {
            int f = spread.Dequeue();
            foreach (int g in whole.Neighbours[f])
                if (reached[g] && label[g] < 0)
                {
                    label[g] = label[f];
                    result.BySpread++;
                    spread.Enqueue(g);
                }
        }
        // A reached area with no navmesh under it and no group (new collision on its own): the navmesh whose box is nearest
        {
            var all = Enumerable.Range(0, current.Meshes.Count).Where(m => current.Meshes[m].Faces.Count > 0).ToList();
            if (all.Count > 0)
            {
                for (int f = 0; f < whole.Faces.Count; f++)
                {
                    if (!reached[f] || label[f] >= 0)
                        continue;

                    // The whole unlabelled area goes with its first face's choice, so it stays in one navmesh
                    int m = Nearest(all, whole.Centre(f));
                    var area = new Queue<int>();
                    label[f] = m;
                    area.Enqueue(f);
                    result.ByNearest++;
                    while (area.Count > 0)
                    {
                        int g = area.Dequeue();
                        foreach (int h in whole.Neighbours[g])
                            if (reached[h] && label[h] < 0)
                            {
                                label[h] = m;
                                result.ByNearest++;
                                area.Enqueue(h);
                            }
                    }
                }
            }
        }

        result.Dropped = Enumerable.Range(0, whole.Faces.Count).Count(f => reached[f] && label[f] < 0);

        // 4. Flags: the level and area type most of the current faces under the face have; steep ground marked as drops
        uint Flags(int f)
        {
            uint flags = NgpFlags.DefaultLevel;
            var (ia, ib, ic) = whole.Faces[f];
            Vector3 a = whole.Vertices[ia], b = whole.Vertices[ib], c = whole.Vertices[ic], centre = (a + b + c) / 3;

            if (input.KeepFlags)
            {
                var samples = new List<uint>();
                foreach (var p in new[] { centre, (centre + a) / 2, (centre + b) / 2, (centre + c) / 2 })
                    if (currentGrid.At(p, 1.0f, 1.0f) is var (m, cf, _))
                        samples.Add(current.Meshes[m].FaceFlags[cf]);

                if (samples.Count > 0)
                {
                    uint level = samples.GroupBy(NgpFlags.Level).OrderByDescending(g => g.Count()).First().Key;
                    uint type = samples.Select(NgpFlags.Type).Select(t => StripTypes.Contains(t) ? 0u : t)
                        .GroupBy(t => t).OrderByDescending(g => g.Count()).First().Key;
                    flags = NgpFlags.WithType(level, type);
                    result.FlagsKept++;
                }
            }

            if (input.MarkSteep && NgpFlags.Type(flags) == 0x00)
            {
                var n = Vector3.Cross(b - a, c - a);
                float slope = n.Length() < 1e-6f ? 0 : MathF.Acos(Math.Clamp(MathF.Abs(n.Y) / n.Length(), 0, 1)) * 180 / MathF.PI;
                if (slope >= input.SteepSlope)
                {
                    flags = NgpFlags.WithType(flags, 0x40);
                    result.Steep++;
                }
            }

            return flags;
        }

        // 5. One mesh per navmesh part; one that got no faces keeps its own. The cut edges between meshes are coincident,
        // so the rebuild finds them as gates
        var copy = NgpEdit.Copy(current);
        var changed = new List<int>();
        for (int m = 0; m < copy.Meshes.Count; m++)
        {
            var faces = Enumerable.Range(0, whole.Faces.Count).Where(f => label[f] == m).ToList();
            if (faces.Count == 0)
            {
                result.MeshFaces.Add(-1);
                continue;
            }

            var mesh = new NGP.Mesh { RuntimeSlot = copy.Meshes[m].RuntimeSlot };
            var map = new Dictionary<int, int>();
            short V(int v)
            {
                if (!map.TryGetValue(v, out var i))
                {
                    i = mesh.Vertices.Count;
                    map[v] = i;
                    mesh.Vertices.Add(whole.Vertices[v]);
                }
                return (short)i;
            }

            foreach (int f in faces)
            {
                var (a, b, c) = whole.Faces[f];
                mesh.Faces.Add(new NGP.Face { V1 = V(a), V2 = V(b), V3 = V(c) });
                mesh.FaceFlags.Add(Flags(f));
            }

            if (mesh.Faces.Count > NgpModelEdit.MaxElements || mesh.Vertices.Count > NgpModelEdit.MaxElements)
            {
                result.Error = "MAP_Navigation_Ngp_Regen_Error_Limit";
                return result;
            }

            copy.Meshes[m] = mesh;
            changed.Add(m);
            result.MeshFaces.Add(faces.Count);
        }

        if (changed.Count == 0)
        {
            result.Error = "MAP_Navigation_Ngp_Regen_Error_Nothing";
            return result;
        }

        result.Ngp = NgpRebuild.Rebuild(copy, changed);
        return result;
    }

    /// <summary>
    /// Adds a ladder at each ladder object (foot, +Z and the top of its bounds, map-local), one after another. Returns
    /// the NGP and how many were joined at both ends, at one end, or not made.
    /// </summary>
    public static (NGP Ngp, int Both, int One, int Failed) WithLadders(NGP ngp, IEnumerable<(Vector3 Foot, Vector3 Forward, float Top)> ladders)
    {
        int both = 0, one = 0, failed = 0;
        foreach (var (foot, forward, top) in ladders)
        {
            var r = NgpStripEdit.WithLadder(ngp, foot, forward, top);
            if (r.Ngp == null)
            {
                failed++;
                continue;
            }
            ngp = r.Ngp;
            if (r.BottomJoined && r.TopJoined) both++;
            else if (r.BottomJoined || r.TopJoined) one++;
        }
        return (ngp, both, one, failed);
    }
}
