using ImGuiNET;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.TimeActSelectionManager;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActContextMenu
{
    private TimeActEditorScreen Screen;
    private TimeActSelectionManager Handler;

    public TimeActContextMenu(TimeActEditorScreen screen, TimeActSelectionManager handler)
    {
        Screen = screen;
        Handler = handler;
    }

    /// <summary>
    /// Context menu for the Files list
    /// </summary>
    public void ContainerMenu(bool isSelected, string key)
    {
        if (TimeActBank.IsSaving)
            return;

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
        if (TimeActBank.IsSaving)
            return;

        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActContextMenu##TimeActContextMenu{key}"))
        {
            if(ImGui.Selectable($"Duplicate##duplicateAction{key}"))
            {
                Screen.Selection.CurrentSelectionContext = SelectionContext.TimeAct;
                Screen.Tools.DetermineDuplicateTarget();
            }
            if (ImGui.Selectable($"Delete##deleteAction{key}"))
            {
                Screen.Selection.CurrentSelectionContext = SelectionContext.TimeAct;
                Screen.Tools.DetermineDeleteTarget();
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Context menu for the Animations list
    /// </summary>
    public void TimeActAnimationMenu(bool isSelected, string key)
    {
        if (TimeActBank.IsSaving)
            return;

        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActAnimationContextMenu##TimeActAnimationContextMenu{key}"))
        {
            if (ImGui.Selectable($"Duplicate##duplicateAction{key}"))
            {
                Screen.Selection.CurrentSelectionContext = SelectionContext.Animation;
                Screen.Tools.DetermineDuplicateTarget();
            }
            if (ImGui.Selectable($"Delete##deleteAction{key}"))
            {
                Screen.Selection.CurrentSelectionContext = SelectionContext.Animation;
                Screen.Tools.DetermineDeleteTarget();
            }

            ImGui.EndPopup();
        }
    }
    /// <summary>
    /// Context menu for the Events list
    /// </summary>
    public void TimeActEventMenu(bool isSelected, string key)
    {
        if (TimeActBank.IsSaving)
            return;

        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActEventContextMenu##TimeActEventContextMenu{key}"))
        {
            if (ImGui.Selectable($"Create##createAction{key}"))
            {
                Screen.Selection.CurrentSelectionContext = SelectionContext.Event;
                Screen.Tools.DetermineCreateTarget();
            }
            if (ImGui.Selectable($"Duplicate##duplicateAction{key}"))
            {
                Screen.Selection.CurrentSelectionContext = SelectionContext.Event;
                Screen.Tools.DetermineDuplicateTarget();
            }
            if (ImGui.Selectable($"Delete##deleteAction{key}"))
            {
                Screen.Selection.CurrentSelectionContext = SelectionContext.Event;
                Screen.Tools.DetermineDeleteTarget();
            }

            ImGui.EndPopup();
        }
    }
    /// <summary>
    /// Context menu for the Event Properties list
    /// </summary>
    public void TimeActEventPropertiesMenu(bool isSelected, string key)
    {
        if (TimeActBank.IsSaving)
            return;

        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActEventPropertiesContextMenu##TimeActEventPropertiesContextMenu{key}"))
        {
            ImGui.EndPopup();
        }
    }
}
