using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class AddValueAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM.Param TargetGroup { get; set; }
    private GPARAM.IField TargetField { get; set; }
    private List<GPARAM.IFieldValue> TargetValues { get; set; }
    private List<GPARAM.IFieldValue> StoredValues { get; set; } = new();
    private List<UnkParamExtra> StoredParamExtras { get; set; } = new();

    private bool UseDuplicateOffset { get; set; }
    private int DuplicateID { get; set; }
    private int DuplicateOffset { get; set; }

    public AddValueAction(ProjectEntry project, GPARAM data, GPARAM.Param group, GPARAM.IField field,
        List<GPARAM.IFieldValue> values, int duplicateID, int duplicateOffset, bool useDuplicateOffset)
    {
        Project = project;
        Data = data;
        TargetGroup = group;
        TargetField = field;
        TargetValues = values;
        DuplicateID = duplicateID;
        DuplicateOffset = duplicateOffset;
        UseDuplicateOffset = useDuplicateOffset;
    }

    public override ActionEvent Execute()
    {
        StoredValues = new List<GPARAM.IFieldValue>();

        DispatchOnField(TargetField, TargetValues, add: true);

        StoredParamExtras = new List<UnkParamExtra>(Data.UnkParamExtras);
        UpdateGroupIndexes(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        DispatchOnField(TargetField, StoredValues, add: false);
        Data.UnkParamExtras = StoredParamExtras;

        return ActionEvent.ObjectAddedRemoved;
    }

    private void DispatchOnField(GPARAM.IField field, List<GPARAM.IFieldValue> values, bool add)
    {
        switch (field)
        {
            case SbyteField f: Mutate(f.Values, values, add); break;
            case ByteField f: Mutate(f.Values, values, add); break;
            case ShortField f: Mutate(f.Values, values, add); break;
            case UshortField f: Mutate(f.Values, values, add); break;
            case IntField f: Mutate(f.Values, values, add); break;
            case UintField f: Mutate(f.Values, values, add); break;
            case LongField f: Mutate(f.Values, values, add); break;
            case UlongField f: Mutate(f.Values, values, add); break;
            case FloatField f: Mutate(f.Values, values, add); break;
            case DoubleField f: Mutate(f.Values, values, add); break;
            case BoolField f: Mutate(f.Values, values, add); break;
            case Vector2Field f: Mutate(f.Values, values, add); break;
            case Vector3Field f: Mutate(f.Values, values, add); break;
            case Vector4Field f: Mutate(f.Values, values, add); break;
            case ColorField f: Mutate(f.Values, values, add); break;
            case StringField f: Mutate(f.Values, values, add); break;
        }
    }

    private void Mutate<T>(List<FieldValue<T>> fieldValues, List<GPARAM.IFieldValue> targets, bool add)
    {
        if (add)
        {
            int insertIndex = -1;
            foreach (var target in targets)
            {
                int idx = fieldValues.IndexOf((FieldValue<T>)target);
                if (idx > insertIndex)
                    insertIndex = idx;
            }

            insertIndex = insertIndex < 0 ? fieldValues.Count : insertIndex + 1;

            foreach (var target in targets)
            {
                var newVal = new FieldValue<T>
                {
                    TimeOfDay = target.TimeOfDay,
                    Value = (T)target.Value,
                    ID = UseDuplicateOffset ? target.ID + DuplicateOffset : DuplicateID
                };

                fieldValues.Insert(insertIndex, newVal);
                StoredValues.Add(newVal);
                insertIndex++;
            }
        }
        else
        {
            foreach (var target in targets)
            {
                fieldValues.Remove((FieldValue<T>)target);
            }
        }
    }

    private void UpdateGroupIndexes(GPARAM gparam)
    {
        var newGroupIndexes = new List<UnkParamExtra>();
        int idx = 0;

        foreach (var param in gparam.Params)
        {
            var entry = new UnkParamExtra
            {
                GroupIndex = idx,
                Unk0c = 0
            };

            foreach (var field in param.Fields)
            {
                foreach (var val in field.Values)
                {
                    if (!entry.Ids.Contains(val.ID))
                    {
                        entry.Ids.Add(val.ID);
                    }
                }
            }

            newGroupIndexes.Add(entry);
            ++idx;
        }

        gparam.UnkParamExtras = newGroupIndexes;
    }
}