using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.ParamEditor;

public class ParamFieldInput
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamEditorView ParentView;

    private object _editedPropCache;
    private object _editedTypeCache;
    private object _editedObjCache;
    private bool _changedCache;
    private bool _committedCache;

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
            var val = (long)oldval;
            var strval = $@"{val}";

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                if (ParamUtils.IsFxrString(newValue))
                {
                    newValue = ParamUtils.GetFxrId(newValue);
                }

                var res = long.TryParse(newValue, out val);
                if (res)
                {
                    newval = val;
                    _editedPropCache = newval;
                    _changedCache = true;
                }
            }
        }
        else if (typ == typeof(int))
        {
            var val = (int)oldval;
            var strval = $@"{val}";

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                if (ParamUtils.IsFxrString(newValue))
                {
                    newValue = ParamUtils.GetFxrId(newValue);
                }

                var res = int.TryParse(newValue, out val);
                if (res)
                {
                    newval = val;
                    _editedPropCache = newval;
                    _changedCache = true;
                }
            }
        }
        else if (typ == typeof(uint))
        {
            var val = (uint)oldval;
            var strval = $@"{val}";

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                if (ParamUtils.IsFxrString(newValue))
                {
                    newValue = ParamUtils.GetFxrId(newValue);
                }

                var res = uint.TryParse(newValue, out val);
                if (res)
                {
                    newval = val;
                    _editedPropCache = newval;
                    _changedCache = true;
                }
            }
        }
        else if (typ == typeof(short))
        {
            var val = (short)oldval;
            var strval = $@"{val}";

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                if (ParamUtils.IsFxrString(newValue))
                {
                    newValue = ParamUtils.GetFxrId(newValue);
                }

                var res = short.TryParse(newValue, out val);
                if (res)
                {
                    newval = val;
                    _editedPropCache = newval;
                    _changedCache = true;
                }
            }
        }
        else if (typ == typeof(ushort))
        {
            var val = (ushort)oldval;
            var strval = $@"{val}";

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                if (ParamUtils.IsFxrString(newValue))
                {
                    newValue = ParamUtils.GetFxrId(newValue);
                }

                var res = ushort.TryParse(newValue, out val);
                if (res)
                {
                    newval = val;
                    _editedPropCache = newval;
                    _changedCache = true;
                }
            }
        }
        else if (typ == typeof(sbyte))
        {
            var val = (sbyte)oldval;
            var strval = $@"{val}";

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                if (ParamUtils.IsFxrString(newValue))
                {
                    newValue = ParamUtils.GetFxrId(newValue);
                }

                var res = sbyte.TryParse(newValue, out val);
                if (res)
                {
                    newval = val;
                    _editedPropCache = newval;
                    _changedCache = true;
                }
            }
        }
        else if (typ == typeof(byte))
        {
            var val = (byte)oldval;
            var strval = $@"{val}";

            var input = new InputTextHandler(strval);

            if (input.Draw("##value", out string newValue))
            {
                if (ParamUtils.IsFxrString(newValue))
                {
                    newValue = ParamUtils.GetFxrId(newValue);
                }

                var res = byte.TryParse(newValue, out val);
                if (res)
                {
                    newval = val;
                    _editedPropCache = newval;
                    _changedCache = true;
                }
            }
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
            if (metaContext.IsInvertedPercentage && !CFG.Current.ParamEditor_Field_Input_Display_Traditional_Percentage)
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
                }
            }
        }
        else
        {
            // Using InputText means IsItemDeactivatedAfterEdit doesn't pick up random previous item
            var implMe = "ImplementMe";
            ImGui.InputText("ImplementMe", ref implMe, 256, ImGuiInputTextFlags.ReadOnly);
        }

        _committedCache |= ImGui.IsItemDeactivatedAfterEdit();
    }

    public void SetLastPropertyManual(object newval)
    {
        _editedPropCache = newval;
        _changedCache = true;
        _committedCache = true;
    }

    public bool UpdateProperty(object obj, PropertyInfo prop, object oldval,
        int arrayindex = -1)
    {
        if (_changedCache)
        {
            _editedObjCache = obj;
            _editedTypeCache = prop;
        }
        else if (_editedPropCache != null && _editedPropCache != oldval)
        {
            _changedCache = true;
        }

        if (_changedCache)
        {
            ChangeProperty(_editedTypeCache, _editedObjCache, _editedPropCache, ref _committedCache,
                arrayindex);
        }

        return _changedCache && _committedCache;
    }

    private void ChangeProperty(object prop, object obj, object newval,
        ref bool committed, int arrayindex = -1)
    {
        if (committed)
        {
            if (newval == null)
            {
                // Safety check warned to user, should have proper crash handler instead
                // Smithbox.Log(this, "ParamEditorCommon: Property changed was null", LogLevel.Warning);
                return;
            }

            PropertiesChangedAction action;
            if (arrayindex != -1)
            {
                action = new PropertiesChangedAction((PropertyInfo)prop, arrayindex, obj, newval);
                action.SetPostExecutionAction(undo =>
                {
                    var curParam = ParentView.Selection.GetActiveParam();

                    if (ParentView.ParamTableWindow.IsInTableGroupMode(curParam))
                    {
                        var curGroup = ParentView.ParamTableWindow.CurrentTableGroup;
                        ParentView.ParamTableWindow.UpdateTableGroupSelection(curGroup);
                    }
                });
            }
            else
            {
                action = new PropertiesChangedAction((PropertyInfo)prop, obj, newval);
                action.SetPostExecutionAction(undo =>
                {
                    var curParam = ParentView.Selection.GetActiveParam();

                    if (ParentView.ParamTableWindow.IsInTableGroupMode(curParam))
                    {
                        var curGroup = ParentView.ParamTableWindow.CurrentTableGroup;
                        ParentView.ParamTableWindow.UpdateTableGroupSelection(curGroup);
                    }
                });
            }

            ParentView.Editor.ActionManager.ExecuteAction(action);
        }
    }
}
