using ImGuiNET;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

public class ContextMenu
{
    private TimeActEditorScreen Screen;
    private TimeActSelectionHandler Handler;

    public ContextMenu(TimeActEditorScreen screen, TimeActSelectionHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    public void ContainerMenu(bool isSelected)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"ContainerContextMenu"))
        {
            ImGui.EndPopup();
        }
    }

    public void TimeActMenu(bool isSelected)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActContextMenu"))
        {
            ImGui.EndPopup();
        }
    }

    public void TimeActAnimationMenu(bool isSelected)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActAnimationContextMenu"))
        {
            ImGui.EndPopup();
        }
    }

    public void TimeActEventMenu(bool isSelected)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActEventContextMenu"))
        {
            ImGui.EndPopup();
        }
    }

    public void TimeActEventPropertiesMenu(bool isSelected)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActEventPropertiesContextMenu"))
        {
            ImGui.EndPopup();
        }
    }
}
