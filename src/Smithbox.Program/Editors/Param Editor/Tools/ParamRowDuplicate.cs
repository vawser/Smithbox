using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

public partial class ParamTools
{
    #region Row Duplicate
    /// <summary>
    /// Standard row duplicate menu
    /// </summary>
    public void DisplayRowDuplicate()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // Duplicate Row
        if (ImGui.CollapsingHeader("Duplicate Row"))
        {
            UIHelper.WrappedText("Duplicate the selected rows.");
            UIHelper.WrappedText("");

            if (!Editor._activeView.Selection.RowSelectionExists())
            {
                UIHelper.WrappedText("You must select a row before you can use this action.");
                UIHelper.WrappedText("");
            }
            else
            {
                UIHelper.WrappedText("Amount to Duplicate:");

                DPI.ApplyInputWidth(windowWidth);
                ImGui.InputInt("##Amount", ref CFG.Current.Param_Toolbar_Duplicate_Amount);
                UIHelper.Tooltip("The number of times the current selection will be duplicated.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Duplicate Offset:");

                DPI.ApplyInputWidth(windowWidth);
                ImGui.InputInt("##Offset", ref CFG.Current.Param_Toolbar_Duplicate_Offset);
                UIHelper.Tooltip("The ID offset to apply when duplicating.\nSet to 0 for row indexed params to duplicate as expected.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Deep Copy:");
                UIHelper.Tooltip("If any of these options are enabled, then the tagged fields within the duplicated row will be affected by the duplication offset.\n\nThis lets you easily duplicate sets of rows where the fields tend to refer to other rows (e.g. bullets).");

                // Specific toggles
                ImGui.Checkbox("Affect Attack Field", ref CFG.Current.Param_Toolbar_Duplicate_AffectAttackField);
                UIHelper.Tooltip("Fields tagged as 'Attack' will have the offset applied to their value.\n\nExample: the Attack reference in a Bullet row.");

                ImGui.Checkbox("Affect Bullet Field", ref CFG.Current.Param_Toolbar_Duplicate_AffectBulletField);
                UIHelper.Tooltip("Fields tagged as 'Bullet' will have the offset applied to their value.\n\nExample: the Bullet references in a Bullet row.");

                ImGui.Checkbox("Affect Behavior Field", ref CFG.Current.Param_Toolbar_Duplicate_AffectBehaviorField);
                UIHelper.Tooltip("Fields tagged as 'Behavior' will have the offset applied to their value.\n\nExamples: the Reference ID field in a BehaviorParam row.");

                ImGui.Checkbox("Affect SpEffect Field", ref CFG.Current.Param_Toolbar_Duplicate_AffectSpEffectField);
                UIHelper.Tooltip("Fields tagged as 'SpEffect' will have the offset applied to their value.\n\nExample: the SpEffect references in a Bullet row.");

                ImGui.Checkbox("Affect Equipment Origin Field", ref CFG.Current.Param_Toolbar_Duplicate_AffectSourceField);
                UIHelper.Tooltip("Fields tagged as 'Source' will have the offset applied to their value.\n\nExamples: the Source ID references in an EquipParamProtector row.");


                if (ImGui.Button("Duplicate##duplicateRow", DPI.WholeWidthButton(windowWidth, 24)))
                {
                    DuplicateRow();
                }
            }
        }
    }

    /// <summary>
    /// Standard row duplicate
    /// </summary>
    public void DuplicateRow(bool wholeTableGroupDuplicate = false)
    {
        var curParamKey = Editor._activeView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param param = Editor.Project.ParamData.PrimaryBank.Params[curParamKey];
        List<Param.Row> rows = Editor._activeView.Selection.GetSelectedRows();

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

        var IsInTableGroupMode = Editor._activeView.TableGroupView.IsInTableGroupMode(curParamKey);

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
                var dupeAction = new AddParamsAction(Editor, param, "legacystring", rowsToInsert, true, false, insertIndex, CFG.Current.Param_Toolbar_Duplicate_Offset, true);
                actions.Add(dupeAction);
            }

            var compoundAction = new CompoundAction(actions);
            compoundAction.SetPostExecutionAction(undo =>
            {
                var curParam = Editor._activeView.Selection.GetActiveParam();

                if (Editor._activeView.TableGroupView.IsInTableGroupMode(curParam))
                {
                    var curGroup = Editor._activeView.TableGroupView.CurrentTableGroup;
                    Editor._activeView.TableGroupView.UpdateTableGroupSelection(curGroup);
                }
            });

            Editor.EditorActionManager.ExecuteAction(compoundAction);
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

            if(wholeTableGroupDuplicate)
            {
                foreach(var row in rowsToInsert)
                {
                    row.ID = row.ID + CFG.Current.Param_Toolbar_Duplicate_Offset;
                }
            }

            List<EditorAction> actions = new List<EditorAction>();

            for (int i = 0; i < CFG.Current.Param_Toolbar_Duplicate_Amount; i++)
            {
                var dupeAction = new AddRowsToTableGroup(Editor, param, rowsToInsert, insertIndex, false, true);

                actions.Add(dupeAction);
            }

            var compoundAction = new CompoundAction(actions);
            compoundAction.SetPostExecutionAction(undo =>
            {
                var curParam = Editor._activeView.Selection.GetActiveParam();

                if (Editor._activeView.TableGroupView.IsInTableGroupMode(curParam))
                {
                    var curGroup = Editor._activeView.TableGroupView.CurrentTableGroup;
                    Editor._activeView.TableGroupView.UpdateTableGroupSelection(curGroup);
                }
            });

            Editor.EditorActionManager.ExecuteAction(compoundAction);
        }
    }

    #endregion

    #region Commutative Row Duplicate
    /// <summary>
    /// Commutative row duplicate menu
    /// </summary>
    public void DisplayCommutativeRowDuplicate()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Duplicate Row to Commutative Param"))
        {
            UIHelper.WrappedText("Duplicate the selected rows to another param that shares the same underlying structure.");
            UIHelper.WrappedText("");

            if (!Editor._activeView.Selection.RowSelectionExists())
            {
                UIHelper.WrappedText("You must select a row before you can use this action.");
                UIHelper.WrappedText("");
            }
            else
            {
                UIHelper.WrappedText("Offset:");

                DPI.ApplyInputWidth(windowWidth);
                ImGui.InputInt("##Offset", ref CFG.Current.Param_Toolbar_CommutativeDuplicate_Offset);
                UIHelper.Tooltip("The ID offset to apply when duplicating.\nSet to 0 for row indexed params to duplicate as expected.");
                UIHelper.WrappedText("");

                ImGui.Checkbox("Replace Rows in Target Param", ref CFG.Current.Param_Toolbar_CommutativeDuplicate_ReplaceExistingRows);
                UIHelper.Tooltip("If enabled, rows in the target will be overwritten when duplicating into a commutative param.");

                var curParamKey = Editor._activeView.Selection.GetActiveParam();

                if (curParamKey == null)
                    return;

                Param param = Editor.Project.ParamData.PrimaryBank.Params[curParamKey];

                if (Editor.Project.ParamData.CommutativeParamGroups.Groups.Where(e => e.Params.Contains(curParamKey)).Any())
                {
                    var targetGroup = Editor.Project.ParamData.CommutativeParamGroups.Groups.Where(e => e.Params.Contains(curParamKey)).FirstOrDefault();

                    if (targetGroup == null)
                        return;

                    ImGui.Text("");
                    UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Param:");

                    foreach (var entry in targetGroup.Params)
                    {
                        // Ignore current param
                        if (entry == curParamKey)
                            continue;

                        if (ImGui.Selectable($"{entry}", CFG.Current.Param_Toolbar_CommutativeDuplicate_Target == entry))
                        {
                            CFG.Current.Param_Toolbar_CommutativeDuplicate_Target = entry;
                        }
                    }

                    ImGui.Text("");
                }

                if (ImGui.Button("Duplicate##duplicateRow", DPI.WholeWidthButton(windowWidth, 24)))
                {
                    DuplicateCommutativeRow();
                }
            }
        }
    }

    public bool IsCommutativeParam()
    {
        var isValid = false;

        var paramName = Editor._activeView.Selection.GetActiveParam();

        if (paramName == null)
            return false;

        Param param = Editor.Project.ParamData.PrimaryBank.Params[paramName];

        if (Editor.Project.ParamData.CommutativeParamGroups.Groups == null)
            return false;

        if (Editor.Project.ParamData.CommutativeParamGroups.Groups.Where(e => e.Params.Contains(paramName)).Any())
        {
            isValid = true;
        }

        return isValid;
    }

    /// <summary>
    /// Menu element for the dropdown usage
    /// </summary>
    public void DisplayCommutativeDropDownMenu()
    {
        var curParamKey = Editor._activeView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param param = Editor.Project.ParamData.PrimaryBank.Params[curParamKey];

        if (Editor.Project.ParamData.CommutativeParamGroups.Groups.Where(e => e.Params.Contains(curParamKey)).Any())
        {
            var targetGroup = Editor.Project.ParamData.CommutativeParamGroups.Groups.Where(e => e.Params.Contains(curParamKey)).FirstOrDefault();

            if (targetGroup == null)
                return;

            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Param:");

            foreach (var entry in targetGroup.Params)
            {
                // Ignore current param
                if (entry == curParamKey)
                    continue;

                if (ImGui.MenuItem($"{entry}"))
                {
                    CFG.Current.Param_Toolbar_CommutativeDuplicate_Target = entry;
                    DuplicateCommutativeRow();
                }
            }
        }
    }

    /// <summary>
    /// Commutative row duplicate
    /// </summary>
    public void DuplicateCommutativeRow()
    {
        var curParamKey = Editor._activeView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        var targetParamName = CFG.Current.Param_Toolbar_CommutativeDuplicate_Target;

        if (targetParamName == "")
            return;

        if (curParamKey == targetParamName)
            return;

        if (!Editor.Project.ParamData.PrimaryBank.Params.ContainsKey(curParamKey))
            return;

        Param currentParam = Editor.Project.ParamData.PrimaryBank.Params[curParamKey];

        if (!Editor.Project.ParamData.PrimaryBank.Params.ContainsKey(targetParamName))
            return;

        Param targetParam = Editor.Project.ParamData.PrimaryBank.Params[targetParamName];

        if (targetParam == null)
            return;

        List<Param.Row> selectedRows = Editor._activeView.Selection.GetSelectedRows();

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

        if (CFG.Current.Param_Toolbar_Duplicate_Offset == 0)
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

        actions.Add(new AddParamsAction(Editor, targetParam, "legacystring", rowsToInsert, false, CFG.Current.Param_Toolbar_CommutativeDuplicate_ReplaceExistingRows, insertIndex, CFG.Current.Param_Toolbar_CommutativeDuplicate_Offset, false));

        var compoundAction = new CompoundAction(actions);

        Editor.EditorActionManager.ExecuteAction(compoundAction);
    }
    #endregion
}