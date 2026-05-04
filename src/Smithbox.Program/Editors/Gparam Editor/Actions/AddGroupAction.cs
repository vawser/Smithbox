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

    private List<int> StoredIndices { get; set; } = new();

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
        StoredIndices = new List<int>();

        int insertIndex = Data.Params.Count;

        foreach (var entry in StoredGroups)
        {
            Data.Params.Insert(insertIndex, entry);
            StoredIndices.Add(insertIndex);
            insertIndex++;
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (int i = StoredIndices.Count - 1; i >= 0; i--)
        {
            Data.Params.RemoveAt(StoredIndices[i]);
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
