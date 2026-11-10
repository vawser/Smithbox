using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using SoulsFormats;
using StudioCore.Renderer;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TexEditorView : IEditorView
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
    public TexToolView ToolView;
    public TextureImport TextureImport;
    public TpfCommands Commands;
    public TexNamePrompt NamePrompt = new();
    public LayoutEditor LayoutEditor;

    /// <summary>Which list the mouse was last over, so Copy/Paste/Duplicate/Delete act on its items.</summary>
    public enum ListFocus { None, Containers, Tpfs, Textures }
    public ListFocus FocusedList = ListFocus.None;

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
        TextureImport = new TextureImport(this, Project);
        Commands = new TpfCommands(this, Project);
        LayoutEditor = new LayoutEditor(this, Project);
        ToolView = new TexToolView(this, Project);

        DisplayViewport = new TexDisplayViewport(this, Project);
        Properties = new TexProperties(this, Project);

        // Every edit, undo and redo: the Param Editor's icon previews are looked up again (T7)
        ActionManager.ActionPerformed += () => Smithbox.TextureManager?.IconManager?.NotifyEdited();
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Container List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TextureViewerView);
        if (ImGui.Begin($@"{LOC.Get("TEXVIEW_Window_Container_List")}###textureEditor_ContainerList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_FileList);
                FocusedList = ListFocus.Containers;
                Editor.ViewHandler.ActiveView = this;
            }

            ContainerList.Display(width, height);
        }

        ImGui.End();

        // File List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TextureViewerView);
        if (ImGui.Begin($@"{LOC.Get("TEXVIEW_Window_File_List")}###textureEditor_FileList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_FileList);
                FocusedList = ListFocus.Tpfs;
                Editor.ViewHandler.ActiveView = this;
            }

            FileList.Display(width, height);
        }

        ImGui.End();

        // Texture List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TextureViewerView);
        if (ImGui.Begin($@"{LOC.Get("TEXVIEW_Window_Texture_List")}###textureEditor_TextureList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_FileList);
                FocusedList = ListFocus.Textures;
                Editor.ViewHandler.ActiveView = this;
            }

            TextureList.Display(width, height);
        }

        ImGui.End();

        // Texture Viewport
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_TextureViewerView);
        if (ImGui.Begin($@"{LOC.Get("TEXVIEW_Window_Viewer")}###textureEditor_Viewer_{viewIndex}", GUI.GetTextureViewerFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.TextureViewer_Viewer);
                FocusedList = ListFocus.None;
                Editor.ViewHandler.ActiveView = this;
            }

            DisplayViewport.Display();
        }

        ImGui.End();

        // Properties
        if (CFG.Current.Interface_TextureViewer_Properties)
        {
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_TextureViewerView);
            if (ImGui.Begin($@"{LOC.Get("TEXVIEW_Window_Properties")}###textureEditor_Properties_{viewIndex}", GUI.GetInnerWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.TextureViewer_Properties);
                    FocusedList = ListFocus.None;
                    Editor.ViewHandler.ActiveView = this;
                }

                Properties.Display();
            }

            ImGui.End();
        }

        // Layout (T5)
        if (CFG.Current.Interface_TextureViewer_Layout)
        {
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_TextureViewerView);
            if (ImGui.Begin($@"{LOC.Get("TEXVIEW_Window_Layout")}###textureEditor_Layout_{viewIndex}", GUI.GetInnerWindowFlags()))
            {
                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.TextureViewer_Properties);
                    FocusedList = ListFocus.None;
                    Editor.ViewHandler.ActiveView = this;
                }

                LayoutEditor.DisplayWindow();
            }

            ImGui.End();
        }

        if (CFG.Current.Interface_TextureViewer_ToolWindow)
        {
            /// Tools
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_TextureViewerView);
            if (ImGui.Begin($@"{LOC.Get("TEXVIEW_Window_Tools")}###textureEditor_ToolWindow_{viewIndex}", GUI.GetMainWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.TextureViewer_Tools);
                    FocusedList = ListFocus.None;
                    Editor.ViewHandler.ActiveView = this;
                }

                ToolView.Display();
            }

            ImGui.End();
        }

        NamePrompt.Draw();

        ContainerList.Update();
        TextureImport.Update();
        TextureList.Update();
    }

    /// <summary>
    /// The TPF the selection is in, for edit actions; null when nothing is selected or the TPF can't be edited.
    /// </summary>
    public TextureTarget GetEditTarget()
    {
        if (Selection.SelectedBinder == null || Selection.SelectedTpfBinderFile == null || Selection.SelectedTpf == null)
            return null;

        if (!TextureFormats.IsEditable(Selection.SelectedTpf))
            return null;

        return new TextureTarget(Selection.SelectedBinder, Selection.SelectedTpfBinderFile, Selection.SelectedTpf);
    }

    /// <summary>
    /// Called by texture actions (and their undo) after a texture's fields or payload changed.
    /// </summary>
    public void OnTextureChanged(TPF tpf, TPF.Texture tex)
    {
        if (Selection.SelectedTpf == tpf)
        {
            TextureList.SelectAndLoad(tex);
        }
    }

    /// <summary>
    /// Called by TPF actions after a TPF was added to, removed from or renamed in a container. Selects the given
    /// TPF (and its first texture), or clears the TPF selection when it is null.
    /// </summary>
    public void OnTpfListChanged(BinderContents container, BinderFile select)
    {
        if (Selection.SelectedBinder != container)
            return;

        if (select != null && container.Files.TryGetValue(select, out var tpf))
        {
            Selection.SelectTpfFile(select, tpf);
            Selection.AutoSelectTexture = true;
        }
        else
        {
            Selection.SelectedTpfKey = "";
            Selection.SelectedTpfBinderFile = null;
            Selection.SelectedTpf = null;
            TextureList.SelectAndLoad(null);
        }
    }

    /// <summary>
    /// The selected container, when its TPFs can be added, removed and renamed: a real binder (not the stand-in
    /// for a loose .tpf) whose TPFs are PC ones. Null otherwise.
    /// </summary>
    public BinderContents GetEditableContainer()
    {
        var container = Selection.SelectedBinder;

        if (container == null || container.Loose || container.Binder == null)
            return null;

        if (container.Files.Values.Any(t => !TextureFormats.IsEditable(t)))
            return null;

        return container;
    }

    /// <summary>
    /// Called by texture actions after a texture was added to or removed from a TPF; selects the given texture.
    /// </summary>
    public void OnTextureListChanged(TPF tpf, TPF.Texture select)
    {
        if (Selection.SelectedTpf == tpf)
        {
            TextureList.SelectAndLoad(select);
        }
    }
}
