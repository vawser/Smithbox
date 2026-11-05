using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Import a value: it replaces the target value (overwrite), or is inserted after it (append), or before it
/// (append with insertBefore). With no target, an appended value goes at the end.
/// Edits the field in place and keeps its Capacity in step with its values, which the writer relies on.
/// </summary>
public class InsertFieldValueAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GparamEditorView View { get; set; }

    private FileDictionaryEntry FileEntry { get; set; }

    private GPARAM Data { get; set; }

    private GPARAM.Param Param { get; set; }
    private GPARAM.IField Field { get; set; }

    private GPARAM.IFieldValue TargetValue { get; set; }
    private GPARAM.IFieldValue NewValue { get; set; }

    private bool Overwrite { get; set; }

    // Append before the target rather than after it
    private bool InsertBefore { get; set; }

    // Fixed on the first execute, so redo repeats it exactly
    private bool Placed { get; set; } = false;
    private int InsertIndex { get; set; }

    private List<GPARAM.UnkParamExtra> StoredParamExtras { get; set; } = new();

    public InsertFieldValueAction(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry,
        GPARAM gparam, GPARAM.Param param, GPARAM.IField field, GPARAM.IFieldValue value, GPARAM.IFieldValue newValue,
        bool overwrite, bool insertBefore = false)
    {
        InsertBefore = insertBefore;

        Project = project;
        View = view;

        FileEntry = fileEntry;

        Data = gparam;
        Param = param;
        Field = field;

        TargetValue = value;
        NewValue = newValue;

        Overwrite = overwrite;
    }

    public override ActionEvent Execute()
    {
        // Checked by the import too: a value of another type can't go in the field's list
        if (!Field.AcceptsValue(NewValue))
            return ActionEvent.NoEvent;

        var targetIndex = Field.IndexOfValue(TargetValue);

        // Nothing to overwrite: the target value is gone
        if (Overwrite && targetIndex < 0)
            return ActionEvent.NoEvent;

        if (!Placed)
        {
            Placed = true;

            // Two values in a field can't share an ID: the replaced value's ID is free, any other is not
            var otherIds = Field.Values
                .Where(e => !(Overwrite && ReferenceEquals(e, TargetValue)))
                .Select(e => e.ID)
                .ToHashSet();

            if (otherIds.Contains(NewValue.ID))
            {
                var requested = NewValue.ID;
                NewValue.ID = GparamUtils.NextFreeValueId(Field, requested);

                Smithbox.Log(this, $"Value ID {requested} is already used in {Field.Key}: the imported value uses ID {NewValue.ID}.");
            }

            InsertIndex = targetIndex < 0
                ? Field.Values.Count
                : InsertBefore ? targetIndex : targetIndex + 1;
        }

        StoredParamExtras = GparamUtils.SnapshotParamExtras(Data);

        if (Overwrite)
        {
            Field.ReplaceValueAt(targetIndex, NewValue);
        }
        else
        {
            Field.InsertValueAt(InsertIndex, NewValue);
        }

        GparamUtils.RebuildParamExtras(Data);

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var index = Field.IndexOfValue(NewValue);

        if (index < 0)
            return ActionEvent.NoEvent;

        if (Overwrite)
        {
            Field.ReplaceValueAt(index, TargetValue);
        }
        else
        {
            Field.RemoveValue(NewValue);
        }

        Data.UnkParamExtras = StoredParamExtras;

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }
}
