using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialEditorView : IEditorView
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public MaterialPropertyCache MaterialPropertyCache = new();

    public MaterialSelection Selection;
    public MaterialPropertyInput PropertyInput;

    public MaterialContainerList ContainerList;
    public MaterialFileList FileList;
    public MaterialProperties Properties;
    public MaterialProperties TextureProperties;
    public MaterialToolWindow ToolView;

    public int ViewIndex;

    public MaterialEditorView(MaterialEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;

        Selection = new(this, project);
        PropertyInput = new(this, project);

        ContainerList = new(this, project);
        FileList = new(this, project);
        Properties = new(this, project, MaterialPropertySection.Parameters);
        TextureProperties = new(this, project, MaterialPropertySection.Textures);

        ToolView = new(this, project);
    }

    /// <summary>
    /// Searches the Parameters and Textures windows for a field, as clicking a check finding or search result does
    /// </summary>
    public void SetPropertyFilter(string text)
    {
        Properties.SetFilter(text);
        TextureProperties.SetFilter(text);
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        if (Project.Handler.MaterialData.PrimaryBank == null)
            return;

        // Container List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MaterialEditorView);
        if (ImGui.Begin($@"{LOC.Get("MAT_Window_Container_List")}###materialEditor_ContainerList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MaterialEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            ContainerList.Draw(width, height);
        }

        ImGui.End();

        // File List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MaterialEditorView);
        if (ImGui.Begin($@"{LOC.Get("MAT_Window_File_List")}###materialEditor_FileList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MaterialEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            FileList.Draw(width, height);
        }

        ImGui.End();

        // Parameters: keeps the old Properties window's ID, so saved layouts put it where Properties was
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MaterialEditorView);
        if (ImGui.Begin($@"{LOC.Get("MAT_Window_Parameters")}###materialEditor_Properties_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MaterialEditor_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            Properties.Draw();
        }

        ImGui.End();

        // Textures / Samplers
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MaterialEditorView);
        if (ImGui.Begin($@"{LOC.Get("MAT_Window_Textures")}###materialEditor_Textures_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MaterialEditor_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            TextureProperties.Draw();
        }

        ImGui.End();

        if (CFG.Current.Interface_MaterialEditor_ToolWindow)
        {
            // Tools
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_MaterialEditorView);
            if (ImGui.Begin($@"{LOC.Get("MAT_Window_Tools")}###materialEditor_ToolWindow_{viewIndex}", GUI.GetInnerWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.MaterialEditor_Tools);
                    Editor.ViewHandler.ActiveView = this;
                }

                ToolView.Draw();
            }

            ImGui.End();
        }
    }
}
