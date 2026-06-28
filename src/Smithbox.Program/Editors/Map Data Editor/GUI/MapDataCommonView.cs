using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapParamEditor;
using StudioCore.Utilities;
using System;
using System.Numerics;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataCommonView
{
    public MapDataEditorView View;
    public ProjectEntry Project;

    public MapDataCommonView(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Draw(float width, float height)
    {
        UIHelper.SimpleHeader("Editor Mode", "");

        ImGui.BeginChild("EditorModeSection", new Vector2(0, 40), ImGuiChildFlags.Borders);
        UIHelper.SetInputWidth();
        if (ImGui.BeginCombo("##subEditorMode", View.Selection.SubEditorMode.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(SubEditorType)))
            {
                var curType = (SubEditorType)entry;

                if (curType is SubEditorType.MSB && !MapDataUtils.SupportsMSB(Project))
                {
                    continue;
                }

                if (curType is SubEditorType.ENFL && !MapDataUtils.SupportsENFL(Project))
                {
                    continue;
                }

                if (ImGui.Selectable($"{curType.GetDisplayName()}", curType == View.Selection.SubEditorMode))
                {
                    View.Selection.SubEditorMode = curType;
                }
            }

            ImGui.EndCombo();
        }
        ImGui.EndChild();

        width = ImGui.GetContentRegionAvail().X;
        height = ImGui.GetContentRegionAvail().Y * 0.95f;

        // Handle this within the subeditor class itself
        if (View.Selection.SubEditorMode is SubEditorType.MSB)
        {
            View.MsbEditor.DisplayHeader();

            ImGui.BeginChild("MsbFileList", new Vector2(0, 0), ImGuiChildFlags.Borders);
            View.MsbEditor.DisplaySourceList();
            ImGui.EndChild();
        }
        else if (View.Selection.SubEditorMode is SubEditorType.ENFL)
        {
            View.EnflEditor.DisplayHeader();

            ImGui.BeginChild("EnflFileList", new Vector2(0, 0), ImGuiChildFlags.Borders);
            View.EnflEditor.DisplaySourceList();
            ImGui.EndChild();
        }
    }
}
