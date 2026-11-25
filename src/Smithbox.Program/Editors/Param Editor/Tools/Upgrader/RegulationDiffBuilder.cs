using Andre.Formats;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.ParamEditor.Upgrader;

/// <summary>
/// One loaded regulation: its params, the raw .param files, and the params that couldn't be loaded.
/// </summary>
public sealed class RegulationSnapshot
{
    public ulong Version { get; init; }
    public Dictionary<string, Param> Params { get; init; }
    public Dictionary<string, byte[]> RawFiles { get; init; }
    public HashSet<string> Unloaded { get; init; }
    public List<string> Warnings { get; init; }

    public static RegulationSnapshot Load(byte[] regulationData, RegulationLoadContext context)
    {
        using var binder = RegulationParamLoader.ReadBinder(context.Game, regulationData);
        return Load(binder, context);
    }

    public static RegulationSnapshot Load(IBinder binder, RegulationLoadContext context)
    {
        var warnings = new List<string>();
        var parameters = RegulationParamLoader.LoadParams(binder, context, warnings, out var version);
        var raw = new Dictionary<string, byte[]>(StringComparer.Ordinal);

        foreach (var file in binder.Files)
        {
            if (!file.Name.EndsWith(".PARAM", StringComparison.OrdinalIgnoreCase))
                continue;

            var name = Path.GetFileNameWithoutExtension(file.Name.Replace('\\', '/').Split('/')[^1]);
            raw.TryAdd(name, file.Bytes.ToArray());
        }

        return new RegulationSnapshot
        {
            Version = version,
            Params = parameters,
            RawFiles = raw,
            Unloaded = raw.Keys.Where(k => !parameters.ContainsKey(k)).ToHashSet(StringComparer.Ordinal),
            Warnings = warnings
        };
    }
}

/// <summary>
/// Things the builder noticed that a person may want to look at (unclear field moves, raw row fallbacks).
/// </summary>
public sealed class RegulationDiffReport
{
    public List<string> Notes { get; } = new();
    public int RawRows { get; set; }
    public int ChangedRows { get; set; }
    public int ChangedFields { get; set; }
}

/// <summary>
/// Makes the diff for one version step, then checks that it rebuilds the older version exactly.
/// </summary>
public static class RegulationDiffBuilder
{
    /// <param name="metaDefault">Optional: the Param Meta default value of a field (param name, field key)</param>
    public static RegulationDiff Build(
        RegulationSnapshot older,
        RegulationSnapshot newer,
        RegulationLoadContext context,
        string generator,
        RegulationDiffReport report,
        Func<string, string, string> metaDefault = null)
    {
        if (older.Version >= newer.Version)
            throw new RegulationDiffException(
                $"The older regulation ({older.Version}) is not older than the newer one ({newer.Version}).");

        var diff = new RegulationDiff
        {
            Game = context.Game.ToString(),
            FromVersion = older.Version,
            ToVersion = newer.Version,
            Generator = generator
        };

        // Params the older version can't load are left out. A param the newer version can't load but the older one
        // can is stored whole (as removed), so the chain can go on past it.
        var unloaded = older.Unloaded.ToHashSet(StringComparer.Ordinal);
        diff.UnloadedParams = unloaded.OrderBy(n => n, StringComparer.Ordinal).ToList();

        foreach (var name in newer.Unloaded.Where(older.Params.ContainsKey))
            report.Notes.Add($"{name}: can't be loaded in {newer.Version} (the paramdef doesn't fit); stored whole.");

        foreach (var name in newer.Params.Keys.OrderBy(n => n, StringComparer.Ordinal))
        {
            if (unloaded.Contains(name))
                continue;

            if (!older.Params.ContainsKey(name))
            {
                if (!older.RawFiles.ContainsKey(name))
                    diff.AddedParams.Add(name);
                continue;
            }

            diff.Params.Add(BuildParam(name, older.Params[name], newer.Params[name], older.Version, context,
                report, metaDefault));
        }

        foreach (var name in older.Params.Keys.OrderBy(n => n, StringComparer.Ordinal))
        {
            if (newer.Params.ContainsKey(name))
                continue;

            diff.RemovedParams.Add(new RemovedParam
            {
                Name = name,
                Data = Convert.ToBase64String(older.RawFiles[name])
            });
        }

        // Check what will be in the file, not the object in memory
        Verify(older, newer, RegulationDiffIO.Deserialize(RegulationDiffIO.Serialize(diff)), context);

        return diff;
    }

    private static ParamDiff BuildParam(
        string name, Param older, Param newer, ulong olderVersion, RegulationLoadContext context,
        RegulationDiffReport report, Func<string, string, string> metaDefault)
    {
        var olderLayout = new ParamLayout(older);
        var newerLayout = new ParamLayout(newer);

        var paramDiff = new ParamDiff
        {
            Name = name,
            ParamType = older.ParamType,
            FromRowSize = older.RowSize,
            ToRowSize = newer.RowSize,
            ToHash = RegulationFields.Hash(newer)
        };

        if (older.ParamType != newer.ParamType)
            report.Notes.Add($"{name}: param type changed from {older.ParamType} to {newer.ParamType}; the older type is used to rebuild.");

        var olderKeyed = RegulationFields.KeyedRows(older);
        var newerKeyed = RegulationFields.KeyedRows(newer);
        var olderByKey = olderKeyed.ToDictionary(r => r.Key, r => r.Row);
        var newerByKey = newerKeyed.ToDictionary(r => r.Key, r => r.Row);

        if (!olderLayout.SameAs(newerLayout))
            paramDiff.FieldMoves = DetectMoves(name, olderLayout, newerLayout, olderByKey, newerByKey, report);

        var map = new FieldMap(olderLayout, newerLayout, paramDiff.FieldMoves);

        // A scratch param in the older layout for candidate rows
        var scratch = RegulationFields.CreateParam(context.ParamDefs[older.ParamType], olderVersion, newer);
        if (scratch.RowSize != older.RowSize)
            throw new RegulationDiffException(
                $"{name}: the paramdef gives {scratch.RowSize} byte rows for {olderVersion}, the regulation has {older.RowSize}.");

        foreach (var (key, _) in newerKeyed)
        {
            if (!olderByKey.ContainsKey(key))
                paramDiff.AddedRows.Add(new RowKey { ID = key.ID, Index = key.Index });
        }

        // One candidate row, reused: Param.Row frees its data slot in a finalizer, which isn't safe while the
        // scratch param is still in use, so no candidate is ever dropped before the scratch param is
        var candidate = new Param.Row(0, null, scratch);
        var zeroRow = new byte[scratch.RowSize];

        void ResetCandidate(int id, string rowName)
        {
            candidate.ID = id;
            candidate.Name = rowName;
            candidate.SetRawData(zeroRow);
        }

        foreach (var (key, olderRow) in olderKeyed)
        {
            if (!newerByKey.TryGetValue(key, out var newerRow))
                continue;

            // Fast path: same layout and same bytes
            if (map.Identical && olderRow.DataEquals(newerRow) && olderRow.Name == newerRow.Name)
                continue;

            ResetCandidate(newerRow.ID, newerRow.Name);
            RegulationFields.CopyNewerToOlder(newerRow, newerLayout, candidate, olderLayout, map);

            var change = Describe(olderRow, candidate, olderLayout, report);
            if (change != null)
            {
                change.ID = key.ID;
                change.Index = key.Index;
                paramDiff.ChangedRows.Add(change);
            }
        }

        var olderPosition = 0;
        foreach (var (key, olderRow) in olderKeyed)
        {
            if (!newerByKey.ContainsKey(key))
            {
                ResetCandidate(olderRow.ID, null);
                var change = Describe(olderRow, candidate, olderLayout, report) ?? new RowChange();
                change.ID = key.ID;
                change.Index = key.Index;
                change.Position = olderPosition;
                paramDiff.RemovedRows.Add(change);
            }

            olderPosition++;
        }

        // Order: derived from the newer order unless the common rows were reordered
        var derived = newerKeyed.Select(r => r.Key).Where(olderByKey.ContainsKey).ToList();
        foreach (var removed in paramDiff.RemovedRows.OrderBy(r => r.Position))
            derived.Insert(Math.Min(removed.Position, derived.Count), new RowKeyValue(removed.ID, removed.Index));

        if (!derived.SequenceEqual(olderKeyed.Select(r => r.Key)))
        {
            paramDiff.Order = olderKeyed.Select(r => new RowKey { ID = r.Key.ID, Index = r.Key.Index }).ToList();
            report.Notes.Add($"{name}: rows were reordered; the full older order is stored.");
        }

        if (!map.Identical)
            paramDiff.FieldDefaults = DetectDefaults(name, newer, newerLayout, map, metaDefault);

        if (paramDiff.ChangedRows.Count > 0)
            paramDiff.ValueRemaps = DetectRemaps(name, olderLayout, newerLayout, map, olderByKey, newerByKey, report);

        GC.KeepAlive(candidate);
        return paramDiff;
    }

    /// <summary>
    /// The change that turns the candidate into the older row, or null when they are already the same.
    /// The candidate is changed to match.
    /// </summary>
    private static RowChange Describe(Param.Row older, Param.Row candidate, ParamLayout layout, RegulationDiffReport report)
    {
        var change = new RowChange();
        var any = false;

        if (older.Name != candidate.Name)
        {
            change.NameChanged = true;
            change.Name = older.Name;
            candidate.Name = older.Name;
            any = true;
        }

        var fields = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var i = 0; i < layout.Columns.Length; i++)
        {
            var column = layout.Columns[i];
            var olderValue = column.GetValue(older);

            if (RegulationFields.ValuesEqual(olderValue, column.GetValue(candidate)))
                continue;

            var text = RegulationFields.Format(olderValue);
            fields[layout.Keys[i]] = text;
            column.SetValue(candidate, RegulationFields.Parse(column, text));
        }

        if (fields.Count > 0)
        {
            change.Fields = fields;
            any = true;
        }

        // The fields don't reproduce every byte (bytes no field covers, string padding, NaN payloads)
        if (!older.DataEquals(candidate) || older.ID != candidate.ID)
        {
            change.Fields = null;
            change.Data = Convert.ToHexString(older.GetRawData());
            report.RawRows++;
            any = true;
        }
        else if (fields.Count > 0)
        {
            report.ChangedFields += fields.Count;
        }

        if (any)
            report.ChangedRows++;

        return any ? change : null;
    }

    /// <summary>
    /// A field only in the older layout moved to a newer field when the newer field holds the older field's
    /// vanilla values on every row in both versions.
    /// </summary>
    private static List<FieldMove> DetectMoves(
        string name, ParamLayout older, ParamLayout newer,
        Dictionary<RowKeyValue, Param.Row> olderRows, Dictionary<RowKeyValue, Param.Row> newerRows,
        RegulationDiffReport report)
    {
        var moves = new List<FieldMove>();
        var shared = olderRows.Keys.Where(newerRows.ContainsKey).ToList();

        if (shared.Count == 0)
            return moves;

        var candidatesByTarget = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        for (var o = 0; o < older.Columns.Length; o++)
        {
            var fromKey = older.Keys[o];
            var fromColumn = older.Columns[o];

            // Still in the newer layout with the same shape: not a move
            var sameInNewer = newer[fromKey];
            if (sameInNewer != null && RegulationFields.SameShape(fromColumn, sameInNewer))
                continue;

            if (shared.All(k => RegulationFields.IsDefault(fromColumn.GetValue(olderRows[k]))))
                continue;

            var targets = new List<string>();

            for (var n = 0; n < newer.Columns.Length; n++)
            {
                var toColumn = newer.Columns[n];
                if (!RegulationFields.SameShape(fromColumn, toColumn))
                    continue;

                if (!shared.All(k => RegulationFields.ValuesEqual(
                        fromColumn.GetValue(olderRows[k]), toColumn.GetValue(newerRows[k]))))
                    continue;

                // A target that is also in the older layout must have changed, or it simply kept its values
                var toInOlder = older[newer.Keys[n]];
                if (toInOlder != null && RegulationFields.SameShape(toInOlder, toColumn) &&
                    shared.All(k => RegulationFields.ValuesEqual(
                        toInOlder.GetValue(olderRows[k]), toColumn.GetValue(newerRows[k]))))
                    continue;

                targets.Add(newer.Keys[n]);
            }

            if (targets.Count > 1)
            {
                // Prefer X_old -> X and similar names
                var byName = targets.Where(t => fromKey.StartsWith(t, StringComparison.Ordinal) ||
                                                t.StartsWith(fromKey, StringComparison.Ordinal)).ToList();
                if (byName.Count == 1)
                    targets = byName;
            }

            if (targets.Count == 1)
            {
                if (!candidatesByTarget.TryGetValue(targets[0], out var list))
                    candidatesByTarget[targets[0]] = list = new List<string>();
                list.Add(fromKey);
            }
            else if (targets.Count > 1)
            {
                report.Notes.Add($"{name}: {fromKey} matches several newer fields ({string.Join(", ", targets)}); no move stored.");
            }
        }

        foreach (var (to, froms) in candidatesByTarget)
        {
            if (froms.Count == 1)
            {
                moves.Add(new FieldMove { From = froms[0], To = to });
                report.Notes.Add($"{name}: field moved {froms[0]} -> {to}.");
            }
            else
            {
                report.Notes.Add($"{name}: {string.Join(", ", froms)} all match {to}; no move stored.");
            }
        }

        return moves;
    }

    /// <summary>
    /// For each field new in this step: the value most rows have in the newer version, when at least half of the
    /// rows share it; otherwise the Param Meta default.
    /// </summary>
    private static Dictionary<string, string> DetectDefaults(
        string name, Param newer, ParamLayout layout, FieldMap map, Func<string, string, string> metaDefault)
    {
        var defaults = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var n = 0; n < layout.Columns.Length; n++)
        {
            if (map.OlderForNewer[n] >= 0)
                continue;

            var column = layout.Columns[n];
            string value = null;

            if (newer.Rows.Count > 0)
            {
                var top = newer.Rows
                    .Select(r => RegulationFields.Format(column.GetValue(r)))
                    .GroupBy(v => v, StringComparer.Ordinal)
                    .OrderByDescending(g => g.Count())
                    .First();

                if (top.Count() * 2 >= newer.Rows.Count)
                    value = top.Key;
            }

            value ??= metaDefault?.Invoke(name, layout.Keys[n]);

            if (value == null)
                continue;

            // A zero needs nothing stored: rows the mod added start zeroed
            if (RegulationFields.IsDefault(RegulationFields.Parse(column, value)))
                continue;

            defaults[layout.Keys[n]] = value;
        }

        return defaults;
    }

    /// <summary>
    /// A column-wide value change: an older value nearly every row had (at least 90% of the rows in both versions,
    /// and at least 20) that the update changed to one newer value on at least 95% of those rows.
    /// </summary>
    private static Dictionary<string, Dictionary<string, string>> DetectRemaps(
        string name, ParamLayout older, ParamLayout newer, FieldMap map,
        Dictionary<RowKeyValue, Param.Row> olderRows, Dictionary<RowKeyValue, Param.Row> newerRows,
        RegulationDiffReport report)
    {
        var remaps = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
        var shared = olderRows.Keys.Where(newerRows.ContainsKey).ToList();

        if (shared.Count < 10)
            return remaps;

        for (var n = 0; n < newer.Columns.Length; n++)
        {
            var o = map.OlderForNewer[n];
            if (o < 0)
                continue;

            var olderColumn = older.Columns[o];
            var newerColumn = newer.Columns[n];

            foreach (var group in shared.GroupBy(k => RegulationFields.Format(olderColumn.GetValue(olderRows[k]))))
            {
                var count = group.Count();
                // The column held this value nearly everywhere: a balance change to some rows is not a remap
                if (count < 20 || count * 10 < shared.Count * 9)
                    continue;

                var top = group
                    .GroupBy(k => RegulationFields.Format(newerColumn.GetValue(newerRows[k])))
                    .OrderByDescending(g => g.Count())
                    .First();

                if (top.Key == group.Key || top.Count() * 100 < count * 95)
                    continue;

                remaps[newer.Keys[n]] = new Dictionary<string, string> { [group.Key] = top.Key };
                report.Notes.Add($"{name}: {newer.Keys[n]} changed from {group.Key} to {top.Key} on {top.Count()} of {shared.Count} rows; rows the mod added follow.");
            }
        }

        return remaps;
    }

    /// <summary>
    /// Rebuilds the older version from the diff and compares it with the real one.
    /// </summary>
    public static void Verify(RegulationSnapshot older, RegulationSnapshot newer, RegulationDiff diff,
        RegulationLoadContext context)
    {
        var rebuilt = RegulationDiffApplier.RebuildOlder(newer.Params, diff, context);
        var expected = older.Params.Keys.Where(k => !diff.UnloadedParams.Contains(k)).ToHashSet(StringComparer.Ordinal);

        if (!expected.SetEquals(rebuilt.Keys))
            throw new RegulationDiffException(
                $"Rebuilt params differ: missing {string.Join(", ", expected.Except(rebuilt.Keys))}; extra {string.Join(", ", rebuilt.Keys.Except(expected))}.");

        foreach (var name in expected)
        {
            var a = older.Params[name];
            var b = rebuilt[name];

            if (a.RowSize != b.RowSize)
                throw new RegulationDiffException($"{name}: rebuilt row size {b.RowSize}, expected {a.RowSize}.");

            if (a.Rows.Count != b.Rows.Count)
                throw new RegulationDiffException($"{name}: rebuilt {b.Rows.Count} rows, expected {a.Rows.Count}.");

            for (var i = 0; i < a.Rows.Count; i++)
            {
                var ra = a.Rows[i];
                var rb = b.Rows[i];

                if (ra.ID != rb.ID || ra.Name != rb.Name || !ra.DataEquals(rb))
                    throw new RegulationDiffException(
                        $"{name}: row {i} (ID {ra.ID}) does not rebuild exactly (got ID {rb.ID}).");
            }
        }
    }
}
