using ImGuiNET;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DispositionCustomDataPropertyView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public DispositionCustomDataPropertyView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Disposition - Custom Data Properties##DispositionCustomDataProperties");

        var customDataKey = Selection._selectedDispositionCustomDataKey;
        var customData = Selection._selectedDispositionCustomData;

        if (customDataKey != -1 && customData != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Type");
            ImGui.Text($"Value");

            ImGui.NextColumn();

            // TODO: add editing
            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{customData.Type}");
            ImGui.Text($"{customData.Value}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }
}

