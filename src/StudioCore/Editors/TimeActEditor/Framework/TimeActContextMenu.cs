using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Formats.JSON;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActContextMenu
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActContextMenu(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Context menu for the Files list
    /// </summary>
    public void ContainerMenu(bool isSelected, FileDictionaryEntry entry, BinderContents binder)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"ContainerContextMenu##ContainerContextMenu{entry.Filename}"))
        {
            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Context menu for the Time Act list
    /// </summary>
    public void TimeActMenu(bool isSelected, TAE curTae)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActContextMenu##TimeActContextMenu{curTae.ID}"))
        {
            if(ImGui.Selectable($"Duplicate##duplicateAction{curTae.ID}"))
            {
                Editor.ActionHandler.DetermineDuplicateTarget();
            }
            if (ImGui.Selectable($"Delete##deleteAction{curTae.ID}"))
            {
                Editor.ActionHandler.DetermineDeleteTarget();
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Context menu for the Animations list
    /// </summary>
    public void TimeActAnimationMenu(bool isSelected, TAE.Animation curAnimation)
    {
        if (!isSelected)
            return;

        if (ImGui.BeginPopupContextItem($"TimeActAnimationContextMenu##TimeActAnimationContextMenu{curAnimation.ID}"))
        {
            if (ImGui.Selectable($"Duplicate##duplicateAction{curAnimation.ID}"))
            {
                Editor.ActionHandler.DetermineDuplicateTarget();
            }
            if (ImGui.Selectable($"Delete##deleteAction{curAnimation.ID}"))
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
