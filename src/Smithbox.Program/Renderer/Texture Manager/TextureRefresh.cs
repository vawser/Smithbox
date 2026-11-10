using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Renderer;

/// <summary>
/// Shows saved texture edits in the editors that draw with them (Texture Editor plan, T7).
///
/// Every texture a scene uses (Model Editor, Map Editor, FX Editor, model previews) lives in a resource pool and
/// remembers the container it was loaded from. After the Texture Editor writes containers, the textures each pool
/// took from them are loaded again from the project with <see cref="LoadOptions.Reload"/>: the handles stay the
/// same, so the materials holding them are told through the usual unload/load notifications and pick up the new
/// image. Textures a pool asked for and never found are tried again as well, so a texture added to a TPF appears.
/// Nothing is loaded into a pool that didn't already use the container.
/// </summary>
public static class TextureRefresh
{
    /// <summary>
    /// Reloads, in every pool, the textures that came from these project paths (a .tpfbdt counts as its .tpfbhd).
    /// Returns how many textures were asked to reload.
    /// </summary>
    public static int ReloadSceneTextures(IEnumerable<string> savedPaths)
    {
        var keys = savedPaths
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => Key(p.Replace(".tpfbdt", ".tpfbhd", StringComparison.OrdinalIgnoreCase)))
            .ToHashSet();

        if (keys.Count == 0)
            return 0;

        var count = 0;

        foreach (var pool in ResourceManager.Pools)
        {
            var textures = pool.Handles.Where(h => h.ResourceType == typeof(TextureResource)).ToList();

            // Requested but never found: may be in the saved containers now
            var missing = textures.Where(h => !h.IsLoaded()).Select(h => h.Name).ToList();

            var byContainer = textures
                .Where(h => h.IsLoaded() && h.Source != null && keys.Contains(Key(h.Source.ContainerPath)))
                .GroupBy(h => h.Source.ContainerPath, StringComparer.OrdinalIgnoreCase);

            foreach (var group in byContainer)
            {
                var names = group.Select(h => h.Name).Concat(missing).Distinct().ToList();

                // Their index in the TPF may have changed (a texture added before them)
                pool.ReleasePriority(group);

                pool.LoadContainer(group.Key, new LoadOptions
                {
                    Reload = true,
                    Types = ResourceType.Texture,
                    OnlyNames = names,
                    AccessLevel = group.First().AccessLevel,
                    HideFromLoadWindow = true,
                    JobName = $"Texture Editor refresh: {group.Key}",
                });

                count += group.Count();
            }
        }

        return count;
    }

    /// <summary>As the pools key containers: forward slashes, no leading slash, lower case.</summary>
    private static string Key(string path)
    {
        return (path ?? "").Replace('\\', '/').TrimStart('/').ToLowerInvariant();
    }
}
