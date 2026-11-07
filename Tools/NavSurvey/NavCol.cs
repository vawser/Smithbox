using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Text.RegularExpressions;

// navcol [regex] [max] : how the game's navmeshes sit on the map's collision, to set the parameters of a generator:
// whether navmesh models match collision models by ID, how high navmesh vertices sit over the collision, face sizes and
// slopes, edge lengths, and how far the o set's boundary is inset from the n set's.
static class NavCol
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 10;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var c = new Dictionary<string, long>();
        var examples = new Dictionary<string, string>();
        void Count(string key, long n = 1, string example = null) { c[key] = c.GetValueOrDefault(key) + n; if (example != null) examples.TryAdd(key, example); }
        var values = new Dictionary<string, List<float>>();
        void Value(string key, float v) { if (!values.TryGetValue(key, out var l)) values[key] = l = new(); l.Add(v); }

        foreach (var mapId in maps)
        {
            var map = NavigationReader.Read(fs, mapId, profile, Array.Empty<string>());
            if (map?.Nva == null) continue;

            Dictionary<string, List<Vector3>> collision;
            try { collision = ReadCollision(fs, mapId, profile); }
            catch (Exception e) { Count("collision read failed", 1, $"{mapId}: {e.Message}"); continue; }
            Count("maps");
            Count("collision models", collision.Count);

            var all = collision.Values.SelectMany(t => t).ToList();
            var grid = new TriangleGrid(all, 4f);

            foreach (var model in map.Models.Where(m => !m.IsPlaceholder && m.Quarters.Count == 0))
            {
                var id = $"{model.ModelID:D6}";
                bool match = collision.ContainsKey(id);
                Count(match ? "navmesh model has a collision model with its ID" : "navmesh model without a collision model of its ID", 1, match ? null : $"{mapId} {id}");

                foreach (var set in new[] { NavmeshSet.N, NavmeshSet.O })
                {
                    var mesh = model.File(set)?.Mesh;
                    if (mesh == null || mesh.m_faces.Count == 0) continue;

                    // Faces: vertex counts, slopes, edge lengths
                    for (int f = 0; f < mesh.m_faces.Count; f++)
                    {
                        var face = mesh.m_faces[f];
                        Count($"{set} faces with {Math.Min((int)face.m_numEdges, 8)}{(face.m_numEdges >= 8 ? "+" : "")} edges");
                        var corners = NavigationOverlayBuilder.FaceCorners(mesh, f);
                        var normal = Vector3.Zero;
                        for (int i = 1; i + 1 < corners.Count; i++) normal += Vector3.Cross(corners[i] - corners[0], corners[i + 1] - corners[0]);
                        if (normal.LengthSquared() > 1e-10f)
                        {
                            normal = Vector3.Normalize(normal);
                            Value($"{set} face slope (degrees from flat)", MathF.Acos(MathF.Min(1, MathF.Abs(normal.Y))) * 180 / MathF.PI);
                            Count(normal.Y > 0 ? $"{set} faces wound up (CCW from above)" : $"{set} faces wound down");
                        }
                        if ((f & 7) == 0)
                            for (int k = 0; k < corners.Count; k++)
                                Value($"{set} edge length", Vector3.Distance(corners[k], corners[(k + 1) % corners.Count]));
                    }

                    // Vertex heights over the collision, sampled
                    for (int v = 0; v < mesh.m_vertices.Count; v += 5)
                    {
                        var p = new Vector3(mesh.m_vertices[v].X, mesh.m_vertices[v].Y, mesh.m_vertices[v].Z) + model.Entry(set).Position;
                        var (below, above) = grid.VerticalGaps(p);
                        if (below == null && above == null) { Count($"{set} vertex over no collision"); continue; }
                        if (below != null) Value($"{set} vertex height over the collision below", below.Value);
                        if (above != null && (below == null || above < below)) Count($"{set} vertex nearer collision above than below");
                    }
                }

                // How far the o boundary is inside the n boundary (XZ), sampled
                var n = model.NFile?.Mesh; var o = model.OFile?.Mesh;
                if (n != null && o != null && n.m_faces.Count > 0 && o.m_faces.Count > 0)
                {
                    var nEdges = Boundary(n).ToList();
                    int step = Math.Max(1, Boundary(o).Count() / 200);
                    foreach (var (a, b) in Boundary(o).Where((_, i) => i % step == 0))
                    {
                        var mid = (a + b) / 2;
                        float best = float.MaxValue;
                        foreach (var (p, q) in nEdges) best = MathF.Min(best, DistXZ(mid, p, q));
                        Value("o boundary to the nearest n boundary (XZ)", best);
                    }
                    Value("o/n face area ratio", Area(o) / MathF.Max(1e-3f, Area(n)));
                }

                Count("n file erosion radius " + (model.NFile?.Mesh?.m_erosionRadius ?? -1));
            }
        }

        foreach (var (k, v) in c.OrderBy(x => x.Key)) Console.WriteLine($"{v,10}  {k}{(examples.TryGetValue(k, out var ex) ? "   e.g. " + ex : "")}");
        foreach (var (k, list) in values.OrderBy(x => x.Key))
        {
            list.Sort();
            string P(double q) => list[Math.Min(list.Count - 1, (int)(q * list.Count))].ToString("0.###");
            Console.WriteLine($"{k}: p1 {P(0.01)} p10 {P(0.1)} p50 {P(0.5)} p90 {P(0.9)} p99 {P(0.99)} max {list[^1]:0.###}  (n={list.Count})");
        }
    }

    static IEnumerable<(Vector3, Vector3)> Boundary(hkaiNavMesh m)
    {
        for (int f = 0; f < m.m_faces.Count; f++)
        {
            var face = m.m_faces[f];
            for (int e = face.m_startEdgeIndex; e < face.m_startEdgeIndex + face.m_numEdges; e++)
                if (m.m_edges[e].m_oppositeFace == 0xFFFFFFFF)
                    yield return (V(m, m.m_edges[e].m_a), V(m, m.m_edges[e].m_b));
        }
    }

    static float Area(hkaiNavMesh m)
    {
        float total = 0;
        for (int f = 0; f < m.m_faces.Count; f++)
        {
            var c = NavigationOverlayBuilder.FaceCorners(m, f);
            for (int i = 1; i + 1 < c.Count; i++) total += Vector3.Cross(c[i] - c[0], c[i + 1] - c[0]).Length() / 2;
        }
        return total;
    }

    static Vector3 V(hkaiNavMesh m, int i) => new(m.m_vertices[i].X, m.m_vertices[i].Y, m.m_vertices[i].Z);

    static float DistXZ(Vector3 p, Vector3 a, Vector3 b)
    {
        var p2 = new Vector2(p.X, p.Z); var a2 = new Vector2(a.X, a.Z); var b2 = new Vector2(b.X, b.Z);
        var ab = b2 - a2; float t = ab.LengthSquared() < 1e-10f ? 0 : Math.Clamp(Vector2.Dot(p2 - a2, ab) / ab.LengthSquared(), 0, 1);
        return Vector2.Distance(a2 + ab * t, p2);
    }

    /// <summary>The high collision of a map, by model (the 6 digits past the map name), as world triangles.</summary>
    public static Dictionary<string, List<Vector3>> ReadCollision(VirtualFileSystem fs, string mapId, Havok.HavokProfile profile)
    {
        var bhdPath = $"/map/{mapId.Substring(0, 3)}/{mapId}/h{mapId.Substring(1)}.hkxbhd";
        var bdtPath = Path.ChangeExtension(bhdPath, ".hkxbdt");
        var bxf = BXF4.Read(fs.ReadFile(bhdPath).Value.ToArray(), fs.ReadFile(bdtPath).Value.ToArray());

        var compFile = bxf.Files.FirstOrDefault(f => f.Name.Contains(".compendium"));
        Havok.HavokCompendium compendium = null;
        if (compFile != null)
        {
            var bytes = compFile.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            compendium = Havok.Files.HavokFile.ReadCompendium(bytes, profile);
        }

        var result = new Dictionary<string, List<Vector3>>();
        foreach (var file in bxf.Files.Where(f => f.Name.Contains(".hkx")))
        {
            var name = Path.GetFileName(file.Name).Replace(".dcx", "").Replace(".hkx", "");
            var model = name.Substring(name.LastIndexOf('_') + 1);
            var bytes = file.Bytes.ToArray();
            if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
            var document = Havok.Files.HavokFile.Read(bytes, profile, compendium);
            result[model] = Havok.Shared.CollisionGeometry.Submeshes(document.Objects).SelectMany(t => t).ToList();
        }
        return result;
    }

    /// <summary>Triangles (three points each) bucketed on an XZ grid, for vertical rays.</summary>
    sealed class TriangleGrid
    {
        readonly List<Vector3> T;
        readonly float Cell;
        readonly Dictionary<(int, int), List<int>> Cells = new();

        public TriangleGrid(List<Vector3> triangles, float cell)
        {
            T = triangles; Cell = cell;
            for (int i = 0; i + 2 < T.Count; i += 3)
            {
                float x0 = MathF.Min(T[i].X, MathF.Min(T[i + 1].X, T[i + 2].X)), x1 = MathF.Max(T[i].X, MathF.Max(T[i + 1].X, T[i + 2].X));
                float z0 = MathF.Min(T[i].Z, MathF.Min(T[i + 1].Z, T[i + 2].Z)), z1 = MathF.Max(T[i].Z, MathF.Max(T[i + 1].Z, T[i + 2].Z));
                for (int x = (int)MathF.Floor(x0 / cell); x <= (int)MathF.Floor(x1 / cell); x++)
                    for (int z = (int)MathF.Floor(z0 / cell); z <= (int)MathF.Floor(z1 / cell); z++)
                    {
                        if (!Cells.TryGetValue((x, z), out var l)) Cells[(x, z)] = l = new();
                        l.Add(i);
                    }
            }
        }

        /// <summary>How far the nearest collision is straight below and straight above a point.</summary>
        public (float? Below, float? Above) VerticalGaps(Vector3 p)
        {
            float? below = null, above = null;
            if (!Cells.TryGetValue(((int)MathF.Floor(p.X / Cell), (int)MathF.Floor(p.Z / Cell)), out var list)) return (null, null);
            foreach (var i in list)
            {
                if (HeightAt(T[i], T[i + 1], T[i + 2], p.X, p.Z) is not float y) continue;
                float d = p.Y - y;
                if (d >= -0.05f) { if (below == null || d < below) below = d; }
                else if (above == null || -d < above) above = -d;
            }
            return (below, above);
        }

        static float? HeightAt(Vector3 a, Vector3 b, Vector3 c, float x, float z)
        {
            float d = (b.Z - c.Z) * (a.X - c.X) + (c.X - b.X) * (a.Z - c.Z);
            if (MathF.Abs(d) < 1e-9f) return null;
            float u = ((b.Z - c.Z) * (x - c.X) + (c.X - b.X) * (z - c.Z)) / d;
            float v = ((c.Z - a.Z) * (x - c.X) + (a.X - c.X) * (z - c.Z)) / d;
            float w = 1 - u - v;
            if (u < -1e-4f || v < -1e-4f || w < -1e-4f) return null;
            return u * a.Y + v * b.Y + w * c.Y;
        }
    }
}
