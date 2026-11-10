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

    // Deepest nesting searched for a match: material > list > param > value
    private const int MaxSearchDepth = 8;

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
        GUI.TitleHeader(
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

    /// <summary>
    /// Searches the properties for a field, as clicking a check finding does
    /// </summary>
    public void SetFilter(string text)
    {
        PropertyListFilter = text ?? "";
        ExactPropertyListFilter = false;
    }

    /// <summary>
    /// The selected material's check findings, above its properties
    /// </summary>
    private void DisplayFindings()
    {
        var findings = MaterialEditorChecks.CheckFile(Project.Handler.MaterialData.PrimaryBank,
            View.Selection.SelectedBinder, View.Selection.SelectedFileWrapper);

        for (var i = 0; i < findings.Count; i++)
        {
            if (MaterialEditorChecks.DisplayFinding(findings[i], $"propertyFinding{i}") && findings[i].Target != "")
            {
                SetFilter(findings[i].Target);
            }
        }

        if (findings.Count > 0)
            ImGui.Separator();
    }

    public void DisplayPropertyList(string implType, object sourceObj)
    {
        DisplayFindings();

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

        PropEditGeneric(implType, sourceObj);

        ImGui.Columns(1);
    }

    /// <summary>
    /// Draws the properties of an object. With a search, a property is drawn if it matches or something
    /// inside it does; <paramref name="forceShow"/> draws everything, for the inside of a match.
    /// </summary>
    private void PropEditGeneric(
        string implType,
        object obj,
        int classIndex = -1,
        bool forceShow = false
    )
    {
        if (obj == null)
            return;

        Type type = obj.GetType();

        var classMeta = MaterialMetaHelper.GetMeta(Project, type, implType);

        PropertyInfo[] properties = View.MaterialPropertyCache.GetCachedProperties(type);

        // Properties
        var id = 0;
        foreach (PropertyInfo prop in properties)
        {
            if (IsHidden(prop))
                continue;

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

            var value = prop.GetValue(obj);

            // Filter by Search
            var selfMatch = forceShow || RowMatches(prop.Name, fieldName, value);

            if (!selfMatch && !ChildrenMatch(value, 0))
                continue;

            Type typ = prop.PropertyType;

            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();

            if (prop.Name == "Value" && typ == typeof(object))
            {
                PropGenericFieldRow(prop, typ, classMeta, implType, value, obj, fieldName, fieldDescription,
                    classIndex: classIndex);
            }
            // Array Line
            else if (typ.IsArray)
            {
                var a = (Array)value;
                var open = ImGui.TreeNodeEx($@"{fieldName}", treeFlags);

                ShowFieldHint(value, typ, fieldDescription);

                ImGui.NextColumn();
                ImGui.NextColumn();

                if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
                {
                    var typeLabel = TypeLabel(typ);

                    PropContextRowOpener("arrayTypeCol");

                    ImGui.Text(typeLabel);

                    DisplayContextMenu(fieldName, fieldDescription, typeLabel);

                    ImGui.NextColumn();
                }

                // Array Element Lines
                if (open)
                {
                    Type arrtyp = typ.GetElementType();

                    for (var i = 0; a != null && i < a.Length; i++)
                    {
                        var o = a.GetValue(i);

                        if (!selfMatch && !ElementMatches(o, 0))
                            continue;

                        ImGui.PushID(i);

                        if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                        {
                            var classOpen = ImGui.TreeNodeEx($@"{fieldName}: {i}", treeFlags);

                            ShowFieldHint(o, arrtyp, fieldDescription);

                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);

                            ImGui.Text("");
                            ImGui.NextColumn();

                            if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
                            {
                                var typeLabel = TypeLabel(o?.GetType() ?? arrtyp);

                                PropContextRowOpener("arrayTypeEntryCol");

                                ImGui.Text(typeLabel);

                                DisplayContextMenu(fieldName, fieldDescription, typeLabel);

                                ImGui.NextColumn();
                            }

                            if (classOpen)
                            {
                                // A drawn element is shown whole: it matched, or what holds it did
                                PropEditGeneric(implType, o, i, true);

                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.AlignTextToFramePadding();

                            PropGenericFieldRow(prop, arrtyp, classMeta, implType, o, obj, $@"{fieldName}[{i}]",
                                fieldDescription, arrayIndex: i, classIndex: classIndex);
                        }

                        ImGui.PopID();
                    }

                    ImGui.TreePop();
                }
            }
            else if (typ.IsGenericType && typ.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type arrtyp = typ.GetGenericArguments()[0];
                PropEditorGenericList(classMeta, implType, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex, selfMatch);
            }
            else if (typ.BaseType != null && typ.BaseType.IsGenericType
                && typ.BaseType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type arrtyp = typ.BaseType.GetGenericArguments()[0];
                PropEditorGenericList(classMeta, implType, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex, selfMatch);
            }
            // Class Line
            else if (typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                if (value != null)
                {
                    var open = ImGui.TreeNodeEx($"{fieldName}", treeFlags);

                    ShowFieldHint(value, typ, fieldDescription);

                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);
                    ImGui.Text("");

                    ImGui.NextColumn();

                    if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
                    {
                        var typeLabel = TypeLabel(value.GetType());

                        PropContextRowOpener("classTypeCol");

                        ImGui.Text(typeLabel);

                        DisplayContextMenu(fieldName, fieldDescription, typeLabel);

                        ImGui.NextColumn();
                    }

                    // Class properties
                    if (open)
                    {
                        PropEditGeneric(implType, value, -1, selfMatch);
                        ImGui.TreePop();
                    }
                }
            }
            // Property Line
            else
            {
                PropGenericFieldRow(prop, typ, classMeta, implType, value, obj, fieldName, fieldDescription,
                    classIndex: classIndex);
            }

            ImGui.PopID();

            id++;
        }
    }

    #region Search
    /// <summary>
    /// Properties never shown in the editor
    /// </summary>
    private static bool IsHidden(PropertyInfo prop)
    {
        if (prop.GetCustomAttribute<IgnoreInMaterialEditor>() != null)
            return true;

        // SoulsFile.Compression is inherited by MTD and MATBIN: compressing a file inside its binder breaks it
        if (prop.Name == nameof(MATBIN.Compression) && prop.PropertyType == typeof(DCX.Type))
            return true;

        // Indexers, which can't be read without an index
        if (prop.GetIndexParameters().Length > 0)
            return true;

        return false;
    }

    private bool IsFiltering => !string.IsNullOrWhiteSpace(PropertyListFilter);

    /// <summary>
    /// "val:" searches values, anything else searches field names and text values
    /// </summary>
    private bool IsValueSearch => PropertyListFilter.StartsWith("val:");

    private string SearchTerm => IsValueSearch ? PropertyListFilter["val:".Length..] : PropertyListFilter;

    private bool TermMatches(string text)
    {
        return EditorFilters.IsMatch(SearchTerm, text, ExactPropertyListFilter);
    }

    /// <summary>
    /// Whether a property matches the search by itself. A name search also matches text values, so a
    /// param, sampler or texture is found by its name or path.
    /// </summary>
    private bool RowMatches(string propName, string displayName, object value)
    {
        if (!IsFiltering)
            return true;

        if (IsValueSearch)
            return IsLeafValue(value) && TermMatches(ValueText(value));

        return TermMatches(propName) || TermMatches(displayName) || (value is string s && TermMatches(s));
    }

    /// <summary>
    /// Whether anything inside a list, array or class value matches the search
    /// </summary>
    private bool ChildrenMatch(object value, int depth)
    {
        if (!IsFiltering || value == null || value is string || depth > MaxSearchDepth)
            return false;

        if (value is IEnumerable enumerable)
        {
            foreach (var element in enumerable)
            {
                if (ElementMatches(element, depth + 1))
                    return true;
            }

            return false;
        }

        if (!value.GetType().IsClass)
            return false;

        return ObjectMatches(value, depth + 1);
    }

    /// <summary>
    /// Whether a list or array element matches the search, itself or anything inside it
    /// </summary>
    private bool ElementMatches(object element, int depth)
    {
        if (!IsFiltering)
            return true;

        if (element == null || depth > MaxSearchDepth)
            return false;

        if (IsLeafValue(element))
        {
            return IsValueSearch
                ? TermMatches(ValueText(element))
                : element is string s && TermMatches(s);
        }

        return ObjectMatches(element, depth);
    }

    private bool ObjectMatches(object obj, int depth)
    {
        foreach (var prop in View.MaterialPropertyCache.GetCachedProperties(obj.GetType()))
        {
            if (IsHidden(prop))
                continue;

            var value = prop.GetValue(obj);

            if (RowMatches(prop.Name, prop.Name, value) || ChildrenMatch(value, depth))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Values shown on one row: text, numbers, enums, vectors and arrays of those
    /// </summary>
    private static bool IsLeafValue(object value)
    {
        if (value == null)
            return false;

        if (value is string)
            return true;

        var type = value.GetType();

        if (type.IsArray)
            return type.GetElementType().IsValueType;

        return type.IsValueType;
    }

    private static string ValueText(object value)
    {
        if (value is Array array)
            return string.Join(", ", array.Cast<object>());

        return $"{value}";
    }
    #endregion

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

        ShowFieldHint(value, type, description);

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

        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);

        var oldval = value;
        object newval;

        var typeLabel = TypeLabel(value?.GetType() ?? type);

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

                DisplayContextMenu(name, description, typeLabel);

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

                DisplayContextMenu(name, description, typeLabel);

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
        else if (MaterialUtils.IsDerivedKey(containerObj, prop.Name) && value is uint key)
        {
            DisplayDerivedKey(prop, containerObj, key, name, description, typeLabel);
        }
        else
        {
            propEditResults = View.PropertyInput.PropertyRow(type, oldval, out newval, prop);

            var changed = propEditResults.Item1;
            var committed = propEditResults.Item2;

            DisplayContextMenu(name, description, typeLabel);

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

            PropContextRowOpener("typecol");

            ImGui.Text(typeLabel);
        }

        ImGui.NextColumn();
    }

    /// <summary>
    /// A MATBIN key, which follows its text (SourcePath, Name or Type) and so isn't typed in. A key that doesn't
    /// match its text, from an older edit or another tool, is flagged with a button to correct it.
    /// </summary>
    private void DisplayDerivedKey(PropertyInfo prop, object containerObj, uint key, string name, string description, string typeLabel)
    {
        var source = MaterialUtils.KeySourceProperty(containerObj);
        var sourceText = containerObj.GetType().GetProperty(source)?.GetValue(containerObj) as string;
        var expected = MaterialUtils.MatbinKey(sourceText);

        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled($"{key:X8}");
        GUI.Tooltip(LOC.Get("MAT_Properties_Derived_Key_TT", source));
        DisplayContextMenu(name, description, typeLabel);

        if (key != expected)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, $"{Icons.ExclamationTriangle}");
            GUI.Tooltip(LOC.Get("MAT_Properties_Stale_Key_TT", source, $"{expected:X8}"));

            ImGui.SameLine();
            if (ImGui.SmallButton($"{LOC.Get("MAT_Properties_Fix_Key")}##fixKey"))
            {
                var action = new MatPropertyChange(View, View.Selection.SelectedFileWrapper, prop, containerObj, expected);
                View.ActionManager.ExecuteAction(action);
            }
        }
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

        var propInfo = (PropertyInfo)prop;

        EditorAction action;

        if (MatParamTypeChange.Handles(obj, propInfo.Name))
            action = new MatParamTypeChange(View.Selection.SelectedFileWrapper, obj, newval);
        else if (MatKeyedTextChange.Handles(obj, propInfo.Name) && newval is string text)
            action = new MatKeyedTextChange(View.Selection.SelectedFileWrapper, obj, propInfo, text);
        else
            action = new MatPropertyChange(View, View.Selection.SelectedFileWrapper, propInfo, obj, newval, arrayindex, classIndex);

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
        int classIndex,
        bool showAll
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
                    // Not ParamType.None, which MTD.Param.Write can't write
                    newEntry = new MTD.Param("Untitled", MTD.ParamType.Float, 0f);
                }
                else if (newEntry == null && elementType == typeof(MATBIN.Param))
                {
                    newEntry = new MATBIN.Param();
                }

                // New MATBIN params and samplers otherwise have key 0, which matches no text
                MaterialUtils.ApplyMatbinKey(newEntry);

                var action = new MatAddListEntry(View.Selection.SelectedFileWrapper, prop, obj, newEntry, list.Count);
                View.ActionManager.ExecuteAction(action);
            }
            GUI.Tooltip(LOC.Get("EDITOR_PropEdit_Add_List_Entry_TT"));
        }

        ImGui.NextColumn();

        if (CFG.Current.MaterialEditor_Properties_Display_Type_Column)
        {
            var typeLabel = TypeLabel(prop.PropertyType);

            PropContextRowOpener("listTypeCol");

            ImGui.Text(typeLabel);

            DisplayContextMenu(fieldName, fieldDescription, typeLabel);

            ImGui.NextColumn();
        }

        if (open)
        {
            if (list != null)
            {
                var removeIndex = -1;

                for (var i = 0; i < list.Count; i++)
                {
                    var idx = i;
                    var elem = list[i];

                    if (!showAll && !ElementMatches(elem, 0))
                        continue;

                    ImGui.PushID(i);

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
                            var typeLabel = TypeLabel(elem?.GetType() ?? elementType);

                            PropContextRowOpener("listTypeCol");

                            ImGui.Text(typeLabel);

                            DisplayContextMenu(fieldName, fieldDescription, typeLabel);

                            ImGui.NextColumn();
                        }

                        if (classOpen)
                        {
                            // A drawn element is shown whole: it matched, or what holds it did
                            if (elem != null)
                                PropEditGeneric(implType, elem, idx, true);

                            ImGui.TreePop();
                        }
                    }
                    else
                    {
                        PropGenericFieldRow(prop, elementType, classMeta, implType, elem, obj, $@"{fieldName}[{i}]",
                            fieldDescription, arrayIndex: i, classIndex: classIndex, onRemove: OnRemove);
                    }

                    ImGui.PopID();
                }

                if (removeIndex != -1)
                {
                    var action = new MatRemoveListEntry(View.Selection.SelectedFileWrapper, prop, obj, removeIndex);
                    View.ActionManager.ExecuteAction(action);
                }
            }

            ImGui.TreePop();
        }
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

    private void DisplayContextMenu(string name, string description, string typeLabel)
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
                PlatformUtils.Instance.SetClipboardText(typeLabel);
            }
            GUI.Tooltip(LOC.Get("MAT_PropertyView_Context_Action_Copy_Type_TT"));

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// A readable name for a field's type: MATBIN.Param, List&lt;MATBIN.Param&gt;, Single[]
    /// </summary>
    private static string TypeLabel(Type type)
    {
        if (type == null)
            return "";

        if (type.IsArray)
            return $"{TypeLabel(type.GetElementType())}[]";

        var name = type.Name;

        if (type.IsGenericType)
        {
            name = $"{name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(TypeLabel))}>";
        }

        if (type.IsNested && type.DeclaringType != null)
        {
            name = $"{type.DeclaringType.Name}.{name}";
        }

        return name;
    }

    /// <summary>
    /// The tooltip for a field: its description, then what its type allows
    /// </summary>
    public void ShowFieldHint(object value, Type type, string description)
    {
        var text = description;

        if (value is Array a)
        {
            var str = LOC.Get("MAT_Properties_FieldHint_Array_Type", TypeLabel(a.GetType().GetElementType()));
            if (a.Length > 0)
            {
                str += LOC.Get("MAT_Properties_FieldHint_Array_Length", a.Length);
            }

            text = $"{text}\n{str}";
        }
        else if (value is string s)
        {
            var str = LOC.Get("MAT_Properties_FieldHint_String_Type", TypeLabel(typeof(string)));
            if (s.Length > 0)
            {
                str += LOC.Get("MAT_Properties_FieldHint_String_Length", s.Length);
            }

            text = $"{text}\n{str}";
        }
        else
        {
            var valueType = value?.GetType() ?? type;

            if (valueType != null && valueType.IsValueType)
            {
                var str = LOC.Get("MAT_Properties_FieldHint_Value_Type", TypeLabel(valueType));
                var min = valueType.GetField("MinValue")?.GetValue(null);
                var max = valueType.GetField("MaxValue")?.GetValue(null);
                if (min != null && max != null)
                {
                    str += LOC.Get("MAT_Properties_FieldHint_Value_Min_Max", min, max);
                }

                text = $"{text}\n{str}";
            }
        }

        text = text.Trim();

        // Final description
        if (text != "")
        {
            GUI.Tooltip(text);
        }
    }
}
