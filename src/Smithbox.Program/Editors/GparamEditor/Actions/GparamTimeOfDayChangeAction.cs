using StudioCore.Editor;
using System.Collections.Generic;
using static SoulsFormats.GPARAM;

namespace StudioCore.GraphicsParamEditorNS;

public class GparamTimeOfDayChangeAction : EditorAction
{
    private readonly List<GparamValueChange> Changes = new();

    private string ProvenanceString;

    public GparamTimeOfDayChangeAction(string fileName, string groupName, IField field, IFieldValue fieldValue, object newValue, int index)
    {
        var change = new GparamValueChange();
        change.Index = index;
        change.Field = field;
        change.OldValue = fieldValue.Unk04;
        change.NewValue = newValue;
        Changes.Add(change);

        ProvenanceString = $"Param: {fileName} - Group: {groupName} - Field: {field.Name}";
    }

    public override ActionEvent Execute()
    {
        foreach (GparamValueChange change in Changes)
        {
            if (change.Field is IntField intField)
            {
                intField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is UintField uintField)
            {
                uintField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is ShortField shortField)
            {
                shortField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is SbyteField sbyteField)
            {
                sbyteField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is ByteField byteField)
            {
                byteField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is BoolField boolField)
            {
                boolField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is FloatField floatField)
            {
                floatField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is Vector2Field vector2Field)
            {
                vector2Field.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is Vector3Field vector3Field)
            {
                vector3Field.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is Vector4Field vector4Field)
            {
                vector4Field.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is ColorField colorField)
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
            if (change.Field is IntField intField)
            {
                intField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is UintField uintField)
            {
                uintField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is ShortField shortField)
            {
                shortField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is SbyteField sbyteField)
            {
                sbyteField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is ByteField byteField)
            {
                byteField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is BoolField boolField)
            {
                boolField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is FloatField floatField)
            {
                floatField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is Vector2Field vector2Field)
            {
                vector2Field.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is Vector3Field vector3Field)
            {
                vector3Field.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is Vector4Field vector4Field)
            {
                vector4Field.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is ColorField colorField)
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
        public IField Field;
    }
}