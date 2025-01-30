using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Scene.Meshes;

public static class MeshProviderCache
{
    private static readonly Dictionary<string, MeshProvider> _cache = new();

    public static string GetCacheKey(string virtualResourcePath, string uid = "")
    {
        return $"{virtualResourcePath}+{uid}";
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

        if (_cache.ContainsKey(cacheKey))
        {
            if (_cache[cacheKey] is FlverMeshProvider fmp)
            {
                return fmp;
            }

            throw new Exception("Mesh provider exists but in the wrong form");
        }

        FlverMeshProvider nfmp = new(virtualResourcePath);

        _cache.Add(cacheKey, nfmp);
        return nfmp;
    }

    public static CollisionMeshProvider GetCollisionMeshProvider(string virtualResourcePath)
    {
        if (_cache.ContainsKey(virtualResourcePath))
        {
            if (_cache[virtualResourcePath] is CollisionMeshProvider fmp)
            {
                return fmp;
            }

            throw new Exception("Mesh provider exists but in the wrong form");
        }

        CollisionMeshProvider nfmp = new(virtualResourcePath);
        _cache.Add(virtualResourcePath, nfmp);
        return nfmp;
    }

    public static NavmeshProvider GetNVMMeshProvider(string virtualResourcePath)
    {
        if (_cache.ContainsKey(virtualResourcePath))
        {
            if (_cache[virtualResourcePath] is NavmeshProvider fmp)
            {
                return fmp;
            }

            throw new Exception("Mesh provider exists but in the wrong form");
        }

        NavmeshProvider nfmp = new(virtualResourcePath);
        _cache.Add(virtualResourcePath, nfmp);
        return nfmp;
    }

    public static HavokNavmeshProvider GetHavokNavMeshProvider(string virtualResourcePath, bool temp = false)
    {
        if (!temp && _cache.ContainsKey(virtualResourcePath))
        {
            if (_cache[virtualResourcePath] is HavokNavmeshProvider fmp)
            {
                return fmp;
            }

            throw new Exception("Mesh provider exists but in the wrong form");
        }

        HavokNavmeshProvider nfmp = new(virtualResourcePath);
        if (!temp)
        {
            _cache.Add(virtualResourcePath, nfmp);
        }

        return nfmp;
    }

    public static void InvalidateMeshProvider(IResourceHandle handle)
    {
        if (_cache.ContainsKey(handle.AssetVirtualPath))
        {
            _cache.Remove(handle.AssetVirtualPath);
        }
    }
}
