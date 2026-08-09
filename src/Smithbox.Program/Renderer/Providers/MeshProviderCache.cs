using DotNext.Collections.Generic;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Renderer;

public static class MeshProviderCache
{
    private static readonly Dictionary<string, MeshProvider> _cache = new();
    private static readonly object _lock = new();


    public static string GetCacheKey(string virtualResourcePath, string uid = "")
    {
        return $"{virtualResourcePath}+{uid}";
    }

    public static void InvalidateUidEntries(string uid)
    {
        lock (_lock)
        {
            var keysToRemove = _cache.Keys
            .Where(k => k.Contains($"+{uid}"))
            .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }
    }

    public static FlverMeshProvider GetFlverMeshProvider(
    string virtualResourcePath, IEnumerable<int> masks, string uid)
    {
        if (masks == null || !CFG.Current.Viewport_Enable_Model_Masks)
            return GetFlverMeshProvider(virtualResourcePath, uid);

        var provider = GetFlverMeshProvider(virtualResourcePath,
            $"{uid}_masks{string.Join("", masks)}");
        provider.ModelMasks = masks.ToList();
        return provider;
    }

    public static FlverMeshProvider GetFlverMeshProvider(string virtualResourcePath, IEnumerable<int> masks)
    {
        if (masks == null || !CFG.Current.Viewport_Enable_Model_Masks)
            return GetFlverMeshProvider(virtualResourcePath);

        var provider = GetFlverMeshProvider(virtualResourcePath, $"masks{string.Join("", masks)}");
        provider.ModelMasks = masks.ToList();
        return provider;
    }

    public static FlverMeshProvider GetFlverMeshProvider(string virtualResourcePath, string uid = "")
    {
        var cacheKey = GetCacheKey(virtualResourcePath, uid);

        lock (_lock)
        {
            if (_cache.TryGetValue(cacheKey, out var existing))
            {
                if (existing is FlverMeshProvider fmp)
                {
                    return fmp;
                }

                throw new Exception(LOC.Get("REND_Mesh_Provider_Wrong_Form"));
            }

            FlverMeshProvider nfmp = new(virtualResourcePath);
            _cache.Add(cacheKey, nfmp);
            return nfmp;
        }
    }

    public static CollisionMeshProvider GetCollisionMeshProvider(string virtualResourcePath)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(virtualResourcePath, out var existing))
            {
                if (existing is CollisionMeshProvider fmp)
                {
                    return fmp;
                }

                throw new Exception(LOC.Get("REND_Mesh_Provider_Wrong_Form"));
            }

            CollisionMeshProvider nfmp = new(virtualResourcePath);
            _cache.Add(virtualResourcePath, nfmp);
            return nfmp;
        }
    }

    public static NavmeshProvider GetNVMMeshProvider(string virtualResourcePath)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(virtualResourcePath, out var existing))
            {
                if (existing is NavmeshProvider fmp)
                {
                    return fmp;
                }

                throw new Exception(LOC.Get("REND_Mesh_Provider_Wrong_Form"));
            }

            NavmeshProvider nfmp = new(virtualResourcePath);
            _cache.Add(virtualResourcePath, nfmp);
            return nfmp;
        }
    }

    public static HavokNavmeshProvider GetHavokNavMeshProvider(string virtualResourcePath, bool temp = false)
    {
        lock (_lock)
        {
            if (!temp && _cache.TryGetValue(virtualResourcePath, out var existing))
            {
                if (existing is HavokNavmeshProvider fmp)
                {
                    return fmp;
                }

                throw new Exception(LOC.Get("REND_Mesh_Provider_Wrong_Form"));
            }

            HavokNavmeshProvider nfmp = new(virtualResourcePath);
            if (!temp)
            {
                _cache.Add(virtualResourcePath, nfmp);
            }

            return nfmp;
        }
    }

    public static void InvalidateMeshProvider(IResourceHandle handle)
    {
        lock (_lock)
        {
            _cache.Remove(handle.AssetVirtualPath);
        }
    }
}