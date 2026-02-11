using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.MTD;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialPropertyInput
{
    public MaterialEditorView Parent;
    public ProjectEntry Project;

    private object _changingObject;
    private object _changingPropery;

    private EditorAction _lastUncommittedAction;
    public PropertyInfo RequestedSearchProperty = null;

    public MaterialPropertyInput(MaterialEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    #region Property Input
    public (bool, bool) HandlePropertyInput(Type typ, object oldval, out object newval, PropertyInfo prop, object sourceObj, ParamType paramType = ParamType.None)
    {
        ImGui.SetNextItemWidth(-1);

        newval = null;
        var isChanged = false;

        switch (paramType)
        {
            case MTD.ParamType.Bool:
                typ = typeof(bool);
                break;

            case MTD.ParamType.Float:
                typ = typeof(float);
                break;

            case MTD.ParamType.Float2:
                typ = typeof(Vector2);
                var arr1 = (float[])oldval;
                oldval = new Vector2(arr1[0], arr1[1]);
                break;

            case MTD.ParamType.Float3:
                typ = typeof(Vector3);
                var arr2 = (float[])oldval;
                oldval = new Vector3(arr2[0], arr2[1], arr2[2]);
                break;

            case MTD.ParamType.Float4:
                typ = typeof(Vector4);
                var arr3 = (float[])oldval;
                oldval = new Vector4(arr3[0], arr3[1], arr3[2], arr3[3]);
                break;

            case MTD.ParamType.Int:
                typ = typeof(int);
                break;

            case MTD.ParamType.Int2:
                typ = typeof(Vector2);
                var arr4 = (int[])oldval;
                oldval = new Vector2(arr4[0], arr4[1]);
                break;
        }

        // Special handling for these since they can't be handled like a normal enum
        if (prop.Name == "Value" && sourceObj is MTD.Param mtdParam_blendMode && mtdParam_blendMode.Name == "g_BlendMode")
        {
            Array enumVals = Enum.GetValues(typeof(BlendMode));
            var enumNames = Enum.GetNames(typeof(BlendMode));
            var intVals = new int[enumVals.Length];

            for (var i = 0; i < enumVals.Length; i++)
            {
                intVals[i] = (int)enumVals.GetValue(i);
            }

            var enumVal = (BlendMode)oldval;

            if (Utils.EnumEditor(enumVals, enumNames, enumVal, out var val, intVals))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (prop.Name == "Value" && sourceObj is MTD.Param mtdParam_lightingType && mtdParam_lightingType.Name == "g_LightingType")
        {
            Array enumVals = Enum.GetValues(typeof(LightingType));
            var enumNames = Enum.GetNames(typeof(LightingType));
            var intVals = new int[enumVals.Length];

            for (var i = 0; i < enumVals.Length; i++)
            {
                intVals[i] = (int)enumVals.GetValue(i);
            }

            var enumVal = (LightingType)oldval;

            if (Utils.EnumEditor(enumVals, enumNames, oldval, out var val, intVals))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(long))
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
                Smithbox.Log(this, 
                    $"Color property in \"{prop.DeclaringType}\" does not declare if it supports Alpha. Alpha will be exposed by default",
                    LogLevel.Warning, LogPriority.Low);

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
            ImGui.Text("ImplementMe");
        }

        var isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive();

        // Convert the MTD.Param array value newval back to the actual arrays once input has occured
        switch (paramType)
        {
            case MTD.ParamType.Float2:
                if (newval is Vector2 v2)
                    newval = new float[] { v2.X, v2.Y };
                break;

            case MTD.ParamType.Float3:
                if (newval is Vector3 v3)
                    newval = new float[] { v3.X, v3.Y, v3.Z };
                break;

            case MTD.ParamType.Float4:
                if (newval is Vector4 v4)
                    newval = new float[] { v4.X, v4.Y, v4.Z, v4.W };
                break;

            case MTD.ParamType.Int2:
                if (newval is Vector2 vi2)
                    newval = new int[] { (int)vi2.X, (int)vi2.Y };
                break;
        }

        return (isChanged, isDeactivatedAfterEdit);
    }

    #endregion

    #region Property Change - Update - Commit
    public void UpdateProperty(object prop, object sourceObj, object oldval, object newval,
        bool changed, bool committed, int arrayindex = -1, int classindex = -1)
    {
        if (changed)
        {
            ChangeProperty(prop, sourceObj, oldval, newval, ref committed, arrayindex);
        }

        if (committed)
        {
            CommitProperty(oldval, newval);
        }
    }

    private void ChangeProperty(object prop, object sourceObj, object oldval, object newval,
        ref bool committed, int arrayindex = -1)
    {
        if (prop == _changingPropery && _lastUncommittedAction != null &&
            Parent.ActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            Parent.ActionManager.UndoAction();
        }
        else
        {
            _lastUncommittedAction = null;
        }

        if (_changingObject != null && sourceObj != null && sourceObj != _changingObject)
        {
            committed = true;
        }
        else
        {
            MaterialPropertyChange action;

            if (arrayindex != -1)
            {
                action = new MaterialPropertyChange((PropertyInfo)prop, arrayindex, sourceObj, newval);
            }
            else
            {
                action = new MaterialPropertyChange((PropertyInfo)prop, sourceObj, newval);
            }

            Parent.ActionManager.ExecuteAction(action);

            _lastUncommittedAction = action;
            _changingPropery = prop;
            _changingObject = sourceObj != null ? sourceObj : sourceObj;
        }
    }

    private void CommitProperty(object oldval, object newval)
    {
        // Undo and redo the last action with a rendering update
        if (_lastUncommittedAction != null && Parent.ActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            if (_lastUncommittedAction is MaterialPropertyChange a)
            {
                Parent.ActionManager.ExecuteAction(a);
            }
        }

        _lastUncommittedAction = null;
        _changingPropery = null;
        _changingObject = null;
    }
    #endregion
}
