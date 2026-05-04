using StudioCore.Application;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.GparamEditor;

public class EditValueIdAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM.Param TargetGroup { get; set; }
    private GPARAM.IField TargetField { get; set; }
    private List<GPARAM.IFieldValue> TargetValues { get; set; }

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

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < TargetValues.Count; i++)
        {
            TargetValues[i].ID = StoredOldValues[i];
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
