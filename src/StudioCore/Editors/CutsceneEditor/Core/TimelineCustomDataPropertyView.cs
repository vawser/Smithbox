using ImGuiNET;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;
public class TimelineCustomDataPropertyView
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;
    private CutscenePropertyDecorator Decorator;

    public TimelineCustomDataPropertyView(CutsceneEditorScreen screen)
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
        ImGui.Begin("Timeline - Custom Data Properties##TimelineCustomDataProperties");

        var customDataKey = Selection._selectedTimelineCustomDataKey;
        var customData = Selection._selectedTimelineCustomData;

        if (customDataKey != -1 && customData != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Type");
            ImGui.Text($"Unk44");
            ImGui.Text($"Value");

            ImGui.NextColumn();

            // TODO: add editing
            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{customData.Type}");
            ImGui.Text($"{customData.Unk44}");
            ImGui.Text($"{customData.Value}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }
}

