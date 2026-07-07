using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Utilities;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.ParamEditor;
public class ParamComparisonTool
{
    public ParamEditorView View;
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


    public ParamComparisonTool(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_Comparison_Header")}##dataComparisonHeader"))
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
        // Comparison Report
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_Comparison_Tab_Report")}##reportTab"))
        {
            DisplayComparisonReport(false);

            ImGui.EndTabItem();
        }
    }

    public void ParamComparisonTab()
    {
        var projectList = Smithbox.Orchestrator.Projects;
        var paramData = Project.Handler.ParamData;

        // Param Comparison
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_Comparison_Tab_Param_Compare")}##paramCompareTab"))
        {
            // Loaded Projects
            GUI.SimpleHeader(
                LOC.Get("PARAM_Comparison_Header_Loaded_Projects"),
                LOC.Get("PARAM_Comparison_Header_Loaded_Projects_TT"));

            ImGui.BeginChild("loadedProjectsSection", new Vector2(0, 100), ImGuiChildFlags.Borders);

            foreach (var proj in projectList)
            {
                if (proj == null)
                    continue;

                if (proj.Descriptor.ProjectType != View.Project.Descriptor.ProjectType)
                    continue;

                if (proj == View.Project)
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
                        // Unload
                        if (ImGui.Selectable($"{LOC.Get("PARAM_Comparison_Project_Context_Unload")}##unloadAction"))
                        {
                            TargetParamComparison = TargetLoadedProject.Descriptor.ProjectName;
                            ClearTargetParamComparison();
                        }

                        ImGui.EndPopup();
                    }
                }
            }

            ImGui.EndChild();

            // Avaliable Porjects
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_Comparison_Header_Available_Projects"),
                LOC.Get("PARAM_Comparison_Header_Available_Projects_TT"));

            ImGui.BeginChild("avaliableProjectsSection", new Vector2(0, 200), ImGuiChildFlags.Borders);

            foreach (var proj in projectList)
            {
                if (proj == null)
                    continue;

                if (proj.Descriptor.ProjectType != View.Project.Descriptor.ProjectType)
                    continue;

                if (proj == View.Project)
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

            // Options
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_Comparison_Header_Options"),
                LOC.Get("PARAM_Comparison_Header_Options_TT"));

            // Toggle: Import Row Names on Report Generation for Primary Bank
            ImGui.Checkbox($"{LOC.Get("PARAM_Comparison_Checkbox_Primary_Row_Name_Import")}##primaryRowNameImport",
                ref ImportNamesOnGeneration_Primary);

            // Toggle: Import Row Names on Report Generation for Comparison Bank
            ImGui.Checkbox($"{LOC.Get("PARAM_Comparison_Checkbox_Comparison_Row_Name_Import")}##comparisonRowNameImport", 
                ref ImportNamesOnGeneration_Compare);

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_Comparison_Header_Actions"),
                LOC.Get("PARAM_Comparison_Header_Actions_TT"));

            GUI.MultiButtonInput("paramComparisonActions",
                "clearParamComparisons", 
                LOC.Get("PARAM_Comparison_Action_Clear_All"),
                LOC.Get("PARAM_Comparison_Action_Clear_All_TT"),
                ClearAllParamComparisons);

            ImGui.EndTabItem();
        }
    }

    public void RowComparisonTab()
    {
        var compareRow = View.Selection.GetCompareRow();

        // Row Comparison
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_Comparison_Tab_Row_Compare")}##rowCompareTab"))
        {
            // Current Row Comparison
            GUI.SimpleHeader(
                LOC.Get("PARAM_Comparison_Header_Current_Row_Compare"),
                LOC.Get("PARAM_Comparison_Header_Current_Row_Compare_TT"));

            if (compareRow == null)
            {
                GUI.WrappedText(LOC.Get("PARAM_Comparison_No_Row_Compare"));
            }
            else
            {
                GUI.WrappedText($"{compareRow.ID} {compareRow.Name}");
            }

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_Comparison_Header_Actions"),
                LOC.Get("PARAM_Comparison_Header_Actions_TT"));

            GUI.MultiButtonInput("rowComparisonActions",
                "clearRowComparison", 
                LOC.Get("PARAM_Comparison_Action_Clear_Row_Compare"),
                LOC.Get("PARAM_Comparison_Action_Clear_Row_Compare_TT"),
                ClearRowComparison);

            ImGui.EndTabItem();
        }
    }

    public void FieldComparisonTab()
    {
        var compareCol = View.Selection.GetCompareCol();

        // Field Comparison
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_Comparison_Tab_Field_Compare")}##fieldCompareTab"))
        {
            // Current Field Comparison
            GUI.SimpleHeader(
                LOC.Get("PARAM_Comparison_Header_Current_Field_Compare"),
                LOC.Get("PARAM_Comparison_Header_Current_Field_Compare_TT"));

            if (compareCol == null)
            {
                GUI.WrappedText(LOC.Get("PARAM_Comparison_No_Field_Compare"));
            }
            else
            {
                GUI.WrappedText($"{compareCol.Def.InternalName}");
            }

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_Comparison_Header_Actions"),
                LOC.Get("PARAM_Comparison_Header_Actions_TT"));

            GUI.MultiButtonInput("fieldComparisonActions",
                "clearFieldComparison", 
                LOC.Get("PARAM_Comparison_Action_Clear_Field_Compare"),
                LOC.Get("PARAM_Comparison_Action_Clear_Field_Compare_TT"),
                ClearFieldComparison);

            ImGui.EndTabItem();
        }
    }

    public void DisplayDropdown()
    {
        var paramData = Project.Handler.ParamData;

        // Comparison Report
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_Comparison_Menu_Header_Report")}##comparisonReportMenuHeader"))
        {
            // View
            if (ImGui.MenuItem($"{LOC.Get("PARAM_Comparison_Menu_View_Report")}##viewReportAction"))
            {
                ViewReport();
            }
            GUI.Tooltip(LOC.Get("PARAM_Comparison_Menu_View_Report_TT"));

            ImGui.EndMenu();
        }

        // Param Comparison
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_Comparison_Menu_Header_Param_Compare")}##paramCompareMenuHeader"))
        {
            // Select project for param comparison
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_Comparison_Menu_Header_Select_Project")}##selectProjectMenuHeader"))
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

            // Clear specific param comparison
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_Comparison_Menu_Header_Clear_Param_Compare")}##clearParamCompareMenuHeader", paramData.AuxBanks.Count > 0))
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

            // Clear All
            if (ImGui.MenuItem($"{LOC.Get("PARAM_Comparison_Menu_Clear_All")}##clearAllAction"))
            {
                ClearAllParamComparisons();
            }
            GUI.Tooltip(LOC.Get("PARAM_Comparison_Menu_Clear_All"));

            ImGui.EndMenu();
        }

        // Row Comparison
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_Comparison_Menu_Header_Row_Compare")}##rowCompareMenuHeader"))
        {
            // Clear
            if (ImGui.MenuItem($"{LOC.Get("PARAM_Comparison_Menu_Clear_Row_Compare")}##clearRowCompareAction"))
            {
                ClearRowComparison();
            }
            GUI.Tooltip(LOC.Get("PARAM_Comparison_Menu_Clear_Row_Compare_TT"));

            ImGui.EndMenu();
        }

        // Field Comparison
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_Comparison_Menu_Header_Field_Compare")}##fieldCompareMenuHeader"))
        {
            // Clear
            if (ImGui.MenuItem($"{LOC.Get("PARAM_Comparison_Menu_Clear_Field_Compare")}##clearFieldCompareAction"))
            {
                ClearFieldComparison();
            }
            GUI.Tooltip(LOC.Get("PARAM_Comparison_Menu_Clear_Field_Compare_TT"));

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
        if (View != null && View.Selection.GetCompareRow() != null)
        {
            View.Selection.SetCompareRow(null);
        }
    }

    public void ClearFieldComparison()
    {
        if (View != null && View.Selection.GetCompareCol() != null)
        {
            View.Selection.SetCompareCol(null);
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
        var paramData = View.Project.Handler.ParamData;

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

        AddLog(LOC.Get("PARAM_Comparison_Report_Hint"));

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
                AddLog(LOC.Get("PARAM_Comparison_Report_Missing_Param_Key"));
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
                    AddLog("");
                    AddLog($"[-- {paramKey} --]");
                }

                if (!string.IsNullOrEmpty(primaryRow.Name))
                {
                    AddLog(LOC.Get("PARAM_Comparison_Report_Missing_Row_ID_With_Name", primaryRow.ID, primaryRow.Name));
                }
                else
                {
                    AddLog(LOC.Get("PARAM_Comparison_Report_Missing_Row_ID", primaryRow.ID));
                }

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
                        AddLog("");
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
        var paramData = View.Project.Handler.ParamData;

        GUI.SimpleHeader(
            LOC.Get("PARAM_ComparisonReport_Header"),
            LOC.Get("PARAM_ComparisonReport_Header_TT"));

        GUI.WrappedText(LOC.Get("PARAM_ComparisonReport_Primary_Bank_Version", paramData.PrimaryBank.ParamVersion));

        if (TargetProjectName == "Vanilla")
        {
            GUI.WrappedText(LOC.Get("PARAM_ComparisonReport_Comparison_Bank_Version", paramData.VanillaBank.ParamVersion));
        }
        else
        {
            if (paramData.AuxBanks.Count > 0)
            {
                if (paramData.AuxBanks.ContainsKey(TargetProjectName))
                {
                    var auxBank = paramData.AuxBanks[TargetProjectName];
                    GUI.WrappedText(LOC.Get("PARAM_ComparisonReport_Comparison_Bank_Version", auxBank.ParamVersion));
                }
            }
            else
            {
                GUI.WrappedText(LOC.Get("PARAM_ComparisonReport_Comparison_Bank_No_Load"));
            }
        }

        if (TargetProject == null)
            AllowGenerate = false;

        var projectList = Smithbox.Orchestrator.Projects;

        // Project to Compare:
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ComparisonReport_Header_Project_to_Compare"),
            LOC.Get("PARAM_ComparisonReport_Header_Project_to_Compare_TT"));

        GUI.SetInputWidth();
        if (ImGui.BeginCombo("##targetProjectComparison", TargetProjectName))
        {
            // Special-case for pointing to the vanilla bank
            if (ImGui.Selectable($"{LOC.Get("PARAM_ComparisonReport_Vanilla_Select")}##vanillaSelect", TargetProjectName == "Vanilla"))
            {
                TargetProject = View.Project;
                TargetProjectName = "Vanilla";
            }

            foreach (var proj in projectList)
            {
                if (proj == null)
                    continue;

                if (proj.Descriptor.ProjectType != View.Project.Descriptor.ProjectType)
                    continue;

                if (proj == View.Project)
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

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ComparisonReport_Header_Options"),
            LOC.Get("PARAM_ComparisonReport_Header_Options_TT"));

        // Toggle: Import Row Names on Report Generation for Primary Bank
        ImGui.Checkbox($"{LOC.Get("PARAM_Comparison_Checkbox_Primary_Row_Name_Import")}##importPrimaryRowName", 
            ref ImportNamesOnGeneration_Primary);

        // Toggle: Import Row Names on Report Generation for Comparison Bank
        ImGui.Checkbox($"{LOC.Get("PARAM_Comparison_Checkbox_Comparison_Row_Name_Import")}##importCompareRowName",
            ref ImportNamesOnGeneration_Compare);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ComparisonReport_Header_Targeted_Params"),
            LOC.Get("PARAM_ComparisonReport_Header_Targeted_Params_TT"));

        // Add
        if (ImGui.Button($"{Icons.Plus}##paramTargetAdd"))
        {
            TargetedParams.Add("");
        }
        GUI.Tooltip(LOC.Get("PARAM_ComparisonReport_Param_Target_Add_TT"));

        ImGui.SameLine();

        // Remove
        if (TargetedParams.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
            }
            GUI.Tooltip(LOC.Get("PARAM_ComparisonReport_Param_Target_Remove_TT"));

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
            }
            GUI.Tooltip(LOC.Get("PARAM_ComparisonReport_Param_Target_Remove_TT"));
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button($"{LOC.Get("PARAM_ComparisonReport_Param_Target_Reset")}##paramTargetReset"))
        {
            TargetedParams = new List<string>();
        }
        GUI.Tooltip(LOC.Get("PARAM_ComparisonReport_Param_Target_Reset_TT"));

        for (int i = 0; i < TargetedParams.Count; i++)
        {
            var curCommand = TargetedParams[i];
            var curText = curCommand;

            if (ImGui.InputText($"##paramTargetInput{i}", ref curText, 255))
            {
                TargetedParams[i] = curText;
            }
            GUI.Tooltip(LOC.Get("PARAM_ComparisonReport_Param_to_Include_TT"));
        }

        ImGui.Separator();

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ComparisonReport_Header_Action"),
            LOC.Get("PARAM_ComparisonReport_Header_Action_TT"));

        if (isPopup)
        {
            GUI.MultiButtonInput("reportActions",
                "generateReport", 
                LOC.Get("PARAM_ComparisonReport_Action_Generate_Report"),
                LOC.Get("PARAM_ComparisonReport_Action_Generate_Report_TT"),
                StartReportGeneration,

                "copyReport",
                LOC.Get("PARAM_ComparisonReport_Action_Copy_Report"),
                LOC.Get("PARAM_ComparisonReport_Action_Copy_Report_TT"), 
                CopyToClipboard,

                "closeReport",
                LOC.Get("PARAM_ComparisonReport_Action_Close_Report"),
                LOC.Get("PARAM_ComparisonReport_Action_Close_Report_TT"), 
                CloseReport);
        }
        else
        {
            GUI.MultiButtonInput("reportActions",
                "generateReport",
                LOC.Get("PARAM_ComparisonReport_Action_Generate_Report"),
                LOC.Get("PARAM_ComparisonReport_Action_Generate_Report_TT"), 
                StartReportGeneration,

                "copyReport",
                LOC.Get("PARAM_ComparisonReport_Action_Copy_Report"),
                LOC.Get("PARAM_ComparisonReport_Action_Copy_Report_TT"), 
                CopyToClipboard);
        }

        if (IsReportGenerated)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_ComparisonReport_Header_Report"),
                LOC.Get("PARAM_ComparisonReport_Header_Report_TT"));

            var size = ImGui.GetContentRegionAvail();
            if (size.Y < 0)
                size.Y = 250f;

            ImGui.InputTextMultiline("##reportText", ref ReportText, GUI.GetTextInputBuffer(ReportText), size, ImGuiInputTextFlags.ReadOnly);
        }
        else if (IsGeneratingReport)
        {
            ImGui.Text(LOC.Get("PARAM_ComparisonReport_Generating_Report"));
            ImGui.Text(LOC.Get("PARAM_ComparisonReport_Current_Param", CurrentParamProcessing));
        }
    }

    public void StartReportGeneration()
    {
        TaskManager.LiveTask task = new(
                    "paramEditor_generateComparisonReport",
                    LOC.Get("SYS_Header"),
                    LOC.Get("PARAM_ComparisonReport_Task_Generate_PASS"),
                    LOC.Get("PARAM_ComparisonReport_Task_Generate_FAIL"),
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
        var size = GUI.GetMediumPopupSize();

        if (ShowReportModal)
        {
            ImGui.OpenPopup($"{LOC.Get("PARAM_ComparisonReport_Modal_Name")}###paramComparisonReportModal");
        }

        if (ImGui.BeginPopupModal($"{LOC.Get("PARAM_ComparisonReport_Modal_Name")}###paramComparisonReportModal",
            ref ShowReportModal, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
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

            await View.Project.Handler.ParamData.SetupAuxBank(TargetProject, true);
            AllowGenerate = true;
        }
    }
}
