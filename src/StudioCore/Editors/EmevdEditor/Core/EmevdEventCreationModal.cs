using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.EmevdEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the EMEVD.Event creation menu
/// </summary>
public class EmevdEventCreationModal
{
    private EmevdEditorScreen Screen;
    private EmevdPropertyDecorator Decorator;
    private EmevdSelectionManager Selection;

    public bool ShowModal = false;

    public EmevdEventCreationModal(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    public void Display()
    {
        if(ShowModal)
        {
            ImGui.OpenPopup("Create EMEVD Event");
        }

        EventCreationMenu();
    }

    public void EventCreationMenu()
    {
        if (ImGui.BeginPopupModal("Create EMEVD Event", ref ShowModal, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
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