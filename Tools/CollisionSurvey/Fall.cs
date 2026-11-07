using Andre.IO.VFS;
using SoulsFormats;
using System.Numerics;
using System.Text.RegularExpressions;

// What the game's fall protection (f) collision looks like next to the high collision of the same model, for the
// Collision Editor's fall protection generator: how much of it is vertical wall, how tall the walls are, where they
// sit against the high collision's bounds, its materials, and whether its files can be Create templates.
//   CollisionSurvey <GAME> <mapId> [max files] fall [file filter regex]
static class Fall
{
    public static void Survey(VirtualFileSystem fs, string gameName, string mapId, int maxFiles, Regex filter)
    {
        var profile = Havok.HavokProfile.FromGame(gameName);
        var fall = ReadBinder(fs, mapId, "f", profile, filter);
        var high = ReadBinder(fs, mapId, "h", profile, filter);

        Console.WriteLine($"f files {fall.Count}, h files {high.Count}, f with an h of the same model {fall.Keys.Count(high.ContainsKey)}");

        var shown = 0;
        var materials = new Dictionary<uint, int>();
        var templates = 0;
        var verticalAll = new List<float>();

        foreach (var (model, f) in fall.OrderBy(e => e.Key))
        {
            if (f.IsTemplate) templates++;

            foreach (var m in f.Materials) materials[m] = materials.GetValueOrDefault(m) + 1;

            var tris = f.Triangles;
            var vertical = 0;
            var heights = new List<float>();

            for (var t = 0; t < tris.Count / 3; t++)
            {
                var a = tris[t * 3]; var b = tris[t * 3 + 1]; var c = tris[t * 3 + 2];
                var n = Vector3.Cross(b - a, c - a);
                if (n.LengthSquared() < 1e-12f) continue;
                n = Vector3.Normalize(n);
                if (MathF.Abs(n.Y) < 0.05f) vertical++;
                heights.Add(MathF.Max(a.Y, MathF.Max(b.Y, c.Y)) - MathF.Min(a.Y, MathF.Min(b.Y, c.Y)));
            }

            var frac = tris.Count == 0 ? 0 : vertical / (float)(tris.Count / 3);
            verticalAll.Add(frac);

            if (shown++ >= maxFiles) continue;

            var fb = Bounds(tris);
            var line = $"{model}: bodies {f.Bodies} tris {tris.Count / 3} vertical {100 * frac:0}% tri height p50 {Median(heights):0.##} max {(heights.Count == 0 ? 0 : heights.Max()):0.##} " +
                $"f bounds {fb.Min:0.#} - {fb.Max:0.#} mats {string.Join(",", f.Materials.Distinct())} template {f.IsTemplate}";

            if (high.TryGetValue(model, out var h))
            {
                var hb = Bounds(h.Triangles);
                line += $"\n    h bounds {hb.Min:0.#} - {hb.Max:0.#} | f.minY-h.minY {fb.Min.Y - hb.Min.Y:0.##} f.maxY-h.maxY {fb.Max.Y - hb.Max.Y:0.##} " +
                    $"xz grow {fb.Min.X - hb.Min.X:0.##},{fb.Min.Z - hb.Min.Z:0.##} / {fb.Max.X - hb.Max.X:0.##},{fb.Max.Z - hb.Max.Z:0.##}";
            }

            Console.WriteLine(line);

            // The first few walls of the first files, to see their shape
            if (shown <= 3)
            {
                for (var t = 0; t < Math.Min(6, tris.Count / 3); t++)
                    Console.WriteLine($"      tri {t}: {tris[t * 3]:0.##} {tris[t * 3 + 1]:0.##} {tris[t * 3 + 2]:0.##}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"template-able f files {templates}/{fall.Count}");
        Console.WriteLine($"vertical fraction p50 {Median(verticalAll):0.##} files >=90% vertical {verticalAll.Count(v => v >= 0.9f)}/{verticalAll.Count}");
        foreach (var g in Shapes.GroupBy(s => string.Join(" ", s.Split(' ').Skip(1).Take(2)))) Console.WriteLine($"shape {g.Key}: {g.Count()}");
        foreach (var s in Shapes.Where(s => s.Contains("Extern")).Take(6).Concat(Shapes.Where(s => s.Contains("Compound")).Take(4))) Console.WriteLine("  " + s);
        Console.WriteLine($"materials {string.Join(", ", materials.OrderByDescending(e => e.Value).Select(e => $"{e.Key}:{e.Value}"))}");
    }

    public static readonly List<string> Shapes = new();

    private static string Describe(Havok.Shared.IHavokBody b)
    {
        if (b is Havok.hk2018.hknpBodyCinfo c)
        {
            var s = c.m_shape;
            var extra = s is Havok.hk2018.hknpConvexPolytopeShape p ? $" verts {p.m_vertices.Count} faces {p.m_faces.Count} radius {p.m_convexRadius}" : "";
            return $"name {c.m_name} mat {c.m_materialId} filter 0x{c.m_collisionFilterInfo:X} pos {c.m_position:0.##} rot {c.m_orientation:0.###} motion {c.m_motionId} quality {c.m_qualityId} flags {c.m_flags} userData {c.m_userData} shapeUser {s?.m_userData} props {s?.m_properties?.GetType().Name}{extra}";
        }
        if (b is Havok.hk2019.hknpBodyCinfo d)
        {
            var s = d.m_shape;
            var extra = "";
            if (s is Havok.hk2019.hknpExternMeshShape e)
            {
                var g = (e.m_geometry as Havok.hk2019.hknpDefaultExternMeshShapeGeometry)?.m_geometry;
                extra = $" geom {e.m_geometry?.GetType().Name} tris {g?.m_triangles.Count} verts {g?.m_vertices.Count} w {string.Join(",", g?.m_vertices.Select(v => v.W).Distinct() ?? [])} triMat {string.Join(",", g?.m_triangles.Select(t => t.m_material).Distinct() ?? [])} keyBits {e.m_numShapeKeyBits} tag 0x{e.m_shapeTagCodecInfo:X} mutable {e.m_isMutable} useTriMat {(e.m_geometry as Havok.hk2019.hknpDefaultExternMeshShapeGeometry)?.m_useTriangleMaterialAsShapeTag}";
            }
            if (s is Havok.hk2019.hknpCompoundShape cs) extra = $" compound instances {cs.m_instances.m_elements.Count} first {cs.m_instances.m_elements.FirstOrDefault()?.m_shape?.GetType().Name}";
            return $"name {d.m_name} mat {d.m_materialId} filter 0x{d.m_collisionFilterInfo:X} pos {d.m_position:0.##} motion {d.m_motionId} flags {d.m_flags} shapeUser {s?.m_userData}{extra}";
        }
        return "";
    }

    private static float Median(List<float> v) => v.Count == 0 ? 0 : v.OrderBy(x => x).ElementAt(v.Count / 2);

    private static (Vector3 Min, Vector3 Max) Bounds(List<Vector3> points)
    {
        if (points.Count == 0) return (Vector3.Zero, Vector3.Zero);
        var min = new Vector3(float.MaxValue); var max = new Vector3(float.MinValue);
        foreach (var p in points) { min = Vector3.Min(min, p); max = Vector3.Max(max, p); }
        return (min, max);
    }

    private sealed record FileInfo(List<Vector3> Triangles, List<uint> Materials, int Bodies, bool IsTemplate);

    private static Dictionary<string, FileInfo> ReadBinder(VirtualFileSystem fs, string mapId, string type,
        Havok.HavokProfile profile, Regex filter)
    {
        var result = new Dictionary<string, FileInfo>();
        var bhdPath = $"/map/{mapId.Substring(0, 3)}/{mapId}/{type}{mapId.Substring(1)}.hkxbhd";
        var bdtPath = Path.ChangeExtension(bhdPath, ".hkxbdt");

        if (!fs.FileExists(bhdPath))
        {
            Console.WriteLine($"no {type} binder");
            return result;
        }

        var bxf = BXF4.Read(fs.ReadFile(bhdPath).Value.ToArray(), fs.ReadFile(bdtPath).Value.ToArray());

        var compFile = bxf.Files.FirstOrDefault(f => f.Name.Contains(".compendium"));
        Havok.HavokCompendium compendium = null;
        if (compFile != null)
        {
            var bytes = compFile.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            compendium = Havok.Files.HavokFile.ReadCompendium(bytes, profile);
        }

        foreach (var file in bxf.Files.Where(f => f.Name.Contains(".hkx.dcx") && filter.IsMatch(f.Name)))
        {
            var name = Path.GetFileName(file.Name);
            var model = name.Replace(".hkx.dcx", "").Substring(name.LastIndexOf('_') + 1);

            try
            {
                var bytes = file.Bytes.ToArray();
                if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();

                var document = Havok.Files.HavokFile.Read(bytes, profile, compendium);
                var bodies = Havok.Shared.CollisionGeometry.Bodies(document.Objects).ToList();
                if (type == "f" && Shapes.Count < 400) foreach (var b in bodies) Shapes.Add($"{model} {b.GetType().Name} {b.Shape?.GetType().Name} {Describe(b)}");
                var mats = new List<uint>();

                foreach (var body in bodies)
                {
                    if (body.Shape is not Havok.Shared.IHavokCompressedMesh mesh) continue;
                    var byKey = StudioCore.Editors.MapEditor.CollisionEditorShapes.GetMaterialsByKey(body.Shape);
                    var shift = 32 - mesh.BitsPerKey;
                    foreach (var (key, _, _, _) in Havok.Shared.CollisionGeometry.CompressedTriangles(mesh))
                        mats.Add(byKey.GetValueOrDefault(key >> shift));
                }

                result[model] = new FileInfo(
                    Havok.Shared.CollisionGeometry.Submeshes(document.Objects).SelectMany(t => t).ToList(),
                    mats, bodies.Count, StudioCore.Editors.MapEditor.CollisionEditorShapes.IsTemplate(document));
            }
            catch (Exception e)
            {
                Console.WriteLine($"{name}: read failed {e.Message}");
            }
        }

        return result;
    }
}
