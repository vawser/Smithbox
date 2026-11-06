using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActToolWindow
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    private ToolSections Sections = new();

    public TimeActToolWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
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


        if (CFG.Current.Interface_TimeActEditor_Tool_LocalSearch)
        {
            View.LocalSearchTool.OnToolWindow();
        }

        if (CFG.Current.Interface_TimeActEditor_Tool_GlobalSearch)
        {
            View.GlobalSearchTool.OnToolWindow();
        }

        if (CFG.Current.Interface_TimeActEditor_Tool_MassEdit)
        {
            View.MassEditTool.OnToolWindow();
        }

        // Animation

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Animation"));


        if (CFG.Current.Interface_TimeActEditor_Tool_ComboBuilder)
        {
            View.ComboBuilderTool.OnToolWindow();
        }

        // Preview

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Preview"));


        if (CFG.Current.Interface_TimeActEditor_Tool_CharacterPreview)
        {
            View.CharacterPreview.OnToolWindow();
        }

        // Player characters are dressed by the Character Preview, as their masks come from what they wear
        if (CFG.Current.Interface_TimeActEditor_Tool_ModelMaskToggler &&
            !CharacterComposition.IsComposite(View.Selection.SelectedTimeActModelWrapper?.Name))
        {
            View.MaskToggler.OnToolWindow();
        }

        // Viewport

        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));


        if (CFG.Current.Interface_TimeActEditor_Tool_GridConfiguration)
        {
            View.GridTool.OnToolWindow();
        }

        if (CFG.Current.Interface_TimeActEditor_Tool_ViewportRendering)
        {
            View.RenderingTool.OnToolWindow();
        }

        if (CFG.Current.Interface_TimeActEditor_Tool_ResourceMonitor)
        {
            View.ResourceListTool.Display("timeActEditor", View.Universe);
        }

        Sections.End();
    }

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("TAE_ToolWindow_View_Header")}##viewMenuHeader"))
        {
            // Local Search
            if (ImGui.MenuItem($"{LOC.Get("TAE_ToolWindow_View_Toggle_Local_Search")}##localSearchToolToggle"))
            {
                CFG.Current.Interface_TimeActEditor_Tool_LocalSearch = !CFG.Current.Interface_TimeActEditor_Tool_LocalSearch;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Tool_LocalSearch);

            // Global Search
            if (ImGui.MenuItem($"{LOC.Get("TAE_ToolWindow_View_Toggle_Global_Search")}##globalSearchToolToggle"))
            {
                CFG.Current.Interface_TimeActEditor_Tool_GlobalSearch = !CFG.Current.Interface_TimeActEditor_Tool_GlobalSearch;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Tool_GlobalSearch);

            // Mass Edit
            if (ImGui.MenuItem($"{LOC.Get("TAE_ToolWindow_View_Toggle_Mass_Edit")}##massEditToolToggle"))
            {
                CFG.Current.Interface_TimeActEditor_Tool_MassEdit = !CFG.Current.Interface_TimeActEditor_Tool_MassEdit;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Tool_MassEdit);

            // Combo Builder
            if (ImGui.MenuItem($"{LOC.Get("TAE_ToolWindow_View_Toggle_Combo_Builder")}##comboBuilderToolToggle"))
            {
                CFG.Current.Interface_TimeActEditor_Tool_ComboBuilder = !CFG.Current.Interface_TimeActEditor_Tool_ComboBuilder;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Tool_ComboBuilder);

            // Character Preview
            if (ImGui.MenuItem($"{LOC.Get("TAE_ToolWindow_View_Toggle_Character_Preview")}##characterPreviewToggle"))
            {
                CFG.Current.Interface_TimeActEditor_Tool_CharacterPreview = !CFG.Current.Interface_TimeActEditor_Tool_CharacterPreview;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Tool_CharacterPreview);

            // Model Mask Toggler
            if (ImGui.MenuItem($"{LOC.Get("TAE_ToolWindow_View_Toggle_Mask_Toggler")}##maskTogglerToggle"))
            {
                CFG.Current.Interface_TimeActEditor_Tool_ModelMaskToggler = !CFG.Current.Interface_TimeActEditor_Tool_ModelMaskToggler;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Tool_ModelMaskToggler);

            // Grid Configuration
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_GridConfig_Header")}##toggleGridConfigurationTool"))
            {
                CFG.Current.Interface_TimeActEditor_Tool_GridConfiguration = !CFG.Current.Interface_TimeActEditor_Tool_GridConfiguration;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Tool_GridConfiguration);

            // Viewport Rendering
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Rendering_Tool_Header")}##toggleViewportRenderingTool"))
            {
                CFG.Current.Interface_TimeActEditor_Tool_ViewportRendering = !CFG.Current.Interface_TimeActEditor_Tool_ViewportRendering;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Tool_ViewportRendering);

            // Resource Monitor
            if (ImGui.MenuItem($"{LOC.Get("TAE_ToolWindow_View_Toggle_Resource_Monitor")}##resourceMonitorToggle"))
            {
                CFG.Current.Interface_TimeActEditor_Tool_ResourceMonitor = !CFG.Current.Interface_TimeActEditor_Tool_ResourceMonitor;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Tool_ResourceMonitor);

            ImGui.EndMenu();
        }
    }
}
