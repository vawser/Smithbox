using ImGuiNET;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.TimeActSelectionHandler;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActContextMenu
{
    private TimeActEditorScreen Screen;
    private TimeActSelectionHandler Handler;

    public TimeActContextMenu(TimeActEditorScreen screen, TimeActSelectionHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    public void ContainerMenu(bool isSelected, string key)
    {
        if (AnimationBank.IsSaving)
            return;

        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"ContainerContextMenu##ContainerContextMenu{key}"))
        {
            ImGui.EndPopup();
        }
    }

    public void TimeActMenu(bool isSelected, string key)
    {
        if (AnimationBank.IsSaving)
            return;

        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActContextMenu##TimeActContextMenu{key}"))
        {
            if(ImGui.Selectable($"Duplicate##duplicateAction{key}"))
            {
                Screen.SelectionHandler.CurrentSelectionContext = SelectionContext.TimeAct;
                Screen.ActionHandler.DetermineDuplicateTarget();
            }
            if (ImGui.Selectable($"Delete##deleteAction{key}"))
            {
                Screen.SelectionHandler.CurrentSelectionContext = SelectionContext.TimeAct;
                Screen.ActionHandler.DetermineDeleteTarget();
            }

            ImGui.EndPopup();
        }
    }

    public void TimeActAnimationMenu(bool isSelected, string key)
    {
        if (AnimationBank.IsSaving)
            return;

        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActAnimationContextMenu##TimeActAnimationContextMenu{key}"))
        {
            if (ImGui.Selectable($"Duplicate##duplicateAction{key}"))
            {
                Screen.SelectionHandler.CurrentSelectionContext = SelectionContext.Animation;
                Screen.ActionHandler.DetermineDuplicateTarget();
            }
            if (ImGui.Selectable($"Delete##deleteAction{key}"))
            {
                Screen.SelectionHandler.CurrentSelectionContext = SelectionContext.Animation;
                Screen.ActionHandler.DetermineDeleteTarget();
            }

            ImGui.EndPopup();
        }
    }

    public void TimeActEventMenu(bool isSelected, string key)
    {
        if (AnimationBank.IsSaving)
            return;

        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActEventContextMenu##TimeActEventContextMenu{key}"))
        {
            if (ImGui.Selectable($"Create##createAction{key}"))
            {
                Screen.SelectionHandler.CurrentSelectionContext = SelectionContext.Event;
                Screen.ActionHandler.DetermineCreateTarget();
            }
            if (ImGui.Selectable($"Duplicate##duplicateAction{key}"))
            {
                Screen.SelectionHandler.CurrentSelectionContext = SelectionContext.Event;
                Screen.ActionHandler.DetermineDuplicateTarget();
            }
            if (ImGui.Selectable($"Delete##deleteAction{key}"))
            {
                Screen.SelectionHandler.CurrentSelectionContext = SelectionContext.Event;
                Screen.ActionHandler.DetermineDeleteTarget();
            }

            ImGui.EndPopup();
        }
    }

    public void TimeActEventPropertiesMenu(bool isSelected, string key)
    {
        if (AnimationBank.IsSaving)
            return;

        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActEventPropertiesContextMenu##TimeActEventPropertiesContextMenu{key}"))
        {
            ImGui.EndPopup();
        }
    }
}
