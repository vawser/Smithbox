using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;

public partial class ParamTools
{
    public ProjectEntry ParamMerge_TargetProject;

    public bool ParamMerge_TargetUniqueOnly = false;

    public bool ParamMerge_InProgress = false;

    public List<string> TargetedParams = new();

    public async void DisplayParamMerge()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
        var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);
        var thirdButtonSize = new Vector2(windowWidth * 0.975f / 3, 32);
        var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);
        var inputButtonSize = new Vector2((windowWidth * 0.225f), 32);

        // Merge Params
        if (ImGui.CollapsingHeader("Merge Params"))
        {
            UIHelper.WrappedText("Select a compatible project below to merge into your current project.");
            UIHelper.WrappedText("");
            UIHelper.WrappedText("You will need to create a project for the external mod first, it will then appear below.");
            UIHelper.WrappedText("");

            UIHelper.SimpleHeader("availableProjects", "Compatible Projects:", "List of projects you can merge into your current project", UI.Current.ImGui_AliasName_Text);

            foreach (var proj in Editor.Project.BaseEditor.ProjectManager.Projects)
            {
                if (proj == null)
                    continue;

                if (proj.ProjectType != Editor.Project.ProjectType)
                    continue;

                if (proj == Editor.Project.BaseEditor.ProjectManager.SelectedProject)
                    continue;

                var isSelected = false;

                if (ParamMerge_TargetProject != null)
                {
                    isSelected = ParamMerge_TargetProject.ProjectName == proj.ProjectName;
                }

                    
                if (ImGui.Selectable($"{proj.ProjectName}", isSelected))
                {
                    ParamMerge_TargetProject = proj;
                }
            }

            UIHelper.WrappedText("");

            /// Targeted Param
            UIHelper.SimpleHeader("paramTargets", "Targeted Params", "Leave blank to target all params.", UI.Current.ImGui_AliasName_Text);

            // Add
            if (ImGui.Button($"{Icons.Plus}##paramTargetAdd_paramMerge"))
            {
                TargetedParams.Add("");
            }
            UIHelper.Tooltip("Add new param target input row.");

            ImGui.SameLine();

            // Remove
            if (TargetedParams.Count < 2)
            {
                ImGui.BeginDisabled();

                if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_paramMerge"))
                {
                    TargetedParams.RemoveAt(TargetedParams.Count - 1);
                }
                UIHelper.Tooltip("Remove last added param target input row.");

                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_paramMerge"))
                {
                    TargetedParams.RemoveAt(TargetedParams.Count - 1);
                    UIHelper.Tooltip("Remove last added param target input row.");
                }
            }

            ImGui.SameLine();

            // Reset
            if (ImGui.Button("Reset##paramTargetReset_paramMerge"))
            {
                TargetedParams = new List<string>();
            }
            UIHelper.Tooltip("Reset param target input rows.");

            for (int i = 0; i < TargetedParams.Count; i++)
            {
                var curCommand = TargetedParams[i];
                var curText = curCommand;

                ImGui.SetNextItemWidth(400f);
                if (ImGui.InputText($"##paramTargetInput{i}_paramMerge", ref curText, 255))
                {
                    TargetedParams[i] = curText;
                }
                UIHelper.Tooltip("The param target to include.");
            }

            UIHelper.WrappedText("");

            // Options
            UIHelper.SimpleHeader("mergeOptions", "Options", "Options to apply when merging.", UI.Current.ImGui_AliasName_Text);
            ImGui.Checkbox("Merge Unique Rows only", ref ParamMerge_TargetUniqueOnly);

            UIHelper.WrappedText("");

            if (ParamMerge_TargetProject == null || ParamMerge_InProgress)
            {
                ImGui.BeginDisabled();
                if (ImGui.Button("Merge##action_MergeParam", defaultButtonSize))
                {
                }
                ImGui.EndDisabled();
            }
            else if(!ParamMerge_InProgress)
            {
                if (ImGui.Button("Merge##action_MergeParam", defaultButtonSize))
                {
                    ParamMerge_InProgress = true;

                    await Editor.Project.ParamData.SetupAuxBank(ParamMerge_TargetProject, true);

                    MergeParamHandler();

                    ParamMerge_InProgress = false;
                }
            }
        }
    }

    public void MergeParamHandler()
    {
        var auxBank = Editor.Project.ParamData.AuxBanks[ParamMerge_TargetProject.ProjectName];

        // ParamSearchEngine: auxparam {ParamMerge_TargetProject.ProjectName}
        // RowSearchEngine: modified && unique ID:
        // MERowOperation: paste

        // Apply the merge massedit script here
        var command = $"auxparam {ParamMerge_TargetProject.ProjectName} .*: modified && unique ID: paste;";

        if (!ParamMerge_TargetUniqueOnly)
        {
            command = $"auxparam {ParamMerge_TargetProject.ProjectName} .*: modified ID: paste;";
        }

        Editor.MassEditHandler.ApplyMassEdit(command);
    }
}
