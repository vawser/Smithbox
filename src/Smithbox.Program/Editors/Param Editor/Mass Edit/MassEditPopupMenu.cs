using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Application;
using StudioCore.Editors.Common;
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
        var size = UIHelper.GetSmallPopupSize();
        var popupFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize;

        var delimiter = CFG.Current.Param_Export_Delimiter[0];

        if (ImGui.BeginPopup("massEditMenuRegex", popupFlags))
        {
            ImGui.BeginChild("massEditMenuRegexChild", size, ImGuiChildFlags.Borders);
            DisplayMassEditMenu();
            ImGui.EndChild();

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVExport", popupFlags))
        {
            ImGui.BeginChild("massEditMenuCSVExportChild", size, ImGuiChildFlags.Borders);
            DisplayCsvExportMenu();
            ImGui.EndChild();

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVExport", popupFlags))
        {
            ImGui.BeginChild("massEditMenuSingleCSVExportChild", size, ImGuiChildFlags.Borders);
            DisplaySingleCsvExportMenu();
            ImGui.EndChild();

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVImport", popupFlags))
        {
            ImGui.BeginChild("massEditMenuCSVImportChild", size, ImGuiChildFlags.Borders);
            DisplayCsvImportMenu();
            ImGui.EndChild();

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVImport", popupFlags))
        {
            ImGui.BeginChild("massEditMenuSingleCSVImportChild", size, ImGuiChildFlags.Borders);
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
        ImGui.InputTextMultiline("##MEditOutput", ref Parent.State.MassEditOutput_CSV, UIHelper.GetTextInputBuffer(Parent.State.MassEditOutput_CSV),
            new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4), ImGuiInputTextFlags.ReadOnly);
    }

    public void DisplaySingleCsvExportMenu()
    {
        ImGui.Text(Parent.State.MassEdit_SingleField_CSV);
        ImGui.InputTextMultiline("##MEditOutput", ref Parent.State.MassEditOutput_CSV, UIHelper.GetTextInputBuffer(Parent.State.MassEditOutput_CSV),
            new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4), ImGuiInputTextFlags.ReadOnly);
    }

    public void DisplayCsvImportMenu()
    {
        var delimiter = CFG.Current.Param_Export_Delimiter[0];

        ImGui.InputTextMultiline("##MEditRegexInput", ref Parent.State.MassEditInput_CSV, 256 * 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4));
        ImGui.Checkbox("Append new rows instead of ID based insertion (this will create out-of-order IDs)",
            ref Parent.State.MassEdit_CSV_AppendOnly);

        if (Parent.State.MassEdit_CSV_AppendOnly)
        {
            ImGui.Checkbox("Replace existing rows instead of updating them (they will be moved to the end)",
                ref Parent.State.MassEdit_CSV_ReplaceRows);
        }

        MassEditUtils.DelimiterInputText();

        if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
        {
            (var result, CompoundAction action) = ParamIO.ApplyCSV(Parent.Project, Parent.CurrentView.GetPrimaryBank(),
                Parent.State.MassEditInput_CSV, Parent.CurrentView.Selection.GetActiveParam(), Parent.State.MassEdit_CSV_AppendOnly,
                Parent.State.MassEdit_CSV_AppendOnly && Parent.State.MassEdit_CSV_ReplaceRows, delimiter);

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

        ImGui.Text(Parent.State.MassEditResult_CSV);
    }

    public void DisplaySingleCsvImportMenu()
    {
        var delimiter = CFG.Current.Param_Export_Delimiter[0];

        ImGui.Text(Parent.State.MassEdit_SingleField_CSV);
        ImGui.InputTextMultiline("##MEditRegexInput", ref Parent.State.MassEditInput_CSV, 256 * 65536,
            new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4));

        MassEditUtils.DelimiterInputText();

        if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
        {
            (var result, CompoundAction action) = ParamIO.ApplySingleCSV(Parent.Project, Parent.CurrentView.GetPrimaryBank(),
                Parent.State.MassEditInput_CSV, Parent.CurrentView.Selection.GetActiveParam(), Parent.State.MassEdit_SingleField_CSV,
                delimiter, false);

            if (action != null)
            {
                Parent.CurrentView.Editor.ActionManager.ExecuteAction(action);
            }

            Parent.State.MassEditResult_CSV = result;
        }

        ImGui.Text(Parent.State.MassEditResult_CSV);
    }
}
