using Havok.Shared;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StudioCore.Port;

/// <summary>What a map port is asked to do (Smithbox Port Plan.md §2).</summary>
public sealed class PortRequest
{
    public ProjectDescriptor Input;
    public ProjectDescriptor Output;
    public string Ds3Map;
    public string TargetMap;
    /// <summary>Added to every ported part's DS3 position.</summary>
    public Vector3 Offset;
    /// <summary>Remove the target map's vanilla map pieces, collisions, assets and enemies first (§4.1).</summary>
    public bool ClearTarget;
    public bool Collision = true;
    /// <summary>Static and breakable objects as ER assets (§4.6).</summary>
    public bool Objects = true;
    /// <summary>Breakable objects keep their broken state and break; off, they come over as static objects.</summary>
    public bool Breakables = true;
    /// <summary>Enclose the ported map in a shadow-only box that keeps the sun out, as ER's catacombs do (§4.9).</summary>
    public bool LightShell;
}

/// <summary>
/// A whole DS3 map into an existing ER map (Smithbox Port Plan.md, P1 and P1a): every map piece the DS3 MSB places,
/// with its materials and textures, the h and l collision, and the MSB parts at their DS3 transforms plus the offset.
/// Reads the input project, writes the output project's folder only. Runs on a background thread.
/// </summary>
public sealed class MapPort
{
    public PortLog Log { get; } = new();
    public string Stage { get; private set; } = "";
    public float Progress { get; private set; }
    public bool Succeeded { get; private set; }
    public string ReportPath { get; private set; }

    private PortRequest Request;
    private PortGame Input;
    private PortGame Output;
    private PortConfig Config;
    private HitMaterialConfig HitConfig;
    private PortManifest Manifest;
    private string ManifestPath;

    public static string AssetFolder => Path.Combine(AppContext.BaseDirectory, "Assets", "Game Port", "DS3_ER");

    public void Run(PortRequest request, CancellationToken cancel)
    {
        Request = request;
        try
        {
            Execute(cancel);
            Succeeded = Log.Failures == 0;
        }
        catch (OperationCanceledException)
        {
            Log.Line();
            Log.Warn("Cancelled. Files written before the cancel stay in the output project.");
        }
        catch (Exception e)
        {
            Log.Fail($"{e.GetType().Name}: {e.Message}");
            Log.Line(e.StackTrace ?? "");
            Log.Warn($"The port stopped at '{Stage}'. Files written before it (map pieces, collision, assets) are in the output project, " +
                "but the later stages (materials, textures, MSB, params) were not: the target map may place models that don't match. " +
                "Fix the cause and port again before loading the map in game.");
        }
        finally
        {
            SaveManifestAndReport();
            Input?.Dispose();
            Output?.Dispose();
            Stage = Succeeded ? "Done" : "Finished with failures";
            Progress = 1;
        }
    }

    private void Step(string stage, float progress)
    {
        Stage = stage;
        Progress = progress;
        Log.Line();
        Log.Line($"== {stage}");
    }

    private void Execute(CancellationToken cancel)
    {
        var ds3Map = Request.Ds3Map;
        var target = Request.TargetMap;

        Step("Opening projects", 0);
        Config = JsonSerializer.Deserialize(File.ReadAllText(Path.Combine(AssetFolder, "Materials.json")), PortJsonContext.Default.PortConfig);
        HitConfig = JsonSerializer.Deserialize(File.ReadAllText(Path.Combine(AssetFolder, "HitMaterials.json")), PortJsonContext.Default.HitMaterialConfig);
        Input = PortGame.Open(Request.Input);
        Output = PortGame.Open(Request.Output);
        ManifestPath = Path.Combine(Request.Output.ProjectPath, ".smithbox", "port", "DS3_ER.json");
        Manifest = File.Exists(ManifestPath)
            ? JsonSerializer.Deserialize(File.ReadAllText(ManifestPath), PortJsonContext.Default.PortManifest) ?? new()
            : new();
        Log.Line($"Input  {Request.Input.ProjectName}: {Request.Input.ProjectPath} over {Request.Input.DataPath}");
        Log.Line($"Output {Request.Output.ProjectName}: {Request.Output.ProjectPath} over {Request.Output.DataPath}");
        Log.Line($"{ds3Map} -> {target}, offset {Request.Offset}, clear {Request.ClearTarget}, collision {Request.Collision}, objects {Request.Objects}, breakables {Request.Breakables}");
        Log.Line($"Materials.json: {Config.Generated}; {Config.Rules.Count} rules, {Config.Shaders.Count} shaders");

        var ds3Msb = MSB3.Read(Input.Read($"/map/mapstudio/{ds3Map}.msb.dcx"));
        var targetMsbPath = $"/map/mapstudio/{target}.msb.dcx";
        var vanillaMsb = MSBE.Read(Output.ReadVanilla(targetMsbPath) ?? Output.Read(targetMsbPath));
        var msb = MSBE.Read(Output.Read(targetMsbPath));
        Log.Line($"DS3 MSB: {ds3Msb.Parts.MapPieces.Count} map pieces of {ds3Msb.Parts.MapPieces.Select(p => p.ModelName).Distinct().Count()} models, {ds3Msb.Parts.Collisions.Count} collisions");

        // Map pieces
        Step("Map pieces", 0.02f);
        var pieces = new PortPieces(Config, Input, Output, Manifest, Log, ds3Map, target);
        var models = ds3Msb.Parts.MapPieces.Select(p => p.ModelName).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m => m).ToList();
        var pieceNumbers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var pieceBoxes = new Dictionary<string, (Vector3 Min, Vector3 Max)>(StringComparer.OrdinalIgnoreCase);
        var erTemplate = Output.MapPieces(target).FirstOrDefault(p => Output.ReadVanilla(p) != null)
            ?? throw new InvalidOperationException($"{target} has no vanilla map piece to take the binder layout from");
        var templateBnd = BND4.Read(Output.Read(erTemplate));
        var templateFile = templateBnd.Files.First(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase));
        var templateStem = Path.GetFileNameWithoutExtension(templateFile.Name);

        int done = 0, ported = 0;
        foreach (var model in models)
        {
            cancel.ThrowIfCancellationRequested();
            Progress = 0.02f + 0.48f * done++ / Math.Max(1, models.Count);
            Stage = $"Map pieces ({done}/{models.Count})";

            var ds3Path = $"/map/{ds3Map}/{ds3Map}_{model[1..]}.mapbnd.dcx";
            var bytes = Input.Read(ds3Path);
            if (bytes == null) { Log.Warn($"{model}: {ds3Path} not found"); continue; }

            var bnd = BND4.Read(bytes);
            var file = bnd.Files.FirstOrDefault(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase));
            if (file == null) { Log.Warn($"{model}: no FLVER in {ds3Path}"); continue; }

            var number = ModelNumber(target, model, n => Output.Exists($"{Output.MapFolder(target)}{target}_{n}.mapbnd.dcx"), pieceNumbers.Values);
            pieceNumbers[model] = number;
            var newStem = $"{target}_{number}";

            var flver = FLVER2.Read(file.Bytes);
            var original = FLVER2.Read(file.Bytes);
            var stem = Path.GetFileNameWithoutExtension(file.Name);
            pieceBoxes[model] = (flver.Header.BoundingBoxMin, flver.Header.BoundingBoxMax);
            pieces.Convert(flver, stem, newStem, out var uvFixes);

            var outBnd = BND4.Read(Output.Read(erTemplate));
            outBnd.Files = [new BinderFile(templateFile.Flags, templateFile.ID,
                templateFile.Name.Replace(templateStem, newStem), flver.Write())];
            var outPath = $"{Output.MapFolder(target)}{newStem}.mapbnd.dcx";
            Output.Write(outPath, outBnd.Write());

            // Read back
            var back = BND4.Read(Output.Read(outPath));
            pieces.CheckPiece(newStem, original, FLVER2.Read(back.Files[0].Bytes), uvFixes);
            ported++;
        }
        Log.Line($"Ported {ported} of {models.Count} map piece models");
        Log.Line("Rules used (materials):");
        foreach (var (rule, count) in pieces.RuleCounts.OrderByDescending(r => r.Value))
            Log.Line($"  {count,5}  {rule}");
        Log.Line("Notes:");
        pieces.LogNotes();

        // Objects as assets: their materials and textures join the map pieces' below
        var assets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (Request.Objects)
        {
            Step("Objects", 0.42f);
            var objects = new PortObjects(Input, Output, Manifest, Log, pieces, HitConfig, Request.Breakables);
            objects.Run(ds3Msb, cancel, p => { Progress = 0.42f + 0.08f * p; Stage = $"Objects ({p:P0})"; });
            assets = objects.Assets;
        }

        // MATBINs into allmaterial
        Step("Materials", 0.5f);
        WriteMatbins(pieces.Matbins.Values.ToList());

        // Textures
        Step("Textures", 0.55f);
        var missing = pieces.Jobs.Values.SelectMany(j => new[] { j.Source, j.Spec }).Where(n => n != null && !pieces.Textures.ContainsKey(n)).Distinct().ToList();
        if (missing.Count > 0)
        {
            Stage = "Textures (searching the other DS3 areas)";
            var found = pieces.FindTextures(missing, ds3Map);
            Log.Line($"{missing.Count} textures not in {ds3Map[..3]}'s binders: {found} found in other areas");
        }
        PortTextures.Run(Config, Output, pieces.Textures, pieces.Jobs.Values, Log,
            p => Progress = 0.55f + 0.25f * p, cancel);

        // Collision
        var collisionNumbers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (Request.Collision)
        {
            Step("Collision", 0.8f);
            PortCollision(ds3Msb, ds3Map, target, collisionNumbers, cancel);
        }

        // MSB
        Step("MSB", 0.92f);
        WriteMsb(ds3Msb, vanillaMsb, msb, target, pieceNumbers, collisionNumbers, pieceBoxes, assets);
    }

    /// <summary>
    /// The ER model number of a DS3 model in the target map: the manifest's, else the DS3 number when the target has
    /// no such file, else the next free one from 900000. Keyed by target, DS3 map and model: two DS3 maps share model
    /// names (m001500 in m30 and in m38), and keys without the DS3 map gave a second map's port the first one's
    /// numbers, so its models were written over the first's files (2026-11-21, m38 then m30 into m10_01).
    /// </summary>
    private string ModelNumber(string target, string ds3Model, Func<string, bool> exists, IEnumerable<string> takenThisRun)
    {
        var key = $"{target}|{Request.Ds3Map}|{ds3Model}".ToLowerInvariant();
        if (Manifest.Models.TryGetValue(key, out var known)) return known;

        // Every number the manifest gave this target's models of the same kind (m or h), from any DS3 map
        var taken = takenThisRun.Concat(Manifest.Models.Where(m => m.Key.StartsWith(target + "|", StringComparison.OrdinalIgnoreCase)
                && char.ToLowerInvariant(m.Key.Split('|')[^1][0]) == char.ToLowerInvariant(ds3Model[0])).Select(m => m.Value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var number = ds3Model[1..];
        if (exists(number) || taken.Contains(number))
        {
            var n = 900000;
            while (exists($"{n:000000}") || taken.Contains($"{n:000000}")) n++;
            number = $"{n:000000}";
        }
        Manifest.Models[key] = number;
        return number;
    }

    // ---- materials ----------------------------------------------------------------------------------------------

    /// <summary>
    /// The project's allmaterial (else the game's) with the ported MATBINs added; a MATBIN of the same name (an earlier
    /// run) is replaced.
    /// </summary>
    private void WriteMatbins(List<PortPieces.PortedMatbin> matbins)
    {
        const string path = "/material/allmaterial.matbinbnd.dcx";
        var bnd = BND4.Read(Output.ReadProjectOnly(path) ?? Output.Read(path));
        var before = bnd.Files.Count;
        var names = matbins.Select(m => m.BinderName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var replaced = bnd.Files.RemoveAll(f => names.Contains(f.Name));
        var flags = bnd.Files[0].Flags;
        var nextId = bnd.Files.Max(f => f.ID) + 1;
        foreach (var m in matbins.OrderBy(m => m.Name))
            bnd.Files.Add(new BinderFile(flags, nextId++, m.BinderName, m.Matbin.Write()));
        Output.Write(path, bnd.Write());
        Log.Line($"allmaterial: {before - replaced} kept + {matbins.Count} ported ({replaced} replaced from an earlier run)");

        // Read back
        var back = BND4.Read(Output.Read(path));
        Log.Check(back.Files.Select(f => f.ID).Distinct().Count() == back.Files.Count, "allmaterial: binder ids unique");
        Log.Check(back.Files.Select(f => f.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == back.Files.Count, "allmaterial: names unique");
        var written = back.Files.Where(f => names.Contains(f.Name)).ToList();
        Log.Check(written.Count == matbins.Count, "allmaterial: every ported MATBIN present");
        foreach (var f in written)
        {
            var mb = MATBIN.Read(f.Bytes);
            Log.Check(mb.Key == Editors.MaterialEditor.MaterialKeys.Compute(mb.SourcePath)
                && mb.Params.All(p => p.Key == Editors.MaterialEditor.MaterialKeys.Compute(p.Name))
                && mb.Samplers.All(s => s.Key == Editors.MaterialEditor.MaterialKeys.Compute(s.Type)), $"{f.Name}: keys");
        }
    }

    // ---- collision ----------------------------------------------------------------------------------------------

    /// <summary>The h mesh box of each ported collision model (its own space), for the EnvMap bounds.</summary>
    private readonly Dictionary<string, (Vector3 Min, Vector3 Max)> CollisionBoxes = new(StringComparer.OrdinalIgnoreCase);

    private void PortCollision(MSB3 ds3Msb, string ds3Map, string target, Dictionary<string, string> numbers, CancellationToken cancel)
    {
        var ds3Naming = CollisionEditorNaming.For("DS3");
        var erNaming = CollisionEditorNaming.For("ER");
        var ds3Models = ds3Msb.Parts.Collisions.Select(p => p.ModelName).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(m => m).ToList();
        var needsRow = new Dictionary<int, int>();
        var builtH = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var type in new[] { "h", "l" })
        {
            cancel.ThrowIfCancellationRequested();
            var ds3Bhd = $"/map/{ds3Map}/{type}{ds3Map[1..]}.hkxbhd";
            var erBhd = $"{Output.MapFolder(target)}{type}{target[1..]}.hkxbhd";
            if (!Input.Exists(ds3Bhd)) { Log.Warn($"{type}: DS3 {ds3Bhd} not found"); continue; }
            if (!Output.Exists(erBhd)) { Log.Warn($"{type}: ER {erBhd} not found; {target} gets no {type} collision"); continue; }

            var ds3 = BXF4.Read(Input.Read(ds3Bhd), Input.Read(Path.ChangeExtension(ds3Bhd, ".hkxbdt")));
            var er = BXF4.Read(Output.Read(erBhd), Output.Read(Path.ChangeExtension(erBhd, ".hkxbdt")));
            var compendiumFile = er.Files.FirstOrDefault(f => f.Name.Contains(".compendium"));
            var compendium = compendiumFile == null ? null : Havok.Files.HavokFile.ReadCompendium(Bytes(compendiumFile), Havok.HavokProfile.ER);

            var hkx = er.Files.Where(f => f.Name.Contains(".hkx.dcx") && f.Name.Split('\\').Length == 2).ToList();
            var sample = hkx.FirstOrDefault() ?? throw new InvalidOperationException($"{erBhd} has no collision file to copy the layout from");
            var folder = sample.Name.Split('\\')[0];
            DCX.Decompress(sample.Bytes, out DCX.Type compression);

            // The template: the first single-body compressed mesh, as the Collision Editor's Create tab picks it
            byte[] templateBytes = null;
            foreach (var f in hkx)
            {
                var b = Bytes(f);
                if (CollisionEditorShapes.IsTemplate(Havok.Files.HavokFile.Read(b, Havok.HavokProfile.ER, compendium))) { templateBytes = b; break; }
            }
            if (templateBytes == null) { Log.Warn($"{type}: no ER template collision in {erBhd}"); continue; }

            int built = 0, failed = 0;
            foreach (var model in ds3Models)
            {
                cancel.ThrowIfCancellationRequested();
                var id = model[1..];
                var ds3File = ds3.Files.FirstOrDefault(f => Path.GetFileName(f.Name.Replace('\\', '/')).Equals(ds3Naming.FileName(type, ds3Map, id), StringComparison.OrdinalIgnoreCase));
                if (ds3File == null) { if (type == "h") Log.Warn($"{model}: no {type} file in {ds3Bhd}"); continue; }

                var number = numbers.TryGetValue(model, out var known) ? known
                    : ModelNumber(target, model, n => hkx.Any(f => f.Name.EndsWith(erNaming.FileName("h", target, n), StringComparison.OrdinalIgnoreCase)), numbers.Values);
                numbers[model] = number;

                try
                {
                    var doc = Havok.Files.HavokFile.Read(Bytes(ds3File), Havok.HavokProfile.DS3);
                    var vertices = new List<Vector3>();
                    var materials = new List<uint>();
                    foreach (var body in CollisionGeometry.Bodies(doc.Objects))
                    {
                        if (body.Shape is not IHavokCompressedMesh mesh) continue;
                        var transform = CollisionGeometry.BodyTransform(body);
                        var byKey = CollisionEditorShapes.GetMaterialsByKey(body.Shape);
                        var shift = 32 - mesh.BitsPerKey;
                        foreach (var (key, a, b, c) in CollisionGeometry.CompressedTriangles(mesh))
                        {
                            vertices.Add(Vector3.Transform(a, transform));
                            vertices.Add(Vector3.Transform(b, transform));
                            vertices.Add(Vector3.Transform(c, transform));
                            var m = HitConfig.Map(byKey.TryGetValue(key >> shift, out var dm) ? dm : 0, out var row);
                            if (row) needsRow[(int)(dm % 100)] = needsRow.GetValueOrDefault((int)(dm % 100)) + 1;
                            materials.Add(m);
                        }
                    }
                    if (materials.Count == 0) { Log.Warn($"{model} {type}: no triangles"); continue; }

                    var template = Havok.Files.HavokFile.Read(templateBytes, Havok.HavokProfile.ER, compendium);
                    if (!CollisionEditorShapes.MakeNewCollision(template, number, vertices, Enumerable.Range(0, vertices.Count).ToList(), materials))
                    {
                        Log.Warn($"{model} {type}: the Collision Editor's builder refused the mesh");
                        failed++;
                        continue;
                    }

                    var name = $"{folder}\\{erNaming.FileName(type, target, number)}";
                    var data = DCX.Compress(Havok.Files.HavokFile.Write(template), compression);
                    var existing = er.Files.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (existing != null) existing.Bytes = data;
                    else er.Files.Add(new BinderFile(sample.Flags, er.Files.Max(f => f.ID) + 1, name, data) { CompressionType = sample.CompressionType });

                    // Read back: one body, the same triangle count and materials
                    var back = CollisionGeometry.Bodies(Havok.Files.HavokFile.Read(DCX.Decompress(data).ToArray(), Havok.HavokProfile.ER, compendium).Objects).ToList();
                    var backMesh = back.Count == 1 ? back[0].Shape as IHavokCompressedMesh : null;
                    Log.Check(backMesh != null && CollisionGeometry.CompressedTriangles(backMesh).Count() == materials.Count, $"{model} {type}: read back with {materials.Count} triangles");
                    built++;
                    if (type == "h")
                    {
                        builtH.Add(model);
                        CollisionBoxes[model] = (vertices.Aggregate(new Vector3(float.MaxValue), Vector3.Min), vertices.Aggregate(new Vector3(float.MinValue), Vector3.Max));
                    }
                }
                catch (Exception e)
                {
                    Log.Warn($"{model} {type}: {e.GetType().Name}: {e.Message}");
                    failed++;
                }
            }

            er.Write(out byte[] bhd, out byte[] bdt);
            Output.Write(erBhd, bhd);
            Output.Write(Path.ChangeExtension(erBhd, ".hkxbdt"), bdt);
            Log.Line($"{type}: {built} built, {failed} failed, into {erBhd}");
        }

        // A collision part needs its h file: models whose h mesh wasn't built get no part
        foreach (var model in numbers.Keys.Where(m => !builtH.Contains(m)).ToList())
        {
            numbers.Remove(model);
            Log.Warn($"{model}: no h collision built, so it gets no collision part");
        }

        foreach (var (surface, tris) in needsRow.OrderBy(n => n.Key))
            Log.Warn($"surface {surface} ({tris} triangles) needs a new HitMtrlParam row with disableFallDamage; mapped to {HitConfig.NeedsNewRow[surface]} for now");
    }

    private static byte[] Bytes(BinderFile f)
    {
        var b = f.Bytes.ToArray();
        return DCX.Is(b) ? DCX.Decompress(b).ToArray() : b;
    }

    // ---- MSB ----------------------------------------------------------------------------------------------------

    private void WriteMsb(MSB3 ds3Msb, MSBE vanilla, MSBE msb, string target, Dictionary<string, string> pieceNumbers, Dictionary<string, string> collisionNumbers,
        Dictionary<string, (Vector3 Min, Vector3 Max)> pieceBoxes, Dictionary<string, string> assets)
    {
        // Templates for the new parts and models, from the vanilla map (before any clear)
        var pieceTemplate = vanilla.Parts.MapPieces.FirstOrDefault() ?? throw new InvalidOperationException($"{target} has no vanilla map piece part");
        var pieceModelTemplate = vanilla.Models.MapPieces.First(m => m.Name == pieceTemplate.ModelName);
        var colTemplate = vanilla.Parts.Collisions.FirstOrDefault();
        var colModelTemplate = colTemplate == null ? null : vanilla.Models.Collisions.FirstOrDefault(m => m.Name == colTemplate.ModelName);

        // An earlier run's parts and models go first, so a re-run replaces them
        // (every model the manifest gave this map, so one an earlier run placed and this run doesn't is removed too)
        var ourModels = pieceNumbers.Values.Select(n => $"m{n}").Concat(collisionNumbers.Values.Select(n => $"h{n}")).Concat(assets.Values).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var portModels = Manifest.Models.Where(m => m.Key.StartsWith(target + "|", StringComparison.OrdinalIgnoreCase))
            .Select(m => $"{m.Key.Split('|')[^1][0]}{m.Value}")
            .Concat(Manifest.Models.Where(m => m.Key.StartsWith("asset|", StringComparison.OrdinalIgnoreCase)).Select(m => m.Value))
            .Concat(ourModels).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var removed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var earlier = RemoveParts(msb, p => portModels.Contains(p.ModelName ?? ""), removed);
        msb.Models.MapPieces.RemoveAll(m => portModels.Contains(m.Name));
        msb.Models.Collisions.RemoveAll(m => portModels.Contains(m.Name));
        msb.Models.Assets.RemoveAll(m => portModels.Contains(m.Name));
        if (earlier > 0) Log.Line($"Removed {earlier} parts of an earlier run");

        if (Request.ClearTarget)
        {
            var vanillaNames = vanilla.Parts.GetEntries().Select(p => p.Name).ToHashSet(StringComparer.Ordinal);
            var cleared = RemoveParts(msb, p => p is not MSBE.Part.Player && vanillaNames.Contains(p.Name), removed);
            Log.Line($"Cleared {cleared} vanilla parts (map pieces, collisions, connect collisions, assets, enemies, dummies); player starts kept");
        }

        var (events, references) = DropReferences(msb, removed);
        if (removed.Count > 0) Log.Line($"Removed {events} events that placed a removed part; cleared {references} references to removed parts");

        var names = msb.Parts.GetEntries().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        string PartName(string model)
        {
            var k = 0;
            while (names.Contains($"{model}_{k:0000}")) k++;
            names.Add($"{model}_{k:0000}");
            return $"{model}_{k:0000}";
        }

        // Map pieces
        int pieces = 0;
        foreach (var model in pieceNumbers.Values.Distinct())
        {
            var m = (MSBE.Model.MapPiece)pieceModelTemplate.DeepCopy();
            m.Name = $"m{model}";
            m.SibPath = Regex.Replace(pieceModelTemplate.SibPath ?? "", @"m\d{6}", m.Name);
            msb.Models.MapPieces.Add(m);
        }
        foreach (var ds3 in ds3Msb.Parts.MapPieces)
        {
            if (!pieceNumbers.TryGetValue(ds3.ModelName, out var number)) continue;
            var part = (MSBE.Part.MapPiece)pieceTemplate.DeepCopy();
            Place(part, $"m{number}", PartName($"m{number}"), ds3.Position, ds3.Rotation, ds3.Scale, pieceTemplate.SibPath);
            msb.Parts.MapPieces.Add(part);
            pieces++;
        }

        // Collisions
        int collisions = 0;
        if (colTemplate != null && collisionNumbers.Count > 0)
        {
            foreach (var model in collisionNumbers.Values.Distinct())
            {
                var m = (MSBE.Model.Collision)colModelTemplate.DeepCopy();
                m.Name = $"h{model}";
                m.SibPath = Regex.Replace(colModelTemplate.SibPath ?? "", @"h\d{6}", m.Name);
                msb.Models.Collisions.Add(m);
            }
            foreach (var ds3 in ds3Msb.Parts.Collisions)
            {
                if (!collisionNumbers.TryGetValue(ds3.ModelName, out var number)) continue;
                var part = (MSBE.Part.Collision)colTemplate.DeepCopy();
                Place(part, $"h{number}", PartName($"h{number}"), ds3.Position, ds3.Rotation, ds3.Scale, colTemplate.SibPath);
                msb.Parts.Collisions.Add(part);
                collisions++;
            }
        }
        else if (Request.Collision && collisionNumbers.Count > 0)
            Log.Warn($"{target} has no vanilla collision part to copy; no collision parts added");

        // Assets (§4.6): one per DS3 object of a ported model, from a vanilla asset part with no links of its own
        int assetParts = 0;
        var assetTemplate = vanilla.Parts.Assets.FirstOrDefault(a => a.EntityID <= 0) ?? vanilla.Parts.Assets.FirstOrDefault();
        if (assets.Count > 0 && assetTemplate == null)
            Log.Warn($"{target} has no vanilla asset part to copy; no asset parts added");
        else if (assets.Count > 0)
        {
            var assetModelTemplate = vanilla.Models.Assets.First(m => m.Name == assetTemplate.ModelName);
            var allNames = vanilla.Parts.GetEntries().Concat(msb.Parts.GetEntries()).Select(p => p.Name).Where(n => n != null).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var asset in assets.Values.Distinct())
            {
                var m = (MSBE.Model.Asset)assetModelTemplate.DeepCopy();
                m.Name = asset;
                m.SibPath = Regex.Replace(assetModelTemplate.SibPath ?? "", @"AEG\d{3}_\d{3}", asset, RegexOptions.IgnoreCase)
                    .Replace($@"\{assetTemplate.ModelName[..6]}\", $@"\{asset[..6]}\", StringComparison.OrdinalIgnoreCase);
                msb.Models.Assets.Add(m);
            }
            foreach (var ds3 in ds3Msb.Parts.Objects)
            {
                if (!assets.TryGetValue(ds3.ModelName, out var asset)) continue;
                var part = (MSBE.Part.Asset)assetTemplate.DeepCopy();
                var name = PartName(asset);
                Place(part, asset, name, ds3.Position, ds3.Rotation, ds3.Scale, null);
                part.SibPath = Regex.Replace(assetTemplate.SibPath ?? "", @"AEG\d{3}_\d{3}", asset, RegexOptions.IgnoreCase);
                ClearReferences(part, allNames, 0);
                msb.Parts.Assets.Add(part);
                assetParts++;
            }
        }

        // GI: one EnvMap box around the playable area, which the collision covers (map pieces include sky domes and
        // backdrops kilometres across); the map pieces when no collision was ported
        var fromCollision = CollisionBoxes.Count > 0;
        var placed = fromCollision
            ? ds3Msb.Parts.Collisions.Where(p => collisionNumbers.ContainsKey(p.ModelName) && CollisionBoxes.ContainsKey(p.ModelName))
                .Select(p => ($"h{collisionNumbers[p.ModelName]}", p.Position + Request.Offset, p.Rotation, p.Scale, CollisionBoxes[p.ModelName].Min, CollisionBoxes[p.ModelName].Max)).ToList()
            : ds3Msb.Parts.MapPieces.Where(p => pieceNumbers.ContainsKey(p.ModelName) && pieceBoxes.ContainsKey(p.ModelName))
                .Select(p => ($"m{pieceNumbers[p.ModelName]}", p.Position + Request.Offset, p.Rotation, p.Scale, pieceBoxes[p.ModelName].Min, pieceBoxes[p.ModelName].Max)).ToList();
        var outliers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var boundsModels = placed.Select(p => p.Item1).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (placed.Count > 0)
        {
            var (boundsMin, boundsMax) = PortBounds(placed, 30, MaxBoundsReach, outliers);
            Log.Line($"EnvMap bounds from the {(fromCollision ? "collision" : "map piece")} parts ({placed.Count})"
                + (outliers.Count > 0 ? $"; left out {outliers.Count} models over {MaxBoundsReach:0} m wide: {string.Join(", ", outliers.Take(10))}" : ""));
            boundsModels.ExceptWith(outliers);
            AddEnvMapBox(msb, vanilla, target, boundsMin, boundsMax);
            if (Request.LightShell)
                AddLightShell(msb, vanilla, target, boundsMin, boundsMax, PartName);
        }
        else if (Request.LightShell)
            Log.Warn("No ported parts to size the light-blocking shell by; none added");

        var path = $"/map/mapstudio/{target}.msb.dcx";
        Output.Write(path, msb.Write());
        Log.Line($"MSB: {pieces} map piece parts, {collisions} collision parts, {assetParts} asset parts added, at the DS3 positions + {Request.Offset}");

        // Read back
        var back = MSBE.Read(Output.Read(path));
        Log.Check(back.Parts.MapPieces.Count(p => ourModels.Contains(p.ModelName)) == pieces, "MSB: every ported map piece part");
        Log.Check(back.Parts.Collisions.Count(p => ourModels.Contains(p.ModelName)) == collisions, "MSB: every ported collision part");
        Log.Check(back.Parts.Assets.Count(p => ourModels.Contains(p.ModelName)) == assetParts, "MSB: every ported asset part");
        Log.Check(back.Models.Assets.Count(m => ourModels.Contains(m.Name)) == assets.Values.Distinct().Count(), "MSB: every ported asset model");
        Log.Check(back.Parts.GetEntries().Select(p => p.Name).Distinct().Count() == back.Parts.GetEntries().Count(), "MSB: part names unique");
        Log.Check(!back.Parts.GetEntries().Any(p => portModels.Contains(p.ModelName ?? "") && !ourModels.Contains(p.ModelName)), "MSB: no part left on a port model this run didn't write");
        Log.Check(back.Parts.Players.Count == vanilla.Parts.Players.Count || !Request.ClearTarget, "MSB: player starts kept");

        // Groups cleared on every ported part (no draw, display or alternate groups)
        var ported = back.Parts.MapPieces.Where(p => ourModels.Contains(p.ModelName)).Cast<MSBE.Part>()
            .Concat(back.Parts.Collisions.Where(p => ourModels.Contains(p.ModelName)))
            .Concat(back.Parts.Assets.Where(p => ourModels.Contains(p.ModelName))).ToList();
        Log.Check(ported.All(p => p.GetType().GetProperties().Select(x => x.GetValue(p)).OfType<MSBE.Part.DisplayDataStruct>()
            .All(d => d.UseAltGroups == 0 && d.DisplayGroups.All(g => g == 0) && d.DrawGroups.All(g => g == 0) && d.AltDisplayGroups.All(g => g == 0) && d.AltDrawGroups.All(g => g == 0))),
            "MSB: ported parts have no groups");

        // The EnvMap box: a 10-character Env_BoxNNN Box with its volume ID set, holding every ported part's position
        if (Manifest.Models.TryGetValue($"{target}|envbox".ToLowerInvariant(), out var boxName))
        {
            var box = back.Regions.EnvironmentMapEffectBoxes.FirstOrDefault(r => r.Name == boxName);
            if (Log.Check(box is { Shape: MSB.Shape.Box } && box.Name.Length == 10 && box.EnvMapNumber >= 0, $"MSB: EnvMap box {boxName} (Box shape, 10-character name, volume ID set)"))
            {
                var b = (MSB.Shape.Box)box.Shape;
                var lo = box.Position - new Vector3(b.Width / 2, 0, b.Depth / 2);
                var hi = box.Position + new Vector3(b.Width / 2, b.Height, b.Depth / 2);
                // Each part's model box centre, placed: a part's position says nothing of where its geometry is (DS3
                // collision parts sit at the origin with world-space meshes, outside m40's box at offset 0)
                var outside = placed.Where(p => boundsModels.Contains(p.Item1)).Select(p =>
                {
                    var world = new Renderer.Transform(p.Item2, p.Item3 * (MathF.PI / 180), p.Item4).WorldMatrix;
                    return Vector3.Transform((p.Item5 + p.Item6) / 2, world);
                }).Count(c => c.X < lo.X || c.Y < lo.Y || c.Z < lo.Z || c.X > hi.X || c.Y > hi.Y || c.Z > hi.Z);
                Log.Check(outside == 0, $"MSB: the EnvMap box holds every part its bounds were taken from ({outside} outside)");
            }
        }
    }

    /// <summary>A ported part: model, name, the DS3 transform plus the offset, and no groups (§4.4).</summary>
    private void Place(MSBE.Part part, string model, string name, Vector3 position, Vector3 rotation, Vector3 scale, string sibTemplate)
    {
        part.Name = name;
        part.ModelName = model;
        part.SibPath = Regex.Replace(sibTemplate ?? "", @"[mh]\d{6}", model);
        part.EntityID = 0;
        part.Position = position + Request.Offset;
        part.Rotation = rotation;
        part.Scale = scale;
        ClearGroups(part);
    }

    /// <summary>
    /// ER draws a part with no draw groups everywhere (95% of ER's assets have none; only the legacy dungeons cull by
    /// group), and DS3's group bits mean nothing in an ER map. So every group array is cleared, and the alternate
    /// groups a vanilla template carries (UseAltGroups swaps or merges them in on load, MSBE Exe Analysis §1) are
    /// switched off, as is the unrelated DisplayGroupStruct. All-bits groups did not work: in game the template's
    /// alternate groups merged over them and many pieces didn't draw.
    /// </summary>
    private static void ClearGroups(MSBE.Part part)
    {
        foreach (var prop in part.GetType().GetProperties())
        {
            if (prop.GetValue(part) is MSBE.Part.DisplayDataStruct display)
            {
                foreach (var groups in new[] { display.DisplayGroups, display.AltDisplayGroups, display.AltDisplayGroupsMask,
                             display.DrawGroups, display.AltDrawGroups, display.AltDrawGroupsMask })
                    if (groups != null) Array.Clear(groups);
                display.UseAltGroups = 0;
                display.NoAltGroupMerge = 0;
            }
            else if (prop.GetValue(part) is MSBE.Part.DisplayGroupStruct displayGroup)
            {
                displayGroup.Condition = 0;
                if (displayGroup.DispGroups != null) Array.Clear(displayGroup.DispGroups);
            }
        }
    }

    /// <summary>
    /// An EnvMapEffectBox around the whole ported map (§4.4), so ER's GI applies to it: a copy of the target map's
    /// largest vanilla box (usually the coarse whole-map one), with that box's volume ID set as a property, under a
    /// free 10-character Env_BoxNNN name (the game skips any other name length or a non-Box shape). The region
    /// position is the box's bottom centre.
    /// </summary>
    private void AddEnvMapBox(MSBE msb, MSBE vanilla, string target, Vector3 min, Vector3 max)
    {
        var key = $"{target}|envbox".ToLowerInvariant();
        if (Manifest.Models.TryGetValue(key, out var earlierName))
            msb.Regions.EnvironmentMapEffectBoxes.RemoveAll(r => r.Name == earlierName);

        var template = vanilla.Regions.EnvironmentMapEffectBoxes
            .Where(r => r.Shape is MSB.Shape.Box && r.Name?.Length == 10)
            .OrderByDescending(r => r.Shape is MSB.Shape.Box b ? (double)b.Width * b.Depth * b.Height : 0)
            .FirstOrDefault();
        if (template == null) { Log.Warn($"{target} has no vanilla EnvMapEffectBox to copy; the ported map gets no GI"); return; }

        var templateId = template.EnvMapNumber >= 0 ? template.EnvMapNumber : int.Parse(template.Name[7..]);

        var names = msb.Regions.GetEntries().Select(r => r.Name).ToHashSet(StringComparer.Ordinal);
        var name = earlierName != null && !names.Contains(earlierName) ? earlierName : null;
        for (int n = 999; name == null && n >= 0; n--)
            if (!names.Contains($"Env_Box{n:000}")) name = $"Env_Box{n:000}";
        if (name == null) { Log.Warn($"{target}: no free Env_BoxNNN name; the ported map gets no GI"); return; }

        var box = (MSBE.Region.EnvironmentMapEffectBox)template.DeepCopy();
        box.Name = name;
        box.EnvMapNumber = (short)templateId;
        var size = max - min;
        box.Shape = new MSB.Shape.Box(size.X, size.Z, size.Y);
        box.Position = new Vector3((min.X + max.X) / 2, min.Y, (min.Z + max.Z) / 2);
        box.Rotation = Vector3.Zero;
        msb.Regions.EnvironmentMapEffectBoxes.Add(box);
        Manifest.Models[key] = name;

        Log.Line($"EnvMap box {name}: GI volume {templateId} of {template.Name} ({((MSB.Shape.Box)template.Shape).Width:0}x{((MSB.Shape.Box)template.Shape).Height:0}x{((MSB.Shape.Box)template.Shape).Depth:0}), " +
            $"over {size.X:0} x {size.Y:0} x {size.Z:0} m at {box.Position}");
    }

    /// <summary>The ER catacomb whose shell the light-blocking box copies (map piece m000000, part m000000_1000).</summary>
    private const string ShellTemplateMap = "m30_00_00_00";

    /// <summary>
    /// A light-blocking shell around the ported map, as ER's catacombs have one (Smithbox Port Plan.md §4.9): their
    /// map piece m000000 is a black (m39_12_001, c80_SYSTEX_BLACK), double-sided hull round the dungeon, placed as a
    /// shadow-only part (IsShadowOnly, PartsDrawParamID 2044), so it isn't drawn but keeps the sun out. Here: a closed
    /// box over the port's bounds, built on that piece's FLVER (material, GX list, layout) and placed with a copy of its
    /// part (groups cleared). The model number is kept per target by the manifest, so a re-run replaces it.
    /// </summary>
    private void AddLightShell(MSBE msb, MSBE vanilla, string target, Vector3 min, Vector3 max, Func<string, string> partName)
    {
        var templateBinderPath = $"/map/{ShellTemplateMap[..3]}/{ShellTemplateMap}/{ShellTemplateMap}_000000.mapbnd.dcx";
        var templateBytes = Output.ReadVanilla(templateBinderPath);
        var templateMsbBytes = Output.ReadVanilla($"/map/mapstudio/{ShellTemplateMap}.msb.dcx");
        if (templateBytes == null || templateMsbBytes == null) { Log.Warn($"{templateBinderPath} or its MSB not found in the ER game; no light-blocking shell"); return; }
        var templateMsb = MSBE.Read(templateMsbBytes);
        var templatePart = templateMsb.Parts.MapPieces.FirstOrDefault(p => p.ModelName == "m000000");
        var templateModel = templateMsb.Models.MapPieces.FirstOrDefault(m => m.Name == "m000000");
        if (templatePart == null || templateModel == null) { Log.Warn($"{ShellTemplateMap} has no m000000 part; no light-blocking shell"); return; }

        // The model number: the manifest's for this target, else the next free one from 900000
        var key = $"{target}|mlightshell".ToLowerInvariant();
        var folder = Output.MapFolder(target);
        if (!Manifest.Models.TryGetValue(key, out var number))
        {
            var taken = Manifest.Models.Where(m => m.Key.StartsWith(target + "|", StringComparison.OrdinalIgnoreCase)).Select(m => m.Value).ToHashSet();
            var n = 900000;
            while (taken.Contains($"{n:000000}") || Output.Exists($"{folder}{target}_{n:000000}.mapbnd.dcx")) n++;
            Manifest.Models[key] = number = $"{n:000000}";
        }
        var stem = $"{target}_{number}";

        // The FLVER: the template's, its one mesh rebuilt as a box (24 vertices, 12 triangles, both sides drawn)
        var bnd = BND4.Read(templateBytes);
        var file = bnd.Files.First(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase));
        var flver = FLVER2.Read(file.Bytes);
        var mesh = flver.Meshes[0];
        flver.Meshes = [mesh];
        var proto = mesh.Vertices[0];
        mesh.Vertices = new List<FLVER.Vertex>();
        var indices = new List<int>();
        var corners = Enumerable.Range(0, 8).Select(c => new Vector3((c & 1) == 0 ? min.X : max.X, (c & 2) == 0 ? min.Y : max.Y, (c & 4) == 0 ? min.Z : max.Z)).ToArray();
        int[][] faces = [[0, 2, 6, 4], [1, 5, 7, 3], [0, 4, 5, 1], [2, 3, 7, 6], [0, 1, 3, 2], [4, 6, 7, 5]];
        Vector3[] inward = [Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY, -Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitZ];
        for (int f = 0; f < 6; f++)
        {
            var first = mesh.Vertices.Count;
            foreach (var c in faces[f])
                mesh.Vertices.Add(new FLVER.Vertex(proto) { Position = corners[c], Normal = inward[f] });
            indices.AddRange([first, first + 1, first + 2, first, first + 2, first + 3]);
        }
        var faceSet = mesh.FaceSets[0];
        faceSet.Flags = FLVER2.FaceSet.FSFlags.None;
        faceSet.TriangleStrip = false;
        faceSet.CullBackfaces = false;
        faceSet.Indices = indices;
        mesh.FaceSets = [faceSet];
        mesh.BoundingBox = new FLVER2.Mesh.BoundingBoxes { Unk = (min + max) / 2, Min = (max - min) / 2, Max = Vector3.Zero };
        flver.Header.BoundingBoxMin = min;
        flver.Header.BoundingBoxMax = max;
        foreach (var node in flver.Nodes)
        {
            node.Name = node.Name.Replace($"{ShellTemplateMap}_000000", stem, StringComparison.OrdinalIgnoreCase).Replace("m000000", $"m{number}", StringComparison.OrdinalIgnoreCase);
            node.BoundingBoxMin = min;
            node.BoundingBoxMax = max;
        }
        file.Name = file.Name.Replace($"{ShellTemplateMap}_000000", stem, StringComparison.OrdinalIgnoreCase).Replace(ShellTemplateMap, target, StringComparison.OrdinalIgnoreCase);
        file.Bytes = flver.Write();
        bnd.Files = [file];
        var path = $"{folder}{stem}.mapbnd.dcx";
        Output.Write(path, bnd.Write());

        // The model and the part: copies of the catacomb's, the part at the origin (the box is in world space)
        var model = (MSBE.Model.MapPiece)templateModel.DeepCopy();
        model.Name = $"m{number}";
        model.SibPath = Regex.Replace(templateModel.SibPath ?? "", @"m\d{6}", model.Name).Replace(ShellTemplateMap, target, StringComparison.OrdinalIgnoreCase);
        msb.Models.MapPieces.Add(model);
        var part = (MSBE.Part.MapPiece)templatePart.DeepCopy();
        part.Name = partName(model.Name);
        part.ModelName = model.Name;
        part.SibPath = (templatePart.SibPath ?? "").Replace(ShellTemplateMap, target, StringComparison.OrdinalIgnoreCase);
        part.EntityID = 0;
        part.Position = Vector3.Zero;
        part.Rotation = Vector3.Zero;
        part.Scale = Vector3.One;
        ClearGroups(part);
        msb.Parts.MapPieces.Add(part);
        var size = max - min;
        Log.Line($"Light-blocking shell {part.Name}: a shadow-only box of {size.X:0} x {size.Y:0} x {size.Z:0} m (from {ShellTemplateMap}'s m000000: PartsDrawParamID {part.PartsDrawParamID}, IsShadowOnly {part.IsShadowOnly})");

        // Read back: one double-sided mesh of 12 triangles
        var back = FLVER2.Read(BND4.Read(Output.Read(path)).Files[0].Bytes);
        Log.Check(back.Meshes.Count == 1 && back.Meshes[0].FaceSets[0].Indices.Count == 36 && !back.Meshes[0].FaceSets[0].CullBackfaces, "Light-blocking shell: one double-sided box mesh");
    }

    /// <summary>
    /// Models wider than this (their box's largest side, scaled) are left out of the EnvMap bounds: a DS3 map places
    /// its sky dome and distant backdrops as map pieces kilometres across (m30's made the first box 29 km a side).
    /// </summary>
    private const float MaxBoundsReach = 2000;

    /// <summary>
    /// The world box of the ported parts: each model's box corners through its part's scale, rotation (MSB Euler
    /// degrees, placed as Smithbox's renderer does: Transform.EulerRotation) and position; plus a margin. The DS3
    /// collision parts sit at the origin with world-space meshes, so a box per part (not a sphere about the part's
    /// position) is what keeps this to the map's extent.
    /// </summary>
    private static (Vector3 Min, Vector3 Max) PortBounds(IEnumerable<(string Model, Vector3 Position, Vector3 Rotation, Vector3 Scale, Vector3 BoxMin, Vector3 BoxMax)> parts,
        float margin, float maxSize, HashSet<string> outliers)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        foreach (var (model, position, rotation, scale, boxMin, boxMax) in parts)
        {
            var size = (boxMax - boxMin) * Vector3.Abs(scale);
            if (MathF.Max(size.X, MathF.Max(size.Y, size.Z)) > maxSize) { outliers.Add(model); continue; }

            var world = new Renderer.Transform(position, rotation * (MathF.PI / 180), scale).WorldMatrix;
            for (int c = 0; c < 8; c++)
            {
                var corner = new Vector3((c & 1) == 0 ? boxMin.X : boxMax.X, (c & 2) == 0 ? boxMin.Y : boxMax.Y, (c & 4) == 0 ? boxMin.Z : boxMax.Z);
                var p = Vector3.Transform(corner, world);
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
        }
        return (min - new Vector3(margin), max + new Vector3(margin));
    }

    /// <summary>Removes the parts of the cleared kinds that match, noting their names.</summary>
    private static int RemoveParts(MSBE msb, Func<MSBE.Part, bool> match, HashSet<string> removed)
    {
        int count = 0;
        int Remove<T>(List<T> list) where T : MSBE.Part
        {
            foreach (var p in list.Where(p => match(p))) removed.Add(p.Name);
            return list.RemoveAll(p => match(p));
        }
        count += Remove(msb.Parts.MapPieces);
        count += Remove(msb.Parts.Collisions);
        count += Remove(msb.Parts.ConnectCollisions);
        count += Remove(msb.Parts.Assets);
        count += Remove(msb.Parts.Enemies);
        count += Remove(msb.Parts.DummyAssets);
        count += Remove(msb.Parts.DummyEnemies);
        return count;
    }

    /// <summary>
    /// The MSB writer resolves every part name and throws on a missing one, so: events whose own part is gone are
    /// removed, and every other part reference on parts, regions and events to a removed part is cleared. The MSBE
    /// properties resolved against the parts: ...PartName, CollisionName, ...PartNames, GroupPartsNames (platoons),
    /// LinkedAssetNames.
    /// </summary>
    private static readonly string[] SinglePartReferences = ["PartName", "CollisionName"];
    private static readonly string[] PartListReferences = ["PartNames", "PartsNames", "LinkedAssetNames"];

    private static (int Events, int References) DropReferences(MSBE msb, HashSet<string> removed)
    {
        if (removed.Count == 0) return (0, 0);

        int events = 0;
        foreach (var list in EntryLists(msb.Events))
        {
            var items = list.Cast<MSBE.Event>().ToList();
            foreach (var e in items.Where(e => e.PartName != null && removed.Contains(e.PartName)))
            {
                list.Remove(e);
                events++;
            }
        }

        int references = 0;
        foreach (var entry in msb.Parts.GetEntries().Cast<object>().Concat(msb.Regions.GetEntries()).Concat(msb.Events.GetEntries()))
            references += ClearReferences(entry, removed, 0);
        return (events, references);
    }

    private static int ClearReferences(object obj, HashSet<string> removed, int depth)
    {
        if (obj == null || depth > 3) return 0;
        int count = 0;
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0) continue;
            if (prop.PropertyType == typeof(string) && prop.CanWrite && SinglePartReferences.Any(r => prop.Name.EndsWith(r)))
            {
                if (prop.GetValue(obj) is string s && removed.Contains(s)) { prop.SetValue(obj, null); count++; }
            }
            else if (prop.PropertyType == typeof(string[]) && PartListReferences.Any(r => prop.Name.EndsWith(r)))
            {
                if (prop.GetValue(obj) is string[] a)
                    for (int i = 0; i < a.Length; i++)
                        if (a[i] != null && removed.Contains(a[i])) { a[i] = null; count++; }
            }
            else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)
                     && prop.PropertyType.Namespace == "SoulsFormats")
            {
                count += ClearReferences(prop.GetValue(obj), removed, depth + 1);
            }
        }
        return count;
    }

    /// <summary>The typed lists of an MSB param (its List properties).</summary>
    private static IEnumerable<IList> EntryLists(object param) =>
        param.GetType().GetProperties().Where(p => typeof(IList).IsAssignableFrom(p.PropertyType)).Select(p => (IList)p.GetValue(param)).Where(l => l != null);

    // ---- manifest and report ------------------------------------------------------------------------------------

    private void SaveManifestAndReport()
    {
        try
        {
            if (Manifest != null && ManifestPath != null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ManifestPath));
                File.WriteAllText(ManifestPath, JsonSerializer.Serialize(Manifest, PortJsonContext.Default.PortManifest));
            }

            if (Request?.Output != null && Directory.Exists(Request.Output.ProjectPath))
            {
                var folder = Path.Combine(Request.Output.ProjectPath, ".smithbox", "port");
                Directory.CreateDirectory(folder);
                ReportPath = Path.Combine(folder, $"DS3_ER {Request.Ds3Map} to {Request.TargetMap} {DateTime.Now:yyyy-MM-dd HH-mm}.txt");
                Log.Line();
                Log.Line($"{Log.Failures} failed checks, {Log.Warnings} warnings");
                File.WriteAllLines(ReportPath, Log.Snapshot());
            }
        }
        catch (Exception e)
        {
            Log.Warn($"Could not save the manifest or report: {e.Message}");
        }
    }
}
