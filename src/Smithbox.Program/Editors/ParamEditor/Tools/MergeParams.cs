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

    public void DisplayParamMerge()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth * 0.475f, 32);
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

            // Options
            UIHelper.SimpleHeader("mergeOptions", "Options", "Options to apply when merging.", UI.Current.ImGui_AliasName_Text);
            ImGui.Checkbox("Merge Unique Row IDs only", ref ParamMerge_TargetUniqueOnly);
            UIHelper.Tooltip("If enabled, rows where the ID is unique will be merged.");

            UIHelper.WrappedText("");

            if (ParamMerge_TargetProject != null)
            {
                // Load
                if (!Project.ParamData.AuxBanks.ContainsKey(ParamMerge_TargetProject.ProjectName))
                {
                    if (ImGui.Button("Load##action_Load", defaultButtonSize))
                    {
                        Task<bool> loadTask = Editor.Project.ParamData.SetupAuxBank(ParamMerge_TargetProject, true);
                        Task.WaitAll(loadTask);
                        Editor.Project.ParamData.RefreshParamDifferenceCacheTask(true);
                    }

                    ImGui.SameLine();
                    ImGui.BeginDisabled();
                    if (ImGui.Button("Merge##action_MergeParam", defaultButtonSize))
                    {
                    }
                    ImGui.EndDisabled();
                }
                else
                {
                    ImGui.BeginDisabled();
                    if (ImGui.Button("Load##action_Load", defaultButtonSize))
                    {
                    }
                    ImGui.EndDisabled();

                    ImGui.SameLine();
                    // Merge
                    if (ParamMerge_InProgress)
                    {
                        ImGui.BeginDisabled();
                        if (ImGui.Button("Merge##action_MergeParam", defaultButtonSize))
                        {
                        }
                        ImGui.EndDisabled();
                    }
                    else if (!ParamMerge_InProgress)
                    {
                        if (ImGui.Button("Merge##action_MergeParam", defaultButtonSize))
                        {
                            MergeParamHandler();
                        }
                    }
                }
            }
        }
    }

    public void MergeParamHandler()
    {
        ParamMerge_InProgress = true;

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

        ParamMerge_InProgress = false;
    }
}
