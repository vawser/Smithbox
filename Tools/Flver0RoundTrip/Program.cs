using SoulsFormats;
using System.Text;

// Reads every Demon's Souls FLVER0, writes it back with FLVER0.Write and compares the bytes.
// Usage: Flver0RoundTrip <USRDIR> <report.md> [filter]
// Each file lands in one bucket: exact, differs (with the region of the first differing byte), or a write / re-read failure.

var root = args.Length > 0 ? args[0] : @"G:\Game Data Repository\Demon's Souls\PS3_GAME\USRDIR";
var reportPath = args.Length > 1 ? args[1] : "flver0_roundtrip.md";
var filter = args.Length > 2 ? args[2] : null;
var editorMode = filter == "editor";
if (editorMode) filter = null;

if (filter != null && filter.StartsWith("dump:"))
{
    // Per-mesh face figures next to the header's two face counts, for working out how the game counts them
    var want = filter[5..];
    foreach (var (name, bytes) in EnumerateFlvers(root))
    {
        if (!name.Contains(want, StringComparison.OrdinalIgnoreCase)) continue;
        var f = FLVER0.Read(bytes);
        var br = new BinaryReaderEx(bytes[6] == (byte)'B', bytes);
        Console.WriteLine($"{name} v0x{f.Header.Version:X} hdr40={br.GetInt32(0x40)} hdr44={br.GetInt32(0x44)} idxSize={f.Header.VertexIndexSize}");
        foreach (var m in f.Meshes)
        {
            int restarts = m.Indices.Count(i => i == 0xFFFF);
            Console.WriteLine($"  mat '{f.Materials[m.MaterialIndex].Name}' mtd '{f.Materials[m.MaterialIndex].MTD}' dyn={m.Dynamic} strip={m.TriangleStrip} idx={m.Indices.Count} restarts={restarts} " +
                $"faceNoDeg={m.GetFaceCount(f.Header.Version, false)} faceDeg={m.GetFaceCount(f.Header.Version, true)} tri={m.Triangulate(f.Header.Version, false, false).Count / 3}");
        }
    }
    return;
}

if (filter != null && filter.StartsWith("raw:"))
{
    // Raw offsets of the material and mesh tables, original next to rewritten
    var want = filter[4..];
    foreach (var (name, bytes) in EnumerateFlvers(root))
    {
        if (!name.Contains(want, StringComparison.OrdinalIgnoreCase)) continue;
        var f = FLVER0.Read(bytes);
        foreach (var (label, data) in new[] { ("orig", bytes), ("new", f.Write()) })
        {
            var br = new BinaryReaderEx(data[6] == (byte)'B', data);
            int o = 0x80 + f.Dummies.Count * 0x40;
            Console.WriteLine($"{name} [{label}] v0x{f.Header.Version:X} dataOffset=0x{br.GetInt32(0xC):X} len={data.Length}");
            for (int i = 0; i < f.Materials.Count; i++)
            {
                var m = o + i * 0x20;
                Console.WriteLine($"  mat{i}: name=0x{br.GetInt32(m):X} mtd=0x{br.GetInt32(m + 4):X} tex=0x{br.GetInt32(m + 8):X} layouts=0x{br.GetInt32(m + 12):X} len=0x{br.GetInt32(m + 16):X} layoutHdr=0x{br.GetInt32(m + 20):X} layoutCount={f.Materials[i].Layouts.Count}");
            }
            o += f.Materials.Count * 0x20 + f.Nodes.Count * 0x80;
            for (int i = 0; i < Math.Min(4, f.Meshes.Count); i++)
            {
                var m = o + i * 0x64 + 0x48;
                Console.WriteLine($"  mesh{i}: idxLen=0x{br.GetInt32(m):X} idxOff=0x{br.GetInt32(m + 4):X} bufLen=0x{br.GetInt32(m + 8):X} bufOff=0x{br.GetInt32(m + 12):X} vbl1=0x{br.GetInt32(m + 16):X} vbl2=0x{br.GetInt32(m + 20):X}");
            }
        }
    }
    return;
}

if (filter == "oldtables")
{
    // Per version: how many materials have no layout header and how many meshes no vertex buffer list
    var tally = new SortedDictionary<int, int[]>();
    foreach (var (name, bytes) in EnumerateFlvers(root))
    {
        FLVER0 f;
        try { f = FLVER0.Read(bytes); } catch { continue; }
        var br = new BinaryReaderEx(bytes[6] == (byte)'B', bytes);
        if (!tally.TryGetValue(f.Header.Version, out var t)) tally[f.Header.Version] = t = new int[5];
        t[0]++;
        int o = 0x80 + f.Dummies.Count * 0x40;
        for (int i = 0; i < f.Materials.Count; i++)
            if (br.GetInt32(o + i * 0x20 + 20) == 0) t[1]++; else t[2]++;
        o += f.Materials.Count * 0x20 + f.Nodes.Count * 0x80;
        for (int i = 0; i < f.Meshes.Count; i++)
            if (br.GetInt32(o + i * 0x64 + 0x58) == 0) t[3]++; else t[4]++;
    }
    foreach (var (v, t) in tally)
        Console.WriteLine($"v0x{v:X}: files={t[0]} materials no-header={t[1]} header={t[2]} meshes no-list={t[3]} list={t[4]}");
    return;
}

if (filter == "faces2")
{
    // Degenerate-triangle rules tested against the header's first face count
    var rules = new (string Name, Func<FLVER0.Mesh, int, int, int, bool> Skip)[]
    {
        ("any equal index", (m, a, b, c) => a == b || a == c || b == c),
        ("a==b or b==c", (m, a, b, c) => a == b || b == c),
        ("equal positions", (m, a, b, c) => { var p = m.Vertices; return p[a].Position == p[b].Position || p[a].Position == p[c].Position || p[b].Position == p[c].Position; }),
        ("equal index or zero area", (m, a, b, c) => a == b || a == c || b == c || System.Numerics.Vector3.Cross(m.Vertices[b].Position - m.Vertices[a].Position, m.Vertices[c].Position - m.Vertices[a].Position) == System.Numerics.Vector3.Zero),
        ("equal index, restart only 0xFFFF in strips", (m, a, b, c) => a == b || a == c || b == c),
    };
    var missCount = new int[rules.Length];
    var samples = new List<string>[rules.Length];
    for (int r = 0; r < rules.Length; r++) samples[r] = new();
    foreach (var (name, bytes) in EnumerateFlvers(root))
    {
        FLVER0 f;
        try { f = FLVER0.Read(bytes); } catch { continue; }
        int h40 = new BinaryReaderEx(bytes[6] == (byte)'B', bytes).GetInt32(0x40);
        for (int r = 0; r < rules.Length; r++)
        {
            int n = 0;
            foreach (var m in f.Meshes)
                for (int i = 0; i < m.Indices.Count - 2; i++)
                {
                    int a = m.Indices[i], b = m.Indices[i + 1], c = m.Indices[i + 2];
                    if (a == 0xFFFF || b == 0xFFFF || c == 0xFFFF) continue;
                    if (!rules[r].Skip(m, a, b, c)) n++;
                }
            if (n != h40) { missCount[r]++; if (samples[r].Count < 4) samples[r].Add($"{name}: h40={h40} got={n}"); }
        }
    }
    for (int r = 0; r < rules.Length; r++)
    {
        Console.WriteLine($"{rules[r].Name}: {missCount[r]} misses");
        foreach (var s in samples[r]) Console.WriteLine($"   {s}");
    }
    return;
}

if (filter == "faces")
{
    // Tests candidate formulas for the header's two face counts against every file
    var miss = new Dictionary<string, List<string>>();
    int total = 0;
    void Miss(string k, string s) { if (!miss.TryGetValue(k, out var l)) miss[k] = l = new(); l.Add(s); }
    foreach (var (name, bytes) in EnumerateFlvers(root))
    {
        FLVER0 f;
        try { f = FLVER0.Read(bytes); } catch { continue; }
        total++;
        var br = new BinaryReaderEx(bytes[6] == (byte)'B', bytes);
        int h40 = br.GetInt32(0x40), h44 = br.GetInt32(0x44);
        int v = f.Header.Version;
        int noDeg = 0, deg = 0, idxM2 = 0, listTri = 0;
        foreach (var m in f.Meshes)
        {
            noDeg += StripFaces(m, false);
            deg += StripFaces(m, true);
            idxM2 += Math.Max(0, m.Indices.Count - 2);
            listTri += m.Indices.Count / 3;
        }
        bool list15 = v >= 0x15 && f.Meshes.Any(m => !m.TriangleStrip);
        string kind = $"v{(v >= 0x15 ? "15+" : "<15")}{(list15 ? " lists" : "")}";
        if (h40 != noDeg) Miss($"{kind} h40 != strip faces w/o degenerates", $"{name}: h40={h40} noDeg={noDeg} deg={deg} listTri={listTri}");
        if (h44 != idxM2) Miss($"{kind} h44 != sum(idx-2)", $"{name}: h44={h44} idxM2={idxM2} deg={deg} noDeg={noDeg}");
        if (h44 != deg) Miss($"{kind} h44 != strip faces with degenerates", $"{name}: h44={h44} idxM2={idxM2} deg={deg} noDeg={noDeg}");
        if (h44 != noDeg) Miss($"{kind} h44 != strip faces w/o degenerates", $"{name}: h44={h44} idxM2={idxM2} deg={deg} noDeg={noDeg}");
    }
    Console.WriteLine($"{total} files");
    foreach (var (k, l) in miss.OrderBy(k => k.Key))
    {
        Console.WriteLine($"{k}: {l.Count}");
        foreach (var s in l.Take(4)) Console.WriteLine($"   {s}");
    }
    return;
}

// Strip faces: triangles of three indices with no restart, optionally keeping degenerate ones; a triangle list counts Count / 3
static int StripFaces(FLVER0.Mesh m, bool includeDegenerate)
{
    if (!m.TriangleStrip && m.Indices.Count % 3 == 0 && !m.Indices.Contains(0xFFFF) && false)
        return m.Indices.Count / 3;
    int c = 0;
    for (int i = 0; i < m.Indices.Count - 2; i++)
    {
        int a = m.Indices[i], b = m.Indices[i + 1], d = m.Indices[i + 2];
        if (a == 0xFFFF || b == 0xFFFF || d == 0xFFFF) continue;
        if (includeDegenerate || (a != b && a != d && b != d)) c++;
    }
    return c;
}

var buckets = new SortedDictionary<string, List<string>>();
void Add(string bucket, string sample)
{
    if (!buckets.TryGetValue(bucket, out var list))
        buckets[bucket] = list = new List<string>();
    list.Add(sample);
}

foreach (var (name, bytes) in EnumerateFlvers(root))
{
    if (filter != null && !name.Contains(filter, StringComparison.OrdinalIgnoreCase))
        continue;

    FLVER0 flver;
    try { flver = FLVER0.Read(bytes); }
    catch (Exception e) { Add("read failed (original)", $"{name}: {e.Message}"); continue; }

    byte[] written;
    try
    {
        // "editor" mode: through the Model Editor's FLVER2 and back, as a DES save does
        written = editorMode
            ? StudioCore.Editors.ModelEditor.Flver0Converter.ToFlver0(StudioCore.Editors.ModelEditor.Flver0Converter.ToFlver2(flver)).Write()
            : flver.Write();
    }
    catch (Exception e) { Add($"write failed: {e.GetType().Name}", $"{name}: {e.Message}"); continue; }

    FLVER0 reread;
    try { reread = FLVER0.Read(written); }
    catch (Exception e) { Add("re-read failed", $"{name}: {e.Message}"); continue; }

    var semantic = SemanticDiff(flver, reread);
    if (semantic != null)
        Add($"semantic: {semantic.Split(':')[0]}", $"{name}: {semantic}");

    if (written.Length == bytes.Length && written.AsSpan().SequenceEqual(bytes))
    {
        Add("exact", name);
        continue;
    }

    int first = FirstDiff(bytes, written);
    if (written.Length != bytes.Length)
    {
        Add($"size differs: first diff {Region(flver, bytes, first)}", $"{name}: v0x{flver.Header.Version:X} first diff 0x{first:X} (orig {bytes.Length}, new {written.Length}) orig[{Hex(bytes, first)}] new[{Hex(written, first)}]");
        continue;
    }

    // Same size: every region with a differing byte
    var regions = new SortedSet<string>();
    var firstPer = new Dictionary<string, int>();
    for (int i = first; i < bytes.Length; i++)
    {
        if (bytes[i] == written[i]) continue;
        var r = Region(flver, bytes, i);
        if (regions.Add(r)) firstPer[r] = i;
    }
    foreach (var r in regions)
        Add($"differs at: {r}", $"{name}: v0x{flver.Header.Version:X} at 0x{firstPer[r]:X} orig[{Hex(bytes, firstPer[r])}] new[{Hex(written, firstPer[r])}]");
    Add($"same size, differs only in: {string.Join(", ", regions)}", name);
}

var sb = new StringBuilder();
sb.AppendLine("# FLVER0 roundtrip (Demon's Souls)");
sb.AppendLine();
sb.AppendLine("| Bucket | Files |");
sb.AppendLine("|---|---|");
foreach (var (k, v) in buckets)
    sb.AppendLine($"| {k} | {v.Count} |");
sb.AppendLine();
foreach (var (k, v) in buckets)
{
    if (k == "exact") continue;
    sb.AppendLine($"## {k}");
    foreach (var s in v.Take(15))
        sb.AppendLine($"- {s}");
    sb.AppendLine();
}
File.WriteAllText(reportPath, sb.ToString());
Console.WriteLine(sb.ToString());

static int FirstDiff(byte[] a, byte[] b)
{
    int n = Math.Min(a.Length, b.Length);
    for (int i = 0; i < n; i++)
        if (a[i] != b[i]) return i;
    return n;
}

static string Hex(byte[] a, int at)
{
    int start = Math.Max(0, at - (at % 4));
    return Convert.ToHexString(a, start, Math.Max(0, Math.Min(16, a.Length - start)));
}

// Names the struct the original file has at an offset, from the fixed table sizes and the header's data offset
static string Region(FLVER0 f, byte[] orig, int offset)
{
    var br = new BinaryReaderEx(orig[6] == (byte)'B', orig);
    int dataOffset = br.GetInt32(0xC);
    int o = 0x80;
    if (offset < o) return $"header +0x{offset & ~3:X}";
    int next = o + f.Dummies.Count * 0x40;
    if (offset < next) return $"dummy +0x{(offset - o) % 0x40:X}";
    o = next; next = o + f.Materials.Count * 0x20;
    if (offset < next) return $"material +0x{(offset - o) % 0x20:X}";
    o = next; next = o + f.Nodes.Count * 0x80;
    if (offset < next) return $"node +0x{(offset - o) % 0x80:X}";
    o = next; next = o + f.Meshes.Count * 0x64;
    if (offset < next) return $"mesh header +0x{((offset - o) % 0x64) & ~3:X}";
    if (offset < dataOffset) return "substructs (strings, textures, layouts, buffer lists)";
    return "data (indices / vertices)";
}

static string SemanticDiff(FLVER0 a, FLVER0 b)
{
    if (a.Header.Version != b.Header.Version) return "header: version";
    if (a.Header.VertexIndexSize != b.Header.VertexIndexSize) return "header: vertex index size";
    if (a.Header.Unk4A != b.Header.Unk4A || a.Header.Unk4B != b.Header.Unk4B || a.Header.Unk4C != b.Header.Unk4C || a.Header.Unk5C != b.Header.Unk5C) return "header: unk";
    if (a.Dummies.Count != b.Dummies.Count || a.Nodes.Count != b.Nodes.Count || a.Materials.Count != b.Materials.Count || a.Meshes.Count != b.Meshes.Count) return "counts";
    for (int i = 0; i < a.Nodes.Count; i++)
        if (a.Nodes[i].Name != b.Nodes[i].Name || a.Nodes[i].Translation != b.Nodes[i].Translation) return $"node: {i}";
    for (int i = 0; i < a.Materials.Count; i++)
    {
        var ma = a.Materials[i]; var mb = b.Materials[i];
        if (ma.Name != mb.Name || ma.MTD != mb.MTD) return $"material: name/mtd {i}";
        if (ma.Textures.Count != mb.Textures.Count) return $"material: texture count {i}";
        for (int t = 0; t < ma.Textures.Count; t++)
            if (ma.Textures[t].Path != mb.Textures[t].Path || ma.Textures[t].ParamName != mb.Textures[t].ParamName) return $"material: texture {i}/{t}";
        if (ma.Layouts.Count != mb.Layouts.Count) return $"material: layout count {i}";
    }
    for (int i = 0; i < a.Meshes.Count; i++)
    {
        var ma = a.Meshes[i]; var mb = b.Meshes[i];
        if (ma.Dynamic != mb.Dynamic || ma.MaterialIndex != mb.MaterialIndex || ma.CullBackfaces != mb.CullBackfaces || ma.TriangleStrip != mb.TriangleStrip || ma.NodeIndex != mb.NodeIndex || ma.LayoutIndex != mb.LayoutIndex) return $"mesh: flags {i}";
        if (!ma.BoneIndices.SequenceEqual(mb.BoneIndices)) return $"mesh: bone indices {i}";
        if (!ma.Indices.SequenceEqual(mb.Indices)) return $"mesh: indices {i}";
        if (ma.Vertices.Count != mb.Vertices.Count) return $"mesh: vertex count {i}";
        for (int v = 0; v < ma.Vertices.Count; v++)
        {
            var va = ma.Vertices[v]; var vb = mb.Vertices[v];
            if (va.Position != vb.Position) return $"vertex: position {i}/{v}";
            if (va.Normal != vb.Normal || va.NormalW != vb.NormalW) return $"vertex: normal {i}/{v}";
            if (va.UVs.Count != vb.UVs.Count || !va.UVs.SequenceEqual(vb.UVs)) return $"vertex: uv {i}/{v}";
            if (va.Tangents.Count != vb.Tangents.Count || !va.Tangents.SequenceEqual(vb.Tangents)) return $"vertex: tangent {i}/{v}";
        }
    }
    return null;
}

static IEnumerable<(string Name, byte[] Bytes)> EnumerateFlvers(string root)
{
    foreach (var dir in Directory.GetDirectories(Path.Combine(root, "chr")))
    {
        var id = Path.GetFileName(dir);
        var bndPath = Path.Combine(dir, $"{id}.chrbnd");
        if (!File.Exists(bndPath))
        {
            var loose = Path.Combine(dir, $"{id}.flver");
            if (File.Exists(loose))
                yield return ($"chr/{id}.flver", File.ReadAllBytes(loose));
            continue;
        }
        foreach (var f in BND3.Read(bndPath).Files.Where(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)))
            yield return ($"chr/{id}/{Path.GetFileName(f.Name)}", f.Bytes.ToArray());
    }

    foreach (var (folder, ext) in new[] { ("obj", ".objbnd"), ("parts", ".partsbnd") })
    {
        foreach (var bndPath in Directory.GetFiles(Path.Combine(root, folder), "*" + ext))
        {
            BND3 bnd;
            try { bnd = BND3.Read(bndPath); } catch { continue; }
            foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)))
                yield return ($"{folder}/{Path.GetFileNameWithoutExtension(bndPath)}/{Path.GetFileName(f.Name)}", f.Bytes.ToArray());
        }
    }

    foreach (var dir in Directory.GetDirectories(Path.Combine(root, "map"), "m??_??_??_??"))
        foreach (var flver in Directory.GetFiles(dir, "*.flver"))
            yield return ($"map/{Path.GetFileName(dir)}/{Path.GetFileName(flver)}", File.ReadAllBytes(flver));
}
