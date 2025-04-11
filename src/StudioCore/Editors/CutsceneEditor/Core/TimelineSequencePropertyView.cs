using ImGuiNET;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;
public class TimelineSequencePropertyView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public TimelineSequencePropertyView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Timeline - Sequence Properties##TimelineSequenceProperties");

        var customData = Selection._selectedTimelineCustomData;
        var sequenceKey = Selection._selectedTimelineSequenceKey;
        var sequence = Selection._selectedTimelineSequence;

        if (customData != null && customData.Sequences.Count > 0)
        {
            if (sequenceKey != -1 && sequence != null)
            {
                ImGui.Columns(2);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Value Type");
                ImGui.Text($"Point Type");
                ImGui.Text($"Value Index");

                ImGui.NextColumn();

                // TODO: add editing
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{sequence.ValueType}");
                ImGui.Text($"{sequence.PointType}");
                ImGui.Text($"{sequence.ValueIndex}");

                ImGui.Columns(1);
            }
        }

        ImGui.End();
    }
}

