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

        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_TextureViewer);
        if (ImGui.Begin($"{LOC.Get("TEXVIEW_Window_Tools")}###ToolConfigureWindow_TextureViewer", UIHelper.GetMainWindowFlags()))
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
        if (ImGui.BeginMenu($"{LOC.Get("TEXVIEW_Tools_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_Tools_View_Export_Texture")}##exportTextureToggle"))
            {
                CFG.Current.Interface_TextureViewer_Tool_ExportTexture = !CFG.Current.Interface_TextureViewer_Tool_ExportTexture;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Tool_ExportTexture);

            ImGui.EndMenu();
        }
    }

    public void DisplayMenubar()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXVIEW_Tools_Header_Tools")}##toolsMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_Tools_Action_Export_Texture")}##exportTextureAction", InputManager.GetHint(KeybindID.TextureViewer_Export_Texture)))
            {
                TextureExport.ExportTextureHandler();
            }
            UIHelper.Tooltip(LOC.Get("TEXVIEW_Tools_Action_Export_Texture_TT"));

            ImGui.EndMenu();
        }
    }
}
