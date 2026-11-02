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
    /// Sekiro collision, which is read as tagfiles into the HKX2 classes. Keyed by file name, like HavokContainers.
    /// </summary>
    public Dictionary<string, HKX2.hkRootLevelContainer> TagContainers = new();

    public Dictionary<string, List<string>> MapCollisions = new();

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

    public void Clear()
    {
        HavokContainers.Clear();

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

        if (Project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.NR)
        {
            if (!MapCollisions.ContainsKey(mapId))
                MapCollisions.Add(mapId, new List<string>());

            LoadMapCollision(mapId, "h");
            LoadMapCollision(mapId, "l");
            LoadMapCollision(mapId, "f");
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
    /// Gets the collision of a Sekiro map, e.g. l10_00_00_00_000100.hkx.dcx. Null if it isn't loaded.
    /// </summary>
    public HKX2.hkRootLevelContainer GetTagContainer(string fileName)
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

            HKX2.TagFileDeserializer.Compendium compendium = null;

            var compendiumFile = packedBinder.Files.FirstOrDefault(e => e.Name.Contains(".compendium"));
            if (compendiumFile != null)
            {
                compendium = HKX2.TagFileDeserializer.ReadCompendium(DCX.Decompress(compendiumFile.Bytes).ToArray());
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
                    var container = new HKX2.TagFileDeserializer().Deserialize(fileBytes, compendium) as HKX2.hkRootLevelContainer;

                    if (container == null)
                        return;

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

            // TEMP: SDT collision diagnostics
            Smithbox.Log(this, $"[SDT Collision] {bhdPath}: {files.Count} hkx files, {MapCollisions[mapId].Count(n => n.StartsWith(type))} read, compendium {(compendium != null ? "found" : "missing")}");
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

            lock (TagContainers)
            {
                TagContainers.Remove(name);
            }
        }

        MapCollisions.Remove(mapId);
    }

    private void LoadMapCollision(string mapId, string type)
    {
        using var __scope = Profiler.TracyZoneAuto();
        byte[] CompendiumBytes = null;

        var bdtPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbdt");
        var bhdPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbhd");

        if (Project.VFS.FS.FileExists(bdtPath) && Project.VFS.FS.FileExists(bhdPath))
        {
            try
            {
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
                    return;

                var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

                HavokBinarySerializer serializer = new HavokBinarySerializer();

                // Get compendium
                foreach (var file in packedBinder.Files)
                {
                    if (file.Name.Contains(".compendium.dcx"))
                    {
                        CompendiumBytes = DCX.Decompress(file.Bytes).ToArray();
                    }
                }

                if (CompendiumBytes != null)
                {
                    using MemoryStream memoryStream = new MemoryStream(CompendiumBytes);
                    serializer.LoadCompendium(memoryStream);
                }

                foreach (var file in packedBinder.Files)
                {
                    var parts = file.Name.Split('\\');

                    if (parts.Length != 2)
                        continue;

                    var name = parts[1];

                    if (!file.Name.Contains(".hkx.dcx"))
                        continue;

                    var FileBytes = DCX.Decompress(file.Bytes).ToArray();

                    try
                    {
                        using (MemoryStream memoryStream = new MemoryStream(FileBytes))
                        {
                            hkRootLevelContainer fileHkx;
                            try
                            {
                                fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);

                                if (!HavokContainers.ContainsKey(name))
                                {
                                    HavokContainers.Add(name, fileHkx);

                                    MapCollisions[mapId].Add(name);
                                }
                            }
                            catch (InvalidDataException ex)
                            {
                                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_HKX", name), ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Serialize_HKX", name), ex);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_HKXBND", bdtPath), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_HKXBND", bdtPath));
        }
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
        var success = SaveMapCollision(mapId, "h");
        success &= SaveMapCollision(mapId, "l");
        success &= SaveMapCollision(mapId, "f");

        return success;
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
