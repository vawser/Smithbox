#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Renderer;

/// <summary>
/// Shows Material Editor edits in the editors that draw with them (Material Editor plan, F6).
///
/// A FLVER's GPU materials are built when it loads, from the MTD/MATBIN the bank holds then (FlverResource). After a
/// material changes, the FLVERs that use it are loaded again from their container with <see cref="LoadOptions.Reload"/>,
/// as <see cref="TextureRefresh"/> does for textures: the handles stay the same, and the scenes are told through the
/// usual unload/load notifications. Only FLVERs already loaded are touched.
/// </summary>
public static class MaterialRefresh
{
    /// <summary>
    /// Reloads, in every pool, the loaded FLVERs with a material of one of these short names, except those
    /// <paramref name="skip"/> says to leave. Returns how many FLVERs were asked to reload.
    /// </summary>
    public static int ReloadModelsUsing(IEnumerable<string> materialNames, Func<string, bool>? skip = null)
    {
        var names = materialNames.Where(n => !string.IsNullOrEmpty(n)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (names.Count == 0)
            return 0;

        var count = 0;

        foreach (var pool in ResourceManager.Pools)
        {
            var affected = pool.Handles
                .Where(h => h.ResourceType == typeof(FlverResource) && h.IsLoaded() && h.Source != null)
                .Where(h => skip == null || !skip(h.Name))
                .Where(h => UsesAny(h, names))
                .ToList();

            foreach (var group in affected.GroupBy(h => h.Source.ContainerPath, StringComparer.OrdinalIgnoreCase))
            {
                pool.ReleasePriority(group);

                pool.LoadContainer(group.Key, new LoadOptions
                {
                    Reload = true,
                    Types = ResourceType.Flver,
                    OnlyNames = group.Select(h => h.Name).ToList(),
                    AccessLevel = group.First().AccessLevel,
                    HideFromLoadWindow = true,
                    JobName = $"Material Editor refresh: {group.Key}",
                });

                count += group.Count();
            }
        }

        return count;
    }

    private static bool UsesAny(IResourceHandle handle, HashSet<string> names)
    {
        if (handle is not ResourceHandle<FlverResource> flver)
            return false;

        var materials = flver.Get()?.GPUMaterials;

        return materials != null && materials.Any(m => m?.MaterialName != null && names.Contains(m.MaterialName));
    }
}
