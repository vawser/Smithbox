using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections;
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
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuter;

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

        DisplayObjectProperties(type, targetEntry.Value, prefix: "", postfix: "");
    }

    private void DisplayObjectProperties(Type type, object entry, string prefix, string postfix)
    {
        var properties = ReflectionHelper.GetCachedProperties(type);

        foreach (PropertyInfo prop in properties)
        {
            // Arrays and lists must be checked before IsPropertyClass because List<T> is also a class.
            if (ReflectionHelper.IsPropertyArray(type, entry, prop))
            {
                DisplayArrayPropertyEntries(type, entry, prop, prefix, postfix);
            }
            else if (ReflectionHelper.IsPropertyList(type, entry, prop))
            {
                DisplayListPropertyEntries(type, entry, prop, prefix, postfix);
            }
            else if (ReflectionHelper.IsPropertyClass(type, entry, prop))
            {
                // Sub-classes (including Shape subclasses): recurse with a prefixed label.
                var subValue = prop.GetValue(entry);
                if (subValue != null)
                {
                    var subType = subValue.GetType();
                    DisplayObjectProperties(subType, subValue, prefix, postfix);
                }
            }
            else
            {
                DisplayPropertyEntry(type, entry, prop, prefix, postfix);
            }
        }
    }

    private void DisplayArrayPropertyEntries(Type type, object entry, PropertyInfo prop, string prefix, string postfix)
    {
        var array = prop.GetValue(entry) as Array;
        if (array == null)
            return;

        for (int i = 0; i < array.Length; i++)
        {
            var element = array.GetValue(i);
            if (element == null)
                continue;

            var elementType = element.GetType();

            if (ReflectionHelper.IsScalarType(elementType))
            {
                DisplayPropertyEntry(elementType, element, prop, prefix, postfix: $"[{i}]", true);
            }
            else
            {
                // Recurse into the element's own properties.
                DisplayObjectProperties(elementType, element, prefix, postfix: $"[{i}]");
            }
        }
    }

    private void DisplayListPropertyEntries(Type type, object entry, PropertyInfo prop, string prefix, string postfix)
    {
        var list = prop.GetValue(entry) as IList;
        if (list == null)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            var element = list[i];
            if (element == null)
                continue;

            var elementType = element.GetType();

            if (ReflectionHelper.IsScalarType(elementType))
            {
                DisplayPropertyEntry(elementType, element, prop, prefix, postfix: $"[{i}]", true);
            }
            else
            {
                DisplayObjectProperties(elementType, element, prefix, postfix: $"[{i}]");
            }
        }
    }

    public void DisplayPropertyEntry(Type type, object entry, PropertyInfo prop, string prefix, string postfix, bool isScalar = false)
    {
        if (CanDisplayPropertyRow(type, entry, prop))
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            HandlePropertyTitle(type, entry, prop, prefix, postfix, isScalar);

            ImGui.TableSetColumnIndex(1);
            HandlePropertyValue(type, entry, prop, prefix, postfix, isScalar);
        }

        // Metadata row
        //if(true)
        //{
        //    ImGui.TableNextRow();

        //    ImGui.TableSetColumnIndex(0);
        //    UIHelper.WrappedText("ENUM");

        //    ImGui.TableSetColumnIndex(1);
        //    UIHelper.WrappedText("TEST");
        //}
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

    public void HandlePropertyTitle(Type type, object entry, PropertyInfo prop, string prefix, string postfix, bool isScalar = false)
    {
        var meta = View.Project.Handler.MapDataHandler.MsbMeta.GetFieldMeta(prop.Name, type);

        // Field Name — community name replaces only the leaf name; the prefix is always structural.
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

        UIHelper.WrappedText($"{prefix}{fieldName}{postfix}");
        UIHelper.Tooltip(fieldDescription);
    }

    public void HandlePropertyValue(Type type, object entry, PropertyInfo prop, string prefix, string postfix, bool isScalar = false)
    {
        if (isScalar)
        {
            if (entry != null)
            {
                UIHelper.WrappedText($"{entry.ToString()}");
            }
        }
        else
        {
            var curValue = prop.GetValue(entry);
            var propType = prop.PropertyType;

            if (curValue != null)
            {
                UIHelper.WrappedText($"{curValue.ToString()}");
            }
        }
    }


}
