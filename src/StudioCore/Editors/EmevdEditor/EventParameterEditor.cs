using ImGuiNET;
using StudioCore.EmevdEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

public class EventParameterEditor
{
    private EmevdEditorScreen Screen;

    public EventParameterEditor(EmevdEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {

    }

    public void Display()
    {
        if (Screen._selectedEvent != null)
        {
            foreach (var para in Screen._selectedEvent.Parameters)
            {
                ImGui.Text($"InstructionIndex: {para.InstructionIndex}");
                ImGui.Text($"TargetStartByte: {para.TargetStartByte}");
                ImGui.Text($"SourceStartByte: {para.SourceStartByte}");
                ImGui.Text($"ByteCount: {para.ByteCount}");
                ImGui.Text($"UnkID: {para.UnkID}");
            }
        }
    }
}
