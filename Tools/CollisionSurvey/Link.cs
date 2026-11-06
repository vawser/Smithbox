using Andre.IO.VFS;
using SoulsFormats;
using System.Numerics;
using System.Text.RegularExpressions;

// How closely the low and high collision of the same model follow each other, for the Collision Editor's linked
// layers: for each model with both, the distance from each low vertex to the high surface, and from each high vertex
// to the low surface, with how many low vertices are exactly high vertices.
//   CollisionSurvey <GAME> <mapId> [max files] link [file filter regex]
static class Link
{
    private const float CellSize = 2f;
    private const float MaxSearch = 8f;

    public static void Survey(VirtualFileSystem fs, string gameName, string mapId, int maxFiles, Regex filter)
    {
        var profile = Havok.HavokProfile.FromGame(gameName);
        var high = ReadBinder(fs, mapId, "h", profile, filter);
        var low = ReadBinder(fs, mapId, "l", profile, filter);

        var lowToHighAll = new List<float>();
        var highToLowAll = new List<float>();
        int exactAll = 0, lowVertsAll = 0, models = 0;

        foreach (var (model, highTris) in high.OrderBy(e => e.Key))
        {
            if (!low.TryGetValue(model, out var lowTris) || highTris.Count < 9 || lowTris.Count < 9)
                continue;

            if (models++ >= maxFiles)
                break;

            var highVerts = new HashSet<Vector3>(highTris);
            var lowVerts = lowTris.Distinct().ToList();
            var highGrid = new TriangleGrid(highTris);
            var lowGrid = new TriangleGrid(lowTris);

            var lowToHigh = lowVerts.Select(highGrid.Distance).ToList();
            var highToLow = highVerts.Select(lowGrid.Distance).ToList();
            var exact = lowVerts.Count(highVerts.Contains);

            lowToHighAll.AddRange(lowToHigh);
            highToLowAll.AddRange(highToLow);
            exactAll += exact;
            lowVertsAll += lowVerts.Count;

            Console.WriteLine($"{model}: high {highTris.Count / 3} tris {highVerts.Count} verts, low {lowTris.Count / 3} tris {lowVerts.Count} verts, " +
                $"low verts exactly high {exact} ({100.0 * exact / lowVerts.Count:0.#}%)");
            Console.WriteLine($"    low->high {Percentiles(lowToHigh)}");
            Console.WriteLine($"    high->low {Percentiles(highToLow)}");
        }

        Console.WriteLine();
        Console.WriteLine($"ALL {models} models: low verts exactly high {exactAll}/{lowVertsAll} ({100.0 * exactAll / Math.Max(1, lowVertsAll):0.#}%)");
        Console.WriteLine($"    low->high {Percentiles(lowToHighAll)}");
        Console.WriteLine($"    high->low {Percentiles(highToLowAll)}");
    }

    private static string Percentiles(List<float> values)
    {
        if (values.Count == 0) return "none";
        var sorted = values.OrderBy(v => v).ToList();
        string P(double p) { var v = sorted[Math.Min(sorted.Count - 1, (int)(p * sorted.Count))]; return v >= MaxSearch ? $">{MaxSearch}" : v.ToString("0.###"); }
        var within = new[] { 0.01f, 0.05f, 0.1f, 0.25f, 0.5f, 1f }
            .Select(d => $"<={d}:{100.0 * sorted.Count(v => v <= d) / sorted.Count:0.#}%");
        return $"p50 {P(0.5)} p90 {P(0.9)} p99 {P(0.99)} max {P(1.0 - 1e-9)} | {string.Join(" ", within)}";
    }

    /// <summary>The world triangles of every file of a binder, by model (the name past the map ID).</summary>
    private static Dictionary<string, List<Vector3>> ReadBinder(VirtualFileSystem fs, string mapId, string type,
        Havok.HavokProfile profile, Regex filter)
    {
        var bhdPath = $"/map/{mapId.Substring(0, 3)}/{mapId}/{type}{mapId.Substring(1)}.hkxbhd";
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

        foreach (var file in bxf.Files.Where(f => f.Name.Contains(".hkx.dcx") && filter.IsMatch(f.Name)))
        {
            var name = Path.GetFileName(file.Name);
            var model = name.Substring(name.IndexOf('_', 4) + 1).Replace(".hkx.dcx", "");

            try
            {
                var bytes = file.Bytes.ToArray();
                if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();

                var document = Havok.Files.HavokFile.Read(bytes, profile, compendium);
                result[model] = Havok.Shared.CollisionGeometry.Submeshes(document.Objects).SelectMany(t => t).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{name}: read failed {e.Message}");
            }
        }

        return result;
    }

    /// <summary>Triangles bucketed on a grid, for the distance from a point to the nearest one.</summary>
    private sealed class TriangleGrid
    {
        private readonly List<Vector3> Corners;
        private readonly Dictionary<(int, int, int), List<int>> Cells = new();

        public TriangleGrid(List<Vector3> corners)
        {
            Corners = corners;

            for (var t = 0; t < corners.Count / 3; t++)
            {
                var min = Vector3.Min(corners[t * 3], Vector3.Min(corners[t * 3 + 1], corners[t * 3 + 2]));
                var max = Vector3.Max(corners[t * 3], Vector3.Max(corners[t * 3 + 1], corners[t * 3 + 2]));
                var (x0, y0, z0) = Cell(min);
                var (x1, y1, z1) = Cell(max);

                for (var x = x0; x <= x1; x++)
                for (var y = y0; y <= y1; y++)
                for (var z = z0; z <= z1; z++)
                {
                    if (!Cells.TryGetValue((x, y, z), out var list)) Cells[(x, y, z)] = list = new();
                    list.Add(t);
                }
            }
        }

        private static (int, int, int) Cell(Vector3 p) =>
            ((int)MathF.Floor(p.X / CellSize), (int)MathF.Floor(p.Y / CellSize), (int)MathF.Floor(p.Z / CellSize));

        /// <summary>The distance to the nearest triangle, or MaxSearch if none is that close.</summary>
        public float Distance(Vector3 p)
        {
            var best = MaxSearch;
            var (cx, cy, cz) = Cell(p);
            var reach = (int)MathF.Ceiling(MaxSearch / CellSize);
            var seen = new HashSet<int>();

            // Rings of cells outwards, stopping once the nearest found is closer than the next ring can be
            for (var r = 0; r <= reach; r++)
            {
                if ((r - 1) * CellSize > best) break;

                for (var x = cx - r; x <= cx + r; x++)
                for (var y = cy - r; y <= cy + r; y++)
                for (var z = cz - r; z <= cz + r; z++)
                {
                    if (Math.Max(Math.Abs(x - cx), Math.Max(Math.Abs(y - cy), Math.Abs(z - cz))) != r) continue;
                    if (!Cells.TryGetValue((x, y, z), out var list)) continue;

                    foreach (var t in list)
                    {
                        if (!seen.Add(t)) continue;
                        var d = Vector3.Distance(p, ClosestPoint(p, Corners[t * 3], Corners[t * 3 + 1], Corners[t * 3 + 2]));
                        if (d < best) best = d;
                    }
                }
            }

            return best;
        }

        // Ericson, Real-Time Collision Detection 5.1.5
        private static Vector3 ClosestPoint(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a, ac = c - a, ap = p - a;
            float d1 = Vector3.Dot(ab, ap), d2 = Vector3.Dot(ac, ap);
            if (d1 <= 0 && d2 <= 0) return a;

            Vector3 bp = p - b;
            float d3 = Vector3.Dot(ab, bp), d4 = Vector3.Dot(ac, bp);
            if (d3 >= 0 && d4 <= d3) return b;

            float vc = d1 * d4 - d3 * d2;
            if (vc <= 0 && d1 >= 0 && d3 <= 0) return a + ab * (d1 / (d1 - d3));

            Vector3 cp = p - c;
            float d5 = Vector3.Dot(ab, cp), d6 = Vector3.Dot(ac, cp);
            if (d6 >= 0 && d5 <= d6) return c;

            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0 && d2 >= 0 && d6 <= 0) return a + ac * (d2 / (d2 - d6));

            float va = d3 * d6 - d5 * d4;
            if (va <= 0 && d4 - d3 >= 0 && d5 - d6 >= 0) return b + (c - b) * ((d4 - d3) / (d4 - d3 + (d5 - d6)));

            float denom = 1 / (va + vb + vc);
            return a + ab * (vb * denom) + ac * (vc * denom);
        }
    }
}
