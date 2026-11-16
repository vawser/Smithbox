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

        // In param order (the selection is in click order), so the copies keep the rows' order and "after the
        // selection" means after the bottom-most selected row
        List<Param.Row> rows = curView.Selection.GetSelectedRows()
            .Where(r => r != null && param.IndexOfRow(r) >= 0)
            .OrderBy(r => param.IndexOfRow(r))
            .ToList();

        if (rows.Count == 0)
        {
            return;
        }

        int insertIndex = -1;
        var offset = CFG.Current.Param_Toolbar_Duplicate_Offset;
        var amount = CFG.Current.Param_Toolbar_Duplicate_Amount;

        var IsInTableGroupMode = curView.ParamTableWindow.IsInTableGroupMode(curParamKey);

        // Default Behavior
        if (!IsInTableGroupMode)
        {
            DuplicateRows(curView.Editor, param, rows, undo =>
            {
                var curParam = curView.Selection.GetActiveParam();

                if (curView.ParamTableWindow.IsInTableGroupMode(curParam))
                {
                    var curGroup = curView.ParamTableWindow.CurrentTableGroupID;
                    curView.ParamTableWindow.UpdateTableGroupSelection(curGroup);
                }
            });
        }
        // Row Group Mode behavior
        else
        {
            insertIndex = param.IndexOfRow(rows.Last()) + 1;

            List<EditorAction> actions = new List<EditorAction>();

            for (int i = 0; i < amount; i++)
            {
                // Copy i of a whole group gets the group ID + (i + 1) * offset, so each copy is its own group
                // (adding the offset once gave every copy the same group ID). Rows duplicated within a group
                // keep the group ID.
                var idShift = wholeTableGroupDuplicate ? (i + 1) * offset : 0;

                List<Param.Row> rowsToInsert = new();

                foreach (Param.Row r in rows)
                {
                    Param.Row newrow = new(r);
                    newrow.ID = r.ID + idShift;
                    rowsToInsert.Add(newrow);
                }

                // Each row is inserted at the same index, so insert the last one first
                rowsToInsert.Reverse();

                // Each copy goes after the previous ones, so the groups appear in order
                var copyIndex = insertIndex + i * rows.Count;

                var dupeAction = new AddRowsToTableGroup(curView.Editor, param, rowsToInsert, copyIndex, false, true);

                actions.Add(dupeAction);
            }

            // A whole-group duplicate selects the first copy, and its undo goes back to the source group.
            // Rows duplicated within a group stay in it.
            var sourceGroup = rows[0].ID;
            var firstCopyGroup = sourceGroup + offset;

            var compoundAction = new CompoundAction(actions);
            compoundAction.SetPostExecutionAction(undo =>
            {
                var curParam = curView.Selection.GetActiveParam();

                if (curParam != curParamKey || !curView.ParamTableWindow.IsInTableGroupMode(curParam))
                    return;

                if (wholeTableGroupDuplicate && amount > 0)
                {
                    curView.ParamTableWindow.UpdateTableGroupSelection(undo ? sourceGroup : firstCopyGroup);
                    curView.ParamTableWindow.RequestScrollToGroup();
                }
                else
                {
                    var curGroup = curView.ParamTableWindow.CurrentTableGroupID;
                    curView.ParamTableWindow.UpdateTableGroupSelection(curGroup);
                }
            });

            curView.Editor.ActionManager.ExecuteAction(compoundAction);
        }
    }

    /// <summary>
    /// Duplicates the given rows of <paramref name="param"/> with the duplicate offset and amount (and reference
    /// offsets) set in the Duplicate menu, as one undoable step. Not for table-group mode, where a row's ID is its
    /// group. Returns the copies, in param order, or an empty list if nothing was duplicated.
    /// </summary>
    public static List<Param.Row> DuplicateRows(ParamEditorScreen editor, Param param, IEnumerable<Param.Row> selectedRows,
        Action<bool> postExecutionAction = null)
    {
        // In param order (a selection is usually in click order), so the copies keep the rows' order and "after the
        // selection" means after the bottom-most selected row
        List<Param.Row> rows = selectedRows
            .Where(r => r != null && param.IndexOfRow(r) >= 0)
            .Distinct()
            .OrderBy(r => param.IndexOfRow(r))
            .ToList();

        if (rows.Count == 0)
            return new List<Param.Row>();

        var insertIndex = -1;
        var offset = CFG.Current.Param_Toolbar_Duplicate_Offset;
        var amount = CFG.Current.Param_Toolbar_Duplicate_Amount;

        // AddParamsAction clones the rows itself, so the selected rows are passed as they are
        List<Param.Row> rowsToInsert = new(rows);

        if (offset == 0)
        {
            insertIndex = param.IndexOfRow(rows.Last()) + 1;

            // Each row is inserted at the same index, so insert the last one first
            rowsToInsert.Reverse();
        }

        List<AddParamsAction> dupeActions = new();

        // Each copy finds the next free ID after the previous copies (ID + offset, ID + 2 * offset, ...), and
        // AddParamsAction offsets the copy's reference fields by the same amount its ID moved
        for (int i = 0; i < amount; i++)
        {
            dupeActions.Add(new AddParamsAction(editor, param, "legacystring", rowsToInsert, true, false, insertIndex, offset, true));
        }

        var compoundAction = new CompoundAction(dupeActions.Cast<EditorAction>().ToList());

        if (postExecutionAction != null)
            compoundAction.SetPostExecutionAction(postExecutionAction);

        // Refused while a save is running
        if (!editor.ActionManager.ExecuteAction(compoundAction))
            return new List<Param.Row>();

        return dupeActions
            .SelectMany(action => action.GetResultantRows())
            .Where(row => param.IndexOfRow(row) >= 0)
            .OrderBy(row => param.IndexOfRow(row))
            .ToList();
    }
    #endregion

    #region Reference Offsets
    // The DeepCopyTarget kinds in the param meta, each with its option. A duplicate offsets the reference fields
    // of every ticked kind by the duplicate offset, so the copy points at the matching copies of its attacks etc.
    private const string Target_Attack = "Attack";
    private const string Target_Bullet = "Bullet";
    private const string Target_Behavior = "Behavior";
    private const string Target_SpEffect = "SpEffect";
    private const string Target_Source = "Source";

    /// <summary>
    /// The DeepCopyTarget kinds whose option is ticked
    /// </summary>
    public static HashSet<string> GetEnabledDeepCopyTargets()
    {
        var enabled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (CFG.Current.Param_Toolbar_Duplicate_AffectAttackField)
            enabled.Add(Target_Attack);

        if (CFG.Current.Param_Toolbar_Duplicate_AffectBulletField)
            enabled.Add(Target_Bullet);

        if (CFG.Current.Param_Toolbar_Duplicate_AffectBehaviorField)
            enabled.Add(Target_Behavior);

        if (CFG.Current.Param_Toolbar_Duplicate_AffectSpEffectField)
            enabled.Add(Target_SpEffect);

        if (CFG.Current.Param_Toolbar_Duplicate_AffectSourceField)
            enabled.Add(Target_Source);

        return enabled;
    }

    /// <summary>
    /// The active param's fields that have a DeepCopyTarget, with their target kinds
    /// </summary>
    public static List<(string Field, List<string> Targets)> GetDeepCopyFields(ParamEditorView curView)
    {
        var paramKey = curView.Selection.GetActiveParam();

        if (paramKey == null || !curView.GetPrimaryBank().Params.TryGetValue(paramKey, out var param))
            return new List<(string, List<string>)>();

        return GetDeepCopyFields(curView.GetParamData(), param);
    }

    /// <summary>
    /// The fields of <paramref name="param"/> that have a DeepCopyTarget, with their target kinds
    /// </summary>
    public static List<(string Field, List<string> Targets)> GetDeepCopyFields(ParamData paramData, Param param)
    {
        var result = new List<(string, List<string>)>();

        if (param?.AppliedParamdef == null)
            return result;

        var paramMeta = paramData.GetParamMeta(param.AppliedParamdef);

        if (paramMeta == null)
            return result;

        foreach (var fieldDef in param.AppliedParamdef.Fields)
        {
            var fieldMeta = paramData.GetParamFieldMeta(paramMeta, fieldDef);

            if (fieldMeta?.DeepCopyTargetType == null || fieldMeta.DeepCopyTargetType.Count == 0)
                continue;

            result.Add((fieldDef.InternalName, fieldMeta.DeepCopyTargetType.Select(t => t.Trim()).ToList()));
        }

        return result;
    }

    /// <summary>
    /// The reference-offset options for the active param (only the kinds it has), and a preview of the fields a
    /// duplicate will change. Drawn in the Duplicate menu.
    /// </summary>
    public static void DisplayReferenceOffsetOptions(ParamEditorView curView)
    {
        DisplayReferenceOffsetOptions(GetDeepCopyFields(curView));
    }

    /// <inheritdoc cref="DisplayReferenceOffsetOptions(ParamEditorView)"/>
    public static void DisplayReferenceOffsetOptions(ParamData paramData, Param param)
    {
        DisplayReferenceOffsetOptions(GetDeepCopyFields(paramData, param));
    }

    private static void DisplayReferenceOffsetOptions(List<(string Field, List<string> Targets)> fields)
    {
        ImGui.Separator();
        ImGui.TextDisabled(LOC.Get("PARAM_Duplicate_Reference_Header"));

        if (fields.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("PARAM_Duplicate_Reference_None"));
            return;
        }

        var kinds = new HashSet<string>(fields.SelectMany(f => f.Targets), StringComparer.OrdinalIgnoreCase);

        if (kinds.Contains(Target_Attack))
            ImGui.Checkbox($"{LOC.Get("PARAM_Duplicate_Reference_Attack")}##dupRefAttack", ref CFG.Current.Param_Toolbar_Duplicate_AffectAttackField);

        if (kinds.Contains(Target_Bullet))
            ImGui.Checkbox($"{LOC.Get("PARAM_Duplicate_Reference_Bullet")}##dupRefBullet", ref CFG.Current.Param_Toolbar_Duplicate_AffectBulletField);

        if (kinds.Contains(Target_Behavior))
            ImGui.Checkbox($"{LOC.Get("PARAM_Duplicate_Reference_Behavior")}##dupRefBehavior", ref CFG.Current.Param_Toolbar_Duplicate_AffectBehaviorField);

        if (kinds.Contains(Target_SpEffect))
            ImGui.Checkbox($"{LOC.Get("PARAM_Duplicate_Reference_SpEffect")}##dupRefSpEffect", ref CFG.Current.Param_Toolbar_Duplicate_AffectSpEffectField);

        if (kinds.Contains(Target_Source))
            ImGui.Checkbox($"{LOC.Get("PARAM_Duplicate_Reference_Source")}##dupRefSource", ref CFG.Current.Param_Toolbar_Duplicate_AffectSourceField);

        var enabled = GetEnabledDeepCopyTargets();
        var affected = fields.Where(f => f.Targets.Any(enabled.Contains)).Select(f => f.Field).ToList();

        if (affected.Count == 0)
            return;

        var offset = CFG.Current.Param_Toolbar_Duplicate_Offset;

        if (offset == 0)
        {
            ImGui.TextDisabled(LOC.Get("PARAM_Duplicate_Reference_Offset_Zero"));
            return;
        }

        ImGui.TextDisabled(LOC.Get("PARAM_Duplicate_Reference_Preview", offset));

        foreach (var field in affected)
        {
            ImGui.BulletText(field);
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

        if (curView.GetParamData().ParamCommutativityGroups.Groups == null)
            return false;

        if (curView.GetParamData().ParamCommutativityGroups.Groups
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

        if (curView.GetParamData().ParamCommutativityGroups.Groups
            .Where(e => e.Params.Contains(curParamKey)).Any())
        {
            var targetGroup = curView.GetParamData().ParamCommutativityGroups.Groups
                .Where(e => e.Params.Contains(curParamKey)).FirstOrDefault();

            if (targetGroup == null)
                return;

            GUI.WrappedText(LOC.Get("PARAM_DuplicateTool_Target_Param"));

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

        List<Param.Row> selectedRows = curView.Selection.GetSelectedRows()?.Where(r => r != null).ToList();

        if (selectedRows == null || selectedRows.Count == 0)
        {
            return;
        }

        List<Param.Row> rowsToInsert = new();

        foreach (Param.Row r in selectedRows)
        {
            Param.Row newrow = new(r, targetParam);
            rowsToInsert.Add(newrow);
        }

        // The copies always go into the target by ID (-1 = no fixed index). An index taken from the source param
        // meant nothing in the target, and threw when the target had fewer rows. With offset 0 the copy keeps its
        // ID: an existing row with that ID is replaced if "Replace existing rows" is on, otherwise the copy takes
        // the next free ID after it.
        int insertIndex = -1;

        List<EditorAction> actions = new List<EditorAction>();

        var offset = CFG.Current.Param_Toolbar_CommutativeDuplicate_Offset;

        var replaceExisting = CFG.Current.Param_Toolbar_CommutativeDuplicate_ReplaceExistingRows;

        actions.Add(new AddParamsAction(curView.Editor, targetParam, "legacystring", rowsToInsert, false, replaceExisting, insertIndex, offset, false));

        var compoundAction = new CompoundAction(actions);

        curView.Editor.ActionManager.ExecuteAction(compoundAction);
    }

    #endregion
}
