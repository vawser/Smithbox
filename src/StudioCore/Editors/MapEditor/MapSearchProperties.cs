using ImGuiNET;
using SoulsFormats;
using SoulsFormats.Util;
using StudioCore.Editor;
using StudioCore.MsbEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

public class MapSearchProperties
{
    // TODO: make SearchMatchType.Range work correctly: the Imgui input is being reset.

    private enum SearchMatchType
    {
        Exact = 0,
        Range = 1
    }

    private readonly Dictionary<string, List<WeakReference<Entity>>> FoundObjects = new();
    private readonly Universe Universe;
    private readonly MapPropertyCache _propCache;

    private string[] SearchMatchTypeStrings = new string[]{ "Exact", "Range" };
    private SearchMatchType currentSearchMatchType = SearchMatchType.Exact;
    private int currentSearchMatchTypeIndex;

    public PropertyInfo Property
    {
        get => _property;
        set
        {
            if (value != null)
            {
                _property = value;
                PropertyType = value.PropertyType;
            }
            else
            {
                PropertyType = null;
                ValidType = false;
            }
        }
    }

    private PropertyInfo _property = null;
    private Type PropertyType = null;
    private dynamic PropertyValue = null;
    private dynamic MinPropertyValue = null;
    private dynamic MaxPropertyValue = null;
    private bool ValidType = false;
    private bool _propSearchMatchNameOnly = true;
    private string _propertyNameSearchString = "";


    public MapSearchProperties(Universe universe, MapPropertyCache propCache)
    {
        Universe = universe;
        _propCache = propCache;
    }

    public bool InitializeSearchValue(string initialValue = null)
    {
        if (PropertyType != null)
        {
            if (PropertyType == typeof(bool) || PropertyType == typeof(bool[]))
            {
                PropertyValue = bool.TryParse(initialValue, out var val) ? val : default;
                MinPropertyValue = bool.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = bool.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(byte) || PropertyType == typeof(byte[]))
            {
                PropertyValue = byte.TryParse(initialValue, out var val) ? val : default;
                MinPropertyValue = byte.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = byte.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(sbyte) || PropertyType == typeof(sbyte[]))
            {
                PropertyValue = sbyte.TryParse(initialValue, out sbyte val) ? val : default;
                MinPropertyValue = sbyte.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = sbyte.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(char) || PropertyType == typeof(char[]))
            {
                PropertyValue = char.TryParse(initialValue, out var val) ? val : default;
                MinPropertyValue = char.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = char.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(short) || PropertyType == typeof(short[]))
            {
                PropertyValue = short.TryParse(initialValue, out var val) ? val : default;
                MinPropertyValue = short.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = short.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(ushort) || PropertyType == typeof(ushort[]))
            {
                PropertyValue = ushort.TryParse(initialValue, out var val) ? val : default;
                MinPropertyValue = ushort.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = ushort.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(int) || PropertyType == typeof(int[]))
            {
                PropertyValue = int.TryParse(initialValue, out var val) ? val : default;
                MinPropertyValue = int.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = int.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(uint) || PropertyType == typeof(uint[]))
            {
                PropertyValue = uint.TryParse(initialValue, out var val) ? val : default;
                MinPropertyValue = uint.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = uint.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(long) || PropertyType == typeof(long[]))
            {
                PropertyValue = long.TryParse(initialValue, out var val) ? val : default;
                MinPropertyValue = long.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = long.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(ulong) || PropertyType == typeof(ulong[]))
            {
                PropertyValue = ulong.TryParse(initialValue, out var val) ? val : default;
                MinPropertyValue = ulong.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = ulong.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(float) || PropertyType == typeof(float[]))
            {
                PropertyValue = float.TryParse(initialValue, out var val) ? val : default;
                MinPropertyValue = float.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = float.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(double) || PropertyType == typeof(double[]))
            {
                PropertyValue = double.TryParse(initialValue, out var val) ? val : default;
                MinPropertyValue = double.TryParse(initialValue, out var minVal) ? minVal : default;
                MaxPropertyValue = double.TryParse(initialValue, out var maxVal) ? maxVal : default;
                return true;
            }

            if (PropertyType == typeof(string) || PropertyType == typeof(string[]))
            {
                PropertyValue = initialValue ?? "";
                MinPropertyValue = initialValue ?? "";
                MaxPropertyValue = initialValue ?? "";
                return true;
            }

            if (PropertyType.IsEnum)
            {
                PropertyValue = PropertyType.GetEnumValues().GetValue(0);
                MinPropertyValue = PropertyType.GetEnumValues().GetValue(0);
                MaxPropertyValue = PropertyType.GetEnumValues().GetValue(0);
                return true;
            }
        }

        return false;
    }

    public bool CheckValueRange(object value)
    {
        string val = value.ToString();

        if (PropertyType != null)
        {
            if (PropertyType == typeof(bool) || PropertyType == typeof(bool[]))
            {
                MinPropertyValue = bool.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = bool.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
            }

            if (PropertyType == typeof(byte) || PropertyType == typeof(byte[]))
            {
                MinPropertyValue = byte.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = byte.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
                return true;
            }

            if (PropertyType == typeof(sbyte) || PropertyType == typeof(sbyte[]))
            {
                MinPropertyValue = sbyte.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = sbyte.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
                return true;
            }

            if (PropertyType == typeof(char) || PropertyType == typeof(char[]))
            {
                MinPropertyValue = char.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = char.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
                return true;
            }

            if (PropertyType == typeof(short) || PropertyType == typeof(short[]))
            {
                MinPropertyValue = short.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = short.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
                return true;
            }

            if (PropertyType == typeof(ushort) || PropertyType == typeof(ushort[]))
            {
                MinPropertyValue = ushort.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = ushort.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
                return true;
            }

            if (PropertyType == typeof(int) || PropertyType == typeof(int[]))
            {
                MinPropertyValue = int.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = int.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
                return true;
            }

            if (PropertyType == typeof(uint) || PropertyType == typeof(uint[]))
            {
                MinPropertyValue = uint.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = uint.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
                return true;
            }

            if (PropertyType == typeof(long) || PropertyType == typeof(long[]))
            {
                MinPropertyValue = long.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = long.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
                return true;
            }

            if (PropertyType == typeof(ulong) || PropertyType == typeof(ulong[]))
            {
                MinPropertyValue = ulong.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = ulong.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
                return true;
            }

            if (PropertyType == typeof(float) || PropertyType == typeof(float[]))
            {
                MinPropertyValue = float.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = float.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
                return true;
            }

            if (PropertyType == typeof(double) || PropertyType == typeof(double[]))
            {
                MinPropertyValue = double.TryParse(val, out var minVal) ? minVal : MinPropertyValue;
                MaxPropertyValue = double.TryParse(val, out var maxVal) ? maxVal : MaxPropertyValue;
                return true;
            }

            if (PropertyType == typeof(string) || PropertyType == typeof(string[]))
            {
                return false;
            }

            if (PropertyType.IsEnum)
            {
                return false;
            }

            if (val >= MinPropertyValue && val <= MaxPropertyValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public bool SearchValue(bool searchFieldchanged)
    {
        if (currentSearchMatchType == SearchMatchType.Exact)
        {
            ImGui.Text("Value");
            ImGui.NextColumn();
            var ret = false;
            if (PropertyType == typeof(bool) || PropertyType == typeof(bool[]))
            {
                var val = (bool)PropertyValue;
                if (ImGui.Checkbox("##valBool", ref val) || searchFieldchanged)
                {
                    PropertyValue = val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(byte) || PropertyType == typeof(byte[]))
            {
                var val = (int)PropertyValue;
                if (ImGui.InputInt("##valbyte", ref val) || searchFieldchanged)
                {
                    PropertyValue = (byte)val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(sbyte) || PropertyType == typeof(sbyte[]))
            {
                int val = (int)PropertyValue;
                if (ImGui.InputInt("##valsbyte", ref val) || searchFieldchanged == true)
                {
                    PropertyValue = (sbyte)val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(char) || PropertyType == typeof(char[]))
            {
                var val = (int)PropertyValue;
                if (ImGui.InputInt("##valchar", ref val) || searchFieldchanged)
                {
                    PropertyValue = (char)val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(short) || PropertyType == typeof(short[]))
            {
                var val = (int)PropertyValue;
                if (ImGui.InputInt("##valshort", ref val) || searchFieldchanged)
                {
                    PropertyValue = (short)val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(ushort) || PropertyType == typeof(ushort[]))
            {
                var val = (int)PropertyValue;
                if (ImGui.InputInt("##valushort", ref val) || searchFieldchanged)
                {
                    PropertyValue = (ushort)val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(int) || PropertyType == typeof(int[]))
            {
                var val = (int)PropertyValue;
                if (ImGui.InputInt("##valint", ref val) || searchFieldchanged)
                {
                    PropertyValue = val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(uint) || PropertyType == typeof(uint[]))
            {
                var val = (int)PropertyValue;
                if (ImGui.InputInt("##valuint", ref val) || searchFieldchanged)
                {
                    PropertyValue = (uint)val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(long) || PropertyType == typeof(long[]))
            {
                var val = (int)PropertyValue;
                if (ImGui.InputInt("##vallong", ref val) || searchFieldchanged)
                {
                    PropertyValue = (long)val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(ulong) || PropertyType == typeof(ulong[]))
            {
                var val = (int)PropertyValue;
                if (ImGui.InputInt("##valulong", ref val) || searchFieldchanged)
                {
                    PropertyValue = (ulong)val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(float) || PropertyType == typeof(float[]))
            {
                var val = (float)PropertyValue;
                if (ImGui.InputFloat("##valFloat", ref val) || searchFieldchanged)
                {
                    PropertyValue = val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(double) || PropertyType == typeof(double[]))
            {
                var val = (double)PropertyValue;
                if (ImGui.InputDouble("##valDouble", ref val) || searchFieldchanged)
                {
                    PropertyValue = val;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(string) || PropertyType == typeof(string[]))
            {
                string val = PropertyValue;
                if (ImGui.InputText("##valString", ref val, 99) || searchFieldchanged)
                {
                    PropertyValue = val;
                    ret = true;
                }
            }
            else if (PropertyType.IsEnum)
            {
                var enumVals = PropertyType.GetEnumValues();
                var enumNames = PropertyType.GetEnumNames();
                int[] intVals = new int[enumVals.Length];

                if (searchFieldchanged == true)
                {
                    ret = true;
                }

                if (PropertyType.GetEnumUnderlyingType() == typeof(byte))
                {
                    for (var i = 0; i < enumVals.Length; i++)
                        intVals[i] = (byte)enumVals.GetValue(i);

                    if (Utils.EnumEditor(enumVals, enumNames, PropertyValue, out object val, intVals))
                    {
                        PropertyValue = val;
                        ret = true;
                    }
                }
                else if (PropertyType.GetEnumUnderlyingType() == typeof(int))
                {
                    for (var i = 0; i < enumVals.Length; i++)
                        intVals[i] = (int)enumVals.GetValue(i);

                    if (Utils.EnumEditor(enumVals, enumNames, PropertyValue, out object val, intVals))
                    {
                        PropertyValue = val;
                        ret = true;
                    }
                }
                else if (PropertyType.GetEnumUnderlyingType() == typeof(uint))
                {
                    for (var i = 0; i < enumVals.Length; i++)
                        intVals[i] = (int)(uint)enumVals.GetValue(i);

                    if (Utils.EnumEditor(enumVals, enumNames, PropertyValue, out object val, intVals))
                    {
                        PropertyValue = val;
                        ret = true;
                    }
                }
                else
                {
                    ImGui.Text("Enum underlying type not implemented");
                }
            }
            else
            {
                ImGui.Text("Value type not implemented");
            }

            ImGui.NextColumn();

            return ret;
        }

        if (currentSearchMatchType == SearchMatchType.Range)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Minimum Value");
            ImGui.Text("Maximum Value");

            ImGui.NextColumn();
            var ret = false;

            if (PropertyType == typeof(bool) || PropertyType == typeof(bool[]))
            {
                var minVal = (bool)MinPropertyValue;
                if (ImGui.Checkbox("##valBoolmin", ref minVal) || searchFieldchanged)
                {
                    MinPropertyValue = minVal;
                    ret = true;
                }

                var maxVal = (bool)MaxPropertyValue;
                if (ImGui.Checkbox("##valBoolmax", ref maxVal) || searchFieldchanged)
                {
                    MaxPropertyValue = maxVal;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(byte) || PropertyType == typeof(byte[]))
            {
                var minVal = (int)MinPropertyValue;
                if (ImGui.InputInt("##valbytemin", ref minVal) || searchFieldchanged)
                {
                    MinPropertyValue = (byte)minVal;
                    ret = true;
                }

                var maxVal = (int)MaxPropertyValue;
                if (ImGui.InputInt("##valbytemax", ref maxVal) || searchFieldchanged)
                {
                    MaxPropertyValue = (byte)maxVal;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(sbyte) || PropertyType == typeof(sbyte[]))
            {
                int minVal = (int)MinPropertyValue;
                if (ImGui.InputInt("##valsbytemin", ref minVal) || searchFieldchanged == true)
                {
                    MinPropertyValue = (sbyte)minVal;
                    ret = true;
                }

                int maxVal = (int)MaxPropertyValue;
                if (ImGui.InputInt("##valsbytemax", ref maxVal) || searchFieldchanged == true)
                {
                    MaxPropertyValue = (sbyte)maxVal;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(char) || PropertyType == typeof(char[]))
            {
                var minVal = (int)MinPropertyValue;
                if (ImGui.InputInt("##valcharmin", ref minVal) || searchFieldchanged)
                {
                    MinPropertyValue = (char)minVal;
                    ret = true;
                }

                var maxVal = (int)MaxPropertyValue;
                if (ImGui.InputInt("##valcharmax", ref maxVal) || searchFieldchanged)
                {
                    MaxPropertyValue = (char)maxVal;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(short) || PropertyType == typeof(short[]))
            {
                var minVal = (int)MinPropertyValue;
                if (ImGui.InputInt("##valshortmin", ref minVal) || searchFieldchanged)
                {
                    MinPropertyValue = (short)minVal;
                    ret = true;
                }

                var maxVal = (int)MaxPropertyValue;
                if (ImGui.InputInt("##valshortmax", ref maxVal) || searchFieldchanged)
                {
                    MaxPropertyValue = (short)maxVal;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(ushort) || PropertyType == typeof(ushort[]))
            {
                var minVal = (int)MinPropertyValue;
                if (ImGui.InputInt("##valushortmin", ref minVal) || searchFieldchanged)
                {
                    MinPropertyValue = (ushort)minVal;
                    ret = true;
                }

                var maxVal = (int)MaxPropertyValue;
                if (ImGui.InputInt("##valushortmax", ref maxVal) || searchFieldchanged)
                {
                    MaxPropertyValue = (ushort)maxVal;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(int) || PropertyType == typeof(int[]))
            {
                var minVal = (int)MinPropertyValue;
                if (ImGui.InputInt("##valintmin", ref minVal) || searchFieldchanged)
                {
                    MinPropertyValue = minVal;
                    ret = true;
                }

                var maxVal = (int)MaxPropertyValue;
                if (ImGui.InputInt("##valintmax", ref maxVal) || searchFieldchanged)
                {
                    MaxPropertyValue = maxVal;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(uint) || PropertyType == typeof(uint[]))
            {
                var minVal = (int)MinPropertyValue;
                if (ImGui.InputInt("##valuintmin", ref minVal) || searchFieldchanged)
                {
                    MinPropertyValue = (uint)minVal;
                    ret = true;
                }

                var maxVal = (int)MaxPropertyValue;
                if (ImGui.InputInt("##valuintmax", ref maxVal) || searchFieldchanged)
                {
                    MaxPropertyValue = (uint)maxVal;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(long) || PropertyType == typeof(long[]))
            {
                var minVal = (int)MinPropertyValue;
                if (ImGui.InputInt("##vallongmin", ref minVal) || searchFieldchanged)
                {
                    MinPropertyValue = (long)minVal;
                    ret = true;
                }

                var maxVal = (int)MaxPropertyValue;
                if (ImGui.InputInt("##vallongmax", ref maxVal) || searchFieldchanged)
                {
                    MaxPropertyValue = (long)maxVal;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(ulong) || PropertyType == typeof(ulong[]))
            {
                var minVal = (int)MinPropertyValue;
                if (ImGui.InputInt("##valulongmin", ref minVal) || searchFieldchanged)
                {
                    MinPropertyValue = (ulong)minVal;
                    ret = true;
                }

                var maxVal = (int)MaxPropertyValue;
                if (ImGui.InputInt("##valulong", ref maxVal) || searchFieldchanged)
                {
                    MaxPropertyValue = (ulong)maxVal;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(float) || PropertyType == typeof(float[]))
            {
                var minVal = (float)MinPropertyValue;
                if (ImGui.InputFloat("##valFloatmin", ref minVal) || searchFieldchanged)
                {
                    MinPropertyValue = minVal;
                    ret = true;
                }

                var maxVal = (float)MaxPropertyValue;
                if (ImGui.InputFloat("##valFloatmax", ref maxVal) || searchFieldchanged)
                {
                    MaxPropertyValue = maxVal;
                    ret = true;
                }
            }
            else if (PropertyType == typeof(double) || PropertyType == typeof(double[]))
            {
                var minVal = (double)MinPropertyValue;
                if (ImGui.InputDouble("##valDoublemin", ref minVal) || searchFieldchanged)
                {
                    MinPropertyValue = minVal;
                    ret = true;
                }

                var maxVal = (double)MaxPropertyValue;
                if (ImGui.InputDouble("##valDouble", ref maxVal) || searchFieldchanged)
                {
                    MaxPropertyValue = maxVal;
                    ret = true;
                }
            }
            else
            {
                ImGui.Text("Value type not implemented");
            }

            ImGui.NextColumn();

            return ret;
        }

        return false;
    }

    public void OnGui(string[] propSearchCmd = null)
    {
        bool newSearch = false;
        var selectFirstResult = false;
        if (propSearchCmd != null)
        {
            ImGui.SetNextWindowFocus();
            ValidType = InitializeSearchValue();
            newSearch = true;
            selectFirstResult = propSearchCmd.Contains("selectFirstResult");
            _propertyNameSearchString = "";
        }

        if (!CFG.Current.Interface_MapEditor_PropertySearch)
            return;

        if (ImGui.Begin("Search Properties"))
        {
            // propcache
            var selection = Universe.Selection.GetSingleFilteredSelection<Entity>();
            if (selection == null)
            {
                ImGui.Text("Select entity for dropdown list.");
            }
            else
            {
                ImGui.Spacing();
                ImGui.Text($"Selected type: {selection.WrappedObject.GetType().Name}");

                if (ImGui.BeginCombo("##SearchPropCombo", "Select property..."))
                {
                    var props = _propCache.GetCachedFields(selection.WrappedObject);
                    foreach (var prop in props)
                    {
                        if (ImGui.Selectable(prop.Name))
                        {
                            Property = prop;
                            ValidType = InitializeSearchValue();
                            newSearch = true;
                            _propertyNameSearchString = "";
                            break;
                        }
                    }
                    ImGui.EndCombo();
                }
            }

            if (ImGui.Button("Help##PropSearchHelpMenu"))
            {
                ImGui.OpenPopup("##PropSearchHelpPopup");
            }
            if (ImGui.BeginPopup("##PropSearchHelpPopup"))
            {
                ImGui.Text($"To search through properties, you can:\nA. Type property name below.\nB. Select an entity, then right click a field in Property Editor or use dropdown menu below.");
                ImGui.EndPopup();
            }

            ImGui.SameLine();
            if (ImGui.Checkbox("Include properties with same name", ref _propSearchMatchNameOnly))
            {
                newSearch = true;
            }

            /*
            if (ImGui.Combo("Value Search Type##searchType", ref currentSearchMatchTypeIndex, SearchMatchTypeStrings, SearchMatchTypeStrings.Length))
            {
                switch ((SearchMatchType)currentSearchMatchTypeIndex)
                {
                    case SearchMatchType.Exact:
                        currentSearchMatchType = SearchMatchType.Exact;
                        break;
                    case SearchMatchType.Range:
                        currentSearchMatchType = SearchMatchType.Range;
                        break;
                    default:
                        throw new Exception("Invalid SearchMatchType");
                }
            }
            */

            if (ImGui.InputText("Property Name", ref _propertyNameSearchString, 255))
            {
                Property = null;
                PropertyType = null;

                // Find the first property that matches the given name.
                // Definitely replace this (along with everything else, really).
                HashSet<Type> typeCache = new();
                foreach (KeyValuePair<string, ObjectContainer> m in Universe.LoadedObjectContainers)
                {
                    if (m.Value == null)
                    {
                        continue;
                    }

                    foreach (Entity o in m.Value.Objects)
                    {
                        Type typ = o.WrappedObject.GetType();
                        if (typeCache.Contains(typ))
                            continue;
                        var prop = PropFinderUtil.FindProperty(_propertyNameSearchString, o.WrappedObject);
                        if (prop != null)
                        {
                            Property = prop;
                            ValidType = InitializeSearchValue();
                            _propSearchMatchNameOnly = true;
                            newSearch = true;
                            goto end;
                        }
                        typeCache.Add(o.WrappedObject.GetType());
                    }
                }
            end:;
            }

            ImGui.Separator();
            ImGui.Columns(2);

            if (Property != null && ValidType)
            {
                ImGui.Text("Property Name");
                ImGui.NextColumn();
                ImGui.Text(Property.Name);
                ImGui.NextColumn();

                ImGui.Text("Type");
                ImGui.NextColumn();
                ImGui.Text(PropertyType.Name);
                ImGui.NextColumn();

                if (SearchValue(newSearch))
                {
                    FoundObjects.Clear();
                    foreach (ObjectContainer o in Universe.LoadedObjectContainers.Values)
                    {
                        if (o == null)
                        {
                            continue;
                        }

                        if (o is MapContainer m)
                        {
                            foreach (Entity ob in m.Objects)
                            {
                                if (ob is MsbEntity e)
                                {
                                    var value = PropFinderUtil.FindPropertyValue(Property, ob.WrappedObject, _propSearchMatchNameOnly);

                                    if (value == null)
                                    {
                                        // Object does not contain target property.
                                        continue;
                                    }

                                    if (PropertyType.IsArray)
                                    {
                                        // Property is an array, scan through each index for value matches.
                                        foreach (var p in (Array)value)
                                        {
                                            if (currentSearchMatchType == SearchMatchType.Exact)
                                            {
                                                if (p != null && p.Equals(PropertyValue))
                                                {
                                                    if (!FoundObjects.ContainsKey(e.ContainingMap.Name))
                                                    {
                                                        FoundObjects.Add(e.ContainingMap.Name, new List<WeakReference<Entity>>());
                                                    }
                                                    FoundObjects[e.ContainingMap.Name].Add(new WeakReference<Entity>(e));
                                                    break;
                                                }
                                            }
                                            if (currentSearchMatchType == SearchMatchType.Range)
                                            {
                                                if (p != null && CheckValueRange(value))
                                                {
                                                    if (!FoundObjects.ContainsKey(e.ContainingMap.Name))
                                                    {
                                                        FoundObjects.Add(e.ContainingMap.Name, new List<WeakReference<Entity>>());
                                                    }
                                                    FoundObjects[e.ContainingMap.Name].Add(new WeakReference<Entity>(e));
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (currentSearchMatchType == SearchMatchType.Exact)
                                        {
                                            if (value.Equals(PropertyValue))
                                            {
                                                if (!FoundObjects.ContainsKey(e.ContainingMap.Name))
                                                {
                                                    FoundObjects.Add(e.ContainingMap.Name,
                                                        new List<WeakReference<Entity>>());
                                                }

                                                FoundObjects[e.ContainingMap.Name].Add(new WeakReference<Entity>(e));
                                            }
                                        }
                                        if (currentSearchMatchType == SearchMatchType.Range)
                                        {
                                            if (CheckValueRange(value))
                                            {
                                                if (!FoundObjects.ContainsKey(e.ContainingMap.Name))
                                                {
                                                    FoundObjects.Add(e.ContainingMap.Name,
                                                        new List<WeakReference<Entity>>());
                                                }

                                                FoundObjects[e.ContainingMap.Name].Add(new WeakReference<Entity>(e));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            ImGui.Columns(1);
            if (FoundObjects.Count > 0)
            {
                ImGui.Text("Search Results");
                ImGui.Separator();
                ImGui.BeginChild("Search Results");
                foreach (KeyValuePair<string, List<WeakReference<Entity>>> f in FoundObjects)
                {
                    if (ImGui.TreeNodeEx(f.Key, ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        foreach (WeakReference<Entity> o in f.Value)
                        {
                            Entity obj;
                            if (o.TryGetTarget(out obj))
                            {
                                if (selectFirstResult)
                                {
                                    // TODO: We may also want to frame this result when requested via selectFirstResult.
                                    Universe.Selection.ClearSelection();
                                    Universe.Selection.AddSelection(obj);
                                    selectFirstResult = false;
                                }

                                bool itemFocused = ImGui.IsItemFocused();
                                bool selected = false;
                                if (ImGui.Selectable(obj.Name, Universe.Selection.GetSelection().Contains(obj),
                                        ImGuiSelectableFlags.AllowDoubleClick))
                                {
                                    selected = true;
                                }
                                Utils.EntitySelectionHandler(Universe.Selection, obj, selected, itemFocused, f.Value);
                            }
                        }

                        ImGui.TreePop();
                    }
                }

                ImGui.EndChild();
            }
        }

        ImGui.End();
    }

    
}
