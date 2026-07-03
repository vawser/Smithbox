using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.MetadataEditor;

public class ParamMetaMenu
{
    public ProjectMetadataScreen Editor;

    public ParamMetaMenu(ProjectMetadataScreen editor)
    {
        Editor = editor;
    }

    public void Display()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        UIHelper.SimpleHeader(
            LOC.Get("META_Header_Param_Meta"),
            LOC.Get("META_Header_Param_Meta_TT"));

        ImGui.BeginChild("ParamMetaEditorSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        ImGui.TextDisabled(LOC.Get("META_Editor_WIP"));

        ImGui.EndChild();
    }
}
