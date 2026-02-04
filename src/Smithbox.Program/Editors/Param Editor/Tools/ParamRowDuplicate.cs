using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public static class ParamRowDuplicate
{
    #region Duplicate
    public static void ApplyDuplicate(ParamEditorView curView, bool wholeTableGroupDuplicate = false)
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

        List<Param.Row> rowsToInsert = new();
        int insertIndex = -1;

        foreach (Param.Row r in rows)
        {
            Param.Row newrow = new(r);
            rowsToInsert.Add(newrow);
        }

        var IsInTableGroupMode = curView.ParamTableWindow.IsInTableGroupMode(curParamKey);

        // Default Behavior
        if (!IsInTableGroupMode)
        {
            if (CFG.Current.Param_Toolbar_Duplicate_Offset == 0)
            {
                var lastRow = rows.Last();

                for (int i = 0; i < param.Rows.Count; i++)
                {
                    var curRow = param.Rows[i];

                    if (lastRow == curRow)
                    {
                        insertIndex = i + 1;
                        break;
                    }
                }

                rowsToInsert.Reverse();
            }

            List<EditorAction> actions = new List<EditorAction>();

            for (int i = 0; i < CFG.Current.Param_Toolbar_Duplicate_Amount; i++)
            {
                var dupeAction = new AddParamsAction(curView.Editor, param, "legacystring", rowsToInsert, true, false, insertIndex, CFG.Current.Param_Toolbar_Duplicate_Offset, true);
                actions.Add(dupeAction);
            }

            var compoundAction = new CompoundAction(actions);
            compoundAction.SetPostExecutionAction(undo =>
            {
                var curParam = curView.Selection.GetActiveParam();

                if (curView.ParamTableWindow.IsInTableGroupMode(curParam))
                {
                    var curGroup = curView.ParamTableWindow.CurrentTableGroup;
                    curView.ParamTableWindow.UpdateTableGroupSelection(curGroup);
                }
            });

            curView.Editor.ActionManager.ExecuteAction(compoundAction);
        }
        // Row Group Mode behavior
        else
        {
            var lastRow = rows.Last();

            for (int i = 0; i < param.Rows.Count; i++)
            {
                var curRow = param.Rows[i];

                if (lastRow == curRow)
                {
                    insertIndex = i + 1;
                    break;
                }
            }

            rowsToInsert.Reverse();

            if (wholeTableGroupDuplicate)
            {
                foreach (var row in rowsToInsert)
                {
                    row.ID = row.ID + CFG.Current.Param_Toolbar_Duplicate_Offset;
                }
            }

            List<EditorAction> actions = new List<EditorAction>();

            for (int i = 0; i < CFG.Current.Param_Toolbar_Duplicate_Amount; i++)
            {
                var dupeAction = new AddRowsToTableGroup(curView.Editor, param, rowsToInsert, insertIndex, false, true);

                actions.Add(dupeAction);
            }

            var compoundAction = new CompoundAction(actions);
            compoundAction.SetPostExecutionAction(undo =>
            {
                var curParam = curView.Selection.GetActiveParam();

                if (curView.ParamTableWindow.IsInTableGroupMode(curParam))
                {
                    var curGroup = curView.ParamTableWindow.CurrentTableGroup;
                    curView.ParamTableWindow.UpdateTableGroupSelection(curGroup);
                }
            });

            curView.Editor.ActionManager.ExecuteAction(compoundAction);
        }
    }
    #endregion

    #region Commutative Duplicate
    public static bool IsCommutativeParam(ParamEditorView curView)
    {
        var isValid = false;

        var paramName = curView.Selection.GetActiveParam();

        if (paramName == null)
            return false;

        Param param = curView.GetPrimaryBank().Params[paramName];

        if (curView.GetParamData().CommutativeParamGroups.Groups == null)
            return false;

        if (curView.GetParamData().CommutativeParamGroups.Groups
            .Where(e => e.Params.Contains(paramName)).Any())
        {
            isValid = true;
        }

        return isValid;
    }

    public static void ApplyCommutativeDuplicate(ParamEditorView curView)
    {
        var curParamKey = curView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param param = curView.GetPrimaryBank().Params[curParamKey];

        if (curView.GetParamData().CommutativeParamGroups.Groups
            .Where(e => e.Params.Contains(curParamKey)).Any())
        {
            var targetGroup = curView.GetParamData().CommutativeParamGroups.Groups
                .Where(e => e.Params.Contains(curParamKey)).FirstOrDefault();

            if (targetGroup == null)
                return;

            UIHelper.WrappedText("Target Param:");

            foreach (var entry in targetGroup.Params)
            {
                // Ignore current param
                if (entry == curParamKey)
                    continue;

                if (ImGui.MenuItem($"{entry}"))
                {
                    CFG.Current.Param_Toolbar_CommutativeDuplicate_Target = entry;

                    DuplicateCommutativeRow(curView);
                }
            }
        }
    }
    private static void DuplicateCommutativeRow(ParamEditorView curView)
    {
        var curParamKey = curView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        var targetParamName = CFG.Current.Param_Toolbar_CommutativeDuplicate_Target;

        if (targetParamName == "")
            return;

        if (curParamKey == targetParamName)
            return;

        if (!curView.GetPrimaryBank().Params.ContainsKey(curParamKey))
            return;

        Param currentParam = curView.GetPrimaryBank().Params[curParamKey];

        if (!curView.GetPrimaryBank().Params.ContainsKey(targetParamName))
            return;

        Param targetParam = curView.GetPrimaryBank().Params[targetParamName];

        if (targetParam == null)
            return;

        List<Param.Row> selectedRows = curView.Selection.GetSelectedRows();

        if (selectedRows.Count == 0)
        {
            return;
        }

        List<Param.Row> rowsToInsert = new();

        foreach (Param.Row r in selectedRows)
        {
            Param.Row newrow = new(r, targetParam);
            rowsToInsert.Add(newrow);
        }

        int insertIndex = -1;

        if (CFG.Current.Param_Toolbar_CommutativeDuplicate_Offset == 0)
        {
            var lastRow = selectedRows.Last();

            for (int i = 0; i < currentParam.Rows.Count; i++)
            {
                var curRow = currentParam.Rows[i];

                if (lastRow == curRow)
                {
                    insertIndex = i + 1;
                    break;
                }
            }

            rowsToInsert.Reverse();
        }

        List<EditorAction> actions = new List<EditorAction>();

        var offset = CFG.Current.Param_Toolbar_CommutativeDuplicate_Offset;

        var replaceExisting = CFG.Current.Param_Toolbar_CommutativeDuplicate_ReplaceExistingRows;

        actions.Add(new AddParamsAction(curView.Editor, targetParam, "legacystring", rowsToInsert, false, replaceExisting, insertIndex, offset, false));

        var compoundAction = new CompoundAction(actions);

        curView.Editor.ActionManager.ExecuteAction(compoundAction);
    }

    #endregion
}
