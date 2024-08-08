using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editors.ModelEditor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Actions;

public class ActionSubMenu
{
    private TextEditorScreen Screen;
    public ActionHandler Handler;

    public ActionSubMenu(TextEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            Handler.DuplicateHandler();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            Handler.DeleteHandler();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.TEXT_SyncDescriptions))
        {
            Handler.SyncDescriptionHandler();
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
            if (ImGui.MenuItem("Duplicate Entries", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                Handler.DuplicateHandler();
            }
            ImguiUtils.ShowHoverTooltip("Duplicates current selection.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Delete Entries", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                Handler.DeleteHandler();
            }
            ImguiUtils.ShowHoverTooltip("Deletes current selection.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Sync Description Entries", KeyBindings.Current.TEXT_SyncDescriptions.HintText))
            {
                Handler.SyncDescriptionHandler();
            }
            ImguiUtils.ShowHoverTooltip("Sync the description of all selected entries to the description of the first member of the selection.");


            ImGui.EndMenu();
        }
    }
}
