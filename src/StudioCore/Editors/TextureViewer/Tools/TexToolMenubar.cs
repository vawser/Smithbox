using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.TextureViewer;

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
            if (ImGui.MenuItem("Export Texture", KeyBindings.Current.TEXTURE_ExportTexture.HintText))
            {
                Tools.ExportTextureHandler();
            }
            UIHelper.ShowHoverTooltip($"Export currently selected texture.");

            ImGui.EndMenu();
        }
    }
}

