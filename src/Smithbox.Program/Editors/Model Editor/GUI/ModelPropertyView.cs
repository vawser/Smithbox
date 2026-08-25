using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
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
        HashSet<Entity> entSelection = View.ViewportSelection.GetFilteredSelection<Entity>();

        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedListFilter_modelEditor_Properties", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("modelEditor_Properties", ref PropertyListFilter, ref ExactPropertyListFilter);

        // Toggle Community Field Names
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Book}", DPI.IconButtonSize))
        {
            CFG.Current.ModelEditor_Properties_Enable_Commmunity_Names = !CFG.Current.ModelEditor_Properties_Enable_Commmunity_Names;
        }

        var communityFieldNameMode = LOC.Get("MODEL_Properties_Toggle_Community_Names_Internal");
        if (CFG.Current.ModelEditor_Properties_Enable_Commmunity_Names)
            communityFieldNameMode = LOC.Get("MODEL_Properties_Toggle_Community_Names_Community");

        GUI.Tooltip(LOC.Get("MODEL_Properties_Toggle_Community_Names_TT", communityFieldNameMode));

        ImGui.EndChild();

        // Properties
        ImGui.BeginChild("propedit", ImGuiChildFlags.Borders);

        if(View.Selection.SelectedModelWrapper != null && View.Selection.SelectedModelWrapper.Container != null)
        {
            if(entSelection.Count > 1)
            {
                Entity firstEnt = entSelection.First();

                ImGui.TextColored(new Vector4(0.5f, 1.0f, 0.0f, 1.0f), LOC.Get("MODEL_Properties_Multi_Edit"));

                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_MultipleInput_Background);
                ImGui.BeginChild("Model_EditingMultipleObjsChild");

                PropEditorSelectedEntities(View.ViewportSelection);

                ImGui.PopStyleColor();
                ImGui.EndChild();
            }
            else if(entSelection.Any())
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

                PropEditorSelectedEntities(View.ViewportSelection);
            }
            else
            {
                ImGui.Text(LOC.Get("MODEL_Properties_No_Selection"));
            }
        }

        ImGui.EndChild();
    }

    private void PropEditorSelectedEntities(ViewportSelection selection, int classIndex = -1)
    {
        var entities = selection.GetFilteredSelection<ModelEntity>();
        var types = entities.Select(t => t.WrappedObject.GetType()).Distinct();
        var first = entities.First();
        var type = types.First();

        var objType = first.WrappedObject.GetType();

        // var meta = Editor.Project.ModelData.Meta.GetMeta(type, false);

        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get("MODEL_Properties_Col_Object_Type"));

        //if (meta != null)
        //{
        //    UIHelper.Tooltip(meta.Wiki);
        //}

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Text(type.Name);

        ImGui.NextColumn();

        if (types.Count() > 1)
        {
            return;
        }

        ImGui.Separator();

        PropEditorGeneric(selection, entities, entities.First().WrappedObject, classIndex: classIndex);

        ImGui.Columns(1);
    }

    private void PropEditorGeneric(
        ViewportSelection selection,
        IEnumerable<Entity> entSelection,
        object obj,
        int classIndex = -1
    )
    {

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
            var treeFlags = ImGuiTreeNodeFlags.DefaultOpen;

            // var meta = Editor.Project.ModelData.Meta.GetFieldMeta(prop.Name, type);

            // Field Name
            var fieldName = prop.Name;

            //if (CFG.Current.ModelEditor_Enable_Commmunity_Names && !meta.IsEmpty)
            //{
            //    fieldName = meta.AltName;
            //}

            // Field Description
            var fieldDescription = "";

            //if (!meta.IsEmpty)
            //{
            //    fieldDescription = meta.Wiki;
            //}

            // Handle property display (and search filtering)
            if (!DisplayProperty(obj, prop, type))
                continue;

            var ignoreProp = prop.GetCustomAttribute<IgnoreInModelEditor>();
            if(ignoreProp != null)
            {
                continue;
            }

            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();
            Type typ = prop.PropertyType;

            if (typ.IsArray)
            {
                var a = (Array)prop.GetValue(obj);
                var open = ImGui.TreeNodeEx($@"{fieldName}s", treeFlags);
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
                                PropEditorGeneric(selection, entSelection, o, i);
                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.AlignTextToFramePadding();
                            var array = obj as object[];

                            PropGenericFieldRow(selection, entSelection, prop, typ.GetElementType(), a.GetValue(i), $@"{fieldName}[{i}]", i, classIndex);
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
                PropEditorGenericList(selection, entSelection, firstEnt, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex);
            }
            else if (typ.BaseType != null && typ.BaseType.IsGenericType
                && typ.BaseType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type arrtyp = typ.BaseType.GetGenericArguments()[0];
                PropEditorGenericList(selection, entSelection, firstEnt, obj, prop, arrtyp, fieldName, fieldDescription, treeFlags, classIndex);
            }
            else if (typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                var o = prop.GetValue(obj);
                if (o != null)
                {
                    var open = ImGui.TreeNodeEx($"{fieldName}", treeFlags);
                    ShowFieldHint(obj, prop, fieldDescription);
                    ImGui.NextColumn();
                    ImGui.SetNextItemWidth(-1);
                    ImGui.Text(o.GetType().Name);
                    ImGui.NextColumn();

                    if (open)
                    {
                        PropEditorGeneric(selection, entSelection, o);
                        ImGui.TreePop();
                    }
                }

                ImGui.PopID();
            }
            else
            {
                PropGenericFieldRow(selection, entSelection, prop, typ, prop.GetValue(obj), $"{fieldName}", classIndex);

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

            var str = LOC.Get("MODEL_Properties_Field_Hint_Array_Type", prop.ReflectedType.Name);
            if (a.Length > 0)
            {
                str += LOC.Get("MODEL_Properties_Field_Hint_Array_Length", a.Length);
            }

            text = $"{text}\n{str}";
        }

        if (propType.IsValueType)
        {
            var str = LOC.Get("MODEL_Properties_Field_Hint_Value_Type", propType.Name);
            var min = propType.GetField("MinValue")?.GetValue(propType);
            var max = propType.GetField("MaxValue")?.GetValue(propType);
            if (min != null && max != null)
            {
                str += LOC.Get("MODEL_Properties_Field_Hint_Value_Min_Max", min, max);
            }

            text = $"{text}\n{str}";
        }
        else if (propType == typeof(string))
        {
            var a = (Array)prop.GetValue(obj);

            var str = LOC.Get("MODEL_Properties_Field_Hint_String_Type", propType.Name);
            if (a.Length > 0)
            {
                str += LOC.Get("MODEL_Properties_Field_Hint_String_Length", a.Length);
            }

            text = $"{text}\n{str}";
        }

        // Final description
        GUI.Tooltip(text);
    }

    private void PropGenericFieldRow(
        ViewportSelection selection,
        IEnumerable<Entity> entSelection,
        PropertyInfo prop,
        Type type,
        object obj,
        string name,
        int arrayIndex = -1,
        int classIndex = -1,
        Action onRemove = null
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

        ShowFieldHint(obj, prop, fieldDescription);

        ImGui.NextColumn();
        ImGui.SetNextItemWidth(-1);

        var oldval = obj;
        object newval;

        // Property Editor UI
        (bool, bool) propEditResults = PropertyRow(type, oldval, out newval, prop, entSelection);

        var changed = propEditResults.Item1;
        var committed = propEditResults.Item2;

        DisplayModelPropertyContextMenu(selection, prop, obj, arrayIndex, fieldName);

        if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
        {
            ImGui.SetItemDefaultFocus();
        }

        // Dummy References
        var dummyRef = prop.GetCustomAttribute<DummyReference>();
        if(dummyRef != null)
        {
            var container = View.Selection.SelectedModelWrapper.Container;
            var value = int.Parse(oldval.ToString());

            ImGui.NextColumn();

            ImGui.Text("");

            ImGui.NextColumn();

            for(int i = 0; i < container.Dummies.Count; i++)
            {
                var curDummy = container.Dummies[i];

                if(i == value)
                {
                    var dummy = (FLVER.Dummy)curDummy.WrappedObject;

                    if (ImGui.Button($"{Icons.Binoculars}##dummySelect{i}"))
                    {
                        EditorCommandQueue.AddCommand($"model/select/dummy/{i}");
                    }

                    ImGui.SameLine();

                    ImGui.Text(LOC.Get("MODEL_Properties_Dummy_Ref_Hint", i, dummy.ReferenceID));
                }
            }
        }

        // Node References
        var nodeRef = prop.GetCustomAttribute<NodeReference>();
        if (nodeRef != null)
        {
            var container = View.Selection.SelectedModelWrapper.Container;
            var value = int.Parse(oldval.ToString());

            ImGui.NextColumn();

            ImGui.Text("");

            ImGui.NextColumn();

            for (int i = 0; i < container.Nodes.Count; i++)
            {
                var curNode = container.Nodes[i];

                if (i == value)
                {
                    var node = (FLVER.Node)curNode.WrappedObject;

                    if (ImGui.Button($"{Icons.Binoculars}##nodeSelect{i}"))
                    {
                        EditorCommandQueue.AddCommand($"model/select/node/{i}");
                    }

                    ImGui.SameLine();

                    ImGui.Text($"{node.Name}");
                }
            }
        }

        // Material References
        var matRef = prop.GetCustomAttribute<MaterialReference>();
        if (matRef != null)
        {
            var container = View.Selection.SelectedModelWrapper.Container;
            var value = int.Parse(oldval.ToString());

            ImGui.NextColumn();

            ImGui.Text("");

            ImGui.NextColumn();

            for (int i = 0; i < container.Materials.Count; i++)
            {
                var curMaterial = container.Materials[i];

                if (i == value)
                {
                    var material = (FLVER2.Material)curMaterial.WrappedObject;

                    if (ImGui.Button($"{Icons.Binoculars}##matSelect{i}"))
                    {
                        EditorCommandQueue.AddCommand($"model/select/material/{i}");
                    }

                    ImGui.SameLine();

                    ImGui.Text($"{material.Name}");
                }
            }
        }

        UpdateProperty(prop, entSelection, oldval, newval, changed, committed, arrayIndex, classIndex);

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

    private void DisplayModelPropertyContextMenu(ViewportSelection selection, PropertyInfo prop, 
        object obj, int arrayIndex, string fieldName)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("ModelPropContextMenu");
        }

        if (ImGui.BeginPopup("ModelPropContextMenu"))
        {
            // Copy Property Name
            if (ImGui.Selectable($"{LOC.Get("MODEL_Properties_Context_Action_Copy_Prop_Name")}##CopyPropName"))
            {
                PlatformUtils.Instance.SetClipboardText(fieldName);
            }
            GUI.Tooltip(LOC.Get("MODEL_Properties_Context_Action_Copy_Prop_Name_TT"));

            // Copy Property Type
            if (ImGui.Selectable($"{LOC.Get("MODEL_Properties_Context_Action_Copy_Prop_Type")}##CopyPropType"))
            {
                var propType = prop.PropertyType;

                if (propType != null)
                {
                    var primitiveType = propType.ToString().Replace("System.", "");
                    PlatformUtils.Instance.SetClipboardText(primitiveType);
                }
            }
            GUI.Tooltip(LOC.Get("MODEL_Properties_Context_Action_Copy_Prop_Type_TT"));

            ImGui.EndPopup();
        }
    }

    #region Property Input
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
    #endregion

    #region Property Change - Update - Commit

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

        //// Undo and redo the last action with a rendering update
        //if (_lastUncommittedAction != null && View.ViewportActionManager.PeekUndoAction() == _lastUncommittedAction)
        //{
        //    if (_lastUncommittedAction is PropertiesChangedAction a)
        //    {
        //        // Kinda a hack to prevent a jumping glitch
        //        a.SetPostExecutionAction(null);

        //        View.ViewportActionManager.UndoAction();

        //        if (selection != null)
        //        {
        //            a.SetPostExecutionAction(undo =>
        //            {
        //                if (destroyRenderModel)
        //                {
        //                    if (selection.RenderSceneMesh != null)
        //                    {
        //                        selection.RenderSceneMesh = null;
        //                    }
        //                }

        //                selection.UpdateRenderModel();
        //            });
        //        }

        //        View.ViewportActionManager.ExecuteAction(a);
        //    }
        //}

        _lastUncommittedAction = null;
        _changingPropery = null;
        _changingObject = null;
    }
    #endregion


    public bool DisplayProperty(object propObj, PropertyInfo prop, Type type)
    {
        var propName = prop.Name;

        // Automatic conditions that hide the property

        if (!prop.CanWrite && !prop.PropertyType.IsArray)
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

    private void PropEditorGenericList(
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
        ShowFieldHint(obj, prop, fieldDescription);
        ImGui.NextColumn();

        if (list != null)
        {
            if (ImGui.Button("+##addListEntry"))
            {
                var newEntry = PropFinderUtil.CreateDefaultListElement(elementType);
                var action = new AddListEntryAction(firstEnt, prop, obj, newEntry, list.Count);
                View.ViewportActionManager.ExecuteAction(action);
            }
            GUI.Tooltip("Add a new entry to the end of this list.");
        }
        ImGui.NextColumn();

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
                        ShowFieldHint(obj, prop, fieldDescription);
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
                        if (classOpen)
                        {
                            if (elem != null)
                                PropEditorGeneric(selection, entSelection, elem, idx);

                            ImGui.TreePop();
                        }
                    }
                    else
                    {
                        PropGenericFieldRow(selection, entSelection, prop, elementType, elem, $@"{fieldName}[{i}]", i, classIndex, OnRemove);
                    }

                    ImGui.PopID();
                }

                if (removeIndex != -1)
                {
                    var action = new RemoveListEntryAction(firstEnt, prop, obj, removeIndex);
                    View.ViewportActionManager.ExecuteAction(action);
                }
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }
}
