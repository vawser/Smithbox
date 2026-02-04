using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class ParamComparisonTools
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

    public ParamComparisonTools(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void ComparisonMenu(ParamEditorView curView)
    {
        if (ImGui.MenuItem("View comparison report"))
        {
            ViewReport();
        }
        UIHelper.Tooltip("View a text report that details the differences between the current project params and the vanilla params.");

        if (ImGui.MenuItem("Toggle vanilla param column"))
        {
            CFG.Current.Param_ShowVanillaColumn = !CFG.Current.Param_ShowVanillaColumn;
        }

        if (ImGui.MenuItem("Clear current row comparison"))
        {
            if (curView != null && curView.Selection.GetCompareRow() != null)
            {
                curView.Selection.SetCompareRow(null);
            }
        }

        if (ImGui.MenuItem("Clear current field comparison"))
        {
            if (curView != null && curView.Selection.GetCompareCol() != null)
            {
                curView.Selection.SetCompareCol(null);
            }
        }

        if (ImGui.MenuItem("Clear all param comparisons"))
        {
            if (Project.Handler.ParamData.AuxBanks.Count > 0)
            {
                Project.Handler.ParamData.AuxBanks = new Dictionary<string, ParamBank>();
            }
        }

        if (ImGui.BeginMenu("Select project for param comparison"))
        {
            var projectList = Smithbox.Orchestrator.Projects;

            // Display compatible projects
            foreach (var proj in projectList)
            {
                if (proj == null)
                    continue;

                if (proj.Descriptor.ProjectType != Project.Descriptor.ProjectType)
                    continue;

                if (proj == Smithbox.Orchestrator.SelectedProject)
                    continue;

                var isSelected = false;

                if (ImGui.Selectable($"{proj.Descriptor.ProjectName}", isSelected))
                {
                    LoadComparisonParams(proj);
                }
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        var paramData = Project.Handler.ParamData;

        if (ImGui.BeginMenu("Clear param comparison...", paramData.AuxBanks.Count > 0))
        {
            for (var i = 0; i < paramData.AuxBanks.Count; i++)
            {
                KeyValuePair<string, ParamBank> pb = paramData.AuxBanks.ElementAt(i);
                if (ImGui.MenuItem(pb.Key))
                {
                    paramData.AuxBanks.Remove(pb.Key);
                    break;
                }
            }

            ImGui.EndMenu();
        }
    }

    public async void LoadComparisonParams(ProjectEntry proj)
    {
        await Project.Handler.ParamData.SetupAuxBank(proj, true);
    }

    public void ViewReport()
    {
        ShowReportModal = true;
    }

    public void GenerateReport()
    {
        var paramData = Editor.Project.Handler.ParamData;

        IsReportGenerated = false;
        IsGeneratingReport = true;
        ReportText = "";

        var primaryBank = paramData.PrimaryBank;
        var compareBank = paramData.VanillaBank;

        if (TargetProjectName != "Vanilla")
        {
            var auxBank = paramData.AuxBanks.Where(e => e.Key == TargetProjectName).FirstOrDefault();
            compareBank = auxBank.Value;
        }

        if (ImportNamesOnGeneration_Primary)
        {
            RowNameHelper.ImportRowNames(
                Project,
                Project.Handler.ParamData.PrimaryBank,
                ParamRowNameImportType.Community);
        }

        if (ImportNamesOnGeneration_Compare)
        {
            if (TargetProjectName == "Vanilla")
            {
                RowNameHelper.ImportRowNames(
                    Project,
                    Project.Handler.ParamData.VanillaBank,
                    ParamRowNameImportType.Community);
            }
            else
            {
                var auxBank = paramData.AuxBanks.Where(e => e.Key == TargetProjectName).FirstOrDefault();

                RowNameHelper.ImportRowNames(
                    Project,
                    auxBank.Value,
                    ParamRowNameImportType.Community);
            }
        }

        AddLog($"# Field values follow this format: [Comparison Value] -> [Primary Value]");

        foreach (var param in primaryBank.Params)
        {
            CurrentParamProcessing = param.Key;

            var primaryParam = param.Value;

            if (TargetedParams.Count > 0)
            {
                if (!TargetedParams.Contains(param.Key))
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

            if (indexedCompareRow == null)
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
    public string TargetProjectName = "";
    public bool AllowGenerate = true;
    public bool LoadAuxBank = false;

    public void Display()
    {
        var windowWidth = 800f;
        var windowHeight = 600f;

        var paramData = Editor.Project.Handler.ParamData;

        UIHelper.WrappedText("Comparison Report");
        ImGui.Separator();
        UIHelper.WrappedText($"Primary Bank - Param Version: {paramData.PrimaryBank.ParamVersion}");

        if (TargetProjectName == "Vanilla")
        {
            UIHelper.WrappedText($"Comparison Bank - Param Version: {paramData.VanillaBank.ParamVersion}");
        }
        else
        {
            if (paramData.AuxBanks.Count > 0)
            {
                if (paramData.AuxBanks.ContainsKey(TargetProjectName))
                {
                    var auxBank = paramData.AuxBanks[TargetProjectName];
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

        var projectList = Smithbox.Orchestrator.Projects;

        if (ImGui.BeginCombo("##targetProjectComparison", TargetProjectName))
        {
            // Special-case for pointing to the vanilla bank
            if (ImGui.Selectable($"Vanilla", TargetProjectName == "Vanilla"))
            {
                TargetProject = Editor.Project;
                TargetProjectName = "Vanilla";
            }

            foreach (var proj in projectList)
            {
                if (proj == null)
                    continue;

                if (proj.Descriptor.ProjectType != Editor.Project.Descriptor.ProjectType)
                    continue;

                if (proj == Editor.Project)
                    continue;

                var isSelected = false;

                if (TargetProject != null)
                {
                    isSelected = TargetProject.Descriptor.ProjectName == proj.Descriptor.ProjectName;
                }

                if (ImGui.Selectable($"{proj.Descriptor.ProjectName}", isSelected))
                {
                    TargetProject = proj;
                    TargetProjectName = proj.Descriptor.ProjectName;
                    LoadParamBank = true;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.Separator();

        ImGui.Checkbox("Import Row Names on Report Generation for Primary Bank", ref ImportNamesOnGeneration_Primary);
        ImGui.Checkbox("Import Row Names on Report Generation for Comparison Bank", ref ImportNamesOnGeneration_Compare);

        UIHelper.SimpleHeader("Targeted Params", "Leave blank to target all params.");

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

            if (ImGui.InputText($"##paramTargetInput{i}", ref curText, 255))
            {
                TargetedParams[i] = curText;
            }
            UIHelper.Tooltip("The param target to include.");
        }

        ImGui.Separator();

        if (IsReportGenerated)
        {
            UIHelper.SimpleHeader("Report", "");

            ImGui.InputTextMultiline("##reportText", ref ReportText, UIHelper.GetTextInputBuffer(ReportText), new Vector2(windowWidth, windowHeight), ImGuiInputTextFlags.ReadOnly);

            if (ImGui.Button("Re-generate"))
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
            if (ImGui.Button("Copy"))
            {
                PlatformUtils.Instance.SetClipboardText(ReportText);
            }
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                ShowReportModal = false;
            }
        }
        else if (IsGeneratingReport)
        {
            ImGui.Text("Report is being generated...");
            ImGui.Text($"Current Param: {CurrentParamProcessing}");

            if (ImGui.Button("Close"))
            {
                ShowReportModal = false;
            }
        }
        else
        {
            if (ImGui.Button("Generate"))
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
            if (ImGui.Button("Close"))
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

        Update();
    }

    public bool LoadParamBank = false;

    public async void Update()
    {
        if (LoadParamBank)
        {
            LoadParamBank = false;

            AllowGenerate = false;

            await Editor.Project.Handler.ParamData.SetupAuxBank(TargetProject, true);
            AllowGenerate = true;
        }
    }
}
