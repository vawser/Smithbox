using Hexa.NET.ImGui;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class InternalFilePropertyView
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelContextMenu ContextMenu;
    private ModelPropertyDecorator Decorator;

    public InternalFilePropertyView(ModelEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        Decorator = screen.Decorator;
    }

    public void Display()
    {
        var buttonSize = new Vector2(20, 20);
        var container = Editor.ResManager.LoadedFlverContainer;
        var name = container.InternalFlvers.First().Name;

        ImGui.Separator();
        ImGui.Text($"Filename: {container.FlverFileName}");
        ImGui.Separator();

        ImGui.Text($"Internal Files:");
        foreach (var entry in container.InternalFlvers)
        {
            ImGui.AlignTextToFramePadding();
            if (ImGui.Button($"{Icons.Bars}", buttonSize))
            {
                UIHelper.CopyToClipboard(entry.Name);
            }
            UIHelper.Tooltip("Copy name to clipboard.");
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{entry.Name}");
        }

        ImGui.Separator();
    }
}
