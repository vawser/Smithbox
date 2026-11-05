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

    private ToolSections Sections = new();

    public GparamToolView(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        LocalSearchTool = new(view, project, GparamSearchMode.Local);
        GlobalSearchTool = new(view, project, GparamSearchMode.Global);
        MassEditTool = new(view, project);
        DataTransferTool = new(view, project);
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

        if (CFG.Current.Interface_GparamEditor_Tool_ViewportPreview)
        {
            // Viewport Preview
            View.ViewportPreviewTool.OnToolWindow();
        }

        if (CFG.Current.Interface_GparamEditor_Tool_ViewportRendering)
        {
            // Viewport Rendering
            View.RenderingTool.OnToolWindow();
        }

        if (CFG.Current.Interface_GparamEditor_Tool_GridConfiguration)
        {
            // Grid Configuration
            View.GridTool.OnToolWindow();
        }

        if (CFG.Current.Interface_GparamEditor_Tool_DataTransfer)
        {
            // Data Transfer
            DataTransferTool.OnToolWindow();
        }

        if (CFG.Current.Interface_GparamEditor_Tool_LocalSearch)
        {
            // Local Search
            LocalSearchTool.OnToolWindow();
        }

        if (CFG.Current.Interface_GparamEditor_Tool_GlobalSearch)
        {
            // Global Search
            GlobalSearchTool.OnToolWindow();
        }

        if (CFG.Current.Interface_GparamEditor_Tool_MassEdit)
        {
            // Mass Edit
            MassEditTool.OnToolWindow();
        }

        Sections.End();
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("GPARAM_Tools_View_Header")}##viewHeader"))
        {
            // Data Transfer
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_Tools_View_Data_Transfer")}##dataTransferToggle"))
            {
                CFG.Current.Interface_GparamEditor_Tool_DataTransfer = !CFG.Current.Interface_GparamEditor_Tool_DataTransfer;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_DataTransfer);

            // Local Search
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_Tools_View_Local_Search")}##localSearchToggle"))
            {
                CFG.Current.Interface_GparamEditor_Tool_LocalSearch = !CFG.Current.Interface_GparamEditor_Tool_LocalSearch;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_LocalSearch);

            // Global Search
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_Tools_View_Global_Search")}##globalSearchToggle"))
            {
                CFG.Current.Interface_GparamEditor_Tool_GlobalSearch = !CFG.Current.Interface_GparamEditor_Tool_GlobalSearch;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_GlobalSearch);

            // Mass Edit
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_Tools_View_Mass_Edit")}##massEditToggle"))
            {
                CFG.Current.Interface_GparamEditor_Tool_MassEdit = !CFG.Current.Interface_GparamEditor_Tool_MassEdit;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_MassEdit);

            ImGui.Separator();

            // Viewport Preview
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_ToolWindow_View_Toggle_ViewportPreview")}##viewportPreviewToggle"))
            {
                CFG.Current.Interface_GparamEditor_Tool_ViewportPreview = !CFG.Current.Interface_GparamEditor_Tool_ViewportPreview;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_ViewportPreview);

            // Viewport Rendering
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Rendering_Tool_Header")}##viewportRenderingToggle"))
            {
                CFG.Current.Interface_GparamEditor_Tool_ViewportRendering = !CFG.Current.Interface_GparamEditor_Tool_ViewportRendering;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_ViewportRendering);

            // Grid Configuration
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_GridConfig_Header")}##gridConfigurationToggle"))
            {
                CFG.Current.Interface_GparamEditor_Tool_GridConfiguration = !CFG.Current.Interface_GparamEditor_Tool_GridConfiguration;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Tool_GridConfiguration);

            ImGui.EndMenu();
        }
    }
}
