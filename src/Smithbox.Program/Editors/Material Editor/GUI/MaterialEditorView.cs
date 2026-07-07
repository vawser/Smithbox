using Hexa.NET.ImGui;
using HKLib.hk2018.hkaiCollisionAvoidance;
using Silk.NET.SDL;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialEditorView
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
        Properties = new(this, project);

        ToolView = new(this, project);
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        if (Project.Handler.MaterialData.PrimaryBank == null)
            return;

        // Container List
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MaterialEditorView);
        if (ImGui.Begin($@"Container List##materialEditor_ContainerList_{viewIndex}", GUI.GetInnerWindowFlags()))
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
        if (ImGui.Begin($@"File List##materialEditor_FileList_{viewIndex}", GUI.GetInnerWindowFlags()))
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

        // Properties
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MaterialEditorView);
        if (ImGui.Begin($@"Properties##materialEditor_Properties_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MaterialEditor_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            Properties.Draw();
        }

        ImGui.End();

        // Tools
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_MaterialEditorView);
        if (ImGui.Begin($@"Tools##materialEditor_ToolWindow_{viewIndex}", GUI.GetInnerWindowFlags()))
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
