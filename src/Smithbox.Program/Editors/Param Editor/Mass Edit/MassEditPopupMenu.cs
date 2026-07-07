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
        GUI.SimpleHeader("Mass Edit", "");
        ImGui.Text("param PARAM: id VALUE: FIELD: = VALUE;");

        GUI.Spacer();
        MassEditUtils.MassEditHeader(Parent,
            "Input", "Type in the mass edit commands you wish to apply here.");

        GUI.MultilineTextInput("massEditInput", ref Parent.State.CurrentMenuInput);

        GUI.Spacer();
        GUI.SimpleHeader("Output", "");
        ImGui.Text(Parent.State.MassEditResult);

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");
        GUI.MultiButtonInput("massEditActions",
            "submitMassEdit", "Submit", "", SubmitMassEdit);
    }

    public void SubmitMassEdit()
    {
        Parent.CurrentView.Selection.SortSelection();

        Parent.ApplyMassEdit(Parent.State.CurrentMenuInput);
    }

    public void DisplayCsvExportMenu()
    {
        GUI.SimpleHeader("CSV Export", "");

        GUI.MultilineTextInput("csvExportText", ref Parent.State.MassEditOutput_CSV);

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");
        GUI.MultiButtonInput("csvExportActions",
            "copyCsvOutput", "Copy to Clipboard", "", CopyCsvOutput);
    }

    public void DisplaySingleCsvExportMenu()
    {
        GUI.SimpleHeader($"CSV Export ({Parent.State.MassEdit_SingleField_CSV})", "");

        GUI.MultilineTextInput("csvExportText", ref Parent.State.MassEditOutput_CSV);

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");
        GUI.MultiButtonInput("csvExportActions",
            "copyCsvOutput", "Copy to Clipboard", "", CopyCsvOutput);
    }

    public void DisplayCsvImportMenu()
    {
        GUI.SimpleHeader("CSV Import", "");

        GUI.MultilineTextInput("csvImportText", ref Parent.State.MassEditInput_CSV);

        GUI.Spacer();
        GUI.SimpleHeader("Delimiter", "");
        MassEditUtils.DelimiterInputText();

        GUI.Spacer();
        GUI.SimpleHeader("Options", "");
        ImGui.Checkbox("Append Mode", ref CFG.Current.Param_CSV_Append_Only);
        GUI.Tooltip("Append new rows instead of ID based insertion (this will create out-of-order IDs)");

        if (CFG.Current.Param_CSV_Append_Only)
        {
            ImGui.Checkbox("Replace Existing Rows", ref CFG.Current.Param_CSV_Replace_Row);
            GUI.Tooltip("Replace existing rows instead of updating them (they will be moved to the end)");
        }

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("csvImportActions",
            "importCsv", "Import", "", ImportCsv);

        GUI.Spacer();
        GUI.SimpleHeader("Result", "");
        ImGui.Text(Parent.State.MassEditResult_CSV);
    }

    public void DisplaySingleCsvImportMenu()
    {
        GUI.SimpleHeader($"CSV Import ({Parent.State.MassEdit_SingleField_CSV}", "");

        GUI.MultilineTextInput("csvImportText", ref Parent.State.MassEditInput_CSV);

        GUI.Spacer();
        GUI.SimpleHeader("Delimiter", "");
        MassEditUtils.DelimiterInputText();

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("csvImportActions",
            "importCsv", "Import", "", ImportSingleCsv);

        GUI.Spacer();
        GUI.SimpleHeader("Result", "");
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
