using Andre.IO.VFS;
using Havok.hk2018;
using StudioCore.Editors.MapEditor;
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;

// navgen [regex] [maxMaps] [maxModels] [n params] [o params] : NavigationGenerator on the game's collision models.
// For each model with collision of its ID it generates the n and o sets and checks the result (adjacency that pairs back,
// counter-clockwise convex faces, the face tree, the file written and read back), then compares it with the game's
// navmesh: how much of the game's mesh the generated one covers, how much of the generated one the game's covers,
// and the face counts and sizes. Parameter overrides: "CellSize=0.3,AgentRadius=0.4".
static class NavGen
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int maxMaps = args.Length > 2 ? int.Parse(args[2]) : 5;
        int maxModels = args.Length > 3 ? int.Parse(args[3]) : 4;
        var pn = Override(NavGenParameters.DefaultN, args.Length > 4 ? args[4] : "");
        var po = Override(NavGenParameters.DefaultO, args.Length > 5 ? args[5] : "");
        Console.WriteLine($"n: {pn}\no: {po}");

        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(maxMaps).ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var c = new Dictionary<string, long>();
        var examples = new Dictionary<string, string>();
        void Count(string key, long n = 1, string example = null) { c[key] = c.GetValueOrDefault(key) + n; if (example != null) examples.TryAdd(key, example); }
        var values = new Dictionary<string, List<double>>();
        void Value(string key, double v) { if (!values.TryGetValue(key, out var l)) values[key] = l = new(); l.Add(v); }

        foreach (var mapId in maps)
        {
            var map = NavigationReader.Read(fs, mapId, profile, Array.Empty<string>());
            if (map?.Nva == null) continue;
            Dictionary<string, List<Vector3>> collision;
            try { collision = NavCol.ReadCollision(fs, mapId, profile); }
            catch { Count("collision read failed"); continue; }
            var template = NavGenFile.FindTemplate(map);
            if (template == null) { Count("maps without a template file"); continue; }

            var allTriangles = collision.Values.SelectMany(t => t).ToList();
            foreach (var model in map.Models.Where(m => !m.IsPlaceholder && m.Quarters.Count == 0 && m.NFile?.Mesh != null).Take(maxModels))
            {
                // The map's collision within the game mesh's bounds, so the comparison measures the generator, not which files it reads
                var bounds = model.NFile.Mesh.m_aabb;
                var lo = new Vector3(bounds.m_min.X, bounds.m_min.Y, bounds.m_min.Z) - new Vector3(2); var hi = new Vector3(bounds.m_max.X, bounds.m_max.Y, bounds.m_max.Z) + new Vector3(2);
                var triangles = new List<Vector3>();
                for (int t = 0; t + 2 < allTriangles.Count; t += 3)
                {
                    var tmin = Vector3.Min(allTriangles[t], Vector3.Min(allTriangles[t + 1], allTriangles[t + 2])); var tmax = Vector3.Max(allTriangles[t], Vector3.Max(allTriangles[t + 1], allTriangles[t + 2]));
                    if (tmax.X < lo.X || tmin.X > hi.X || tmax.Y < lo.Y || tmin.Y > hi.Y || tmax.Z < lo.Z || tmin.Z > hi.Z) continue;
                    triangles.Add(allTriangles[t]); triangles.Add(allTriangles[t + 1]); triangles.Add(allTriangles[t + 2]);
                }
                Count(collision.TryGetValue($"{model.ModelID:D6}", out var own) && own.Count > 30 ? "model: collision of its ID has geometry" : "model: collision of its ID missing or a stub");
                for (int t = 0; t + 2 < triangles.Count; t += 3)
                {
                    var nrm = Vector3.Cross(triangles[t + 1] - triangles[t], triangles[t + 2] - triangles[t]);
                    if (nrm.LengthSquared() < 1e-12f) continue;
                    nrm = Vector3.Normalize(nrm);
                    Count(nrm.Y > 0.7f ? "collision: flat-ish, cross(b-a, c-a) up" : nrm.Y < -0.7f ? "collision: flat-ish, cross(b-a, c-a) down" : "collision: steep");
                }
                var docs = new Dictionary<NavmeshSet, Havok.HavokDocument>();
                foreach (var (set, p) in new[] { (NavmeshSet.N, pn), (NavmeshSet.O, po) })
                {
                    var game = model.File(set)?.Mesh;
                    if (game == null || game.m_faces.Count == 0) continue;

                    var clock = Stopwatch.StartNew();
                    var result = NavigationGenerator.Generate(triangles, p, game);
                    Value($"{set} seconds", clock.Elapsed.TotalSeconds);
                    if (!result.Ok)
                    {
                        var bmin = triangles.Aggregate(new Vector3(float.MaxValue), Vector3.Min); var bmax = triangles.Aggregate(new Vector3(float.MinValue), Vector3.Max);
                        int up = 0; for (int t = 0; t + 2 < triangles.Count; t += 3) if (Vector3.Cross(triangles[t + 1] - triangles[t], triangles[t + 2] - triangles[t]).Y > 0) up++;
                        var flipped = new List<Vector3>(); for (int t = 0; t + 2 < triangles.Count; t += 3) { flipped.Add(triangles[t]); flipped.Add(triangles[t + 2]); flipped.Add(triangles[t + 1]); }
                        var retry = NavigationGenerator.Generate(flipped, p, game);
                        Count($"{set} generate failed: {result.Error}", 1, $"{mapId} {model.ModelID}");
                        Console.WriteLine($"  failed {mapId} {model.ModelID} {set}: {triangles.Count / 3} triangles, {up} up, bounds {bmin} {bmax}, game faces {game.m_faces.Count} game aabb {game.m_aabb.m_min} {game.m_aabb.m_max}; flipped: {(retry.Ok ? retry.Mesh.m_faces.Count + " faces" : retry.Error)}");
                        continue;
                    }
                    Count($"{set} generated");
                    var mesh = result.Mesh;
                    Value($"{set} snapped vertices %", 100.0 * result.SnappedVertices / Math.Max(1, mesh.m_vertices.Count));

                    var problem = Validate(mesh);
                    Count(problem == null ? $"{set} mesh: valid" : $"{set} mesh: invalid", 1, problem == null ? null : $"{mapId} {model.ModelID}: {problem}");

                    var doc = NavGenFile.Build(template, mesh);
                    docs[set] = doc;
                    if (doc == null) { Count($"{set} file: template without a mediator"); continue; }
                    var back = Havok.Files.HavokFile.Read(Havok.Files.HavokFile.Write(doc), profile);
                    var backMesh = back.ObjectsOf<hkaiNavMesh>().First();
                    var backTree = back.ObjectsOf<hkaiStaticTreeNavMeshQueryMediator>().First().m_tree.m_treePtr.m_tree;
                    var treeProblem = NavigationTree.Check(backTree, backMesh);
                    bool same = backMesh.m_faces.Count == mesh.m_faces.Count && backMesh.m_edges.Count == mesh.m_edges.Count && Validate(backMesh) == null;
                    Count(same && treeProblem == null ? $"{set} file: writes and reads back, tree covers every face" : $"{set} file: problem", 1,
                        same && treeProblem == null ? null : $"{mapId} {model.ModelID}: {treeProblem}");
                    Count(back.ObjectsOf<hkaiNavMeshClearanceCache>().Any() ? $"{set} file: has a clearance cache" : $"{set} file: no clearance cache");

                    // Against the game's
                    Value($"{set} faces generated / game", (double)mesh.m_faces.Count / game.m_faces.Count);
                    Value($"{set} game area covered by generated %", Covered(game, mesh));
                if (set == NavmeshSet.N) Console.WriteLine($"  {mapId} {model.ModelID:D6}: game {game.m_faces.Count} faces covered {Covered(game, mesh):0}%, generated {mesh.m_faces.Count} faces on game {Covered(mesh, map.Models.Select(m => m.File(set)?.Mesh).Where(m => m != null && m.m_faces.Count > 0).ToList()):0}%, {triangles.Count / 3} input triangles, game y {game.m_aabb.m_min.Y:0}..{game.m_aabb.m_max.Y:0} gen y {mesh.m_aabb.m_min.Y:0}..{mesh.m_aabb.m_max.Y:0}");
                    Value($"{set} generated area on any of the game's meshes %", Covered(mesh, map.Models.Select(m => m.File(set)?.Mesh).Where(m => m != null && m.m_faces.Count > 0).ToList()));
                    foreach (var f in Enumerable.Range(0, mesh.m_faces.Count)) Count($"{set} generated faces with {mesh.m_faces[f].m_numEdges} edges");
                }

                // Into a free slot (renamed), and in place of the model; each checked with the validator and undone
                if (docs.Count == 2 && docs.Values.All(d => d != null))
                {
                    int baseErrors = NavigationValidator.Validate(map, _ => null).Count(i => i.Severity == NavIssueSeverity.Error);
                    var nvaBytes = map.Nva.Write();
                    var slot = NavModelEdit.FreeSlots(map).FirstOrDefault();
                    var targets = new List<(string, NavmeshModel, int?)> { ("replace", model, null) };
                    if (slot != null) targets.Add(("slot", slot, Enumerable.Range(900000, 1000).First(id => NavModelEdit.ModelIdFree(map, slot, id))));
                    foreach (var (kind, target, id) in targets)
                    {
                        var state = new NavModelState(map);
                        var r = NavModelEdit.Generate(map, target, docs[NavmeshSet.N], docs[NavmeshSet.O], id);
                        if (!r.Ok) { Count($"generate into {kind}: refused {r.Error}", 1, $"{mapId} {model.ModelID}"); continue; }
                        var files = map.Files.ToDictionary(x => x.Key, x => x.Value);
                        foreach (var ch in r.Files) { if (ch.After == null) files.Remove(ch.Name); else files[ch.Name] = NavigationReader.FromDocument(ch.Name, ch.After); }
                        var after = new NavigationMap(map.MapId, map.Nva, files, map.Onav, map.Nvcs);
                        var errors = NavigationValidator.Validate(after, _ => null).Where(i => i.Severity == NavIssueSeverity.Error).ToList();
                        Count(errors.Count <= baseErrors ? $"generate into {kind}: no new validator errors" : $"generate into {kind}: new validator errors", 1, errors.Count <= baseErrors ? null : $"{mapId} {model.ModelID}: {string.Join("; ", errors.Take(3).Select(i => i.Code + " " + string.Join(",", i.Args)))}");
                        state.Restore();
                        Count(map.Nva.Write().AsSpan().SequenceEqual(nvaBytes) ? $"generate into {kind}: restore byte-identical" : $"generate into {kind}: restore differs", 1);
                    }
                }
            }
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,9}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
        foreach (var (k, list) in values.OrderBy(x => x.Key))
        {
            list.Sort();
            string P(double q) => list[Math.Min(list.Count - 1, (int)(q * list.Count))].ToString("0.###");
            Console.WriteLine($"{k}: p10 {P(0.1)} p50 {P(0.5)} p90 {P(0.9)} mean {list.Average():0.###}  (n={list.Count})");
        }
    }

    static NavGenParameters Override(NavGenParameters p, string text)
    {
        foreach (var part in text.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=');
            var prop = typeof(NavGenParameters).GetProperty(kv[0]);
            object value = prop.PropertyType == typeof(int) ? int.Parse(kv[1]) : prop.PropertyType == typeof(bool) ? bool.Parse(kv[1]) : float.Parse(kv[1]);
            p = p with { };
            prop.SetValue(p, value);
        }
        return p;
    }

    /// <summary>Adjacency that pairs back, faces counter-clockwise from above and convex in XZ.</summary>
    public static string Validate(hkaiNavMesh mesh)
    {
        for (int f = 0; f < mesh.m_faces.Count; f++)
        {
            var face = mesh.m_faces[f];
            if (face.m_numEdges < 3) return $"face {f} has {face.m_numEdges} edges";
            var corners = NavigationOverlayBuilder.FaceCorners(mesh, f);
            for (int k = 0; k < corners.Count; k++)
            {
                var a = corners[k]; var b = corners[(k + 1) % corners.Count]; var cc = corners[(k + 2) % corners.Count];
                float cross = (b.X - a.X) * (cc.Z - b.Z) - (b.Z - a.Z) * (cc.X - b.X);
                // Counter-clockwise from above (Y up) is a negative XZ cross with X right and Z towards the viewer
                if (cross > 1e-4f) return $"face {f} is not convex and counter-clockwise at corner {k}";
            }
            for (int e = face.m_startEdgeIndex; e < face.m_startEdgeIndex + face.m_numEdges; e++)
            {
                var edge = mesh.m_edges[e];
                int next = face.m_startEdgeIndex + (e - face.m_startEdgeIndex + 1) % face.m_numEdges;
                if (edge.m_b != mesh.m_edges[next].m_a) return $"face {f} edges don't chain";
                if (edge.m_oppositeEdge == 0xFFFFFFFF) continue;
                var other = mesh.m_edges[(int)edge.m_oppositeEdge];
                if (other.m_oppositeEdge != e || other.m_oppositeFace != f) return $"edge {e} doesn't pair back";
                if (other.m_a != edge.m_b || other.m_b != edge.m_a) return $"edge {e} and its opposite don't share vertices";
            }
        }
        return mesh.m_faceData.Count == mesh.m_faces.Count && mesh.m_edgeData.Count == mesh.m_edges.Count ? null : "data counts";
    }

    static double Area(hkaiNavMesh m)
    {
        double total = 0;
        for (int f = 0; f < m.m_faces.Count; f++)
        {
            var c = NavigationOverlayBuilder.FaceCorners(m, f);
            for (int i = 1; i + 1 < c.Count; i++) total += Vector3.Cross(c[i] - c[0], c[i + 1] - c[0]).Length() / 2;
        }
        return total;
    }

    /// <summary>The share of mesh A's area, sampled on a 0.5 m grid in each face, that lies over a face of mesh B within 1 m in height.</summary>
    static double Covered(hkaiNavMesh a, hkaiNavMesh b) => Covered(a, new[] { b });

    static double Covered(hkaiNavMesh a, IReadOnlyList<hkaiNavMesh> bs)
    {
        var grid = new Dictionary<(int, int), List<List<Vector3>>>();
        const float cell = 4f;
        foreach (var b in bs)
        for (int f = 0; f < b.m_faces.Count; f++)
        {
            var corners = NavigationOverlayBuilder.FaceCorners(b, f);
            float x0 = corners.Min(p => p.X), x1 = corners.Max(p => p.X), z0 = corners.Min(p => p.Z), z1 = corners.Max(p => p.Z);
            for (int x = (int)MathF.Floor(x0 / cell); x <= (int)MathF.Floor(x1 / cell); x++)
                for (int z = (int)MathF.Floor(z0 / cell); z <= (int)MathF.Floor(z1 / cell); z++)
                {
                    if (!grid.TryGetValue((x, z), out var l)) grid[(x, z)] = l = new();
                    l.Add(corners);
                }
        }

        long inside = 0, total = 0;
        for (int f = 0; f < a.m_faces.Count; f++)
        {
            var corners = NavigationOverlayBuilder.FaceCorners(a, f);
            foreach (var p in Samples(corners))
            {
                total++;
                if (!grid.TryGetValue(((int)MathF.Floor(p.X / cell), (int)MathF.Floor(p.Z / cell)), out var list)) continue;
                foreach (var bc in list)
                {
                    if (InsideXZ(bc, p) && HeightAt(bc, p) is float hb && MathF.Abs(hb - p.Y) < 1.0f) { inside++; break; }
                }
            }
        }
        return total == 0 ? 0 : 100.0 * inside / total;
    }

    static IEnumerable<Vector3> Samples(List<Vector3> corners)
    {
        // The centre, and points of a 0.5 m grid over the face's bounds that fall inside it
        var centre = corners.Aggregate(Vector3.Zero, (s, p) => s + p) / corners.Count;
        yield return centre;
        float x0 = corners.Min(p => p.X), x1 = corners.Max(p => p.X), z0 = corners.Min(p => p.Z), z1 = corners.Max(p => p.Z);
        for (float x = MathF.Ceiling(x0 * 2) / 2; x < x1; x += 0.5f)
            for (float z = MathF.Ceiling(z0 * 2) / 2; z < z1; z += 0.5f)
            {
                var p = new Vector3(x, 0, z);
                if (InsideXZ(corners, p) && HeightAt(corners, p) is float h) yield return p with { Y = h };
            }
    }

    /// <summary>The height of a convex face at a point in XZ, from the fan triangle under it, or null.</summary>
    static float? HeightAt(List<Vector3> poly, Vector3 p)
    {
        for (int i = 1; i + 1 < poly.Count; i++)
        {
            Vector3 a = poly[0], b = poly[i], c = poly[i + 1];
            float d = (b.Z - c.Z) * (a.X - c.X) + (c.X - b.X) * (a.Z - c.Z);
            if (MathF.Abs(d) < 1e-9f) continue;
            float u = ((b.Z - c.Z) * (p.X - c.X) + (c.X - b.X) * (p.Z - c.Z)) / d;
            float v = ((c.Z - a.Z) * (p.X - c.X) + (a.X - c.X) * (p.Z - c.Z)) / d;
            float w = 1 - u - v;
            if (u >= -1e-3f && v >= -1e-3f && w >= -1e-3f) return u * a.Y + v * b.Y + w * c.Y;
        }
        return null;
    }

    static bool InsideXZ(List<Vector3> poly, Vector3 p)
    {
        bool inside = false;
        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
            if ((poly[i].Z > p.Z) != (poly[j].Z > p.Z) && p.X < (poly[j].X - poly[i].X) * (p.Z - poly[i].Z) / (poly[j].Z - poly[i].Z) + poly[i].X)
                inside = !inside;
        return inside;
    }
}