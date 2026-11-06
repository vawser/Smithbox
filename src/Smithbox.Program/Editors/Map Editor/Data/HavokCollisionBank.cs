using Havok.Shared;
using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using Tracy;

namespace StudioCore.Editors.MapEditor;

public class HavokCollisionBank
{
    public ProjectEntry Project;

    public Dictionary<string, hkRootLevelContainer> HavokContainers = new Dictionary<string, hkRootLevelContainer>();

    /// <summary>
    /// Sekiro collision: tagfiles read by the unified Havok library with their binder's compendium. Keyed by file
    /// name, like HavokContainers.
    /// </summary>
    public Dictionary<string, Havok.HavokDocument> TagContainers = new();

    /// <summary>
    /// AC6 collision, which is Havok 2019. Keyed by file name, like HavokContainers.
    /// </summary>
    public Dictionary<string, HKLib.hk2019.hkRootLevelContainer> AC6Containers = new();

    public Dictionary<string, List<string>> MapCollisions = new();

    /// <summary>
    /// The map and type of the binder each listed ER, NR or AC6 collision file is in, keyed by file name.
    /// </summary>
    private Dictionary<string, (string MapId, string Type)> CollisionBinders = new();

    /// <summary>
    /// The ER, NR or AC6 collision binders which have been read into HavokContainers or AC6Containers.
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
        HavokContainers.Clear();
        AC6Containers.Clear();
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
        if (Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.NR
            or ProjectType.AC6)
        {
            if (!MapCollisions.ContainsKey(mapId))
                MapCollisions.Add(mapId, new List<string>());

            foreach (var type in GetCollisionBinderTypes())
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
    public IRootContainer GetHKLibContainer(string fileName)
    {
        if (CollisionBinders.TryGetValue(fileName, out var binder) &&
            ReadBinders.Add(GetBinderKey(binder.MapId, binder.Type)))
        {
            ReadMapCollision(binder.MapId, binder.Type);
        }

        return HavokContainers.TryGetValue(fileName, out var container)
            ? container
            : AC6Containers.GetValueOrDefault(fileName);
    }

    private static string GetBinderKey(string mapId, string type) => $"{mapId}:{type}";

    /// <summary>
    /// Opens the collision binder of one type of a map, from the project if it has its own copy.
    /// Null if the map has no binder of this type.
    /// </summary>
    private BXF4Reader OpenCollisionBinder(string mapId, string type)
    {
        var bdtPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbdt");
        var bhdPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbhd");

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

        var isAC6 = Project.Descriptor.ProjectType is ProjectType.AC6;

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

            var containers = new IRootContainer[files.Count];

            Parallel.For(0, files.Count, i =>
            {
                try
                {
                    containers[i] = isAC6
                        ? HKLibTagfileReader.Read2019(files[i].Bytes, compendium)
                        : HKLibTagfileReader.Read2018(files[i].Bytes, compendium);
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_HKX", files[i].Name), ex);
                }
            });

            for (int i = 0; i < files.Count; i++)
            {
                switch (containers[i])
                {
                    case HKLib.hk2019.hkRootLevelContainer ac6Container:
                        AC6Containers.TryAdd(files[i].Name, ac6Container);
                        break;
                    case hkRootLevelContainer container:
                        HavokContainers.TryAdd(files[i].Name, container);
                        break;
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
            HavokContainers.Remove(name);
            AC6Containers.Remove(name);
            CollisionBinders.Remove(name);

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

    public bool SaveMapCollisionFiles(string mapId)
    {
        if (Project.Descriptor.ProjectType is ProjectType.AC6)
        {
            var ac6Success = SaveAC6MapCollision(mapId, "h");
            ac6Success &= SaveAC6MapCollision(mapId, "l");
            ac6Success &= SaveAC6MapCollision(mapId, "f");

            return ac6Success;
        }

        var success = SaveMapCollision(mapId, "h");
        success &= SaveMapCollision(mapId, "l");
        success &= SaveMapCollision(mapId, "f");

        return success;
    }

    /// <summary>
    /// Writes the loaded collision of an AC6 map back into its binder. The binder's compendium is loaded first, so the
    /// written files refer to it like the original files do.
    /// </summary>
    public bool SaveAC6MapCollision(string mapId, string type)
    {
        var bdtPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbdt");
        var bhdPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbhd");

        // Not every map has every collision type, so a missing binder is not a failure
        if (!Project.VFS.FS.FileExists(bdtPath) || !Project.VFS.FS.FileExists(bhdPath))
            return true;

        try
        {
            var bdtData = Project.VFS.ProjectFS.FileExists(bdtPath)
                ? Project.VFS.ProjectFS.ReadFile(bdtPath)
                : Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.ProjectFS.FileExists(bhdPath)
                ? Project.VFS.ProjectFS.ReadFile(bhdPath)
                : Project.VFS.FS.ReadFile(bhdPath);

            if (bdtData == null || bhdData == null)
                return true;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            var serializer = new HKLib.Serialization.hk2019.Binary.HavokBinarySerializer();

            var compendiumFile = packedBinder.Files.FirstOrDefault(e => e.Name.Contains(".compendium"));
            if (compendiumFile != null)
            {
                using var compendiumStream = new MemoryStream(DCX.Decompress(compendiumFile.Bytes).ToArray());
                serializer.LoadCompendium(compendiumStream);
            }

            var success = true;
            var anyWritten = false;

            foreach (var file in packedBinder.Files)
            {
                var parts = file.Name.Split('\\');

                if (parts.Length != 2 || !file.Name.Contains(".hkx.dcx"))
                    continue;

                var name = parts[1];

                if (!AC6Containers.TryGetValue(name, out var container))
                    continue;

                try
                {
                    // Keep the compression of the original file
                    DCX.Decompress(file.Bytes, out DCX.Type compression);

                    using var memoryStream = new MemoryStream();
                    serializer.Write(container, memoryStream);

                    file.Bytes = DCX.Compress(memoryStream.ToArray(), compression);
                    anyWritten = true;
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Serialize_HKX", name), ex);
                    success = false;
                }
            }

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

    public bool SaveMapCollision(string mapId, string type)
    {
        var success = true;

        var bdtPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbdt");
        var bhdPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbhd");

        if (Project.VFS.FS.FileExists(bdtPath) && Project.VFS.FS.FileExists(bhdPath))
        {
            try
            {
                // Read the existing binder (project override first, then base game) so we
                // preserve any entries that this bank doesn't hold in-memory.
                var bdtData = Project.VFS.FS.ReadFile(bdtPath);
                var bhdData = Project.VFS.FS.ReadFile(bhdPath);

                if (Project.VFS.ProjectFS.FileExists(bdtPath))
                {
                    bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
                }
                if (Project.VFS.ProjectFS.FileExists(bhdPath))
                {
                    bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
                }

                if (bdtData == null || bhdData == null)
                    return success;

                var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

                HavokBinarySerializer serializer = new HavokBinarySerializer();

                bool anyWritten = false;

                foreach (var file in packedBinder.Files)
                {
                    var parts = file.Name.Split('\\');

                    if (parts.Length != 2)
                        continue;

                    var name = parts[1];

                    if (!file.Name.Contains(".hkx.dcx"))
                        continue;

                    // Only re-serialize entries we actually have loaded (and presumably edited)
                    if (!HavokContainers.ContainsKey(name))
                        continue;

                    try
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            serializer.Write(HavokContainers[name], memoryStream);

                            // NOTE: assumes DCX_KRAK to match ER/NR collision packaging.
                            // Swap this for whatever DCX.Type the project actually uses if different.
                            var compressedBytes = DCX.Compress(memoryStream.ToArray(), DCX.Type.DCX_KRAK);

                            file.Bytes = compressedBytes;
                            anyWritten = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Serialize_HKX", name), ex);
                        success = false;
                    }
                }

                if (!anyWritten)
                    return success;

                // BXF4.Write returns the BHD bytes and outputs the BDT bytes via out param.
                packedBinder.Write(out byte[] newBhdBytes, out byte[] newBdtBytes);

                // NOTE: writing back to the project overlay (not the base game files).
                // Replace WriteFile with whatever the actual VFS write method is named
                // if it differs from this.
                Project.VFS.ProjectFS.WriteFile(bhdPath, newBhdBytes);
                Project.VFS.ProjectFS.WriteFile(bdtPath, newBdtBytes);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_HKXBND", bdtPath), e);
                success = false;
            }
        }
        else
        {
            // Not every map has every collision type, so a missing binder is not a failure
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_HKXBND", bdtPath));
        }

        return success;
    }

}
