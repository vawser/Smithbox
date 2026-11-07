using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Elden Ring navigation data of the loaded maps, joined as a <see cref="NavigationMap"/>: the map's NVA (the
/// entity the Map Editor edits), its navmesh files (read by <see cref="HavokNavmeshBank"/>), its ONAV and the NVC
/// files that connect it to its neighbours. The ONAV and NVC files are read here, the first time a map is asked for,
/// and saved with the map when an edit marks them (<see cref="MarkEdited"/>).
/// </summary>
public class NavigationBank
{
    private readonly ProjectEntry Project;

    /// <summary>The ONAV of each map read so far; null for a map without one.</summary>
    private readonly Dictionary<string, ONAV> OnavFiles = new();

    /// <summary>The NVC files read so far, by name, e.g. m60_33_41_00_m60_33_42_00.</summary>
    private readonly Dictionary<string, NvcFile> NvcFiles = new();

    /// <summary>The NVC and ONAV files an edit has changed, by name, until they are saved.</summary>
    private readonly HashSet<string> EditedNvcs = new();
    private readonly HashSet<string> EditedOnavs = new();

    /// <summary>The navmesh files built from each Havok document, so they aren't searched again.</summary>
    private readonly Dictionary<Havok.HavokDocument, NavmeshFile> FileCache = new();

    public NavigationBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse() => Project.Descriptor.ProjectType is ProjectType.ER;

    private HavokNavmeshBank NavmeshBank => Project.Handler.MapData.HavokNavmeshBank;

    private Andre.IO.VFS.VirtualFileSystem FS => Project.Handler.MapData.PrimaryBank.TargetFS;

    /// <summary>
    /// The navigation data of a loaded map, or null if its NVA isn't loaded. Reads its navmesh files, ONAV and NVC
    /// files the first time. Built anew on each call, so it reflects edits to the NVA's navmesh list.
    /// </summary>
    public NavigationMap GetMap(MapContainer map)
    {
        if (!CanUse() || map?.NavmeshParent?.WrappedObject is not NVA_ER nva)
            return null;

        var files = new Dictionary<string, NavmeshFile>();

        foreach (var name in NavmeshBank.GetMapNavmeshNames(map.Name))
        {
            var document = NavmeshBank.GetDocument(name);

            if (document == null)
                continue;

            if (!FileCache.TryGetValue(document, out var file))
            {
                file = NavigationReader.FromDocument(name, document);
                FileCache[document] = file;
            }

            files[name] = file;
        }

        return new NavigationMap(map.Name, nva, files, GetOnav(map.Name), GetNvcs(map.Name));
    }

    /// <summary>
    /// Whether a map's navmesh files can be read, which needs the map to have been loaded with Havok.
    /// </summary>
    public bool HasNavmeshFiles(MapContainer map) => NavmeshBank.HasMapNavmeshes(map.Name);

    /// <summary>The vanilla navigation data read so far, by map; null for a map the game doesn't have.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, NavigationMap> VanillaMaps = new();

    /// <summary>
    /// A map's navigation data as the game ships it, read from the vanilla files. Safe to call from a background task.
    /// </summary>
    public NavigationMap GetVanillaMap(string mapId)
    {
        return VanillaMaps.GetOrAdd(mapId, id =>
        {
            try
            {
                var profile = Havok.HavokProfile.FromGame(Project.Descriptor.ProjectType.ToString());
                return NavigationReader.Read(Project.VFS.VanillaFS, id, profile, NavigationReader.NvcNamesFor(id, NvcNames));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Read", NavigationReader.NvaPath(id)), e);
                return null;
            }
        });
    }

    private List<string> nvcNames;

    /// <summary>The names of every NVC file of the game, e.g. m60_33_41_00_m60_33_42_00.</summary>
    private List<string> NvcNames => nvcNames ??= Project.Locator.FileDictionary.Entries
        .Where(e => e.Extension == "nvc").Select(e => e.Filename).Distinct().ToList();

    /// <summary>
    /// The issues of the vanilla data of a map, checked against the vanilla data of the same neighbours, which
    /// the project's issues are compared with. Safe to call from a background task.
    /// </summary>
    public List<NavIssue> GetVanillaIssues(string mapId, IReadOnlyCollection<string> neighbours)
    {
        var vanilla = GetVanillaMap(mapId);

        if (vanilla == null)
            return new List<NavIssue>();

        return NavigationValidator.Validate(vanilla, id => neighbours.Contains(id) ? GetVanillaMap(id) : null);
    }

    private ONAV GetOnav(string mapId)
    {
        if (OnavFiles.TryGetValue(mapId, out var cached))
            return cached;

        ONAV onav = null;
        var entry = Project.Locator.FileDictionary.Entries.FirstOrDefault(e => e.Extension == "onav" && e.Filename == mapId);

        if (entry != null && FS.FileExists(entry.Path))
        {
            try
            {
                onav = ONAV.Read(FS.ReadFile(entry.Path).Value);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Read", entry.Path), e);
            }
        }

        OnavFiles[mapId] = onav;
        return onav;
    }

    private List<NvcFile> GetNvcs(string mapId)
    {
        var nvcs = new List<NvcFile>();

        // Named after both maps, so a map's files have its name as either half
        var names = NavigationReader.NvcNamesFor(mapId, NvcNames).ToHashSet();
        var entries = Project.Locator.FileDictionary.Entries.Where(e => e.Extension == "nvc" && names.Contains(e.Filename));

        foreach (var entry in entries)
        {
            if (!NvcFiles.TryGetValue(entry.Filename, out var nvc))
            {
                if (!FS.FileExists(entry.Path))
                    continue;

                try
                {
                    nvc = new NvcFile { Name = entry.Filename, Nvc = NVC.Read(FS.ReadFile(entry.Path).Value) };
                    NvcFiles[entry.Filename] = nvc;
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Read", entry.Path), e);
                    continue;
                }
            }

            nvcs.Add(nvc);
        }

        return nvcs;
    }

    /// <summary>
    /// Marks what a remap or an edit changed, so the map's save writes those files.
    /// </summary>
    public void MarkEdited(MapContainer map, NavRemapResult result)
    {
        if (result.NvaChanged)
            map.MarkDirty(MapFileType.NVA);

        foreach (var nvc in result.NvcsChanged)
        {
            EditedNvcs.Add(nvc.Name);
            map.MarkDirty(MapFileType.NVC);
        }
    }

    /// <summary>The navmesh bank's current document of a file, or null if it has none (e.g. its map was unloaded).</summary>
    public Havok.HavokDocument GetNavmeshDocument(string name) => NavmeshBank.GetDocument(name);

    /// <summary>
    /// Puts a navmesh document in a map's navmesh files under a name, in place of the one it has, or as a new file;
    /// null takes the file out. The next GetMap sees it.
    /// </summary>
    public void SetNavmeshDocument(MapContainer map, string name, Havok.HavokDocument document)
    {
        NavmeshBank.SetDocument(map.Name, name, document);
    }

    /// <summary>
    /// Marks a map's ONAV as changed, so the map's save writes it.
    /// </summary>
    public void MarkOnavEdited(MapContainer map)
    {
        EditedOnavs.Add(map.Name);
        map.MarkDirty(MapFileType.ONAV);
    }

    /// <summary>
    /// Writes the edited NVC files that name the map. A file shared with a neighbour is written once, by
    /// whichever map is saved first.
    /// </summary>
    public bool SaveNVC(MapContainer map)
    {
        if (!CanUse())
            return true;

        var success = true;

        foreach (var name in EditedNvcs.Where(n => n.StartsWith(map.Name) || n.EndsWith(map.Name)).ToList())
        {
            var entry = Project.Locator.FileDictionary.Entries.FirstOrDefault(e => e.Extension == "nvc" && e.Filename == name);

            if (entry == null || !NvcFiles.TryGetValue(name, out var nvc))
                continue;

            try
            {
                WriteFile(entry.Path, nvc.Nvc.Write);
                EditedNvcs.Remove(name);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Write", entry.Path), e);
                success = false;
            }
        }

        return success;
    }

    public bool SaveONAV(MapContainer map)
    {
        if (!CanUse() || !EditedOnavs.Contains(map.Name) || OnavFiles.GetValueOrDefault(map.Name) is not ONAV onav)
            return true;

        var entry = Project.Locator.FileDictionary.Entries.FirstOrDefault(e => e.Extension == "onav" && e.Filename == map.Name);

        if (entry == null)
            return true;

        try
        {
            WriteFile(entry.Path, onav.Write);
            EditedOnavs.Remove(map.Name);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Write", entry.Path), e);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Writes a file to the project, unless it is the same as the file it replaces
    /// </summary>
    private void WriteFile(string path, Func<byte[]> write)
    {
        var newData = write();

        if (FS.FileExists(path) && BytePerfectHelper.Md5Equal(FS.ReadFile(path).Value.ToArray(), newData))
            return;

        Project.VFS.ProjectFS.WriteFile(path, newData);
    }

    /// <summary>
    /// Releases a map's ONAV and the NVC files no loaded map names any more. Files with unsaved edits are kept.
    /// </summary>
    public void OnUnloadMap(string mapId, IEnumerable<string> loadedMaps)
    {
        if (!EditedOnavs.Contains(mapId))
            OnavFiles.Remove(mapId);

        var loaded = loadedMaps.Where(m => m != mapId).ToHashSet();

        foreach (var name in NvcFiles.Keys.ToList())
        {
            var nvc = NvcFiles[name];

            if (!EditedNvcs.Contains(name) && !loaded.Contains(nvc.MapA) && !loaded.Contains(nvc.MapB))
                NvcFiles.Remove(name);
        }

        // Documents of the unloaded map are released by the navmesh bank
        foreach (var document in FileCache.Keys.Where(d => d.Objects.Count == 0 || !NavmeshBank.Documents.ContainsValue(d)).ToList())
            FileCache.Remove(document);
    }
}
