#nullable enable
using StudioCore.Application;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace StudioCore.Renderer;

/// <summary>
///     Game specific "where might this missing resource be" rules. Used when something asked a pool for a texture
///     that no loaded container has: some games keep a model's textures in a container that nothing has loaded.
///     Everything here is driven by the texture's name.
/// </summary>
internal static class ResourceFallbacks
{
    private static readonly Regex ChrIdPrefix = new(@"^(c\d{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    ///     For every texture the pool has been asked for but doesn't have, tries to find a container that holds it.
    /// </summary>
    public static void LoadMissingTextures(ResourcePool pool)
    {
        ProjectEntry? project = Smithbox.Orchestrator.SelectedProject;
        if (project == null)
        {
            return;
        }

        HashSet<string> vfsPaths = new();
        HashSet<string> externalPaths = new();

        foreach (IResourceHandle handle in pool.Handles)
        {
            if (handle.IsLoaded() || handle.ResourceType != typeof(TextureResource))
            {
                continue;
            }

            var name = handle.Name;

            // DS1: map textures can be loose TPFs named after the texture
            if (CFG.Current.MapEditor_TextureLoad_MapPieces && project.Descriptor.ProjectType is ProjectType.DS1)
            {
                var path = Path.Join(project.Descriptor.DataPath, "map", "tx", $"{name}.tpf");
                if (File.Exists(path))
                {
                    externalPaths.Add(path);
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
                AddIfExists(project, vfsPaths, $"aet/{name.Substring(0, 10)}/tex");
            }

            // Character textures whose container is named after a different character than the model
            // e.g. the chrbnd is c2025 but the texbnd is c2029
            if (CFG.Current.MapEditor_TextureLoad_Characters)
            {
                Match m = ChrIdPrefix.Match(name);
                if (m.Success)
                {
                    AddIfExists(project, vfsPaths, $"chr/{m.Groups[1].Value}/tex");
                }
            }
        }

        LoadOptions options = new()
        {
            Types = ResourceType.Texture, JobName = LOC.Get("REND_Job_Additional_Textures")
        };

        if (vfsPaths.Count > 0)
        {
            pool.LoadContainers(vfsPaths, options);
        }

        foreach (var path in externalPaths)
        {
            pool.LoadExternalContainer(path, options);
        }
    }

    /// <summary>
    ///     Loads the world map textures that ship with Smithbox
    /// </summary>
    public static void LoadWorldMapTextures(ResourcePool pool)
    {
        ProjectEntry? project = Smithbox.Orchestrator.SelectedProject;
        if (project == null || project.Descriptor.ProjectType is not (ProjectType.ER 
                or ProjectType.NR 
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

    private static void AddIfExists(ProjectEntry project, HashSet<string> paths, string virtualPath)
    {
        var relative = PathBuilder.GetRelativePath(project, virtualPath);
        if (!string.IsNullOrEmpty(relative) && project.VFS.FS.FileExists(relative))
        {
            paths.Add(relative);
        }
    }
}
