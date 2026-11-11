using SoulsFormats;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Every texture name in the project and the containers that hold it (Material Editor plan, F4), so a sampler path
/// can be checked and picked. Reads only TPF entry tables (<see cref="TpfTextureNames"/>) of the locator's texture
/// files, and for .tpfbhd only the header's file names, which are the texture names in every surveyed game, so no
/// .tpfbdt is read. DES/DS1/DS1R also keep textures in chrbnds. ER: 85,660 names, about 40 s with the files cached
/// by the OS. Runs when asked, in the background, cached in &lt;project&gt;/.smithbox/Project/Material/Textures.json,
/// stale when a texture file in the project folder is newer. Same pattern as <see cref="MaterialUsage"/>.
/// </summary>
public class MaterialTextureIndex
{
    private readonly ProjectEntry Project;

    public MaterialTextureIndex(ProjectEntry project)
    {
        Project = project;
    }

    private string CachePath => Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Material", "Textures.json");

    private sealed class Cache
    {
        public DateTime ScannedAt { get; set; }

        /// <summary>The containers, which the names refer to by index</summary>
        public List<string> Containers { get; set; } = new();

        /// <summary>Texture name (no extension) to the containers holding it</summary>
        public Dictionary<string, int[]> Names { get; set; } = new();
    }

    private Cache Current;
    private Dictionary<string, int[]> ByName;
    private List<string> SortedNames;
    private bool Loaded;

    public DateTime? ScannedAt => Current?.ScannedAt;
    public int NameCount => ByName?.Count ?? 0;
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
            return Current != null;
        }
    }

    /// <summary>
    /// The containers holding a texture of this name (the stem of a path), empty if none or not scanned
    /// </summary>
    public List<string> ContainersOf(string textureName)
    {
        EnsureLoaded();

        if (ByName == null || !ByName.TryGetValue(textureName ?? "", out var indices))
            return new();

        return indices.Select(i => Current.Containers[i]).ToList();
    }

    public bool Contains(string textureName) => ContainersOf(textureName).Count > 0;

    /// <summary>
    /// Every texture name, sorted, for the picker
    /// </summary>
    public IReadOnlyList<string> AllNames
    {
        get
        {
            EnsureLoaded();
            return SortedNames ?? (IReadOnlyList<string>)Array.Empty<string>();
        }
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
            Smithbox.LogError(this, LOC.Get("MAT_TextureIndex_Failed_Load", CachePath), e);
        }
    }

    private void Apply(Cache cache)
    {
        if (cache == null)
            return;

        Current = cache;
        ByName = new Dictionary<string, int[]>(cache.Names, StringComparer.OrdinalIgnoreCase);
        SortedNames = ByName.Keys.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        IsStale = ProjectChangedSince(cache.ScannedAt);
    }

    private bool ProjectChangedSince(DateTime time)
    {
        try
        {
            var root = Project.Descriptor.ProjectPath;
            if (!Directory.Exists(root))
                return false;

            foreach (var dir in Directory.EnumerateDirectories(root))
            {
                if (Path.GetFileName(dir).StartsWith("."))
                    continue;

                foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                {
                    var name = file.ToLowerInvariant();
                    if ((name.Contains(".tpf") || name.Contains("texbnd") || name.Contains("partsbnd") || name.Contains("objbnd") ||
                         name.Contains("chrbnd") || name.Contains("commonbnd") || name.Contains("ffxbnd")) &&
                        File.GetLastWriteTimeUtc(file) > time)
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
            Smithbox.LogError(this, LOC.Get("MAT_TextureIndex_Failed_Save", CachePath), e);
        }

        Smithbox.Log(this, LOC.Get("MAT_TextureIndex_Scanned", finished.Names.Count, finished.Containers.Count));
    }

    public void Cancel() => Cancellation?.Cancel();

    /// <summary>Starts the scan. The containers are gathered here, on the main thread.</summary>
    public void StartScan()
    {
        if (IsScanning)
            return;

        var locator = Project.Locator;
        var sources = new List<(string path, bool packed)>();
        sources.AddRange(locator.TextureFiles.Entries.Select(e => (e.Path, false)));
        sources.AddRange((locator.TexturePackedFiles.Entries ?? new()).Select(e => (e.Path, true)));

        // The older games keep character textures in the chrbnd
        if (Project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
            sources.AddRange(locator.ChrFiles.Entries.Select(e => (e.Path, false)));

        sources = sources.DistinctBy(s => s.path, StringComparer.OrdinalIgnoreCase).ToList();

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
                var names = new ConcurrentDictionary<string, ConcurrentBag<int>>(StringComparer.OrdinalIgnoreCase);
                var done = 0;

                Parallel.ForEach(Enumerable.Range(0, sources.Count),
                    new ParallelOptions { CancellationToken = token, MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1) }, i =>
                {
                    var (path, packed) = sources[i];
                    Status = Path.GetFileName(path);

                    try
                    {
                        var data = fs.FileExists(path) ? fs.ReadFile(path) : null;
                        if (data != null)
                        {
                            var found = packed
                                ? TpfTextureNames.PackedFileNames(data.Value) ?? new List<string>()
                                : Tpfs(data.Value, 0).SelectMany(t => TpfTextureNames.Read(t) ?? new List<string>());

                            foreach (var name in found)
                                names.GetOrAdd(Stem(name), _ => new()).Add(i);
                        }
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        Smithbox.LogError(this, LOC.Get("MAT_TextureIndex_Failed_Source", path), e);
                    }

                    Done = Interlocked.Increment(ref done);
                });

                Finished = new Cache
                {
                    ScannedAt = DateTime.UtcNow,
                    Containers = sources.Select(s => s.path).ToList(),
                    Names = names.ToDictionary(kv => kv.Key, kv => kv.Value.Distinct().OrderBy(x => x).ToArray())
                };
            }
            catch (OperationCanceledException)
            {
                IsScanning = false;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAT_TextureIndex_Failed"), e);
                IsScanning = false;
            }
        }, token);
    }

    /// <summary>
    /// The name a texture path or file name refers to: N:\…\tex\AEG007_001_a.tif and AEG007_001_a.tpf.dcx are AEG007_001_a
    /// </summary>
    public static string Stem(string name)
    {
        var file = (name ?? "").Replace('\\', '/').Split('/')[^1];

        // Up to two extensions (.tpf.dcx)
        file = Path.GetFileNameWithoutExtension(file);
        if (file.EndsWith(".tpf", StringComparison.OrdinalIgnoreCase))
            file = Path.GetFileNameWithoutExtension(file);

        return file;
    }

    /// <summary>The TPFs in a container: itself if it is one, else those in the binder and in binders in it</summary>
    internal static IEnumerable<Memory<byte>> Tpfs(Memory<byte> data, int depth)
    {
        if (DCX.Is(data))
            data = DCX.Decompress(data);

        if (TpfTextureNames.IsTpf(data.Span))
        {
            yield return data;
            yield break;
        }

        var magic = data.Length >= 4 ? Encoding.ASCII.GetString(data.Span[..4]) : "";
        IBinder binder = magic == "BND4" ? BND4.Read(data) : magic == "BND3" ? BND3.Read(data) : null;

        if (binder == null)
            yield break;

        foreach (var file in binder.Files)
        {
            var name = file.Name ?? "";

            if (name.EndsWith(".tpf", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".tpf.dcx", StringComparison.OrdinalIgnoreCase))
            {
                yield return file.Bytes;
            }
            else if (depth == 0 && (name.EndsWith("bnd", StringComparison.OrdinalIgnoreCase) || name.EndsWith("bnd.dcx", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var inner in Tpfs(file.Bytes, 1))
                    yield return inner;
            }
        }
    }
}
