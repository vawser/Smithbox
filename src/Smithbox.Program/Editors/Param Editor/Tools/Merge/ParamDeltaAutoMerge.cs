using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamDeltaConflictStrategy
{
    StopOnConflict = 0,
    PreferFirst = 1,
    PreferLast = 2
}

public enum ParamDeltaMergeConflictType
{
    FieldValue = 0,
    RowState = 1,

    /// <summary>
    /// Two sources changed the same row table group differently. Resolved by picking one side's whole group.
    /// </summary>
    RowGroup = 2
}

public enum ParamDeltaConflictResolution
{
    Unresolved = 0,

    /// <summary>
    /// The candidate holding the earliest source's change
    /// </summary>
    UseEarlier = 1,

    /// <summary>
    /// The candidate holding the latest source's change
    /// </summary>
    UseLater = 2,

    Manual = 3,

    /// <summary>
    /// The candidate at <see cref="ParamDeltaMergeConflict.SelectedCandidate"/>
    /// </summary>
    UseSource = 4
}

/// <summary>
/// One distinct change at a conflict's location, with every source that made it (sources that made the same
/// change share one candidate)
/// </summary>
public sealed class ParamDeltaConflictCandidate
{
    public List<string> Sources { get; init; } = new();
    public string Source => string.Join(", ", Sources);

    /// <summary>
    /// The field value (FieldValue), or a description of the row / group
    /// </summary>
    public string Value { get; init; } = "";

    public RowDeltaState State { get; init; }

    internal RowDelta RowSnapshot { get; init; }
    internal RowGroupDelta GroupSnapshot { get; init; }

    // Source order (0 = earliest) of the first and last source with this change
    internal int FirstOrder { get; init; }
    internal int LastOrder { get; set; }
}

/// <summary>
/// One location (field, row or group) that two or more sources changed differently. Every distinct change is a
/// candidate, in the order the sources first made them; the resolution picks one (or a manual value for a field).
/// </summary>
public sealed class ParamDeltaMergeConflict
{
    public ParamDeltaMergeConflictType Type { get; init; }
    public string ParamName { get; init; } = "";
    public int RowID { get; init; }
    public int RowIndex { get; init; }
    public string Field { get; init; } = "";

    /// <summary>
    /// The field's annotated name (the internal name when it has none)
    /// </summary>
    public string FieldDisplayName { get; init; } = "";
    public string RowName { get; init; } = "";

    /// <summary>
    /// The field's vanilla value; null when the row isn't in vanilla
    /// </summary>
    public string VanillaValue { get; init; }

    public List<ParamDeltaConflictCandidate> Candidates { get; init; } = new();

    public ParamDeltaConflictResolution Resolution { get; set; } = ParamDeltaConflictResolution.Unresolved;
    public int SelectedCandidate { get; set; }
    public string ManualValue { get; set; } = "";

    /// <summary>
    /// False when the manual value can't be parsed as the field's type (a Manual resolution then counts as
    /// unresolved)
    /// </summary>
    public bool ManualValid { get; set; } = true;

    internal PARAMDEF.Field FieldDef { get; init; }

    /// <summary>
    /// For a field conflict on a row that some sources deleted: the row's RowState conflict
    /// </summary>
    internal ParamDeltaMergeConflict Parent { get; set; }

    /// <summary>
    /// The candidate holding the latest source's change
    /// </summary>
    public int LaterIndex
    {
        get
        {
            var best = 0;
            for (var i = 1; i < Candidates.Count; i++)
            {
                if (Candidates[i].LastOrder > Candidates[best].LastOrder)
                    best = i;
            }

            return best;
        }
    }

    public int? SelectedIndex => Resolution switch
    {
        ParamDeltaConflictResolution.UseEarlier => 0,
        ParamDeltaConflictResolution.UseLater => LaterIndex,
        ParamDeltaConflictResolution.UseSource when SelectedCandidate >= 0 && SelectedCandidate < Candidates.Count
            => SelectedCandidate,
        _ => null
    };

    public ParamDeltaConflictCandidate Selected => SelectedIndex is int i ? Candidates[i] : null;

    /// <summary>
    /// The row was resolved as deleted, so this field conflict no longer matters
    /// </summary>
    public bool IsSuperseded => Parent != null && Parent.Selected?.State == RowDeltaState.Deleted;

    public bool IsResolved => IsSuperseded ||
        (Resolution != ParamDeltaConflictResolution.Unresolved &&
         (Resolution != ParamDeltaConflictResolution.Manual || ManualValid) &&
         (Resolution == ParamDeltaConflictResolution.Manual || Selected != null));

    // The earliest and latest changes, for callers that show two sides (Full Mod merge report)
    public string ExistingSource => Candidates.Count > 0 ? Candidates[0].Source : "";
    public string IncomingSource => Candidates.Count > 0 ? Candidates[LaterIndex].Source : "";
    public string ExistingValue => Candidates.Count > 0 ? Candidates[0].Value : "";
    public string IncomingValue => Candidates.Count > 0 ? Candidates[LaterIndex].Value : "";
    public RowDeltaState ExistingState => Candidates.Count > 0 ? Candidates[0].State : default;
    public RowDeltaState IncomingState => Candidates.Count > 0 ? Candidates[LaterIndex].State : default;
}

public sealed class ParamDeltaAutoMergeResult
{
    public ParamDeltaPatch Patch { get; init; } = new();
    public List<ParamDeltaMergeConflict> Conflicts { get; } = new();
    public List<string> Errors { get; } = new();
    public int SourceCount { get; set; }
    public int SafeRows { get; set; }
    public int SafeFields { get; set; }
    public int IdenticalFields { get; set; }
    public int SafeGroups { get; set; }
    public int IdenticalGroups { get; set; }
    public ParamDeltaConflictStrategy Strategy { get; set; }

    public int ResolvedConflicts => Conflicts.Count(e => e.IsResolved);
    public int UnresolvedConflicts => Conflicts.Count - ResolvedConflicts;

    public bool CanApply => Errors.Count == 0 && Conflicts.All(e => e.IsResolved);
}

public sealed class ParamDeltaAutoMergeEngine
{
    private readonly ParamDeltaPatcher Patcher;

    private readonly record struct RowKey(string ParamName, int ID, int Index);
    private readonly record struct GroupKey(string ParamName, int ID);

    private readonly record struct RowEntry(int Order, string Source, RowDelta Row);
    private readonly record struct GroupEntry(int Order, string Source, RowGroupDelta Group);

    // Vanilla rows by (ID, Index), per param, for the current Merge call
    private Dictionary<string, Dictionary<(int ID, int Index), Param.Row>> VanillaPositions = new(StringComparer.Ordinal);

    public ParamDeltaAutoMergeEngine(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
    }

    public static ParamDeltaConflictResolution ResolutionFromStrategy(ParamDeltaConflictStrategy strategy)
    {
        return strategy switch
        {
            ParamDeltaConflictStrategy.PreferFirst => ParamDeltaConflictResolution.UseEarlier,
            ParamDeltaConflictStrategy.PreferLast => ParamDeltaConflictResolution.UseLater,
            _ => ParamDeltaConflictResolution.Unresolved
        };
    }

    /// <summary>
    /// Writes the conflict choices into the merged patch. Idempotent: called whenever a choice changes and before
    /// build, import and save. Row and group choices go first, since a row choice replaces the whole row that the
    /// field choices then write into.
    /// </summary>
    public void ApplyConflictResolutions(ParamDeltaAutoMergeResult result)
    {
        if (result == null)
            return;

        var paramsByName = result.Patch.Params.ToDictionary(e => e.Name, StringComparer.Ordinal);
        var rowIndex = new Dictionary<(string Param, int ID, int Index), int>();

        foreach (var param in result.Patch.Params)
        {
            for (var i = 0; i < param.Rows.Count; i++)
                rowIndex[(param.Name, param.Rows[i].ID, param.Rows[i].Index)] = i;
        }

        var ordered = result.Conflicts
            .Where(e => e.Type != ParamDeltaMergeConflictType.FieldValue)
            .Concat(result.Conflicts.Where(e => e.Type == ParamDeltaMergeConflictType.FieldValue));

        foreach (var conflict in ordered)
        {
            if (!conflict.IsResolved || conflict.IsSuperseded)
                continue;

            if (!paramsByName.TryGetValue(conflict.ParamName, out var param))
                continue;

            var selected = conflict.Selected;

            if (conflict.Type == ParamDeltaMergeConflictType.RowGroup)
            {
                if (selected?.GroupSnapshot == null)
                    continue;

                var groupIndex = param.Groups.FindIndex(e => e.ID == conflict.RowID);
                var clone = ParamRowGroups.CloneGroup(selected.GroupSnapshot);

                if (groupIndex >= 0)
                    param.Groups[groupIndex] = clone;
                else
                    param.Groups.Add(clone);

                continue;
            }

            var key = (conflict.ParamName, conflict.RowID, conflict.RowIndex);

            if (conflict.Type == ParamDeltaMergeConflictType.RowState)
            {
                if (selected?.RowSnapshot == null)
                    continue;

                var rowClone = CloneRow(selected.RowSnapshot);

                if (rowIndex.TryGetValue(key, out var index))
                {
                    param.Rows[index] = rowClone;
                }
                else
                {
                    rowIndex[key] = param.Rows.Count;
                    param.Rows.Add(rowClone);
                }

                continue;
            }

            // Field value
            if (!rowIndex.TryGetValue(key, out var fieldRowIndex))
                continue;

            var row = param.Rows[fieldRowIndex];
            if (row.State == RowDeltaState.Deleted)
                continue;

            var value = conflict.Resolution == ParamDeltaConflictResolution.Manual
                ? conflict.ManualValue ?? ""
                : selected?.Value;

            if (value == null)
                continue;

            var field = row.Fields.FirstOrDefault(e => e.Field == conflict.Field);
            if (field == null)
            {
                field = new FieldDelta { Field = conflict.Field };
                row.Fields.Add(field);
            }

            field.Value = value;
        }
    }

    /// <summary>
    /// Merges the sources (earliest first) against vanilla. Every source's change at a location is collected first;
    /// one distinct change is merged safely, two or more are one conflict listing each distinct change.
    /// </summary>
    public ParamDeltaAutoMergeResult Merge(
        IReadOnlyList<DeltaImportEntry> sources,
        ParamDeltaConflictStrategy strategy)
    {
        var result = new ParamDeltaAutoMergeResult
        {
            SourceCount = sources.Count,
            Strategy = strategy,
            Patch = new ParamDeltaPatch
            {
                ProjectType = Patcher.Project.Descriptor.ProjectType,
                ParamVersion = Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion,
                Tag = LOC.Get("PARAM_AutoMerge_Patch_Tag", sources.Count)
            }
        };

        if (sources.Count < 2)
        {
            result.Errors.Add(LOC.Get("PARAM_AutoMerge_Missing_Two_Delta_Patches"));
            return result;
        }

        ValidateSources(sources, result);
        if (result.Errors.Count > 0)
            return result;

        VanillaPositions = new Dictionary<string, Dictionary<(int ID, int Index), Param.Row>>(StringComparer.Ordinal);

        var rowEntries = new Dictionary<RowKey, List<RowEntry>>();
        var groupEntries = new Dictionary<GroupKey, List<GroupEntry>>();
        var vanillaGroups = new Dictionary<string, Dictionary<int, RowGroupDelta>>(StringComparer.Ordinal);

        for (var order = 0; order < sources.Count; order++)
        {
            var source = sources[order];

            foreach (var sourceParamDelta in source.Delta.Params)
            {
                var paramDelta = PrepareParamDelta(sourceParamDelta);

                foreach (var group in paramDelta.Groups)
                {
                    var incoming = NormalizeGroup(paramDelta.Name, group, vanillaGroups);
                    if (incoming == null)
                        continue;

                    var key = new GroupKey(paramDelta.Name, group.ID);
                    if (!groupEntries.TryGetValue(key, out var list))
                        groupEntries[key] = list = new List<GroupEntry>();

                    list.Add(new GroupEntry(order, source.Filename, incoming));
                }

                foreach (var originalRow in paramDelta.Rows)
                {
                    var incoming = NormalizeRow(paramDelta.Name, originalRow);
                    var key = new RowKey(paramDelta.Name, incoming.ID, incoming.Index);

                    if (!rowEntries.TryGetValue(key, out var list))
                        rowEntries[key] = list = new List<RowEntry>();

                    list.Add(new RowEntry(order, source.Filename, incoming));
                }
            }
        }

        var mergedGroups = new Dictionary<GroupKey, RowGroupDelta>();
        foreach (var (key, entries) in groupEntries)
        {
            mergedGroups[key] = MergeGroup(key, entries, strategy, result);
        }

        var mergedRows = new Dictionary<RowKey, RowDelta>();
        foreach (var (key, entries) in rowEntries)
        {
            mergedRows[key] = MergeRow(key, entries, strategy, result);
        }

        var paramNames = mergedRows.Keys.Select(e => e.ParamName)
            .Concat(mergedGroups.Keys.Select(e => e.ParamName))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(e => e, StringComparer.Ordinal);

        foreach (var paramName in paramNames)
        {
            var paramDelta = new ParamDelta { Name = paramName };

            foreach (var row in mergedRows
                         .Where(e => e.Key.ParamName == paramName)
                         .OrderBy(e => e.Key.ID)
                         .ThenBy(e => e.Key.Index))
            {
                if (row.Value.State == RowDeltaState.Modified && row.Value.Fields.Count == 0)
                    continue;

                paramDelta.Rows.Add(CloneRow(row.Value));
            }

            foreach (var group in mergedGroups
                         .Where(e => e.Key.ParamName == paramName)
                         .OrderBy(e => e.Key.ID))
            {
                paramDelta.Groups.Add(ParamRowGroups.CloneGroup(group.Value));
            }

            if (paramDelta.Rows.Count > 0 || paramDelta.Groups.Count > 0)
                result.Patch.Params.Add(paramDelta);
        }

        VanillaPositions = new(StringComparer.Ordinal);

        return result;
    }

    /// <summary>
    /// A copy of a source's param delta. For a row table param, rows from patches written before groups existed
    /// are turned into whole groups (positions counted against vanilla, which is what those patches were built
    /// against), so every table param change is merged as groups.
    /// </summary>
    private ParamDelta PrepareParamDelta(ParamDelta source)
    {
        var copy = new ParamDelta
        {
            Name = source.Name,
            Rows = source.Rows.ToList(),
            Groups = source.Groups.ToList()
        };

        if (copy.Rows.Count > 0 && ParamRowGroups.IsTableParam(Patcher.Project, source.Name))
        {
            Patcher.Project.Handler.ParamData.VanillaBank.Params.TryGetValue(source.Name, out var vanillaParam);

            copy.Rows = copy.Rows.Select(CloneRow).ToList();
            ParamRowGroups.ConvertPositionRows(copy, vanillaParam);
        }

        return copy;
    }

    /// <summary>
    /// A source's group with its state taken from vanilla, not the source (a Selected/All export writes every group
    /// it holds). Null when the source leaves the group as it is in vanilla, which doesn't count as a change.
    /// </summary>
    private RowGroupDelta NormalizeGroup(string paramName, RowGroupDelta source,
        Dictionary<string, Dictionary<int, RowGroupDelta>> vanillaGroupCache)
    {
        var vanillaGroup = GetVanillaGroup(paramName, source.ID, vanillaGroupCache);
        var incoming = ParamRowGroups.CloneGroup(source);

        if (incoming.State == RowDeltaState.Deleted || incoming.Rows.Count == 0)
        {
            if (vanillaGroup == null)
                return null;

            incoming.State = RowDeltaState.Deleted;
            incoming.Rows.Clear();
        }
        else if (vanillaGroup == null)
        {
            incoming.State = RowDeltaState.Added;
        }
        else
        {
            if (ParamRowGroups.GroupContentEquals(incoming, vanillaGroup))
                return null;

            incoming.State = RowDeltaState.Modified;
        }

        return incoming;
    }

    /// <summary>
    /// Three-way merge of one row table group: one distinct change is taken; two or more are one RowGroup conflict
    /// holding each distinct whole group.
    /// </summary>
    private RowGroupDelta MergeGroup(GroupKey key, List<GroupEntry> entries, ParamDeltaConflictStrategy strategy,
        ParamDeltaAutoMergeResult result)
    {
        var candidates = new List<(RowGroupDelta Group, ParamDeltaConflictCandidate Candidate)>();

        foreach (var entry in entries)
        {
            var match = candidates.FindIndex(c => ParamRowGroups.GroupContentEquals(c.Group, entry.Group));

            if (match >= 0)
            {
                candidates[match].Candidate.Sources.Add(entry.Source);
                candidates[match].Candidate.LastOrder = entry.Order;
                continue;
            }

            candidates.Add((entry.Group, new ParamDeltaConflictCandidate
            {
                Sources = new List<string> { entry.Source },
                Value = DescribeGroup(entry.Group),
                State = entry.Group.State,
                GroupSnapshot = ParamRowGroups.CloneGroup(entry.Group),
                FirstOrder = entry.Order,
                LastOrder = entry.Order
            }));
        }

        if (candidates.Count == 1)
        {
            result.SafeGroups++;
            result.IdenticalGroups += entries.Count - 1;
            return candidates[0].Group;
        }

        var conflict = new ParamDeltaMergeConflict
        {
            Type = ParamDeltaMergeConflictType.RowGroup,
            ParamName = key.ParamName,
            RowID = key.ID,
            RowName = candidates.Select(c => c.Group.Rows.FirstOrDefault()?.Name)
                .FirstOrDefault(n => !string.IsNullOrEmpty(n)) ?? "",
            Candidates = candidates.Select(c => c.Candidate).ToList(),
            Resolution = ResolutionFromStrategy(strategy)
        };

        result.Conflicts.Add(conflict);

        // Until it's resolved, the patch holds the strategy's pick (or the earliest change)
        return ParamRowGroups.CloneGroup((conflict.Selected ?? conflict.Candidates[0]).GroupSnapshot);
    }

    /// <summary>
    /// One row's changes from every source. Fields are merged among the sources that keep the row: one distinct
    /// value is safe, two or more are a FieldValue conflict. If some sources delete the row and others keep it,
    /// that's a RowState conflict (Deleted or the kept, field-merged row); choosing Deleted makes its field conflicts
    /// irrelevant.
    /// </summary>
    private RowDelta MergeRow(RowKey key, List<RowEntry> entries, ParamDeltaConflictStrategy strategy,
        ParamDeltaAutoMergeResult result)
    {
        var deleting = entries.Where(e => e.Row.State == RowDeltaState.Deleted).ToList();
        var keeping = entries.Where(e => e.Row.State != RowDeltaState.Deleted).ToList();

        if (keeping.Count == 0)
        {
            result.SafeRows++;
            return CloneRow(deleting[0].Row);
        }

        var vanillaRow = GetVanillaRow(key.ParamName, key.ID, key.Index);
        var fieldConflicts = new List<ParamDeltaMergeConflict>();
        var kept = MergeFields(key, keeping, vanillaRow, strategy, result, fieldConflicts);

        if (deleting.Count == 0)
        {
            result.SafeRows++;
            result.Conflicts.AddRange(fieldConflicts);
            return kept;
        }

        var deletedCandidate = new ParamDeltaConflictCandidate
        {
            Sources = deleting.Select(e => e.Source).ToList(),
            Value = LOC.Get("PARAM_AutoMerge_Row_Description_Deleted"),
            State = RowDeltaState.Deleted,
            RowSnapshot = new RowDelta { ID = key.ID, Index = key.Index, Name = kept.Name, State = RowDeltaState.Deleted },
            FirstOrder = deleting.Min(e => e.Order),
            LastOrder = deleting.Max(e => e.Order)
        };

        var keptCandidate = new ParamDeltaConflictCandidate
        {
            Sources = keeping.Select(e => e.Source).Distinct().ToList(),
            Value = LOC.Get("PARAM_AutoMerge_Row_Description_Kept", kept.State, kept.Fields.Count),
            State = kept.State,
            RowSnapshot = CloneRow(kept),
            FirstOrder = keeping.Min(e => e.Order),
            LastOrder = keeping.Max(e => e.Order)
        };

        var stateConflict = new ParamDeltaMergeConflict
        {
            Type = ParamDeltaMergeConflictType.RowState,
            ParamName = key.ParamName,
            RowID = key.ID,
            RowIndex = key.Index,
            RowName = kept.Name ?? vanillaRow?.Name ?? "",
            Candidates = deletedCandidate.FirstOrder <= keptCandidate.FirstOrder
                ? new List<ParamDeltaConflictCandidate> { deletedCandidate, keptCandidate }
                : new List<ParamDeltaConflictCandidate> { keptCandidate, deletedCandidate },
            Resolution = ResolutionFromStrategy(strategy)
        };

        foreach (var fieldConflict in fieldConflicts)
            fieldConflict.Parent = stateConflict;

        result.Conflicts.Add(stateConflict);
        result.Conflicts.AddRange(fieldConflicts);

        return CloneRow((stateConflict.Selected ?? stateConflict.Candidates[0]).RowSnapshot);
    }

    private RowDelta MergeFields(RowKey key, List<RowEntry> keeping, Param.Row vanillaRow,
        ParamDeltaConflictStrategy strategy, ParamDeltaAutoMergeResult result, List<ParamDeltaMergeConflict> conflicts)
    {
        var merged = new RowDelta
        {
            ID = key.ID,
            Index = key.Index,
            // The actual row state is determined against vanilla, not by the sources
            // (Selected/All delta exports represent full rows as Added)
            State = vanillaRow != null ? RowDeltaState.Modified : RowDeltaState.Added,
            Name = keeping.Select(e => e.Row.Name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? ""
        };

        // Each field's values, in the order the sources first set them
        var fieldOrder = new List<string>();
        var fieldValues = new Dictionary<string, List<(int Order, string Source, string Value)>>(StringComparer.Ordinal);

        foreach (var entry in keeping)
        {
            foreach (var field in entry.Row.Fields)
            {
                if (!fieldValues.TryGetValue(field.Field, out var values))
                {
                    fieldValues[field.Field] = values = new();
                    fieldOrder.Add(field.Field);
                }

                values.Add((entry.Order, entry.Source, field.Value ?? ""));
            }
        }

        foreach (var fieldName in fieldOrder)
        {
            var values = fieldValues[fieldName];
            var candidates = new List<ParamDeltaConflictCandidate>();

            foreach (var (order, source, value) in values)
            {
                var match = candidates.Find(c => string.Equals(c.Value, value, StringComparison.Ordinal));

                if (match != null)
                {
                    match.Sources.Add(source);
                    match.LastOrder = order;
                    continue;
                }

                candidates.Add(new ParamDeltaConflictCandidate
                {
                    Sources = new List<string> { source },
                    Value = value,
                    State = merged.State,
                    FirstOrder = order,
                    LastOrder = order
                });
            }

            if (candidates.Count == 1)
            {
                merged.Fields.Add(new FieldDelta { Field = fieldName, Value = candidates[0].Value });
                result.SafeFields++;
                result.IdenticalFields += values.Count - 1;
                continue;
            }

            var def = vanillaRow?.Def.Fields.FirstOrDefault(f => f.InternalName == fieldName);

            var conflict = new ParamDeltaMergeConflict
            {
                Type = ParamDeltaMergeConflictType.FieldValue,
                ParamName = key.ParamName,
                RowID = key.ID,
                RowIndex = key.Index,
                Field = fieldName,
                FieldDisplayName = GetFieldDisplayName(vanillaRow, fieldName),
                RowName = merged.Name.Length > 0 ? merged.Name : vanillaRow?.Name ?? "",
                VanillaValue = vanillaRow != null ? GetFieldValueString(vanillaRow, fieldName) : null,
                FieldDef = def,
                Candidates = candidates,
                Resolution = ResolutionFromStrategy(strategy)
            };

            conflicts.Add(conflict);

            // Until it's resolved, the patch holds the strategy's pick (or the earliest value)
            merged.Fields.Add(new FieldDelta
            {
                Field = fieldName,
                Value = (conflict.Selected ?? candidates[0]).Value
            });
        }

        return merged;
    }

    private string GetFieldDisplayName(Param.Row vanillaRow, string fieldName)
    {
        if (vanillaRow == null)
            return fieldName;

        var paramData = Patcher.Project.Handler.ParamData;
        var annotations = paramData.GetParamAnnotations(vanillaRow.Def.ParamType);
        var name = paramData.GetFieldAnnotation(annotations, fieldName)?.Name;

        return string.IsNullOrWhiteSpace(name) ? fieldName : name;
    }

    private static string GetFieldValueString(Param.Row row, string fieldName)
    {
        var column = row.Columns.FirstOrDefault(e => e.Def.InternalName == fieldName);
        if (column == null)
            return null;

        var value = column.GetValue(row);

        if (column.Def.InternalType == "dummy8" && column.Def.ArrayLength > 1)
            return ParamUtils.Dummy8Write((byte[])value);

        return value?.ToString() ?? "";
    }

    private static string DescribeGroup(RowGroupDelta group)
    {
        return group.State == RowDeltaState.Deleted
            ? LOC.Get("PARAM_AutoMerge_Group_Description_Deleted")
            : LOC.Get("PARAM_AutoMerge_Group_Description", group.State, group.Rows.Count);
    }

    private RowGroupDelta GetVanillaGroup(string paramName, int id,
        Dictionary<string, Dictionary<int, RowGroupDelta>> cache)
    {
        if (!cache.TryGetValue(paramName, out var groups))
        {
            groups = new Dictionary<int, RowGroupDelta>();

            if (Patcher.Project.Handler.ParamData.VanillaBank.Params.TryGetValue(paramName, out var vanillaParam))
            {
                foreach (var (groupID, rows) in ParamRowGroups.Build(vanillaParam))
                {
                    groups[groupID] = ParamRowGroups.SerializeGroup(groupID, rows, RowDeltaState.Modified);
                }
            }

            cache[paramName] = groups;
        }

        return groups.GetValueOrDefault(id);
    }

    private void ValidateSources(IReadOnlyList<DeltaImportEntry> sources, ParamDeltaAutoMergeResult result)
    {
        var projectType = Patcher.Project.Descriptor.ProjectType;
        var paramVersion = Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion;

        foreach (var source in sources)
        {
            if (source.Delta.ProjectType != projectType)
            {
                result.Errors.Add(
                    LOC.Get("PARAM_AutoMerge_Invalid_Project_Type", source.Filename, source.Delta.ProjectType, projectType));
            }

            if (source.Delta.ParamVersion != paramVersion)
            {
                result.Errors.Add(
                    LOC.Get("PARAM_AutoMerge_Invalid_Param_Version", source.Filename,
                    ParamUtils.ParseRegulationVersion(source.Delta.ParamVersion),
                    ParamUtils.ParseRegulationVersion(paramVersion)));
            }
        }
    }

    private RowDelta NormalizeRow(string paramName, RowDelta source)
    {
        var normalized = CloneRow(source);
        var vanillaRow = GetVanillaRow(paramName, source.ID, source.Index);

        if (source.State == RowDeltaState.Deleted)
        {
            normalized.Fields.Clear();
            return normalized;
        }

        if (vanillaRow == null)
        {
            normalized.State = RowDeltaState.Added;
            return normalized;
        }

        normalized.State = RowDeltaState.Modified;

        // A full-row delta (Selected/All export) may label an existing vanilla row as Added.
        // Remove values that are identical to vanilla so they do not create false conflicts.
        if (source.State == RowDeltaState.Added)
        {
            normalized.Fields = normalized.Fields
                .Where(field => !FieldEqualsVanilla(vanillaRow, field))
                .ToList();
        }

        return normalized;
    }

    private bool FieldEqualsVanilla(Param.Row vanillaRow, FieldDelta field)
    {
        var column = vanillaRow.Columns.FirstOrDefault(e => e.Def.InternalName == field.Field);
        if (column == null)
            return false;

        var value = column.GetValue(vanillaRow);
        string vanillaValue;

        if (column.Def.InternalType == "dummy8" && column.Def.ArrayLength > 1)
            vanillaValue = ParamUtils.Dummy8Write((byte[])value);
        else
            vanillaValue = value?.ToString() ?? "";

        return string.Equals(vanillaValue, field.Value, StringComparison.Ordinal);
    }

    /// <summary>
    /// The vanilla row at (ID, Index), numbered as <see cref="ParamRowGroups.BuildPositionMap"/> does. The map is
    /// built once per param per merge (a scan per row was rows x changed rows for a whole-project source).
    /// </summary>
    private Param.Row GetVanillaRow(string paramName, int id, int index)
    {
        if (!VanillaPositions.TryGetValue(paramName, out var positions))
        {
            Patcher.Project.Handler.ParamData.VanillaBank.Params.TryGetValue(paramName, out var vanillaParam);
            positions = ParamRowGroups.BuildPositionMap(vanillaParam);
            VanillaPositions[paramName] = positions;
        }

        if (positions.TryGetValue((id, index), out var row))
            return row;

        return null;
    }

    private static RowDelta CloneRow(RowDelta row)
    {
        var clone = new RowDelta
        {
            ID = row.ID,
            Index = row.Index,
            Name = row.Name,
            State = row.State
        };

        foreach (var field in row.Fields)
            clone.Fields.Add(CloneField(field));

        return clone;
    }

    private static FieldDelta CloneField(FieldDelta field)
    {
        return new FieldDelta
        {
            Field = field.Field,
            Value = field.Value
        };
    }
}

public sealed class ParamDeltaAutoMergeTool
{
    private readonly ParamDeltaPatcher Patcher;
    private readonly ParamDeltaAutoMergeEngine Engine;
    private readonly ParamRegulationAutoMerge RegulationMerge;
    private readonly ParamFullModAutoMerge FullModMerge;
    private readonly HashSet<string> SelectedFiles = new(StringComparer.OrdinalIgnoreCase);
    // Direct merge sources, earliest first (filled with two empty rows by LoadDirectMergeSettings)
    private readonly List<MergeSourceEntry> MergeSources = new();
    private readonly List<string> FullModFolderPaths = new() { "", "" };

    private ParamDeltaAutoMergeResult LastResult;
    private RegulationMergeAnalysis LastRegulationAnalysis;
    private FullModMergeAnalysis LastFullModAnalysis;
    private ParamDeltaConflictStrategy Strategy = ParamDeltaConflictStrategy.StopOnConflict;
    private string OutputName = "auto_merged";
    private string RegulationOutputPath = "";
    private string RegulationDeltaName = "";
    private string RegulationBuildStatus = "";
    private string FullModOutputPath = "";
    private string FullModBuildStatus = "";
    private bool FullModEnableBinderMerge = true;
    private bool FullModIgnoreMetadata = true;
    private bool FullModWriteReport = true;
    private string FullModConflictFilter = "";
    private const int ConflictRowsPerPage = 100;
    private const int MaxListedWarnings = 100;
    private int FullModConflictPage = 0;

    private bool DisplayMergeReportSummary = true;
    private bool DisplayBinderMergeSummary = true;
    private bool DisplayConflictResolver = true;
    private bool DisplayWarnings = true;
    private bool DisplayErrors = true;

    public ParamDeltaAutoMergeTool(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
        Engine = new ParamDeltaAutoMergeEngine(patcher);
        RegulationMerge = new ParamRegulationAutoMerge(patcher, Engine);
        FullModMerge = new ParamFullModAutoMerge(RegulationMerge)
        {
            VanillaGparam = ReadVanillaGparam
        };
    }

    /// <summary>
    /// The game's GPARAM for a Full Mod merge file (Gparam Merge Plan.md D4): the loose file at the same path, or
    /// the entry of that name in the game's binder there. Null when the game has none.
    /// </summary>
    private SoulsFormats.GPARAM ReadVanillaGparam(string relativePath, string entryName)
    {
        var fs = Patcher.Project?.VFS?.VanillaFS;
        if (fs == null || string.IsNullOrWhiteSpace(relativePath))
            return null;

        var path = "/" + relativePath.Replace('\\', '/').TrimStart('/');
        if (!fs.FileExists(path))
            return null;

        var key = StudioCore.Editors.GparamEditor.GparamMergeFileKey.FromPath(entryName ?? path);
        var (gparam, _) = StudioCore.Editors.GparamEditor.GparamMergeSource.ReadGparam(fs.ReadFile(path)?.ToArray(), key, path);
        return gparam;
    }
    public void DisplayConflictPolicy()
    {
        GUI.SimpleHeader(
            LOC.Get("PARAM_AutoMerge_Conflict_Policy_Header"),
            LOC.Get("PARAM_AutoMerge_Conflict_Policy_Header_TT"));

        var strategyName = GetStrategyName(Strategy);
        if (ImGui.BeginCombo("##autoMergeStrategy", strategyName))
        {
            foreach (ParamDeltaConflictStrategy value in Enum.GetValues(typeof(ParamDeltaConflictStrategy)))
            {
                if (ImGui.Selectable(GetStrategyName(value), value == Strategy))
                {
                    Strategy = value;
                    LastResult = null;
                    LastRegulationAnalysis = null;
                    LastFullModAnalysis = null;
                    RegulationBuildStatus = "";
                    FullModBuildStatus = "";
                }
            }
            ImGui.EndCombo();
        }
    }

    public void DisplayDeltaPatchMerge()
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_AutoMerge_Delta_Merge_Source_List_Header"),
            LOC.Get("PARAM_AutoMerge_Delta_Merge_Source_List_Header_TT"));

        // Select All
        if (ImGui.Button($"{Icons.Bars}##selectAllAction", DPI.IconButtonSize))
        {
            foreach (var entry in Patcher.Selection.ImportList)
            {
                if (entry.Delta.ProjectType == Patcher.Project.Descriptor.ProjectType &&
                    entry.Delta.ParamVersion == Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion)
                    SelectedFiles.Add(entry.Filename);
            }
        }
        GUI.Tooltip(LOC.Get("PARAM_AutoMerge_Delta_Merge_Select_All_TT"));

        ImGui.SameLine();

        // Clear Selection
        if (ImGui.Button($"{Icons.Minus}##clearSelectionAction", DPI.IconButtonSize))
        {
            SelectedFiles.Clear();
            LastResult = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_AutoMerge_Delta_Merge_Clear_Selection_TT"));

        ImGui.SameLine();

        // Refresh List
        if (ImGui.Button($"{Icons.Refresh}##refreshListAction", DPI.IconButtonSize))
        {
            Patcher.Selection.RefreshImportList();
            LastResult = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_AutoMerge_Delta_Merge_Refresh_List_TT"));

        // List
        ImGui.BeginChild("autoMergeSourceList", new System.Numerics.Vector2(0, 180), ImGuiChildFlags.Borders);

        foreach (var entry in Patcher.Selection.ImportList)
        {
            var isSelected = SelectedFiles.Contains(entry.Filename);
            var version = ParamUtils.ParseRegulationVersion(entry.Delta.ParamVersion);
            var sameGame = entry.Delta.ProjectType == Patcher.Project.Descriptor.ProjectType;
            var sameVersion = entry.Delta.ParamVersion == Patcher.Project.Handler.ParamData.PrimaryBank.ParamVersion;
            var compatibility = !sameGame ? LOC.Get("PARAM_AutoMerge_Delta_Merge_Diff_Game") : !sameVersion ? LOC.Get("PARAM_AutoMerge_Delta_Merge_Diff_Param_Ver") : "";
            var label = $"{entry.Filename} [{version}]{compatibility}##autoMerge_{entry.Filename.GetHashCode()}";

            if (ImGui.Checkbox(label, ref isSelected))
            {
                if (isSelected)
                    SelectedFiles.Add(entry.Filename);
                else
                    SelectedFiles.Remove(entry.Filename);

                LastResult = null;
            }
        }

        ImGui.EndChild();

        // Output Filename
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_AutoMerge_Delta_Merge_Output_Filename_Header"),
            LOC.Get("PARAM_AutoMerge_Delta_Merge_Output_Filename_Header_TT"));

        ImGui.InputTextWithHint("##autoMergeOutputName", LOC.Get("PARAM_AutoMerge_Delta_Merge_Output_Filename_Hint"), 
            ref OutputName, 255);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_AutoMerge_Actions_Header"),
            LOC.Get("PARAM_AutoMerge_Actions_Header_TT"));

        GUI.MultiButtonInput("deltaMergeActions",
            "analyze",
            LOC.Get("PARAM_AutoMerge_Analyze_Delta_Merge_Action"),
            LOC.Get("PARAM_AutoMerge_Analyze_Delta_Merge_Action_TT"),
            AnalyzeDeltaPatches);

        if (LastResult == null)
            return;

        // Summary
        GUI.Spacer();
        DisplayMergeSummary(LastResult, "delta");

        if (!LastResult.CanApply)
            return;

        GUI.MultiButtonInput("summaryMergeActions",
            "saveMergedDelta",
            LOC.Get("PARAM_AutoMerge_Save_Merged_Delta"),
            LOC.Get("PARAM_AutoMerge_Save_Merged_Delta_TT"),
            SaveMergedDelta,

            "importMergedDelta",
            LOC.Get("PARAM_AutoMerge_Import_Merged_Delta"),
            LOC.Get("PARAM_AutoMerge_Import_Merged_Delta_TT"),
            ImportMergedDelta);
    }

    private void SaveMergedDelta()
    {
        var name = string.IsNullOrWhiteSpace(OutputName) ? "auto_merged" : OutputName.Trim();
        if (!ConfirmDeltaOverwrite(name))
            return;

        Engine.ApplyConflictResolutions(LastResult);
        Patcher.WriteDeltaPatch(LastResult.Patch, name);
        Patcher.Selection.RefreshImportList();
    }

    /// <summary>
    /// False when a delta patch with this name exists and the user doesn't want to replace it
    /// </summary>
    private static bool ConfirmDeltaOverwrite(string name)
    {
        var writePath = Path.Combine(ProjectUtils.GetParamDeltaFolder(), $"{name}.json");

        if (!File.Exists(writePath))
            return true;

        var dialog = PlatformUtils.Instance.MessageBox(
            LOC.Get("PARAM_DeltaPatcher_Exporter_Overwrite_Patch", writePath),
            LOC.Get("SYS_Warning_Header"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        return dialog is not DialogResult.No;
    }

    private void ImportMergedDelta()
    {
        Engine.ApplyConflictResolutions(LastResult);
        var name = string.IsNullOrWhiteSpace(OutputName) ? "auto_merged" : OutputName.Trim();
        Patcher.Importer.ImportMergedDelta(name, LastResult.Patch);
    }

    /// <summary>
    /// One source row: a regulation file or another project (by name, so it survives the project list reloading)
    /// </summary>
    private sealed class MergeSourceEntry
    {
        public bool IsProject;
        public string Path = "";
        public string ProjectName = "";

        public const string ProjectPrefix = "project:";

        public string Serialize() => IsProject ? ProjectPrefix + ProjectName : Path;

        public static MergeSourceEntry Deserialize(string value)
        {
            return value.StartsWith(ProjectPrefix, StringComparison.Ordinal)
                ? new MergeSourceEntry { IsProject = true, ProjectName = value[ProjectPrefix.Length..] }
                : new MergeSourceEntry { Path = value };
        }

        public bool IsEmpty => IsProject ? string.IsNullOrWhiteSpace(ProjectName) : string.IsNullOrWhiteSpace(Path);
    }

    /// <summary>
    /// The other projects that can be a source: same game, not this project
    /// </summary>
    private List<ProjectEntry> GetMergeableProjects()
    {
        return Smithbox.Orchestrator.Projects
            .Where(p => p != null && p != Patcher.Project &&
                p.Descriptor.ProjectType == Patcher.Project.Descriptor.ProjectType)
            .ToList();
    }

    private void DisplayProjectSourceCombo(MergeSourceEntry entry, int index, int sourceNumber, List<ProjectEntry> projects)
    {
        var known = projects.Any(p => p.Descriptor.ProjectName == entry.ProjectName);
        var preview = string.IsNullOrEmpty(entry.ProjectName)
            ? LOC.Get("PARAM_DirectMerge_Select_Project_Hint")
            : known ? entry.ProjectName : LOC.Get("PARAM_DirectMerge_Project_Missing", entry.ProjectName);

        if (ImGui.BeginCombo($"{LOC.Get("PARAM_DirectMerge_Source", sourceNumber)}##autoMergeProjectSource{index}", preview))
        {
            if (projects.Count == 0)
                ImGui.TextDisabled(LOC.Get("PARAM_DirectMerge_No_Other_Projects"));

            foreach (var project in projects)
            {
                var name = project.Descriptor.ProjectName;

                if (ImGui.Selectable($"{name}##project_{index}_{project.Descriptor.ProjectGUID}", name == entry.ProjectName))
                {
                    entry.ProjectName = name;
                    ClearRegulationAnalysis();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Project_Source_TT"));
    }

    private MergeSourceEntry NewSourceEntry()
    {
        // Games without regulation files can only merge projects
        return new MergeSourceEntry { IsProject = !RegulationMerge.CanUseRegulationFiles };
    }

    public void DisplayDirectRegulationMerge()
    {
        var regulationFiles = RegulationMerge.CanUseRegulationFiles;

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DirectMerge_Options_Header"),
            LOC.Get("PARAM_DirectMerge_Options_Header_TT"));

        LoadDirectMergeSettings();
        PollRegulationAnalysis();

        var analyzing = RegulationAnalysisTask != null;

        // Nothing that changes the analysis can be edited while it runs
        if (analyzing)
            ImGui.BeginDisabled();

        var autoUpgradeRegulation = RegulationMerge.AutoUpgradeMismatchedVersions;

        // Upgrade older sources to this project's param version
        if (ImGui.Checkbox($"{LOC.Get("PARAM_DirectMerge_Apply_ParamVer_AutoUpgrade")}##autoMergeRegUpgrade", ref autoUpgradeRegulation))
        {
            RegulationMerge.AutoUpgradeMismatchedVersions = autoUpgradeRegulation;
            ClearRegulationAnalysis();
        }
        GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Apply_ParamVer_AutoUpgrade_TT"));

        // Include this project
        var includeProject = RegulationMerge.IncludeProject;
        if (ImGui.Checkbox($"{LOC.Get("PARAM_DirectMerge_Include_Project")}##autoMergeRegIncludeProject", ref includeProject))
        {
            RegulationMerge.IncludeProject = includeProject;
            ClearRegulationAnalysis();
        }
        GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Include_Project_TT"));

        // Sources
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DirectMerge_Sources_Header"),
            LOC.Get("PARAM_DirectMerge_Sources_Header_TT"));

        // Add
        if (ImGui.Button($"{Icons.Plus}##addEntryAction", DPI.IconButtonSize))
        {
            MergeSources.Add(NewSourceEntry());
            ClearRegulationAnalysis();
        }
        GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Add_Regulation_Source_TT"));

        ImGui.SameLine();

        // Reset
        if (ImGui.Button($"{LOC.Get("PARAM_DirectMerge_Reset_Source_List")}##resetEntryList", DPI.SelectorButtonSize))
        {
            MergeSources.Clear();
            MergeSources.Add(NewSourceEntry());
            MergeSources.Add(NewSourceEntry());

            RegulationOutputPath = "";
            ClearRegulationAnalysis();
            SaveDirectMergeSettings();
        }
        GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Reset_Source_List_TT"));

        // Sources, earliest first. This project (when included) is always the earliest.
        var sourceNumber = 1;

        if (RegulationMerge.IncludeProject)
        {
            ImGui.TextDisabled(LOC.Get("PARAM_DirectMerge_Source_Project_Row", sourceNumber));
            GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Include_Project_TT"));
            sourceNumber++;
        }

        var moveFrom = -1;
        var moveTo = -1;
        var removeAt = -1;
        var projects = GetMergeableProjects();

        for (var i = 0; i < MergeSources.Count; i++)
        {
            var entry = MergeSources[i];

            // Move earlier
            ImGui.BeginDisabled(i == 0);
            if (ImGui.Button($"{Icons.ArrowUp}##moveSourceUp{i}", DPI.IconButtonSize))
            {
                moveFrom = i;
                moveTo = i - 1;
            }
            GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Move_Source_Earlier_TT"));
            ImGui.EndDisabled();

            ImGui.SameLine();

            // Move later
            ImGui.BeginDisabled(i == MergeSources.Count - 1);
            if (ImGui.Button($"{Icons.ArrowDown}##moveSourceDown{i}", DPI.IconButtonSize))
            {
                moveFrom = i;
                moveTo = i + 1;
            }
            GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Move_Source_Later_TT"));
            ImGui.EndDisabled();

            ImGui.SameLine();

            // Remove (one entry is always kept)
            ImGui.BeginDisabled(MergeSources.Count < 2);
            if (ImGui.Button($"{Icons.Times}##removeSource{i}", DPI.IconButtonSize))
            {
                removeAt = i;
            }
            GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Remove_Regulation_Source_TT"));
            ImGui.EndDisabled();

            ImGui.SameLine();

            // Type: regulation file or project (projects only for games without regulation files)
            ImGui.BeginDisabled(!regulationFiles);
            ImGui.SetNextItemWidth(130f * DPI.UIScale());
            if (ImGui.BeginCombo($"##sourceType{i}", LOC.Get(entry.IsProject
                    ? "PARAM_DirectMerge_Source_Type_Project"
                    : "PARAM_DirectMerge_Source_Type_File")))
            {
                if (ImGui.Selectable(LOC.Get("PARAM_DirectMerge_Source_Type_File"), !entry.IsProject) && entry.IsProject)
                {
                    entry.IsProject = false;
                    ClearRegulationAnalysis();
                }

                if (ImGui.Selectable(LOC.Get("PARAM_DirectMerge_Source_Type_Project"), entry.IsProject) && !entry.IsProject)
                {
                    entry.IsProject = true;
                    ClearRegulationAnalysis();
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Source_Type_TT"));
            ImGui.EndDisabled();

            ImGui.SameLine();

            if (entry.IsProject)
            {
                DisplayProjectSourceCombo(entry, i, sourceNumber, projects);
            }
            else
            {
                // Select
                if (ImGui.Button($"{LOC.Get("PARAM_DirectMerge_Select_Path")}##selectPath{i}", DPI.SelectorButtonSize))
                {
                    var dialog = PlatformUtils.Instance.OpenFileDialog(LOC.Get("PARAM_DirectMerge_Select_Regulation"), out var path);

                    if (dialog)
                    {
                        entry.Path = path;
                        ClearRegulationAnalysis();
                    }
                }

                ImGui.SameLine();

                var value = entry.Path;
                if (ImGui.InputText($"{LOC.Get("PARAM_DirectMerge_Source", sourceNumber)}##autoMergeRegSource{i}", ref value, 1024))
                {
                    entry.Path = value;
                    ClearRegulationAnalysis();
                }
            }

            sourceNumber++;
        }

        // Applied after the loop, so the list doesn't change while it's drawn
        if (moveFrom >= 0)
        {
            (MergeSources[moveFrom], MergeSources[moveTo]) = (MergeSources[moveTo], MergeSources[moveFrom]);
            ClearRegulationAnalysis();
        }
        else if (removeAt >= 0)
        {
            MergeSources.RemoveAt(removeAt);
            ClearRegulationAnalysis();
        }

        GUI.MultiButtonInput("directMergeActions",
            "analyze",
            LOC.Get("PARAM_DirectMerge_Analyze_Regulation_Files"),
            LOC.Get("PARAM_DirectMerge_Analyze_Regulation_Files_TT"),
            AnalyzeRegulations);

        if (analyzing)
            ImGui.EndDisabled();

        // Status: what to do next
        GUI.Spacer();
        GUI.WrappedText(GetDirectMergeStatus());

        if (LastRegulationAnalysis == null)
            return;

        if (LastRegulationAnalysis.Sources.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DirectMerge_Loaded_Sources_Header"),
                LOC.Get("PARAM_DirectMerge_Loaded_Sources_Header_TT"));

            foreach (var source in LastRegulationAnalysis.Sources)
            {
                var versionText = source.AutoUpgraded
                    ? $"{ParamUtils.ParseRegulationVersion(source.OriginalParamVersion)} -> {ParamUtils.ParseRegulationVersion(source.ParamVersion)} {LOC.Get("PARAM_DirectMerge_Loaded_Autoupgraded")}"
                    : ParamUtils.ParseRegulationVersion(source.ParamVersion);

                GUI.WrappedText(
                    $"• {source.Filename} | {versionText} | " +
                    $"{source.ParsedParamCount} {LOC.Get("PARAM_DirectMerge_Loaded_Params")} | {source.Delta.Params.Sum(e => e.Rows.Count)} {LOC.Get("PARAM_DirectMerge_Loaded_Changed_Rows")}" +
                    (source.Delta.Params.Any(e => e.Groups.Count > 0)
                        ? $" | {source.Delta.Params.Sum(e => e.Groups.Count)} {LOC.Get("PARAM_DirectMerge_Loaded_Changed_Groups")}"
                        : ""));
            }
        }

        if (LastRegulationAnalysis.Warnings.Count > 0)
        {
            GUI.Spacer();
            GUI.ConditionalHeader(
                LOC.Get("PARAM_DirectMerge_Warnings_Header"),
                LOC.Get("PARAM_DirectMerge_Warnings_Header_TT"),
                ref DisplayWarnings);

            if(DisplayWarnings)
            {
                ImGui.BeginChild("warningsSection", new Vector2(0, 100));

                foreach (var warning in LastRegulationAnalysis.Warnings.Take(MaxListedWarnings))
                {
                    GUI.WrappedText($"• {warning}");
                }

                if (LastRegulationAnalysis.Warnings.Count > MaxListedWarnings)
                {
                    ImGui.TextDisabled(LOC.Get("PARAM_AutoMerge_More_Warnings",
                        LastRegulationAnalysis.Warnings.Count - MaxListedWarnings));
                }

                ImGui.EndChild();
            }
        }

        if (LastRegulationAnalysis.Errors.Count > 0)
        {
            GUI.Spacer();
            GUI.ConditionalHeader(
                LOC.Get("PARAM_DirectMerge_Errors_Header"),
                LOC.Get("PARAM_DirectMerge_Errors_Header_TT"),
                ref DisplayErrors);

            if (DisplayErrors)
            {
                foreach (var error in LastRegulationAnalysis.Errors)
                {
                    GUI.WrappedText($"• {error}");
                }
            }

            return;
        }

        if (LastRegulationAnalysis.MergeResult == null)
            return;

        GUI.Spacer();
        DisplayMergeSummary(LastRegulationAnalysis.MergeResult, "reg");

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DirectMerge_Output_Header"),
            LOC.Get("PARAM_DirectMerge_Output_Header_TT"));

        // Shown before it can be used, so the next step is visible; the status line above says why it's off
        var outputReady = IsDirectMergeReady();

        if (!outputReady)
            ImGui.BeginDisabled();

        // A regulation can only be built for games with encrypted regulation files (regulationFiles, above)

        // Output Path: a save dialog, so a new file name can be typed
        if (regulationFiles &&
            ImGui.Button($"{LOC.Get("PARAM_DirectMerge_Select_Path")}##selectOutputPath", DPI.SelectorButtonSize))
        {
            var filter = Patcher.Project.Descriptor.ProjectType == ProjectType.DS3
                ? FilterStrings.Data0Filter
                : FilterStrings.RegulationBinFilter;

            var startFolder = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(RegulationOutputPath))
                    startFolder = Path.GetDirectoryName(Path.GetFullPath(RegulationOutputPath.Trim().Trim('"'))) ?? "";
            }
            catch (Exception)
            {
                // An invalid typed path: open the dialog in its default folder
            }

            if (PlatformUtils.Instance.SaveFileDialog(LOC.Get("PARAM_DirectMerge_Select_Output_Regulation"),
                    new[] { filter }, startFolder, out var path) && !string.IsNullOrEmpty(path))
            {
                RegulationOutputPath = path;
            }
        }

        if (regulationFiles)
        {
            ImGui.SameLine();

            ImGui.InputTextWithHint(
                $"{LOC.Get("PARAM_DirectMerge_OutputPath")}##autoMergeRegOutput",
                LOC.Get("PARAM_DirectMerge_OutputPath_Hint"),
                ref RegulationOutputPath, 1024);
        }

        // Delta name, used by Save Merged Delta
        ImGui.InputTextWithHint(
            $"{LOC.Get("PARAM_DirectMerge_Delta_Name")}##autoMergeRegDeltaName",
            LOC.Get("PARAM_DirectMerge_Delta_Name_Hint"),
            ref RegulationDeltaName, 255);
        GUI.Tooltip(LOC.Get("PARAM_DirectMerge_Delta_Name_TT"));

        GUI.Spacer();

        if (regulationFiles)
        {
            GUI.MultiButtonInput("directMergeBuildActions",
                "buildRegulation",
                LOC.Get("PARAM_DirectMerge_Build_Regulation_Action"),
                LOC.Get("PARAM_DirectMerge_Build_Regulation_Action_TT"),
                BuildRegulation,

                "importMergedChanges",
                LOC.Get("PARAM_DirectMerge_Import_Merged_Changes"),
                LOC.Get("PARAM_DirectMerge_Import_Merged_Changes_TT"),
                ImportMergedRegulationChanges,

                "saveMergedChanges",
                LOC.Get("PARAM_DirectMerge_Save_Merged_Delta"),
                LOC.Get("PARAM_DirectMerge_Save_Merged_Delta_TT"),
                SaveMergedRegulationChanges);
        }
        else
        {
            GUI.MultiButtonInput("directMergeBuildActions",
                "importMergedChanges",
                LOC.Get("PARAM_DirectMerge_Import_Merged_Changes"),
                LOC.Get("PARAM_DirectMerge_Import_Merged_Changes_TT"),
                ImportMergedRegulationChanges,

                "saveMergedChanges",
                LOC.Get("PARAM_DirectMerge_Save_Merged_Delta"),
                LOC.Get("PARAM_DirectMerge_Save_Merged_Delta_TT"),
                SaveMergedRegulationChanges);
        }

        if (!outputReady)
            ImGui.EndDisabled();

        if (!string.IsNullOrWhiteSpace(RegulationBuildStatus))
        {
            GUI.Spacer();
            GUI.WrappedText(RegulationBuildStatus);
        }
    }

    #region Direct merge state
    private Task<RegulationMergeAnalysis> RegulationAnalysisTask;
    private bool RegulationAnalysisIncludesProject;
    private int RegulationAnalysisActionVersion;
    private bool DirectMergeSettingsLoaded;

    private void ClearRegulationAnalysis()
    {
        LastRegulationAnalysis = null;
        RegulationBuildStatus = "";
    }

    /// <summary>
    /// True when the project was a source and has been edited since the analysis: the merge holds its old state
    /// </summary>
    private bool IsRegulationAnalysisStale()
    {
        return LastRegulationAnalysis != null && RegulationAnalysisIncludesProject &&
            Patcher.View.Editor.ActionManager.Version != RegulationAnalysisActionVersion;
    }

    private bool IsDirectMergeReady()
    {
        return RegulationAnalysisTask == null && LastRegulationAnalysis != null &&
            LastRegulationAnalysis.CanBuild && !IsRegulationAnalysisStale();
    }

    private string GetDirectMergeStatus()
    {
        if (RegulationAnalysisTask != null)
            return LOC.Get("PARAM_DirectMerge_Status_Analyzing");

        if (LastRegulationAnalysis == null)
            return LOC.Get("PARAM_DirectMerge_Status_Not_Analyzed");

        if (LastRegulationAnalysis.Errors.Count > 0)
            return LOC.Get("PARAM_DirectMerge_Status_Errors", LastRegulationAnalysis.Errors.Count);

        if (IsRegulationAnalysisStale())
            return LOC.Get("PARAM_DirectMerge_Status_Stale");

        var unresolved = LastRegulationAnalysis.MergeResult?.UnresolvedConflicts ?? 0;
        if (unresolved > 0)
            return LOC.Get("PARAM_DirectMerge_Status_Conflicts", unresolved);

        return LOC.Get(RegulationMerge.CanUseRegulationFiles
            ? "PARAM_DirectMerge_Status_Ready"
            : "PARAM_DirectMerge_Status_Ready_No_Regulation");
    }

    /// <summary>
    /// The project's last sources and output path, once per session
    /// </summary>
    private void LoadDirectMergeSettings()
    {
        if (DirectMergeSettingsLoaded)
            return;

        DirectMergeSettingsLoaded = true;

        var descriptor = Patcher.Project.Descriptor;

        MergeSources.Clear();

        if (descriptor.DirectMergeSources?.Count > 0)
        {
            // Regulation files saved for a game that can't read them are dropped
            MergeSources.AddRange(descriptor.DirectMergeSources
                .Select(MergeSourceEntry.Deserialize)
                .Where(e => e.IsProject || RegulationMerge.CanUseRegulationFiles));
        }

        while (MergeSources.Count < 2)
            MergeSources.Add(NewSourceEntry());

        if (!string.IsNullOrWhiteSpace(descriptor.DirectMergeOutputPath))
            RegulationOutputPath = descriptor.DirectMergeOutputPath;
    }

    private void SaveDirectMergeSettings()
    {
        var descriptor = Patcher.Project.Descriptor;
        var sources = MergeSources.Where(e => !e.IsEmpty).Select(e => e.Serialize()).ToList();
        var output = RegulationOutputPath?.Trim() ?? "";

        if ((descriptor.DirectMergeSources ?? new()).SequenceEqual(sources) && descriptor.DirectMergeOutputPath == output)
            return;

        descriptor.DirectMergeSources = sources;
        descriptor.DirectMergeOutputPath = output;

        Smithbox.Orchestrator.SaveProject(Patcher.Project);
    }

    /// <summary>
    /// Picks up a finished background analysis (called every frame on the UI thread)
    /// </summary>
    private void PollRegulationAnalysis()
    {
        if (RegulationAnalysisTask == null || !RegulationAnalysisTask.IsCompleted)
            return;

        var task = RegulationAnalysisTask;
        RegulationAnalysisTask = null;
        Patcher.MergeProgressModal.DisplayModal = false;

        if (task.IsFaulted)
        {
            var failed = new RegulationMergeAnalysis();
            failed.Errors.Add(task.Exception?.GetBaseException().Message ?? "");
            LastRegulationAnalysis = failed;
            return;
        }

        LastRegulationAnalysis = task.Result;

        var firstFile = LastRegulationAnalysis.Sources.FirstOrDefault(e => !e.IsProject);
        if (firstFile != null && string.IsNullOrWhiteSpace(RegulationOutputPath))
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(firstFile.Path));
            RegulationOutputPath = Path.Combine(directory ?? "", Patcher.Project.Descriptor.ProjectType == ProjectType.DS3
                ? "merged_Data0.bdt"
                : "merged_regulation.bin");
        }

        SaveDirectMergeSettings();
    }
    #endregion

    private void ImportMergedRegulationChanges()
    {
        if (!IsDirectMergeReady())
            return;

        Engine.ApplyConflictResolutions(LastRegulationAnalysis.MergeResult);
        Patcher.Importer.ImportMergedDelta(LOC.Get("PARAM_DirectMerge_Import_Name"), LastRegulationAnalysis.MergeResult.Patch);

        // The import itself doesn't make the analysis stale (importing it again changes nothing), but undoing it
        // or any other edit does
        RegulationAnalysisActionVersion = Patcher.View.Editor.ActionManager.Version;
        RegulationBuildStatus = LOC.Get("PARAM_DirectMerge_Imported_Merged_Changes");
    }

    private void SaveMergedRegulationChanges()
    {
        if (!IsDirectMergeReady())
            return;

        var name = string.IsNullOrWhiteSpace(RegulationDeltaName) ? "direct_merge" : RegulationDeltaName.Trim();
        if (!ConfirmDeltaOverwrite(name))
            return;

        Engine.ApplyConflictResolutions(LastRegulationAnalysis.MergeResult);
        Patcher.WriteDeltaPatch(LastRegulationAnalysis.MergeResult.Patch, name);
        Patcher.Selection.RefreshImportList();
        RegulationBuildStatus = LOC.Get("PARAM_DirectMerge_Saved_Merged_Changes", name);
    }

    public void DisplayFullModMerge()
    {
        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ProjectMerge_Options_Header"),
            LOC.Get("PARAM_ProjectMerge_Options_Header_TT"));

        // Merge Binder Containers by Internal Entry
        if (ImGui.Checkbox($"{LOC.Get("PARAM_ProjectMerge_Merge_By_Internal_Entry")}##autoMergeFullBinder", ref FullModEnableBinderMerge))
        {
            LastFullModAnalysis = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Merge_By_Internal_Entry_TT"));

        // Auto-upgrade Regulation Versions
        var autoUpgradeRegulation = RegulationMerge.AutoUpgradeMismatchedVersions;
        if (ImGui.Checkbox($"{LOC.Get("PARAM_ProjectMerge_AutoUpgrade_Regulation")}##autoMergeFullUpgradeReg", ref autoUpgradeRegulation))
        {
            RegulationMerge.AutoUpgradeMismatchedVersions = autoUpgradeRegulation;
            LastFullModAnalysis = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_AutoUpgrade_Regulation_TT"));

        // Ignore Metadata Files
        if (ImGui.Checkbox($"{LOC.Get("PARAM_ProjectMerge_Merge_Metadata")}##autoMergeFullMetadata", ref FullModIgnoreMetadata))
        {
            LastFullModAnalysis = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Merge_Metadata_TT"));

        // Write Merge Report
        ImGui.Checkbox($"{LOC.Get("PARAM_ProjectMerge_Write_Merge_Report")}##autoMergeFullReport", ref FullModWriteReport);
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Write_Merge_Report_TT"));

        // Sources
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ProjectMerge_Sources_Header"),
            LOC.Get("PARAM_ProjectMerge_Sources_Header_TT"));


        // Add
        if (ImGui.Button($"{Icons.Plus}##addEntryAction", DPI.IconButtonSize))
        {
            FullModFolderPaths.Add("");
            LastFullModAnalysis = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Add_Project_Source_TT"));

        ImGui.SameLine();

        // Remove
        if (FullModFolderPaths.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##removeEntryAction", DPI.IconButtonSize))
            {
            }
            GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Remove_Project_Source_TT"));

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##mapSelectionRemove", DPI.IconButtonSize))
            {
                FullModFolderPaths.RemoveAt(FullModFolderPaths.Count - 1);
                LastFullModAnalysis = null;
            }
            GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Remove_Project_Source_TT"));
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button($"{LOC.Get("PARAM_ProjectMerge_Reset_Source_List")}##resetEntryList", DPI.SelectorButtonSize))
        {
            for (var i = 0; i < FullModFolderPaths.Count; i++)
            {
                FullModFolderPaths[i] = "";
            }

            LastFullModAnalysis = null;
            FullModOutputPath = "";
            FullModBuildStatus = "";
        }
        GUI.Tooltip(LOC.Get("PARAM_ProjectMerge_Reset_Source_List_TT"));

        // Sources
        for (var i = 0; i < FullModFolderPaths.Count; i++)
        {
            // Select
            if (ImGui.Button($"{LOC.Get("PARAM_ProjectMerge_Select_Path")}##selectPath{i}", DPI.SelectorButtonSize))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog(LOC.Get("PARAM_ProjectMerge_Select_Project_Folder"), out var path);

                if (dialog)
                {
                    FullModFolderPaths[i] = path;
                    LastFullModAnalysis = null;
                    FullModBuildStatus = "";
                }
            }

            ImGui.SameLine();

            var value = FullModFolderPaths[i];
            if (ImGui.InputText($"{LOC.Get("PARAM_ProjectMerge_Source", i + 1)}##autoMergeProjectSource{i}", ref value, 1024))
            {
                FullModFolderPaths[i] = value;
                LastFullModAnalysis = null;
                FullModBuildStatus = "";
            }
        }

        GUI.MultiButtonInput("analyzeActions",
            "analyze",
            LOC.Get("PARAM_ProjectMerge_Analyze_Mod_Folders"),
            LOC.Get("PARAM_ProjectMerge_Analyze_Mod_Folders_TT"),
            AnalyzeFullModFolders);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ProjectMerge_Output_Header"),
            LOC.Get("PARAM_ProjectMerge_Output_Header_TT"));

        // Select
        if (ImGui.Button($"{LOC.Get("PARAM_ProjectMerge_Select_Path")}##selectPath_output", DPI.SelectorButtonSize))
        {
            var dialog = PlatformUtils.Instance.OpenFolderDialog(LOC.Get("PARAM_ProjectMerge_Select_Project_Folder"), out var path);

            if (dialog)
            {
                FullModOutputPath = path;
            }
        }

        ImGui.SameLine();

        ImGui.InputText($"{LOC.Get("PARAM_ProjectMerge_Output_Folder")}##autoMergeFullOutput", ref FullModOutputPath, 1024);

        if (LastFullModAnalysis == null)
            return;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ProjectMerge_Actions_Header"),
            LOC.Get("PARAM_ProjectMerge_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("buildActions",
            "build",
            LOC.Get("PARAM_ProjectMerge_Build_Merge"),
            LOC.Get("PARAM_ProjectMerge_Build_Merge_TT"),
            BuildFullModFolder,
            LastFullModAnalysis.CanBuild);

        if (!string.IsNullOrWhiteSpace(FullModBuildStatus))
        {
            GUI.Spacer();
            GUI.WrappedText(FullModBuildStatus);
        }

        // Summary
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("PARAM_ProjectMerge_Summary_Header"),
            LOC.Get("PARAM_ProjectMerge_Summary_Header_TT"),
            ref DisplayMergeReportSummary);

        if (DisplayMergeReportSummary)
        {
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Source_Folders", LastFullModAnalysis.SourceFolders.Count)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Files_Scanned", LastFullModAnalysis.ScannedFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Unique_Files_to_Copy", LastFullModAnalysis.UniqueFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Identical_Duplicate_Files", LastFullModAnalysis.IdenticalFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Binder_Files_to_Merge", LastFullModAnalysis.BinderFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Gparam_Files_to_Merge", LastFullModAnalysis.GparamFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Regulation_Merges", LastFullModAnalysis.RegulationFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Ignored_Metadata_Files", LastFullModAnalysis.IgnoredFiles)}");
            ImGui.Text($"{LOC.Get("PARAM_ProjectMerge_Conflicts", LastFullModAnalysis.Conflicts.Count)}");
        }

        if (LastFullModAnalysis.Files.Any(e => e.Action == FullModMergeAction.BinderMerge))
        {
            GUI.Spacer();
            GUI.ConditionalHeader(
                LOC.Get("PARAM_ProjectMerge_Binder_Merge_Summary_Header"),
                LOC.Get("PARAM_ProjectMerge_Binder_Merge_Summary_Header_TT"),
                ref DisplayBinderMergeSummary);

            if(DisplayBinderMergeSummary)
            {
                foreach (var plan in LastFullModAnalysis.Files.Where(e => e.Action == FullModMergeAction.BinderMerge).Take(100))
                {
                    var summary = plan.BinderSummary;
                    if (summary == null)
                        continue;

                    GUI.WrappedText(
                        LOC.Get("PARAM_ProjectMerge_Binder_Merge_Log",
                        plan.RelativePath,
                        summary.AddedEntries,
                        summary.IdenticalEntries,
                        summary.NestedBinderMerges,
                        summary.MatbinSemanticMerges,
                        summary.Conflicts));

                    if (summary.GparamSemanticMerges > 0)
                    {
                        GUI.WrappedText(LOC.Get("PARAM_ProjectMerge_Binder_Gparam_Merges",
                            plan.RelativePath, summary.GparamSemanticMerges));
                    }
                }
            }
        }

        // Warnings
        if (LastFullModAnalysis.Warnings.Count > 0)
        {
            GUI.Spacer();
            GUI.ConditionalHeader(
                LOC.Get("PARAM_ProjectMerge_Warnings_Header"),
                LOC.Get("PARAM_ProjectMerge_Warnings_Header_TT", LastFullModAnalysis.Warnings.Count),
                ref DisplayWarnings);

            if (DisplayWarnings)
            {
                ImGui.BeginChild("warningsSection", new Vector2(0, 100));

                foreach (var warning in LastFullModAnalysis.Warnings.Take(MaxListedWarnings))
                {
                    GUI.WrappedText($"• {warning}");
                }

                if (LastFullModAnalysis.Warnings.Count > MaxListedWarnings)
                {
                    ImGui.TextDisabled(LOC.Get("PARAM_AutoMerge_More_Warnings",
                        LastFullModAnalysis.Warnings.Count - MaxListedWarnings));
                }

                ImGui.EndChild();
            }
        }

        if (LastFullModAnalysis.Errors.Count > 0)
        {
            GUI.Spacer();
            GUI.ConditionalHeader(
                LOC.Get("PARAM_ProjectMerge_Errors_Header"),
                LOC.Get("PARAM_ProjectMerge_Errors_Header_TT", LastFullModAnalysis.Errors.Count),
                ref DisplayErrors);

            if (DisplayErrors)
            {
                foreach (var error in LastFullModAnalysis.Errors)
                {
                    GUI.WrappedText($"• {error}");
                }
            }

            return;
        }

        if (LastFullModAnalysis.Conflicts.Any(e => e.Type != FullModMergeConflictType.Regulation))
        {
            GUI.Spacer();
            DisplayFullModConflictResolver(LastFullModAnalysis);
        }

        if (LastFullModAnalysis.RegulationAnalysis?.MergeResult?.Conflicts.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_ProjectMerge_RegulationConflicts_Header"),
                LOC.Get("PARAM_ProjectMerge_RegulationConflicts_Header_TT"));

            DisplayMergeSummary(LastFullModAnalysis.RegulationAnalysis.MergeResult, "fullreg");
        }
    }

    private void DisplayFullModConflictResolver(FullModMergeAnalysis analysis)
    {
        var editableConflicts = analysis.Conflicts.Where(e => e.Type != FullModMergeConflictType.Regulation).ToList();
        var unresolved = editableConflicts.Count(e => !e.IsResolved);

        GUI.ConditionalHeader(
            LOC.Get("PARAM_ProjectMerge_FileConflictResolve_Header", editableConflicts.Count),
            LOC.Get("PARAM_ProjectMerge_FileConflictResolve_Header_TT"),
            ref DisplayConflictResolver);

        if (DisplayConflictResolver)
        {
            ImGui.Text(LOC.Get("PARAM_ProjectMerge_Resolved_Unresolved_Hint", editableConflicts.Count - unresolved, unresolved));

            ImGui.InputText(
                $"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Filter")}##autoMergeFullConflictFilter",
                ref FullModConflictFilter, 512);

            // Visible -> Earlier
            if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Earlier")}##autoMergeFullResolveEarlier"))
            {
                foreach (var conflict in FilterFullModConflicts(analysis))
                {
                    conflict.Resolution = FullModConflictResolution.UseEarlier;
                }
            }

            ImGui.SameLine();

            // Visible -> Later
            if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Later")}##autoMergeFullResolveLater"))
            {
                foreach (var conflict in FilterFullModConflicts(analysis))
                {
                    conflict.Resolution = FullModConflictResolution.UseLater;
                }
            }

            ImGui.SameLine();

            // Reset Visible
            if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Reset")}##autoMergeFullResolveReset"))
            {
                foreach (var conflict in FilterFullModConflicts(analysis))
                {
                    conflict.Resolution = FullModConflictResolution.Unresolved;
                }
            }

            var filtered = FilterFullModConflicts(analysis).ToList();
            var pageCount = Math.Max(1, (filtered.Count + ConflictRowsPerPage - 1) / ConflictRowsPerPage);
            FullModConflictPage = Math.Clamp(FullModConflictPage, 0, pageCount - 1);

            if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Page_Back")}##autoMergeFullConflictPrev") && FullModConflictPage > 0)
            {
                FullModConflictPage--;
            }

            ImGui.SameLine();

            ImGui.Text($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Page", FullModConflictPage + 1, pageCount, filtered.Count)}");

            ImGui.SameLine();

            if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Page_Next")}##autoMergeFullConflictNext") && FullModConflictPage + 1 < pageCount)
            {
                FullModConflictPage++;
            }

            var page = filtered.Skip(FullModConflictPage * ConflictRowsPerPage).Take(ConflictRowsPerPage);

            if (ImGui.BeginTable(
                    "autoMergeFullConflictTable",
                    6,
                    ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV |
                    ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchSame))
            {
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Path"));
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Earlier"));
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Later"));
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Resolution"));
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Manual_Value"));
                ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictEditor_Reason"));
                ImGui.TableHeadersRow();

                var index = FullModConflictPage * ConflictRowsPerPage;
                foreach (var conflict in page)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    GUI.WrappedText(string.IsNullOrWhiteSpace(conflict.InternalPath)
                        ? conflict.RelativePath
                        : $"{conflict.RelativePath} :: {conflict.InternalPath}");

                    ImGui.TableSetColumnIndex(1);
                    GUI.WrappedText(string.IsNullOrWhiteSpace(conflict.ExistingValue)
                        ? conflict.ExistingSource
                        : $"{conflict.ExistingValue}\n{conflict.ExistingSource}");

                    ImGui.TableSetColumnIndex(2);
                    GUI.WrappedText(string.IsNullOrWhiteSpace(conflict.IncomingValue)
                        ? conflict.IncomingSource
                        : $"{conflict.IncomingValue}\n{conflict.IncomingSource}");

                    ImGui.TableSetColumnIndex(3);
                    var label = GetFullModResolutionName(conflict.Resolution);
                    if (ImGui.BeginCombo($"##fullConflictResolution{index}", label))
                    {
                        foreach (FullModConflictResolution resolution in Enum.GetValues(typeof(FullModConflictResolution)))
                        {
                            if (resolution == FullModConflictResolution.Manual && !conflict.SupportsManual)
                                continue;

                            if (ImGui.Selectable(GetFullModResolutionName(resolution), conflict.Resolution == resolution))
                            {
                                conflict.Resolution = resolution;
                                if (resolution == FullModConflictResolution.Manual && string.IsNullOrEmpty(conflict.ManualValue))
                                    conflict.ManualValue = conflict.ExistingValue;
                            }
                        }
                        ImGui.EndCombo();
                    }

                    ImGui.TableSetColumnIndex(4);
                    if (conflict.SupportsManual && conflict.Resolution == FullModConflictResolution.Manual)
                    {
                        var manual = conflict.ManualValue ?? "";
                        if (ImGui.InputText($"##fullConflictManual{index}", ref manual, 1024))
                        {
                            conflict.ManualValue = manual;
                        }
                    }
                    else
                    {
                        ImGui.TextDisabled("-");
                    }

                    ImGui.TableSetColumnIndex(5);
                    GUI.WrappedText(conflict.Message);
                    index++;
                }

                ImGui.EndTable();
            }
        }
    }

    private IEnumerable<FullModMergeConflict> FilterFullModConflicts(FullModMergeAnalysis analysis)
    {
        var source = analysis.Conflicts.Where(e => e.Type != FullModMergeConflictType.Regulation);
        if (string.IsNullOrWhiteSpace(FullModConflictFilter))
            return source;

        var filter = FullModConflictFilter.Trim();
        return source.Where(e =>
            e.RelativePath.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            e.InternalPath.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            e.ExistingSource.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            e.IncomingSource.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            e.Message.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetFullModResolutionName(FullModConflictResolution resolution)
    {
        return resolution switch
        {
            FullModConflictResolution.UseEarlier => LOC.Get("PARAM_AutoMerge_ConflictResolution_UseEarlier"),
            FullModConflictResolution.UseLater => LOC.Get("PARAM_AutoMerge_ConflictResolution_UseLater"),
            FullModConflictResolution.Manual => LOC.Get("PARAM_AutoMerge_ConflictResolution_Manual"),
            _ => LOC.Get("PARAM_AutoMerge_ConflictResolution_Unresolved")
        };
    }

    private void AnalyzeFullModFolders()
    {
        FullModBuildStatus = "";
        FullModConflictPage = 0;
        LastFullModAnalysis = FullModMerge.Analyze(
            FullModFolderPaths,
            Strategy,
            FullModEnableBinderMerge,
            FullModIgnoreMetadata);

        if (LastFullModAnalysis.SourceFolders.Count > 0 && string.IsNullOrWhiteSpace(FullModOutputPath))
        {
            var first = LastFullModAnalysis.SourceFolders[0];
            var parent = System.IO.Directory.GetParent(first)?.FullName ?? first;
            FullModOutputPath = System.IO.Path.Combine(parent, "Merged_Mod");
        }
    }

    private void BuildFullModFolder()
    {
        try
        {
            FullModMerge.Build(LastFullModAnalysis, FullModOutputPath, FullModWriteReport);
            var cleanOutputPath = FullModOutputPath.Trim().Trim('"');
            FullModBuildStatus = LOC.Get("PARAM_ProjectMerge_Merged_Mod", Path.GetFullPath(cleanOutputPath));
        }
        catch (Exception ex)
        {
            FullModBuildStatus = LOC.Get("PARAM_ProjectMerge_Merged_Mod_Failed", ex.Message);
        }
    }

    /// <summary>
    /// Runs the analysis on a worker thread behind the progress modal (which also stops param edits while the
    /// project's params are read). <see cref="PollRegulationAnalysis"/> picks up the result.
    /// </summary>
    private void AnalyzeRegulations()
    {
        if (RegulationAnalysisTask != null)
            return;

        ClearRegulationAnalysis();
        SaveDirectMergeSettings();

        // Project entries are looked up here, on the UI thread; a project that's gone is an error in the analysis
        var projects = GetMergeableProjects();
        var missingProjects = new List<string>();
        var specs = new List<MergeSourceSpec>();

        foreach (var entry in MergeSources.Where(e => !e.IsEmpty))
        {
            if (!entry.IsProject)
            {
                specs.Add(MergeSourceSpec.File(entry.Path));
                continue;
            }

            var project = projects.FirstOrDefault(p => p.Descriptor.ProjectName == entry.ProjectName);
            if (project == null)
                missingProjects.Add(entry.ProjectName);
            else
                specs.Add(MergeSourceSpec.FromProject(project));
        }

        if (missingProjects.Count > 0)
        {
            var failed = new RegulationMergeAnalysis();
            foreach (var name in missingProjects)
                failed.Errors.Add(LOC.Get("PARAM_DirectMerge_Error_Project_Missing", name));

            LastRegulationAnalysis = failed;
            return;
        }
        var strategy = Strategy;
        var includeProject = RegulationMerge.IncludeProject;
        var modal = Patcher.MergeProgressModal;

        RegulationAnalysisIncludesProject = includeProject;
        RegulationAnalysisActionVersion = Patcher.View.Editor.ActionManager.Version;
        GetConflictView("reg").Page = 0;

        modal.SetProgress(new DeltaBuildProgress { PhaseLabel = LOC.Get("PARAM_DirectMerge_Progress_Header") });
        modal.InitialLayout = false;
        modal.DisplayModal = true;

        RegulationAnalysisTask = Task.Run(() => RegulationMerge.Analyze(specs, strategy, includeProject,
            (step, percent) => modal.ReportProgress?.Invoke(new DeltaBuildProgress
            {
                PhaseLabel = LOC.Get("PARAM_DirectMerge_Progress_Header"),
                StepLabel = step,
                Percent = percent
            })));
    }

    private void BuildRegulation()
    {
        if (!IsDirectMergeReady() || !RegulationMerge.CanUseRegulationFiles)
            return;

        SaveDirectMergeSettings();

        try
        {
            RegulationMerge.BuildMergedRegulation(LastRegulationAnalysis, RegulationOutputPath);
            var cleanOutputPath = RegulationOutputPath.Trim().Trim('"');
            RegulationBuildStatus = LOC.Get("PARAM_DirectMerge_Merged_Regulation", Path.GetFullPath(cleanOutputPath));
        }
        catch (Exception ex)
        {
            RegulationBuildStatus = LOC.Get("PARAM_ProjectMerge_Merged_Mod_Failed", ex.Message);
        }
    }

    private void AnalyzeDeltaPatches()
    {
        var sources = Patcher.Selection.ImportList
            .Where(e => SelectedFiles.Contains(e.Filename))
            .ToList();

        LastResult = Engine.Merge(sources, Strategy);
    }

    #region Conflict editor
    /// <summary>
    /// Filter, page and toggles of one conflict editor (delta, reg, fullreg)
    /// </summary>
    private sealed class ConflictViewState
    {
        public string Filter = "";
        public int Page;

        // 0 = all, 1 = fields, 2 = rows, 3 = groups
        public int TypeFilter;
        public bool UnresolvedOnly;
    }

    private readonly Dictionary<string, ConflictViewState> ConflictViews = new();

    private ConflictViewState GetConflictView(string idSuffix)
    {
        if (!ConflictViews.TryGetValue(idSuffix, out var view))
            ConflictViews[idSuffix] = view = new ConflictViewState();

        return view;
    }

    private static bool MatchesConflictView(ParamDeltaMergeConflict conflict, ConflictViewState view)
    {
        if (view.UnresolvedOnly && conflict.IsResolved)
            return false;

        var typeMatches = view.TypeFilter switch
        {
            1 => conflict.Type == ParamDeltaMergeConflictType.FieldValue,
            2 => conflict.Type == ParamDeltaMergeConflictType.RowState,
            3 => conflict.Type == ParamDeltaMergeConflictType.RowGroup,
            _ => true
        };

        if (!typeMatches)
            return false;

        if (string.IsNullOrWhiteSpace(view.Filter))
            return true;

        var filter = view.Filter.Trim();

        return conflict.ParamName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            conflict.Field.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            conflict.FieldDisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            conflict.RowName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            conflict.RowID.ToString().Contains(filter, StringComparison.Ordinal) ||
            conflict.Candidates.Any(c => c.Source.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetConflictTypeFilterName(int typeFilter)
    {
        return typeFilter switch
        {
            1 => LOC.Get("PARAM_AutoMerge_ConflictEditor_Field"),
            2 => LOC.Get("PARAM_AutoMerge_ConflictEditor_RowState"),
            3 => LOC.Get("PARAM_AutoMerge_ConflictEditor_RowGroup"),
            _ => LOC.Get("PARAM_AutoMerge_ConflictEditor_Type_All")
        };
    }

    private void DisplayMergeSummary(ParamDeltaAutoMergeResult result, string idSuffix)
    {
        GUI.SimpleHeader(
            LOC.Get("PARAM_AutoMerge_Merge_Summary_Header"),
            LOC.Get("PARAM_AutoMerge_Merge_Summary_Header_TT"));

        ImGui.Text(LOC.Get("PARAM_AutoMerge_Summary_Sources", result.SourceCount));
        ImGui.Text(LOC.Get("PARAM_AutoMerge_Summary_Rows_Merged", result.Patch.Params.Sum(e => e.Rows.Count)));

        if (result.Patch.Params.Any(e => e.Groups.Count > 0) || result.IdenticalGroups > 0)
        {
            ImGui.Text(LOC.Get("PARAM_AutoMerge_Summary_Groups_Merged",
                result.Patch.Params.Sum(e => e.Groups.Count), result.IdenticalGroups));
        }
        ImGui.Text(LOC.Get("PARAM_AutoMerge_Summary_Safe_Field_Changes", result.SafeFields));
        ImGui.Text(LOC.Get("PARAM_AutoMerge_Summary_Identical_Fields", result.IdenticalFields));
        ImGui.Text(LOC.Get("PARAM_AutoMerge_Summary_Conflicts_Resolved_Unresolved", result.Conflicts.Count, result.ResolvedConflicts, result.UnresolvedConflicts));

        if (result.Errors.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_AutoMerge_Merge_Errors_Header"),
                LOC.Get("PARAM_AutoMerge_Merge_Errors_Header_TT"));

            foreach (var error in result.Errors)
            {
                GUI.WrappedText($"• {error}");
            }
        }

        if (result.Conflicts.Count > 0)
        {
            GUI.Spacer();

            if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Header", result.Conflicts.Count)}##autoMergeConflicts_{idSuffix}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                DisplayConflictEditor(result, idSuffix);
            }
        }

        // Resolutions are applied to the patch when one changes and before build, import and save, not every frame

        if (!result.CanApply)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("PARAM_AutoMerge_ConflictEditor_Cannot_Apply_Hint"));
        }
    }

    private void DisplayConflictEditor(ParamDeltaAutoMergeResult result, string idSuffix)
    {
        var view = GetConflictView(idSuffix);

        // Filter: param, row ID or name, field, source
        ImGui.InputTextWithHint($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Filter")}##paramConflictFilter_{idSuffix}",
            LOC.Get("PARAM_AutoMerge_ConflictEditor_Filter_Hint"), ref view.Filter, 512);
        GUI.Tooltip(LOC.Get("PARAM_AutoMerge_ConflictEditor_Filter_TT"));

        // Type
        ImGui.SetNextItemWidth(160f * DPI.UIScale());
        if (ImGui.BeginCombo($"{LOC.Get("PARAM_AutoMerge_ConflictTable_Type")}##paramConflictType_{idSuffix}",
                GetConflictTypeFilterName(view.TypeFilter)))
        {
            for (var i = 0; i < 4; i++)
            {
                if (ImGui.Selectable(GetConflictTypeFilterName(i), view.TypeFilter == i))
                    view.TypeFilter = i;
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Unresolved_Only")}##paramConflictUnresolved_{idSuffix}",
            ref view.UnresolvedOnly);

        var filtered = result.Conflicts.Where(c => MatchesConflictView(c, view)).ToList();

        if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Earlier")}##paramResolveEarlier_{idSuffix}"))
        {
            foreach (var conflict in filtered)
                conflict.Resolution = ParamDeltaConflictResolution.UseEarlier;

            Engine.ApplyConflictResolutions(result);
        }
        GUI.Tooltip(LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Earlier_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Later")}##paramResolveLater_{idSuffix}"))
        {
            foreach (var conflict in filtered)
                conflict.Resolution = ParamDeltaConflictResolution.UseLater;

            Engine.ApplyConflictResolutions(result);
        }
        GUI.Tooltip(LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Later_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Visible_Reset")}##paramResolveReset_{idSuffix}"))
        {
            foreach (var conflict in filtered)
                conflict.Resolution = ParamDeltaConflictResolution.Unresolved;
        }

        var pageCount = Math.Max(1, (filtered.Count + ConflictRowsPerPage - 1) / ConflictRowsPerPage);
        view.Page = Math.Clamp(view.Page, 0, pageCount - 1);

        if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Page_Back")}##paramConflictPrev_{idSuffix}") && view.Page > 0)
            view.Page--;

        ImGui.SameLine();
        ImGui.Text(LOC.Get("PARAM_AutoMerge_ConflictEditor_Page", view.Page + 1, pageCount, filtered.Count));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("PARAM_AutoMerge_ConflictEditor_Page_Next")}##paramConflictNext_{idSuffix}") && view.Page + 1 < pageCount)
            view.Page++;

        var page = filtered.Skip(view.Page * ConflictRowsPerPage).Take(ConflictRowsPerPage);

        if (!ImGui.BeginTable(
                $"paramConflictTable_{idSuffix}",
                6,
                ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV |
                ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
            return;

        ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_ParamRowField"), ImGuiTableColumnFlags.None, 1.2f);
        ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Changes"), ImGuiTableColumnFlags.None, 1.6f);
        ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Vanilla"), ImGuiTableColumnFlags.None, 0.6f);
        ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Resolution"), ImGuiTableColumnFlags.None, 1.0f);
        ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Manual_Value"), ImGuiTableColumnFlags.None, 0.8f);
        ImGui.TableSetupColumn(LOC.Get("PARAM_AutoMerge_ConflictTable_Type"), ImGuiTableColumnFlags.None, 0.5f);
        ImGui.TableHeadersRow();

        var index = view.Page * ConflictRowsPerPage;

        foreach (var conflict in page)
        {
            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            DisplayConflictLocation(conflict);

            ImGui.TableSetColumnIndex(1);
            DisplayConflictCandidates(conflict);

            ImGui.TableSetColumnIndex(2);
            if (conflict.Type == ParamDeltaMergeConflictType.FieldValue && conflict.VanillaValue != null)
                GUI.WrappedText(conflict.VanillaValue);
            else if (conflict.Type == ParamDeltaMergeConflictType.FieldValue)
                ImGui.TextDisabled(LOC.Get("PARAM_AutoMerge_ConflictEditor_Not_In_Vanilla"));
            else
                ImGui.TextDisabled("-");

            ImGui.TableSetColumnIndex(3);
            DisplayConflictResolution(result, conflict, $"{idSuffix}_{index}");

            ImGui.TableSetColumnIndex(4);
            DisplayConflictManualValue(result, conflict, $"{idSuffix}_{index}");

            ImGui.TableSetColumnIndex(5);
            ImGui.Text(conflict.Type switch
            {
                ParamDeltaMergeConflictType.FieldValue => LOC.Get("PARAM_AutoMerge_ConflictEditor_Field"),
                ParamDeltaMergeConflictType.RowGroup => LOC.Get("PARAM_AutoMerge_ConflictEditor_RowGroup"),
                _ => LOC.Get("PARAM_AutoMerge_ConflictEditor_RowState")
            });

            index++;
        }

        ImGui.EndTable();
    }

    private static void DisplayConflictLocation(ParamDeltaMergeConflict conflict)
    {
        var rowIndex = conflict.RowIndex > 0 ? $":{conflict.RowIndex}" : "";
        var rowName = string.IsNullOrWhiteSpace(conflict.RowName) ? "" : $" {conflict.RowName}";

        var location = conflict.Type == ParamDeltaMergeConflictType.RowGroup
            ? LOC.Get("PARAM_AutoMerge_ConflictEditor_Group_Path", conflict.ParamName, conflict.RowID) + rowName
            : $"{conflict.ParamName} / {conflict.RowID}{rowIndex}{rowName}";

        GUI.WrappedText(location);

        if (conflict.Type != ParamDeltaMergeConflictType.FieldValue)
            return;

        GUI.WrappedText(conflict.FieldDisplayName);

        if (conflict.FieldDisplayName != conflict.Field)
            GUI.Tooltip(conflict.Field);
    }

    /// <summary>
    /// Every distinct change with the sources that made it; the selected one is highlighted
    /// </summary>
    private static void DisplayConflictCandidates(ParamDeltaMergeConflict conflict)
    {
        var selectedIndex = conflict.Resolution == ParamDeltaConflictResolution.Manual ? null : conflict.SelectedIndex;

        for (var i = 0; i < conflict.Candidates.Count; i++)
        {
            var candidate = conflict.Candidates[i];
            var text = $"{candidate.Source}: {candidate.Value}";

            if (selectedIndex == i)
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, text);
            else
                GUI.WrappedText(text);

            if (conflict.Type == ParamDeltaMergeConflictType.RowGroup)
            {
                // Compared with the first other change
                var other = conflict.Candidates.FirstOrDefault(c => c != candidate);
                DisplayGroupSnapshotTooltip(candidate.GroupSnapshot, other?.GroupSnapshot);
            }
        }
    }

    private void DisplayConflictResolution(ParamDeltaAutoMergeResult result, ParamDeltaMergeConflict conflict, string id)
    {
        if (conflict.IsSuperseded)
        {
            ImGui.TextDisabled(LOC.Get("PARAM_AutoMerge_ConflictEditor_Superseded"));
            return;
        }

        ImGui.SetNextItemWidth(-1);
        if (!ImGui.BeginCombo($"##paramResolution_{id}", GetResolutionLabel(conflict)))
            return;

        var changed = false;

        if (ImGui.Selectable(LOC.Get("PARAM_AutoMerge_ConflictResolution_Unresolved"),
                conflict.Resolution == ParamDeltaConflictResolution.Unresolved))
        {
            conflict.Resolution = ParamDeltaConflictResolution.Unresolved;
        }

        var selectedIndex = conflict.Resolution == ParamDeltaConflictResolution.Manual ? null : conflict.SelectedIndex;

        for (var i = 0; i < conflict.Candidates.Count; i++)
        {
            if (ImGui.Selectable($"{LOC.Get("PARAM_AutoMerge_ConflictResolution_UseSource", conflict.Candidates[i].Source)}##candidate{i}",
                    selectedIndex == i))
            {
                conflict.Resolution = ParamDeltaConflictResolution.UseSource;
                conflict.SelectedCandidate = i;
                changed = true;
            }
        }

        if (conflict.Type == ParamDeltaMergeConflictType.FieldValue &&
            ImGui.Selectable(LOC.Get("PARAM_AutoMerge_ConflictResolution_Manual"),
                conflict.Resolution == ParamDeltaConflictResolution.Manual))
        {
            if (string.IsNullOrEmpty(conflict.ManualValue))
                conflict.ManualValue = (conflict.Selected ?? conflict.Candidates[0]).Value;

            conflict.Resolution = ParamDeltaConflictResolution.Manual;
            conflict.ManualValid = ParamRowGroups.IsValidFieldValue(conflict.FieldDef, conflict.ManualValue);
            changed = true;
        }

        ImGui.EndCombo();

        if (changed)
            Engine.ApplyConflictResolutions(result);
    }

    private void DisplayConflictManualValue(ParamDeltaAutoMergeResult result, ParamDeltaMergeConflict conflict, string id)
    {
        if (conflict.Type != ParamDeltaMergeConflictType.FieldValue ||
            conflict.Resolution != ParamDeltaConflictResolution.Manual || conflict.IsSuperseded)
        {
            ImGui.TextDisabled("-");
            return;
        }

        var manual = conflict.ManualValue ?? "";

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputText($"##manualConflict_{id}", ref manual, 1024))
        {
            conflict.ManualValue = manual;
            conflict.ManualValid = ParamRowGroups.IsValidFieldValue(conflict.FieldDef, manual);

            if (conflict.ManualValid)
                Engine.ApplyConflictResolutions(result);
        }

        if (!conflict.ManualValid)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color,
                LOC.Get("PARAM_AutoMerge_ConflictEditor_Invalid_Value", conflict.FieldDef?.DisplayType.ToString() ?? ""));
        }
    }

    private static string GetResolutionLabel(ParamDeltaMergeConflict conflict)
    {
        if (conflict.Resolution == ParamDeltaConflictResolution.Manual)
            return LOC.Get("PARAM_AutoMerge_ConflictResolution_Manual");

        var selected = conflict.Selected;

        return selected == null
            ? LOC.Get("PARAM_AutoMerge_ConflictResolution_Unresolved")
            : LOC.Get("PARAM_AutoMerge_ConflictResolution_UseSource", selected.Source);
    }

    /// <summary>
    /// Hovering a group change lists its rows, with the rows that the other change doesn't have at the same
    /// position marked
    /// </summary>
    private static void DisplayGroupSnapshotTooltip(RowGroupDelta group, RowGroupDelta other)
    {
        if (group == null || !ImGui.IsItemHovered())
            return;

        ImGui.BeginTooltip();

        if (group.State == RowDeltaState.Deleted)
        {
            ImGui.Text(LOC.Get("PARAM_AutoMerge_Group_Description_Deleted"));
        }
        else
        {
            const int maxRows = 40;

            for (var i = 0; i < group.Rows.Count && i < maxRows; i++)
            {
                var row = group.Rows[i];
                var same = other != null && i < other.Rows.Count && ParamRowGroups.RowFieldsEqual(row, other.Rows[i]);
                var label = LOC.Get("PARAM_DeltaPatcher_Preview_Group_Row_Display_Name", i, row.Name);

                if (same)
                    ImGui.TextDisabled(label);
                else
                    ImGui.Text($"* {label}");
            }

            if (group.Rows.Count > maxRows)
                ImGui.TextDisabled(LOC.Get("PARAM_AutoMerge_Group_More_Rows", group.Rows.Count - maxRows));
        }

        ImGui.EndTooltip();
    }
    #endregion

    private static string GetStrategyName(ParamDeltaConflictStrategy strategy)
    {
        return strategy switch
        {
            ParamDeltaConflictStrategy.StopOnConflict => LOC.Get("PARAM_AutoMerge_ConflictStrategy_StopOnConflict"),
            ParamDeltaConflictStrategy.PreferFirst => LOC.Get("PARAM_AutoMerge_ConflictStrategy_PreferEarlierSource"),
            ParamDeltaConflictStrategy.PreferLast => LOC.Get("PARAM_AutoMerge_ConflictStrategy_PreferLaterSource"),
            _ => strategy.ToString()
        };
    }
}
