using CsvHelper;
using Hexa.NET.ImGui;
using HKLib.hk2018;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Utilities;
using System.Drawing;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

public class HavokPropertyView
{
    private HavokEditorView View;
    private ProjectEntry Project;

    private object _changingProperty;
    private EditorAction _lastUncommittedAction;

    public string PropFilter = "";
    public bool ExactPropFilter = false;

    public HavokBehaviorView BehaviorView;

    public HavokPropertyView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        BehaviorView = new(view, project);
    }

    public void Draw()
    {
        GUI.SimpleHeader(
            LOC.Get("HAVOK_PropertyView_Header"),
            LOC.Get("HAVOK_PropertyView_Header_TT"));

        DisplayHeader();

        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            DisplayPropertyEditor(data.AnimationBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            DisplayPropertyEditor(data.BehaviorBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            DisplayPropertyEditor(data.CharacterBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            DisplayPropertyEditor(data.MapCollisionBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            DisplayPropertyEditor(data.AssetCollisionBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            DisplayPropertyEditor(data.NavmeshBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            DisplayPropertyEditor(data.CutsceneBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            DisplayPropertyEditor(data.PartBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            DisplayPropertyEditor(data.RumbleBank);
        }
        else
        {
            ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

            GUI.WrappedText($"No internal file has been selected.");

            ImGui.EndChild();
        }
    }

    public void DisplayHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedList_HavokProperties", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("havokPropSearch", ref PropFilter, ref ExactPropFilter);

        // Toggle Community Field Names
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Book}", DPI.IconButtonSize))
        {
            CFG.Current.HavokEditor_Properties_Display_Community_Names = !CFG.Current.HavokEditor_Properties_Display_Community_Names;
        }

        var communityFieldNameMode = "Internal";
        if (CFG.Current.HavokEditor_Properties_Display_Community_Names)
            communityFieldNameMode = "Community";

        GUI.Tooltip($"Toggle field name display type between Internal and Community.\nCurrent Mode: {communityFieldNameMode}");

        // Type Column
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Calculator}##toggleTypeCol"))
        {
            CFG.Current.HavokEditor_Properties_Display_Type_Column = !CFG.Current.HavokEditor_Properties_Display_Type_Column;
        }

        var typeColumnVis = "Internal";
        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            typeColumnVis = "Community";

        GUI.Tooltip($"Toggle the visibilty of the field type column.\nCurrent Mode: {typeColumnVis}");

        // Raw Data Fields
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Database}##toggleRawDataFields"))
        {
            CFG.Current.HavokEditor_Properties_Display_Raw_Data_Fields = !CFG.Current.HavokEditor_Properties_Display_Raw_Data_Fields;
        }

        var rawDataVis = "Hide Mesh Data";
        if (CFG.Current.HavokEditor_Properties_Display_Raw_Data_Fields)
            rawDataVis = "Show Mesh Data";

        GUI.Tooltip($"Toggle the visibilty of fields tagged as 'mesh data'.\nCurrent Mode: {rawDataVis}");

        // Auto-Open Tree
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Tree}##toggleAutoOpen"))
        {
            CFG.Current.HavokEditor_Properties_Auto_Open_Tree = !CFG.Current.HavokEditor_Properties_Auto_Open_Tree;
        }

        var autoTreeMode = "Tree Nodes are opened automatically.";
        if (CFG.Current.HavokEditor_Properties_Auto_Open_Tree)
            autoTreeMode = "Tree Nodes require the user to open them.";

        GUI.Tooltip($"Toggle the opening behavior of tree nodes.\nCurrent Mode: {autoTreeMode}");

        // Special Property View Mode for Behavior
        if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            ImGui.SameLine();

            var previewName = LOC.Get(View.Selection.PropertyViewType.GetDisplayName());

            ImGui.SetNextItemWidth(100f * DPI.UIScale());
            if (ImGui.BeginCombo("##subEditorMode", previewName))
            {
                foreach (var entry in Enum.GetValues(typeof(HavokPropertyViewType)))
                {
                    var curType = (HavokPropertyViewType)entry;

                    var displayName = LOC.Get(curType.GetDisplayName());

                    if (ImGui.Selectable(displayName, curType == View.Selection.PropertyViewType))
                    {
                        View.Selection.PropertyViewType = curType;
                    }
                }

                ImGui.EndCombo();
            }
        }

        ImGui.EndChild();
    }

    public void DisplayPropertyEditor(Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict)
    {
        if (View.Selection.BinderFileEntry == null)
        {
            ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

            GUI.WrappedText($"No source file has been selected.");

            ImGui.EndChild();

            return;
        }

        if (!bankDict.ContainsKey(View.Selection.BinderFileEntry))
        {
            ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

            GUI.WrappedText($"Bank does not contain a file entry with this path:\n{View.Selection.BinderFileEntry.Path}");

            ImGui.EndChild();

            return;
        }

        if (View.Selection.FilePath == null)
        {
            ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

            GUI.WrappedText($"No internal file has been selected.");

            ImGui.EndChild();

            return;
        }

        if (!bankDict[View.Selection.BinderFileEntry].ContainsKey(View.Selection.FilePath))
        {
            ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

            GUI.WrappedText($"Binder does not contain a file with this path:\n{View.Selection.FilePath}");

            ImGui.EndChild();

            return;
        }

        ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

        if (View.Selection.PropertyViewType is HavokPropertyViewType.Flat)
        {
            var sourceObject = bankDict[View.Selection.BinderFileEntry][View.Selection.FilePath];

            if (sourceObject != null)
            {
                View.Selection.ApplyFileSpecificTreeSearches(sourceObject);

                HavokPropEdit(sourceObject);
            }
            else
            {
                GUI.WrappedText($"File has not been loaded yet.");
            }
        }
        else
        {
            var sourceObject = bankDict[View.Selection.BinderFileEntry][View.Selection.FilePath];

            if (sourceObject != null)
            {
                View.Selection.ApplyFileSpecificTreeSearches(sourceObject);

                if (View.Selection.IsBehaviorGraph)
                {
                    BehaviorView.Draw(sourceObject);
                }
                else
                {
                    HavokPropEdit(sourceObject);
                }
            }
            else
            {
                GUI.WrappedText($"File has not been loaded yet.");
            }
        }

        ImGui.EndChild();
    }

    public void HavokPropEdit(hkRootLevelContainer root)
    {
        var type = root.GetType();

        var columnCount = 2;
        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
            columnCount = 3;
        }

        ImGui.Columns(columnCount);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Object Type");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Text(type.Name);

        ImGui.NextColumn();

        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
            ImGui.NextColumn();
        }

        var havokMeta = HavokMetaHelper.GetMeta(Project, type);

        HavokPropEditGeneric(root, havokMeta);

        ImGui.Columns(1);
    }

    private void HavokPropEditGeneric(object obj, HavokClass havokMeta, int classIndex = -1)
    {
        if (obj == null)
            return;

        var scale = DPI.UIScale();
        Type type = obj.GetType();

        FieldInfo[] properties = View.PropertyCache.GetCachedHavokFields(type);

        // Properties
        var id = 0;
        foreach (FieldInfo prop in properties)
        {
            havokMeta = HavokMetaHelper.GetMeta(Project, type);

            var treeFlags = ImGuiTreeNodeFlags.None;

            if(CFG.Current.HavokEditor_Properties_Auto_Open_Tree)
            {
                treeFlags = ImGuiTreeNodeFlags.DefaultOpen;
            }

            // Field Name
            var fieldName = prop.Name;
            var fieldDescription = "";

            if (havokMeta != null)
            {
                if (CFG.Current.HavokEditor_Properties_Display_Community_Names)
                {
                    fieldName = HavokMetaHelper.GetFieldName(havokMeta, prop.Name);
                }

                fieldDescription = $"{HavokMetaHelper.GetFieldDescription(havokMeta, prop.Name)}";
            }

            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();
            Type typ = prop.FieldType;

            if (typ.IsArray)
            {
                var a = (Array)prop.GetValue(obj);
                var open = ImGui.TreeNodeEx($@"{fieldName}", treeFlags);
                GUI.Tooltip(fieldDescription);
                ImGui.NextColumn();
                ImGui.NextColumn();
                if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
                {
                    PropContextRowOpener("arrayTypeCol");

                    ImGui.Text(type.FullName);

                    DisplayContextMenu(fieldName, fieldDescription, prop);

                    ImGui.NextColumn();
                }

                if (open)
                {
                    for (var i = 0; i < a.Length; i++)
                    {
                        ImGui.PushID(i);
                        Type arrtyp = typ.GetElementType();
                        if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                        {
                            var classOpen = ImGui.TreeNodeEx($@"{fieldName}: {i}", treeFlags);
                            GUI.Tooltip(fieldDescription);
                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);
                            var o = a.GetValue(i);
                            ImGui.Text("");
                            ImGui.NextColumn();
                            if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
                            {
                                PropContextRowOpener("arrayTypeEntryCol");

                                ImGui.Text(type.FullName);

                                DisplayContextMenu(fieldName, fieldDescription, prop);

                                ImGui.NextColumn();
                            }

                            if (classOpen)
                            {
                                HavokPropEditGeneric(o, havokMeta, i);
                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.AlignTextToFramePadding();
                            var array = obj as object[];

                            // Handle property display (and search filtering)
                            if (DisplayProperty(havokMeta, obj, prop, type))
                            {
                                PropGenericFieldRow(prop, typ.GetElementType(), havokMeta, a.GetValue(i), obj, $@"{fieldName}[{i}]", fieldDescription, i, classIndex);
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
                            var open = ImGui.TreeNodeEx($@"{fieldName}: {i}", treeFlags);
                            GUI.Tooltip(fieldDescription);
                            ImGui.NextColumn();
                            ImGui.SetNextItemWidth(-1);
                            var o = itemprop.GetValue(l, new object[] { i });
                            ImGui.Text("");
                            ImGui.NextColumn();
                            if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
                            {
                                PropContextRowOpener("listTypeCol");

                                ImGui.Text(type.FullName);

                                DisplayContextMenu(fieldName, fieldDescription, prop);

                                ImGui.NextColumn();
                            }

                            if (open)
                            {
                                HavokPropEditGeneric(o, havokMeta);
                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            // Handle property display (and search filtering)
                            if (DisplayProperty(havokMeta, obj, prop, type))
                            {
                                PropGenericFieldRow(prop, arrtyp, havokMeta, itemprop.GetValue(l, new object[] { i }), obj, $@"{fieldName}[{i}]", fieldDescription, i, classIndex);
                            }
                        }
                        ImGui.PopID();
                    }
                }

                ImGui.PopID();
            }
            else if (typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                var o = prop.GetValue(obj);
                if (o != null)
                {
                    var open = ImGui.TreeNodeEx($"{fieldName}", treeFlags);
                    GUI.Tooltip(fieldDescription);
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);
                    ImGui.Text("");
                    ImGui.NextColumn();
                    if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
                    {
                        PropContextRowOpener("classTypeCol");

                        ImGui.Text(type.FullName);

                        DisplayContextMenu(fieldName, fieldDescription, prop);

                        ImGui.NextColumn();
                    }

                    if (open)
                    {
                        HavokPropEditGeneric(o, havokMeta);
                        ImGui.TreePop();
                    }
                }

                ImGui.PopID();
            }
            else
            {
                // Handle property display (and search filtering)
                if (DisplayProperty(havokMeta, obj, prop, type))
                {
                    PropGenericFieldRow(prop, typ, havokMeta, prop.GetValue(obj), obj, fieldName, fieldDescription, classIndex);
                }

                ImGui.PopID();
            }

            id++;
        }
    }

    public bool DisplayProperty(HavokClass havokMeta, object propObj, FieldInfo prop, Type type)
    {
        var propName = prop.Name;

        if (havokMeta != null)
        {
            var isRawData = HavokMetaHelper.IsRawData(havokMeta, prop.Name);

            if (!CFG.Current.HavokEditor_Properties_Display_Raw_Data_Fields)
            {
                if (isRawData)
                {
                    return false;
                }
            }
        }

        // Normal filter
        var isMatch = EditorFilters.IsMatch(PropFilter, propName, ExactPropFilter);
        var isValueMatch = false;

        if (PropFilter.StartsWith("val:"))
            isValueMatch = true;

        if (!isMatch && !isValueMatch)
        {
            return false;
        }
        else if (isValueMatch)
        {
            // TODO: currently doesn't match correctly with array list values
            var valStr = PropFilter.Replace("val:", "");

            var propVal = prop.GetValue(propObj);

            if (propVal != null)
            {
                var value = $"{propVal}";

                if (ExactPropFilter)
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
        FieldInfo prop,
        Type type,
        HavokClass havokMeta,
        object value,
        object containerObj,
        string name,
        string description,
        int arrayIndex = -1,
        int classIndex = -1
    )
    {
        PropContextRowOpener("nameCol");

        ImGui.Text(name);
        GUI.Tooltip(description);

        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);

        var oldval = value;
        object newval;

        // Property Editor UI
        (bool, bool) propEditResults = PropertyRow(type, oldval, out newval, prop);
        var changed = propEditResults.Item1;
        var committed = propEditResults.Item2;

        DisplayContextMenu(name, description, prop);

        if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
        {
            ImGui.SetItemDefaultFocus();
        }

        if (HavokPropertyDecorators.ParamRefRow(View, havokMeta, prop, oldval, ref newval))
        {
            changed = true;
            committed = true;
        }

        UpdateProperty(prop, containerObj, oldval, newval, changed, committed, arrayIndex, classIndex);

        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
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

    private static void PropContextRowOpener(string id)
    {
        ImGui.Selectable($"###{id}", false, ImGuiSelectableFlags.AllowOverlap);
        ImGui.SameLine();
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("HavokPropertiesContextMenu");
        }
    }

    private void DisplayContextMenu(string name, string description, FieldInfo prop)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("HavokPropertiesContextMenu");
        }

        if (ImGui.BeginPopup("HavokPropertiesContextMenu"))
        {
            if (ImGui.Selectable(@"Copy Property Name##CopyPropName"))
            {
                PlatformUtils.Instance.SetClipboardText(name);
            }

            if (ImGui.Selectable(@"Copy Property Description##CopyPropDesc"))
            {
                PlatformUtils.Instance.SetClipboardText(description);
            }

            if (ImGui.Selectable(@"Copy Property Type##CopyPropType"))
            {
                var reflectedType = prop.ReflectedType;
                if (reflectedType != null)
                {
                    PlatformUtils.Instance.SetClipboardText(reflectedType.FullName);
                }
            }

            ImGui.EndPopup();
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

        var action = new HavokPropChange(View, (FieldInfo)prop, obj, newval, arrayindex, classIndex);
        View.ActionManager.ExecuteAction(action);

        _lastUncommittedAction = action;
        _changingProperty = prop;
    }

    private (bool, bool) PropertyRow(Type typ, object oldval, out object newval, FieldInfo prop)
    {
        ImGui.SetNextItemWidth(-1);

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
        else if (typ == typeof(ulong))
        {
            var val = (ulong)oldval;
            var strval = $@"{val}";

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                var res = ulong.TryParse(newValue, out val);
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

            if (ImGui.InputInt("##value", ref val))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(uint))
        {
            var val = (uint)oldval;
            var strval = $@"{val}";

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
        else if (typ == typeof(short))
        {
            int val = (short)oldval;

            if (ImGui.InputInt("##value", ref val))
            {
                newval = (short)val;
                isChanged = true;
            }
        }
        else if (typ == typeof(ushort))
        {
            var val = (ushort)oldval;
            var strval = $@"{val}";

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
        else if (typ == typeof(sbyte))
        {
            int val = (sbyte)oldval;

            if (ImGui.InputInt("##value", ref val))
            {
                newval = (sbyte)val;
                isChanged = true;
            }
        }
        else if (typ == typeof(byte))
        {
            var val = (byte)oldval;
            var strval = $@"{val}";

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
        else if (typ == typeof(Vector4))
        {
            var val = (Vector4)oldval;

            if (ImGui.DragFloat4("##value", ref val, 0.1f))
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
            ImGui.Text($"ImplementMe: {prop.FieldType}");
        }

        var isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive();

        return (isChanged, isDeactivatedAfterEdit);
    }
}