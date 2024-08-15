using HKLib.hk2018.TypeRegistryTest;
using ImGuiNET;
using Org.BouncyCastle.Asn1.X509.Qualified;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static HKLib.hk2018.hkaiUserEdgeUtils;
using static SoulsFormats.NVA;
using static StudioCore.Editors.MapEditor.MapPropertyEditor;

namespace StudioCore.Editors.MapEditor.MapQuery;

public class MapQueryEngine
{
    private MapEditorScreen Screen;
    private MapQueryBank Bank;

    public string _searchInputMap = "";
    public string _searchInputProperty = "";
    public string _searchInputValue = "";
    public Dictionary<string, List<MapObjectMatch>> Matches = new();

    public bool _targetProjectFiles = false;
    public bool _looseStringMatch = false;

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

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        if (Bank.MapBankInitialized)
        {
            ImguiUtils.WrappedText("Search through all maps for usage of the specificed property value.");
            ImguiUtils.WrappedText("");

            ImguiUtils.WrappedText("Map Filter:");
            ImGui.InputText("##mapFilter", ref _searchInputMap, 255);
            ImguiUtils.ShowHoverTooltip(MapQueryTooltips.MAP_FILTER_TOOLTIP);

            ImguiUtils.WrappedText("Property Name Filter:");
            ImGui.InputText("##propertyNameFilter", ref _searchInputProperty, 255);
            ImguiUtils.ShowHoverTooltip(MapQueryTooltips.PROPERTY_NAME_FILTER_TOOLTIP);

            ImguiUtils.WrappedText("Property Value Filter:");
            ImGui.InputText("##propertyValueFilter", ref _searchInputValue, 255);
            ImguiUtils.ShowHoverTooltip(MapQueryTooltips.PROPERTY_VALUE_FILTER_TOOLTIP);

            ImguiUtils.WrappedText("");
            ImGui.Checkbox("Target Project Files", ref _targetProjectFiles);
            ImguiUtils.ShowHoverTooltip("Uses the project map files instead of game root.");
            ImGui.Checkbox("Loose String Match", ref _looseStringMatch);
            ImguiUtils.ShowHoverTooltip("Only require the string property to contain the search string, instead of requiring an exact match.");

            ImguiUtils.WrappedText("");

            if (!MayRunQuery)
            {
                ImGui.BeginDisabled();
                if (ImGui.Button("Search", defaultButtonSize))
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
            }

            ImguiUtils.WrappedText("");

            ImGui.Separator();
            ImguiUtils.WrappedText($"Search Results:");
            ImGui.Separator();

            if (QueryComplete)
            {
                DisplayResults();
            }
            else if(!MayRunQuery)
            {
                ImguiUtils.WrappedText($"Search query is not yet complete...");
            }
        }
        else
        {
            ImguiUtils.WrappedText("Map Query Engine is loading...");
            ImguiUtils.WrappedText("");
        }
    }

    public bool QueryComplete = false;
    public bool MayRunQuery = true;

    public string mapInput = "";
    public string propertyInput = "";
    public string valueInput = "";

    public async void RunQuery()
    {
        if (!Bank.MapBankInitialized)
            return;

        QueryComplete = false;
        MayRunQuery = false;

        Matches = new();

        // Assigned to new vars so we can ignore if the user edits the input vars via the text input
        mapInput = _searchInputMap.ToLower();
        propertyInput = _searchInputProperty.ToLower();
        valueInput = _searchInputValue.ToLower();

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

            CheckPropertyList(mapName, map, map.Parts.GetEntries());
            CheckPropertyList(mapName, map, map.Regions.GetEntries());
            CheckPropertyList(mapName, map, map.Events.GetEntries());
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
        var searchValue = _searchInputValue.ToLower();

        if (searchValue == $"{propValue}".ToLower())
        {
            return true;
        }

        return false;
    }

    public void DisplayResults()
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
                    }
                }
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
