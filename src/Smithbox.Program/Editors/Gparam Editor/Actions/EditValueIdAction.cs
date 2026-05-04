using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class EditValueIdAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM.Param TargetGroup { get; set; }
    private GPARAM.IField TargetField { get; set; }
    private List<GPARAM.IFieldValue> TargetValues { get; set; }

    private List<UnkParamExtra> StoredParamExtras { get; set; } = new();

    private List<int> StoredOldValues { get; set; } = new();
    private int NewValue { get; set; }

    public EditValueIdAction(ProjectEntry project, GPARAM data, GPARAM.Param group, GPARAM.IField field,
        List<GPARAM.IFieldValue> values, int newValue)
    {
        Project = project;
        Data = data;
        TargetGroup = group;
        TargetField = field;
        TargetValues = values;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        StoredOldValues = new List<int>();

        foreach (var val in TargetValues)
        {
            StoredOldValues.Add(val.ID);
            val.ID = NewValue;
        }

        StoredParamExtras = Data.UnkParamExtras.Select(x => x.Clone()).ToList();

        UpdateGroupIndexes(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < TargetValues.Count; i++)
        {
            TargetValues[i].ID = StoredOldValues[i];
        }

        Data.UnkParamExtras = StoredParamExtras;

        return ActionEvent.ObjectAddedRemoved;
    }
    private void UpdateGroupIndexes(GPARAM gparam)
    {
        var newGroupIndexes = new List<UnkParamExtra>();
        int idx = 0;

        foreach (var group in gparam.Params)
        {
            var entry = new UnkParamExtra
            {
                GroupIndex = idx,
                Unk0c = 0
            };

            foreach (var field in group.Fields)
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
