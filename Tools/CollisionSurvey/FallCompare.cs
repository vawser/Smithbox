using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

// Checks the Collision Editor's fall protection walls (CollisionEditorFallShapes) against an ER/NR map's f files:
//   1. each game body's shape rebuilt from its own triangles: tree valid (every triangle once, boxes hold them),
//      equal to the game's, and the shape's fields equal the game's;
//   2. a body made by MakeBody equals the game's, field by field (shape aside);
//   3. walls generated along the outer edges of each model's h mesh: how many of the game's wall bottoms lie on them;
//   4. a generated f file written and read back.
//   CollisionSurvey ER <mapId> [max shown] fallcompare [file filter regex]
static class FallCompare
{
    public static void Survey(VirtualFileSystem fs, string gameName, string mapId, int maxShown, Regex filter)
    {
        var profile = Havok.HavokProfile.FromGame(gameName);
        var fall = ReadBinder(fs, mapId, "f", profile, filter);
        var high = ReadBinder(fs, mapId, "h", profile, filter);

        int bodies = 0, valid = 0, equalTree = 0, equalShape = 0, equalBody = 0, shown = 0;

        foreach (var (model, doc) in fall)
        {
            var system = CollisionEditorFallShapes.GetSystem(doc);
            if (system == null) continue;

            foreach (var body in system.m_bodyCinfos)
            {
                if (body.m_shape is not hknpExternMeshShape game) continue;
                bodies++;

                var g = ((hknpDefaultExternMeshShapeGeometry)game.m_geometry).m_geometry;
                var verts = g.m_vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList();
                var idx = g.m_triangles.SelectMany(t => new[] { t.m_a, t.m_b, t.m_c }).ToList();
                var built = CollisionEditorFallShapes.MakeShape(verts, idx);
                var tree = ((hknpExternMeshShapeData)built.m_boundingVolumeData).m_simdTree;

                var problem = CheckTree(tree, verts, idx);
                if (problem == null) valid++;
                else if (shown++ < maxShown) Console.WriteLine($"{model}: tree invalid: {problem}");

                if (SameTree(tree, ((hknpExternMeshShapeData)game.m_boundingVolumeData).m_simdTree)) equalTree++;

                var shapeDiff = Diff(game, built, "shape", ["m_geometry", "m_boundingVolumeData"]);
                var geomDiff = Diff(g, ((hknpDefaultExternMeshShapeGeometry)built.m_geometry).m_geometry, "geometry", []);
                if (shapeDiff.Count + geomDiff.Count == 0) equalShape++;
                else if (shown++ < maxShown) Console.WriteLine($"{model}: shape differs: {string.Join("; ", shapeDiff.Concat(geomDiff).Take(4))}");

                var made = CollisionEditorFallShapes.MakeBody(new CollisionEditorFallShapes.WallBody(verts, idx), body.m_collisionFilterInfo);
                var bodyDiff = Diff(body, made, "body", ["m_shape"]);
                if (bodyDiff.Count == 0) equalBody++;
                else if (shown++ < maxShown) Console.WriteLine($"{model}: body differs: {string.Join("; ", bodyDiff.Take(4))}");
            }
        }

        Console.WriteLine($"bodies {bodies}: rebuilt tree valid {valid}, tree equal to game {equalTree}, shape fields equal {equalShape}, made body equal {equalBody}");

        // 3. Generated walls against the game's, along every outer edge and then only those with a drop past them
        // (the ground being every h file of the map, which assumes the map's collision parts sit at the origin)
        var mapGround = new FallGround();
        foreach (var hdoc in high.Values) mapGround.Add(Havok.Shared.CollisionGeometry.Submeshes(hdoc.Objects), Matrix4x4.Identity);
        var minDrop = float.Parse(Environment.GetEnvironmentVariable("MINDROP") ?? "1.0");

        foreach (var drop in new[] { null, new CollisionEditorFallShapes.DropFilter(mapGround, minDrop) })
        {
            int gameBottoms = 0, gameNear = 0, genBottoms = 0, genNear = 0, models = 0, generatedWalls = 0, gameWalls = 0;

            foreach (var (model, fdoc) in fall)
            {
                if (!high.TryGetValue(model, out var hdoc)) continue;
                var system = CollisionEditorFallShapes.GetSystem(fdoc);
                if (system == null || system.m_bodyCinfos.Count == 0) continue;
                models++;

                var tris = Havok.Shared.CollisionGeometry.Submeshes(hdoc.Objects);
                var result = CollisionEditorFallShapes.BuildWalls(tris, Matrix4x4.Identity, FallProtectionDirection.All, CollisionEditorFallShapes.GameWallHeight, drop);
                generatedWalls += result.TriangleCount / 2;

                var bottoms = result.Bodies.SelectMany(b => b.Vertices.Where((v, i) => i % 2 == 0)).ToList();
                var grid = bottoms.GroupBy(Cell).ToDictionary(e => e.Key, e => e.ToList());

                var game = new List<Vector3>();
                foreach (var body in system.m_bodyCinfos)
                {
                    var g = ((body.m_shape as hknpExternMeshShape)?.m_geometry as hknpDefaultExternMeshShapeGeometry)?.m_geometry;
                    if (g == null) continue;
                    gameWalls += g.m_triangles.Count / 2;
                    for (var i = 0; i < g.m_vertices.Count; i += 2) game.Add(new Vector3(g.m_vertices[i].X, g.m_vertices[i].Y, g.m_vertices[i].Z));
                }
                var gameGrid = game.GroupBy(Cell).ToDictionary(e => e.Key, e => e.ToList());

                foreach (var p in game) { gameBottoms++; if (Near(grid, p, 1f)) gameNear++; }
                foreach (var p in bottoms) { genBottoms++; if (Near(gameGrid, p, 1f)) genNear++; }

                if (models <= maxShown)
                    Console.WriteLine($"{model}: outer {result.OuterEdges} kept {result.KeptEdges}, generated {result.TriangleCount / 2} walls; game {game.Count / 2}ish walls");
            }

            Console.WriteLine($"{(drop == null ? "all outer edges" : $"drop edges (min {minDrop})")}, {models} models: generated walls {generatedWalls} vs game {gameWalls}; " +
                $"game bottoms within 1 m of a generated one {100.0 * gameNear / Math.Max(1, gameBottoms):0.#}%, generated within 1 m of a game one {100.0 * genNear / Math.Max(1, genBottoms):0.#}%");
        }

        // 4. A generated file written and read back
        var template = fall.Values.FirstOrDefault(CollisionEditorFallShapes.IsTemplate);
        var source = high.Values.FirstOrDefault();
        if (template != null && source != null)
        {
            var walls = CollisionEditorFallShapes.BuildWalls(Havok.Shared.CollisionGeometry.Submeshes(source.Objects), Matrix4x4.Identity, FallProtectionDirection.North, 1.8f);
            var system = CollisionEditorFallShapes.GetSystem(template);
            system.m_bodyCinfos.Clear();
            system.m_bodyCinfos.AddRange(walls.Bodies.Select(b => CollisionEditorFallShapes.MakeBody(b, 1)));

            var written = Havok.Files.HavokFile.Write(template);
            var reread = Havok.Files.HavokFile.Read(written, profile, template.Compendium);
            var back = CollisionEditorFallShapes.GetSystem(reread);
            var rereadTris = Havok.Shared.CollisionGeometry.Submeshes(reread.Objects).Sum(t => t.Count / 3);
            var diffs = back.m_bodyCinfos.Zip(system.m_bodyCinfos).SelectMany(p => Diff(p.First, p.Second, "body", [])).ToList();

            Console.WriteLine($"write/read: {walls.Bodies.Count} north bodies {walls.TriangleCount} tris -> {back.m_bodyCinfos.Count} bodies {rereadTris} tris, {written.Length} bytes, field differences {diffs.Count}");
            foreach (var d in diffs.Take(5)) Console.WriteLine("    " + d);
        }
    }

    private static (int, int, int) Cell(Vector3 p) => ((int)MathF.Floor(p.X), (int)MathF.Floor(p.Y), (int)MathF.Floor(p.Z));

    private static bool Near(Dictionary<(int, int, int), List<Vector3>> grid, Vector3 p, float d)
    {
        var (x, y, z) = Cell(p);
        for (var i = -1; i <= 1; i++) for (var j = -1; j <= 1; j++) for (var k = -1; k <= 1; k++)
            if (grid.TryGetValue((x + i, y + j, z + k), out var list) && list.Any(q => Vector3.Distance(q, p) <= d)) return true;
        return false;
    }

    private static string CheckTree(hkcdSimdTree tree, List<Vector3> verts, List<int> idx)
    {
        var seen = new int[idx.Count / 3];
        if (tree.m_nodes.Count < 2 || tree.m_nodes[0].m_isLeaf || tree.m_nodes[1].m_isLeaf) return "sentinel or root";

        var stack = new Stack<int>(); stack.Push(1);
        while (stack.Count > 0)
        {
            var n = tree.m_nodes[stack.Pop()];
            for (var lane = 0; lane < 4; lane++)
            {
                var d = n.m_data[lane];
                var min = new Vector3(Lane(n.m_lx, lane), Lane(n.m_ly, lane), Lane(n.m_lz, lane));
                var max = new Vector3(Lane(n.m_hx, lane), Lane(n.m_hy, lane), Lane(n.m_hz, lane));
                if (n.m_isLeaf)
                {
                    if (d == 0xFFFFFFFF) continue;
                    seen[d]++;
                    for (var k = 0; k < 3; k++) { var v = verts[idx[(int)d * 3 + k]]; if (Vector3.Min(v, min) != min || Vector3.Max(v, max) != max) return $"tri {d} outside its box"; }
                }
                else if (d != 0) stack.Push((int)d);
            }
        }
        return seen.All(s => s == 1) ? null : $"triangles seen {string.Join(",", seen.Select(s => s.ToString()).Distinct())}";
    }

    private static float Lane(Vector4 v, int lane) => lane switch { 0 => v.X, 1 => v.Y, 2 => v.Z, _ => v.W };

    private static bool SameTree(hkcdSimdTree a, hkcdSimdTree b)
    {
        if (a.m_isCompact != b.m_isCompact || a.m_nodes.Count != b.m_nodes.Count) return false;
        for (var i = 0; i < a.m_nodes.Count; i++)
        {
            var x = a.m_nodes[i]; var y = b.m_nodes[i];
            if (x.m_isLeaf != y.m_isLeaf || x.m_isActive != y.m_isActive || x.m_parent != y.m_parent || !x.m_data.SequenceEqual(y.m_data)) return false;
            if (x.m_lx != y.m_lx || x.m_hx != y.m_hx || x.m_ly != y.m_ly || x.m_hy != y.m_hy || x.m_lz != y.m_lz || x.m_hz != y.m_hz) return false;
        }
        return true;
    }

    /// <summary>The public fields that differ between two objects of a class, recursing into Havok objects and lists.</summary>
    private static List<string> Diff(object a, object b, string path, string[] skip)
    {
        var result = new List<string>();
        if (a == null || b == null) { if (a != b) result.Add($"{path}: {a?.GetType().Name ?? "null"} vs {b?.GetType().Name ?? "null"}"); return result; }
        if (a.GetType() != b.GetType()) { result.Add($"{path}: type {a.GetType().Name} vs {b.GetType().Name}"); return result; }

        var type = a.GetType();
        if (type.IsPrimitive || type.IsEnum || a is string || a is Vector4 || a is Vector3 || a is Quaternion || a is Matrix4x4)
        {
            if (!a.Equals(b)) result.Add($"{path}: {a} vs {b}");
            return result;
        }
        if (a is System.Collections.IList la && b is System.Collections.IList lb)
        {
            if (la.Count != lb.Count) { result.Add($"{path}: count {la.Count} vs {lb.Count}"); return result; }
            for (var i = 0; i < la.Count && result.Count < 8; i++) result.AddRange(Diff(la[i], lb[i], $"{path}[{i}]", []));
            return result;
        }
        foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (skip.Contains(f.Name)) continue;
            result.AddRange(Diff(f.GetValue(a), f.GetValue(b), $"{path}.{f.Name}", []));
        }
        return result;
    }

    private static Dictionary<string, Havok.HavokDocument> ReadBinder(VirtualFileSystem fs, string mapId, string type,
        Havok.HavokProfile profile, Regex filter)
    {
        var result = new Dictionary<string, Havok.HavokDocument>();
        var bhdPath = $"/map/{mapId.Substring(0, 3)}/{mapId}/{type}{mapId.Substring(1)}.hkxbhd";
        var bxf = BXF4.Read(fs.ReadFile(bhdPath).Value.ToArray(), fs.ReadFile(Path.ChangeExtension(bhdPath, ".hkxbdt")).Value.ToArray());
        var compFile = bxf.Files.FirstOrDefault(f => f.Name.Contains(".compendium"));
        Havok.HavokCompendium compendium = null;
        if (compFile != null) { var b = compFile.Bytes.ToArray(); if (DCX.Is(b)) b = DCX.Decompress(b).ToArray(); compendium = Havok.Files.HavokFile.ReadCompendium(b, profile); }

        foreach (var file in bxf.Files.Where(f => f.Name.Contains(".hkx.dcx") && filter.IsMatch(f.Name)))
        {
            var name = Path.GetFileName(file.Name);
            var model = name.Replace(".hkx.dcx", "").Substring(name.LastIndexOf('_') + 1);
            var bytes = file.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            try { result[model] = Havok.Files.HavokFile.Read(bytes, profile, compendium); }
            catch (Exception e) { Console.WriteLine($"{name}: read failed {e.Message}"); }
        }
        return result;
    }
}
