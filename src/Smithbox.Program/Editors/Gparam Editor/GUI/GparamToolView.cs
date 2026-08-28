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
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_GparamEditor_Tool_DataTransfer)
        {
            // Data Transfer
            if (ImGui.CollapsingHeader($"{LOC.Get("GPARAM_Tools_Data_Transfer")}##dataTransferHeader"))
            {
                DataTransferTool.Display();
            }
        }

        if (CFG.Current.Interface_GparamEditor_Tool_Finder)
        {
            // Data Finder
            if (ImGui.CollapsingHeader($"{LOC.Get("GPARAM_Tools_Data_Finder")}##dataFinderHeader"))
            {
                DataFinder.Display();
            }
        }

        if (CFG.Current.Interface_GparamEditor_Tool_QuickEdit)
        {
            // Quick Edit
            if (ImGui.CollapsingHeader($"{LOC.Get("GPARAM_Tools_Quick_Edit")}##quickEditHeader"))
            {
                View.QuickEditHandler.DisplayInputWindow();
            }

            // Quick Edit
            if (ImGui.CollapsingHeader($"{LOC.Get("GPARAM_Tools_Quick_Edit_Commands")}##quickEditCommandsHeader"))
            {
                QuickEditCheatsheet.Display();
            }
        }
    }

    public void DisplayDropdown()
    {
        if (ImGui.BeginMenu($"{LOC.Get("GPARAM_Tools_Menu")}##toolsHeader"))
        {
            DataTransferTool.DisplayDropdown();

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("GPARAM_Tools_View_Header")}##viewHeader"))
        {
            // Data Transfer
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_Tools_View_Data_Transfer")}##dataTransferToggle"))
            {
                CFG.Current.Interface_GparamEditor_Tool_DataTransfer = !CFG.Current.Interface_GparamEditor_Tool_DataTransfer;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_DataTransfer);

            // Quick Edit
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_Tools_View_Quick_Edit")}##quickEditToggle"))
            {
                CFG.Current.Interface_GparamEditor_Tool_QuickEdit = !CFG.Current.Interface_GparamEditor_Tool_QuickEdit;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_QuickEdit);

            // Data Finder
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_Tools_View_Data_Finder")}##dataFinderToggle"))
            {
                CFG.Current.Interface_GparamEditor_Tool_Finder = !CFG.Current.Interface_GparamEditor_Tool_Finder;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_Finder);

            ImGui.EndMenu();
        }
    }
}
