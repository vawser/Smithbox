using Hexa.NET.ImGui;
using StudioCore.EmevdEditor;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the EMEVD.Instruction creation menu
/// </summary>
public class EmevdInstructionCreationModal
{
    private EmevdEditorScreen Screen;
    private EmevdPropertyDecorator Decorator;
    private EmevdSelectionManager Selection;

    public bool ShowModal = false;

    public EmevdInstructionCreationModal(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    public void Display()
    {
        if (ShowModal)
        {
            ImGui.OpenPopup("Create EMEVD Event Instruction");
        }

        EventCreationMenu();
    }

    public void EventCreationMenu()
    {
        if (ImGui.BeginPopupModal("Create EMEVD Event Instruction", ref ShowModal, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
        {
            Vector2 listboxSize = new Vector2(520, 400);
            Vector2 buttonSize = new Vector2(520 * 0.5f, 24);



            if (ImGui.Button("Create", buttonSize))
            {
                ShowModal = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Close", buttonSize))
            {
                ShowModal = false;
            }

            ImGui.EndPopup();
        }
    }
}
