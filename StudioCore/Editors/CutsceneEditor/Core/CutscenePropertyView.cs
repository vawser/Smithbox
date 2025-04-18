using Hexa.NET.ImGui;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class CutscenePropertyView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public CutscenePropertyView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Cutscene Properties##CutsceneProperties");

        var cutsceneKey = Selection._selectedCutsceneKey;
        var cutscene = Selection._selectedCutscene;

        if (cutsceneKey != -1 && cutscene != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Framerate");
            ImGui.Text($"Resource Directory");

            ImGui.NextColumn();

            // TODO: add editing
            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{cutscene.Framerate}");
            ImGui.Text($"{cutscene.ResourceDirectory}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }
}


