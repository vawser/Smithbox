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

public class TexToolMenubar
{
    private TextureViewerScreen Screen;
    public TexTools Tools;

    public TexToolMenubar(TextureViewerScreen screen)
    {
        Screen = screen;
        Tools = screen.Tools;
    }

    public void Shortcuts()
    {
        
    }

    public void Display()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Export Texture", KeyBindings.Current.TEXTURE_ExportTexture.HintText))
            {
                Tools.ExportTextureHandler();
            }

            ImGui.EndMenu();
        }
    }
}

