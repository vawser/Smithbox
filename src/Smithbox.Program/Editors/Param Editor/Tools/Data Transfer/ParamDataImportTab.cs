using Hexa.NET.ImGui;
using StudioCore.Editors.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StudioCore.Editors.ParamEditor;

public class ParamDataImportTab
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public ParamDataTransferTool Parent;

    public ParamDataImportTab(ParamEditorView view, ProjectEntry project, ParamDataTransferTool parent)
    {
        View = view;
        Project = project;
        Parent = parent;
    }

    public void Display()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        // Import
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_DataTransfer_Tab_Import")}##importTab"))
        {
            GUI.WrappedText(LOC.Get("PARAM_DataTransfer_ImportTab_Hint"));

            // Import Mode
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Import_Mode"),
                LOC.Get("PARAM_DataTransfer_Header_Import_Mode_TT"));

            var curImportMode = LOC.Get(Parent.ImportMode.GetDisplayName());

            GUI.SetInputWidth();
            if (ImGui.BeginCombo("##csvImportMode", curImportMode))
            {
                foreach (var entry in Enum.GetValues(typeof(CsvImportMode)))
                {
                    var curExportType = (CsvImportMode)entry;

                    var displayName = LOC.Get(curExportType.GetDisplayName());

                    if (ImGui.Selectable(displayName, curExportType == Parent.ImportMode))
                    {
                        Parent.ImportMode = curExportType;
                    }
                }

                ImGui.EndCombo();
            }

            if (Parent.ImportMode is CsvImportMode.SelectedParam)
            {
                // CSV Input
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("PARAM_DataTransfer_Header_CSV_Input"),
                    LOC.Get("PARAM_DataTransfer_Header_CSV_Input_TT"));

                GUI.MultilineTextInput("csvImportText", ref activeView.MassEdit.State.MassEditInput_CSV);
            }

            if (Parent.ImportMode is CsvImportMode.AllParams)
            {
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("PARAM_DataTransfer_Header_Import_Directory"),
                    LOC.Get("PARAM_DataTransfer_Header_Import_Directory_TT"));

                GUI.SinglelineTextInputWithHint("csvImportDir", ref Parent.ImportDirectory,
                    LOC.Get("PARAM_DataTransfer_Import_Dir_Hint"));

                GUI.MultiButtonInput("csvImportDirActions",
                    "setDirectory",
                    LOC.Get("PARAM_DataTransfer_Action_Set_Import_Directory"),
                    LOC.Get("PARAM_DataTransfer_Action_Set_Import_Directory_TT"),
                    Parent.SetImportDirectory,

                    "openDirectory",
                    LOC.Get("PARAM_DataTransfer_Action_Open_Import_Directory"),
                    LOC.Get("PARAM_DataTransfer_Action_Open_Import_Directory_TT"),
                    Parent.OpenImportDirectory);
            }

            // Import Type
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Import_Type"),
                LOC.Get("PARAM_DataTransfer_Header_Import_Type_TT"));

            var previewName = LOC.Get(Parent.ImportType.GetDisplayName());

            GUI.SetInputWidth();
            if (ImGui.BeginCombo("##csvImportType", previewName))
            {
                foreach (var entry in Enum.GetValues(typeof(CsvImportType)))
                {
                    var handlingType = (CsvImportType)entry;

                    var displayName = LOC.Get(handlingType.GetDisplayName());

                    if (ImGui.Selectable(displayName, handlingType == Parent.ImportType))
                    {
                        Parent.ImportType = handlingType;
                    }
                }

                ImGui.EndCombo();
            }

            // Specific Field for Import
            if (Parent.ImportType is CsvImportType.SpecificField)
            {
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("PARAM_DataTransfer_Header_Specific_Field"),
                    LOC.Get("PARAM_DataTransfer_Header_Specific_Field_TT"));

                GUI.SinglelineTextInputWithHint("SpecificFieldInput", ref Parent.SpecificFieldName,
                    LOC.Get("PARAM_DataTransfer_Specific_Field_Hint"));
            }

            // Delimiter
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Delimiter"),
                LOC.Get("PARAM_DataTransfer_Header_Delimiter_TT"));

            MassEditUtils.DelimiterInputText();

            // Options
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Options"),
                LOC.Get("PARAM_DataTransfer_Header_Options_TT"));

            // Append New Rows
            ImGui.Checkbox($"{LOC.Get("PARAM_DataTransfer_Checkbox_Ignore_Existing_Rows")}##toggleIgnoreExistingRows",
                ref CFG.Current.Param_CSV_Ignore_Existing_Rows);

            GUI.Tooltip(LOC.Get("PARAM_DataTransfer_Checkbox_Ignore_Existing_Rows_TT"));

            // Append New Rows
            ImGui.Checkbox($"{LOC.Get("PARAM_DataTransfer_Checkbox_Append_Mode")}##toggleAppendMode",
                ref CFG.Current.Param_CSV_Append_Only);

            GUI.Tooltip(LOC.Get("PARAM_DataTransfer_Checkbox_Append_Mode_TT"));

            if (CFG.Current.Param_CSV_Append_Only)
            {
                // Replace Existing Rows
                ImGui.Checkbox($"{LOC.Get("PARAM_DataTransfer_Checkbox_Replace_Existing")}##toggleReplaceExisting",
                    ref CFG.Current.Param_CSV_Replace_Row);

                GUI.Tooltip(LOC.Get("PARAM_DataTransfer_Checkbox_Replace_Existing_TT"));
            }

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Actions"),
                LOC.Get("PARAM_DataTransfer_Header_Actions_TT"));

            if (Parent.ImportMode is CsvImportMode.SelectedParam)
            {
                GUI.MultiButtonInput("csvImportActions",
                "importCsv",
                LOC.Get("PARAM_DataTransfer_Action_Import"),
                LOC.Get("PARAM_DataTransfer_Action_Import_TT"),
                Parent.ImportCsv,

                "importCsvFromFile",
                LOC.Get("PARAM_DataTransfer_Action_File_Import"),
                LOC.Get("PARAM_DataTransfer_Action_File_Import_TT"),
                Parent.ImportCsvFromFile,

                "pasteFromClipboard",
                LOC.Get("PARAM_MassEdit_Action_Paste_Clipboard"),
                LOC.Get("PARAM_MassEdit_Action_Paste_Clipboard_TT"),
                Parent.PasteFromClipboard);

                // Result
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("PARAM_DataTransfer_Header_Result"),
                    LOC.Get("PARAM_DataTransfer_Header_Result_TT"));

                ImGui.Text(activeView.MassEdit.State.MassEditResult_CSV);
            }

            if (Parent.ImportMode is CsvImportMode.AllParams)
            {
                GUI.MultiButtonInput("csvAllImportActions",
                "importCsvFromSourceFolder",
                LOC.Get("PARAM_DataTransfer_Action_Import_From_Source"),
                LOC.Get("PARAM_DataTransfer_Action_Import_From_Source_TT"),
                Parent.ImportCsvFromSourceFolder);
            }

            ImGui.EndTabItem();
        }
    }
}
