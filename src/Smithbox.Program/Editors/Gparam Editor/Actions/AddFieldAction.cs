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

    private List<int> StoredIndices { get; set; } = new();

    public AddFieldAction(ProjectEntry project, GPARAM data, GPARAM.Param group, List<GparamAnnotationFieldEntry> fields)
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
        StoredIndices = new List<int>();

        int insertIndex = TargetGroup.Fields.Count;

        foreach (var entry in StoredFields)
        {
            TargetGroup.Fields.Insert(insertIndex, entry);
            StoredIndices.Add(insertIndex);
            insertIndex++;
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (int i = StoredIndices.Count - 1; i >= 0; i--)
        {
            TargetGroup.Fields.RemoveAt(StoredIndices[i]);
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
