using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class ModelToolWindow
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    // Tools
    public ModelGridConfiguration ModelGridTool;
    public ModelInsightView ModelInsightMenu;
    public ModelInstanceFinder ModelInstanceFinder;
    public ModelMaskToggler ModelMaskToggler;
    public ModelInsightHelper ModelInsightHelper;

    // Actions
    public CreateAction CreateAction;
    public DuplicateAction DuplicateAction;
    public DeleteAction DeleteAction;
    public FrameAction FrameAction;
    public GotoAction GotoAction;
    public PullToCameraAction PullToCameraAction;
    public ReorderAction ReorderAction;


    public ModelToolWindow(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        ModelGridTool = new ModelGridConfiguration(Editor, Project);
        ModelInsightMenu = new ModelInsightView(Editor, Project);
        ModelInstanceFinder = new ModelInstanceFinder(Editor, Project);
        ModelMaskToggler = new ModelMaskToggler(Editor, Project);

        CreateAction = new CreateAction(Editor, Project);
        DuplicateAction = new DuplicateAction(Editor, Project);
        DeleteAction = new DeleteAction(Editor, Project);
        FrameAction = new FrameAction(Editor, Project);
        GotoAction = new GotoAction(Editor, Project);
        PullToCameraAction = new PullToCameraAction(Editor, Project);
        ReorderAction = new ReorderAction(Editor, Project);

        ModelInsightHelper = new ModelInsightHelper(Editor, Project);
    }

    public void Display()
    {
        if (CFG.Current.Interface_ModelEditor_ScreenshotMode)
            return;

        if (!CFG.Current.Interface_ModelEditor_ToolWindow)
            return;

        var activeView = Editor.ViewHandler.ActiveView;

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
                CreateAction.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration)
            {
                ModelGridTool.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelInsight)
            {
                ModelInsightMenu.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder)
            {
                ModelInstanceFinder.OnToolWindow();
            }

            if (CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler)
            {
                ModelMaskToggler.OnToolWindow();
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


