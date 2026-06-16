using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TexEditorView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public TexViewSelection Selection;

    public TexViewerZoom ZoomState;

    public TexContainerList ContainerList;
    public TexInternalFileList FileList;
    public TexTextureFileList TextureList;
    public TexDisplayViewport DisplayViewport;
    public TexProperties Properties;

    public int ViewIndex;

    public TexEditorView(TextureViewerScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;

        Selection = new TexViewSelection(this, Project);

        ZoomState = new TexViewerZoom(this, Project);

        ContainerList = new TexContainerList(this, Project);
        FileList = new TexInternalFileList(this, Project);
        TextureList = new TexTextureFileList(this, Project);

        DisplayViewport = new TexDisplayViewport(this, Project);
        Properties = new TexProperties(this, Project);
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Container List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_TextureViewerView);
        if (ImGui.Begin($@"Container List##textureEditor_ContainerList_{viewIndex}", UIHelper.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            ContainerList.Display(width, height);
        }

        ImGui.End();

        // File List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_TextureViewerView);
        if (ImGui.Begin($@"File List##textureEditor_FileList_{viewIndex}", UIHelper.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            FileList.Display(width, height);
        }

        ImGui.End();

        // Texture List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_TextureViewerView);
        if (ImGui.Begin($@"Texture List##textureEditor_TextureList_{viewIndex}", UIHelper.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            TextureList.Display(width, height);
        }

        ImGui.End();

        // Texture Viewport
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_TextureViewerView);
        if (ImGui.Begin($@"Viewer##textureEditor_Viewer_{viewIndex}", UIHelper.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_Viewer);
                Editor.ViewHandler.ActiveView = this;
            }

            DisplayViewport.Display();
        }

        ImGui.End();

        // Properties
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_TextureViewerView);
        if (ImGui.Begin($@"Properties##textureEditor_Properties_{viewIndex}", UIHelper.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            Properties.Display();
        }

        ImGui.End();

        ContainerList.Update();
        TextureList.Update();
    }
}
