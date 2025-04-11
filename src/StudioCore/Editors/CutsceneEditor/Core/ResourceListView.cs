using ImGuiNET;
using SoulsFormats;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;
public class ResourceListView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public ResourceListView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Resources##ResourceList");

        var cutsceneKey = Selection._selectedCutsceneKey;
        var cutscene = Selection._selectedCutscene;

        var resourceKey = Selection._selectedResourceKey;
        var resource = Selection._selectedResource;

        // Resources
        if (cutsceneKey != -1 && cutscene != null)
        {
            for (int i = 0; i < cutscene.Resources.Count; i++)
            {
                MQB.Resource entry = cutscene.Resources[i];

                if (ImGui.Selectable($@"{entry.Name}##{entry.Name}{i}", i == resourceKey))
                {
                    Selection.ResetCut();
                    Selection.ResetTimeline();
                    Selection.ResetTimelineCustomData();
                    Selection.ResetTimelineSequence();
                    Selection.ResetTimelineSequencePoint();
                    Selection.ResetDisposition();
                    Selection.ResetDispositionSequence();
                    Selection.ResetDispositionCustomData();
                    Selection.ResetDispositionTransform();

                    Selection.SetResource(i, entry);
                }
            }
        }

        ImGui.End();
    }
}

