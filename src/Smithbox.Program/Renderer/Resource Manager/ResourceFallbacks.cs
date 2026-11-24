#nullable enable
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    // The SFX textures of DES and DS1 are named sNNNNN, and live in the SFX binders (ffxbnd)
    private static readonly Regex SfxTexture = new(@"^s\d{5}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // The loads of an SFX texture's own copy that the SFX rule has replaced, so it asks once for each. A later load of
    // the model's copy (after it was unloaded) is a new source, and is replaced again
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ResourceLoadContext, object> _sfxReplaced = new();

    // The containers each pool has loaded for the texture name index, so a holder that didn't deliver isn't retried
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ResourcePool, HashSet<string>> _indexTried = new();

    // The (container, texture) pairs each pool has loaded for the name rules and which haven't delivered (yet). A rule's
    // container is loaded for just the textures asked of it, so it never counts as loaded as a whole: this is what
    // stops a container that doesn't have a texture from being read for it again on every pass.
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ResourcePool, HashSet<string>> _ruleTried = new();

    private static string RulePair(string container, string name) => $"{container}|{name}";

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

        // The containers the name rules named, with the textures each is asked for
        Dictionary<string, HashSet<string>> ruleNames = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> ruleTried = _ruleTried.GetValue(pool, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));

        // A pair that delivered no longer counts as tried, so that once its texture is unloaded (the model was closed)
        // the next model that needs it gets it again. Done here rather than when the load ends, as a pass runs once
        // every job is done, by when the textures of the last one are registered.
        lock (ruleTried)
        {
            ruleTried.RemoveWhere(pair =>
            {
                var bar = pair.LastIndexOf('|');
                return bar >= 0 && pool.IsLoaded<TextureResource>(pair.Substring(bar + 1));
            });
        }

        HashSet<string> tried = _indexTried.GetValue(pool, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        var indexed = 0;

        // The containers this pass loads for the index, so textures that share a holder don't each load another one
        HashSet<string> indexPaths = new(StringComparer.OrdinalIgnoreCase);

        // The SFX binders to load from, with the textures each replaces
        Dictionary<string, HashSet<string>> sfxNames = new(StringComparer.OrdinalIgnoreCase);
        List<IResourceHandle> sfxReplaced = new();

        foreach (IResourceHandle handle in pool.Handles)
        {
            if (handle.ResourceType != typeof(TextureResource))
            {
                continue;
            }

            // An SFX texture is drawn with its SFX binder's copy, also where a model has its own: the game has the
            // binders resident, and their copy wins. DS1R c2811's chrbnd holds a solid magenta s03011 for its boulder,
            // whose real texture is in sfx/FRPG_SfxBnd_c4100 and _m14. Only for holders the texture name index lists
            if (handle.IsLoaded() && SfxTexture.IsMatch(handle.Name) && handle.Source is { } source &&
                !IsSfxContainer(source.ContainerPath) && !_sfxReplaced.TryGetValue(source, out _))
            {
                var holder = TextureNameIndex.Get(project.Descriptor.ProjectType)?.Find(handle.Name)
                    .Where(c => IsSfxContainer(c.Path) && project.VFS.FS.FileExists(c.Path))
                    .OrderBy(c => c.Rank).ThenBy(c => c.Path, StringComparer.OrdinalIgnoreCase)
                    .Select(c => c.Path).FirstOrDefault();

                _sfxReplaced.AddOrUpdate(source, new object());

                if (holder != null)
                {
                    if (!sfxNames.TryGetValue(holder, out var names))
                    {
                        names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        sfxNames.Add(holder, names);
                    }

                    names.Add(handle.Name);
                    sfxReplaced.Add(handle);
                }
            }

            if (handle.IsLoaded())
            {
                continue;
            }

            var name = handle.Name;

            // Whether a rule below names a container for this texture that isn't loaded yet. A rule whose container
            // is loaded already, while the texture is still missing, named the wrong one, so the index gets a turn.
            var ruleMatched = false;

            // DS1: map textures can be loose TPFs named after the texture
            if (CFG.Current.MapEditor.MapEditor_TextureLoad_MapPieces && project.Descriptor.ProjectType is ProjectType.DS1)
            {
                var path = Path.Join(project.Descriptor.DataPath, "map", "tx", $"{name}.tpf");
                if (File.Exists(path) && !pool.IsContainerLoaded(path))
                {
                    externalPaths.Add(path);
                    ruleMatched = true;
                }
            }

            // DS1: unpacked, a character's textures are loose TPFs named after the texture, in the character's folder
            // (chr/c5220/c5220_01.tpf), which the packed game keeps in its chrtpfbhd. The project's copy wins
            if (CFG.Current.MapEditor.MapEditor_TextureLoad_Characters && project.Descriptor.ProjectType is ProjectType.DS1)
            {
                Match chr = ChrIdPrefix.Match(name);
                if (chr.Success)
                {
                    foreach (var root in new[] { project.Descriptor.ProjectPath, project.Descriptor.DataPath })
                    {
                        var path = Path.Join(root, "chr", chr.Groups[1].Value, $"{name}.tpf");
                        if (!File.Exists(path))
                        {
                            continue;
                        }

                        if (!pool.IsContainerLoaded(path))
                        {
                            externalPaths.Add(path);
                            ruleMatched = true;
                        }

                        break;
                    }
                }
            }

            // Asset textures are in the AET container named by the first 10 characters of the texture name.
            // These are loaded after the models since many models use textures from other assets.
            if (CFG.Current.MapEditor.MapEditor_TextureLoad_Objects &&
                project.Descriptor.ProjectType is ProjectType.ER 
                or ProjectType.AC6 
                or ProjectType.NR
                or ProjectType.DB_NT
                or ProjectType.DB &&
                name.StartsWith("aet") && name.Length >= 10)
            {
                ruleMatched |= AddRuleContainer(project, pool, ruleNames, ruleTried, $"aet/{name.Substring(0, 10)}/tex", name);
            }

            // Character textures whose container is named after a different character than the model
            // e.g. the chrbnd is c2025 but the texbnd is c2029
            if (CFG.Current.MapEditor.MapEditor_TextureLoad_Characters)
            {
                Match m = ChrIdPrefix.Match(name);
                if (m.Success)
                {
                    ruleMatched |= AddRuleContainer(project, pool, ruleNames, ruleTried, $"chr/{m.Groups[1].Value}/tex", name);
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

        // Containers a rule names that are loaded already are skipped by the pool, so only the rest count
        foreach (var path in vfsPaths)
        {
            if (!pool.IsContainerLoaded(path))
            {
                queued++;
            }
        }

        queued += externalPaths.Count + ruleNames.Count + sfxNames.Count;

        List<Task> loads = new();

        // The SFX copies replace the loaded ones, which give up their priority for it
        if (sfxNames.Count > 0)
        {
            pool.ReleasePriority(sfxReplaced);

            foreach (var (path, names) in sfxNames)
            {
                LoadOptions sfxOptions = options.Clone();
                sfxOptions.OnlyNames = names;
                sfxOptions.Reload = true;

                loads.Add(pool.LoadContainer(path, sfxOptions));
            }
        }

        // Only the asked for textures are loaded from a rule's container: an AET or character container holds the
        // textures of many models, and a single one of them is often all that is missing
        foreach (var (path, names) in ruleNames)
        {
            lock (ruleTried)
            {
                foreach (var name in names)
                {
                    ruleTried.Add(RulePair(path, name));
                }
            }

            LoadOptions ruleOptions = options.Clone();
            ruleOptions.OnlyNames = names;

            loads.Add(pool.LoadContainer(path, ruleOptions));
        }

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

    private static bool IsSfxContainer(string? path)
    {
        return path != null && path.Contains(".ffxbnd", StringComparison.OrdinalIgnoreCase);
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
    ///     Loads the compass texture that ships with Smithbox. It is a loose DDS in the Assets folder, read directly so
    ///     that no Oodle (DCX_KRAK) is needed: the texture is wrapped in an uncompressed in-memory TPF for the pool.
    /// </summary>
    public static void LoadCompassTextures(ResourcePool pool)
    {
        const string containerName = "smithbox_compass.tpf";

        var absPath = Path.Combine(AppContext.BaseDirectory, "Assets", "MSB", "compass.dds");
        if (!File.Exists(absPath) || pool.IsContainerLoaded(containerName))
        {
            return;
        }

        byte[] dds;
        try
        {
            dds = File.ReadAllBytes(absPath);
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(ResourceFallbacks), LOC.Get("REND_Resource_Load_Failed", absPath), e);
            return;
        }

        // The texture is named "compass", as the TPF it used to ship in had it (smithbox/compass/compass)
        var tpf = new SoulsFormats.TPF();
        tpf.Textures.Add(new SoulsFormats.TPF.Texture("compass", 0, 0, dds, SoulsFormats.TPF.TPFPlatform.PC));

        pool.LoadContainerData(containerName, tpf.Write(),
            new LoadOptions { Types = ResourceType.Texture, JobName = LOC.Get("REND_Job_Compass_Textures") });
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
    ///     Asks the container a virtual path names for a texture, if the container exists. True when it does and it
    ///     hasn't been asked for this texture already (or is loaded in full), i.e. the rule still has something to try
    ///     for the texture.
    /// </summary>
    private static bool AddRuleContainer(ProjectEntry project, ResourcePool pool,
        Dictionary<string, HashSet<string>> ruleNames, HashSet<string> ruleTried, string virtualPath, string name)
    {
        var relative = PathBuilder.GetRelativePath(project, virtualPath);
        if (string.IsNullOrEmpty(relative) || !project.VFS.FS.FileExists(relative))
        {
            return false;
        }

        // Loaded in full, or already asked for this texture: either way the texture isn't in it
        if (pool.IsContainerLoaded(relative))
        {
            return false;
        }

        lock (ruleTried)
        {
            if (ruleTried.Contains(RulePair(relative, name)))
            {
                return false;
            }
        }

        if (!ruleNames.TryGetValue(relative, out var names))
        {
            names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ruleNames.Add(relative, names);
        }

        names.Add(name);
        return true;
    }
}
