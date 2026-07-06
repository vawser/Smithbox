using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class VisibilityActionTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    public VisibilityActionTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        ImGui.BeginChild("VisActionSection", ImGuiChildFlags.Borders);

        ImGui.BeginTabBar("visActionTabs");

        ViewportFiltersTab();
        EditorVisibilityTab();
        GameVisibilityTab();

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    public void ViewportFiltersTab()
    {
        if (ImGui.BeginTabItem("Viewport Filters##viewportVisTab"))
        {
            View.ViewportFiltersAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void EditorVisibilityTab()
    {
        if (ImGui.BeginTabItem("Editor Visibility##editorVisTab"))
        {
            View.EditorVisibilityAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void GameVisibilityTab()
    {
        if (ImGui.BeginTabItem("Game Visibility##gameVisTab"))
        {
            View.GameVisibilityAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }
}
