using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActEditorView : IEditorView
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public Sdl2Window Window;
    public GraphicsDevice Device;
    public RenderScene RenderScene;

    public ViewportActionManager ViewportActionManager = new();
    public ActionManager ActionManager = new();

    /// <summary>
    /// Undo and redo go through this, so they first go to what they change
    /// </summary>
    public TimeActHistory History;

    public int ViewIndex;

    /// <summary>
    /// Set to bring the view's window to the front the next time it is drawn
    /// </summary>
    public bool RequestFocus;

    public TimeActSelection Selection = new();
    public ViewportSelection ViewportSelection = new();

    public TimeActViewportFilters ViewportFilters;
    public TimeActUniverse Universe;
    public TimeActPlayback Playback;

    public TimeActSourceWindow SourceWindow;
    public TimeActSelectionWindow SelectionWindow;
    public TimeActAnimationSelectionWindow AnimationSelectionWindow;
    public TimeActGraphWindow GraphWindow;
    public TimeActViewportWindow ViewportWindow;
    public TimeActPropertiesWindow PropertiesWindow;
    public TimeActToolWindow ToolWindow;

    // Tools
    public ViewportGridTool GridTool;
    public ViewportRenderingTool RenderingTool;
    public ResourceListTool ResourceListTool;
    public ModelCharacterPreview CharacterPreview;
    public ModelMaskToggler MaskToggler;
    public TimeActSearchTool LocalSearchTool;
    public TimeActSearchTool GlobalSearchTool;
    public TimeActMassEditTool MassEditTool;
    public TimeActComboBuilderTool ComboBuilderTool;
    public TimeActHistoryTool HistoryTool;

    public TimeActEditorView(TimeActEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        Window = Smithbox.Instance._context.Window;
        Device = Smithbox.Instance._context.Device;
        RenderScene = new();

        ViewIndex = imguiId;

        // The actions edit the time acts, not the model, so the model in the viewport is never marked as modified:
        // that would keep its FLVER in memory and stop it being read again (and so show Model Editor saves)
        History = new TimeActHistory(this, project);

        Universe = new TimeActUniverse(this, project);
        Playback = new TimeActPlayback(this, project);

        // Editing the parameters of an event can change what it displays
        ViewportActionManager.ActionPerformed += Playback.EventDisplay.Invalidate;

        ViewportWindow = new(this, project);
        ViewportFilters = new(this, project);

        SourceWindow = new(this, project);
        SelectionWindow = new(this, project);
        AnimationSelectionWindow = new(this, project);
        GraphWindow = new(this, project);
        PropertiesWindow = new(this, project);
        ToolWindow = new(this, project);

        // Tools
        GridTool = new ViewportGridTool(ViewportGridEditor.TimeAct, () => ViewportWindow?.Viewport as VulkanViewport);
        RenderingTool = new ViewportRenderingTool(() => ViewportWindow?.Viewport as VulkanViewport);
        ResourceListTool = new ResourceListTool();
        CharacterPreview = new ModelCharacterPreview(project, GetCharacterPreviewTarget);
        MaskToggler = new ModelMaskToggler(project, GetModelMaskTarget);
        LocalSearchTool = new TimeActSearchTool(this, project, TimeActSearchMode.Local);
        GlobalSearchTool = new TimeActSearchTool(this, project, TimeActSearchMode.Global);
        MassEditTool = new TimeActMassEditTool(this, project);
        ComboBuilderTool = new TimeActComboBuilderTool(this, project);
        HistoryTool = new TimeActHistoryTool(this, project);
    }

    private ModelMaskTarget GetModelMaskTarget()
    {
        TimeActModelWrapper wrapper = Selection.SelectedTimeActModelWrapper;

        return wrapper?.Container != null
            ? new ModelMaskTarget(wrapper.Name, wrapper.FLVER, wrapper.Container.Meshes)
            : null;
    }

    private CharacterPreviewTarget GetCharacterPreviewTarget()
    {
        TimeActModelWrapper wrapper = Selection.SelectedTimeActModelWrapper;
        TimeActContainer container = wrapper?.Container;

        if (container == null)
            return null;

        return new CharacterPreviewTarget(wrapper.Name, container.CharacterAppearance, appearance =>
        {
            container.AssignCharacterParts(appearance);

            Universe.ScheduleTextureRefresh();

            Playback.OnPartsChanged();
        });
    }

    /// <summary>
    /// Releases the view's viewport (its render pipeline's GPU targets, passes and queues, gizmos and grids), with its
    /// selection and history, once the view has been closed and its source released. Also called when the project
    /// closes. The view must not be displayed or updated afterwards.
    /// </summary>
    public void Dispose()
    {
        ViewportSelection.ClearSelection();
        ViewportActionManager.Clear();

        (ViewportWindow?.Viewport as VulkanViewport)?.Dispose();
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        ViewportFilters.Update();

        // Before anything reads the selection, so every window only sees events of the animation on show
        Selection.PruneEvents();

        Playback.Update();

        // After the playback, so it sees where the clock got to this frame
        ComboBuilderTool.Update();

        DisplayWindows(dockspaceId, viewIndex, doFocus, isActiveView);

        ViewportSelection.ClearGotoTarget();
    }

    private void DisplayWindows(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Source Window
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TimeActEditorView);
        if (ImGui.Begin($@"{LOC.Get("TAE_Editor_Source_Window")}##timeActEditor_SourceWindow_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TimeActEditor_SourceWindow);
                Editor.ViewHandler.ActiveView = this;
            }

            SourceWindow.Display(width, height);
        }

        ImGui.End();

        // Selection Window
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TimeActEditorView);
        if (ImGui.Begin($@"{LOC.Get("TAE_Editor_Selection_Window")}##timeActEditor_SelectionWindow_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TimeActEditor_SelectionWindow);
                Editor.ViewHandler.ActiveView = this;
            }

            SelectionWindow.Display(width, height);
        }

        ImGui.End();

        // Animation Selection Window
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TimeActEditorView);
        if (ImGui.Begin($@"{LOC.Get("TAE_Editor_Animation_Selection_Window")}##timeActEditor_AnimationSelectionWindow_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TimeActEditor_AnimationSelectionWindow);
                Editor.ViewHandler.ActiveView = this;
            }

            AnimationSelectionWindow.Display(width, height);
        }

        ImGui.End();

        // Graph Window
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TimeActEditorView);
        if (ImGui.Begin($@"{LOC.Get("TAE_Editor_Graph_Window")}##timeActEditor_GraphWindow_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TimeActEditor_GraphWindow);
                Editor.ViewHandler.ActiveView = this;
            }

            GraphWindow.Display(width, height);
        }

        ImGui.End();

        // Viewport
        ViewportWindow.Display(dockspaceId);

        // Properties Window
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TimeActEditorView);
        if (ImGui.Begin($@"{LOC.Get("TAE_Editor_Properties_Window")}##timeActEditor_PropertiesWindow_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TimeActEditor_PropertiesWindow);
                Editor.ViewHandler.ActiveView = this;
            }

            PropertiesWindow.Display(width, height);
        }

        ImGui.End();

        if (CFG.Current.Interface_TimeActEditor_ToolWindow)
        {
            // Tool Window
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_TimeActEditorView);
            if (ImGui.Begin($@"{LOC.Get("TAE_Editor_Tool_Window")}##timeActEditor_ToolWindow_{viewIndex}", GUI.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.TimeActEditor_ToolWindow);
                    Editor.ViewHandler.ActiveView = this;
                }

                ToolWindow.Display(width, height);
            }

            ImGui.End();
        }
    }
}
