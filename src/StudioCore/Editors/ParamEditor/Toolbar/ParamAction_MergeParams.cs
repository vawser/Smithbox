using Andre.Formats;
using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_MergeParams
    {
        public static void Select()
        {
            if (ImGui.RadioButton("Merge Params##tool_MergeParams", ParamToolbar.SelectedAction == ParamToolbarAction.MergeParams))
            {
                ParamToolbar.SelectedAction = ParamToolbarAction.MergeParams;
            }
            ImguiUtils.ShowHoverTooltip("Use this to merge a target regulation.bin into your current project. This process is 'simple', and thus may produce a broken mod if you attempt to merge complex mods.");

            if (!CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        private static string targetRegulationPath = "";
        private static string targetLooseParamPath = "";
        private static string targetEnemyParamPath = "";

        private static string[] allParamTypes =
        {
            FilterStrings.RegulationBinFilter, FilterStrings.Data0Filter, FilterStrings.ParamBndDcxFilter,
            FilterStrings.ParamBndFilter, FilterStrings.EncRegulationFilter
        };

        public static void Configure()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.MergeParams)
            {
                ImguiUtils.WrappedText("Use this to merge a target regulation.bin into your current project.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedText("Merging will bring all unique param rows from the target regulation into your project.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedText("This process is 'simple', and thus may produce a broken mod if you attempt to merge complex mods.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Target Regulation");
                ImguiUtils.ShowHoverTooltip("This is the target regulation.bin you wish to merge.");

                ImGui.InputText("##targetRegulationPath", ref targetRegulationPath, 255);
                ImGui.SameLine();
                if(ImGui.Button($@"{ForkAwesome.FileO}"))
                {
                    if (PlatformUtils.Instance.OpenFileDialog("Select target regulation.bin...", allParamTypes, out var path))
                    {
                        targetRegulationPath = path;
                    }
                }
                ImguiUtils.WrappedText("");

                if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    ImguiUtils.WrappedText("Target Loose Params");
                    ImguiUtils.ShowHoverTooltip("This is the target loose param folder you wish to merge.");

                    ImGui.InputText("##targetLooseParamPath", ref targetLooseParamPath, 255);
                    ImGui.SameLine();
                    if (ImGui.Button($@"{ForkAwesome.FileO}"))
                    {
                        if (PlatformUtils.Instance.OpenFileDialog("Select target loose param folder...", allParamTypes, out var path))
                        {
                            targetLooseParamPath = path;
                        }
                    }
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Target Regulation");
                    ImguiUtils.ShowHoverTooltip("This is the target enemy param you wish to merge.");

                    ImGui.InputText("##targetEnemyParamPath", ref targetEnemyParamPath, 255);
                    ImGui.SameLine();
                    if (ImGui.Button($@"{ForkAwesome.FileO}"))
                    {
                        if (PlatformUtils.Instance.OpenFileDialog("Select target loose param folder...", allParamTypes, out var path))
                        {
                            targetEnemyParamPath = path;
                        }
                    }
                    ImguiUtils.WrappedText("");
                }
            }
        }

        public static void Act()
        {

            if (ParamToolbar.SelectedAction == ParamToolbarAction.MergeParams)
            {
                if (ImGui.Button("Apply##action_Selection_DuplicateRow", new Vector2(200, 32)))
                {
                    if (CFG.Current.Interface_ParamEditor_PromptUser)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"You are about to use the Merge Params action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            MergeParams();
                        }
                    }
                    else
                    {
                        MergeParams();
                    }
                }
            }
        }

        public static void MergeParams()
        {
            if(targetRegulationPath == "")
            {
                PlatformUtils.Instance.MessageBox("Target Regulation path is empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(!targetRegulationPath.Contains("regulation.bin"))
            {
                PlatformUtils.Instance.MessageBox("Target Regulation path is does not point to a regulation.bin file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ParamBank.LoadAuxBank(targetRegulationPath, null, null);
            if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
            {
                ParamBank.LoadAuxBank(targetRegulationPath, targetLooseParamPath, targetEnemyParamPath);
            }

            var auxBank = ParamBank.AuxBanks.First();

            // Apply the merge massedit script here
            var command = $"auxparam {auxBank.Key} .*: modified && unique ID: paste;";
            //TaskLogs.AddLog(command);
            ExecuteMassEdit(command);
        }

        public static void ExecuteMassEdit(string command)
        {
            Smithbox.EditorHandler.ParamEditor._activeView._selection.SortSelection();
            (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(ParamBank.PrimaryBank,
                command, Smithbox.EditorHandler.ParamEditor._activeView._selection);

            if (child != null)
            {
                ParamToolbar.EditorActionManager.PushSubManager(child);
            }

            if (r.Type == MassEditResultType.SUCCESS)
            {
                TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
                    TaskManager.RequeueType.Repeat,
                    true, TaskLogs.LogPriority.Low,
                    () => ParamBank.RefreshAllParamDiffCaches(false)));
            }
        }
    }
}
