using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Editors;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Application;

public class ProjectToolWindow
{
    public ProjectScreen Editor;

    private ToolSections Sections = new();

    public ProjectToolWindow(ProjectScreen editor)
    {
        Editor = editor;
    }

    public void Draw()
    {

        if (ImGui.BeginMenuBar())
        {
            // View
            if (ImGui.BeginMenu($"{LOC.Get("PROJECT_Tools_Header_Tools")}##viewMenuHeader"))
            {
                // Project Merge
                if (ImGui.MenuItem($"{LOC.Get("PROJECT_Tools_View_Project_Merge")}##viewToggle_ProjectMerge"))
                {
                    CFG.Current.ProjectEditor_Show_Tool_ProjectMerge = !CFG.Current.ProjectEditor_Show_Tool_ProjectMerge;
                }
                GUI.ShowActiveStatus(CFG.Current.ProjectEditor_Show_Tool_ProjectMerge);

                ImGui.EndMenu();
            }

            ToolSections.OptionsMenu();

            ImGui.EndMenuBar();
        }

        Sections.Begin();

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (curProject != null && curProject.Initialized && curProject.Handler != null)
        {
            if (!CFG.Current.ProjectEditor_Show_Tool_ProjectMerge)
            {
                // Every tool is toggled off in the Tools menu
                GUI.WrappedText(LOC.Get("PROJECT_Tools_No_Tools_Enabled"));
            }
            else
            {
                var activeView = curProject.Handler.ParamEditor?.ViewHandler.ActiveView;

                if (activeView != null)
                {
                    DisplayProjectMerge(activeView);
                }
                else
                {
                    GUI.WrappedText(LOC.Get("PROJECT_Tools_Project_Merge_Requires_Param_Editor"));
                }
            }
        }
        else
        {
            GUI.WrappedText(LOC.Get("PROJECT_Tools_No_Selected_Project"));
        }

        Sections.End();
    }

    public void DisplayProjectMerge(ParamEditorView view)
    {
        if (ToolSections.Header($"{LOC.Get("PROJECT_Tools_Project_Merge")}##projectMergeHeader"))
        {
            ImGui.BeginChild("ProjectMergeSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("PARAM_ProjectMerge_Hint"));

            GUI.Spacer();
            view.ToolMenu.DeltaPatcher.AutoMergeTool.DisplayConflictPolicy();
            view.ToolMenu.DeltaPatcher.AutoMergeTool.DisplayFullModMerge();

            ImGui.EndChild();
        }
    }
}
