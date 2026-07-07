using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

public class FileEditorView
{
    public FileBrowserScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public FileSelection Selection;

    public FileListView FileList;
    public FileItemView ItemViewer;
    public FileToolView ToolView;

    public int ViewIndex;

    public FileEditorView(FileBrowserScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;

        Selection = new(this, project);

        FileList = new(this, project);
        ItemViewer = new(this, project);
        ToolView = new(this, project);
    }


    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // File List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_FileBrowserView);
        if (ImGui.Begin($@"File List##fileBrowser_FileList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.FileBrowser_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            FileList.Display();
        }

        ImGui.End();

        // Item Viewer
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_FileBrowserView);
        if (ImGui.Begin($@"Item Viewer##fileBrowser_ItemViwwer_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.FileBrowser_Item);
                Editor.ViewHandler.ActiveView = this;
            }

            ItemViewer.Display();
        }

        ImGui.End();

        // Tools
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_FileBrowserView);
        if (ImGui.Begin($@"Tools##fileBrowser_ToolWindow_{viewIndex}", GUI.GetMainWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.FileBrowser_Tools);
                Editor.ViewHandler.ActiveView = this;
            }

            ToolView.Display();
        }

        ImGui.End();
    }
}
