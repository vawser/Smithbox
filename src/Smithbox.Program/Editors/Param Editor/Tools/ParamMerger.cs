using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
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
            UIHelper.WrappedText("Select a compatible project below to merge into your current project.");
            UIHelper.WrappedText("");
            UIHelper.WrappedText("You will need to create a project for the external mod first, it will then appear below.");
            UIHelper.WrappedText("");

            if (ParamMerge_TargetProject != null)
            {
                // Load
                if (!paramData.AuxBanks.ContainsKey(ParamMerge_TargetProject.Descriptor.ProjectName))
                {
                    if (ImGui.Button("Load##action_Load"))
                    {
                        Task<bool> loadTask = paramData.SetupAuxBank(ParamMerge_TargetProject, true);

                        Task.WaitAll(loadTask);

                        paramData.RefreshParamDifferenceCacheTask(true);
                        TargetParams = new();
                    }

                    ImGui.SameLine();
                    ImGui.BeginDisabled();
                    if (ImGui.Button("Merge##action_MergeParam"))
                    {
                    }
                    ImGui.EndDisabled();
                }
                else
                {
                    ImGui.BeginDisabled();
                    if (ImGui.Button("Load##action_Load"))
                    {
                    }
                    ImGui.EndDisabled();

                    ImGui.SameLine();
                    // Merge
                    if (ParamMerge_InProgress)
                    {
                        ImGui.BeginDisabled();
                        if (ImGui.Button("Merge##action_MergeParam"))
                        {
                        }
                        ImGui.EndDisabled();
                    }
                    else if (!ParamMerge_InProgress)
                    {
                        if (ImGui.Button("Merge##action_MergeParam"))
                        {
                            MergeParamHandler();
                        }
                    }
                }
            }
            else
            {
                ImGui.Text("Select a project from below.");
            }

            UIHelper.SimpleHeader("Compatible Projects:", "List of projects you can merge into your current project");

            var projectList = Smithbox.Orchestrator.Projects;

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

            UIHelper.WrappedText("");

            // Options
            UIHelper.SimpleHeader("Options", "Options to apply when merging.");

            ImGui.Checkbox("Merge Unique Row IDs only", ref ParamMerge_TargetUniqueOnly);
            UIHelper.Tooltip("If enabled, rows where the ID is unique will be merged.");

            UIHelper.WrappedText("");

            // Target Params
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
                ImGui.InputText("##paramToggleFilter", ref TargetParamFilter, 255);

                ImGui.SameLine();
                if (ImGui.Button("Toggle All"))
                {
                    foreach (var param in TargetParams)
                    {
                        TargetParams[param.Key] = !TargetParams[param.Key];
                    }
                }

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
            }
            else
            {
                ImGui.Text("...");
            }

            UIHelper.WrappedText("");
        }
    }

    public void MergeParamHandler()
    {
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
