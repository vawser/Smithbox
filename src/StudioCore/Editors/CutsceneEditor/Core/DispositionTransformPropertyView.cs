using Hexa.NET.ImGui;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;
public class DispositionTransformPropertyView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public DispositionTransformPropertyView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Disposition - Transform Properties##DispositionTransformProperties");

        var transformKey = Selection._selectedDispositionTransformKey;
        var transform = Selection._selectedDispositionTransform;

        if (transformKey != -1 && transform != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Frame");
            ImGui.Text($"Translation");
            ImGui.Text($"Unk10");
            ImGui.Text($"Unk1C");
            ImGui.Text($"Rotation");
            ImGui.Text($"Unk34");
            ImGui.Text($"Unk40");
            ImGui.Text($"Scale");
            ImGui.Text($"Unk58");
            ImGui.Text($"Unk64");

            ImGui.NextColumn();

            // TODO: add editing
            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{transform.Frame}");
            ImGui.Text($"{transform.Translation}");
            ImGui.Text($"{transform.Unk10}");
            ImGui.Text($"{transform.Unk1C}");
            ImGui.Text($"{transform.Rotation}");
            ImGui.Text($"{transform.Unk34}");
            ImGui.Text($"{transform.Unk40}");
            ImGui.Text($"{transform.Scale}");
            ImGui.Text($"{transform.Unk58}");
            ImGui.Text($"{transform.Unk64}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }
}

