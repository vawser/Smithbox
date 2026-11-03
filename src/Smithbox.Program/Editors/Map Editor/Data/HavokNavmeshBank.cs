using Havok.Shared;
using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;
using StudioCore.Renderer;
using Tracy;

namespace StudioCore.Editors.MapEditor;

public class HavokNavmeshBank
{
    public ProjectEntry Project;

    public Dictionary<string, NVA> Files = new();
    public Dictionary<string, NVA_ER> ER_Files = new();

    public Dictionary<string, hkRootLevelContainer> HKX3_Containers = new Dictionary<string, hkRootLevelContainer>();

    /// <summary>
    /// AC6 navmeshes, which are Havok 2019
    /// </summary>
    public Dictionary<string, HKLib.hk2019.hkRootLevelContainer> AC6_Containers = new();

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
        if (Project.Descriptor.ProjectType is ProjectType.DS3 
            or ProjectType.BB 
            or ProjectType.SDT
            or ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR)
            return true;

        // NOTE: SDT doesn't render the meshes as it doesn't have HKX support yet

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
    public IRootContainer GetContainer(string name)
    {
        // Navmeshes are named after their map, e.g. n30_00_00_00_000100 is in m30_00_00_00
        foreach (var mapId in UnreadMaps.Where(m => name.AsSpan(1).StartsWith(m.AsSpan(1))).ToList())
        {
            UnreadMaps.Remove(mapId);
            ReadHavokNavmeshModels(mapId);
        }

        return HKX3_Containers.TryGetValue(name, out var container)
            ? container
            : AC6_Containers.GetValueOrDefault(name);
    }

    public void OnUnloadMap(string mapId)
    {
        UnreadMaps.Remove(mapId);

        if (MapNavmeshes.Remove(mapId, out var navmeshNames))
        {
            foreach (var name in navmeshNames)
            {
                HKX3_Containers.Remove(name);
                AC6_Containers.Remove(name);
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

                var isAC6 = Project.Descriptor.ProjectType is ProjectType.AC6;
                var containers = new IRootContainer[files.Count];

                Parallel.For(0, files.Count, i =>
                {
                    try
                    {
                        containers[i] = isAC6
                            ? HKLibTagfileReader.Read2019(files[i].Bytes)
                            : HKLibTagfileReader.Read2018(files[i].Bytes);
                    }
                    catch (Exception ex)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_NVA_HKX", files[i].Name), ex);
                    }
                });

                var navmeshNames = MapNavmeshes.TryGetValue(mapId, out var names) ? names : MapNavmeshes[mapId] = new();

                for (int i = 0; i < files.Count; i++)
                {
                    var added = containers[i] switch
                    {
                        HKLib.hk2019.hkRootLevelContainer ac6Container => AC6_Containers.TryAdd(files[i].Name, ac6Container),
                        hkRootLevelContainer container => HKX3_Containers.TryAdd(files[i].Name, container),
                        _ => false
                    };

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
    /// Writes a loaded navmesh file, or returns null if the file isn't loaded.
    /// </summary>
    private byte[] WriteNavmeshFile(string name)
    {
        using var memoryStream = new MemoryStream();

        if (Project.Descriptor.ProjectType is ProjectType.AC6)
        {
            if (!AC6_Containers.TryGetValue(name, out var container))
                return null;

            new HKLib.Serialization.hk2019.Binary.HavokBinarySerializer().Write(container, memoryStream);
        }
        else
        {
            if (!HKX3_Containers.TryGetValue(name, out var container))
                return null;

            new HavokBinarySerializer().Write(container, memoryStream);
        }

        return memoryStream.ToArray();
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
                        var fileBytes = WriteNavmeshFile(name);
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

        foreach (var entry in Project.Locator.NavmeshFiles.Entries)
        {
            if (entry.Filename != map.Name)
                continue;

            if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

                try
                {
                    var mapID = Path.GetFileNameWithoutExtension(map.Name);

                    if (map.Name == mapID)
                    {
                        if (fs.FileExists(entry.Path))
                        {
                            var fileData = fs.ReadFile(entry.Path);
                            var nva = NVA_ER.Read(fileData.Value);

                            WriteNavmeshInfo(map, nva);
                            WriteFaceData(map, nva);
                            WriteNodeBank(map, nva);
                            WriteSection3(map, nva);
                            WriteConnectors(map, nva);
                            WriteLevelConnectors(map, nva);
                            WriteSection9(map, nva);
                            WriteSection10(map, nva);
                            WriteSection11(map, nva);
                            WriteSection13(map, nva);

                            var newFileData = nva.Write();

                            if (!BytePerfectHelper.Md5Equal(fileData.Value.ToArray(), newFileData))
                            {
                                Project.VFS.ProjectFS.WriteFile(entry.Path, newFileData);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_NVA", entry.Path), e);
                    success = false;
                }
            }
            else
            {
                var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

                try
                {
                    var mapID = Path.GetFileNameWithoutExtension(map.Name);

                    if (map.Name == mapID)
                    {
                        if (fs.FileExists(entry.Path))
                        {
                            var fileData = fs.ReadFile(entry.Path);
                            var nva = NVA.Read(fileData.Value);

                            WriteNavmeshInfo(map, nva);
                            WriteFaceData(map, nva);
                            WriteNodeBank(map, nva);
                            WriteSection3(map, nva);
                            WriteConnectors(map, nva);
                            WriteLevelConnectors(map, nva);

                            var newFileData = nva.Write();

                            if (!BytePerfectHelper.Md5Equal(fileData.Value.ToArray(), newFileData))
                            {
                                Project.VFS.ProjectFS.WriteFile(entry.Path, newFileData);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_NVA", entry.Path), e);
                    success = false;
                }
            }
        }

        return success;
    }

    public void WriteNavmeshInfo(MapContainer map, NVA nva)
    {
        var version = nva.NavmeshInfoEntries.Version;

        var newSection = new NVA.NavmeshInfoSection((int)version);

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA.NavmeshInfo))
            {
                var navmesh = (NVA.NavmeshInfo)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.NavmeshInfoEntries = newSection;
    }
    public void WriteNavmeshInfo(MapContainer map, NVA_ER nva)
    {
        var version = nva.Navmeshes.Version;

        var newSection = new NVA_ER.NavmeshSection((int)version);

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA_ER.Navmesh))
            {
                var navmesh = (NVA_ER.Navmesh)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Navmeshes = newSection;
    }

    public void WriteFaceData(MapContainer map, NVA nva)
    {
        var version = nva.FaceDataEntries.Version;

        var newSection = new NVA.FaceDataSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA.FaceData))
            {
                var navmesh = (NVA.FaceData)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.FaceDataEntries = newSection;
    }

    public void WriteFaceData(MapContainer map, NVA_ER nva)
    {
        var version = nva.FaceDatas.Version;

        var newSection = new NVA_ER.FaceDataSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA_ER.FaceData))
            {
                var navmesh = (NVA_ER.FaceData)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.FaceDatas = newSection;
    }

    public void WriteNodeBank(MapContainer map, NVA nva)
    {
        var version = nva.NodeBankEntries.Version;

        var newSection = new NVA.NodeBankSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA.NodeBank))
            {
                var navmesh = (NVA.NodeBank)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.NodeBankEntries = newSection;
    }

    public void WriteNodeBank(MapContainer map, NVA_ER nva)
    {
        var version = nva.NodeBanks.Version;

        var newSection = new NVA_ER.NodeBankSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA_ER.NodeBank))
            {
                var navmesh = (NVA_ER.NodeBank)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.NodeBanks = newSection;
    }

    private static bool HasNavmeshEntities<T>(MapContainer map)
    {
        return map.NavmeshParent.Children.Any(e => e.WrappedObject is T);
    }

    public void WriteSection3(MapContainer map, NVA nva)
    {
        // These entries aren't loaded into the map, so the section is kept as it was read
        if (!HasNavmeshEntities<NVA.Entry3>(map))
            return;

        var version = nva.Section3Entries.Version;

        var newSection = new NVA.Section3();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA.Entry3))
            {
                var navmesh = (NVA.Entry3)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Section3Entries = newSection;
    }

    public void WriteSection3(MapContainer map, NVA_ER nva)
    {
        // These entries aren't loaded into the map, so the section is kept as it was read
        if (!HasNavmeshEntities<NVA_ER.Entry3>(map))
            return;

        var version = nva.Entries3.Version;

        var newSection = new NVA_ER.Section3();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA_ER.Entry3))
            {
                var navmesh = (NVA_ER.Entry3)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Entries3 = newSection;
    }

    public void WriteConnectors(MapContainer map, NVA nva)
    {
        var version = nva.ConnectorEntries.Version;

        var newSection = new NVA.ConnectorSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA.Connector))
            {
                var navmesh = (NVA.Connector)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.ConnectorEntries = newSection;
    }

    public void WriteConnectors(MapContainer map, NVA_ER nva)
    {
        var version = nva.Connectors.Version;

        var newSection = new NVA_ER.ConnectorSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA_ER.Connector))
            {
                var navmesh = (NVA_ER.Connector)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Connectors = newSection;
    }

    public void WriteLevelConnectors(MapContainer map, NVA nva)
    {
        var version = nva.LevelConnectorEntries.Version;

        var newSection = new NVA.LevelConnectorSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA.LevelConnector))
            {
                var navmesh = (NVA.LevelConnector)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.LevelConnectorEntries = newSection;
    }

    public void WriteLevelConnectors(MapContainer map, NVA_ER nva)
    {
        var version = nva.LevelConnectors.Version;

        var newSection = new NVA_ER.LevelConnectorSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA_ER.LevelConnector))
            {
                var navmesh = (NVA_ER.LevelConnector)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.LevelConnectors = newSection;
    }

    public void WriteSection9(MapContainer map, NVA_ER nva)
    {
        // Not every version has this section, and its entries aren't loaded into the map,
        // so the section is kept as it was read
        if (nva.Entries9 == null || !HasNavmeshEntities<NVA_ER.Entry9>(map))
            return;

        var version = nva.Entries9.Version;

        var newSection = new NVA_ER.Section9();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA_ER.Entry9))
            {
                var navmesh = (NVA_ER.Entry9)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Entries9 = newSection;
    }

    public void WriteSection10(MapContainer map, NVA_ER nva)
    {
        // Not every version has this section
        if (nva.Entries10 == null)
            return;

        var version = nva.Entries10.Version;

        var newSection = new NVA_ER.Section10();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA_ER.Entry10))
            {
                var navmesh = (NVA_ER.Entry10)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Entries10 = newSection;
    }

    public void WriteSection11(MapContainer map, NVA_ER nva)
    {
        // Not every version has this section
        if (nva.Entries11 == null)
            return;

        var version = nva.Entries11.Version;

        var newSection = new NVA_ER.Section11();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA_ER.Entry11))
            {
                var navmesh = (NVA_ER.Entry11)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Entries11 = newSection;
    }

    public void WriteSection13(MapContainer map, NVA_ER nva)
    {
        // Not every version has this section, e.g. AC6 has no Section 13
        if (nva.Entries13 == null)
            return;

        var version = nva.Entries13.Version;

        var newSection = new NVA_ER.Section13();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NVA_ER.Entry13))
            {
                var navmesh = (NVA_ER.Entry13)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Entries13 = newSection;
    }
}
