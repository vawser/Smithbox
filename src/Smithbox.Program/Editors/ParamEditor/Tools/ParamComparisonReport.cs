using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using static StudioCore.Editors.ParamEditor.Data.ParamBank;

namespace StudioCore.Editors.ParamEditor.Tools;

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

    public List<string> TargetedParams = new();

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

        if(TargetProjectName != "Vanilla")
        {
            var auxBank = Project.ParamData.AuxBanks.Where(e => e.Key == TargetProjectName).FirstOrDefault();
            compareBank = auxBank.Value;
        }

        if (ImportNamesOnGeneration_Primary)
        {
            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.Community);
        }

        if (ImportNamesOnGeneration_Compare)
        {
            if(TargetProjectName == "Vanilla")
            {
                Project.ParamData.VanillaBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.Community);
            }
            else
            {
                var auxBank = Project.ParamData.AuxBanks.Where(e => e.Key == TargetProjectName).FirstOrDefault();
                Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.Community);
            }
        }

        AddLog($"# Field values follow this format: [Comparison Value] -> [Primary Value]");

        foreach (var param in primaryBank.Params)
        {
            CurrentParamProcessing = param.Key;

            var primaryParam = param.Value;

            if(TargetedParams.Count > 0)
            {
                if(!TargetedParams.Contains(param.Key))
                {
                    continue;
                }
            }

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

    public ProjectEntry TargetProject;
    public string TargetProjectName;
    public bool AllowGenerate = true;
    public bool LoadAuxBank = false;

    public async void Display()
    {
        var textPaneSize = new Vector2(800, 600);

        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Comparison Report");
        ImGui.Separator();
        UIHelper.WrappedText($"Primary Bank - Param Version: {Editor.Project.ParamData.PrimaryBank.ParamVersion}");

        if (TargetProjectName == "Vanilla")
        {
            UIHelper.WrappedText($"Comparison Bank - Param Version: {Editor.Project.ParamData.VanillaBank.ParamVersion}");
        }
        else
        {
            if (Editor.Project.ParamData.AuxBanks.Count > 0)
            {
                if (Editor.Project.ParamData.AuxBanks.ContainsKey(TargetProjectName))
                {
                    var auxBank = Editor.Project.ParamData.AuxBanks[TargetProjectName];
                    UIHelper.WrappedText($"Comparison Bank - Param Version: {auxBank.ParamVersion}");
                }
            }
            else
            {
                UIHelper.WrappedText($"Comparison Bank - No Comparison Bank loaded.");
            }
        }

        if (TargetProject == null)
            AllowGenerate = false;

        var projectList = Editor.Project.BaseEditor.ProjectManager.Projects;
  
        if (ImGui.BeginCombo("##targetProjectComparison", TargetProjectName))
        {
            // Special-case for pointing to the vanilla bank
            if (ImGui.Selectable($"Vanilla", TargetProjectName == "Vanilla"))
            {
                TargetProject = Editor.Project;
                TargetProjectName = "Vanilla";
            }

            foreach (var proj in Editor.Project.BaseEditor.ProjectManager.Projects)
            {
                if (proj == null)
                    continue;

                if (proj.ProjectType != Editor.Project.ProjectType)
                    continue;

                if (proj == Editor.Project)
                    continue;

                var isSelected = false;

                if (TargetProject != null)
                {
                    isSelected = TargetProject.ProjectName == proj.ProjectName;
                }

                if (ImGui.Selectable($"{proj.ProjectName}", isSelected))
                {
                    TargetProject = proj;
                    TargetProjectName = proj.ProjectName;
                    LoadAuxBank = true;
                }
            }

            ImGui.EndCombo();
        }

        if (LoadAuxBank)
        {
            AllowGenerate = false;
            LoadAuxBank = false;
            await Editor.Project.ParamData.SetupAuxBank(TargetProject, true);
            AllowGenerate = true;
        }

        ImGui.Separator();

        ImGui.Checkbox("Import Row Names on Report Generation for Primary Bank", ref ImportNamesOnGeneration_Primary);
        ImGui.Checkbox("Import Row Names on Report Generation for Comparison Bank", ref ImportNamesOnGeneration_Compare);

        UIHelper.SimpleHeader("paramTargets", "Targeted Params", "Leave blank to target all params.", UI.Current.ImGui_AliasName_Text);

        // Add
        if (ImGui.Button($"{Icons.Plus}##paramTargetAdd"))
        {
            TargetedParams.Add("");
        }
        UIHelper.Tooltip("Add new param target input row.");

        ImGui.SameLine();

        // Remove
        if (TargetedParams.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
            }
            UIHelper.Tooltip("Remove last added param target input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
                UIHelper.Tooltip("Remove last added param target input row.");
            }
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##paramTargetReset"))
        {
            TargetedParams = new List<string>();
        }
        UIHelper.Tooltip("Reset param target input rows.");

        for (int i = 0; i < TargetedParams.Count; i++)
        {
            var curCommand = TargetedParams[i];
            var curText = curCommand;

            ImGui.SetNextItemWidth(400f);
            if (ImGui.InputText($"##paramTargetInput{i}", ref curText, 255))
            {
                TargetedParams[i] = curText;
            }
            UIHelper.Tooltip("The param target to include.");
        }

        ImGui.Separator();

        if (IsReportGenerated)
        {
            UIHelper.SimpleHeader("paramReport", "Report", "", UI.Current.ImGui_AliasName_Text);

            var buttonSize = new Vector2(800 / 3, 32);

            ImGui.InputTextMultiline("##reportText", ref ReportText, UIHelper.GetTextInputBuffer(ReportText), textPaneSize, ImGuiInputTextFlags.ReadOnly);

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
