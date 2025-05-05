using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

public class ParamComparisonReport
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public bool ShowReportModal = false;

    public string CurrentParamProcessing = "";

    public string ReportText = "";

    public bool IsReportGenerated = false;
    public bool IsGeneratingReport = false;

    public bool ImportNamesOnGeneration_Primary = false;
    public bool ImportNamesOnGeneration_Compare = false;

    public ParamComparisonReport(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void ViewReport()
    {
        ShowReportModal = true;
    }

    public void GenerateReport()
    {
        IsReportGenerated = false;
        IsGeneratingReport = true;
        ReportText = "";

        var primaryBank = Editor.Project.ParamData.PrimaryBank;
        var compareBank = Editor.Project.ParamData.VanillaBank;

        if(CurrentCompareBankType == 1)
        {
            if (Editor.Project.ParamData.AuxBanks.Count <= 0)
            {
                PlatformUtils.Instance.MessageBox("No comparison bank has been loaded. Report generation ended.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);

                IsGeneratingReport = false;
                IsReportGenerated = false;
                return;
            }
            else
            {
                var auxBank = Editor.Project.ParamData.AuxBanks.First().Value;
                compareBank = auxBank;
            }
        }

        if (ImportNamesOnGeneration_Primary)
        {
            Editor.EditorActionManager.ExecuteAction(
                primaryBank.LoadParamDefaultNames(null, false, false, false));
        }

        if (ImportNamesOnGeneration_Compare)
        {
            Editor.EditorActionManager.ExecuteAction(
                compareBank.LoadParamDefaultNames(null, false, false, false));
        }

        AddLog($"# Field values follow this format: [Comparison Value] -> [Primary Value]");

        foreach (var param in primaryBank.Params)
        {
            CurrentParamProcessing = param.Key;

            var primaryParam = param.Value;
            if (compareBank.Params.ContainsKey(param.Key))
            {
                var compareParam = compareBank.Params[param.Key];

                ReportDifferences(param.Key, primaryParam, compareParam);
            }
            else
            {
                AddLog($"{param.Key} does not exist in comparison.");
            }
        }

        IsGeneratingReport = false;
        IsReportGenerated = true;
    }

    public void ReportDifferences(string paramKey, Param primaryParam, Param compareParam)
    {
        bool HadParamDifference = false;
        bool HadRowDifference = false;
        bool HadCellDifference = false;

        foreach (var primaryRow in primaryParam.Rows)
        {
            Param.Row indexedPrimaryRow = primaryParam?[primaryRow.ID];
            Param.Row indexedCompareRow = compareParam?[primaryRow.ID];

            if(indexedCompareRow == null)
            {
                if (!HadParamDifference)
                {
                    HadParamDifference = true;
                    AddLog($"[-- {paramKey} --]");
                }

                if (indexedPrimaryRow.Name != "")
                {
                    AddLog($"  {indexedPrimaryRow.ID} ({indexedPrimaryRow.Name}) does not exist in comparison.");
                }
                else
                {
                    AddLog($"  {indexedPrimaryRow.ID} does not exist in comparison.");
                }
            }
            else
            {
                HadRowDifference = false;
                HadCellDifference = false;

                foreach (var primaryCell in indexedPrimaryRow.Cells)
                {
                    var compareCell = indexedCompareRow.Cells.Where(e => e.Def == primaryCell.Def).FirstOrDefault();

                    if (!compareCell.IsNull())
                    {
                        var primaryValue = primaryCell.Value.ToString();
                        var compareValue = compareCell.Value.ToString();

                        if (primaryValue != compareValue)
                        {
                            if (!HadParamDifference)
                            {
                                HadParamDifference = true;
                                AddLog($"[-- {paramKey} --]");
                            }

                            if (!HadRowDifference)
                            {
                                HadRowDifference = true;
                                if (indexedPrimaryRow.Name != "")
                                {
                                    AddLog($"  {indexedPrimaryRow.ID} {indexedPrimaryRow.Name}:");
                                }
                                else
                                {
                                    AddLog($"  {indexedPrimaryRow.ID}:");
                                }
                            }

                            HadCellDifference = true;

                            AddLog($"    [{primaryCell.Def.InternalName}]: {compareValue} -> {primaryValue}");
                        }
                    }
                }

                if (HadCellDifference)
                {
                    AddLog($"");
                }
            }
        }
    }

    public void AddLog(string text)
    {
        ReportText = $"{ReportText}{text}\n";
    }

    public int CurrentCompareBankType = 0;

    public string[] CompareBankType =
    {
        "Vanilla Bank",
        "Aux Bank"
    };

    public void Display()
    {
        var textPaneSize = new Vector2(800, 600);

        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Comparison Report");
        ImGui.Separator();
        UIHelper.WrappedText($"Primary Bank - Param Version: {Editor.Project.ParamData.PrimaryBank.ParamVersion}");

        if (CurrentCompareBankType == 0)
        {
            UIHelper.WrappedText($"Comparison Bank - Param Version: {Editor.Project.ParamData.VanillaBank.ParamVersion}");
        }
        else if (CurrentCompareBankType == 1)
        {
            if (Editor.Project.ParamData.AuxBanks.Count > 0)
            {
                var auxBank = Editor.Project.ParamData.AuxBanks.First().Value;
                UIHelper.WrappedText($"Comparison Bank - Param Version: {auxBank.ParamVersion}");
            }
            else
            {
                UIHelper.WrappedText($"Comparison Bank - No Comparison Bank loaded.");
            }
        }
        ImGui.Separator();
        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Comparison Bank Type");
        if (ImGui.Combo("##compareBankTarget", ref CurrentCompareBankType, CompareBankType, CompareBankType.Length))
        {
        }
        ImGui.Checkbox("Import Row Names on Report Generation for Primary Bank", ref ImportNamesOnGeneration_Primary);
        ImGui.Checkbox("Import Row Names on Report Generation for Comparison Bank", ref ImportNamesOnGeneration_Compare);

        ImGui.Separator();

        if (IsReportGenerated)
        {
            var buttonSize = new Vector2(800 / 3, 32);

            ImGui.InputTextMultiline("##reportText", ref ReportText, uint.MaxValue, textPaneSize, ImGuiInputTextFlags.ReadOnly);

            if (ImGui.Button("Re-generate", buttonSize))
            {
                TaskManager.LiveTask task = new(
                    "paramEditor_generateComparisonReport",
                    "Param Editor",
                    "comparison report has been generated.",
                    "comparison report failed to be generated.",
                    TaskManager.RequeueType.None,
                    false,
                    GenerateReport);

                TaskManager.Run(task);
            }
            ImGui.SameLine();
            if (ImGui.Button("Copy", buttonSize))
            {
                UIHelper.CopyToClipboard(ReportText);
            }
            ImGui.SameLine();
            if (ImGui.Button("Close", buttonSize))
            {
                ShowReportModal = false;
            }
        }
        else if(IsGeneratingReport)
        {
            var buttonSize = new Vector2(800, 32);

            ImGui.Text("Report is being generated...");
            ImGui.Text($"Current Param: {CurrentParamProcessing}");

            if (ImGui.Button("Close", buttonSize))
            {
                ShowReportModal = false;
            }
        }
        else
        {
            var buttonSize = new Vector2(800 / 2, 32);

            if (ImGui.Button("Generate", buttonSize))
            {
                TaskManager.LiveTask task = new(
                    "paramEditor_generateComparisonReport",
                    "Param Editor",
                    "comparison report has been generated.",
                    "comparison report failed to be generated.",
                    TaskManager.RequeueType.None,
                    false,
                    GenerateReport
                );

                TaskManager.Run(task);
            }
            ImGui.SameLine();
            if (ImGui.Button("Close", buttonSize))
            {
                ShowReportModal = false;
            }
        }
    }

    public void HandleReportModal()
    {
        if (ShowReportModal)
        {
            ImGui.OpenPopup("Param Comparison Report");
        }

        if (ImGui.BeginPopupModal("Param Comparison Report", ref ShowReportModal, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
        {
            Display();

            ImGui.EndPopup();
        }
    }
}
