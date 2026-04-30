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
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        DisplayMenubar();

        if (!CFG.Current.Interface_ModelEditor_ScreenshotMode)
        {
            // FLVER
            if (ImGui.Begin($@"FLVER##ModelFlverWindow{ViewIndex}", UIHelper.GetInnerWindowFlags()))
            {
                float width = ImGui.GetContentRegionAvail().X;
                float height = ImGui.GetContentRegionAvail().Y * CFG.Current.Interace_Editor_Display_Inner_Height_Percent;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.ModelEditor_FileList);
                    Editor.ViewHandler.ActiveView = this;
                }

                SourceList.Display(width, height * CFG.Current.ModelEditor_Display_SourceList_Percentage);
                SelectionList.Display(width, height * CFG.Current.ModelEditor_Display_SelectionList_Percentage);
                Contents.Display(width, height * CFG.Current.ModelEditor_Display_Contents_Percentage);
            }

            ImGui.End();
        }

        // Viewport
        ViewportWindow.Display();

        if (!CFG.Current.Interface_ModelEditor_ScreenshotMode)
        {
            // Properties
            if (ImGui.Begin($@"Properties##ModelPropertiesWindow{ViewIndex}", UIHelper.GetInnerWindowFlags()))
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
    public void DisplayMenubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Options"))
            {
                if (ImGui.BeginMenu("Containers"))
                {
                    if (ImGui.MenuItem("Include Alias in Search"))
                    {
                        CFG.Current.ModelEditor_Containers_IncludeAliasInSearch = !CFG.Current.ModelEditor_Containers_IncludeAliasInSearch;
                    }
                    UIHelper.Tooltip($"If enabled, when filtering the source list, alias will be included. Can be slower than normal.");
                    UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_Containers_IncludeAliasInSearch);

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Files"))
                {
                    if (ImGui.MenuItem("Auto-Select First Entries"))
                    {
                        CFG.Current.ModelEditor_Files_AutoLoadFirstEntry = !CFG.Current.ModelEditor_Files_AutoLoadFirstEntry;
                    }
                    UIHelper.Tooltip($"If enabled, the first entry in the list will be loaded automatically.");
                    UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_Files_AutoLoadFirstEntry);

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Contents"))
                {
                    if (ImGui.MenuItem("Display Node Name in Mesh Entry"))
                    {
                        CFG.Current.ModelEditor_Contents_NodeNameInMeshEntry = !CFG.Current.ModelEditor_Contents_NodeNameInMeshEntry;
                    }
                    UIHelper.Tooltip($"If enabled, the linked node name is displayed in the mesh entry name.");
                    UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_Contents_NodeNameInMeshEntry);

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Display"))
                {
                    ImGui.SliderFloat("Containers##containersDisplayPercentage", ref CFG.Current.ModelEditor_Display_SourceList_Percentage, 0.01f, 0.99f);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        var remainder = 1f - CFG.Current.ModelEditor_Display_SourceList_Percentage;

                        StudioMath.Redistribute(
                            ref CFG.Current.ModelEditor_Display_SelectionList_Percentage,
                            ref CFG.Current.ModelEditor_Display_Contents_Percentage,
                            remainder);
                    }
                    UIHelper.Tooltip("The percentage of the window the Containers section occupies.");

                    ImGui.SliderFloat("Files##filesDisplayPercentage", ref CFG.Current.ModelEditor_Display_SelectionList_Percentage, 0.01f, 0.99f);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        var remainder = 1f - CFG.Current.ModelEditor_Display_SelectionList_Percentage;

                        StudioMath.Redistribute(
                            ref CFG.Current.ModelEditor_Display_SourceList_Percentage,
                            ref CFG.Current.ModelEditor_Display_Contents_Percentage,
                            remainder);
                    }
                    UIHelper.Tooltip("The percentage of the window the Files section occupies.");

                    ImGui.SliderFloat("Contents##contentsDisplayPercentage", ref CFG.Current.ModelEditor_Display_Contents_Percentage, 0.01f, 0.99f);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        var remainder = 1f - CFG.Current.ModelEditor_Display_Contents_Percentage;

                        StudioMath.Redistribute(
                            ref CFG.Current.ModelEditor_Display_SourceList_Percentage,
                            ref CFG.Current.ModelEditor_Display_SelectionList_Percentage,
                            remainder);
                    }
                    UIHelper.Tooltip("The percentage of the window the Contents section occupies.");

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }
}
