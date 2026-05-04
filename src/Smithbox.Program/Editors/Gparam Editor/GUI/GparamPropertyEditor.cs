using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class GparamPropertyEditor
{
    private GparamEditorView Parent;
    private ProjectEntry Project;

    private object _editedValueCache;

    // Value has been changed via input
    private bool _changedCache;

    // Value can be changed in the GPARAM
    private bool _committedCache;

    // Value to change without commit
    private bool _uncommittedCache;

    private bool _isHoldingColor;
    private Vector4 _heldColor;

    public GparamPropertyEditor(GparamEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public unsafe void ValueField(GPARAM data, Param group, IField field, IFieldValue value, int idx)
    {
        _changedCache = false;
        _committedCache = false;
        _uncommittedCache = false;

        ImGui.SetNextItemWidth(-1);

        object oldValue = null;
        object newValue = null;

        var groupId = Parent.Selection.GetSelectedGroup().Key;
        var fieldId = field.Key;
        var isBool = GparamMetaUtils.IsFieldBoolean(Project, groupId, fieldId);

        // INT
        if (field is IntField intField)
        {
            int fieldValue = intField.Values[idx].Value;
            int intInput = fieldValue;
            oldValue = fieldValue;

            if (isBool)
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }

                    _editedValueCache = newValue;
                    _changedCache = true;
                }

                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref intInput))
                {
                    newValue = intInput;

                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // UINT
        else if (field is UintField uintField)
        {
            uint fieldValue = uintField.Values[idx].Value;
            uint uintInput = fieldValue;
            oldValue = fieldValue;

            var strval = $@"{uintInput}";

            if (isBool)
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputText($"##value{idx}", ref strval, 16))
                {
                    bool result = uint.TryParse(strval, out uintInput);

                    if (result)
                    {
                        newValue = uintInput;
                        _editedValueCache = newValue;
                        _changedCache = true;
                    }
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // SHORT
        else if (field is ShortField shortField)
        {
            short fieldValue = shortField.Values[idx].Value;
            int shortInput = fieldValue;
            oldValue = fieldValue;

            if (isBool)
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref shortInput))
                {
                    newValue = shortInput;
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        else if (field is UshortField ushortField)
        {
            ushort fieldValue = ushortField.Values[idx].Value;
            int shortInput = fieldValue;
            oldValue = fieldValue;

            if (isBool)
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref shortInput))
                {
                    newValue = shortInput;
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // SBYTE
        else if (field is SbyteField sbyteField)
        {
            sbyte fieldValue = sbyteField.Values[idx].Value;
            int sbyteInput = fieldValue;
            oldValue = fieldValue;

            if (isBool)
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref sbyteInput))
                {
                    newValue = sbyteInput;
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // BYTE
        else if (field is ByteField byteField)
        {
            byte fieldValue = byteField.Values[idx].Value;
            byte byteInput = fieldValue;
            oldValue = fieldValue;

            var strval = $@"{byteInput}";

            if (isBool)
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }
                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputText($"##value{idx}", ref strval, 16))
                {
                    bool result = byte.TryParse(strval, out byteInput);

                    if (result)
                    {
                        newValue = byteInput;
                        _editedValueCache = newValue;
                        _changedCache = true;
                    }
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // LONG
        else if (field is LongField longField)
        {
            long fieldValue = longField.Values[idx].Value;
            int intInput = (int)fieldValue;
            oldValue = fieldValue;

            if (isBool)
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }

                    _editedValueCache = newValue;
                    _changedCache = true;
                }

                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref intInput))
                {
                    newValue = intInput;

                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // ULONG
        else if (field is UlongField ulongField)
        {
            ulong fieldValue = ulongField.Values[idx].Value;
            int intInput = (int)fieldValue;
            oldValue = fieldValue;

            if (isBool)
            {
                bool boolInput = false;
                if (fieldValue > 0)
                    boolInput = true;

                if (ImGui.Checkbox($"##value{idx}", ref boolInput))
                {
                    if (boolInput)
                    {
                        newValue = 1;
                    }
                    else
                    {
                        newValue = 0;
                    }

                    _editedValueCache = newValue;
                    _changedCache = true;
                }

                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
            else
            {
                if (ImGui.InputInt($"##value{idx}", ref intInput))
                {
                    newValue = intInput;

                    _editedValueCache = newValue;
                    _changedCache = true;
                }
                _committedCache = ImGui.IsItemDeactivatedAfterEdit();
            }
        }
        // BOOL
        else if (field is BoolField boolField)
        {
            bool fieldValue = boolField.Values[idx].Value;
            bool boolInput = fieldValue;
            oldValue = fieldValue;

            if (ImGui.Checkbox($"##value{idx}", ref boolInput))
            {
                newValue = boolInput;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        // FLOAT
        else if (field is FloatField floatField)
        {
            float fieldValue = floatField.Values[idx].Value;
            float floatInput = fieldValue;
            oldValue = fieldValue;

            if (ImGui.InputFloat($"##value{idx}", ref floatInput, 0.1f, 1.0f, Utils.ImGui_InputFloatFormat(floatInput)))
            {
                newValue = floatInput;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        // VECTOR2
        else if (field is Vector2Field vector2Field)
        {
            Vector2 fieldValue = vector2Field.Values[idx].Value;
            Vector2 vector2Input = fieldValue;
            oldValue = fieldValue;

            if (ImGui.InputFloat2($"##value{idx}", ref vector2Input))
            {
                newValue = vector2Input;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        // VECTOR3
        else if (field is Vector3Field vector3Field)
        {
            Vector3 fieldValue = vector3Field.Values[idx].Value;
            Vector3 vector3Input = fieldValue;
            oldValue = fieldValue;

            if (ImGui.InputFloat3($"##value{idx}", ref vector3Input))
            {
                newValue = vector3Input;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        // VECTOR4
        else if (field is Vector4Field vector4Field &&
            !CFG.Current.GparamEditor_Value_List_Display_Color_Edit_V4)
        {
            Vector4 fieldValue = vector4Field.Values[idx].Value;
            Vector4 vector4Input = fieldValue;
            oldValue = fieldValue;

            if (ImGui.InputFloat4($"##value{idx}", ref vector4Input))
            {
                newValue = vector4Input;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        // VECTOR4 (COLOR EDIT)
        else if (field is Vector4Field vectorColorField &&
            CFG.Current.GparamEditor_Value_List_Display_Color_Edit_V4)
        {
            Vector4 fieldValue = vectorColorField.Values[idx].Value;
            Vector4 colorInput = fieldValue;
            oldValue = fieldValue;

            if (!_isHoldingColor)
            {
                _isHoldingColor = true;
                _heldColor = (Vector4)oldValue;
            }

            var flags = ImGuiColorEditFlags.AlphaOpaque;

            if (CFG.Current.GparamEditor_Color_Edit_Mode is ColorEditDisplayMode.RGB)
            {
                flags = flags | ImGuiColorEditFlags.DisplayRgb;
            }
            if (CFG.Current.GparamEditor_Color_Edit_Mode is ColorEditDisplayMode.Decimal)
            {
                flags = flags | ImGuiColorEditFlags.Float;
            }
            if (CFG.Current.GparamEditor_Color_Edit_Mode is ColorEditDisplayMode.HSV)
            {
                flags = flags | ImGuiColorEditFlags.DisplayHsv;
            }

            if (ImGui.ColorEdit4($"##value{idx}", ref colorInput, flags))
            {
                newValue = colorInput;
                _editedValueCache = newValue;
                _changedCache = true;
                _uncommittedCache = true;
            }

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                newValue = colorInput;
                _editedValueCache = newValue;
                _changedCache = true;
                _committedCache = true;
            }
        }
        // COLOR
        else if (field is ColorField colorField)
        {
            Color raw = colorField.Values[idx].Value;
            Vector4 fieldValue = new(raw.R / 255.0f, raw.G / 255.0f, raw.B / 255.0f, raw.A / 255.0f);
            Vector4 colorInput = fieldValue;
            oldValue = fieldValue;

            var flags = ImGuiColorEditFlags.AlphaOpaque;

            if (ImGui.ColorEdit4($"##value{idx}", ref colorInput, flags))
            {
                Color trueColorInput = Color.FromArgb(
                    (int)(colorInput.W * 255.0f),
                    (int)(colorInput.X * 255.0f),
                    (int)(colorInput.Y * 255.0f),
                    (int)(colorInput.Z * 255.0f)
                    );

                newValue = trueColorInput;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        // String
        else if (field is StringField strfield)
        {
            string fieldValue = strfield.Values[idx].Value;
            string strInput = fieldValue;
            oldValue = fieldValue;

            if (ImGui.InputText($"##value{idx}", ref strInput, 255))
            {
                newValue = strInput;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else
        {
            Smithbox.Log(this, $"{field.Name} {field.GetType()} is not supported.", LogLevel.Warning);
        }

        // Update and Commit
        if (_editedValueCache != null && _editedValueCache != oldValue)
        {
            _changedCache = true;
        }

        if (_changedCache)
        {
            if (_committedCache)
            {
                if (_isHoldingColor)
                {
                    _isHoldingColor = false;

                    // Reset color to original color befor edit
                    // so undo reverts in the expected fashion
                    if (field is Vector4Field vector4Field)
                    {
                        vector4Field.Values[idx].Value = _heldColor;
                    }
                }

                if (newValue == null)
                {
                    return;
                }
                else
                {
                    var action = new EditValueAction(Project, data, group, field, 
                        new List<IFieldValue>() { value }, newValue, ValueChangeType.Set);
                    Parent.ActionManager.ExecuteAction(action);
                }
            }
            // Only used for Vec4 color
            else if (_uncommittedCache)
            {
                if (newValue == null)
                {
                    return;
                }
                else
                {
                    if (field is Vector4Field vector4Field)
                    {
                        var assignedValue = (Vector4)newValue;
                        var result = assignedValue;

                        vector4Field.Values[idx].Value = result;
                    }
                }
            }
        }
    }

    public unsafe void IdField(GPARAM data, Param group, IField field, IFieldValue value, int idx)
    {
        _changedCache = false;
        _committedCache = false;

        ImGui.SetNextItemWidth(-1);

        object oldValue = null;
        object newValue = null;

        int fieldValue = field.Values[idx].ID;
        int intInput = fieldValue;
        oldValue = fieldValue;

        if (ImGui.InputInt($"##id{idx}", ref intInput))
        {
            newValue = intInput;
            _editedValueCache = newValue;
            _changedCache = true;
        }
        _committedCache = ImGui.IsItemDeactivatedAfterEdit();

        // Update and Commit
        if (_editedValueCache != null && _editedValueCache != oldValue)
        {
            _changedCache = true;
        }

        if (_changedCache)
        {
            if (_committedCache)
            {
                if (newValue == null)
                {
                    return;
                }
                else
                {
                    var action = new EditValueIdAction(Project, data, group, field, new List<IFieldValue>() { value }, (int)newValue);

                    Parent.ActionManager.ExecuteAction(action);
                }
            }
        }
    }

    public unsafe void TimeOfDayField(GPARAM data, Param group, IField field, IFieldValue value, int idx)
    {
        _changedCache = false;
        _committedCache = false;

        ImGui.SetNextItemWidth(-1);

        object oldValue = null;
        object newValue = null;

        float fieldValue = field.Values[idx].TimeOfDay;
        float floatInput = fieldValue;
        oldValue = fieldValue;

        if (ImGui.InputFloat($"##tod{idx}", ref floatInput))
        {
            newValue = floatInput;
            _editedValueCache = newValue;
            _changedCache = true;
        }
        _committedCache = ImGui.IsItemDeactivatedAfterEdit();

        // Update and Commit
        if (_editedValueCache != null && _editedValueCache != oldValue)
        {
            _changedCache = true;
        }

        if (_changedCache)
        {
            if (_committedCache)
            {
                if (newValue == null)
                {
                    return;
                }
                else
                {
                    var action = new EditValueTimeOfDayAction(Project, data, group, field, new List<IFieldValue>() { value }, (float)newValue);

                    Parent.ActionManager.ExecuteAction(action);
                }
            }
        }
    }

}
