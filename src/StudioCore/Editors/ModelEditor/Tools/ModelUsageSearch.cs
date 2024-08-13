using SoulsFormats;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using System.Text.RegularExpressions;
using StudioCore.Editor;

namespace StudioCore.Editors.ModelEditor.Tools;

public class ModelUsageSearch
{
    private ModelEditorScreen Screen;

    public string _searchInput = "";
    public List<ModelUsageMatch> Matches = new List<ModelUsageMatch>();

    public List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();
    public bool _targetProjectFiles = false;
    public bool _looseModelNameMatch = false;

    private bool SetupSearch = true;

    public Dictionary<string, IMsb> MapList = new Dictionary<string, IMsb>();

    public ModelUsageSearch(ModelEditorScreen screen)
    {
        Screen = screen;
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

            var mapDir = $"{Smithbox.GameRoot}/map/mapstudio/";

            if (_targetProjectFiles)
            {
                mapDir = $"{Smithbox.ProjectRoot}/map/mapstudio/";
            }

            foreach (var entry in Directory.EnumerateFiles(mapDir))
            {
                if (entry.Contains(".msb.dcx"))
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
                    ResourceDescriptor ad = MapLocator.GetMapMSB(name);
                    if (ad.AssetPath != null)
                    {
                        resMaps.Add(ad);
                    }
                }
            }

            if (Smithbox.ProjectType == ProjectType.DES)
            {
                foreach (var res in resMaps)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(res.AssetPath));
                    var msb = MSBD.Read(res.AssetPath);

                    if(!MapList.ContainsKey(name))
                        MapList.Add(name, msb);
                }
            }
            if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
            {
                foreach (var res in resMaps)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(res.AssetPath));
                    var msb = MSB1.Read(res.AssetPath);

                    if (!MapList.ContainsKey(name))
                        MapList.Add(name, msb);
                }
            }
            if (Smithbox.ProjectType == ProjectType.DS2 || Smithbox.ProjectType == ProjectType.DS2S)
            {
                foreach (var res in resMaps)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(res.AssetPath));
                    var msb = MSB2.Read(res.AssetPath);

                    if (!MapList.ContainsKey(name))
                        MapList.Add(name, msb);
                }
            }
            if (Smithbox.ProjectType == ProjectType.DS3)
            {
                foreach (var res in resMaps)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(res.AssetPath));
                    var msb = MSB3.Read(res.AssetPath);

                    if (!MapList.ContainsKey(name))
                        MapList.Add(name, msb);
                }
            }
            if (Smithbox.ProjectType == ProjectType.BB)
            {
                foreach (var res in resMaps)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(res.AssetPath));
                    var msb = MSBB.Read(res.AssetPath);

                    if (!MapList.ContainsKey(name))
                        MapList.Add(name, msb);
                }
            }
            if (Smithbox.ProjectType == ProjectType.SDT)
            {
                foreach (var res in resMaps)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(res.AssetPath));
                    var msb = MSBS.Read(res.AssetPath);

                    if (!MapList.ContainsKey(name))
                        MapList.Add(name, msb);
                }
            }
            if (Smithbox.ProjectType == ProjectType.ER)
            {
                foreach (var res in resMaps)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(res.AssetPath));
                    var msb = MSBE.Read(res.AssetPath);

                    if (!MapList.ContainsKey(name))
                        MapList.Add(name, msb);
                }
            }
            if (Smithbox.ProjectType == ProjectType.AC6)
            {
                foreach (var res in resMaps)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(res.AssetPath));
                    var msb = MSB_AC6.Read(res.AssetPath);

                    if (!MapList.ContainsKey(name))
                        MapList.Add(name, msb);
                }
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
            if(modelName != null)
                modelName = modelName.ToLower();

            if (_looseModelNameMatch)
            {
                if (modelName.Contains(searchInput))
                {
                    if(!Matches.Any(e => e.MapName == mapName))
                    {
                        var match = new ModelUsageMatch(mapName, modelName, entry.Name);
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
                        var match = new ModelUsageMatch(mapName, modelName, entry.Name);
                        Matches.Add(match);
                    }
                }
            }
        }
    }

    public void DisplayInstances()
    {
        if(Matches.Count > 0)
        {
            ImGui.Separator();
            ImguiUtils.WrappedText($"Instances of {_searchInput}:");
            ImGui.Separator();
            foreach (var entry in Matches)
            {
                if(ImGui.Selectable($"{entry.MapName}"))
                {
                    EditorCommandQueue.AddCommand($"map/load/{entry.MapName}");
                    EditorCommandQueue.AddCommand($"map/select/{entry.MapName}/{entry.EntityName}/Part");
                }
                var aliasName = AliasUtils.GetMapNameAlias(entry.MapName);
                AliasUtils.DisplayAlias(aliasName);
            }
        }
    }
}

public class ModelUsageMatch
{
    public string MapName;
    public string ModelName;
    public string EntityName;

    public ModelUsageMatch(string mapname, string modelname, string entityName) 
    { 
        MapName = mapname;
        ModelName = modelname;
        EntityName = entityName;
    }
}
