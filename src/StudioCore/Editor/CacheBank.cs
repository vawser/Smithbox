using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editor;

public class UICache
{
    private static readonly Dictionary<(IEditor, object, string), object> caches = new();

    /// <summary>
    ///     Gets/Sets a cache. The cached data is intended to have a lifetime until the contextual object is modified, or the
    ///     UIScreen object is refreshed.
    /// </summary>
    public static T GetCached<T>(IEditor editor, object context, Func<T> getValue)
    {
        return GetCached(editor, context, "", getValue);
    }

    /// <summary>
    ///     Gets/Sets a cache with a specific key, avoiding any case where there would be conflict over the context-giving
    ///     object
    /// </summary>
    public static T GetCached<T>(IEditor editor, object context, string key, Func<T> getValue)
    {
        (IEditor editor, object context, string key) trueKey = (editor, context, key);
        if (!caches.ContainsKey(trueKey))
        {
            caches[trueKey] = getValue();
        }

        return (T)caches[trueKey];
    }

    /// <summary>
    ///     Removes cached data related to the context object
    /// </summary>
    public static void RemoveCache(IEditor editor, object context)
    {
        IEnumerable<KeyValuePair<(IEditor, object, string), object>> toRemove =
            caches.Where(keypair => keypair.Key.Item1 == editor && keypair.Key.Item2 == context);

        foreach (KeyValuePair<(IEditor, object, string), object> kp in toRemove)
        {
            caches.Remove(kp.Key);
        }
    }

    /// <summary>
    ///     Removes cached data within the UIScreen's domain
    /// </summary>
    public static void RemoveCache(IEditor editor)
    {
        IEnumerable<KeyValuePair<(IEditor, object, string), object>> toRemove =
            caches.Where(keypair => keypair.Key.Item1 == editor);

        foreach (KeyValuePair<(IEditor, object, string), object> kp in toRemove)
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
