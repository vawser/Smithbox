using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

public class TimeActActionMenubar
{
    private TimeActEditorScreen Screen;
    public TimeActActionHandler ActionHandler;

    public TimeActActionMenubar(TimeActEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            if (ImGui.Button("Duplicate", UI.MenuButtonSize))
            {
                ActionHandler.DetermineDuplicateTarget();
            }
            UIHelper.ShowHoverTooltip($"Duplicates the current selection.\n{KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText}");

            if (ImGui.Button("Delete", UI.MenuButtonSize))
            {
                ActionHandler.DetermineDeleteTarget();
            }
            UIHelper.ShowHoverTooltip($"Deletes the current selection.\n{KeyBindings.Current.CORE_DeleteSelectedEntry.HintText}");

            ImGui.EndMenu();
        }
    }
}
