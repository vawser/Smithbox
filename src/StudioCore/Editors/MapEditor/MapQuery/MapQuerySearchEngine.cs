using CommunityToolkit.HighPerformance;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static StudioCore.Editors.MapEditor.MapPropertyEditor;

namespace StudioCore.Editors.MapEditor.MapQuery;

public class MapQuerySearchEngine : IMapQueryEngine
{
    private MapEditorScreen Screen;
    private MapQueryBank Bank;

    public bool IsOpen = false;

    public string _searchInputMap = "";
    public string _searchInputProperty = "";
    public string _searchInputValue = "";
    public Dictionary<string, List<MapObjectMatch>> Matches = new();

    public bool _targetProjectFiles = false;
    public bool _looseStringMatch = false;

    public bool QueryComplete = false;
    public bool MayRunQuery = true;

    public string MapFilterInputs = "";
    public string PropertyFilterInputs = "";
    public string ValueFilterInputs = "";

    public MapQuerySearchEngine(MapEditorScreen screen)
    {
        Screen = screen;
        Bank = new MapQueryBank(this);
    }

    public void OnProjectChanged()
    {
        Bank.OnProjectChanged();
        Bank.SetupData();
    }

    public void DisplayInput()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);
        var halfButtonSize = new Vector2(windowWidth / 2, 32);

        if (Bank.MapBankInitialized)
        {
            ImguiUtils.WrappedText("Search through all maps for usage of the specificed property value.");
            ImguiUtils.WrappedText("");

            // Map Filter
            ImguiUtils.WrappedText("Map Filter:");
            ImguiUtils.ShowHoverTooltip("Target this specific string when querying the map. Supports regex.\n\n" + $"Multiple filters can be used by using the '|' symbol between each filter, acting as an OR operator.");
            ImGui.InputText("##mapFilter", ref _searchInputMap, 255);

            if (ImGui.BeginPopupContextItem($"MapFilterContextMenu"))
            {
                // Quick Regex buttons
                if (ImGui.Selectable("Exact"))
                {
                    _searchInputMap = $"^{_searchInputMap}$";
                }
                ImguiUtils.ShowHoverTooltip("Apply regex that makes the current input match exactly.");

                ImGui.EndPopup();
            }

            // Property Filter
            ImguiUtils.WrappedText("Property Filter:");
            ImguiUtils.ShowHoverTooltip("Target this specific string when querying the property name. Supports regex.\n\n" + $"Multiple filters can be used by using the '|' symbol between each filter, acting as an OR operator.");
            ImGui.InputText("##propertyNameFilter", ref _searchInputProperty, 255);

            // TODO: add arrow button that lets user traverse a tree that displays the MSB structure, allowing them to select properties easily without needing to load a map

            if (ImGui.BeginPopupContextItem($"PropertyFilterContextMenu"))
            {
                // Quick Regex buttons
                if (ImGui.Selectable("Exact"))
                {
                    _searchInputProperty = $"^{_searchInputProperty}$";
                }
                ImguiUtils.ShowHoverTooltip("Apply regex that makes the current input match exactly.");
                if (ImGui.Selectable("Index"))
                {
                    _searchInputProperty = @$"{_searchInputProperty}\[0\]";
                }
                ImguiUtils.ShowHoverTooltip("Escaped square brackets for targeting specific index in array properties.");

                ImGui.EndPopup();
            }

            // Value Filter
            ImguiUtils.WrappedText("Value Filter:");
            ImguiUtils.ShowHoverTooltip("Target this specific string when querying the property value. Supports regex.\n\n" + $"Multiple filters can be used by using the '|' symbol between each filter, acting as an OR operator.");
            ImGui.InputText("##propertyValueFilter", ref _searchInputValue, 255);

            if (ImGui.BeginPopupContextItem($"ValueFilterContextMenu"))
            {
                // Quick Regex buttons
                if (ImGui.Selectable("Exact"))
                {
                    _searchInputValue = $"^{_searchInputValue}$";
                }
                ImguiUtils.ShowHoverTooltip("Apply regex that makes the current input match exactly.");
                if (ImGui.Selectable("Non-Zero Number"))
                {
                    _searchInputValue = "^[1-9]\\d*$";
                }
                ImguiUtils.ShowHoverTooltip("Apply regex that makes the current input match non-zero numbers.");

                ImGui.EndPopup();
            }

            ImguiUtils.WrappedText("");
            ImGui.Checkbox("Target Project Files", ref _targetProjectFiles);
            ImguiUtils.ShowHoverTooltip("Uses the project map files instead of game root.");

            ImguiUtils.WrappedText("");

            if (!MayRunQuery)
            {
                ImGui.BeginDisabled();
                if (ImGui.Button("Search", halfButtonSize))
                {
                }
                ImGui.EndDisabled();
                ImGui.SameLine();
                ImGui.BeginDisabled();
                if (ImGui.Button("Clear", halfButtonSize))
                {
                }
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Search", halfButtonSize))
                {
                    RunQuery();
                }
                ImGui.SameLine();
                if (ImGui.Button("Clear", halfButtonSize))
                {
                    _searchInputMap = "";
                    _searchInputProperty = "";
                    _searchInputValue = "";
                }
            }
        }
        else
        {
            ImguiUtils.WrappedText("Map Query Engine is loading...");
            ImguiUtils.WrappedText("");
        }
    }

    public void DisplayResults()
    {
        if (Bank.MapBankInitialized)
        {
            ImguiUtils.WrappedText("");

            ImGui.Separator();
            ImguiUtils.WrappedText($"Search Results:");
            ImguiUtils.ShowHoverTooltip("Result rows are presented as: <entity name>, <name alias>, <matched value>");
            ImGui.Separator();

            if (QueryComplete)
            {
                DisplayResultList();
            }
            else if (!MayRunQuery)
            {
                ImguiUtils.WrappedText($"Search query is not yet complete...");
            }
        }
    }

    List<Regex> MapFilterPatterns = new List<Regex>();
    List<Regex> PropertyFilterPatterns = new List<Regex>();
    List<Regex> ValueFilterPatterns = new List<Regex>();

    public async void RunQuery()
    {
        if (!Bank.MapBankInitialized)
            return;

        QueryComplete = false;
        MayRunQuery = false;

        Matches = new();
        MapFilterPatterns = new List<Regex>();
        PropertyFilterPatterns = new List<Regex>();
        ValueFilterPatterns = new List<Regex>();

        // Assigned to new vars so we can ignore if the user edits the input vars via the text input
        MapFilterInputs = _searchInputMap.ToLower();
        PropertyFilterInputs = _searchInputProperty.ToLower();
        ValueFilterInputs = _searchInputValue.ToLower();

        MapFilterPatterns = BuildFilterPatterns(MapFilterInputs);
        PropertyFilterPatterns = BuildFilterPatterns(PropertyFilterInputs);
        ValueFilterPatterns = BuildFilterPatterns(ValueFilterInputs);

        // Load the maps async so the main thread isn't blocked
        Task<bool> runQueryTask = BuildResults();

        bool result = await runQueryTask;
        QueryComplete = result;
        MayRunQuery = result;
    }

    public List<Regex> BuildFilterPatterns(string inputs)
    {
        List<Regex> patternList = new();

        var sections = inputs.Split("|");

        for (int i = 0; i < sections.Count(); i++)
        {
            var input = sections[i].ToLower();

            try
            {
                Regex filter = new Regex(input, RegexOptions.IgnoreCase);
                patternList.Add(filter);
            }
            catch
            {
                TaskLogs.AddLog($"Failed to add filter pattern due to invalid regex expression: {input}");
            }
        }

        return patternList;
    }

    public async Task<bool> BuildResults()
    {
        await Task.Delay(1000);

        // Map
        foreach (KeyValuePair<string, IMsb> entry in Bank.GetMaps())
        {
            var mapName = entry.Key;
            var map = entry.Value;

            // Apply map filters here
            if (IsMapFilterMatch(mapName))
            {
                CheckPropertyList(mapName, map, map.Parts.GetEntries());
                CheckPropertyList(mapName, map, map.Regions.GetEntries());
                CheckPropertyList(mapName, map, map.Events.GetEntries());
            }
        }

        return true;
    }

    public void CheckPropertyList<T>(string mapName, IMsb map, IReadOnlyList<T> list)
    {
        // Property List
        foreach (var entry in list)
        {
            var properties = entry.GetType().GetProperties();

            var entityName = "";

            var msbEntity = (IMsbEntry)entry;
            if (msbEntity != null)
            {
                entityName = msbEntity.Name;
            }

            CheckProperties(entry, properties, mapName, entityName);
        }
    }

    public void CheckProperties<T>(T obj, PropertyInfo[] properties, string mapName, string entityName)
    {
        // Property
        foreach (PropertyInfo prop in properties)
        {
            Type typ = prop.PropertyType;

            if (typ.IsArray)
            {
                var a = (Array)prop.GetValue(obj);
                for (var i = 0; i < a.Length; i++)
                {
                    Type arrtyp = typ.GetElementType();

                    if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                    {
                        var o = a.GetValue(i);
                        CheckProperties(o, o.GetType().GetProperties(), mapName, entityName);
                    }
                    else
                    {
                        CheckProperty(prop, obj, properties, mapName, entityName, a, i);
                    }
                }
            }
            else if (typ.IsGenericType && typ.GetGenericTypeDefinition() == typeof(List<>))
            {
                var l = prop.GetValue(obj);
                PropertyInfo itemprop = l.GetType().GetProperty("Item");
                var count = (int)l.GetType().GetProperty("Count").GetValue(l);
                for (var i = 0; i < count; i++)
                {
                    Type arrtyp = typ.GetGenericArguments()[0];
                    if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                    {
                        var o = itemprop.GetValue(l, new object[] { i });
                        CheckProperties(o, o.GetType().GetProperties(), mapName, entityName);
                    }
                    else
                    {
                        CheckProperty(prop, obj, properties, mapName, entityName);
                    }
                }
            }
            else if (typ.IsClass && typ == typeof(MSB.Shape))
            {
                var o = prop.GetValue(obj);
                var shapetype = Enum.Parse<RegionShape>(o.GetType().Name);
                var shap = (int)shapetype;

                CheckProperties(o, o.GetType().GetProperties(), mapName, entityName);
            }
            else if (typ == typeof(BTL.LightType))
            {
                var o = prop.GetValue(obj);
                var enumTypes = Enum.Parse<LightType>(o.ToString());
                var thisType = (int)enumTypes;

                CheckProperties(o, o.GetType().GetProperties(), mapName, entityName);
            }
            else if (typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                var o = prop.GetValue(obj);

                CheckProperties(o, o.GetType().GetProperties(), mapName, entityName);
            }
            else
            {
                CheckProperty(prop, obj, properties, mapName, entityName);
            }
        }
    }

    public void CheckProperty<T>(PropertyInfo prop, T obj, PropertyInfo[] properties, string mapName, string entityName, Array arr = null, int index = -1)
    {
        var propName = prop.Name;
        var propValue = prop.GetValue(obj, null);

        if (arr != null && index != -1)
        {
            propName = $"{prop.Name}[{index}]";
            propValue = arr.GetValue(index);
        }

        if (IsPropertyMatch(propName, index))
        {
            if (IsValueMatch(propValue, prop))
            {
                MapObjectMatch match = new MapObjectMatch(obj, mapName, entityName, propName, $"{propValue}");

                if(!Matches.ContainsKey(mapName))
                {
                    Matches.Add(mapName, new List<MapObjectMatch>() { match } );
                }
                else
                {
                    Matches[mapName].Add(match);
                }
            }
        }
    }

    public void DisplayResultList()
    {
        if (Matches.Count > 0)
        {
            for (int i = 0; i < Matches.Count; i++)
            {
                var pair = Matches.ElementAt(i);
                var mapName = pair.Key;
                var mapAlias = AliasUtils.GetMapNameAlias(mapName);
                var displayName = $"{mapName}: {mapAlias}";
                var objectMatches = pair.Value;

                if (ImGui.CollapsingHeader($"Matches in {displayName}##header{mapName}"))
                {
                    foreach (var entry in objectMatches)
                    {
                        if (ImGui.Selectable($"{entry.EntityName}##row{i}"))
                        {
                            EditorCommandQueue.AddCommand($"map/load/{entry.MapName}");
                            EditorCommandQueue.AddCommand($"map/select/{entry.MapName}/{entry.EntityName}");
                        }
                        if (ImGui.IsItemVisible())
                        {
                            // Entity Name
                            var alias = GetUnknownAlias(entry.EntityName);
                            AliasUtils.DisplayColoredAlias(alias, CFG.Current.ImGui_AliasName_Text);

                            // Value
                            AliasUtils.DisplayColoredAlias($"- {entry.PropertyName}: {entry.PropertyValue}", CFG.Current.ImGui_Benefit_Text_Color);
                        }
                    }
                }
                ImguiUtils.ShowHoverTooltip($"Number of matches: {objectMatches.Count}");
            }
        }
    }

    private string GetUnknownAlias(string rawName)
    {
        rawName = rawName.ToLower();
        if(rawName.Contains("_"))
        {
            rawName = rawName.Split("_")[0];
        }

        string alias = "";

        var chrAlias =  GetAlias(rawName, Smithbox.AliasCacheHandler.AliasCache.Characters);
        var assetAlias = GetAlias(rawName, Smithbox.AliasCacheHandler.AliasCache.Assets);
        var mapPieceAlias = GetAlias(rawName, Smithbox.AliasCacheHandler.AliasCache.MapPieces);

        if (chrAlias != "")
            return chrAlias;

        if(assetAlias != "")
            return assetAlias;

        if (mapPieceAlias != "")
            return mapPieceAlias;

        return alias;
    }

    private string GetAlias(string name, Dictionary<string, AliasReference> referenceDict)
    {
        var lowerName = name.ToLower();

        if (referenceDict.ContainsKey(lowerName))
        {
            return referenceDict[lowerName].name;
        }

        return "";
    }
    
    public bool IsMapFilterMatch(string mapName)
    {
        if (MapFilterInputs == "")
            return true;

        foreach (var entry in MapFilterPatterns)
        {
            if (entry.IsMatch(mapName))
            {
                //TaskLogs.AddLog($"Match: {propName}");
                return true;
            }
        }

        return false;
    }


    public bool IsPropertyMatch(string propName, int index)
    {
        if (PropertyFilterInputs == "")
            return true;

        var checkedName = propName;

        for(int i = 0; i < PropertyFilterPatterns.Count; i++)
        {
            var entry = PropertyFilterPatterns[i];

            if (entry.IsMatch(checkedName))
            {
                //TaskLogs.AddLog($"Match: {propName}");
                return true;
            }
        }

        return false;
    }

    public bool IsValueMatch(object propValue, PropertyInfo prop)
    {
        if (ValueFilterInputs == "")
            return true;

        foreach (var entry in ValueFilterPatterns)
        {
            if (entry.IsMatch($"{propValue}"))
            {
                //TaskLogs.AddLog($"Match: {propValue}");
                return true;
            }
        }

        return false;
    }
    public void AddMapFilterInput(string mapID)
    {
        var addition = $"{mapID}";

        if (_searchInputMap != "")
        {
            addition = $"|{addition}";
        }

        _searchInputMap = _searchInputMap + addition;
    }

    public void AddPropertyFilterInput(PropertyInfo property, int arrayIndex)
    {
        var addition = $"{property.Name}";

        if(arrayIndex != -1)
        {
            addition = @$"{addition}\[{arrayIndex}\]";
        }

        if (_searchInputProperty != "")
        {
            addition = $"|{addition}";
        }

        _searchInputProperty = _searchInputProperty + addition;
    }
    public bool GetProjectFileUsage()
    {
        return _targetProjectFiles;
    }
}

