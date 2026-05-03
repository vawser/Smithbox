using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class AddFieldAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM.Param TargetGroup { get; set; }
    private List<GparamAnnotationFieldEntry> TargetFields { get; set; }
    private List<GPARAM.IField> StoredFields { get; set; } = new();

    public AddFieldAction(ProjectEntry project, GPARAM data, GPARAM.Param group, List<GparamAnnotationFieldEntry> fields)
    {
        Project = project;
        Data = data;
        TargetGroup = group;
        TargetFields = fields;
    }

    public override ActionEvent Execute()
    {
        foreach (var entry in TargetFields)
        {
            var newField = GparamConstructUtils.AddNewField(entry);
            TargetGroup.Fields.Add(newField);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for(int i = 0; i < Data.Params.Count; i++)
        {
            var curParam = Data.Params[i];

            if(curParam.Key == StoredGroup.Key)
            {
                Data.Params[i] = StoredGroup;
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
