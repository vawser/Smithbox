using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using static SoulsFormats.MTD;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialPropertyInput
{
    public MaterialEditorView Parent;
    public ProjectEntry Project;

    public PropertyInfo RequestedSearchProperty = null;

    public MaterialPropertyInput(MaterialEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    #region Property Input

    public (bool, bool) PropertyRow(Type typ, object oldval, out object newval, PropertyInfo prop)
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
            if (ImGui.DragFloat("##value", ref val, MaterialValueInputs.Speed(val), float.MinValue, float.MaxValue,
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
            if (ImGui.DragFloat2("##value", ref val, MaterialValueInputs.Speed(val)))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(Vector3))
        {
            var val = (Vector3)oldval;

            if (ImGui.DragFloat3("##value", ref val, MaterialValueInputs.Speed(val)))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(Vector4))
        {
            var val = (Vector4)oldval;

            if (ImGui.DragFloat4("##value", ref val, MaterialValueInputs.Speed(val)))
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
            ImGui.Text($"ImplementMe: {prop.PropertyType}");
        }

        // Neither writer has a form for None: MTD.Param.Write writes a value block the reader rejects,
        // MATBIN.Param.WriteData throws and fails the binder's save
        if (isChanged && newval is MTD.ParamType.None or MATBIN.ParamType.None)
        {
            newval = null;
            isChanged = false;
        }

        var isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive();

        return (isChanged, isDeactivatedAfterEdit);
    }

    public (bool, bool) HandleMtdPropertyInput(Type typ, object oldval, out object newval, PropertyInfo prop, object sourceObj, MTD.ParamType paramType = MTD.ParamType.None)
    {
        ImGui.SetNextItemWidth(-1);

        newval = null;
        var isChanged = false;

        // Colour params get a colour picker
        if (prop.Name == "Value" && MaterialValueInputs.TryColor(sourceObj, oldval, out newval, out var colorResult))
            return colorResult;

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
                // Edited as ints, below
                typ = typeof(int[]);
                break;
        }

        // Special handling for these since they can't be handled like a normal enum.
        // Only while the param is an int, which the enum is read from and written back as.
        if (prop.Name == "Value" && sourceObj is MTD.Param { Name: "g_BlendMode", Type: MTD.ParamType.Int })
        {
            Array enumVals = Enum.GetValues(typeof(BlendMode));
            var enumNames = Enum.GetNames(typeof(BlendMode));
            var intVals = new int[enumVals.Length];

            for (var i = 0; i < enumVals.Length; i++)
            {
                intVals[i] = (int)enumVals.GetValue(i);
            }

            var enumVal = (BlendMode)Convert.ToInt32(oldval);

            if (Utils.EnumEditor(enumVals, enumNames, enumVal, out var val, intVals))
            {
                // Kept an int, as MTD.Param.Write and the renderer read it
                newval = Convert.ToInt32(val);
                isChanged = true;
            }
        }
        else if (prop.Name == "Value" && sourceObj is MTD.Param { Name: "g_LightingType", Type: MTD.ParamType.Int })
        {
            Array enumVals = Enum.GetValues(typeof(LightingType));
            var enumNames = Enum.GetNames(typeof(LightingType));
            var intVals = new int[enumVals.Length];

            for (var i = 0; i < enumVals.Length; i++)
            {
                intVals[i] = (int)enumVals.GetValue(i);
            }

            var enumVal = (LightingType)Convert.ToInt32(oldval);

            if (Utils.EnumEditor(enumVals, enumNames, enumVal, out var val, intVals))
            {
                // Kept an int, as MTD.Param.Write and the renderer read it
                newval = Convert.ToInt32(val);
                isChanged = true;
            }
        }
        else if (paramType == MTD.ParamType.Int2)
        {
            isChanged = InputInt2((int[])oldval, out newval);
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
            if (ImGui.DragFloat("##value", ref val, MaterialValueInputs.Speed(val), float.MinValue, float.MaxValue,
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
            if (ImGui.DragFloat2("##value", ref val, MaterialValueInputs.Speed(val)))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(Vector3))
        {
            var val = (Vector3)oldval;

            if (ImGui.DragFloat3("##value", ref val, MaterialValueInputs.Speed(val)))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(Vector4))
        {
            var val = (Vector4)oldval;

            if (ImGui.DragFloat4("##value", ref val, MaterialValueInputs.Speed(val)))
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
        }

        return (isChanged, isDeactivatedAfterEdit);
    }

    /// <summary>
    /// An Int2 param edited as two ints. The new value is a new array, so undo keeps the old one.
    /// </summary>
    private static bool InputInt2(int[] oldval, out object newval)
    {
        newval = null;

        var val = new int[2];
        Array.Copy(oldval, val, Math.Min(oldval.Length, 2));

        if (ImGui.InputInt2("##value", ref val[0]))
        {
            newval = val;
            return true;
        }

        return false;
    }


    public (bool, bool) HandleMatbinPropertyInput(Type typ, object oldval, out object newval, PropertyInfo prop, object sourceObj, MATBIN.ParamType paramType = MATBIN.ParamType.None)
    {
        ImGui.SetNextItemWidth(-1);

        newval = null;
        var isChanged = false;

        // Colour params get a colour picker, and g_BlendMode the blend mode combo the MTD one has
        if (prop.Name == "Value" && MaterialValueInputs.TryColor(sourceObj, oldval, out newval, out var colorResult))
            return colorResult;

        if (prop.Name == "Value" && MaterialValueInputs.TryBlendMode(sourceObj, oldval, out newval, out var blendResult))
            return blendResult;

        switch (paramType)
        {
            case MATBIN.ParamType.Bool:
                typ = typeof(bool);
                break;

            case MATBIN.ParamType.Float:
                typ = typeof(float);
                break;

            case MATBIN.ParamType.Float2:
                typ = typeof(Vector2);
                var arr1 = (float[])oldval;
                oldval = new Vector2(arr1[0], arr1[1]);
                break;

            case MATBIN.ParamType.Float3:
                typ = typeof(Vector3);
                var arr2 = (float[])oldval;
                oldval = new Vector3(arr2[0], arr2[1], arr2[2]);
                break;

            case MATBIN.ParamType.Float4:
                typ = typeof(Vector4);
                var arr3 = (float[])oldval;
                oldval = new Vector4(arr3[0], arr3[1], arr3[2], arr3[3]);
                break;

            case MATBIN.ParamType.Float5:
                // TODO
                break;

            case MATBIN.ParamType.Int:
                typ = typeof(int);
                break;

            case MATBIN.ParamType.Int2:
                // Edited as ints, below
                typ = typeof(int[]);
                break;
        }

        if (paramType == MATBIN.ParamType.Int2)
        {
            isChanged = InputInt2((int[])oldval, out newval);
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
            if (ImGui.DragFloat("##value", ref val, MaterialValueInputs.Speed(val), float.MinValue, float.MaxValue,
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
            if (ImGui.DragFloat2("##value", ref val, MaterialValueInputs.Speed(val)))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(Vector3))
        {
            var val = (Vector3)oldval;

            if (ImGui.DragFloat3("##value", ref val, MaterialValueInputs.Speed(val)))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(Vector4))
        {
            var val = (Vector4)oldval;

            if (ImGui.DragFloat4("##value", ref val, MaterialValueInputs.Speed(val)))
            {
                newval = val;
                isChanged = true;
            }
        }
        else if (paramType == MATBIN.ParamType.Float5)
        {
            var arr = (float[])oldval;
            var v3 = new Vector3(arr[0], arr[1], arr[2]);
            var v4 = arr[3];
            var v5 = arr[4];

            bool changed3 = ImGui.DragFloat3("##value_f5_xyz", ref v3, MaterialValueInputs.Speed(v3));

            ImGui.SetNextItemWidth(-1);
            bool changed4 = ImGui.DragFloat("##value_f5_w", ref v4, MaterialValueInputs.Speed(v4), float.MinValue, float.MaxValue, Utils.ImGui_InputFloatFormat(v4));

            ImGui.SetNextItemWidth(-1);
            bool changed5 = ImGui.DragFloat("##value_f5_v", ref v5, MaterialValueInputs.Speed(v5), float.MinValue, float.MaxValue,
                Utils.ImGui_InputFloatFormat(v5));

            if (changed3 || changed4 || changed5)
            {
                newval = new float[] { v3.X, v3.Y, v3.Z, v4, v5 };
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
            ImGui.Text($"ImplementMe: {paramType}");
        }

        var isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive();

        switch (paramType)
        {
            case MATBIN.ParamType.Float2:
                if (newval is Vector2 v2)
                    newval = new float[] { v2.X, v2.Y };
                break;

            case MATBIN.ParamType.Float3:
                if (newval is Vector3 v3)
                    newval = new float[] { v3.X, v3.Y, v3.Z };
                break;

            case MATBIN.ParamType.Float4:
                if (newval is Vector4 v4)
                    newval = new float[] { v4.X, v4.Y, v4.Z, v4.W };
                break;
        }

        return (isChanged, isDeactivatedAfterEdit);
    }

    #endregion
}
