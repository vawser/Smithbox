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
public class TimelineSequenceListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public TimelineSequenceListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Timeline - Sequences##TimelineSequencesProperties");

        var customDataKey = Selection._selectedTimelineCustomDataKey;
        var customData = Selection._selectedTimelineCustomData;

        var sequenceKey = Selection._selectedTimelineSequenceKey;
        var sequence = Selection._selectedTimelineSequence;

        if (customDataKey != -1 && customData != null)
        {
            for (int i = 0; i < customData.Sequences.Count; i++)
            {
                MQB.CustomData.Sequence entry = customData.Sequences[i];

                if (ImGui.Selectable($@"ID {i}##{i}", i == sequenceKey))
                {
                    Selection.ResetTimelineSequencePoint();

                    Selection.SetTimelineSequence(i, entry);
                }
            }
        }

        ImGui.End();
    }
}

