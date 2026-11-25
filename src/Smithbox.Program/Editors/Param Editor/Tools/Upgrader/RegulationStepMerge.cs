using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor.Upgrader;

public sealed class UpgradeFieldConflict
{
    public string Field { get; init; }
    public string ModValue { get; init; }
    public string OldValue { get; init; }
    public string NewValue { get; init; }
}

/// <summary>
/// A row where the mod and the update disagree. RowConflict: both added a row with this ID (the mod's row is
/// kept). Otherwise Fields lists the fields both changed to different values (the mod's values are kept).
/// </summary>
public sealed class UpgradeConflict
{
    public ulong ToVersion { get; init; }
    public string Param { get; init; }
    public int RowID { get; init; }
    public bool RowConflict { get; init; }
    public List<UpgradeFieldConflict> Fields { get; } = new();
}

/// <summary>
/// One upgrade step: the mod at the older version, merged with the older and newer vanilla versions.
///
/// Rows are classified as before (match, modified, deleted, added, name change). A modified row is merged field
/// by field: a field the mod left at its older vanilla value takes the newer vanilla value, and a field the mod
/// changed keeps the mod's value. Fields that moved in the step follow the step's FieldMoves; fields new in the
/// step take the newer vanilla value, or for rows the mod added, the step's FieldDefaults.
/// </summary>
public static class RegulationStepMerge
{
    private enum Operation
    {
        Match,
        Modify,
        Delete,
        Add,
        NameChange
    }

    public static Dictionary<string, Param> Merge(
        Dictionary<string, Param> mod,
        Dictionary<string, Param> oldVanilla,
        Dictionary<string, Param> newVanilla,
        RegulationDiff diff,
        List<UpgradeConflict> conflicts)
    {
        var result = new Dictionary<string, Param>(StringComparer.Ordinal);
        var paramDiffs = diff?.Params.ToDictionary(p => p.Name, StringComparer.Ordinal)
                         ?? new Dictionary<string, ParamDiff>(StringComparer.Ordinal);

        foreach (var (name, newParam) in newVanilla)
        {
            // A param new in this version, or one the mod doesn't have: take it as it is
            if (!oldVanilla.TryGetValue(name, out var oldParam) || !mod.TryGetValue(name, out var modParam))
            {
                result.Add(name, newParam);
                continue;
            }

            paramDiffs.TryGetValue(name, out var paramDiff);
            result.Add(name, MergeParam(name, modParam, oldParam, newParam, paramDiff, diff?.ToVersion ?? 0, conflicts));
        }

        return result;
    }

    public static Param MergeParam(
        string name, Param source, Param oldVanilla, Param newVanilla, ParamDiff paramDiff, ulong toVersion,
        List<UpgradeConflict> conflicts)
    {
        var sourceLayout = new ParamLayout(source);
        var oldLayout = new ParamLayout(oldVanilla);
        var newLayout = new ParamLayout(newVanilla);
        var map = new FieldMap(oldLayout, newLayout, paramDiff?.FieldMoves);

        // For each older vanilla column, the mod's column with the same key and shape
        var sourceForOld = new int[oldLayout.Columns.Length];
        for (var o = 0; o < oldLayout.Columns.Length; o++)
        {
            sourceForOld[o] = sourceLayout.IndexByKey.TryGetValue(oldLayout.Keys[o], out var s) &&
                              RegulationFields.SameShape(sourceLayout.Columns[s], oldLayout.Columns[o])
                ? s
                : -1;
        }

        var byteCopy = map.Identical && sourceLayout.SameAs(oldLayout);

        var defaults = new List<(int Column, object Value)>();
        var remaps = new List<(int Column, object From, object To)>();
        if (paramDiff != null)
        {
            foreach (var (field, values) in paramDiff.ValueRemaps)
            {
                if (!newLayout.IndexByKey.TryGetValue(field, out var n))
                    continue;

                foreach (var (from, to) in values)
                    remaps.Add((n, RegulationFields.Parse(newLayout.Columns[n], from), RegulationFields.Parse(newLayout.Columns[n], to)));
            }

            foreach (var (field, value) in paramDiff.FieldDefaults)
            {
                if (newLayout.IndexByKey.TryGetValue(field, out var n))
                    defaults.Add((n, RegulationFields.Parse(newLayout.Columns[n], value)));
            }
        }

        // Rows in the mod that are not in the older vanilla version (everything starts as added)
        Dictionary<int, List<Param.Row>> addedRows = new(source.Rows.Count);
        // Older vanilla rows the mod deleted
        Dictionary<int, int> deletedRows = new();
        // Rows in both that the mod changed: (mod row, older vanilla row)
        Dictionary<int, List<(Param.Row Mod, Param.Row Old)>> modifiedRows = new();
        // Rows whose only change is the name
        Dictionary<int, List<Param.Row>> renamedRows = new();
        // The ordered edit operations for each ID
        Dictionary<int, List<Operation>> operations = new(source.Rows.Count);

        List<Operation> Ops(int id)
        {
            if (!operations.TryGetValue(id, out var list))
                operations[id] = list = new List<Operation>();
            return list;
        }

        foreach (var row in source.Rows)
        {
            if (!addedRows.TryGetValue(row.ID, out var list))
                addedRows[row.ID] = list = new List<Param.Row>();
            list.Add(row);
        }

        foreach (var row in oldVanilla.Rows)
        {
            if (!addedRows.TryGetValue(row.ID, out var list))
            {
                deletedRows[row.ID] = deletedRows.GetValueOrDefault(row.ID) + 1;
                Ops(row.ID).Add(Operation.Delete);
                continue;
            }

            var modRow = list[0];
            list.RemoveAt(0);
            if (list.Count == 0)
                addedRows.Remove(row.ID);

            if (byteCopy ? row.DataEquals(modRow) : SameValues(modRow, sourceLayout, row, oldLayout, sourceForOld))
            {
                if (modRow.Name == row.Name)
                {
                    Ops(row.ID).Add(Operation.Match);
                }
                else
                {
                    Ops(row.ID).Add(Operation.NameChange);
                    if (!renamedRows.TryGetValue(row.ID, out var renamed))
                        renamedRows[row.ID] = renamed = new List<Param.Row>();
                    renamed.Add(modRow);
                }

                continue;
            }

            Ops(row.ID).Add(Operation.Modify);
            if (!modifiedRows.TryGetValue(row.ID, out var modified))
                modifiedRows[row.ID] = modified = new List<(Param.Row, Param.Row)>();
            modified.Add((modRow, row));
        }

        // One Add operation per remaining mod row
        foreach (var (id, list) in addedRows)
        {
            foreach (var _ in list)
                Ops(id).Add(Operation.Add);
        }

        var dest = new Param(newVanilla);

        Param.Row BuildAdded(Param.Row modRow)
        {
            if (byteCopy)
            {
                var copy = new Param.Row(modRow, dest);
                foreach (var (n, from, to) in remaps)
                {
                    if (RegulationFields.ValuesEqual(newLayout.Columns[n].GetValue(copy), from))
                        newLayout.Columns[n].SetValue(copy, to);
                }
                return copy;
            }

            // Start from the mod's bytes at the same offsets (keeps bits no field names), then set every field:
            // from the mod where the field has a source, otherwise the step's default or zero
            var row = new Param.Row(modRow.ID, modRow.Name, dest);
            row.SetRawData(RegulationFields.ClippedCopy(modRow.GetRawData(), newLayout.RowSize));

            for (var n = 0; n < newLayout.Columns.Length; n++)
            {
                var o = map.OlderForNewer[n];
                var s = o >= 0 ? sourceForOld[o] : -1;
                newLayout.Columns[n].SetValue(row, s >= 0
                    ? sourceLayout.Columns[s].GetValue(modRow)
                    : RegulationFields.ZeroValue(newLayout.Columns[n]));
            }

            foreach (var (n, value) in defaults)
                newLayout.Columns[n].SetValue(row, value);

            foreach (var (n, from, to) in remaps)
            {
                if (RegulationFields.ValuesEqual(newLayout.Columns[n].GetValue(row), from))
                    newLayout.Columns[n].SetValue(row, to);
            }

            return row;
        }

        Param.Row BuildModified(Param.Row modRow, Param.Row oldRow, Param.Row newRow)
        {
            var row = new Param.Row(newRow, dest) { Name = modRow.Name };
            UpgradeConflict conflict = null;

            for (var n = 0; n < newLayout.Columns.Length; n++)
            {
                var o = map.OlderForNewer[n];
                if (o < 0)
                    continue; // New in this step: the newer vanilla value

                var s = sourceForOld[o];
                if (s < 0)
                    continue;

                var modValue = sourceLayout.Columns[s].GetValue(modRow);
                var oldValue = oldLayout.Columns[o].GetValue(oldRow);

                if (RegulationFields.ValuesEqual(modValue, oldValue))
                    continue; // The mod didn't change it: the newer vanilla value

                var newValue = newLayout.Columns[n].GetValue(newRow);
                newLayout.Columns[n].SetValue(row, modValue);

                if (!RegulationFields.ValuesEqual(oldValue, newValue) && !RegulationFields.ValuesEqual(modValue, newValue))
                {
                    conflict ??= new UpgradeConflict { ToVersion = toVersion, Param = name, RowID = newRow.ID };
                    conflict.Fields.Add(new UpgradeFieldConflict
                    {
                        Field = newLayout.Keys[n],
                        ModValue = RegulationFields.Format(modValue),
                        OldValue = RegulationFields.Format(oldValue),
                        NewValue = RegulationFields.Format(newValue)
                    });
                }
            }

            if (conflict != null)
                conflicts?.Add(conflict);

            return row;
        }

        var pendingAdds = addedRows.Keys.OrderBy(e => e).ToArray();
        var currPendingAdd = 0;
        var lastID = 0;

        foreach (var row in newVanilla.Rows)
        {
            // Slot in mod-added rows that come before this ID
            while (currPendingAdd < pendingAdds.Length &&
                   pendingAdds[currPendingAdd] >= lastID &&
                   pendingAdds[currPendingAdd] < row.ID)
            {
                if (addedRows.TryGetValue(pendingAdds[currPendingAdd], out var adds))
                {
                    foreach (var arow in adds)
                        dest.AddRow(BuildAdded(arow));

                    addedRows.Remove(pendingAdds[currPendingAdd]);
                    operations.Remove(pendingAdds[currPendingAdd]);
                }

                currPendingAdd++;
            }

            lastID = row.ID;

            if (!operations.TryGetValue(row.ID, out var ops))
            {
                // A row only the update has
                dest.AddRow(new Param.Row(row, dest));
                continue;
            }

            var operation = ops[0];
            ops.RemoveAt(0);
            if (ops.Count == 0)
                operations.Remove(row.ID);

            switch (operation)
            {
                case Operation.Add:
                {
                    // The mod and the update both added this ID: the mod's row wins
                    conflicts?.Add(new UpgradeConflict
                    {
                        ToVersion = toVersion, Param = name, RowID = row.ID, RowConflict = true
                    });

                    var list = addedRows[row.ID];
                    dest.AddRow(BuildAdded(list[0]));
                    list.RemoveAt(0);
                    if (list.Count == 0)
                        addedRows.Remove(row.ID);
                    break;
                }
                case Operation.Match:
                    dest.AddRow(new Param.Row(row, dest));
                    break;
                case Operation.Delete:
                    break;
                case Operation.Modify:
                {
                    var list = modifiedRows[row.ID];
                    var (modRow, oldRow) = list[0];
                    list.RemoveAt(0);
                    dest.AddRow(BuildModified(modRow, oldRow, row));
                    break;
                }
                case Operation.NameChange:
                {
                    var list = renamedRows[row.ID];
                    var newRow = new Param.Row(row, dest) { Name = list[0].Name };
                    list.RemoveAt(0);
                    dest.AddRow(newRow);
                    break;
                }
            }
        }

        // Remaining mod-added rows
        for (; currPendingAdd < pendingAdds.Length; currPendingAdd++)
        {
            if (!addedRows.TryGetValue(pendingAdds[currPendingAdd], out var adds))
                continue;

            foreach (var arow in adds)
                dest.AddRow(BuildAdded(arow));

            addedRows.Remove(pendingAdds[currPendingAdd]);
        }

        return dest;
    }

    private static bool SameValues(Param.Row modRow, ParamLayout sourceLayout, Param.Row oldRow, ParamLayout oldLayout,
        int[] sourceForOld)
    {
        if (modRow.ID != oldRow.ID)
            return false;

        for (var o = 0; o < oldLayout.Columns.Length; o++)
        {
            var s = sourceForOld[o];
            if (s < 0)
                return false;

            if (!RegulationFields.ValuesEqual(sourceLayout.Columns[s].GetValue(modRow), oldLayout.Columns[o].GetValue(oldRow)))
                return false;
        }

        return true;
    }
}
