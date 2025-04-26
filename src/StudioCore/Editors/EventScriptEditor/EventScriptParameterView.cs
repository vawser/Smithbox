using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;

namespace StudioCore.Editors.EventScriptEditorNS;

public class EventScriptParameterView
{
    public Project Project;
    public EventScriptEditor Editor;

    public EventScriptParameterView(Project curProject, EventScriptEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        Editor.EditorFocus.SetFocusContext(EventScriptEditorContext.EventProperties);

        if (Editor.Selection.SelectedEvent != null)
        {
            var evt = Editor.Selection.SelectedEvent;

            ImGui.Text($"{evt.RestBehavior}");

            ImGui.Separator();

            foreach (var para in Editor.Selection.SelectedEvent.Parameters)
            {
                ImGui.Text($"InstructionIndex: {para.InstructionIndex}");
                ImGui.Text($"TargetStartByte: {para.TargetStartByte}");
                ImGui.Text($"SourceStartByte: {para.SourceStartByte}");
                ImGui.Text($"ByteCount: {para.ByteCount}");
                ImGui.Text($"UnkID: {para.UnkID}");

                ImGui.Separator();
            }
        }
    }
}
