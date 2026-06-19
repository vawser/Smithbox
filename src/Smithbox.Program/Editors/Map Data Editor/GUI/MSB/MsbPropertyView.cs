using Andre.Formats;
using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Veldrid.MetalBindings;
using Vortice.Vulkan;
using static HKLib.hk2018.hkaiUserEdgeUtils;
using static SoulsFormats.PARAMDEF;

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

        if (View.Selection.SelectedEntries.Count == 0)
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

            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedText($"Property");

            // Value
            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();
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

        var index = 0;
        DisplayObjectProperties(ref index, type, targetEntry.Value, prefix: "", postfix: "");
    }

    private void DisplayObjectProperties(ref int index, Type type, object entry, string prefix, string postfix)
    {
        var properties = ReflectionHelper.GetCachedProperties(type);

        foreach (PropertyInfo prop in properties)
        {
            // Arrays and lists must be checked before IsPropertyClass because List<T> is also a class.
            if (ReflectionHelper.IsPropertyArray(type, entry, prop))
            {
                DisplayArrayPropertyEntries(ref index, type, entry, prop, prefix, postfix);
            }
            else if (ReflectionHelper.IsPropertyList(type, entry, prop))
            {
                DisplayListPropertyEntries(ref index, type, entry, prop, prefix, postfix);
            }
            else if (ReflectionHelper.IsPropertyClass(type, entry, prop))
            {
                // Sub-classes (including Shape subclasses): recurse with a prefixed label.
                var subValue = prop.GetValue(entry);
                if (subValue != null)
                {
                    var subType = subValue.GetType();
                    DisplayObjectProperties(ref index, subType, subValue, prefix, postfix);
                }
            }
            else
            {
                DisplayPropertyEntry(ref index, type, entry, prop, prefix, postfix);
            }

            index++;
        }
    }

    private void DisplayArrayPropertyEntries(ref int index, Type type, object entry, PropertyInfo prop, string prefix, string postfix)
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
                DisplayPropertyEntry(ref index, elementType, element, prop, prefix, postfix: $"[{i}]", true);
            }
            else
            {
                // Recurse into the element's own properties.
                DisplayObjectProperties(ref index, elementType, element, prefix, postfix: $"[{i}]");
            }

            index++;
        }
    }

    private void DisplayListPropertyEntries(ref int index, Type type, object entry, PropertyInfo prop, string prefix, string postfix)
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
                DisplayPropertyEntry(ref index, elementType, element, prop, prefix, postfix: $"[{i}]", true);
            }
            else
            {
                DisplayObjectProperties(ref index, elementType, element, prefix, postfix: $"[{i}]");
            }

            index++;
        }
    }

    public void DisplayPropertyEntry(ref int index, Type type, object entry, PropertyInfo prop, string prefix, string postfix, bool isScalar = false)
    {
        var meta = View.Project.Handler.MapData.Meta.GetMeta(type, false);
        var fieldMeta = View.Project.Handler.MapDataHandler.MsbMeta.GetFieldMeta(prop.Name, type);

        var context = new MsbPropertyContext(index, type, entry, prop, prefix, postfix, isScalar, meta, fieldMeta);

        if (CanDisplayPropertyRow(context))
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            HandlePropertyTitle(context);

            ImGui.TableSetColumnIndex(1);
            HandlePropertyValue(context);
        }

        if (CanDisplayPropertyRow(context) && HasPropertyMetaRow(context))
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            HandlePropertyMetaTitle(context);

            ImGui.TableSetColumnIndex(1);
            HandlePropertyMetaValue(context);
        }
    }

    public bool CanDisplayPropertyRow(MsbPropertyContext context)
    {
        var propName = context.Prop.Name;

        // Automatic conditions that hide the property

        if (!context.Prop.CanWrite && !context.Prop.PropertyType.IsArray)
        {
            return false;
        }

        // IMsbEntry.Name needs special handling to keep it unique
        if (typeof(IMsbEntry).IsAssignableFrom(context.Type) && context.Prop.Name == "Name")
            return false;

        // Index Properties are hidden by default
        if (context.FieldMeta != null && context.FieldMeta.IndexProperty)
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
        var isMatch = EditorFilters.IsMatch(PropertyListFilter, propName, ExactPropertyListFilter, context.FieldMeta.AltName);
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

            var propVal = context.Prop.GetValue(context.Entry);

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

    public void HandlePropertyTitle(MsbPropertyContext context)
    {
        // Field Name — community name replaces only the leaf name; the prefix is always structural.
        var fieldName = context.Prop.Name;

        if (CFG.Current.MapEditor_Properties_Enable_Commmunity_Names && context.FieldMeta != null && !context.FieldMeta.IsEmpty)
        {
            fieldName = context.FieldMeta.AltName;
        }

        // Field Description
        var fieldDescription = "";
        if (context.FieldMeta != null && !context.FieldMeta.IsEmpty)
        {
            fieldDescription = context.FieldMeta.Wiki;
        }

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText($"{context.Prefix}{fieldName}{context.Postfix}");
        UIHelper.Tooltip(fieldDescription);
    }

    // FIX: not handling array properties correctly
    public void HandlePropertyValue(MsbPropertyContext context)
    {
        if (context.IsScalar)
        {
            if (context.Entry != null)
            {
                //UIHelper.WrappedText($"{entry.ToString()}");

                object newValue;

                (bool, bool) propEditResults = PropertyRow(context.Index, context.Type, context.Entry, out newValue, context.Prop);

                var changed = propEditResults.Item1;
                var committed = propEditResults.Item2;

                UpdateProperty(context, context.Entry, newValue, changed, committed);
            }
        }
        else
        {
            var curValue = context.Prop.GetValue(context.Entry);
            var propType = context.Prop.PropertyType;

            if (curValue != null)
            {
                //UIHelper.WrappedText($"{curValue.ToString()}");

                object newval;

                (bool, bool) propEditResults = PropertyRow(context.Index, propType, curValue, out newval, context.Prop);

                var changed = propEditResults.Item1;
                var committed = propEditResults.Item2;

                UpdateProperty(context, curValue, newval, changed, committed);
            }
        }
    }

    public void UpdateProperty(MsbPropertyContext context, object oldValue, object newValue, bool changed, bool committed)
    {
        if (changed)
        {
            var action = new MsbPropertyChange(context.Prop, context.Entry, oldValue, newValue, context.Index);
            View.ActionManager.ExecuteAction(action);
        }
    }

    private (bool, bool) PropertyRow(int index, Type typ, object oldval, out object newval, PropertyInfo prop)
    {
        ImGui.PushID(index);
        var meta = View.Project.Handler.MapDataHandler.MsbMeta.GetFieldMeta(prop.Name, typ);

        ImGui.SetNextItemWidth(-1);
        ImGui.AlignTextToFramePadding();

        newval = null;
        var isChanged = false;
        if (typ == typeof(long))
        {
            var val = (long)oldval;
            var strval = $@"{val}";

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                var res = long.TryParse(newValue, out val);
                if (res)
                {
                    newval = val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(int))
        {
            var val = (int)oldval;

            if (meta != null && meta.IsBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                if (ImGui.InputInt("##value", ref val))
                {
                    newval = val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(uint))
        {
            var val = (uint)oldval;
            var strval = $@"{val}";

            if (meta != null && meta.IsBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                var input = new InputTextHandler(strval);

                if (input.Draw("##value", out string newValue))
                {
                    var res = uint.TryParse(newValue, out val);
                    if (res)
                    {
                        newval = val;
                        isChanged = true;
                    }
                }
            }
        }
        else if (typ == typeof(short))
        {
            int val = (short)oldval;

            if (meta != null && meta.IsBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = (short)val;
                    isChanged = true;
                }
            }
            else
            {
                if (ImGui.InputInt("##value", ref val))
                {
                    newval = (short)val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(ushort))
        {
            var val = (ushort)oldval;
            var strval = $@"{val}";

            if (meta != null && meta.IsBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                var input = new InputTextHandler(strval);

                if (input.Draw("##value", out string newValue))
                {
                    var res = ushort.TryParse(newValue, out val);
                    if (res)
                    {
                        newval = val;
                        isChanged = true;
                    }
                }
            }
        }
        else if (typ == typeof(sbyte))
        {
            int val = (sbyte)oldval;

            if (meta != null && meta.IsBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = (sbyte)val;
                    isChanged = true;
                }
            }
            else
            {
                if (ImGui.InputInt("##value", ref val))
                {
                    newval = (sbyte)val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(byte))
        {
            var val = (byte)oldval;
            var strval = $@"{val}";

            if (meta != null && meta.IsBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                if (ImGui.Checkbox("##value", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                var input = new InputTextHandler(strval);

                if (input.Draw("##value", out string newValue))
                {
                    var res = byte.TryParse(newValue, out val);
                    if (res)
                    {
                        newval = val;
                        isChanged = true;
                    }
                }
            }
        }
        else if (typ == typeof(bool))
        {
            var val = (bool)oldval;
            if (ImGui.Checkbox("##value", ref val))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(float))
        {
            var val = (float)oldval;
            if (ImGui.DragFloat("##value", ref val, 0.1f, float.MinValue, float.MaxValue,
                    Utils.ImGui_InputFloatFormat(val)))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(string))
        {
            var val = (string)oldval;
            if (val == null)
            {
                val = "";
            }

            var input = new InputTextHandler(val);

            if (input.Draw("##value", out string newValue))
            {
                newval = newValue;
                isChanged = true;
            }
        }
        else if (typ == typeof(Vector2))
        {
            var val = (Vector2)oldval;
            if (ImGui.DragFloat2("##value", ref val, 0.1f))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(Vector3))
        {
            var val = (Vector3)oldval;

            if (ImGui.DragFloat3("##value", ref val, 0.1f))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ.BaseType == typeof(Enum))
        {
            Array enumVals = typ.GetEnumValues();
            var enumNames = typ.GetEnumNames();
            var intVals = new int[enumVals.Length];

            if (typ.GetEnumUnderlyingType() == typeof(byte))
            {
                for (var i = 0; i < enumVals.Length; i++)
                {
                    intVals[i] = (byte)enumVals.GetValue(i);
                }

                if (Utils.EnumEditor(enumVals, enumNames, oldval, out var val, intVals))
                {
                    newval = val;
                    isChanged = true;
                }
            }
            else if (typ.GetEnumUnderlyingType() == typeof(sbyte))
            {
                for (var i = 0; i < enumVals.Length; i++)
                {
                    intVals[i] = (sbyte)enumVals.GetValue(i);
                }

                if (Utils.EnumEditor(enumVals, enumNames, oldval, out var val, intVals))
                {
                    newval = val;
                    isChanged = true;
                }
            }
            else if (typ.GetEnumUnderlyingType() == typeof(int))
            {
                for (var i = 0; i < enumVals.Length; i++)
                {
                    intVals[i] = (int)enumVals.GetValue(i);
                }

                if (Utils.EnumEditor(enumVals, enumNames, oldval, out var val, intVals))
                {
                    newval = val;
                    isChanged = true;
                }
            }
            else if (typ.GetEnumUnderlyingType() == typeof(uint))
            {
                for (var i = 0; i < enumVals.Length; i++)
                {
                    intVals[i] = (int)(uint)enumVals.GetValue(i);
                }

                if (Utils.EnumEditor(enumVals, enumNames, oldval, out var val, intVals))
                {
                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                ImGui.Text("ImplementMe");
            }
        }
        else if (typ == typeof(Color))
        {
            var att = prop?.GetCustomAttribute<SupportsAlphaAttribute>();
            if (att != null)
            {
                if (att.Supports == false)
                {
                    var color = (Color)oldval;
                    Vector3 val = new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
                    if (ImGui.ColorEdit3("##value", ref val))
                    {
                        Color newColor = Color.FromArgb((int)(val.X * 255.0f), (int)(val.Y * 255.0f),
                            (int)(val.Z * 255.0f));
                        newval = newColor;
                        isChanged = true;
                    }
                }
                else
                {
                    var color = (Color)oldval;
                    Vector4 val = new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

                    var flags = ImGuiColorEditFlags.AlphaOpaque;

                    if (ImGui.ColorEdit4("##value", ref val, flags))
                    {
                        Color newColor = Color.FromArgb((int)(val.W * 255.0f), (int)(val.X * 255.0f),
                            (int)(val.Y * 255.0f), (int)(val.Z * 255.0f));
                        newval = newColor;
                        isChanged = true;
                    }
                }
            }
            else
            {
                // SoulsFormats does not define if alpha should be exposed. Expose alpha by default.
                //Smithbox.Log(this,
                //    $"Color property in \"{prop.DeclaringType}\" does not declare if it supports Alpha. Alpha will be exposed by default",
                //    LogLevel.Warning, LogPriority.Low);

                var color = (Color)oldval;
                Vector4 val = new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

                var flags = ImGuiColorEditFlags.AlphaOpaque;

                if (ImGui.ColorEdit4("##value", ref val, flags))
                {
                    Color newColor = Color.FromArgb((int)(val.W * 255.0f), (int)(val.X * 255.0f),
                        (int)(val.Y * 255.0f), (int)(val.Z * 255.0f));
                    newval = newColor;
                    isChanged = true;
                }
            }
        }
        else
        {
            ImGui.Text("ImplementMe");
        }

        var isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive();

        ImGui.PopID();

        return (isChanged, isDeactivatedAfterEdit);
    }

    public bool HasPropertyMetaRow(MsbPropertyContext context)
    {
        if (context.FieldMeta == null)
            return false;

        var hasMetaElement = false;

        // Param References
        if (context.FieldMeta.ParamRef.Count > 0)
            hasMetaElement = true;

        return hasMetaElement;
    }

    public void HandlePropertyMetaTitle(MsbPropertyContext context)
    {
        HandleParamRefTitle(context);
    }

    public void HandlePropertyMetaValue(MsbPropertyContext context)
    {
        var oldValue = context.Entry;
        if(!context.IsScalar)
        {
            oldValue = context.Prop.GetValue(context.Entry);
        }

        HandleParamRefHint(context, oldValue);
        HandleParamRefClick(context, oldValue);
        HandleParamRefContext(context, oldValue);
    }

    #region Param References
    public List<ParamRef> GetParamReferences(MsbPropertyContext context)
    {
        List<ParamRef> refs = new();

        if (context.FieldMeta == null)
            return refs;

        foreach (var pRef in context.FieldMeta.ParamRef)
        {
            refs.Add(new ParamRef(null, pRef.ParamName));
        }

        return refs;
    }

    public void HandleParamRefTitle(MsbPropertyContext context)
    {
        if (context.FieldMeta == null)
            return;

        if (context.FieldMeta.ParamRef.Count <= 0)
            return;

        List<ParamRef> refs = GetParamReferences(context);

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(@"   <");

        List<string> inactiveRefs = new();
        var first = true;
        foreach (ParamRef r in refs)
        {
            var inactiveRef = false;

            if (inactiveRef)
            {
                inactiveRefs.Add(r.ParamName);
            }
            else
            {
                if (first)
                {
                    ImGui.SameLine();
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted(r.ParamName);
                }
                else
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted("    " + r.ParamName);
                }

                first = false;
            }
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefInactive_Text);

        foreach (var inactive in inactiveRefs)
        {
            ImGui.SameLine();
            if (first)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("!" + inactive);
            }
            else
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("!" + inactive);
            }

            first = false;
        }

        ImGui.PopStyleColor();

        ImGui.SameLine();

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(">");

        ImGui.PopStyleVar();
    }

    public void HandleParamRefHint(MsbPropertyContext context, object oldValue)
    {
        if (context.FieldMeta == null)
            return;

        if (context.FieldMeta.ParamRef.Count <= 0)
            return;

        if (Project.Handler.ParamEditor == null)
            return;

        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        List<ParamRef> refs = GetParamReferences(context);

        List<(string, Param.Row, string)> matches = ParamReferenceResolver.ResolveParamReferences(activeView, refs, "", null, oldValue);

        var entryFound = matches.Count > 0;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);
        ImGui.BeginGroup();

        foreach ((var param, Param.Row row, var adjName) in matches)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(adjName);
        }

        ImGui.PopStyleColor();
        if (!entryFound)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefMissing_Text);

            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("---");
            ImGui.PopStyleColor();
        }

        ImGui.EndGroup();
    }

    public void HandleParamRefClick(MsbPropertyContext context, object oldValue)
    {
        if (context.FieldMeta == null)
            return;

        if (context.FieldMeta.ParamRef.Count <= 0)
            return;

        if (Project.Handler.ParamEditor == null)
            return;

        List<ParamRef> refs = GetParamReferences(context);

        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && InputManager.HasCtrlDown())
        {
            if (refs != null)
            {
                (string, Param.Row, string)? primaryRef =
                    ParamReferenceResolver.ResolveParamReferences(activeView, refs, "", null, oldValue)?.FirstOrDefault();

                if (primaryRef?.Item2 != null)
                {
                    if (InputManager.HasShiftDown())
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/select/new/{primaryRef?.Item1}/{primaryRef?.Item2.ID}");
                    }
                    else
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/select/-1/{primaryRef?.Item1}/{primaryRef?.Item2.ID}");
                    }
                }
            }
        }
    }
    public void HandleParamRefContext(MsbPropertyContext context, object oldValue)
    {
        if (context.FieldMeta == null)
            return;

        if (context.FieldMeta.ParamRef.Count <= 0)
            return;

        if (Project.Handler.ParamEditor == null)
            return;

        List<ParamRef> refs = GetParamReferences(context);

        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        if (ImGui.BeginPopupContextItem($"{context.Prop.Name}EnumContextMenu"))
        {
            DisplayParamRefContextMenu(activeView, refs, oldValue);

            ImGui.EndPopup();
        }
    }

    public void DisplayParamRefContextMenu(ParamEditorView curView, List<ParamRef> reftypes, object oldValue)
    {
        if (curView.GetPrimaryBank().Params == null)
        {
            return;
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);

        // Add Goto statements
        List<(string, Param.Row, string)> refs = ParamReferenceResolver.ResolveParamReferences(curView, reftypes, "", null, oldValue);

        int index = 0;

        foreach ((string, Param.Row, string) rf in refs)
        {
            if (ImGui.Selectable($@"Go to {rf.Item3}##GoToElement{index}"))
            {
                EditorCommandQueue.AddCommand($@"param/select/-1/{rf.Item1}/{rf.Item2.ID}");
            }

            if (ImGui.Selectable($@"Go to {rf.Item3} in new view##GoToElementInView{index}"))
            {
                EditorCommandQueue.AddCommand($@"param/select/new/{rf.Item1}/{rf.Item2.ID}");
            }

            index++;
        }

        ImGui.PopStyleColor();
    }
    #endregion
}

public class MsbPropertyContext
{
    public int Index;
    public Type Type;
    public object Entry;
    public PropertyInfo Prop;
    public string Prefix;
    public string Postfix;
    public bool IsScalar;
    public MapEntityPropertyMeta Meta;
    public MapEntityPropertyFieldMeta FieldMeta;

    public MsbPropertyContext(int index, Type type, object entry, PropertyInfo prop, string prefix, string postfix, bool isScalar, MapEntityPropertyMeta meta, MapEntityPropertyFieldMeta fieldMeta)
    {
        Index = index;
        Type = type;
        Entry = entry;
        Prop = prop;
        Prefix = prefix;
        Postfix = postfix;
        IsScalar = isScalar;
        Meta = meta;
        FieldMeta = fieldMeta;
    }
}