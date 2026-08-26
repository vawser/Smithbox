using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class ModelToolWindow
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public ModelDataTransferTool DataTransferTool;

    public ModelToolWindow(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        DataTransferTool = new(view, project);
    }

    public void DisplayDropdown()
    {
        // Tools
        if (ImGui.BeginMenu($"{LOC.Get("MODEL_ToolWindow_Tools_Header")}##toolsMenuHeader"))
        {
            DataTransferTool.DisplayDropdown();

            ImGui.EndMenu();
        }
    }

    public void Display()
    {
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration)
        {
            View.ModelGridTool.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder)
        {
            View.ModelInstanceFinder.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler)
        {
            View.ModelMaskToggler.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ResourceMonitor)
        {
            View.ResourceListTool.Display("modelEditor", View.Universe);
        }
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("MODEL_ToolWindow_View_Header")}##viewMenuHeader"))
        {
            // Grid Configuration
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Grid_Configuration")}##toggleGridConfigTool"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration = !CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration);

            // Model Instance Finder
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Instance_Finder")}##toggleInstanceFinder"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder = !CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder);

            // Model Mask Toggler
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Mask_Toggler")}##modelMaskToggler"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler = !CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler);

            // Resource Monitor
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Resource_Monitor")}##resourceMonitorToggle"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ResourceMonitor = !CFG.Current.Interface_ModelEditor_Tool_ResourceMonitor;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ResourceMonitor);

            ImGui.EndMenu();
        }
    }
}


