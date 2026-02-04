using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.TextureViewer;

public class TexToolView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TextureExport TextureExport;

    public TexToolView(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        TextureExport = new TextureExport(editor, Project);
    }

    public void Display()
    {
        if (!CFG.Current.Interface_TextureViewer_ToolWindow)
            return;

        if (ImGui.Begin("Tools##ToolConfigureWindow_TextureViewer", UIHelper.GetMainWindowFlags()))
        {
            FocusManager.SetFocus(EditorFocusContext.TextureViewer_Tools);

            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            // Export Texture
            if (CFG.Current.Interface_TextureViewer_Tool_ExportTexture)
            {
                TextureExport.Display();
            }
        }

        ImGui.End();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Export Texture"))
            {
                CFG.Current.Interface_TextureViewer_Tool_ExportTexture = !CFG.Current.Interface_TextureViewer_Tool_ExportTexture;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Tool_ExportTexture);

            ImGui.EndMenu();
        }
    }

    public void DisplayMenubar()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            if (ImGui.MenuItem("Export Texture", InputManager.GetHint(KeybindID.TextureViewer_Export_Texture)))
            {
                TextureExport.ExportTextureHandler();
            }
            UIHelper.Tooltip($"Export currently selected texture.");

            ImGui.EndMenu();
        }
    }
}
