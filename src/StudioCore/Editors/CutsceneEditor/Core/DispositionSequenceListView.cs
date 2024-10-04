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
public class DispositionSequenceListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public DispositionSequenceListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Disposition - Sequences##DispositionSequencesProperties");

        var customDataKey = Selection._selectedDispositionCustomDataKey;
        var customData = Selection._selectedDispositionCustomData;

        var sequenceKey = Selection._selectedDispositionSequenceKey;
        var sequence = Selection._selectedDispositionSequence;

        if (customDataKey != -1 && customData != null)
        {
            for (int i = 0; i < customData.Sequences.Count; i++)
            {
                MQB.CustomData.Sequence entry = customData.Sequences[i];

                if (ImGui.Selectable($@"ID {i}##{i}", i == sequenceKey))
                {
                    Selection.ResetDispositionSequencePoint();

                    Selection.SetDispositionSequence(i, entry);
                }
            }
        }

        ImGui.End();
    }
}

