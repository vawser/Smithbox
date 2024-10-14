using ImGuiNET;
using StudioCore.Interface;
using StudioCore.TalkEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the action menubar entries for this editor
/// </summary>
public class EsdActionMenubar
{
    private EsdEditorScreen Screen;

    public EsdActionMenubar(EsdEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {

    }

    public void Display()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.Button("Test", UI.MenuButtonSize))
            {

            }

            ImGui.EndMenu();
        }
    }
}

