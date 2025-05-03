using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.ImGui;
using System.Text.RegularExpressions;
using StudioCore.Editor;
using StudioCore.Utilities;
using StudioCore.Interface;
using StudioCore.Resource.Locators;
using StudioCore.Core;

namespace StudioCore.Editors.ModelEditor.Utils;

public class GlobalModelSearch
{
    private ModelEditorScreen Editor;

    public string _searchInput = "";
    public List<MapModelMatch> Matches = new List<MapModelMatch>();

    public List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();
    public bool _targetProjectFiles = false;
    public bool _looseModelNameMatch = false;

    private bool SetupSearch = true;

    public Dictionary<string, IMsb> MapList = new Dictionary<string, IMsb>();

    public GlobalModelSearch(ModelEditorScreen screen)
    {
        Editor = screen;
    }

    public void OnProjectChanged()
    {
        SetupSearch = true;
        MapList = new Dictionary<string, IMsb>();
    }

    public void SearchMaps()
    {
        if (SetupSearch)
        {
            SetupSearch = false;

            resMaps = new List<ResourceDescriptor>();
            void FindLooseMaps(string dir, string wildcard)
            {
                foreach (var entry in Directory.EnumerateFiles(dir, wildcard, SearchOption.AllDirectories))
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
                    ResourceDescriptor ad = MapLocator.GetMapMSB(Editor.Project, name);
                    if (ad.AssetPath != null)
                    {
                        resMaps.Add(ad);
                    }
                }
            }

            void AddMaps(Func<string, IMsb> read)
            {
                foreach (var res in resMaps)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(res.AssetPath));
                    var msb = read(res.AssetPath);

                    if (!MapList.ContainsKey(name))
                        MapList.Add(name, msb);
                }
            }

            if (Editor.Project.ProjectType is ProjectType.ACFA or ProjectType.ACV or ProjectType.ACVD)
            {
                string modelMapDir;
                string sysMapDir;
                if (_targetProjectFiles)
                {
                    modelMapDir = $"{Editor.Project.ProjectPath}/model/map/";
                    sysMapDir = $"{Editor.Project.ProjectPath}/model/system/";
                }
                else
                {
                    modelMapDir = $"{Editor.Project.DataPath}/model/map/";
                    sysMapDir = $"{Editor.Project.DataPath}/model/system/";
                }

                FindLooseMaps(modelMapDir, "*.msb");
                FindLooseMaps(sysMapDir, "*.msb");
            }
            else
            {
                string mapDir;
                if (_targetProjectFiles)
                    mapDir = $"{Editor.Project.ProjectPath}/map/mapstudio/";
                else
                    mapDir = $"{Editor.Project.DataPath}/map/mapstudio/";

                FindLooseMaps(mapDir, "*.msb.dcx");
            }

            switch (Editor.Project.ProjectType)
            {
                case ProjectType.DES:
                    AddMaps(MSBD.Read);
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    AddMaps(MSB1.Read);
                    break;
                case ProjectType.DS2:
                case ProjectType.DS2S:
                    AddMaps(MSB2.Read);
                    break;
                case ProjectType.DS3:
                    AddMaps(MSB3.Read);
                    break;
                case ProjectType.BB:
                    AddMaps(MSBB.Read);
                    break;
                case ProjectType.SDT:
                    AddMaps(MSBS.Read);
                    break;
                case ProjectType.ER:
                    AddMaps(MSBE.Read);
                    break;
                case ProjectType.AC6:
                    AddMaps(MSB_AC6.Read);
                    break;
                case ProjectType.ACFA:
                    AddMaps(MSBFA.Read);
                    break;
                case ProjectType.ACV:
                    AddMaps(MSBV.Read);
                    break;
                case ProjectType.ACVD:
                    AddMaps(MSBVD.Read);
                    break;
            }
        }

        Matches = new();

        foreach (KeyValuePair<string, IMsb> entry in MapList)
        {
            CompileResults(entry.Key, entry.Value);
        }
    }

    public void CompileResults(string mapName, IMsb map)
    {
        var searchInput = _searchInput.ToLower();

        foreach (var entry in map.Parts.GetEntries())
        {
            var modelName = entry.ModelName;

            if (modelName != null)
            {
                modelName = modelName.ToLower();

                if (_looseModelNameMatch)
                {
                    if (modelName.Contains(searchInput))
                    {
                        if (!Matches.Any(e => e.MapName == mapName))
                        {
                            var match = new MapModelMatch(mapName, modelName, entry.Name);
                            Matches.Add(match);
                        }
                    }
                }
                else
                {
                    if (modelName == searchInput)
                    {
                        if (!Matches.Any(e => e.MapName == mapName))
                        {
                            var match = new MapModelMatch(mapName, modelName, entry.Name);
                            Matches.Add(match);
                        }
                    }
                }
            }
        }
    }

    public void DisplayInstances()
    {
        if (Matches.Count > 0)
        {
            ImGui.Separator();
            UIHelper.WrappedText($"Instances of {_searchInput}:");
            ImGui.Separator();
            foreach (var entry in Matches)
            {
                if (ImGui.Selectable($"{entry.MapName}"))
                {
                    EditorCommandQueue.AddCommand($"map/load/{entry.MapName}");
                    EditorCommandQueue.AddCommand($"map/select/{entry.MapName}/{entry.EntityName}/Part");
                }
                var aliasName = AliasUtils.GetMapNameAlias(Editor.Project, entry.MapName);
                UIHelper.DisplayAlias(aliasName);
            }
        }
    }
}

public class MapModelMatch
{
    public string MapName;
    public string ModelName;
    public string EntityName;

    public MapModelMatch(string mapname, string modelname, string entityName)
    {
        MapName = mapname;
        ModelName = modelname;
        EntityName = entityName;
    }
}
