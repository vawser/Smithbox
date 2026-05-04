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
    private List<GPARAM.IField> StoredFields { get; set; } = new();

    private List<int> StoredIndices { get; set; } = new();

    public DeleteFieldAction(ProjectEntry project, GPARAM data, GPARAM.Param group, List<GPARAM.IField> fields)
    {
        Project = project;
        Data = data;
        TargetGroup = group;

        StoredFields = new List<GPARAM.IField>(fields);
    }

    public override ActionEvent Execute()
    {
        StoredIndices = new List<int>();

        foreach (var entry in StoredFields)
        {
            int idx = TargetGroup.Fields.IndexOf(entry);
            if (idx >= 0)
            {
                StoredIndices.Add(idx);
            }
        }

        StoredIndices.Sort((a, b) => b.CompareTo(a));

        foreach (int idx in StoredIndices)
        {
            TargetGroup.Fields.RemoveAt(idx);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        StoredIndices.Sort();

        for (int i = 0; i < StoredFields.Count; i++)
        {
            int insertIndex = i < StoredIndices.Count
                ? StoredIndices[i]
                : TargetGroup.Fields.Count;

            TargetGroup.Fields.Insert(insertIndex, StoredFields[i]);
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
