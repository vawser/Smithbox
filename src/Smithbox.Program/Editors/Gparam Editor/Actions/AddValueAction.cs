using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Adds values to a field: either duplicates of existing values (placed after the last of them), or values built by
/// the caller (placed at the end, e.g. the default value for an empty field).
/// A value never takes an ID the field already uses: a colliding ID moves up to the next free one.
/// </summary>
public class AddValueAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM Data { get; set; }
    private GPARAM.Param TargetGroup { get; set; }
    private GPARAM.IField TargetField { get; set; }

    private List<GPARAM.IFieldValue> SourceValues { get; set; }
    private List<GPARAM.IFieldValue> PrebuiltValues { get; set; }

    private bool UseDuplicateOffset { get; set; }
    private int DuplicateID { get; set; }
    private int DuplicateOffset { get; set; }

    // Built on the first execute and reused by redo, as later actions on the redo stack refer to these objects
    private List<(int Index, GPARAM.IFieldValue Value)> Inserted { get; set; } = null;

    private List<GPARAM.UnkParamExtra> StoredParamExtras { get; set; } = new();

    /// <summary>
    /// Duplicate the values. Each copy's ID is <paramref name="duplicateID"/>, or its source's ID plus
    /// <paramref name="duplicateOffset"/> if <paramref name="useDuplicateOffset"/>, moved up to the next free ID.
    /// </summary>
    public AddValueAction(ProjectEntry project, GPARAM data, GPARAM.Param group, GPARAM.IField field,
        List<GPARAM.IFieldValue> values, int duplicateID, int duplicateOffset, bool useDuplicateOffset)
    {
        Project = project;
        Data = data;
        TargetGroup = group;
        TargetField = field;
        SourceValues = new List<GPARAM.IFieldValue>(values);
        DuplicateID = duplicateID;
        DuplicateOffset = duplicateOffset;
        UseDuplicateOffset = useDuplicateOffset;
    }

    /// <summary>
    /// Add these already built values at the end of the field. Their IDs are moved up to the next free one if taken.
    /// </summary>
    public AddValueAction(ProjectEntry project, GPARAM data, GPARAM.Param group, GPARAM.IField field,
        List<GPARAM.IFieldValue> newValues)
    {
        Project = project;
        Data = data;
        TargetGroup = group;
        TargetField = field;
        PrebuiltValues = new List<GPARAM.IFieldValue>(newValues);
    }

    public override ActionEvent Execute()
    {
        if (Inserted == null)
        {
            Inserted = PrebuiltValues != null ? PlacePrebuilt() : PlaceDuplicates();
        }

        if (Inserted.Count == 0)
            return ActionEvent.NoEvent;

        StoredParamExtras = GparamUtils.SnapshotParamExtras(Data);

        // Lowest first: each index is where the value sits with the earlier ones already in place
        foreach (var entry in Inserted)
        {
            TargetField.InsertValueAt(entry.Index, entry.Value);
        }

        GparamUtils.RebuildParamExtras(Data);

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        if (Inserted == null || Inserted.Count == 0)
            return ActionEvent.NoEvent;

        foreach (var entry in Inserted)
        {
            TargetField.RemoveValue(entry.Value);
        }

        Data.UnkParamExtras = StoredParamExtras;

        Project.Handler.GparamData.PrimaryBank.MarkAsDirty(Data);

        return ActionEvent.ObjectAddedRemoved;
    }

    private List<(int Index, GPARAM.IFieldValue Value)> PlaceDuplicates()
    {
        var result = new List<(int, GPARAM.IFieldValue)>();

        var sources = SourceValues
            .Where(e => e != null && TargetField.IndexOfValue(e) >= 0)
            .OrderBy(e => TargetField.IndexOfValue(e))
            .ToList();

        if (sources.Count == 0)
            return result;

        // The copies go straight after the last of the values being copied
        var insertIndex = sources.Max(e => TargetField.IndexOfValue(e)) + 1;

        var taken = new HashSet<int>();

        foreach (var source in sources)
        {
            var copy = TargetField.CloneValue(source);

            var requested = UseDuplicateOffset ? source.ID + DuplicateOffset : DuplicateID;
            copy.ID = GparamUtils.NextFreeValueId(TargetField, requested, taken);
            taken.Add(copy.ID);

            if (copy.ID != requested)
            {
                Smithbox.Log(this, $"Value ID {requested} is already used in {TargetField.Key}: the duplicate uses ID {copy.ID}.");
            }

            result.Add((insertIndex, copy));
            insertIndex++;
        }

        return result;
    }

    private List<(int Index, GPARAM.IFieldValue Value)> PlacePrebuilt()
    {
        var result = new List<(int, GPARAM.IFieldValue)>();

        var insertIndex = TargetField.Values.Count;
        var taken = new HashSet<int>();

        foreach (var value in PrebuiltValues)
        {
            if (value == null || !TargetField.AcceptsValue(value))
                continue;

            value.ID = GparamUtils.NextFreeValueId(TargetField, value.ID, taken);
            taken.Add(value.ID);

            result.Add((insertIndex, value));
            insertIndex++;
        }

        return result;
    }
}
