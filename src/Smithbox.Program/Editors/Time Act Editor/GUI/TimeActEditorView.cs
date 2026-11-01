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

    public int ViewIndex;

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
    public ResourceListTool ResourceListTool;
    public ModelCharacterPreview CharacterPreview;

    public TimeActEditorView(TimeActEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        Window = Smithbox.Instance._context.Window;
        Device = Smithbox.Instance._context.Device;
        RenderScene = new();

        ViewIndex = imguiId;

        // The action history is cleared whenever the loaded model changes, so every action applies to the loaded model
        ViewportActionManager.ActionPerformed += MarkLoadedModelDirty;
        ActionManager.ActionPerformed += MarkLoadedModelDirty;

        Universe = new TimeActUniverse(this, project);
        Playback = new TimeActPlayback(this, project);

        ViewportWindow = new(this, project);
        ViewportFilters = new(this, project);

        SourceWindow = new(this, project);
        SelectionWindow = new(this, project);
        AnimationSelectionWindow = new(this, project);
        GraphWindow = new(this, project);
        PropertiesWindow = new(this, project);
        ToolWindow = new(this, project);

        // Tools
        GridTool = new ViewportGridTool(ViewportGridEditor.TimeAct);
        ResourceListTool = new ResourceListTool();
        CharacterPreview = new ModelCharacterPreview(project, GetCharacterPreviewTarget);
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

    private void MarkLoadedModelDirty()
    {
        if (Selection.SelectedTimeActModelWrapper != null)
        {
            Selection.SelectedTimeActModelWrapper.IsDirty = true;
        }
    }
    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        Playback.Update();

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
            if (ImGui.Begin($@"{LOC.Get("TAE_Editor_Tool_Window")}##timeActEditor_ToolWindow_{viewIndex}", GUI.GetInnerWindowFlags()))
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
