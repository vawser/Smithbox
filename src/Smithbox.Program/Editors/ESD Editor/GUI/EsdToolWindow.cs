using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The ESD Editor's Tools window: Palette (see Claude/Documentation/Tool UI Styling.md)
/// </summary>
public class EsdToolWindow
{
    private readonly EsdEditorView View;

    public EsdPaletteTool PaletteTool;

    private readonly ToolSections Sections = new();

    public EsdToolWindow(EsdEditorView view)
    {
        View = view;
        PaletteTool = new(view);
    }

    public void Draw()
    {
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ToolSections.OptionsMenu();

            ImGui.EndMenuBar();
        }

        Sections.Begin();

        // Editing
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Editing"));

        if (CFG.Current.Interface_EsdEditor_Tool_Palette)
        {
            // Palette
            PaletteTool.OnToolWindow();
        }

        Sections.End();
    }

    private void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##esdToolViewMenu"))
        {
            // Palette
            if (ImGui.MenuItem($"{LOC.Get("ESD_ToolWindow_View_Toggle_Palette")}##esdPaletteToolToggle"))
                CFG.Current.Interface_EsdEditor_Tool_Palette = !CFG.Current.Interface_EsdEditor_Tool_Palette;
            GUI.ShowActiveStatus(CFG.Current.Interface_EsdEditor_Tool_Palette);

            ImGui.EndMenu();
        }
    }
}
