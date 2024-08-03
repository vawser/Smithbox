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

    public void ContainerMenu(bool isSelected, string key)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"ContainerContextMenu##ContainerContextMenu{key}"))
        {
            ImGui.EndPopup();
        }
    }

    public void TimeActMenu(bool isSelected, string key)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActContextMenu##TimeActContextMenu{key}"))
        {
            ImGui.EndPopup();
        }
    }

    private bool InAnimationPropertyMode = false;

    public void TimeActAnimationMenu(bool isSelected, string key)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActAnimationContextMenu##TimeActAnimationContextMenu{key}"))
        {
            ImGui.EndPopup();
        }
    }

    public void TimeActEventMenu(bool isSelected, string key)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActEventContextMenu##TimeActEventContextMenu{key}"))
        {
            ImGui.EndPopup();
        }
    }

    public void TimeActEventPropertiesMenu(bool isSelected, string key)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActEventPropertiesContextMenu##TimeActEventPropertiesContextMenu{key}"))
        {
            ImGui.EndPopup();
        }
    }
}
