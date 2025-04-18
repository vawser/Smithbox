using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class DispositionCustomDataListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public DispositionCustomDataListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Disposition - Custom Data##DispositionCustomDataListView");

        var dispositionKey = Selection._selectedDispositionKey;
        var disposition = Selection._selectedDisposition;

        if (dispositionKey != -1 && disposition != null)
        {
            for (int i = 0; i < disposition.CustomData.Count; i++)
            {
                MQB.CustomData entry = disposition.CustomData[i];

                if (ImGui.Selectable($@"{entry.Name}##{i}", i == Selection._selectedDispositionCustomDataKey))
                {
                    Selection.ResetDispositionSequence();
                    Selection.ResetDispositionSequencePoint();

                    Selection.SetDispositionCustomData(i, entry);
                }
            }
        }

        ImGui.End();
    }
}

