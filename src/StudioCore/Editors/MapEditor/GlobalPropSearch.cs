using ImGuiNET;
using Org.BouncyCastle.Asn1.X509.Qualified;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Interface;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class GlobalPropSearch
{
    private MapEditorScreen Screen;

    public string _searchInputProperty = "";
    public string _searchInputValue = "";
    public List<MapObjectMatch> Matches = new List<MapObjectMatch>();

    public List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();

    private bool SetupSearch = true;
    public Dictionary<string, IMsb> MapList = new Dictionary<string, IMsb>();

    public bool _targetProjectFiles = false;
    public bool _looseStringMatch = false;

    public GlobalPropSearch(MapEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {
        SetupSearch = true;
        MapList = new Dictionary<string, IMsb>();
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        ImguiUtils.WrappedText("Search through all maps for usage of the specificed property value.");
        ImguiUtils.WrappedText("");

        ImguiUtils.WrappedText("Property Name:");
        ImGui.InputText("##propertyName", ref _searchInputProperty, 255);

        ImguiUtils.WrappedText("Property Value:");
        ImGui.InputText("##propertyValue", ref _searchInputValue, 255);

        ImguiUtils.WrappedText("");
        ImGui.Checkbox("Target Project Files", ref _targetProjectFiles);
        ImguiUtils.ShowHoverTooltip("Uses the project map files instead of game root.");
        ImGui.Checkbox("Loose String Match", ref _looseStringMatch);
        ImguiUtils.ShowHoverTooltip("Only require the string property to contain the search string, instead of requiring an exact match.");

        ImguiUtils.WrappedText("");

        if (ImGui.Button("Search", defaultButtonSize))
        {
            SearchMaps();
        }
        ImguiUtils.ShowHoverTooltip("Initial usage will be slow as all maps have to be loaded. Subsequent usage will be instant.");

        ImguiUtils.WrappedText("");

        DisplayResults();
    }

    public void SetupMapData()
    {
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

                if (!MapList.ContainsKey(name))
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

    public void SearchMaps()
    {
        if (SetupSearch)
        {
            SetupSearch = false;

            SetupMapData();
        }

        Matches = new();

        foreach (KeyValuePair<string, IMsb> entry in MapList)
        {
            BuildResults(entry.Key, entry.Value);
            break; // Temp
        }
    }

    public void BuildResults(string mapName, IMsb map)
    {
        GenerateResults(mapName, map, map.Parts.GetEntries());
        GenerateResults(mapName, map, map.Regions.GetEntries());
        GenerateResults(mapName, map, map.Events.GetEntries());
    }

    public void GenerateResults<T>(string mapName, IMsb map, IReadOnlyList<T> list)
    {
        var searchName = _searchInputProperty.ToLower();
        var searchValue = _searchInputValue.ToLower();

        foreach (var entry in list)
        {
            var properties = entry.GetType().GetProperties();

            foreach (var prop in properties)
            {
                var propName = prop.Name.ToLower();

                if (prop.PropertyType.IsArray)
                {
                    //TaskLogs.AddLog($"{prop.PropertyType} - {prop.Name} - Array");
                }
                if (prop.PropertyType.IsClass)
                {
                    //TaskLogs.AddLog($"{prop.PropertyType} - {prop.Name} - Class");
                }
                else
                {
                    //TaskLogs.AddLog($"{prop.PropertyType} - {prop.Name} - {prop.GetValue(entry)}");

                    if (_looseStringMatch)
                    {
                        if (propName.Contains(searchName))
                        {
                            TaskLogs.AddLog($"{propName} match");
                        }
                    }
                    else
                    {
                        if (searchName == propName)
                        {
                            TaskLogs.AddLog($"{propName} match");
                        }
                    }
                }
            }
        }
    }

    public void DisplayResults()
    {
        if (Matches.Count > 0)
        {
            ImGui.Separator();
            ImguiUtils.WrappedText($"Instances of {_searchInputProperty} == {_searchInputValue}:");
            ImGui.Separator();
            foreach (var entry in Matches)
            {
                if (ImGui.Selectable($"{entry.MapName}"))
                {
                    EditorCommandQueue.AddCommand($"map/load/{entry.MapName}");
                    EditorCommandQueue.AddCommand($"map/select/{entry.MapName}/{entry.EntityName}/{entry.EntityGroupType}");
                }
                var aliasName = AliasUtils.GetMapNameAlias(entry.MapName);
                AliasUtils.DisplayAlias(aliasName);
            }
        }
    }
}

public class MapObjectMatch
{
    public string MapName;
    public string EntityName;

    public string EntityGroupType;
    public string EntityType;

    public string PropertyName;
    public string PropertyValue;

    public MapObjectMatch(string mapname, string entityName, string propName, string propValue)
    {
        MapName = mapname;
        EntityName = entityName;
        PropertyName = propName;
        PropertyValue = propValue;
    }
}
