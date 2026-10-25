#nullable enable
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

/// <summary>
///     Bridges the locators (which work out where a game's assets are stored) to the pool.
///     A <see cref="ResourceDescriptor" /> only has to be turned into a container path and a name;
///     after that the pool does the work.
/// </summary>
public static class ResourcePoolAssetExtensions
{
    /// <summary>
    ///     Loads the container holding an asset, unless the asset is already loaded.
    /// </summary>
    public static Task LoadAsset<T>(this ResourcePool pool, ProjectEntry project, ResourceDescriptor asset,
        LoadOptions options) where T : class, IResource, IDisposable, new()
    {
        if (asset.AssetVirtualPath != null && pool.IsLoaded<T>(asset.Name, options.AccessLevel))
        {
            return Task.CompletedTask;
        }

        var path = asset.GetContainerPath(project);
        return path == null ? Task.CompletedTask : pool.LoadContainer(path, options);
    }

    /// <summary>
    ///     Loads the containers holding a set of assets as one job. Containers shared by several assets are loaded once.
    /// </summary>
    /// <param name="onlyListedAssets">
    ///     Load only the listed assets from each container, rather than everything in it
    /// </param>
    public static Task LoadAssets(this ResourcePool pool, ProjectEntry project, IEnumerable<ResourceDescriptor> assets,
        LoadOptions options, bool onlyListedAssets = false)
    {
        var located = assets
            .Where(a => a != null)
            .Select(a => (Asset: a, Path: a.GetContainerPath(project)))
            .Where(a => a.Path != null)
            .ToList();

        if (located.Count == 0)
        {
            return Task.CompletedTask;
        }

        if (!onlyListedAssets)
        {
            return pool.LoadContainers(located.Select(a => a.Path!).Distinct(StringComparer.OrdinalIgnoreCase),
                options);
        }

        List<Task> tasks = new();
        foreach (var group in located.GroupBy(a => a.Path!, StringComparer.OrdinalIgnoreCase))
        {
            LoadOptions groupOptions = options.Clone();
            groupOptions.OnlyNames = group.Select(a => a.Asset.Name);
            tasks.Add(pool.LoadContainer(group.Key, groupOptions));
        }

        return Task.WhenAll(tasks);
    }
}
