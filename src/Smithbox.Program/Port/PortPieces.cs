using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SoulsFormats;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.TextureViewer;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Port;

/// <summary>
/// DS3 map pieces into ER map pieces (Material Mapping.md §3-5, §8.2): each DS3 material becomes a MATBIN from its
/// shader's template (one per distinct result over the whole map, names kept across runs by the manifest), the FLVER
/// takes ER's header, skeleton set, the donors' GX lists and layouts, and its vertices are rebuilt for those layouts.
/// The textures the MATBINs name are collected as jobs for <see cref="PortTextures"/>.
/// </summary>
public sealed class PortPieces
{
    public sealed class MetalInfo
    {
        public float Coverage;
        public float Mean;
        public string Note;
        public bool IsMetal;
    }

    public sealed class PortedMaterial
    {
        public string Ds3Mtd;
        public PortRule Rule;
        public string Shader;
        public MATBIN Matbin;
        public string Name;
        public List<string> Dropped = new();
        public List<string> Unfilled = new();
        public MetalInfo Metal;
    }

    /// <summary>A written MATBIN and its allmaterial entry name.</summary>
    public sealed record PortedMatbin(string Name, string BinderName, MATBIN Matbin, string Shader);

    private readonly PortConfig Config;
    private readonly PortGame Input;
    private readonly PortManifest Manifest;
    private readonly PortLog Log;
    private readonly string TargetShort;
    private readonly Dictionary<string, MTD> Mtds;

    /// <summary>The DS3 area's textures: name -> binder file (a TPF).</summary>
    public Dictionary<string, BinderFile> Textures { get; }

    public int AetGroup { get; }
    public SortedDictionary<string, TextureJob> Jobs { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, PortedMatbin> Matbins { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> RuleCounts { get; } = new();

    private readonly Dictionary<string, MetalInfo> MetalCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> Notes = new();

    public PortPieces(PortConfig config, PortGame input, PortGame output, PortManifest manifest, PortLog log, string ds3Map, string targetMap)
    {
        Config = config;
        Input = input;
        Manifest = manifest;
        Log = log;
        TargetShort = targetMap[..6];

        Mtds = BND4.Read(input.Read("/mtd/allmaterialbnd.mtdbnd.dcx")).Files
            .Where(f => f.Name.EndsWith(".mtd", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(f => Path.GetFileNameWithoutExtension(f.Name), f => MTD.Read(f.Bytes), StringComparer.OrdinalIgnoreCase);

        // The DS3 area's texture binders (mAA_000N.tpfbhd)
        Textures = new Dictionary<string, BinderFile>(StringComparer.OrdinalIgnoreCase);
        var area = ds3Map[..3];
        foreach (var bhd in input.Paths.Where(p => Regex.IsMatch(p, $@"^/map/{area}/{area}_\d{{4}}\.tpfbhd$", RegexOptions.IgnoreCase)).OrderBy(p => p))
        {
            var bxf = BXF4.Read(input.Read(bhd), input.Read(Path.ChangeExtension(bhd, ".tpfbdt")));
            foreach (var f in bxf.Files)
                Textures.TryAdd(Path.GetFileName(f.Name.Replace('\\', '/')).Split('.')[0], f);
        }
        Log.Line($"DS3 {ds3Map}: {Mtds.Count} MTDs, {Textures.Count} area textures");

        // The AET group: the manifest's, else the first group from AetGroupFrom that neither the game nor the project uses
        if (manifest.AetGroup >= 0)
            AetGroup = manifest.AetGroup;
        else
        {
            var used = output.Paths.Select(p => Regex.Match(p, @"^/asset/aet/aet(\d{3})/", RegexOptions.IgnoreCase))
                .Where(m => m.Success).Select(m => int.Parse(m.Groups[1].Value)).ToHashSet();
            AetGroup = Enumerable.Range(config.AetGroupFrom, 1000 - config.AetGroupFrom).First(g => !used.Contains(g));
            manifest.AetGroup = AetGroup;
        }
        Log.Line($"AET group {AetGroup:000}");
    }

    /// <summary>
    /// Finds textures the map's own area binders don't have in the other DS3 areas' binders (shared objects such as
    /// m40's Plinth of Fire), one binder at a time, until all are found. Returns how many were found.
    /// </summary>
    public int FindTextures(IEnumerable<string> names, string ds3Map)
    {
        var wanted = names.Where(n => n != null && !Textures.ContainsKey(n)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (wanted.Count == 0) return 0;

        var area = ds3Map[..3];
        int found = 0;
        foreach (var bhd in Input.Paths.Where(p => Regex.IsMatch(p, @"^/map/m\d\d/m\d\d_\d{4}\.tpfbhd$", RegexOptions.IgnoreCase) && !p.StartsWith($"/map/{area}/", StringComparison.OrdinalIgnoreCase)).OrderBy(p => p))
        {
            if (wanted.Count == 0) break;
            var bxf = BXF4.Read(Input.Read(bhd), Input.Read(Path.ChangeExtension(bhd, ".tpfbdt")));
            foreach (var f in bxf.Files)
            {
                var name = Path.GetFileName(f.Name.Replace('\\', '/')).Split('.')[0];
                if (!wanted.Remove(name)) continue;
                Textures[name] = f;
                found++;
                Log.Line($"  {name}: found in {bhd}");
            }
        }
        return found;
    }

    private string Aet(string key, string suffix)
    {
        key = key.ToLowerInvariant();
        if (!Manifest.AetIds.TryGetValue(key, out var id))
        {
            id = Manifest.AetIds.Count == 0 ? 0 : Manifest.AetIds.Values.Max() + 1;
            if (id > 999) throw new InvalidOperationException($"AET group {AetGroup:000} is full (1000 containers)");
            Manifest.AetIds[key] = id;
        }
        return $"AET{AetGroup:000}_{id:000}_{suffix}";
    }

    private void Note(string text) => Notes[text] = Notes.GetValueOrDefault(text) + 1;

    /// <summary>Converts one DS3 map piece FLVER in place into an ER one named newStem. Returns its materials.</summary>
    public List<PortedMaterial> Convert(FLVER2 flver, string stem, string newStem, out List<(bool FloatUv, Vector2 Shift, bool OutOfRange)> uvFixes)
    {
        var ported = new List<PortedMaterial>();

        foreach (var mat in flver.Materials)
            ported.Add(ConvertMaterial(mat));

        // ER header and skeleton, the donors' GX lists, materials pointing at the new MATBINs
        Config.Model.Apply(flver);
        flver.GXLists = new List<FLVER2.GXList>();
        var gxIndex = new Dictionary<string, int>();
        if (flver.Nodes.Count > 0)
            flver.Nodes[0].Name = flver.Nodes[0].Name.Replace(stem, newStem);

        for (int i = 0; i < flver.Materials.Count; i++)
        {
            var pm = ported[i];
            var donor = Donor(pm.Shader);
            if (!gxIndex.TryGetValue(pm.Shader, out var gx))
            {
                gxIndex[pm.Shader] = gx = flver.GXLists.Count;
                flver.GXLists.Add(donor.Gx.ToList());
            }

            flver.Materials[i] = new FLVER2.Material
            {
                Name = flver.Materials[i].Name,
                MTD = pm.Matbin.SourcePath,
                GXIndex = gx,
                Index = i,
                Textures = donor.Textures.Select(t => t.ToTexture()).ToList()
            };
        }

        // The donors' layouts, the vertices rebuilt for them
        var layouts = new List<FLVER2.BufferLayout>();
        var layoutIndex = new Dictionary<string, int>();
        uvFixes = new List<(bool, Vector2, bool)>();
        foreach (var mesh in flver.Meshes)
        {
            var donor = Donor(ported[mesh.MaterialIndex].Shader);
            var key = string.Join(" ", donor.Layout.Select(m => $"{m.Semantic}{m.Index}:{m.Type}"));
            if (!layoutIndex.TryGetValue(key, out var li))
            {
                layoutIndex[key] = li = layouts.Count;
                var layout = new FLVER2.BufferLayout();
                layout.AddRange(donor.Layout.Select(m => m.ToMember()));
                layouts.Add(layout);
            }

            var src = mesh.VertexBuffers.SelectMany(vb => flver.BufferLayouts[vb.LayoutIndex]).ToList();
            var dst = layouts[li];
            foreach (var v in mesh.Vertices) Remap(v, src, dst, donor.NormalW);

            // ER's Short2 UVs hold +-16: a mesh beyond that moves by whole UVs (the textures repeat, so nothing changes)
            var floatUv = src.FirstOrDefault(m => m.Semantic == FLVER.LayoutSemantic.UV)?.Type is FLVER.LayoutType.Float2 or FLVER.LayoutType.Float3 or FLVER.LayoutType.Float4;
            var shift = Vector2.Zero;
            var outOfRange = false;
            var realUvs = dst.Where(m => m.Semantic == FLVER.LayoutSemantic.UV)
                .SelectMany(m => Enumerable.Repeat(m.Type != FLVER.LayoutType.UByte4Norm, Entries(m.Type))).ToList();
            if (mesh.Vertices.Count > 0 && realUvs.Count > 0 && realUvs[0])
            {
                var min = new Vector2(mesh.Vertices.Min(v => v.UVs[0].X), mesh.Vertices.Min(v => v.UVs[0].Y));
                var max = new Vector2(mesh.Vertices.Max(v => v.UVs[0].X), mesh.Vertices.Max(v => v.UVs[0].Y));
                if (min.X < -15.99f || min.Y < -15.99f || max.X > 15.99f || max.Y > 15.99f)
                {
                    shift = -new Vector2(MathF.Round((min.X + max.X) / 2), MathF.Round((min.Y + max.Y) / 2));
                    foreach (var v in mesh.Vertices)
                        for (int k = 0; k < realUvs.Count; k++)
                            if (realUvs[k]) v.UVs[k] += new Vector3(shift, 0);
                    var lo = min + shift;
                    var hi = max + shift;
                    outOfRange = lo.X < -16 || lo.Y < -16 || hi.X > 32767f / 2048 || hi.Y > 32767f / 2048;
                    if (outOfRange)
                        Log.Warn($"{newStem}: a mesh's UVs span {max.X - min.X:0.#} x {max.Y - min.Y:0.#}, beyond ER's Short2 range even after the shift");
                }
            }
            uvFixes.Add((floatUv, shift, outOfRange));

            mesh.VertexBuffers = [new FLVER2.VertexBuffer(li)];
        }
        flver.BufferLayouts = layouts;

        return ported;
    }

    private DonorTemplate Donor(string shader) => Config.Shaders[shader].Donor ?? Config.Shaders[Config.DefaultShader].Donor;

    private PortedMaterial ConvertMaterial(FLVER2.Material mat)
    {
        var mtdName = Leaf(mat.MTD);
        var name = MtdName.Parse(mtdName);
        Mtds.TryGetValue(mtdName, out var mtd);

        var ds3Ranked = PortRoles.Rank(mat.Textures.Select(t => t.ParamName));
        string Tex(string role, int rank) => mat.Textures.Zip(ds3Ranked).Where(x => x.Second.Role == role && x.Second.Rank == rank)
            .Select(x => Leaf(x.First.Path)).FirstOrDefault(p => p.Length > 0);

        var albedo = Tex("A", 0);
        var spec = Tex("M", 0);
        var metal = Metal(spec);
        var rule = Config.Rules.FirstOrDefault(r => r.Matches(name, metal.IsMetal));
        var shader = rule?.Shader ?? Config.DefaultShader;
        if (!Config.Shaders.TryGetValue(shader, out var tpl)) { shader = Config.DefaultShader; tpl = Config.Shaders[shader]; }
        RuleCounts[rule?.Id ?? "(default)"] = RuleCounts.GetValueOrDefault(rule?.Id ?? "(default)") + 1;

        var pm = new PortedMaterial { Ds3Mtd = mtdName, Rule = rule, Shader = shader, Metal = metal };

        var mb = new MATBIN { ShaderPath = tpl.ShaderPath };
        foreach (var p in tpl.Params) mb.Params.Add(p.ToParam());
        foreach (var c in Config.CopiedParams)
        {
            var dst = mb.Params.FirstOrDefault(p => p.Name == c);
            var src = mtd?.Params.FirstOrDefault(p => p.Name == c);
            if (dst != null && src != null) dst.Value = CopyInto(dst.Value, src.Value);
        }

        var isMetal = tpl.Samplers.Any(x => x.Role == "M") && spec != null;
        var used = new HashSet<int>();
        foreach (var s in tpl.Samplers)
        {
            var sampler = new MATBIN.Sampler { Type = s.Type, Unk14 = new Vector2(s.Unk14[0], s.Unk14[1]) };
            var hit = mat.Textures.Select((t, i) => (t, i, r: ds3Ranked[i]))
                .FirstOrDefault(x => !used.Contains(x.i) && x.r.Role == s.Role && x.r.Rank == s.Rank && Leaf(x.t.Path).Length > 0);

            if (hit.t != null)
            {
                used.Add(hit.i);
                var leaf = Leaf(hit.t.Path);
                var ds3Base = Regex.Replace(leaf, "_[^_]+$", "");

                // A metal material's albedo is recoloured with its F0, so it and its metallic map get a container
                // of their own (keyed by albedo + specular map); everything else is keyed by its DS3 name
                var metalKey = $"{Regex.Replace(albedo ?? "", "_[^_]+$", "")}|{spec}";
                var own = isMetal && s.Role is "M" or "A" && s.Rank == 0;
                var aet = own ? Aet(metalKey, Config.Suffixes[s.Role]) : Aet(ds3Base, Config.Suffixes[s.Role]);
                var kind = !own ? TextureJobKind.Copy : s.Role == "M" ? TextureJobKind.Metallic : TextureJobKind.MetalAlbedo;
                var source = kind == TextureJobKind.Metallic ? albedo : leaf;
                if (Jobs.TryGetValue(aet, out var prior) && (prior.Kind != kind || !string.Equals(prior.Source, source, StringComparison.OrdinalIgnoreCase)))
                    Log.Warn($"{aet}: wanted as {kind} of {leaf}, already {prior.Kind} of {prior.Source}");
                Jobs.TryAdd(aet, new TextureJob { Aet = aet, Kind = kind, Source = source, Spec = spec });
                sampler.Path = $@"N:\GR\data\Asset\Environment\texture\AET{AetGroup:000}\{aet}.tif";

                var scale = hit.t.TilingScale;
                if (s.Role == "O" && mtd?.Params.FirstOrDefault(p => p.Name == "g_DetailBumpCoordScale")?.Value is float[] d && d.Length >= 2)
                    scale *= new Vector2(d[0], d[1]);
                if (scale != Vector2.One) sampler.Unk14 = scale;
            }
            else
            {
                sampler.Path = s.FallbackPath;
                pm.Unfilled.Add($"{s.Role}{s.Rank}");
            }
            mb.Samplers.Add(sampler);
        }

        for (int i = 0; i < mat.Textures.Count; i++)
            if (!used.Contains(i) && Leaf(mat.Textures[i].Path).Length > 0)
                pm.Dropped.Add($"{ds3Ranked[i].Role}{ds3Ranked[i].Rank}");

        // One MATBIN per distinct result, its name kept across runs
        var identity = TargetShort + "|" + mb.ShaderPath + "|" + string.Join("|", mb.Params.Select(p => ParamTemplate.Format(p.Value)))
            + "|" + string.Join("|", mb.Samplers.Select(s => $"{s.Path}@{s.Unk14.X},{s.Unk14.Y}"));
        if (!Manifest.Matbins.TryGetValue(identity, out var matbinName))
        {
            var taken = Manifest.Matbins.Values.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var n = 1;
            while (taken.Contains($"{TargetShort}_port_{n:000}")) n++;
            matbinName = $"{TargetShort}_port_{n:000}";
            Manifest.Matbins[identity] = matbinName;
        }

        if (Matbins.TryGetValue(matbinName, out var existing))
        {
            pm.Matbin = existing.Matbin;
        }
        else
        {
            mb.SourcePath = $@"N:\GR\data\Material\mtd\Map_{TargetShort}\matxml\{matbinName}.matxml";
            MaterialKeys.Apply(mb);
            foreach (var p in mb.Params) MaterialKeys.Apply(p);
            foreach (var s in mb.Samplers) MaterialKeys.Apply(s);
            pm.Matbin = mb;

            var binderName = Regex.Replace(tpl.BinderName, @"Map_m\d\d_\d\d", $"Map_{TargetShort}");
            binderName = Regex.Replace(binderName, @"\\matbin_DLC\d\d\\", @"\matbin\", RegexOptions.IgnoreCase);
            binderName = binderName[..(binderName.LastIndexOf('\\') + 1)] + matbinName + ".matbin";
            Matbins[matbinName] = new PortedMatbin(matbinName, binderName, mb, shader);
        }
        pm.Name = matbinName;

        foreach (var d in pm.Dropped.Distinct()) Note($"DS3 texture with no ER sampler dropped: {d}");
        foreach (var u in pm.Unfilled.Distinct()) Note($"ER sampler with no DS3 texture ({shader}): {u}");
        if (rule?.Status is "open" or "handcheck") Note($"{rule.Status}: {rule.Id} ({rule.Note})");
        return pm;
    }

    /// <summary>The notes collected over the map, most common first.</summary>
    public void LogNotes()
    {
        foreach (var (text, count) in Notes.OrderByDescending(n => n.Value))
            Log.Line($"  {count,5}  {text}");
    }

    public static string Leaf(string path) => string.IsNullOrEmpty(path) ? "" : Path.GetFileNameWithoutExtension(path.Replace('\\', '/'));

    // ---- metallic test ------------------------------------------------------------------------------------------

    /// <summary>
    /// Whether a DS3 material is metal: the share of its specular map's pixels whose linear F0 luma is above
    /// MetallicThreshold (0.1: dielectrics sit at 0.02-0.08), against MetallicCoverage (Material Mapping.md M7).
    /// </summary>
    private MetalInfo Metal(string spec)
    {
        if (spec == null) return new MetalInfo { Note = "no specular texture" };
        if (MetalCache.TryGetValue(spec, out var hit)) return hit;

        var info = new MetalInfo();
        MetalCache[spec] = info;
        var (rgb, w, h) = Specular(Textures, spec);
        if (rgb == null) { info.Note = $"{spec} not in the area binders"; return info; }

        int above = 0;
        double sum = 0;
        for (int i = 0; i < w * h; i++)
        {
            var f0 = 0.2126f * rgb[i * 3] + 0.7152f * rgb[i * 3 + 1] + 0.0722f * rgb[i * 3 + 2];
            sum += f0;
            if (f0 > Config.MetallicThreshold) above++;
        }

        info.Coverage = above / (float)(w * h);
        info.Mean = (float)(sum / (w * h));
        info.IsMetal = info.Coverage >= Config.MetallicCoverage;
        return info;
    }

    internal static float ToLinear(float v) => v <= 0.04045f ? v / 12.92f : MathF.Pow((v + 0.055f) / 1.055f, 2.4f);

    /// <summary>
    /// The specular map as linear RGB. Shared constant-colour maps are named for their sRGB value
    /// (Reflectance_060_060_060_r) and live outside the area binders.
    /// </summary>
    internal static (float[] Rgb, int W, int H) Specular(Dictionary<string, BinderFile> textures, string spec)
    {
        var (img, srgb) = Decode(textures, spec);
        var constant = Regex.Match(spec ?? "", @"^Reflectance_(\d{3})_(\d{3})_(\d{3})_r$", RegexOptions.IgnoreCase);
        if (img == null && constant.Success)
            (img, srgb) = (new Image<Rgba32>(1, 1, new Rgba32(byte.Parse(constant.Groups[1].Value), byte.Parse(constant.Groups[2].Value), byte.Parse(constant.Groups[3].Value))), true);
        if (img == null) return (null, 0, 0);

        var rgb = new float[img.Width * img.Height * 3];
        for (int y = 0; y < img.Height; y++)
            for (int x = 0; x < img.Width; x++)
            {
                var p = img[x, y];
                var i = (y * img.Width + x) * 3;
                rgb[i] = srgb ? ToLinear(p.R / 255f) : p.R / 255f;
                rgb[i + 1] = srgb ? ToLinear(p.G / 255f) : p.G / 255f;
                rgb[i + 2] = srgb ? ToLinear(p.B / 255f) : p.B / 255f;
            }
        var result = (rgb, img.Width, img.Height);
        img.Dispose();
        return result;
    }

    /// <summary>A texture's top mip as stored, and whether its DDS format is sRGB (the GPU linearises it on sampling).</summary>
    internal static (Image<Rgba32> Image, bool Srgb) Decode(Dictionary<string, BinderFile> textures, string name)
    {
        if (name == null || !textures.TryGetValue(name, out var f)) return (null, false);
        var tex = TPF.Read(f.Bytes).Textures.FirstOrDefault();
        if (tex == null) return (null, false);
        var dds = tex.Bytes.ToArray();
        // DX10 header: DXGI format at 128. sRGB: R8G8B8A8 29, BC1 72, BC2 75, BC3 78, B8G8R8A8 91, B8G8R8X8 93, BC7 99.
        // A legacy FourCC (DXT1...) has no sRGB flag: DS3 authors these maps in sRGB, so it counts as sRGB
        var srgb = dds.Length < 148 || System.Text.Encoding.ASCII.GetString(dds, 84, 4) != "DX10"
            || BitConverter.ToInt32(dds, 128) is 29 or 72 or 75 or 78 or 91 or 93 or 99;
        return (DdsDecoder.DecodeToImage(dds, out _), srgb);
    }

    /// <summary>A DS3 MTD value written into an ER param: the DS3 values fill the first elements, the rest keep the template's.</summary>
    private static object CopyInto(object current, object ds3)
    {
        var src = ds3 switch
        {
            bool b => [b ? 1f : 0f],
            int i => [i],
            float f => [f],
            int[] a => a.Select(x => (float)x).ToArray(),
            float[] a => a,
            _ => Array.Empty<float>()
        };
        if (src.Length == 0) return current;
        return current switch
        {
            bool => src[0] != 0,
            int => (int)src[0],
            float => src[0],
            int[] a => a.Select((x, i) => i < src.Length ? (int)src[i] : x).ToArray(),
            float[] a => a.Select((x, i) => i < src.Length ? src[i] : x).ToArray(),
            _ => current
        };
    }

    // ---- vertices -----------------------------------------------------------------------------------------------

    /// <summary>
    /// DS3's float UVs (MultiBlend's UV0/UV1 Float2) hold the Short-scaled value: its vertex shader multiplies them by
    /// 1/2048 (MultiBlend2_et1_lightmap Fwd.vpo: mul o5.xy, v4.xy, 0.000488).
    /// </summary>
    public const float UvFloatScale = 2048;

    private static int Entries(FLVER.LayoutType t) => t is FLVER.LayoutType.Float4 or FLVER.LayoutType.UByte4Norm or FLVER.LayoutType.Short4 or FLVER.LayoutType.Half4 ? 2 : 1;

    /// <summary>
    /// Rebuilds a vertex's UV, colour and tangent lists for the ER layout. UVs: byte-normalised ER members take the DS3
    /// byte-normalised UVs (DS3 MultiBlend's TEXCOORD2: .x is the blend weight), the others the DS3 non-byte UVs in
    /// order, float ones divided by 2048 (so UV0 keeps its first pair and the DS3 lightmap pair drops). Colours: the same
    /// index (DS3 MultiBlend's COLOR0.a sets the blend edge sharpness), else white. Tangents: in order, else the first.
    /// </summary>
    private void Remap(FLVER.Vertex v, List<FLVER.LayoutMember> src, FLVER2.BufferLayout dst, int normalW)
    {
        static bool ByteNorm(FLVER.LayoutType t) => t == FLVER.LayoutType.UByte4Norm;
        static bool IsFloat(FLVER.LayoutType t) => t is FLVER.LayoutType.Float2 or FLVER.LayoutType.Float3 or FLVER.LayoutType.Float4;

        var real = new List<Vector3>();
        var bytes = new List<Vector3>();
        var colors = new Dictionary<int, FLVER.VertexColor>();
        int uv = 0, color = 0;
        foreach (var m in src)
        {
            if (m.Semantic == FLVER.LayoutSemantic.UV)
                for (int k = 0; k < Entries(m.Type); k++)
                {
                    var e = v.UVs[uv++];
                    if (ByteNorm(m.Type)) bytes.Add(e);
                    else real.Add(IsFloat(m.Type) ? e / UvFloatScale : e);
                }
            else if (m.Semantic == FLVER.LayoutSemantic.VertexColor)
                colors[m.Index] = v.Colors[color++];
        }

        var uvs = new List<Vector3>();
        var cols = new List<FLVER.VertexColor>();
        var tans = new List<Vector4>();
        int ri = 0, bi = 0, ti = 0;
        foreach (var m in dst)
        {
            if (m.Semantic == FLVER.LayoutSemantic.UV)
                for (int k = 0; k < Entries(m.Type); k++)
                {
                    var from = ByteNorm(m.Type) ? bytes : real;
                    ref var i = ref (ByteNorm(m.Type) ? ref bi : ref ri);
                    if (i < from.Count) uvs.Add(from[i++]);
                    else uvs.Add(Vector3.Zero);
                }
            else if (m.Semantic == FLVER.LayoutSemantic.VertexColor)
            {
                if (colors.TryGetValue(m.Index, out var c)) cols.Add(c);
                else cols.Add(new FLVER.VertexColor(1f, 1f, 1f, 1f));
            }
            else if (m.Semantic == FLVER.LayoutSemantic.Tangent)
            {
                if (ti < v.Tangents.Count) tans.Add(v.Tangents[ti++]);
                else tans.Add(v.Tangents.Count > 0 ? v.Tangents[0] : new Vector4(1, 0, 0, 1));
            }
        }

        var srcNormal = src.First(m => m.Semantic == FLVER.LayoutSemantic.Normal).Type;
        if (srcNormal != FLVER.LayoutType.UByte4 && srcNormal != FLVER.LayoutType.UByte4Norm && srcNormal != FLVER.LayoutType.Byte4)
            v.NormalW = normalW;

        v.UVs = uvs;
        v.Colors = cols;
        v.Tangents = tans;
    }

    /// <summary>The ported FLVER against the DS3 one: counts, faces, positions exact, normals, UV0.</summary>
    public void CheckPiece(string stem, FLVER2 ds3, FLVER2 er, List<(bool FloatUv, Vector2 Shift, bool OutOfRange)> uvFixes)
    {
        Log.Check(er.Header.Version == 0x2001A && er.Skeletons != null, $"{stem}: ER version and skeleton set");
        Log.Check(er.Meshes.Count == ds3.Meshes.Count && er.Materials.Count == ds3.Materials.Count, $"{stem}: mesh and material counts");

        double maxNormal = 0, maxUv = 0;
        bool positions = true;
        for (int i = 0; i < Math.Min(er.Meshes.Count, ds3.Meshes.Count); i++)
        {
            var a = ds3.Meshes[i];
            var b = er.Meshes[i];
            Log.Check(a.Vertices.Count == b.Vertices.Count, $"{stem} mesh {i}: vertex count");
            Log.Check(a.FaceSets.Count == b.FaceSets.Count && a.FaceSets.Zip(b.FaceSets).All(x => x.First.Indices.SequenceEqual(x.Second.Indices)), $"{stem} mesh {i}: face sets");
            for (int k = 0; k < Math.Min(a.Vertices.Count, b.Vertices.Count); k++)
            {
                positions &= a.Vertices[k].Position == b.Vertices[k].Position;
                maxNormal = Math.Max(maxNormal, (a.Vertices[k].Normal - b.Vertices[k].Normal).Length());
                if (uvFixes[i].OutOfRange || a.Vertices[k].UVs.Count == 0 || b.Vertices[k].UVs.Count == 0) continue;
                var expected = new Vector2(a.Vertices[k].UVs[0].X, a.Vertices[k].UVs[0].Y) / (uvFixes[i].FloatUv ? UvFloatScale : 1) + uvFixes[i].Shift;
                maxUv = Math.Max(maxUv, (expected - new Vector2(b.Vertices[k].UVs[0].X, b.Vertices[k].UVs[0].Y)).Length());
            }
        }
        Log.Check(positions, $"{stem}: positions unchanged");
        Log.Check(maxNormal < 0.03, $"{stem}: normals within byte precision ({maxNormal:0.####})");
        Log.Check(maxUv <= 1.0 / 2048, $"{stem}: UV0 within the Short2 step ({maxUv:0.######})");
    }
}
