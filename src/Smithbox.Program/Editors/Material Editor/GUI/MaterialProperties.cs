using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialProperties
{
    public MaterialEditorView Parent;
    public ProjectEntry Project;

    private string PropertyListFilter = "";
    private bool ExactPropertyListFilter = false;

    public MaterialProperties(MaterialEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Draw()
    {
        var scale = DPI.UIScale();

        DisplayTitle();
        DisplayHeader();
        DisplayPropertyList();

        ProcessListActions();
    }

    public void DisplayTitle()
    {
        GUI.SimpleHeader(
            LOC.Get("MAT_Properties_Header_Properties"),
            LOC.Get("MAT_Properties_Header_Properties_TT"));
    }

    public void DisplayHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("MaterialPropertySectionHeader", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("materialEditor_PropertyList",
            ref PropertyListFilter, ref ExactPropertyListFilter);

        // Toggle Community Field Names
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Book}", DPI.IconButtonSize))
        {
            CFG.Current.MaterialEditor_Properties_Display_Community_Names = !CFG.Current.MaterialEditor_Properties_Display_Community_Names;
        }

        var communityFieldNameMode = LOC.Get("MAT_Properties_DisplayCommunityNames_Internal");
        if (CFG.Current.MaterialEditor_Properties_Display_Community_Names)
            communityFieldNameMode = LOC.Get("MAT_Properties_DisplayCommunityNames_Community");

        GUI.Tooltip(LOC.Get("MAT_Properties_DisplayCommunityNames_Hint", communityFieldNameMode));

        ImGui.EndChild();
    }

    public void DisplayPropertyList()
    {
        ImGui.BeginChild("MaterialProperties", ImGuiChildFlags.Borders);

        // var meta = Editor.Project.MaterialData.Meta.GetMeta(type, false);

        ImGui.Columns(2);

        // Header
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(LOC.Get("MAT_Properties_Object_Type"));

            //if (meta != null)
            //{
            //    UIHelper.Tooltip(meta.Wiki);
            //}

            ImGui.NextColumn();
            ImGui.NextColumn();
        }

        ImGui.Separator();

        // Property Lines
        {
            if (Parent.Selection.SourceType is MaterialSourceType.MTD)
            {
                if (Parent.Selection.SelectedMTD != null)
                {
                    PropertyHandler("MTD", Parent.Selection.SelectedMTD);
                }
            }

            if (Parent.Selection.SourceType is MaterialSourceType.MATBIN)
            {
                if (Parent.Selection.SelectedMATBIN != null)
                {
                    PropertyHandler("MATBIN", Parent.Selection.SelectedMATBIN);
                }
            }
        }

        ImGui.Columns(1);

        ImGui.EndChild();
    }

    private void PropertyHandler(
        string implType,
        object obj,
        int classIndex = -1
    )
    {
        if (ProcessingListActions)
            return;

        var scale = DPI.UIScale();
        Type type = obj.GetType();

        PropertyInfo[] properties = Parent.MaterialPropertyCache.GetCachedProperties(type);

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
            var valueStr = $"{prop.GetValue(obj)}";
            var isMatch = EditorFilters.IsMatch(PropertyListFilter, fieldName, ExactPropertyListFilter, valueStr);

            if (!isMatch)
            {
                continue;
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

            // Array Line
            if (typ.IsArray)
            {
                var a = (Array)prop.GetValue(obj);
                var open = ImGui.TreeNodeEx($@"{fieldName}s", ImGuiTreeNodeFlags.DefaultOpen);

                ShowFieldHint(obj, prop, fieldDescription);

                ImGui.NextColumn();
                ImGui.NextColumn();

                // Array Element Lines
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
                                PropertyHandler(implType, o, i);

                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.AlignTextToFramePadding();
                            var array = obj as object[];

                            if (implType == "MTD")
                            {
                                DisplayMtdPropertyLine(obj, prop, typ.GetElementType(), a.GetValue(i), $@"{fieldName}[{i}]", i, classIndex);
                            }

                            if (implType == "MATBIN")
                            {
                                DisplayMatbinPropertyLine(obj, prop, typ.GetElementType(), a.GetValue(i), $@"{fieldName}[{i}]", i, classIndex);
                            }
                        }

                        ImGui.PopID();
                    }

                    ImGui.TreePop();
                }

                ImGui.PopID();
            }
            // List Line
            else if (typ.IsGenericType && typ.GetGenericTypeDefinition() == typeof(List<>))
            {
                var l = prop.GetValue(obj);

                if (l != null)
                {
                    PropertyInfo itemprop = l.GetType().GetProperty("Item");
                    var count = (int)l.GetType().GetProperty("Count").GetValue(l);

                    if(ImGui.CollapsingHeader($"{fieldName}##listHeader_{fieldName}", ImGuiTreeNodeFlags.DefaultOpen))
                    {
                        ImGui.NextColumn();
                        ImGui.NextColumn();

                        // List Element Lines
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

                                // Add the list adjustmnet action shere instead of the name
                                //ImGui.Text(o.GetType().Name);
                                if (arrtyp == typeof(MTD.Param) || arrtyp == typeof(MTD.Texture))
                                {
                                    if (ImGui.Button($"{Icons.Plus}##duplicateEntry{i}", DPI.IconButtonSize))
                                    {
                                        DuplicateListEntry(arrtyp, obj, i);
                                    }
                                    GUI.Tooltip(LOC.Get("MAT_Properties_Action_Duplicate_TT"));

                                    ImGui.SameLine();

                                    if (ImGui.Button($"{Icons.Minus}##removeEntry{i}", DPI.IconButtonSize))
                                    {
                                        RemoveListEntry(arrtyp, obj, i);
                                    }
                                    GUI.Tooltip(LOC.Get("MAT_Properties_Action_Delete_TT"));
                                }

                                ImGui.NextColumn();

                                if (open)
                                {
                                    PropertyHandler(implType, o);

                                    ImGui.TreePop();
                                }

                                ImGui.PopID();
                            }
                            else
                            {

                                if (implType == "MTD")
                                {
                                    DisplayMtdPropertyLine(obj, prop, arrtyp, itemprop.GetValue(l, [i]), $@"{fieldName}[{i}]", i, classIndex);
                                }

                                if (implType == "MATBIN")
                                {
                                    DisplayMatbinPropertyLine(obj, prop, arrtyp, itemprop.GetValue(l, [i]), $@"{fieldName}[{i}]", i, classIndex);
                                }

                                ImGui.PopID();
                            }
                        }
                    }
                    else
                    {
                        ImGui.NextColumn();
                        ImGui.NextColumn();
                    }
                }

                ImGui.PopID();
            }
            // Material Param 'Value' line
            else if (Parent.Selection.SourceType is MaterialSourceType.MTD && 
                prop.Name == "Value" && typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                var o = prop.GetValue(obj);
                if (o != null)
                {
                    var actualType = typ;

                    var actualParam = (MTD.Param)obj;

                    if (implType == "MTD")
                    {
                        DisplayMtdPropertyLine(obj, prop, actualType, o, $"{fieldName}", classIndex, -1, actualParam.Type);
                    }
                }

                ImGui.PopID();
            }
            // MATBIN Param 'Value' line
            else if (Parent.Selection.SourceType is MaterialSourceType.MATBIN &&
                prop.Name == "Value" && typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                var o = prop.GetValue(obj);
                if (o != null)
                {
                    var actualType = typ;

                    var actualParam = (MATBIN.Param)obj;

                    if (implType == "MATBIN")
                    {
                        DisplayMatbinPropertyLine(obj, prop, actualType, o, $"{fieldName}", classIndex, -1, actualParam.Type);
                    }
                }

                ImGui.PopID();
            }
            // Class Line
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

                    // Class properties
                    if (open)
                    {
                        PropertyHandler(implType, o);
                        ImGui.TreePop();
                    }
                }

                ImGui.PopID();
            }
            // Property Line
            else
            {
                if (implType == "MTD")
                {
                    DisplayMtdPropertyLine(obj, prop, typ, prop.GetValue(obj), $"{fieldName}", classIndex);
                }

                if (implType == "MATBIN")
                {
                    DisplayMatbinPropertyLine(obj, prop, typ, prop.GetValue(obj), $"{fieldName}", classIndex);
                }

                ImGui.PopID();
            }

            id++;
        }
    }

    // MTD
    private void DisplayMtdPropertyLine(
        object sourceObj,
        PropertyInfo prop,
        Type type,
        object obj,
        string name,
        int arrayIndex = -1,
        int classIndex = -1,
        MTD.ParamType paramType = MTD.ParamType.None
    )
    {
        OpenMaterialPropertyContextMenu();

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
        (bool, bool) propEditResults = Parent.PropertyInput.HandleMtdPropertyInput(type, oldval, out newval, prop, sourceObj, paramType);

        var changed = propEditResults.Item1;
        var committed = propEditResults.Item2;

        DisplayMaterialPropertyContextMenu(prop, obj, arrayIndex, fieldName);

        if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
        {
            ImGui.SetItemDefaultFocus();
        }

        Parent.PropertyInput.UpdateProperty(prop, sourceObj, oldval, newval, changed, committed, arrayIndex, classIndex);

        ImGui.NextColumn();
    }


    // MATBIN
    private void DisplayMatbinPropertyLine(
        object sourceObj,
        PropertyInfo prop,
        Type type,
        object obj,
        string name,
        int arrayIndex = -1,
        int classIndex = -1,
        MATBIN.ParamType paramType = MATBIN.ParamType.None
    )
    {
        OpenMaterialPropertyContextMenu();

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
        (bool, bool) propEditResults = Parent.PropertyInput.HandleMatbinPropertyInput(type, oldval, out newval, prop, sourceObj, paramType);

        var changed = propEditResults.Item1;
        var committed = propEditResults.Item2;

        DisplayMaterialPropertyContextMenu(prop, obj, arrayIndex, fieldName);

        if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
        {
            ImGui.SetItemDefaultFocus();
        }

        Parent.PropertyInput.UpdateProperty(prop, sourceObj, oldval, newval, changed, committed, arrayIndex, classIndex);

        ImGui.NextColumn();
    }

    public void ShowFieldHint(object obj, PropertyInfo prop, string description)
    {
        var text = description;

        // Property Details
        var propType = prop.ReflectedType;

        if (propType.IsArray)
        {
            var a = (Array)prop.GetValue(obj);

            var str = LOC.Get("MAT_Properties_FieldHint_Array_Type", prop.ReflectedType.Name);
            if (a.Length > 0)
            {
                str += LOC.Get("MAT_Properties_FieldHint_Array_Length", a.Length);
            }

            text = $"{text}\n{str}";
        }

        if (propType.IsValueType)
        {
            var str = LOC.Get("MAT_Properties_FieldHint_Value_Type", propType.Name);
            var min = propType.GetField("MinValue")?.GetValue(propType);
            var max = propType.GetField("MaxValue")?.GetValue(propType);
            if (min != null && max != null)
            {
                str += LOC.Get("MAT_Properties_FieldHint_Value_Min_Max", min, max);
            }

            text = $"{text}\n{str}";
        }
        else if (propType == typeof(string))
        {
            var a = (Array)prop.GetValue(obj);

            var str = LOC.Get("MAT_Properties_FieldHint_String_Type", propType.Name);
            if (a.Length > 0)
            {
                str += LOC.Get("MAT_Properties_FieldHint_String_Length", a.Length);
            }

            text = $"{text}\n{str}";
        }

        // Final description
        GUI.Tooltip(text);
    }

    private static void OpenMaterialPropertyContextMenu()
    {
        ImGui.Selectable("", false, ImGuiSelectableFlags.AllowOverlap);

        ImGui.SameLine();

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("MaterialPropContextMenu");
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
            // Copy Property Name
            if (ImGui.Selectable($"{LOC.Get("MAT_Properties_Action_Copy_Property_Name")}###CopyPropName"))
            {
                PlatformUtils.Instance.SetClipboardText(fieldName);
            }
            GUI.Tooltip(LOC.Get("MAT_Properties_Action_Copy_Property_Name_TT"));

            // Copy Property Type
            if (ImGui.Selectable($"{LOC.Get("MAT_Properties_Action_Copy_Property_Name")}###CopyPropType"))
            {
                var propType = prop.PropertyType;

                if (propType != null)
                {
                    var primitiveType = propType.ToString().Replace("System.", "");
                    PlatformUtils.Instance.SetClipboardText(primitiveType);
                }
            }
            GUI.Tooltip(LOC.Get("MAT_Properties_Action_Copy_Property_Type_TT"));

            ImGui.EndPopup();
        }
    }

    public bool ProcessingListActions = false;
    public List<EditorAction> ListActionsToProcess = new();

    public void ProcessListActions()
    {
        if (ListActionsToProcess.Count > 0)
        {
            ProcessingListActions = true;

            foreach(var action in ListActionsToProcess)
            {
                Parent.ActionManager.ExecuteAction(action);
            }

            ProcessingListActions = false;
        }
    }

    // TODO: actionize them
    public void DuplicateListEntry(Type arrType, object obj, int index)
    {
        if (arrType == typeof(MTD.Param))
        {
            var mtd = (MTD)obj;

            var targetParam = mtd.Params.ElementAt(index);
            var newParam = new MTD.Param(targetParam.Name, targetParam.Type, targetParam.Value);

            mtd.Params.Insert(index + 1, newParam);
        }

        if (arrType == typeof(MTD.Texture))
        {
            var mtd = (MTD)obj;

            var targetTex = mtd.Textures.ElementAt(index);
            var newTex = new MTD.Texture();
            newTex.Type = targetTex.Type;
            newTex.Extended = targetTex.Extended;
            newTex.UVNumber = targetTex.UVNumber;
            newTex.ShaderDataIndex = targetTex.ShaderDataIndex;
            newTex.Path = targetTex.Path;
            newTex.UnkFloats = targetTex.UnkFloats;

            mtd.Textures.Insert(index + 1, newTex);
        }
    }

    public void RemoveListEntry(Type arrType, object obj, int index)
    {
        if (arrType == typeof(MTD.Param))
        {
            var mtd = (MTD)obj;

            mtd.Params.RemoveAt(index);
        }

        if (arrType == typeof(MTD.Texture))
        {
            var mtd = (MTD)obj;

            mtd.Textures.RemoveAt(index);
        }
    }
}
