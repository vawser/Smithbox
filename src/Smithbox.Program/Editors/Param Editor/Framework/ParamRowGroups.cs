using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Shared rules for params whose rows can share an ID.
///
/// Row table params (listed in Table Params.json) are compared and merged as whole groups: a group is every row
/// with one ID, in param order, and two groups are equal when they have the same rows (by data) in the same order.
///
/// Other params keep the older (ID, Index) position key, where Index is the row's position among the rows with
/// its ID. That key is computed here only, so every tool counts it the same way.
/// </summary>
public static class ParamRowGroups
{
    #region Table params
    public static bool IsTableParam(ProjectEntry project, string paramName)
    {
        var list = project?.Handler?.ParamData?.TableParamList?.Params;

        return list != null && paramName != null && list.Contains(paramName);
    }
    #endregion

    #region Groups
    /// <summary>
    /// The rows of each ID, in param order
    /// </summary>
    public static Dictionary<int, List<Param.Row>> Build(Param param)
    {
        var result = new Dictionary<int, List<Param.Row>>();

        if (param == null)
            return result;

        foreach (var row in param.Rows)
        {
            if (!result.TryGetValue(row.ID, out var group))
            {
                group = new List<Param.Row>();
                result.Add(row.ID, group);
            }

            group.Add(row);
        }

        return result;
    }

    public static List<Param.Row> GetGroup(Param param, int id)
    {
        return param == null ? new List<Param.Row>() : param.Rows.Where(r => r.ID == id).ToList();
    }

    /// <summary>
    /// Same number of rows, with the same data in the same order. Names are ignored (regulation files hold none).
    /// </summary>
    public static bool GroupEquals(IReadOnlyList<Param.Row> a, IReadOnlyList<Param.Row> b)
    {
        if (a.Count != b.Count)
            return false;

        for (var i = 0; i < a.Count; i++)
        {
            if (!a[i].DataEquals(b[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// The groups of <paramref name="source"/> that differ from <paramref name="baseline"/>: Added (ID not in the
    /// baseline), Modified (different rows), Deleted (ID only in the baseline). Sorted by ID.
    /// </summary>
    public static List<RowGroupDelta> DiffGroups(Param source, Param baseline)
    {
        var result = new List<RowGroupDelta>();

        var sourceGroups = Build(source);
        var baseGroups = Build(baseline);

        foreach (var (id, rows) in sourceGroups)
        {
            if (!baseGroups.TryGetValue(id, out var baseRows))
            {
                result.Add(SerializeGroup(id, rows, RowDeltaState.Added));
            }
            else if (!GroupEquals(rows, baseRows))
            {
                result.Add(SerializeGroup(id, rows, RowDeltaState.Modified));
            }
        }

        foreach (var id in baseGroups.Keys)
        {
            if (!sourceGroups.ContainsKey(id))
            {
                result.Add(new RowGroupDelta { ID = id, State = RowDeltaState.Deleted });
            }
        }

        result.Sort((x, y) => x.ID.CompareTo(y.ID));

        return result;
    }

    /// <summary>
    /// Removes every row with <paramref name="id"/> and puts <paramref name="newRows"/> in their place: at the old
    /// group's first position, or in ID order for a new ID. An empty list deletes the group.
    /// Returns the index the group was inserted at, or -1 if nothing was inserted.
    /// </summary>
    public static int ReplaceGroup(Param param, int id, IReadOnlyList<Param.Row> newRows)
    {
        var insertIndex = -1;

        for (var i = param.Rows.Count - 1; i >= 0; i--)
        {
            if (param.Rows[i].ID == id)
            {
                param.RemoveRowAt(i);
                insertIndex = i;
            }
        }

        if (newRows.Count == 0)
            return -1;

        if (insertIndex < 0)
            insertIndex = GetSortedInsertIndex(param, id);

        for (var i = 0; i < newRows.Count; i++)
        {
            param.InsertRow(insertIndex + i, newRows[i]);
        }

        return insertIndex;
    }

    /// <summary>
    /// The first index whose row has a higher ID, so a new group goes in ID order
    /// </summary>
    public static int GetSortedInsertIndex(Param param, int id)
    {
        for (var i = 0; i < param.Rows.Count; i++)
        {
            if (param.Rows[i].ID > id)
                return i;
        }

        return param.Rows.Count;
    }
    #endregion

    #region Serialized groups
    public static string SerializeValue(Param.Column column, Param.Row row)
    {
        var value = column.GetValue(row);

        if (value is byte[] bytes)
            return ParamUtils.Dummy8Write(bytes);

        return value?.ToString() ?? "";
    }

    /// <summary>
    /// Every field of the row
    /// </summary>
    public static RowDelta SerializeRow(Param.Row row, int index, RowDeltaState state = RowDeltaState.Added)
    {
        var delta = new RowDelta
        {
            ID = row.ID,
            Index = index,
            Name = row.Name ?? "",
            State = state
        };

        foreach (var column in row.Columns)
        {
            delta.Fields.Add(new FieldDelta
            {
                Field = column.Def.InternalName,
                Value = SerializeValue(column, row)
            });
        }

        return delta;
    }

    public static RowGroupDelta SerializeGroup(int id, IReadOnlyList<Param.Row> rows, RowDeltaState state)
    {
        var group = new RowGroupDelta { ID = id, State = state };

        for (var i = 0; i < rows.Count; i++)
        {
            group.Rows.Add(SerializeRow(rows[i], i));
        }

        return group;
    }

    /// <summary>
    /// Same fields with the same values. Names are ignored.
    /// </summary>
    public static bool RowFieldsEqual(RowDelta a, RowDelta b)
    {
        if (a.Fields.Count != b.Fields.Count)
            return false;

        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var field in a.Fields)
            values[field.Field] = field.Value;

        foreach (var field in b.Fields)
        {
            if (!values.TryGetValue(field.Field, out var value) || !string.Equals(value, field.Value, StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Both delete the group, or both hold the same rows in the same order. Names are ignored.
    /// </summary>
    public static bool GroupContentEquals(RowGroupDelta a, RowGroupDelta b)
    {
        var aDeleted = a.State == RowDeltaState.Deleted || a.Rows.Count == 0;
        var bDeleted = b.State == RowDeltaState.Deleted || b.Rows.Count == 0;

        if (aDeleted || bDeleted)
            return aDeleted == bDeleted;

        if (a.Rows.Count != b.Rows.Count)
            return false;

        for (var i = 0; i < a.Rows.Count; i++)
        {
            if (!RowFieldsEqual(a.Rows[i], b.Rows[i]))
                return false;
        }

        return true;
    }

    public static RowGroupDelta CloneGroup(RowGroupDelta group)
    {
        var clone = new RowGroupDelta { ID = group.ID, State = group.State };

        foreach (var row in group.Rows)
            clone.Rows.Add(CloneRow(row));

        return clone;
    }

    public static RowDelta CloneRow(RowDelta row)
    {
        var clone = new RowDelta
        {
            ID = row.ID,
            Index = row.Index,
            Name = row.Name,
            State = row.State
        };

        foreach (var field in row.Fields)
            clone.Fields.Add(new FieldDelta { Field = field.Field, Value = field.Value });

        return clone;
    }

    /// <summary>
    /// The rows a group delta describes, created for <paramref name="target"/>.
    /// Each row starts as a copy of a template (the old row at the same position, else the first old row, else
    /// <paramref name="fallbackTemplate"/>), so fields the delta doesn't hold keep a sensible value, then the
    /// delta's fields are written. A row without a name keeps the old row's name at the same position.
    /// </summary>
    public static List<Param.Row> CreateGroupRows(Param target, RowGroupDelta group,
        IReadOnlyList<Param.Row> oldRows, Param.Row fallbackTemplate)
    {
        var result = new List<Param.Row>();

        if (group.State == RowDeltaState.Deleted)
            return result;

        for (var i = 0; i < group.Rows.Count; i++)
        {
            var rowDelta = group.Rows[i];
            var oldRow = i < oldRows.Count ? oldRows[i] : null;
            var template = oldRow ?? oldRows.FirstOrDefault() ?? fallbackTemplate;

            if (template == null)
                throw new InvalidOperationException(LOC.Get("PARAM_RowGroups_Error_No_Template", group.ID));

            var row = new Param.Row(template, target)
            {
                ID = group.ID,
                Name = !string.IsNullOrWhiteSpace(rowDelta.Name) ? rowDelta.Name : oldRow?.Name ?? ""
            };

            ApplyFields(row, rowDelta.Fields);
            result.Add(row);
        }

        return result;
    }

    /// <summary>
    /// Writes serialized field values to a row. Values that don't parse for the field's type are skipped.
    /// </summary>
    /// <summary>
    /// Whether <see cref="ApplyFields"/> can write this string into a field of this type (it skips values it can't
    /// parse). True for a missing def or a type it doesn't convert.
    /// </summary>
    public static bool IsValidFieldValue(PARAMDEF.Field def, string value)
    {
        if (def == null)
            return true;

        value ??= "";

        switch (def.DisplayType)
        {
            case PARAMDEF.DefType.s8: return sbyte.TryParse(value, out _);
            case PARAMDEF.DefType.s16: return short.TryParse(value, out _);
            case PARAMDEF.DefType.s32:
            case PARAMDEF.DefType.b32: return int.TryParse(value, out _);
            case PARAMDEF.DefType.f32:
            case PARAMDEF.DefType.angle32: return float.TryParse(value, out _);
            case PARAMDEF.DefType.f64: return double.TryParse(value, out _);
            case PARAMDEF.DefType.u16: return ushort.TryParse(value, out _);
            case PARAMDEF.DefType.u32: return uint.TryParse(value, out _);
            case PARAMDEF.DefType.u8:
            case PARAMDEF.DefType.dummy8:
                // Byte arrays are written as [a|b|c]; Dummy8Read returns null for anything else
                if (def.ArrayLength > 1)
                    return ParamUtils.Dummy8Read(value, def.ArrayLength) != null;

                return byte.TryParse(value, out _);
            default:
                return true;
        }
    }

    public static void ApplyFields(Param.Row curRow, IEnumerable<FieldDelta> fields)
    {
        foreach (var field in fields)
        {
            var curFieldDef = curRow.Def.Fields.FirstOrDefault(e => e.InternalName == field.Field);

            if (curFieldDef == null)
                continue;

            var curCol = curRow.Columns.FirstOrDefault(e => e.Def == curFieldDef);

            if (curCol == null)
                continue;

            // The string has to be cast to the field's type, since SetValue takes an object
            switch (curFieldDef.DisplayType)
            {
                case PARAMDEF.DefType.s8:
                    if (sbyte.TryParse(field.Value, out var sbyteVal))
                        curCol.SetValue(curRow, sbyteVal);
                    break;
                case PARAMDEF.DefType.s16:
                    if (short.TryParse(field.Value, out var shortVal))
                        curCol.SetValue(curRow, shortVal);
                    break;
                case PARAMDEF.DefType.s32:
                case PARAMDEF.DefType.b32:
                    if (int.TryParse(field.Value, out var intVal))
                        curCol.SetValue(curRow, intVal);
                    break;
                case PARAMDEF.DefType.f32:
                case PARAMDEF.DefType.angle32:
                    if (float.TryParse(field.Value, out var floatVal))
                        curCol.SetValue(curRow, floatVal);
                    break;
                case PARAMDEF.DefType.f64:
                    if (double.TryParse(field.Value, out var doubleVal))
                        curCol.SetValue(curRow, doubleVal);
                    break;
                case PARAMDEF.DefType.u8:
                case PARAMDEF.DefType.dummy8:
                    if (curCol.Def.ArrayLength > 1)
                    {
                        byte[] val = ParamUtils.Dummy8Read(field.Value, curCol.Def.ArrayLength);
                        curCol.SetValue(curRow, val);
                    }
                    else if (byte.TryParse(field.Value, out var byteVal))
                    {
                        curCol.SetValue(curRow, byteVal);
                    }
                    break;
                case PARAMDEF.DefType.u16:
                    if (ushort.TryParse(field.Value, out var ushortVal))
                        curCol.SetValue(curRow, ushortVal);
                    break;
                case PARAMDEF.DefType.u32:
                    if (uint.TryParse(field.Value, out var uintVal))
                        curCol.SetValue(curRow, uintVal);
                    break;
                case PARAMDEF.DefType.fixstr:
                case PARAMDEF.DefType.fixstrW:
                    curCol.SetValue(curRow, field.Value);
                    break;
            }
        }
    }
    #endregion

    #region Position keys (params outside the table list)
    /// <summary>
    /// The (ID, Index) key of every row. The scan starts as if the previous row had ID 0, so a param whose first
    /// row has ID 0 numbers that ID from 1. Delta patches written so far use this numbering, so it is kept.
    /// </summary>
    public static Dictionary<(int ID, int Index), Param.Row> BuildPositionMap(Param param)
    {
        var result = new Dictionary<(int ID, int Index), Param.Row>();

        if (param == null)
            return result;

        var currentRowID = 0;
        var internalIndex = 0;

        foreach (var row in param.Rows)
        {
            if (row.ID == currentRowID)
                internalIndex++;
            else
                internalIndex = 0;

            result[(row.ID, internalIndex)] = row;
            currentRowID = row.ID;
        }

        return result;
    }

    public static Param.Row FindRowAtPosition(Param param, int id, int index)
    {
        if (param == null)
            return null;

        var currentRowID = 0;
        var internalIndex = 0;

        foreach (var row in param.Rows)
        {
            if (row.ID == currentRowID)
                internalIndex++;
            else
                internalIndex = 0;

            if (row.ID == id && internalIndex == index)
                return row;

            currentRowID = row.ID;
        }

        return null;
    }

    /// <summary>
    /// The Index the first row of <paramref name="id"/> has under the position numbering (1 for an ID 0 group at
    /// the start of the param, else 0)
    /// </summary>
    public static int PositionIndexOffset(Param param, int id)
    {
        if (param == null || id != 0 || param.Rows.Count == 0)
            return 0;

        return param.Rows[0].ID == 0 ? 1 : 0;
    }

    /// <summary>
    /// Inserts <paramref name="newRow"/> before the row now at (id, index), after the last row of the ID if the
    /// group is shorter, or in ID order for a new ID
    /// </summary>
    public static void InsertRowAtPosition(Param param, Param.Row newRow, int id, int index)
    {
        var positions = new List<int>();

        for (var i = 0; i < param.Rows.Count; i++)
        {
            if (param.Rows[i].ID == id)
                positions.Add(i);
        }

        if (positions.Count == 0)
        {
            param.InsertRow(GetSortedInsertIndex(param, id), newRow);
            return;
        }

        var localIndex = index - PositionIndexOffset(param, id);

        if (localIndex >= 0 && localIndex < positions.Count)
        {
            param.InsertRow(positions[localIndex], newRow);
            return;
        }

        param.InsertRow(positions[^1] + 1, newRow);
    }
    #endregion

    #region Older patches
    /// <summary>
    /// Moves the row deltas of a table param into whole-group deltas, so patches written before groups existed
    /// go through the group path. Each group is built by applying its row deltas, by position, to
    /// <paramref name="baseParam"/>'s group (the param the positions were counted in). All positions are
    /// resolved against the unchanged group first, so a delete or insert doesn't move a later delta onto
    /// another row. Groups already in <paramref name="paramDelta"/> win over converted rows with the same ID.
    /// </summary>
    public static void ConvertPositionRows(ParamDelta paramDelta, Param baseParam)
    {
        if (paramDelta.Rows.Count == 0)
            return;

        var existingGroups = paramDelta.Groups.Select(g => g.ID).ToHashSet();
        var baseGroups = Build(baseParam);

        foreach (var idGroup in paramDelta.Rows.GroupBy(r => r.ID))
        {
            var id = idGroup.Key;

            if (existingGroups.Contains(id))
                continue;

            baseGroups.TryGetValue(id, out var baseRows);
            baseRows ??= new List<Param.Row>();

            var serializedBase = baseRows.Select((r, i) => SerializeRow(r, i)).ToList();
            var offset = PositionIndexOffset(baseParam, id);

            var rows = ApplyPositionRows(serializedBase, idGroup.ToList(), offset);

            var state = rows.Count == 0
                ? RowDeltaState.Deleted
                : baseRows.Count > 0 ? RowDeltaState.Modified : RowDeltaState.Added;

            var group = new RowGroupDelta { ID = id, State = state };

            for (var i = 0; i < rows.Count; i++)
            {
                rows[i].ID = id;
                rows[i].Index = i;
                rows[i].State = RowDeltaState.Added;
                group.Rows.Add(rows[i]);
            }

            paramDelta.Groups.Add(group);
        }

        paramDelta.Rows.Clear();
        paramDelta.Groups.Sort((x, y) => x.ID.CompareTo(y.ID));
    }

    private static List<RowDelta> ApplyPositionRows(List<RowDelta> baseRows, List<RowDelta> positionRows, int offset)
    {
        var slots = baseRows.Select(CloneRow).ToList();
        var deleted = new bool[slots.Count];
        var inserts = new List<(int Position, RowDelta Row)>();

        foreach (var rowDelta in positionRows)
        {
            var pos = rowDelta.Index - offset;
            var inRange = pos >= 0 && pos < slots.Count;

            switch (rowDelta.State)
            {
                case RowDeltaState.Deleted:
                    if (inRange)
                        deleted[pos] = true;
                    break;

                case RowDeltaState.Modified when inRange:
                    MergeFields(slots[pos], rowDelta);
                    break;

                default:
                    // Added, or Modified with no row at that position (older exports wrote rows appended to a
                    // group as Modified): a new row, starting from the row it goes before (or the first row)
                    var template = inRange ? slots[pos] : slots.FirstOrDefault();
                    var newRow = template != null ? CloneRow(template) : new RowDelta();
                    newRow.Name = "";
                    MergeFields(newRow, rowDelta);
                    inserts.Add((inRange ? pos : slots.Count, newRow));
                    break;
            }
        }

        var result = new List<RowDelta>();

        for (var i = 0; i <= slots.Count; i++)
        {
            foreach (var insert in inserts.Where(e => e.Position == i))
                result.Add(insert.Row);

            if (i < slots.Count && !deleted[i])
                result.Add(slots[i]);
        }

        return result;
    }

    private static void MergeFields(RowDelta target, RowDelta source)
    {
        if (!string.IsNullOrWhiteSpace(source.Name))
            target.Name = source.Name;

        foreach (var field in source.Fields)
        {
            var existing = target.Fields.FirstOrDefault(e => e.Field == field.Field);

            if (existing != null)
                existing.Value = field.Value;
            else
                target.Fields.Add(new FieldDelta { Field = field.Field, Value = field.Value });
        }
    }
    #endregion
}
