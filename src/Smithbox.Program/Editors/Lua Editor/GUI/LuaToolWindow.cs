using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The Lua Editor's Tools window, laid out as the other editors' (ToolSections categories and headers). Browse: the
/// Act Outline, the selected script's definitions, and the Global Search, a string or value in every script. Simulation: the Battle Simulation, the selected battle goal run on
/// its character in the viewport, and First Person Mode, walking as its target. Viewport: the Viewport Filters (what the viewport shows of the simulation: effects,
/// bullet lines, the target), and the shared Grid Configuration tool, for the simulation viewport's grids (saved in
/// LuaEditorSettings.LuaEditor_Grids).
/// </summary>
public class LuaToolWindow
{
    private readonly LuaEditorScreen Editor;
    private readonly ToolSections Sections = new();

    public LuaOutline ActOutline { get; }
    public LuaGlobalSearch GlobalSearch { get; }
    public ViewportGridTool GridTool { get; }
    public LuaViewportFiltersTool FiltersTool { get; } = new();
    public LuaFirstPersonTool FirstPersonTool { get; }

    public LuaToolWindow(LuaEditorScreen editor)
    {
        Editor = editor;
        ActOutline = new LuaOutline(editor);
        GlobalSearch = new LuaGlobalSearch(editor);
        FirstPersonTool = new LuaFirstPersonTool(editor);
        GridTool = new ViewportGridTool(ViewportGridEditor.Lua, () => Editor.Viewer.Viewport as VulkanViewport);
    }

    public void Draw()
    {
        Sections.Begin();

        // Browse
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));
        ActOutline.OnToolWindow();
        GlobalSearch.OnToolWindow();

        // Simulation
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Simulation"));
        Editor.BattleView.OnToolWindow();
        FirstPersonTool.OnToolWindow();

        // Viewport
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));
        FiltersTool.OnToolWindow();
        GridTool.OnToolWindow();

        Sections.End();
    }
}
