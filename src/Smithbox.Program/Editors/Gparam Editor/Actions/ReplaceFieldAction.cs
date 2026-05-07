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

// Used to add a value to an empty field
public class ReplaceFieldAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM.Param TargetGroup { get; set; }
    private List<GparamAnnotationFieldEntry> TargetFields { get; set; }
    private List<GPARAM.IField> StoredFields { get; set; } = new();
    private List<GPARAM.IField> UndoFields { get; set; } = new();

    public ReplaceFieldAction(ProjectEntry project, GPARAM data, GPARAM.Param group, List<GparamAnnotationFieldEntry> fields)
    {
        Project = project;
        Data = data;
        TargetGroup = group;
        TargetFields = fields;

        foreach (var entry in TargetFields)
        {
            var newField = GparamConstructUtils.AddNewField(entry);
            StoredFields.Add(newField);
        }
    }

    public override ActionEvent Execute()
    {
        foreach (var target in TargetFields)
        {
            var targetField = StoredFields.FirstOrDefault(e => e.Key == target.ID);
            var curField = TargetGroup.Fields.FirstOrDefault(e => e.Key == target.ID);
            var fieldIndex = TargetGroup.Fields.IndexOf(curField);

            if(fieldIndex != -1)
            {
                TargetGroup.Fields[fieldIndex] = targetField;

                UndoFields.Add(targetField);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        // Eh, we just don't support undoing this action

        return ActionEvent.ObjectAddedRemoved;
    }
}
