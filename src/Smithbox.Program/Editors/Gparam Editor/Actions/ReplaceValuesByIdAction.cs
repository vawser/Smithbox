using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Data Transfer's Match ID paste: each pasted ID's values replace all of that ID's values in the field (an ID has
/// one value per time-of-day keyframe), where its first value was; an ID the field doesn't have goes before the first
/// higher ID, else at the end. Undone by restoring a snapshot (GparamMergeSnapshot), so every object older undo steps
/// hold stays live (Claude/Documentation/Gparam.md §2).
/// </summary>
public class ReplaceValuesByIdAction : EditorAction
{
    private readonly ProjectEntry Project;
    private readonly GPARAM Data;
    private readonly GPARAM.IField Field;
    private readonly List<GPARAM.IFieldValue> Values;

    private GparamMergeSnapshot Before;

    public ReplaceValuesByIdAction(ProjectEntry project, GPARAM data, GPARAM.IField field, List<GPARAM.IFieldValue> values)
    {
        Project = project;
        Data = data;
        Field = field;
        Values = values.Where(v => v != null && field.AcceptsValue(v)).ToList();
    }

    public override ActionEvent Execute()
    {
        if (Values.Count == 0 || Data.Params.SelectMany(p => p.Fields).All(f => !ReferenceEquals(f, Field)))
            return ActionEvent.NoEvent;

        Before = GparamMergeSnapshot.Take(Data);

        foreach (var ids in Values.GroupBy(v => v.ID).OrderBy(g => g.Key))
        {
            var current = GparamMerge.KeyframesOf(Field, ids.Key);

            int index;
            if (current.Count > 0)
            {
                index = Field.IndexOfValue(current[0]);

                foreach (var value in current)
                    Field.RemoveValue(value);
            }
            else
            {
                var next = Field.Values.FirstOrDefault(e => e.ID > ids.Key);
                index = next != null ? Field.IndexOfValue(next) : Field.Values.Count;
            }

            foreach (var value in ids)
                Field.InsertValueAt(index++, value);
        }

        GparamUtils.RebuildParamExtras(Data);
        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        if (Before == null)
            return ActionEvent.NoEvent;

        Before.Restore();
        Before = null;

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }
}
