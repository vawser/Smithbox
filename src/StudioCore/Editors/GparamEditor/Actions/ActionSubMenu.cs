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
        if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Delete))
        {
            Screen.DeleteValueRow();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.Core_Duplicate))
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
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Duplicate Value Row"))
            {
                Screen.DuplicateValueRow();
            }
            ImguiUtils.ShowHoverTooltip("Duplicates the current value row selection.");

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Delete Value Row"))
            {
                Screen.DeleteValueRow();
            }
            ImguiUtils.ShowHoverTooltip("Deletes the current value row selection.");

            ImGui.EndMenu();
        }
    }
}
