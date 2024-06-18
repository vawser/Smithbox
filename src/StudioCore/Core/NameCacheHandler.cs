using StudioCore.Caches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Core;

/// <summary>
/// Handles the name caches used for ImGui lists such as in the Asset Browser
/// </summary>
public class NameCacheHandler
{
    public AssetBrowserNameCache AssetBrowserNameCache;
    public MapNameCache MapNameCache;

    public bool ReloadNameCaches;

    public NameCacheHandler()
    {
        AssetBrowserNameCache = new AssetBrowserNameCache();
        MapNameCache = new MapNameCache();
    }

    public void UpdateCaches()
    {
        AssetBrowserNameCache.BuildCache();
        MapNameCache.BuildCache();
    }

    /// <summary>
    /// Update loop, used to spin until trigger variables occur
    /// </summary>
    public void OnGui()
    {
        if (ReloadNameCaches)
        {
            ReloadNameCaches = false;
            AssetBrowserNameCache.BuildCache();
            MapNameCache.BuildCache();
        }
    }
}
