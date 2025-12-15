using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;

namespace StudioCore.Editors.TextureViewer;

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

