using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Editors.TimeActEditor.Actions;

namespace StudioCore.Editors.TimeActEditor.Tools;

public class ToolSubMenu
{
    private TimeActEditorScreen Screen;
    public ActionHandler Handler;

    public ToolSubMenu(TimeActEditorScreen screen, ActionHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    public void Shortcuts()
    {

    }

    public void OnProjectChanged()
    {

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
        */
    }
}
