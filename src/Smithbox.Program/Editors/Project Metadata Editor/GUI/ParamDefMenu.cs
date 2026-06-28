using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.MetadataEditor;

public class ParamDefMenu
{
    public ProjectMetadataScreen Editor;

    public ParamDefMenu(ProjectMetadataScreen editor)
    {
        Editor = editor;
    }

    public void Display()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        UIHelper.SimpleHeader(
            LOC.Get("META_Header_Param_Def"),
            LOC.Get("META_Header_Param_Def_TT"));

        ImGui.BeginChild("ParamDefEditorSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        ImGui.TextDisabled("Coming soon...");

        ImGui.EndChild();
    }
}
