using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamDeleteFieldAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM StoredGPARAM { get; set; }
    private GPARAM CurrentGPARAM { get; set; }
    private GPARAM.Param TargetParam { get; set; }
    private GPARAM.IField TargetField { get; set; }

    public GparamDeleteFieldAction(ProjectEntry project, GPARAM data,GPARAM.Param param, GPARAM.IField field)
    {
        Project = project;
        CurrentGPARAM = data;
        StoredGPARAM = data.Clone(); // Full clone so the data isn't modified post edit
        TargetParam = param;
        TargetField = field;
    }

    public override ActionEvent Execute()
    {
        TargetParam.Fields.Remove(TargetField);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        CurrentGPARAM = StoredGPARAM;

        return ActionEvent.ObjectAddedRemoved;
    }
}
