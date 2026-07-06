using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class CommonActionTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    public CommonActionTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        ImGui.BeginChild("CommonActionSection", ImGuiChildFlags.Borders);

        ImGui.BeginTabBar("commonActionTabs");

        CreateTab();
        TranslateTab();
        RotateTab();
        DuplicateTab();
        DuplicateToMapTab();
        PullToCameraTab();
        ReplicateTab();
        ScrambleTab();

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    public void CreateTab()
    {
        if (ImGui.BeginTabItem("Create##CreateTab"))
        {
            View.CreateAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void DuplicateTab()
    {
        if (ImGui.BeginTabItem("Duplicate##DuplicateTab"))
        {
            View.DuplicateAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void DuplicateToMapTab()
    {
        if (ImGui.BeginTabItem("Duplicate to Map##DuplicateToMapTab"))
        {
            View.DuplicateToMapAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void PullToCameraTab()
    {
        if (ImGui.BeginTabItem("Pull to Camera##PullToCameraTab"))
        {
            View.PullToCameraAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void TranslateTab()
    {
        if (ImGui.BeginTabItem("Translate##TranslateTab"))
        {
            View.TranslateAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void RotateTab()
    {
        if (ImGui.BeginTabItem("Rotate##RotateTab"))
        {
            View.RotateAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void ScrambleTab()
    {
        if (ImGui.BeginTabItem("Scramble##ScrambleTab"))
        {
            View.ScrambleAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void ReplicateTab()
    {
        if (ImGui.BeginTabItem("Replicate##ReplicateTab"))
        {
            View.ReplicateAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }
}
