using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.MassEdit;

public class MassEditHandler
{
    private ParamEditorScreen Editor;

    public string _currentMEditRegexInput = "";
    public string _lastMEditRegexInput = "";
    public string _mEditRegexResult = "";
    public bool retainMassEditCommand = false;

    public string _newScriptName = "";
    public string _newScriptBody = "";
    public bool _newScriptIsCommon = true;
    public MassEditScript _selectedMassEditScript;

    public MassEditHandler(ParamEditorScreen editor)
    {
        Editor = editor;
    }

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

            // Options
            ImGui.Checkbox("Retain Input", ref retainMassEditCommand);
            UIHelper.WideTooltip("Retain the mass edit command in the input text area after execution.");
            UIHelper.WrappedText("");

            // AutoFill
            var res = AutoFill.MassEditCompleteAutoFill(Editor);
            if (res != null)
            {
                _currentMEditRegexInput = _currentMEditRegexInput + res;
            }

            // Input
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Input:");

            ImGui.InputTextMultiline("##MEditRegexInput", ref _currentMEditRegexInput, 65536,
            new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));

            if (ImGui.Button("Apply##action_Selection_MassEdit_Execute", halfButtonSize))
            {
                ExecuteMassEdit();
            }
            UIHelper.Tooltip($"{KeyBindings.Current.PARAM_ExecuteMassEdit.HintText}");


            ImGui.SameLine();
            if (ImGui.Button("Clear##action_Selection_MassEdit_Clear", halfButtonSize))
            {
                _currentMEditRegexInput = "";
            }

            ImGui.Text("");

            // Output
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Output:");
            UIHelper.WideTooltip("Success state of the Mass Edit command that was previously used.\n\nRemember to handle clipboard state between edits with the 'clear' command");
            ImGui.SameLine();
            UIHelper.WrappedText($"{_mEditRegexResult}");

            ImGui.InputTextMultiline("##MEditRegexOutput", ref _lastMEditRegexInput, 65536,
                new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()), ImGuiInputTextFlags.ReadOnly);
            UIHelper.WrappedText("");
        }
    }

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
                if (ImGui.BeginCombo("##massEditScripts", _selectedMassEditScript.name))
                {
                    foreach (var script in MassEditScript.scriptList)
                    {
                        if (ImGui.Selectable(script.name, _selectedMassEditScript.name == script.name))
                        {
                            _selectedMassEditScript = script;
                        }
                    }

                    ImGui.EndCombo();
                }
                if (_selectedMassEditScript != null)
                {
                    if (ImGui.Button("Load", thirdButtonSize))
                    {
                        _currentMEditRegexInput = _selectedMassEditScript.GenerateMassedit();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Edit", thirdButtonSize))
                    {
                        _newScriptName = _selectedMassEditScript.name;
                        _newScriptBody = _selectedMassEditScript.GenerateMassedit();
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
            ImGui.InputText("##scriptName", ref _newScriptName, 255);
            UIHelper.Tooltip("The file name used for this script.");
            UIHelper.WrappedText("");

            var Size = ImGui.GetWindowSize();
            float EditX = Size.X / 100 * 975;
            float EditY = Size.Y / 100 * 10;

            UIHelper.WrappedText("Script:");
            UIHelper.Tooltip("The mass edit script.");
            ImGui.InputTextMultiline("##newMassEditScript", ref _newScriptBody, 65536, new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));
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

    public void ExecuteMassEdit()
    {
        var command = _currentMEditRegexInput;

        Editor._activeView._selection.SortSelection();
        (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(Editor.Project.ParamData.PrimaryBank,
            _currentMEditRegexInput, Editor._activeView._selection);

        if (child != null)
        {
            Editor.EditorActionManager.PushSubManager(child);
        }

        if (r.Type == MassEditResultType.SUCCESS)
        {
            _lastMEditRegexInput = _currentMEditRegexInput;
            _currentMEditRegexInput = "";
            Editor.Project.ParamData.RefreshParamDifferenceCacheTask();
        }

        _mEditRegexResult = r.Information;

        if (retainMassEditCommand)
        {
            _currentMEditRegexInput = command;
        }
    }

    public void MassEditScriptSetup()
    {
        if (MassEditScript.scriptList.Count > 0)
        {
            if (_selectedMassEditScript == null)
            {
                _selectedMassEditScript = MassEditScript.scriptList[0];
            }
        }
    }

    public void SaveMassEditScript()
    {
        if (_newScriptName == "")
        {
            PlatformUtils.Instance.MessageBox($"Name must not be empty.", "Smithbox", MessageBoxButtons.OK);
            return;
        }

        var projectScriptDir = $"{Editor.Project.ProjectPath}\\.smithbox\\Assets\\Scripts\\";
        var scriptPath = $"{projectScriptDir}{_newScriptName}.txt";

        // Check both so the name is unique everywhere
        if (!File.Exists(scriptPath))
        {
            var filename = Path.GetFileNameWithoutExtension(scriptPath);

            try
            {
                var fs = new FileStream(scriptPath, FileMode.Create);
                var data = Encoding.ASCII.GetBytes(_newScriptBody);
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
            PlatformUtils.Instance.MessageBox($"{_newScriptName}.txt already exists within the Scripts folder.", "Smithbox", MessageBoxButtons.OK);
        }

        MassEditScript.ReloadScripts(Editor);
    }
}
