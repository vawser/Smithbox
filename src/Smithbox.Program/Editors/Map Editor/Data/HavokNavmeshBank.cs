using Havok.Shared;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using Tracy;

namespace StudioCore.Editors.MapEditor;

public class HavokNavmeshBank
{
    public ProjectEntry Project;

    public Dictionary<string, NVA> Files = new();
    public Dictionary<string, NVA_ER> ER_Files = new();

    /// <summary>
    /// AC6 nav volumes for flying movement (.nvva), which are laid out like the AC6 NVA
    /// </summary>
    public Dictionary<string, NVA_ER> NavVolumeFiles = new();

    /// <summary>
    /// ER, NR and AC6 navmeshes, read by the unified Havok library, keyed by name (n30_00_00_00_000100). The Map Editor
    /// edits these and SaveHavokNavmeshModels writes them back.
    /// </summary>
    public Dictionary<string, Havok.HavokDocument> Documents = new();

    /// <summary>
    /// The loaded ER, NR or AC6 maps whose navmeshes haven't been read yet
    /// </summary>
    private HashSet<string> UnreadMaps = new();

    /// <summary>
    /// The navmeshes read for each map, so they can be released when it is unloaded
    /// </summary>
    private Dictionary<string, List<string>> MapNavmeshes = new();

    public HavokNavmeshBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse()
    {
        // The games whose navmeshes are Havok (hkaiNavMesh), which the unified Havok library reads
        if (Project.Descriptor.ProjectType is ProjectType.DS3
            or ProjectType.BB
            or ProjectType.SDT
            or ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
            return true;

        return false;
    }

    /// <summary>
    /// The viewport reads its own navmeshes, so the navmeshes of a map are only read here when one is first asked for.
    /// </summary>
    public void OnLoadMap(string mapId)
    {
        if (Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR)
        {
            UnreadMaps.Add(mapId);
        }
    }

    /// <summary>
    /// Gets an ER, NR or AC6 navmesh, e.g. n30_00_00_00_000100, reading the navmeshes of its map the first time
    /// one of them is asked for. Null if the navmesh isn't in a loaded map.
    /// </summary>
    public Havok.HavokDocument GetDocument(string name)
    {
        // Navmeshes are named after their map, e.g. n30_00_00_00_000100 is in m30_00_00_00
        foreach (var mapId in UnreadMaps.Where(m => name.AsSpan(1).StartsWith(m.AsSpan(1))).ToList())
        {
            UnreadMaps.Remove(mapId);
            ReadHavokNavmeshModels(mapId);
        }

        return Documents.GetValueOrDefault(name);
    }

    /// <summary>
    /// The names of an ER, NR or AC6 map's navmesh files, reading them the first time they are asked for.
    /// Empty if the map isn't loaded or Havok loading is skipped.
    /// </summary>
    public IReadOnlyList<string> GetMapNavmeshNames(string mapId)
    {
        if (UnreadMaps.Remove(mapId))
        {
            ReadHavokNavmeshModels(mapId);
        }

        return MapNavmeshes.TryGetValue(mapId, out var names) ? names : Array.Empty<string>();
    }

    /// <summary>
    /// Whether a map's navmesh files can be read: it is loaded with Havok, or they have been read already.
    /// </summary>
    public bool HasMapNavmeshes(string mapId) => UnreadMaps.Contains(mapId) || MapNavmeshes.ContainsKey(mapId);

    /// <summary>The navmesh files an edit took out of a map, by map, so its save drops them from the binder.</summary>
    private readonly Dictionary<string, HashSet<string>> RemovedNavmeshes = new();

    /// <summary>
    /// Puts a navmesh document in a map in place of the one it has under that name, or adds it as a new file of the
    /// map's binder; null takes the file out. Used by the Navigation tool's model edits, which build new documents
    /// rather than change the root of a loaded one. The map's save then writes, adds or drops the file.
    /// </summary>
    public void SetDocument(string mapId, string name, Havok.HavokDocument document)
    {
        // The map's files are read first, so the new document isn't replaced by a later read
        GetMapNavmeshNames(mapId);

        var names = MapNavmeshes.TryGetValue(mapId, out var list) ? list : MapNavmeshes[mapId] = new();
        var removed = RemovedNavmeshes.TryGetValue(mapId, out var set) ? set : RemovedNavmeshes[mapId] = new();

        if (document == null)
        {
            Documents.Remove(name);
            names.Remove(name);
            removed.Add(name);
            return;
        }

        Documents[name] = document;
        removed.Remove(name);

        if (!names.Contains(name))
            names.Add(name);
    }

    public void OnUnloadMap(string mapId)
    {
        UnreadMaps.Remove(mapId);
        RemovedNavmeshes.Remove(mapId);

        if (MapNavmeshes.Remove(mapId, out var navmeshNames))
        {
            foreach (var name in navmeshNames)
            {
                Documents.Remove(name);
            }
        }

        if (Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR)
        {
            foreach (ResourcePool pool in ResourceManager.Pools)
            {
                foreach (IResourceHandle handle in pool.Handles)
                {
                    if (handle.ResourceType == typeof(HavokNavmeshResource))
                    {
                        handle.Release(true);
                    }
                }
            }
        }
    }

    private void ReadHavokNavmeshModels(string mapId)
    {
        using var __scope = Profiler.TracyZoneAuto();

        var binderEntry = Project.Locator.FileDictionary.Entries.FirstOrDefault(
            e => e.Filename == mapId &&
            e.Extension == "nvmhktbnd");

        if (binderEntry == null)
            return;

        if (Project.VFS.FS.FileExists(binderEntry.Path))
        {
            try
            {
                var binderData = Project.VFS.FS.ReadFile(binderEntry.Path);

                if (Project.VFS.ProjectFS.FileExists(binderEntry.Path))
                {
                    binderData = Project.VFS.ProjectFS.ReadFile(binderEntry.Path);
                }

                if (binderData == null)
                    return;

                using var binder = new BND4Reader(binderData.Value);

                // The binder reads its files through one position, so they are taken out here and then read in
                // parallel, as they don't depend on each other. Each file holds its own types.
                var files = binder.Files
                    .Select(file => (Name: Path.GetFileNameWithoutExtension(file.Name), Bytes: binder.ReadFile(file)))
                    .ToList();

                var profile = Havok.HavokProfile.FromGame(Project.Descriptor.ProjectType.ToString());

                if (profile == null)
                    return;

                var documents = new Havok.HavokDocument[files.Count];

                Parallel.For(0, files.Count, i =>
                {
                    try
                    {
                        var bytes = DCX.Is(files[i].Bytes) ? DCX.Decompress(files[i].Bytes) : files[i].Bytes;

                        documents[i] = Havok.Files.HavokFile.Read(bytes.ToArray(), profile);
                    }
                    catch (Exception ex)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_NVA_HKX", files[i].Name), ex);
                    }
                });

                var navmeshNames = MapNavmeshes.TryGetValue(mapId, out var names) ? names : MapNavmeshes[mapId] = new();

                for (int i = 0; i < files.Count; i++)
                {
                    var added = documents[i] != null && Documents.TryAdd(files[i].Name, documents[i]);

                    if (added)
                        navmeshNames.Add(files[i].Name);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_NVA_HKXBND", binderEntry.Path), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_NVA_HKXBND", binderEntry.Path));
        }
    }

    /// <summary>
    /// Writes a loaded navmesh file in the form it was read, compressed like the stored file. Null if the file isn't loaded.
    /// </summary>
    private byte[] WriteNavmeshFile(string name, Memory<byte> stored)
    {
        if (!Documents.TryGetValue(name, out var document))
            return null;

        var bytes = Havok.Files.HavokFile.Write(document);

        if (!DCX.Is(stored))
            return bytes;

        DCX.Decompress(stored, out DCX.Type compression);

        return DCX.Compress(bytes, compression).ToArray();
    }

    /// <summary>
    /// A new file entry for a navmesh binder, laid out like the game's: the path of the binder's other files, their
    /// flags, and the next ID of the file's block (10000 + for n, 20000 + for o, 40000 + for q).
    /// </summary>
    private static BinderFile NewBinderFile(BND4 binder, string name, byte[] bytes)
    {
        int block = name[0] switch { 'n' => 10000, 'o' => 20000, 'q' => 40000, _ => 90000 };
        int id = binder.Files.Where(f => f.ID >= block && f.ID < block + 10000).Select(f => f.ID + 1).DefaultIfEmpty(block).Max();

        var template = binder.Files.FirstOrDefault();
        var directory = template == null ? "" : template.Name.Substring(0, template.Name.LastIndexOf('\\') + 1);

        return new BinderFile(template?.Flags ?? Binder.FileFlags.Flag1, id, $"{directory}{name}.hkx", bytes);
    }

    public bool SaveHavokNavmeshModels(string mapId)
    {
        var success = true;

        var binderEntry = Project.Locator.FileDictionary.Entries.FirstOrDefault(
            e => e.Filename == mapId &&
            e.Extension == "nvmhktbnd");

        if (binderEntry == null)
            return success;

        if (Project.VFS.FS.FileExists(binderEntry.Path))
        {
            try
            {
                var binderData = Project.VFS.FS.ReadFile(binderEntry.Path);

                if (Project.VFS.ProjectFS.FileExists(binderEntry.Path))
                {
                    binderData = Project.VFS.ProjectFS.ReadFile(binderEntry.Path);
                }

                if (binderData == null)
                    return success;

                var binder = BND4.Read(binderData.Value);

                bool anyWritten = false;

                foreach (var file in binder.Files)
                {
                    var name = Path.GetFileNameWithoutExtension(file.Name);

                    if (!file.Name.Contains(".hkx"))
                        continue;

                    try
                    {
                        // Only re-serialize entries we actually have loaded (and presumably edited)
                        var fileBytes = WriteNavmeshFile(name, file.Bytes);
                        if (fileBytes == null)
                            continue;

                        file.Bytes = fileBytes;
                        anyWritten = true;
                    }
                    catch (Exception ex)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Serialize_NVA_HKX", name), ex);
                        success = false;
                    }
                }

                // Files a model edit took out, and loaded files the binder doesn't have yet
                if (RemovedNavmeshes.TryGetValue(mapId, out var removed) && removed.Count > 0)
                {
                    if (binder.Files.RemoveAll(f => removed.Contains(Path.GetFileNameWithoutExtension(f.Name))) > 0)
                        anyWritten = true;
                }

                if (MapNavmeshes.TryGetValue(mapId, out var loadedNames))
                {
                    var present = binder.Files.Select(f => Path.GetFileNameWithoutExtension(f.Name)).ToHashSet();

                    foreach (var name in loadedNames.Where(n => !present.Contains(n)).ToList())
                    {
                        try
                        {
                            binder.Files.Add(NewBinderFile(binder, name, WriteNavmeshFile(name, Memory<byte>.Empty)));
                            anyWritten = true;
                        }
                        catch (Exception ex)
                        {
                            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Serialize_NVA_HKX", name), ex);
                            success = false;
                        }
                    }

                    // The game names its files; the IDs follow its blocks (n 10000+, o 20000+, q 40000+)
                    binder.Files.Sort((a, b) => a.ID.CompareTo(b.ID));
                }

                if (!anyWritten)
                    return success;

                var writtenBinder = binder.Write();

                Project.VFS.ProjectFS.WriteFile(binderEntry.Path, writtenBinder);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_NVA_HKXBND", binderEntry.Path), e);
                success = false;
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_NVA_HKXBND", binderEntry.Path));
            success = false;
        }

        return success;
    }

    public void LoadNVA(MapContainer map, MapResourceHandler handler)
    {
        if (!CanUse())
            return;

        if (Files.ContainsKey(map.Name))
        {
            var nva = Files[map.Name];

            if (nva != null)
            {
                map.LoadNVA(map.Name, nva);
            }
        }
        else if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 && ER_Files.ContainsKey(map.Name))
        {
            var nva = ER_Files[map.Name];

            if (nva != null)
            {
                map.LoadNVA(map.Name, nva);
            }
        }
        else
        {
            var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

            var entry = Project.Locator.NavmeshFiles.Entries.FirstOrDefault(e => e.Filename == map.Name);
            if (entry != null)
            {
                if (fs.FileExists(entry.Path))
                {
                    try
                    {
                        var nvaData = fs.ReadFile(entry.Path);

                        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6)
                        {
                            try
                            {
                                var nva = NVA_ER.Read(nvaData.Value);

                                ER_Files.Add(Path.GetFileNameWithoutExtension(entry.Filename), nva);

                                map.LoadNVA(map.Name, nva);

                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_NVA", entry.Path), e);
                            }
                        }
                        else
                        {
                            try
                            {
                                var nva = NVA.Read(nvaData.Value);

                                Files.Add(Path.GetFileNameWithoutExtension(entry.Filename), nva);

                                map.LoadNVA(map.Name, nva);

                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_NVA", entry.Path), e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_VPS", entry.Path), e);
                    }
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_VPS", entry.Path));
                }
            }
        }
    }

    public bool SaveNVA(MapContainer map)
    {
        var success = true;

        if (!CanUse())
            return success;

        // The navmesh entity holds the whole file, which its properties edit directly
        var root = map.NavmeshParent?.WrappedObject;

        if (root == null)
            return success;

        foreach (var entry in Project.Locator.NavmeshFiles.Entries)
        {
            if (entry.Filename != map.Name)
                continue;

            try
            {
                switch (root)
                {
                    case NVA_ER nvaER:
                        WriteNavmeshFile(entry.Path, nvaER.Write);
                        break;
                    case NVA nva:
                        WriteNavmeshFile(entry.Path, nva.Write);
                        break;
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_NVA", entry.Path), e);
                success = false;
            }
        }

        return success;
    }

    public void LoadNVVA(MapContainer map)
    {
        if (Project.Descriptor.ProjectType is not ProjectType.AC6)
            return;

        if (NavVolumeFiles.TryGetValue(map.Name, out var cached))
        {
            if (cached != null)
            {
                map.LoadNVVA(map.Name, cached);
            }

            return;
        }

        var entry = Project.Locator.NavVolumeFiles.Entries.FirstOrDefault(e => e.Filename == map.Name);

        // Not every map has a nav volume
        if (entry == null)
            return;

        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        if (!fs.FileExists(entry.Path))
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_VPS", entry.Path));
            return;
        }

        try
        {
            var nvva = NVA_ER.Read(fs.ReadFile(entry.Path).Value);

            NavVolumeFiles.Add(entry.Filename, nvva);

            map.LoadNVVA(map.Name, nvva);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_NVA", entry.Path), e);
        }
    }

    public bool SaveNVVA(MapContainer map)
    {
        if (Project.Descriptor.ProjectType is not ProjectType.AC6 || map.NavVolumeParent?.WrappedObject is not NVA_ER nvva)
            return true;

        var entry = Project.Locator.NavVolumeFiles.Entries.FirstOrDefault(e => e.Filename == map.Name);

        if (entry == null)
            return true;

        try
        {
            WriteNavmeshFile(entry.Path, nvva.Write);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_NVA", entry.Path), e);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Writes a navmesh file to the project, unless it is the same as the file it replaces
    /// </summary>
    private void WriteNavmeshFile(string path, Func<byte[]> write)
    {
        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        if (!fs.FileExists(path))
            return;

        var fileData = fs.ReadFile(path);
        var newFileData = write();

        if (!BytePerfectHelper.Md5Equal(fileData.Value.ToArray(), newFileData))
        {
            Project.VFS.ProjectFS.WriteFile(path, newFileData);
        }
    }
}
