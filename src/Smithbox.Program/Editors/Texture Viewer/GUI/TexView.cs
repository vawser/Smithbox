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

public class TexView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexViewSelection Selection;
    public TexViewerZoom ViewerZoom;
    public TexContainerListMenu ContainerListMenu;
    public TexInternalFileListMenu InternalFileListMenu;
    public TexTextureFileListMenu TextureFileListMenu;
    public TexDisplayWindow DisplayView;
    public TexPropertyWindow PropertyView;

    public int ViewIndex;
    private int _imguiId = -1;

    public bool JumpToSelectedRow = false;
    public bool _isSearchBarActive = false;

    public TexView(TextureViewerScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;
        _imguiId = imguiId;

        Selection = new TexViewSelection(this, Project);

        ViewerZoom = new TexViewerZoom(this, Project);

        ContainerListMenu = new TexContainerListMenu(this, Project);
        InternalFileListMenu = new TexInternalFileListMenu(this, Project);
        TextureFileListMenu = new TexTextureFileListMenu(this, Project);

        DisplayView = new TexDisplayWindow(this, Project);
        PropertyView = new TexPropertyWindow(this, Project);
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        var columnCount = 3;

        if(!CFG.Current.Interface_TextureViewer_Properties)
        {
            columnCount = 2;
        }

        if (EditorTableUtils.ImGuiTableStdColumns("textureTable", columnCount, true))
        {
            ImGui.TableSetupColumn("textureCol1", ImGuiTableColumnFlags.None, 0.3f);
            ImGui.TableSetupColumn("textureCol2", ImGuiTableColumnFlags.None, 0.5f);

            if (CFG.Current.Interface_TextureViewer_Properties)
            {
                ImGui.TableSetupColumn("textureCol3", ImGuiTableColumnFlags.None, 0.2f);
            }

            var windowWidth = ImGui.GetWindowSize().X;
            var windowHeight = ImGui.GetWindowSize().Y;

            if (ImGui.TableNextColumn())
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_FileList);

                var width = ImGui.GetColumnWidth();
                var containerHeight = windowHeight * 0.2f;
                var internalFileHeight = windowHeight * 0.05f;
                var fileHeight = windowHeight * 0.5f;

                ContainerListMenu.Display(width, containerHeight);
                InternalFileListMenu.Display(width, internalFileHeight);
                TextureFileListMenu.Display(width, fileHeight);
            }

            if (ImGui.TableNextColumn())
            {
                if(ImGui.TableGetHoveredColumn() != -1)
                {
                    Editor.ViewHandler.ActiveView = this;
                }

                FocusManager.SetFocus(EditorFocusContext.TextureViewer_Viewer);

                DisplayView.Display();
            }

            if (CFG.Current.Interface_TextureViewer_Properties)
            {
                if (ImGui.TableNextColumn())
                {
                    if (ImGui.TableGetHoveredColumn() != -1)
                    {
                        Editor.ViewHandler.ActiveView = this;
                    }

                    FocusManager.SetFocus(EditorFocusContext.TextureViewer_Properties);

                    PropertyView.Display();
                }
            }

            ImGui.EndTable();
        }

        ContainerListMenu.Update();
        TextureFileListMenu.Update();
    }
}
