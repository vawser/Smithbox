using ImGuiNET;
using SoulsFormats.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.MsbEditor;

public class SearchProperties
{
    private readonly Dictionary<string, List<WeakReference<Entity>>> FoundObjects = new();
    private readonly Universe Universe;
    private readonly PropertyCache _propCache;

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
    private bool ValidType = false;
    private bool _propSearchMatchNameOnly = true;
    private string _propertyNameSearchString = "";

    public SearchProperties(Universe universe, PropertyCache propCache)
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
                return true;
            }

            if (PropertyType == typeof(byte) || PropertyType == typeof(byte[]))
            {
                PropertyValue = byte.TryParse(initialValue, out var val) ? val : default;
                return true;
            }
            
            if (PropertyType == typeof(sbyte) || PropertyType == typeof(sbyte[]))
            {
                PropertyValue = sbyte.TryParse(initialValue, out sbyte val) ? val : default;
                return true;
            }
            
            if (PropertyType == typeof(char) || PropertyType == typeof(char[]))
            {
                PropertyValue = char.TryParse(initialValue, out var val) ? val : default;
                return true;
            }

            if (PropertyType == typeof(short) || PropertyType == typeof(short[]))
            {
                PropertyValue = short.TryParse(initialValue, out var val) ? val : default;
                return true;
            }

            if (PropertyType == typeof(ushort) || PropertyType == typeof(ushort[]))
            {
                PropertyValue = ushort.TryParse(initialValue, out var val) ? val : default;
                return true;
            }

            if (PropertyType == typeof(int) || PropertyType == typeof(int[]))
            {
                PropertyValue = int.TryParse(initialValue, out var val) ? val : default;
                return true;
            }

            if (PropertyType == typeof(uint) || PropertyType == typeof(uint[]))
            {
                PropertyValue = uint.TryParse(initialValue, out var val) ? val : default;
                return true;
            }

            if (PropertyType == typeof(long) || PropertyType == typeof(long[]))
            {
                PropertyValue = long.TryParse(initialValue, out var val) ? val : default;
                return true;
            }

            if (PropertyType == typeof(ulong) || PropertyType == typeof(ulong[]))
            {
                PropertyValue = ulong.TryParse(initialValue, out var val) ? val : default;
                return true;
            }

            if (PropertyType == typeof(float) || PropertyType == typeof(float[]))
            {
                PropertyValue = float.TryParse(initialValue, out var val) ? val : default;
                return true;
            }

            if (PropertyType == typeof(double) || PropertyType == typeof(double[]))
            {
                PropertyValue = double.TryParse(initialValue, out var val) ? val : default;
                return true;
            }

            if (PropertyType == typeof(string) || PropertyType == typeof(string[]))
            {
                PropertyValue = initialValue ?? "";
                return true;
            }
            
            if (PropertyType.IsEnum)
            {
                PropertyValue = PropertyType.GetEnumValues().GetValue(0);
                return true;
            }
        }

        return false;
    }

    public bool SearchValue(bool searchFieldchanged)
    {
        ImGui.Text("Value (Exact)");
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
            if (ImGui.InputInt("##valbyte", ref val) || searchFieldchanged)
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
                end: ;
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

                        if (o is Map m)
                        {
                            foreach (Entity ob in m.Objects)
                            {
                                if (ob is MapEntity e)
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
