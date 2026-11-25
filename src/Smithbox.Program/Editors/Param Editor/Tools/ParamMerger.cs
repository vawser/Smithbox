using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// The Param Editor's Param Merge tool section: merges regulation files and other Smithbox projects (optionally with
/// this project as the earliest source) through the auto-merge engine. The UI lives in
/// <see cref="ParamDeltaAutoMergeTool.DisplayDirectRegulationMerge"/>.
/// <para>
/// The Mass Edit merger that used to be a second tab here (auxparam paste commands) was replaced by project sources
/// in this merge; its code is in .archive/src/Smithbox.Program/Editors/Param Editor/Tools/ParamMerger.cs.
/// </para>
/// </summary>
public class ParamMerger
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public ParamMerger(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        if (!ToolSections.Header($"{LOC.Get("PARAM_Merger_Header")}##paramMergerHeader"))
            return;

        ImGui.BeginChild("ParamMergerToolSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("PARAM_Merger_Hint"));

        GUI.Spacer();
        View.ToolMenu.DeltaPatcher.AutoMergeTool.DisplayConflictPolicy();
        View.ToolMenu.DeltaPatcher.AutoMergeTool.DisplayDirectRegulationMerge();

        ImGui.EndChild();
    }
}
