using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The collision the first person mode walks on: the map collision of every collision part in the loaded maps, and
/// the collision of their assets, in world space with a tree each. Built from the game files on a background task,
/// independent of which collision the viewport displays, as hiding collision mustn't drop the player through the
/// map. Collision the Collision Editor has edited is taken from the collision bank, so edits are walked on.
///
/// Built on request (<see cref="RequestBuild"/>); <see cref="Update"/> picks the result up on the main thread.
/// Meshes whose part hasn't moved and whose source hasn't changed are kept from the last build.
/// </summary>
public class FirstPersonCollisionWorld
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    /// <summary>The collision walked on. Empty until the first build finishes; replaced as a whole by each build.</summary>
    public FirstPersonCollisionSet Current { get; private set; } = new();

    public FirstPersonBuildStatus Status { get; private set; } = new();

    public bool IsBuilding => BuildTask != null;

    /// <summary>True when something changed since the current set was built, so it should be built again.</summary>
    public bool IsOutOfDate => Generation != BuiltGeneration || BuiltSettings != CurrentSettings();

    private Task<BuildResult> BuildTask;
    private int Generation;
    private int BuiltGeneration = -1;
    private Settings BuiltSettings;

    // Caches, only touched under CacheLock (the build task and the main thread's invalidations)
    private readonly object CacheLock = new();
    private readonly Dictionary<string, FirstPersonCollisionReader.MapBinder> Binders = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<Vector3>> LocalTriangles = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FirstPersonMesh> PlacedMeshes = new(StringComparer.Ordinal);

    public FirstPersonCollisionWorld(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>The games whose collision is read: those the collision bank holds (first build scope).</summary>
    public bool CanUse()
    {
        return Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.NR
            or ProjectType.AC6
            or ProjectType.DS3
            or ProjectType.DB_NT
            or ProjectType.SDT;
    }

    private record struct Settings(HavokCollisionType Layer, bool FallProtection, bool Assets, bool DummyAssets);

    private static Settings CurrentSettings() => new(
        CFG.Current.MapEditor_FirstPerson_Layer is HavokCollisionType.High ? HavokCollisionType.High : HavokCollisionType.Low,
        CFG.Current.MapEditor_FirstPerson_Fall_Protection,
        CFG.Current.MapEditor_FirstPerson_Assets,
        CFG.Current.MapEditor_FirstPerson_Dummy_Assets);

    #region Invalidation

    /// <summary>A map was loaded or unloaded: its binders may have changed on disk, and its parts come or go.</summary>
    public void OnMapChanged(string mapId)
    {
        lock (CacheLock)
        {
            foreach (var key in Binders.Keys.Where(k => k.StartsWith(mapId + "|", StringComparison.OrdinalIgnoreCase)).ToList())
                Binders.Remove(key);

            foreach (var key in LocalTriangles.Keys.Where(k => k.StartsWith("map|" + mapId + "|", StringComparison.OrdinalIgnoreCase)).ToList())
                LocalTriangles.Remove(key);

            foreach (var key in PlacedMeshes.Where(p => string.Equals(p.Value.MapId, mapId, StringComparison.OrdinalIgnoreCase)).Select(p => p.Key).ToList())
                PlacedMeshes.Remove(key);
        }

        Generation++;
    }

    /// <summary>
    /// A map collision file's document changed (Collision Editor commit, discard, undo, generated fall protection):
    /// its triangles are read again from the bank's document on the next build.
    /// </summary>
    public void OnCollisionFileChanged(string mapId, string fileName)
    {
        var name = FirstPersonCollisionReader.NormalizeName(fileName);
        var source = MapSourceKey(mapId, name);

        lock (CacheLock)
        {
            LocalTriangles.Remove(source);

            foreach (var key in PlacedMeshes.Where(p => p.Value.Source == source).Select(p => p.Key).ToList())
                PlacedMeshes.Remove(key);
        }

        Generation++;
    }

    #endregion

    #region Build

    /// <summary>One placement to build: where it is, and the sources to try for its triangles, in order.</summary>
    private sealed class Job
    {
        public FirstPersonMeshKind Kind;
        public string MapId;
        public string PartName;
        public string PlacementKey;
        public Matrix4x4 World;
        public List<Candidate> Candidates = new();
    }

    /// <summary>A source of triangles: a map collision file (with the bank's document if it has read it) or an asset.</summary>
    private sealed class Candidate
    {
        public string SourceKey;

        // Map collision
        public string MapId;
        public string Type;
        public string FileName;
        public Havok.HavokDocument Document;

        // Asset collision
        public string ContainerPath;
        public string[] Names;
    }

    private sealed class BuildResult
    {
        public FirstPersonCollisionSet Set;
        public FirstPersonBuildStatus Status;
        public Settings Settings;
        public int Generation;
    }

    /// <summary>
    /// Starts a build if none is running and the current set is out of date (or always, with force). Returns false if
    /// there is nothing to build from (game not supported, no map loaded, collision loading off).
    /// </summary>
    public bool RequestBuild(bool force = false)
    {
        if (!CanUse() || !CFG.Current.MapEditor_ModelLoad_Collisions)
            return false;

        if (BuildTask != null)
            return true;

        if (!force && !IsOutOfDate)
            return true;

        var settings = CurrentSettings();
        var jobs = GatherJobs(settings, out var mapCount, out var filteredParts);

        if (mapCount == 0)
            return false;

        // Another layer or set of sources: nothing placed for the old settings is kept
        if (settings != BuiltSettings)
        {
            lock (CacheLock)
                PlacedMeshes.Clear();
        }

        var generation = Generation;
        var game = Project.Descriptor.ProjectType.ToString();
        var readFile = MakeFileReader();

        Status = new FirstPersonBuildStatus { Building = true, Maps = mapCount, Total = jobs.Count, MapPartsFiltered = filteredParts };
        var status = Status;

        BuildTask = Task.Run(() => Build(jobs, settings, generation, game, readFile, status));

        return true;
    }

    /// <summary>Picks up a finished build. Call once a frame while the mode's tool is shown or active.</summary>
    public void Update()
    {
        if (BuildTask == null || !BuildTask.IsCompleted)
            return;

        var task = BuildTask;
        BuildTask = null;

        if (task.IsFaulted)
        {
            Status = new FirstPersonBuildStatus { Failed = true, Errors = { task.Exception?.GetBaseException().Message ?? "" } };
            Smithbox.LogError(this, "[First Person] Collision build failed.", task.Exception?.GetBaseException());
            return;
        }

        var result = task.Result;

        Current = result.Set;
        Status = result.Status;
        BuiltSettings = result.Settings;
        BuiltGeneration = result.Generation;

        Smithbox.Log(this, $"[First Person] Built {Status.Describe()}", LogLevel.Debug);

        foreach (var error in Status.Errors.Take(20))
            Smithbox.Log(this, $"[First Person]   {error}", LogLevel.Warning);
    }

    /// <summary>Reads project files through the VFS, the mod's copy first, as the collision bank does.</summary>
    private Func<string, byte[]> MakeFileReader()
    {
        var vfs = Project.VFS;

        return path =>
        {
            try
            {
                if (vfs.ProjectFS.FileExists(path))
                    return vfs.ProjectFS.ReadFile(path)?.ToArray();

                return vfs.FS.FileExists(path) ? vfs.FS.ReadFile(path)?.ToArray() : null;
            }
            catch
            {
                return null;
            }
        };
    }

    /// <summary>
    /// The placements to build, from the loaded maps, read on the main thread: the parts, their world matrices and
    /// the bank's documents can change under a background task.
    /// </summary>
    private List<Job> GatherJobs(Settings settings, out int mapCount, out int filteredParts)
    {
        var jobs = new List<Job>();
        mapCount = 0;
        filteredParts = 0;

        var bank = View.HavokCollisionBank;
        var primary = settings.Layer is HavokCollisionType.High ? "h" : "l";
        var fallback = primary == "h" ? "l" : "h";
        var assetCandidates = new Dictionary<string, Candidate>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            var container = entry.Value.MapContainer;

            if (container == null)
                continue;

            mapCount++;
            var mapId = entry.Key.Filename;

            // The bank's file names of this map, by lookup name, for its documents
            var bankNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (bank.MapCollisions.TryGetValue(mapId, out var names))
            {
                foreach (var name in names)
                    bankNames.TryAdd(FirstPersonCollisionReader.NormalizeName(name), name);
            }

            Candidate MapCandidate(string type, string fileName)
            {
                var name = FirstPersonCollisionReader.NormalizeName(fileName);
                var bankName = bankNames.GetValueOrDefault(name);

                return new Candidate
                {
                    SourceKey = MapSourceKey(mapId, name),
                    MapId = mapId,
                    Type = type,
                    FileName = name,
                    Document = bankName == null ? null : bank.PeekEditableDocument(bankName)
                };
            }

            foreach (var obj in container.Objects)
            {
                if (obj is not MsbEntity entity || !entity.HasTransform)
                    continue;

                var modelName = GetModelName(entity);

                if (string.IsNullOrEmpty(modelName))
                    continue;

                var world = entity.GetWorldMatrix();

                if (EntityHelper.IsPartCollision(entity))
                {
                    if (modelName.Length < 2)
                        continue;

                    // Only the collision the player collides with: the others are for enemies, objects, cameras
                    if (!IsPlayerCollision(entity))
                    {
                        filteredParts++;
                        continue;
                    }

                    // l10_00_00_00_000100: the map's id, then the model's number
                    var stem = $"{mapId.Substring(1)}_{modelName.Substring(1)}";

                    var job = NewJob(FirstPersonMeshKind.Map, mapId, entity, "map", world);
                    job.Candidates.Add(MapCandidate(primary, primary + stem));
                    job.Candidates.Add(MapCandidate(fallback, fallback + stem));
                    jobs.Add(job);

                    if (settings.FallProtection)
                    {
                        var fall = NewJob(FirstPersonMeshKind.Map, mapId, entity, "fall", world);
                        fall.Candidates.Add(MapCandidate("f", "f" + stem));
                        jobs.Add(fall);
                    }

                    continue;
                }

                if (!settings.Assets)
                    continue;

                var isPrefab = EntityHelper.IsPartPrefabAsset(entity);
                var isDummy = EntityHelper.IsPartDummyAsset(entity);

                if (!isPrefab && !EntityHelper.IsPartAsset(entity) && !(isDummy && settings.DummyAssets))
                    continue;

                if (isPrefab)
                {
                    var members = Project.Handler.MapData.PrefabAssetBank.GetMembers(modelName);

                    for (var i = 0; i < members.Count; i++)
                    {
                        var candidate = AssetCandidate(members[i].ModelName, settings.Layer, assetCandidates);

                        if (candidate == null)
                            continue;

                        var job = NewJob(FirstPersonMeshKind.Asset, mapId, entity, $"member{i}", members[i].LocalTransform * world);
                        job.Candidates.Add(candidate);
                        jobs.Add(job);
                    }

                    continue;
                }

                var assetCandidate = AssetCandidate(modelName, settings.Layer, assetCandidates);

                if (assetCandidate == null)
                    continue;

                var assetJob = NewJob(FirstPersonMeshKind.Asset, mapId, entity, "asset", world);
                assetJob.Candidates.Add(assetCandidate);
                jobs.Add(assetJob);
            }
        }

        return jobs;
    }

    private static Job NewJob(FirstPersonMeshKind kind, string mapId, MsbEntity entity, string slot, Matrix4x4 world)
    {
        return new Job
        {
            Kind = kind,
            MapId = mapId,
            PartName = entity.Name,
            // The entity itself is the placement: two parts may share a name while a map is being edited
            PlacementKey = $"{mapId}|{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(entity)}|{entity.Name}|{slot}",
            World = world
        };
    }

    /// <summary>An asset model's collision source, or null if it has none. One per model, shared by its placements.</summary>
    private Candidate AssetCandidate(string modelName, HavokCollisionType layer, Dictionary<string, Candidate> known)
    {
        if (known.TryGetValue(modelName, out var candidate))
            return candidate;

        candidate = null;

        try
        {
            var asset = ModelLocator.GetMapObjCollision(Project, modelName, layer);
            var containerPath = asset.IsValid() ? asset.GetContainerPath(Project) : null;

            if (containerPath != null)
            {
                candidate = new Candidate
                {
                    SourceKey = $"asset|{containerPath}",
                    ContainerPath = containerPath,
                    Names = new[]
                    {
                        asset.AssetName,
                        asset.AssetVirtualPath,
                        Path.GetFileNameWithoutExtension(asset.AssetArchiveVirtualPath ?? "")
                    }
                };
            }
        }
        catch (Exception e)
        {
            Smithbox.Log(this, $"[First Person] No collision locator for {modelName}: {e.Message}", LogLevel.Debug);
        }

        known[modelName] = candidate;
        return candidate;
    }

    private static string MapSourceKey(string mapId, string normalizedName) => $"map|{mapId}|{normalizedName}";

    private BuildResult Build(List<Job> jobs, Settings settings, int generation, string game, Func<string, byte[]> readFile,
        FirstPersonBuildStatus progress)
    {
        var timer = Stopwatch.StartNew();
        var set = new FirstPersonCollisionSet();
        var status = new FirstPersonBuildStatus { Maps = progress.Maps, Total = jobs.Count, MapPartsFiltered = progress.MapPartsFiltered };
        var placed = new Dictionary<string, FirstPersonMesh>(StringComparer.Ordinal);
        var missingModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var models = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Error(string message)
        {
            lock (status.Errors)
            {
                if (status.Errors.Count < 200)
                    status.Errors.Add(message);
            }
        }

        foreach (var job in jobs)
        {
            progress.Done++;

            FirstPersonMesh mesh = null;
            bool usedFallback = false;

            // Kept from the last build if the part hasn't moved and the source is the one it would pick now
            lock (CacheLock)
            {
                if (PlacedMeshes.TryGetValue(job.PlacementKey, out var previous)
                    && previous.World == job.World
                    && job.Candidates.Any(c => c.SourceKey == previous.Source)
                    && LocalTriangles.ContainsKey(previous.Source))
                {
                    mesh = previous;
                    usedFallback = job.Candidates[0].SourceKey != previous.Source;
                }
            }

            if (mesh == null)
            {
                for (var i = 0; i < job.Candidates.Count && mesh == null; i++)
                {
                    var candidate = job.Candidates[i];
                    var local = GetLocalTriangles(candidate, game, readFile, Error);

                    if (local == null || local.Count == 0)
                        continue;

                    mesh = new FirstPersonMesh
                    {
                        Kind = job.Kind,
                        MapId = job.MapId,
                        PartName = job.PartName,
                        Source = candidate.SourceKey,
                        World = job.World,
                        Tree = new FirstPersonTriangleTree(FirstPersonCollisionReader.Place(local, job.World))
                    };

                    usedFallback = i > 0;
                }
            }

            if (job.Kind == FirstPersonMeshKind.Asset)
            {
                var model = job.Candidates[0].SourceKey;
                models.Add(model);

                if (mesh == null)
                    missingModels.Add(model);
            }

            if (mesh == null)
            {
                if (job.Kind == FirstPersonMeshKind.Map)
                    status.MapPartsMissing++;

                continue;
            }

            placed[job.PlacementKey] = mesh;
            set.Add(mesh);

            if (job.Kind == FirstPersonMeshKind.Map)
            {
                status.MapParts++;
                status.MapTriangles += mesh.Tree.TriangleCount;

                if (usedFallback)
                    status.MapPartsFallback++;
            }
            else
            {
                status.AssetPlacements++;
                status.AssetTriangles += mesh.Tree.TriangleCount;
            }
        }

        lock (CacheLock)
        {
            PlacedMeshes.Clear();

            foreach (var (key, mesh) in placed)
                PlacedMeshes[key] = mesh;

            // The binders' bytes are only needed to read files not read yet; the triangles are kept, so a rebuild
            // reads a binder again only for a part that has a file it hasn't used (a Stormveil binder is ~20 MB)
            Binders.Clear();
        }

        status.AssetModels = models.Count;
        status.AssetModelsMissing = missingModels.Count;
        status.MemoryBytes = set.MemoryBytes;
        status.Milliseconds = timer.ElapsedMilliseconds;

        return new BuildResult { Set = set, Status = status, Settings = settings, Generation = generation };
    }

    /// <summary>
    /// A source's triangles in its part's space, read once and kept: the bank's document if it has one (it may hold
    /// edits), else the game file. Null (and kept as null) if there is none.
    /// </summary>
    private List<Vector3> GetLocalTriangles(Candidate candidate, string game, Func<string, byte[]> readFile, Action<string> error)
    {
        lock (CacheLock)
        {
            if (LocalTriangles.TryGetValue(candidate.SourceKey, out var cached))
                return cached;
        }

        List<Vector3> triangles = null;

        if (candidate.ContainerPath != null)
        {
            triangles = FirstPersonCollisionReader.ReadAssetTriangles(readFile, game, candidate.ContainerPath, candidate.Names, error);
        }
        else if (candidate.Document != null)
        {
            triangles = FirstPersonCollisionReader.TrianglesOf(candidate.Document, candidate.FileName, error);
        }
        else
        {
            var binder = GetBinder(candidate.MapId, candidate.Type, game, readFile, error);
            triangles = binder?.ReadTriangles(candidate.FileName, error);
        }

        lock (CacheLock)
        {
            LocalTriangles[candidate.SourceKey] = triangles;
        }

        return triangles;
    }

    private FirstPersonCollisionReader.MapBinder GetBinder(string mapId, string type, string game, Func<string, byte[]> readFile,
        Action<string> error)
    {
        var key = $"{mapId}|{type}";

        lock (CacheLock)
        {
            if (Binders.TryGetValue(key, out var cached))
                return cached;
        }

        FirstPersonCollisionReader.MapBinder binder = null;

        try
        {
            binder = FirstPersonCollisionReader.MapBinder.Read(readFile, game, mapId, type);
        }
        catch (Exception e)
        {
            error($"{key}: binder read failed: {e.Message}");
        }

        lock (CacheLock)
        {
            Binders[key] = binder;
        }

        return binder;
    }

    /// <summary>The hit filter of the collision the player collides with.</summary>
    public const byte PlayerHitFilter = 8;

    /// <summary>
    /// Whether a collision part is collided with by the player: its HitFilterID is 8. A part without the field (not
    /// expected in the supported games) counts as the player's.
    /// </summary>
    private static bool IsPlayerCollision(MsbEntity entity)
    {
        try
        {
            return entity.GetPropertyValue("HitFilterID") switch
            {
                byte b => b == PlayerHitFilter,
                null => true,
                var other => Convert.ToInt32(other) == PlayerHitFilter
            };
        }
        catch
        {
            return true;
        }
    }

    private static string GetModelName(MsbEntity entity)
    {
        try
        {
            return entity.GetPropertyValue("ModelName") as string;
        }
        catch
        {
            return null;
        }
    }

    #endregion
}

/// <summary>What the last first person collision build made, for the tool's status line and the log.</summary>
public sealed class FirstPersonBuildStatus
{
    public bool Building;
    public bool Failed;

    public int Maps;

    // Progress while building: placements looked at, out of all of them
    public volatile int Done;
    public int Total;

    public int MapParts;
    public int MapPartsFallback;
    public int MapPartsMissing;

    // Collision parts left out as they aren't the player's (HitFilterID other than 8)
    public int MapPartsFiltered;
    public long MapTriangles;

    public int AssetPlacements;
    public int AssetModels;
    public int AssetModelsMissing;
    public long AssetTriangles;

    public long MemoryBytes;
    public long Milliseconds;

    public readonly List<string> Errors = new();

    public string Describe()
    {
        return $"{Maps} maps: {MapParts} collision parts ({MapPartsFallback} on the other layer, {MapPartsMissing} without collision, {MapPartsFiltered} not the player's), "
            + $"{MapTriangles:N0} triangles; {AssetPlacements} asset placements of {AssetModels} models "
            + $"({AssetModelsMissing} without readable collision), {AssetTriangles:N0} triangles; "
            + $"{MemoryBytes / (1024.0 * 1024.0):0.0} MB, {Milliseconds} ms, {Errors.Count} errors";
    }
}
