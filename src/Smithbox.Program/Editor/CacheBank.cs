using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editor;

public class UICache
{
    private static readonly Dictionary<(EditorScreen, object, string), object> caches = new();

    /// <summary>
    ///     Gets/Sets a cache. The cached data is intended to have a lifetime until the contextual object is modified, or the
    ///     UIScreen object is refreshed.
    /// </summary>
    public static T GetCached<T>(EditorScreen UIScreen, object context, Func<T> getValue)
    {
        return GetCached(UIScreen, context, "", getValue);
    }

    /// <summary>
    ///     Gets/Sets a cache with a specific key, avoiding any case where there would be conflict over the context-giving
    ///     object
    /// </summary>
    public static T GetCached<T>(EditorScreen UIScreen, object context, string key, Func<T> getValue)
    {
        (EditorScreen UIScreen, object context, string key) trueKey = (UIScreen, context, key);
        if (!caches.ContainsKey(trueKey))
        {
            caches[trueKey] = getValue();
        }

        return (T)caches[trueKey];
    }

    /// <summary>
    ///     Removes cached data related to the context object
    /// </summary>
    public static void RemoveCache(EditorScreen UIScreen, object context)
    {
        IEnumerable<KeyValuePair<(EditorScreen, object, string), object>> toRemove =
            caches.Where(keypair => keypair.Key.Item1 == UIScreen && keypair.Key.Item2 == context);
        foreach (KeyValuePair<(EditorScreen, object, string), object> kp in toRemove)
        {
            caches.Remove(kp.Key);
        }
    }

    /// <summary>
    ///     Removes cached data within the UIScreen's domain
    /// </summary>
    public static void RemoveCache(EditorScreen UIScreen)
    {
        IEnumerable<KeyValuePair<(EditorScreen, object, string), object>> toRemove =
            caches.Where(keypair => keypair.Key.Item1 == UIScreen);
        foreach (KeyValuePair<(EditorScreen, object, string), object> kp in toRemove)
        {
            caches.Remove(kp.Key);
        }
    }

    /// <summary>
    ///     Clears all caches
    /// </summary>
    public static void ClearCaches()
    {
        caches.Clear();
    }
}
