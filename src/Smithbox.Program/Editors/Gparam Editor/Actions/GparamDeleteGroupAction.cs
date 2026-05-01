using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamDeleteGroupAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM StoredGPARAM { get; set; }
    private GPARAM CurrentGPARAM { get; set; }
    private List<GPARAM.Param> TargetParams { get; set; }

    public GparamDeleteGroupAction(ProjectEntry project, GPARAM data, List<GPARAM.Param> targetParams)
    {
        Project = project;
        CurrentGPARAM = data;
        StoredGPARAM = data.Clone(); // Full clone so the data isn't modified post edit
        TargetParams = targetParams;
    }

    public override ActionEvent Execute()
    {
        foreach (var entry in TargetParams)
        {
            CurrentGPARAM.Params.Remove(entry);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        CurrentGPARAM.Params = StoredGPARAM.Params;

        return ActionEvent.ObjectAddedRemoved;
    }
}
