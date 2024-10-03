using ImGuiNET;
using StudioCore.EmevdEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

public class EmevdEventHandler
{
    private EmevdEditorScreen Screen;

    public EmevdEventHandler(EmevdEditorScreen screen)
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
            var evt = Screen._selectedEvent;

            ImGui.Text($"{evt.RestBehavior}");

            ImGui.Separator();

            foreach (var para in Screen._selectedEvent.Parameters)
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
