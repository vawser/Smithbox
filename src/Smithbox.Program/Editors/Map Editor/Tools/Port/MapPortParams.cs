using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Port;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Applies a port's param rows (<see cref="PortParamPlan"/>) through the Param Editor: one undoable edit in its
/// loaded params, so the rows show there as modified and are written to regulation.bin when the params are saved,
/// with the user's other param edits rather than over them.
/// </summary>
public static class MapPortParams
{
    /// <summary>
    /// The Param Editor and its loaded param of this name. False when the project has no Param Editor or its params
    /// haven't finished loading.
    /// </summary>
    public static bool TryGetParam(ProjectEntry project, string paramName, out ParamEditorScreen editor, out Param param)
    {
        editor = project.Handler.ParamEditor;
        param = null;

        var bank = project.Handler.ParamData?.PrimaryBank;
        if (editor == null || bank?.Params == null)
            return false;

        return bank.Params.TryGetValue(paramName, out param) && param != null;
    }

    /// <summary>The AEG groups (row ID / 1000) the loaded asset param has rows in, for the port to avoid.</summary>
    public static HashSet<int> AssetParamGroups(ProjectEntry project)
    {
        return TryGetParam(project, PortObjects.AssetParamName, out _, out var param)
            ? param.Rows.Select(r => r.ID / 1000).ToHashSet()
            : new HashSet<int>();
    }

    /// <summary>
    /// Builds each planned row from its template row in the loaded param and adds them as one action in the Param
    /// Editor's history (rows with a planned ID are replaced). Problems go to the port's log. Main thread only.
    /// Returns the number of rows applied.
    /// </summary>
    public static int Apply(ProjectEntry project, PortParamPlan plan, PortLog log)
    {
        if (plan == null || plan.Rows.Count == 0)
            return 0;

        log.Line();
        log.Line($"== Params (through the Param Editor)");

        if (!TryGetParam(project, plan.ParamName, out var editor, out var param))
        {
            log.Fail(LOC.Get("MAP_Port_Params_Unavailable", plan.ParamName, plan.Rows.Count));
            return 0;
        }

        var rows = new List<Param.Row>();
        int values = 0, skippedFields = 0;

        foreach (var planned in plan.Rows)
        {
            var template = param[planned.TemplateId];
            if (template == null)
            {
                log.Fail(LOC.Get("MAP_Port_Params_No_Template", plan.ParamName, planned.TemplateId, planned.Id));
                continue;
            }

            var row = new Param.Row(template) { ID = planned.Id, Name = planned.Name };

            foreach (var (field, value) in planned.Values)
            {
                if (row[field] is not { } cell || value == null)
                {
                    skippedFields++;
                    continue;
                }

                try
                {
                    cell.SetValue(Convert.ChangeType(value, cell.Value.GetType()));
                    values++;
                }
                catch (Exception e) when (e is InvalidCastException or FormatException or OverflowException)
                {
                    skippedFields++;
                }
            }

            rows.Add(row);
        }

        if (rows.Count == 0)
            return 0;

        var action = new AddParamsAction(editor, param, plan.ParamName, rows, appendOnly: false, replaceParams: true)
        {
            DisplayName = LOC.Get("MAP_Port_Params_Action", rows.Count, plan.ParamName)
        };

        editor.ActionManager.ExecuteAction(action);

        // Each planned ID should now be in the param once
        var missing = rows.Count(r => param[r.ID] == null);
        log.Check(missing == 0, $"{plan.ParamName}: {missing} planned rows are missing after the edit");

        log.Line($"{plan.ParamName}: {rows.Count} rows added or replaced in the Param Editor, {values} DS3 values"
            + (skippedFields > 0 ? $", {skippedFields} fields ER doesn't have skipped" : "")
            + ". Save the params to write them to regulation.bin.");

        return rows.Count;
    }
}
