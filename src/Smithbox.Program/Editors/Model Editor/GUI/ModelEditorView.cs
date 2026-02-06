using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorView
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public Sdl2Window Window;
    public GraphicsDevice Device;
    public RenderScene RenderScene;

    public ViewportActionManager ViewportActionManager = new();
    public ActionManager ActionManager = new();

    public int ViewIndex;

    public ModelSelection Selection = new();
    public ViewportSelection ViewportSelection = new();
    public ModelPropertyCache ModelPropertyCache = new();
    public ModelEntityTypeCache EntityTypeCache = new();

    public ModelViewportFilters ViewportFilters;
    public ModelUniverse Universe;

    public ModelViewportWindow ViewportWindow;
    public ModelSourceList SourceList;
    public ModelSelectionList SelectionList;
    public ModelContents Contents;
    public ModelProperties Properties;

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


    public ModelEditorView(ModelEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        Window = Smithbox.Instance._context.Window;
        Device = Smithbox.Instance._context.Device;
        RenderScene = new();

        ViewIndex = imguiId;

        Universe = new ModelUniverse(this, project);

        ViewportWindow = new(this, project);

        ViewportFilters = new(this, project);

        SourceList = new(this, project);
        SelectionList = new(this, project);
        Contents = new(this, project);
        Properties = new(this, project);

        // Tools
        ModelGridTool = new ModelGridConfiguration(this, Project);
        ModelInsightMenu = new ModelInsightView(this, Project);
        ModelInstanceFinder = new ModelInstanceFinder(this, Project);
        ModelMaskToggler = new ModelMaskToggler(this, Project);

        // Actions
        CreateAction = new CreateAction(this, Project);
        DuplicateAction = new DuplicateAction(this, Project);
        DeleteAction = new DeleteAction(this, Project);
        FrameAction = new FrameAction(this, Project);
        GotoAction = new GotoAction(this, Project);
        PullToCameraAction = new PullToCameraAction(this, Project);
        ReorderAction = new ReorderAction(this, Project);

        ModelInsightHelper = new ModelInsightHelper(this, Project);

        ActionManager.AddEventHandler(Contents);
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        if (!CFG.Current.Interface_ModelEditor_ScreenshotMode)
        {
            // FLVER
            if (ImGui.Begin($@"FLVER##ModelFlverWindow{ViewIndex}", UIHelper.GetMainWindowFlags()))
            {
                float width = ImGui.GetContentRegionAvail().X;
                float height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.ModelEditor_FileList);
                    Editor.ViewHandler.ActiveView = this;
                }

                SourceList.Display(width, height * 0.2f);
                SelectionList.Display(width, height * 0.1f);
                Contents.Display(width, height * 0.7f);
            }

            ImGui.End();
        }

        // Viewport
        ViewportWindow.Display();

        if (!CFG.Current.Interface_ModelEditor_ScreenshotMode)
        {
            // Properties
            if (ImGui.Begin($@"Properties##ModelPropertiesWindow{ViewIndex}", UIHelper.GetMainWindowFlags()))
            {
                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.ModelEditor_FileList);
                    Editor.ViewHandler.ActiveView = this;
                }

                Properties.Display();
            }

            ImGui.End();
        }

        
        ViewportSelection.ClearGotoTarget();
    }
}
