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
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public VisibilityActionTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        var activeView = Editor.ViewHandler.ActiveView;

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
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.ViewportFiltersAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void EditorVisibilityTab()
    {
        if (ImGui.BeginTabItem("Editor Visibility##editorVisTab"))
        {
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.EditorVisibilityAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void GameVisibilityTab()
    {
        if (ImGui.BeginTabItem("Game Visibility##gameVisTab"))
        {
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.GameVisibilityAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }
}
