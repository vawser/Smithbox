using SoulsFormats;
using StudioCore.Editors.MaterialEditor;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

/// <summary>
/// usage GAME: times a Used By scan (FlverMaterialNames over every model container) and checks it against full
/// FLVER reads on a sample. Containers are the model binders and loose FLVERs under /chr, /parts, /asset, /map,
/// /obj and DS2's /model.
/// </summary>
public static class UsageSurvey
{
    private static readonly string[] Folders = ["/chr", "/parts", "/asset", "/map", "/obj", "/model"];
    private static readonly string[] Extensions = ["chrbnd", "partsbnd", "geombnd", "mapbnd", "objbnd", "flver", "bnd", "flverbnd"];

    public static void Run(GameMaterials g, string root)
    {
        Console.WriteLine("== usage (Used By scan)");

        var dictName = g.Name == "DB_NT" ? "DB-NT" : g.Name;
        var json = JsonDocument.Parse(File.ReadAllText(Path.Combine("Assets", "File Dictionaries", $"{dictName}-File-Dictionary.json")));

        var paths = json.RootElement.GetProperty("Entries").EnumerateArray()
            .Where(e => !(e.GetProperty("Archive").GetString() ?? "").Contains("sd"))
            .Select(e => (path: e.GetProperty("Path").GetString(), folder: e.GetProperty("Folder").GetString() ?? "", ext: e.GetProperty("Extension").GetString() ?? ""))
            .Where(e => Folders.Any(f => e.folder.StartsWith(f, StringComparison.OrdinalIgnoreCase)) && Extensions.Contains(e.ext))
            .Select(e => e.path).Distinct().ToList();

        var timer = Stopwatch.StartNew();
        var flvers = 0;
        var failed = 0;
        var bytes = 0L;
        var uses = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var sample = new ConcurrentBag<(Memory<byte> data, List<string> names)>();

        Parallel.ForEach(paths, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, path =>
        {
            var file = g.Fs.FileExists(path) ? g.Fs.ReadFile(path) : null;
            if (file == null) return;

            Interlocked.Add(ref bytes, file.Value.Length);

            foreach (var (_, data) in Flvers(file.Value))
            {
                var names = FlverMaterialNames.Read(data);
                if (names == null) { Interlocked.Increment(ref failed); continue; }

                Interlocked.Increment(ref flvers);
                foreach (var n in names.Distinct())
                    uses.AddOrUpdate(GameMaterials.ShortName(n), 1, (_, c) => c + 1);

                if (sample.Count < 200 && Random.Shared.Next(50) == 0)
                    sample.Add((data, names));
            }
        });

        Console.WriteLine($"  {paths.Count} containers ({bytes / 1048576} MB as stored), {flvers} FLVERs, {failed} unreadable, {timer.Elapsed.TotalSeconds:0.0} s");
        Console.WriteLine($"  {uses.Count} distinct material names used");

        // Against full reads
        var agree = 0;
        foreach (var (data, names) in sample)
        {
            try
            {
                var raw = DCX.Is(data) ? DCX.Decompress(data) : data;
                var arr = raw.ToArray();
                IFlver flver = FLVER2.IsRead(arr, out var f2) ? f2 : FLVER0.Read(arr);
                if (flver.Materials.Select(m => m.MTD).SequenceEqual(names)) agree++;
            }
            catch { }
        }
        Console.WriteLine($"  sample: {agree}/{sample.Count} match a full FLVER read");

        // How the bank's materials are used
        var materialNames = g.Binders.Where(b => b.Selected)
            .SelectMany(b => GameMaterials.Files(b).Select(x => GameMaterials.ShortName(x.file.Name))).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var used = materialNames.Count(n => uses.ContainsKey(n));
        var unknown = uses.Keys.Count(k => !materialNames.Contains(k, StringComparer.OrdinalIgnoreCase));
        Console.WriteLine($"  of {materialNames.Count} materials in the binders, {used} are used by a model; {unknown} names models use have no material file");
        Console.WriteLine($"  most used: {string.Join(", ", uses.OrderByDescending(kv => kv.Value).Take(5).Select(kv => $"{kv.Key} x{kv.Value}"))}");
    }

    /// <summary>
    /// The FLVERs in a container: itself if it is one, else the FLVER files of the binder (one level of nesting)
    /// </summary>
    private static IEnumerable<(string name, Memory<byte> data)> Flvers(Memory<byte> data, int depth = 0)
    {
        if (DCX.Is(data))
        {
            Memory<byte> decompressed;
            try { decompressed = DCX.Decompress(data); } catch { yield break; }
            data = decompressed;
        }

        if (FlverMaterialNames.IsFlver(data.Span))
        {
            yield return ("", data);
            yield break;
        }

        IBinder binder = null;
        try
        {
            var magic = data.Length >= 4 ? System.Text.Encoding.ASCII.GetString(data.Span[..4]) : "";
            if (magic == "BND4") binder = BND4.Read(data);
            else if (magic == "BND3") binder = BND3.Read(data);
        }
        catch { }

        if (binder == null) yield break;

        foreach (var f in binder.Files)
        {
            var name = f.Name ?? "";
            if (name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".flver.dcx", StringComparison.OrdinalIgnoreCase))
                yield return (name, f.Bytes);
            else if (depth == 0 && (name.EndsWith("bnd", StringComparison.OrdinalIgnoreCase) || name.EndsWith("bnd.dcx", StringComparison.OrdinalIgnoreCase)))
                foreach (var inner in Flvers(f.Bytes, 1))
                    yield return inner;
        }
    }
}
