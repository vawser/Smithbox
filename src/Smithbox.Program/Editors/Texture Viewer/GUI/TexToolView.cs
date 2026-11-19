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

    public ScaleformConfigurationTool ScaleformConfiguration;

    public ScaleformReloaderTool ScaleformReloader;

    public ItemIconInjectorTool ItemIconInjector;

    private ToolSections Sections = new();

    public TexToolView(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        TextureExport = new TextureExport(view, Project);
        ScaleformConfiguration = new ScaleformConfigurationTool(view, Project);
        ScaleformReloader = new ScaleformReloaderTool(view, Project);
        ItemIconInjector = new ItemIconInjectorTool(view, Project);
    }

    public void Display()
    {
        Sections.Begin();

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Editing"));

        // Import Texture
        View.TextureImport.OnToolWindow();

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Data_Transfer"));

        // Export Texture
        TextureExport.Display();

        // GFX: the GFx mode's tools, for projects with menu movies
        if (ScaleformConfiguration.IsAvailable)
        {
            ToolSections.Category(LOC.Get("TEXVIEW_ToolCategory_GFX"));

            // Scaleform Configuration (preview substitutes for placeholder images)
            ScaleformConfiguration.OnToolWindow();

            // Scaleform Reloader (reload saved movies in the running game), for games with its offsets
            ScaleformReloader.OnToolWindow();

            // Item Icon Injector (new icon into the atlas and every movie's icon page), for movies with icon pages
            ItemIconInjector.OnToolWindow();
        }

        Sections.End();
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
