﻿using Hexa.NET.ImGui;
using StudioCore.Editors.TimeActEditor.Bank;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActContextMenu
{
    private TimeActEditorScreen Editor;
    private TimeActSelectionManager Handler;

    public TimeActContextMenu(TimeActEditorScreen screen, TimeActSelectionManager handler)
    {
        Editor = screen;
        Handler = handler;
    }

    /// <summary>
    /// Context menu for the Files list
    /// </summary>
    public void ContainerMenu(bool isSelected, string key)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"ContainerContextMenu##ContainerContextMenu{key}"))
        {
            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Context menu for the Time Act list
    /// </summary>
    public void TimeActMenu(bool isSelected, string key)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActContextMenu##TimeActContextMenu{key}"))
        {
            if(ImGui.Selectable($"Duplicate##duplicateAction{key}"))
            {
                Editor.ActionHandler.DetermineDuplicateTarget();
            }
            if (ImGui.Selectable($"Delete##deleteAction{key}"))
            {
                Editor.ActionHandler.DetermineDeleteTarget();
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Context menu for the Animations list
    /// </summary>
    public void TimeActAnimationMenu(bool isSelected, string key)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActAnimationContextMenu##TimeActAnimationContextMenu{key}"))
        {
            if (ImGui.Selectable($"Duplicate##duplicateAction{key}"))
            {
                Editor.ActionHandler.DetermineDuplicateTarget();
            }
            if (ImGui.Selectable($"Delete##deleteAction{key}"))
            {
                Editor.ActionHandler.DetermineDeleteTarget();
            }

            ImGui.EndPopup();
        }
    }
    /// <summary>
    /// Context menu for the Events list
    /// </summary>
    public void TimeActEventMenu(bool isSelected, string key)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActEventContextMenu##TimeActEventContextMenu{key}"))
        {
            if (ImGui.Selectable($"Create##createAction{key}"))
            {
                Editor.ActionHandler.DetermineCreateTarget();
            }
            if (ImGui.Selectable($"Duplicate##duplicateAction{key}"))
            {
                Editor.ActionHandler.DetermineDuplicateTarget();
            }
            if (ImGui.Selectable($"Delete##deleteAction{key}"))
            {
                Editor.ActionHandler.DetermineDeleteTarget();
            }

            ImGui.EndPopup();
        }
    }
    /// <summary>
    /// Context menu for the Event Properties list
    /// </summary>
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
