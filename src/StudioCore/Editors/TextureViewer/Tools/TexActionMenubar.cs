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

namespace StudioCore.Editors.TextureViewer.Tools;

public class TexActionMenubar
{
    private TextureViewerScreen Screen;
    public TexTools Handler;

    public TexActionMenubar(TextureViewerScreen screen)
    {
        Screen = screen;
        Handler = new TexTools(screen);
    }

    public void DisplayMenu()
    {
        /*
        if (ImGui.BeginMenu("Actions"))
        {

            ImGui.EndMenu();
        }
        */
    }
}
