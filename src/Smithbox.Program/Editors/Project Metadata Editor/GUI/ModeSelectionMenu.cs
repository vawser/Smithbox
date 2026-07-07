using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Octokit;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapDataEditor;
using StudioCore.Editors.MapParamEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.MetadataEditor;

public class ModeSelectionMenu
{
    public ProjectMetadataScreen Editor;

    public ModeSelectionMenu(ProjectMetadataScreen editor)
    {
        Editor = editor;
    }

    public void Display()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if(curProject == null)
        {
            ImGui.Text("META_Select_Project_First");
            return;
        }

        var curMode = Editor.Selection.EditorMode;

        GUI.SimpleHeader(
            LOC.Get("META_Header_Editor_Mode"),
            LOC.Get("META_Header_Editor_Mode_TT"));

        ImGui.BeginChild("EditorModeSection", new Vector2(0, 40), ImGuiChildFlags.Borders);
        GUI.SetInputWidth();

        var previewName = LOC.Get(curMode.GetDisplayName());

        if (ImGui.BeginCombo("##subEditorMode", previewName))
        {
            foreach (var entry in Enum.GetValues(typeof(MetadataEditorMode)))
            {
                var curType = (MetadataEditorMode)entry;

                var displayName = LOC.Get(curType.GetDisplayName());

                if (ImGui.Selectable(displayName, curType == curMode))
                {
                    Editor.Selection.EditorMode = curType;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.EndChild();
    }
}
