using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Draws a field's value input and turns a finished edit into an undoable action.
/// <para>
/// ImGui only reports that a field was left (deactivated after an edit) on the frame after the last keystroke, when
/// the widget itself no longer returns a change. So the value, object and property of the edit in progress are kept
/// in the <c>_edited*</c> caches between frames, and committed when the field is left. The caches are cleared each
/// time a field is left, so a later field can never commit an earlier field's value.
/// </para>
/// </summary>
public class ParamFieldInput
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamEditorView ParentView;

    private delegate bool TryParseValue<T>(string text, out T value);

    // The edit in progress: the value typed so far, and the object and property it belongs to
    private object _editedPropCache;
    private object _editedTypeCache;
    private object _editedObjCache;

    // The text in the field being edited, while it doesn't parse as the field's type (null while it does).
    // Leaving the field with such text keeps the current value instead of committing the last value that parsed.
    private string _invalidTextCache;

    // Per drawn field: it changed this frame, it was left after an edit (not by Escape), it was left at all
    private bool _changedCache;
    private bool _committedCache;
    private bool _deactivatedCache;

    public ParamFieldInput(ParamEditorScreen editor, ProjectEntry project, ParamEditorView curView)
    {
        Editor = editor;
        Project = project;
        ParentView = curView;
    }

    public unsafe void DisplayFieldInput(FieldMetaContext metaContext, Type typ, object oldval, ref object newval)
    {
        _changedCache = false;
        _committedCache = false;
        _deactivatedCache = false;
        ImGui.SetNextItemWidth(-1);

        try
        {
            if (metaContext.IsBool)
            {
                dynamic val = oldval;
                bool checkVal = val > 0;
                if (ImGui.Checkbox("##valueBool", ref checkVal))
                {
                    newval = Convert.ChangeType(checkVal ? 1 : 0, oldval.GetType());
                    _editedPropCache = newval;
                    _changedCache = true;
                }

                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
                ImGui.SameLine();
                ImGui.SetNextItemWidth(-1);
            }
        }
        catch
        {
        }

        if (typ == typeof(long))
        {
            IntegerInput<long>(oldval, ref newval, long.TryParse);
        }
        else if (typ == typeof(int))
        {
            IntegerInput<int>(oldval, ref newval, int.TryParse);
        }
        else if (typ == typeof(uint))
        {
            IntegerInput<uint>(oldval, ref newval, uint.TryParse);
        }
        else if (typ == typeof(short))
        {
            IntegerInput<short>(oldval, ref newval, short.TryParse);
        }
        else if (typ == typeof(ushort))
        {
            IntegerInput<ushort>(oldval, ref newval, ushort.TryParse);
        }
        else if (typ == typeof(sbyte))
        {
            IntegerInput<sbyte>(oldval, ref newval, sbyte.TryParse);
        }
        else if (typ == typeof(byte))
        {
            IntegerInput<byte>(oldval, ref newval, byte.TryParse);
        }
        else if (typ == typeof(bool))
        {
            var val = (bool)oldval;
            if (ImGui.Checkbox("##value", ref val))
            {
                newval = val;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(float))
        {
            // Display in-game form of this property (i.e. 75% instead of 0.25)
            if (metaContext.IsInvertedPercentage && CFG.Current.ParamEditor_Field_Input_Display_Traditional_Percentage)
            {
                float fakeVal = (1 - (float)oldval) * 100;

                if (ImGui.InputFloat("##value", ref fakeVal, 0.0f, 1.0f, ParamFieldUtils.ImGui_InputFloatFormat(fakeVal, 3, 3)))
                {
                    // Restore actual value
                    float realVal = (1 - (fakeVal / 100));
                    newval = realVal;
                    _editedPropCache = newval;
                    _changedCache = true;
                }
            }
            else
            {
                var val = (float)oldval;
                if (ImGui.InputFloat("##value", ref val, 0.1f, 1.0f, ParamFieldUtils.ImGui_InputFloatFormat(val)))
                {
                    newval = val;
                    _editedPropCache = newval;
                    _changedCache = true;
                }
            }
        }
        else if (typ == typeof(double))
        {
            var tempValue = (double)oldval;
            double step = 0.1;
            double stepFast = 1.0;
            var format = InterfaceUtils.CreateFloatFormat((float)tempValue);
            byte* formatPtr = InterfaceUtils.StringToUtf8(format);

            if (ImGui.InputScalar($"##value", ImGuiDataType.Double, &tempValue, &step, &stepFast, formatPtr))
            {
                newval = tempValue;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(string))
        {
            var val = (string)oldval;
            if (val == null)
                val = "";

            var input = new InputTextHandler(val);

            if (input.Draw("##value", out string newValue))
            {
                newval = newValue;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(Vector2))
        {
            var val = (Vector2)oldval;
            if (ImGui.InputFloat2("##value", ref val))
            {
                newval = val;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(Vector3))
        {
            var val = (Vector3)oldval;
            if (ImGui.InputFloat3("##value", ref val))
            {
                newval = val;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(byte[]))
        {
            var bval = (byte[])oldval;
            var val = ParamUtils.Dummy8Write(bval);
            if (ImGui.InputText("##value", ref val, 9999))
            {
                var nval = ParamUtils.Dummy8Read(val, bval.Length);
                if (nval != null)
                {
                    newval = nval;
                    _editedPropCache = newval;
                    _changedCache = true;
                    _invalidTextCache = null;
                }
                else
                {
                    _invalidTextCache = val;
                }
            }
        }
        else
        {
            // Using InputText means IsItemDeactivatedAfterEdit doesn't pick up random previous item
            var implMe = "ImplementMe";
            ImGui.InputText("ImplementMe", ref implMe, 256, ImGuiInputTextFlags.ReadOnly);
        }

        // Left after an edit by Enter, Tab or a click elsewhere; Escape reverts instead
        _committedCache |= GUI.InputCommitted();
        _deactivatedCache = ImGui.IsItemDeactivated();
    }

    /// <summary>
    /// A text input for an integer field. Each keystroke that parses becomes the pending value. While the text
    /// doesn't parse (letters, or a number outside the type's range), the field is outlined, and leaving it keeps
    /// the current value: committing the last value that did parse would turn "300" in a byte field into 30.
    /// </summary>
    private void IntegerInput<T>(object oldval, ref object newval, TryParseValue<T> tryParse) where T : struct
    {
        var input = new InputTextHandler($@"{oldval}");

        if (input.Draw("##value", out string newValue))
        {
            if (ParamUtils.IsFxrString(newValue))
            {
                newValue = ParamUtils.GetFxrId(newValue);
            }

            if (tryParse(newValue, out T val))
            {
                newval = val;
                _editedPropCache = newval;
                _changedCache = true;
                _invalidTextCache = null;
            }
            else
            {
                _invalidTextCache = newValue;
            }
        }

        if (_invalidTextCache != null && ImGui.IsItemActive())
        {
            ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(),
                ImGui.GetColorU32(UI.Current.ImGui_Invalid_Text_Color));

            ImGui.SetTooltip(LOC.Get("PARAM_FieldInput_Invalid_Value_TT",
                typeof(T).Name, GetTypeBound<T>("MinValue"), GetTypeBound<T>("MaxValue")));
        }
    }

    private static string GetTypeBound<T>(string fieldName)
    {
        return typeof(T).GetField(fieldName)?.GetValue(null)?.ToString() ?? "";
    }

    /// <summary>
    /// Sets the pending value from outside the input widget (context menus, shortcuts), and commits it this frame
    /// </summary>
    public void SetLastPropertyManual(object newval)
    {
        _editedPropCache = newval;
        _changedCache = true;
        _committedCache = true;
        _invalidTextCache = null;
    }

    /// <summary>
    /// Call straight after <see cref="DisplayFieldInput"/> (or <see cref="SetLastPropertyManual"/>) for the same
    /// field. Returns true when an edit was committed this frame.
    /// </summary>
    public bool UpdateProperty(object obj, PropertyInfo prop, object oldval,
        int arrayindex = -1)
    {
        if (_changedCache)
        {
            _editedObjCache = obj;
            _editedTypeCache = prop;
        }

        var committed = false;

        if (_committedCache)
        {
            if (_invalidTextCache != null)
            {
                // The text left in the field doesn't parse: keep the current value rather than a partial one
                Smithbox.Log(this, LOC.Get("PARAM_FieldInput_Invalid_Value_Rejected", _invalidTextCache, oldval),
                    LogLevel.Warning);
            }
            else if (_editedPropCache != null && _editedObjCache != null && _editedTypeCache != null)
            {
                committed = ChangeProperty(_editedTypeCache, _editedObjCache, _editedPropCache, arrayindex);
            }
        }

        // The field was left (committed, rejected, or cancelled with Escape): drop the edit in progress, so it can't
        // be committed later by another field
        if (_committedCache || _deactivatedCache)
        {
            ClearPendingEdit();
        }

        return committed;
    }

    private void ClearPendingEdit()
    {
        _editedPropCache = null;
        _editedObjCache = null;
        _editedTypeCache = null;
        _invalidTextCache = null;
    }

    /// <summary>
    /// Applies the edit as an undoable action. Returns false, and adds no undo entry, when the value is unchanged.
    /// </summary>
    private bool ChangeProperty(object prop, object obj, object newval, int arrayindex = -1)
    {
        if (newval == null)
        {
            return false;
        }

        var propInfo = (PropertyInfo)prop;

        if (IsSameValue(GetCurrentValue(propInfo, obj, arrayindex), newval))
        {
            return false;
        }

        PropertiesChangedAction action;
        if (arrayindex != -1)
        {
            action = new PropertiesChangedAction(propInfo, arrayindex, obj, newval);
        }
        else
        {
            action = new PropertiesChangedAction(propInfo, obj, newval);
        }

        action.SetPostExecutionAction(undo =>
        {
            var curParam = ParentView.Selection.GetActiveParam();

            if (ParentView.ParamTableWindow.IsInTableGroupMode(curParam))
            {
                var curGroup = ParentView.ParamTableWindow.CurrentTableGroupID;
                ParentView.ParamTableWindow.UpdateTableGroupSelection(curGroup);
            }
        });

        ParentView.Editor.ActionManager.ExecuteAction(action);

        return true;
    }

    private static object GetCurrentValue(PropertyInfo prop, object obj, int arrayindex)
    {
        try
        {
            var value = prop.GetValue(obj);

            if (arrayindex != -1 && value is Array array)
            {
                return array.GetValue(arrayindex);
            }

            return value;
        }
        catch
        {
            // Unknown: treat as changed
            return null;
        }
    }

    private static bool IsSameValue(object current, object newval)
    {
        if (current is byte[] currentBytes && newval is byte[] newBytes)
        {
            return ParamUtils.ByteArrayEquals(currentBytes, newBytes);
        }

        return Equals(current, newval);
    }
}
