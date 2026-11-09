using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.FxEditor;

/// <summary>The FX Editor's Tools window: the viewport preview, plus the shared grid and rendering tools.</summary>
public class FxToolWindow
{
    private readonly FxEditorScreen Editor;

    private readonly ToolSections Sections = new();

    public FxToolWindow(FxEditorScreen editor)
    {
        Editor = editor;
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

        // Preview
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Preview"));

        if (CFG.Current.Interface_FxEditor_Tool_Preview)
            Editor.PreviewTool.OnToolWindow();

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        if (CFG.Current.Interface_FxEditor_Tool_Search)
            Editor.SearchTool.OnToolWindow();

        if (CFG.Current.Interface_FxEditor_Tool_References)
            Editor.ReferencesTool.OnToolWindow();

        // Assets
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Assets"));

        if (CFG.Current.Interface_FxEditor_Tool_Resources)
            Editor.ResourcesTool.OnToolWindow();

        // Viewport
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));

        if (CFG.Current.Interface_FxEditor_Tool_GridConfiguration)
            Editor.GridTool.OnToolWindow();

        // Rendering: sets its own category, so it comes last
        if (CFG.Current.Interface_FxEditor_Tool_ViewportRendering)
            Editor.RenderingTool.OnToolWindow();

        Sections.End();
    }

    private void ViewMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("FX_ToolWindow_View_Header")}##fxToolViewMenu"))
            return;

        // Preview
        if (ImGui.MenuItem($"{LOC.Get("FX_ToolWindow_View_Toggle_Preview")}##fxPreviewToolToggle"))
            CFG.Current.Interface_FxEditor_Tool_Preview = !CFG.Current.Interface_FxEditor_Tool_Preview;
        GUI.ShowActiveStatus(CFG.Current.Interface_FxEditor_Tool_Preview);

        // Search
        if (ImGui.MenuItem($"{LOC.Get("FX_ToolWindow_View_Toggle_Search")}##fxSearchToolToggle"))
            CFG.Current.Interface_FxEditor_Tool_Search = !CFG.Current.Interface_FxEditor_Tool_Search;
        GUI.ShowActiveStatus(CFG.Current.Interface_FxEditor_Tool_Search);

        // References
        if (ImGui.MenuItem($"{LOC.Get("FX_ToolWindow_View_Toggle_References")}##fxReferencesToolToggle"))
            CFG.Current.Interface_FxEditor_Tool_References = !CFG.Current.Interface_FxEditor_Tool_References;
        GUI.ShowActiveStatus(CFG.Current.Interface_FxEditor_Tool_References);

        // Resources
        if (ImGui.MenuItem($"{LOC.Get("FX_ToolWindow_View_Toggle_Resources")}##fxResourcesToolToggle"))
            CFG.Current.Interface_FxEditor_Tool_Resources = !CFG.Current.Interface_FxEditor_Tool_Resources;
        GUI.ShowActiveStatus(CFG.Current.Interface_FxEditor_Tool_Resources);

        // Grid Configuration
        if (ImGui.MenuItem($"{LOC.Get("FX_ToolWindow_View_Toggle_Grid")}##fxGridToolToggle"))
            CFG.Current.Interface_FxEditor_Tool_GridConfiguration = !CFG.Current.Interface_FxEditor_Tool_GridConfiguration;
        GUI.ShowActiveStatus(CFG.Current.Interface_FxEditor_Tool_GridConfiguration);

        // Viewport Rendering
        if (ImGui.MenuItem($"{LOC.Get("FX_ToolWindow_View_Toggle_Rendering")}##fxRenderingToolToggle"))
            CFG.Current.Interface_FxEditor_Tool_ViewportRendering = !CFG.Current.Interface_FxEditor_Tool_ViewportRendering;
        GUI.ShowActiveStatus(CFG.Current.Interface_FxEditor_Tool_ViewportRendering);

        ImGui.EndMenu();
    }
}
