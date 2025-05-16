using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
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
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
        var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);
        var thirdButtonSize = new Vector2(windowWidth * 0.975f / 3, 32);
        var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);
        var inputButtonSize = new Vector2((windowWidth * 0.225f), 32);

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
                ParamTargetElement(ref CurrentTargetCategory, "The target for the Row Name trimming.", defaultButtonSize);

                if (ImGui.Button("Trim##action_TrimRowNames", defaultButtonSize))
                {
                    TrimRowNames();
                }
            }
        }
    }

    public void TrimRowNames()
    {
        var selectedParam = Editor._activeView.Selection;
        var curParamKey = selectedParam.GetActiveParam();

        if (curParamKey == null)
            return;

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
        UIHelper.WrappedText("Target Category:");
        ImGui.SetNextItemWidth(size.X);
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
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
        var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);
        var thirdButtonSize = new Vector2(windowWidth * 0.975f / 3, 32);
        var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);
        var inputButtonSize = new Vector2((windowWidth * 0.225f), 32);

        if (ImGui.CollapsingHeader("Sort Rows"))
        {
            UIHelper.WrappedText("Sort the rows for the currently selected param by their ID.");
            UIHelper.WrappedText("");

            if (ImGui.Button("Sort##action_SortRows", defaultButtonSize))
            {
                SortRows();
            }
        }
    }

    public void SortRows()
    {
        if (Editor._activeView.Selection.ActiveParamExists())
        {
            TaskLogs.AddLog($"Param rows sorted for {Editor._activeView.Selection.GetActiveParam()}");
            Editor.EditorActionManager.ExecuteAction(MassParamEditOther.SortRows(Editor.Project, Editor.Project.ParamData.PrimaryBank, Editor._activeView.Selection.GetActiveParam()));
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
}

public enum TargetType
{
    [Display(Name = "Selected Rows")] SelectedRows,
    [Display(Name = "Selected Param")] SelectedParam,
    [Display(Name = "All Params")] AllParams
}

