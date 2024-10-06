using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editors.EmevdEditor.Framework;
using StudioCore.EmevdEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the action menubar entries for this editor
/// </summary>
public class EmevdActionMenubar
{
    private EmevdEditorScreen Screen;
    private EmevdActionHandler ActionHandler;

    public EmevdActionMenubar(EmevdEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = new EmevdActionHandler(screen);
    }

    public void OnProjectChanged()
    {

    }

    public void Display()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Test"))
            {

            }

            ImGui.EndMenu();
        }
    }
}
