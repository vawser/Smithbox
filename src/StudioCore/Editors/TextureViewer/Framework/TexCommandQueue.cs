using CommunityToolkit.HighPerformance.Buffers;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TextureViewer.TextureFolderBank;

namespace StudioCore.Editors.TextureViewer;

public class TexCommandQueue
{
    private TextureViewerScreen Screen;
    private TexViewSelection Selection;

    public TexCommandQueue(TextureViewerScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
    }

    /// <summary>
    /// Parse the editor command queue for this editor.
    /// </summary>
    public void Parse(string[] initcmd)
    {
        if (initcmd != null && initcmd.Length > 1)
        {
            // View Image:
            // e.g. "texture/view/01_common/SB_GarageTop_04"
            if (initcmd[0] == "view" && initcmd.Length >= 3)
            {
                LoadTextureContainer(initcmd[1]);
                LoadTextureFile(initcmd[2]);
            }
        }
    }

    /// <summary>
    /// Load a texture container by the passed container key
    /// </summary>
    public void LoadTextureContainer(string container)
    {
        if (Selection._selectedTextureContainerKey != container)
        {
            foreach (var (name, info) in TextureFolderBank.FolderBank)
            {
                if (name == container)
                {
                    Selection._selectedTextureContainerKey = name;
                    Selection.SelectTextureContainer(info);
                }
            }
        }
    }

    /// <summary>
    /// Load a texture by the passed file key
    /// </summary>
    public void LoadTextureFile(string filename)
    {
        if (Selection._selectedTextureContainerKey != filename)
        {
            if (Selection._selectedTextureContainer != null && Selection._selectedTextureContainerKey != "")
            {
                TextureViewInfo data = Selection._selectedTextureContainer;

                if (data.Textures != null)
                {
                    foreach (var tex in data.Textures)
                    {
                        if (tex.Name == filename)
                        {
                            Selection._selectedTextureKey = tex.Name;
                            Selection._selectedTexture = tex;
                        }
                    }
                }
            }
        }
    }
}
