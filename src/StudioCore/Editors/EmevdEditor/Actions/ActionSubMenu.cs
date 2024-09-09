using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.EmevdEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor.Actions;

public class ActionSubMenu
{
    private EmevdEditorScreen Screen;
    private ActionHandler Handler;

    public ActionSubMenu(EmevdEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }
    public void Shortcuts()
    {

    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Actions"))
        {

            ImGui.EndMenu();
        }
    }
}
