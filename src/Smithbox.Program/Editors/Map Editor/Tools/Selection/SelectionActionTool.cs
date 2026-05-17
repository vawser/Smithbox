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
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public SelectActionTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        var activeView = Editor.ViewHandler.ActiveView;

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
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.SelectionGroupTool.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void SelectAllTab()
    {
        if (ImGui.BeginTabItem("Mass Selection##selectAllTab"))
        {
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.SelectAllAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void SelectCollisionReferenceTab()
    {
        if (!(Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S))
        {
            if (ImGui.BeginTabItem("Collision Selection##selectColRefTab"))
            {
                var activeView = Editor.ViewHandler.ActiveView;
                activeView.SelectCollisionRefAction.OnToolWindow();

                ImGui.EndTabItem();
            }
        }
    }

    public void BoxSelectionTab()
    {
        if (ImGui.BeginTabItem("Box Selection##selectBoxTab"))
        {
            var activeView = Editor.ViewHandler.ActiveView;
            activeView.BoxSelectionAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }
}
