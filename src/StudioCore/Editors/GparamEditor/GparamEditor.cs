using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.BanksMain;
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
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;
public class GparamEditor
{
    public static unsafe void PropertyField(int idx, IField field, IFieldValue value)
    {
        Type fieldType = field.GetType();

        ImGui.SetNextItemWidth(-1);

        // INT
        if (field is GPARAM.IntField intField)
        {
            int fieldValue = intField.Values[idx].Value;
            int intInput = fieldValue;

            if (GparamFormatBank.Bank.IsBooleanProperty(field.Key))
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if(boolInput)
                    {
                        intField.Values[idx].Value = 1;
                    }
                    else
                    {
                        intField.Values[idx].Value = 0;
                    }
                }
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref intInput))
                {
                    intField.Values[idx].Value = intInput;
                }
            }
        }
        // UINT
        else if (field is GPARAM.UintField uintField)
        {
            uint fieldValue = uintField.Values[idx].Value;
            uint uintInput = fieldValue;

            var strval = $@"{uintInput}";

            if (GparamFormatBank.Bank.IsBooleanProperty(field.Key))
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        uintField.Values[idx].Value = 1;
                    }
                    else
                    {
                        uintField.Values[idx].Value = 0;
                    }
                }
            }
            else
            {
                if (ImGui.InputText($"##value{idx}", ref strval, 16))
                {
                    bool result = uint.TryParse(strval, out uintInput);

                    if (result)
                    {
                        uintField.Values[idx].Value = uintInput;
                    }
                }
            }
        }
        // SHORT
        else if (field is GPARAM.ShortField shortField)
        {
            short fieldValue = shortField.Values[idx].Value;
            int shortInput = fieldValue;

            if (GparamFormatBank.Bank.IsBooleanProperty(field.Key))
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        shortField.Values[idx].Value = 1;
                    }
                    else
                    {
                        shortField.Values[idx].Value = 0;
                    }
                }
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref shortInput))
                {
                    shortField.Values[idx].Value = (short)shortInput;
                }
            }
        }
        // SBYTE
        else if (field is GPARAM.SbyteField sbyteField)
        {
            short fieldValue = sbyteField.Values[idx].Value;
            int sbyteInput = fieldValue;

            if (GparamFormatBank.Bank.IsBooleanProperty(field.Key))
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        sbyteField.Values[idx].Value = 1;
                    }
                    else
                    {
                        sbyteField.Values[idx].Value = 0;
                    }
                }
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref sbyteInput))
                {
                    sbyteField.Values[idx].Value = (sbyte)sbyteInput;
                }
            }
        }
        // BYTE
        else if (field is GPARAM.ByteField byteField)
        {
            byte fieldValue = byteField.Values[idx].Value;
            byte byteInput = fieldValue;

            var strval = $@"{byteInput}";

            if (GparamFormatBank.Bank.IsBooleanProperty(field.Key))
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        byteField.Values[idx].Value = 1;
                    }
                    else
                    {
                        byteField.Values[idx].Value = 0;
                    }
                }
            }
            else
            {
                if (ImGui.InputText($"##value{idx}", ref strval, 16))
                {
                    bool result = byte.TryParse(strval, out byteInput);

                    if (result)
                    {
                        byteField.Values[idx].Value = byteInput;
                    }
                }
            }
        }
        // BOOL
        else if (field is GPARAM.BoolField boolField)
        {
            bool fieldValue = boolField.Values[idx].Value;
            bool boolInput = fieldValue;

            if (ImGui.Checkbox($"##value{idx}", ref boolInput))
            {
                boolField.Values[idx].Value = boolInput;
            }
        }
        // FLOAT
        else if (field is GPARAM.FloatField floatField)
        {
            float fieldValue = floatField.Values[idx].Value;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##value{idx}", ref floatInput, 0.1f, 1.0f, 
                Utils.ImGui_InputFloatFormat(floatInput)))
            {
                floatField.Values[idx].Value = floatInput;
            }
        }
        // VECTOR2
        else if (field is GPARAM.Vector2Field vector2Field)
        {
            Vector2 fieldValue = vector2Field.Values[idx].Value;
            Vector2 vector2Input = fieldValue;

            if (ImGui.InputFloat2($"##value{idx}", ref vector2Input))
            {
                vector2Field.Values[idx].Value = vector2Input;
            }
        }
        // VECTOR3
        else if (field is GPARAM.Vector3Field vector3Field)
        {
            Vector3 fieldValue = vector3Field.Values[idx].Value;
            Vector3 vector3Input = fieldValue;

            if (ImGui.InputFloat3($"##value{idx}", ref vector3Input))
            {
                vector3Field.Values[idx].Value = vector3Input;
            }
        }
        // VECTOR4
        else if (field is GPARAM.Vector4Field vector4Field &&
            !CFG.Current.Gparam_DisplayColorEditForVector4Fields)
        {
            Vector4 fieldValue = vector4Field.Values[idx].Value;
            Vector4 vector4Input = fieldValue;

            if (ImGui.InputFloat4($"##value{idx}", ref vector4Input))
            {
                vector4Field.Values[idx].Value = vector4Input;
            }
        }
        // VECTOR4 (COLOR EDIT)
        else if (field is GPARAM.Vector4Field vectorColorField && 
            CFG.Current.Gparam_DisplayColorEditForVector4Fields)
        {
            Vector4 fieldValue = vectorColorField.Values[idx].Value;
            Vector4 colorInput = fieldValue;

            if (ImGui.ColorEdit4($"##value{idx}", ref colorInput))
            {
                vectorColorField.Values[idx].Value = colorInput;
            }
        }
        // COLOR
        else if (field is GPARAM.ColorField colorField)
        {
            Color raw = colorField.Values[idx].Value;
            Vector4 fieldValue = new(raw.R / 255.0f, raw.G / 255.0f, raw.B / 255.0f, raw.A / 255.0f);
            Vector4 colorInput = fieldValue;

            if (ImGui.ColorEdit4($"##value{idx}", ref colorInput))
            {
                Color trueColorInput = Color.FromArgb(
                    (int)(colorInput.W * 255.0f),
                    (int)(colorInput.X * 255.0f),
                    (int)(colorInput.Y * 255.0f),
                    (int)(colorInput.Z * 255.0f)
                    );

                colorField.Values[idx].Value = trueColorInput;
            }
        }
        else
        {
            TaskLogs.AddLog($"{field.Name} {field.GetType()} is not supported");
        }
    }
}
