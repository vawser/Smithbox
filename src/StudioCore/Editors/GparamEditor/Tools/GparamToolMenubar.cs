using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor.Tools;

public class GparamToolMenubar
{
    private GparamEditorScreen Screen;
    public GparamActionHandler Handler;

    public GparamToolMenubar(GparamEditorScreen screen)
    {
        Screen = screen;
        Handler = new GparamActionHandler(screen);
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Color Picker"))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            ImGui.EndMenu();
        }
    }
}

