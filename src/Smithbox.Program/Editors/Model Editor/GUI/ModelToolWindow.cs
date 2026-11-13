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
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ToolSections.OptionsMenu();

            ImGui.EndMenuBar();
        }

        Sections.Begin();

        // Actions
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Actions"));

        if (CFG.Current.Interface_ModelEditor_Tool_History)
        {
            View.HistoryTool.OnToolWindow();
        }

        // Assets
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Assets"));

        if (CFG.Current.Interface_ModelEditor_Tool_AssetBrowser)
        {
            if (ToolSections.Header($"{LOC.Get("MODEL_AssetBrowser_Header")}##modelAssetBrowserHeader"))
            {
                View.AssetBrowserTool.Display();
            }
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder)
        {
            View.ModelInstanceFinder.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelExport)
        {
            View.ModelExportTool.OnToolWindow();
        }

        // Preview
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Preview"));

        if (CFG.Current.Interface_ModelEditor_Tool_Animation)
        {
            View.ModelAnimationTool.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_CharacterPreview)
        {
            View.ModelCharacterPreview.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler)
        {
            View.ModelMaskToggler.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_DisplayRotation)
        {
            View.ModelDisplayRotationTool.OnToolWindow();
        }

        // Viewport
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));

        if (CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration)
        {
            View.GridTool.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ResourceMonitor)
        {
            View.ResourceListTool.Display("modelEditor", View.Universe);
        }

        // Rendering: sets its own category, so it comes last
        if (CFG.Current.Interface_ModelEditor_Tool_ViewportRendering)
        {
            View.RenderingTool.OnToolWindow();
        }

        Sections.End();
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("MODEL_ToolWindow_View_Header")}##viewMenuHeader"))
        {
            // History
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Toggle_History")}##toggleHistoryTool"))
            {
                CFG.Current.Interface_ModelEditor_Tool_History = !CFG.Current.Interface_ModelEditor_Tool_History;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_History);

            // Grid Configuration
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_GridConfig_Header")}##toggleGridConfigTool"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration = !CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration);

            // Viewport Rendering
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Rendering_Tool_Header")}##toggleViewportRenderingTool"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ViewportRendering = !CFG.Current.Interface_ModelEditor_Tool_ViewportRendering;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ViewportRendering);

            // Model Instance Finder
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Instance_Finder")}##toggleInstanceFinder"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder = !CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder);

            // Model Mask Toggler
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Mask_Toggler")}##modelMaskToggler"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler = !CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler);

            // Animation
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Animation")}##modelAnimationToggle"))
            {
                CFG.Current.Interface_ModelEditor_Tool_Animation = !CFG.Current.Interface_ModelEditor_Tool_Animation;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_Animation);

            // Character Preview
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Character_Preview")}##modelCharacterPreviewToggle"))
            {
                CFG.Current.Interface_ModelEditor_Tool_CharacterPreview = !CFG.Current.Interface_ModelEditor_Tool_CharacterPreview;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_CharacterPreview);

            // Display Rotation
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Toggle_Display_Rotation")}##modelDisplayRotationToggle"))
            {
                CFG.Current.Interface_ModelEditor_Tool_DisplayRotation = !CFG.Current.Interface_ModelEditor_Tool_DisplayRotation;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_DisplayRotation);

            // Mdoel Export
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Model_Export")}##modelExportToggle"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelExport = !CFG.Current.Interface_ModelEditor_Tool_ModelExport;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelExport);

            // Asset Browser
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Asset_Browser")}##assetBrowserToggle"))
            {
                CFG.Current.Interface_ModelEditor_Tool_AssetBrowser = !CFG.Current.Interface_ModelEditor_Tool_AssetBrowser;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_AssetBrowser);

            // Resource Monitor
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Resource_Monitor")}##resourceMonitorToggle"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ResourceMonitor = !CFG.Current.Interface_ModelEditor_Tool_ResourceMonitor;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ResourceMonitor);

            ImGui.EndMenu();
        }
    }
}


