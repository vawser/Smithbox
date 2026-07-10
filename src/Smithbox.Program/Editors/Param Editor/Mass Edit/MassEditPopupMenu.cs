using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MassEditPopupMenu
{
    public MassEdit Parent;

    public MassEditPopupMenu(MassEdit parent)
    {
        Parent = parent;
    }

    public void Display()
    {
        var smallSize = GUI.GetSmallPopupSize();
        var mediumSize = GUI.GetMediumPopupSize();
        var popupFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize;

        var delimiter = CFG.Current.Param_Export_Delimiter[0];

        if (ImGui.BeginPopup("massEditMenuRegex", popupFlags))
        {
            ImGui.BeginChild("massEditMenuRegexChild", smallSize, ImGuiChildFlags.Borders);
            DisplayMassEditMenu();
            ImGui.EndChild();

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVExport", popupFlags))
        {
            ImGui.BeginChild("massEditMenuCSVExportChild", smallSize, ImGuiChildFlags.Borders);
            DisplayCsvExportMenu();
            ImGui.EndChild();

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVExport", popupFlags))
        {
            ImGui.BeginChild("massEditMenuSingleCSVExportChild", smallSize, ImGuiChildFlags.Borders);
            DisplaySingleCsvExportMenu();
            ImGui.EndChild();

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVImport", popupFlags))
        {
            ImGui.BeginChild("massEditMenuCSVImportChild", mediumSize, ImGuiChildFlags.Borders);
            DisplayCsvImportMenu();
            ImGui.EndChild();

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVImport", popupFlags))
        {
            ImGui.BeginChild("massEditMenuSingleCSVImportChild", mediumSize, ImGuiChildFlags.Borders);
            DisplaySingleCsvImportMenu();
            ImGui.EndChild();

            ImGui.EndPopup();
        }
        else
        {
            Parent.State.DisplayMassEditPopup = false;
            Parent.State.MassEditOutput_CSV = "";
        }
    }

    public void DisplayMassEditMenu()
    {
        // Mass Edit
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header"),
            LOC.Get("PARAM_MassEdit_Header_TT"));

        ImGui.Text(LOC.Get("PARAM_MassEdit_Menu_Hint"));

        GUI.Spacer();
        MassEditUtils.MassEditHeader(Parent,
            LOC.Get("PARAM_MassEdit_Menu_Input"),
            LOC.Get("PARAM_MassEdit_Menu_Input_TT"));

        GUI.MultilineTextInput("massEditInput", ref Parent.State.CurrentMenuInput);

        // Output
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Output"),
            LOC.Get("PARAM_MassEdit_Header_Output_TT"));

        ImGui.Text(Parent.State.MassEditResult);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Actions"),
            LOC.Get("PARAM_MassEdit_Header_Actions_TT"));

        GUI.MultiButtonInput("massEditActions",
            "submitMassEdit", 
            LOC.Get("PARAM_MassEdit_Action_Submit"),
            LOC.Get("PARAM_MassEdit_Action_Submit_TT"),
            SubmitMassEdit);
    }

    public void SubmitMassEdit()
    {
        Parent.CurrentView.Selection.SortSelection();

        Parent.ApplyMassEdit(Parent.State.CurrentMenuInput);
    }

    public void DisplayCsvExportMenu()
    {
        // CSV Export
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_CSV_Export"),
            LOC.Get("PARAM_MassEdit_Header_CSV_Export_TT"));

        GUI.MultilineTextInput("csvExportText", ref Parent.State.MassEditOutput_CSV);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Actions"),
            LOC.Get("PARAM_MassEdit_Header_Actions_TT"));

        GUI.MultiButtonInput("csvExportActions",
            "copyCsvOutput", 
            LOC.Get("PARAM_MassEdit_Action_Copy_to_Clipboard"),
            LOC.Get("PARAM_MassEdit_Action_Copy_to_Clipboard_TT"),
            CopyCsvOutput);
    }

    public void DisplaySingleCsvExportMenu()
    {
        // CSV Export (<field>)
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_CSV_Export_Field", Parent.State.MassEdit_SingleField_CSV),
            LOC.Get("PARAM_MassEdit_Header_CSV_Export_TT"));

        GUI.MultilineTextInput("csvExportText", ref Parent.State.MassEditOutput_CSV);

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Actions"),
            LOC.Get("PARAM_MassEdit_Header_Actions_TT"));

        GUI.MultiButtonInput("csvExportActions",
            "copyCsvOutput",
            LOC.Get("PARAM_MassEdit_Action_Copy_to_Clipboard"),
            LOC.Get("PARAM_MassEdit_Action_Copy_to_Clipboard_TT"), 
            CopyCsvOutput);
    }

    public void DisplayCsvImportMenu()
    {
        // CSV Import
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_CSV_Import"),
            LOC.Get("PARAM_MassEdit_Header_CSV_Import_TT"));

        GUI.MultilineTextInput("csvImportText", ref Parent.State.MassEditInput_CSV);

        // Delimiter
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Delimiter"),
            LOC.Get("PARAM_MassEdit_Header_Delimiter_TT"));

        MassEditUtils.DelimiterInputText();

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Options"),
            LOC.Get("PARAM_MassEdit_Header_Options_TT"));

        // Toggle: Append New ROws
        ImGui.Checkbox($"{LOC.Get("PARAM_MassEdit_Checkbox_Append_Mode")}##toggleAppendMode", 
            ref CFG.Current.Param_CSV_Append_Only);
        GUI.Tooltip(LOC.Get("PARAM_MassEdit_Checkbox_Append_Mode_TT"));

        if (CFG.Current.Param_CSV_Append_Only)
        {
            // Toggle: Replace Existing Rows
            ImGui.Checkbox($"{LOC.Get("PARAM_MassEdit_Checkbox_Replace_Existing")}##toggleReplaceExisting", 
                ref CFG.Current.Param_CSV_Replace_Row);
            GUI.Tooltip(LOC.Get("Replace existing rows instead of updating them (they will be moved to the end)"));
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Actions"),
            LOC.Get("PARAM_MassEdit_Header_Actions_TT"));

        GUI.MultiButtonInput("csvImportActions",
            "importCsv", 
            LOC.Get("PARAM_MassEdit_Action_Import_CSV"),
            LOC.Get("PARAM_MassEdit_Action_Import_CSV_TT"),
            ImportCsv);

        // Result
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Result"),
            LOC.Get("PARAM_MassEdit_Header_Result_TT"));

        ImGui.Text(Parent.State.MassEditResult_CSV);
    }

    public void DisplaySingleCsvImportMenu()
    {
        // CSV Import
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_CSV_Import_Field", Parent.State.MassEdit_SingleField_CSV),
            LOC.Get("PARAM_MassEdit_Header_CSV_Import_TT"));

        GUI.MultilineTextInput("csvImportText", ref Parent.State.MassEditInput_CSV);

        // Delimiter
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Delimiter"),
            LOC.Get("PARAM_MassEdit_Header_Delimiter_TT"));

        MassEditUtils.DelimiterInputText();

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Actions"),
            LOC.Get("PARAM_MassEdit_Header_Actions_TT"));

        GUI.MultiButtonInput("csvImportActions",
            "importCsv",
            LOC.Get("PARAM_MassEdit_Action_Import_CSV"),
            LOC.Get("PARAM_MassEdit_Action_Import_CSV_TT"), 
            ImportSingleCsv);

        // Result
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Result"),
            LOC.Get("PARAM_MassEdit_Header_Result_TT"));

        ImGui.Text(Parent.State.MassEditResult_CSV);
    }

    public void CopyCsvOutput()
    {
        PlatformUtils.Instance.SetClipboardText(Parent.State.MassEditOutput_CSV);
    }

    public void ImportCsv()
    {
        var delimiter = CFG.Current.Param_Export_Delimiter[0];

        (var result, CompoundAction action) = ParamIO.ApplyCSV(Parent.Project, Parent.CurrentView.GetPrimaryBank(),
                Parent.State.MassEditInput_CSV, Parent.CurrentView.Selection.GetActiveParam(), CFG.Current.Param_CSV_Append_Only,
                CFG.Current.Param_CSV_Append_Only && CFG.Current.Param_CSV_Replace_Row, delimiter);

        if (action != null)
        {
            if (action.HasActions)
            {
                Parent.CurrentView.Editor.ActionManager.ExecuteAction(action);
            }

            Parent.CurrentView.GetParamData().RefreshParamDifferenceCacheTask();
        }

        Parent.State.MassEditResult_CSV = result;
    }

    public void ImportSingleCsv()
    {
        var delimiter = CFG.Current.Param_Export_Delimiter[0];

        (var result, CompoundAction action) = ParamIO.ApplySingleCSV(Parent.Project, Parent.CurrentView.GetPrimaryBank(),
                Parent.State.MassEditInput_CSV, Parent.CurrentView.Selection.GetActiveParam(), Parent.State.MassEdit_SingleField_CSV,
                delimiter, false);

        if (action != null)
        {
            Parent.CurrentView.Editor.ActionManager.ExecuteAction(action);
        }

        Parent.State.MassEditResult_CSV = result;
    }
}
