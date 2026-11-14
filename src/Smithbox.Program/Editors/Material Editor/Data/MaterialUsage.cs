using SoulsFormats;
using System.Text;
using System.Text.Json;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>A model that uses a material: its container, the FLVER in it, and how the Model Editor opens it</summary>
public record MaterialUse(string Container, string ContainerName, string Kind, string Flver);

/// <summary>
/// Which models use each material (Material Editor plan, F5). Reads the material table of every FLVER in the
/// model containers the Model Editor lists (characters, assets/objects, parts, map pieces), without their meshes
/// (<see cref="FlverMaterialNames"/>): ER's 30k FLVERs take about 20 s with the files cached by the OS. It runs only
/// when asked, on a background task, and the result is kept in &lt;project&gt;/.smithbox/Project/Material/Usage.json.
/// The cache is marked out of date when a model file in the project folder is newer than the scan. Models are read
/// as saved, so unsaved Model Editor changes aren't seen. Same pattern as the FX Editor's external references.
/// </summary>
public class MaterialUsage
{
    private readonly ProjectEntry Project;

    public MaterialUsage(ProjectEntry project)
    {
        Project = project;
    }

    private string CachePath => Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Material", "Usage.json");

    private sealed class Cache
    {
        public DateTime ScannedAt { get; set; }
        public int Containers { get; set; }
        public int Flvers { get; set; }

        /// <summary>Material short name to its uses</summary>
        public Dictionary<string, List<MaterialUse>> Uses { get; set; } = new();
    }

    private Dictionary<string, List<MaterialUse>> ByName;
    private bool Loaded;

    /// <summary>A model's kind and the short names of its materials, from the same scan</summary>
    public record ModelMaterials(string Kind, List<string> Materials);

    // Model name (the FLVER's file name up to its first '.', as the game names loaded models) to its materials
    private Dictionary<string, ModelMaterials> ByModel;

    /// <summary>The materials of a model, by the game's model name (c2010, AEG099_001); null when unknown</summary>
    public ModelMaterials MaterialsOfModel(string modelName)
    {
        EnsureLoaded();
        return ByModel != null && ByModel.TryGetValue(modelName ?? "", out var model) ? model : null;
    }

    /// <summary>The kinds of model (Character, Asset, Part, MapPiece) that use a material</summary>
    public IEnumerable<string> KindsOf(string shortName) => UsesOf(shortName).Select(u => u.Kind).Distinct();

    private static string ModelNameOf(string flver)
    {
        var cut = Math.Max(flver.LastIndexOf('\\'), flver.LastIndexOf('/'));
        var name = cut >= 0 ? flver[(cut + 1)..] : flver;
        var dot = name.IndexOf('.');
        return dot >= 0 ? name[..dot] : name;
    }

    public DateTime? ScannedAt { get; private set; }
    public int ContainerCount { get; private set; }
    public int FlverCount { get; private set; }
    public bool IsStale { get; private set; }

    // Scan progress
    public volatile bool IsScanning;
    public volatile int Done;
    public volatile int Total;
    public volatile string Status = "";
    private CancellationTokenSource Cancellation;
    private volatile Cache Finished;

    public bool HasScan
    {
        get
        {
            EnsureLoaded();
            return ScannedAt != null;
        }
    }

    /// <summary>The models using a material, by short name (empty until a scan has run)</summary>
    public List<MaterialUse> UsesOf(string shortName)
    {
        EnsureLoaded();
        return ByName != null && ByName.TryGetValue(shortName ?? "", out var list) ? list : new();
    }

    private void EnsureLoaded()
    {
        if (Loaded)
            return;
        Loaded = true;

        try
        {
            if (File.Exists(CachePath))
                Apply(JsonSerializer.Deserialize<Cache>(File.ReadAllText(CachePath)));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAT_Usage_Failed_Load", CachePath), e);
        }
    }

    private void Apply(Cache cache)
    {
        if (cache == null)
            return;

        ByName = new Dictionary<string, List<MaterialUse>>(cache.Uses, StringComparer.OrdinalIgnoreCase);

        var byModel = new Dictionary<string, ModelMaterials>(StringComparer.OrdinalIgnoreCase);
        foreach (var (material, uses) in ByName)
        {
            foreach (var use in uses)
            {
                var model = ModelNameOf(use.Flver);
                if (!byModel.TryGetValue(model, out var entry))
                    byModel[model] = entry = new ModelMaterials(use.Kind, new List<string>());

                if (!entry.Materials.Contains(material, StringComparer.OrdinalIgnoreCase))
                    entry.Materials.Add(material);
            }
        }

        foreach (var entry in byModel.Values)
            entry.Materials.Sort(StringComparer.OrdinalIgnoreCase);

        ByModel = byModel;
        ScannedAt = cache.ScannedAt;
        ContainerCount = cache.Containers;
        FlverCount = cache.Flvers;
        IsStale = ProjectChangedSince(cache.ScannedAt);
    }

    /// <summary>Whether a model file in the project folder was written after the time</summary>
    private bool ProjectChangedSince(DateTime time)
    {
        try
        {
            var root = Project.Descriptor.ProjectPath;
            foreach (var dir in new[] { "chr", "asset", "obj", "parts", "map", "model" })
            {
                var path = Path.Join(root, dir);
                if (!Directory.Exists(path))
                    continue;

                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    var name = file.ToLowerInvariant();
                    if ((name.Contains("bnd") || name.Contains(".flver")) && File.GetLastWriteTimeUtc(file) > time)
                        return true;
                }
            }
        }
        catch
        {
            // An unreadable folder isn't a reason to distrust the cache
        }

        return false;
    }

    /// <summary>Picks up a finished scan. Call on the main thread.</summary>
    public void Update()
    {
        var finished = Finished;
        if (finished == null)
            return;

        Finished = null;
        Apply(finished);
        IsScanning = false;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CachePath));
            File.WriteAllText(CachePath, JsonSerializer.Serialize(finished));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAT_Usage_Failed_Save", CachePath), e);
        }

        Smithbox.Log(this, LOC.Get("MAT_Usage_Scanned", finished.Flvers, finished.Containers, finished.Uses.Count));
    }

    public void Cancel() => Cancellation?.Cancel();

    /// <summary>Starts the scan. The containers are gathered here, on the main thread.</summary>
    public void StartScan()
    {
        if (IsScanning)
            return;

        var locator = Project.Locator;
        var sources = new List<(FileDictionaryEntry entry, string kind)>();
        sources.AddRange(locator.ChrFiles.Entries.Select(e => (e, "Character")));
        sources.AddRange(locator.AssetFiles.Entries.Select(e => (e, "Asset")));
        sources.AddRange(locator.PartFiles.Entries.Select(e => (e, "Part")));
        sources.AddRange(locator.MapPieceFiles.Entries.Select(e => (e, "MapPiece")));

        var fs = Project.VFS.FS;

        Total = sources.Count;
        Done = 0;
        IsScanning = true;
        Cancellation = new CancellationTokenSource();
        var token = Cancellation.Token;

        Task.Run(() =>
        {
            try
            {
                var uses = new System.Collections.Concurrent.ConcurrentDictionary<string, List<MaterialUse>>(StringComparer.OrdinalIgnoreCase);
                var flvers = 0;
                var done = 0;

                Parallel.ForEach(sources, new ParallelOptions { CancellationToken = token, MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1) }, source =>
                {
                    Status = source.entry.Filename;

                    try
                    {
                        var data = fs.FileExists(source.entry.Path) ? fs.ReadFile(source.entry.Path) : null;
                        if (data != null)
                        {
                            foreach (var (flverName, flverData) in Flvers(data.Value, 0))
                            {
                                var names = FlverMaterialNames.Read(flverData);
                                if (names == null)
                                    continue;

                                Interlocked.Increment(ref flvers);

                                var use = new MaterialUse(source.entry.Path, source.entry.Filename, source.kind,
                                    flverName == "" ? source.entry.Filename : Path.GetFileName(flverName.Replace('\\', '/')));

                                foreach (var name in names.Select(MaterialBank.ShortNameOf).Distinct(StringComparer.OrdinalIgnoreCase))
                                {
                                    var list = uses.GetOrAdd(name, _ => new List<MaterialUse>());
                                    lock (list)
                                        list.Add(use);
                                }
                            }
                        }
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        Smithbox.LogError(this, LOC.Get("MAT_Usage_Failed_Source", source.entry.Path), e);
                    }

                    Done = Interlocked.Increment(ref done);
                });

                Finished = new Cache
                {
                    ScannedAt = DateTime.UtcNow,
                    Containers = sources.Count,
                    Flvers = flvers,
                    Uses = uses.ToDictionary(kv => kv.Key, kv => kv.Value.OrderBy(u => u.Kind).ThenBy(u => u.ContainerName).ToList())
                };
            }
            catch (OperationCanceledException)
            {
                IsScanning = false;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAT_Usage_Failed"), e);
                IsScanning = false;
            }
        }, token);
    }

    /// <summary>
    /// The FLVERs in a container: itself if it is one, else the FLVER files of the binder and of binders in it
    /// </summary>
    private static IEnumerable<(string name, Memory<byte> data)> Flvers(Memory<byte> data, int depth)
    {
        if (DCX.Is(data))
            data = DCX.Decompress(data);

        if (FlverMaterialNames.IsFlver(data.Span))
        {
            yield return ("", data);
            yield break;
        }

        var magic = data.Length >= 4 ? Encoding.ASCII.GetString(data.Span[..4]) : "";
        IBinder binder = magic == "BND4" ? BND4.Read(data) : magic == "BND3" ? BND3.Read(data) : null;

        if (binder == null)
            yield break;

        foreach (var file in binder.Files)
        {
            var name = file.Name ?? "";

            if (name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".flver.dcx", StringComparison.OrdinalIgnoreCase))
            {
                yield return (name, file.Bytes);
            }
            else if (depth == 0 && (name.EndsWith("bnd", StringComparison.OrdinalIgnoreCase) || name.EndsWith("bnd.dcx", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var inner in Flvers(file.Bytes, 1))
                    yield return inner;
            }
        }
    }
}
