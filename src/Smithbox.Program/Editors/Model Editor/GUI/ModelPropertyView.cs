using Hexa.NET.ImGui;
using HKLib.hk2018.hkHashMapDetail;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Utilities;
using System.Collections;
using System.Drawing;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.ModelEditor;

public class ModelPropertyView
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public ModelPropertyView(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private object _changingObject;
    private object _changingPropery;

    private ViewportAction _lastUncommittedAction;
    public PropertyInfo RequestedSearchProperty = null;

    public bool Focus = false;

    private string PropertyListFilter = "";
    private bool ExactPropertyListFilter = false;

    public void Display()
    {
        DisplayHeader();
        DisplayPropertyEditor();
    }

    public void DisplayHeader()
    {
        GUI.DisplayHeader("headerSection_ModelEditor");

        EditorFilters.DisplaySearchbar("propSearch_ModelEditor", ref PropertyListFilter, ref ExactPropertyListFilter);

        // Toggle: Community Names
        GUI.DisplayToggleButton("communityNameToggle", Icons.Book, 
            ref CFG.Current.ModelEditor_Properties_Enable_Commmunity_Names,
            "MODEL_Properties_Toggle_Community_Names_Internal",
            "MODEL_Properties_Toggle_Community_Names_Community",
            "MODEL_Properties_Toggle_Community_Names_TT");

        // Toggle: Type Column
        GUI.DisplayToggleButton("typeColToggle", Icons.Calculator, 
            ref CFG.Current.ModelEditor_Properties_Enable_Type_Column,
            "MODEL_Properties_Toggle_Type_Col_Hide",
            "MODEL_Properties_Toggle_Type_Col_Show",
            "MODEL_Properties_Toggle_Type_Col_TT");

        // Toggle: Mesh Data
        GUI.DisplayToggleButton("meshDataToggle", Icons.Database,
            ref CFG.Current.ModelEditor_Properties_Enable_Mesh_Fields,
            "MODEL_PropertyView_Mesh_Data_Toggle_Hide",
            "MODEL_PropertyView_Mesh_Data_Toggle_Show",
            "MODEL_PropertyView_Mesh_Data_Toggle_TT");

        // Toggle: Tree Auto-Open
        GUI.DisplayToggleButton("treeAutoOpenToggle", Icons.Tree,
            ref CFG.Current.ModelEditor_Properties_Auto_Open_Tree,
            "MODEL_PropertyView_TreeState_Open",
            "MODEL_PropertyView_TreeState_Closed",
            "MODEL_PropertyView_TreeState_TT");

        GUI.EndHeader();
    }

    public void DisplayPropertyEditor()
    {
        if (View.Selection.SelectedModelWrapper == null)
        {
            ImGui.BeginChild("propertyEditorSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("MODEL_Properties_No_Selection"));

            ImGui.EndChild();

            return;
        }

        if (View.Selection.SelectedModelWrapper.Container == null)
        {
            ImGui.BeginChild("propertyEditorSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("MODEL_Properties_No_Selection"));

            ImGui.EndChild();

            return;
        }

        HashSet<Entity> entSelection = View.ViewportSelection.GetFilteredSelection<Entity>();

        ImGui.BeginChild("propertyEditorSection", ImGuiChildFlags.Borders);

        if (entSelection.Count > 1)
        {
            Entity firstEnt = entSelection.First();

            ImGui.TextColored(new Vector4(0.5f, 1.0f, 0.0f, 1.0f), LOC.Get("MODEL_Properties_Multi_Edit"));

            ImGui.Separator();
            ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_MultipleInput_Background);
            ImGui.BeginChild("Model_EditingMultipleObjsChild");

            PropertyTreeHeader(View.ViewportSelection);

            ImGui.PopStyleColor();
            ImGui.EndChild();
        }
        else if (entSelection.Any())
        {
            Entity firstEnt = entSelection.First();

            if (firstEnt.WrappedObject == null)
            {
                ImGui.Text(LOC.Get("MODEL_Properties_Select_to_Edit"));
                ImGui.EndChild();
                ImGui.End();
                ImGui.PopStyleColor(2);
                return;
            }

            PropertyTreeHeader(View.ViewportSelection);
        }
        else
        {
            ImGui.Text(LOC.Get("MODEL_Properties_No_Selection"));
        }

        ImGui.EndChild();
    }

    private void PropertyTreeHeader(ViewportSelection selection, int classIndex = -1)
    {
        var entities = selection.GetFilteredSelection<ModelEntity>();
        var types = entities.Select(t => t.WrappedObject.GetType()).Distinct();
        var first = entities.First();
        var type = types.First();

        if (types.Count() > 1)
        {
            return;
        }

        var objType = first.WrappedObject.GetType();

        var metaType = "FLVER";
        if(first.WrappedObject is CLM2)
        {
            metaType = "CLM2";
        }
        else if (first.WrappedObject is HKXPWV)
        {
            metaType = "HKXPWV";
        }
        else if (first.WrappedObject is EDGE)
        {
            metaType = "EDGE";
        }
        else if (first.WrappedObject is GRASS)
        {
            metaType = "GRASS";
        }

        var meta = ModelMetaHelper.GetMeta(Project, type, metaType);

        var columnCount = 2;
        if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
        {
            columnCount = 3;
        }

        ImGui.Columns(columnCount);

        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get("MODEL_Properties_Col_Object_Type"));

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Text(type.Name);

        ImGui.NextColumn();

        if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
        {
            ImGui.NextColumn();
        }

        PropEditorGeneric(meta, metaType, selection, entities, entities.First().WrappedObject, classIndex);

        ImGui.Columns(1);
    }

    private void PropEditorGeneric(
        ModelClass classMeta,
        string metaType,
        ViewportSelection selection,
        IEnumerable<Entity> entSelection,
        object obj,
        int classIndex = -1
    )
    {
        if (obj == null)
            return;

        var scale = DPI.UIScale();
        Entity firstEnt = entSelection.First();
        Type type = obj.GetType();

        PropertyInfo[] properties = View.ModelPropertyCache.GetCachedProperties(type)
            .Where(p => p.GetIndexParameters().Length == 0)
            .ToArray();

        // Properties
        var id = 0;
        foreach (PropertyInfo prop in properties)
        {
            classMeta = ModelMetaHelper.GetMeta(Project, type, metaType);

            var treeFlags = ImGuiTreeNodeFlags.None;

            if (CFG.Current.ModelEditor_Properties_Auto_Open_Tree)
            {
                treeFlags = ImGuiTreeNodeFlags.DefaultOpen;
            }

            // Field Name
            var fieldName = prop.Name;
            var fieldDescription = "";

            if (classMeta != null)
            {
                if (CFG.Current.ModelEditor_Properties_Enable_Commmunity_Names)
                {
                    fieldName = ModelMetaHelper.GetFieldName(classMeta, prop.Name);
                }

                fieldDescription = $"{ModelMetaHelper.GetFieldDescription(classMeta, prop.Name)}";
            }

            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();

            Type typ = prop.PropertyType;

            if (typ.IsArray)
            {
                var a = (Array)prop.GetValue(obj);
                var open = ImGui.TreeNodeEx($@"{fieldName}s", treeFlags);
                GUI.Tooltip(fieldDescription);
                ImGui.NextColumn();
                ImGui.NextColumn();
                if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
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

                            if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
                            {
                                PropContextRowOpener("arrayTypeEntryCol");

                                ImGui.Text(type.FullName);

                                DisplayContextMenu(fieldName, fieldDescription, prop);

                                ImGui.NextColumn();
                            }

                            if (classOpen)
                            {
                                PropEditorGeneric(classMeta, metaType, selection, entSelection, o, i);
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
                                PropGenericFieldRow(classMeta, selection, entSelection, prop, typ.GetElementType(), a.GetValue(i), $@"{fieldName}[{i}]", fieldDescription, i, classIndex);
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
                PropEditorGenericList(classMeta, metaType, selection, entSelection, firstEnt, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex);
            }
            else if (typ.BaseType != null && typ.BaseType.IsGenericType
                && typ.BaseType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type arrtyp = typ.BaseType.GetGenericArguments()[0];
                PropEditorGenericList(classMeta, metaType, selection, entSelection, firstEnt, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex);
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
                    if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
                    {
                        PropContextRowOpener("classTypeCol");

                        ImGui.Text(type.FullName);

                        DisplayContextMenu(fieldName, fieldDescription, prop);

                        ImGui.NextColumn();
                    }

                    if (open)
                    {
                        PropEditorGeneric(classMeta, metaType, selection, entSelection, o);
                        ImGui.TreePop();
                    }
                }

                ImGui.PopID();
            }
            else
            {
                // Handle property display (and search filtering)
                if (DisplayProperty(classMeta, obj, prop, type))
                {
                    PropGenericFieldRow(classMeta, selection, entSelection, prop, typ, prop.GetValue(obj), $"{fieldName}", fieldDescription, classIndex);
                }

                ImGui.PopID();
            }

            id++;
        }
    }

    private void PropGenericFieldRow(
        ModelClass classMeta,
        ViewportSelection selection,
        IEnumerable<Entity> entSelection,
        PropertyInfo prop,
        Type type,
        object obj,
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

        var oldval = obj;
        object newval;

        // Property Editor UI
        (bool, bool) propEditResults = PropertyRow(type, oldval, out newval, prop, entSelection);

        var changed = propEditResults.Item1;
        var committed = propEditResults.Item2;

        DisplayContextMenu(name, description, prop);

        if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
        {
            ImGui.SetItemDefaultFocus();
        }

        if (ModelPropertyDecorators.ParamRefRow(View, classMeta, prop, oldval, ref newval))
        {
            changed = true;
            committed = true;
        }

        if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
        {
            ImGui.NextColumn();

            var reflectedType = prop.ReflectedType;
            if (reflectedType != null)
            {
                PropContextRowOpener("typecol");

                ImGui.Text(reflectedType.FullName);
            }
        }

        ModelPropertyDecorators.DummyRefRow(View, classMeta, prop, oldval, ref newval);
        ModelPropertyDecorators.NodeRefRow(View, classMeta, prop, oldval, ref newval);
        ModelPropertyDecorators.MaterialRefRow(View, classMeta, prop, oldval, ref newval);

        UpdateProperty(prop, entSelection, oldval, newval, changed, committed, arrayIndex, classIndex);

        ImGui.NextColumn();
    }

    public bool DisplayProperty(ModelClass classMeta, object propObj, PropertyInfo prop, Type type)
    {
        var fieldMeta = ModelMetaHelper.GetFieldMeta(classMeta, prop.Name);

        var propName = prop.Name;

        // Automatic conditions that hide the property
        if(fieldMeta != null)
        {
            if(!CFG.Current.ModelEditor_Properties_Enable_Mesh_Fields)
            {
                if (fieldMeta.IsMeshData)
                    return false;
            }
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

    private void PropEditorGenericList(
        ModelClass classMeta,
        string metaType,
        ViewportSelection selection,
        IEnumerable<Entity> entSelection,
        Entity firstEnt,
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
            if (ImGui.Button("+##addListEntry"))
            {
                var newEntry = PropFinderUtil.CreateDefaultListElement(elementType);
                var action = new ModelAddListEntryAction(firstEnt, prop, obj, newEntry, list.Count);
                View.ViewportActionManager.ExecuteAction(action);
            }
            GUI.Tooltip("Add a new entry to the end of this list.");
        }

        ImGui.NextColumn();

        if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
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

                        ImGui.AlignTextToFramePadding();
                        ImGui.Text(elem?.GetType().Name ?? "null");

                        ImGui.SameLine();

                        ImGui.AlignTextToFramePadding();
                        if (ImGui.Button("-##removeListEntry"))
                        {
                            OnRemove();
                        }
                        GUI.Tooltip("Remove this entry from the list.");

                        if (elementType == typeof(EDGE.Edge))
                        {
                            // EDGE Points: Show
                            ImGui.SameLine();
                            ImGui.AlignTextToFramePadding();
                            if (ImGui.Button($"{Icons.Eye}##edgePointsShow"))
                            {
                                var startEdgePoint = PropFinderUtil.FindPropertyValue("V1", elem);
                                if(startEdgePoint != null)
                                {
                                    CFG.Current.StartEdgePoint = (Vector3)startEdgePoint;
                                }

                                var endEdgePoint = PropFinderUtil.FindPropertyValue("V2", elem);
                                if (endEdgePoint != null)
                                {
                                    CFG.Current.EndEdgePoint = (Vector3)endEdgePoint;
                                }

                                var pullEdgePoint = PropFinderUtil.FindPropertyValue("V3", elem);
                                if (pullEdgePoint != null)
                                {
                                    CFG.Current.PullEdgePoint = (Vector3)pullEdgePoint;
                                }

                                CFG.Current.DisplayEdgePoints = true;
                            }
                            GUI.Tooltip("Show the edge points for this entry in the viewport.");

                            // EDGE Points: Hide
                            ImGui.SameLine();
                            ImGui.AlignTextToFramePadding();
                            if (ImGui.Button($"{Icons.EyeSlash}##edgePointsHide"))
                            {
                                CFG.Current.DisplayEdgePoints = false;
                            }
                            GUI.Tooltip("Hide any edge points in the viewport.");
                        }

                        ImGui.NextColumn();

                        if (CFG.Current.ModelEditor_Properties_Enable_Type_Column)
                        {
                            PropContextRowOpener("listTypeCol");

                            ImGui.Text(elem?.GetType().FullName);

                            DisplayContextMenu(fieldName, fieldDescription, prop);

                            ImGui.NextColumn();
                        }

                        if (classOpen)
                        {
                            if (elem != null)
                                PropEditorGeneric(classMeta, metaType, selection, entSelection, elem, idx);

                            ImGui.TreePop();
                        }
                    }
                    else
                    {
                        PropGenericFieldRow(classMeta, selection, entSelection, prop, elementType, elem, $@"{fieldName}[{i}]", fieldDescription, i, classIndex, OnRemove);
                    }

                    ImGui.PopID();
                }

                if (removeIndex != -1)
                {
                    var action = new ModelRemoveListEntryAction(firstEnt, prop, obj, removeIndex);
                    View.ViewportActionManager.ExecuteAction(action);
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
            ImGui.OpenPopup("ModelPropertiesContextMenu");
        }
    }

    private void DisplayContextMenu(string name, string description, PropertyInfo prop)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("ModelPropertiesContextMenu");
        }

        if (ImGui.BeginPopup("ModelPropertiesContextMenu"))
        {
            // Copy Field Name
            if (ImGui.Selectable($"{LOC.Get("MODEL_PropertyView_Context_Action_Copy_Name")}##CopyPropName"))
            {
                PlatformUtils.Instance.SetClipboardText(name);
            }
            GUI.Tooltip(LOC.Get("MODEL_PropertyView_Context_Action_Copy_Name_TT"));

            // Copy Field Description
            if (ImGui.Selectable($"{LOC.Get("MODEL_PropertyView_Context_Action_Copy_Description")}##CopyPropDesc"))
            {
                PlatformUtils.Instance.SetClipboardText(description);
            }
            GUI.Tooltip(LOC.Get("MODEL_PropertyView_Context_Action_Copy_Description_TT"));

            // Copy Field Type
            if (ImGui.Selectable($"{LOC.Get("MODEL_PropertyView_Context_Action_Copy_Type")}##CopyPropType"))
            {
                var reflectedType = prop.ReflectedType;
                if (reflectedType != null)
                {
                    PlatformUtils.Instance.SetClipboardText(reflectedType.FullName);
                }
            }
            GUI.Tooltip(LOC.Get("MODEL_PropertyView_Context_Action_Copy_Type_TT"));

            ImGui.EndPopup();
        }
    }

    private (bool, bool) PropertyRow(Type typ, object oldval, out object newval, PropertyInfo prop, 
        IEnumerable<Entity> entSelection)
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

            bool showNormalInput = true;

            if (showNormalInput)
            {
                if (ImGui.DragFloat3("##value", ref val, 0.1f))
                {
                    newval = val;
                    isChanged = true;
                }
            }
            else
            {
                ImGui.BeginDisabled();
                if (ImGui.DragFloat3("##value", ref val, 0.1f))
                {
                    newval = val;
                    isChanged = true;
                }
                ImGui.EndDisabled();
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

            bool supportsAlpha;

            if (att != null)
            {
                supportsAlpha = att.Supports;
            }
            else
            {
                supportsAlpha = true;
            }

            var color = (Color)oldval;

            if (EditColor(color, supportsAlpha, out var edited))
            {
                newval = edited;
                isChanged = true;
            }
        }
        else
        {
            ImGui.Text("ImplementMe");
        }

        var isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive();

        return (isChanged, isDeactivatedAfterEdit);
    }

    public bool EditColor(Color input, bool supportsAlpha, out Color output)
    {
        if (supportsAlpha)
        {
            Vector4 val = new(
                input.R / 255f,
                input.G / 255f,
                input.B / 255f,
                input.A / 255f
            );

            if (ImGui.ColorEdit4("##value", ref val, ImGuiColorEditFlags.AlphaOpaque))
            {
                val = Clamp01(val);

                output = Color.FromArgb(
                    FloatToByte(val.W),
                    FloatToByte(val.X),
                    FloatToByte(val.Y),
                    FloatToByte(val.Z)
                );
                return true;
            }
        }
        else
        {
            Vector3 val = new(
                input.R / 255f,
                input.G / 255f,
                input.B / 255f
            );

            if (ImGui.ColorEdit3("##value", ref val))
            {
                val.X = Clamp01(val.X);
                val.Y = Clamp01(val.Y);
                val.Z = Clamp01(val.Z);

                output = Color.FromArgb(
                    FloatToByte(val.X),
                    FloatToByte(val.Y),
                    FloatToByte(val.Z)
                );
                return true;
            }
        }

        output = input;
        return false;
    }

    public float Clamp01(float v) => MathF.Max(0f, MathF.Min(1f, v));
    public Vector4 Clamp01(Vector4 v)
    {
        v.X = Clamp01(v.X);
        v.Y = Clamp01(v.Y);
        v.Z = Clamp01(v.Z);
        v.W = Clamp01(v.W);
        return v;
    }

    public int FloatToByte(float v)
    {
        v = Clamp01(v);
        return (int)MathF.Round(v * 255f);
    }

    private void UpdateProperty(object prop, Entity selection, object obj, object oldval, object newval,
        bool changed, bool committed, int arrayindex = -1)
    {
        if (changed)
        {
            ChangeProperty(prop, selection, obj, oldval, newval, ref committed, arrayindex);
        }

        if (committed)
        {
            CommitProperty(selection, oldval, newval, false);
        }
    }

    private void UpdateProperty(object prop, IEnumerable<Entity> selection,  object oldval, object newval, 
        bool changed, bool committed, int arrayindex, int classIndex)
    {
        foreach (var ent in selection)
        {
            if (changed)
            {
                ent.CachedAliasName = null;
                ent.BuildReferenceMap();
            }
        }

        if (changed)
        {
            ChangePropertyMultiple(prop, selection, oldval, newval, ref committed, arrayindex, classIndex);

            foreach (var ent in selection)
            {
                ent.BuildReferenceMap();
            }
        }

        if (committed)
        {
            if (_lastUncommittedAction != null && View.ViewportActionManager.PeekUndoAction() == _lastUncommittedAction)
            {
                if (_lastUncommittedAction is PropMultChangeAction a)
                {
                    View.ViewportActionManager.UndoAction();
                    View.ViewportActionManager.ExecuteAction(a);
                }

                _lastUncommittedAction = null;
                _changingPropery = null;
                _changingObject = null;
            }
        }
    }
    private void ChangePropertyMultiple(object prop, IEnumerable<Entity> ents, object oldval, object newval, ref bool committed,
        int arrayindex = -1, int classIndex = -1)
    {
        if (prop == _changingPropery && _lastUncommittedAction != null &&
            View.ViewportActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            View.ViewportActionManager.UndoAction();
        }
        else
        {
            _lastUncommittedAction = null;
        }

        var set = ents.ToHashSet();
        ModelPropertyChangeAction action;
        foreach (Entity selection in ents)
        {
            if (selection != null && _changingObject != null && !set.SetEquals((HashSet<Entity>)_changingObject))
            {
                committed = true;
                return;
            }
        }

        action = new ModelPropertyChangeAction(View, (PropertyInfo)prop, set, newval, arrayindex, classIndex);

        View.ViewportActionManager.ExecuteAction(action);

        _lastUncommittedAction = action;
        _changingPropery = prop;
        _changingObject = set;
    }

    private void ChangeProperty(object prop, Entity selection, object obj, object oldval, object newval,
        ref bool committed, int arrayindex = -1)
    {
        if (prop == _changingPropery && _lastUncommittedAction != null &&
            View.ViewportActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            View.ViewportActionManager.UndoAction();
        }
        else
        {
            _lastUncommittedAction = null;
        }

        if (_changingObject != null && selection != null && selection.WrappedObject != _changingObject)
        {
            committed = true;
        }
        else
        {
            PropChangeAction action;
            if (arrayindex != -1)
            {
                action = new PropChangeAction(selection, (PropertyInfo)prop, arrayindex, obj, newval);
            }
            else
            {
                action = new PropChangeAction(selection, (PropertyInfo)prop, obj, newval);
            }

            View.ViewportActionManager.ExecuteAction(action);

            _lastUncommittedAction = action;
            _changingPropery = prop;
            _changingObject = selection != null ? selection.WrappedObject : obj;
        }
    }

    private void CommitProperty(Entity selection, object oldval, object newval, bool destroyRenderModel)
    {
        // Invalidate name cache
        if (selection != null)
        {
            selection.Name = null;
        }

        selection.BuildReferenceMap();

        _lastUncommittedAction = null;
        _changingPropery = null;
        _changingObject = null;
    }


}
