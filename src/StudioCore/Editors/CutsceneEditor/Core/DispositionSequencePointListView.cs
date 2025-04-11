using ImGuiNET;
using SoulsFormats;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;
public class DispositionSequencePointListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public DispositionSequencePointListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Disposition - Sequence - Points##DispositionSequencePointsList");

        var customDataKey = Selection._selectedDispositionSequenceKey;
        var customData = Selection._selectedDispositionCustomData;
        var sequenceKey = Selection._selectedDispositionSequenceKey;
        var sequence = Selection._selectedDispositionSequence;
        var pointKey = Selection._selectedDispositionSequencePointKey;
        var point = Selection._selectedDispositionSequencePoint;

        if (customData != null && customData.Sequences.Count > 0)
        {
            if (customDataKey != -1 && sequence != null)
            {
                for (int i = 0; i < sequence.Points.Count; i++)
                {
                    MQB.CustomData.Sequence.Point entry = sequence.Points[i];

                    if (ImGui.Selectable($@"ID {i}##{i}", i == pointKey))
                    {
                        Selection.SetDispositionSequencePoint(i, entry);
                    }
                }
            }
        }

        ImGui.End();
    }
}

