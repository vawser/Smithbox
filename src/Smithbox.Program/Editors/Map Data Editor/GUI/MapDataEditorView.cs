using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MaterialEditor;
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
    public void Display(bool doFocus, bool isActiveView)
    {
        var columnCount = 2;
        var windowFlags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        if (ImGui.BeginTable("mapDataTable", columnCount,
            ImGuiTableFlags.Resizable |
            ImGuiTableFlags.SizingStretchProp |
            ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn("##CommonCol", ImGuiTableColumnFlags.WidthStretch, 0.25f);
            ImGui.TableSetupColumn("##SubEditorCol", ImGuiTableColumnFlags.WidthStretch, 0.5f);

            // --- Column 1 ---
            ImGui.TableNextColumn();

            float width = ImGui.GetContentRegionAvail().X;
            float height = ImGui.GetContentRegionAvail().Y * CFG.Current.Interace_Editor_Display_Inner_Height_Percent;

            ImGui.BeginChild("##CommonViewArea", new Vector2(0, 0), windowFlags);

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.MapDataEditor_CommonView);
                Editor.ViewHandler.ActiveView = this;
            }

            CommonView.Draw(width, height);

            ImGui.EndChild();

            // --- Column 2 ---
            ImGui.TableNextColumn();

            ImGui.BeginChild("##SubEditorArea", new Vector2(0, 0), windowFlags);

            if (Selection.SubEditorMode is SubEditorType.MSB)
            {
                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.MapDataEditor_MsbEditor);
                    Editor.ViewHandler.ActiveView = this;
                }

                MsbEditor.Draw();
            }
            else if (Selection.SubEditorMode is SubEditorType.ENFL)
            {
                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.MapDataEditor_EnflEditor);
                    Editor.ViewHandler.ActiveView = this;
                }

                EnflEditor.Draw();
            }
            else
            {

            }

            ImGui.EndChild();

            ImGui.EndTable();
        }
    }
}
