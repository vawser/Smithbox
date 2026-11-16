using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public static class ParamRowOperations
{
    #region Set Row Name / ID
    /// <summary>
    /// Renames the given rows, as one undoable step.
    /// The change is applied directly rather than through a mass-edit command string, so any name
    /// works, including ones containing ';' or starting with '##', which the mass-edit grammar treats as syntax.
    /// </summary>
    public static bool SetRowName(ParamEditorView curView, IEnumerable<Param.Row> rows, string newName)
    {
        return SetRowProperty(curView, rows, nameof(Param.Row.Name), newName ?? "");
    }

    /// <summary>
    /// Sets the ID of the given rows, as one undoable step. Several rows may end up sharing an ID; that is
    /// intended for table-group params, where a group is a set of rows with the same ID.
    /// </summary>
    public static bool SetRowID(ParamEditorView curView, IEnumerable<Param.Row> rows, int newID)
    {
        return SetRowProperty(curView, rows, nameof(Param.Row.ID), newID);
    }

    private static bool SetRowProperty(ParamEditorView curView, IEnumerable<Param.Row> rows, string propertyName, object value)
    {
        var prop = typeof(Param.Row).GetProperty(propertyName);

        // Only rows whose value actually changes, so an unchanged commit doesn't add an empty undo step
        var actions = rows
            .Where(row => row != null && !Equals(prop.GetValue(row), value))
            .Select(row => (EditorAction)new PropertiesChangedAction(prop, -1, row, value))
            .ToList();

        if (actions.Count == 0)
            return false;

        // Refused while a save is running
        if (!curView.Editor.ActionManager.ExecuteAction(new CompoundAction(actions)))
            return false;

        curView.GetParamData().RefreshParamDifferenceCacheTask();

        return true;
    }
    #endregion

    #region Set Row to Default
    public static void SetRowToDefault(ParamEditorView curView)
    {
        var curParamKey = curView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        if (!curView.GetPrimaryBank().Params.TryGetValue(curParamKey, out Param baseParam) || baseParam == null)
            return;

        if (!curView.GetVanillaBank().Params.TryGetValue(curParamKey, out Param vanillaParam) || vanillaParam == null)
            return;

        var rows = curView.Selection.GetSelectedRows()?.Where(r => r != null).ToList();

        if (rows == null || rows.Count == 0)
            return;

        // Vanilla columns by field name, in case the two params' column lists differ
        var vanillaColumns = new Dictionary<string, Param.Column>();

        foreach (var vCol in vanillaParam.Columns)
        {
            vanillaColumns.TryAdd(vCol.Def.InternalName, vCol);
        }

        // Each row's values are set back to vanilla in place (the row keeps its name, its position and its object),
        // so the selection, the Fields window and earlier undo steps all still refer to the row in the param.
        // Replacing the row object instead left the selection holding a row that was no longer in the param.
        List<EditorAction> actions = new();

        foreach (Param.Row bRow in rows)
        {
            var vRow = FindMatchingVanillaRow(baseParam, vanillaParam, bRow);

            if (vRow == null)
                continue;

            foreach (var col in baseParam.Columns)
            {
                if (!vanillaColumns.TryGetValue(col.Def.InternalName, out var vCol))
                    continue;

                var vanillaValue = vRow[vCol].Value;

                if (vanillaValue == null || vanillaValue.GetType() != col.ValueType)
                    continue;

                actions.AppendParamEditAction(bRow, (ParamEditorPseudoColumn.None, col), vanillaValue);
            }
        }

        if (actions.Count == 0)
            return;

        curView.Editor.ActionManager.ExecuteAction(new CompoundAction(actions));
        curView.GetParamData().RefreshParamDifferenceCacheTask();
    }

    /// <summary>
    /// The vanilla row for <paramref name="row"/>: the vanilla row with the same ID, matched by occurrence when the
    /// ID appears more than once (the second row with ID 100 matches the second vanilla row with ID 100).
    /// </summary>
    private static Param.Row FindMatchingVanillaRow(Param baseParam, Param vanillaParam, Param.Row row)
    {
        var occurrence = 0;

        foreach (var r in baseParam.Rows)
        {
            if (r == row)
                break;

            if (r.ID == row.ID)
                occurrence++;
        }

        Param.Row first = null;
        var seen = 0;

        foreach (var vRow in vanillaParam.Rows)
        {
            if (vRow.ID != row.ID)
                continue;

            first ??= vRow;

            if (seen == occurrence)
                return vRow;

            seen++;
        }

        // Fewer vanilla rows with this ID than in the mod: fall back to the first one
        return first;
    }
    #endregion

    #region Copy Row Details
    public static void CopyRowDetails(ParamEditorView curView, bool includeName = false)
    {
        var curParamKey = curView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param param = curView.GetPrimaryBank().Params[curParamKey];
        List<Param.Row> rows = curView.Selection.GetSelectedRows();

        if (rows.Count == 0)
        {
            return;
        }

        var output = "";

        foreach (var entry in rows)
        {
            var id = entry.ID;
            var name = entry.Name;

            var entryOutput = $"{id}";

            if (includeName)
            {
                entryOutput = $"{id};{name}";
            }

            if (output == "")
            {
                output = $"{entryOutput}";
            }
            else
            {
                output = $"{output}, {entryOutput}";
            }
        }

        PlatformUtils.Instance.SetClipboardText(output);
    }
    #endregion

    #region Proliferate Name
    public static void ProliferateRowName(ParamEditorView curView, string targetField, bool appendName, string appendPrefix)
    {
        if (targetField == null)
            return;

        var curParamKey = curView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param baseParam = curView.GetPrimaryBank().Params[curParamKey];

        if (baseParam == null)
            return;

        List<Param.Row> rows = curView.Selection.GetSelectedRows();

        var paramMeta = curView.GetParamData().GetParamMeta(baseParam.AppliedParamdef);

        var actions = new List<EditorAction>();

        var displayWarning = false;

        foreach (Param.Row row in rows)
        {
            var fieldDef = row.Def.Fields.FirstOrDefault(e => e.InternalName == targetField);

            if (fieldDef == null)
            {
                displayWarning = true;
                continue;
            }

            var targetCell = row.Cells.Where(e => e.Def == fieldDef).FirstOrDefault();
            var fieldMeta = curView.GetParamData().GetParamFieldMeta(paramMeta, fieldDef);

            if (fieldMeta == null)
                continue;

            List<(string, Param.Row, string)> refs = ParamReferenceResolver.ResolveParamReferences(
                curView, fieldMeta.RefTypes, fieldMeta.RefGroup, row, targetCell.Value);

            foreach ((string, Param.Row, string) rf in refs)
            {
                if (row == null || curView.Editor.ActionManager == null)
                {
                    continue;
                }

                if(appendName)
                {
                    rf.Item2.Name = $"{rf.Item2.Name}{appendPrefix}{row.Name}";
                }
                else
                {
                    rf.Item2.Name = row.Name;
                }

                // The referenced row belongs to another param
                ParamDirtyTracker.MarkDirty(rf.Item2);
            }
        }

        if (displayWarning)
        {
            Smithbox.Log(typeof(ParamRowOperations), 
                LOC.Get("PARAM_ProliferateRowName_Log_Failed_to_Find_Field", targetField));
        }
    }
    #endregion

    #region Inherit Row Name
    public static void InheritRowName(ParamEditorView curView, string targetField, bool appendName, string appendPrefix)
    {
        if (targetField == null)
            return;

        var curParamKey = curView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param baseParam = curView.GetPrimaryBank().Params[curParamKey];

        if (baseParam == null)
            return;

        List<Param.Row> rows = curView.Selection.GetSelectedRows();

        var paramMeta = curView.GetParamData().GetParamMeta(baseParam.AppliedParamdef);

        var actions = new List<EditorAction>();

        var displayWarning = false;

        foreach (Param.Row row in rows)
        {
            var fieldDef = row.Def.Fields.FirstOrDefault(e => e.InternalName == targetField);

            if (fieldDef == null)
            {
                displayWarning = true;
                continue;
            }

            var targetCell = row.Cells.Where(e => e.Def == fieldDef).FirstOrDefault();
            var fieldMeta = curView.GetParamData().GetParamFieldMeta(paramMeta, fieldDef);

            if (fieldMeta == null)
                continue;

            List<(string, Param.Row, string)> refs = ParamReferenceResolver.ResolveParamReferences(curView, fieldMeta.RefTypes, fieldMeta.RefGroup, row, targetCell.Value);

            foreach ((string, Param.Row, string) rf in refs)
            {
                if (row == null || curView.Editor.ActionManager == null)
                {
                    continue;
                }

                if(appendName)
                {
                    row.Name = $"{row.Name}{appendPrefix}{rf.Item2.Name}";
                }
                else
                {
                    row.Name = rf.Item2.Name;
                }
            }
        }

        if (rows.Count > 0)
        {
            ParamDirtyTracker.MarkDirty(baseParam);
        }

        if (displayWarning)
        {
            Smithbox.Log(typeof(ParamRowOperations),
                LOC.Get("PARAM_ProliferateRowName_Log_Failed_to_Find_Field", targetField));
        }
    }
    #endregion

    #region Inherit Row Name from FMG
    public static void InheritRowNameFromFMG(ParamEditorView curView, string targetField, bool appendName, string appendPrefix)
    {
        if (targetField == null)
            return;

        var curParamKey = curView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param baseParam = curView.GetPrimaryBank().Params[curParamKey];

        if (baseParam == null)
            return;

        List<Param.Row> rows = curView.Selection.GetSelectedRows();

        var paramMeta = curView.GetParamData().GetParamMeta(baseParam.AppliedParamdef);

        var actions = new List<EditorAction>();

        var displayWarning = false;

        foreach (Param.Row row in rows)
        {
            var fieldDef = row.Def.Fields.FirstOrDefault(e => e.InternalName == targetField);

            if (fieldDef == null)
            {
                displayWarning = true;
                continue;
            }

            var targetCell = row.Cells.Where(e => e.Def == fieldDef).FirstOrDefault();
            var fieldMeta = curView.GetParamData().GetParamFieldMeta(paramMeta, fieldDef);

            if (fieldMeta == null)
                continue;

            List<TextResult> refs = ParamReferenceResolver.ResolveTextReferences(curView, fieldMeta.FmgRef, row, targetCell.Value);

            foreach (var result in refs)
            {
                if (row == null || curView.Editor.ActionManager == null)
                {
                    continue;
                }

                if (appendName)
                {
                    row.Name = $"{row.Name}{appendPrefix}{result.Entry.Text}";
                }
                else
                {
                    row.Name = result.Entry.Text;
                }
            }
        }

        if (rows.Count > 0)
        {
            ParamDirtyTracker.MarkDirty(baseParam);
        }

        if (displayWarning)
        {
            Smithbox.Log(typeof(ParamRowOperations),
                LOC.Get("PARAM_ProliferateRowName_Log_Failed_to_Find_Field", targetField));
        }
    }
    #endregion

    #region Inherit Row Name from Row FMG
    public static void InheritRowNameFromRowFMG(ParamEditorView curView, bool appendName, string appendPrefix)
    {
        var curParamKey = curView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param baseParam = curView.GetPrimaryBank().Params[curParamKey];

        if (baseParam == null)
            return;

        List<Param.Row> rows = curView.Selection.GetSelectedRows();

        var decorator = curView.RowDecorators.GetFmgRowDecorator(curParamKey);

        foreach (Param.Row row in rows)
        {
            var fmgEntry = decorator.GetRowFmgEntry(row);
            if (fmgEntry != null)
            {
                if (appendName)
                {
                    row.Name = $"{row.Name}{appendPrefix}{fmgEntry.Text}";
                }
                else
                {
                    row.Name = fmgEntry.Text;
                }
            }
        }

        if (rows.Count > 0)
        {
            ParamDirtyTracker.MarkDirty(baseParam);
        }
    }
    #endregion

    #region Inherit Row Name from Alias
    public static void InheritRowNameFromAlias(ParamEditorView curView, string targetField, bool appendName, string appendPrefix)
    {
        if (targetField == null)
            return;

        var curParamKey = curView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param baseParam = curView.GetPrimaryBank().Params[curParamKey];

        if (baseParam == null)
            return;

        List<Param.Row> rows = curView.Selection.GetSelectedRows();

        var paramMeta = curView.GetParamData().GetParamMeta(baseParam.AppliedParamdef);

        var actions = new List<EditorAction>();

        var displayWarning = false;

        foreach (Param.Row row in rows)
        {
            var fieldDef = row.Def.Fields.FirstOrDefault(e => e.InternalName == targetField);

            if (fieldDef == null)
            {
                displayWarning = true;
                continue;
            }

            var targetCell = row.Cells.Where(e => e.Def == fieldDef).FirstOrDefault();
            var fieldMeta = curView.GetParamData().GetParamFieldMeta(paramMeta, fieldDef);

            if (fieldMeta == null)
                continue;

            if (fieldMeta.ShowCharacterEnumList)
            {
                foreach (var entry in curView.Editor.Project.Handler.ProjectData.Aliases[ProjectAliasType.Characters])
                {
                    var text = entry.ID.Substring(1);
                    if (text == $"{targetCell.Value}")
                    {
                        if (appendName)
                        {
                            row.Name = $"{row.Name}{appendPrefix}{entry.Name}";
                        }
                        else
                        {
                            row.Name = entry.Name;
                        }
                        break;
                    }
                }
            }

            if (fieldMeta.ShowCutsceneEnumList)
            {
                foreach (var entry in curView.Editor.Project.Handler.ProjectData.Aliases[ProjectAliasType.Cutscenes])
                {
                    if (entry.ID == $"{targetCell.Value}")
                    {
                        if (appendName)
                        {
                            row.Name = $"{row.Name}{appendPrefix}{entry.Name}";
                        }
                        else
                        {
                            row.Name = entry.Name;
                        }
                        break;
                    }
                }
            }

            if (fieldMeta.ShowFlagEnumList)
            {
                foreach (var entry in curView.Editor.Project.Handler.ProjectData.Aliases[ProjectAliasType.EventFlags])
                {
                    if (entry.ID == $"{targetCell.Value}")
                    {
                        if (appendName)
                        {
                            row.Name = $"{row.Name}{appendPrefix}{entry.Name}";
                        }
                        else
                        {
                            row.Name = entry.Name;
                        }
                        break;
                    }
                }
            }

            if (fieldMeta.ShowMovieEnumList)
            {
                foreach (var entry in curView.Editor.Project.Handler.ProjectData.Aliases[ProjectAliasType.Movies])
                {
                    if (entry.ID == $"{targetCell.Value}")
                    {
                        if (appendName)
                        {
                            row.Name = $"{row.Name}{appendPrefix}{entry.Name}";
                        }
                        else
                        {
                            row.Name = entry.Name;
                        }
                        break;
                    }
                }
            }

            if (fieldMeta.ShowParticleEnumList)
            {
                foreach (var entry in curView.Editor.Project.Handler.ProjectData.Aliases[ProjectAliasType.Particles])
                {
                    if (entry.ID == $"{targetCell.Value}")
                    {
                        if (appendName)
                        {
                            row.Name = $"{row.Name}{appendPrefix}{entry.Name}";
                        }
                        else
                        {
                            row.Name = entry.Name;
                        }
                        break;
                    }
                }
            }

            if (fieldMeta.ShowSoundEnumList)
            {
                foreach (var entry in curView.Editor.Project.Handler.ProjectData.Aliases[ProjectAliasType.Sounds])
                {
                    if (entry.ID == $"{targetCell.Value}")
                    {
                        if (appendName)
                        {
                            row.Name = $"{row.Name}{appendPrefix}{entry.Name}";
                        }
                        else
                        {
                            row.Name = entry.Name;
                        }
                        break;
                    }
                }
            }
        }

        if (rows.Count > 0)
        {
            ParamDirtyTracker.MarkDirty(baseParam);
        }

        if (displayWarning)
        {
            Smithbox.Log(typeof(ParamRowOperations),
                LOC.Get("PARAM_ProliferateRowName_Log_Failed_to_Find_Field", targetField));
        }
    }
    #endregion

    #region Adjust Row Name
    public static void AdjustRowName(ParamEditorView curView, string adjustment, int insertIndex, ParamRowNameAdjustType type)
    {
        if (type != ParamRowNameAdjustType.Clear && string.IsNullOrEmpty(adjustment))
            return;

        string curParamKey = curView.Selection.GetActiveParam();

        if (string.IsNullOrEmpty(curParamKey))
            return;

        Param baseParam = curView.GetPrimaryBank().Params[curParamKey];

        if (baseParam == null)
            return;

        List<Param.Row> rows = curView.Selection.GetSelectedRows();

        var paramMeta = curView.GetParamData().GetParamMeta(baseParam.AppliedParamdef);

        var commands = new List<string>();

        foreach (Param.Row row in rows)
        {
            var command = $"param {curParamKey}: id {row.ID}: Name: = ";

            if (type is ParamRowNameAdjustType.Prepend)
            {
                command = $"{command}{adjustment}{row.Name}";
            }
            if (type is ParamRowNameAdjustType.Postpend)
            {
                command = $"{command}{row.Name}{adjustment}";
            }
            if (type is ParamRowNameAdjustType.Remove)
            {
                command = $"{command}{row.Name.Replace(adjustment, "")}";
            }
            if (type is ParamRowNameAdjustType.Clear)
            {
                command = $"{command} ";
            }
            if (type is ParamRowNameAdjustType.Insert)
            {
                var newName = row.Name.Insert(insertIndex, adjustment);
                command = $"{command}{newName}";
            }

            commands.Add(command);
        }

        foreach (var entry in commands)
        {
            curView.MassEdit.ExecuteMassEdit(entry, curView.GetPrimaryBank(), curView.Selection);
        }

    }
    #endregion

    #region Replace String in Row Name
    public static void ReplaceStringInRowName(ParamEditorView curView, List<string> targetStrings, string replaceString)
    {
        if (targetStrings.Count == 0)
            return;

        string curParamKey = curView.Selection.GetActiveParam();

        if (string.IsNullOrEmpty(curParamKey))
            return;

        Param baseParam = curView.GetPrimaryBank().Params[curParamKey];

        if (baseParam == null)
            return;

        List<Param.Row> rows = curView.Selection.GetSelectedRows();

        var paramMeta = curView.GetParamData().GetParamMeta(baseParam.AppliedParamdef);

        var commands = new List<string>();

        foreach (Param.Row row in rows)
        {
            var curName = row.Name;

            foreach (var entry in targetStrings)
            {
                if (curName.Contains(entry))
                {
                    var newName = curName.Replace(entry, replaceString);
                    var command = $"param {curParamKey}: id {row.ID}: Name: = {newName}";
                    commands.Add(command);
                }
            }
        }

        var singleCommand = "";
        foreach (var entry in commands)
        {
            singleCommand = $"{singleCommand}{entry};\n";
        }

        curView.MassEdit.ExecuteMassEdit(singleCommand, curView.GetPrimaryBank(), curView.Selection);
    }

    #endregion
}
