#nullable enable
using Andre.Formats;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Data;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.ParamEditor;

public class ParamIO
{
    public static string GenerateColumnLabels(ProjectEntry project, Param param, char separator)
    {
        var str = "";
        str += $@"ID{separator}Name{separator}";

        var paramdef = param.AppliedParamdef;

        if (paramdef != null)
        {
            foreach (PARAMDEF.Field? f in paramdef.Fields.FindAll(f => f.IsValidForRegulationVersion(project.ParamData.PrimaryBank.ParamVersion)))
            {
                str += $@"{f.InternalName}{separator}";
            }
        }

        return str + "\n";
    }

    public static string GenerateCSV(ProjectEntry project, IReadOnlyList<Param.Row> rows, Param param, char separator)
    {
        var gen = "";
        gen += GenerateColumnLabels(project, param, separator);

        foreach (Param.Row row in rows)
        {
            var name = row.Name == null ? "null" : row.Name.Replace(separator, '-');
            var rowgen = $@"{row.ID}{separator}{name}";

            foreach (Param.Column cell in row.Columns)
            {
                rowgen += $@"{separator}{row[cell].Value.ToParamEditorString()}";
            }

            gen += rowgen + "\n";
        }

        return gen;
    }

    public static string GenerateSingleCSV(IReadOnlyList<Param.Row> rows, Param param, string field, char separator)
    {
        var gen = $@"ID{separator}{field}" + "\n";
        foreach (Param.Row row in rows)
        {
            string rowgen;
            if (field.Equals("Name"))
            {
                var name = row.Name == null ? "null" : row.Name.Replace(separator, '-');
                rowgen = $@"{row.ID}{separator}{name}";
            }
            else
            {
                var fieldValue = ParamUtils.GetFieldExportString(row, field);

                rowgen = $@"{row.ID}{separator}{fieldValue}";
            }

            gen += rowgen + "\n";
        }

        return gen;
    }

    public static (string, CompoundAction?) ApplyCSV(ProjectEntry project, ParamBank bank, string csvString, string param,
        bool appendOnly, bool replaceParams, char separator)
    {
        Param p = bank.Params[param];
        if (p == null)
        {
            return ("No Param selected", null);
        }

        var paramdef = p.AppliedParamdef;

        if (paramdef != null)
        {
            var csvLength = paramdef.Fields.FindAll(f => f.IsValidForRegulationVersion(bank.ParamVersion)).Count + 2; // Include ID and name
            var csvLines = csvString.Split("\n");
            if (csvLines[0].StartsWith($@"ID{separator}Name"))
                csvLines[0] = ""; //skip column label row

            var changeCount = 0;
            var addedCount = 0;
            List<EditorAction> actions = new();
            List<Param.Row> addedParams = new();
            foreach (var csvLine in csvLines)
            {
                if (csvLine.Trim().Equals(""))
                {
                    continue;
                }

                var csvs = csvLine.Trim().Split(separator);
                if (csvs.Length != csvLength && !(csvs.Length == csvLength + 1 && csvs[csvLength].Trim().Equals("")))
                {
                    return ("CSV has wrong number of values", null);
                }

                var id = int.Parse(csvs[0]);
                var name = csvs[1];
                Param.Row? row = p[id];
                if (row == null || replaceParams)
                {
                    row = new Param.Row(id, name, p);
                    addedParams.Add(row);
                }

                if (!name.Equals(row.Name))
                {
                    actions.Add(new PropertiesChangedAction(row.GetType().GetProperty("Name"), -1, row, name));
                }

                var index = 2;

                foreach (Param.Column col in row.Columns)
                {
                    var v = csvs[index];
                    index++;

                    if (col.ValueType.IsArray)
                    {
                        var newval = ParamUtils.Dummy8Read(v, col.Def.ArrayLength);

                        if (newval == null)
                        {
                            return ($@"Could not assign {v} to field {col.Def.InternalName}", null);
                        }

                        actions.AppendParamEditAction(row, (PseudoColumn.None, col), newval);
                    }
                    else
                    {
                        var newval = Convert.ChangeType(v, row.Get((PseudoColumn.None, col)).GetType());

                        if (newval == null)
                        {
                            return ($@"Could not assign {v} to field {col.Def.InternalName}", null);
                        }

                        actions.AppendParamEditAction(row, (PseudoColumn.None, col), newval);
                    }
                }
            }

            changeCount = actions.Count;
            addedCount = addedParams.Count;
            if (addedCount != 0)
            {
                actions.Add(new AddParamsAction(project.ParamEditor, p, "legacystring", addedParams, appendOnly, replaceParams));
            }

            return ($@"{changeCount} cells affected, {addedCount} rows added", new CompoundAction(actions));
        }

        return ("Unable to parse CSV into correct data types", null);
    }

    public static (string, CompoundAction?) ApplySingleCSV(ProjectEntry project, ParamBank bank, string csvString, string param,
        string field, char separator, bool ignoreMissingRows, bool onlyAffectEmptyNames = false, bool onlyAffectVanillaNames = false, bool skipInvalidLines = false)
    {
        var getVanillaRow = onlyAffectVanillaNames;
        try
        {
            Param.Row? FindRow(Param p, int id, int idCount, out int iteration)
            {
                iteration = 1;
                for (var i = 0; i < p.Rows.Count; i++)
                {
                    if (p.Rows[i].ID == id)
                    {
                        if (iteration == idCount)
                        {
                            return p.Rows[i];
                        }
                        else
                        {
                            // This is a dupe row and this name is meant for a different row, keep iterating to find the next row with this ID.
                            iteration++;
                        }
                    }
                }

                return null;
            }

            Param p = bank.Params[param];
            Param? p_vanilla = null;
            if (getVanillaRow)
            {
                p_vanilla = project.ParamData.VanillaBank.Params[param];
            }
            if (p == null)
            {
                return ("No Param selected", null);
            }

            var csvLines = csvString.Split("\n");
            if (csvLines[0].Trim().StartsWith($@"ID{separator}"))
            {
                if (!csvLines[0].Contains($@"ID{separator}{field}"))
                {
                    return ("CSV has wrong field name", null);
                }

                csvLines[0] = ""; //skip column label row
            }

            Dictionary<int, int> idCounts = new();
            var changeCount = 0;
            List<EditorAction> actions = new();

            foreach (var csvLine in csvLines)
            {
                if (csvLine.Trim().Equals(""))
                {
                    continue;
                }

                var csvs = csvLine.Trim().Split(separator, 2);

                // Used to skip the empty IDs in the name lists
                if (skipInvalidLines)
                {
                    if (csvs.Length != 2)
                        continue;
                }


                if (csvs.Length != 2 && !(csvs.Length == 3 && csvs[2].Trim().Equals("")))
                {
                    return ("CSV has wrong number of values", null);
                }

                var id = int.Parse(csvs[0]);

                // Track how many times this ID has been defined for the purposes of handing dupe ID row names.
                idCounts.TryAdd(id, 0);
                var idCount = idCounts[id] = idCounts[id] + 1;

                var value = csvs[1];

                Param.Row? row = FindRow(p, id, idCount, out var idIteration);
                Param.Row? row_vanilla = null;
                if (getVanillaRow)
                {
                    if (p_vanilla != null)
                    {
                        row_vanilla = FindRow(p_vanilla, id, idCount, out var idIteration_Vanilla);
                    }
                }

                if (row == null)
                {
                    if (ignoreMissingRows)
                    {
                        continue;
                    }
                    if (idIteration <= 1)
                    {
                        return ($@"Could not locate row {id}", null);
                    }
                    else
                    {
                        return ($@"Could not locate row {id}, iteration {idIteration}", null);
                    }
                }

                if (field.Equals("Name"))
                {
                    if (value.Equals(row.Name))
                    {
                        continue;
                    }

                    // 'onlyAffectEmptyNames' and 'onlyAffectVanillaNames' are only used by "Import Row Names" function at the moment.
                    if (onlyAffectVanillaNames)
                    {
                        if (row_vanilla != null && row.Name == row_vanilla.Name)
                        {
                            actions.Add(new PropertiesChangedAction(row.GetType().GetProperty("Name"), -1, row, value));
                        }
                    }
                    else if (onlyAffectEmptyNames == false || string.IsNullOrEmpty(row.Name))
                    {
                        actions.Add(new PropertiesChangedAction(row.GetType().GetProperty("Name"), -1, row, value));
                    }
                }
                else
                {
                    Param.Column? col = p[field];
                    if (col == null)
                    {
                        return ($@"Could not locate field {field}", null);
                    }

                    if (col.ValueType.IsArray)
                    {
                        var newval = ParamUtils.Dummy8Read(value, col.Def.ArrayLength);
                        if (newval == null)
                        {
                            return ($@"Could not assign {value} to field {col.Def.InternalName}", null);
                        }

                        actions.AppendParamEditAction(row, (PseudoColumn.None, col), newval);
                    }
                    else
                    {
                        var newval = Convert.ChangeType(value, row.Get((PseudoColumn.None, col)).GetType());
                        if (newval == null)
                        {
                            return ($@"Could not assign {value} to field {col.Def.InternalName}", null);
                        }

                        actions.AppendParamEditAction(row, (PseudoColumn.None, col), newval);
                    }
                }
            }

            changeCount = actions.Count;
            return ($@"{changeCount} rows affected", new CompoundAction(actions));
        }
        catch
        {
            return ("Unable to parse CSV into correct data types", null);
        }
    }
}
