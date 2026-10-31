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

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_Animation)
        {
            View.ModelAnimationTool.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_AssetBrowser)
        {
            if (ImGui.CollapsingHeader($"{LOC.Get("MODEL_AssetBrowser_Header")}##modelAssetBrowserHeader"))
            {
                View.AssetBrowserTool.Display();
            }
        }

        if (CFG.Current.Interface_ModelEditor_Tool_CharacterPreview)
        {
            View.ModelCharacterPreview.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelMaskToggler)
        {
            View.ModelMaskToggler.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelExport)
        {
            View.ModelExportTool.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration)
        {
            View.ModelGridTool.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ModelInstanceFinder)
        {
            View.ModelInstanceFinder.OnToolWindow();
        }

        if (CFG.Current.Interface_ModelEditor_Tool_ResourceMonitor)
        {
            View.ResourceListTool.Display("modelEditor", View.Universe);
        }
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("MODEL_ToolWindow_View_Header")}##viewMenuHeader"))
        {
            // Grid Configuration
            if (ImGui.MenuItem($"{LOC.Get("MODEL_ToolWindow_View_Grid_Configuration")}##toggleGridConfigTool"))
            {
                CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration = !CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_ModelGridConfiguration);

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


