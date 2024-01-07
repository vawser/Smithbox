using System;
using System.Collections.Generic;

namespace StudioCore.Resource;

/// <summary>
///     Requested access level to a given resource
/// </summary>
public enum AccessLevel
{
    /// <summary>
    ///     Resource is not loaded
    /// </summary>
    AccessUnloaded,

    /// <summary>
    ///     Access to this resource is intended for low level editing only
    /// </summary>
    AccessEditOnly,

    /// <summary>
    ///     Access to this resource is intended to be from the GPU in an optimized form
    ///     only, and is not intended to be mutated or read from the CPU. Used for textures
    ///     and models that aren't being edited primarily.
    /// </summary>
    AccessGPUOptimizedOnly,

    /// <summary>
    ///     This resource is intended to be accessed by both the GPU and accessed/modified
    ///     by the CPU.
    /// </summary>
    AccessFull
}

public interface IResourceHandle
{
    public string AssetVirtualPath { get; }

    public AccessLevel AccessLevel { get; }

    public int EventListenerCount { get; }

    public int GetReferenceCounts();
    public void Acquire();
    public void Release();

    /// <summary>
    ///     Should only be used by ResourceManager
    /// </summary>
    public void _ResourceLoaded(IResource resource, AccessLevel accessLevel);

    public void AddResourceEventListener(IResourceEventListener listener, AccessLevel accessLevel, int tag = 0);

    public void RemoveResourceEventListener(IResourceEventListener listener);

    public void Unload();

    public void UnloadIfUnused();

    public bool IsLoaded();
}

/// <summary>
///     A handle to a resource, which may or may not be loaded. Once a resource is unloaded, it may not be
///     reloaded with this handle and a new one must be constructed.
/// </summary>
/// <typeparam name="T">The resource that is wrapped by this handler</typeparam>
public class ResourceHandle<T> : IResourceHandle where T : class, IResource, IDisposable, new()
{
    protected object AcquireFreeLock = new();
    protected List<EventListener> EventListeners = new();
    protected object HandlerLock = new();

    protected object LoadingLock = new();

    protected int ReferenceCount;

    protected T Resource;

    public ResourceHandle(string virtualPath)
    {
        AssetVirtualPath = virtualPath;
    }

    public bool IsLoaded { get; protected set; }

    /// <summary>
    ///     Virtual path of the entire asset. Used to implement loading
    /// </summary>
    public string AssetVirtualPath { get; }

    public AccessLevel AccessLevel { get; protected set; } = AccessLevel.AccessUnloaded;

    /// <summary>
    ///     Adds a handler that is called every time this resource is loaded. If the resource
    ///     is loaded at the time this handler is added, the handler is called immediately
    ///     To prevent deadlock, these handlers should not trigger a load/unload of the resource
    /// </summary>
    /// <param name="handler"></param>
    public void AddResourceEventListener(IResourceEventListener listener, AccessLevel accessLevel, int tag = 0)
    {
        EventListeners.Add(new EventListener(
            new WeakReference<IResourceEventListener>(listener), accessLevel, tag));

        if (IsLoaded)
        {
            if (ResourceManager.CheckAccessLevel(accessLevel, AccessLevel))
            {
                listener.OnResourceLoaded(this, tag);
            }
        }
    }

    public void RemoveResourceEventListener(IResourceEventListener listener)
    {
        // To implement
    }

    public int EventListenerCount => EventListeners.Count;

    public void _ResourceLoaded(IResource resource, AccessLevel accessLevel)
    {
        // If there's already a resource make sure it's unloaded and everyone notified
        Unload();

        Resource = (T)resource;
        AccessLevel = accessLevel;
        IsLoaded = true;

        foreach (EventListener listener in EventListeners)
        {
            IResourceEventListener l;
            var succ = listener.Listener.TryGetTarget(out l);
            if (succ)
            {
                if (ResourceManager.CheckAccessLevel(listener.AccessLevel, accessLevel))
                {
                    l.OnResourceLoaded(this, listener.Tag);
                }
            }
        }
    }

    /// <summary>
    ///     Unloads the resource by notifying all the users and then scheduling it for deletion in the resource manager
    /// </summary>
    public void Unload()
    {
        if (Resource == null)
        {
            return;
        }

        foreach (EventListener listener in EventListeners)
        {
            IResourceEventListener l;
            var succ = listener.Listener.TryGetTarget(out l);
            if (succ)
            {
                l.OnResourceUnloaded(this, listener.Tag);
            }
        }

        T handle = Resource;
        Resource = null;
        IsLoaded = false;
        handle.Dispose();
    }

    public void UnloadIfUnused()
    {
        if (ReferenceCount <= 0)
        {
            ResourceManager.UnloadResource(this, true);
        }
    }

    bool IResourceHandle.IsLoaded()
    {
        return IsLoaded;
    }

    public int GetReferenceCounts()
    {
        return ReferenceCount;
    }

    public void Acquire()
    {
        ReferenceCount++;
    }

    public void Release()
    {
        var unload = false;
        ReferenceCount--;
        if (ReferenceCount == 0 && IsLoaded)
        {
            unload = true;
        }

        if (ReferenceCount < 0)
        {
            throw new Exception($@"Resource {AssetVirtualPath} reference count already 0");
        }

        if (unload)
        {
            ResourceManager.UnloadResource(this, true);
        }
    }

    public T Get()
    {
        return Resource;
    }

    /// <summary>
    ///     Constructs a temporary handle for a resource. This is useful for creating "fake"
    ///     resources that aren't serialized and registered with the resource management system
    ///     yet such as previews for freshly imported models
    /// </summary>
    /// <param name="res">The resource to create a handle from</param>
    /// <returns></returns>
    public static ResourceHandle<T> TempHandleFromResource(T res)
    {
        var ret = new ResourceHandle<T>("temp");
        ret.AccessLevel = AccessLevel.AccessFull;
        ret.IsLoaded = true;
        ret.Resource = res;
        return ret;
    }

    public override string ToString()
    {
        return AssetVirtualPath;
    }

    protected readonly record struct EventListener(
        WeakReference<IResourceEventListener> Listener,
        AccessLevel AccessLevel,
        int Tag);
}
