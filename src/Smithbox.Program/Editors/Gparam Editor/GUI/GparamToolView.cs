using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public class GparamToolView
{
    private GparamEditorView View;
    private ProjectEntry Project;

    public GparamDataFinder DataFinder;
    public GparamDataTransferTool DataTransferTool;

    public GparamToolView(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        DataFinder = new(view, project);
        DataTransferTool = new(view, project);
    }

    public void Display()
    {
        if (!CFG.Current.Interface_GparamEditor_ToolWindow)
            return;

        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_GparamEditor_Tool_DataTransfer)
        {
            if (ImGui.CollapsingHeader("Data Transfer"))
            {
                DataTransferTool.Display();
            }
        }

        if (CFG.Current.Interface_GparamEditor_Tool_Finder)
        {
            if (ImGui.CollapsingHeader("Data Finder"))
            {
                DataFinder.Display();
            }
        }

        if (CFG.Current.Interface_GparamEditor_Tool_QuickEdit)
        {
            if (ImGui.CollapsingHeader("Quick Edit"))
            {
                View.QuickEditHandler.DisplayInputWindow();
            }

            if (ImGui.CollapsingHeader("Quick Edit Commands"))
            {
                QuickEditCheatsheet.Display();
            }
        }
    }

    public void DisplayDropdown()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            DataTransferTool.DisplayDropdown();

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Quick Edit"))
            {
                CFG.Current.Interface_GparamEditor_Tool_QuickEdit = !CFG.Current.Interface_GparamEditor_Tool_QuickEdit;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_QuickEdit);

            if (ImGui.MenuItem("Finder"))
            {
                CFG.Current.Interface_GparamEditor_Tool_Finder = !CFG.Current.Interface_GparamEditor_Tool_Finder;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_Finder);

            ImGui.EndMenu();
        }
    }
}
