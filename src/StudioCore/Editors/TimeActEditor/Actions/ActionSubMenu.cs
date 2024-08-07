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
        if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Delete))
        {
            Handler.DetermineDeleteTarget();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Duplicate))
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
            if (ImGui.MenuItem("Duplicate"))
            {
                Handler.DetermineDuplicateTarget();
            }
            ImguiUtils.ShowHoverTooltip("Duplicates the current selection.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Delete"))
            {
                Handler.DetermineDeleteTarget();
            }
            ImguiUtils.ShowHoverTooltip("Deletes the current selection.");

            ImGui.EndMenu();
        }
    }
}
