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

/// <summary>
/// Handles the action menubar entries for this editor
/// </summary>
public class EmevdActionMenubar
{
    private EmevdEditorScreen Screen;
    private EmevdTools Handler;

    public EmevdActionMenubar(EmevdEditorScreen screen)
    {
        Screen = screen;
        Handler = new EmevdTools(screen);
    }

    public void OnProjectChanged()
    {

    }

    public void Display()
    {
        if (ImGui.BeginMenu("Actions"))
        {

            ImGui.EndMenu();
        }
    }
}
