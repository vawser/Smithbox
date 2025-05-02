using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class TimelineListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public TimelineListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Timelines##TimelineList");

        var cutKey = Selection._selectedCutKey;
        var cut = Selection._selectedCut;

        var timelineKey = Selection._selectedTimelineKey;
        var timeline = Selection._selectedTimeline;

        if (cutKey != -1 && cut != null)
        {
            // Timelines
            for (int i = 0; i < cut.Timelines.Count; i++)
            {
                MQB.Timeline entry = cut.Timelines[i];

                if (ImGui.Selectable($@"ID {i}##{i}", i == timelineKey))
                {
                    Selection.ResetDisposition();
                    Selection.ResetDispositionSequence();
                    Selection.ResetDispositionCustomData();
                    Selection.ResetDispositionTransform();

                    Selection.SetTimeline(i, entry);
                }
            }
        }

        ImGui.End();
    }
}


