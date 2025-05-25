using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Interface;
using StudioCore.MaterialEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Andre.Formats.Param;
using static SoulsFormats.MATBIN;

namespace StudioCore.MaterialEditorNS;

public class MaterialFieldInput
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialFieldInput(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }


    public unsafe void DisplayFieldInput(string imguiID, object field, object curValue)
    {
        var inputWidth = 800f;

        var wasChanged = false;
        var commitChange = false;
        var newValue = field;
        var fieldType = field.GetType();

        var inputFlags = ImGuiInputTextFlags.None;

        ImGui.SetNextItemWidth(inputWidth);

        // Long
        if (fieldType == typeof(long))
        {
            var tempValue = (long)curValue;
            var stringValue = $@"{tempValue}";

            if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
            {
                var result = long.TryParse(stringValue, out tempValue);
                if (result)
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }
        }

        // Signed Integer
        if (fieldType == typeof(int))
        {
            var tempValue = (int)curValue;

            if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Unsigned Integer
        if (fieldType == typeof(uint))
        {
            var tempValue = (uint)curValue;
            var stringValue = $@"{tempValue}";

            if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
            {
                var result = uint.TryParse(stringValue, out tempValue);
                if (result)
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }
        }

        // Signed Short
        if (fieldType == typeof(short))
        {
            int tempValue = (short)curValue;

            if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Unsigned Short
        if (fieldType == typeof(ushort))
        {
            var tempValue = (ushort)curValue;
            var stringValue = $@"{tempValue}";

            if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
            {
                var result = ushort.TryParse(stringValue, out tempValue);
                if (result)
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }
        }

        // Signed Byte
        if (fieldType == typeof(sbyte))
        {
            int tempValue = (sbyte)curValue;

            if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Unsigned Byte
        if (fieldType == typeof(byte))
        {
            var tempValue = (byte)curValue;
            var stringValue = $@"{tempValue}";

            if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
            {
                var result = byte.TryParse(stringValue, out tempValue);
                if (result)
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }
        }

        // Boolean
        if (fieldType == typeof(bool))
        {
            var tempValue = (bool)curValue;

            if (ImGui.Checkbox($"##value_{imguiID}", ref tempValue))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Float
        if (fieldType == typeof(float))
        {
            var tempValue = (float)curValue;
            var format = CreateFloatFormat(tempValue);
            var formatPtr = InterfaceUtils.StringToUtf8(format);

            if (ImGui.InputFloat($"##value_{imguiID}", ref tempValue, 0.1f, 1.0f, formatPtr, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }

            InterfaceUtils.FreeUtf8(formatPtr);
        }

        // Double
        if (fieldType == typeof(double))
        {
            var tempValue = (double)curValue;
            double step = 0.1;
            double stepFast = 1.0;
            var format = CreateFloatFormat((float)tempValue);
            byte* formatPtr = InterfaceUtils.StringToUtf8(format);

            if (ImGui.InputScalar($"##value_{imguiID}", ImGuiDataType.Double, &tempValue, &step, &stepFast, formatPtr, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // String
        if (fieldType == typeof(string))
        {
            var tempValue = (string)curValue;

            if (ImGui.InputText($"##value_{imguiID}", ref tempValue, 128, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Vector2
        if (fieldType == typeof(Vector2))
        {
            var tempValue = (Vector2)curValue;

            if (ImGui.InputFloat2($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Vector3
        if (fieldType == typeof(Vector3))
        {
            var tempValue = (Vector3)curValue;

            if (ImGui.InputFloat3($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Vector4
        if (fieldType == typeof(Vector4))
        {
            var tempValue = (Vector4)curValue;

            if (ImGui.InputFloat4($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Byte Array
        if (fieldType == typeof(byte[]))
        {
            var bval = (byte[])curValue;
            var tempValue = ConvertToTextualPadding(bval);

            if (ImGui.InputText($"##value_{imguiID}", ref tempValue, 9999, inputFlags))
            {
                var nVal = ConvertToBytePadding(tempValue, bval.Length);

                if (nVal != null)
                {
                    newValue = nVal;
                    wasChanged = true;
                }
            }
        }

        commitChange |= ImGui.IsItemDeactivatedAfterEdit();

        // Apply action
        if (commitChange && wasChanged)
        {
            // TODO
            //var changeAction = new ParamFieldChange(curRow, curField, curValue, newValue);
            //Editor.ActionManager.ExecuteAction(changeAction);
        }
    }

    public unsafe void DisplayParamInput(string imguiID, object field, ParamType paramType)
    {
        var inputWidth = 400f;

        var wasChanged = false;
        var commitChange = false;
        var newValue = field;
        var fieldType = field.GetType();

        var inputFlags = ImGuiInputTextFlags.None;

        ImGui.SetNextItemWidth(inputWidth);

        // Signed Integer
        if (paramType is ParamType.Int)
        {
            var tempValue = (int)field;

            if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // TODO: add support for the int arrays (2)

        // Boolean
        if (paramType is ParamType.Bool)
        {
            var tempValue = (bool)field;

            if (ImGui.Checkbox($"##value_{imguiID}", ref tempValue))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Float
        if (paramType is ParamType.Float)
        {
            var tempValue = (float)field;
            var format = CreateFloatFormat(tempValue);
            var formatPtr = InterfaceUtils.StringToUtf8(format);

            if (ImGui.InputFloat($"##value_{imguiID}", ref tempValue, 0.1f, 1.0f, formatPtr, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }

            InterfaceUtils.FreeUtf8(formatPtr);
        }

        // TODO: add support for the float arrays (2, 3, 4, 5)

        commitChange |= ImGui.IsItemDeactivatedAfterEdit();

        // Apply action
        if (commitChange && wasChanged)
        {
            // TODO
            //var changeAction = new ParamFieldChange(curRow, curField, curValue, newValue);
            //Editor.ActionManager.ExecuteAction(changeAction);
        }
    }

    /// <summary>
    /// Helper for the float formatting in the InputFloat input elements
    /// </summary>
    /// <param name="f"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public unsafe string CreateFloatFormat(float f, int min = 3, int max = 6)
    {
        var split = f.ToString("F6").TrimEnd('0').Split('.');
        return $"%.{Math.Clamp(split.Last().Length, min, max)}f";
    }

    /// <summary>
    /// Converts the padding byte array into a textual representation for editing 
    /// </summary>
    /// <param name="dummy8"></param>
    /// <returns></returns>
    public string ConvertToTextualPadding(byte[] dummy8)
    {
        string val = null;
        foreach (var b in dummy8)
        {
            if (val == null)
            {
                val = "[" + b;
            }
            else
            {
                val += "|" + b;
            }
        }

        if (val == null)
        {
            val = "[]";
        }
        else
        {
            val += "]";
        }

        return val;
    }

    /// <summary>
    /// Converts the textual representation of padding back into a byte array
    /// </summary>
    /// <param name="dummy8"></param>
    /// <param name="expectedLength"></param>
    /// <returns></returns>
    public byte[] ConvertToBytePadding(string dummy8, int expectedLength)
    {
        var nval = new byte[expectedLength];
        if (!(dummy8.StartsWith('[') && dummy8.EndsWith(']')))
        {
            return null;
        }

        var spl = dummy8.Substring(1, dummy8.Length - 2).Split('|');
        if (nval.Length != spl.Length)
        {
            return null;
        }

        for (var i = 0; i < nval.Length; i++)
        {
            if (!byte.TryParse(spl[i], out nval[i]))
            {
                return null;
            }
        }

        return nval;
    }
}
