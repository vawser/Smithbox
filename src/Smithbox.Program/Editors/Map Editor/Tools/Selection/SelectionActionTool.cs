using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class SelectActionTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    public SelectActionTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        ImGui.BeginChild("SelectActionSection", ImGuiChildFlags.Borders);

        ImGui.BeginTabBar("selectActionTabs");

        SelectionGroupTab();
        SelectAllTab();
        SelectCollisionReferenceTab();
        BoxSelectionTab();

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    public void SelectionGroupTab()
    {
        if (ImGui.BeginTabItem("Selection Groups##selectionGroupTab"))
        {
            View.SelectionGroupTool.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void SelectAllTab()
    {
        if (ImGui.BeginTabItem("Mass Selection##selectAllTab"))
        {
            View.SelectAllAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void SelectCollisionReferenceTab()
    {
        if (!(Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S))
        {
            if (ImGui.BeginTabItem("Collision Selection##selectColRefTab"))
            {
                View.SelectCollisionRefAction.OnToolWindow();

                ImGui.EndTabItem();
            }
        }
    }

    public void BoxSelectionTab()
    {
        if (ImGui.BeginTabItem("Box Selection##selectBoxTab"))
        {
            View.BoxSelectionAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }
}
