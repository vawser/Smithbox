using Hexa.NET.ImGui;
using StudioCore.Editor;
using StudioCore.Editors.HavokEditor.Action;
using StudioCore.Editors.HavokEditor.Framework;
using StudioCore.HavokEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor.Core;

public class HavokPropertiesView
{
    private HavokEditorScreen Screen;

    private HavokFieldCache FieldCache = new();

    private int depth = 0;

    public HavokPropertiesView(HavokEditorScreen screen)
    {
        Screen = screen;
        FieldCache = new();
    }

    public void OnGui()
    {
        var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit;

        ImGui.Begin("Fields##HavokEntryProperties");

        if (ImGui.BeginTable($"fieldTable", 2, tableFlags))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Name");

            ImGui.TableSetColumnIndex(1);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Value");

            ImGui.TableSetColumnIndex(1);

            Setup();

            ImGui.EndTable();
        }

        ImGui.End();
    }

    private void Setup()
    {
        var objectHierarchy = Screen.Selection.ObjectHierarchy;
        var selectedObjectClass = Screen.Selection.SelectedObjectClass;

        if (objectHierarchy == null)
            return;

        if (selectedObjectClass == null)
            return;

        if (objectHierarchy.ContainsKey(selectedObjectClass))
        {
            var curEntries = objectHierarchy[selectedObjectClass];

            var curEntry = curEntries[Screen.Selection.SelectedObjectClassEntryIndex];
            if(curEntry != null)
                FieldEditor(curEntry);
        }
    }

    private void FieldEditor(object obj, int classIndex = -1)
    {
        var sourceObj = obj;
        Type type = obj.GetType();
        FieldInfo[] fields = FieldCache.GetCachedFields(type);

        var id = 0;

        foreach (var field in fields)
        {
            var meta = Screen.Selection.GetFieldMeta(field.Name);

            var fieldName = field.Name;
            var fieldDescription = "";

            if(meta != null)
            {
                fieldName = meta.Name;
                fieldDescription = meta.Description;
            }

            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();
            Type typ = field.FieldType;

            if (typ.IsArray)
            {
                var a = (Array)field.GetValue(obj);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.TextColored(UI.Current.ImGui_Havok_Header, $"{fieldName}");
                if (fieldDescription != "")
                    UIHelper.ShowHoverTooltip(fieldDescription);

                ImGui.TableSetColumnIndex(1);

                for (var i = 0; i < a.Length; i++)
                {
                    ImGui.PushID(i);
                    Type arrtyp = typ.GetElementType();

                    if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                    {
                        FieldEditorListReferenceRow(field, arrtyp, sourceObj, a.GetValue(i), i);
                    }
                    else
                    {
                        ImGui.AlignTextToFramePadding();
                        var array = obj as object[];
                        FieldEditorRow(field, typ.GetElementType(), sourceObj, a.GetValue(i), i, classIndex);
                    }

                    ImGui.PopID();
                }

                ImGui.PopID();
            }
            else if (typ.IsGenericType && typ.GetGenericTypeDefinition() == typeof(List<>))
            {
                var l = field.GetValue(obj);
                PropertyInfo itemprop = l.GetType().GetProperty("Item");
                var count = (int)l.GetType().GetProperty("Count").GetValue(l);

                for (var i = 0; i < count; i++)
                {
                    ImGui.PushID(i);

                    Type arrtyp = typ.GetGenericArguments()[0];

                    if (arrtyp.IsClass && arrtyp != typeof(string) && !arrtyp.IsArray)
                    {
                        FieldEditorListReferenceRow(field, arrtyp, sourceObj, itemprop.GetValue(l, [i]), i);
                    }
                    else
                    {
                        FieldEditorRow(field, arrtyp, sourceObj, itemprop.GetValue(l, [i]), i, classIndex);
                    }

                    ImGui.PopID();
                }

                ImGui.PopID();
            }
            else if (typ.IsClass && typ != typeof(string) && !typ.IsArray)
            {
                var o = field.GetValue(obj);

                if (o != null)
                {
                    FieldEditorIndividualReferenceRow(field, typ, sourceObj, o);
                }

                ImGui.PopID();
            }
            else
            {
                FieldEditorRow(field, typ, sourceObj, field.GetValue(obj), classIndex);
                ImGui.PopID();
            }

            id++;
        }
    }

    private void FieldEditorIndividualReferenceRow(FieldInfo field, Type type, object sourceObj, object curObj)
    {
        var meta = Screen.Selection.GetFieldMeta(field.Name);
        var fieldName = field.Name;
        var fieldDescription = "";

        if (meta != null)
        {
            fieldName = meta.Name;
            fieldDescription = meta.Description;
        }

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        ImGui.TextColored(UI.Current.ImGui_Havok_Highlight, $"{fieldName}");
        if(fieldDescription != "")
            UIHelper.ShowHoverTooltip(fieldDescription);

        ImGui.TableSetColumnIndex(1);

        ImGui.Text($"{type}");

        // TODO: these need to add to the current table: curObj however is a separate sourceObj
    }

    private void FieldEditorListReferenceRow(FieldInfo field, Type type, object sourceObj, object curObj, int index = -1)
    {
        var meta = Screen.Selection.GetFieldMeta(field.Name);
        var fieldName = field.Name;
        var fieldDescription = "";

        if (meta != null)
        {
            fieldName = meta.Name;
            fieldDescription = meta.Description;
        }

        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);

        ImGui.TextColored(UI.Current.ImGui_Havok_Reference, $"[{index}] {fieldName}");
        if (fieldDescription != "")
            UIHelper.ShowHoverTooltip(fieldDescription);

        ImGui.TableSetColumnIndex(1);

        ImGui.Text($"{type}");

        // TODO: these need to add to the current table: curObj however is a separate sourceObj
    }

    private void FieldEditorRow(FieldInfo field, Type type,
        object sourceObj, object obj,
        int arrayIndex = -1,
        int classIndex = -1
    )
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        //PropContextRowOpener();

        var meta = Screen.Selection.GetFieldMeta(field.Name);
        var fieldName = field.Name;
        var fieldDescription = "";

        if (meta != null)
        {
            fieldName = meta.Name;
            fieldDescription = meta.Description;
        }

        ImGui.Text(fieldName);
        if (fieldDescription != "")
            UIHelper.ShowHoverTooltip(fieldDescription);

        ImGui.TableSetColumnIndex(1);

        ImGui.SetNextItemWidth(-1);
        var oldval = obj;
        object newval;

        // Property Editor UI
        (bool, bool) fieldEditResults = FieldRow(type, oldval, out newval, field);
        var changed = fieldEditResults.Item1;
        var committed = fieldEditResults.Item2;

        //DisplayPropContextMenu(meta, prop, obj, arrayIndex);

        if (ImGui.IsItemActive() && !ImGui.IsWindowFocused())
        {
            ImGui.SetItemDefaultFocus();
        }

        UpdateField(field, sourceObj, oldval, newval, changed, committed, arrayIndex, classIndex);
    }

    private (bool, bool) FieldRow(Type typ, object oldval, out object newval, FieldInfo field)
    {
        ImGui.SetNextItemWidth(-1);

        newval = null;
        var isChanged = false;
        if (typ == typeof(long))
        {
            var val = (long)oldval;
            var strval = $@"{val}";

            if (ImGui.InputText("##value", ref strval, 99))
            {
                var res = long.TryParse(strval, out val);
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

            if (ImGui.InputText("##value", ref strval, 16))
            {
                var res = uint.TryParse(strval, out val);
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

            if (ImGui.InputText("##value", ref strval, 5))
            {
                var res = ushort.TryParse(strval, out val);
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

            if (ImGui.InputText("##value", ref strval, 3))
            {
                var res = byte.TryParse(strval, out val);
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

            if (ImGui.InputText("##value", ref val, 99))
            {
                newval = val;
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
                ImGui.Text("Enum: Not implemented");
            }
        }
        else
        {
            ImGui.Text($"Type: Not implemented : {typ}");
        }

        var isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive();

        return (isChanged, isDeactivatedAfterEdit);
    }

    private EditorAction _lastUncommittedAction;
    private object _changingProperty;

    private void UpdateField(object field, object sourceObj, object oldval, object newval, 
        bool changed, bool committed, int arrayindex, int classIndex)
    {
        if (changed)
        {
            ChangeField(field, sourceObj, oldval, newval, ref committed, arrayindex, classIndex);
        }

        if (committed)
        {
            if (_lastUncommittedAction != null && Screen.EditorActionManager.PeekUndoAction() == _lastUncommittedAction)
            {
                if (_lastUncommittedAction is HavokPropertyChange a)
                {
                    Screen.EditorActionManager.UndoAction();
                    Screen.EditorActionManager.ExecuteAction(a);
                }

                _lastUncommittedAction = null;
                _changingProperty = null;
            }
        }
    }

    private void ChangeField(object field, object sourceObj, object oldval, object newval, 
        ref bool committed,
        int arrayindex = -1, int classIndex = -1)
    {
        if (field == _changingProperty && _lastUncommittedAction != null &&
            Screen.EditorActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            Screen.EditorActionManager.UndoAction();
        }
        else
        {
            _lastUncommittedAction = null;
        }

        var action = new HavokPropertyChange((FieldInfo)field, sourceObj, field, newval);

        if (arrayindex > -1)
            action = new HavokPropertyChange((FieldInfo)field, sourceObj, field, newval, arrayindex);

        Screen.EditorActionManager.ExecuteAction(action);

        _lastUncommittedAction = action;
        _changingProperty = field;
    }
}
