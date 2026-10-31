#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

/// <summary>
///     A set of loaded resources that can be looked up by name.
///     <para>
///         Load: hand the pool a container file (a binder, a TPF, or a single model/collision/navmesh file) with
///         <see cref="LoadContainer(string, LoadOptions?)" /> and every resource inside it is loaded.
///     </para>
///     <para>
///         Find: <see cref="Find{T}" /> returns the first resource called <c>name</c>. "First" means the one from the
///         container that was added to the pool first. Names ignore case, directories and file extensions
///         (see <see cref="ResourceName" />).
///     </para>
///     <para>
///         Loading is asynchronous, so <see cref="Find{T}" /> may be called before the resource exists. It then returns
///         a handle that is not loaded yet; the handle fills in when the resource arrives (listeners are notified
///         at that point). This is also how model textures work: a model asks its pool for each texture by name.
///     </para>
///     <para>
///         Pools are independent of each other. Use one pool per thing that needs its own set of names (e.g. one for
///         each map being viewed) if the same name can mean different resources.
///     </para>
/// </summary>
public sealed class ResourcePool : IDisposable
{
    private readonly Dictionary<string, (ResourceType Types, AccessLevel Access)> _completed = new();
    private readonly Dictionary<string, int> _containerSequence = new();
    private readonly Dictionary<(string Name, Type Type), Entry> _entries = new();
    private readonly Dictionary<string, Task> _inFlight = new();
    private readonly object _lock = new();
    private readonly Dictionary<int, string> _sequenceToContainer = new();

    private bool _disposed;
    private int _nextSequence;
    private IResourceHandle[] _snapshot = Array.Empty<IResourceHandle>();
    private bool _snapshotDirty;

    public ResourcePool(string name)
    {
        Name = name;
        ResourceManager.RegisterPool(this);
    }

    public string Name { get; }

    /// <summary>
    ///     Models in this pool are for looking at the shape of and nothing else. They don't ask for their
    ///     textures, whatever the viewport's texturing setting is, and take the quickest route through loading.
    ///     Set this before anything is loaded.
    /// </summary>
    public bool ModelsOnly { get; init; }

    /// <summary>
    ///     Snapshot of every handle in the pool, loaded or not. Cheap to call repeatedly.
    /// </summary>
    public IReadOnlyList<IResourceHandle> Handles
    {
        get
        {
            lock (_lock)
            {
                if (_snapshotDirty)
                {
                    _snapshot = _entries.Values.Select(e => e.Handle).ToArray();
                    _snapshotDirty = false;
                }

                return _snapshot;
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Clear();
        ResourceManager.UnregisterPool(this);
    }

    #region Loading

    /// <summary>
    ///     Loads every resource in a container file from the project's virtual file system.
    ///     <para>
    ///         Supported containers: binders (*bnd, *bnd.dcx), split binders (*bhd + *bdt), TPFs, and loose
    ///         model (flver), collision/navmesh (hkx) and nvm files. Binders can contain further containers.
    ///     </para>
    ///     <para>
    ///         Loading the same container again is cheap: resources that are already loaded are skipped unless
    ///         <see cref="LoadOptions.Reload" /> is set. The returned task completes once the container's files have
    ///         been read and parsed; resources become visible to <see cref="Find{T}" /> consumers a moment later,
    ///         on the main thread. Use handle listeners to react to a specific resource arriving.
    ///     </para>
    /// </summary>
    /// <param name="path">Project-relative path of the container.</param>
    public Task LoadContainer(string path, LoadOptions? options = null)
    {
        return LoadContainers(new[] { path }, options);
    }

    /// <summary>
    ///     Loads several containers as a single job. Container order is priority order: when two containers hold
    ///     a resource with the same name, the one listed first is returned by <see cref="Find{T}" />.
    /// </summary>
    public Task LoadContainers(IEnumerable<string> paths, LoadOptions? options = null)
    {
        return Load(paths.Select(p => (p, ContainerOrigin.Vfs, default(Memory<byte>))), options);
    }

    /// <summary>
    ///     Loads a container file that is not part of the project's virtual file system (an absolute path).
    /// </summary>
    public Task LoadExternalContainer(string absolutePath, LoadOptions? options = null)
    {
        return Load(new[] { (absolutePath, ContainerOrigin.External, default(Memory<byte>)) }, options);
    }

    /// <summary>
    ///     Loads a container from memory. <paramref name="name" /> must carry the container's file extension
    ///     (e.g. "c1234.chrbnd.dcx" or "my_model.flver") because the extension tells the loader what the data is.
    /// </summary>
    public Task LoadContainerData(string name, Memory<byte> data, LoadOptions? options = null)
    {
        return Load(new[] { (name, ContainerOrigin.Memory, data) }, options);
    }

    private Task Load(IEnumerable<(string Path, ContainerOrigin Origin, Memory<byte> Data)> sources,
        LoadOptions? options)
    {
        options ??= new LoadOptions();

        var waits = new List<Task>();
        var added = new List<string>();

        lock (_lock)
        {
            ResourceJobBuilder? builder = null;

            foreach (var source in sources)
            {
                if (string.IsNullOrWhiteSpace(source.Path))
                {
                    continue;
                }

                var key = ContainerKey(source.Path);

                // Already being loaded: join that load rather than starting a second one
                if (!options.Reload && _inFlight.TryGetValue(key, out Task? running))
                {
                    waits.Add(running);
                    continue;
                }

                // Already loaded, in full, in a compatible way
                if (!options.Reload && source.Origin != ContainerOrigin.Memory && IsCoveredByCompletedLoad(key, options))
                {
                    continue;
                }

                builder ??= ResourceManager.CreateNewJob(options.JobName ?? $"{Name}: {source.Path}");
                builder.SetHidden(options.HideFromLoadWindow);
                builder.AddContainer(new ContainerLoadAction(this, source.Path, source.Origin, source.Data, options,
                    GetSequence(key), success => OnContainerFinished(key, options, source.Origin, success)));
                added.Add(key);
            }

            if (builder == null)
            {
                return Task.WhenAll(waits);
            }

            Task job = builder.Complete();
            foreach (var key in added)
            {
                _inFlight[key] = job;
            }

            job.ContinueWith(_ =>
            {
                lock (_lock)
                {
                    foreach (var key in added)
                    {
                        if (_inFlight.TryGetValue(key, out Task? t) && t == job)
                        {
                            _inFlight.Remove(key);
                        }
                    }
                }
            }, TaskScheduler.Default);

            waits.Add(job);
            return waits.Count == 1 ? job : Task.WhenAll(waits);
        }
    }

    /// <summary>
    ///     True if the whole container has been loaded into this pool and nothing from it has been unloaded since.
    /// </summary>
    public bool IsContainerLoaded(string path)
    {
        lock (_lock)
        {
            return _completed.ContainsKey(ContainerKey(path));
        }
    }

    /// <summary>
    ///     Unloads everything that came from a container. Resources still referenced by renderables are unloaded
    ///     as well; their listeners are told through the usual unload notification.
    /// </summary>
    public void UnloadContainer(string path)
    {
        List<IResourceHandle> toUnload = new();

        lock (_lock)
        {
            var key = ContainerKey(path);
            if (!_containerSequence.TryGetValue(key, out var sequence))
            {
                return;
            }

            foreach (Entry e in _entries.Values)
            {
                if (e.Order != long.MaxValue && e.Order >> 32 == sequence)
                {
                    toUnload.Add(e.Handle);
                }
            }

            _completed.Remove(key);
        }

        foreach (IResourceHandle h in toUnload)
        {
            ResourceManager.UnloadResource(h, false);
        }
    }

    private bool IsCoveredByCompletedLoad(string key, LoadOptions options)
    {
        if (!_completed.TryGetValue(key, out var done))
        {
            return false;
        }

        // A load that was restricted to some names can't be assumed to cover a later, wider request
        if (options.OnlyNames != null)
        {
            return false;
        }

        return (options.Types & ~done.Types) == 0 &&
               ResourceManager.CheckAccessLevel(options.AccessLevel, done.Access);
    }

    private void OnContainerFinished(string key, LoadOptions options, ContainerOrigin origin, bool success)
    {
        // Containers are only recorded as complete when everything in them was requested
        if (!success || options.OnlyNames != null || origin == ContainerOrigin.Memory)
        {
            return;
        }

        lock (_lock)
        {
            _completed[key] = (options.Types, options.AccessLevel);
        }
    }

    private int GetSequence(string key)
    {
        // Called under _lock. A container keeps its priority when it is loaded again.
        if (!_containerSequence.TryGetValue(key, out var seq))
        {
            seq = _nextSequence++;
            _containerSequence.Add(key, seq);
            _sequenceToContainer.Add(seq, key);
        }

        return seq;
    }

    private static string ContainerKey(string path)
    {
        return path.Replace('\\', '/').TrimStart('/').ToLowerInvariant();
    }

    #endregion

    #region Finding

    /// <summary>
    ///     Gets the resource of type <typeparamref name="T" /> called <paramref name="name" />.
    ///     If it isn't loaded (yet) the returned handle is empty and fills in when the resource arrives.
    /// </summary>
    public ResourceHandle<T> Find<T>(string name) where T : class, IResource, IDisposable, new()
    {
        return (ResourceHandle<T>)GetOrCreateHandle(typeof(T), name);
    }

    /// <summary>
    ///     Same as <see cref="Find{T}" /> with the type chosen at runtime.
    /// </summary>
    public IResourceHandle Find(Type resourceType, string name)
    {
        return GetOrCreateHandle(resourceType, name);
    }

    /// <summary>
    ///     Gets the handle only if the pool already knows the resource. Never creates an empty handle.
    /// </summary>
    public bool TryFind<T>(string name, out ResourceHandle<T>? handle) where T : class, IResource, IDisposable, new()
    {
        lock (_lock)
        {
            if (_entries.TryGetValue((ResourceName.Normalize(name), typeof(T)), out Entry? e))
            {
                handle = (ResourceHandle<T>)e.Handle;
                return true;
            }
        }

        handle = null;
        return false;
    }

    /// <summary>
    ///     True if the resource is in the pool, loaded, and usable at the given access level.
    /// </summary>
    public bool IsLoaded<T>(string name, AccessLevel accessLevel = AccessLevel.AccessGPUOptimizedOnly)
        where T : class, IResource, IDisposable, new()
    {
        return TryFind(name, out ResourceHandle<T>? h) && h!.IsLoaded &&
               ResourceManager.CheckAccessLevel(accessLevel, h.AccessLevel);
    }

    /// <summary>
    ///     Calls the listener (on the main thread) when the named resource is loaded, and again whenever it is reloaded.
    ///     Works before the resource exists.
    /// </summary>
    public void AddListener<T>(string name, IResourceEventListener listener, AccessLevel accessLevel, int tag = 0)
        where T : class, IResource, IDisposable, new()
    {
        ResourceManager.AddResourceListener(Find<T>(name), listener, accessLevel, tag);
    }

    internal IResourceHandle GetOrCreateHandle(Type type, string name)
    {
        var key = (ResourceName.Normalize(name), type);

        lock (_lock)
        {
            if (!_entries.TryGetValue(key, out Entry? e))
            {
                e = new Entry(ResourceManager.ConstructHandle(type, this, key.Item1));
                _entries.Add(key, e);
                _snapshotDirty = true;
            }

            return e.Handle;
        }
    }

    #endregion

    #region Registration (called by the resource manager)

    /// <summary>
    ///     Whether a resource is worth loading right now. False when a resource of that name and type is already
    ///     loaded from an equal or higher priority container.
    /// </summary>
    internal bool ShouldLoad(string normalizedName, Type type, long order, AccessLevel accessLevel, bool reload)
    {
        if (reload)
        {
            return true;
        }

        lock (_lock)
        {
            if (_entries.TryGetValue((normalizedName, type), out Entry? e) && e.Handle.IsLoaded() &&
                e.Order <= order && ResourceManager.CheckAccessLevel(accessLevel, e.Handle.AccessLevel))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Adds a freshly loaded resource. Runs on the main thread. If a higher priority resource with the same name
    ///     is already present the new one is discarded.
    /// </summary>
    internal void Register(ResourceLoadContext ctx, IResource resource, AccessLevel accessLevel)
    {
        IResourceHandle handle;
        var accepted = false;

        lock (_lock)
        {
            var key = (ctx.Name, resource.GetType());
            if (!_entries.TryGetValue(key, out Entry? e))
            {
                e = new Entry(ResourceManager.ConstructHandle(key.Item2, this, ctx.Name));
                _entries.Add(key, e);
                _snapshotDirty = true;
            }

            handle = e.Handle;

            if (ctx.Order <= e.Order)
            {
                e.Order = ctx.Order;
                accepted = true;
            }
        }

        if (accepted)
        {
            handle._SetSource(ctx);
            handle._ResourceLoaded(resource, accessLevel);
        }
        else if (resource is IDisposable d)
        {
            d.Dispose();
        }
    }

    internal void Remove(IResourceHandle handle)
    {
        lock (_lock)
        {
            var key = (handle.Name, handle.ResourceType);
            if (_entries.TryGetValue(key, out Entry? e) && ReferenceEquals(e.Handle, handle))
            {
                _entries.Remove(key);
                _snapshotDirty = true;

                // Something that came from this container is gone, so it no longer counts as fully loaded
                if (e.Order != long.MaxValue && _sequenceToContainer.TryGetValue((int)(e.Order >> 32), out var c))
                {
                    _completed.Remove(c);
                }
            }
        }
    }

    #endregion

    #region Housekeeping

    /// <summary>
    ///     Unloads every resource in the pool that nothing is using.
    /// </summary>
    public void UnloadUnused()
    {
        foreach (IResourceHandle h in Handles)
        {
            if (h.IsLoaded() && h.GetReferenceCounts() == 0)
            {
                h.UnloadIfUnused();
            }
        }
    }

    /// <summary>
    ///     Unloads every resource in the pool.
    /// </summary>
    public void Clear()
    {
        foreach (IResourceHandle h in Handles)
        {
            ResourceManager.UnloadResource(h, false);
        }

        lock (_lock)
        {
            _completed.Clear();
        }
    }

    public override string ToString()
    {
        return Name;
    }

    private sealed class Entry
    {
        public readonly IResourceHandle Handle;

        // Priority of the resource currently held. long.MaxValue means "nothing loaded yet".
        public long Order = long.MaxValue;

        public Entry(IResourceHandle handle)
        {
            Handle = handle;
        }
    }

    #endregion
}
