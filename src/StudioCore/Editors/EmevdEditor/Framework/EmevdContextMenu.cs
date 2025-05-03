using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.EmevdEditor;
using static StudioCore.Editors.EmevdEditor.EmevdBank;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the context menus used by the view classes.
/// </summary>
public class EmevdContextMenu
{
    private EmevdEditorScreen Screen;
    private EmevdPropertyDecorator Decorator;
    private EmevdSelectionManager Selection;

    public EmevdContextMenu(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    /// <summary>
    /// Context menu for the selection in the File list
    /// </summary>
    public void FileContextMenu(EventScriptInfo info)
    {
        if (ImGui.BeginPopupContextItem($"FileContext##FileContext{info.Name}"))
        {

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Context menu for the selection in the Event list
    /// </summary>
    public void EventContextMenu(EMEVD.Event evt)
    {
        if (ImGui.BeginPopupContextItem($"EventContext##EventContext{evt.ID}"))
        {
            if (ImGui.Selectable($"Create##createActionEvent{evt.ID}"))
            {
                Screen.EventCreationModal.ShowModal = true;
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Context menu for the selection in the Instruction list
    /// </summary>
    public void InstructionContextMenu(EMEVD.Instruction ins)
    {
        if (ImGui.BeginPopupContextItem($"InstructionContext##InstructionContext{ins.ID}"))
        {
            if (ImGui.Selectable($"Create##createActionInstruction{ins.ID}"))
            {
                Screen.InstructionCreationModal.ShowModal = true;
            }

            ImGui.EndPopup();
        }
    }
}
