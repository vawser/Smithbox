#nullable enable
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

/// <summary>
///     Game specific "where might this missing resource be" rules. Used when something asked a pool for a texture
///     that no loaded container has: some games keep a model's textures in a container that nothing has loaded.
///     Everything here is driven by the texture's name.
/// </summary>
internal static class ResourceFallbacks
{
    private static readonly Regex ChrIdPrefix = new(@"^(c\d{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // The containers each pool has loaded for the texture name index, so a holder that didn't deliver isn't retried
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ResourcePool, HashSet<string>> _indexTried = new();

    /// <summary>
    ///     For every texture the pool has been asked for but doesn't have, tries to find a container that holds it.
    ///     The task completes once the containers it started have been read. <paramref name="queued" /> is how many
    ///     containers it started, so a caller can tell whether looking again could find more.
    /// </summary>
    public static Task LoadMissingTextures(ResourcePool pool, out int queued)
    {
        queued = 0;

        ProjectEntry? project = Smithbox.Orchestrator.SelectedProject;
        if (project == null)
        {
            return Task.CompletedTask;
        }

        HashSet<string> vfsPaths = new();
        HashSet<string> externalPaths = new();

        HashSet<string> tried = _indexTried.GetValue(pool, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        var indexed = 0;

        // The containers this pass loads for the index, so textures that share a holder don't each load another one
        HashSet<string> indexPaths = new(StringComparer.OrdinalIgnoreCase);

        foreach (IResourceHandle handle in pool.Handles)
        {
            if (handle.IsLoaded() || handle.ResourceType != typeof(TextureResource))
            {
                continue;
            }

            var name = handle.Name;

            // Whether a rule below names a container for this texture that isn't loaded yet. A rule whose container
            // is loaded already, while the texture is still missing, named the wrong one, so the index gets a turn.
            var ruleMatched = false;

            // DS1: map textures can be loose TPFs named after the texture
            if (CFG.Current.MapEditor_TextureLoad_MapPieces && project.Descriptor.ProjectType is ProjectType.DS1)
            {
                var path = Path.Join(project.Descriptor.DataPath, "map", "tx", $"{name}.tpf");
                if (File.Exists(path) && !pool.IsContainerLoaded(path))
                {
                    externalPaths.Add(path);
                    ruleMatched = true;
                }
            }

            // Asset textures are in the AET container named by the first 10 characters of the texture name.
            // These are loaded after the models since many models use textures from other assets.
            if (CFG.Current.MapEditor_TextureLoad_Objects &&
                project.Descriptor.ProjectType is ProjectType.ER 
                or ProjectType.AC6 
                or ProjectType.NR
                or ProjectType.DB_NT
                or ProjectType.DB &&
                name.StartsWith("aet") && name.Length >= 10)
            {
                ruleMatched |= AddIfExists(project, pool, vfsPaths, $"aet/{name.Substring(0, 10)}/tex");
            }

            // Character textures whose container is named after a different character than the model
            // e.g. the chrbnd is c2025 but the texbnd is c2029
            if (CFG.Current.MapEditor_TextureLoad_Characters)
            {
                Match m = ChrIdPrefix.Match(name);
                if (m.Success)
                {
                    ruleMatched |= AddIfExists(project, pool, vfsPaths, $"chr/{m.Groups[1].Value}/tex");
                }
            }

            // Last: the shipped texture name index, for textures whose container nothing above can name (DES c6050's
            // head textures are only in an m06 area TPF). Only when no rule above has a container still to load for
            // it. A container is tried until its load delivers, so one that fails isn't retried every pass.
            if (!ruleMatched)
            {
                lock (tried)
                {
                    var container = TextureNameIndex.ChooseContainer(project, pool, name, tried, indexPaths);
                    if (container != null && tried.Add(container) && indexPaths.Add(container) && vfsPaths.Add(container))
                    {
                        indexed++;
                    }
                }
            }
        }

        if (indexed > 0)
        {
            Smithbox.Log(typeof(ResourceFallbacks), $"Resource pool '{pool.Name}': loading {indexed} texture containers found by the texture name index.");
        }

        LoadOptions options = new()
        {
            Types = ResourceType.Texture, JobName = LOC.Get("REND_Job_Additional_Textures")
        };

        List<Task> loads = new();

        // Containers a rule names that are loaded already are skipped by the pool, so only the rest count
        foreach (var path in vfsPaths)
        {
            if (!pool.IsContainerLoaded(path))
            {
                queued++;
            }
        }

        queued += externalPaths.Count;

        if (vfsPaths.Count > 0)
        {
            Task load = pool.LoadContainers(vfsPaths, options);

            // A container that delivered no longer counts as tried. While it stays loaded ChooseContainer passes over
            // its textures anyway; once its textures are unloaded (the model was closed) it must be loadable again,
            // or the next model that needs them never gets them.
            if (indexPaths.Count > 0)
            {
                load = load.ContinueWith(_ =>
                {
                    lock (tried)
                    {
                        foreach (var path in indexPaths)
                        {
                            if (pool.IsContainerLoaded(path))
                            {
                                tried.Remove(path);
                            }
                        }
                    }
                }, TaskScheduler.Default);
            }

            loads.Add(load);
        }

        foreach (var path in externalPaths)
        {
            loads.Add(pool.LoadExternalContainer(path, options));
        }

        return Task.WhenAll(loads);
    }

    /// <summary>
    ///     Loads the world map textures that ship with Smithbox
    /// </summary>
    public static void LoadWorldMapTextures(ResourcePool pool)
    {
        ProjectEntry? project = Smithbox.Orchestrator.SelectedProject;
        // ER's world map is built from the game's own map tiles (WorldMapTileSet), so only the others use the bundled image
        if (project == null || project.Descriptor.ProjectType is not (ProjectType.NR
                or ProjectType.DB_NT
                or ProjectType.DB))
        {
            return;
        }

        LoadSmithboxTextures(pool, project, "smithbox/world_map", LOC.Get("REND_Job_World_Map_Textures"));
    }

    /// <summary>
    ///     Loads the compass textures that ship with Smithbox
    /// </summary>
    public static void LoadCompassTextures(ResourcePool pool)
    {
        ProjectEntry? project = Smithbox.Orchestrator.SelectedProject;
        if (project == null)
        {
            return;
        }

        LoadSmithboxTextures(pool, project, "smithbox/compass", LOC.Get("REND_Job_Compass_Textures"));
    }

    private static void LoadSmithboxTextures(ResourcePool pool, ProjectEntry project, string virtualPath, string jobName)
    {
        var absPath = PathBuilder.GetAbsolutePath(project, virtualPath);
        if (absPath == null || !File.Exists(absPath) || pool.IsContainerLoaded(absPath))
        {
            return;
        }

        pool.LoadExternalContainer(absPath, new LoadOptions { Types = ResourceType.Texture, JobName = jobName });
    }

    /// <summary>
    ///     Queues the container a virtual path names, if it exists. True when it does and isn't loaded yet, i.e. the
    ///     rule still has something to try for the texture.
    /// </summary>
    private static bool AddIfExists(ProjectEntry project, ResourcePool pool, HashSet<string> paths, string virtualPath)
    {
        var relative = PathBuilder.GetRelativePath(project, virtualPath);
        if (string.IsNullOrEmpty(relative) || !project.VFS.FS.FileExists(relative))
        {
            return false;
        }

        paths.Add(relative);
        return !pool.IsContainerLoaded(relative);
    }
}
