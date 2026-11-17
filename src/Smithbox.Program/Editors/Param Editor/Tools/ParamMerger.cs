using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using StudioCore.Application;
using StudioCore.Editors.Common;
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
    public ParamEditorView View;
    public ProjectEntry Project;

    public ProjectEntry ParamMerge_TargetProject;

    public bool ParamMerge_TargetUniqueOnly = false;

    public bool ParamMerge_InProgress = false;

    public bool DisplayParamToggles = true;
    public string TargetParamFilter = "";
    public Dictionary<string, bool> TargetParams = new();

    public bool ParamMerge_IncludeAdded = true;
    public bool ParamMerge_IncludeModified = true;

    public ParamMerger(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }


    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);

        // Merge Params
        if (ToolSections.Header($"{LOC.Get("PARAM_Merger_Header")}##paramMergerHeader"))
        {
            ImGui.BeginChild("ParamMergerToolSection", ImGuiChildFlags.Borders);

            if (ImGui.BeginTabBar("mergerTabs"))
            {
                if (ImGui.BeginTabItem($"{LOC.Get("PARAM_Merger_Direct_Merger_Header")}##directTab"))
                {
                    GUI.WrappedText(LOC.Get("PARAM_Merger_Direct_Merger_Hint"));

                    GUI.Spacer();
                    View.ToolMenu.DeltaPatcher.AutoMergeTool.DisplayConflictPolicy();
                    View.ToolMenu.DeltaPatcher.AutoMergeTool.DisplayDirectRegulationMerge();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{LOC.Get("PARAM_Merger_MassEdit_Merger_Header")}##massEditTab"))
                {
                    DisplayMassEditMerger();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.EndChild();
        }
    }

    public void DisplayMassEditMerger()
    {
        var paramData = Project.Handler.ParamData;

        GUI.WrappedText(LOC.Get("PARAM_Merger_Hint"));
        GUI.Spacer();

        // Compatible Projects
        GUI.SimpleHeader(
            LOC.Get("PARAM_Merger_Header_Compatible_Projects"),
            LOC.Get("PARAM_Merger_Header_Compatible_Projects_TT"));

        var projectList = Smithbox.Orchestrator.Projects;

        ImGui.BeginChild("ProjectListSection", new Vector2(0, 200f), ImGuiChildFlags.Borders);

        foreach (var proj in projectList)
        {
            if (proj == null)
                continue;

            if (proj.Descriptor.ProjectType != View.Project.Descriptor.ProjectType)
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

            if (isSelected)
            {
                if (ImGui.BeginPopupContextWindow("targetProjectContext"))
                {
                    if (ImGui.Selectable($"{LOC.Get("PARAM_Merger_Action_Load_Project")}##loadProject"))
                    {
                        LoadTargetProject(proj);
                    }

                    ImGui.EndPopup();
                }
            }
        }

        ImGui.EndChild();

        GUI.ConditionalMultiButtonInput("mergeActions",
            "loadProject",
            LOC.Get("PARAM_Merger_Action_Load_Project"),
            LOC.Get("PARAM_Merger_Action_Load_Project_TT"),
            LoadProjectAction,
            true,

            "mergeProject",
            LOC.Get("PARAM_Merger_Action_Merge_Project"),
            LOC.Get("PARAM_Merger_Action_Merge_Project_TT"),
            MergeParamsAction,
            CanMergeProject());

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_Merger_Header_Options"),
            LOC.Get("PARAM_Merger_Header_Options_TT"));

        // Include Added
        ImGui.Checkbox($"{LOC.Get("PARAM_Merger_Checkbox_Include_Added")}##toggleIncludeAdded",
            ref ParamMerge_IncludeAdded);
        GUI.Tooltip(LOC.Get("PARAM_Merger_Checkbox_Include_Added_TT"));

        // Include Modified
        ImGui.Checkbox($"{LOC.Get("PARAM_Merger_Checkbox_Include_Modified")}##toggleIncludeModified",
            ref ParamMerge_IncludeModified);
        GUI.Tooltip(LOC.Get("PARAM_Merger_Checkbox_Include_Modified_TT"));

        // Target Params
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("PARAM_Merger_Header_Target_Params"),
            LOC.Get("PARAM_Merger_Header_Target_Params_TT"),
            ref DisplayParamToggles);

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
            GUI.HintTextInput("paramToggleFilter", ref TargetParamFilter, LOC.Get("PARAM_Merger_Param_Filter_Hint"));

            GUI.MultiButtonInput("paramToggleActions",
                "toggleAllParams",
                LOC.Get("PARAM_Merger_Action_Toggle_All_Params"),
                LOC.Get("PARAM_Merger_Action_Toggle_All_Params_TT"),
                ToggleParamsAction);

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
        if (ParamMerge_TargetProject == null)
            return;

        LoadTargetProject(ParamMerge_TargetProject);
    }

    public void LoadTargetProject(ProjectEntry project)
    {
        var paramData = Project.Handler.ParamData;

        if(paramData == null)
        {
            Smithbox.LogError<ParamMerger>(LOC.Get("PARAM_Merger_Log_Param_Data_Not_Loaded"));
            return;
        }

        Task<bool> loadTask = paramData.SetupAuxBank(project, true);

        Task.WaitAll(loadTask);

        paramData.RefreshParamDifferenceCacheTask(true);
        TargetParams = new();

        Smithbox.Log<ParamMerger>(LOC.Get("PARAM_Merger_Log_Loaded_Project", project.Descriptor.ProjectName));
    }

    public bool CanMergeProject()
    {
        if (ParamMerge_InProgress)
            return false;

        if (ParamMerge_TargetProject == null)
            return false;

        var paramData = View.Project.Handler.ParamData;

        if (paramData == null)
            return false;

        if (!paramData.AuxBanks.ContainsKey(ParamMerge_TargetProject.Descriptor.ProjectName))
        {
            return false;
        }

        return true;
    }

    public void MergeParamsAction()
    {
        if (ParamMerge_InProgress)
            return;

        if (ParamMerge_TargetProject == null)
        {
            Smithbox.Log<ParamMerger>(LOC.Get("PARAM_Merger_Log_No_Target_Project"));
            return;
        }

        var paramData = View.Project.Handler.ParamData;

        if(paramData == null)
        {
            return;
        }

        if (!paramData.AuxBanks.TryGetValue(ParamMerge_TargetProject.Descriptor.ProjectName, out var auxBank))
        {
            Smithbox.Log<ParamMerger>(LOC.Get("PARAM_Merger_Log_Project_Not_Loaded", ParamMerge_TargetProject.Descriptor.ProjectName));
            return;
        }

        ParamMerge_InProgress = true;

        // Row table params are merged as whole groups: pasting rows one at a time replaces the first row with
        // the ID each time, which can't tell apart rows that share an ID
        var tableParams = TargetParams
            .Where(e => e.Value && ParamRowGroups.IsTableParam(Project, e.Key))
            .Select(e => e.Key)
            .ToHashSet();

        MergeTableParams(auxBank, tableParams);

        // ParamSearchEngine: auxparam {ParamMerge_TargetProject.ProjectName}
        // RowSearchEngine: modified && unique ID:
        // MERowOperation: paste

        // Added
        if (ParamMerge_IncludeAdded)
        {
            foreach (var entry in TargetParams)
            {
                if (entry.Value && !tableParams.Contains(entry.Key))
                {
                    var command = $"auxparam {ParamMerge_TargetProject.Descriptor.ProjectName} {entry.Key}: added: paste;";

                    View.MassEdit.ApplyMassEdit(command);
                }
            }
        }
        if (ParamMerge_IncludeModified)
        {
            foreach (var entry in TargetParams)
            {
                if (entry.Value && !tableParams.Contains(entry.Key))
                {
                    var command = $"auxparam {ParamMerge_TargetProject.Descriptor.ProjectName} {entry.Key}: auxmodified: paste;";

                    if(!ParamMerge_IncludeAdded)
                    {
                        command = $"auxparam {ParamMerge_TargetProject.Descriptor.ProjectName} {entry.Key}: !added && auxmodified: paste;";
                    }

                    View.MassEdit.ApplyMassEdit(command);
                }
            }
        }

        ParamMerge_InProgress = false;

        Smithbox.Log<ParamMerger>(LOC.Get("PARAM_Merger_Log_Merged_Project", ParamMerge_TargetProject.Descriptor.ProjectName));
    }

    /// <summary>
    /// Copies each changed group of the target project into this project, as one undoable action.
    /// Added: the ID isn't in vanilla. Modified: the target's group differs from vanilla.
    /// A group already equal to this project's group is skipped. Deleted groups aren't merged, the same as
    /// rows in the Mass Edit merge.
    /// </summary>
    private void MergeTableParams(ParamBank auxBank, HashSet<string> tableParams)
    {
        var paramData = View.Project.Handler.ParamData;
        var actions = new List<EditorAction>();
        var groupCount = 0;

        foreach (var paramKey in tableParams)
        {
            if (!auxBank.Params.TryGetValue(paramKey, out var auxParam) ||
                !paramData.PrimaryBank.Params.TryGetValue(paramKey, out var primaryParam))
                continue;

            paramData.VanillaBank.Params.TryGetValue(paramKey, out var vanillaParam);

            var vanillaGroups = ParamRowGroups.Build(vanillaParam);
            var primaryGroups = ParamRowGroups.Build(primaryParam);
            var changes = new List<(int ID, List<Param.Row> NewRows)>();

            foreach (var (id, auxRows) in ParamRowGroups.Build(auxParam))
            {
                if (vanillaGroups.TryGetValue(id, out var vanillaRows))
                {
                    if (!ParamMerge_IncludeModified || ParamRowGroups.GroupEquals(auxRows, vanillaRows))
                        continue;
                }
                else if (!ParamMerge_IncludeAdded)
                {
                    continue;
                }

                if (primaryGroups.TryGetValue(id, out var primaryRows) && ParamRowGroups.GroupEquals(auxRows, primaryRows))
                    continue;

                changes.Add((id, auxRows.Select(r => new Param.Row(r, primaryParam)).ToList()));
            }

            if (changes.Count == 0)
                continue;

            actions.Add(new ReplaceRowGroupsAction(View.Editor, primaryParam, changes));
            groupCount += changes.Count;

            Smithbox.Log<ParamMerger>(LOC.Get("PARAM_Merger_Log_Merged_Groups", changes.Count, paramKey));
        }

        if (actions.Count == 0)
            return;

        var compound = new CompoundAction(actions);
        compound.DisplayName = LOC.Get("PARAM_Merger_Action_Merge_Groups", groupCount);

        View.Editor.ActionManager.ExecuteAction(compound);
    }
}
