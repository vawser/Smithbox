using DotNext.Collections.Generic;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class DeleteValueAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM.Param TargetGroup { get; set; }
    private GPARAM.IField TargetField { get; set; }
    private List<GPARAM.IFieldValue> StoredValues { get; set; }
    private List<GPARAM.UnkParamExtra> StoredParamExtras { get; set; }

    private List<int> StoredIndices { get; set; } = new();

    public DeleteValueAction(ProjectEntry project, GPARAM data, GPARAM.Param group, GPARAM.IField field, List<GPARAM.IFieldValue> values)
    {
        Project = project;
        Data = data;
        TargetGroup = group;
        TargetField = field;
        StoredValues = new List<GPARAM.IFieldValue>(values);
    }

    public override ActionEvent Execute()
    {
        StoredIndices = new List<int>();
        StoredParamExtras = Data.UnkParamExtras.Select(x => x.Clone()).ToList();

        RemoveValues(TargetField, StoredValues);
        UpdateGroupIndexes(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        AddValues(TargetField, StoredValues);
        Data.UnkParamExtras = StoredParamExtras;

        return ActionEvent.ObjectAddedRemoved;
    }

    private void RemoveValues(GPARAM.IField field, List<GPARAM.IFieldValue> values)
    {
        DispatchOnField(field, values, true);
    }

    private void AddValues(GPARAM.IField field, List<GPARAM.IFieldValue> values)
    {
        DispatchOnField(field, values, false);
    }

    private void DispatchOnField(GPARAM.IField field, List<GPARAM.IFieldValue> values, bool remove)
    {
        switch (field)
        {
            case GPARAM.SbyteField f: Mutate(f.Values, values, remove); break;
            case GPARAM.ByteField f: Mutate(f.Values, values, remove); break;
            case GPARAM.ShortField f: Mutate(f.Values, values, remove); break;
            case GPARAM.UshortField f: Mutate(f.Values, values, remove); break;
            case GPARAM.IntField f: Mutate(f.Values, values, remove); break;
            case GPARAM.UintField f: Mutate(f.Values, values, remove); break;
            case GPARAM.LongField f: Mutate(f.Values, values, remove); break;
            case GPARAM.UlongField f: Mutate(f.Values, values, remove); break;
            case GPARAM.FloatField f: Mutate(f.Values, values, remove); break;
            case GPARAM.DoubleField f: Mutate(f.Values, values, remove); break;
            case GPARAM.BoolField f: Mutate(f.Values, values, remove); break;
            case GPARAM.Vector2Field f: Mutate(f.Values, values, remove); break;
            case GPARAM.Vector3Field f: Mutate(f.Values, values, remove); break;
            case GPARAM.Vector4Field f: Mutate(f.Values, values, remove); break;
            case GPARAM.ColorField f: Mutate(f.Values, values, remove); break;
            case GPARAM.StringField f: Mutate(f.Values, values, remove); break;
        }
    }

    private void Mutate<T>(List<GPARAM.FieldValue<T>> fieldValues, List<GPARAM.IFieldValue> targets, bool remove)
    {
        if (remove)
        {
            foreach (var target in targets)
            {
                int idx = fieldValues.IndexOf((FieldValue<T>)target);
                if (idx >= 0)
                {
                    StoredIndices.Add(idx);
                }
            }

            StoredIndices.Sort((a, b) => b.CompareTo(a));

            foreach (int idx in StoredIndices)
            {
                fieldValues.RemoveAt(idx);
            }
        }
        else
        {
            StoredIndices.Sort();

            for (int i = 0; i < targets.Count; i++)
            {
                int insertIndex = i < StoredIndices.Count
                    ? StoredIndices[i]
                    : fieldValues.Count;

                fieldValues.Insert(insertIndex, (FieldValue<T>)targets[i]);
            }
        }

        if (TargetField is Field<T> f)
        {
            f.Capacity = (short)fieldValues.Count;
        }
    }

    private void UpdateGroupIndexes(GPARAM gparam)
    {
        var newGroupIndexes = new List<GPARAM.UnkParamExtra>();
        int idx = 0;

        foreach (var param in gparam.Params)
        {
            var newGroupIndexList = new GPARAM.UnkParamExtra();
            newGroupIndexList.GroupIndex = idx;
            newGroupIndexList.Unk0c = 0;

            foreach (var field in param.Fields)
            {
                foreach (var val in field.Values)
                {
                    if (!newGroupIndexList.Ids.Contains(val.ID))
                    {
                        newGroupIndexList.Ids.Add(val.ID);
                    }
                }
            }

            newGroupIndexes.Add(newGroupIndexList);
            ++idx;
        }

        gparam.UnkParamExtras = newGroupIndexes;
    }
}