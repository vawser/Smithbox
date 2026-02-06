using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class ModelToolWindow
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public ModelToolWindow(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (CFG.Current.Interface_ModelEditor_ScreenshotMode)
            return;

        if (!CFG.Current.Interface_ModelEditor_ToolWindow)
            return;

        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (ImGui.Begin("Tool Window##modelEditorTools", UIHelper.GetMainWindowFlags()))
        {
            FocusManager.SetFocus(EditorFocusContext.ModelEditor_Tools);

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_CreateAction)
            {
                activeView.CreateAction.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration)
            {
                activeView.ModelGridTool.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelInsight)
            {
                activeView.ModelInsightMenu.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder)
            {
                activeView.ModelInstanceFinder.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler)
            {
                activeView.ModelMaskToggler.OnToolWindow();
            }
        }

        ImGui.End();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Create"))
            {
                CFG.Current.Interface_ModelEditor_Tool_CreateAction = !CFG.Current.Interface_ModelEditor_Tool_CreateAction;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_CreateAction);

            if (ImGui.MenuItem("Model Grid Configuration"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration = !CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration);

            if (ImGui.MenuItem("Model Insight"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelInsight = !CFG.Current.Interface_ModelEditor_Tool_ModelInsight;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelInsight);

            if (ImGui.MenuItem("Model Instance Finder"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder = !CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder);

            if (ImGui.MenuItem("Model Mask Toggler"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler = !CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler);

            ImGui.EndMenu();
        }
    }

    public void OnMenubar()
    {

    }
}


