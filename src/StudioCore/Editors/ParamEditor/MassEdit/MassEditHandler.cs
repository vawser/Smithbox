using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.ParamEditor.MassEdit;

/// <summary>
/// Main handler for all Mass Edit tools
/// </summary>
public class MassEditHandler
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public string ME_Result = "";
    public string CurrentInput = "";
    public string PreviousInput = "";

    public string ME_CSV_Input = "";
    public string ME_CSV_Output = "";
    public string ME_Single_CSV_Field = "";

    public bool ME_Edit_Popup_Open;
    public bool _mEditCSVAppendOnly;
    public bool _mEditCSVReplaceRows;
    public string _mEditCSVResult = "";

    public string NewScriptName = "";
    public string NewScriptContents = "";
    public bool IsScriptCommon = true;

    public MassEditScript Current_ME_Script;

    // These are created here so the Mass Edit systems are local to the current project
    public SearchEngine<ParamSelection, (MassEditRowSource, Param.Row)> parse;
    public ParamSearchEngine pse;
    public RowSearchEngine rse;
    public CellSearchEngine cse;
    public VarSearchEngine vse;
    public AutoFill AutoFill;

    public MassParamEditRegex MassParamEditRegex;

    public MEGlobalOperation GlobalOps;
    public MERowOperation RowOps;
    public MEValueOperation FieldOps;
    public MEOperationArgument OperationArgs;

    public MassEditHandler(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Setup()
    {
        pse = new ParamSearchEngine(Project);
        rse = new RowSearchEngine(Project);
        parse = new ParamAndRowSearchEngine(Project);
        cse = new CellSearchEngine(Project);
        vse = new VarSearchEngine(Project);

        MassParamEditRegex = new MassParamEditRegex(Project);

        GlobalOps = new MEGlobalOperation(Project);
        RowOps = new MERowOperation(Project);
        FieldOps = new MEValueOperation(Project);
        OperationArgs = new MEOperationArgument(Project);

        AutoFill = new AutoFill(Project);
    }

    /// <summary>
    /// Helper method for the default mass edit configuration
    /// </summary>
    /// <param name="commandString"></param>
    public void ApplyMassEdit(string commandString)
    {
        ExecuteMassEdit(commandString, Project.ParamData.PrimaryBank, Editor._activeView.Selection);
    }

    /// <summary>
    /// Main execution function
    /// </summary>
    /// <param name="commandString"></param>
    /// <param name="targetBank"></param>
    /// <param name="context"></param>
    public void ExecuteMassEdit(string commandString, ParamBank targetBank, ParamSelection context)
    {
        Editor._activeView.Selection.SortSelection();

        MassParamEditRegex = new MassParamEditRegex(Project);
        (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(targetBank, commandString, context);

        if (child != null)
        {
            Editor.EditorActionManager.PushSubManager(child);
        }

        if (r.Type == MassEditResultType.SUCCESS)
        {
            Editor.Project.ParamData.RefreshParamDifferenceCacheTask();
        }

        ME_Result = r.Information;
    }


    #region Mass Edit Popup Window
    public void OpenMassEditPopup(string popup)
    {
        ImGui.OpenPopup(popup);
        ME_Edit_Popup_Open = true;
    }

    public void DisplayMassEditPopupWindow()
    {
        var scale = DPI.GetUIScale();

        // Popup size relies on magic numbers. Multiline maxlength is also arbitrary.
        if (ImGui.BeginPopup("massEditMenuRegex"))
        {
            ImGui.Text("param PARAM: id VALUE: FIELD: = VALUE;");
            UIHints.AddImGuiHintButton("MassEditHint", ref UIHints.MassEditHint);

            ImGui.InputTextMultiline("##MEditRegexInput", ref CurrentInput, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale);

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                Editor._activeView.Selection.SortSelection();

                ApplyMassEdit(CurrentInput);
            }

            ImGui.Text(ME_Result);

            if (AutoFill != null)
            {
                var result = AutoFill.MassEditCompleteAutoFill();
                if (result != null)
                {
                    if (string.IsNullOrWhiteSpace(CurrentInput))
                    {
                        CurrentInput = result;
                    }
                    else
                    {
                        CurrentInput += "\n" + result;
                    }
                }
            }

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVExport"))
        {
            ImGui.InputTextMultiline("##MEditOutput", ref ME_CSV_Output, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale, ImGuiInputTextFlags.ReadOnly);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVExport"))
        {
            ImGui.Text(ME_Single_CSV_Field);
            ImGui.InputTextMultiline("##MEditOutput", ref ME_CSV_Output, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale, ImGuiInputTextFlags.ReadOnly);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVImport"))
        {
            ImGui.InputTextMultiline("##MEditRegexInput", ref ME_CSV_Input, 256 * 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale);
            ImGui.Checkbox("Append new rows instead of ID based insertion (this will create out-of-order IDs)",
                ref _mEditCSVAppendOnly);

            if (_mEditCSVAppendOnly)
            {
                ImGui.Checkbox("Replace existing rows instead of updating them (they will be moved to the end)",
                    ref _mEditCSVReplaceRows);
            }

            DelimiterInputText();

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                (var result, CompoundAction action) = ParamIO.ApplyCSV(Project, Project.ParamData.PrimaryBank,
                    ME_CSV_Input, Editor._activeView.Selection.GetActiveParam(), _mEditCSVAppendOnly,
                    _mEditCSVAppendOnly && _mEditCSVReplaceRows, CFG.Current.Param_Export_Delimiter[0]);

                if (action != null)
                {
                    if (action.HasActions)
                    {
                        Editor.EditorActionManager.ExecuteAction(action);
                    }

                    Project.ParamData.RefreshParamDifferenceCacheTask();
                }

                _mEditCSVResult = result;
            }

            ImGui.Text(_mEditCSVResult);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVImport"))
        {
            ImGui.Text(ME_Single_CSV_Field);
            ImGui.InputTextMultiline("##MEditRegexInput", ref ME_CSV_Input, 256 * 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale);

            DelimiterInputText();

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                (var result, CompoundAction action) = ParamIO.ApplySingleCSV(Project, Project.ParamData.PrimaryBank,
                    ME_CSV_Input, Editor._activeView.Selection.GetActiveParam(), ME_Single_CSV_Field,
                    CFG.Current.Param_Export_Delimiter[0], false);

                if (action != null)
                {
                    Editor.EditorActionManager.ExecuteAction(action);
                }

                _mEditCSVResult = result;
            }

            ImGui.Text(_mEditCSVResult);
            ImGui.EndPopup();
        }
        else
        {
            ME_Edit_Popup_Open = false;
            ME_CSV_Output = "";
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
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
        var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);
        var thirdButtonSize = new Vector2(windowWidth * 0.975f / 3, 32);
        var inputBoxSize = new Vector2(windowWidth * 0.725f, 32);
        var inputButtonSize = new Vector2(windowWidth * 0.225f, 32);

        if (ImGui.CollapsingHeader("Mass Edit - Window"))
        {
            var Size = ImGui.GetWindowSize();
            float EditX = Size.X * 0.975f;
            float EditY = Size.Y * 0.1f;

            UIHelper.WrappedText("Write and execute mass edit commands here.");
            UIHelper.WrappedText("");

            // AutoFill
            if (AutoFill != null)
            {
                var res = AutoFill.MassEditCompleteAutoFill();
                if (res != null)
                {
                    CurrentInput = CurrentInput + res;
                }
            }

            // Input
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Input:");

            ImGui.InputTextMultiline("##MEditRegexInput", ref CurrentInput, 65536,
            new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));

            if (ImGui.Button("Apply##action_Selection_MassEdit_Execute", halfButtonSize))
            {
                ExecuteMassEdit(CurrentInput, Project.ParamData.PrimaryBank, Editor._activeView.Selection);
            }
            UIHelper.Tooltip($"{KeyBindings.Current.PARAM_ExecuteMassEdit.HintText}");

            ImGui.SameLine();
            if (ImGui.Button("Clear##action_Selection_MassEdit_Clear", halfButtonSize))
            {
                CurrentInput = "";
            }

            ImGui.Text("");

            // Output
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Output:");
            UIHelper.WideTooltip("Success state of the Mass Edit command that was previously used.\n\nRemember to handle clipboard state between edits with the 'clear' command");
            ImGui.SameLine();
            UIHelper.WrappedText($"{ME_Result}");
        }
    }

    public void ConstructCommandFromField(string internalName)
    {
        var propertyName = internalName.Replace(" ", "\\s");
        string currInput = Editor.MassEditHandler.CurrentInput;

        if (currInput == "")
        {
            // Add selection section if input is empty
            CurrentInput = $"selection: {propertyName}: ";
        }
        else
        {
            // Otherwise just add the property name
            currInput = $"{currInput}{propertyName}";
            CurrentInput = currInput;
        }
    }
    #endregion

    #region Mass Edit Script Window
    public void DisplayMassEditScriptMenu()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
        var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);
        var thirdButtonSize = new Vector2(windowWidth * 0.975f / 3, 32);
        var inputBoxSize = new Vector2(windowWidth * 0.725f, 32);
        var inputButtonSize = new Vector2(windowWidth * 0.225f, 32);

        if (ImGui.CollapsingHeader("Mass Edit - Scripts"))
        {
            MassEditScriptSetup();

            UIHelper.WrappedText("Load and edit mass edit scripts here.");
            UIHelper.WrappedText("");

            // Ignore the combo box if no files exist
            if (MassEditScript.scriptList.Count > 0)
            {
                UIHelper.WrappedText("Existing Scripts:");

                // Scripts
                ImGui.SetNextItemWidth(defaultButtonSize.X);
                if (ImGui.BeginCombo("##massEditScripts", Current_ME_Script.name))
                {
                    foreach (var script in MassEditScript.scriptList)
                    {
                        if (ImGui.Selectable(script.name, Current_ME_Script.name == script.name))
                        {
                            Current_ME_Script = script;
                        }
                    }

                    ImGui.EndCombo();
                }
                if (Current_ME_Script != null)
                {
                    if (ImGui.Button("Load", thirdButtonSize))
                    {
                        CurrentInput = Current_ME_Script.GenerateMassedit();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Edit", thirdButtonSize))
                    {
                        NewScriptName = Current_ME_Script.name;
                        NewScriptContents = Current_ME_Script.GenerateMassedit();
                    }
                    ImGui.SameLine();
                }

                if (ImGui.Button("Reload", thirdButtonSize))
                {
                    MassEditScript.ReloadScripts(Editor);
                }
            }

            UIHelper.WrappedText("");

            ImGui.SetNextItemWidth(defaultButtonSize.X);
            UIHelper.WrappedText("New Script:");
            ImGui.InputText("##scriptName", ref NewScriptName, 255);
            UIHelper.Tooltip("The file name used for this script.");
            UIHelper.WrappedText("");

            var Size = ImGui.GetWindowSize();
            float EditX = Size.X / 100 * 975;
            float EditY = Size.Y / 100 * 10;

            UIHelper.WrappedText("Script:");
            UIHelper.Tooltip("The mass edit script.");
            ImGui.InputTextMultiline("##newMassEditScript", ref NewScriptContents, 65536, new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));
            UIHelper.WrappedText("");

            if (ImGui.Button("Save", halfButtonSize))
            {
                SaveMassEditScript();
            }
            ImGui.SameLine();
            if (ImGui.Button("Open Script Folder", halfButtonSize))
            {
                var projectScriptDir = $"{Editor.Project.ProjectPath}\\.smithbox\\Assets\\Scripts\\";

                Process.Start("explorer.exe", projectScriptDir);
            }
        }
    }

    public void MassEditScriptSetup()
    {
        if (MassEditScript.scriptList.Count > 0)
        {
            if (Current_ME_Script == null)
            {
                Current_ME_Script = MassEditScript.scriptList[0];
            }
        }
    }

    public void SaveMassEditScript()
    {
        if (NewScriptName == "")
        {
            PlatformUtils.Instance.MessageBox($"Name must not be empty.", "Smithbox", MessageBoxButtons.OK);
            return;
        }

        var projectScriptDir = $"{Editor.Project.ProjectPath}\\.smithbox\\Assets\\Scripts\\";
        var scriptPath = $"{projectScriptDir}{NewScriptName}.txt";

        // Check both so the name is unique everywhere
        if (!File.Exists(scriptPath))
        {
            var filename = Path.GetFileNameWithoutExtension(scriptPath);

            try
            {
                var fs = new FileStream(scriptPath, FileMode.Create);
                var data = Encoding.ASCII.GetBytes(NewScriptContents);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();

                TaskLogs.AddLog($"Mass Edit: saved mass edit script: {filename} at {scriptPath}.");
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Mass Edit: to save mass edit script: {filename} at {scriptPath}\n{ex}");
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox($"{NewScriptName}.txt already exists within the Scripts folder.", "Smithbox", MessageBoxButtons.OK);
        }

        MassEditScript.ReloadScripts(Editor);
    }
    #endregion
}
