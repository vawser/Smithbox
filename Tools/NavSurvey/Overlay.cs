using Andre.IO.VFS;
using StudioCore.Editors.MapEditor;
using System.Numerics;
using System.Text.RegularExpressions;

// overlay [regex] [max] : the Navigation tool's overlay and face picking on game maps
static class Overlay
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var re = new Regex(args.Length > 1 ? args[1] : ".");
        int max = args.Length > 2 ? int.Parse(args[2]) : 20;
        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value)
            .Distinct().Where(m => re.IsMatch(m)).Take(max).ToList();
        var nvcNames = Regex.Matches(dict, @"""/map/nvc/(m\d\d_\d\d_\d\d_\d\d_m\d\d_\d\d_\d\d_\d\d)\.nvc\.dcx""").Select(m => m.Groups[1].Value).Distinct().ToList();
        var profile = Havok.HavokProfile.FromGame("ER");
        var all = Enum.GetValues<NavOverlayLayer>().ToHashSet();
        var totals = new Dictionary<string, long>();
        void Add(string k, long v) => totals[k] = totals.GetValueOrDefault(k) + v;
        var rng = new Random(1);

        foreach (var mapId in maps)
        {
            var map = NavigationReader.Read(fs, mapId, profile, NavigationReader.NvcNamesFor(mapId, nvcNames));
            if (map == null) continue;

            foreach (var set in new[] { NavmeshSet.N, NavmeshSet.O })
            {
                var overlay = NavigationOverlayBuilder.Build(map, set, _ => Matrix4x4.Identity, Matrix4x4.Identity, all);
                foreach (var ((layer, _), batch) in overlay.Batches)
                {
                    Add($"{set} {layer} lines", batch.Lines.Count);
                    Add($"{set} {layer} triangles", batch.Triangles.Count);
                }
                Add($"{set} face data values", overlay.Batches.Keys.Count(k => k.Layer == NavOverlayLayer.Faces));
                Add($"{set} user edges without a target", overlay.UserEdgesWithoutTarget);

                // Every face's own centre, seen from just above, picks that face (or one at the same spot)
                foreach (var entry in NavigationOverlayBuilder.EntriesOf(map, set))
                {
                    var mesh = map.FileOf(entry)?.Mesh;
                    if (mesh == null || mesh.m_faces.Count == 0) continue;
                    for (int s = 0; s < 20; s++)
                    {
                        int f = rng.Next(mesh.m_faces.Count);
                        var centre = NavigationOverlayBuilder.FaceCentre(mesh, f);
                        var hit = NavigationOverlayBuilder.Pick(map, set, centre + new Vector3(0, 0.05f, 0), -Vector3.UnitY, _ => Matrix4x4.Identity);
                        Add("pick tests", 1);
                        if (hit != null && hit.Entry == entry && hit.Face == f) Add("pick: same face", 1);
                        else if (hit != null && MathF.Abs(hit.Point.Y - centre.Y) < 0.01f) Add("pick: another face at the same point", 1);
                        else Add($"pick: missed ({(hit == null ? "nothing" : "other")})", 1);
                    }
                }
            }

            // How user edges point at their target face
            foreach (var file in map.Files.Values.Where(f => f.Mesh != null))
                foreach (var e in file.Mesh.m_edges.Where(e => ((int)e.m_flags & 16) != 0))
                    Add($"user edge opposite section {(e.m_oppositeFace == 0xFFFFFFFF ? "none" : (e.m_oppositeFace >> 22).ToString("X"))}", 1);
        }

        foreach (var (k, v) in totals.OrderBy(x => x.Key)) Console.WriteLine($"{k}: {v}");
    }
}
