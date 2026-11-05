using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.GparamEditor;

public class AddGroupAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }

    // Built once, so a redo puts back the same objects that later actions on the redo stack refer to
    private List<GPARAM.Param> NewGroups { get; set; } = new();

    private List<GPARAM.UnkParamExtra> StoredParamExtras { get; set; } = new();

    public AddGroupAction(ProjectEntry project, GPARAM data, List<GparamAnnotationEntry> groups)
    {
        Project = project;
        Data = data;

        foreach (var entry in groups)
        {
            NewGroups.Add(GparamConstructUtils.AddNewParam(Project, entry));
        }
    }

    public override ActionEvent Execute()
    {
        if (NewGroups.Count == 0)
            return ActionEvent.NoEvent;

        StoredParamExtras = GparamUtils.SnapshotParamExtras(Data);
        var previousParams = new List<GPARAM.Param>(Data.Params);

        Data.Params.AddRange(NewGroups);

        GparamUtils.RebuildParamExtras(Data, previousParams);

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        if (NewGroups.Count == 0)
            return ActionEvent.NoEvent;

        // By reference, not by index, in case the list has moved since
        foreach (var group in NewGroups)
        {
            Data.Params.Remove(group);
        }

        Data.UnkParamExtras = StoredParamExtras;

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }
}
