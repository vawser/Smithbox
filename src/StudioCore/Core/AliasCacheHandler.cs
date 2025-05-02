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
public class AliasCacheHandler
{
    public AliasCache AliasCache;

    public bool ReloadAliasCaches;

    public AliasCacheHandler()
    {
        AliasCache = new AliasCache();
    }

    public void UpdateCaches()
    {
        AliasCache.BuildCache();
    }

    /// <summary>
    /// Update loop, used to spin until trigger variables occur
    /// </summary>
    public void OnGui()
    {
        if (ReloadAliasCaches)
        {
            ReloadAliasCaches = false;
            AliasCache.BuildCache();
        }
    }
}
