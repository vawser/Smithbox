using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

public class ActionSubMenu
{
    private TimeActEditorScreen Screen;
    public ActionHandler Handler;

    public ActionSubMenu(TimeActEditorScreen screen, ActionHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            Handler.DetermineDeleteTarget();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            Handler.DetermineDuplicateTarget();
        }
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                Handler.DetermineDuplicateTarget();
            }
            ImguiUtils.ShowHoverTooltip("Duplicates the current selection.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                Handler.DetermineDeleteTarget();
            }
            ImguiUtils.ShowHoverTooltip("Deletes the current selection.");

            ImGui.EndMenu();
        }
    }
}
