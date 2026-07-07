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
        if (ImGui.BeginMenu("Tools"))
        {
            DataTransferTool.DisplayDropdown();

            ImGui.EndMenu();
        }
    }

    public void Display()
    {
        if (CFG.Current.Interface_ModelEditor_ScreenshotMode)
            return;

        if (!CFG.Current.Interface_ModelEditor_ToolWindow)
            return;

        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ImGui.EndMenuBar();
        }

        //if (CFG.Current.MaterialEditor_Tool_Data_Transfer)
        //{
        //    if (ImGui.CollapsingHeader("Data Transfer"))
        //    {
        //        DataTransferTool.Display();
        //    }
        //}

        if (CFG.Current.Interface_ModelEditor_Tool_CreateAction)
        {
            View.CreateAction.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration)
        {
            View.ModelGridTool.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelInsight)
        {
            //View.ModelInsightMenu.OnToolWindow();
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
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Data Transfer"))
            {
                CFG.Current.MaterialEditor_Tool_Data_Transfer = !CFG.Current.MaterialEditor_Tool_Data_Transfer;
            }
            GUI.ShowActiveStatus(CFG.Current.MaterialEditor_Tool_Data_Transfer);

            if (ImGui.MenuItem("Create"))
            {
                CFG.Current.Interface_ModelEditor_Tool_CreateAction = !CFG.Current.Interface_ModelEditor_Tool_CreateAction;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_CreateAction);

            if (ImGui.MenuItem("Model Grid Configuration"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration = !CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration);

            if (ImGui.MenuItem("Model Insight"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelInsight = !CFG.Current.Interface_ModelEditor_Tool_ModelInsight;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelInsight);

            if (ImGui.MenuItem("Model Instance Finder"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder = !CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder);

            if (ImGui.MenuItem("Model Mask Toggler"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler = !CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler);

            ImGui.EndMenu();
        }
    }
}


