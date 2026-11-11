using SoulsFormats;
using StudioCore.Editors.MaterialEditor;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

/// <summary>
/// textures GAME: what a texture index for the Material Editor would take and cover.
///  - the containers: the locator's texture files (tpf, texbnd, ffxbnd, commonbnd, objbnd, partsbnd, DS2 bnd) and
///    packed ones (tpfbhd), plus, for comparison, chrbnd / geombnd / mapbnd that might hold textures too
///  - TpfTextureNames against full TPF reads on a sample
///  - whether a tpfbhd's inner file names are its texture names (then the .tpfbdt needn't be read)
///  - how many texture names the materials' paths give are found
/// </summary>
public static class TextureSurvey
{
    private record Container(string Path, string Ext, bool Locator);

    public static void Run(GameMaterials g)
    {
        Console.WriteLine("== textures (texture index)");

        var dictName = g.Name == "DB_NT" ? "DB-NT" : g.Name;
        var json = JsonDocument.Parse(File.ReadAllText(Path.Combine("Assets", "File Dictionaries", $"{dictName}-File-Dictionary.json")));
        var ds2 = g.Name is "DS2" or "DS2S";

        var containers = json.RootElement.GetProperty("Entries").EnumerateArray()
            .Where(e => !(e.GetProperty("Archive").GetString() ?? "").Contains("sd"))
            .Select(e => (path: e.GetProperty("Path").GetString(), folder: e.GetProperty("Folder").GetString() ?? "", ext: e.GetProperty("Extension").GetString() ?? ""))
            .Select(e =>
            {
                var locator = e.ext is "tpf" or "texbnd" or "ffxbnd" or "commonbnd" or "objbnd" or "partsbnd" or "tpfbhd" ||
                    (ds2 && e.ext == "bnd" && (e.folder == "/model/obj" || e.folder.Contains("/model/parts")));
                var extra = e.ext is "chrbnd" or "geombnd" or "mapbnd";
                return locator || extra ? new Container(e.path, e.ext, locator) : null;
            })
            .Where(c => c != null)
            .GroupBy(c => c.Path).Select(gr => gr.First()).ToList();

        var index = new ConcurrentDictionary<string, ConcurrentBag<string>>(StringComparer.OrdinalIgnoreCase);
        var extraIndex = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        var tpfs = 0;
        var failed = 0;
        var bytes = 0L;
        var sample = new ConcurrentBag<(Memory<byte> data, List<string> names)>();
        var bhdChecked = 0;
        var bhdNamesMatch = 0;
        var bhdExamples = new ConcurrentBag<string>();
        var timer = Stopwatch.StartNew();

        Parallel.ForEach(containers, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, c =>
        {
            if (c.Ext == "tpfbhd")
            {
                var bhdData = g.Fs.FileExists(c.Path) ? g.Fs.ReadFile(c.Path) : null;
                if (bhdData == null) return;

                var bhd = TpfTextureNames.PackedFileNames(bhdData.Value) ?? new List<string>();
                var stems = bhd.Select(n => Stem(n)).ToList();

                // The header's names as texture names
                foreach (var stem in stems)
                    index.GetOrAdd(stem, _ => new()).Add(c.Path);

                // Check against the real names on a few
                if (Interlocked.Increment(ref bhdChecked) <= 30)
                {
                    var bdtPath = c.Path[..^"tpfbhd".Length] + "tpfbdt";
                    var bdtData = g.Fs.FileExists(bdtPath) ? g.Fs.ReadFile(bdtPath) : null;
                    if (bdtData != null)
                    {
                        IBinder bxf = bhdData.Value.Span[3] == (byte)'3' ? BXF3.Read(bhdData.Value, bdtData.Value) : BXF4.Read(bhdData.Value, bdtData.Value);
                        var real = bxf.Files.SelectMany(f => TpfTextureNames.Read(f.Bytes) ?? new()).Select(Stem).ToHashSet(StringComparer.OrdinalIgnoreCase);
                        if (stems.ToHashSet(StringComparer.OrdinalIgnoreCase).SetEquals(real)) Interlocked.Increment(ref bhdNamesMatch);
                        else bhdExamples.Add($"{c.Path}: header {string.Join(",", stems.Take(2))} vs textures {string.Join(",", real.Take(2))}");
                    }
                }
                else
                {
                    Interlocked.Decrement(ref bhdChecked);
                    Interlocked.Increment(ref bhdChecked);
                }
                return;
            }

            var file = g.Fs.FileExists(c.Path) ? g.Fs.ReadFile(c.Path) : null;
            if (file == null) return;
            Interlocked.Add(ref bytes, file.Value.Length);

            foreach (var tpf in Tpfs(file.Value, 0))
            {
                var names = TpfTextureNames.Read(tpf);
                if (names == null) { Interlocked.Increment(ref failed); continue; }

                Interlocked.Increment(ref tpfs);
                foreach (var n in names)
                {
                    if (c.Locator) index.GetOrAdd(Stem(n), _ => new()).Add(c.Path);
                    else extraIndex[Stem(n)] = 0;
                }

                if (sample.Count < 300 && Random.Shared.Next(40) == 0)
                    sample.Add((tpf, names));
            }
        });

        Console.WriteLine($"  {containers.Count(c => c.Locator)} locator containers + {containers.Count(c => !c.Locator)} chrbnd/geombnd/mapbnd; " +
            $"{bytes / 1048576} MB read (tpfbdt not read), {tpfs} TPFs, {failed} unreadable, {timer.Elapsed.TotalSeconds:0.0} s");
        Console.WriteLine($"  {index.Count} texture names in the locator containers, {extraIndex.Keys.Count(k => !index.ContainsKey(k))} more only in chrbnd/geombnd/mapbnd");
        Console.WriteLine($"  tpfbhd: {bhdNamesMatch}/{Math.Min(bhdChecked, 30)} checked have header names equal to their texture names");
        foreach (var e in bhdExamples.Take(3)) Console.WriteLine($"      e.g. {e}");

        var agree = 0;
        foreach (var (data, names) in sample)
        {
            try
            {
                var raw = (DCX.Is(data) ? DCX.Decompress(data) : data).ToArray();
                if (TPF.Read(raw).Textures.Select(t => t.Name).SequenceEqual(names)) agree++;
            }
            catch { }
        }
        Console.WriteLine($"  sample: {agree}/{sample.Count} match a full TPF read");

        // Coverage of the names materials give
        var wanted = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var b in g.Binders.Where(b => b.Selected))
        {
            foreach (var (_, data, _) in GameMaterials.Files(b))
            {
                try
                {
                    var paths = b.Kind == MaterialKind.MTD
                        ? MTD.Read(data).Textures.Select(t => t.Path)
                        : MATBIN.Read(data).Samplers.Select(s => s.Path);
                    foreach (var p in paths.Where(p => !string.IsNullOrWhiteSpace(p)))
                        wanted.Add(Stem(p));
                }
                catch { }
            }
        }

        var found = wanted.Count(w => index.ContainsKey(w));
        var foundExtra = wanted.Count(w => !index.ContainsKey(w) && extraIndex.ContainsKey(w));
        var missing = wanted.Where(w => !index.ContainsKey(w) && !extraIndex.ContainsKey(w)).Take(6).ToList();
        Console.WriteLine($"  material texture paths: {wanted.Count} distinct names, {found} found in the locator containers, " +
            $"{foundExtra} only in chrbnd/geombnd/mapbnd, {wanted.Count - found - foundExtra} not found");
        if (missing.Count > 0) Console.WriteLine($"      not found e.g. {string.Join(", ", missing)}");
    }

    private static string Stem(string name) =>
        Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension((name ?? "").Replace('\\', '/').Split('/')[^1]));

    private static IEnumerable<Memory<byte>> Tpfs(Memory<byte> data, int depth)
    {
        if (DCX.Is(data))
        {
            Memory<byte> d;
            try { d = DCX.Decompress(data); } catch { yield break; }
            data = d;
        }

        if (TpfTextureNames.IsTpf(data.Span)) { yield return data; yield break; }

        var magic = data.Length >= 4 ? Encoding.ASCII.GetString(data.Span[..4]) : "";
        IBinder binder = null;
        try { binder = magic == "BND4" ? BND4.Read(data) : magic == "BND3" ? BND3.Read(data) : null; } catch { }
        if (binder == null) yield break;

        foreach (var f in binder.Files)
        {
            var n = f.Name ?? "";
            if (n.EndsWith(".tpf", StringComparison.OrdinalIgnoreCase) || n.EndsWith(".tpf.dcx", StringComparison.OrdinalIgnoreCase))
                yield return f.Bytes;
            else if (depth == 0 && (n.EndsWith("bnd", StringComparison.OrdinalIgnoreCase) || n.EndsWith("bnd.dcx", StringComparison.OrdinalIgnoreCase)))
                foreach (var inner in Tpfs(f.Bytes, 1)) yield return inner;
        }
    }
}
