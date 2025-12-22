using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public partial class ParamTools
{
    public ProjectEntry ParamMerge_TargetProject;

    public bool ParamMerge_TargetUniqueOnly = false;

    public bool ParamMerge_InProgress = false;

    public bool DisplayParamToggles = true;
    public string TargetParamFilter = "";
    public Dictionary<string, bool> TargetParams = new();

    public void DisplayParamMerge()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);

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
                if (!Project.ParamData.AuxBanks.ContainsKey(ParamMerge_TargetProject.ProjectName))
                {
                    if (ImGui.Button("Load##action_Load", DPI.HalfWidthButton(windowWidth, 24)))
                    {
                        Task<bool> loadTask = Editor.Project.ParamData.SetupAuxBank(ParamMerge_TargetProject, true);
                        Task.WaitAll(loadTask);
                        Editor.Project.ParamData.RefreshParamDifferenceCacheTask(true);
                        TargetParams = new();
                    }

                    ImGui.SameLine();
                    ImGui.BeginDisabled();
                    if (ImGui.Button("Merge##action_MergeParam", DPI.HalfWidthButton(windowWidth, 24)))
                    {
                    }
                    ImGui.EndDisabled();
                }
                else
                {
                    ImGui.BeginDisabled();
                    if (ImGui.Button("Load##action_Load", DPI.HalfWidthButton(windowWidth, 24)))
                    {
                    }
                    ImGui.EndDisabled();

                    ImGui.SameLine();
                    // Merge
                    if (ParamMerge_InProgress)
                    {
                        ImGui.BeginDisabled();
                        if (ImGui.Button("Merge##action_MergeParam", DPI.HalfWidthButton(windowWidth, 24)))
                        {
                        }
                        ImGui.EndDisabled();
                    }
                    else if (!ParamMerge_InProgress)
                    {
                        if (ImGui.Button("Merge##action_MergeParam", DPI.HalfWidthButton(windowWidth, 24)))
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

            // Target Params
            UIHelper.ConditionalHeader("targetParamOptions", "Target Params", "The params to merge.", UI.Current.ImGui_AliasName_Text, ref DisplayParamToggles);

            // Generate bool dict once
            if (TargetParams.Count == 0)
            {
                foreach (var entry in Project.ParamData.PrimaryBank.Params)
                {
                    TargetParams.Add(entry.Key, true);
                }
            }

            if (DisplayParamToggles)
            {
                ImGui.InputText("##paramToggleFilter", ref TargetParamFilter, 255);

                ImGui.SameLine();
                if (ImGui.Button("Toggle All", DPI.StandardButtonSize))
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

                    if(TargetParamFilter != "")
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

        var auxBank = Editor.Project.ParamData.AuxBanks[ParamMerge_TargetProject.ProjectName];

        // ParamSearchEngine: auxparam {ParamMerge_TargetProject.ProjectName}
        // RowSearchEngine: modified && unique ID:
        // MERowOperation: paste

        foreach(var entry in TargetParams)
        {
            if (entry.Value)
            {
                var command = $"auxparam {ParamMerge_TargetProject.ProjectName} {entry.Key}: modified ID: paste;";

                if (ParamMerge_TargetUniqueOnly)
                {
                    command = $"auxparam {ParamMerge_TargetProject.ProjectName} {entry.Key}: modified && unique ID: paste;";
                }

                Editor.MassEditHandler.ApplyMassEdit(command);
            }
        }

        ParamMerge_InProgress = false;
    }
}
