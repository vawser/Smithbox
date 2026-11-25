using Andre.Formats;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.ParamEditor.Upgrader;

public sealed class RegulationDiffException : Exception
{
    public RegulationDiffException(string message) : base(message) { }
}

/// <summary>
/// Rebuilds the older version of a step from the newer version and the step's diff.
/// </summary>
public static class RegulationDiffApplier
{
    /// <summary>
    /// Returns the older params. The newer params are not changed; a param the step didn't change (same
    /// layout, same rows) is returned as the same object.
    /// </summary>
    public static Dictionary<string, Param> RebuildOlder(
        Dictionary<string, Param> newer, RegulationDiff diff, RegulationLoadContext context)
    {
        var result = new Dictionary<string, Param>(StringComparer.Ordinal);
        var added = diff.AddedParams.ToHashSet(StringComparer.Ordinal);
        var unloaded = diff.UnloadedParams.ToHashSet(StringComparer.Ordinal);
        var paramDiffs = diff.Params.ToDictionary(p => p.Name, StringComparer.Ordinal);

        foreach (var (name, newerParam) in newer)
        {
            if (added.Contains(name) || unloaded.Contains(name))
                continue;

            if (!paramDiffs.TryGetValue(name, out var paramDiff))
                throw new RegulationDiffException(
                    $"{name} is in the {diff.ToVersion} regulation but not in the {diff.FromVersion} -> {diff.ToVersion} diff.");

            result.Add(name, RebuildParam(newerParam, paramDiff, diff.FromVersion, context));
        }

        foreach (var paramDiff in diff.Params)
        {
            if (!result.ContainsKey(paramDiff.Name))
                throw new RegulationDiffException(
                    $"{paramDiff.Name} is missing from the {diff.ToVersion} regulation.");
        }

        foreach (var removed in diff.RemovedParams)
        {
            var param = RegulationParamLoader.LoadParam(
                removed.Name, Convert.FromBase64String(removed.Data), context, diff.FromVersion, null);

            if (param == null)
                throw new RegulationDiffException($"{removed.Name} (removed in {diff.ToVersion}) could not be loaded.");

            result.Add(removed.Name, param);
        }

        return result;
    }

    public static Param RebuildParam(Param newer, ParamDiff diff, ulong olderVersion, RegulationLoadContext context)
    {
        var hash = RegulationFields.Hash(newer);
        if (hash != diff.ToHash)
            throw new RegulationDiffException(
                $"{diff.Name} does not match the version the diff was made from (the game's regulation.bin may be modified).");

        if (!context.ParamDefs.TryGetValue(diff.ParamType, out var def))
            throw new RegulationDiffException($"{diff.Name}: no paramdef for {diff.ParamType}.");

        var older = RegulationFields.CreateParam(def, olderVersion, newer);

        if (older.RowSize != diff.FromRowSize)
            throw new RegulationDiffException(
                $"{diff.Name}: the paramdef gives {older.RowSize} byte rows for {olderVersion}, the diff expects {diff.FromRowSize}.");

        var olderLayout = new ParamLayout(older);
        var newerLayout = new ParamLayout(newer);
        var map = new FieldMap(olderLayout, newerLayout, diff.FieldMoves);

        // Unchanged: reuse the newer param
        if (map.Identical && !diff.HasRowChanges)
            return newer;

        var addedKeys = diff.AddedRows.Select(k => new RowKeyValue(k.ID, k.Index)).ToHashSet();
        var changes = diff.ChangedRows.ToDictionary(c => new RowKeyValue(c.ID, c.Index));

        var rows = new List<(RowKeyValue Key, Param.Row Row)>();

        foreach (var (key, newerRow) in RegulationFields.KeyedRows(newer))
        {
            if (addedKeys.Contains(key))
                continue;

            var row = new Param.Row(newerRow.ID, newerRow.Name, older);
            RegulationFields.CopyNewerToOlder(newerRow, newerLayout, row, olderLayout, map);

            if (changes.TryGetValue(key, out var change))
                ApplyChange(row, olderLayout, change);

            rows.Add((key, row));
        }

        foreach (var removed in diff.RemovedRows.OrderBy(r => r.Position))
        {
            var row = new Param.Row(removed.ID, null, older);
            ApplyChange(row, olderLayout, removed);

            var position = Math.Clamp(removed.Position, 0, rows.Count);
            rows.Insert(position, (new RowKeyValue(removed.ID, removed.Index), row));
        }

        if (diff.Order != null)
        {
            var byKey = rows.ToDictionary(r => r.Key, r => r.Row);
            rows = diff.Order.Select(k =>
            {
                var key = new RowKeyValue(k.ID, k.Index);
                if (!byKey.TryGetValue(key, out var row))
                    throw new RegulationDiffException($"{diff.Name}: row {k.ID} [{k.Index}] in Order does not exist.");
                return (key, row);
            }).ToList();
        }

        foreach (var (_, row) in rows)
            older.AddRow(row);

        return older;
    }

    internal static void ApplyChange(Param.Row row, ParamLayout layout, RowChange change)
    {
        if (change.NameChanged)
            row.Name = change.Name;

        if (change.Data != null)
        {
            row.SetRawData(Convert.FromHexString(change.Data));
            return;
        }

        if (change.Fields == null)
            return;

        foreach (var (field, value) in change.Fields)
        {
            var column = layout[field];
            if (column == null)
                throw new RegulationDiffException($"Field {field} is not in the {row.ID} row's layout.");

            column.SetValue(row, RegulationFields.Parse(column, value));
        }
    }
}
