using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.TextureViewer;

public class TexToolView
{
    public TexEditorView View;
    public ProjectEntry Project;

    public TextureExport TextureExport;

    private ToolSections Sections = new();

    public TexToolView(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        TextureExport = new TextureExport(view, Project);
    }

    public void Display()
    {
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ToolSections.OptionsMenu();

            ImGui.EndMenuBar();
        }

        Sections.Begin();

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Editing"));

        // Import Texture
        if (CFG.Current.Interface_TextureViewer_Tool_ImportTexture)
        {
            View.TextureImport.OnToolWindow();
        }

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Data_Transfer"));

        // Export Texture
        if (CFG.Current.Interface_TextureViewer_Tool_ExportTexture)
        {
            TextureExport.Display();
        }

        Sections.End();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXVIEW_Tools_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_Tools_View_Export_Texture")}##exportTextureToggle"))
            {
                CFG.Current.Interface_TextureViewer_Tool_ExportTexture = !CFG.Current.Interface_TextureViewer_Tool_ExportTexture;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Tool_ExportTexture);

            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_ToolWindow_View_Toggle_Import_Texture")}##importTextureToggle"))
            {
                CFG.Current.Interface_TextureViewer_Tool_ImportTexture = !CFG.Current.Interface_TextureViewer_Tool_ImportTexture;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Tool_ImportTexture);

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
            GUI.Tooltip(LOC.Get("TEXVIEW_Tools_Action_Export_Texture_TT"));

            ImGui.EndMenu();
        }
    }
}
