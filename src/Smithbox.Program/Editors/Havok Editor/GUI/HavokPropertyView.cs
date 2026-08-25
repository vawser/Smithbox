using Hexa.NET.ImGui;
using HKLib.hk2018;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections;
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

        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            DisplayHeader();
            DisplayPropertyEditor(data.AnimationBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            if (View.PropertyView.BehaviorView.IsBehaviorGraph &&
                View.Selection.PropertyViewType is HavokPropertyViewType.Structured)
            {
                BehaviorView.DisplayHeader();
            }
            else
            {
                DisplayHeader();
            }

            DisplayPropertyEditor(data.BehaviorBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            DisplayHeader();
            DisplayPropertyEditor(data.CharacterBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            DisplayHeader();
            DisplayPropertyEditor(data.MapCollisionBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            DisplayHeader();
            DisplayPropertyEditor(data.AssetCollisionBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            DisplayHeader();
            DisplayPropertyEditor(data.NavmeshBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            DisplayHeader();
            DisplayPropertyEditor(data.CutsceneBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            DisplayHeader();
            DisplayPropertyEditor(data.PartBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            DisplayHeader();
            DisplayPropertyEditor(data.RumbleBank);
        }
        else
        {
            DisplayHeader();

            ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("HAVOK_PropertyView_No_Internal_File_Selected"));

            ImGui.EndChild();
        }
    }

    public void DisplayHeader()
    {
        GUI.DisplayHeader("headerSection_HavokEditor");

        EditorFilters.DisplaySearchbar("propSearch_HavokEditor", ref PropFilter, ref ExactPropFilter);

        // Toggle: Community Names
        GUI.DisplayToggleButton("communityNameToggle", Icons.Book,
            ref CFG.Current.HavokEditor_Properties_Display_Community_Names,
            "HAVOK_PropertyView_Field_Name_Toggle_Internal",
            "HAVOK_PropertyView_Field_Name_Toggle_Community",
            "HAVOK_PropertyView_Field_Name_Toggle_TT");

        // Toggle: Type Column
        GUI.DisplayToggleButton("typeColToggle", Icons.Calculator,
            ref CFG.Current.HavokEditor_Properties_Display_Type_Column,
            "HAVOK_PropertyView_Type_Column_Toggle_Hide",
            "HAVOK_PropertyView_Type_Column_Toggle_Show",
            "HAVOK_PropertyView_Type_Column_Toggle_TT");

        // Toggle: Mesh Data
        GUI.DisplayToggleButton("meshDataToggle", Icons.Database,
            ref CFG.Current.HavokEditor_Properties_Display_Raw_Data_Fields,
            "HAVOK_PropertyView_Mesh_Data_Toggle_Hide",
            "HAVOK_PropertyView_Mesh_Data_Toggle_Show",
            "HAVOK_PropertyView_Mesh_Data_Toggle_TT");

        // Toggle: Tree Auto-Open
        GUI.DisplayToggleButton("treeAutoOpenToggle", Icons.Tree,
            ref CFG.Current.HavokEditor_Properties_Auto_Open_Tree,
            "HAVOK_PropertyView_TreeState_Open",
            "HAVOK_PropertyView_TreeState_Closed",
            "HAVOK_PropertyView_TreeState_TT");

        // Toggle: Property Bags
        GUI.DisplayToggleButton("propBagToggle", Icons.ShoppingBag,
            ref CFG.Current.HavokEditor_Properties_Display_Property_Bags,
            "HAVOK_PropertyView_PropBag_Hide",
            "HAVOK_PropertyView_PropBag_Show",
            "HAVOK_PropertyView_PropBag_TT");

        // Property View Type
        if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            if(View.Selection.PropertyViewType is HavokPropertyViewType.Flat)
            {
                ImGui.SameLine();
                BehaviorView.DisplayHeader(true);
            }
        }

        GUI.EndHeader();
    }

    public void Shortcuts()
    {

    }

    public void DisplayPropertyEditor(Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict)
    {
        if (View.Selection.BinderFileEntry == null)
        {
            ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("HAVOK_PropertyView_No_Source_File_Selected"));

            ImGui.EndChild();

            return;
        }

        if (!bankDict.ContainsKey(View.Selection.BinderFileEntry))
        {
            ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("HAVOK_PropertyView_Bank_Missing_File", View.Selection.BinderFileEntry.Path));

            ImGui.EndChild();

            return;
        }

        if (View.Selection.FilePath == null)
        {
            ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("HAVOK_PropertyView_No_Internal_File_Selected"));

            ImGui.EndChild();

            return;
        }

        if (!bankDict[View.Selection.BinderFileEntry].ContainsKey(View.Selection.FilePath))
        {
            ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("HAVOK_PropertyView_Binder_Missing_File", View.Selection.FilePath));

            ImGui.EndChild();

            return;
        }

        if (View.Selection.PropertyViewType is HavokPropertyViewType.Flat)
        {
            ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

            var sourceObject = bankDict[View.Selection.BinderFileEntry][View.Selection.FilePath];

            if (sourceObject != null)
            {
                View.Selection.ApplyFileSpecificTreeSearches(sourceObject);

                HavokPropEdit(sourceObject);
            }
            else
            {
                GUI.WrappedText(LOC.Get("HAVOK_PropertyView_File_Not_Loaded"));
            }

            ImGui.EndChild();
        }
        else
        {
            var sourceObject = bankDict[View.Selection.BinderFileEntry][View.Selection.FilePath];

            if (sourceObject != null)
            {
                View.Selection.ApplyFileSpecificTreeSearches(sourceObject);

                if (View.PropertyView.BehaviorView.IsBehaviorGraph)
                {
                    BehaviorView.Draw(sourceObject);
                }
                else
                {
                    ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

                    HavokPropEdit(sourceObject);

                    ImGui.EndChild();
                }
            }
            else
            {
                ImGui.BeginChild("havokPropEditSection", ImGuiChildFlags.Borders);

                GUI.WrappedText(LOC.Get("HAVOK_PropertyView_File_Not_Loaded"));

                ImGui.EndChild();
            }
        }
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
        ImGui.Text(LOC.Get("HAVOK_PropertyView_Field"));

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

    public void HavokPropEditGeneric(object obj, HavokClass havokMeta, int classIndex = -1)
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

            Type typ = prop.FieldType;

            if (!CFG.Current.HavokEditor_Properties_Display_Property_Bags)
            {
                if (typ == typeof(hkPropertyBag))
                    continue;
            }

            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();

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
                Type arrtyp = typ.GetGenericArguments()[0];
                PropEditorGenericList(havokMeta, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex);
            }
            else if (typ.BaseType != null && typ.BaseType.IsGenericType
                && typ.BaseType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type arrtyp = typ.BaseType.GetGenericArguments()[0];
                PropEditorGenericList(havokMeta, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex);
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
            if (ImGui.Button("-##removeListEntry"))
            {
                onRemove();
            }
            GUI.Tooltip("Remove this entry from the list.");
        }

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

    private void PropContextRowOpener(string id)
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
            // Copy Field Name
            if (ImGui.Selectable($"{LOC.Get("HAVOK_PropertyView_Context_Action_Copy_Name")}##CopyPropName"))
            {
                PlatformUtils.Instance.SetClipboardText(name);
            }
            GUI.Tooltip(LOC.Get("HAVOK_PropertyView_Context_Action_Copy_Name_TT"));

            // Copy Field Description
            if (ImGui.Selectable($"{LOC.Get("HAVOK_PropertyView_Context_Action_Copy_Description")}##CopyPropDesc"))
            {
                PlatformUtils.Instance.SetClipboardText(description);
            }
            GUI.Tooltip(LOC.Get("HAVOK_PropertyView_Context_Action_Copy_Description_TT"));

            // Copy Field Type
            if (ImGui.Selectable($"{LOC.Get("HAVOK_PropertyView_Context_Action_Copy_Type")}##CopyPropType"))
            {
                var reflectedType = prop.ReflectedType;
                if (reflectedType != null)
                {
                    PlatformUtils.Instance.SetClipboardText(reflectedType.FullName);
                }
            }
            GUI.Tooltip(LOC.Get("HAVOK_PropertyView_Context_Action_Copy_Type_TT"));

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

    private void PropEditorGenericList(
        HavokClass havokMeta,
        object obj,
        FieldInfo prop,
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
            if (ImGui.Button("+##addListEntry"))
            {
                var newEntry = PropFinderUtil.CreateDefaultListElement(elementType);
                var action = new HavokAddListEntryAction(prop, obj, newEntry, list.Count);
                View.ActionManager.ExecuteAction(action);
            }
            GUI.Tooltip("Add a new entry to the end of this list.");
        }
        ImGui.NextColumn();

        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
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
                        if (ImGui.Button("-##removeListEntry"))
                        {
                            OnRemove();
                        }
                        GUI.Tooltip("Remove this entry from the list.");

                        ImGui.NextColumn();

                        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
                        {
                            PropContextRowOpener("listTypeCol");

                            ImGui.Text(elem?.GetType().FullName);

                            DisplayContextMenu(fieldName, fieldDescription, prop);

                            ImGui.NextColumn();
                        }

                        if (classOpen)
                        {
                            if (elem != null)
                                HavokPropEditGeneric(elem, havokMeta, idx);

                            ImGui.TreePop();
                        }
                    }
                    else
                    {
                        // Handle property display (and search filtering)
                        if (DisplayProperty(havokMeta, obj, prop, elementType))
                        {
                            PropGenericFieldRow(prop, elementType, havokMeta, elem, obj, $@"{fieldName}[{i}]", fieldDescription, i, classIndex, OnRemove);
                        }
                    }

                    ImGui.PopID();
                }

                if (removeIndex != -1)
                {
                    var action = new HavokRemoveListEntryAction(prop, obj, removeIndex);
                    View.ActionManager.ExecuteAction(action);
                }
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }
}