using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.GparamEditor;

public class GparamEditorView : IEditorView
{
    public GparamEditorScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public Sdl2Window Window;
    public GraphicsDevice Device;
    public RenderScene RenderScene;

    public ViewportActionManager ViewportActionManager = new();
    public ViewportSelection ViewportSelection = new();

    public GparamUniverse Universe;
    public GparamViewportWindow ViewportWindow;

    // Viewport tools
    public GparamViewportPreviewTool ViewportPreviewTool;
    public ViewportRenderingTool RenderingTool;
    public ViewportGridTool GridTool;

    public int ViewIndex;

    public GparamSelection Selection;

    public GparamPropertyEditor PropertyEditor;

    public GparamQuickEdit QuickEditHandler;

    public GparamFileList FileListView;
    public GparamGroupList GroupListView;
    public GparamFieldList FieldListView;
    public GparamValueList FieldValueListView;
    public GparamToolView ToolView;

    public GparamEditorView(GparamEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;

        Window = Smithbox.Instance._context.Window;
        Device = Smithbox.Instance._context.Device;
        RenderScene = new();

        Selection = new GparamSelection(this, Project);

        PropertyEditor = new GparamPropertyEditor(this, Project);
        QuickEditHandler = new GparamQuickEdit(this, Project);

        FileListView = new GparamFileList(this, Project);
        GroupListView = new GparamGroupList(this, Project);
        FieldListView = new GparamFieldList(this, Project);
        FieldValueListView = new GparamValueList(this, Project);
        Universe = new GparamUniverse(this, Project);
        ViewportWindow = new GparamViewportWindow(this, Project);

        // An edit, or its undo, changes the lighting the viewport shows
        ActionManager.ActionPerformed += ViewportWindow.MarkLightingDirty;

        ViewportPreviewTool = new GparamViewportPreviewTool(this, Project);
        RenderingTool = new ViewportRenderingTool(() => ViewportWindow?.Viewport as VulkanViewport);
        GridTool = new ViewportGridTool(ViewportGridEditor.Gparam, () => ViewportWindow?.Viewport as VulkanViewport);

        ToolView = new GparamToolView(this, Project);
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Files
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_GparamEditorView);
        if (ImGui.Begin($@"{LOC.Get("GPARAM_Editor_File_List")}###gparamEditor_FileList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.GparamEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            FileListView.Display();
        }

        ImGui.End();

        // Groups
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_GparamEditorView);
        if (ImGui.Begin($@"{LOC.Get("GPARAM_Editor_Group_List")}###gparamEditor_GroupList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.GparamEditor_GroupList);
                Editor.ViewHandler.ActiveView = this;
            }

            GroupListView.Display();
        }

        ImGui.End();

        // Fields
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_GparamEditorView);
        if (ImGui.Begin($@"{LOC.Get("GPARAM_Editor_Field_List")}###gparamEditor_FieldList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.GparamEditor_FieldList);
                Editor.ViewHandler.ActiveView = this;
            }

            FieldListView.Display();
        }

        ImGui.End();

        // Field Values
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_GparamEditorView);
        if (ImGui.Begin($@"{LOC.Get("GPARAM_Editor_Value_List")}###gparamEditor_FieldValueList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.GparamEditor_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            FieldValueListView.Display();
        }

        ImGui.End();

        // Viewport
        ViewportWindow.Display(dockspaceId);

        if (CFG.Current.Interface_GparamEditor_ToolWindow)
        {
            // Tools
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_GparamEditorView);
            if (ImGui.Begin($@"{LOC.Get("GPARAM_Editor_Tools")}##gparamEditor_ToolWindow_{viewIndex}", GUI.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.GparamEditor_Tools);
                    Editor.ViewHandler.ActiveView = this;
                }

                ToolView.Display();
            }

            ImGui.End();
        }
    }
}
