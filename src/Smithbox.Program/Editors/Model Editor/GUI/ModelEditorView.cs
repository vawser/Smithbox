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

public class ModelEditorView : IEditorView, IActionEventHandler
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
    public ModelDisplayRotationTool ModelDisplayRotationTool;
    public ResourceListTool ResourceListTool;
    public ModelExportTool ModelExportTool;
    public AssetBrowserTool AssetBrowserTool;
    public ModelHistoryTool HistoryTool;

    // Actions
    public DuplicateAction DuplicateAction;
    public DeleteAction DeleteAction;
    public FrameAction FrameAction;
    public GotoAction GotoAction;
    public PullToCameraAction PullToCameraAction;
    public ReorderAction ReorderAction;
    public AddAction AddAction;

    public ModelEditorView(ModelEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        Window = Smithbox.Instance._context.Window;
        Device = Smithbox.Instance._context.Device;
        RenderScene = new();

        ViewIndex = imguiId;

        EntityTypeCache = new(this, project);

        // The history always belongs to the loaded model: switching models parks it with the model it belongs to
        // (or clears it), so every action applies to the loaded model
        ViewportActionManager.ActionPerformed += MarkLoadedModelDirty;
        ActionManager.ActionPerformed += MarkLoadedModelDirty;

        ViewportActionManager.AddEventHandler(this);
        ActionManager.AddEventHandler(this);

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
        ModelDisplayRotationTool = new ModelDisplayRotationTool(this, Project);
        ResourceListTool = new ResourceListTool();
        ModelExportTool = new ModelExportTool(this, Project);
        AssetBrowserTool = new AssetBrowserTool(Project, LOC.Get("MODEL_AssetBrowser_Hint"),
            e => SourceList.SelectAndLoadModel(e.ID));
        HistoryTool = new ModelHistoryTool(this, Project);

        // Actions
        DuplicateAction = new DuplicateAction(this, Project);
        DeleteAction = new DeleteAction(this, Project);
        FrameAction = new FrameAction(this, Project);
        GotoAction = new GotoAction(this, Project);
        PullToCameraAction = new PullToCameraAction(this, Project);
        ReorderAction = new ReorderAction(this, Project);
        AddAction = new AddAction(this, Project);
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
        var wrapper = Selection.SelectedModelWrapper;

        if (wrapper != null)
        {
            wrapper.IsDirty = true;

            // Any edit may have changed vertices, so the meshes work out their pivots again
            if (wrapper.Container?.View == this)
            {
                foreach (var mesh in wrapper.Container.Meshes.OfType<ModelEntity>())
                {
                    mesh.InvalidateMeshPivot();
                }
            }

            // Redraw the model from its edits once they pause
            if (CFG.Current.ModelEditor_Viewport_Live_Preview && wrapper.Container?.View == this)
            {
                wrapper.Container.MarkPreviewStale();
            }
        }
    }

    /// <summary>
    /// Redraws the loaded model from its edited data now, whether or not the live preview is on
    /// </summary>
    public void RefreshViewportFromEdits()
    {
        var wrapper = Selection.SelectedModelWrapper;

        if (wrapper?.Container?.View == this)
        {
            wrapper.Container.MarkPreviewStale(immediate: true);
        }
    }

    /// <summary>
    /// Keeps the FLVER's material, node, dummy and mesh lists in step with the objects after they are added, removed
    /// or moved (and when that is undone or redone). The tools that read the FLVER (the Mask Toggler, Export) would
    /// otherwise see the lists as they were when the model was last saved.
    /// </summary>
    public void OnActionEvent(ActionEvent evt)
    {
        if (!evt.HasFlag(ActionEvent.ObjectAddedRemoved))
            return;

        var wrapper = Selection.SelectedModelWrapper;

        if (wrapper?.Container != null && wrapper.Container.View == this && wrapper.FLVER != null)
        {
            wrapper.UpdateFLVER();
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

        // A model parked in another view gives up its history there, as its objects belong to that view
        if (wrapper.Parked != null && wrapper.Parked.View != this)
        {
            Smithbox.Log(this, LOC.Get("MODEL_History_Dropped_Other_View", wrapper.Name, wrapper.Parked.History.Count,
                wrapper.Parked.View.ViewIndex));

            wrapper.Parked.View.DiscardParked(wrapper);
        }

        var current = Selection.SelectedModelWrapper;

        // Switching to another model keeps this one's history when it has any; loading the same model again is a
        // reload, which drops it
        if (current != null && current != wrapper && CanPark(current))
        {
            ParkModel(current);
        }
        else
        {
            current?.Unload();
        }

        Selection.SelectedModelWrapper = wrapper;

        ActionManager.Clear();

        if (wrapper.Parked != null)
        {
            UnparkModel(wrapper);
        }
        else
        {
            ViewportActionManager.Clear();

            wrapper.Load(this);
        }

        EnforceParkLimit();

        return true;
    }

    #region Parking
    /// <summary>
    /// Whether switching away from the model keeps it, with its history: it is loaded here, has undo or redo steps,
    /// and models are kept at all
    /// </summary>
    private bool CanPark(ModelWrapper wrapper)
    {
        return CFG.Current.ModelEditor_History_Parked_Limit > 0 &&
               wrapper.Container?.View == this &&
               (ViewportActionManager.CanUndo() || ViewportActionManager.CanRedo());
    }

    /// <summary>
    /// Takes the loaded model out of the view, keeping its objects, history and selection for when it is loaded again
    /// </summary>
    private void ParkModel(ModelWrapper wrapper)
    {
        var container = wrapper.Container;

        ModelAnimationTool.StopAll();

        // The FLVER is what a save or the save check reads while the objects are parked
        if (wrapper.FLVER != null)
        {
            wrapper.UpdateFLVER();
        }

        var selection = ViewportSelection.GetSelection().OfType<Entity>().ToList();

        wrapper.Parked = new ParkedModel
        {
            View = this,
            Container = container,
            History = ViewportActionManager.TakeHistory(),
            Selection = selection,
            ParkedAt = System.Diagnostics.Stopwatch.GetTimestamp()
        };

        Universe.ParkModel(container);

        wrapper.Container = null;

        if (Selection.SelectedModelWrapper == wrapper)
        {
            Selection.SelectedModelWrapper = null;
        }
    }

    /// <summary>
    /// Puts a model parked in this view back, with its history and selection
    /// </summary>
    private void UnparkModel(ModelWrapper wrapper)
    {
        var parked = wrapper.Parked;
        wrapper.Parked = null;

        wrapper.Container = parked.Container;

        Universe.RestoreParkedModel(wrapper, parked.Container);

        ViewportActionManager.RestoreHistory(parked.History);

        ViewportSelection.ClearSelection();

        foreach (var entity in parked.Selection.Where(e => parked.Container.Objects.Contains(e) ||
                                                           e == parked.Container.RootObject))
        {
            ViewportSelection.AddSelection(entity);
        }

        Smithbox.Log(this, LOC.Get("MODEL_History_Restored", wrapper.Name, parked.History.Count));
    }

    /// <summary>
    /// Drops a model parked in this view: its objects are released and its history is lost. Its edits stay in the
    /// FLVER, so nothing unsaved is lost.
    /// </summary>
    public void DiscardParked(ModelWrapper wrapper)
    {
        var parked = wrapper.Parked;

        if (parked == null || parked.View != this)
            return;

        wrapper.Parked = null;

        Universe.DiscardParkedModel(parked.Container);
    }

    /// <summary>
    /// The models parked in this view, oldest first
    /// </summary>
    public List<ModelWrapper> GetParkedModels()
    {
        return Project.Handler.ModelData?.PrimaryBank?.Models.Values
            .SelectMany(e => e.Models)
            .Where(m => m.Parked?.View == this)
            .OrderBy(m => m.Parked.ParkedAt)
            .ToList() ?? new();
    }

    /// <summary>
    /// Keeps the number of parked models within the limit, dropping saved models before unsaved ones, oldest first.
    /// Dropping an unsaved one only loses its history: its edits are in the FLVER.
    /// </summary>
    private void EnforceParkLimit()
    {
        var parked = GetParkedModels();
        var excess = parked.Count - Math.Max(0, CFG.Current.ModelEditor_History_Parked_Limit);

        if (excess <= 0)
            return;

        foreach (var wrapper in parked.OrderBy(m => m.IsDirty).ThenBy(m => m.Parked.ParkedAt).Take(excess))
        {
            Smithbox.Log(this, LOC.Get("MODEL_History_Dropped_Limit", wrapper.Name, wrapper.Parked.History.Count));

            DiscardParked(wrapper);
        }
    }
    #endregion

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
    /// <summary>
    /// Releases the view's viewport (its render pipeline's GPU targets, passes and queues, gizmos and grids), with its
    /// selection and history, once the view has been closed and its models unloaded. Also called when the project
    /// closes. The view must not be displayed or updated afterwards.
    /// </summary>
    public void Dispose()
    {
        ViewportSelection.ClearSelection();
        ViewportActionManager.Clear();

        (ViewportWindow?.Viewport as VulkanViewport)?.Dispose();
    }

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

            foreach (var model in GetParkedModels())
            {
                DiscardParked(model);
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

        // Makes and swaps in the preview of the edited model
        var loadedWrapper = Selection.SelectedModelWrapper;

        if (loadedWrapper?.Container?.View == this)
        {
            loadedWrapper.Container.UpdatePreview(loadedWrapper);
        }

        if (CFG.Current.Interface_ModelEditor_EditorDisplayType is EditorDisplayType.Windows)
        {
            DisplayWindows(dockspaceId, viewIndex, doFocus, isActiveView);
        }

        if (CFG.Current.Interface_ModelEditor_EditorDisplayType is EditorDisplayType.TabbedPanels)
        {
            DisplayTabbedPanels(dockspaceId, viewIndex, doFocus, isActiveView);
        }

        ViewportSelection.ClearGotoTarget();

        // Before the animation, which places the bone and dummy markers on top of the rotation
        ModelDisplayRotationTool.Update();

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
