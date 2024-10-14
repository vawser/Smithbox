using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextActionMenubar
{
    private TextEditorScreen Screen;
    private TextActionHandler ActionHandler;

    public TextActionMenubar(TextEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    public void Display()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            ///--------------------
            // Create
            ///--------------------
            if (ImGui.Button("Create", UI.MenuButtonWideSize))
            {
                Screen.EntryCreationModal.ShowModal = true;
            }
            UIHelper.ShowHoverTooltip($"Create new text entries.\n{KeyBindings.Current.CORE_CreateNewEntry.HintText}");

            ///--------------------
            // Duplicate
            ///--------------------
            if (ImGui.Button("Duplicate", UI.MenuButtonWideSize))
            {
                Screen.ActionHandler.DuplicateEntries();
            }
            UIHelper.ShowHoverTooltip($"Duplicate the currently selected text entries\n{KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText}");

            ///--------------------
            // Delete
            ///--------------------
            if (ImGui.Button("Delete", UI.MenuButtonWideSize))
            {
                Screen.ActionHandler.DeleteEntries();
            }
            UIHelper.ShowHoverTooltip($"Delete the currently selected text entries.\n{KeyBindings.Current.CORE_DeleteSelectedEntry.HintText}");


            ImGui.EndMenu();
        }
    }
}
