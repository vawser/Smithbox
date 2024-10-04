using ImGuiNET;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class TimelineSequencePointPropertyView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public TimelineSequencePointPropertyView(CutsceneEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        Decorator = screen.Decorator;
    }

    public void OnProjectChanged()
    {

    }
    public void Display()
    {
        ImGui.Begin("Timeline - Sequence - Point Properties##TimelineSequencePointProperties");

        var sequence = Selection._selectedTimelineSequence;
        var pointKey = Selection._selectedDispositionSequencePointKey;
        var point = Selection._selectedDispositionSequencePoint;

        if (sequence != null && sequence.Points.Count > 0)
        {
            if (pointKey != -1 && point != null)
            {
                ImGui.Columns(2);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Value");
                ImGui.Text($"Unk08");
                ImGui.Text($"Unk10");
                ImGui.Text($"Unk14");

                ImGui.NextColumn();

                // TODO: add editing
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{point.Value}");
                ImGui.Text($"{point.Unk08}");
                ImGui.Text($"{point.Unk10}");
                ImGui.Text($"{point.Unk14}");

                ImGui.Columns(1);
            }
        }

        ImGui.End();
    }
}

