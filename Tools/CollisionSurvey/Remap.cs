using Andre.IO.VFS;
using Havok.Shared;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;

// Checks what the Collision Editor's commit relies on to move Sekiro's wall marks: each low collision is decoded as the
// editor decodes it (welded, with each triangle's stored key), a vertex is moved, the mesh is rebuilt with
// CollisionEditorShapes, and each triangle's new key must find that same triangle in the rebuilt mesh. The WALL and
// NOJUMPWALL keys of each collision must also all be keys the decode gives, so they can all be moved.
//   CollisionSurvey <GAME> <mapId> [max files] remap
static class Remap
{
    public static void Survey(VirtualFileSystem fs, string gameName, string mapId, int maxFiles)
    {
        var profile = Havok.HavokProfile.FromGame(gameName);
        var folder = gameName is "DS3" or "SDT" or "BB" ? $"/map/{mapId}" : $"/map/{mapId.Substring(0, 3)}/{mapId}";

        var binder = BXF4.Read(fs.ReadFile($"{folder}/l{mapId.Substring(1)}.hkxbhd").Value.ToArray(),
            fs.ReadFile($"{folder}/l{mapId.Substring(1)}.hkxbdt").Value.ToArray());

        var compFile = binder.Files.FirstOrDefault(f => f.Name.Contains(".compendium"));
        Havok.HavokCompendium compendium = null;
        if (compFile != null)
        {
            var c = compFile.Bytes.ToArray();
            if (DCX.Is(c)) c = DCX.Decompress(c).ToArray();
            compendium = Havok.Files.HavokFile.ReadCompendium(c, profile);
        }

        // The WALL files of the map, by model (h000100), if it has them
        var walls = new Dictionary<string, WALL>(StringComparer.OrdinalIgnoreCase);
        var noJumps = new Dictionary<string, NOJUMPWALL>(StringComparer.OrdinalIgnoreCase);
        var wallBhd = $"/map/{mapId}/{mapId}.wallbhd";

        if (fs.FileExists(wallBhd))
        {
            var wallBinder = BXF4.Read(fs.ReadFile(wallBhd).Value.ToArray(), fs.ReadFile($"/map/{mapId}/{mapId}.wallbdt").Value.ToArray());

            foreach (var file in wallBinder.Files)
            {
                var name = Path.GetFileName(file.Name.Replace('\\', '/'));
                var model = name.Split('.')[0];
                var data = DCX.Decompress(file.Bytes);

                if (name.EndsWith(".wall.dcx", StringComparison.OrdinalIgnoreCase)) walls[model] = WALL.Read(data);
                else if (name.EndsWith(".nojumpwall.dcx", StringComparison.OrdinalIgnoreCase)) noJumps[model] = NOJUMPWALL.Read(data);
            }
        }

        Console.WriteLine($"{walls.Count} WALL and {noJumps.Count} NOJUMPWALL files");

        int checkedFiles = 0, okFiles = 0, buildFailed = 0;
        long wallMarks = 0, wallFound = 0, noJumpMarks = 0, noJumpFound = 0;

        foreach (var file in binder.Files.Where(f => f.Name.Contains(".hkx.dcx")))
        {
            if (checkedFiles >= maxFiles) break;

            var bytes = file.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            var document = Havok.Files.HavokFile.Read(bytes, profile, compendium);

            var body = CollisionGeometry.Bodies(document.Objects).FirstOrDefault(b => CollisionEditorShapes.IsSupportedShape(b.Shape));
            if (body == null) continue;

            var mesh = (IHavokCompressedMesh)body.Shape;

            // Decoded as CollisionEditorBody.Decode does
            var vertices = new List<Vector3>();
            var weld = new Dictionary<Vector3, int>();
            var indices = new List<int>();
            var keys = new List<uint>();
            var materialOfKey = CollisionEditorShapes.GetMaterialsByKey(body.Shape);
            var materials = new List<uint>();
            var shift = 32 - mesh.BitsPerKey;

            int Add(Vector3 v)
            {
                if (!weld.TryGetValue(v, out var i)) { i = vertices.Count; vertices.Add(v); weld[v] = i; }
                return i;
            }

            foreach (var (key, a, b, c) in CollisionGeometry.CompressedTriangles(mesh))
            {
                indices.Add(Add(a)); indices.Add(Add(b)); indices.Add(Add(c));
                keys.Add(key);
                materials.Add(materialOfKey.GetValueOrDefault(key >> shift));
            }

            if (indices.Count < 3) continue;
            checkedFiles++;

            // The marks that would be moved
            var model = "h" + Path.GetFileName(file.Name.Replace('\\', '/')).Split('_').Last().Replace(".hkx.dcx", "");
            var keySet = new HashSet<uint>(keys);

            if (walls.TryGetValue(model, out var wall))
            {
                wallMarks += wall.Triangles.Count;
                wallFound += wall.Triangles.Count(t => keySet.Contains(t.ShapeKey));
            }

            if (noJumps.TryGetValue(model, out var noJump))
            {
                noJumpMarks += noJump.ShapeKeys.Count;
                noJumpFound += noJump.ShapeKeys.Count(keySet.Contains);
            }

            // An edit: the first vertex raised by half a unit
            vertices[0] += new Vector3(0, 0.5f, 0);

            CollisionEditorShapes.BuiltShape built;
            try
            {
                built = CollisionEditorShapes.BuildShape(body.Shape, vertices, indices, materials);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{Path.GetFileName(file.Name.Replace('\\', '/'))}: {keys.Count} tris, {vertices.Count} verts, BUILD FAILED {e.Message}");
                buildFailed++;
                continue;
            }

            // Each triangle's new key must find it again in the rebuilt mesh
            var rebuilt = CollisionGeometry.CompressedTriangles((IHavokCompressedMesh)built.Shape)
                .ToDictionary(t => t.Key, t => (t.A, t.B, t.C));

            var missing = 0;
            var maxError = 0f;

            for (var t = 0; t < keys.Count; t++)
            {
                if (!rebuilt.TryGetValue(built.StoredKeys[t], out var tri)) { missing++; continue; }

                var expected = new[] { vertices[indices[t * 3]], vertices[indices[t * 3 + 1]], vertices[indices[t * 3 + 2]] };
                var actual = new[] { tri.A, tri.B, tri.C };

                // The builder may start a triangle at another corner; the set of corners must match
                var error = expected.Max(e => actual.Min(a => Vector3.Distance(a, e)));
                maxError = MathF.Max(maxError, error);
            }

            var distinctKeys = built.StoredKeys.Distinct().Count() == built.StoredKeys.Length;
            var ok = missing == 0 && distinctKeys && maxError < 0.05f;
            if (ok) okFiles++;

            Console.WriteLine($"{Path.GetFileName(file.Name.Replace('\\', '/'))}: {keys.Count} tris, new keys distinct {distinctKeys}, " +
                $"not found {missing}, max corner error {maxError:0.####}, WALL {wall?.Triangles.Count ?? 0}, NOJUMP {noJump?.ShapeKeys.Count ?? 0} {(ok ? "OK" : "FAIL")}");
        }

        Console.WriteLine();
        Console.WriteLine($"ALL: {buildFailed} failed to build; {okFiles}/{checkedFiles - buildFailed} rebuilt files find every triangle by its new key; " +
            $"WALL marks on decoded keys {wallFound}/{wallMarks}, NOJUMPWALL {noJumpFound}/{noJumpMarks}");
    }
}
