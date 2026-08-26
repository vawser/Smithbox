using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialProperties
{
    public MaterialEditorView View;
    public ProjectEntry Project;

    private object _changingProperty;
    private EditorAction _lastUncommittedAction;

    private string PropertyListFilter = "";
    private bool ExactPropertyListFilter = false;

    public MaterialProperties(MaterialEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Draw()
    {
        var scale = DPI.UIScale();

        DisplayTitle();
        DisplayHeader();

        ImGui.BeginChild("MaterialProperties", ImGuiChildFlags.Borders);

        if (View.Selection.SourceType is MaterialSourceType.MTD)
        {
            if (View.Selection.SelectedMTD != null)
            {
                DisplayPropertyList("MTD", View.Selection.SelectedMTD);
            }
        }

        if (View.Selection.SourceType is MaterialSourceType.MATBIN)
        {
            if (View.Selection.SelectedMATBIN != null)
            {
                DisplayPropertyList("MATBIN", View.Selection.SelectedMATBIN);
            }
        }

        ImGui.EndChild();
    }

    public void DisplayTitle()
    {
        GUI.SimpleHeader(
            LOC.Get("MAT_Properties_Header_Properties"),
            LOC.Get("MAT_Properties_Header_Properties_TT"));
    }

    public void DisplayHeader()
    {
        ImGui.BeginChild("MaterialPropertySectionHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("materialEditor_PropertyList",
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

        // Type Column
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Calculator}##toggleTypeCol"))
        {
            CFG.Current.MaterialEditor_Properties_Display_Type_Column = !CFG.Current.MaterialEditor_Properties_Display_Type_Column;
        }

        var typeColumnVis = LOC.Get("MAT_PropertyView_Type_Column_Toggle_Hide");
        if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
            typeColumnVis = LOC.Get("MAT_PropertyView_Type_Column_Toggle_Show");

        GUI.Tooltip(LOC.Get("MAT_PropertyView_Type_Column_Toggle_TT", typeColumnVis));

        ImGui.EndChild();
    }

    public void DisplayPropertyList(string implType, object sourceObj)
    {
        var type = sourceObj.GetType();

        var columnCount = 2;
        if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
        {
            columnCount = 3;
        }

        ImGui.Columns(columnCount);

        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get("MAT_Properties_Object_Type"));

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Text(type.Name);

        ImGui.NextColumn();

        if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
        {
            ImGui.NextColumn();
        }

        var classMeta = MaterialMetaHelper.GetMeta(Project, type, implType);
        PropEditGeneric(implType, sourceObj, classMeta);

        ImGui.Columns(1);
    }

    private void PropEditGeneric(
        string implType,
        object obj,
        MaterialClass classMeta,
        int classIndex = -1
    )
    {
        if (obj == null)
            return;

        var scale = DPI.UIScale();
        Type type = obj.GetType();

        PropertyInfo[] properties = View.MaterialPropertyCache.GetCachedProperties(type);

        // Properties
        var id = 0;
        foreach (PropertyInfo prop in properties)
        {
            classMeta = MaterialMetaHelper.GetMeta(Project, type, implType);

            var treeFlags = ImGuiTreeNodeFlags.DefaultOpen;

            // Field Name
            var fieldName = prop.Name;
            var fieldDescription = "";

            if (classMeta != null)
            {
                if (CFG.Current.MaterialEditor_Properties_Display_Community_Names)
                {
                    fieldName = MaterialMetaHelper.GetFieldName(classMeta, prop.Name);
                }

                fieldDescription = $"{MaterialMetaHelper.GetFieldDescription(classMeta, prop.Name)}";
            }

            // Filter by Search
            if(!DisplayProperty(classMeta, obj, prop, type))
            {
                continue;
            }

            Type typ = prop.PropertyType;

            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();

            if (prop.Name == "Value" && typ == typeof(object))
            {
                if (DisplayProperty(classMeta, obj, prop, type))
                {
                    PropGenericFieldRow(prop, typ, classMeta, implType, prop.GetValue(obj), obj, fieldName, fieldDescription, classIndex);
                }
                ImGui.PopID();
            }
            // Array Line
            else if (typ.IsArray)
            {
                var a = (Array)prop.GetValue(obj);
                var open = ImGui.TreeNodeEx($@"{fieldName}", treeFlags);

                ShowFieldHint(obj, prop, fieldDescription);

                ImGui.NextColumn();
                ImGui.NextColumn();

                if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
                {
                    PropContextRowOpener("arrayTypeCol");

                    ImGui.Text(type.FullName);

                    DisplayContextMenu(fieldName, fieldDescription, prop);

                    ImGui.NextColumn();
                }

                // Array Element Lines
                if (open)
                {
                    for (var i = 0; i < a.Length; i++)
                    {
                        ImGui.PushID(i);
                        Type arrtyp = typ.GetElementType();
                        if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                        {
                            var classOpen = ImGui.TreeNodeEx($@"{fieldName}: {i}", treeFlags);

                            ShowFieldHint(obj, prop, fieldDescription);

                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);

                            var o = a.GetValue(i);

                            ImGui.Text("");
                            ImGui.NextColumn();

                            if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
                            {
                                PropContextRowOpener("arrayTypeEntryCol");

                                ImGui.Text(type.FullName);

                                DisplayContextMenu(fieldName, fieldDescription, prop);

                                ImGui.NextColumn();
                            }

                            if (classOpen)
                            {
                                PropEditGeneric(implType, o, classMeta, i);

                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.AlignTextToFramePadding();
                            var array = obj as object[];

                            // Handle property display (and search filtering)
                            if (DisplayProperty(classMeta, obj, prop, type))
                            {
                                PropGenericFieldRow(prop, typ.GetElementType(), classMeta, implType, a.GetValue(i), obj, $@"{fieldName}[{i}]", fieldDescription, i, classIndex);
                            }
                        }

                        ImGui.PopID();
                    }

                    ImGui.TreePop();
                }

                ImGui.PopID();
            }
            else if (typ.IsGenericType && typ.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type arrtyp = typ.GetGenericArguments()[0];
                PropEditorGenericList(classMeta, implType, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex);
            }
            else if (typ.BaseType != null && typ.BaseType.IsGenericType
                && typ.BaseType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type arrtyp = typ.BaseType.GetGenericArguments()[0];
                PropEditorGenericList(classMeta, implType, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex);
            }
            // Class Line
            else if (typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                var o = prop.GetValue(obj);
                if (o != null)
                {
                    var open = ImGui.TreeNodeEx($"{fieldName}", treeFlags);

                    ShowFieldHint(obj, prop, fieldDescription);

                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);
                    ImGui.Text("");

                    ImGui.NextColumn();

                    if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
                    {
                        PropContextRowOpener("classTypeCol");

                        ImGui.Text(type.FullName);

                        DisplayContextMenu(fieldName, fieldDescription, prop);

                        ImGui.NextColumn();
                    }

                    // Class properties
                    if (open)
                    {
                        PropEditGeneric(implType, o, classMeta);
                        ImGui.TreePop();
                    }
                }

                ImGui.PopID();
            }
            // Property Line
            else
            {
                // Handle property display (and search filtering)
                if (DisplayProperty(classMeta, obj, prop, type))
                {
                    PropGenericFieldRow(prop, typ, classMeta, implType, prop.GetValue(obj), obj, fieldName, fieldDescription, classIndex);
                }

                ImGui.PopID();
            }

            id++;
        }
    }

    public bool DisplayProperty(MaterialClass classMeta, object propObj, PropertyInfo prop, Type type)
    {
        var propName = prop.Name;

        var ignoreProp = prop.GetCustomAttribute<IgnoreInMaterialEditor>();
        if (ignoreProp != null)
        {
            return false;
        }

        // Normal filter
        var isMatch = EditorFilters.IsMatch(PropertyListFilter, propName, ExactPropertyListFilter);
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

            var propVal = prop.GetValue(propObj);

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


    private void PropGenericFieldRow(
        PropertyInfo prop,
        Type type,
        MaterialClass classMeta,
        string implType,
        object value,
        object containerObj,
        string name,
        string description,
        int arrayIndex = -1,
        int classIndex = -1,
        Action onRemove = null

    )
    {
        PropContextRowOpener("nameCol");

        ImGui.Text(name);

        // Remove-from-list button (only passed in by list entries)
        if (onRemove != null)
        {
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            if (ImGui.Button($"{LOC.Get("EDITOR_PropEdit_Remove_List_Entry")}##removeListEntry"))
            {
                onRemove();
            }
            GUI.Tooltip(LOC.Get("EDITOR_PropEdit_Remove_List_Entry_TT"));
        }

        GUI.Tooltip(description);

        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);

        var oldval = value;
        object newval;

        // Property Editor UI
        (bool, bool) propEditResults = (false, false);

        // Determine param type
        if (View.Selection.SourceType is MaterialSourceType.MATBIN &&
            implType == "MATBIN" &&
            prop.Name == "Value" && 
            type.IsClass && type != typeof(string) && !type.IsArray)
        {
            var o = prop.GetValue(containerObj);
            if (o != null)
            {
                var actualParam = (MATBIN.Param)containerObj;

                propEditResults = View.PropertyInput.HandleMatbinPropertyInput(type, oldval, out newval, prop, containerObj, actualParam.Type);

                var changed = propEditResults.Item1;
                var committed = propEditResults.Item2;

                DisplayContextMenu(name, description, prop);

                if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
                {
                    ImGui.SetItemDefaultFocus();
                }

                if (MaterialPropertyDecorators.ParamRefRow(View, classMeta, prop, oldval, ref newval))
                {
                    changed = true;
                    committed = true;
                }

                UpdateProperty(prop, containerObj, oldval, newval, changed, committed, arrayIndex, classIndex);
            }
        }
        else if (View.Selection.SourceType is MaterialSourceType.MTD &&
            implType == "MTD" &&
            prop.Name == "Value" &&
            type.IsClass && type != typeof(string) && !type.IsArray)
        {
            var o = prop.GetValue(containerObj);
            if (o != null)
            {
                var actualParam = (MTD.Param)containerObj;

                propEditResults = View.PropertyInput.HandleMtdPropertyInput(type, oldval, out newval, prop, containerObj, actualParam.Type);

                var changed = propEditResults.Item1;
                var committed = propEditResults.Item2;

                DisplayContextMenu(name, description, prop);

                if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
                {
                    ImGui.SetItemDefaultFocus();
                }

                if (MaterialPropertyDecorators.ParamRefRow(View, classMeta, prop, oldval, ref newval))
                {
                    changed = true;
                    committed = true;
                }

                UpdateProperty(prop, containerObj, oldval, newval, changed, committed, arrayIndex, classIndex);
            }
        }
        else
        {
            propEditResults = View.PropertyInput.PropertyRow(type, oldval, out newval, prop);

            var changed = propEditResults.Item1;
            var committed = propEditResults.Item2;

            DisplayContextMenu(name, description, prop);

            if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
            {
                ImGui.SetItemDefaultFocus();
            }

            if (MaterialPropertyDecorators.ParamRefRow(View, classMeta, prop, oldval, ref newval))
            {
                changed = true;
                committed = true;
            }

            UpdateProperty(prop, containerObj, oldval, newval, changed, committed, arrayIndex, classIndex);
        }

        if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
        {
            ImGui.NextColumn();

            var reflectedType = prop.ReflectedType;
            if (reflectedType != null)
            {
                PropContextRowOpener("typecol");

                ImGui.Text(reflectedType.FullName);
            }
        }

        ImGui.NextColumn();
    }

    private void UpdateProperty(object prop, object obj, object oldval, object newval,
        bool changed, bool committed, int arrayindex = -1, int classIndex = -1)
    {
        if (changed)
        {
            ChangeProperty(prop, obj, oldval, newval, ref committed, arrayindex, classIndex);
        }

        if (committed)
        {
            if (_lastUncommittedAction != null && View.ActionManager.PeekUndoAction() == _lastUncommittedAction)
            {
                _lastUncommittedAction = null;
                _changingProperty = null;
            }
        }
    }

    private void ChangeProperty(object prop, object obj, object oldval, object newval,
        ref bool committed,
        int arrayindex = -1, int classIndex = -1)
    {
        if (prop == _changingProperty && _lastUncommittedAction != null &&
            View.ActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            View.ActionManager.UndoAction();
        }
        else
        {
            _lastUncommittedAction = null;
        }

        var action = new MatPropertyChange(View, (PropertyInfo)prop, obj, newval, arrayindex, classIndex);
        View.ActionManager.ExecuteAction(action);

        _lastUncommittedAction = action;
        _changingProperty = prop;
    }

    private void PropEditorGenericList(
        MaterialClass classMeta,
        string implType,
        object obj,
        PropertyInfo prop,
        Type elementType,
        string fieldName,
        string fieldDescription,
        ImGuiTreeNodeFlags treeFlags,
        int classIndex
    )
    {
        var list = (IList)prop.GetValue(obj);

        var open = ImGui.TreeNodeEx($@"{fieldName}", treeFlags);
        GUI.Tooltip(fieldDescription);
        ImGui.NextColumn();

        if (list != null)
        {
            ImGui.AlignTextToFramePadding();
            if (ImGui.Button($"{LOC.Get("EDITOR_PropEdit_Add_List_Entry")}##addListEntry"))
            {
                var newEntry = PropFinderUtil.CreateDefaultListElement(elementType);

                if(newEntry == null && elementType == typeof(MTD.Param))
                {
                    newEntry = new MTD.Param("Untitled", MTD.ParamType.None, 0);
                }
                else if (newEntry == null && elementType == typeof(MATBIN.Param))
                {
                    newEntry = new MATBIN.Param();
                }

                var action = new MatAddListEntry(prop, obj, newEntry, list.Count);
                View.ActionManager.ExecuteAction(action);
            }
            GUI.Tooltip(LOC.Get("EDITOR_PropEdit_Add_List_Entry_TT"));
        }

        ImGui.NextColumn();

        if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
        {
            PropContextRowOpener("listTypeCol");

            ImGui.Text(prop?.GetType().FullName);

            DisplayContextMenu(fieldName, fieldDescription, prop);

            ImGui.NextColumn();
        }

        if (open)
        {
            if (list != null)
            {
                var removeIndex = -1;

                for (var i = 0; i < list.Count; i++)
                {
                    ImGui.PushID(i);
                    var idx = i;
                    var elem = list[i];
                    void OnRemove() => removeIndex = idx;

                    if (elementType.IsClass && elementType != typeof(string) && !elementType.IsArray)
                    {
                        var classOpen = ImGui.TreeNodeEx($@"{fieldName}: {i}", treeFlags);
                        GUI.Tooltip(fieldDescription);
                        ImGui.NextColumn();
                        ImGui.SetNextItemWidth(-1);
                        ImGui.Text(elem?.GetType().Name ?? "null");

                        ImGui.SameLine();
                        ImGui.AlignTextToFramePadding();
                        if (ImGui.Button($"{LOC.Get("EDITOR_PropEdit_Remove_List_Entry")}##removeListEntry"))
                        {
                            OnRemove();
                        }
                        GUI.Tooltip(LOC.Get("EDITOR_PropEdit_Remove_List_Entry_TT"));

                        ImGui.NextColumn();

                        if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
                        {
                            PropContextRowOpener("listTypeCol");

                            ImGui.Text(elem?.GetType().FullName);

                            DisplayContextMenu(fieldName, fieldDescription, prop);

                            ImGui.NextColumn();
                        }

                        if (classOpen)
                        {
                            if (elem != null)
                                PropEditGeneric(implType, elem, classMeta, idx);

                            ImGui.TreePop();
                        }
                    }
                    else
                    {
                        // Handle property display (and search filtering)
                        if (DisplayProperty(classMeta, obj, prop, elementType))
                        {
                            PropGenericFieldRow(prop, elementType, classMeta, implType, elem, obj, $@"{fieldName}[{i}]", fieldDescription, i, classIndex, OnRemove);
                        }
                    }

                    ImGui.PopID();
                }

                if (removeIndex != -1)
                {
                    var action = new MatRemoveListEntry(prop, obj, removeIndex);
                    View.ActionManager.ExecuteAction(action);
                }
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private void PropContextRowOpener(string id)
    {
        ImGui.Selectable($"###{id}", false, ImGuiSelectableFlags.AllowOverlap);
        ImGui.SameLine();
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("MaterialPropertiesContextMenu");
        }
    }

    private void DisplayContextMenu(string name, string description, PropertyInfo prop)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("MaterialPropertiesContextMenu");
        }

        if (ImGui.BeginPopup("MaterialPropertiesContextMenu"))
        {
            // Copy Field Name
            if (ImGui.Selectable($"{LOC.Get("MAT_PropertyView_Context_Action_Copy_Name")}##CopyPropName"))
            {
                PlatformUtils.Instance.SetClipboardText(name);
            }
            GUI.Tooltip(LOC.Get("MAT_PropertyView_Context_Action_Copy_Name_TT"));

            // Copy Field Description
            if (ImGui.Selectable($"{LOC.Get("MAT_PropertyView_Context_Action_Copy_Description")}##CopyPropDesc"))
            {
                PlatformUtils.Instance.SetClipboardText(description);
            }
            GUI.Tooltip(LOC.Get("MAT_PropertyView_Context_Action_Copy_Description_TT"));

            // Copy Field Type
            if (ImGui.Selectable($"{LOC.Get("MAT_PropertyView_Context_Action_Copy_Type")}##CopyPropType"))
            {
                var reflectedType = prop.ReflectedType;
                if (reflectedType != null)
                {
                    PlatformUtils.Instance.SetClipboardText(reflectedType.FullName);
                }
            }
            GUI.Tooltip(LOC.Get("MAT_PropertyView_Context_Action_Copy_Type_TT"));

            ImGui.EndPopup();
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


}
