using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace StudioCore.Renderer;

/// <summary>
/// Runs resource loading jobs and unloading in the background, and keeps track of the resource pools.
/// Resources themselves live in <see cref="ResourcePool"/>s; most code only needs a pool
/// (<see cref="DefaultPool"/> or one it creates) and never talks to this class directly.
/// </summary>
public static class ResourceManager
{
    public static Smithbox BaseEditor;

    public static QueuedTaskScheduler JobScheduler = new(4, "JobMaster");
    public static readonly TaskFactory JobTaskFactory = new(JobScheduler);

    public static readonly ConcurrentDictionary<ResourceJob, int> ActiveJobProgress = new();

    public static readonly BufferBlock<AddResourceLoadNotificationRequest> _notificationRequests = new();

    public static readonly BufferBlock<UnloadResourceRequest> _unloadRequests = new();

    private static readonly object _poolLock = new();
    private static readonly List<ResourcePool> _pools = new();
    private static ResourcePool[] _poolSnapshot = Array.Empty<ResourcePool>();
    private static readonly Lazy<ResourcePool> _defaultPool = new(() => new ResourcePool("default"));

    private static readonly Lazy<ResourcePool> _connectCollisionPool = new(() => new ResourcePool("connect collision"));

    public static int _prevCount;

    // A pending missing texture load, shared by everything that asks for one before it runs
    private static readonly object _postTextureLock = new();
    private static MissingTextureRequest _postTextureLoad;

    // Passes a request makes before it gives up on containers that keep failing to deliver
    private const int MaxMissingTexturePasses = 4;

    private sealed class MissingTextureRequest
    {
        public readonly TaskCompletionSource Done = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Passes;
    }

    public static bool _scheduleWorldMapLoad;
    public static bool _scheduleCompassLoad;

    /// <summary>
    /// A pool that exists for the lifetime of the program, for code that doesn't need its own.
    /// </summary>
    public static ResourcePool DefaultPool => _defaultPool.Value;

    /// <summary>
    /// Connect collisions share files, and therefore names, with normal low collisions, so they live in their own pool.
    /// Load into it with <see cref="LoadOptions.IsConnectCollision"/> set.
    /// </summary>
    public static ResourcePool ConnectCollisionPool => _connectCollisionPool.Value;

    /// <summary>
    /// All pools that currently exist
    /// </summary>
    public static IReadOnlyList<ResourcePool> Pools
    {
        get
        {
            lock (_poolLock)
            {
                return _poolSnapshot;
            }
        }
    }

    internal static void RegisterPool(ResourcePool pool)
    {
        lock (_poolLock)
        {
            _pools.Add(pool);
            _poolSnapshot = _pools.ToArray();
        }
    }

    internal static void UnregisterPool(ResourcePool pool)
    {
        lock (_poolLock)
        {
            _pools.Remove(pool);
            _poolSnapshot = _pools.ToArray();
        }
    }

    /// <summary>
    /// Every handle in every pool, keyed by <see cref="IResourceHandle.FullName"/>. For tooling.
    /// </summary>
    public static IEnumerable<KeyValuePair<string, IResourceHandle>> GetResourceDatabase()
    {
        foreach (ResourcePool pool in Pools)
        {
            foreach (IResourceHandle handle in pool.Handles)
            {
                yield return new KeyValuePair<string, IResourceHandle>(handle.FullName, handle);
            }
        }
    }

    public static ConcurrentDictionary<ResourceJob, int> GetActiveJobProgress()
    {
        return ActiveJobProgress;
    }

    public static IResourceHandle ConstructHandle(Type t, ResourcePool pool, string name)
    {
        if (t == typeof(FlverResource))
        {
            return new ResourceHandle<FlverResource>(pool, name);
        }

        if (t == typeof(HavokCollisionResource))
        {
            return new ResourceHandle<HavokCollisionResource>(pool, name);
        }

        if (t == typeof(HavokNavmeshResource))
        {
            return new ResourceHandle<HavokNavmeshResource>(pool, name);
        }

        if (t == typeof(NVMNavmeshResource))
        {
            return new ResourceHandle<NVMNavmeshResource>(pool, name);
        }

        if (t == typeof(TextureResource))
        {
            return new ResourceHandle<TextureResource>(pool, name);
        }

        throw new Exception(LOC.Get("REND_Unhandled_Resource_Type"));
    }

    /// <summary>
    ///     See if you can use a target resource's access level with a given access level
    /// </summary>
    /// <param name="src">The access level you want to use a resource at</param>
    /// <param name="target">The access level the resource is at</param>
    /// <returns></returns>
    public static bool CheckAccessLevel(AccessLevel src, AccessLevel target)
    {
        // Full access level means anything can use it
        if (target == AccessLevel.AccessFull)
        {
            return true;
        }

        return src == target;
    }

    public static void UnloadUnusedResources()
    {
        foreach (ResourcePool pool in Pools)
        {
            pool.UnloadUnused();
        }
    }

    public static ResourceJobBuilder CreateNewJob(string name)
    {
        return new ResourceJobBuilder(name);
    }

    /// <summary>
    ///     Calls the listener, on the main UI thread, once the resource is loaded (and again if it is reloaded).
    ///     The handle may be empty at the time of the call.
    /// </summary>
    public static void AddResourceListener(IResourceHandle handle, IResourceEventListener listener, AccessLevel al,
        int tag = 0)
    {
        _notificationRequests.Post(new AddResourceLoadNotificationRequest(handle, listener, al, tag));
    }

    public static void UnloadResource(IResourceHandle resource, bool unloadOnlyIfUnused)
    {
        _unloadRequests.Post(new UnloadResourceRequest(resource, unloadOnlyIfUnused));
    }

    public static void SchedulePostTextureRefresh()
    {
        LoadMissingTexturesAsync();
    }

    /// <summary>
    ///     Loads the textures that models asked for but no loaded container had, from the containers
    ///     <see cref="ResourceFallbacks"/> finds for them. This runs once no job is in flight, as only then is every
    ///     loaded texture registered with its pool, so a load awaits this before it ends its
    ///     <see cref="ResourceLoadTracker"/> session to make these part of its own progress.
    ///     The task completes once a pass finds nothing more to load. Each pass runs once every job is done, so the
    ///     textures of the one before are registered by then, and anything started meanwhile (grass, composite
    ///     character parts, the Model Editor's preview) has asked for its textures too.
    /// </summary>
    public static Task LoadMissingTexturesAsync()
    {
        lock (_postTextureLock)
        {
            _postTextureLoad ??= new MissingTextureRequest();
            return _postTextureLoad.Done.Task;
        }
    }

    private static void RunMissingTextureLoad()
    {
        MissingTextureRequest request;

        lock (_postTextureLock)
        {
            request = _postTextureLoad;
            _postTextureLoad = null;
        }

        if (request == null)
        {
            return;
        }

        request.Passes++;

        var loads = new List<Task>();
        var queued = 0;

        try
        {
            foreach (ResourcePool pool in Pools)
            {
                loads.Add(ResourceFallbacks.LoadMissingTextures(pool, out var poolQueued));
                queued += poolQueued;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(ResourceManager), "Failed to start the load of missing textures.", e);
        }

        // A failed container is logged by its job; the waiting load carries on either way
        Task.WhenAll(loads).ContinueWith(_ =>
        {
            if (queued == 0 || request.Passes >= MaxMissingTexturePasses)
            {
                request.Done.TrySetResult();
                return;
            }

            // Look again once idle. A request made meanwhile is joined rather than run twice.
            MissingTextureRequest joined;

            lock (_postTextureLock)
            {
                if (_postTextureLoad == null)
                {
                    _postTextureLoad = request;
                    return;
                }

                joined = _postTextureLoad;
            }

            joined.Done.Task.ContinueWith(_ => request.Done.TrySetResult(), TaskScheduler.Default);
        }, TaskScheduler.Default);
    }

    public static void ScheduleWorldMapRefresh()
    {
        _scheduleWorldMapLoad = true;
    }
    public static void ScheduleCompassRefresh()
    {
        _scheduleCompassLoad = true;
    }

    public static void UpdateTasks()
    {
        // The main thread runs the uploads that free the load memory budget, so it must never wait on it
        LoadMemoryBudget.SetMainThread();

        // Process any resource notification requests
        var res = _notificationRequests.TryReceiveAll(out IList<AddResourceLoadNotificationRequest> requests);

        if (res)
        {
            foreach (AddResourceLoadNotificationRequest r in requests)
            {
                r.Handle.AddResourceEventListener(r.Listener, r.AccessLevel, r.tag);
            }
        }

        // If no loading job is currently in flight, process any unload requests
        var count = ActiveJobProgress.Count;
        if (count == 0)
        {
            if (_unloadRequests.TryReceiveAll(out IList<UnloadResourceRequest> toUnload))
            {
                foreach (UnloadResourceRequest r in toUnload)
                {
                    if (r.UnloadOnlyIfUnused && r.Resource.GetReferenceCounts() > 0)
                    {
                        continue;
                    }

                    r.Resource.Unload();
                    if (r.Resource.GetReferenceCounts() > 0)
                    {
                        continue;
                    }

                    r.Resource.Pool?.Remove(r.Resource);
                }
            }
        }

        if (count > 0)
        {
            // Loading may go on for a long time; geometry already read is uploaded once its buffer goes quiet,
            // rather than only once every job is done
            SceneRenderer.GeometryBufferAllocator?.FlushIfIdle(GeometryFlushIdleMs);

            HashSet<ResourceJob> toRemove = new();
            foreach (KeyValuePair<ResourceJob, int> job in ActiveJobProgress)
            {
                int committed = job.Key.ProcessLoadedResources();

                if (job.Key.Finished && committed < ResourceJob.MaxResourceCommitsPerFrame)
                {
                    toRemove.Add(job.Key);
                }
            }

            foreach (ResourceJob rm in toRemove)
            {
                int o;
                ActiveJobProgress.TryRemove(rm, out o);
                ResourceLoadTracker.Retire(rm);
                _memoryReleasePending = true;
            }
        }
        else
        {
            if (SceneRenderer.GeometryBufferAllocator != null &&
                SceneRenderer.GeometryBufferAllocator.HasStagingOrPending())
            {
                SceneRenderer.GeometryBufferAllocator.FlushStaging(true);
            }

            RunMissingTextureLoad();

            if (_scheduleWorldMapLoad)
            {
                _scheduleWorldMapLoad = false;
                ResourceFallbacks.LoadWorldMapTextures(DefaultPool);
            }

            if (_scheduleCompassLoad)
            {
                _scheduleCompassLoad = false;
                ResourceFallbacks.LoadCompassTextures(DefaultPool);
            }
        }

        ResourceLoadTracker.Update();

        ReleaseMemoryAfterLoad();

        _prevCount = ActiveJobProgress.Count;
    }

    // How long the geometry staging buffer has to go without a new allocation before it is uploaded during a load
    private const long GeometryFlushIdleMs = 1000;

    // Upload tasks that some features queue again every frame, so a queue this short counts as idle
    private const int IdleUploadQueueLength = 200;

    // How long loading has to stay idle before memory is given back, so a load that follows straight on isn't held up
    private const long MemoryReleaseIdleMs = 5000;

    // Memory the GC holds beyond what it uses that is worth a blocking collection to give back
    private const long MemoryReleaseThreshold = 512L * 1024 * 1024;

    private static readonly System.Diagnostics.Stopwatch _idleClock = System.Diagnostics.Stopwatch.StartNew();
    private static bool _memoryReleasePending;
    private static long _idleSinceMs = -1;

    /// <summary>
    ///     A load decompresses every file it reads into large buffers, which leaves the GC holding gigabytes of
    ///     committed memory once they are freed: it keeps freed memory to reuse rather than give it back, even after
    ///     a compacting collection. Once loading has been idle for a while, an aggressive collection returns it.
    /// </summary>
    private static void ReleaseMemoryAfterLoad()
    {
        if (!_memoryReleasePending)
        {
            return;
        }

        var (_, lowQueue) = SceneRenderer.GetUploadQueueCounts();
        if (ActiveJobProgress.Count > 0 || lowQueue > IdleUploadQueueLength)
        {
            _idleSinceMs = -1;
            return;
        }

        var now = _idleClock.ElapsedMilliseconds;
        if (_idleSinceMs < 0)
        {
            _idleSinceMs = now;
            return;
        }

        if (now - _idleSinceMs < MemoryReleaseIdleMs)
        {
            return;
        }

        _memoryReleasePending = false;
        _idleSinceMs = -1;

        GCMemoryInfo info = GC.GetGCMemoryInfo();
        var unused = info.TotalCommittedBytes - info.HeapSizeBytes + info.FragmentedBytes;
        if (unused < MemoryReleaseThreshold)
        {
            return;
        }

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
    }

    public static void ClearUnusedResources()
    {
        if (ActiveJobProgress.Count == 0)
        {
            UnloadUnusedResources();
        }
    }

    /// <summary>
    ///     Unloads everything in every pool. Resources are found by name and containers by their path within the
    ///     project, and games share both, so anything left behind would stand in for the next project's files.
    /// </summary>
    public static void ClearAllPools()
    {
        foreach (ResourcePool pool in Pools)
        {
            pool.Clear();
        }
    }

    public static void Shutdown()
    {
        JobScheduler.Dispose();
        JobScheduler = null;
    }
}
