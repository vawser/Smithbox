﻿using Andre.Formats;
using ImGuiNET;
using StudioCore.AssetLocator;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
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
            if (ImGui.RadioButton("Mass Edit Scripts##tool_MassEditScripts", ParamToolbarView.SelectedAction == ParamEditorAction.MassEditScripts))
            {
                ParamToolbarView.SelectedAction = ParamEditorAction.MassEditScripts;
            }
            ImguiUtils.ShowHoverTooltip("Use this to load and edit Mass Edit scripts.");
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

            if (ParamToolbarView.SelectedAction == ParamEditorAction.MassEditScripts)
            {
                ImGui.Text("Load and edit mass edit scripts here.");
                ImGui.Text("");

                // Ignore the combo box if no files exist
                if (MassEditScript.scriptList.Count > 0)
                {
                    ImGui.Text("Existing Scripts:");

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
                    ImGui.Text("");

                    if (_selectedMassEditScript != null)
                    {
                        if (ImGui.Button("Load", new Vector2(150, 32)))
                        {
                            ParamAction_MassEdit._currentMEditRegexInput = _selectedMassEditScript.GenerateMassedit();
                            ParamToolbarView.SelectedAction = ParamEditorAction.MassEdit;
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

                ImGui.Text("");

                ImGui.Text("New Script:");
                ImGui.InputText("##scriptName", ref _newScriptName, 255);
                ImguiUtils.ShowHoverTooltip("The file name used for this script.");
                ImGui.Text("");

                ImGui.Checkbox("Is Common Script", ref _newScriptIsCommon);
                ImguiUtils.ShowHoverTooltip($"Save the script as a common script for all project types.\nIf not, then the script will only appear for {Project.Type} projects.");
                ImGui.Text("");

                ImGui.Text("Script:");
                ImguiUtils.ShowHoverTooltip("The mass edit script.");
                ImGui.InputTextMultiline("##newMassEditScript", ref _newScriptBody, 65536, new Vector2(500, ImGui.GetTextLineHeightWithSpacing() * 24) * Smithbox.GetUIScale());
                ImGui.Text("");

                if (ImGui.Button("Save", new Vector2(150, 32)))
                {
                    SaveMassEditScript();
                }
                ImGui.SameLine();
                if (ImGui.Button("Open Script Folder", new Vector2(150, 32)))
                {
                    var dir = ParamAssetLocator.GetMassEditScriptGameDir();
                    Process.Start("explorer.exe", dir);
                }
            }
        }

        public static void Act()
        {
            if (ParamToolbarView.SelectedAction == ParamEditorAction.MassEditScripts)
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

            var path = "";
            var commonPath = "";
            var gameDirPath = "";

            var commonDir = ParamAssetLocator.GetMassEditScriptCommonDir();
            commonPath = Path.Combine(commonDir, $"{_newScriptName}.txt");

            var gameDir = ParamAssetLocator.GetMassEditScriptGameDir();
            gameDirPath = Path.Combine(gameDir, $"{_newScriptName}.txt");

            if (_newScriptIsCommon)
            {
                path = commonPath;
                if (!Directory.Exists(commonDir))
                {
                    Directory.CreateDirectory(commonDir);
                }
            }
            else
            {
                path = gameDirPath;
                if (!Directory.Exists(gameDir))
                {
                    Directory.CreateDirectory(gameDir);
                }
            }

            // Check both so the name is unique everywhere
            if (!File.Exists(gameDirPath) && !File.Exists(commonPath))
            {
                try
                {
                    var fs = new FileStream(path, System.IO.FileMode.Create);
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