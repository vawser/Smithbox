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
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public CommonActionTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        var activeView = Editor.ViewHandler.ActiveView;

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
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.CreateAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void DuplicateTab()
    {
        if (ImGui.BeginTabItem("Duplicate##DuplicateTab"))
        {
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.DuplicateAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void DuplicateToMapTab()
    {
        if (ImGui.BeginTabItem("Duplicate to Map##DuplicateToMapTab"))
        {
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.DuplicateToMapAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void PullToCameraTab()
    {
        if (ImGui.BeginTabItem("Pull to Camera##PullToCameraTab"))
        {
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.PullToCameraAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void TranslateTab()
    {
        if (ImGui.BeginTabItem("Translate##TranslateTab"))
        {
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.TranslateAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void RotateTab()
    {
        if (ImGui.BeginTabItem("Rotate##RotateTab"))
        {
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.RotateAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void ScrambleTab()
    {
        if (ImGui.BeginTabItem("Scramble##ScrambleTab"))
        {
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.ScrambleAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void ReplicateTab()
    {
        if (ImGui.BeginTabItem("Replicate##ReplicateTab"))
        {
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.ReplicateAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }
}
