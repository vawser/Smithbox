using SoulsFormats;
using System.Text;

// Lightmap mode: what DES's MTDs say about g_Lightmap (UV number, shader), how map pieces lay their UVs out against it,
// and what the lightmap textures are. Usage: DesModelSurvey lightmap <USRDIR> <report.md>
static class LightmapSurvey
{
    public static void Run(string root, string reportPath)
    {
        var sb = new StringBuilder("# DES lightmap survey\n\n");

        // MTDs
        var mtds = new Dictionary<string, MTD>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in BND3.Read(Path.Combine(root, "mtd", "mtd.mtdbnd")).Files)
        {
            try { mtds[Path.GetFileNameWithoutExtension(f.Name)] = MTD.Read(f.Bytes.ToArray()); } catch { }
        }

        var uvSig = new Dictionary<string, int>();
        var shaders = new Dictionary<string, int>();
        var lightmapParams = new Dictionary<string, int>();
        foreach (var (name, mtd) in mtds)
        {
            if (!mtd.Textures.Any(t => t.Type == "g_Lightmap")) continue;
            var sig = string.Join(" ", mtd.Textures.Select(t => $"{t.Type}:uv{t.UVNumber}"));
            uvSig[sig] = uvSig.GetValueOrDefault(sig) + 1;
            var sh = Path.GetFileName(mtd.ShaderPath ?? "");
            shaders[sh] = shaders.GetValueOrDefault(sh) + 1;
            foreach (var p in mtd.Params)
            {
                var k = $"{p.Name} = {FormatValue(p.Value)}";
                lightmapParams[k] = lightmapParams.GetValueOrDefault(k) + 1;
            }
        }

        sb.AppendLine("## Lightmapped MTD texture/UV signatures\n");
        foreach (var (k, v) in uvSig.OrderByDescending(x => x.Value)) sb.AppendLine($"- {v}: `{k}`");
        sb.AppendLine("\n## Their shaders\n");
        foreach (var (k, v) in shaders.OrderByDescending(x => x.Value).Take(40)) sb.AppendLine($"- {v}: `{k}`");
        sb.AppendLine("\n## Their params\n");
        foreach (var (k, v) in lightmapParams.OrderByDescending(x => x.Value).Take(60)) sb.AppendLine($"- {v}: `{k}`");

        // Map pieces: UV count per lightmapped material, and the range of each UV set
        var uvLayout = new Dictionary<string, int>();
        var uvRanges = new Dictionary<string, (float min, float max, int n)>();
        (float min, float max, int n) empty = (float.MaxValue, float.MinValue, 0);
        var lightmapNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var sampleCount = 0;
        foreach (var dir in Directory.GetDirectories(Path.Combine(root, "map"), "m??_??_??_??"))
        {
            foreach (var path in Directory.GetFiles(dir, "*.flver"))
            {
                FLVER0 flver;
                try { flver = FLVER0.Read(path); } catch { continue; }

                foreach (var mesh in flver.Meshes)
                {
                    var mat = flver.Materials[mesh.MaterialIndex];
                    var lm = mat.Textures.FirstOrDefault(t => t.ParamName == "g_Lightmap");
                    if (lm == null || mesh.Vertices.Count == 0) continue;

                    lightmapNames.Add(Path.GetFileNameWithoutExtension(lm.Path.Replace('\\', '/')));
                    var hasD2 = mat.Textures.Any(t => t.ParamName == "g_Diffuse_2");
                    var key = $"uvs {mesh.Vertices[0].UVs.Count}, diffuse_2 {(hasD2 ? 1 : 0)}, mtd {Path.GetFileNameWithoutExtension(mat.MTD)}";
                    uvLayout[key] = uvLayout.GetValueOrDefault(key) + 1;

                    if (sampleCount++ > 4000) continue;
                    for (var u = 0; u < mesh.Vertices[0].UVs.Count; u++)
                    {
                        var rk = $"uvs {mesh.Vertices[0].UVs.Count} diffuse_2 {(hasD2 ? 1 : 0)} set {u}";
                        var r = uvRanges.GetValueOrDefault(rk, empty);
                        foreach (var v in mesh.Vertices)
                        {
                            if (u >= v.UVs.Count) continue;
                            r.min = Math.Min(r.min, Math.Min(v.UVs[u].X, v.UVs[u].Y));
                            r.max = Math.Max(r.max, Math.Max(v.UVs[u].X, v.UVs[u].Y));
                        }
                        r.n++;
                        uvRanges[rk] = r;
                    }
                }
            }
        }

        sb.AppendLine("\n## Lightmapped map meshes by UV count\n");
        foreach (var (k, v) in uvLayout.OrderByDescending(x => x.Value).Take(40)) sb.AppendLine($"- {v}: `{k}`");
        sb.AppendLine("\n## UV ranges (min/max of u and v together)\n");
        foreach (var (k, r) in uvRanges.OrderBy(x => x.Key)) sb.AppendLine($"- `{k}`: {r.min:F2} .. {r.max:F2} over {r.n} meshes");

        // The lightmap textures in the area TPFs
        var formats = new Dictionary<string, int>();
        var found = 0;
        foreach (var area in Directory.GetDirectories(Path.Combine(root, "map"), "m??"))
        {
            foreach (var tpfPath in Directory.GetFiles(area, "*.tpf"))
            {
                TPF tpf;
                try { tpf = TPF.Read(tpfPath); } catch { continue; }
                foreach (var t in tpf.Textures)
                {
                    if (!lightmapNames.Contains(t.Name)) continue;
                    found++;
                    var k = $"format {t.Format} type {t.Type} mips {t.Mipmaps} size {Size(t)}";
                    formats[k] = formats.GetValueOrDefault(k) + 1;
                }
            }
        }

        sb.AppendLine($"\n## Lightmap textures: {lightmapNames.Count} names, {found} found in area TPFs\n");
        foreach (var (k, v) in formats.OrderByDescending(x => x.Value).Take(30)) sb.AppendLine($"- {v}: `{k}`");
        sb.AppendLine("\nSample names: " + string.Join(", ", lightmapNames.Take(15)));

        File.WriteAllText(reportPath, sb.ToString());
        Console.WriteLine($"Wrote {reportPath}");
    }

    static string Size(TPF.Texture t) => t.Header != null ? $"{t.Header.Width}x{t.Header.Height}" : $"{t.Bytes.Length}b";

    static string FormatValue(object v) => v switch
    {
        float[] a => string.Join(",", a.Select(x => x.ToString("0.###"))),
        int[] a => string.Join(",", a),
        float f => f.ToString("0.###"),
        _ => v?.ToString() ?? "null"
    };
}
