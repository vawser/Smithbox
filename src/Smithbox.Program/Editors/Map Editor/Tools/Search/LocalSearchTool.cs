using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

public class LocalSearchTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    private readonly Dictionary<string, List<WeakReference<Entity>>> FoundObjects = new();

    private int currentSearchMatchTypeIndex;

    public string[] propSearchCmd = null;
    public bool UpdatePropSearch = false;

    private PropertyInfo _property = null;
    private Type PropertyType = null;
    private dynamic PropertyValue = null;
    private dynamic MinPropertyValue = null;
    private dynamic MaxPropertyValue = null;
    private bool ValidType = false;
    private bool _propSearchMatchNameOnly = true;
    private string _propertyNameSearchString = "";

    public bool FocusLocalPropertySearch = false;

    public bool ExplicitPropertyInput = false;

    public PropertyInfo TargetProp = null;

    public LocalSearchTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        if (FocusLocalPropertySearch)
        {
            FocusLocalPropertySearch = false;
            ImGui.SetNextItemOpen(true);
        }

        Display();
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();

        GUI.WrappedText("Search loaded map for specific property values.");
        GUI.WrappedText("");

        // propcache
        var selection = View.ViewportSelection.GetSingleFilteredSelection<Entity>();
        if (selection == null)
        {
            ImGui.Text("Select entity for dropdown list.");
        }
        else
        {
            GUI.SimpleHeader("Target Property", $"The internal type of the current selection is {selection.WrappedObject.GetType().Name}");

            if (!ExplicitPropertyInput)
            {
                var previewName = "";
                if (TargetProp != null)
                    previewName = TargetProp.Name;

                if (ImGui.BeginCombo("##SearchPropCombo", previewName))
                {
                    var props = View.MapPropertyCache.GetCachedFields(selection.WrappedObject);
                    foreach (var prop in props)
                    {
                        if (ImGui.Selectable(prop.Name))
                        {
                            TargetProp = prop;
                            Property = prop;
                            ValidType = InitializeSearchValue();
                            newSearch = true;
                            _propertyNameSearchString = "";
                            break;
                        }
                    }
                    ImGui.EndCombo();
                }
                GUI.Tooltip("The target property to search across.");
            }
            else if(ExplicitPropertyInput)
            {
                GUI.SinglelineTextInput("PropertyName", ref _propertyNameSearchString, "Property");

                GUI.MultiButtonInput("manualInputAction",
                    "populateProperty", "Populate", "", PopulateForProperty);
            }

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Bars}##propertyInputSwitch", DPI.IconButtonSize))
            {
                ExplicitPropertyInput = !ExplicitPropertyInput;
            }
        }

        GUI.Spacer();
        GUI.SimpleHeader("Options", "");

        if (ImGui.Checkbox("Include properties with same name", ref _propSearchMatchNameOnly))
        {
            newSearch = true;
        }
        GUI.Tooltip("Includes properties that share the same name in the search.");

        GUI.Spacer();
        GUI.SimpleHeader("Target Property", "");

        if (Property != null)
        {
            ImGui.TextWrapped($"{Property.Name}");
        }
        else
        {
            ImGui.TextWrapped($"None");
        }

        GUI.Spacer();
        GUI.SimpleHeader("Target Property Type", "");

        if (PropertyType != null)
        {
            ImGui.TextWrapped($"{PropertyType.Name}");
        }
        else
        {
            ImGui.TextWrapped($"None");
        }
        GUI.Spacer();
        GUI.SimpleHeader("Target Value", "");

        if (Property != null && PropertyType != null)
        {
            SearchValue(newSearch);
        }
        else
        {
            ImGui.TextWrapped($"No property has been specified yet.");
        }

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("localSearchActions",
            "searchProperty", "Search", "", SearchForPropertyValue);


        GUI.Spacer();
        GUI.SimpleHeader("Results", "");

        ImGui.BeginChild("##localSearchResultsList", ImGuiChildFlags.Borders);

        if (FoundObjects.Count > 0)
        {
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
                                View.ViewportSelection.ClearSelection();
                                View.ViewportSelection.AddSelection(obj);
                                selectFirstResult = false;
                            }

                            bool itemFocused = ImGui.IsItemFocused();
                            bool selected = false;
                            if (ImGui.Selectable(obj.Name, View.ViewportSelection.GetSelection().Contains(obj),
                                    ImGuiSelectableFlags.AllowDoubleClick))
                            {
                                selected = true;
                                MsbUtils.EntitySelectionHandler(View, View.ViewportSelection, obj, selected, itemFocused, f.Value);
                                View.FrameAction.ApplyViewportFrame();
                            }

                        }
                    }

                    ImGui.TreePop();
                }
            }
        }
        else
        {

            ImGui.Text($"No map objects found.");
        }

        ImGui.EndChild();
    }

    public void SearchOccured()
    {
        if (PropertyType == null)
            return;


        FoundObjects.Clear();
        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
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
                                if (p != null && (PropertyType == typeof(string[]) ? ((string)p).Contains((string)PropertyValue, StringComparison.OrdinalIgnoreCase) : p.Equals(PropertyValue)))
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
                            if (PropertyType == typeof(string) || PropertyType == typeof(string[]) ? ((string)value).Contains((string)PropertyValue, StringComparison.OrdinalIgnoreCase) : value.Equals(PropertyValue))
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

    public void PopulateForProperty()
    {
        Property = null;
        PropertyType = null;

        bool endProcess = false;

        // Find the first property that matches the given name.
        // Definitely replace this (along with everything else, really).
        HashSet<Type> typeCache = new();

        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (endProcess)
                break;

            if (entry.Value.MapContainer == null)
            {
                continue;
            }

            foreach (Entity o in entry.Value.MapContainer.Objects)
            {
                if (endProcess)
                    break;

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
                    endProcess = true;
                    continue;
                }
                typeCache.Add(o.WrappedObject.GetType());
            }
        }
    }

    public void SearchForPropertyValue()
    {
        SearchOccured();
    }

    public bool newSearch = false;
    public bool selectFirstResult = false;

    public void Update()
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

        return ret;
    }
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
}
