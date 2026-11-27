using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public class GparamToolView
{
    private GparamEditorView View;
    private ProjectEntry Project;

    public GparamSearchTool LocalSearchTool;
    public GparamSearchTool GlobalSearchTool;
    public GparamMassEditTool MassEditTool;
    public GparamDataTransferTool DataTransferTool;
    public GparamMergeTool MergeTool;
    public GparamReloaderTool ReloaderTool;

    private ToolSections Sections = new();

    public GparamToolView(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        LocalSearchTool = new(view, project, GparamSearchMode.Local);
        GlobalSearchTool = new(view, project, GparamSearchMode.Global);
        MassEditTool = new(view, project);
        DataTransferTool = new(view, project);
        MergeTool = new(view, project);
        ReloaderTool = new(view, project);
    }

    public void Display()
    {
        Sections.Begin();

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        // Local Search
        LocalSearchTool.OnToolWindow();

        // Global Search
        GlobalSearchTool.OnToolWindow();

        // Mass Edit
        MassEditTool.OnToolWindow();

        // Data
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Data"));

        // Data Transfer
        DataTransferTool.OnToolWindow();

        // Merge
        MergeTool.OnToolWindow();

        // Viewport
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));

        // Viewport Preview
        View.ViewportPreviewTool.OnToolWindow();

        // Grid Configuration
        View.GridTool.OnToolWindow();

        // Rendering: sets its own category, so it comes after the Viewport tools
        View.RenderingTool.OnToolWindow();

        // Game Memory
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Game_Memory"));

        // Gparam Reloader
        ReloaderTool.OnToolWindow();

        Sections.End();
    }
}
