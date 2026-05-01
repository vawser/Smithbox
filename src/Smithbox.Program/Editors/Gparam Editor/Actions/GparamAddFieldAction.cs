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

public class GparamAddFieldAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM CurrentGparam { get; set; }
    private GPARAM.Param StoredParam { get; set; }
    private GPARAM.Param TargetParam { get; set; }
    private List<GparamAnnotationFieldEntry> EntryList { get; set; }

    public GparamAddFieldAction(ProjectEntry project, GPARAM data, GPARAM.Param param, List<GparamAnnotationFieldEntry> targetEntries)
    {
        Project = project;
        CurrentGparam = data;
        StoredParam = param.Clone();
        TargetParam = param;
        EntryList = targetEntries;
    }

    public override ActionEvent Execute()
    {
        foreach (var entry in EntryList)
        {
            var newField = GparamConstructUtils.AddNewField(entry);
            TargetParam.Fields.Add(newField);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for(int i = 0; i < CurrentGparam.Params.Count; i++)
        {
            var curParam = CurrentGparam.Params[i];

            if(curParam.Key == StoredParam.Key)
            {
                CurrentGparam.Params[i] = StoredParam;
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
