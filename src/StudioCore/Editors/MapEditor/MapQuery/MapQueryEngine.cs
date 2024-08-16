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
using System.Threading.Tasks;
using static StudioCore.Editors.MapEditor.MapPropertyEditor;

namespace StudioCore.Editors.MapEditor.MapQuery;

public class MapQueryEngine
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

    public bool DisplayWikiSection = false;
    public bool DisplayMapFilterSection = true;
    public bool DisplayPropertyFilterSection = true;
    public bool DisplayValueFilterSection = true;

    private bool DisplayResultListSection = true;

    public bool QueryComplete = false;
    public bool MayRunQuery = true;

    public string MapFilterInputs = "";
    public string PropertyFilterInputs = "";
    public string ValueFilterInputs = "";

    /// <summary>
    /// Track the property filter index so we can use the correct value filter
    /// when dealing with more than 1 filter argument.
    /// </summary>
    private int CurrentPropertyFilterIndex = -1;

    public MapQueryEngine(MapEditorScreen screen)
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

            ImguiUtils.WrappedText("Map Filter:");
            ImGui.InputText("##mapFilter", ref _searchInputMap, 255);

            ImguiUtils.WrappedText("Property Filter:");
            ImGui.InputText("##propertyNameFilter", ref _searchInputProperty, 255);

            ImguiUtils.WrappedText("Value Filter:");
            ImGui.InputText("##propertyValueFilter", ref _searchInputValue, 255);

            ImguiUtils.WrappedText("");
            ImGui.Checkbox("Target Project Files", ref _targetProjectFiles);
            ImguiUtils.ShowHoverTooltip("Uses the project map files instead of game root.");
            ImGui.Checkbox("Loose Property Match", ref _looseStringMatch);
            ImguiUtils.ShowHoverTooltip("Only require the string property to contain the search string, instead of requiring an exact match.");

            ImguiUtils.WrappedText("");

            if (!MayRunQuery)
            {
                ImGui.BeginDisabled();
                if (ImGui.Button("Search", halfButtonSize))
                {
                }
                ImGui.EndDisabled();
                ImGui.BeginDisabled();
                if (ImGui.Button("Clear", halfButtonSize))
                {
                }
                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button("Search", defaultButtonSize))
                {
                    RunQuery();
                }
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
            ImGui.SameLine();
            if (ImGui.Button($"{ForkAwesome.Bars}##resultListToggle"))
            {
                DisplayResultListSection = !DisplayResultListSection;
            }
            ImGui.Separator();

            if (DisplayResultListSection)
            {
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
    }

    public async void RunQuery()
    {
        if (!Bank.MapBankInitialized)
            return;

        QueryComplete = false;
        MayRunQuery = false;

        Matches = new();

        // Assigned to new vars so we can ignore if the user edits the input vars via the text input
        MapFilterInputs = _searchInputMap.ToLower();
        PropertyFilterInputs = _searchInputProperty.ToLower();
        ValueFilterInputs = _searchInputValue.ToLower();

        // Load the maps async so the main thread isn't blocked
        Task<bool> runQueryTask = BuildResults();

        bool result = await runQueryTask;
        QueryComplete = result;
        MayRunQuery = result;
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
        // Apply property filters here

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

        if (IsPropertyMatch(propName))
        {
            if (IsValueMatch(propValue))
            {
                MapObjectMatch match = new MapObjectMatch(mapName, entityName, propName, $"{propValue}");

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

    public bool IsPropertyMatch(string propName)
    {
        var searchName = _searchInputProperty.ToLower();

        if (_looseStringMatch)
        {
            if (propName.Contains(searchName))
            {
                return true;
            }
        }
        else
        {
            if (searchName == propName.ToLower())
            {
                return true;
            }
        }

        return false;
    }

    public bool IsValueMatch(object propValue)
    {
        // Apply value properties here

        var searchValue = _searchInputValue.ToLower();

        if (searchValue == $"{propValue}".ToLower())
        {
            return true;
        }

        return false;
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
                            var alias = GetUnknownAlias(entry.EntityName);
                            AliasUtils.DisplayAlias(alias);
                        }
                    }
                }
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
        // Blank
        if (MapFilterInputs == "")
            return true;

        var sections = MapFilterInputs.Split("|");

        foreach(var entry in sections)
        {
            // String

            // Regex

            // Group
        }

        return false;
    }

    public void AddMapFilterInput(string mapID)
    {
        var addition = $"{mapID}";

        if (_searchInputMap != "")
        {
            addition = $"+{addition}";
        }

        _searchInputMap = _searchInputMap + addition;
    }

    public void AddPropertyFilterInput(string propertyName)
    {
        var addition = $"{propertyName}";

        if (_searchInputProperty != "")
        {
            addition = $"+{addition}";
        }

        _searchInputProperty = _searchInputProperty + addition;
    }

    public void DisplayWiki()
    {
        ImguiUtils.WrappedText("");

        ImGui.Separator();
        ImguiUtils.WrappedText($"Help:");
        ImGui.SameLine();
        if (ImGui.Button($"{ForkAwesome.Bars}##wikiToggle"))
        {
            DisplayWikiSection = !DisplayWikiSection;
        }
        ImGui.Separator();

        if (DisplayWikiSection)
        {
            // Map Filters
            ImGui.Separator();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, "Map Filters:");
            ImGui.SameLine();
            if (ImGui.Button($"{ForkAwesome.Bars}##mapFilterToggle"))
            {
                DisplayMapFilterSection = !DisplayMapFilterSection;
            }
            ImGui.Separator();
            if (DisplayMapFilterSection)
            {
                ImguiUtils.WrappedText($"Represents the map you want to match.");
                ImguiUtils.WrappedText($"Multiple filters can be used by using the '|' symbol between each filter, acting as an OR operator.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"<blank>");
                ImguiUtils.WrappedText("Targets all maps.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"<string>");
                ImguiUtils.WrappedText("Targets the specified map.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"regex:[<string>]");
                ImguiUtils.WrappedText("Targets the specified maps whose name matches the specified regex.");
                ImguiUtils.WrappedText("");

                // Speical groups for ER
                if (Smithbox.ProjectType is Core.ProjectType.ER)
                {
                    ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"group:[<string>]");
                    ImguiUtils.WrappedText("Targets the specified pre-defined map group.\n\n" +
                        "The possible map groups are:\n" +
                        " legacy - Target maps such as legacy dungeons and other non-tile maps.\n" +
                        " tile - Target all the open world tile maps.\n" +
                        " vanilla_tile - Target all the open world tile maps for the base game.\n" +
                        " dlc_tile - Target all the open world tile maps for the DLC.");
                    ImguiUtils.WrappedText("");
                }
            }

            // Property Filter
            ImGui.Separator();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, "Property Filters:");
            ImGui.SameLine();
            if (ImGui.Button($"{ForkAwesome.Bars}##propertyFilterToggle"))
            {
                DisplayPropertyFilterSection = !DisplayPropertyFilterSection;
            }
            ImGui.Separator();
            if (DisplayPropertyFilterSection)
            {
                ImguiUtils.WrappedText($"Represents the property you want to match.");
                ImguiUtils.WrappedText($"Multiple filters can be used by using the '|' symbol between each filter, acting as an OR operator.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"<blank>");
                ImguiUtils.WrappedText("Targets all properties.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"<string>");
                ImguiUtils.WrappedText("Targets the specified property string.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"regex:[<string>]");
                ImguiUtils.WrappedText("Targets the specified properties whose name matches the specified regex.");
                ImguiUtils.WrappedText("");
            }

            // Value Filter
            ImGui.Separator();
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, "Value Filters:");
            ImGui.SameLine();
            if (ImGui.Button($"{ForkAwesome.Bars}##valueFilterToggle"))
            {
                DisplayValueFilterSection = !DisplayValueFilterSection;
            }
            ImGui.Separator();
            if (DisplayValueFilterSection)
            {
                ImguiUtils.WrappedText($"Represents the property value you want to match.");
                ImguiUtils.WrappedText($"Multiple filters can be used by using the '|' symbol between each filter, acting as an OR operator.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"<blank>");
                ImguiUtils.WrappedText("Targets all values.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"<value>");
                ImguiUtils.WrappedText("Targets the specified property value.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"regex:[<string>]");
                ImguiUtils.WrappedText("Targets the specified property values whose value matches the specified regex.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"list:[<value>,<value>,<etc>]");
                ImguiUtils.WrappedText("Targets the specified property values within the list.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"range:[<min value>,<max value>]");
                ImguiUtils.WrappedText("Targets the specified property values between the minimum and maximum (inclusive).");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"below:[<value>]");
                ImguiUtils.WrappedText("Targets the specified property values below the specified value (inclusive).");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, $"above:[<value>]");
                ImguiUtils.WrappedText("Targets the specified property values above the specified value (inclusive).");
                ImguiUtils.WrappedText("");
            }
        }
    }
}

public class MapObjectMatch
{
    public string MapName = "";
    public string EntityName = "";

    public string PropertyName = "";
    public string PropertyValue = "";

    public MapObjectMatch(string mapname, string entityName, string propName, string propValue)
    {
        MapName = mapname;
        EntityName = entityName;
        PropertyName = propName;
        PropertyValue = propValue;
    }
}
