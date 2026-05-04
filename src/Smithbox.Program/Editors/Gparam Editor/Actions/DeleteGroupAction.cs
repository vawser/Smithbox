using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class DeleteGroupAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private List<GPARAM.Param> TargetGroups { get; set; } = new();
    private List<GPARAM.Param> StoredGroups { get; set; } = new();

    private List<int> StoredIndices { get; set; } = new();

    public DeleteGroupAction(ProjectEntry project, GPARAM data, List<GPARAM.Param> groups)
    {
        Project = project;
        Data = data;
        TargetGroups = groups;

        StoredGroups = new List<GPARAM.Param>(TargetGroups);
    }

    public override ActionEvent Execute()
    {
        StoredIndices = new List<int>();

        foreach (var entry in TargetGroups)
        {
            int idx = Data.Params.IndexOf(entry);
            if (idx >= 0)
            {
                StoredIndices.Add(idx);
            }
        }

        StoredIndices.Sort((a, b) => b.CompareTo(a));

        foreach (int idx in StoredIndices)
        {
            Data.Params.RemoveAt(idx);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        StoredIndices.Sort();

        for (int i = 0; i < StoredGroups.Count; i++)
        {
            int insertIndex = i < StoredIndices.Count
                ? StoredIndices[i]
                : StoredGroups.Count;

            Data.Params.Insert(insertIndex, StoredGroups[i]);
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
