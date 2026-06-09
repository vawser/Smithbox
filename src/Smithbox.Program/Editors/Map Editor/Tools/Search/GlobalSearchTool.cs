using CommunityToolkit.HighPerformance;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static StudioCore.Editors.MapEditor.MapPropertyView;

namespace StudioCore.Editors.MapEditor;

public class GlobalSearchTool : IMapQueryEngine
{
    public MapEditorView View;
    public ProjectEntry Project;

    public MapQueryBank Bank;

    public bool IsOpen = false;

    public string _searchInputMap = "";
    public string _searchInputProperty = "";
    public string _searchInputValue = "";
    public Dictionary<string, List<MapObjectMatch>> Matches = new();

    public bool _targetProjectFiles = false;
    public bool _looseStringMatch = false;
    public bool _ignoreDummyParts = false;

    public bool QueryComplete = false;
    public bool MayRunQuery = true;
    public bool UserLoadedData = false;

    public string MapFilterInputs = "";
    public string PropertyFilterInputs = "";
    public string ValueFilterInputs = "";

    public GlobalSearchTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Bank = new MapQueryBank(View, this);
    }

    public void Setup()
    {
        Bank.SetupData();
        UserLoadedData = true;
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (!Bank.MapBankInitialized && !UserLoadedData)
        {
            if (ImGui.Button("Load Map Data", DPI.WholeWidthButton(windowWidth, 24)))
            {
                Setup();
            }
        }

        DisplayInput();

        DisplayResults();
    }

    public void DisplayInput()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (Bank.MapBankInitialized)
        {
            UIHelper.WrappedText("Search through all maps for usage of the specificed property value.");

            // Map Filter
            UIHelper.Spacer();
            UIHelper.SimpleHeader("Map Filter", "Target this specific string when querying the map. Supports regex.\n\n" + $"Multiple filters can be used by using the '|' symbol between each filter, acting as an OR operator.");

            UIHelper.SinglelineTextInput("MapFilter", ref _searchInputMap);

            if (ImGui.BeginPopupContextItem($"MapFilterContextMenu"))
            {
                // Quick Regex buttons
                if (ImGui.Selectable("Exact"))
                {
                    _searchInputMap = $"^{_searchInputMap}$";
                }
                UIHelper.Tooltip("Apply regex that makes the current input match exactly.");

                ImGui.EndPopup();
            }

            // Property Filter
            UIHelper.Spacer();
            UIHelper.SimpleHeader("Property Filter", "Target this specific string when querying the property name. Supports regex.\n\n" + $"Multiple filters can be used by using the '|' symbol between each filter, acting as an OR operator.");

            UIHelper.SinglelineTextInput("PropNameFilter", ref _searchInputProperty);

            // TODO: add arrow button that lets user traverse a tree that displays the MSB structure, allowing them to select properties easily without needing to load a map

            if (ImGui.BeginPopupContextItem($"PropertyFilterContextMenu"))
            {
                // Quick Regex buttons
                if (ImGui.Selectable("Exact"))
                {
                    _searchInputProperty = $"^{_searchInputProperty}$";
                }
                UIHelper.Tooltip("Apply regex that makes the current input match exactly.");
                if (ImGui.Selectable("Index"))
                {
                    _searchInputProperty = @$"{_searchInputProperty}\[0\]";
                }
                UIHelper.Tooltip("Escaped square brackets for targeting specific index in array properties.");

                ImGui.EndPopup();
            }

            // Value Filter
            UIHelper.Spacer();
            UIHelper.SimpleHeader("Value Filter", "Target this specific string when querying the property value. Supports regex.\n\n" + $"Multiple filters can be used by using the '|' symbol between each filter, acting as an OR operator.");

            UIHelper.SinglelineTextInput("PropValueFilter", ref _searchInputValue);

            if (ImGui.BeginPopupContextItem($"ValueFilterContextMenu"))
            {
                // Quick Regex buttons
                if (ImGui.Selectable("Exact"))
                {
                    _searchInputValue = $"^{_searchInputValue}$";
                }
                UIHelper.Tooltip("Apply regex that makes the current input match exactly.");
                if (ImGui.Selectable("Non-Zero Number"))
                {
                    _searchInputValue = "^[1-9]\\d*$";
                }
                UIHelper.Tooltip("Apply regex that makes the current input match non-zero numbers.");

                ImGui.EndPopup();
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Options", "");

            ImGui.Checkbox("Target Project Files", ref _targetProjectFiles);
            UIHelper.Tooltip("Uses the project map files instead of game root.");

            ImGui.Checkbox("Ignore Dummy Map Objects", ref _ignoreDummyParts);
            UIHelper.Tooltip("Excludes DummyObject and DummyEnemy entries from the search.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Actions", "");

            UIHelper.MultiButtonInput("globalSearchActions",
                "search", "Search", "", RunMapSearch,
                "clearSearch", "Clear", "", ClearSearchInputs);
        }
        else if (UserLoadedData)
        {
            UIHelper.WrappedText("Setting up global map search...");
            UIHelper.WrappedText("");
        }
    }

    public void RunMapSearch()
    {
        if(!MayRunQuery)
        {
            Smithbox.LogError<GlobalSearchTool>("Already running a search.");
            return;
        }
        RunQuery();
    }

    public void ClearSearchInputs()
    {
        _searchInputMap = "";
        _searchInputProperty = "";
        _searchInputValue = "";
    }

    public void DisplayResults()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (Bank.MapBankInitialized)
        {
            UIHelper.Spacer();
            UIHelper.SimpleHeader("Search Results", "Result rows are presented as: <entity name>, <name alias>, <matched value>");

            if (QueryComplete)
            {
                UIHelper.MultiButtonInput("resultActions",
                    "copyResults", "Copy Results to Clipboard", "", CopyResultsToClipboard);

                UIHelper.Spacer();
                UIHelper.SimpleHeader("Filters", "");

                ImGui.Checkbox("Header", ref CFG.Current.GlobalMapSearch_CopyResults_IncludeHeader);
                UIHelper.Tooltip("Include the map headers in the clipboard text.");

                ImGui.SameLine();

                ImGui.Checkbox("Index", ref CFG.Current.GlobalMapSearch_CopyResults_IncludeIndex);
                UIHelper.Tooltip("Include the result index in the clipboard text.");

                ImGui.SameLine();

                ImGui.Checkbox("Entity Name", ref CFG.Current.GlobalMapSearch_CopyResults_IncludeEntityName);
                UIHelper.Tooltip("Include the entity name in the clipboard text.");

                ImGui.SameLine();

                ImGui.Checkbox("Entity Alias", ref CFG.Current.GlobalMapSearch_CopyResults_IncludeEntityAlias);
                UIHelper.Tooltip("Include the entity alias in the clipboard text.");

                ImGui.SameLine();

                ImGui.Checkbox("Property Name", ref CFG.Current.GlobalMapSearch_CopyResults_IncludePropertyName);
                UIHelper.Tooltip("Include the property name in the clipboard text.");

                ImGui.SameLine();

                ImGui.Checkbox("Property Value", ref CFG.Current.GlobalMapSearch_CopyResults_IncludePropertyValue);
                UIHelper.Tooltip("Include the property value in the clipboard text.");
            }

            if (QueryComplete)
            {
                DisplayResultList();
            }
            else if (!MayRunQuery)
            {
                UIHelper.WrappedText($"Search query is not yet complete...");
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
                Smithbox.Log(this, $"Failed to add filter pattern due to invalid regex expression: {input}", LogLevel.Warning);
            }
        }

        return patternList;
    }

    public async Task<bool> BuildResults()
    {
        await Task.Yield();

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
            if (_ignoreDummyParts)
            {
                if(IsDummyPart(entry))
                {
                    continue;
                }
            }

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

    private bool IsDummyPart<T>(T obj)
    {
        if(obj.GetType() == typeof(MSB1.Part.DummyObject) ||
           obj.GetType() == typeof(MSB3.Part.DummyObject) ||
           obj.GetType() == typeof(MSBB.Part.DummyObject) ||
           obj.GetType() == typeof(MSBD.Part.DummyObject) ||
           obj.GetType() == typeof(MSBE.Part.DummyAsset) ||
           obj.GetType() == typeof(MSBS.Part.DummyObject) ||
           obj.GetType() == typeof(MSB_AC6.Part.DummyAsset) ||
           obj.GetType() == typeof(MSB_NR.Part.DummyAsset) ||
           obj.GetType() == typeof(MSB1.Part.DummyObject))
        {
            return true;
        }

        if (obj.GetType() == typeof(MSB1.Part.DummyEnemy) ||
           obj.GetType() == typeof(MSB3.Part.DummyEnemy) ||
           obj.GetType() == typeof(MSBB.Part.DummyEnemy) ||
           obj.GetType() == typeof(MSBD.Part.DummyEnemy) ||
           obj.GetType() == typeof(MSBE.Part.DummyEnemy) ||
           obj.GetType() == typeof(MSBS.Part.DummyEnemy) ||
           obj.GetType() == typeof(MSB_AC6.Part.DummyEnemy) ||
           obj.GetType() == typeof(MSB_NR.Part.DummyEnemy) ||
           obj.GetType() == typeof(MSB1.Part.DummyEnemy))
        {
            return true;
        }

        return false;
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

                if (o != null)
                {
                    CheckProperties(o, o.GetType().GetProperties(), mapName, entityName);
                }
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

                if (!Matches.ContainsKey(mapName))
                {
                    Matches.Add(mapName, new List<MapObjectMatch>() { match });
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
            ImGui.BeginChild("##mapQueryResultList", ImGuiChildFlags.Borders);

            for (int i = 0; i < Matches.Count; i++)
            {
                var pair = Matches.ElementAt(i);
                var mapName = pair.Key;
                var mapAlias = AliasHelper.GetMapNameAlias(View.Project, mapName);
                var displayName = $"{mapName}: {mapAlias}";
                var objectMatches = pair.Value;

                if (ImGui.CollapsingHeader($"Matches in {displayName}##header{mapName}{i}"))
                {
                    int index = 0;
                    foreach (var entry in objectMatches)
                    {
                        if (ImGui.Selectable($"{entry.EntityName}##row{i}{index}"))
                        {
                            EditorCommandQueue.AddCommand($"map/load/{entry.MapName}");
                            EditorCommandQueue.AddCommand($"map/select/{entry.MapName}/{entry.EntityName}");
                        }
                        if (ImGui.IsItemVisible())
                        {
                            // Entity Name
                            var alias = GetUnknownAlias(entry.EntityName);
                            UIHelper.DisplayColoredAlias(alias, UI.Current.ImGui_AliasName_Text);

                            // Value
                            UIHelper.DisplayColoredAlias($"- {entry.PropertyName}: {entry.PropertyValue}", UI.Current.ImGui_Benefit_Text_Color);
                        }
                        index++;
                    }
                }
                UIHelper.Tooltip($"Number of matches: {objectMatches.Count}");
            }

            ImGui.EndChild();
        }
    }

    private StringBuilder _outputString = new StringBuilder();

    public void CopyResultsToClipboard()
    {
        if (Matches.Count == 0) return;

        var includeHeader = CFG.Current.GlobalMapSearch_CopyResults_IncludeHeader;
        var includeIndex = CFG.Current.GlobalMapSearch_CopyResults_IncludeIndex;
        var includeEntityName = CFG.Current.GlobalMapSearch_CopyResults_IncludeEntityName;
        var includeEntityAlias = CFG.Current.GlobalMapSearch_CopyResults_IncludeEntityAlias;
        var includePropertyName = CFG.Current.GlobalMapSearch_CopyResults_IncludePropertyName;
        var includePropertyValue = CFG.Current.GlobalMapSearch_CopyResults_IncludePropertyValue;
        var delimiter = CFG.Current.GlobalMapSearch_CopyResults_Delimiter;

        var estimatedSize = Matches.Sum(m => m.Value.Count) * 80;
        _outputString = new StringBuilder(estimatedSize);

        var parts = new List<string>(6);

        foreach (var (mapName, objectMatches) in Matches)
        {
            if (includeHeader)
            {
                var mapAlias = AliasHelper.GetMapNameAlias(View.Project, mapName);
                _outputString.Append($"\n#---------------------------\n{mapName}: {mapAlias}\n#---------------------------\n");
            }

            int index = 0;
            foreach (var entry in objectMatches)
            {
                parts.Clear();

                if (includeIndex) 
                    parts.Add($"{index}");

                if (includeEntityName) 
                    parts.Add(entry.EntityName);

                if (includeEntityAlias)
                {
                    var alias = GetUnknownAlias(entry.EntityName);

                    if (alias != "") 
                        parts.Add(alias);
                }

                if (includePropertyName) 
                    parts.Add(entry.PropertyName);

                if (includePropertyValue) 
                    parts.Add(entry.PropertyValue);

                _outputString.AppendLine(string.Join(delimiter, parts));

                index++;
            }
        }

        PlatformUtils.Instance.SetClipboardText(_outputString.ToString());
    }

    private string GetUnknownAlias(string rawName)
    {
        rawName = rawName.ToLower();
        if (rawName.Contains("_"))
        {
            rawName = rawName.Split("_")[0];
        }

        string alias = "";

        var chrAlias = AliasHelper.GetCharacterAlias(View.Project, rawName);
        var assetAlias = AliasHelper.GetAssetAlias(View.Project, rawName);
        var mapPieceAlias = AliasHelper.GetMapPieceAlias(View.Project, rawName);

        if (chrAlias != "")
            return chrAlias;

        if (assetAlias != "")
            return assetAlias;

        if (mapPieceAlias != "")
            return mapPieceAlias;

        return alias;
    }

    public bool IsMapFilterMatch(string mapName)
    {
        if (MapFilterInputs == "")
            return true;

        foreach (var entry in MapFilterPatterns)
        {
            if (entry.IsMatch(mapName))
            {
                //Smithbox.Log(this, $"Match: {propName}");
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

        for (int i = 0; i < PropertyFilterPatterns.Count; i++)
        {
            var entry = PropertyFilterPatterns[i];

            if (entry.IsMatch(checkedName))
            {
                //Smithbox.Log(this, $"Match: {propName}");
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
                //Smithbox.Log(this, $"Match: {propValue}");
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

        if (arrayIndex != -1)
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

