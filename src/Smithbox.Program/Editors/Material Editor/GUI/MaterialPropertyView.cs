using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using static SoulsFormats.MTD;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialPropertyView
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    private string PropertySearch = "";

    public MaterialPropertyView(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Draw()
    {
        Editor.FocusManager.SwitchMaterialEditorContext(MaterialEditorContext.Properties);

        var scale = DPI.UIScale();

        // Header
        ImGui.AlignTextToFramePadding();
        ImGui.InputText("##materialPropertySearch", ref PropertySearch, 255);
        UIHelper.Tooltip("Filter the properties by field names that exactly or partially match your input.");

        // Toggle Community Field Names
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Book}", DPI.IconButtonSize))
        {
            CFG.Current.MaterialEditor_DisplayCommunityFieldNames = !CFG.Current.MaterialEditor_DisplayCommunityFieldNames;
        }

        var communityFieldNameMode = "Internal";
        if (CFG.Current.MaterialEditor_DisplayCommunityFieldNames)
            communityFieldNameMode = "Community";

        UIHelper.Tooltip($"Toggle field name display type between Internal and Community.\nCurrent Mode: {communityFieldNameMode}");

        ImGui.Separator();

        // Properties
        ImGui.BeginChild("materialPropEdit");

        // var meta = Editor.Project.MaterialData.Meta.GetMeta(type, false);

        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Object Type");

        //if (meta != null)
        //{
        //    UIHelper.Tooltip(meta.Wiki);
        //}

        ImGui.NextColumn();
        ImGui.NextColumn();

        ImGui.Separator();

        if (Editor.Selection.SourceType is MaterialSourceType.MTD)
        {
            if (Editor.Selection.SelectedMTD != null)
            {
                PropertyHandler(Editor.Selection.SelectedMTD);
            }
        }

        if (Editor.Selection.SourceType is MaterialSourceType.MATBIN)
        {
            if (Editor.Selection.SelectedMATBIN != null)
            {
                PropertyHandler(Editor.Selection.SelectedMATBIN);
            }
        }

        ImGui.Columns(1);

        ImGui.EndChild();
    }

    private void PropertyHandler(
        object obj,
        int classIndex = -1
    )
    {
        var scale = DPI.UIScale();
        Type type = obj.GetType();

        PropertyInfo[] properties = Editor.MaterialPropertyCache.GetCachedProperties(type);

        // Properties
        var id = 0;
        foreach (PropertyInfo prop in properties)
        {
            // var meta = Editor.Project.MaterialData.Meta.GetFieldMeta(prop.Name, type);

            // Field Name
            var fieldName = prop.Name;

            //if (CFG.Current.MaterialEditor_DisplayCommunityFieldNames && !meta.IsEmpty)
            //{
            //    fieldName = meta.AltName;
            //}

            // Field Description
            var fieldDescription = "";

            //if (!meta.IsEmpty)
            //{
            //    fieldDescription = meta.Wiki;
            //}

            // Filter by Search
            var filterTerm = PropertySearch.ToLower();

            if (PropertySearch != "")
            {
                if (!prop.Name.ToLower().Contains(filterTerm))
                {
                    continue;
                }
            }

            var ignoreProp = prop.GetCustomAttribute<IgnoreInMaterialEditor>();
            if (ignoreProp != null)
            {
                continue;
            }

            //if (!meta.IsEmpty && PropertySearch != "")
            //{
            //    if (!meta.AltName.ToLower().Contains(filterTerm))
            //    {
            //        continue;
            //    }
            //}

            if (!prop.CanWrite && !prop.PropertyType.IsArray)
            {
                continue;
            }

            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();
            Type typ = prop.PropertyType;

            if (typ.IsArray)
            {
                var a = (Array)prop.GetValue(obj);
                var open = ImGui.TreeNodeEx($@"{fieldName}s", ImGuiTreeNodeFlags.DefaultOpen);

                ShowFieldHint(obj, prop, fieldDescription);

                ImGui.NextColumn();
                ImGui.NextColumn();
                if (open)
                {
                    for (var i = 0; i < a.Length; i++)
                    {
                        ImGui.PushID(i);
                        Type arrtyp = typ.GetElementType();
                        if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                        {
                            var classOpen = ImGui.TreeNodeEx($@"{fieldName}: {i}", ImGuiTreeNodeFlags.DefaultOpen);

                            ShowFieldHint(obj, prop, fieldDescription);

                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);

                            var o = a.GetValue(i);

                            ImGui.Text(o.GetType().Name);
                            ImGui.NextColumn();

                            if (classOpen)
                            {
                                PropertyHandler(o, i);

                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.AlignTextToFramePadding();
                            var array = obj as object[];

                            DisplayModelPropertyLine(obj, prop, typ.GetElementType(), a.GetValue(i), $@"{fieldName}[{i}]", i, classIndex);
                        }

                        ImGui.PopID();
                    }

                    ImGui.TreePop();
                }

                ImGui.PopID();
            }
            else if (typ.IsGenericType && typ.GetGenericTypeDefinition() == typeof(List<>))
            {
                var l = prop.GetValue(obj);
                if (l != null)
                {
                    PropertyInfo itemprop = l.GetType().GetProperty("Item");
                    var count = (int)l.GetType().GetProperty("Count").GetValue(l);

                    for (var i = 0; i < count; i++)
                    {
                        ImGui.PushID(i);

                        Type arrtyp = typ.GetGenericArguments()[0];

                        if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                        {
                            var open = ImGui.TreeNodeEx($@"{fieldName}: {i}", ImGuiTreeNodeFlags.DefaultOpen);

                            ShowFieldHint(obj, prop, fieldDescription);

                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);

                            var o = itemprop.GetValue(l, new object[] { i });

                            ImGui.Text(o.GetType().Name);

                            ImGui.NextColumn();

                            if (open)
                            {
                                PropertyHandler(o);

                                ImGui.TreePop();
                            }

                            ImGui.PopID();
                        }
                        else
                        {
                            DisplayModelPropertyLine(obj, prop, arrtyp, itemprop.GetValue(l, [i]), $@"{fieldName}[{i}]", i, classIndex);

                            ImGui.PopID();
                        }
                    }
                }

                ImGui.PopID();
            }
            else if (Editor.Selection.SourceType is MaterialSourceType.MTD && 
                prop.Name == "Value" && typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                var o = prop.GetValue(obj);
                if (o != null)
                {
                    var actualType = typ;

                    var actualParam = (MTD.Param)obj;


                    DisplayModelPropertyLine(obj, prop, actualType, o, $"{fieldName}", classIndex, -1, actualParam.Type);
                }

                ImGui.PopID();
            }
            else if (typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                var o = prop.GetValue(obj);
                if (o != null)
                {
                    var open = ImGui.TreeNodeEx($"{fieldName}", ImGuiTreeNodeFlags.DefaultOpen);

                    ShowFieldHint(obj, prop, fieldDescription);

                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);

                    ImGui.Text(o.GetType().Name);

                    ImGui.NextColumn();

                    if (open)
                    {
                        PropertyHandler(o);
                        ImGui.TreePop();
                    }
                }

                ImGui.PopID();
            }
            else
            {
                DisplayModelPropertyLine(obj, prop, typ, prop.GetValue(obj), $"{fieldName}", classIndex);

                ImGui.PopID();
            }

            id++;
        }
    }

    public void ShowFieldHint(object obj, PropertyInfo prop, string description)
    {
        var text = description;

        // Property Details
        var propType = prop.ReflectedType;

        if (propType.IsArray)
        {
            var a = (Array)prop.GetValue(obj);

            var str = $"Array Type: {prop.ReflectedType.Name}";
            if (a.Length > 0)
            {
                str += $" (Length: {a.Length})";
            }

            text = $"{text}\n{str}";
        }

        if (propType.IsValueType)
        {
            var str = $"Value Type: {propType.Name}";
            var min = propType.GetField("MinValue")?.GetValue(propType);
            var max = propType.GetField("MaxValue")?.GetValue(propType);
            if (min != null && max != null)
            {
                str += $" (Min {min}, Max {max})";
            }

            text = $"{text}\n{str}";
        }
        else if (propType == typeof(string))
        {
            var a = (Array)prop.GetValue(obj);

            var str = $"String Type: {propType.Name}";
            if (a.Length > 0)
            {
                str += $" (Length: {a.Length})";
            }

            text = $"{text}\n{str}";
        }

        // Final description
        UIHelper.Tooltip(text);
    }

    private void DisplayModelPropertyLine(
        object sourceObj,
        PropertyInfo prop,
        Type type,
        object obj,
        string name,
        int arrayIndex = -1,
        int classIndex = -1,
        ParamType paramType = ParamType.None
    )
    {
        OpenModelPropertyContextMenu();

        // var meta = Editor.Project.MapData.Meta.GetFieldMeta(prop.Name, prop.ReflectedType);

        // Field Name
        var fieldName = prop.Name;

        //if (CFG.Current.MapEditor_Enable_Commmunity_Names && !meta.IsEmpty)
        //{
        //    fieldName = meta.AltName;

        //    if (meta.ArrayProperty)
        //    {
        //        fieldName = $"{meta.AltName}: {arrayIndex}";
        //    }
        //}

        // Field Description
        var fieldDescription = "";

        //if (!meta.IsEmpty)
        //{
        //    fieldDescription = meta.Wiki;
        //}

        ImGui.Text(fieldName);

        ShowFieldHint(obj, prop, fieldDescription);

        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);

        var oldval = obj;
        object newval;

        // Property Editor UI
        (bool, bool) propEditResults = Editor.PropertyHandler.HandlePropertyInput(type, oldval, out newval, prop, sourceObj, paramType);

        var changed = propEditResults.Item1;
        var committed = propEditResults.Item2;

        DisplayMaterialPropertyContextMenu(prop, obj, arrayIndex, fieldName);

        if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
        {
            ImGui.SetItemDefaultFocus();
        }

        Editor.PropertyHandler.UpdateProperty(prop, sourceObj, oldval, newval, changed, committed, arrayIndex, classIndex);

        ImGui.NextColumn();
    }

    private static void OpenModelPropertyContextMenu()
    {
        ImGui.Selectable("", false, ImGuiSelectableFlags.AllowOverlap);

        ImGui.SameLine();

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("ModelPropContextMenu");
        }
    }

    private void DisplayMaterialPropertyContextMenu(PropertyInfo prop,
        object obj, int arrayIndex, string fieldName)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("MaterialPropContextMenu");
        }

        if (ImGui.BeginPopup("MaterialPropContextMenu"))
        {
            if (ImGui.Selectable(@"Copy Property Name##CopyPropName"))
            {
                PlatformUtils.Instance.SetClipboardText(fieldName);
            }

            if (ImGui.Selectable(@"Copy Property Type##CopyPropType"))
            {
                var propType = prop.PropertyType;

                if (propType != null)
                {
                    var primitiveType = propType.ToString().Replace("System.", "");
                    PlatformUtils.Instance.SetClipboardText(primitiveType);
                }
            }

            ImGui.EndPopup();
        }
    }
}
