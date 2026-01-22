using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
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
    #region Set Row to Default
    public static void SetRowToDefault(ParamView curView)
    {
        var curParamKey = curView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param baseParam = curView.GetPrimaryBank().Params[curParamKey];
        Param vanillaParam = curView.GetVanillaBank().Params[curParamKey];

        if (baseParam == null)
            return;

        if (vanillaParam == null)
            return;

        List<Param.Row> rows = curView.Selection.GetSelectedRows();

        List<Param.Row> rowsToInsert = new();

        foreach (Param.Row bRow in rows)
        {
            foreach (var vRow in vanillaParam.Rows)
            {
                if (vRow.ID == bRow.ID)
                {
                    Param.Row newrow = new(vRow, baseParam);
                    newrow.Name = bRow.Name; // Keep the current name
                    rowsToInsert.Add(newrow);
                }
            }
        }

        List<EditorAction> actions = new List<EditorAction>();

        actions.Add(new AddParamsAction(curView.Editor, baseParam, "legacystring", rowsToInsert, false, true, -1, 0, false));

        var compoundAction = new CompoundAction(actions);

        curView.Editor.ActionManager.ExecuteAction(compoundAction);
    }
    #endregion

    #region Copy Row Details
    public static void CopyRowDetails(ParamView curView, bool includeName = false)
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
    public static void ProliferateRowName(ParamView curView, string targetField)
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
                curView, fieldMeta.RefTypes, row, targetCell.Value);

            foreach ((string, Param.Row, string) rf in refs)
            {
                if (row == null || curView.Editor.ActionManager == null)
                {
                    continue;
                }

                rf.Item2.Name = row.Name;
            }
        }

        if (displayWarning)
        {
            TaskLogs.AddLog($"Failed to find field with internal name of: {targetField}");
        }
    }
    #endregion

    #region Inherit Row Name
    public static void InheritRowName(ParamView curView, string targetField)
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

            List<(string, Param.Row, string)> refs = ParamReferenceResolver.ResolveParamReferences(curView, fieldMeta.RefTypes, row, targetCell.Value);

            foreach ((string, Param.Row, string) rf in refs)
            {
                if (row == null || curView.Editor.ActionManager == null)
                {
                    continue;
                }

                row.Name = rf.Item2.Name;
            }
        }

        if (displayWarning)
        {
            TaskLogs.AddLog($"Failed to find field with internal name of: {targetField}");
        }
    }
    #endregion

    #region Inherit Row Name from FMG
    public static void InheritRowNameFromFMG(ParamView curView, string targetField)
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

                row.Name = result.Entry.Text;
            }
        }

        if (displayWarning)
        {
            TaskLogs.AddLog($"Failed to find field with internal name of: {targetField}");
        }
    }
    #endregion

    #region Inherit Row Name from Alias
    public static void InheritRowNameFromAlias(ParamView curView, string targetField)
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
                        row.Name = entry.Name;
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
                        row.Name = entry.Name;
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
                        row.Name = entry.Name;
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
                        row.Name = entry.Name;
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
                        row.Name = entry.Name;
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
                        row.Name = entry.Name;
                        break;
                    }
                }
            }
        }

        if (displayWarning)
        {
            TaskLogs.AddLog($"Failed to find field with internal name of: {targetField}");
        }
    }
    #endregion

    #region Adjust Row Name
    public static void AdjustRowName(ParamView curView, string adjustment, ParamRowNameAdjustType type)
    {
        if (string.IsNullOrEmpty(adjustment))
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
            var command = $"selection: Name: = ";

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

            commands.Add(command);
        }

        foreach (var entry in commands)
        {
            curView.MassEdit.ExecuteMassEdit(entry, curView.GetPrimaryBank(), curView.Selection);
        }

    }
    #endregion
}
