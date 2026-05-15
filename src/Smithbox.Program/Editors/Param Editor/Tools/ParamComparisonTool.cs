using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.GparamEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Veldrid.MetalBindings;

namespace StudioCore.Editors.ParamEditor;
public class ParamComparisonTool
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

    private readonly StringBuilder _reportBuilder = new StringBuilder();

    public ProjectEntry TargetLoadedProject = null;
    public ProjectEntry TargetProject = null;
    public string TargetProjectName = "";
    public bool AllowGenerate = true;
    public bool LoadAuxBank = false;

    public string TargetParamComparison = "";


    public ParamComparisonTool(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Data Comparison"))
        {
            ImGui.BeginChild("ComparisonSection", ImGuiChildFlags.Borders);

            ImGui.BeginTabBar("dataComparisonTabs");

            ComparisonReportTab();
            ParamComparisonTab();
            RowComparisonTab();
            FieldComparisonTab();

            ImGui.EndTabBar();

            ImGui.EndChild();
        }
    }

    public void ComparisonReportTab()
    {
        if (ImGui.BeginTabItem($"Comparison Report"))
        {
            DisplayComparisonReport(false);

            ImGui.EndTabItem();
        }
    }

    public void ParamComparisonTab()
    {
        var projectList = Smithbox.Orchestrator.Projects;
        var paramData = Project.Handler.ParamData;

        if (ImGui.BeginTabItem($"Param Comparison"))
        {
            UIHelper.SimpleHeader("Loaded Projects", "There projects have been loaded and are displayed in the comparison column(s).");

            ImGui.BeginChild("loadedProjectsSection", new Vector2(0, 100), ImGuiChildFlags.Borders);

            foreach (var proj in projectList)
            {
                if (proj == null)
                    continue;

                if (proj.Descriptor.ProjectType != Editor.Project.Descriptor.ProjectType)
                    continue;

                if (proj == Editor.Project)
                    continue;

                if (!paramData.AuxBanks.ContainsKey(proj.Descriptor.ProjectName))
                    continue;

                var isSelected = false;

                if (TargetLoadedProject != null)
                {
                    isSelected = TargetLoadedProject.Descriptor.ProjectName == proj.Descriptor.ProjectName;
                }

                if (ImGui.Selectable($"{proj.Descriptor.ProjectName}", isSelected))
                {
                    TargetLoadedProject = proj;
                }

                if (isSelected)
                {
                    if (ImGui.BeginPopupContextItem($"context_{proj.Descriptor.ProjectGUID}"))
                    {
                        if (ImGui.Selectable("Unload"))
                        {
                            TargetParamComparison = TargetLoadedProject.Descriptor.ProjectName;
                            ClearTargetParamComparison();
                        }

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Avaliable Projects", "These projects can be loaded.");

            ImGui.BeginChild("avaliableProjectsSection", new Vector2(0, 200), ImGuiChildFlags.Borders);

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

            ImGui.EndChild();

            UIHelper.SetInputWidth();

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Options", "");
            ImGui.Checkbox("Import Row Names on Report Generation for Primary Bank", ref ImportNamesOnGeneration_Primary);
            ImGui.Checkbox("Import Row Names on Report Generation for Comparison Bank", ref ImportNamesOnGeneration_Compare);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Actions", "");

            UIHelper.MultiButtonInput("paramComparisonActions",
                "clearParamComparisons", "Clear All", "", ClearAllParamComparisons);

            ImGui.EndTabItem();
        }
    }

    public void RowComparisonTab()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        var compareRow = activeView.Selection.GetCompareRow();

        if (ImGui.BeginTabItem($"Row Comparison"))
        {
            UIHelper.SimpleHeader("Current Row Comparison", "");

            if (compareRow == null)
            {
                UIHelper.WrappedText("No row comparison target set.");
            }
            else
            {
                UIHelper.WrappedText($"{compareRow.ID} {compareRow.Name}");
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Actions", "");

            UIHelper.MultiButtonInput("rowComparisonActions",
                "clearRowComparison", "Clear", "", ClearRowComparison);

            ImGui.EndTabItem();
        }
    }

    public void FieldComparisonTab()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        var compareCol = activeView.Selection.GetCompareCol();

        if (ImGui.BeginTabItem($"Field Comparison"))
        {
            UIHelper.SimpleHeader("Current Field Comparison", "");

            if (compareCol == null)
            {
                UIHelper.WrappedText("No field comparison target set.");
            }
            else
            {
                UIHelper.WrappedText($"{compareCol.Def.InternalName}");
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Actions", "");

            UIHelper.MultiButtonInput("fieldComparisonActions",
                "clearFieldComparison", "Clear", "", ClearFieldComparison);

            ImGui.EndTabItem();
        }
    }

    public void DisplayDropdown()
    {
        var activeView = Editor.ViewHandler.ActiveView;
        var paramData = Project.Handler.ParamData;

        // Comparison Report
        if (ImGui.BeginMenu("Comparison Report"))
        {
            if (ImGui.MenuItem("View"))
            {
                ViewReport();
            }
            UIHelper.Tooltip("View a text report that details the differences between the current project params and the vanilla params.");

            ImGui.EndMenu();
        }

        // Param Comparison
        if (ImGui.BeginMenu("Param Comparison"))
        {
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
                        TargetProject = proj;
                        LoadComparisonParams();
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Clear specific param comparison", paramData.AuxBanks.Count > 0))
            {
                for (var i = 0; i < paramData.AuxBanks.Count; i++)
                {
                    KeyValuePair<string, ParamBank> pb = paramData.AuxBanks.ElementAt(i);

                    if (ImGui.MenuItem(pb.Key))
                    {
                        TargetParamComparison = pb.Key;
                        ClearTargetParamComparison();
                        break;
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("Clear all"))
            {
                ClearAllParamComparisons();
            }
            UIHelper.Tooltip("Unloads all comparison param banks.");

            ImGui.EndMenu();
        }

        // Row Comparison
        if (ImGui.BeginMenu("Row Comparison"))
        {
            if (ImGui.MenuItem("Clear"))
            {
                ClearRowComparison();
            }
            UIHelper.Tooltip("Clears the current row comparison target.");

            ImGui.EndMenu();
        }

        // Field Comparison
        if (ImGui.BeginMenu("Field Comparison"))
        {
            if (ImGui.MenuItem("Clear"))
            {
                ClearFieldComparison();
            }
            UIHelper.Tooltip("Clears the current field comparison target.");

            ImGui.EndMenu();
        }
    }

    public void ClearTargetParamComparison()
    {
        var paramData = Project.Handler.ParamData;

        if (TargetParamComparison != "")
        {
            if (paramData.AuxBanks.ContainsKey(TargetParamComparison))
            {
                paramData.AuxBanks.Remove(TargetParamComparison);
            }
        }
    }

    public void ClearAllParamComparisons()
    {
        if (Project.Handler.ParamData.AuxBanks.Count > 0)
        {
            Project.Handler.ParamData.AuxBanks = new Dictionary<string, ParamBank>();
        }
    }

    public void ClearRowComparison()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView != null && activeView.Selection.GetCompareRow() != null)
        {
            activeView.Selection.SetCompareRow(null);
        }
    }

    public void ClearFieldComparison()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView != null && activeView.Selection.GetCompareCol() != null)
        {
            activeView.Selection.SetCompareCol(null);
        }
    }

    public async void LoadComparisonParams()
    {
        await Project.Handler.ParamData.SetupAuxBank(TargetProject, true);
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
        _reportBuilder.Clear();

        var primaryBank = paramData.PrimaryBank;
        var compareBank = paramData.VanillaBank;

        if (TargetProjectName != "Vanilla")
        {
            if (paramData.AuxBanks.TryGetValue(TargetProjectName, out var auxBankValue))
            {
                compareBank = auxBankValue;
            }
        }

        if (ImportNamesOnGeneration_Primary)
        {
            RowNameHelper.ImportRowNames(
                Project,
                Project.Handler.ParamData.PrimaryBank,
                CFG.Current.ParamEditor_Import_Language);
        }

        if (ImportNamesOnGeneration_Compare)
        {
            if (TargetProjectName == "Vanilla")
            {
                RowNameHelper.ImportRowNames(
                    Project,
                    Project.Handler.ParamData.VanillaBank,
                    CFG.Current.ParamEditor_Import_Language);
            }
            else
            {
                if (paramData.AuxBanks.TryGetValue(TargetProjectName, out var auxBankValue))
                {
                    RowNameHelper.ImportRowNames(
                        Project,
                        auxBankValue,
                        CFG.Current.ParamEditor_Import_Language);
                }
            }
        }

        AddLog($"# Field values follow this format: [Comparison Value] -> [Primary Value]");

        var targetedSet = TargetedParams.Count > 0 ? new HashSet<string>(TargetedParams) : null;

        foreach (var param in primaryBank.Params)
        {
            CurrentParamProcessing = param.Key;

            var primaryParam = param.Value;

            if (targetedSet != null && !targetedSet.Contains(param.Key))
            {
                continue;
            }

            if (compareBank.Params.TryGetValue(param.Key, out var compareParam))
            {
                ReportDifferences(param.Key, primaryParam, compareParam);
            }
            else
            {
                AddLog($"{param.Key} does not exist in comparison.");
            }
        }
        ReportText = _reportBuilder.ToString();

        IsGeneratingReport = false;
        IsReportGenerated = true;
    }

    public void ReportDifferences(string paramKey, Param primaryParam, Param compareParam)
    {
        var compareRowsById = new Dictionary<int, Queue<Param.Row>>(compareParam.Rows.Count);
        foreach (var row in compareParam.Rows)
        {
            if (!compareRowsById.TryGetValue(row.ID, out var queue))
                compareRowsById[row.ID] = queue = new Queue<Param.Row>();
            queue.Enqueue(row);
        }

        var columns = primaryParam.Columns;

        bool HadParamDifference = false;

        foreach (var primaryRow in primaryParam.Rows)
        {
            if (!compareRowsById.TryGetValue(primaryRow.ID, out var queue) || queue.Count == 0)
            {
                if (!HadParamDifference)
                {
                    HadParamDifference = true;
                    AddLog($"[-- {paramKey} --]");
                }

                if (!string.IsNullOrEmpty(primaryRow.Name))
                    AddLog($"  {primaryRow.ID} ({primaryRow.Name}) does not exist in comparison.");
                else
                    AddLog($"  {primaryRow.ID} does not exist in comparison.");

                continue;
            }

            var compareRow = queue.Dequeue();

            if (primaryRow.DataEquals(compareRow))
                continue;

            bool HadRowDifference = false;
            bool HadCellDifference = false;

            for (int i = 0; i < columns.Count; i++)
            {
                var column = columns[i];

                if (column.Def.DisplayType == PARAMDEF.DefType.dummy8)
                    continue;

                var primaryValue = column.GetValue(primaryRow);
                var compareValue = column.GetValue(compareRow);

                if (!primaryValue.Equals(compareValue))
                {
                    if (!HadParamDifference)
                    {
                        HadParamDifference = true;
                        AddLog($"[-- {paramKey} --]");
                    }

                    if (!HadRowDifference)
                    {
                        HadRowDifference = true;
                        if (!string.IsNullOrEmpty(primaryRow.Name))
                            AddLog($"  {primaryRow.ID} {primaryRow.Name}:");
                        else
                            AddLog($"  {primaryRow.ID}:");
                    }

                    HadCellDifference = true;

                    AddLog($"    [{column.Def.InternalName}]: {compareValue} -> {primaryValue}");
                }
            }

            if (HadCellDifference)
            {
                AddLog(string.Empty);
            }
        }
    }

    public void AddLog(string text)
    {
        _reportBuilder.AppendLine(text);
    }

    public void DisplayComparisonReport(bool isPopup = true)
    {
        var paramData = Editor.Project.Handler.ParamData;

        UIHelper.SimpleHeader("Comparison Report", "");

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

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Project to Compare", "");

        UIHelper.SetInputWidth();
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

        UIHelper.SimpleHeader("Options", "");
        ImGui.Checkbox("Import Row Names on Report Generation for Primary Bank", ref ImportNamesOnGeneration_Primary);
        ImGui.Checkbox("Import Row Names on Report Generation for Comparison Bank", ref ImportNamesOnGeneration_Compare);

        UIHelper.Spacer();
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

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        if (isPopup)
        {
            UIHelper.MultiButtonInput("reportActions",
                "generateReport", "Generate Report", "", StartReportGeneration,
                "copyReport", "Copy Report to Clipboard", "", CopyToClipboard,
                "closeReport", "Close Report", "", CloseReport);
        }
        else
        {
            UIHelper.MultiButtonInput("reportActions",
                "generateReport", "Generate Report", "", StartReportGeneration,
                "copyReport", "Copy Report to Clipboard", "", CopyToClipboard);
        }

        if (IsReportGenerated)
        {
            UIHelper.Spacer();
            UIHelper.SimpleHeader("Report", "");

            var size = ImGui.GetContentRegionAvail();
            if (size.Y < 0)
                size.Y = 250f;

            ImGui.InputTextMultiline("##reportText", ref ReportText, UIHelper.GetTextInputBuffer(ReportText), size, ImGuiInputTextFlags.ReadOnly);
        }
        else if (IsGeneratingReport)
        {
            ImGui.Text("Report is being generated...");
            ImGui.Text($"Current Param: {CurrentParamProcessing}");
        }
    }

    public void StartReportGeneration()
    {
        TaskManager.LiveTask task = new(
                    "paramEditor_generateComparisonReport",
                    "[Param Editor]",
                    "Comparison report has been generated.",
                    "Comparison report failed to be generated.",
                    TaskManager.RequeueType.None,
                    false,
                    GenerateReport);

        TaskManager.Run(task);
    }

    public void CopyToClipboard()
    {
        PlatformUtils.Instance.SetClipboardText(ReportText);
    }

    public void CloseReport()
    {
        ShowReportModal = false;
    }

    public void HandleReportModal()
    {
        var size = UIHelper.GetMediumPopupSize();

        if (ShowReportModal)
        {
            ImGui.OpenPopup("Param Comparison Report");
        }

        if (ImGui.BeginPopupModal("Param Comparison Report", ref ShowReportModal, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.BeginChild("ReportSection", size, ImGuiChildFlags.Borders);
            DisplayComparisonReport();
            ImGui.EndChild();

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
