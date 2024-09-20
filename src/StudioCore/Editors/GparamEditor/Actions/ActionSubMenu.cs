using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor.Actions;

public class ActionSubMenu
{
    private GparamEditorScreen Screen;
    public ActionHandler Handler;

    public ActionSubMenu(GparamEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            Screen.DeleteValueRow();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            Screen.DuplicateValueRow();
        }
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Duplicate Value Row", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                Screen.DuplicateValueRow();
            }
            UIHelper.ShowHoverTooltip("Duplicates the current value row selection.");

            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Delete Value Row", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                Screen.DeleteValueRow();
            }
            UIHelper.ShowHoverTooltip("Deletes the current value row selection.");

            ImGui.EndMenu();
        }
    }
}
