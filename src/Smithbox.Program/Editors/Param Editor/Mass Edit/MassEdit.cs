using Andre.Formats;
using Hexa.NET.ImGui;
using SixLabors.ImageSharp.Advanced;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MassEdit
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamView CurrentView;

    public MassEditTemplateMenu TemplateMenu;

    public string CurrentMassEditInput = "";

    public string MassEditResult = "";
    public string CurrentMenuInput = "";
    public string PreviousMenuInput = "";

    public string MassEditInput_CSV = "";
    public string MassEditOutput_CSV = "";
    public string MassEdit_SingleField_CSV = "";

    public bool DisplayMassEditPopup;

    public bool MassEdit_CSV_AppendOnly;
    public bool MassEdit_CSV_ReplaceRows;
    public string MassEditResult_CSV = "";

    // These are created here so the Mass Edit systems are local to the current project
    public SearchEngine<ParamSelection, (ParamMassEditRowSource, Param.Row)> PARSE;
    public ParamSearchEngine PSE;
    public RowSearchEngine RSE;
    public CellSearchEngine CSE;
    public VarSearchEngine VSE;

    public AutoFill AutoFill;

    public MassParamEditRegex MassParamEditRegex;

    public MEGlobalOperation GlobalOps;
    public MERowOperation RowOps;
    public MEValueOperation FieldOps;
    public MEOperationArgument OperationArgs;

    public MassEdit(ParamEditorScreen editor, ProjectEntry project, ParamView parentView)
    {
        Editor = editor;
        Project = project;
        CurrentView = parentView;

        TemplateMenu = new(this);

        PSE = new ParamSearchEngine(CurrentView);
        RSE = new RowSearchEngine(CurrentView);
        PARSE = new ParamAndRowSearchEngine(CurrentView, this);
        CSE = new CellSearchEngine(CurrentView);
        VSE = new VarSearchEngine(CurrentView);

        MassParamEditRegex = new MassParamEditRegex(CurrentView);

        GlobalOps = new MEGlobalOperation(CurrentView);
        RowOps = new MERowOperation(CurrentView);
        FieldOps = new MEValueOperation(CurrentView);
        OperationArgs = new MEOperationArgument(CurrentView);

        AutoFill = new AutoFill(CurrentView, this);
    }

    public void ApplyMassEdit(string commandString)
    {
        ExecuteMassEdit(commandString, CurrentView.GetPrimaryBank(), CurrentView.Selection);
    }

    public void ExecuteMassEdit(string commandString, ParamBank targetBank, ParamSelection context)
    {
        CurrentView.Selection.SortSelection();

        MassParamEditRegex = new MassParamEditRegex(CurrentView);

        (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(targetBank, commandString, context);

        if (child != null)
        {
            CurrentView.Editor.ActionManager.PushSubManager(child);
        }

        if (r.Type == ParamMassEditResultType.SUCCESS)
        {
            CurrentView.GetParamData().RefreshParamDifferenceCacheTask();
        }

        MassEditResult = r.Information;
    }


    #region Mass Edit Popup Window
    public void OpenMassEditPopup(string popup)
    {
        ImGui.OpenPopup(popup);
        DisplayMassEditPopup = true;
    }

    public void DisplayMassEditPopupWindow()
    {
        var delimiter = CFG.Current.Param_Export_Delimiter[0];

        // Popup size relies on magic numbers. Multiline maxlength is also arbitrary.
        if (ImGui.BeginPopup("massEditMenuRegex"))
        {
            ImGui.Text("param PARAM: id VALUE: FIELD: = VALUE;");
            ParamEditorHints.AddImGuiHintButton("MassEditHint", ref ParamEditorHints.MassEditHint);

            ImGui.InputTextMultiline("##MEditRegexInput", ref CurrentMenuInput, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4));

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                CurrentView.Selection.SortSelection();

                ApplyMassEdit(CurrentMenuInput);
            }

            ImGui.Text(MassEditResult);

            if (AutoFill != null)
            {
                var result = AutoFill.MassEditCompleteAutoFill();
                if (result != null)
                {
                    if (string.IsNullOrWhiteSpace(CurrentMenuInput))
                    {
                        CurrentMenuInput = result;
                    }
                    else
                    {
                        CurrentMenuInput += "\n" + result;
                    }
                }
            }

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVExport"))
        {
            ImGui.InputTextMultiline("##MEditOutput", ref MassEditOutput_CSV, UIHelper.GetTextInputBuffer(MassEditOutput_CSV),
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4), ImGuiInputTextFlags.ReadOnly);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVExport"))
        {
            ImGui.Text(MassEdit_SingleField_CSV);
            ImGui.InputTextMultiline("##MEditOutput", ref MassEditOutput_CSV, UIHelper.GetTextInputBuffer(MassEditOutput_CSV),
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4), ImGuiInputTextFlags.ReadOnly);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVImport"))
        {
            ImGui.InputTextMultiline("##MEditRegexInput", ref MassEditInput_CSV, 256 * 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4));
            ImGui.Checkbox("Append new rows instead of ID based insertion (this will create out-of-order IDs)",
                ref MassEdit_CSV_AppendOnly);

            if (MassEdit_CSV_AppendOnly)
            {
                ImGui.Checkbox("Replace existing rows instead of updating them (they will be moved to the end)",
                    ref MassEdit_CSV_ReplaceRows);
            }

            DelimiterInputText();

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                (var result, CompoundAction action) = ParamIO.ApplyCSV(Project, CurrentView.GetPrimaryBank(),
                    MassEditInput_CSV, CurrentView.Selection.GetActiveParam(), MassEdit_CSV_AppendOnly,
                    MassEdit_CSV_AppendOnly && MassEdit_CSV_ReplaceRows, delimiter);

                if (action != null)
                {
                    if (action.HasActions)
                    {
                        CurrentView.Editor.ActionManager.ExecuteAction(action);
                    }

                    CurrentView.GetParamData().RefreshParamDifferenceCacheTask();
                }

                MassEditResult_CSV = result;
            }

            ImGui.Text(MassEditResult_CSV);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVImport"))
        {
            ImGui.Text(MassEdit_SingleField_CSV);
            ImGui.InputTextMultiline("##MEditRegexInput", ref MassEditInput_CSV, 256 * 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4));

            DelimiterInputText();

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                (var result, CompoundAction action) = ParamIO.ApplySingleCSV(Project, CurrentView.GetPrimaryBank(),
                    MassEditInput_CSV, CurrentView.Selection.GetActiveParam(), MassEdit_SingleField_CSV,
                    delimiter, false);

                if (action != null)
                {
                    CurrentView.Editor.ActionManager.ExecuteAction(action);
                }

                MassEditResult_CSV = result;
            }

            ImGui.Text(MassEditResult_CSV);
            ImGui.EndPopup();
        }
        else
        {
            DisplayMassEditPopup = false;
            MassEditOutput_CSV = "";
        }
    }

    private static void DelimiterInputText()
    {
        var displayDelimiter = CFG.Current.Param_Export_Delimiter;

        if (displayDelimiter == "\t")
        {
            displayDelimiter = "\\t";
        }

        if (ImGui.InputText("Delimiter", ref displayDelimiter, 2))
        {
            if (displayDelimiter == "\\t")
                displayDelimiter = "\t";

            CFG.Current.Param_Export_Delimiter = displayDelimiter;
        }
    }
    #endregion

    #region Mass Edit Window
    public void DisplayMassEditMenu()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var inputBoxSize = new Vector2(windowWidth * 0.725f, 32);

        var Size = ImGui.GetWindowSize();
        float EditX = Size.X * 0.92f;
        float EditY = Size.Y * 0.1f;

        UIHelper.WrappedText("Write and execute mass edit commands here.");
        UIHelper.WrappedText("");

        // AutoFill
        if (AutoFill != null)
        {
            var res = AutoFill.MassEditCompleteAutoFill();
            if (res != null)
            {
                CurrentMenuInput = CurrentMenuInput + res;
            }
        }

        // Input
        UIHelper.WrappedText("Input:");

        ImGui.InputTextMultiline("##MEditRegexInput", ref CurrentMenuInput, 65536,
        new Vector2(EditX, EditY));

        if (ImGui.Button("Apply##action_Selection_MassEdit_Execute"))
        {
            ExecuteMassEdit(
                CurrentMenuInput,
                CurrentView.GetPrimaryBank(),
                CurrentView.Selection);
        }

        ImGui.SameLine();
        if (ImGui.Button("Clear##action_Selection_MassEdit_Clear"))
        {
            CurrentMenuInput = "";
        }

        ImGui.Text("");

        // Output
        UIHelper.WrappedText("Output:");
        UIHelper.WideTooltip("Success state of the Mass Edit command that was previously used.\n\nRemember to handle clipboard state between edits with the 'clear' command");

        ImGui.SameLine();

        UIHelper.WrappedText($"{MassEditResult}");
    }

    public void ConstructCommandFromField(string internalName)
    {
        var propertyName = internalName.Replace(" ", "\\s");
        string currInput = CurrentMenuInput;

        if (currInput == "")
        {
            // Add selection section if input is empty
            CurrentMenuInput = $"selection: {propertyName}: ";
        }
        else
        {
            // Otherwise just add the property name
            currInput = $"{currInput}{propertyName}";
            CurrentMenuInput = currInput;
        }
    }
    #endregion

}
