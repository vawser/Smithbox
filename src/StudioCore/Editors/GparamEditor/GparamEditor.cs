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
    public static unsafe void ValueField(int idx, IField field, IFieldValue value)
    {
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
            sbyte fieldValue = sbyteField.Values[idx].Value;
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

            var flags = ImGuiColorEditFlags.None;

            if(CFG.Current.Gparam_ColorEdit_RGB)
            {
                flags = ImGuiColorEditFlags.DisplayRGB;
            }
            if (CFG.Current.Gparam_ColorEdit_Decimal)
            {
                flags = ImGuiColorEditFlags.Float;
            }
            if (CFG.Current.Gparam_ColorEdit_HSV)
            {
                flags = ImGuiColorEditFlags.DisplayHSV;
            }

            if (ImGui.ColorEdit4($"##value{idx}", ref colorInput, flags))
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

    public static unsafe void TimeOfDayField(int idx, IField field, IFieldValue value)
    {
        ImGui.SetNextItemWidth(-1);

        // INT
        if (field is GPARAM.IntField intField)
        {
            float fieldValue = intField.Values[idx].Unk04;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
            {
                intField.Values[idx].Unk04 = floatInput;
            }
        }
        // UINT
        else if (field is GPARAM.UintField uintField)
        {
            float fieldValue = uintField.Values[idx].Unk04;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
            {
                uintField.Values[idx].Unk04 = floatInput;
            }
        }
        // SHORT
        else if (field is GPARAM.ShortField shortField)
        {
            float fieldValue = shortField.Values[idx].Unk04;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
            {
                shortField.Values[idx].Unk04 = floatInput;
            }
        }
        // SBYTE
        else if (field is GPARAM.SbyteField sbyteField)
        {
            float fieldValue = sbyteField.Values[idx].Unk04;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
            {
                sbyteField.Values[idx].Unk04 = floatInput;
            }
        }
        // BYTE
        else if (field is GPARAM.ByteField byteField)
        {
            float fieldValue = byteField.Values[idx].Unk04;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
            {
                byteField.Values[idx].Unk04 = floatInput;
            }
        }
        // BOOL
        else if (field is GPARAM.BoolField boolField)
        {
            float fieldValue = boolField.Values[idx].Unk04;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
            {
                boolField.Values[idx].Unk04 = floatInput;
            }
        }
        // FLOAT
        else if (field is GPARAM.FloatField floatField)
        {
            float fieldValue = floatField.Values[idx].Unk04;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
            {
                floatField.Values[idx].Unk04 = floatInput;
            }
        }
        // VECTOR2
        else if (field is GPARAM.Vector2Field vector2Field)
        {
            float fieldValue = vector2Field.Values[idx].Unk04;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
            {
                vector2Field.Values[idx].Unk04 = floatInput;
            }
        }
        // VECTOR3
        else if (field is GPARAM.Vector3Field vector3Field)
        {
            float fieldValue = vector3Field.Values[idx].Unk04;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
            {
                vector3Field.Values[idx].Unk04 = floatInput;
            }
        }
        // VECTOR4
        else if (field is GPARAM.Vector4Field vector4Field)
        {
            float fieldValue = vector4Field.Values[idx].Unk04;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
            {
                vector4Field.Values[idx].Unk04 = floatInput;
            }
        }
        // COLOR
        else if (field is GPARAM.ColorField colorField)
        {
            float fieldValue = colorField.Values[idx].Unk04;
            float floatInput = fieldValue;

            if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
            {
                colorField.Values[idx].Unk04 = floatInput;
            }
        }
        else
        {
            TaskLogs.AddLog($"{field.Name} {field.GetType()} is not supported");
        }
    }

    /// <summary>
    /// Update the group index lists to reflect any additions
    /// or removals in terms of value row ids
    /// </summary>
    public static void UpdateGroupIndexes(GPARAM gparam)
    {
        var newGroupIndexes = new List<UnkParamExtra>();
        int idx = 0;

        foreach (var param in gparam.Params)
        {
            var newGroupIndexList = new UnkParamExtra();
            newGroupIndexList.Unk00 = idx;
            newGroupIndexList.Unk0c = 0; // Always 0

            foreach (var field in param.Fields)
            {
                var values = field.Values;

                foreach(var val in values)
                {
                    if (!newGroupIndexList.Ids.Contains(val.Id))
                    {
                        newGroupIndexList.Ids.Add(val.Id);
                    }
                }
            }

            newGroupIndexes.Add(newGroupIndexList);

            ++idx;
        }

        gparam.UnkParamExtras = newGroupIndexes;
    }

    /// <summary>
    /// Add selected value row (with specified new ID) to target field value list
    /// </summary>
    /// <param name="targetField"></param>
    /// <param name="targetValue"></param>
    /// <param name="newRowId"></param>
    public static void AddPropertyValueRow(IField targetField, IFieldValue targetValue, int newRowId)
    {
        if (targetField is SbyteField sbyteField)
        {
            GPARAM.SbyteField castField = (SbyteField)targetField;

            var dupeVal = new GPARAM.FieldValue<sbyte>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (sbyte)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is ByteField byteField)
        {
            GPARAM.ByteField castField = (ByteField)targetField;

            var dupeVal = new GPARAM.FieldValue<byte>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (byte)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is ShortField shortField)
        {
            GPARAM.ShortField castField = (ShortField)targetField;

            var dupeVal = new GPARAM.FieldValue<short>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (short)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is IntField intField)
        {
            GPARAM.IntField castField = (IntField)targetField;

            var dupeVal = new GPARAM.FieldValue<int>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (int)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is UintField uintField)
        {
            GPARAM.UintField castField = (UintField)targetField;

            var dupeVal = new GPARAM.FieldValue<uint>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (uint)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is FloatField floatField)
        {
            GPARAM.FloatField castField = (FloatField)targetField;

            var dupeVal = new GPARAM.FieldValue<float>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (float)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is BoolField boolField)
        {
            GPARAM.BoolField castField = (BoolField)targetField;

            var dupeVal = new GPARAM.FieldValue<bool>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (bool)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is Vector2Field vector2Field)
        {
            GPARAM.Vector2Field castField = (Vector2Field)targetField;

            var dupeVal = new GPARAM.FieldValue<Vector2>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (Vector2)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is Vector3Field vector3Field)
        {
            GPARAM.Vector3Field castField = (Vector3Field)targetField;

            var dupeVal = new GPARAM.FieldValue<Vector3>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (Vector3)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is Vector4Field vector4Field)
        {
            GPARAM.Vector4Field castField = (Vector4Field)targetField;

            var dupeVal = new GPARAM.FieldValue<Vector4>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (Vector4)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
        if (targetField is ColorField colorField)
        {
            GPARAM.ColorField castField = (ColorField)targetField;

            var dupeVal = new GPARAM.FieldValue<Color>();
            dupeVal.Id = newRowId;
            dupeVal.Unk04 = targetValue.Unk04;
            dupeVal.Value = (Color)targetValue.Value;

            castField.Values.Add(dupeVal);
        }
    }

    /// <summary>
    /// Removed selected value row from target field value list
    /// </summary>
    /// <param name="targetField"></param>
    /// <param name="targetValue"></param>
    public static void RemovePropertyValueRow(IField targetField, IFieldValue targetValue)
    {
        if (targetField is SbyteField sbyteField)
        {
            GPARAM.SbyteField castField = (SbyteField)targetField;
            castField.Values.Remove((FieldValue<sbyte>)targetValue);
        }
        if (targetField is ByteField byteField)
        {
            GPARAM.ByteField castField = (ByteField)targetField;
            castField.Values.Remove((FieldValue<byte>)targetValue);
        }
        if (targetField is ShortField shortField)
        {
            GPARAM.ShortField castField = (ShortField)targetField;
            castField.Values.Remove((FieldValue<short>)targetValue);
        }
        if (targetField is IntField intField)
        {
            GPARAM.IntField castField = (IntField)targetField;
            castField.Values.Remove((FieldValue<int>)targetValue);
        }
        if (targetField is UintField uintField)
        {
            GPARAM.UintField castField = (UintField)targetField;
            castField.Values.Remove((FieldValue<uint>)targetValue);
        }
        if (targetField is FloatField floatField)
        {
            GPARAM.FloatField castField = (FloatField)targetField;
            castField.Values.Remove((FieldValue<float>)targetValue);
        }
        if (targetField is BoolField boolField)
        {
            GPARAM.BoolField castField = (BoolField)targetField;
            castField.Values.Remove((FieldValue<bool>)targetValue);
        }
        if (targetField is Vector2Field vector2Field)
        {
            GPARAM.Vector2Field castField = (Vector2Field)targetField;
            castField.Values.Remove((FieldValue<Vector2>)targetValue);
        }
        if (targetField is Vector3Field vector3Field)
        {
            GPARAM.Vector3Field castField = (Vector3Field)targetField;
            castField.Values.Remove((FieldValue<Vector3>)targetValue);
        }
        if (targetField is Vector4Field vector4Field)
        {
            GPARAM.Vector4Field castField = (Vector4Field)targetField;
            castField.Values.Remove((FieldValue<Vector4>)targetValue);
        }
        if (targetField is ColorField colorField)
        {
            GPARAM.ColorField castField = (ColorField)targetField;
            castField.Values.Remove((FieldValue<Color>)targetValue);
        }
    }
}
