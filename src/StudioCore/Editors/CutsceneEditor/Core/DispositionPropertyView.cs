using ImGuiNET;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class DispositionPropertyView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public DispositionPropertyView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Disposition Properties##DispositionPropertiesView");

        var dispositionKey = Selection._selectedDispositionKey;
        var disposition = Selection._selectedDisposition;

        if (dispositionKey != -1 && disposition != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"ResourceIndex");
            ImGui.Text($"Unk08");
            ImGui.Text($"StartFrame");
            ImGui.Text($"Duration");
            ImGui.Text($"Unk14");
            ImGui.Text($"Unk18");
            ImGui.Text($"Unk1C");
            ImGui.Text($"Unk20");
            ImGui.Text($"Unk28");

            ImGui.NextColumn();

            // TODO: add editing
            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{disposition.ResourceIndex}");
            ImGui.Text($"{disposition.Unk08}");
            ImGui.Text($"{disposition.StartFrame}");
            ImGui.Text($"{disposition.Duration}");
            ImGui.Text($"{disposition.Unk14}");
            ImGui.Text($"{disposition.Unk18}");
            ImGui.Text($"{disposition.Unk1C}");
            ImGui.Text($"{disposition.Unk20}");
            ImGui.Text($"{disposition.Unk28}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }
}

