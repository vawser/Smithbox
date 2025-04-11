using ImGuiNET;
using SoulsFormats;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;
public class TimelineSequencePointListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public TimelineSequencePointListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Timeline - Sequence - Points##TimelineSequencePointsList");

        var customData = Selection._selectedTimelineCustomData;
        var sequenceKey = Selection._selectedTimelineSequenceKey;
        var sequence = Selection._selectedTimelineSequence;
        var pointKey = Selection._selectedTimelineSequencePointKey;
        var point = Selection._selectedTimelineSequencePoint;

        if (customData != null && customData.Sequences.Count > 0)
        {
            if (sequenceKey != -1 && sequence != null)
            {
                for (int i = 0; i < sequence.Points.Count; i++)
                {
                    MQB.CustomData.Sequence.Point entry = sequence.Points[i];

                    if (ImGui.Selectable($@"ID {i}##{i}", i == pointKey))
                    {
                        Selection.SetTimelineSequencePoint(i, entry);
                    }
                }
            }
        }

        ImGui.End();
    }
}

