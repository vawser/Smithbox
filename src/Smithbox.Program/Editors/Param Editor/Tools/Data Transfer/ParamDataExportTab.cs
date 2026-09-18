using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid.MetalBindings;

namespace StudioCore.Editors.ParamEditor;

public class ParamDataExportTab
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public ParamDataTransferTool Parent;

    public bool DisplayFieldSelector = true;

    private string _activeParamCache = "";

    public ParamDataExportTab(ParamEditorView view, ProjectEntry project, ParamDataTransferTool parent)
    {
        View = view;
        Project = project;
        Parent = parent;
    }

    public void Display()
    {
        var activeParam = View.Selection.GetActiveParam();

        // Only update the field selector boolean list on param selection change
        if (_activeParamCache != activeParam)
        {
            _activeParamCache = activeParam;
            RefreshFieldInclusionList();
        }

        // Export
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_DataTransfer_Tab_Export")}##exportTab"))
        {
            GUI.WrappedText(LOC.Get("PARAM_DataTransfer_ExportTab_Hint"));

            // Export Mode
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Export_Mode"),
                LOC.Get("PARAM_DataTransfer_Header_Export_Mode_TT"));

            var previewCsvExportMode = LOC.Get(Parent.CsvExportMode.GetDisplayName());

            GUI.SetInputWidth();
            if (ImGui.BeginCombo("##CsvExportMode", previewCsvExportMode))
            {
                foreach (var entry in Enum.GetValues(typeof(CsvExportMode)))
                {
                    var CsvExportMode = (CsvExportMode)entry;

                    var displayName = LOC.Get(CsvExportMode.GetDisplayName());

                    if (ImGui.Selectable(displayName, CsvExportMode == Parent.CsvExportMode))
                    {
                        Parent.CsvExportMode = CsvExportMode;
                    }
                }

                ImGui.EndCombo();
            }

            // Export Type
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Export_Type"),
                LOC.Get("PARAM_DataTransfer_Header_Export_Type_TT"));

            var previewName = LOC.Get(Parent.CsvExportType.GetDisplayName());

            GUI.SetInputWidth();
            if (ImGui.BeginCombo("##csvExportType", previewName))
            {
                foreach (var entry in Enum.GetValues(typeof(CsvExportType)))
                {
                    var curExportType = (CsvExportType)entry;

                    var displayName = LOC.Get(curExportType.GetDisplayName());

                    if (ImGui.Selectable(displayName, curExportType == Parent.CsvExportType))
                    {
                        Parent.CsvExportType = curExportType;
                    }
                }

                ImGui.EndCombo();
            }

            if (Parent.CsvExportMode is CsvExportMode.Window)
            {
                DisplayWindowExport();
            }
            else if (Parent.CsvExportMode is CsvExportMode.File)
            {
                DisplayFileExport();
            }

            ImGui.EndTabItem();
        }
    }

    public void DisplayWindowExport()
    {
        if(Parent.CsvExportType is CsvExportType.AllParams or CsvExportType.ModifiedParams)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("PARAM_DataTransfer_Export_Window_Section_Invalid"));
            return;
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DataTransfer_Header_Actions"),
            LOC.Get("PARAM_DataTransfer_Header_Actions_TT"));

        GUI.MultiButtonInput("csvExportActions",
            "exportCsv",
            LOC.Get("PARAM_DataTransfer_Export_CSV_to_Window"),
            LOC.Get("PARAM_DataTransfer_Export_CSV_to_Window_TT"),
            Parent.ExportSingleToClipboard);

        // Field Selector (conditional header)
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("PARAM_DataTransfer_Header_Field_Selector"),
            LOC.Get("PARAM_DataTransfer_Header_Field_Selector_TT"),
            ref DisplayFieldSelector);

        if(DisplayFieldSelector)
        {
            DisplayFieldInclusionSection();
        }

        // Export Window
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DataTransfer_Header_Export_Window"),
            LOC.Get("PARAM_DataTransfer_Header_Export_Window_TT"));

        // Has to use TextUnformatted as the CSV output string can be massive,
        // and it exceeds the internal buffers used by InputTextMultiline
        ImGui.BeginChild("OutputTextSection", new Vector2(0, 250), ImGuiChildFlags.Borders);
        ImGui.TextUnformatted(Parent.ExportString);
        ImGui.EndChild();

        GUI.MultiButtonInput("csvOutputActions",
            "copyToClipboard",
            LOC.Get("PARAM_DataTransfer_Action_Copy_to_Clipboard"),
            LOC.Get("PARAM_DataTransfer_Action_Copy_to_Clipboard_TT"),
            Parent.CopyOutputToClipboard);
    }

    public bool IncludeName = true;
    public Dictionary<string, bool> FieldInclusionDict = new();

    private void RefreshFieldInclusionList()
    {
        var primaryBank = View.GetPrimaryBank();
        var activeParam = View.Selection.GetActiveParam();

        if (activeParam == null)
            return;

        FieldInclusionDict.Clear();

        if (primaryBank.Params.ContainsKey(activeParam))
        {
            var curParam = primaryBank.Params[activeParam];

            var fields = curParam.AppliedParamdef.Fields;

            foreach(var field in fields )
            {
                FieldInclusionDict.Add(field.InternalName, true);
            }
        }
    }

    public void DisplayFieldInclusionSection()
    {
        if (ImGui.Button($"{LOC.Get("PARAM_DataTransfer_FieldSelector_Toggle_All")}##fieldInclusionToggleAll", DPI.SelectorButtonSize))
        {
            foreach(var entry in FieldInclusionDict)
            {
                IncludeName = true;
                FieldInclusionDict[entry.Key] = true;
            }
        }

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("PARAM_DataTransfer_FieldSelector_Clear_All")}##fieldInclusionClearAll", DPI.SelectorButtonSize))
        {
            foreach (var entry in FieldInclusionDict)
            {
                IncludeName = false;
                FieldInclusionDict[entry.Key] = false;
            }
        }

        ImGui.BeginChild($"##fieldInclusionSection", new Vector2(0, 200) * DPI.UIScale(), ImGuiChildFlags.Borders);

        ImGui.Checkbox($"Name##toggleField_Name", ref IncludeName);

        foreach (var entry in FieldInclusionDict)
        {
            var curValue = entry.Value;
            ImGui.Checkbox($"{entry.Key}##toggleField_{entry.Key}", ref curValue);
            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                FieldInclusionDict[entry.Key] = curValue;
            }
        }

        ImGui.EndChild();
    }

    public void DisplayFileExport()
    {
        // Export Directory
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DataTransfer_Header_Export_Directory"),
            LOC.Get("PARAM_DataTransfer_Header_Export_Directory_TT"));

        GUI.SinglelineTextInputWithHint("csvExportDir", ref Parent.ExportDirectory,
            LOC.Get("PARAM_DataTransfer_Export_Dir_Hint"));

        GUI.MultiButtonInput("csvExportDir",
            "setDirectory",
            LOC.Get("PARAM_DataTransfer_Action_Set_Export_Directory"),
            LOC.Get("PARAM_DataTransfer_Action_Set_Export_Directory_TT"),
            Parent.SetExportDirectory,

            "openDirectory",
            LOC.Get("PARAM_DataTransfer_Action_Open_Export_Directory"),
            LOC.Get("PARAM_DataTransfer_Action_Open_Export_Directory_TT"),
            Parent.OpenExportDirectory);

        if (Parent.CsvExportType != CsvExportType.AllParams)
        {
            // Export Filename
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Export_Filename"),
                LOC.Get("PARAM_DataTransfer_Header_Export_Filename_TT"));

            GUI.SinglelineTextInputWithHint("csvExportFilename", ref Parent.ExportFilename, LOC.Get("PARAM_DataTransfer_Export_Filename_Hint"));

            // Export Output
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Export_Output"),
                LOC.Get("PARAM_DataTransfer_Header_Export_Output_TT"));

            // Has to use TextUnformatted as the CSV output string can be massive,
            // and it exceeds the internal buffers used by InputTextMultiline
            ImGui.BeginChild("OutputTextSection", new Vector2(0, 250), ImGuiChildFlags.Borders);
            ImGui.TextUnformatted(Parent.ExportString);
            ImGui.EndChild();

            GUI.MultiButtonInput("csvOutputActions",
                "copyToClipboard",
                LOC.Get("PARAM_DataTransfer_Action_Copy_to_Clipboard"),
                LOC.Get("PARAM_DataTransfer_Action_Copy_to_Clipboard_TT"),
                Parent.CopyOutputToClipboard);
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DataTransfer_Header_Actions"),
            LOC.Get("PARAM_DataTransfer_Header_Actions_TT"));

        if (Parent.CsvExportType is CsvExportType.AllParams or CsvExportType.ModifiedParams)
        {
            GUI.MultiButtonInput("csvMultipleExportActions",
                "exportCsvFile",
                LOC.Get("PARAM_DataTransfer_Action_Export_to_File"),
                LOC.Get("PARAM_DataTransfer_Action_Export_to_File_TT"),
                Parent.ExportMultipleToFile);
        }
        else
        {
            GUI.MultiButtonInput("csvSingleExportActions",
                "exportCsvClipboard",
                LOC.Get("PARAM_DataTransfer_Action_Copy_to_Clipboard"),
                LOC.Get("PARAM_DataTransfer_Action_Copy_to_Clipboard_TT"),
                Parent.ExportSingleToClipboard,

                "exportCsvFile",
                LOC.Get("PARAM_DataTransfer_Action_Export_to_File"),
                LOC.Get("PARAM_DataTransfer_Action_Export_to_File_TT"),
                Parent.ExportSingleToFile);
        }
    }
}
