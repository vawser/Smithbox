using Hexa.NET.ImGui;
using StudioCore.Core;

namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Handles the EMEVD event parameter viewing and editing.
/// </summary>
public class EmevdEventParameterView
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public EmevdEventParameterView(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the event parameter view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Event Properties##EventParameterView");
        Editor.Selection.SwitchWindowContext(EmevdEditorContext.EventProperties);

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

        ImGui.End();
    }
}
