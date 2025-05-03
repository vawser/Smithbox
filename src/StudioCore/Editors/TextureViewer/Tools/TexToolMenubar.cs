using Hexa.NET.ImGui;
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
    private TextureViewerScreen Editor;
    public TexTools Tools;

    public TexToolMenubar(TextureViewerScreen screen)
    {
        Editor = screen;
        Tools = screen.Tools;
    }

    public void Shortcuts()
    {
        
    }

    public void Display()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            if (ImGui.MenuItem("Export Texture", KeyBindings.Current.TEXTURE_ExportTexture.HintText))
            {
                Tools.ExportTextureHandler();
            }
            UIHelper.Tooltip($"Export currently selected texture.");

            ImGui.EndMenu();
        }
    }
}

