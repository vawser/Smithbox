using Havok.Shared;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using Tracy;

namespace StudioCore.Editors.MapEditor;

public class HavokCollisionBank
{
    public ProjectEntry Project;

    /// <summary>
    /// ER, NR and AC6 collision, read by the unified Havok library with their binder's compendium, keyed by file name
    /// (l30_00_00_00_000100.hkx.dcx). The Map Editor edits these and SaveMapCollisionFiles writes them back.
    /// </summary>
    public Dictionary<string, Havok.HavokDocument> MapDocuments = new();

    /// <summary>
    /// Sekiro collision: tagfiles read by the unified Havok library with their binder's compendium. Keyed by file
    /// name, like MapDocuments.
    /// </summary>
    public Dictionary<string, Havok.HavokDocument> TagContainers = new();

    public Dictionary<string, List<string>> MapCollisions = new();

    /// <summary>
    /// The map and type of the binder each listed ER, NR or AC6 collision file is in, keyed by file name.
    /// </summary>
    private Dictionary<string, (string MapId, string Type)> CollisionBinders = new();

    /// <summary>
    /// The ER, NR or AC6 collision binders which have been read into MapDocuments.
    /// </summary>
    private HashSet<string> ReadBinders = new();

    private bool ObjectCollisionVisible = false;

    public HavokCollisionBank(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>
    /// The collision types the project has, in the order they are preferred when only one can be displayed.
    /// </summary>
    public IEnumerable<HavokCollisionType> GetSupportedCollisionTypes()
    {
        yield return HavokCollisionType.Low;
        yield return HavokCollisionType.High;

        if (Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            yield return HavokCollisionType.FallProtection;
        }
    }

    public bool IsCollisionTypeVisible(HavokCollisionType type)
    {
        return type switch
        {
            HavokCollisionType.Low => CFG.Current.MapEditor_Display_Low_Collision,
            HavokCollisionType.High => CFG.Current.MapEditor_Display_High_Collision,
            HavokCollisionType.FallProtection => CFG.Current.MapEditor_Display_FallProtection_Collision,
            _ => false
        };
    }

    /// <summary>
    /// The collision types displayed for each collision part. Always holds at least one type.
    /// </summary>
    public List<HavokCollisionType> GetVisibleCollisionTypes()
    {
        var types = GetSupportedCollisionTypes().Where(IsCollisionTypeVisible).ToList();

        // A collision part always displays something, so it can still be seen and selected
        if (types.Count == 0)
            types.Add(HavokCollisionType.Low);

        return types;
    }

    /// <summary>
    /// The collision type used where only one can be displayed, e.g. object collision.
    /// </summary>
    public HavokCollisionType PrimaryCollisionType => GetVisibleCollisionTypes()[0];

    /// <summary>
    /// Shows or hides a collision type, then rebuilds the collision meshes. The last visible type can't be hidden.
    /// </summary>
    public void ToggleCollisionType(HavokCollisionType type)
    {
        var visible = !IsCollisionTypeVisible(type);

        if (!visible && GetVisibleCollisionTypes().Count <= 1)
            return;

        switch (type)
        {
            case HavokCollisionType.Low:
                CFG.Current.MapEditor_Display_Low_Collision = visible;
                break;
            case HavokCollisionType.High:
                CFG.Current.MapEditor_Display_High_Collision = visible;
                break;
            case HavokCollisionType.FallProtection:
                CFG.Current.MapEditor_Display_FallProtection_Collision = visible;
                break;
        }

        // The Model Editor displays a single type, which follows the one displayed here
        CFG.Current.CurrentHavokCollisionType = PrimaryCollisionType;

        RefreshCollision();
    }

    /// <summary>
    /// Releases the read collision. ER, NR and AC6 collision is read again the next time it is asked for.
    /// </summary>
    public void Clear()
    {
        MapDocuments.Clear();
        ReadBinders.Clear();

        lock (TagContainers)
        {
            TagContainers.Clear();
        }
    }

    public void OnLoadMap(string mapId)
    {
        using var __scope = Profiler.TracyZoneAuto();
        if (!CFG.Current.MapEditor_ModelLoad_Collisions)
            return;

        // The viewport reads its own collision, so these are only listed here and read when they are first asked for
        if (IsListedGame)
        {
            if (!MapCollisions.ContainsKey(mapId))
                MapCollisions.Add(mapId, new List<string>());

            // Not every game has every type (DS3 has no fall protection), so a missing binder isn't reported
            foreach (var type in GetCollisionBinderTypes().Where(t => HasCollisionBinder(mapId, t)))
            {
                ListMapCollision(mapId, type);
            }
        }

        if (Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (!MapCollisions.ContainsKey(mapId))
                MapCollisions.Add(mapId, new List<string>());

            LoadTagMapCollision(mapId, "h");
            LoadTagMapCollision(mapId, "l");
        }
    }

    /// <summary>
    /// The games whose map collision is listed when a map loads and read when it is first asked for (MapDocuments):
    /// ER, NR and AC6, and DS3. SDT's is read at load instead (TagContainers).
    /// </summary>
    private bool IsListedGame => Project.Descriptor.ProjectType is ProjectType.ER
        or ProjectType.NR
        or ProjectType.AC6
        or ProjectType.DS3;

    /// <summary>
    /// The paths of a map's collision binder of one type. ER, NR and AC6 keep a map in its area's folder
    /// (map/m30/m30_00_00_00/h30_00_00_00.hkxbhd); DS3 and SDT keep it in the map's own (map/m30_00_00_00/...).
    /// </summary>
    private (string Bhd, string Bdt) GetBinderPaths(string mapId, string type)
    {
        var folder = Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.BB
            ? Path.Join("map", mapId)
            : Path.Join("map", mapId.Substring(0, 3), mapId);

        return (Path.Join(folder, $"{type}{mapId.Substring(1)}.hkxbhd"), Path.Join(folder, $"{type}{mapId.Substring(1)}.hkxbdt"));
    }

    /// <summary>
    /// Collision files of Sekiro that the Map Editor has changed, the only ones of its binders that are written: the
    /// rest are read at load for the walls and viewport, and keep their bytes.
    /// </summary>
    private readonly HashSet<string> ModifiedTagFiles = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The editable document of a map collision file, read if it hasn't been: MapDocuments for the listed games,
    /// TagContainers for SDT. Null if the file isn't in a loaded map.
    /// </summary>
    public Havok.HavokDocument GetEditableDocument(string fileName)
    {
        return Project.Descriptor.ProjectType is ProjectType.SDT ? GetTagDocument(fileName) : GetMapDocument(fileName);
    }

    /// <summary>
    /// The editable document of a map collision file if it has been read, without reading it.
    /// </summary>
    public Havok.HavokDocument PeekEditableDocument(string fileName)
    {
        return Project.Descriptor.ProjectType is ProjectType.SDT
            ? GetTagDocument(fileName)
            : MapDocuments.GetValueOrDefault(fileName);
    }

    /// <summary>
    /// Marks a collision file as changed, so it is written when its binder is saved. Only Sekiro needs it: the other
    /// games write every read file of a binder.
    /// </summary>
    public void MarkModified(string fileName)
    {
        if (Project.Descriptor.ProjectType is ProjectType.SDT)
            ModifiedTagFiles.Add(fileName);
    }

    /// <summary>The document of a file of a binder being saved, or null if it isn't to be written.</summary>
    private Havok.HavokDocument GetDocumentToSave(string fileName)
    {
        if (Project.Descriptor.ProjectType is ProjectType.SDT)
            return ModifiedTagFiles.Contains(fileName) ? GetTagDocument(fileName) : null;

        return MapDocuments.GetValueOrDefault(fileName);
    }

    private static IEnumerable<string> GetCollisionBinderTypes()
    {
        yield return "h";
        yield return "l";
        yield return "f";
    }

    /// <summary>
    /// Gets the collision of an ER, NR or AC6 map, e.g. l30_00_00_00_000100.hkx.dcx. Its binder is read the first
    /// time one of its files is asked for. Null if the file isn't in a loaded map.
    /// </summary>
    public Havok.HavokDocument GetMapDocument(string fileName)
    {
        if (CollisionBinders.TryGetValue(fileName, out var binder) &&
            ReadBinders.Add(GetBinderKey(binder.MapId, binder.Type)))
        {
            ReadMapCollision(binder.MapId, binder.Type);
        }

        return MapDocuments.GetValueOrDefault(fileName);
    }

    private static string GetBinderKey(string mapId, string type) => $"{mapId}:{type}";

    /// <summary>
    /// Opens the collision binder of one type of a map, from the project if it has its own copy.
    /// Null if the map has no binder of this type.
    /// </summary>
    private BXF4Reader OpenCollisionBinder(string mapId, string type)
    {
        var (bhdPath, bdtPath) = GetBinderPaths(mapId, type);

        if (!Project.VFS.FS.FileExists(bdtPath) || !Project.VFS.FS.FileExists(bhdPath))
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_HKXBND", bdtPath));
            return null;
        }

        var bdtData = Project.VFS.ProjectFS.FileExists(bdtPath)
            ? Project.VFS.ProjectFS.ReadFile(bdtPath)
            : Project.VFS.FS.ReadFile(bdtPath);
        var bhdData = Project.VFS.ProjectFS.FileExists(bhdPath)
            ? Project.VFS.ProjectFS.ReadFile(bhdPath)
            : Project.VFS.FS.ReadFile(bhdPath);

        if (bdtData == null || bhdData == null)
            return null;

        return new BXF4Reader((Memory<byte>)bhdData, (Memory<byte>)bdtData);
    }

    private static List<BinderFileHeader> GetCollisionFiles(BinderReader binder)
    {
        return binder.Files
            .Where(e => e.Name.Contains(".hkx.dcx") && e.Name.Split('\\').Length == 2)
            .ToList();
    }

    /// <summary>
    /// Lists the collision files in one binder of a map, without reading them.
    /// </summary>
    private void ListMapCollision(string mapId, string type)
    {
        var binderKey = GetBinderKey(mapId, type);

        try
        {
            using var binder = OpenCollisionBinder(mapId, type);

            if (binder == null)
                return;

            foreach (var file in GetCollisionFiles(binder))
            {
                var name = file.Name.Split('\\')[1];

                if (CollisionBinders.TryAdd(name, (mapId, type)))
                {
                    MapCollisions[mapId].Add(name);
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_HKXBND", binderKey), e);
        }
    }

    /// <summary>
    /// Reads the collision files in one binder of an ER, NR or AC6 map. These are tagfiles which refer to a
    /// compendium held in the same binder, Havok 2018 for ER and NR, Havok 2019 for AC6.
    /// </summary>
    private void ReadMapCollision(string mapId, string type)
    {
        using var __scope = Profiler.TracyZoneAuto();

        var profile = Havok.HavokProfile.FromGame(Project.Descriptor.ProjectType.ToString());

        if (profile == null)
            return;

        try
        {
            using var binder = OpenCollisionBinder(mapId, type);

            if (binder == null)
                return;

            // The binder reads its files through one position, so they are taken out here and then read in
            // parallel, as they don't depend on each other
            var compendium = HavokCompendiumSource.Find(binder);
            var files = GetCollisionFiles(binder)
                .Select(file => (Name: file.Name.Split('\\')[1], Bytes: binder.ReadFile(file)))
                .ToList();

            var documents = new Havok.HavokDocument[files.Count];

            Parallel.For(0, files.Count, i =>
            {
                try
                {
                    var bytes = DCX.Is(files[i].Bytes) ? DCX.Decompress(files[i].Bytes) : files[i].Bytes;

                    documents[i] = Havok.Files.HavokFile.Read(bytes.ToArray(), profile, compendium?.Unified(profile));
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_HKX", files[i].Name), ex);
                }
            });

            for (int i = 0; i < files.Count; i++)
            {
                if (documents[i] != null)
                {
                    MapDocuments.TryAdd(files[i].Name, documents[i]);
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_HKXBND", GetBinderKey(mapId, type)), e);
        }
    }

    /// <summary>
    /// Gets the collision of a Sekiro map, e.g. l10_00_00_00_000100.hkx.dcx. Null if it isn't loaded.
    /// </summary>
    public Havok.HavokDocument GetTagDocument(string fileName)
    {
        lock (TagContainers)
        {
            return TagContainers.GetValueOrDefault(fileName);
        }
    }

    /// <summary>
    /// Reads the collision of a Sekiro map. These are tagfiles that refer to a compendium held in the same binder.
    /// The files don't depend on each other, so they are read in parallel.
    /// </summary>
    private void LoadTagMapCollision(string mapId, string type)
    {
        using var __scope = Profiler.TracyZoneAuto();

        var bdtPath = Path.Join("map", mapId, $"{type}{mapId.Substring(1)}.hkxbdt");
        var bhdPath = Path.Join("map", mapId, $"{type}{mapId.Substring(1)}.hkxbhd");

        if (!Project.VFS.FS.FileExists(bdtPath) || !Project.VFS.FS.FileExists(bhdPath))
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_HKXBND", bdtPath));
            return;
        }

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (bdtData == null || bhdData == null)
                return;

            using var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            var profile = Havok.HavokProfile.SDT;
            Havok.HavokCompendium compendium = null;

            var compendiumFile = packedBinder.Files.FirstOrDefault(e => e.Name.Contains(".compendium"));
            if (compendiumFile != null)
            {
                compendium = Havok.Files.HavokFile.ReadCompendium(DCX.Decompress(compendiumFile.Bytes).ToArray(), profile);
            }

            var files = packedBinder.Files
                .Where(e => e.Name.Contains(".hkx.dcx") && e.Name.Split('\\').Length == 2)
                .ToList();

            Parallel.ForEach(files, file =>
            {
                var name = file.Name.Split('\\')[1];

                try
                {
                    var fileBytes = DCX.Decompress(file.Bytes).ToArray();
                    var container = Havok.Files.HavokFile.Read(fileBytes, profile, compendium);

                    lock (TagContainers)
                    {
                        if (TagContainers.TryAdd(name, container))
                        {
                            MapCollisions[mapId].Add(name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_HKX", name), ex);
                }
            });
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_HKXBND", bdtPath), e);
        }
    }

    public void OnUnloadMap(string mapId)
    {
        // Always release, even if collision loading was toggled off after this map was loaded,
        // so a later reload reads the files again instead of reusing stale (possibly edited) containers
        if (!MapCollisions.TryGetValue(mapId, out var collisionNames))
            return;

        foreach (var name in collisionNames)
        {
            MapDocuments.Remove(name);
            CollisionBinders.Remove(name);
            NewFiles.Remove(name);
            ModifiedTagFiles.Remove(name);

            lock (TagContainers)
            {
                TagContainers.Remove(name);
            }
        }

        foreach (var type in GetCollisionBinderTypes())
        {
            ReadBinders.Remove(GetBinderKey(mapId, type));
        }

        MapCollisions.Remove(mapId);
    }

    public void RefreshCollision()
    {
        foreach(var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if(entry.Value.MapContainer != null)
            {
                foreach(var ent in entry.Value.MapContainer.Objects)
                {
                    if(EntityHelper.IsPartCollision(ent) || EntityHelper.IsPartConnectCollision(ent))
                    {
                        if (ent is MsbEntity msbEnt)
                        {
                            msbEnt.AssignDrawable();
                        }
                        ent.UpdateRenderModel();
                    }
                }
            }
        }

        RefreshObjectCollision();
    }

    /// <summary>
    /// Object collision is only read while it is displayed, so it has to be picked up when the filter
    /// is turned on. Checked here as the filter can be changed from several places (e.g. the presets).
    /// </summary>
    public void UpdateObjectCollision(RenderFilter filter)
    {
        var visible = filter.HasFlag(RenderFilter.ObjectCollision);

        if (visible && !ObjectCollisionVisible)
        {
            RefreshObjectCollision();
        }

        ObjectCollisionVisible = visible;
    }

    private void RefreshObjectCollision()
    {
        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                foreach (var ent in entry.Value.MapContainer.Objects)
                {
                    // Entities still being set up get their collision along with the rest of their drawable
                    if (ent is MsbEntity { SetupRenderMesh: true } msbEnt)
                    {
                        msbEnt.AssignObjectCollisionDrawable();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Collision files made in the Map Editor that a binder may not have yet, keyed by file name: SaveMapCollision
    /// adds them to their binder.
    /// </summary>
    private readonly Dictionary<string, (string MapId, string Type)> NewFiles = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Reads a file of one binder of an ER, NR or AC6 map afresh, to base a new collision on: the first whose
    /// document has a single body, which the accept check takes. Read with the binder's compendium, so the new file
    /// refers to it as the binder's other files do. Null if the binder has no such file.
    /// </summary>
    public Havok.HavokDocument ReadTemplateDocument(string mapId, string type, Func<Havok.HavokDocument, bool> accept)
    {
        var profile = Havok.HavokProfile.FromGame(Project.Descriptor.ProjectType.ToString());

        if (profile == null)
            return null;

        try
        {
            using var binder = OpenCollisionBinder(mapId, type);

            if (binder == null)
                return null;

            var compendium = HavokCompendiumSource.Find(binder);

            foreach (var file in GetCollisionFiles(binder))
            {
                try
                {
                    var bytes = binder.ReadFile(file);
                    bytes = DCX.Is(bytes) ? DCX.Decompress(bytes) : bytes;

                    var document = Havok.Files.HavokFile.Read(bytes.ToArray(), profile, compendium?.Unified(profile));

                    if (accept(document))
                        return document;
                }
                catch (Exception)
                {
                    // Files that can't be read are skipped: any readable one will do
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_HKXBND", GetBinderKey(mapId, type)), e);
        }

        return null;
    }

    /// <summary>
    /// Whether a map's binders list a collision file of this name, of any type.
    /// </summary>
    public bool HasMapCollision(string mapId, string fileName)
    {
        return MapCollisions.TryGetValue(mapId, out var names)
            && names.Any(n => string.Equals(n, fileName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Whether a map has a collision binder of a type (h, l or f).
    /// </summary>
    public bool HasCollisionBinder(string mapId, string type)
    {
        var (bhdPath, bdtPath) = GetBinderPaths(mapId, type);

        return Project.VFS.FS.FileExists(bdtPath) && Project.VFS.FS.FileExists(bhdPath);
    }

    /// <summary>
    /// Adds a new collision file to a map, to be written into its binder by SaveMapCollision. It is listed and held as
    /// the files read from the binder are.
    /// </summary>
    public void AddMapCollision(string mapId, string type, string fileName, Havok.HavokDocument document)
    {
        if (!MapCollisions.ContainsKey(mapId))
            MapCollisions.Add(mapId, new List<string>());

        if (!MapCollisions[mapId].Contains(fileName, StringComparer.OrdinalIgnoreCase))
            MapCollisions[mapId].Add(fileName);

        CollisionBinders[fileName] = (mapId, type);
        NewFiles[fileName] = (mapId, type);

        // Sekiro's collision is held as it is read at load, where the walls and viewport find it
        if (Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            lock (TagContainers)
            {
                TagContainers[fileName] = document;
            }

            ModifiedTagFiles.Add(fileName);
        }
        else
        {
            MapDocuments[fileName] = document;
        }
    }

    /// <summary>
    /// Adds the new collision files of a binder that it doesn't have yet, as its other collision files are stored:
    /// in the same folder of the binder, with their flags and compression, and the next free ID.
    /// </summary>
    private bool AddNewFiles(BXF4 binder, string mapId, string type, ref bool anyWritten)
    {
        var success = true;

        var pending = NewFiles
            .Where(e => e.Value.MapId == mapId && e.Value.Type == type)
            .Select(e => e.Key)
            .ToList();

        if (pending.Count == 0)
            return true;

        var existing = binder.Files
            .Where(f => f.Name != null && f.Name.Contains(".hkx.dcx") && f.Name.Split('\\').Length == 2)
            .ToList();

        var sample = existing.FirstOrDefault();

        if (sample == null)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_HKXBND", GetBinderKey(mapId, type)));
            return false;
        }

        var folder = sample.Name.Split('\\')[0];
        DCX.Decompress(sample.Bytes, out DCX.Type compression);

        foreach (var name in pending)
        {
            // Written by the loop over the binder's files once it has it
            if (existing.Any(f => string.Equals(f.Name.Split('\\')[1], name, StringComparison.OrdinalIgnoreCase)))
            {
                NewFiles.Remove(name);
                continue;
            }

            if (GetDocumentToSave(name) is not { } document)
                continue;

            try
            {
                var bytes = DCX.Compress(Havok.Files.HavokFile.Write(document), compression);
                var id = binder.Files.Count == 0 ? 0 : binder.Files.Max(f => f.ID) + 1;

                binder.Files.Add(new BinderFile(sample.Flags, id, $"{folder}\\{name}", bytes)
                {
                    CompressionType = sample.CompressionType
                });

                NewFiles.Remove(name);
                ModifiedTagFiles.Remove(name);
                anyWritten = true;
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Serialize_HKX", name), ex);
                success = false;
            }
        }

        return success;
    }

    public bool SaveMapCollisionFiles(string mapId)
    {
        var success = true;

        foreach (var type in GetCollisionBinderTypes())
        {
            success &= SaveMapCollision(mapId, type);
        }

        return success;
    }

    /// <summary>
    /// Writes the loaded collision of one binder of an ER, NR or AC6 map back into it. Each file is written in the form
    /// it was read (referring to the binder's compendium if it did) and keeps its compression. Files that weren't read
    /// keep their bytes.
    /// </summary>
    public bool SaveMapCollision(string mapId, string type)
    {
        var (bhdPath, bdtPath) = GetBinderPaths(mapId, type);

        // Not every map has every collision type, so a missing binder is not a failure
        if (!Project.VFS.FS.FileExists(bdtPath) || !Project.VFS.FS.FileExists(bhdPath))
            return true;

        try
        {
            // The project's copy first, so entries this bank doesn't hold keep any earlier changes
            var bdtData = Project.VFS.ProjectFS.FileExists(bdtPath)
                ? Project.VFS.ProjectFS.ReadFile(bdtPath)
                : Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.ProjectFS.FileExists(bhdPath)
                ? Project.VFS.ProjectFS.ReadFile(bhdPath)
                : Project.VFS.FS.ReadFile(bhdPath);

            if (bdtData == null || bhdData == null)
                return true;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            var success = true;
            var anyWritten = false;

            foreach (var file in packedBinder.Files)
            {
                var parts = file.Name.Split('\\');

                if (parts.Length != 2 || !file.Name.Contains(".hkx.dcx"))
                    continue;

                var name = parts[1];

                if (GetDocumentToSave(name) is not { } document)
                    continue;

                try
                {
                    // Keep the compression of the original file
                    DCX.Decompress(file.Bytes, out DCX.Type compression);

                    file.Bytes = DCX.Compress(Havok.Files.HavokFile.Write(document), compression);
                    ModifiedTagFiles.Remove(name);
                    anyWritten = true;
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Serialize_HKX", name), ex);
                    success = false;
                }
            }

            success &= AddNewFiles(packedBinder, mapId, type, ref anyWritten);

            if (!anyWritten)
                return success;

            packedBinder.Write(out byte[] newBhdBytes, out byte[] newBdtBytes);

            Project.VFS.ProjectFS.WriteFile(bhdPath, newBhdBytes);
            Project.VFS.ProjectFS.WriteFile(bdtPath, newBdtBytes);

            return success;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_HKXBND", bdtPath), e);
            return false;
        }
    }
}
