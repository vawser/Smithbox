using StudioCore.Application;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class EditValueAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM.Param TargetGroup { get; set; }
    private GPARAM.IField TargetField { get; set; }
    private List<GPARAM.IFieldValue> TargetValues { get; set; }
    private List<object> StoredOldValues { get; set; } = new();
    private object NewValue { get; set; }
    private ValueChangeType ChangeType { get; set; }

    public EditValueAction(ProjectEntry project, GPARAM data, GPARAM.Param group, GPARAM.IField field,
        List<GPARAM.IFieldValue> values, object newValue, ValueChangeType changeType)
    {
        Project = project;
        Data = data;
        TargetGroup = group;
        TargetField = field;
        TargetValues = values;
        NewValue = newValue;
        ChangeType = changeType;
    }

    public override ActionEvent Execute()
    {
        StoredOldValues = new List<object>();
        DispatchOnField(TargetField, TargetValues, true);
        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        DispatchOnField(TargetField, TargetValues, false);
        return ActionEvent.ObjectAddedRemoved;
    }

    private void DispatchOnField(GPARAM.IField field, List<GPARAM.IFieldValue> values, bool apply)
    {
        switch (field)
        {
            case SbyteField f: Mutate(f.Values, values, apply); break;
            case ByteField f: Mutate(f.Values, values, apply); break;
            case ShortField f: Mutate(f.Values, values, apply); break;
            case UshortField f: Mutate(f.Values, values, apply); break;
            case IntField f: Mutate(f.Values, values, apply); break;
            case UintField f: Mutate(f.Values, values, apply); break;
            case LongField f: Mutate(f.Values, values, apply); break;
            case UlongField f: Mutate(f.Values, values, apply); break;
            case FloatField f: Mutate(f.Values, values, apply); break;
            case DoubleField f: Mutate(f.Values, values, apply); break;
            case BoolField f: Mutate(f.Values, values, apply); break;
            case Vector2Field f: Mutate(f.Values, values, apply); break;
            case Vector3Field f: Mutate(f.Values, values, apply); break;
            case Vector4Field f: Mutate(f.Values, values, apply); break;
            case ColorField f: Mutate(f.Values, values, apply); break;
            case StringField f: Mutate(f.Values, values, apply); break;
        }
    }

    private void Mutate<T>(List<FieldValue<T>> fieldValues, List<GPARAM.IFieldValue> targets, bool apply)
    {
        if (apply)
        {
            foreach (var target in targets)
            {
                var fieldValue = (FieldValue<T>)target;
                StoredOldValues.Add(fieldValue.Value);
                fieldValue.Value = ApplyChange(fieldValue.Value, (T)NewValue);
            }
        }
        else
        {
            for (int i = 0; i < targets.Count; i++)
            {
                var fieldValue = (FieldValue<T>)targets[i];
                fieldValue.Value = (T)StoredOldValues[i];
            }
        }
    }

    private T ApplyChange<T>(T current, T incoming)
    {
        if (ChangeType == ValueChangeType.Set)
            return incoming;

        // Bool and string only support Set
        if (current is bool || current is string)
            return incoming;

        return ChangeType switch
        {
            ValueChangeType.Addition => ApplyArithmetic(current, incoming, (a, b) => a + b),
            ValueChangeType.Subtraction => ApplyArithmetic(current, incoming, (a, b) => a - b),
            ValueChangeType.Multiplication => ApplyArithmetic(current, incoming, (a, b) => a * b),
            _ => incoming
        };
    }

    private static T ApplyArithmetic<T>(T current, T incoming, Func<dynamic, dynamic, dynamic> op)
    {
        try
        {
            return (T)op((dynamic)current, (dynamic)incoming);
        }
        catch
        {
            return current;
        }
    }
}