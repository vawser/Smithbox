using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.GparamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class ParamMerger
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ProjectEntry ParamMerge_TargetProject;

    public bool ParamMerge_TargetUniqueOnly = false;

    public bool ParamMerge_InProgress = false;

    public bool DisplayParamToggles = true;
    public string TargetParamFilter = "";
    public Dictionary<string, bool> TargetParams = new();

    public ParamMerger(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }


    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);

        var paramData = Project.Handler.ParamData;

        // Merge Params
        if (ImGui.CollapsingHeader("Param Merger"))
        {
            ImGui.BeginChild("ParamMergerToolSection", ImGuiChildFlags.Borders);

            UIHelper.WrappedText("Select a compatible project below to merge into your current project.");
            UIHelper.WrappedText("You will need to create a project for the external mod first, it will then appear below.");
            UIHelper.WrappedText("");

            UIHelper.SimpleHeader("Compatible Projects:", "List of projects you can merge into your current project");

            var projectList = Smithbox.Orchestrator.Projects;

            ImGui.BeginChild("ProjectListSection", new Vector2(0, 200f), ImGuiChildFlags.Borders);

            foreach (var proj in projectList)
            {
                if (proj == null)
                    continue;

                if (proj.Descriptor.ProjectType != Editor.Project.Descriptor.ProjectType)
                    continue;

                if (proj == Smithbox.Orchestrator.SelectedProject)
                    continue;

                var isSelected = false;

                if (ParamMerge_TargetProject != null)
                {
                    isSelected = ParamMerge_TargetProject.Descriptor.ProjectName == proj.Descriptor.ProjectName;
                }

                if (ImGui.Selectable($"{proj.Descriptor.ProjectName}", isSelected))
                {
                    ParamMerge_TargetProject = proj;
                }
            }

            ImGui.EndChild();

            UIHelper.MultiButtonInput("mergeActions",
                "loadProject", "Load Selected Project", "", LoadProjectAction,
                "mergeProject", "Merge Selected Project", "Merges the params from the selected project into our current project.", MergeParamsAction);

            // Options
            UIHelper.WrappedText("");
            UIHelper.SimpleHeader("Options", "Options to apply when merging.");

            ImGui.Checkbox("Merge Unique Row IDs only", ref ParamMerge_TargetUniqueOnly);
            UIHelper.Tooltip("If enabled, rows where the ID is unique will be merged.");

            // Target Params
            UIHelper.WrappedText("");
            UIHelper.ConditionalHeader("Target Params", "The params to merge.", ref DisplayParamToggles);

            // Generate bool dict once
            if (TargetParams.Count == 0)
            {
                foreach (var entry in paramData.PrimaryBank.Params)
                {
                    TargetParams.Add(entry.Key, true);
                }
            }

            if (DisplayParamToggles)
            {
                UIHelper.SinglelineTextInput("paramToggleFilter", ref TargetParamFilter);

                UIHelper.MultiButtonInput("paramToggleActions",
                    "toggleAllParams", "Toggle All Params", "", ToggleParamsAction);

                ImGui.BeginChild("ParamToggleList", new Vector2(0, ImGui.GetContentRegionAvail().Y * 0.9f), ImGuiChildFlags.Borders);

                foreach (var param in TargetParams)
                {
                    var curTruth = param.Value;
                    var display = true;

                    if (TargetParamFilter != "")
                    {
                        var paramKey = param.Key.ToLower();
                        var filter = TargetParamFilter.ToLower();

                        display = false;

                        if (paramKey.Contains(filter))
                        {
                            display = true;
                        }
                    }

                    if (display)
                    {
                        ImGui.Checkbox($"{param.Key}##param_{param.Key}", ref curTruth);
                        if (ImGui.IsItemDeactivatedAfterEdit())
                        {
                            TargetParams[param.Key] = curTruth;
                        }
                    }
                }

                ImGui.EndChild();
            }
            else
            {
                ImGui.Text("...");
            }

            ImGui.EndChild();
        }
    }

    public void ToggleParamsAction()
    {
        foreach (var param in TargetParams)
        {
            TargetParams[param.Key] = !TargetParams[param.Key];
        }
    }

    public void LoadProjectAction()
    {
        var paramData = Project.Handler.ParamData;

        if (!paramData.AuxBanks.ContainsKey(ParamMerge_TargetProject.Descriptor.ProjectName))
        {
            Task<bool> loadTask = paramData.SetupAuxBank(ParamMerge_TargetProject, true);

            Task.WaitAll(loadTask);

            paramData.RefreshParamDifferenceCacheTask(true);
            TargetParams = new();
        }
    }

    public void MergeParamsAction()
    {
        if (ParamMerge_InProgress)
            return;

        ParamMerge_InProgress = true;

        var auxBank = Editor.Project.Handler.ParamData.AuxBanks[ParamMerge_TargetProject.Descriptor.ProjectName];

        // ParamSearchEngine: auxparam {ParamMerge_TargetProject.ProjectName}
        // RowSearchEngine: modified && unique ID:
        // MERowOperation: paste

        foreach (var entry in TargetParams)
        {
            if (entry.Value)
            {
                var command = $"auxparam {ParamMerge_TargetProject.Descriptor.ProjectName} {entry.Key}: modified ID: paste;";

                if (ParamMerge_TargetUniqueOnly)
                {
                    command = $"auxparam {ParamMerge_TargetProject.Descriptor.ProjectName} {entry.Key}: modified && unique ID: paste;";
                }

                Editor.ViewHandler.ActiveView.MassEdit.ApplyMassEdit(command);
            }
        }

        ParamMerge_InProgress = false;
    }
}
