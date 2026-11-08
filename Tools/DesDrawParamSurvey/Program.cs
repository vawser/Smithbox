using SoulsFormats;
using System.Text;
using Param = Andre.Formats.Param;

// Surveys the Demon's Souls DrawParam banks (param/drawparam/aAA_drawparam.parambnd, mAA_<Bank>.param) against the
// byte IDs that map parts use to pick their rows (MSBD Part LightID, FogID, ScatterID, LensFlareID, ShadowID, DofID,
// ToneMapID, ToneCorrectID) and the Light events (PointLightID). Feeds the DES DrawParam lighting plan.
// Usage: DesDrawParamSurvey <USRDIR> <DES paramdef folder> <report.md>

var root = args.Length > 0 ? args[0] : @"G:\Game Data Repository\Demon's Souls\PS3_GAME\USRDIR";
var defFolder = args.Length > 1 ? args[1] : @"..\..\src\Smithbox.Data\Assets\PARAM\DES\Defs";
var reportPath = args.Length > 2 ? args[2] : "des_drawparam_survey.md";

if (args.Length > 3 && args[3] == "lights")
{
    // Does a Light event's PointLightID match a dummy of the FLVER of the map piece it names?
    int matched = 0, unmatched = 0, noModel = 0;
    var notes = new List<string>();
    foreach (var msbPath in Directory.GetFiles(Path.Combine(root, "map", "MapStudio"), "m0*.msb"))
    {
        var mapId = Path.GetFileNameWithoutExtension(msbPath);
        var msb = MSBD.Read(msbPath);
        foreach (var ev in msb.Events.Lights)
        {
            var piece = msb.Parts.MapPieces.FirstOrDefault(p => p.Name == ev.PartName);
            if (piece == null) { noModel++; continue; }
            var flverPath = Path.Combine(root, "map", mapId, piece.ModelName + "A" + mapId.Substring(1, 2) + ".flver");
            if (!File.Exists(flverPath))
                flverPath = Directory.GetFiles(Path.Combine(root, "map", mapId), piece.ModelName + "*.flver").FirstOrDefault();
            if (flverPath == null) { noModel++; continue; }
            var flver = FLVER0.Read(flverPath);
            var refs = flver.Dummies.Select(d => (int)d.ReferenceID).ToHashSet();
            if (refs.Contains(ev.PointLightID)) matched++;
            else { unmatched++; if (notes.Count < 8) notes.Add($"{mapId} {ev.Name} id {ev.PointLightID} part {piece.Name} model {Path.GetFileName(flverPath)} dummy refs {string.Join(",", refs.OrderBy(r => r).Take(12))}"); }
        }
    }
    Console.WriteLine($"Light events on map pieces: {matched} match a dummy ReferenceID, {unmatched} don't; {noModel} not on a map piece or model not found");
    foreach (var n in notes) Console.WriteLine("  " + n);
    return;
}

var defs = Directory.GetFiles(defFolder, "*.xml").Select(f => PARAMDEF.XmlDeserialize(f)).ToList();

if (args.Length > 3 && args[3] == "envexport")
{
    // Phase 5: decode the env cubemaps (DXT1 = TPF format 0, A8R8G8B8 = format 10) and write each as a strip of its
    // six faces (+X -X +Y -Y +Z -Z, DDS order) to a PNG, with each face's average colour, to see what the four
    // EnvSpc maps of a set are and how bright the textures are against the LightBank scales.
    var outDir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(reportPath))!, "envtex");
    Directory.CreateDirectory(outDir);
    var o = new StringBuilder();
    o.AppendLine("# DES env cubemap export");
    o.AppendLine();
    o.AppendLine("Average colour per face, stored values 0-255 (+X -X +Y -Y +Z -Z), and the whole cube's average.");
    o.AppendLine();
    var tpfs = new List<string> { Path.Combine(root, "other", "envlight.tpf") };
    tpfs.AddRange(Directory.GetDirectories(Path.Combine(root, "map"), "m??").Select(d => Path.Combine(d, Path.GetFileName(d) + "_9999.tpf")).Where(File.Exists));
    foreach (var tpfPath in tpfs)
    {
        var tpf = TPF.Read(tpfPath);
        o.AppendLine($"## {Path.GetRelativePath(root, tpfPath)}");
        o.AppendLine();
        o.AppendLine("| Texture | Size | Mips | +X | -X | +Y | -Y | +Z | -Z | Cube avg | Max |");
        o.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|");
        foreach (var t in tpf.Textures.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase))
        {
            byte[] dds = t.Headerize();
            int h = BitConverter.ToInt32(dds, 12), w = BitConverter.ToInt32(dds, 16), mips = Math.Max(1, BitConverter.ToInt32(dds, 28));
            int pfFlags = BitConverter.ToInt32(dds, 80), fourCC = BitConverter.ToInt32(dds, 84), bits = BitConverter.ToInt32(dds, 88);
            uint rMask = BitConverter.ToUInt32(dds, 92), gMask = BitConverter.ToUInt32(dds, 96), bMask = BitConverter.ToUInt32(dds, 100);
            bool dxt1 = fourCC == 0x31545844;
            // PS3 format 10: the Headerizer's DDS says 24-bit RGB, but the deswizzled data is 4 bytes a pixel, A R G B
            bool argbBytes = !dxt1 && t.Format == 10 && (dds.Length - 128) >= w * h * 4 * 6;
            if (argbBytes) bits = 32;
            int offset = 128;
            int MipBytes(int mw, int mh) => dxt1 ? Math.Max(1, (mw + 3) / 4) * Math.Max(1, (mh + 3) / 4) * 8 : mw * mh * (bits / 8);
            int faceBytes = 0;
            for (int m = 0; m < mips; m++) faceBytes += MipBytes(Math.Max(1, w >> m), Math.Max(1, h >> m));

            using var img = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(w * 6, h);
            var faceAvg = new string[6];
            double tr = 0, tg = 0, tb = 0; int maxV = 0;
            for (int face = 0; face < 6; face++)
            {
                int start = offset + face * faceBytes;
                if (start + MipBytes(w, h) > dds.Length) { faceAvg[face] = "—"; continue; }
                var px = new (byte R, byte G, byte B)[w * h];
                if (dxt1)
                {
                    for (int by = 0; by < (h + 3) / 4; by++)
                        for (int bx = 0; bx < (w + 3) / 4; bx++)
                        {
                            int b = start + (by * ((w + 3) / 4) + bx) * 8;
                            ushort c0 = BitConverter.ToUInt16(dds, b), c1 = BitConverter.ToUInt16(dds, b + 2);
                            uint idx = BitConverter.ToUInt32(dds, b + 4);
                            (int R, int G, int B) U(ushort c) => (((c >> 11) & 31) * 255 / 31, ((c >> 5) & 63) * 255 / 63, (c & 31) * 255 / 31);
                            var p0 = U(c0); var p1 = U(c1);
                            (int R, int G, int B)[] pal = c0 > c1
                                ? new (int R, int G, int B)[] { p0, p1, ((2 * p0.R + p1.R) / 3, (2 * p0.G + p1.G) / 3, (2 * p0.B + p1.B) / 3), ((p0.R + 2 * p1.R) / 3, (p0.G + 2 * p1.G) / 3, (p0.B + 2 * p1.B) / 3) }
                                : new (int R, int G, int B)[] { p0, p1, ((p0.R + p1.R) / 2, (p0.G + p1.G) / 2, (p0.B + p1.B) / 2), (0, 0, 0) };
                            for (int i = 0; i < 16; i++)
                            {
                                int x = bx * 4 + i % 4, y = by * 4 + i / 4;
                                if (x >= w || y >= h) continue;
                                var c = pal[(idx >> (2 * i)) & 3];
                                px[y * w + x] = ((byte)c.R, (byte)c.G, (byte)c.B);
                            }
                        }
                }
                else
                {
                    int bpp = bits / 8;
                    int Shift(uint mask) => mask == 0 ? 0 : System.Numerics.BitOperations.TrailingZeroCount(mask);
                    for (int i = 0; i < w * h; i++)
                    {
                        if (argbBytes) { px[i] = (dds[start + i * 4 + 1], dds[start + i * 4 + 2], dds[start + i * 4 + 3]); continue; }
                        uint v = bpp == 4 ? BitConverter.ToUInt32(dds, start + i * 4) : (uint)(dds[start + i * bpp] | dds[start + i * bpp + 1] << 8 | dds[start + i * bpp + 2] << 16);
                        px[i] = ((byte)((v & rMask) >> Shift(rMask)), (byte)((v & gMask) >> Shift(gMask)), (byte)((v & bMask) >> Shift(bMask)));
                    }
                }
                double r = 0, g = 0, bl = 0;
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        var c = px[y * w + x];
                        img[face * w + x, y] = new SixLabors.ImageSharp.PixelFormats.Rgba32(c.R, c.G, c.B, 255);
                        r += c.R; g += c.G; bl += c.B; maxV = Math.Max(maxV, Math.Max(c.R, Math.Max(c.G, c.B)));
                    }
                r /= w * h; g /= w * h; bl /= w * h;
                tr += r / 6; tg += g / 6; tb += bl / 6;
                faceAvg[face] = $"{r:0},{g:0},{bl:0}";
            }
            var pngName = $"{Path.GetFileNameWithoutExtension(tpfPath)}_{t.Name}.png";
            SixLabors.ImageSharp.ImageExtensions.SaveAsPng(img, Path.Combine(outDir, pngName));
            o.AppendLine($"| {t.Name} | {w}x{h} | {mips} | {string.Join(" | ", faceAvg)} | {tr:0},{tg:0},{tb:0} | {maxV} |");
        }
        o.AppendLine();
    }
    File.WriteAllText(reportPath, o.ToString());
    Console.WriteLine($"Wrote {reportPath} and {outDir}");
    return;
}

if (args.Length > 3 && args[3] == "envtex")
{
    // Phase 5: what are the LightBank envDif/envSpc IDs? List other/envlight.tpf, the loose mAA_envlighttexbank.param
    // files and every LightBank row's env IDs, and look for env-looking textures in the map TPFs.
    var o = new StringBuilder();
    o.AppendLine("# DES env light textures survey");
    o.AppendLine();
    void ListTpf(string path)
    {
        var tpf = TPF.Read(path);
        o.AppendLine($"### {Path.GetRelativePath(root, path)} ({tpf.Textures.Count} textures, platform {tpf.Platform})");
        o.AppendLine();
        o.AppendLine("| Name | Format | Type | Mips | Size | Bytes | Flags1 |");
        o.AppendLine("|---|---|---|---|---|---|---|");
        foreach (var t in tpf.Textures)
            o.AppendLine($"| {t.Name} | {t.Format} | {t.Type} | {t.Mipmaps} | {(t.Header == null ? "—" : $"{t.Header.Width}x{t.Header.Height}")} | {t.Bytes.Length} | {t.Flags1} |");
        o.AppendLine();
    }
    ListTpf(Path.Combine(root, "other", "envlight.tpf"));

    // Env-looking textures in the map folders' TPFs
    o.AppendLine("## Env-looking textures in map TPFs");
    o.AppendLine();
    foreach (var tpfPath in Directory.GetFiles(Path.Combine(root, "map"), "*.tpf*", SearchOption.AllDirectories))
    {
        try
        {
            var bytes = tpfPath.EndsWith(".dcx") ? DCX.Decompress(tpfPath).ToArray() : File.ReadAllBytes(tpfPath);
            var tpf = TPF.Read(bytes);
            var hits = tpf.Textures.Where(t => t.Name.Contains("env", StringComparison.OrdinalIgnoreCase) || t.Type == TPF.TexType.Cubemap).ToList();
            if (hits.Count > 0)
                o.AppendLine($"- {Path.GetRelativePath(root, tpfPath)}: " + string.Join(", ", hits.Take(12).Select(t => $"{t.Name} ({t.Type}, fmt {t.Format}, {(t.Header == null ? "?" : $"{t.Header.Width}x{t.Header.Height}")})")) + (hits.Count > 12 ? $" … {hits.Count - 12} more" : ""));
        }
        catch (Exception e) { o.AppendLine($"- {Path.GetRelativePath(root, tpfPath)}: read failed ({e.GetType().Name})"); }
    }
    o.AppendLine();

    // The loose EnvLightTexBank params
    var envDef = defs.First(d => d.ParamType.Contains("ENV_LIGHT", StringComparison.OrdinalIgnoreCase));
    o.AppendLine($"## Loose EnvLightTexBank params (paramdef {envDef.ParamType})");
    o.AppendLine();
    foreach (var pPath in Directory.GetFiles(Path.Combine(root, "param", "drawparam"), "*_envlighttexbank.param"))
    {
        var p = Param.Read(File.ReadAllBytes(pPath));
        o.AppendLine($"### {Path.GetFileName(pPath)} ({p.ParamType}, {p.Rows.Count} rows)");
        o.AppendLine();
        if (p.ParamType != envDef.ParamType) { o.AppendLine($"- param type differs from the def; skipped"); o.AppendLine(); continue; }
        p.ApplyParamdef(envDef);
        foreach (var r in p.Rows)
        {
            var vals = r.Cells.Where(c => c.Def.DisplayType != PARAMDEF.DefType.dummy8 && !(c.Def.InternalName.StartsWith("col") || c.Def.InternalName.StartsWith("degRot")) || (c.Def.InternalName.StartsWith("colA") && Convert.ToInt32(c.Value) != 0))
                .Select(c => $"{c.Def.InternalName}={c.Value}");
            o.AppendLine($"- **{r.ID}** '{r.Name}': {string.Join(", ", vals)}");
        }
        o.AppendLine();
    }

    // The bnd EnvLightTexBank (inside aAA_drawparam.parambnd) for comparison, and every LightBank row's env IDs
    o.AppendLine("## LightBank env IDs per area (envDif / envSpc_0-3 → rows)");
    o.AppendLine();
    foreach (var bndPath in Directory.GetFiles(Path.Combine(root, "param", "drawparam"), "a??_drawparam.parambnd"))
    {
        var area = Path.GetFileName(bndPath).Substring(1, 2);
        foreach (var f in BND3.Read(bndPath).Files)
        {
            var name = Path.GetFileNameWithoutExtension(f.Name);
            var p = Param.Read(f.Bytes);
            var def = defs.FirstOrDefault(d => d.ParamType == p.ParamType);
            if (def == null) continue;
            p.ApplyParamdef(def);
            if (name.EndsWith("EnvLightTexBank"))
            {
                o.AppendLine($"- a{area} bnd {name}: rows " + string.Join("; ", p.Rows.Select(r => $"{r.ID} isUse={r["isUse"].Value.Value} dif={r["resNameId_Dif0"].Value.Value} spc={r["resNameId_Spc0"].Value.Value},{r["resNameId_Spc1"].Value.Value},{r["resNameId_Spc2"].Value.Value},{r["resNameId_Spc3"].Value.Value}")));
            }
            if (name.EndsWith("_LightBank"))
            {
                var groups = p.Rows.GroupBy(r => $"{r["envDif"].Value.Value}/{r["envSpc_0"].Value.Value},{r["envSpc_1"].Value.Value},{r["envSpc_2"].Value.Value},{r["envSpc_3"].Value.Value}")
                    .Select(g => $"{g.Key} ×{g.Count()}");
                o.AppendLine($"- a{area} {name} ({f.Name}): " + string.Join("; ", groups));

                // Does each ID resolve to EnvDif_mAA_NNN / EnvSpc_mAA_NNN in map/mAA/mAA_9999.tpf (names compared case-insensitively)?
                var tpfPath = Path.Combine(root, "map", "m" + area, "m" + area + "_9999.tpf");
                var names = File.Exists(tpfPath) ? TPF.Read(tpfPath).Textures.Select(t => t.Name.ToLowerInvariant()).ToHashSet() : new HashSet<string>();
                var missing = new SortedSet<string>();
                int ok = 0;
                foreach (var r in p.Rows)
                {
                    foreach (var (field, prefix) in new[] { ("envDif", "envdif"), ("envSpc_0", "envspc"), ("envSpc_1", "envspc"), ("envSpc_2", "envspc"), ("envSpc_3", "envspc") })
                    {
                        var id = Convert.ToInt32(r[field].Value.Value);
                        var tex = $"{prefix}_m{area}_{id:000}";
                        if (names.Contains(tex)) ok++; else missing.Add($"{tex} (row {r.ID})");
                    }
                }
                o.AppendLine($"  - resolve in m{area}_9999.tpf: {ok} found, {missing.Count} missing" + (missing.Count > 0 ? ": " + string.Join(", ", missing.Take(16)) : ""));
                o.AppendLine($"  - textures in m{area}_9999.tpf: " + string.Join(", ", names.OrderBy(n => n)));
            }
        }
    }
    o.AppendLine();
    File.WriteAllText(reportPath, o.ToString());
    Console.WriteLine($"Wrote {reportPath}");
    return;
}
var sb = new StringBuilder();
sb.AppendLine("# DES DrawParam survey");
sb.AppendLine();
sb.AppendLine($"Generated by `Tools/DesDrawParamSurvey` on {DateTime.Now:yyyy-MM-dd HH:mm}.");
sb.AppendLine();

// --- Banks: area -> bank -> param
var areas = new SortedDictionary<string, Dictionary<string, Param>>();
foreach (var bndPath in Directory.GetFiles(Path.Combine(root, "param", "drawparam"), "a??_drawparam.parambnd"))
{
    var area = Path.GetFileName(bndPath).Substring(1, 2);
    var banks = new Dictionary<string, Param>(StringComparer.OrdinalIgnoreCase);
    foreach (var f in BND3.Read(bndPath).Files)
    {
        var name = Path.GetFileNameWithoutExtension(f.Name); // mAA_LightBank
        // Only the mAA_ banks, the ones Smithbox lights with: the parambnd also holds sAA_ banks of the same name
        if (!name.StartsWith("m", StringComparison.OrdinalIgnoreCase)) continue;
        var bank = name.Substring(name.IndexOf('_') + 1);
        var p = Param.Read(f.Bytes);
        var def = defs.FirstOrDefault(d => d.ParamType == p.ParamType);
        if (def == null) { sb.AppendLine($"- no paramdef for {name} ({p.ParamType})"); continue; }
        p.ApplyParamdef(def);
        banks[bank] = p;
    }
    areas[area] = banks;
}

string[] bankOrder = ["LightBank", "FogBank", "LightScatteringBank", "LensFlareBank", "ShadowBank", "DofBank", "ToneMapBank", "ToneCorrectBank", "PointLightBank", "EnvLightTexBank", "LensFlareExBank"];

sb.AppendLine("## Banks per area (row count, row ID range)");
sb.AppendLine();
sb.AppendLine("| Area | " + string.Join(" | ", bankOrder) + " |");
sb.AppendLine("|---|" + string.Concat(bankOrder.Select(_ => "---|")));
foreach (var (area, banks) in areas)
    sb.AppendLine($"| a{area} | " + string.Join(" | ", bankOrder.Select(b => banks.TryGetValue(b, out var p) ? (p.Rows.Count == 0 ? "0" : $"{p.Rows.Count} ({p.Rows.Min(r => r.ID)}-{p.Rows.Max(r => r.ID)})") : "—")) + " |");
sb.AppendLine();

// --- Part IDs per map
(string Bank, Func<MSBD.Part, byte> Get)[] partIds =
[
    ("LightBank", p => p.LightID), ("FogBank", p => p.FogID), ("LightScatteringBank", p => p.ScatterID),
    ("LensFlareBank", p => p.LensFlareID), ("ShadowBank", p => p.ShadowID), ("DofBank", p => p.DofID),
    ("ToneMapBank", p => p.ToneMapID), ("ToneCorrectBank", p => p.ToneCorrectID)
];

sb.AppendLine("## IDs used by map parts");
sb.AppendLine();
sb.AppendLine("Per map: for each bank, the distinct IDs the parts use (count), by part kind. `!` marks an ID with no row in the area's bank.");
sb.AppendLine();

var kindAgreement = new SortedDictionary<string, int>();
var lightEvents = new List<string>();
foreach (var msbPath in Directory.GetFiles(Path.Combine(root, "map", "MapStudio"), "*.msb"))
{
    var mapId = Path.GetFileNameWithoutExtension(msbPath);
    var area = mapId.Substring(1, 2);
    MSBD msb;
    try { msb = MSBD.Read(msbPath); } catch (Exception e) { sb.AppendLine($"- {mapId}: read failed {e.Message}"); continue; }
    areas.TryGetValue(area, out var banks);

    var kinds = new (string Kind, IEnumerable<MSBD.Part> Parts)[]
    {
        ("map pieces", msb.Parts.MapPieces), ("collisions", msb.Parts.Collisions), ("objects", msb.Parts.Objects),
        ("enemies", msb.Parts.Enemies), ("players", msb.Parts.Players)
    };

    sb.AppendLine($"### {mapId} (area a{area}{(banks == null ? ", **no drawparam**" : "")})");
    sb.AppendLine();
    sb.AppendLine("| Bank | " + string.Join(" | ", kinds.Select(k => $"{k.Kind} ({k.Parts.Count()})")) + " |");
    sb.AppendLine("|---|" + string.Concat(kinds.Select(_ => "---|")));
    foreach (var (bank, get) in partIds)
    {
        var cells = kinds.Select(k =>
        {
            var used = k.Parts.GroupBy(get).OrderByDescending(g => g.Count()).Select(g =>
            {
                bool missing = banks == null || !banks.TryGetValue(bank, out var p) || p.Rows.All(r => r.ID != g.Key);
                return $"{g.Key}{(missing ? "!" : "")} ({g.Count()})";
            });
            return string.Join(", ", used.Take(6)) + (k.Parts.GroupBy(get).Count() > 6 ? " …" : "");
        });
        sb.AppendLine($"| {bank} | " + string.Join(" | ", cells) + " |");

        // Do map pieces and collisions agree on the most-used ID?
        var mp = msb.Parts.MapPieces.GroupBy(get).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key;
        var col = msb.Parts.Collisions.GroupBy(get).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key;
        if (mp != null && col != null)
        {
            var key = $"{bank}: most-used ID of map pieces {(mp == col ? "==" : "!=")} collisions";
            kindAgreement[key] = kindAgreement.GetValueOrDefault(key) + 1;
        }
    }
    sb.AppendLine();

    // Light events: a point light at a region
    foreach (var ev in msb.Events.Lights)
    {
        var region = msb.Regions.GetEntries().FirstOrDefault(r => r.Name == ev.RegionName);
        bool hasRow = banks != null && banks.TryGetValue("PointLightBank", out var pl) && pl.Rows.Any(r => r.ID == ev.PointLightID);
        lightEvents.Add($"| {mapId} | {ev.Name} | {ev.PointLightID}{(hasRow ? "" : "!")} | {ev.PointLightBankID} | {(region == null ? "—" : $"{region.Name} ({region.Position.X:F1}, {region.Position.Y:F1}, {region.Position.Z:F1})")} | {ev.PartName ?? "—"} |");
    }
}

sb.AppendLine("## Map pieces against collisions");
sb.AppendLine();
foreach (var (k, v) in kindAgreement) sb.AppendLine($"- {k}: {v} maps");
sb.AppendLine();

sb.AppendLine($"## Light events ({lightEvents.Count})");
sb.AppendLine();
sb.AppendLine("| Map | Event | PointLightID | PointLightBankID | Region | Part |");
sb.AppendLine("|---|---|---|---|---|---|");
foreach (var l in lightEvents.Take(60)) sb.AppendLine(l);
if (lightEvents.Count > 60) sb.AppendLine($"| … {lightEvents.Count - 60} more | | | | | |");
sb.AppendLine();

// --- Value ranges per field, every area's rows
sb.AppendLine("## Field value ranges (all areas)");
sb.AppendLine();
foreach (var bank in new[] { "LightBank", "ToneMapBank", "FogBank", "ShadowBank", "ToneCorrectBank", "LightScatteringBank", "PointLightBank" })
{
    var rows = areas.Values.Where(b => b.ContainsKey(bank)).SelectMany(b => b[bank].Rows).ToList();
    if (rows.Count == 0) continue;
    sb.AppendLine($"### {bank} ({rows.Count} rows)");
    sb.AppendLine();
    sb.AppendLine("| Field | Min | Max | Distinct |");
    sb.AppendLine("|---|---|---|---|");
    foreach (var cell in rows[0].Cells)
    {
        var name = cell.Def.InternalName;
        if (cell.Def.DisplayType == PARAMDEF.DefType.dummy8) continue;
        var values = rows.Select(r => Convert.ToDouble(r.Cells.First(c => c.Def.InternalName == name).Value)).ToList();
        sb.AppendLine($"| {name} | {values.Min():0.###} | {values.Max():0.###} | {values.Distinct().Count()} |");
    }
    sb.AppendLine();
}

// --- Example rows: the light, tone map, fog, shadow rows the map pieces of a few maps use most
sb.AppendLine("## Example rows: what the map pieces of each main map use most");
sb.AppendLine();
foreach (var mapId in new[] { "m01_00_00_00", "m02_00_00_00", "m03_00_00_00", "m04_00_00_00", "m05_00_00_00", "m06_00_00_00", "m07_00_00_00", "m08_00_00_00" })
{
    var msbPath = Path.Combine(root, "map", "MapStudio", mapId + ".msb");
    if (!File.Exists(msbPath)) continue;
    var msb = MSBD.Read(msbPath);
    var banks = areas[mapId.Substring(1, 2)];
    sb.AppendLine($"### {mapId}");
    sb.AppendLine();
    foreach (var (bank, get) in partIds.Where(p => p.Bank is "LightBank" or "ToneMapBank" or "FogBank" or "ShadowBank" or "ToneCorrectBank"))
    {
        var id = msb.Parts.MapPieces.GroupBy(get).OrderByDescending(g => g.Count()).First().Key;
        var row = banks.TryGetValue(bank, out var p) ? p.Rows.FirstOrDefault(r => r.ID == id) : null;
        if (row == null) { sb.AppendLine($"- {bank} {id}: no row"); continue; }
        var vals = row.Cells.Where(c => c.Def.DisplayType != PARAMDEF.DefType.dummy8).Select(c => $"{c.Def.InternalName}={c.Value}");
        sb.AppendLine($"- **{bank} {id}** '{row.Name}': {string.Join(", ", vals)}");
    }
    sb.AppendLine();
}

File.WriteAllText(reportPath, sb.ToString());
Console.WriteLine($"Wrote {reportPath}");
