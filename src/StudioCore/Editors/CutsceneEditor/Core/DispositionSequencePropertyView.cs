using Hexa.NET.ImGui;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;
public class DispositionSequencePropertyView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public DispositionSequencePropertyView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Disposition - Sequence Properties##DispositionSequenceProperties");

        var customData = Selection._selectedDispositionCustomData;

        var sequenceKey = Selection._selectedDispositionSequenceKey;
        var sequence = Selection._selectedDispositionSequence;

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
