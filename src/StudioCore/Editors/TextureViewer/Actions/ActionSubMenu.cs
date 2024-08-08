using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Actions;


public class ActionSubMenu
{
    private TextureViewerScreen Screen;
    public ActionHandler Handler;

    public ActionSubMenu(TextureViewerScreen screen)
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
