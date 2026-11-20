using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The Lua Editor's Tools window, laid out as the other editors' (ToolSections categories and headers). Browse: the
/// Act Outline, the selected script's definitions. Viewport: the shared Grid Configuration tool, for the AI
/// simulation viewport's grids (saved in CFG.LuaEditor_Grids).
/// </summary>
public class LuaToolWindow
{
    private readonly LuaEditorScreen Editor;
    private readonly ToolSections Sections = new();

    public LuaOutline ActOutline { get; }
    public ViewportGridTool GridTool { get; }

    public LuaToolWindow(LuaEditorScreen editor)
    {
        Editor = editor;
        ActOutline = new LuaOutline(editor);
        GridTool = new ViewportGridTool(ViewportGridEditor.Lua, () => Editor.Viewer.Viewport as VulkanViewport);
    }

    public void Draw()
    {
        Sections.Begin();

        // Browse
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));
        ActOutline.OnToolWindow();

        // Viewport
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));
        GridTool.OnToolWindow();

        Sections.End();
    }
}
