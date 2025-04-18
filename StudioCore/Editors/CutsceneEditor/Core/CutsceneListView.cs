using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class CutsceneListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public CutsceneListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Cutscenes##CutsceneList");

        var selectedFile = Selection._selectedFileInfo;

        var cutsceneKey = Selection._selectedCutsceneKey;
        var cutscene = Selection._selectedCutscene;

        if (selectedFile != null)
        {
            for (int i = 0; i < selectedFile.CutsceneFiles.Count; i++)
            {
                MQB entry = selectedFile.CutsceneFiles[i];

                if (ImGui.Selectable($@"{entry.Name}##{entry.Name}{i}", i == cutsceneKey))
                {
                    Selection.ResetCut();
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

                    Selection.SetCutscene(i, entry);
                }
            }
        }

        ImGui.End();
    }
}

