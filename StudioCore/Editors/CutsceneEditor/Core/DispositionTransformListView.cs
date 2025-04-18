using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;
public class DispositionTransformListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public DispositionTransformListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Disposition - Transforms##DispositionTransformListView");

        var dispositionKey = Selection._selectedDispositionKey;
        var disposition = Selection._selectedDisposition;

        var transformKey = Selection._selectedDispositionTransformKey;
        var transform = Selection._selectedDispositionTransform;

        if (dispositionKey != -1 && disposition != null)
        {
            for (int i = 0; i < disposition.Transforms.Count; i++)
            {
                MQB.Transform entry = disposition.Transforms[i];

                if (ImGui.Selectable($@"ID {i}##{i}", i == transformKey))
                {
                    Selection.ResetDispositionSequence();
                    Selection.ResetDispositionSequencePoint();

                    Selection.SetDispositionTransform(i, entry);
                }
            }
        }

        ImGui.End();
    }
}

