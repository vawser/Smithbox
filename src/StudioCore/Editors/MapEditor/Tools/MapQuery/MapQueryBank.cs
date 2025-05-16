using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools.MapQuery;

public class MapQueryBank
{
    private MapEditorScreen Editor;

    private IMapQueryEngine Engine;

    public bool MapBankInitialized = false;

    private List<ResourceDescriptor> MapResources = new List<ResourceDescriptor>();

    public Dictionary<string, IMsb> MapList = new Dictionary<string, IMsb>();

    public MapQueryBank(MapEditorScreen editor, IMapQueryEngine engine)
    {
        Editor = editor;

        Engine = engine;

        if (Editor.Project.ProjectType is ProjectType.Undefined)
        {
            MapBankInitialized = true;
        }
    }

    public void OnProjectChanged()
    {
        MapBankInitialized = false;
        MapList = new Dictionary<string, IMsb>();

        if (Editor.Project.ProjectType is ProjectType.Undefined)
        {
            MapBankInitialized = true;
        }
    }

    public Dictionary<string, IMsb> GetMaps()
    {
        return MapList;
    }

    public async void SetupData()
    {
        if (!CFG.Current.MapEditor_LoadMapQueryData)
            return;

        ReadMapResources();

        // Load the maps async so the main thread isn't blocked
        Task<bool> loadMapsTask = ReadMaps();

        bool result = await loadMapsTask;
        MapBankInitialized = result;
    }

    public void ReadMapResources()
    {
        MapResources = new List<ResourceDescriptor>();

        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            var mapDir = $"{Editor.Project.DataPath}/map/";

            if (Engine.GetProjectFileUsage())
            {
                mapDir = $"{Editor.Project.ProjectPath}/map/";
            }

            if (Directory.Exists(mapDir))
            {
                foreach (var entry in Directory.EnumerateDirectories(mapDir))
                {
                    foreach (var fileEntry in Directory.EnumerateFiles(entry))
                    {
                        if (fileEntry.Contains(".msb"))
                        {
                            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fileEntry));
                            ResourceDescriptor ad = MapLocator.GetMapMSB(Editor.Project, name);
                            if (ad.AssetPath != null)
                            {
                                MapResources.Add(ad);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            var innerDir = "mapstudio";
            var ext = ".msb.dcx";

            if (Editor.Project.ProjectType is ProjectType.DS1)
            {
                innerDir = "MapStudio";
                ext = ".msb";
            }

            if (Editor.Project.ProjectType is ProjectType.DS1R)
            {
                innerDir = "MapStudioNew";
                ext = ".msb";
            }

            var mapDir = $"{Editor.Project.DataPath}/map/{innerDir}/ ";

            if (Engine.GetProjectFileUsage())
            {
                mapDir = $"{Editor.Project.ProjectPath}/map/{innerDir}/";
            }

            if (Directory.Exists(mapDir))
            {
                foreach (var entry in Directory.EnumerateFiles(mapDir))
                {
                    if (entry.Contains($"{ext}"))
                    {
                        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
                        ResourceDescriptor ad = MapLocator.GetMapMSB(Editor.Project, name);
                        if (ad.AssetPath != null)
                        {
                            MapResources.Add(ad);
                        }
                    }
                }
            }
        }
    }

    public async Task<bool> ReadMaps()
    {
        await Task.Yield();

        foreach (var resource in MapResources)
        {
            if (resource == null)
                continue;

            if (resource.AssetPath == null || resource.AssetPath == "")
                continue;

            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(resource.AssetPath));
            IMsb msb = null;

            if (Editor.Project.ProjectType == ProjectType.DES)
            {
                msb = MSBD.Read(resource.AssetPath);
            }
            if (Editor.Project.ProjectType == ProjectType.DS1 || Editor.Project.ProjectType == ProjectType.DS1R)
            {
                msb = MSB1.Read(resource.AssetPath);
            }
            if (Editor.Project.ProjectType == ProjectType.DS2 || Editor.Project.ProjectType == ProjectType.DS2S)
            {
                msb = MSB2.Read(resource.AssetPath);
            }
            if (Editor.Project.ProjectType == ProjectType.DS3)
            {
                msb = MSB3.Read(resource.AssetPath);
            }
            if (Editor.Project.ProjectType == ProjectType.BB)
            {
                msb = MSBB.Read(resource.AssetPath);
            }
            if (Editor.Project.ProjectType == ProjectType.SDT)
            {
                msb = MSBS.Read(resource.AssetPath);
            }
            if (Editor.Project.ProjectType == ProjectType.ER)
            {
                msb = MSBE.Read(resource.AssetPath);
            }
            if (Editor.Project.ProjectType == ProjectType.AC6)
            {
                msb = MSB_AC6.Read(resource.AssetPath);
            }

            if (msb != null && !MapList.ContainsKey(name))
            {
                MapList.Add(name, msb);
            }
        }

        return true;
    }

}
