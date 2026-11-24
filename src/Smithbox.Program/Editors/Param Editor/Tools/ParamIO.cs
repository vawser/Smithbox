using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// CSV export and import for params.
/// <para>
/// Fields are quoted in the usual CSV way when needed: a field containing the separator, a quote or a line break,
/// or with leading or trailing spaces, is wrapped in quotes, with quotes inside doubled. Unnamed rows export with an
/// empty name. Files from older versions, which wrote unnamed rows as the bare word <c>null</c>, still import those
/// rows as unnamed; a row really named "null" is exported quoted, so it keeps its name.
/// </para>
/// </summary>
public static class ParamIO
{
    private static readonly StringBuilder _reportBuilder = new StringBuilder();

    #region Export
    public static string GenerateCSV(ProjectEntry project, IReadOnlyList<Param.Row> rows, Param param, char separator, bool includeName = true, Dictionary<string, bool> includedFields = null)
    {
        _reportBuilder.Clear();

        // The param's columns, not its paramdef's fields: the columns are what each row has (fields not valid for
        // the regulation version are left out of them), so the header and the values always line up
        var columns = param.Columns
            .Where(c => includedFields == null
                || (includedFields.TryGetValue(c.Def.InternalName, out var include) && include))
            .ToList();

        // Columns
        _reportBuilder.Append(includeName ? $@"ID{separator}Name" : "ID");

        foreach (var col in columns)
        {
            _reportBuilder.Append(separator);
            _reportBuilder.Append(CsvField(col.Def.InternalName, separator));
        }

        _reportBuilder.Append('\n');

        // Data
        foreach (Param.Row row in rows)
        {
            _reportBuilder.Append(row.ID);

            if (includeName)
            {
                _reportBuilder.Append(separator);
                _reportBuilder.Append(CsvName(row.Name, separator));
            }

            foreach (var col in columns)
            {
                _reportBuilder.Append(separator);
                _reportBuilder.Append(CsvField(row[col].Value.ToParamEditorString(), separator));
            }

            _reportBuilder.Append('\n');
        }

        return _reportBuilder.ToString();
    }

    public static string GenerateSingleCSV(IReadOnlyList<Param.Row> rows, Param param, string field, char separator)
    {
        _reportBuilder.Clear();

        _reportBuilder.Append($@"ID{separator}{CsvField(field, separator)}");
        _reportBuilder.Append('\n');

        foreach (Param.Row row in rows)
        {
            _reportBuilder.Append(row.ID);
            _reportBuilder.Append(separator);

            if (field.Equals("Name"))
            {
                _reportBuilder.Append(CsvName(row.Name, separator));
            }
            else
            {
                _reportBuilder.Append(CsvField(ParamUtils.GetFieldExportString(row, field), separator));
            }

            _reportBuilder.Append('\n');
        }

        return _reportBuilder.ToString();
    }

    /// <summary>
    /// A row name for export: empty for an unnamed row, and quoted if it is literally "null" (an unquoted null is
    /// read back as unnamed, for files from older versions)
    /// </summary>
    private static string CsvName(string name, char separator)
    {
        if (name == null)
            return "";

        if (name == "null")
            return "\"null\"";

        return CsvField(name, separator);
    }

    /// <summary>
    /// A value for export, quoted if it contains the separator, a quote or a line break, or has leading or trailing
    /// whitespace
    /// </summary>
    private static string CsvField(string value, char separator)
    {
        value ??= "";

        var needsQuotes = value.IndexOf(separator) >= 0
            || value.IndexOfAny(new[] { '"', '\n', '\r' }) >= 0
            || (value.Length > 0 && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])));

        return needsQuotes ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
    #endregion

    #region Parse
    /// <summary>
    /// One CSV record: its fields, whether each was quoted, and the line it starts on (from 1, header included)
    /// </summary>
    private class CsvRecord
    {
        public int Line;
        public string[] Fields;
        public bool[] Quoted;

        public int Length => Fields.Length;
    }

    /// <summary>
    /// Splits CSV text into records. Quoted fields may contain the separator, doubled quotes and line breaks.
    /// Unquoted fields are trimmed of spaces and tabs (unless that is the separator), as whole lines used to be.
    /// Blank lines are skipped.
    /// </summary>
    private static List<CsvRecord> ParseCsv(string text, char separator)
    {
        var records = new List<CsvRecord>();
        var fields = new List<string>();
        var quoted = new List<bool>();
        var field = new StringBuilder();

        var inQuotes = false;
        var fieldQuoted = false;
        var line = 1;
        var recordLine = 1;

        void EndField()
        {
            var value = field.ToString();

            if (!fieldQuoted)
            {
                value = TrimUnquoted(value, separator);
            }

            fields.Add(value);
            quoted.Add(fieldQuoted);
            field.Clear();
            fieldQuoted = false;
        }

        void EndRecord()
        {
            EndField();

            var isBlank = fields.Count == 1 && fields[0] == "" && !quoted[0];

            if (!isBlank)
            {
                records.Add(new CsvRecord { Line = recordLine, Fields = fields.ToArray(), Quoted = quoted.ToArray() });
            }

            fields.Clear();
            quoted.Clear();
        }

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    if (c == '\n')
                        line++;

                    field.Append(c);
                }
            }
            else if (c == '"' && !fieldQuoted && string.IsNullOrWhiteSpace(field.ToString()))
            {
                // Opening quote (spaces before it are dropped)
                field.Clear();
                inQuotes = true;
                fieldQuoted = true;
            }
            else if (c == separator)
            {
                EndField();
            }
            else if (c == '\n')
            {
                EndRecord();
                line++;
                recordLine = line;
            }
            else if (c == '\r')
            {
                // Part of a CRLF line ending
            }
            else
            {
                field.Append(c);
            }
        }

        if (field.Length > 0 || fields.Count > 0 || fieldQuoted)
        {
            EndRecord();
        }

        return records;
    }

    private static string TrimUnquoted(string value, char separator)
    {
        if (separator == ' ' || separator == '\t')
            return value;

        return value.Trim(' ', '\t');
    }

    /// <summary>
    /// The name in a record's field: an unquoted <c>null</c> (written for unnamed rows by older versions) is unnamed
    /// </summary>
    private static string ImportName(CsvRecord record, int index)
    {
        var value = record.Fields[index];

        return !record.Quoted[index] && value == "null" ? "" : value;
    }

    /// <summary>
    /// Rows grouped by ID, in param order. A param can hold several rows with the same ID, and the n-th CSV record
    /// with an ID is matched to the n-th row with that ID.
    /// </summary>
    private static Dictionary<int, List<Param.Row>> GetRowsById(Param param)
    {
        var rowsById = new Dictionary<int, List<Param.Row>>();

        foreach (var row in param.Rows)
        {
            if (!rowsById.TryGetValue(row.ID, out var list))
            {
                list = new List<Param.Row>();
                rowsById[row.ID] = list;
            }

            list.Add(row);
        }

        return rowsById;
    }
    #endregion

    #region Import
    /// <summary>
    /// Builds the actions for a CSV import. Nothing is changed here: the caller executes the returned action.
    /// Any failure, including an unexpected exception, returns a message and a null action, so a bad file can't
    /// crash the editor or apply part of an import.
    /// </summary>
    public static (string, CompoundAction) ApplyCSV(ProjectEntry project, ParamBank bank, string csvString, string param,
    bool appendOnly, bool mayReplaceRow, char separator)
    {
        try
        {
            return BuildCSVImport(project, bank, csvString, param, appendOnly, mayReplaceRow, separator);
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(ParamIO), LOC.Get("PARAM_CSV_Import_Failed", e.Message), e);
            return (LOC.Get("PARAM_CSV_Import_Failed", e.Message), null);
        }
    }

    private static (string, CompoundAction) BuildCSVImport(ProjectEntry project, ParamBank bank, string csvString, string param,
    bool appendOnly, bool mayReplaceRow, char separator)
    {
        if (param == null)
            return (LOC.Get("PARAM_CSV_No_Param_Selected"), null);

        if (string.IsNullOrEmpty(csvString))
            return (LOC.Get("PARAM_CSV_Empty_File"), null);

        if (!bank.Params.ContainsKey(param))
            return (LOC.Get("PARAM_CSV_Invalid_Param_Name"), null);

        Param p = bank.Params[param];

        if (p == null)
            return (LOC.Get("PARAM_CSV_No_Param_Selected"), null);

        var paramdef = p.AppliedParamdef;
        if (paramdef == null)
            return (LOC.Get("PARAM_CSV_Invalid_Data_Type_Parse"), null);

        var records = ParseCsv(csvString, separator);
        if (records.Count == 0)
            return (LOC.Get("PARAM_CSV_Empty_File"), null);

        // Parse header row to build column index map
        var header = records[0];
        if (header.Length < 2 || header.Fields[0] != "ID")
        {
            return (LOC.Get("PARAM_CSV_Missing_Header"), null);
        }

        var includesName = header.Fields[1] == "Name";

        // Map from field InternalName -> index in CSV columns
        var colIndexMap = new Dictionary<string, int>();

        var startIndex = includesName ? 2 : 1;

        for (var i = startIndex; i < header.Length; i++)
        {
            var colName = header.Fields[i];
            if (!string.IsNullOrEmpty(colName))
            {
                colIndexMap[colName] = i;
            }
        }

        var rowsById = GetRowsById(p);
        var idCounts = new Dictionary<int, int>();

        var changeCount = 0;
        var addedCount = 0;
        List<EditorAction> actions = new();
        List<Param.Row> addedParams = new();

        foreach (var record in records.Skip(1))
        {
            if (record.Length < 2)
                continue;

            if (!int.TryParse(record.Fields[0], out var id))
            {
                return (LOC.Get("PARAM_CSV_Invalid_Row_ID", record.Line, record.Fields[0]), null);
            }

            // The n-th record with this ID updates the n-th row with it. A record beyond the rows the param has
            // for the ID updates the first of them, as before (adding it would give it a different ID).
            idCounts.TryGetValue(id, out var occurrence);
            idCounts[id] = occurrence + 1;

            Param.Row row = null;
            if (rowsById.TryGetValue(id, out var idRows))
            {
                row = occurrence < idRows.Count ? idRows[occurrence] : idRows[0];
            }

            // If a user only wants to import new rows from the CSV
            if(CFG.Current.ParamEditor.Param_CSV_Ignore_Existing_Rows)
            {
                if (row != null)
                    continue;
            }

            // Without a Name column, existing rows keep their name and new rows are unnamed
            string name = includesName ? ImportName(record, 1) : row?.Name ?? "";

            if (row == null || mayReplaceRow)
            {
                row = new Param.Row(id, name, p);
                addedParams.Add(row);
            }

            // An unnamed row (null) and an empty name are the same thing, so importing one over the other is no change
            if (includesName && name != (row.Name ?? ""))
                actions.Add(new PropertiesChangedAction(row.GetType().GetProperty("Name"), -1, row, name));

            // Match columns by name rather than position
            foreach (Param.Column col in row.Columns)
            {
                var internalName = col.Def.InternalName;
                if (!colIndexMap.TryGetValue(internalName, out var csvIndex))
                    continue; // Column not present in CSV, skip it

                if (csvIndex >= record.Length)
                    continue; // Row doesn't have this column's value

                var v = record.Fields[csvIndex];

                if (col.ValueType.IsArray)
                {
                    var newval = ParamUtils.Dummy8Read(v, col.Def.ArrayLength);
                    if (newval == null)
                    {
                        return (LOC.Get("PARAM_CSV_Failed_to_Assign_Value_To_Field", v, internalName), null);
                    }

                    actions.AppendParamEditAction(row, (ParamEditorPseudoColumn.None, col), newval);
                }
                else
                {
                    try
                    {
                        var newval = Convert.ChangeType(v, row.Get((ParamEditorPseudoColumn.None, col)).GetType());
                        if (newval == null)
                        {
                            return (LOC.Get("PARAM_CSV_Failed_to_Assign_Value_To_Field", v, internalName), null);
                        }

                        actions.AppendParamEditAction(row, (ParamEditorPseudoColumn.None, col), newval);
                    }
                    catch(FormatException)
                    {
                        return (LOC.Get("PARAM_CSV_Failed_to_Convert_String_to_Value"), null);
                    }
                }
            }
        }

        changeCount = actions.Count;
        addedCount = addedParams.Count;
        if (addedCount != 0)
        {
            actions.Add(new AddParamsAction(project.Handler.ParamEditor, p, "legacystring", addedParams, appendOnly, mayReplaceRow));
        }

        return (LOC.Get("PARAM_CSV_Result_Effect", changeCount, addedCount), new CompoundAction(actions));
    }

    public static (string, CompoundAction) ApplySingleCSV(ProjectEntry project, ParamBank bank, string csvString, string param,
    string field, char separator, bool ignoreMissingRows, bool onlyAffectEmptyNames = false, bool onlyAffectVanillaNames = false, bool skipInvalidLines = false)
    {
        var getVanillaRow = onlyAffectVanillaNames;
        try
        {
            Param p = bank.Params[param];
            if (p == null)
                return (LOC.Get("PARAM_CSV_No_Param_Selected"), null);

            Param p_vanilla = null;
            if (getVanillaRow)
                p_vanilla = project.Handler.ParamData.VanillaBank.Params[param];

            var records = ParseCsv(csvString ?? "", separator);

            // Parse header to find which CSV column index holds our target field
            var fieldColIndex = 1; // default: assume second column
            if (records.Count > 0 && records[0].Length > 1 && records[0].Fields[0] == "ID")
            {
                var foundIndex = Array.FindIndex(records[0].Fields, h => h.Equals(field, StringComparison.Ordinal));
                if (foundIndex < 0)
                    return (LOC.Get("PARAM_CSV_Wrong_Field_Name"), null);
                fieldColIndex = foundIndex;
                records.RemoveAt(0); // skip header
            }

            var rowsById = GetRowsById(p);
            var vanillaRowsById = p_vanilla != null ? GetRowsById(p_vanilla) : null;

            Dictionary<int, int> idCounts = new();
            var changeCount = 0;
            List<EditorAction> actions = new();

            foreach (var record in records)
            {
                if (skipInvalidLines && record.Length < fieldColIndex + 1)
                    continue;

                if (record.Length < fieldColIndex + 1)
                    return (LOC.Get("PARAM_CSV_Wrong_Value_Count"), null);

                if (!int.TryParse(record.Fields[0], out var id))
                {
                    if (skipInvalidLines)
                        continue;

                    return (LOC.Get("PARAM_CSV_Invalid_Row_ID", record.Line, record.Fields[0]), null);
                }

                idCounts.TryAdd(id, 0);
                var idCount = idCounts[id] = idCounts[id] + 1;

                var value = record.Fields[fieldColIndex];

                // The n-th record with this ID is matched to the n-th row with it
                Param.Row row = rowsById.TryGetValue(id, out var idRows) && idCount <= idRows.Count
                    ? idRows[idCount - 1]
                    : null;

                Param.Row row_vanilla = null;
                if (vanillaRowsById != null && vanillaRowsById.TryGetValue(id, out var vanillaIdRows) && idCount <= vanillaIdRows.Count)
                    row_vanilla = vanillaIdRows[idCount - 1];

                if (row == null)
                {
                    if (ignoreMissingRows)
                        continue;
                    return idCount <= 1
                        ? (LOC.Get("PARAM_CSV_Failed_to_Locate_Row", id), null)
                        : (LOC.Get("PARAM_CSV_Failed_to_Locate_Row_Iteration", id, idCount), null);
                }

                if (field.Equals("Name"))
                {
                    value = ImportName(record, fieldColIndex);

                    // An unnamed row (null) and an empty name are the same thing
                    if (value == (row.Name ?? ""))
                        continue;

                    if (onlyAffectVanillaNames)
                    {
                        if (row_vanilla != null && row.Name == row_vanilla.Name)
                            actions.Add(new PropertiesChangedAction(row.GetType().GetProperty("Name"), -1, row, value));
                    }
                    else if (!onlyAffectEmptyNames || string.IsNullOrEmpty(row.Name))
                    {
                        actions.Add(new PropertiesChangedAction(row.GetType().GetProperty("Name"), -1, row, value));
                    }
                }
                else
                {
                    Param.Column col = p[field];
                    if (col == null)
                        return (LOC.Get("PARAM_CSV_Failed_to_Locate_Field", field), null);

                    try
                    {
                        if (col.ValueType.IsArray)
                        {
                            var newval = ParamUtils.Dummy8Read(value, col.Def.ArrayLength);

                            if (newval == null)
                            {
                                return (LOC.Get("PARAM_CSV_Failed_to_Assign_Value_To_Field", value, col.Def.InternalName), null);
                            }

                            actions.AppendParamEditAction(row, (ParamEditorPseudoColumn.None, col), newval);
                        }
                        else
                        {
                            var newval = Convert.ChangeType(value, row.Get((ParamEditorPseudoColumn.None, col)).GetType());
                            if (newval == null)
                            {
                                return (LOC.Get("PARAM_CSV_Failed_to_Assign_Value_To_Field", value, col.Def.InternalName), null);
                            }

                            actions.AppendParamEditAction(row, (ParamEditorPseudoColumn.None, col), newval);
                        }
                    }
                    catch (FormatException)
                    {
                        return (LOC.Get("PARAM_CSV_Failed_to_Convert_String_to_Value"), null);
                    }
                }
            }

            changeCount = actions.Count;
            return (LOC.Get("PARAM_CSV_Row_Result_Effect", changeCount), new CompoundAction(actions));
        }
        catch
        {
            return (LOC.Get("PARAM_CSV_Invalid_Data_Type_Parse"), null);
        }
    }
    #endregion
}
