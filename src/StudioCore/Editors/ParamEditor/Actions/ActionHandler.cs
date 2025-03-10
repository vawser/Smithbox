using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Tasks;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.BHD5;

namespace StudioCore.Editors.ParamEditor.Actions;
public enum TargetType
{
    [Display(Name = "Selected Rows")] SelectedRows,
    [Display(Name = "Selected Param")] SelectedParam,
    [Display(Name = "All Params")] AllParams
}

public enum SourceType
{
    [Display(Name = "Smithbox")] Smithbox,
    [Display(Name = "Project")] Project,
    [Display(Name = "Developer")] Developer
}

public class ActionHandler
{
    private ParamEditorScreen Screen;

    public TargetType CurrentTargetCategory = TargetType.SelectedParam;
    public SourceType CurrentSourceCategory = SourceType.Smithbox;

    public bool _rowNameImporter_VanillaOnly = false;
    public bool _rowNameImporter_EmptyOnly = false;

    public ActionHandler(ParamEditorScreen screen)
    {
        Screen = screen;
    }

    public bool IsCommutativeParam()
    {
        var isValid = false;

        var paramName = Screen._activeView._selection.GetActiveParam();

        var groups = Smithbox.BankHandler.ParamCommutativeGroups.CommutativeGroups;

        Param param = ParamBank.PrimaryBank.Params[paramName];

        if(groups.Groups.Where(e => e.Params.Contains(paramName)).Any())
        {
            isValid = true;
        }

        return isValid;
    }

    /// <summary>
    /// For the tool window
    /// </summary>
    public void DisplayCommutativeDuplicateToolMenu()
    {
        var paramName = Screen._activeView._selection.GetActiveParam();
        Param param = ParamBank.PrimaryBank.Params[paramName];

        var groups = Smithbox.BankHandler.ParamCommutativeGroups.CommutativeGroups;

        if (groups.Groups.Where(e => e.Params.Contains(paramName)).Any())
        {
            var targetGroup = groups.Groups.Where(e => e.Params.Contains(paramName)).FirstOrDefault();

            if (targetGroup == null)
                return;

            ImGui.Text("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Param:");

            foreach (var entry in targetGroup.Params)
            {
                // Ignore current param
                if (entry == paramName)
                    continue;

                if (ImGui.Selectable($"{entry}", CFG.Current.Param_Toolbar_CommutativeDuplicate_Target == entry))
                {
                    CFG.Current.Param_Toolbar_CommutativeDuplicate_Target = entry;
                }
            }

            ImGui.Text("");
        }
    }

    /// <summary>
    /// For the dropdown menu
    /// </summary>
    public void DisplayCommutativeDuplicateMenu()
    {
        var paramName = Screen._activeView._selection.GetActiveParam();
        Param param = ParamBank.PrimaryBank.Params[paramName];

        var groups = Smithbox.BankHandler.ParamCommutativeGroups.CommutativeGroups;

        if (groups.Groups.Where(e => e.Params.Contains(paramName)).Any())
        {
            var targetGroup = groups.Groups.Where(e => e.Params.Contains(paramName)).FirstOrDefault();

            if (targetGroup == null)
                return;

            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Param:");

            foreach(var entry in targetGroup.Params)
            {
                // Ignore current param
                if (entry == paramName)
                    continue;

                if(ImGui.MenuItem($"{entry}"))
                {
                    CFG.Current.Param_Toolbar_CommutativeDuplicate_Target = entry;
                    CommutativeDuplicateHandler();
                }
            }
        }
    }

    public void CommutativeDuplicateHandler()
    {
        var currentParamName = Screen._activeView._selection.GetActiveParam();
        var targetParamName = CFG.Current.Param_Toolbar_CommutativeDuplicate_Target;

        if (targetParamName == "")
            return;

        if (currentParamName == targetParamName)
            return;

        Param currentParam = ParamBank.PrimaryBank.Params[currentParamName];
        Param targetParam = ParamBank.PrimaryBank.Params[targetParamName];

        if(targetParam == null) 
            return;

        List<Param.Row> selectedRows = Screen._activeView._selection.GetSelectedRows();

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

        List<EditorAction> actions = new List<EditorAction>();

        actions.Add(new AddParamsAction(targetParam, "legacystring", rowsToInsert, false, CFG.Current.Param_Toolbar_CommutativeDuplicate_ReplaceExistingRows, -1, CFG.Current.Param_Toolbar_CommutativeDuplicate_Offset, false));

        var compoundAction = new CompoundAction(actions);

        Screen.EditorActionManager.ExecuteAction(compoundAction);
    }

    public void DuplicateHandler()
    {
        Param param = ParamBank.PrimaryBank.Params[Screen._activeView._selection.GetActiveParam()];
        List<Param.Row> rows = Screen._activeView._selection.GetSelectedRows();

        if (rows.Count == 0)
        {
            return;
        }

        List<Param.Row> rowsToInsert = new();

        foreach (Param.Row r in rows)
        {
            Param.Row newrow = new(r);
            rowsToInsert.Add(newrow);
        }

        List<EditorAction> actions = new List<EditorAction>();

        for (int i = 0; i < CFG.Current.Param_Toolbar_Duplicate_Amount; i++)
        {
            actions.Add(new AddParamsAction(param, "legacystring", rowsToInsert, false, false, -1, CFG.Current.Param_Toolbar_Duplicate_Offset, true));
        }

        var compoundAction = new CompoundAction(actions);

        Screen.EditorActionManager.ExecuteAction(compoundAction);
    }

    public void ExportRowNameHandler()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            if (ParamBank.PrimaryBank.Params != null)
            {
                ExportRowNames();
            }
        }
    }
    public void CopyRowDetailHandler(bool includeName = false)
    {
        Param param = ParamBank.PrimaryBank.Params[Screen._activeView._selection.GetActiveParam()];
        List<Param.Row> rows = Screen._activeView._selection.GetSelectedRows();

        if (rows.Count == 0)
        {
            return;
        }

        var output = "";

        foreach(var entry in rows)
        {
            var id = entry.ID;
            var name = entry.Name;

            var entryOutput = $"{id}";

            if(includeName)
            {
                entryOutput = $"{id};{name}";
            }

            if(output == "")
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

    private void ExportRowNames()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;
        var activeParam = selectedParam.GetActiveParam();

        switch (CurrentTargetCategory)
        {
            case TargetType.SelectedRows:
                ExportRowNamesForRows(selectedParam.GetSelectedRows());
                PlatformUtils.Instance.MessageBox($"Row names for {activeParam} selected rows have been saved.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            case TargetType.SelectedParam:
                ExportRowNamesForParam(activeParam);
                PlatformUtils.Instance.MessageBox($"Row names for {activeParam} have been saved.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            case TargetType.AllParams:
                foreach (var param in ParamBank.PrimaryBank.Params)
                {
                    ExportRowNamesForParam(param.Key);
                }
                PlatformUtils.Instance.MessageBox($"Row names for all params have been saved.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ExportRowNamesForRows(IEnumerable<Param.Row> rows)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var dialog = NativeFileDialogSharp.Dialog.FileSave("txt");
        if (!dialog.IsOk) return;

        var path = dialog.Path;

        List<string> contents = IterateRows(rows);

        var dir = Path.GetDirectoryName(path)!;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllLines(path, contents);
    }

    private void ExportRowNamesForParam(string param)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var dir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Names";
        var path = Path.Combine(dir, $"{param}.txt");

        Param p = ParamBank.PrimaryBank.Params[param];

        List<string> contents = IterateRows(p.Rows);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllLines(path, contents);
    }
    private static List<string> IterateRows(IEnumerable<Param.Row> rows)
    {
        return rows.Select(r => $"{r.ID} {r.Name}").ToList();
    }

    public void ImportRowNameHandler()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            bool _rowNameImport_useProjectNames = CurrentSourceCategory == SourceType.Project;
            bool _rowNameImport_useDeveloperNames = CurrentSourceCategory == SourceType.Developer;

            if (ParamBank.PrimaryBank.Params != null)
            {
                switch (CurrentTargetCategory)
                {
                    case TargetType.SelectedRows:
                        var rows = selectedParam.GetSelectedRows();
                        Screen.EditorActionManager.ExecuteAction(
                            ParamBank.PrimaryBank.LoadParamDefaultNames(
                                selectedParam.GetActiveParam(),
                                _rowNameImporter_EmptyOnly,
                                _rowNameImporter_VanillaOnly,
                                _rowNameImport_useProjectNames,
                                _rowNameImport_useDeveloperNames,
                                rows)
                        );
                        break;
                    case TargetType.SelectedParam:
                        Screen.EditorActionManager.ExecuteAction(
                            ParamBank.PrimaryBank.LoadParamDefaultNames(
                                selectedParam.GetActiveParam(),
                                _rowNameImporter_EmptyOnly,
                                _rowNameImporter_VanillaOnly,
                                _rowNameImport_useProjectNames,
                                _rowNameImport_useDeveloperNames)
                        );
                        break;
                    case TargetType.AllParams:
                        Screen.EditorActionManager.ExecuteAction(
                            ParamBank.PrimaryBank.LoadParamDefaultNames(
                                null,
                                _rowNameImporter_EmptyOnly,
                                _rowNameImporter_VanillaOnly,
                                _rowNameImport_useProjectNames,
                                _rowNameImport_useDeveloperNames)
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public void RowNameTrimHandler()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            if (ParamBank.PrimaryBank.Params != null)
            {
                var activeParam = selectedParam.GetActiveParam();
                var rows = selectedParam.GetSelectedRows();
                switch (CurrentTargetCategory)
                {
                    case TargetType.SelectedRows:
                        if (!rows.Any()) return;
                        TrimRowNames(rows);
                        PlatformUtils.Instance.MessageBox($"Row names for {rows.Count} selected rows have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case TargetType.SelectedParam:
                        TrimRowNames(activeParam);
                        PlatformUtils.Instance.MessageBox($"Row names for {activeParam} have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case TargetType.AllParams:
                        foreach (var param in ParamBank.PrimaryBank.Params)
                        {
                            TrimRowNames(param.Key);
                        }
                        PlatformUtils.Instance.MessageBox($"Row names for all params have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private void TrimRowNames(IEnumerable<Param.Row> rows)
    {
        foreach (Param.Row row in rows)
        {
            row.Name = row.Name.Trim();
        }
    }
    private void TrimRowNames(string param)
    {
        Param p = ParamBank.PrimaryBank.Params[param];
        TrimRowNames(p.Rows);
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
        UIHelper.ShowHoverTooltip(tooltip);
        UIHelper.WrappedText("");
    }

    public void ParamSourceElement(ref SourceType currentSource, string tooltip, Vector2 size)
    {
        UIHelper.WrappedText("Source Category:");
        ImGui.SetNextItemWidth(size.X);
        if (ImGui.BeginCombo("##Source", currentSource.GetDisplayName()))
        {
            foreach (SourceType e in Enum.GetValues<SourceType>())
            {
                var name = e.GetDisplayName();
                if (ImGui.Selectable(name))
                {
                    currentSource = e;
                    break;
                }
            }
            ImGui.EndCombo();
        }
    }

    public void SortRowsHandler()
    {
        if (Screen._activeView._selection.ActiveParamExists())
        {
            TaskLogs.AddLog($"Param rows sorted for {Screen._activeView._selection.GetActiveParam()}");
            Screen.EditorActionManager.ExecuteAction(MassParamEditOther.SortRows(ParamBank.PrimaryBank, Screen._activeView._selection.GetActiveParam()));
        }
    }

    // Merge Params
    public string targetRegulationPath = "";
    public string targetLooseParamPath = "";
    public string targetEnemyParamPath = "";

    public bool targetUniqueOnly = true;

    public string[] allParamTypes =
    {
        FilterStrings.RegulationBinFilter, FilterStrings.Data0Filter, FilterStrings.ParamBndDcxFilter,
        FilterStrings.ParamBndFilter, FilterStrings.EncRegulationFilter
    };

    public void MergeParamHandler()
    {
        if(targetRegulationPath == "")
            {
            PlatformUtils.Instance.MessageBox("Target Regulation path is empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (!targetRegulationPath.Contains("regulation.bin"))
        {
            PlatformUtils.Instance.MessageBox("Target Regulation path is does not point to a regulation.bin file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ParamBank.LoadAuxBank(targetRegulationPath, null, null);
        if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            ParamBank.LoadAuxBank(targetRegulationPath, targetLooseParamPath, targetEnemyParamPath);
        }

        var auxBank = ParamBank.AuxBanks.First();

        // Apply the merge massedit script here
        var command = $"auxparam {auxBank.Key} .*: modified && unique ID: paste;";

        if(!targetUniqueOnly)
        {
            command = $"auxparam {auxBank.Key} .*: modified ID: paste;";
        }

        //TaskLogs.AddLog(command);
        ExecuteMassEdit(command);
    }

    public void ExecuteMassEdit(string command)
    {
        Smithbox.EditorHandler.ParamEditor._activeView._selection.SortSelection();
        (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(ParamBank.PrimaryBank,
            command, Smithbox.EditorHandler.ParamEditor._activeView._selection);

        if (child != null)
        {
            Screen.EditorActionManager.PushSubManager(child);
        }

        if (r.Type == MassEditResultType.SUCCESS)
        {
            ParamBank.RefreshParamDifferenceCacheTask();
        }
    }

    public void RevertRowToDefault()
    {
        Param baseParam = ParamBank.PrimaryBank.Params[Screen._activeView._selection.GetActiveParam()];
        Param vanillaParam = ParamBank.VanillaBank.Params[Screen._activeView._selection.GetActiveParam()];

        if (baseParam == null)
            return;

        if (vanillaParam == null)
            return;

        List<Param.Row> rows = Screen._activeView._selection.GetSelectedRows();

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

        actions.Add(new AddParamsAction(baseParam, "legacystring", rowsToInsert, false, true, -1, 0, false));

        var compoundAction = new CompoundAction(actions);

        Screen.EditorActionManager.ExecuteAction(compoundAction);
    }
}
