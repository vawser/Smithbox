using ImGuiNET;
using StudioCore.EmevdEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor.Tools;

public class ToolSubMenu
{
    private EmevdEditorScreen Screen;

    public ToolSubMenu(EmevdEditorScreen screen)
    {
        Screen = screen;
    }


    public void Shortcuts()
    {

    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Test"))
            {

            }

            ImGui.EndMenu();
        }
    }
}

