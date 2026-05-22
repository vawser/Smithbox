using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vortice.Vulkan;
using static HKLib.hk2018.hkaiUserEdgeUtils;

namespace StudioCore.Editors.MapDataEditor;

/// <summary>
/// The 'fields' view: the properties for the currently selected entry
/// </summary>
public class MsbPropertyView
{
    public MapDataEditorView View;
    public ProjectEntry Project;

    private string PropertyListFilter = "";
    private bool ExactPropertyListFilter = false;

    private readonly Dictionary<string, PropertyInfo[]> PropCache = new();

    public MsbPropertyView(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        UIHelper.SimpleHeader("Properties", "");

        DisplayHeader();
        DisplayProperties();
    }

    public void DisplayHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();

        ImGui.BeginChild($"framedList_MsbEntryProperties", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("MsbEntryPropSearch", ref PropertyListFilter, ref ExactPropertyListFilter);

        DisplayHeaderToggles();

        ImGui.EndChild();
    }

    public void DisplayHeaderToggles()
    {
        // Toggle Community Field Names
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Book}", DPI.IconButtonSize))
        {
            CFG.Current.MapEditor_Properties_Enable_Commmunity_Names = !CFG.Current.MapEditor_Properties_Enable_Commmunity_Names;
        }

        var communityFieldNameMode = "Internal";
        if (CFG.Current.MapEditor_Properties_Enable_Commmunity_Names)
            communityFieldNameMode = "Community";

        UIHelper.Tooltip($"Toggle field name display type between Internal and Community.\nCurrent Mode: {communityFieldNameMode}");

        // Toggle Unknown Properties
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Eye}", DPI.IconButtonSize))
        {
            CFG.Current.MapEditor_Properties_Display_Unknown_Properties = !CFG.Current.MapEditor_Properties_Display_Unknown_Properties;
        }

        var unkFieldDisplayMode = "Hidden";

        if (CFG.Current.MapEditor_Properties_Display_Unknown_Properties)
            unkFieldDisplayMode = "Visible";

        UIHelper.Tooltip($"Toggle the display of unknown fields.\nCurrent Mode: {unkFieldDisplayMode}");

        // Toggle Field Padding
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Hubzilla}"))
        {
            CFG.Current.MapEditor_Field_List_Display_Padding = !CFG.Current.MapEditor_Field_List_Display_Padding;
        }

        var fieldPaddingMode = "Hidden";
        if (!CFG.Current.MapEditor_Field_List_Display_Padding)
            fieldPaddingMode = "Visible";

        UIHelper.Tooltip($"Toggle the display of padding field.\nCurrent Mode: {fieldPaddingMode}");
    }

    public void DisplayProperties()
    {
        float listHeight = ImGui.GetContentRegionAvail().Y;
        ImGui.BeginChild("##msbEntryProperties", new Vector2(0, listHeight));

        if(View.Selection.SelectedEntries.Count == 0)
        {
            ImGui.BeginChild("##disabledSection", ImGuiChildFlags.Borders);
            ImGui.TextDisabled("No entry has been selected.");
            ImGui.EndChild();
        }
        else
        {
            DisplayPropertyTable();
        }

        ImGui.EndChild();
    }

    public void DisplayPropertyTable()
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable;

        if (ImGui.BeginTable($"propertyTable", 2, tblFlags))
        {
            ImGui.TableSetupColumn("Property", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

            // Name
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            UIHelper.WrappedText($"Property");

            // Value
            ImGui.TableSetColumnIndex(1);

            UIHelper.WrappedText($"Value");

            HandlePropertyEntries();

            ImGui.EndTable();
        }
    }

    public void HandlePropertyEntries()
    {
        var targetEntry = View.Selection.SelectedEntries.FirstOrDefault();

        if (targetEntry.Value == null)
            return;

        Type type = targetEntry.Value.GetType();
        var properties = GetCachedProperties(type);

        // Direct
        foreach (PropertyInfo prop in properties)
        {
            var isArray = IsPropertyArray(type, targetEntry, prop);
            var isList = IsPropertyList(type, targetEntry, prop);
            var isShape = IsPropertyShape(type, targetEntry, prop);
            var isClass = IsPropertyClass(type, targetEntry, prop);

            if (!isArray && !isList && !isShape && !isClass)
            {
                DisplayPropertyEntry(type, targetEntry.Value, prop);
            }
        }
    }

    public void DisplayPropertyEntry(Type type, object entry, PropertyInfo prop)
    {
        if (CanDisplayPropertyRow(type, entry, prop))
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            HandlePropertyTitle(type, entry, prop);

            ImGui.TableSetColumnIndex(1);
            HandlePropertyValue(type, entry, prop);
        }
    }

    public void HandlePropertyTitle(Type type, object entry, PropertyInfo prop)
    {
        var meta = View.Project.Handler.MapDataHandler.MsbMeta.GetFieldMeta(prop.Name, type);

        // Field Name
        var fieldName = prop.Name;

        if (CFG.Current.MapEditor_Properties_Enable_Commmunity_Names && !meta.IsEmpty)
        {
            fieldName = meta.AltName;
        }

        // Field Description
        var fieldDescription = "";
        if (!meta.IsEmpty)
        {
            fieldDescription = meta.Wiki;
        }

        UIHelper.WrappedText($"{fieldName}");
        UIHelper.Tooltip(fieldDescription);
    }

    public void HandlePropertyValue(Type type, object entry, PropertyInfo prop)
    {
        var curValue = prop.GetValue(entry);
        var propType = prop.PropertyType;

        if (curValue != null)
        {
            UIHelper.WrappedText($"{curValue.ToString()}");
        }
    }

    public bool IsPropertyArray(Type type, object entry, PropertyInfo prop)
    {
        var curValue = prop.GetValue(entry);
        var propType = prop.PropertyType;

        if (propType.IsArray)
        {
            return true;
        }

        return false;
    }

    public bool IsPropertyList(Type type, object entry, PropertyInfo prop)
    {
        var curValue = prop.GetValue(entry);
        var propType = prop.PropertyType;

        if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return true;
        }

        return false;
    }

    public bool IsPropertyShape(Type type, object entry, PropertyInfo prop)
    {
        var curValue = prop.GetValue(entry);
        var propType = prop.PropertyType;

        if (propType.IsClass && propType == typeof(MSB.Shape))
        {
            return true;
        }

        return false;
    }

    public bool IsPropertyClass(Type type, object entry, PropertyInfo prop)
    {
        var curValue = prop.GetValue(entry);
        var propType = prop.PropertyType;

        if (propType.IsClass && propType != typeof(string) && !propType.IsArray)
        {
            return true;
        }

        return false;
    }

    public bool CanDisplayPropertyRow(Type type, object entry, PropertyInfo prop)
    {
        var meta = View.Project.Handler.MapDataHandler.MsbMeta.GetFieldMeta(prop.Name, type);
        var propName = prop.Name;

        // Automatic conditions that hide the property

        if (!prop.CanWrite && !prop.PropertyType.IsArray)
        {
            return false;
        }

        // IMsbEntry.Name needs special handling to keep it unique
        if (typeof(IMsbEntry).IsAssignableFrom(type) && prop.Name == "Name")
            return false;

        // Index Properties are hidden by default
        if (meta != null && meta.IndexProperty)
            return false;

        if (!CFG.Current.MapEditor_Properties_Display_Unknown_Properties)
        {
            // Rough heuristic since all unknown fields start with Unk
            if (propName.StartsWith("unk", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Normal filter
        var isMatch = EditorFilters.IsMatch(PropertyListFilter, propName, ExactPropertyListFilter, meta.AltName);
        var isValueMatch = false;

        if (PropertyListFilter.StartsWith("val:"))
            isValueMatch = true;

        if (!isMatch && !isValueMatch)
        {
            return false;
        }
        else if (isValueMatch)
        {
            // TODO: currently doesn't match correctly with array list values
            var valStr = PropertyListFilter.Replace("val:", "");

            var propVal = prop.GetValue(entry);

            if (propVal != null)
            {
                var value = $"{propVal}";

                if (ExactPropertyListFilter)
                {
                    if (valStr != value)
                        return false;
                }
                else
                {
                    if (!value.Contains(valStr))
                        return false;
                }
            }
        }

        return true;
    }

    public PropertyInfo[] GetCachedFields(object obj)
    {
        return GetCachedProperties(obj.GetType());
    }

    public PropertyInfo[] GetCachedProperties(Type type)
    {
        if (!PropCache.TryGetValue(type.FullName, out PropertyInfo[] props))
        {
            props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            props = props.OrderBy(p => p.MetadataToken).ToArray();
            PropCache.Add(type.FullName, props);
        }

        return props;
    }
}
