﻿using Andre.Formats;
using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_MassEdit
    {
        public static string _currentMEditRegexInput = "";
        private static string _lastMEditRegexInput = "";
        private static string _mEditRegexResult = "";

        private static bool retainMassEditCommand = false;

        public static void Select()
        {
            if (ImGui.RadioButton("Mass Edit##tool_MassEdit", ParamToolbarView.SelectedAction == ParamEditorAction.MassEdit))
            {
                ParamToolbarView.SelectedAction = ParamEditorAction.MassEdit;
            }
            ImguiUtils.ShowHoverTooltip("Use this to apply Mass Edit commands.");
        }

        public static void Configure()
        {
            if (ParamToolbarView.SelectedAction == ParamEditorAction.MassEdit)
            {
                ImGui.Text("Write and execute mass edit commands here.");
                ImGui.Text("");

                ImGui.Text("Input:");
                ImguiUtils.ShowWideHoverTooltip("Input your mass edit command here.");

                ImGui.InputTextMultiline("##MEditRegexInput", ref _currentMEditRegexInput, 65536,
                new Vector2(500, ImGui.GetTextLineHeightWithSpacing() * 4) * Smithbox.GetUIScale());
                ImGui.Text("");

                ImGui.Text($"Output: {_mEditRegexResult}");
                ImguiUtils.ShowWideHoverTooltip("Success state of the Mass Edit command that was previously used.\n\nRemember to handle clipboard state between edits with the 'clear' command");

                ImGui.InputTextMultiline("##MEditRegexOutput", ref _lastMEditRegexInput, 65536,
                    new Vector2(500, ImGui.GetTextLineHeightWithSpacing() * 4) * Smithbox.GetUIScale(), ImGuiInputTextFlags.ReadOnly);
                ImGui.Text("");

                ImGui.Checkbox("Retain Input", ref retainMassEditCommand);
                ImguiUtils.ShowWideHoverTooltip("Retain the mass edit command in the input text area after execution.");
                ImGui.Text("");

            }
        }

        public static void Act()
        {
            if (ParamToolbarView.SelectedAction == ParamEditorAction.MassEdit)
            {
                if (ImGui.Button("Apply##action_Selection_MassEdit_Execute", new Vector2(200, 32)))
                {
                    var command = _currentMEditRegexInput;
                    ExecuteMassEdit();
                    if (retainMassEditCommand)
                    {
                        _currentMEditRegexInput = command;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Clear##action_Selection_MassEdit_Clear", new Vector2(200, 32)))
                {
                    _currentMEditRegexInput = "";
                }
            }
        }

        public static void ExecuteMassEdit()
        {
            ParamEditorScreen._activeView._selection.SortSelection();
            (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(ParamBank.PrimaryBank,
                _currentMEditRegexInput, ParamEditorScreen._activeView._selection);

            if (child != null)
            {
                ParamToolbarView.EditorActionManager.PushSubManager(child);
            }

            if (r.Type == MassEditResultType.SUCCESS)
            {
                _lastMEditRegexInput = _currentMEditRegexInput;
                _currentMEditRegexInput = "";
                TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
                    TaskManager.RequeueType.Repeat,
                    true, TaskLogs.LogPriority.Low,
                    () => ParamBank.RefreshAllParamDiffCaches(false)));
            }

            _mEditRegexResult = r.Information;
        }
    }
}
