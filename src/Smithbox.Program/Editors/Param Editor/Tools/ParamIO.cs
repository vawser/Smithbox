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


public static class ParamIO
{
    private static readonly StringBuilder _reportBuilder = new StringBuilder();

    public static string GenerateCSV(ProjectEntry project, IReadOnlyList<Param.Row> rows, Param param, char separator)
    {
        _reportBuilder.Clear();

        // Columns
        _reportBuilder.Append($@"ID{separator}Name{separator}");

        var paramdef = param.AppliedParamdef;

        if (paramdef != null)
        {
            foreach (PARAMDEF.Field f in paramdef.Fields.FindAll(f => f.IsValidForRegulationVersion(project.Handler.ParamData.PrimaryBank.ParamVersion)))
            {
                _reportBuilder.Append($@"{f.InternalName}{separator}");
            }
        }

        _reportBuilder.Append("\n");

        // Data
        foreach (Param.Row row in rows)
        {
            var name = row.Name == null ? "null" : row.Name.Replace(separator, '-');
            _reportBuilder.Append($@"{row.ID}{separator}{name}");

            foreach (Param.Column cell in row.Columns)
            {
                _reportBuilder.Append($@"{separator}{row[cell].Value.ToParamEditorString()}");
            }

            _reportBuilder.Append("\n");
        }

        return _reportBuilder.ToString();
    }

    public static string GenerateSingleCSV(IReadOnlyList<Param.Row> rows, Param param, string field, char separator)
    {
        _reportBuilder.Clear();

        _reportBuilder.Append($@"ID{separator}{field}");

        foreach (Param.Row row in rows)
        {
            if (field.Equals("Name"))
            {
                var name = row.Name == null ? "null" : row.Name.Replace(separator, '-');
                _reportBuilder.Append($@"{row.ID}{separator}{name}");
            }
            else
            {
                var fieldValue = ParamUtils.GetFieldExportString(row, field);

                _reportBuilder.Append($@"{row.ID}{separator}{fieldValue}");
            }

            _reportBuilder.Append("\n");
        }

        return _reportBuilder.ToString();
    }

    public static (string, CompoundAction) ApplyCSV(ProjectEntry project, ParamBank bank, string csvString, string param,
    bool appendOnly, bool mayReplaceRow, char separator)
    {
        if (!bank.Params.ContainsKey(param))
            return ("Invalid Param Name", null);

        Param p = bank.Params[param];

        if (p == null)
            return ("No Param selected", null);

        var paramdef = p.AppliedParamdef;
        if (paramdef == null)
            return ("Unable to parse CSV into correct data types", null);

        var csvLines = csvString.Split("\n");
        if (csvLines.Length == 0)
            return ("CSV is empty", null);

        // Parse header row to build column index map
        var headerLine = csvLines[0].Trim();
        if (!headerLine.StartsWith($"ID{separator}Name"))
            return ("CSV missing expected header row starting with ID and Name", null);

        var headerCols = headerLine.Split(separator);

        // Map from field InternalName -> index in CSV columns
        var colIndexMap = new Dictionary<string, int>();
        for (var i = 2; i < headerCols.Length; i++)
        {
            var colName = headerCols[i].Trim();
            if (!string.IsNullOrEmpty(colName))
            {
                colIndexMap[colName] = i;
            }
        }

        var changeCount = 0;
        var addedCount = 0;
        List<EditorAction> actions = new();
        List<Param.Row> addedParams = new();

        foreach (var csvLine in csvLines.Skip(1))
        {
            if (csvLine.Trim().Equals(""))
                continue;

            var csvs = csvLine.Trim().Split(separator);
            if (csvs.Length < 2)
                continue;

            var id = int.Parse(csvs[0]);
            var name = csvs[1];
            Param.Row row = p[id];
            if (row == null || mayReplaceRow)
            {
                row = new Param.Row(id, name, p);
                addedParams.Add(row);
            }

            if (!name.Equals(row.Name))
                actions.Add(new PropertiesChangedAction(row.GetType().GetProperty("Name"), -1, row, name));

            // Match columns by name rather than position
            foreach (Param.Column col in row.Columns)
            {
                var internalName = col.Def.InternalName;
                if (!colIndexMap.TryGetValue(internalName, out var csvIndex))
                    continue; // Column not present in CSV, skip it

                if (csvIndex >= csvs.Length)
                    continue; // Row doesn't have this column's value

                var v = csvs[csvIndex];

                if (col.ValueType.IsArray)
                {
                    var newval = ParamUtils.Dummy8Read(v, col.Def.ArrayLength);
                    if (newval == null)
                    {
                        return ($"Could not assign {v} to field {internalName}", null);
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
                            return ($"Could not assign {v} to field {internalName}", null);
                        }

                        actions.AppendParamEditAction(row, (ParamEditorPseudoColumn.None, col), newval);
                    }
                    catch(FormatException)
                    {
                        return ($"Failed to convert CSV string to value.", null);
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

        return ($"{changeCount} cells affected, {addedCount} rows added", new CompoundAction(actions));
    }

    public static (string, CompoundAction) ApplySingleCSV(ProjectEntry project, ParamBank bank, string csvString, string param,
    string field, char separator, bool ignoreMissingRows, bool onlyAffectEmptyNames = false, bool onlyAffectVanillaNames = false, bool skipInvalidLines = false)
    {
        var getVanillaRow = onlyAffectVanillaNames;
        try
        {
            Param.Row FindRow(Param p, int id, int idCount, out int iteration)
            {
                iteration = 1;
                for (var i = 0; i < p.Rows.Count; i++)
                {
                    if (p.Rows[i].ID == id)
                    {
                        if (iteration == idCount)
                            return p.Rows[i];
                        else
                            iteration++;
                    }
                }
                return null;
            }

            Param p = bank.Params[param];
            if (p == null)
                return ("No Param selected", null);

            Param p_vanilla = null;
            if (getVanillaRow)
                p_vanilla = project.Handler.ParamData.VanillaBank.Params[param];

            var csvLines = csvString.Split("\n");

            // Parse header to find which CSV column index holds our target field
            var fieldColIndex = 1; // default: assume second column
            if (csvLines.Length > 0 && csvLines[0].Trim().StartsWith($"ID{separator}"))
            {
                var headerCols = csvLines[0].Trim().Split(separator);
                var foundIndex = Array.FindIndex(headerCols, h => h.Trim().Equals(field, StringComparison.Ordinal));
                if (foundIndex < 0)
                    return ("CSV has wrong field name", null);
                fieldColIndex = foundIndex;
                csvLines[0] = ""; // skip header
            }

            Dictionary<int, int> idCounts = new();
            var changeCount = 0;
            List<EditorAction> actions = new();

            foreach (var csvLine in csvLines)
            {
                if (csvLine.Trim().Equals(""))
                    continue;

                var csvs = csvLine.Trim().Split(separator);

                if (skipInvalidLines && csvs.Length < fieldColIndex + 1)
                    continue;

                if (csvs.Length < fieldColIndex + 1)
                    return ("CSV has wrong number of values.", null);

                var id = int.Parse(csvs[0]);

                idCounts.TryAdd(id, 0);
                var idCount = idCounts[id] = idCounts[id] + 1;

                var value = csvs[fieldColIndex];

                Param.Row row = FindRow(p, id, idCount, out var idIteration);
                Param.Row row_vanilla = null;
                if (getVanillaRow && p_vanilla != null)
                    row_vanilla = FindRow(p_vanilla, id, idCount, out _);

                if (row == null)
                {
                    if (ignoreMissingRows)
                        continue;
                    return idIteration <= 1
                        ? ($"Could not locate row {id}", null)
                        : ($"Could not locate row {id}, iteration {idIteration}", null);
                }

                if (field.Equals("Name"))
                {
                    if (value.Equals(row.Name))
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
                        return ($"Could not locate field {field}", null);

                    try
                    {
                        if (col.ValueType.IsArray)
                        {
                            var newval = ParamUtils.Dummy8Read(value, col.Def.ArrayLength);
                            if (newval == null)
                                return ($"Could not assign {value} to field {col.Def.InternalName}", null);
                            actions.AppendParamEditAction(row, (ParamEditorPseudoColumn.None, col), newval);
                        }
                        else
                        {
                            var newval = Convert.ChangeType(value, row.Get((ParamEditorPseudoColumn.None, col)).GetType());
                            if (newval == null)
                                return ($"Could not assign {value} to field {col.Def.InternalName}", null);
                            actions.AppendParamEditAction(row, (ParamEditorPseudoColumn.None, col), newval);
                        }
                    }
                    catch (FormatException)
                    {
                        return ($"Failed to convert CSV string to value.", null);
                    }
                }
            }

            changeCount = actions.Count;
            return ($"{changeCount} rows affected", new CompoundAction(actions));
        }
        catch
        {
            return ("Unable to parse CSV into correct data types", null);
        }
    }
}
