using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Xml;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static SoulsFormats.NVA;

namespace StudioCore.Editors.MapEditor;

public class HavokNavmeshBank
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public Dictionary<string, NVA> Files = new();

    public Dictionary<string, hkRootLevelContainer> HKX3_Containers = new Dictionary<string, hkRootLevelContainer>();

    public HavokNavmeshBank(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public bool CanUse()
    {
        if (Project.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT or ProjectType.ER)
            return true;

        // NOTE: SDT doesn't render the meshes as it doesn't have HKX support yet

        return false;
    }

    public void OnLoadMap(string mapId)
    {
        if (Project.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            LoadNavmeshModels(mapId);
        }
    }

    public void OnUnloadMap(string mapId)
    {
        if (Project.ProjectType is ProjectType.ER or ProjectType.NR)
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
        var binderEntry = Project.FileDictionary.Entries.FirstOrDefault(
            e => e.Filename == mapId &&
            e.Extension == "nvmhktbnd");

        if (binderEntry == null)
            return;

        try
        {
            var binderData = Project.FS.ReadFile(binderEntry.Path);

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
                    TaskLogs.AddLog($"[{Project}:Map Editor] Failed to serialize havok file: {name}", LogLevel.Error, LogPriority.High, ex);
                }
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project}:Map Editor] Failed to load navmesh models: {binderEntry.Path}", LogLevel.Error, LogPriority.High, e);
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
        else
        {
            var entry = Project.MapData.NavmeshFiles.Entries.FirstOrDefault(e => e.Filename == map.Name);
            if (entry != null)
            {
                try
                {
                    var nvaData = Project.MapData.PrimaryBank.TargetFS.ReadFileOrThrow(entry.Path);

                    try
                    {
                        var nva = NVA.Read(nvaData);

                        Files.Add(Path.GetFileNameWithoutExtension(entry.Filename), nva);

                        map.LoadHavokNVA(map.Name, nva);

                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {entry.Path} as NVA", LogLevel.Error, LogPriority.High, e);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to read {entry.Path} from VFS", LogLevel.Error, LogPriority.High, e);
                }
            }
        }
    }

    public void SaveHavokNVA(MapEditorScreen editor, MapContainer map)
    {
        if (!CanUse())
            return;

        // Not yet BP for ER, so ignore save for public users
#if !DEBUG
        if (Project.ProjectType is ProjectType.ER or ProjectType.NR)
            return;
#endif

        foreach (var entry in Project.MapData.NavmeshFiles.Entries)
        {
            if (entry.Filename != map.Name)
                continue;

            try
            {
                var mapID = Path.GetFileNameWithoutExtension(map.Name);

                if (map.Name == mapID)
                {
                    var fileData = Project.MapData.PrimaryBank.TargetFS.ReadFileOrThrow(entry.Path);
                    var nva = NVA.Read(fileData);

                    WriteNavmeshInfo(map, nva);
                    WriteFaceData(map, nva);
                    WriteNodeBank(map, nva);
                    WriteSection3(map, nva);
                    WriteConnectors(map, nva);
                    WriteLevelConnectors(map, nva);

                    if (nva.Version is NVAVersion.EldenRing)
                    {
                        WriteSection9(map, nva);
                        WriteSection10(map, nva);
                        WriteSection11(map, nva);
                        WriteSection12(map, nva);
                        WriteSection13(map, nva);
                    }

                    var newFileData = nva.Write();
                    Project.ProjectFS.WriteFile(entry.Path, newFileData);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to write {entry.Path} as NVA", LogLevel.Error, LogPriority.High, e);
            }
        }
    }

    public void WriteNavmeshInfo(MapContainer map, NVA nva)
    {
        var version = nva.NavmeshInfoEntries.Version;

        var newSection = new NavmeshInfoSection((int)version);

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NavmeshInfo))
            {
                var navmesh = (NavmeshInfo)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.NavmeshInfoEntries = newSection;
    }

    public void WriteFaceData(MapContainer map, NVA nva)
    {
        var version = nva.FaceDataEntries.Version;

        var newSection = new FaceDataSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(FaceData))
            {
                var navmesh = (FaceData)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.FaceDataEntries = newSection;
    }

    public void WriteNodeBank(MapContainer map, NVA nva)
    {
        var version = nva.NodeBankEntries.Version;

        var newSection = new NodeBankSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(NodeBank))
            {
                var navmesh = (NodeBank)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.NodeBankEntries = newSection;
    }

    public void WriteSection3(MapContainer map, NVA nva)
    {
        var version = nva.Section3Entries.Version;

        var newSection = new Section3();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(Entry3))
            {
                var navmesh = (Entry3)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Section3Entries = newSection;
    }

    public void WriteConnectors(MapContainer map, NVA nva)
    {
        var version = nva.ConnectorEntries.Version;

        var newSection = new ConnectorSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(Connector))
            {
                var navmesh = (Connector)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.ConnectorEntries = newSection;
    }

    public void WriteLevelConnectors(MapContainer map, NVA nva)
    {
        var version = nva.LevelConnectorEntries.Version;

        var newSection = new LevelConnectorSection();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(LevelConnector))
            {
                var navmesh = (LevelConnector)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.LevelConnectorEntries = newSection;
    }

    public void WriteSection9(MapContainer map, NVA nva)
    {
        var version = nva.Section9Entries.Version;

        var newSection = new Section9();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(Entry9))
            {
                var navmesh = (Entry9)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Section9Entries = newSection;
    }

    public void WriteSection10(MapContainer map, NVA nva)
    {
        var version = nva.Section10Entries.Version;

        var newSection = new Section10();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(Entry10))
            {
                var navmesh = (Entry10)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Section10Entries = newSection;
    }

    public void WriteSection11(MapContainer map, NVA nva)
    {
        var version = nva.Section11Entries.Version;

        var newSection = new Section11();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(Entry11))
            {
                var navmesh = (Entry11)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Section11Entries = newSection;
    }

    public void WriteSection12(MapContainer map, NVA nva)
    {
        var version = nva.Section12Entries.Version;

        var newSection = new Section12();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(Entry12))
            {
                var navmesh = (Entry12)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Section12Entries = newSection;
    }

    public void WriteSection13(MapContainer map, NVA nva)
    {
        var version = nva.Section13Entries.Version;

        var newSection = new Section13();

        foreach (var curEnt in map.NavmeshParent.Children)
        {
            if (curEnt.WrappedObject.GetType() == typeof(Entry13))
            {
                var navmesh = (Entry13)curEnt.WrappedObject;

                newSection.Add(navmesh);
            }
        }

        nva.Section13Entries = newSection;
    }
}
