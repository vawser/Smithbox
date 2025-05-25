using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
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
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexToolMenubar(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            if (ImGui.MenuItem("Export Texture", KeyBindings.Current.TEXTURE_ExportTexture.HintText))
            {
                Editor.Tools.ExportTextureHandler();
            }
            UIHelper.Tooltip($"Export currently selected texture.");

            ImGui.EndMenu();
        }
    }
}

