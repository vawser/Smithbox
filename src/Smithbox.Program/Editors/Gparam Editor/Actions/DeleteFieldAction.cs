using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class DeleteFieldAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM.Param TargetGroup { get; set; }
    private List<GPARAM.IField> TargetFields { get; set; } = new();
    private List<GPARAM.IField> StoredFields { get; set; } = new();

    public DeleteFieldAction(ProjectEntry project, GPARAM data, GPARAM.Param group, List<GPARAM.IField> fields)
    {
        Project = project;
        Data = data;
        Data = data.Clone(); // Full clone so the data isn't modified post edit
        TargetGroup = group;
        TargetField = field;
    }

    public override ActionEvent Execute()
    {
        TargetGroup.Fields.Remove(TargetField);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        CurrentGPARAM = Data;

        return ActionEvent.ObjectAddedRemoved;
    }
}
