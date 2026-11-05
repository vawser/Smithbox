using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

public class DeleteValueAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM.Param TargetGroup { get; set; }
    private GPARAM.IField TargetField { get; set; }
    private List<GPARAM.IFieldValue> TargetValues { get; set; }

    // Each removed value with the index it was at, in ascending index order
    private List<(int Index, GPARAM.IFieldValue Value)> Removed { get; set; } = new();

    private List<GPARAM.UnkParamExtra> StoredParamExtras { get; set; } = new();

    public DeleteValueAction(ProjectEntry project, GPARAM data, GPARAM.Param group, GPARAM.IField field, List<GPARAM.IFieldValue> values)
    {
        Project = project;
        Data = data;
        TargetGroup = group;
        TargetField = field;
        TargetValues = new List<GPARAM.IFieldValue>(values);
    }

    public override ActionEvent Execute()
    {
        // Only the values actually in the field: one already gone would otherwise be inserted again by the undo
        Removed = TargetValues
            .Where(e => e != null)
            .Distinct()
            .Select(e => (Index: TargetField.IndexOfValue(e), Value: e))
            .Where(e => e.Index >= 0)
            .OrderBy(e => e.Index)
            .ToList();

        if (Removed.Count == 0)
            return ActionEvent.NoEvent;

        StoredParamExtras = GparamUtils.SnapshotParamExtras(Data);

        foreach (var entry in Removed)
        {
            TargetField.RemoveValue(entry.Value);
        }

        GparamUtils.RebuildParamExtras(Data);

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        if (Removed.Count == 0)
            return ActionEvent.NoEvent;

        // Lowest first: each index is where the value sat with the lower ones already back in place
        foreach (var entry in Removed)
        {
            TargetField.InsertValueAt(entry.Index, entry.Value);
        }

        Data.UnkParamExtras = StoredParamExtras;

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }
}
