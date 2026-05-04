using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class OverwriteGparamAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM NewData { get; set; }
    private GPARAM StoredData { get; set; }

    public OverwriteGparamAction(ProjectEntry project, GPARAM data, GPARAM newData)
    {
        Project = project;
        Data = data;
        StoredData = data.Clone();
        NewData = newData;
    }

    public override ActionEvent Execute()
    {
        Data = NewData;

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        Data = StoredData;

        return ActionEvent.ObjectAddedRemoved;
    }
}
