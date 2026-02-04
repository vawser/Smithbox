using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TexEditorView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexViewSelection Selection;

    public TexViewerZoom ZoomState;
    public TexFilters Filters;

    public TexContainerList ContainerList;
    public TexInternalFileList InternalFileList;
    public TexTextureFileList FileList;
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
        Filters = new TexFilters(this, Project);

        ContainerList = new TexContainerList(this, Project);
        InternalFileList = new TexInternalFileList(this, Project);
        FileList = new TexTextureFileList(this, Project);

        DisplayViewport = new TexDisplayViewport(this, Project);
        Properties = new TexProperties(this, Project);
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        var columnCount = 3;
        var windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        if (!CFG.Current.Interface_TextureViewer_Properties)
        {
            columnCount = 2;
        }

        if (ImGui.BeginTable("textureTable", columnCount,
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp |
            ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("##FileList", ImGuiTableColumnFlags.WidthStretch, 0.3f);
            ImGui.TableSetupColumn("##Viewer", ImGuiTableColumnFlags.WidthStretch, 0.5f);

            if (CFG.Current.Interface_TextureViewer_Properties)
                ImGui.TableSetupColumn("##Properties", ImGuiTableColumnFlags.WidthStretch, 0.2f);

            // --- Column 1 ---
            ImGui.TableNextColumn();

            float width = ImGui.GetContentRegionAvail().X;
            float height = ImGui.GetContentRegionAvail().Y;

            ImGui.BeginChild("##FileListArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            ContainerList.Display(width, height * 0.2f);
            InternalFileList.Display(width, height * 0.1f);
            FileList.Display(width, height * 0.6f);

            ImGui.EndChild();

            // --- Column 2 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##ViewerArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_Viewer);
                Editor.ViewHandler.ActiveView = this;
            }

            DisplayViewport.Display();

            ImGui.EndChild();

            // --- Column 3 ---
            if (CFG.Current.Interface_TextureViewer_Properties)
            {
                ImGui.TableNextColumn();

                ImGui.BeginChild("##PropertiesArea", new Vector2(0, 0), windowFlags);

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.TextureViewer_Properties);
                    Editor.ViewHandler.ActiveView = this;
                }

                Properties.Display();

                ImGui.EndChild();
            }

            ImGui.EndTable();
        }

        ContainerList.Update();
        FileList.Update();
    }
}
