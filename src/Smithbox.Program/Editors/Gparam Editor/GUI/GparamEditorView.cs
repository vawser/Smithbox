using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public class GparamEditorView
{
    public GparamEditorScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

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

        Selection = new GparamSelection(this, Project);

        PropertyEditor = new GparamPropertyEditor(this, Project);
        QuickEditHandler = new GparamQuickEdit(this, Project);

        FileListView = new GparamFileList(this, Project);
        GroupListView = new GparamGroupList(this, Project);
        FieldListView = new GparamFieldList(this, Project);
        FieldValueListView = new GparamValueList(this, Project);
        ToolView = new GparamToolView(this, Project);
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Files
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_GparamEditorView);
        if (ImGui.Begin($@"Files##gparamEditor_FileList_{viewIndex}", UIHelper.GetInnerWindowFlags()))
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
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_GparamEditorView);
        if (ImGui.Begin($@"Groups##gparamEditor_GroupList_{viewIndex}", UIHelper.GetInnerWindowFlags()))
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
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_GparamEditorView);
        if (ImGui.Begin($@"Fields##gparamEditor_FieldList_{viewIndex}", UIHelper.GetInnerWindowFlags()))
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
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_GparamEditorView);
        if (ImGui.Begin($@"Field Values##gparamEditor_FieldValueList_{viewIndex}", UIHelper.GetInnerWindowFlags()))
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

        // Tools
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_GparamEditorView);
        if (ImGui.Begin($@"Tools##gparamEditor_ToolWindow_{viewIndex}", UIHelper.GetMainWindowFlags()))
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
