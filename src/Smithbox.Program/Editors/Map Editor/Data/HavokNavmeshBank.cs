using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Xml;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tracy;
using static SoulsFormats.NVA;
using static SoulsFormats.NVA_ER;

namespace StudioCore.Editors.MapEditor;

public class HavokNavmeshBank
{
    public MapEditorView View;
    public ProjectEntry Project;

    public Dictionary<string, NVA> Files = new();
    public Dictionary<string, NVA_ER> ER_Files = new();

    public Dictionary<string, hkRootLevelContainer> HKX3_Containers = new Dictionary<string, hkRootLevelContainer>();

    public HavokNavmeshBank(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool CanUse()
    {
        if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT or ProjectType.ER)
            return true;

        // NOTE: SDT doesn't render the meshes as it doesn't have HKX support yet

        return false;
    }

    public void OnLoadMap(string mapId)
    {
        using var __scope = Profiler.TracyZoneAuto();
        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            LoadNavmeshModels(mapId);
        }
    }

    public void OnUnloadMap(string mapId)
    {
        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            foreach (KeyValuePair<string, IResourceHandle> item in ResourceManager.GetResourceDatabase())
            {
                if (item.Key.Contains("nav"))
                {
                    item.Value.Release(true);
                }
            }
        }
    }

    private void LoadNavmeshModels(string mapId)
    {
        var binderEntry = Project.Locator.FileDictionary.Entries.FirstOrDefault(
            e => e.Filename == mapId &&
            e.Extension == "nvmhktbnd");

        if (binderEntry == null)
            return;

        try
        {
            var binderData = Project.VFS.FS.ReadFile(binderEntry.Path);

            if (binderData == null)
                return;

            var binder = new BND4Reader(binderData.Value);

            HavokBinarySerializer serializer = new HavokBinarySerializer();
            HavokXmlSerializer xmlSerializer = null;

            foreach (var file in binder.Files)
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);

                var FileBytes = binder.ReadFile(file).ToArray();

                try
                {
                    using (MemoryStream memoryStream = new MemoryStream(FileBytes))
                    {
                        hkRootLevelContainer fileHkx;
                        try
                        {
                            fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);
                        }
                        catch (InvalidDataException)
                        {
                            if (xmlSerializer == null)
                                xmlSerializer = new HavokXmlSerializer();
                            memoryStream.Position = 0;
                            fileHkx = (hkRootLevelContainer)xmlSerializer.Read(memoryStream);
                        }

                        if (!HKX3_Containers.ContainsKey(name))
                        {
                            HKX3_Containers.Add(name, fileHkx);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, $"[{Project}:Map Editor] Failed to serialize havok file: {name}", LogPriority.High, ex);
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[{Project}:Map Editor] Failed to load navmesh models: {binderEntry.Path}", LogPriority.High, e);
        }
    }

    public void LoadHavokNVA(MapContainer map, MapResourceHandler handler)
    {
        if (!CanUse())
            return;

        if (Files.ContainsKey(map.Name))
        {
            var nva = Files[map.Name];

            if (nva != null)
            {
                map.LoadHavokNVA(map.Name, nva);
            }
        }
        else if (Project.Descriptor.ProjectType is ProjectType.ER && ER_Files.ContainsKey(map.Name))
        {
            var nva = ER_Files[map.Name];

            if (nva != null)
            {
                map.LoadHavokNVA(map.Name, nva);
            }
        }
        else
        {
            var entry = Project.Locator.NavmeshFiles.Entries.FirstOrDefault(e => e.Filename == map.Name);
            if (entry != null)
            {
                try
                {
                    var nvaData = Project.Handler.MapData.PrimaryBank.TargetFS.ReadFileOrThrow(entry.Path);

                    if (Project.Descriptor.ProjectType is ProjectType.ER)
                    {
                        try
                        {
                            var nva = NVA_ER.Read(nvaData);

                            ER_Files.Add(Path.GetFileNameWithoutExtension(entry.Filename), nva);

                            map.LoadHavokNVA(map.Name, nva);

                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(this, $"[Map Editor] Failed to read {entry.Path} as NVA", LogPriority.High, e);
                        }
                    }
                    else
                    {
                        try
                        {
                            var nva = NVA.Read(nvaData);

                            Files.Add(Path.GetFileNameWithoutExtension(entry.Filename), nva);

                            map.LoadHavokNVA(map.Name, nva);

                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(this, $"[Map Editor] Failed to read {entry.Path} as NVA", LogPriority.High, e);
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Map Editor] Failed to read {entry.Path} from VFS", LogPriority.High, e);
                }
            }
        }
    }

    public void SaveHavokNVA(MapContainer map)
    {
        if (!CanUse())
            return;

        foreach (var entry in Project.Locator.NavmeshFiles.Entries)
        {
            if (entry.Filename != map.Name)
                continue;

            if (Project.Descriptor.ProjectType is ProjectType.ER)
            {
                try
                {
                    var mapID = Path.GetFileNameWithoutExtension(map.Name);

                    if (map.Name == mapID)
                    {
                        var fileData = Project.Handler.MapData.PrimaryBank.TargetFS.ReadFileOrThrow(entry.Path);
                        var nva = NVA_ER.Read(fileData);

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

                        if (!BytePerfectHelper.Md5Equal(fileData.ToArray(), newFileData))
                        {
                            Project.VFS.ProjectFS.WriteFile(entry.Path, newFileData);
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Map Editor] Failed to write {entry.Path} as NVA", LogPriority.High, e);
                }
            }
            else
            {
                try
                {
                    var mapID = Path.GetFileNameWithoutExtension(map.Name);

                    if (map.Name == mapID)
                    {
                        var fileData = Project.Handler.MapData.PrimaryBank.TargetFS.ReadFileOrThrow(entry.Path);
                        var nva = NVA.Read(fileData);

                        WriteNavmeshInfo(map, nva);
                        WriteFaceData(map, nva);
                        WriteNodeBank(map, nva);
                        WriteSection3(map, nva);
                        WriteConnectors(map, nva);
                        WriteLevelConnectors(map, nva);

                        var newFileData = nva.Write();

                        if (!BytePerfectHelper.Md5Equal(fileData.ToArray(), newFileData))
                        {
                            Project.VFS.ProjectFS.WriteFile(entry.Path, newFileData);
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Map Editor] Failed to write {entry.Path} as NVA", LogPriority.High, e);
                }
            }
        }
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

    public void WriteSection3(MapContainer map, NVA nva)
    {
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
