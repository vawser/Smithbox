using ImGuiNET;
using SoulsFormats;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class CutListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public CutListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Cuts##CutList");

        var cutsceneKey = Selection._selectedCutsceneKey;
        var cutscene = Selection._selectedCutscene;

        var cutKey = Selection._selectedCutKey;

        // Cuts
        if (cutsceneKey != -1 && cutscene != null)
        {
            for (int i = 0; i < cutscene.Cuts.Count; i++)
            {
                MQB.Cut entry = cutscene.Cuts[i];

                if (ImGui.Selectable($@"{entry.Name}##{entry.Name}{i}", i == cutKey))
                {
                    Selection.ResetResource();
                    Selection.ResetDisposition();
                    Selection.ResetDispositionCustomData();
                    Selection.ResetDispositionTransform();
                    Selection.ResetDispositionSequence();
                    Selection.ResetDispositionSequencePoint();
                    Selection.ResetTimeline();
                    Selection.ResetTimelineCustomData();
                    Selection.ResetTimelineSequence();
                    Selection.ResetTimelineSequencePoint();

                    Selection.SetCut(i, entry);
                }
            }
        }

        ImGui.End();
    }
}

