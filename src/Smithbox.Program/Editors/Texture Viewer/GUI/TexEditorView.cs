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
        DisplayMenubar();

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
            float height = ImGui.GetContentRegionAvail().Y * CFG.Current.Interace_Editor_Display_Inner_Height_Percent;

            ImGui.BeginChild("##FileListArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            ContainerList.Display(width, height * CFG.Current.TextureViewer_Display_ContainerList_Percentage);
            InternalFileList.Display(width, height * CFG.Current.TextureViewer_Display_InternalFileList_Percentage);
            FileList.Display(width, height * CFG.Current.TextureViewer_Display_FileList_Percentage);

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
    public void DisplayMenubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Options"))
            {
                if (ImGui.BeginMenu("Display"))
                {
                    ImGui.SliderFloat("Containers##containersDisplayPercentage", ref CFG.Current.TextureViewer_Display_ContainerList_Percentage, 0.01f, 0.99f);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        var remainder = 1f - CFG.Current.TextureViewer_Display_ContainerList_Percentage;

                        StudioMath.Redistribute(
                            ref CFG.Current.TextureViewer_Display_InternalFileList_Percentage,
                            ref CFG.Current.TextureViewer_Display_FileList_Percentage,
                            remainder);
                    }
                    UIHelper.Tooltip("The percentage of the window the Containers section occupies.");

                    ImGui.SliderFloat("Files##internalFilesDisplayPercentage", ref CFG.Current.TextureViewer_Display_InternalFileList_Percentage, 0.01f, 0.99f);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        var remainder = 1f - CFG.Current.TextureViewer_Display_InternalFileList_Percentage;

                        StudioMath.Redistribute(
                            ref CFG.Current.TextureViewer_Display_ContainerList_Percentage,
                            ref CFG.Current.TextureViewer_Display_FileList_Percentage,
                            remainder);
                    }
                    UIHelper.Tooltip("The percentage of the window the Files section occupies.");

                    ImGui.SliderFloat("Textures##filesDisplayPercentage", ref CFG.Current.TextureViewer_Display_FileList_Percentage, 0.01f, 0.99f);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        var remainder = 1f - CFG.Current.TextureViewer_Display_FileList_Percentage;

                        StudioMath.Redistribute(
                            ref CFG.Current.TextureViewer_Display_ContainerList_Percentage,
                            ref CFG.Current.TextureViewer_Display_InternalFileList_Percentage,
                            remainder);
                    }
                    UIHelper.Tooltip("The percentage of the window the Textures section occupies.");

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }
}
