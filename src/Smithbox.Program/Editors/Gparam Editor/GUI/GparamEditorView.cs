using Hexa.NET.ImGui;
using HKLib.hk2018.hkaiCollisionAvoidance;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.FileBrowser;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamEditorView
{
    public GparamEditorScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public int ViewIndex;

    public GparamSelection Selection;

    public GparamPropertyEditor PropertyEditor;
    public GparamActionHandler ActionHandler;
    public GparamFilters Filters;
    public GparamContextMenu ContextMenu;

    public GparamQuickEdit QuickEditHandler;

    public GparamFileList FileListView;
    public GparamGroupList GroupListView;
    public GparamFieldList FieldListView;
    public GparamValueList FieldValueListView;

    public GparamEditorView(GparamEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;

        Selection = new GparamSelection(this, Project);
        ActionHandler = new GparamActionHandler(this, Project);
        Filters = new GparamFilters(this, Project);
        ContextMenu = new GparamContextMenu(this, Project);

        PropertyEditor = new GparamPropertyEditor(this, Project);
        QuickEditHandler = new GparamQuickEdit(this, Project);

        FileListView = new GparamFileList(this, Project);
        GroupListView = new GparamGroupList(this, Project);
        FieldListView = new GparamFieldList(this, Project);
        FieldValueListView = new GparamValueList(this, Project);
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        var columnCount = 4;
        var windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        if (ImGui.BeginTable("gparamTable", columnCount,
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp |
            ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("##FileList", ImGuiTableColumnFlags.WidthStretch, 0.25f);
            ImGui.TableSetupColumn("##GroupList", ImGuiTableColumnFlags.WidthStretch, 0.25f);
            ImGui.TableSetupColumn("##FieldList", ImGuiTableColumnFlags.WidthStretch, 0.25f);
            ImGui.TableSetupColumn("##FieldValueList", ImGuiTableColumnFlags.WidthStretch, 0.25f);

            // --- Column 1 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##FileListArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.GparamEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            FileListView.Display();

            ImGui.EndChild();


            // --- Column 2 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##GroupListArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.GparamEditor_GroupList);
                Editor.ViewHandler.ActiveView = this;
            }

            GroupListView.Display();

            ImGui.EndChild();

            // --- Column 3 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##FieldListArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.GparamEditor_FieldList);
                Editor.ViewHandler.ActiveView = this;
            }

            FieldListView.Display();

            ImGui.EndChild();

            // --- Column 4 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##FieldValueListArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.GparamEditor_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            FieldValueListView.Display();

            ImGui.EndChild();

            ImGui.EndTable();
        }
    }
}
