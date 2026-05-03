using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class DeleteGroupAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private List<GPARAM.Param> TargetGroups { get; set; } = new();
    private List<GPARAM.Param> StoredGroups { get; set; } = new();

    public DeleteGroupAction(ProjectEntry project, GPARAM data, List<GPARAM.Param> groups)
    {
        Project = project;
        Data = data;
        TargetGroups = groups;
        StoredGroups = new List<GPARAM.Param>(TargetGroups);
    }

    public override ActionEvent Execute()
    {
        foreach (var entry in TargetGroups)
        {
            Data.Params.Remove(entry);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        foreach (var entry in StoredGroups)
        {
            Data.Params.Add(entry);
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
