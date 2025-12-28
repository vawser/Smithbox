using Hexa.NET.ImGui;
using StudioCore.Application;
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

    public void OnGui()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##modelEditorTools", ImGuiWindowFlags.MenuBar))
        {
            Editor.FocusManager.SwitchModelEditorContext(ModelEditorContext.ToolWindow);

            var windowHeight = ImGui.GetWindowHeight();
            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_CreateAction)
            {
                Editor.CreateAction.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration)
            {
                Editor.ModelGridTool.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelInsight)
            {
                Editor.ModelInsightTool.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder)
            {
                Editor.ModelInstanceFinder.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler)
            {
                Editor.ModelMaskToggler.OnToolWindow();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
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


