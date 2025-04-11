using ImGuiNET;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class InternalFilePropertyView
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelContextMenu ContextMenu;
    private ModelPropertyDecorator Decorator;

    public InternalFilePropertyView(ModelEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        Decorator = screen.Decorator;
    }

    public void Display()
    {
        var buttonSize = new Vector2(20, 20);
        var container = Screen.ResManager.LoadedFlverContainer;
        var name = container.InternalFlvers.First().Name;

        ImGui.Separator();
        ImGui.Text($"Filename: {container.FlverFileName}");
        ImGui.Separator();

        ImGui.Text($"Internal Files:");
        foreach (var entry in container.InternalFlvers)
        {
            ImGui.AlignTextToFramePadding();
            if (ImGui.Button($"{ForkAwesome.Bars}", buttonSize))
            {
                UIHelper.CopyToClipboard(entry.Name);
            }
            UIHelper.ShowHoverTooltip("Copy name to clipboard.");
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{entry.Name}");
        }

        ImGui.Separator();
    }
}
