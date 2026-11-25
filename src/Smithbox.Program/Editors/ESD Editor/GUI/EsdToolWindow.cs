using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The ESD Editor's Tools window: Search, Called By, Palette and Event Flags (see Claude/Documentation/UI.md (Tools window conventions))
/// </summary>
public class EsdToolWindow
{
    private readonly EsdEditorView View;

    public EsdPaletteTool PaletteTool;
    public EsdSearchTool SearchTool;
    public EsdCalledByTool CalledByTool;
    public EventFlagTool EventFlagTool;

    private readonly ToolSections Sections = new();

    public EsdToolWindow(EsdEditorView view)
    {
        View = view;
        PaletteTool = new(view);
        SearchTool = new(view);
        CalledByTool = new(view);
        EventFlagTool = new(view.Project, "esd",
            new EventFlagSearch("EVENTFLAG_Tool_Find_Esd", flag => SearchTool.SearchValue(flag, true)));
    }

    public void Draw()
    {
        Sections.Begin();

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        // Search
        SearchTool.OnToolWindow();

        // Browse
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));

        // Called By
        CalledByTool.OnToolWindow();

        // Editing
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Editing"));

        // Palette
        PaletteTool.OnToolWindow();

        // Game Memory
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Game_Memory"));

        // Event Flags
        EventFlagTool.OnToolWindow();

        Sections.End();
    }
}
