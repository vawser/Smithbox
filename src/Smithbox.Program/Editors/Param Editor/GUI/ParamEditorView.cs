using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.ParamEditor;


public class ParamEditorView
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ParamSelection Selection;

    public ParamListWindow ParamListWindow;
    public ParamTableWindow ParamTableWindow;
    public ParamRowWindow ParamRowWindow;
    public ParamFieldWindow ParamFieldWindow;
    public ParamToolMenu ToolMenu;

    public ParamRowDecorators RowDecorators;
    public ParamFieldDecorators FieldDecorators;
    public ParamFieldInput FieldInputHandler;
    public MassEdit MassEdit;

    public int ViewIndex;
    private int _imguiId = -1;

    public bool JumpToSelectedRow = false;
    public bool _isSearchBarActive = false;

    public ParamEditorView(ParamEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;
        _imguiId = imguiId;

        Selection = new(editor, project);

        RowDecorators = new ParamRowDecorators(editor, project, this);
        FieldDecorators = new ParamFieldDecorators(editor, project, this);
        FieldInputHandler = new ParamFieldInput(editor, project, this);
        MassEdit = new MassEdit(editor, project, this);

        ParamListWindow = new ParamListWindow(editor, project, this);
        ParamTableWindow = new ParamTableWindow(editor, project, this);
        ParamRowWindow = new ParamRowWindow(editor, project, this);
        ParamFieldWindow = new ParamFieldWindow(editor, project, this);
        ToolMenu = new ParamToolMenu(this, project);
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        var scrollTo = 0f;

        var activeParam = Selection.GetActiveParam();
        Param.Row activeRow = Selection.GetActiveRow();

        if (ParamTableWindow.IsInTableGroupMode(activeParam))
        {

        }

        // Params
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ParamEditorView);
        if (ImGui.Begin($@"{LOC.Get("PARAM_Window_Params")}###paramEditor_ParamList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.ParamEditor_ParamList);
                Editor.ViewHandler.ActiveView = this;
            }

            ParamListWindow.Display(doFocus, isActiveView, scrollTo);
        }

        ImGui.End();

        // Tables
        if (ParamTableWindow.IsInTableGroupMode(activeParam))
        {
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_ParamEditorView);
            if (ImGui.Begin($@"{LOC.Get("PARAM_Window_Tables")}###paramEditor_TableList_{viewIndex}", GUI.GetInnerWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.ParamEditor_TableList);
                    Editor.ViewHandler.ActiveView = this;
                }

                ParamTableWindow.Display(doFocus, isActiveView, scrollTo, activeParam);
            }

            ImGui.End();
        }

        // Rows
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ParamEditorView);
        if (ImGui.Begin($@"{LOC.Get("PARAM_Window_Rows")}###paramEditor_RowList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);
                Editor.ViewHandler.ActiveView = this;
            }

            ParamRowWindow.Display(doFocus, isActiveView, scrollTo, activeParam);
        }

        ImGui.End();

        // Fields
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ParamEditorView);
        if (ImGui.Begin($@"{LOC.Get("PARAM_Window_Fields")}###paramEditor_FieldList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.ParamEditor_FieldList);
                Editor.ViewHandler.ActiveView = this;
            }

            ParamFieldWindow.Display(isActiveView, activeParam, activeRow);
        }

        ImGui.End();

        // Tools
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_ParamEditorView);
        if (ImGui.Begin($@"{LOC.Get("PARAM_Window_Tool_Window")}###paramEditor_ToolWindow_{viewIndex}", GUI.GetMainWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.ParamEditor_Tools);
                Editor.ViewHandler.ActiveView = this;
            }

            ToolMenu.Draw();
        }

        ImGui.End();

    }

    public ParamData GetParamData()
    {
        return Project.Handler.ParamData;
    }

    public ParamBank GetPrimaryBank()
    {
        return Project.Handler.ParamData.PrimaryBank;
    }

    public ParamBank GetVanillaBank()
    {
        return Project.Handler.ParamData.VanillaBank;
    }

}
