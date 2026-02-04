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

    public int ViewIndex;

    public FileEditorView(FileBrowserScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;

        Selection = new(this, project);

        FileList = new(this, project);
        ItemViewer = new(this, project);
    }


    public void Display(bool doFocus, bool isActiveView)
    {
        var columnCount = 2;
        var windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        if (ImGui.BeginTable("fileBrowserTable", columnCount,
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp |
            ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("##FileList", ImGuiTableColumnFlags.WidthStretch, 0.25f);
            ImGui.TableSetupColumn("##FileViewer", ImGuiTableColumnFlags.WidthStretch, 0.25f);

            // --- Column 1 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##FileListArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.FileBrowser_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            FileList.Display();

            ImGui.EndChild();

            // --- Column 2 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##FileViewerArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.FileBrowser_Item);
                Editor.ViewHandler.ActiveView = this;
            }

            ItemViewer.Display();

            ImGui.EndChild();

            ImGui.EndTable();
        }
    }
}
