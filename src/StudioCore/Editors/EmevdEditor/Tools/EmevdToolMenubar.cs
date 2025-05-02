using Hexa.NET.ImGui;
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
/// Handles the tool menubar entries for this editor
/// </summary>
public class EmevdToolMenubar
{
    private EmevdEditorScreen Screen;

    public EmevdToolMenubar(EmevdEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {

    }

    public void Display()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Test"))
            {

            }

            ImGui.EndMenu();
        }
    }
}

