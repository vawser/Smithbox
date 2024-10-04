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
    public TimeActTools Tools;

    public TimeActActionMenubar(TimeActEditorScreen screen)
    {
        Screen = screen;
        Tools = screen.Tools;
    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                Tools.DetermineDuplicateTarget();
            }
            UIHelper.ShowHoverTooltip("Duplicates the current selection.");

            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                Tools.DetermineDeleteTarget();
            }
            UIHelper.ShowHoverTooltip("Deletes the current selection.");

            ImGui.EndMenu();
        }
    }
}
