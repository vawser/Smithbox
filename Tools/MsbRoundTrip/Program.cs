using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using System.Security.Cryptography;

// Byte-perfect round trip of every Elden Ring MSB: decompressed bytes == MSBE.Read(...).Write(DCX.Type.None).
//   MsbRoundTrip <outFile> [GAME]
// Run with the working directory at src/Smithbox.Data, so the UXM dictionaries are found.
// The output lists each file as "path<TAB>ok|DIFF@offset|ERROR msg<TAB>sha1 of the input", so two runs can be diffed.

var outFile = Path.GetFullPath(args[0]);
var gameName = args.Length > 1 ? args[1] : "ER";

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2))
    .Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());

var root = gamePaths[gameName];
var game = Enum.Parse<Game>(gameName);
var fs = new CompundVirtualFileSystem([ArchiveBinderVirtualFileSystem.FromGameFolder(root, game), new RealVirtualFileSystem(root, false)]);

var paths = fs.GetFileNamesMatchingRecursive("/map/mapstudio", @"(?i)\.msb(\.dcx)?$").Select(p => p.Replace('\\', '/'))
    .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();

// stats mode: histograms of chosen values over every MSB instead of the round trip
if (args.Length > 2 && args[2] == "stats")
{
    var hist = new SortedDictionary<string, SortedDictionary<string, int>>();
    void Add(string key, object v)
    {
        if (!hist.TryGetValue(key, out var h)) hist[key] = h = new();
        var s = v?.ToString() ?? "null";
        h[s] = h.GetValueOrDefault(s) + 1;
    }
    foreach (var path in paths)
    {
        MSBE msb;
        try { msb = MSBE.Read(fs.ReadFile(path).Value.ToArray()); } catch { continue; }
        foreach (var e in msb.Parts.Enemies.Cast<MSBE.Part.EnemyBase>().Concat(msb.Parts.DummyEnemies))
        {
            Add("Enemy time", $"{e.ActivateTimeStartHour:D2}:{e.ActivateTimeStartMinute:D2}-{e.ActivateTimeEndHour:D2}:{e.ActivateTimeEndMinute:D2}");
            Add("Enemy UnkT14", e.UnkT14);
        }
        foreach (var a in msb.Parts.Assets)
        {
            Add("Asset UnkT51", a.UnkT51);
            Add("Asset UnkT13", a.ExcludeActivateRatioOverride_PS4);
            Add("Asset UnkT52", a.ExcludeActivateRatioOverride_XboxOne);
        }
        var map = path.Split('/').Last()[..12];
        foreach (var p in msb.Parts.GetEntries())
        {
            if (p.UnkE3E != 0) Add("UnkE3E examples", $"{map} {p.GetType().Name} {p.Name} = {p.UnkE3E}");
            if (p.UnkE3E != 0 && map.EndsWith("_00") && map.StartsWith("m1")) Add("UnkE3E legacy _00", $"{map} {p.GetType().Name} {p.Name} ({p.ModelName})");
            Add("UnkE3E by map suffix", $"_{map[10..12]} {p.GetType().Name} {p.UnkE3E}");
            if (p.UnkE3C != -1) Add("UnkE3C examples", $"{map} {p.GetType().Name} {p.Name} = {p.UnkE3C}");
            Add("UnkE3E by type", $"{p.GetType().Name} {p.UnkE3E}");
            Add("UnkE3C by type", $"{p.GetType().Name} {p.UnkE3C}");
        }
        foreach (var a in msb.Parts.Assets)
        {
            var u1 = a.AssetUnk1; var u3 = a.AssetUnk3;
            Add("Asset Unk1 switch +0", u1.OverrideCollisionSettings);
            if (u1.OverrideCollisionSettings != 0)
                Add("Asset Unk1 when switch on: Unk24/26 as int (LocationTextID?)", u1.LocationTextID);
            Add("Asset Unk1 +20 (fast travel flag?)", u1.EnableFastTravelEventFlagID);
            Add("Asset Unk1 +2C (play region?)", u1.PlayRegionID);
            Add("Asset Unk3 switch +0", u3.OverrideCollisionGparamSettings);
            Add("Asset Unk3 +09 (gparam override?)", u3.GparamOverride);
            Add("Asset Unk3 +08/+0C", $"{u3.AssetUnkStruct3_Unk08} / {u3.AssetUnkStruct3_Unk0C}");
        }
        foreach (var a in msb.Parts.Assets)
            if (a.UnkT51 != 0) Add("UnkT51 examples", $"{map} {a.Name} ({a.ModelName}) = {a.UnkT51}");
        foreach (var c in msb.Parts.Collisions)
        {
            var v = c.DisplayGroupStruct.Condition;
            Add("Collision DisplayGroupStruct.Condition bytes", v == -1 ? "-1" : $"{v & 0xFF} {(v >> 8) & 0xFF} {(v >> 16) & 0xFF} {(v >> 24) & 0xFF}");
            if (v != -1 && v != 0) Add("Collision Condition (own map)", $"{path.Split('/').Last()[..12]} -> m{v & 0xFF:D2}_{(v >> 8) & 0xFF:D2}_{(v >> 16) & 0xFF:D2}");
        }
        // group A (consumer tracing): collision values copied into ChrIns +2E0..+2F8
        Add("A PartsParam version", msb.Parts.Version);
        foreach (var c in msb.Parts.Collisions)
        {
            Add("A Collision UnkT02 (indoor type?)", c.OutdoorIndoorType);
            Add("A Collision UnkT03 (easy occlusion?)", c.EasyOcclusionID);
            Add("A Collision UnkT4C", c.UnkT4C);
            Add("A Collision UnkT1C vs LocationTextID", c.UnkT1C == -1 ? "-1" : c.UnkT1C == c.LocationTextID ? "== LocationTextID" : c.UnkT1C == Math.Abs(c.LocationTextID) ? "== |LocationTextID|" : "other");
            if (c.UnkT1C != -1) Add("A Collision UnkT1C examples", $"{map} {c.Name} T1C={c.UnkT1C} loc={c.LocationTextID}");
            Add("A Collision PlayRegionID / UnkT38", $"{(c.PlayRegionID == -1 ? "-1" : "set")} / {(c.LegacyPlayRegionID == -1 ? "-1" : "set")}");
            if (c.PlayRegionID != -1 && c.PlayRegionID != 0) Add("A Collision PlayRegionID examples", $"{map} {c.Name} {c.PlayRegionID}");
            if (c.UnkT4C != 0) Add("A Collision UnkT4C examples", $"{map} {c.Name} = {c.UnkT4C}");
            if (c.OutdoorIndoorType != 255 && c.OutdoorIndoorType != 0) Add("A Collision UnkT02 examples", $"{map} {c.Name} = {c.OutdoorIndoorType}");
        }
        foreach (var a in msb.Parts.Assets)
        {
            var u4 = a.AssetUnk4;
            Add("A AssetUnk4 Unk00 / Unk02 / Unk03", $"{u4.OverrideCollisionSoundSettings} / {u4.OutdoorIndoorType} / {u4.EasyOcclusionID}");
        }
        // group B: collision values overriding per-map params
        foreach (var c in msb.Parts.Collisions)
        {
            Add("B Collision UnkT06 (+8) MapGridCreateHeightDetailLimitInfo ID?", c.MapGridCreateHeightDetailLimitInfoID <= 0 ? c.MapGridCreateHeightDetailLimitInfoID.ToString() : "set");
            if (c.MapGridCreateHeightDetailLimitInfoID > 0) Add("B UnkT06 values", c.MapGridCreateHeightDetailLimitInfoID);
            Add("B Collision UnkT14 (PlayerLightScale?)", c.PlayerLightScale);
            Add("B Collision UnkT4D (timezone light 1 on / 2 off?)", c.TimezonePlayerLightScaleOverride);
            Add("B Collision UnkT4E (OpenChrActivateThreshold?)", c.OpenChrActivateThreshold);
            Add("B SceneGparam Unk1C / Unk20 (MapRegionID?)", $"{c.SceneGparamConfigStruct.Unk1C} / {c.SceneGparamConfigStruct.Unk20}");
            if (c.SceneGparamConfigStruct.Unk20 >= 0) Add("B SceneGparam Unk20 values", c.SceneGparamConfigStruct.Unk20);
            Add("B GparamOverride", c.GparamOverride);
        }
        // group C: asset type data read by the CSWorldGeomInfo wrappers
        foreach (var a in msb.Parts.Assets)
        {
            Add("C Asset UnkT01 (collisionType override?)", a.CollisionTypeOverride);
            Add("C Asset UnkT02 low/high byte", $"{a.UnkT02 & 0xFF} / {(a.UnkT02 >> 8) & 0xFF}");
            Add("C Asset UnkT08 (pointer?)", a.UnkT08 == 0 ? "0" : "set");
            Add("C Asset UnkT10", a.UnkT10);
            Add("C Asset UnkT28 as 4 shorts", a.UnkT28 == 0 ? "0" : $"{a.UnkT28 & 0xFFFF} {(a.UnkT28 >> 16) & 0xFFFF}");
            Add("C Asset UnkT30 (pick-up lot?)", a.PickUpItemLotParamID <= 0 ? a.PickUpItemLotParamID.ToString() : "set");
            if (a.PickUpItemLotParamID > 0) Add("C Asset UnkT30 values", a.PickUpItemLotParamID);
            var t34 = a.BreakItemLotParamID;
            Add("C Asset int +34 (break lot?)", t34 <= 0 ? t34.ToString() : "set");
            if (t34 > 0) Add("C Asset int +34 values", t34);
            Add("C Asset UnkT50", a.UnkT50);
        }
        // groups D, E, F
        foreach (var p in msb.Parts.GetEntries())
        {
            var t = p.GetType().Name;
            var tl = p.GetType().GetProperty("TileLoadConfig")?.GetValue(p) as MSBE.Part.TileLoadConfig;
            if (tl != null)
            {
                Add("F TileLoad Unk04 (name prefix length?)", tl.NamePrefixLength);
                if (tl.NamePrefixLength != 0) Add("F Unk04 examples", $"{t} {p.Name} skip {tl.NamePrefixLength} -> '{(p.Name.Length >= tl.NamePrefixLength ? p.Name[tl.NamePrefixLength..] : "?")}'");
                Add("F Unk04 == prefix up to '-'+1", tl.NamePrefixLength == 0 ? "0" : (p.Name.IndexOf('-') + 1 == tl.NamePrefixLength ? "yes" : "no"));
                Add("F TileLoad Unk0C", tl.TileLoadConfig_Unk0C);
                // is Unk0C the index of the collision the part refers to?
                string colName = (p.GetType().GetProperty("CollisionPartName")?.GetValue(p) as string);
                if (colName == null && p is MSBE.Part.Asset asset) colName = !string.IsNullOrEmpty(asset.PartNames[5]) ? asset.PartNames[5] : asset.PartNames[4];
                if (tl.TileLoadConfig_Unk0C >= 0 && !string.IsNullOrEmpty(colName))
                {
                    var ci = msb.Parts.Collisions.FindIndex(c => c.Name == colName);
                    var cci = msb.Parts.ConnectCollisions.FindIndex(c => c.Name == colName);
                    Add("F Unk0C vs referenced collision index", ci < 0 ? "collision not found" : ci == tl.TileLoadConfig_Unk0C ? "== collision index" : $"differs");
                }
                if (tl.TileLoadConfig_Unk0C >= 0) Add("F Unk0C examples", $"{map} {t} {p.Name} = {tl.TileLoadConfig_Unk0C} col={colName} ncol={msb.Parts.Collisions.Count}");
                Add("F TileLoad Unk10", tl.TileLoadConfig_Unk10);
            }
            var s9 = p.GetType().GetProperty("UnkStruct9")?.GetValue(p) as MSBE.Part.UnkStruct9;
            if (s9 != null) Add("D UnkStruct9_Unk00 by type", $"{t} {s9.UnkStruct9_Unk00}");
            var g = p.GetType().GetProperty("GrassConfigStruct")?.GetValue(p) as MSBE.Part.GrassConfigStruct;
            if (g != null) Add("E Grass Unk18 / Unk1a", $"{t} {g.GrassConfigStruct_Unk18} / {g.GrassConfigStruct_Unk1a}");
        }
        // group G: regions
        foreach (var r in msb.Regions.GroupPlacementPoints)
        {
            Add("G GroupPlacementPoint RandomAppearParamID", r.RandomAppearParamID == 0 ? "0" : "set");
            if (r.RandomAppearParamID != 0) Add("G GroupPlacementPoint T00 values", r.RandomAppearParamID);
            Add("G GroupPlacementPoint platoon events", string.Join(",", r.PlatoonEventNames.Select(n => n == null ? "-" : msb.Events.GetEntries().First(x => x.Name == n).GetType().Name)));
            Add("G GroupPlacementPoint unk parts", string.Join(",", r.UnkPartNames.Where(n => n != null)));
        }
        foreach (var r in msb.Regions.Messages)
        {
            Add("G Message UnkT02", r.UnkT02);
            if (r.UnkT02 != 0 && r.UnkT02 != -1) Add("G Message UnkT02 examples", $"{map} {r.Name} msg={r.MessageID} t02={r.UnkT02}");
        }
        foreach (var p in msb.Events.PlatoonInfo)
            Add("Platoon State bytes", $"{p.State} {p.EnableDeactiveMove} {p.UnkT06} {p.UseMemberEntityIDs} ids {p.MemberEntityIDs[0]},{p.MemberEntityIDs[1]}");
    }
    // the regulation params the group B fields fall back to
    try
    {
        var reg = SFUtil.DecryptERRegulation(Path.Combine(root, "regulation.bin"));
        var defDir = Path.Combine("Assets", "PARAM", "ER", "Defs");
        void Param(string file, string def, params string[] fields)
        {
            var f = reg.Files.FirstOrDefault(x => x.Name.EndsWith(file + ".param", StringComparison.OrdinalIgnoreCase));
            if (f == null) { Add("B regulation missing", file); return; }
            var p = PARAM.Read(f.Bytes);
            Add($"B reg {file} row count", p.Rows.Count);
            foreach (var r in p.Rows.Take(400)) Add($"B reg {file} row IDs (first 400)", r.ID);
            if (fields.Length == 0) return;
            p.ApplyParamdef(PARAMDEF.XmlDeserialize(Path.Combine(defDir, def + ".xml")));
            foreach (var r in p.Rows)
                foreach (var fld in fields) Add($"B reg {file}.{fld}", r[fld]?.Value);
        }
        Param("MapDefaultInfoParam", "MapDefaultInfoParam", "PlayerLigntScale", "IsEnableTimezonnePlayerLigntScale", "OpenChrActivateThreshold", "WeatherCreateAssetLimitId");
        Param("MapGdRegionInfoParam", "MapGdRegionInfo", "mapRegionId");
        Param("MapGridCreateHeightDetailLimitInfo", "MapGridCreateHeightDetailLimitInfo");
    }
    catch (Exception e) { Add("B regulation error", e.Message); }

    // F: is TileLoad Unk0C the index of the part's collision in the source tile (TileLoadConfig.MapID)?
    {
        var cols = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);     // map -> collision names
        var parts = new Dictionary<string, MSBE.Part>(StringComparer.OrdinalIgnoreCase);        // "map|part" -> part
        foreach (var path in paths)
        {
            MSBE m; try { m = MSBE.Read(fs.ReadFile(path).Value.ToArray()); } catch { continue; }
            var mn = path.Split('/').Last()[..12];
            cols[mn] = m.Parts.Collisions.Select(c => c.Name).ToList();
            foreach (var p in m.Parts.GetEntries()) parts[mn + "|" + p.Name] = p;
        }
        foreach (var (key, p) in parts)
        {
            var tl = p.GetType().GetProperty("TileLoadConfig")?.GetValue(p) as MSBE.Part.TileLoadConfig;
            if (tl == null || tl.TileLoadConfig_Unk0C < 0) continue;
            var id = tl.MapID;
            var src = $"m{id[3]:D2}_{id[2]:D2}_{id[1]:D2}_{id[0]:D2}";
            Add("F Unk0C MapID examples", $"{key} id=[{string.Join(",", id)}] -> {src} unk0c={tl.TileLoadConfig_Unk0C}");
            if (!cols.TryGetValue(src, out var srcCols)) { Add("F Unk0C source tile", "source MSB not found"); continue; }
            Add("F Unk0C < collisions in source tile", tl.TileLoadConfig_Unk0C < srcCols.Count ? "yes" : $"NO ({srcCols.Count})");
            // the same part in the source tile, and its collision
            var shortName = tl.NamePrefixLength > 0 && p.Name.Length > tl.NamePrefixLength ? p.Name[tl.NamePrefixLength..] : p.Name;
            if (parts.TryGetValue(src + "|" + shortName, out var orig))
            {
                string col = orig.GetType().GetProperty("CollisionPartName")?.GetValue(orig) as string;
                if (col == null && orig is MSBE.Part.Asset oa) col = !string.IsNullOrEmpty(oa.PartNames[5]) ? oa.PartNames[5] : oa.PartNames[4];
                if (string.IsNullOrEmpty(col)) { Add("F Unk0C vs source part's collision", "source part has no collision"); continue; }
                var ci = srcCols.IndexOf(col);
                Add("F Unk0C vs source part's collision", ci == tl.TileLoadConfig_Unk0C ? "== its index" : ci < 0 ? "collision not in source" : "differs");
            }
            else
            {
                Add("F Unk0C vs source part's collision", "source part not found");
                if (hist.GetValueOrDefault("F source names sample")?.Count is null or < 5)
                    Add("F source names sample", string.Join(" ", parts.Keys.Where(k => k.StartsWith(src + "|")).Take(4)) + $"  (looked for {shortName})");
            }
        }
    }

    // membership: every distinct MSB value against the param values it falls back to
    void Member(string msbKey, string regKey)
    {
        if (!hist.TryGetValue(msbKey, out var m) || !hist.TryGetValue(regKey, out var r)) { Add("B check missing", $"{msbKey} / {regKey}"); return; }
        foreach (var v in m.Keys) Add($"B check {msbKey} in {regKey}", r.ContainsKey(v) ? "yes" : $"NO {v}");
    }
    var rowIds = new Dictionary<string, int>();
    try
    {
        // ItemLotParam_map row IDs, for the lot overrides
        var reg2 = SFUtil.DecryptERRegulation(Path.Combine(root, "regulation.bin"));
        var lot = reg2.Files.First(x => x.Name.EndsWith("ItemLotParam_map.param", StringComparison.OrdinalIgnoreCase));
        foreach (var r in PARAM.Read(lot.Bytes).Rows) Add("C reg ItemLotParam_map row IDs", r.ID);
    }
    catch (Exception e) { Add("C regulation error", e.Message); }
    try
    {
        var reg3 = SFUtil.DecryptERRegulation(Path.Combine(root, "regulation.bin"));
        var ra = reg3.Files.First(x => x.Name.EndsWith("RandomAppearParam.param", StringComparison.OrdinalIgnoreCase));
        foreach (var r in PARAM.Read(ra.Bytes).Rows) Add("G reg RandomAppearParam row IDs", r.ID);
    }
    catch (Exception e) { Add("G regulation error", e.Message); }
    Member("G GroupPlacementPoint T00 values", "G reg RandomAppearParam row IDs");
    Member("C Asset UnkT30 values", "C reg ItemLotParam_map row IDs");
    Member("C Asset int +34 values", "C reg ItemLotParam_map row IDs");
    Member("B UnkT06 values", "B reg MapGridCreateHeightDetailLimitInfo row IDs (first 400)");
    Member("B SceneGparam Unk20 values", "B reg MapGdRegionInfoParam.mapRegionId");
    Member("B GparamOverride", "B reg MapDefaultInfoParam.WeatherCreateAssetLimitId");

    foreach (var (k, h) in hist)
    {
        Console.WriteLine($"== {k} ({h.Values.Sum()})");
        foreach (var (v, n) in h.OrderByDescending(x => x.Value).Take(25)) Console.WriteLine($"   {v}\t{n}");
    }
    return;
}

var lines = new List<string>();
int ok = 0, diff = 0, error = 0;
foreach (var path in paths)
{
    string result;
    string sha = "";
    try
    {
        var raw = fs.ReadFile(path).Value.ToArray();
        var plain = DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw;
        sha = Convert.ToHexString(SHA1.HashData(plain));
        var msb = MSBE.Read(plain);
        var back = msb.Write(DCX.Type.None);
        var n = Math.Min(back.Length, plain.Length);
        var at = -1;
        for (var i = 0; i < n; i++) if (back[i] != plain[i]) { at = i; break; }
        if (at < 0 && back.Length != plain.Length) at = n;
        if (at < 0) { result = "ok"; ok++; }
        else { result = $"DIFF@0x{at:X} ({plain.Length} -> {back.Length})"; diff++; }
    }
    catch (Exception e) { result = "ERROR " + e.Message.Replace('\n', ' '); error++; }
    lines.Add($"{path}\t{result}\t{sha}");
}

File.WriteAllLines(outFile, lines);
Console.WriteLine($"{paths.Count} MSBs: {ok} byte-perfect, {diff} differ, {error} errors");
foreach (var l in lines.Where(l => !l.Contains("\tok\t")).Take(20)) Console.WriteLine("  " + l);
