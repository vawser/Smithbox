using Andre.Formats;
using SoulsFormats;
using System.Text.RegularExpressions;

namespace StudioCore.Port;

/// <summary>
/// DS3 objects as ER assets (Smithbox Port Plan.md §4.6, decision D2: static and breakable objects; animated and cloth
/// ones left out). Per object model the MSB places: the objbnd's FLVERs (intact and _1 broken state) through the map
/// piece converter in object mode, its own TPF into the texture jobs, its physics through <see cref="PortPhysics"/>,
/// the asset's geombnd and _l geomhkxbnd (the pot AEG099_200's layout), and an AssetEnvironmentGeometryParam row
/// (a vanilla ER row with DS3's ObjectParam behaviour fields over it). Names: one AEG group per port (the manifest's),
/// a number per DS3 model, so a re-run replaces its own assets.
/// </summary>
public sealed class PortObjects
{
    /// <summary>Template rows (Ds3ErPortSurvey assetrows): a breakable that is static until broken, as DS3's are, and a static prop.</summary>
    public const int BreakableTemplateRow = 27025;
    public const int StaticTemplateRow = 27000;

    /// <summary>The first AEG group tried for a port (groups with no binder and no param row are free).</summary>
    public const int AegGroupFrom = 800;

    /// <summary>
    /// ObjectParam fields copied over the ER row where ER has the same field: what the object does when hit, ridden,
    /// burnt or broken. Sound, SFX, bullet and item lot IDs aren't copied (another game's IDs), nor animation fields.
    /// </summary>
    private static readonly string[] CopiedFields =
    [
        "hp", "defense", "breakStopTime", "isBreakByPlayerCollide", "isBreakByEnemyCollide", "isBreak_ByChrRide",
        "isBreakByHugeenemyCollide", "isDisableBreakForFirstAppear", "isDamageCover", "isAttackBacklash", "isLadder",
        "isMoveObj", "isBurn", "burnTime", "burnBraekRate", "isBreakOnPickUp", "navimeshFlag", "windEffectRate_0",
        "windEffectRate_1", "windEffectType_0", "windEffectType_1", "FragmentInvisibleWaitTime", "FragmentInvisibleTime"
    ];

    private const string ParamFile = "AssetEnvironmentGeometryParam.param";

    private readonly PortGame Input;
    private readonly PortGame Output;
    private readonly PortManifest Manifest;
    private readonly PortLog Log;
    private readonly PortPieces Pieces;
    private readonly PortPhysics Physics;
    private readonly PortObjectStats Stats = new();

    public int AegGroup { get; private set; }

    /// <summary>DS3 object model -> ER asset name (AEGGGG_NNN), for the models this run ported.</summary>
    public Dictionary<string, string> Assets { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Asset name -> what its row copies: the template row and the DS3 ObjectParam row.</summary>
    private readonly Dictionary<string, (int Template, int Ds3Row)> Rows = new(StringComparer.OrdinalIgnoreCase);

    public PortObjects(PortGame input, PortGame output, PortManifest manifest, PortLog log, PortPieces pieces, HitMaterialConfig hitConfig)
    {
        Input = input;
        Output = output;
        Manifest = manifest;
        Log = log;
        Pieces = pieces;
        Physics = new PortPhysics(output, hitConfig, log);
    }

    public void Run(MSB3 ds3Msb, CancellationToken cancel, Action<float> progress)
    {
        var parts = ds3Msb.Parts.Objects.GroupBy(o => o.ModelName, StringComparer.OrdinalIgnoreCase).OrderBy(g => g.Key).ToList();
        Log.Line($"DS3 MSB: {ds3Msb.Parts.Objects.Count} objects of {parts.Count} models");

        var param = ReadAssetParam(out _, out _, out _);
        PickGroup(param);
        Log.Line($"AEG group {AegGroup:000}");

        var geomTemplate = BND4.Read(Output.ReadVanilla("/asset/aeg/aeg099/aeg099_200.geombnd.dcx") ?? throw new InvalidOperationException("aeg099_200.geombnd.dcx not found in the ER game"));
        var flverTemplate = geomTemplate.Files.First(f => f.Name.EndsWith("AEG099_200.flver", StringComparison.OrdinalIgnoreCase));

        int done = 0, skippedParts = 0;
        foreach (var group in parts)
        {
            cancel.ThrowIfCancellationRequested();
            progress(done++ / (float)Math.Max(1, parts.Count));
            var model = group.Key.ToLowerInvariant();
            var path = $"/obj/{model}.objbnd.dcx";
            var bytes = Input.Read(path);
            if (bytes == null) { Log.Warn($"{model}: {path} not found"); skippedParts += group.Count(); continue; }

            var bnd = BND4.Read(bytes);
            BinderFile File(string suffix) => bnd.Files.FirstOrDefault(f => Path.GetFileName(f.Name).Equals(model + suffix, StringComparison.OrdinalIgnoreCase));
            if (bnd.Files.Any(f => f.Name.EndsWith(".anibnd", StringComparison.OrdinalIgnoreCase)))
            {
                Stats.Note("model left out: animated (anibnd)");
                skippedParts += group.Count();
                continue;
            }
            if (bnd.Files.Any(f => f.Name.EndsWith(".clm2", StringComparison.OrdinalIgnoreCase)))
            {
                Stats.Note("model left out: cloth (clm2)");
                skippedParts += group.Count();
                continue;
            }
            var intactFlver = File(".flver");
            if (intactFlver == null) { Log.Warn($"{model}: no FLVER in {path}"); skippedParts += group.Count(); continue; }

            var number = Number(model);
            var asset = $"AEG{AegGroup:000}_{number:000}";
            string Rename(string name) => Regex.Replace(name ?? "", Regex.Escape(model), asset, RegexOptions.IgnoreCase);

            foreach (var tpf in bnd.Files.Where(f => f.Name.EndsWith(".tpf", StringComparison.OrdinalIgnoreCase)))
                Pieces.AddTextures(TPF.Read(tpf.Bytes));

            // Models: intact, and the broken state if it has one
            var files = new List<BinderFile>();
            var renderTriangles = new List<System.Numerics.Vector3>();
            var broken = File("_1.flver");
            foreach (var (src, suffix, id) in new[] { (intactFlver, "", 200), (broken, "_1", 201) })
            {
                if (src == null) continue;
                var flver = FLVER2.Read(src.Bytes);
                var original = FLVER2.Read(src.Bytes);
                if (suffix == "") renderTriangles = RenderTriangles(original);
                var stem = model + suffix;
                Pieces.Convert(flver, stem, asset + suffix, out var uvFixes, new PortPieces.ObjectModel
                {
                    Broken = suffix == "_1",
                    // The intact model's single bone carries the asset's name, as its single body does
                    RenameBone = suffix == "" && original.Nodes.Count(n => !n.Name.StartsWith("dummy", StringComparison.OrdinalIgnoreCase)) == 1
                        ? n => n.StartsWith("dummy", StringComparison.OrdinalIgnoreCase) ? n : asset
                        : Rename
                });
                Pieces.CheckPiece(asset + suffix, original, flver, uvFixes);
                files.Add(new BinderFile(flverTemplate.Flags, id, AssetPath(flverTemplate.Name, asset).Replace(asset + ".flver", asset + suffix + ".flver"), flver.Write()));
            }

            var geom = BND4.Read(Output.ReadVanilla("/asset/aeg/aeg099/aeg099_200.geombnd.dcx"));
            geom.Files = files;
            Output.Write(GeomPath(asset), geom.Write());

            // Physics: intact (_L) and broken (_1_L); a model without an hkx is visual only
            var hkxFiles = new List<BinderFile>();
            foreach (var (suffix, id) in new[] { ("", 300), ("_1", 301) })
            {
                var src = File(suffix + ".hkx");
                if (src == null) continue;
                var built = Physics.Convert(PortPhysics.Bytes(src), Rename, suffix == "" ? asset : null, $"{model}{suffix} ({asset}{suffix})", Stats, suffix == "" ? renderTriangles : null);
                if (built == null) continue;
                var templateFile = Physics.BinderTemplate.Files.First(f => f.ID == id);
                var data = DCX.Is(templateFile.Bytes.ToArray()) ? DCX.Compress(built, DcxType(templateFile)) : built;
                hkxFiles.Add(new BinderFile(templateFile.Flags, id, AssetPath(templateFile.Name, asset), data) { CompressionType = templateFile.CompressionType });
            }
            if (hkxFiles.Count > 0)
            {
                var hkxBnd = BND4.Read(Output.ReadVanilla(PortPhysics.DynamicTemplateBinder));
                hkxBnd.Files = hkxFiles;
                Output.Write(GeomPath(asset).Replace(".geombnd.dcx", "_l.geomhkxbnd.dcx"), hkxBnd.Write());
            }

            var kind = broken != null ? "breakable" : hkxFiles.Count > 0 ? "static collision" : "visual only";
            Stats.Note($"model ported: {kind}");
            Rows[asset] = (broken != null ? BreakableTemplateRow : StaticTemplateRow, int.Parse(model[1..]));
            Assets[group.Key] = asset;
        }

        Log.Line($"Ported {Assets.Count} of {parts.Count} object models; {skippedParts} parts of the others left out");
        foreach (var (note, count) in Stats.Notes) Log.Line($"  {count,5}  {note}");
        WriteParams();
    }

    /// <summary>A FLVER's LOD0 triangles (each mesh's first plain face set), in model space.</summary>
    private static List<System.Numerics.Vector3> RenderTriangles(FLVER2 flver)
    {
        var result = new List<System.Numerics.Vector3>();
        foreach (var mesh in flver.Meshes)
        {
            var faces = mesh.FaceSets.FirstOrDefault(f => f.Flags == FLVER2.FaceSet.FSFlags.None) ?? mesh.FaceSets.FirstOrDefault();
            if (faces == null) continue;
            foreach (var i in faces.Triangulate(mesh.Vertices.Count < ushort.MaxValue))
                if (i < mesh.Vertices.Count) result.Add(mesh.Vertices[i].Position);
        }
        return result.Count % 3 == 0 ? result : result.Take(result.Count - result.Count % 3).ToList();
    }

    /// <summary>An ER asset binder file name with the template's model (AEG099_200) replaced, folder included.</summary>
    private static string AssetPath(string templateName, string asset) =>
        templateName.Replace(@"\AEG099\", $@"\{asset[..6]}\", StringComparison.OrdinalIgnoreCase).Replace("AEG099_200", asset, StringComparison.OrdinalIgnoreCase);

    private static string GeomPath(string asset) => $"/asset/aeg/{asset[..6].ToLowerInvariant()}/{asset.ToLowerInvariant()}.geombnd.dcx";

    private static DCX.Type DcxType(BinderFile f)
    {
        DCX.Decompress(f.Bytes, out DCX.Type type);
        return type;
    }

    /// <summary>The manifest's number for a DS3 model, else the next free one in the group.</summary>
    private int Number(string model)
    {
        var key = $"asset|{model}".ToLowerInvariant();
        if (Manifest.Models.TryGetValue(key, out var known)) return int.Parse(known[7..]);
        var taken = Manifest.Models.Where(m => m.Key.StartsWith("asset|")).Select(m => int.Parse(m.Value[7..])).ToHashSet();
        var n = 0;
        while (taken.Contains(n)) n++;
        if (n > 999) throw new InvalidOperationException($"AEG group {AegGroup:000} is full");
        Manifest.Models[key] = $"AEG{AegGroup:000}_{n:000}";
        return n;
    }

    /// <summary>The manifest's AEG group, else the first from AegGroupFrom with no binder in the game or project and no param row.</summary>
    private void PickGroup(Param param)
    {
        var existing = Manifest.Models.FirstOrDefault(m => m.Key.StartsWith("asset|")).Value;
        if (existing != null) { AegGroup = int.Parse(existing[3..6]); return; }
        var used = Output.Paths.Select(p => Regex.Match(p, @"^/asset/aeg/aeg(\d{3})/", RegexOptions.IgnoreCase)).Where(m => m.Success)
            .Select(m => int.Parse(m.Groups[1].Value)).Concat(param.Rows.Select(r => r.ID / 1000)).ToHashSet();
        AegGroup = Enumerable.Range(AegGroupFrom, 1000 - AegGroupFrom).Concat(Enumerable.Range(1, AegGroupFrom - 1)).First(g => !used.Contains(g));
    }

    // ---- params -------------------------------------------------------------------------------------------------

    /// <summary>The output project's regulation (its own copy, else the game's) and its asset param, paramdef applied.</summary>
    private Param ReadAssetParam(out BND4 regulation, out BinderFile file, out ulong version)
    {
        regulation = SFUtil.DecryptERRegulation(Output.Read("/regulation.bin") ?? throw new InvalidOperationException("ER regulation.bin not found"));
        file = regulation.Files.First(f => f.Name.EndsWith(ParamFile, StringComparison.OrdinalIgnoreCase));
        ulong.TryParse(regulation.Version, out version);
        var param = Param.Read(file.Bytes);
        var def = PARAMDEF.XmlDeserialize(Path.Combine(AppContext.BaseDirectory, "Assets", "PARAM", "ER", "Defs", "AssetGeometryParam.xml"), true);
        param.ApplyParamdef(def, version);
        return param;
    }

    /// <summary>
    /// A row per ported asset (ID GGGNNN): a copy of its template row with DS3's ObjectParam fields over it where ER has
    /// the same field. Rows of an earlier run (the same IDs) are replaced. The regulation is written into the output
    /// project and read back.
    /// </summary>
    private void WriteParams()
    {
        if (Rows.Count == 0) return;
        var param = ReadAssetParam(out var regulation, out var file, out _);

        var ds3Reg = SFUtil.DecryptDS3Regulation(Input.Read("/Data0.bdt") ?? throw new InvalidOperationException("DS3 Data0.bdt not found"));
        var ds3Param = Param.Read(ds3Reg.Files.First(f => f.Name.EndsWith("ObjectParam.param", StringComparison.OrdinalIgnoreCase)).Bytes);
        ds3Param.ApplyParamdef(PARAMDEF.XmlDeserialize(Path.Combine(AppContext.BaseDirectory, "Assets", "PARAM", "DS3", "Defs", "ObjectParam.xml")));

        int copied = 0, noDs3Row = 0;
        foreach (var (asset, (templateId, ds3Id)) in Rows.OrderBy(r => r.Key))
        {
            var id = int.Parse(asset[3..6]) * 1000 + int.Parse(asset[7..10]);
            var template = param.Rows.FirstOrDefault(r => r.ID == templateId) ?? throw new InvalidOperationException($"ER asset param has no template row {templateId}");
            foreach (var old in param.Rows.Where(r => r.ID == id).ToList()) param.RemoveRow(old);

            var row = new Param.Row(template) { ID = id, Name = $"DS3 o{ds3Id:000000}" };
            var ds3Row = ds3Param.Rows.FirstOrDefault(r => r.ID == ds3Id);
            if (ds3Row == null) noDs3Row++;
            else
                foreach (var field in CopiedFields)
                {
                    if (ds3Row[field] is not { } src || row[field] is not { } dst) continue;
                    dst.SetValue(System.Convert.ChangeType(src.Value, dst.Value.GetType()));
                    copied++;
                }
            param.AddRow(row);
        }
        param.Rows = param.Rows.OrderBy(r => r.ID).ToList();

        file.Bytes = param.Write();
        Output.Write("/regulation.bin", SFUtil.EncryptERRegulation(regulation, new byte[16]));
        Log.Line($"{ParamFile}: {Rows.Count} rows (templates {BreakableTemplateRow} breakable, {StaticTemplateRow} static), {copied} DS3 ObjectParam values"
            + (noDs3Row > 0 ? $"; {noDs3Row} models have no ObjectParam row" : ""));

        // Read back
        var back = ReadAssetParam(out _, out _, out _);
        Log.Check(Rows.Keys.All(a => back.Rows.Any(r => r.ID == int.Parse(a[3..6]) * 1000 + int.Parse(a[7..10]))), $"{ParamFile}: every ported asset has its row");
    }
}
