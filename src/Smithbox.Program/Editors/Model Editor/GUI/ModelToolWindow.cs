using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class ModelToolWindow
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public ModelDataTransferTool DataTransferTool;

    private ToolSections Sections = new();

    public ModelToolWindow(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        DataTransferTool = new(view, project);
    }

    public void DisplayDropdown()
    {
        // Tools
        //if (ImGui.BeginMenu($"{LOC.Get("MODEL_ToolWindow_Tools_Header")}##toolsMenuHeader"))
        //{
        //    DataTransferTool.DisplayDropdown();

        //    ImGui.EndMenu();
        //}
    }

    public void Display()
    {
        Sections.Begin();

        //********************************
        // General
        //********************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_General"));

        View.HistoryTool.OnToolWindow();

        //********************************
        // Authoring
        //********************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Authoring"));

        View.DummyTool.OnToolWindow();

        if (View.GrassBuilderTool.CanUse())
        {
            View.GrassBuilderTool.OnToolWindow();
        }

        if (View.EdgeEditorTool.CanUse())
        {
            View.EdgeEditorTool.OnToolWindow();
        }

        View.CollisionTool.OnToolWindow();

        //********************************
        // Preview
        //********************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Preview"));

        View.ModelDisplayRotationTool.OnToolWindow();

        View.ModelAnimationTool.OnToolWindow();

        if (ToolSections.Header($"{LOC.Get("MODEL_AssetBrowser_Header")}##modelAssetBrowserHeader"))
        {
            View.AssetBrowserTool.Display();
        }

        View.ModelCharacterPreview.OnToolWindow();

        View.ModelMaskToggler.OnToolWindow();

        //********************************
        // Search & Edit
        //********************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        View.LocalSearchTool.OnToolWindow();

        View.GlobalSearchTool.OnToolWindow();

        View.MassEditTool.OnToolWindow();

        View.ModelInstanceFinder.OnToolWindow();

        //********************************
        // Data
        //********************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Data"));

        DataTransferTool.OnToolWindow();

        View.ModelExportTool.OnToolWindow();

        //********************************
        // Validation
        //********************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));

        View.VanillaCompareTool.OnToolWindow();

        // Render Snapshots: a developer tool, for checking renderer changes against a baseline
        if (CFG.Current.Developer.Developer_Enable_Tools)
        {
            View.RenderSnapshotTool.OnToolWindow();
        }

        //********************************
        // Viewport
        //********************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));

        View.GridTool.OnToolWindow();

        View.ResourceListTool.Display("modelEditor", View.Universe);

        //********************************
        // Rendering
        //********************************
        View.RenderingTool.OnToolWindow();

        Sections.End();
    }
}


