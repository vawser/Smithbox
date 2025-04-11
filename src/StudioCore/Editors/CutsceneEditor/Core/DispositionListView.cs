using ImGuiNET;
using SoulsFormats;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class DispositionListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public DispositionListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Dispositions##DispositionList");

        var timelineKey = Selection._selectedTimelineKey;
        var timeline = Selection._selectedTimeline;

        var dispositionKey = Selection._selectedDispositionKey;
        var disposition = Selection._selectedDisposition;

        if (timelineKey != -1 && timeline != null)
        {
            for (int i = 0; i < timeline.Dispositions.Count; i++)
            {
                MQB.Disposition entry = timeline.Dispositions[i];

                if (ImGui.Selectable($@"ID {entry.ID}##{entry.ID}{i}", i == dispositionKey))
                {
                    Selection.ResetTimelineCustomData();
                    Selection.ResetTimelineSequence();
                    Selection.ResetTimelineSequencePoint();

                    Selection.SetDisposition(i, entry);
                }
            }
        }

        ImGui.End();
    }
}

