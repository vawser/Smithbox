using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StudioCore.Editors.TimeActEditor.Actions;

namespace StudioCore.Editors.TimeActEditor.Tools;

public class TimeActToolMenubar
{
    private TimeActEditorScreen Screen;
    private TimeActActionHandler ActionHandler;

    public TimeActToolMenubar(TimeActEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    public void DisplayMenu()
    {
        /*
        if (ImGui.BeginMenu("Tools"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");

            if (ImGui.MenuItem("TODO", KeyBindings.Current.TextureViewer_ExportTexture.HintText))
            {

            }

            ImGui.EndMenu();
            }
        }
        */
    }
}
