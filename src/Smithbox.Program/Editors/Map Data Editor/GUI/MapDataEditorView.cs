using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataEditorView
{
    public MapDataEditorScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public MapDataSelection Selection;

    public MapDataCommonView CommonView;

    public MsbEditor MsbEditor;
    public EnflEditor EnflEditor;

    public int ViewIndex;

    public MapDataEditorView(MapDataEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;

        Selection = new(this, project);

        CommonView = new(this, project);
        MsbEditor = new(this, project);
        EnflEditor = new(this, project);
    }
    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        // Sub-Editor Mode
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_MapDataEditorView);
        if (ImGui.Begin($@"Configuration##mapDataEditor_Common_{viewIndex}", UIHelper.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MapDataEditor_CommonView);
                Editor.ViewHandler.ActiveView = this;
            }

            CommonView.Draw(width, height);
        }

        ImGui.End();

        // MSB Editor
        if (Selection.SubEditorMode is SubEditorType.MSB)
        {
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_MapDataEditorView);
            if (ImGui.Begin($@"MSB Editor##mapDataEditor_MsbEditor_{viewIndex}", UIHelper.GetInnerWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.MapDataEditor_MsbEditor);
                    Editor.ViewHandler.ActiveView = this;
                }

                MsbEditor.Draw();
            }

            ImGui.End();
        }

        // MSB Editor
        if (Selection.SubEditorMode is SubEditorType.ENFL)
        {
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_MapDataEditorView);
            if (ImGui.Begin($@"ENFL Editor##mapDataEditor_EnflEditor_{viewIndex}", UIHelper.GetInnerWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.MapDataEditor_EnflEditor);
                    Editor.ViewHandler.ActiveView = this;
                }

                EnflEditor.Draw();
            }

            ImGui.End();
        }
    }
}
