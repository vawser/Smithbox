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
        var smallSize = UIHelper.GetSmallPopupSize();
        var mediumSize = UIHelper.GetMediumPopupSize();
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
        UIHelper.SimpleHeader("Mass Edit", "");
        ImGui.Text("param PARAM: id VALUE: FIELD: = VALUE;");

        UIHelper.Spacer();
        MassEditUtils.MassEditHeader(Parent,
            "Input", "Type in the mass edit commands you wish to apply here.");

        UIHelper.MultilineTextInput("massEditInput", ref Parent.State.CurrentMenuInput);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Output", "");
        ImGui.Text(Parent.State.MassEditResult);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");
        UIHelper.MultiButtonInput("massEditActions",
            "submitMassEdit", "Submit", "", SubmitMassEdit);
    }

    public void SubmitMassEdit()
    {
        Parent.CurrentView.Selection.SortSelection();

        Parent.ApplyMassEdit(Parent.State.CurrentMenuInput);
    }

    public void DisplayCsvExportMenu()
    {
        UIHelper.SimpleHeader("CSV Export", "");

        UIHelper.MultilineTextInput("csvExportText", ref Parent.State.MassEditOutput_CSV);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");
        UIHelper.MultiButtonInput("csvExportActions",
            "copyCsvOutput", "Copy to Clipboard", "", CopyCsvOutput);
    }

    public void DisplaySingleCsvExportMenu()
    {
        UIHelper.SimpleHeader($"CSV Export ({Parent.State.MassEdit_SingleField_CSV})", "");

        UIHelper.MultilineTextInput("csvExportText", ref Parent.State.MassEditOutput_CSV);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");
        UIHelper.MultiButtonInput("csvExportActions",
            "copyCsvOutput", "Copy to Clipboard", "", CopyCsvOutput);
    }

    public void DisplayCsvImportMenu()
    {
        UIHelper.SimpleHeader("CSV Import", "");

        UIHelper.MultilineTextInput("csvImportText", ref Parent.State.MassEditInput_CSV);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Delimiter", "");
        MassEditUtils.DelimiterInputText();

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Options", "");
        ImGui.Checkbox("Append Mode", ref CFG.Current.Param_CSV_Append_Only);
        UIHelper.Tooltip("Append new rows instead of ID based insertion (this will create out-of-order IDs)");

        if (CFG.Current.Param_CSV_Append_Only)
        {
            ImGui.Checkbox("Replace Existing Rows", ref CFG.Current.Param_CSV_Replace_Row);
            UIHelper.Tooltip("Replace existing rows instead of updating them (they will be moved to the end)");
        }

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("csvImportActions",
            "importCsv", "Import", "", ImportCsv);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Result", "");
        ImGui.Text(Parent.State.MassEditResult_CSV);
    }

    public void DisplaySingleCsvImportMenu()
    {
        UIHelper.SimpleHeader($"CSV Import ({Parent.State.MassEdit_SingleField_CSV}", "");

        UIHelper.MultilineTextInput("csvImportText", ref Parent.State.MassEditInput_CSV);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Delimiter", "");
        MassEditUtils.DelimiterInputText();

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("csvImportActions",
            "importCsv", "Import", "", ImportSingleCsv);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Result", "");
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
