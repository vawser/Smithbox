using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editors.MapEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions;

public class ActionSubMenu
{
    private ModelEditorScreen Screen;
    private ActionHandler Handler;

    public ActionSubMenu(ModelEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }

    public void Shortcuts()
    {
        if(InputTracker.GetKeyDown(KeyBindings.Current.CORE_CreateNewEntry))
        {
            Handler.CreateHandler();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            Handler.DuplicateHandler();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            Handler.DeleteHandler();
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
            if (ImGui.MenuItem("Create", KeyBindings.Current.CORE_CreateNewEntry.HintText))
            {
                Handler.CreateHandler();
            }
            ImguiUtils.ShowHoverTooltip("Adds new entry based on current selection in Model Hierarchy.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                Handler.DuplicateHandler();
            }
            ImguiUtils.ShowHoverTooltip("Duplicates current selection in Model Hierarchy.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Delete", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                Handler.DeleteHandler();
            }
            ImguiUtils.ShowHoverTooltip("Deletes current selection in Model Hierarchy.");

            ImGui.EndMenu();
        }
    }

}
