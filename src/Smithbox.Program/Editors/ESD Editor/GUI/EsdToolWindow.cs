using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The ESD Editor's Tools window: Search, Called By and Palette (see Claude/Documentation/Tool UI Styling.md)
/// </summary>
public class EsdToolWindow
{
    private readonly EsdEditorView View;

    public EsdPaletteTool PaletteTool;
    public EsdSearchTool SearchTool;
    public EsdCalledByTool CalledByTool;

    private readonly ToolSections Sections = new();

    public EsdToolWindow(EsdEditorView view)
    {
        View = view;
        PaletteTool = new(view);
        SearchTool = new(view);
        CalledByTool = new(view);
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

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        if (CFG.Current.Interface_EsdEditor_Tool_Search)
        {
            // Search
            SearchTool.OnToolWindow();
        }

        // Browse
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));

        if (CFG.Current.Interface_EsdEditor_Tool_CalledBy)
        {
            // Called By
            CalledByTool.OnToolWindow();
        }

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
            // Search
            if (ImGui.MenuItem($"{LOC.Get("ESD_ToolWindow_View_Toggle_Search")}##esdSearchToolToggle"))
                CFG.Current.Interface_EsdEditor_Tool_Search = !CFG.Current.Interface_EsdEditor_Tool_Search;
            GUI.ShowActiveStatus(CFG.Current.Interface_EsdEditor_Tool_Search);

            // Called By
            if (ImGui.MenuItem($"{LOC.Get("ESD_ToolWindow_View_Toggle_Called_By")}##esdCalledByToolToggle"))
                CFG.Current.Interface_EsdEditor_Tool_CalledBy = !CFG.Current.Interface_EsdEditor_Tool_CalledBy;
            GUI.ShowActiveStatus(CFG.Current.Interface_EsdEditor_Tool_CalledBy);

            // Palette
            if (ImGui.MenuItem($"{LOC.Get("ESD_ToolWindow_View_Toggle_Palette")}##esdPaletteToolToggle"))
                CFG.Current.Interface_EsdEditor_Tool_Palette = !CFG.Current.Interface_EsdEditor_Tool_Palette;
            GUI.ShowActiveStatus(CFG.Current.Interface_EsdEditor_Tool_Palette);

            ImGui.EndMenu();
        }
    }
}
