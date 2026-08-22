using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

public class HavokEditorView : IEditorView
{
    public HavokEditorScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager;

    public HavokSelection Selection;

    public HavokPropertyCache PropertyCache = new();

    public HavokCategoryView CategoryView;
    public HavokBinderView BinderView;
    public HavokFileView FileView;
    public HavokPropertyView PropertyView;
    public HavokToolView Tools;

    public int ViewIndex;
    private int _imguiId = -1;

    public HavokEditorView(HavokEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;
        _imguiId = imguiId;

        Selection = new(this, project);
        ActionManager = new();

        CategoryView = new(this, project);
        BinderView = new(this, project);
        FileView = new(this, project);
        PropertyView = new(this, project);
        Tools = new(this, project);
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Category
        // -> Type of hkx to populate/reference

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
        if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_Category_List")}##havokeditor_CategoryList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.HavokEditor_CategoryList);
                Editor.ViewHandler.ActiveView = this;
            }

            CategoryView.Draw();
        }

        ImGui.End();

        // Binder List
        // -> Binders that contain the specified type of hkx files

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
        if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_Binder_List")}##havokeditor_BinderList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.HavokEditor_BinderList);
                Editor.ViewHandler.ActiveView = this;
            }

            BinderView.Draw();
        }

        ImGui.End();

        // File List
        // -> The hkx files within a binder

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
        if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_File_List")}##havokeditor_FileList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.HavokEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            FileView.Draw();
        }

        ImGui.End();

        // Properties
        // -> Editor for the havok properties (copy MapHavokPropertyView)

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
        if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_Properties")}##havokeditor_Properties_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.HavokEditor_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            PropertyView.Draw();
        }

        ImGui.End();

        // Tools
        if (CFG.Current.Interface_HavokEditor_ToolWindow)
        {
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
            if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_Tools")}##havokeditor_Tools_{viewIndex}", GUI.GetInnerWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.HavokEditor_Tools);
                    Editor.ViewHandler.ActiveView = this;
                }

                Tools.Draw();
            }

            ImGui.End();
        }
    }
}
