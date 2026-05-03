using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class AddGroupAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }

    private List<GparamAnnotationEntry> TargetGroups { get; set; } = new();
    private List<GPARAM.Param> StoredGroups { get; set; } = new();

    public AddGroupAction(ProjectEntry project, GPARAM data, List<GparamAnnotationEntry> groups)
    {
        Project = project;
        Data = data;
        TargetGroups = groups;

        foreach (var entry in TargetGroups)
        {
            var newParam = GparamConstructUtils.AddNewParam(Project, entry);
            StoredGroups.Add(newParam);
        }
    }

    public override ActionEvent Execute()
    {
        foreach(var entry in StoredGroups)
        {
            Data.Params.Add(entry);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        foreach(var entry in StoredGroups)
        {
            Data.Params.Remove(entry);
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
