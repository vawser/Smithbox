using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Renderer;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

// MaterialSurvey <ER|NR|AC6> <outDir> [modelRegex] [maxBinders]
//
// Material Fidelity Phase 1, the census. For every FLVER in the game's map pieces, assets, characters and parts:
// which MATBIN each mesh uses, and from that which shaders, shader tokens and sampler roles the game needs,
// weighted by meshes, triangles and by how often the MSBs place each model.
//
// Run with the working directory at src/Smithbox.Data, so the VFS dictionaries and the shader tables are found.

if (args.Length < 2)
{
    Console.WriteLine("MaterialSurvey <ER|NR|AC6> <outDir> [modelRegex] [maxBinders]");
    return;
}

var gameName = args[0];
var outDir = args[1];
var modelFilter = new Regex(args.Length > 2 ? args[2] : ".", RegexOptions.IgnoreCase);
var maxBinders = args.Length > 3 ? int.Parse(args[3]) : int.MaxValue;
var game = Enum.Parse<Game>(gameName);
Directory.CreateDirectory(outDir);

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2)).Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());
var root = gamePaths[gameName];

List<VirtualFileSystem> layers = [];
try { layers.Add(ArchiveBinderVirtualFileSystem.FromGameFolder(root, game)); }
catch (Exception e) { Console.WriteLine($"No archives ({e.GetType().Name}), reading loose files only"); }
layers.Add(new RealVirtualFileSystem(root, false));
using var fs = new CompundVirtualFileSystem(layers);

var dictName = game == Game.DB_NT ? "DB-NT" : gameName;
var dict = File.ReadAllText(Path.Combine("Assets", "File Dictionaries", $"{dictName}-File-Dictionary.json"));
var table = Census.LoadShaderTable(gameName);
var start = DateTime.Now;

// ---- Materials --------------------------------------------------------------------------------------------------
// Base binders first so the devpatch ones replace their entries, as the game loads them
var matbinBinders = Regex.Matches(dict, @"""(/material/[^""]+\.matbinbnd(?:\.devpatch)?\.dcx)""")
    .Select(m => m.Groups[1].Value).Distinct().OrderBy(p => p.Contains("devpatch") ? 1 : 0).ThenBy(p => p, StringComparer.Ordinal).ToList();

Dictionary<string, MaterialDefinition> definitions = new(StringComparer.OrdinalIgnoreCase);
Dictionary<string, string> matbinSource = new(StringComparer.OrdinalIgnoreCase);
foreach (var binderPath in matbinBinders)
{
    var file = fs.ReadFile(binderPath);
    if (file == null) continue;
    var bnd = BND4.Read(file.Value);
    var count = 0;
    foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".matbin", StringComparison.OrdinalIgnoreCase)))
    {
        var key = Census.Key(f.Name);
        try
        {
            definitions[key] = MaterialDefinition.FromMatbin(key, MATBIN.Read(f.Bytes), table);
            matbinSource[key] = Path.GetFileName(binderPath);
            count++;
        }
        catch (Exception e) { Console.WriteLine($"MATBIN {f.Name}: {e.Message}"); }
    }
    Console.WriteLine($"{binderPath}: {count} MATBINs");
}
Console.WriteLine($"{definitions.Count} materials");

// ---- Placements -------------------------------------------------------------------------------------------------
// How many times the MSBs place each model, keyed as the model binder is named
var placements = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
var placedMaps = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
var msbPaths = Regex.Matches(dict, @"""(/map/mapstudio/(m\d\d_\d\d_\d\d_\d\d)\.msb\.dcx)""")
    .Select(m => (path: m.Groups[1].Value, map: m.Groups[2].Value)).Distinct().ToList();
int msbRead = 0, msbFailed = 0;
foreach (var (path, mapId) in msbPaths)
{
    var file = fs.ReadFile(path);
    if (file == null) continue;
    try
    {
        IMsb msb = game switch
        {
            Game.ER => MSBE.Read(file.Value),
            Game.NR => MSB_NR.Read(file.Value),
            Game.AC6 => MSB_AC6.Read(file.Value),
            _ => throw new NotSupportedException(gameName)
        };
        foreach (var part in msb.Parts.GetEntries())
        {
            var model = part.ModelName;
            if (string.IsNullOrEmpty(model)) continue;
            string key = null;
            if (model[0] is 'm' or 'M') key = $"{mapId}_{model[1..]}";
            else if (model.StartsWith("AEG", StringComparison.OrdinalIgnoreCase) || model[0] is 'c' or 'C') key = model;
            if (key == null) continue;
            placements[key] = placements.GetValueOrDefault(key) + 1;
            if (!placedMaps.TryGetValue(key, out var maps)) placedMaps[key] = maps = new(StringComparer.OrdinalIgnoreCase);
            maps.Add(mapId);
        }
        msbRead++;
    }
    catch (Exception e)
    {
        if (msbFailed++ < 5) Console.WriteLine($"MSB {path}: {e.Message}");
    }
}
Console.WriteLine($"{msbRead} MSBs read, {msbFailed} failed, {placements.Count} placed models");

// ---- Models -----------------------------------------------------------------------------------------------------
var binders = Regex.Matches(dict, @"""(/(map|asset|chr|parts)/[^""]+\.(?:mapbnd|geombnd|chrbnd|partsbnd)\.dcx)""")
    .Select(m => (path: m.Groups[1].Value, category: m.Groups[2].Value)).Distinct()
    .Where(b => modelFilter.IsMatch(b.path)).Take(maxBinders).ToList();
Console.WriteLine($"{binders.Count} model binders");

var meshes = new ConcurrentBag<MeshUse>();
var failures = new ConcurrentBag<string>();
int missing = 0, readCount = 0;

// The VFS is read on this thread, and the binders are parsed in parallel
var work = new System.Collections.Concurrent.BlockingCollection<(string path, string category, Memory<byte> data)>(64);
var parse = Task.Run(() => Parallel.ForEach(work.GetConsumingEnumerable(), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, item =>
{
    try
    {
        var modelKey = Census.Key(item.path);
        var placed = placements.GetValueOrDefault(modelKey);
        var bnd = BND4.Read(item.data);
        foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)))
        {
            var flverKey = Census.Key(f.Name);
            // Only the model the MSB names takes its placements: _1 and the like are destroyed or alternate versions
            var main = flverKey.Equals(modelKey, StringComparison.OrdinalIgnoreCase);
            FLVER2 flver;
            try { flver = FLVER2.Read(f.Bytes); }
            catch (Exception e) { failures.Add($"{item.path} {f.Name}: {e.Message}"); continue; }

            foreach (var mesh in flver.Meshes)
            {
                if (mesh.MaterialIndex < 0 || mesh.MaterialIndex >= flver.Materials.Count) continue;
                var material = flver.Materials[mesh.MaterialIndex];
                var faceSet = mesh.FaceSets.FirstOrDefault(fsx => fsx.Flags == FLVER2.FaceSet.FSFlags.None) ?? mesh.FaceSets.FirstOrDefault();
                var triangles = faceSet == null ? 0 : faceSet.TriangleStrip ? Math.Max(0, faceSet.Indices.Count - 2) : faceSet.Indices.Count / 3;
                meshes.Add(new MeshUse(
                    item.category, modelKey, flverKey, Census.Key(material.MTD), triangles, main ? placed : 0,
                    material.Textures.Select(t => (t.ParamName ?? "", t.Path ?? "")).ToArray()));
            }
        }
    }
    catch (Exception e) { failures.Add($"{item.path}: {e.Message}"); }
}));

foreach (var (path, category) in binders)
{
    var file = fs.ReadFile(path);
    if (file == null) { missing++; continue; }
    work.Add((path, category, file.Value));
    if (++readCount % 2000 == 0) Console.WriteLine($"  {readCount} binders read, {(DateTime.Now - start).TotalSeconds:F0}s");
}
work.CompleteAdding();
parse.Wait();
Console.WriteLine($"{readCount} binders read, {missing} missing, {failures.Count} failures, {meshes.Count} meshes");

// ---- Report -----------------------------------------------------------------------------------------------------
var report = new Census(gameName, definitions, matbinSource, meshes.ToList(), placedMaps);
report.Write(outDir, failures.ToList());
Console.WriteLine($"Done in {(DateTime.Now - start).TotalSeconds:F0}s, written to {outDir}");

record MeshUse(string Category, string Model, string Flver, string Material, int Triangles, int Placements,
    (string Type, string Path)[] Textures)
{
    public long PlacedTriangles => (long)Triangles * Placements;
}
