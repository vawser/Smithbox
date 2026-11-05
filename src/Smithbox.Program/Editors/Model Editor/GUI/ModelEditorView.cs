using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.ModelEditor;

public class ModelEditorView : IEditorView
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
    public ModelEntityTypeCache EntityTypeCache;

    public ModelViewportFilters ViewportFilters;
    public ModelUniverse Universe;

    public ModelViewportWindow ViewportWindow;
    public ModelContainerList SourceList;
    public ModelFileList FileList;
    public ModelContentsView Contents;
    public ModelPropertyView Properties;
    public ModelToolWindow ToolView;

    // Tools
    public ViewportGridTool GridTool;
    public ViewportRenderingTool RenderingTool;
    //public ModelInsightView ModelInsightMenu;
    public ModelInstanceFinder ModelInstanceFinder;
    public ModelMaskToggler ModelMaskToggler;
    public ModelAnimationTool ModelAnimationTool;
    public ModelCharacterPreview ModelCharacterPreview;
    public ResourceListTool ResourceListTool;
    public ModelExportTool ModelExportTool;
    public AssetBrowserTool AssetBrowserTool;

    // Actions
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

        EntityTypeCache = new(this, project);

        // The action history is cleared whenever the loaded model changes, so every action applies to the loaded model
        ViewportActionManager.ActionPerformed += MarkLoadedModelDirty;
        ActionManager.ActionPerformed += MarkLoadedModelDirty;

        Universe = new ModelUniverse(this, project);

        ViewportWindow = new(this, project);

        ViewportFilters = new(this, project);

        SourceList = new(this, project);
        FileList = new(this, project);
        Contents = new(this, project);
        Properties = new(this, project);
        ToolView = new(this, project);

        // Tools
        GridTool = new ViewportGridTool(ViewportGridEditor.Model, () => ViewportWindow?.Viewport as VulkanViewport);
        RenderingTool = new ViewportRenderingTool(() => ViewportWindow?.Viewport as VulkanViewport);
        //ModelInsightMenu = new ModelInsightView(this, Project);
        ModelInstanceFinder = new ModelInstanceFinder(this, Project);
        ModelMaskToggler = new ModelMaskToggler(Project, GetModelMaskTarget);
        ModelAnimationTool = new ModelAnimationTool(this, Project);
        ModelCharacterPreview = new ModelCharacterPreview(Project, GetCharacterPreviewTarget);
        ResourceListTool = new ResourceListTool();
        ModelExportTool = new ModelExportTool(this, Project);
        AssetBrowserTool = new AssetBrowserTool(Project, LOC.Get("MODEL_AssetBrowser_Hint"),
            e => SourceList.SelectAndLoadModel(e.ID));

        // Actions
        DuplicateAction = new DuplicateAction(this, Project);
        DeleteAction = new DeleteAction(this, Project);
        FrameAction = new FrameAction(this, Project);
        GotoAction = new GotoAction(this, Project);
        PullToCameraAction = new PullToCameraAction(this, Project);
        ReorderAction = new ReorderAction(this, Project);
    }

    private ModelMaskTarget GetModelMaskTarget()
    {
        ModelWrapper wrapper = Selection.SelectedModelWrapper;

        return wrapper?.Container != null
            ? new ModelMaskTarget(wrapper.Name, wrapper.FLVER, wrapper.Container.Meshes)
            : null;
    }

    private CharacterPreviewTarget GetCharacterPreviewTarget()
    {
        ModelWrapper wrapper = Selection.SelectedModelWrapper;
        ModelContainer container = wrapper?.Container;

        if (container == null)
            return null;

        return new CharacterPreviewTarget(wrapper.Name, container.CharacterAppearance, appearance =>
        {
            container.AssignCharacterParts(appearance);

            Universe.ScheduleTextureRefresh();
        });
    }

    private void MarkLoadedModelDirty()
    {
        if (Selection.SelectedModelWrapper != null)
        {
            Selection.SelectedModelWrapper.IsDirty = true;
        }
    }

    /// <summary>
    /// Loads a model into this view in place of the one it has loaded. A model can only be loaded in one view at a
    /// time, as its objects wrap the FLVER held by the shared model bank.
    /// Returns false if the model is loaded in another view.
    /// </summary>
    public bool LoadModel(ModelWrapper wrapper)
    {
        if (wrapper == null)
            return false;

        var ownerView = wrapper.Container?.View;

        if (ownerView != null && ownerView != this)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_View_Model_Open_In_Other_View", wrapper.Name, ownerView.ViewIndex));
            return false;
        }

        // Also unloads the model itself when it is being reloaded
        Selection.SelectedModelWrapper?.Unload();

        Selection.SelectedModelWrapper = wrapper;

        ViewportActionManager.Clear();
        ActionManager.Clear();

        wrapper.Load(this);

        return true;
    }

    /// <summary>
    /// How many selected objects the editing actions can act on: the model root is not one of them
    /// </summary>
    public int GetEditableSelectionCount()
    {
        var container = Selection.SelectedModelWrapper?.Container;

        return container != null ? ModelIndexRemapper.GetEditableSelection(this, container).Count : 0;
    }

    /// <summary>
    /// Unloads a model loaded in this view. Models loaded in other views are left alone.
    /// </summary>
    public void UnloadModel(ModelWrapper wrapper)
    {
        if (wrapper == null)
            return;

        var ownerView = wrapper.Container?.View;

        if (ownerView != null && ownerView != this)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_View_Model_Open_In_Other_View", wrapper.Name, ownerView.ViewIndex));
            return;
        }

        if (Selection.SelectedModelWrapper == wrapper)
        {
            Selection.SelectedModelWrapper = null;
        }

        wrapper.Unload();
    }

    /// <summary>
    /// Unloads every model loaded in this view, for when the view is closed. Unsaved changes are kept in the bank.
    /// </summary>
    public void UnloadAllModels()
    {
        ModelAnimationTool.StopAll();

        var bank = Project.Handler.ModelData?.PrimaryBank;

        if (bank != null)
        {
            var ownedModels = bank.Models.Values
                .SelectMany(e => e.Models)
                .Where(e => e.Container?.View == this)
                .ToList();

            foreach (var model in ownedModels)
            {
                UnloadModel(model);
            }
        }

        Selection.SelectedModelWrapper = null;
        Selection.SelectedModelContainerWrapper = null;
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        ViewportFilters.Update();

        // Done before the panels are drawn, as the File List that requests it may not be visible
        FileList.ProcessPendingLoad();

        if (CFG.Current.Interface_ModelEditor_EditorDisplayType is EditorDisplayType.Windows)
        {
            DisplayWindows(dockspaceId, viewIndex, doFocus, isActiveView);
        }

        if (CFG.Current.Interface_ModelEditor_EditorDisplayType is EditorDisplayType.TabbedPanels)
        {
            DisplayTabbedPanels(dockspaceId, viewIndex, doFocus, isActiveView);
        }

        ViewportSelection.ClearGotoTarget();

        ModelAnimationTool.Update();

        ToolView.View.ModelExportTool.DetectFlverSelectionMenu();
        ToolView.View.ModelExportTool.DisplayFlverSelectionMenu();
    }


    private void DisplayWindows(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ModelEditorView);
        if (ImGui.Begin($@"{LOC.Get("MODEL_Window_Source_List")}##modelEditor_SourceList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.ModelEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            SourceList.Display(width, height);
        }

        ImGui.End();

        // File List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ModelEditorView);
        if (ImGui.Begin($@"{LOC.Get("MODEL_Window_File_List")}##modelEditor_FileList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.ModelEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            FileList.Display(width, height);
        }

        ImGui.End();

        // Contents
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ModelEditorView);
        if (ImGui.Begin($@"{LOC.Get("MODEL_Window_Model_Contents")}##modelEditor_Contents_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.ModelEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            Contents.Display(width, height);
        }

        ImGui.End();

        if (CFG.Current.Interface_ModelEditor_ToolWindow)
        {
            // Tools
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ModelEditorView);
            if (ImGui.Begin($@"{LOC.Get("MODEL_Window_Tools")}##modelEditor_ToolWindow_{viewIndex}", GUI.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.ModelEditor_Tools);
                    Editor.ViewHandler.ActiveView = this;
                }

                ToolView.Display();
            }

            ImGui.End();
        }

        // Viewport
        ViewportWindow.Display(dockspaceId);

        // Properties
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ModelEditorView);
        if (ImGui.Begin($@"{LOC.Get("MODEL_Window_Properties")}##modelEditor_Properties_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.ModelEditor_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            Properties.Display();
        }

        ImGui.End();
    }


    private void DisplayTabbedPanels(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Left Panel
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ModelEditorView);
        if (ImGui.Begin($@"{LOC.Get("EDITOR_Window_LeftPanel")}##modelEditor_LeftPanel_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            ImGui.BeginTabBar("leftPanelTabs");

            if (ImGui.BeginTabItem(LOC.Get("MODEL_Window_Source_List")))
            {
                FocusManager.SetFocus(EditorFocusContext.ModelEditor_FileList);
                Editor.ViewHandler.ActiveView = this;

                SourceList.Display(width, height);

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(LOC.Get("MODEL_Window_File_List")))
            {
                FocusManager.SetFocus(EditorFocusContext.ModelEditor_FileList);
                Editor.ViewHandler.ActiveView = this;

                FileList.Display(width, height);

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem(LOC.Get("MODEL_Window_Model_Contents")))
            {
                FocusManager.SetFocus(EditorFocusContext.ModelEditor_FileList);
                Editor.ViewHandler.ActiveView = this;

                Contents.Display(width, height);

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();

        // Viewport
        ViewportWindow.Display(dockspaceId);

        // Right Panel
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ModelEditorView);
        if (ImGui.Begin($@"{LOC.Get("EDITOR_Window_RightPanel")}##modelEditor_RightPanel_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            ImGui.BeginTabBar("rightPanelTabs");

            if (ImGui.BeginTabItem(LOC.Get("MODEL_Window_Properties")))
            {
                FocusManager.SetFocus(EditorFocusContext.ModelEditor_Properties);
                Editor.ViewHandler.ActiveView = this;

                Properties.Display();

                ImGui.EndTabItem();
            }

            if (CFG.Current.Interface_ModelEditor_ToolWindow)
            {
                if (ImGui.BeginTabItem(LOC.Get("MODEL_Window_Tools")))
                {
                    FocusManager.SetFocus(EditorFocusContext.ModelEditor_Tools);
                    Editor.ViewHandler.ActiveView = this;

                    ToolView.Display();

                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
    }
}
