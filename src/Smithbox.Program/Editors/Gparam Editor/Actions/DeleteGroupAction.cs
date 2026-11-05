using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

public class DeleteGroupAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private List<GPARAM.Param> TargetGroups { get; set; }

    // Each removed group with the index it was at, in ascending index order
    private List<(int Index, GPARAM.Param Group)> Removed { get; set; } = new();

    private List<GPARAM.UnkParamExtra> StoredParamExtras { get; set; } = new();

    public DeleteGroupAction(ProjectEntry project, GPARAM data, List<GPARAM.Param> groups)
    {
        Project = project;
        Data = data;
        TargetGroups = new List<GPARAM.Param>(groups);
    }

    public override ActionEvent Execute()
    {
        // Only the groups actually in the file, so the undo restores exactly what was removed
        Removed = TargetGroups
            .Where(e => e != null)
            .Distinct()
            .Select(e => (Index: Data.Params.IndexOf(e), Group: e))
            .Where(e => e.Index >= 0)
            .OrderBy(e => e.Index)
            .ToList();

        if (Removed.Count == 0)
            return ActionEvent.NoEvent;

        StoredParamExtras = GparamUtils.SnapshotParamExtras(Data);
        var previousParams = new List<GPARAM.Param>(Data.Params);

        // Highest first, so the lower indices stay valid
        for (int i = Removed.Count - 1; i >= 0; i--)
        {
            Data.Params.RemoveAt(Removed[i].Index);
        }

        // The extras point at groups by index, which have just shifted
        GparamUtils.RebuildParamExtras(Data, previousParams);

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        if (Removed.Count == 0)
            return ActionEvent.NoEvent;

        // Lowest first: each index is where the group sat with the lower ones already back in place
        foreach (var entry in Removed)
        {
            var index = System.Math.Min(entry.Index, Data.Params.Count);
            Data.Params.Insert(index, entry.Group);
        }

        Data.UnkParamExtras = StoredParamExtras;

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }
}
