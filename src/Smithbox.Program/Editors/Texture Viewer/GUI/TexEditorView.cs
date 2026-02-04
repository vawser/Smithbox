using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.TextureViewer;

public class TexEditorView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexViewSelection Selection;
    public TexViewerZoom ZoomState;
    public TexContainerList ContainerList;
    public TexInternalFileList InternalFileList;
    public TexTextureFileList FileList;
    public TexDisplayViewport DisplayViewport;
    public TexProperties Properties;

    public int ViewIndex;
    private int _imguiId = -1;

    public bool JumpToSelectedRow = false;
    public bool _isSearchBarActive = false;

    public TexEditorView(TextureViewerScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;
        _imguiId = imguiId;

        Selection = new TexViewSelection(this, Project);

        ZoomState = new TexViewerZoom(this, Project);

        ContainerList = new TexContainerList(this, Project);
        InternalFileList = new TexInternalFileList(this, Project);
        FileList = new TexTextureFileList(this, Project);

        DisplayViewport = new TexDisplayViewport(this, Project);
        Properties = new TexProperties(this, Project);
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        var columnCount = 3;

        if(!CFG.Current.Interface_TextureViewer_Properties)
        {
            columnCount = 2;
        }

        if (ImGui.BeginTable("textureTable", columnCount,
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp |
            ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("##File Lists", ImGuiTableColumnFlags.WidthStretch, 0.3f);
            ImGui.TableSetupColumn("##Viewer", ImGuiTableColumnFlags.WidthStretch, 0.5f);

            if (CFG.Current.Interface_TextureViewer_Properties)
                ImGui.TableSetupColumn("##Properties", ImGuiTableColumnFlags.WidthStretch, 0.2f);

            // --- Column 1 ---
            ImGui.TableNextColumn();
            float width = ImGui.GetContentRegionAvail().X;
            float height = ImGui.GetContentRegionAvail().Y;

            ContainerList.Display(width, height * 0.2f);
            InternalFileList.Display(width, height * 0.1f);
            FileList.Display(width, height * 0.6f);

            // --- Column 2 ---
            ImGui.TableNextColumn();
            DisplayViewport.Display();

            // --- Column 3 ---
            if (CFG.Current.Interface_TextureViewer_Properties)
            {
                ImGui.TableNextColumn();
                Properties.Display();
            }

            ImGui.EndTable();
        }

        ContainerList.Update();
        FileList.Update();
    }
}
