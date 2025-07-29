using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using Octokit;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Decorators;
using StudioCore.Editors.TextEditor.Utils;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor.Tools;

public partial class ParamTools
{
    public TargetType CurrentTargetCategory = TargetType.SelectedParam;

    #region Row Name Trimmer
    public void DisplayRowNameTrimmer()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);

        if (ImGui.CollapsingHeader("Trim Row Names"))
        {
            UIHelper.WrappedText("Trim Carriage Return (\\r) characters from row names\nfor the currently selected param, or for all params.");
            UIHelper.WrappedText("");

            if (!Editor._activeView.Selection.ActiveParamExists())
            {
                UIHelper.WrappedText("You must select a param before you can use this action.");
                UIHelper.WrappedText("");
            }
            else
            {
                ParamTargetElement(ref CurrentTargetCategory, "The target for the Row Name trimming.", DPI.WholeWidthButton(windowWidth, 24));

                if (ImGui.Button("Trim##action_TrimRowNames", DPI.WholeWidthButton(windowWidth, 24)))
                {
                    TrimRowNames();
                }
            }
        }
    }

    public enum RowTrimType
    {
        Whitespace,
        NewLines
    }

    public void TrimRowNames(RowTrimType trimType = RowTrimType.Whitespace)
    {
        var selectedParam = Editor._activeView.Selection;
        var curParamKey = selectedParam.GetActiveParam();

        if (curParamKey == null)
            return;

        if (trimType is RowTrimType.Whitespace)
        {
            if (selectedParam.ActiveParamExists())
            {
                if (Editor.Project.ParamData.PrimaryBank.Params != null)
                {
                    var activeParam = selectedParam.GetActiveParam();
                    var rows = selectedParam.GetSelectedRows();
                    switch (CurrentTargetCategory)
                    {
                        case TargetType.SelectedRows:
                            if (!rows.Any()) return;
                            TrimRowNameHelper(rows);
                            PlatformUtils.Instance.MessageBox($"Row names for {rows.Count} selected rows have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case TargetType.SelectedParam:
                            TrimRowNameHelper(activeParam);
                            PlatformUtils.Instance.MessageBox($"Row names for {activeParam} have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        case TargetType.AllParams:
                            foreach (var param in Editor.Project.ParamData.PrimaryBank.Params)
                            {
                                TrimRowNameHelper(param.Key);
                            }
                            PlatformUtils.Instance.MessageBox($"Row names for all params have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        if (trimType is RowTrimType.NewLines)
        {
            var activeParam = selectedParam.GetActiveParam();
            var rows = selectedParam.GetSelectedRows();

            foreach (Param.Row row in rows)
            {
                var newName = row.Name.Replace("\n", " ").Replace("\r", "");
                row.Name = newName;
            }
        }
    }

    private void TrimRowNameHelper(IEnumerable<Param.Row> rows)
    {
        foreach (Param.Row row in rows)
        {
            row.Name = row.Name.Trim();
        }
    }

    private void TrimRowNameHelper(string param)
    {
        Param p = Editor.Project.ParamData.PrimaryBank.Params[param];
        TrimRowNameHelper(p.Rows);
    }

    public void ParamTargetElement(ref TargetType currentTarget, string tooltip, Vector2 size)
    {
        var inputWidth = size.X;

        UIHelper.WrappedText("Target Category:");
        DPI.ApplyInputWidth(inputWidth);
        if (ImGui.BeginCombo("##Target", currentTarget.GetDisplayName()))
        {
            foreach (TargetType e in Enum.GetValues<TargetType>())
            {
                var name = e.GetDisplayName();
                if (ImGui.Selectable(name))
                {
                    currentTarget = e;
                    break;
                }
            }
            ImGui.EndCombo();
        }
        UIHelper.Tooltip(tooltip);
        UIHelper.WrappedText("");
    }
    #endregion

    #region Row Sorter
    public void DisplayRowSorter()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Sort Rows"))
        {
            UIHelper.WrappedText("Sort the rows for the currently selected param by their ID.");
            UIHelper.WrappedText("");

            if (ImGui.Button("Sort##action_SortRows", DPI.WholeWidthButton(windowWidth, 24)))
            {
                SortRows();
            }
        }
    }

    public void SortRows()
    {
        if (Project.ProjectType is Core.ProjectType.AC6)
        {
            var dialog = PlatformUtils.Instance.MessageBox("This action will delete rows if there are multiple rows with the same ID within this param. Do you want to proceed?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (dialog == DialogResult.OK)
            {
                if (Editor._activeView.Selection.ActiveParamExists())
                {
                    TaskLogs.AddLog($"Param rows sorted for {Editor._activeView.Selection.GetActiveParam()}");
                    Editor.EditorActionManager.ExecuteAction(MassParamEditOther.SortRows(Editor.Project, Editor.Project.ParamData.PrimaryBank, Editor._activeView.Selection.GetActiveParam()));
                }
            }
        }
        else
        {
            if (Editor._activeView.Selection.ActiveParamExists())
            {
                TaskLogs.AddLog($"Param rows sorted for {Editor._activeView.Selection.GetActiveParam()}");
                Editor.EditorActionManager.ExecuteAction(MassParamEditOther.SortRows(Editor.Project, Editor.Project.ParamData.PrimaryBank, Editor._activeView.Selection.GetActiveParam()));
            }
        }
    }
    #endregion

    #region Set Row to Default
    public void SetRowToDefault()
    {
        var curParamKey = Editor._activeView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param baseParam = Editor.Project.ParamData.PrimaryBank.Params[curParamKey];
        Param vanillaParam = Editor.Project.ParamData.VanillaBank.Params[curParamKey];

        if (baseParam == null)
            return;

        if (vanillaParam == null)
            return;

        List<Param.Row> rows = Editor._activeView.Selection.GetSelectedRows();

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

        actions.Add(new AddParamsAction(Editor, baseParam, "legacystring", rowsToInsert, false, true, -1, 0, false));

        var compoundAction = new CompoundAction(actions);

        Editor.EditorActionManager.ExecuteAction(compoundAction);
    }
    #endregion

    #region Copy Row Details
    public void CopyRowDetails(bool includeName = false)
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
    public void ProliferateRowName(string targetField)
    {
        if (targetField == null)
            return;

        var curParamKey = Editor._activeView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param baseParam = Editor.Project.ParamData.PrimaryBank.Params[curParamKey];

        if (baseParam == null)
            return;

        List<Param.Row> rows = Editor._activeView.Selection.GetSelectedRows();

        var paramMeta = Editor.Project.ParamData.GetParamMeta(baseParam.AppliedParamdef);

        var actions = new List<EditorAction>();

        foreach (Param.Row row in rows)
        {
            var fieldDef = row.Def.Fields.FirstOrDefault(e => e.InternalName == targetField);
            var targetCell = row.Cells.Where(e => e.Def == fieldDef).FirstOrDefault();
            var fieldMeta = Editor.Project.ParamData.GetParamFieldMeta(paramMeta, fieldDef);

            List<(string, Param.Row, string)> refs = ReferenceResolver.ResolveParamReferences(
                Editor, Editor.Project.ParamData.PrimaryBank, fieldMeta.RefTypes, row, targetCell.Value);

            foreach ((string, Param.Row, string) rf in refs)
            {
                if (row == null || Editor.EditorActionManager == null)
                {
                    continue;
                }

                rf.Item2.Name = row.Name;
            }
        }
    }
    #endregion

    #region Inherit Row Name
    public void InheritRowName(string targetField)
    {
        if (targetField == null)
            return;

        var curParamKey = Editor._activeView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param baseParam = Editor.Project.ParamData.PrimaryBank.Params[curParamKey];

        if (baseParam == null)
            return;

        List<Param.Row> rows = Editor._activeView.Selection.GetSelectedRows();

        var paramMeta = Editor.Project.ParamData.GetParamMeta(baseParam.AppliedParamdef);

        var actions = new List<EditorAction>();

        foreach (Param.Row row in rows)
        {
            var fieldDef = row.Def.Fields.FirstOrDefault(e => e.InternalName == targetField);
            var targetCell = row.Cells.Where(e => e.Def == fieldDef).FirstOrDefault();
            var fieldMeta = Editor.Project.ParamData.GetParamFieldMeta(paramMeta, fieldDef);

            List<(string, Param.Row, string)> refs = ReferenceResolver.ResolveParamReferences(
                Editor, Editor.Project.ParamData.PrimaryBank, fieldMeta.RefTypes, row, targetCell.Value);

            foreach ((string, Param.Row, string) rf in refs)
            {
                if (row == null || Editor.EditorActionManager == null)
                {
                    continue;
                }

                row.Name = rf.Item2.Name;
            }
        }
    }
    #endregion

    #region Inherit Row Name from FMG
    public void InheritRowNameFromFMG(string targetField)
    {
        if (targetField == null)
            return;

        var curParamKey = Editor._activeView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param baseParam = Editor.Project.ParamData.PrimaryBank.Params[curParamKey];

        if (baseParam == null)
            return;

        List<Param.Row> rows = Editor._activeView.Selection.GetSelectedRows();

        var paramMeta = Editor.Project.ParamData.GetParamMeta(baseParam.AppliedParamdef);

        var actions = new List<EditorAction>();

        foreach (Param.Row row in rows)
        {
            var fieldDef = row.Def.Fields.FirstOrDefault(e => e.InternalName == targetField);
            var targetCell = row.Cells.Where(e => e.Def == fieldDef).FirstOrDefault();
            var fieldMeta = Editor.Project.ParamData.GetParamFieldMeta(paramMeta, fieldDef);

            List<TextResult> refs = ReferenceResolver.ResolveTextReferences(
                Editor, fieldMeta.FmgRef, row, targetCell.Value);

            foreach (var result in refs)
            {
                if (row == null || Editor.EditorActionManager == null)
                {
                    continue;
                }

                row.Name = result.Entry.Text;
            }
        }
    }
    #endregion

    #region Adjust Row Name
    public void AdjustRowName(string adjustment, RowNameAdjustType type)
    {
        if (string.IsNullOrEmpty(adjustment))
            return;

        string curParamKey = Editor._activeView.Selection.GetActiveParam();

        if (string.IsNullOrEmpty(curParamKey))
            return;

        Param baseParam = Editor.Project.ParamData.PrimaryBank.Params[curParamKey];

        if (baseParam == null)
            return;

        List<Param.Row> rows = Editor._activeView.Selection.GetSelectedRows();

        var paramMeta = Editor.Project.ParamData.GetParamMeta(baseParam.AppliedParamdef);

        var actions = new List<EditorAction>();

        foreach (Param.Row row in rows)
        {
            if (type is RowNameAdjustType.Prepend)
            {
                row.Name = $"{adjustment}{row.Name}";
            }
            if (type is RowNameAdjustType.Postpend)
            {
                row.Name = $"{row.Name}{adjustment}";
            }
            if (type is RowNameAdjustType.Remove)
            {
                row.Name = row.Name.Replace(adjustment, "");
            }
        }
    }
    #endregion
}

public enum TargetType
{
    [Display(Name = "Selected Rows")] SelectedRows,
    [Display(Name = "Selected Param")] SelectedParam,
    [Display(Name = "All Params")] AllParams
}

public enum RowNameAdjustType
{
    Prepend,
    Postpend,
    Remove
}