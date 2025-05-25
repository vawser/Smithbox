using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using SoulsFormats.Util;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.PropertyEditor;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MapEditor.Framework;

public class LocalSearchView
{
    private MapEditorScreen Editor;

    private enum SearchMatchType
    {
        Exact = 0,
        Range = 1
    }

    private readonly Dictionary<string, List<WeakReference<Entity>>> FoundObjects = new();
    private readonly MapPropertyCache _propCache;

    private int currentSearchMatchTypeIndex;

    public string[] propSearchCmd = null;
    public bool UpdatePropSearch = false;

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


    public LocalSearchView(MapEditorScreen screen)
    {
        Editor = screen;
        _propCache = screen.MapPropertyCache;
    }

    public void Display()
    {
        UIHelper.WrappedText("Search loaded map for specific property values.");
        UIHelper.WrappedText("");

        // propcache
        var selection = Editor.Universe.Selection.GetSingleFilteredSelection<Entity>();
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

        if (ImGui.InputText("Property Name", ref _propertyNameSearchString, 255))
        {

        }

        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 24 * DPI.GetUIScale());

        if (ImGui.Button("Search", buttonSize))
        {
            Property = null;
            PropertyType = null;

            // Find the first property that matches the given name.
            // Definitely replace this (along with everything else, really).
            HashSet<Type> typeCache = new();
            foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
            {
                if (entry.Value.MapContainer == null)
                {
                    continue;
                }

                foreach (Entity o in entry.Value.MapContainer.Objects)
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
                foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
                {
                    if (entry.Value.MapContainer == null)
                    {
                        continue;
                    }

                    if (entry.Value.MapContainer is MapContainer m)
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

                                if (PropertyType.IsArray && value.GetType().IsArray)
                                {
                                    // Property is an array, scan through each index for value matches.
                                    foreach (var p in (Array)value)
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
                                }
                                else
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
                                Editor.Universe.Selection.ClearSelection(Editor);
                                Editor.Universe.Selection.AddSelection(Editor, obj);
                                selectFirstResult = false;
                            }

                            bool itemFocused = ImGui.IsItemFocused();
                            bool selected = false;
                            if (ImGui.Selectable(obj.Name, Editor.Universe.Selection.GetSelection().Contains(obj),
                                    ImGuiSelectableFlags.AllowDoubleClick))
                            {
                                selected = true;
                            }
                            Utils.EntitySelectionHandler(Editor, Editor.Universe.Selection, obj, selected, itemFocused, f.Value);
                        }
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.EndChild();
        }
        else
        {

            ImGui.Text($"No map objects found.");
        }
    }

    public bool newSearch = false;
    public bool selectFirstResult = false;

    public void OnGui()
    {
        if (propSearchCmd != null && UpdatePropSearch)
        {
            UpdatePropSearch = false;

            ValidType = InitializeSearchValue();
            newSearch = true;
            selectFirstResult = propSearchCmd.Contains("selectFirstResult");
            _propertyNameSearchString = "";
        }
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

    public bool SearchValue(bool searchFieldchanged)
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
}
