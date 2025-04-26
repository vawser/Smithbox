using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;
using StudioCore.CutsceneEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class CutsceneFileView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public CutsceneFileView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Files##CutsceneFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in CutsceneBank.FileBank)
        {
            if (ImGui.Selectable($@"{info.Name}", info.Name == Selection._selectedBinderKey))
            {
                Selection.ResetCutscene();
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

                Selection.SetFile(info, binder);
            }
        }

        ImGui.End();
    }
}
