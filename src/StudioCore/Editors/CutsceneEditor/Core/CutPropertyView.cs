using Hexa.NET.ImGui;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class CutPropertyView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public CutPropertyView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Cut Properties##CutProperties");

        var cutKey = Selection._selectedCutKey;
        var cut = Selection._selectedCut;

        if (cutKey != -1 && cut != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Duration");

            ImGui.NextColumn();

            // TODO: add editing
            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{cut.Duration}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }
}


