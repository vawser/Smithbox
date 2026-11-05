using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

public class DeleteFieldAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM.Param TargetGroup { get; set; }
    private List<GPARAM.IField> TargetFields { get; set; }

    // Each removed field with the index it was at, in ascending index order
    private List<(int Index, GPARAM.IField Field)> Removed { get; set; } = new();

    private List<GPARAM.UnkParamExtra> StoredParamExtras { get; set; } = new();

    public DeleteFieldAction(ProjectEntry project, GPARAM data, GPARAM.Param group, List<GPARAM.IField> fields)
    {
        Project = project;
        Data = data;
        TargetGroup = group;
        TargetFields = new List<GPARAM.IField>(fields);
    }

    public override ActionEvent Execute()
    {
        // Only the fields actually in the group, so the undo restores exactly what was removed
        Removed = TargetFields
            .Where(e => e != null)
            .Distinct()
            .Select(e => (Index: TargetGroup.Fields.IndexOf(e), Field: e))
            .Where(e => e.Index >= 0)
            .OrderBy(e => e.Index)
            .ToList();

        if (Removed.Count == 0)
            return ActionEvent.NoEvent;

        StoredParamExtras = GparamUtils.SnapshotParamExtras(Data);

        // Highest first, so the lower indices stay valid
        for (int i = Removed.Count - 1; i >= 0; i--)
        {
            TargetGroup.Fields.RemoveAt(Removed[i].Index);
        }

        // The removed fields' value IDs may no longer be used by the group
        GparamUtils.RebuildParamExtras(Data);

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        if (Removed.Count == 0)
            return ActionEvent.NoEvent;

        // Lowest first: each index is where the field sat with the lower ones already back in place
        foreach (var entry in Removed)
        {
            var index = System.Math.Min(entry.Index, TargetGroup.Fields.Count);
            TargetGroup.Fields.Insert(index, entry.Field);
        }

        Data.UnkParamExtras = StoredParamExtras;

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }
}
