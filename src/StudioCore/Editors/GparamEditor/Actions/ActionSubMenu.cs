using StudioCore.GraphicsEditor;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor.Actions;

public class ActionSubMenu
{
    private GparamEditorScreen Screen;
    public ActionHandler Handler;

    public ActionSubMenu(GparamEditorScreen screen)
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
        // None yet
        /*
        if (ImGui.BeginMenu("Actions"))
        {
            
            ImGui.EndMenu();
        }
        */
    }
}
