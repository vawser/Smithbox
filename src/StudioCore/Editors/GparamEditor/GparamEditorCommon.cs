using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;
public class GparamEditorCommon
{
    private static object _editedPropCache;
    private static object _editedTypeCache;
    private static object _editedObjCache;
    private static bool _changedCache;
    private static bool _committedCache;

    public static unsafe void PropertyField(int index, Type typ, object oldval, ref object newval)
    {
        _changedCache = false;
        _committedCache = false;
        ImGui.SetNextItemWidth(-1);

        if (typ == typeof(GPARAM.IntField))
        {
            var val = (int)oldval;
            if (ImGui.InputInt($"##value{index}", ref val))
            {
                newval = val;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(GPARAM.UintField))
        {
            var val = (uint)oldval;
            var strval = $@"{val}";
            if (ImGui.InputText($"##value{index}", ref strval, 16))
            {
                var res = uint.TryParse(strval, out val);
                if (res)
                {
                    newval = val;
                    _editedPropCache = newval;
                    _changedCache = true;
                }
            }
        }
        else if (typ == typeof(GPARAM.ShortField))
        {
            int val = (short)oldval;
            if (ImGui.InputInt($"##value{index}", ref val))
            {
                newval = (short)val;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(GPARAM.SbyteField))
        {
            int val = (sbyte)oldval;
            if (ImGui.InputInt($"##value{index}", ref val))
            {
                newval = (sbyte)val;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(GPARAM.ByteField))
        {
            var val = (byte)oldval;
            var strval = $@"{val}";
            if (ImGui.InputText($"##value{index}", ref strval, 3))
            {
                var res = byte.TryParse(strval, out val);
                if (res)
                {
                    newval = val;
                    _editedPropCache = newval;
                    _changedCache = true;
                }
            }
        }
        else if (typ == typeof(GPARAM.BoolField))
        {
            var val = (bool)oldval;
            if (ImGui.Checkbox($"##value{index}", ref val))
            {
                newval = val;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(GPARAM.FloatField))
        {
            var val = (float)oldval;
            if (ImGui.InputFloat($"##value{index}", ref val, 0.1f, 1.0f, Utils.ImGui_InputFloatFormat(val)))
            {
                newval = val;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(GPARAM.Vector2Field))
        {
            var val = (Vector2)oldval;
            if (ImGui.InputFloat2($"##value{index}", ref val))
            {
                newval = val;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(GPARAM.Vector3Field))
        {
            var val = (Vector3)oldval;
            if (ImGui.InputFloat3($"##value{index}", ref val))
            {
                newval = val;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(GPARAM.Vector4Field))
        {
            Vector4 val = (Vector4)oldval;

            /*
            if (ImGui.InputFloat4($"##value{index}", ref val))
            {
                newval = val;
                _editedPropCache = newval;
                _changedCache = true;
            }
            */

            var colorVec = new Vector4((val.X), (val.Y), (val.Z), (val.W));

            if (ImGui.ColorEdit4($"##value{index}", ref colorVec))
            {
                newval = colorVec;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else if (typ == typeof(GPARAM.ColorField))
        {
            var color = (Color)oldval;
            Vector4 val = new(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
            if (ImGui.ColorEdit4($"##value{index}", ref val))
            {
                Color newColor = Color.FromArgb((int)(val.W * 255.0f), (int)(val.X * 255.0f),
                            (int)(val.Y * 255.0f), (int)(val.Z * 255.0f));
                newval = newColor;
                _editedPropCache = newval;
                _changedCache = true;
            }
        }
        else
        {
            // Using InputText means IsItemDeactivatedAfterEdit doesn't pick up random previous item
            var implMe = "ImplementMe";
            ImGui.InputText(null, ref implMe, 256, ImGuiInputTextFlags.ReadOnly);
        }

        _committedCache |= ImGui.IsItemDeactivatedAfterEdit();
    }

    public static void SetLastPropertyManual(object newval)
    {
        _editedPropCache = newval;
        _changedCache = true;
        _committedCache = true;
    }

    public static bool UpdateProperty(ActionManager executor, object obj, PropertyInfo prop, object oldval,
        int arrayindex = -1)
    {
        if (_changedCache)
        {
            _editedObjCache = obj;
            _editedTypeCache = prop;
        }
        else if (_editedPropCache != null && _editedPropCache != oldval)
            _changedCache = true;

        if (_changedCache)
            ChangeProperty(executor, _editedTypeCache, _editedObjCache, _editedPropCache, ref _committedCache,
                arrayindex);

        return _changedCache && _committedCache;
    }

    private static void ChangeProperty(ActionManager executor, object prop, object obj, object newval,
        ref bool committed, int arrayindex = -1)
    {
        if (committed)
        {
            if (newval == null)
            {
                // Safety check warned to user, should have proper crash handler instead
                TaskLogs.AddLog("GparamEditorCommon: Property changed was null",
                    LogLevel.Warning);
                return;
            }

            PropertiesChangedAction action;
            if (arrayindex != -1)
                action = new PropertiesChangedAction((PropertyInfo)prop, arrayindex, obj, newval);
            else
                action = new PropertiesChangedAction((PropertyInfo)prop, obj, newval);

            executor.ExecuteAction(action);
        }
    }
}
