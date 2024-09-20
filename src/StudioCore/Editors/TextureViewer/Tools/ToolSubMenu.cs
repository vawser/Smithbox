using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editors.TextEditor.Tools;
using StudioCore.Editors.TextureViewer.Actions;
using StudioCore.Interface;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Tools;

public class ToolSubMenu
{
    private TextureViewerScreen Screen;
    public ActionHandler Handler;

    public ToolSubMenu(TextureViewerScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.TEXTURE_ExportTexture))
        {
            Handler.ExportTextureHandler();
        }
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Export Texture", KeyBindings.Current.TEXTURE_ExportTexture.HintText))
            {
                Handler.ExportTextureHandler();
            }

            ImGui.EndMenu();
        }
    }
}

