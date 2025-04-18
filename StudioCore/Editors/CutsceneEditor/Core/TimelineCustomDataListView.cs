using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class TimelineCustomDataListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public TimelineCustomDataListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Timeline - Custom Data##TimelineCustomDataList");

        var timelineKey = Selection._selectedTimelineKey;
        var timeline = Selection._selectedTimeline;

        var customDataKey = Selection._selectedTimelineCustomDataKey;
        var customData = Selection._selectedTimelineCustomData;

        // Custom Data
        if (timelineKey != -1 && timeline != null)
        {
            for (int i = 0; i < timeline.CustomData.Count; i++)
            {
                MQB.CustomData entry = timeline.CustomData[i];

                if (ImGui.Selectable($@"{entry.Name}##{entry.Name}{i}", i == customDataKey))
                {
                    Selection.ResetDisposition();
                    Selection.ResetDispositionSequence();
                    Selection.ResetDispositionCustomData();
                    Selection.ResetDispositionTransform();

                    Selection.SetTimelineCustomData(i, entry);
                }
            }
        }

        ImGui.End();
    }
}

