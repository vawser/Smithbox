#nullable enable
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Renderer;

/// <summary>
///     Hands out the mesh provider for a resource handle. Asking twice for the same resource gives the same provider,
///     so every renderable of one model shares its GPU data.
/// </summary>
public static class MeshProviderCache
{
    private readonly record struct Key(IResourceHandle Handle, string Uid);

    private static readonly Dictionary<Key, MeshProvider> _cache = new();
    private static readonly object _lock = new();

    /// <summary>
    ///     Gets the provider that suits whatever the handle holds (model, collision or navmesh).
    /// </summary>
    public static MeshProvider GetProvider(IResourceHandle handle)
    {
        return handle switch
        {
            ResourceHandle<FlverResource> h => GetFlverMeshProvider(h),
            ResourceHandle<HavokCollisionResource> h => GetCollisionMeshProvider(h),
            ResourceHandle<NVMNavmeshResource> h => GetNVMMeshProvider(h),
            ResourceHandle<HavokNavmeshResource> h => GetHavokNavMeshProvider(h),
            _ => throw new ArgumentException($"'{handle.FullName}' is a {handle.ResourceType.Name}, which has no mesh.")
        };
    }

    public static void InvalidateUidEntries(string uid)
    {
        if (string.IsNullOrEmpty(uid))
        {
            return;
        }

        lock (_lock)
        {
            foreach (Key key in _cache.Keys.Where(k => k.Uid.StartsWith(uid, StringComparison.Ordinal)).ToList())
            {
                _cache.Remove(key);
            }
        }
    }

    public static FlverMeshProvider GetFlverMeshProvider(ResourceHandle<FlverResource> handle,
        IEnumerable<int>? masks, string uid)
    {
        if (masks == null || !CFG.Current.Viewport_Enable_Model_Masks)
        {
            return GetFlverMeshProvider(handle, uid);
        }

        FlverMeshProvider provider = GetFlverMeshProvider(handle, $"{uid}_masks{string.Join("", masks)}");
        provider.ModelMasks = masks.ToList();
        return provider;
    }

    public static FlverMeshProvider GetFlverMeshProvider(ResourceHandle<FlverResource> handle,
        IEnumerable<int>? masks)
    {
        if (masks == null || !CFG.Current.Viewport_Enable_Model_Masks)
        {
            return GetFlverMeshProvider(handle);
        }

        FlverMeshProvider provider = GetFlverMeshProvider(handle, $"masks{string.Join("", masks)}");
        provider.ModelMasks = masks.ToList();
        return provider;
    }

    public static FlverMeshProvider GetFlverMeshProvider(ResourceHandle<FlverResource> handle, string uid = "")
    {
        return GetOrAdd(handle, uid, () => new FlverMeshProvider(handle));
    }

    public static CollisionMeshProvider GetCollisionMeshProvider(ResourceHandle<HavokCollisionResource> handle)
    {
        return GetOrAdd(handle, "", () => new CollisionMeshProvider(handle));
    }

    public static NavmeshProvider GetNVMMeshProvider(ResourceHandle<NVMNavmeshResource> handle)
    {
        return GetOrAdd(handle, "", () => new NavmeshProvider(handle));
    }

    public static HavokNavmeshProvider GetHavokNavMeshProvider(ResourceHandle<HavokNavmeshResource> handle,
        bool temp = false)
    {
        if (temp)
        {
            return new HavokNavmeshProvider(handle);
        }

        return GetOrAdd(handle, "", () => new HavokNavmeshProvider(handle));
    }

    public static void InvalidateMeshProvider(IResourceHandle handle)
    {
        lock (_lock)
        {
            foreach (Key key in _cache.Keys.Where(k => ReferenceEquals(k.Handle, handle)).ToList())
            {
                _cache.Remove(key);
            }
        }
    }

    private static T GetOrAdd<T>(IResourceHandle handle, string uid, Func<T> create) where T : MeshProvider
    {
        lock (_lock)
        {
            var key = new Key(handle, uid);

            if (_cache.TryGetValue(key, out MeshProvider? existing))
            {
                if (existing is T typed)
                {
                    return typed;
                }

                throw new Exception(LOC.Get("REND_Mesh_Provider_Wrong_Form"));
            }

            T created = create();
            _cache.Add(key, created);
            return created;
        }
    }
}
