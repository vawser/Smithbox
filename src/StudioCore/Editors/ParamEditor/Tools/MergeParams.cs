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

    public bool ParamMerge_TargetUniqueOnly = true;

    public bool ParamMerge_InProgress = false;

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
            UIHelper.WrappedText("This process is 'simple', and thus may produce a broken mod if you attempt to merge complex mods.");
            UIHelper.WrappedText("");

            UIHelper.SimpleHeader("availableProjects", "Compatible Projects:", "List of projects you can merge into your current project", UI.Current.ImGui_AliasName_Text);
            UIHelper.WrappedText("");

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

            if(ParamMerge_TargetProject == null || ParamMerge_InProgress)
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

        // Apply the merge massedit script here
        var command = $"auxparam {ParamMerge_TargetProject.ProjectName} .*: modified && unique ID: paste;";

        if (!ParamMerge_TargetUniqueOnly)
        {
            command = $"auxparam {ParamMerge_TargetProject.ProjectName} .*: modified ID: paste;";
        }

        Editor.MassEditHandler.ApplyMassEdit(command);
    }
}
