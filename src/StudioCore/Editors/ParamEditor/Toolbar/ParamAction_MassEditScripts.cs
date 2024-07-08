using Andre.Formats;
using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_MassEditScripts
    {
        private static string _newScriptName = "";
        private static string _newScriptBody = "";
        private static bool _newScriptIsCommon = true;
        private static MassEditScript _selectedMassEditScript;

        public static void Select()
        {
            if (ImGui.RadioButton("Mass Edit Scripts##tool_MassEditScripts", ParamToolbar.SelectedAction == ParamToolbarAction.MassEditScripts))
            {
                ParamToolbar.SelectedAction = ParamToolbarAction.MassEditScripts;
            }
            ImguiUtils.ShowHoverTooltip("Use this to load and edit Mass Edit scripts.");

            if (!CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure()
        {
            if (MassEditScript.scriptList.Count > 0)
            {
                if (_selectedMassEditScript == null)
                {
                    _selectedMassEditScript = MassEditScript.scriptList[0];
                }
            }

            if (ParamToolbar.SelectedAction == ParamToolbarAction.MassEditScripts)
            {
                ImguiUtils.WrappedText("Load and edit mass edit scripts here.");
                ImguiUtils.WrappedText("");

                // Ignore the combo box if no files exist
                if (MassEditScript.scriptList.Count > 0)
                {
                    ImguiUtils.WrappedText("Existing Scripts:");

                    // Scripts
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
                    ImguiUtils.WrappedText("");

                    if (_selectedMassEditScript != null)
                    {
                        if (ImGui.Button("Load", new Vector2(150, 32)))
                        {
                            ParamAction_MassEdit._currentMEditRegexInput = _selectedMassEditScript.GenerateMassedit();
                            ParamToolbar.SelectedAction = ParamToolbarAction.MassEdit;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Edit", new Vector2(150, 32)))
                        {
                            _newScriptName = _selectedMassEditScript.name;
                            _newScriptBody = _selectedMassEditScript.GenerateMassedit();
                        }
                        ImGui.SameLine();
                    }

                    if (ImGui.Button("Reload", new Vector2(150, 32)))
                    {
                        MassEditScript.ReloadScripts();
                    }
                }

                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("New Script:");
                ImGui.InputText("##scriptName", ref _newScriptName, 255);
                ImguiUtils.ShowHoverTooltip("The file name used for this script.");
                ImguiUtils.WrappedText("");

                var Size = ImGui.GetWindowSize();
                float EditX = (Size.X / 100) * 95;
                float EditY = (Size.Y / 100) * 25;

                ImguiUtils.WrappedText("Script:");
                ImguiUtils.ShowHoverTooltip("The mass edit script.");
                ImGui.InputTextMultiline("##newMassEditScript", ref _newScriptBody, 65536, new Vector2(EditX * Smithbox.GetUIScale(), EditY * Smithbox.GetUIScale()));
                ImguiUtils.WrappedText("");

                if (ImGui.Button("Save", new Vector2(150, 32)))
                {
                    SaveMassEditScript();
                }
                ImGui.SameLine();
                if (ImGui.Button("Open Script Folder", new Vector2(150, 32)))
                {
                    var projectScriptDir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\MassEditScripts\\";

                    Process.Start("explorer.exe", projectScriptDir);
                }
            }
        }

        public static void Act()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.MassEditScripts)
            {
                
            }
        }

        public static void SaveMassEditScript()
        {
            if (_newScriptName == "")
            {
                PlatformUtils.Instance.MessageBox($"Name must not be empty.", "Smithbox", MessageBoxButtons.OK);
                return;
            }

            var projectScriptDir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\MassEditScripts\\";
            var scriptPath = $"{projectScriptDir}{_newScriptName}.txt";

            // Check both so the name is unique everywhere
            if (!File.Exists(scriptPath))
            {
                try
                {
                    var fs = new FileStream(scriptPath, System.IO.FileMode.Create);
                    var data = Encoding.ASCII.GetBytes(_newScriptBody);
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                    fs.Dispose();
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"{ex}");
                }
            }
            else
            {
                PlatformUtils.Instance.MessageBox($"{_newScriptName}.txt already exists within the MassEditScripts folder.", "Smithbox", MessageBoxButtons.OK);
            }

            MassEditScript.ReloadScripts();
        }
    }
}
