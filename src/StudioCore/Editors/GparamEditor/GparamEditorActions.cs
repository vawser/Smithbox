using Google.Protobuf.Reflection;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.GraphicsEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class GparamEditorActions
{
    /// <summary>
    /// GPARAM Editor Actions
    /// </summary>
    /// 
    public class GparamValueChangeAction : EditorAction
    {
        public enum ValueChangeType
        {
            Set,
            Addition,
            Subtraction,
            Multiplication
        }

        private readonly List<GparamValueChange> Changes = new();

        public GparamValueChangeAction(IField field, IFieldValue fieldValue, object newValue, int index, ValueChangeType valueChangeType)
        {
            var change = new GparamValueChange();
            change.Index = index;
            change.Field = field;
            change.OldValue = fieldValue.Value;
            change.NewValue = newValue;
            change.ValueChangeType = valueChangeType;
            Changes.Add(change);
        }

        public override ActionEvent Execute()
        {
            foreach (GparamValueChange change in Changes)
            {
                if (change.Field is GPARAM.IntField intField)
                {
                    var assignedValue = (int)change.NewValue;
                    var result = assignedValue;

                    if (change.ValueChangeType == ValueChangeType.Set)
                    {
                        result = assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Addition)
                    {
                        var currVal = intField.Values[change.Index].Value;
                        result = currVal + assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Subtraction)
                    {
                        var currVal = intField.Values[change.Index].Value;
                        result = currVal - assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Multiplication)
                    {
                        var currVal = intField.Values[change.Index].Value;
                        result = currVal * assignedValue;
                    }

                    if (result > int.MaxValue)
                        result = int.MaxValue;

                    if (result < int.MinValue)
                        result = int.MinValue;

                    intField.Values[change.Index].Value = result;
                }
                if (change.Field is GPARAM.UintField uintField)
                {
                    var assignedValue = (uint)change.NewValue;
                    var result = assignedValue;

                    if (change.ValueChangeType == ValueChangeType.Set)
                    {
                        result = assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Addition)
                    {
                        var currVal = uintField.Values[change.Index].Value;
                        result = currVal + assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Subtraction)
                    {
                        var currVal = uintField.Values[change.Index].Value;
                        result = currVal - assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Multiplication)
                    {
                        var currVal = uintField.Values[change.Index].Value;
                        result = currVal * assignedValue;
                    }

                    if (result > uint.MaxValue)
                        result = uint.MaxValue;

                    if (result < uint.MinValue)
                        result = uint.MinValue;

                    uintField.Values[change.Index].Value = result;
                }
                if (change.Field is GPARAM.ShortField shortField)
                {
                    var assignedValue = (short)change.NewValue;
                    var result = assignedValue;

                    if (change.ValueChangeType == ValueChangeType.Set)
                    {
                        result = assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Addition)
                    {
                        var currVal = shortField.Values[change.Index].Value;
                        result = (short)(currVal + assignedValue);
                    }
                    if (change.ValueChangeType == ValueChangeType.Subtraction)
                    {
                        var currVal = shortField.Values[change.Index].Value;
                        result = (short)(currVal - assignedValue);
                    }
                    if (change.ValueChangeType == ValueChangeType.Multiplication)
                    {
                        var currVal = shortField.Values[change.Index].Value;
                        result = (short)(currVal * assignedValue);
                    }

                    if (result > short.MaxValue)
                        result = short.MaxValue;

                    if (result < short.MinValue)
                        result = short.MinValue;

                    shortField.Values[change.Index].Value = result;
                }
                if (change.Field is GPARAM.SbyteField sbyteField)
                {
                    var assignedValue = (sbyte)change.NewValue;
                    var result = assignedValue;

                    if (change.ValueChangeType == ValueChangeType.Set)
                    {
                        result = assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Addition)
                    {
                        var currVal = sbyteField.Values[change.Index].Value;
                        result = (sbyte)(currVal + assignedValue);
                    }
                    if (change.ValueChangeType == ValueChangeType.Subtraction)
                    {
                        var currVal = sbyteField.Values[change.Index].Value;
                        result = (sbyte)(currVal - assignedValue);
                    }
                    if (change.ValueChangeType == ValueChangeType.Multiplication)
                    {
                        var currVal = sbyteField.Values[change.Index].Value;
                        result = (sbyte)(currVal * assignedValue);
                    }

                    if (result > sbyte.MaxValue)
                        result = sbyte.MaxValue;

                    if (result < sbyte.MinValue)
                        result = sbyte.MinValue;

                    sbyteField.Values[change.Index].Value = result;
                }
                if (change.Field is GPARAM.ByteField byteField)
                {
                    var assignedValue = (byte)change.NewValue;
                    var result = assignedValue;

                    if (change.ValueChangeType == ValueChangeType.Set)
                    {
                        result = assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Addition)
                    {
                        var currVal = byteField.Values[change.Index].Value;
                        result = (byte)(currVal + assignedValue);
                    }
                    if (change.ValueChangeType == ValueChangeType.Subtraction)
                    {
                        var currVal = byteField.Values[change.Index].Value;
                        result = (byte)(currVal - assignedValue);
                    }
                    if (change.ValueChangeType == ValueChangeType.Multiplication)
                    {
                        var currVal = byteField.Values[change.Index].Value;
                        result = (byte)(currVal * assignedValue);
                    }

                    if (result > byte.MaxValue)
                        result = byte.MaxValue;

                    if (result < byte.MinValue)
                        result = byte.MinValue;

                    byteField.Values[change.Index].Value = result;
                }
                if (change.Field is GPARAM.BoolField boolField)
                {
                    var result = boolField.Values[change.Index].Value;
                    if (bool.TryParse(change.NewValue.ToString(), out result))
                    {
                        boolField.Values[change.Index].Value = result;
                    }
                }
                if (change.Field is GPARAM.FloatField floatField)
                {
                    var assignedValue = (float)change.NewValue;
                    var result = assignedValue;

                    if (change.ValueChangeType == ValueChangeType.Set)
                    {
                        result = assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Addition)
                    {
                        var currVal = floatField.Values[change.Index].Value;
                        result = (float)(currVal + assignedValue);
                    }
                    if (change.ValueChangeType == ValueChangeType.Subtraction)
                    {
                        var currVal = floatField.Values[change.Index].Value;
                        result = (float)(currVal - assignedValue);
                    }
                    if (change.ValueChangeType == ValueChangeType.Multiplication)
                    {
                        var currVal = floatField.Values[change.Index].Value;
                        result = (float)(currVal * assignedValue);
                    }

                    if (result > float.MaxValue)
                        result = float.MaxValue;

                    if (result < float.MinValue)
                        result = float.MinValue;

                    floatField.Values[change.Index].Value = result;
                }
                if (change.Field is GPARAM.Vector2Field vector2Field)
                {
                    var assignedValue = (Vector2)change.NewValue;
                    var result = assignedValue;

                    if (change.ValueChangeType == ValueChangeType.Set)
                    {
                        result = assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Addition)
                    {
                        var currVal = vector2Field.Values[change.Index].Value;
                        var newX = currVal.X = currVal.X + assignedValue.X;
                        var newY = currVal.Y = currVal.Y + assignedValue.Y;
                        result = new Vector2(newX, newY);
                    }
                    if (change.ValueChangeType == ValueChangeType.Subtraction)
                    {
                        var currVal = vector2Field.Values[change.Index].Value;
                        var newX = currVal.X = currVal.X - assignedValue.X;
                        var newY = currVal.Y = currVal.Y - assignedValue.Y;
                        result = new Vector2(newX, newY);
                    }
                    if (change.ValueChangeType == ValueChangeType.Multiplication)
                    {
                        var currVal = vector2Field.Values[change.Index].Value;
                        var newX = currVal.X = currVal.X * assignedValue.X;
                        var newY = currVal.Y = currVal.Y * assignedValue.Y;
                        result = new Vector2(newX, newY);
                    }

                    if (result[0] > float.MaxValue)
                        result[0] = float.MaxValue;

                    if (result[0] < float.MinValue)
                        result[0] = float.MinValue;

                    if (result[1] > float.MaxValue)
                        result[1] = float.MaxValue;

                    if (result[1] < float.MinValue)
                        result[1] = float.MinValue;

                    vector2Field.Values[change.Index].Value = result;
                }
                if (change.Field is GPARAM.Vector3Field vector3Field)
                {
                    var assignedValue = (Vector3)change.NewValue;
                    var result = assignedValue;

                    if (change.ValueChangeType == ValueChangeType.Set)
                    {
                        result = assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Addition)
                    {
                        var currVal = vector3Field.Values[change.Index].Value;
                        var newX = currVal.X = currVal.X + assignedValue.X;
                        var newY = currVal.Y = currVal.Y + assignedValue.Y;
                        var newZ = currVal.Z = currVal.Z + assignedValue.Z;
                        result = new Vector3(newX, newY, newZ);
                    }
                    if (change.ValueChangeType == ValueChangeType.Subtraction)
                    {
                        var currVal = vector3Field.Values[change.Index].Value;
                        var newX = currVal.X = currVal.X - assignedValue.X;
                        var newY = currVal.Y = currVal.Y - assignedValue.Y;
                        var newZ = currVal.Z = currVal.Z - assignedValue.Z;
                        result = new Vector3(newX, newY, newZ);
                    }
                    if (change.ValueChangeType == ValueChangeType.Multiplication)
                    {
                        var currVal = vector3Field.Values[change.Index].Value;
                        var newX = currVal.X = currVal.X * assignedValue.X;
                        var newY = currVal.Y = currVal.Y * assignedValue.Y;
                        var newZ = currVal.Z = currVal.Z * assignedValue.Z;
                        result = new Vector3(newX, newY, newZ);
                    }

                    if (result[0] > float.MaxValue)
                        result[0] = float.MaxValue;

                    if (result[0] < float.MinValue)
                        result[0] = float.MinValue;

                    if (result[1] > float.MaxValue)
                        result[1] = float.MaxValue;

                    if (result[1] < float.MinValue)
                        result[1] = float.MinValue;

                    if (result[2] > float.MaxValue)
                        result[2] = float.MaxValue;

                    if (result[2] < float.MinValue)
                        result[2] = float.MinValue;

                    vector3Field.Values[change.Index].Value = result;
                }
                if (change.Field is GPARAM.Vector4Field vector4Field)
                {
                    var assignedValue = (Vector4)change.NewValue;
                    var result = assignedValue;

                    if (change.ValueChangeType == ValueChangeType.Set)
                    {
                        result = assignedValue;
                    }
                    if (change.ValueChangeType == ValueChangeType.Addition)
                    {
                        var currVal = vector4Field.Values[change.Index].Value;
                        var newX = currVal.X = currVal.X + assignedValue.X;
                        var newY = currVal.Y = currVal.Y + assignedValue.Y;
                        var newZ = currVal.Z = currVal.Z + assignedValue.Z;
                        var newW = currVal.W = currVal.W + assignedValue.W;
                        result = new Vector4(newX, newY, newZ, newW);
                    }
                    if (change.ValueChangeType == ValueChangeType.Subtraction)
                    {
                        var currVal = vector4Field.Values[change.Index].Value;
                        var newX = currVal.X = currVal.X - assignedValue.X;
                        var newY = currVal.Y = currVal.Y - assignedValue.Y;
                        var newZ = currVal.Z = currVal.Z - assignedValue.Z;
                        var newW = currVal.W = currVal.W - assignedValue.W;
                        result = new Vector4(newX, newY, newZ, newW);
                    }
                    if (change.ValueChangeType == ValueChangeType.Multiplication)
                    {
                        var currVal = vector4Field.Values[change.Index].Value;
                        var newX = currVal.X = currVal.X * assignedValue.X;
                        var newY = currVal.Y = currVal.Y * assignedValue.Y;
                        var newZ = currVal.Z = currVal.Z * assignedValue.Z;
                        var newW = currVal.W = currVal.W * assignedValue.W;
                        result = new Vector4(newX, newY, newZ, newW);
                    }

                    if (result[0] > float.MaxValue)
                        result[0] = float.MaxValue;

                    if (result[0] < float.MinValue)
                        result[0] = float.MinValue;

                    if (result[1] > float.MaxValue)
                        result[1] = float.MaxValue;

                    if (result[1] < float.MinValue)
                        result[1] = float.MinValue;

                    if (result[2] > float.MaxValue)
                        result[2] = float.MaxValue;

                    if (result[2] < float.MinValue)
                        result[2] = float.MinValue;

                    if (result[3] > float.MaxValue)
                        result[3] = float.MaxValue;

                    if (result[3] < float.MinValue)
                        result[3] = float.MinValue;

                    vector4Field.Values[change.Index].Value = result;
                }
                if (change.Field is GPARAM.ColorField colorField)
                {
                    var assignedValue = (Color)change.NewValue;

                    colorField.Values[change.Index].Value = assignedValue;
                }
            }

            return ActionEvent.NoEvent;
        }

        public override ActionEvent Undo()
        {
            foreach (GparamValueChange change in Changes)
            {
                if (change.Field is GPARAM.IntField intField)
                {
                    intField.Values[change.Index].Value = (int)change.OldValue;
                }
                if (change.Field is GPARAM.UintField uintField)
                {
                    uintField.Values[change.Index].Value = (uint)change.OldValue;
                }
                if (change.Field is GPARAM.ShortField shortField)
                {
                    shortField.Values[change.Index].Value = (short)change.OldValue;
                }
                if (change.Field is GPARAM.SbyteField sbyteField)
                {
                    sbyteField.Values[change.Index].Value = (sbyte)change.OldValue;
                }
                if (change.Field is GPARAM.ByteField byteField)
                {
                    byteField.Values[change.Index].Value = (byte)change.OldValue;
                }
                if (change.Field is GPARAM.BoolField boolField)
                {
                    boolField.Values[change.Index].Value = (bool)change.OldValue;
                }
                if (change.Field is GPARAM.FloatField floatField)
                {
                    floatField.Values[change.Index].Value = (float)change.OldValue;
                }
                if (change.Field is GPARAM.Vector2Field vector2Field)
                {
                    vector2Field.Values[change.Index].Value = (Vector2)change.OldValue;
                }
                if (change.Field is GPARAM.Vector3Field vector3Field)
                {
                    vector3Field.Values[change.Index].Value = (Vector3)change.OldValue;
                }
                if (change.Field is GPARAM.Vector4Field vector4Field)
                {
                    vector4Field.Values[change.Index].Value = (Vector4)change.OldValue;
                }
                if (change.Field is GPARAM.ColorField colorField)
                {
                    colorField.Values[change.Index].Value = (Color)change.OldValue;
                }
            }

            return ActionEvent.NoEvent;
        }

        private class GparamValueChange
        {
            public int Index;
            public object NewValue;
            public object OldValue;
            public ValueChangeType ValueChangeType;
            public SoulsFormats.GPARAM.IField Field;
        }
    }

    public class GparamTimeOfDayChangeAction : EditorAction
    {
        private readonly List<GparamValueChange> Changes = new();

        public GparamTimeOfDayChangeAction(SoulsFormats.GPARAM.IField field, SoulsFormats.GPARAM.IFieldValue fieldValue, object newValue, int index)
        {
            var change = new GparamValueChange();
            change.Index = index;
            change.Field = field;
            change.OldValue = fieldValue.Unk04;
            change.NewValue = newValue;
            Changes.Add(change);
        }

        public override ActionEvent Execute()
        {
            foreach (GparamValueChange change in Changes)
            {
                if (change.Field is GPARAM.IntField intField)
                {
                    intField.Values[change.Index].Unk04 = (float)change.NewValue;
                }
                if (change.Field is GPARAM.UintField uintField)
                {
                    uintField.Values[change.Index].Unk04 = (float)change.NewValue;
                }
                if (change.Field is GPARAM.ShortField shortField)
                {
                    shortField.Values[change.Index].Unk04 = (float)change.NewValue;
                }
                if (change.Field is GPARAM.SbyteField sbyteField)
                {
                    sbyteField.Values[change.Index].Unk04 = (float)change.NewValue;
                }
                if (change.Field is GPARAM.ByteField byteField)
                {
                    byteField.Values[change.Index].Unk04 = (float)change.NewValue;
                }
                if (change.Field is GPARAM.BoolField boolField)
                {
                    boolField.Values[change.Index].Unk04 = (float)change.NewValue;
                }
                if (change.Field is GPARAM.FloatField floatField)
                {
                    floatField.Values[change.Index].Unk04 = (float)change.NewValue;
                }
                if (change.Field is GPARAM.Vector2Field vector2Field)
                {
                    vector2Field.Values[change.Index].Unk04 = (float)change.NewValue;
                }
                if (change.Field is GPARAM.Vector3Field vector3Field)
                {
                    vector3Field.Values[change.Index].Unk04 = (float)change.NewValue;
                }
                if (change.Field is GPARAM.Vector4Field vector4Field)
                {
                    vector4Field.Values[change.Index].Unk04 = (float)change.NewValue;
                }
                if (change.Field is GPARAM.ColorField colorField)
                {
                    colorField.Values[change.Index].Unk04 = (float)change.NewValue;
                }
            }

            return ActionEvent.NoEvent;
        }

        public override ActionEvent Undo()
        {
            foreach (GparamValueChange change in Changes)
            {
                if (change.Field is GPARAM.IntField intField)
                {
                    intField.Values[change.Index].Unk04 = (float)change.OldValue;
                }
                if (change.Field is GPARAM.UintField uintField)
                {
                    uintField.Values[change.Index].Unk04 = (float)change.OldValue;
                }
                if (change.Field is GPARAM.ShortField shortField)
                {
                    shortField.Values[change.Index].Unk04 = (float)change.OldValue;
                }
                if (change.Field is GPARAM.SbyteField sbyteField)
                {
                    sbyteField.Values[change.Index].Unk04 = (float)change.OldValue;
                }
                if (change.Field is GPARAM.ByteField byteField)
                {
                    byteField.Values[change.Index].Unk04 = (float)change.OldValue;
                }
                if (change.Field is GPARAM.BoolField boolField)
                {
                    boolField.Values[change.Index].Unk04 = (float)change.OldValue;
                }
                if (change.Field is GPARAM.FloatField floatField)
                {
                    floatField.Values[change.Index].Unk04 = (float)change.OldValue;
                }
                if (change.Field is GPARAM.Vector2Field vector2Field)
                {
                    vector2Field.Values[change.Index].Unk04 = (float)change.OldValue;
                }
                if (change.Field is GPARAM.Vector3Field vector3Field)
                {
                    vector3Field.Values[change.Index].Unk04 = (float)change.OldValue;
                }
                if (change.Field is GPARAM.Vector4Field vector4Field)
                {
                    vector4Field.Values[change.Index].Unk04 = (float)change.OldValue;
                }
                if (change.Field is GPARAM.ColorField colorField)
                {
                    colorField.Values[change.Index].Unk04 = (float)change.OldValue;
                }
            }

            return ActionEvent.NoEvent;
        }

        private class GparamValueChange
        {
            public int Index;
            public object NewValue;
            public object OldValue;
            public SoulsFormats.GPARAM.IField Field;
        }
    }

    public class GparamBatchChangeAction : EditorAction
    {
        private readonly List<GparamValueChangeAction> ChangeActions = new();

        public GparamBatchChangeAction(List<GparamValueChangeAction> actions)
        {
            ChangeActions = actions;
        }

        public override ActionEvent Execute()
        {
            foreach (GparamValueChangeAction action in ChangeActions)
            {
                action.Execute();
            }

            return ActionEvent.NoEvent;
        }

        public override ActionEvent Undo()
        {
            foreach (GparamValueChangeAction action in ChangeActions)
            {
                action.Undo();
            }

            return ActionEvent.NoEvent;
        }
    }

    public class GparamDuplicateValueRow : EditorAction
    {
        private GPARAM SelectedGPARAM;
        private GparamEditorScreen Screen;
        private IField SelectedField;
        private IFieldValue SelectedFieldValue;
        private int NewRowID;

        public GparamDuplicateValueRow(GparamEditorScreen screen, GPARAM selectedGparam, IField selectedField, IFieldValue fieldValue, int dupeRowId)
        {
            SelectedGPARAM = selectedGparam;
            Screen = screen;
            SelectedField = selectedField;
            SelectedFieldValue = fieldValue;
            NewRowID = dupeRowId;
        }

        public override ActionEvent Execute()
        {
            Screen.ExtendDisplayTruth(SelectedField);

            Screen.PropertyEditor.AddPropertyValueRow(SelectedField, SelectedFieldValue, NewRowID);

            // Update the group index lists to account for the new ID.
            Screen._selectedGparamInfo.WasModified = true;
            Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

            return ActionEvent.NoEvent;
        }

        public override ActionEvent Undo()
        {
            Screen.ReduceDisplayTruth(SelectedField);

            Screen.PropertyEditor.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, NewRowID);

            Screen._selectedGparamInfo.WasModified = false;
            Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

            return ActionEvent.NoEvent;
        }
    }

    public class GparamRemoveValueRow : EditorAction
    {
        private GPARAM SelectedGPARAM;
        private GparamEditorScreen Screen;
        private IField SelectedField;
        private IFieldValue SelectedFieldValue;
        private int RemovedRowID;
        private int RemovedRowIndex;

        public GparamRemoveValueRow(GparamEditorScreen screen, GPARAM selectedGparam, IField selectedField, IFieldValue fieldValue)
        {
            SelectedGPARAM = selectedGparam;
            Screen = screen;
            SelectedField = selectedField;
            SelectedFieldValue = fieldValue;
            RemovedRowID = fieldValue.Id;
        }

        public override ActionEvent Execute()
        {
            Screen.ReduceDisplayTruth(SelectedField);

            RemovedRowIndex = Screen.PropertyEditor.RemovePropertyValueRowById(SelectedField, SelectedFieldValue, SelectedFieldValue.Id);

            Screen._selectedGparamInfo.WasModified = true;
            Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

            return ActionEvent.NoEvent;
        }

        public override ActionEvent Undo()
        {
            Screen.ExtendDisplayTruth(SelectedField);

            Screen.PropertyEditor.AddPropertyValueRowAtIndex(SelectedField, SelectedFieldValue, RemovedRowID, RemovedRowIndex);

            Screen._selectedGparamInfo.WasModified = true;
            Screen.PropertyEditor.UpdateGroupIndexes(SelectedGPARAM);

            return ActionEvent.NoEvent;
        }
    }
}
