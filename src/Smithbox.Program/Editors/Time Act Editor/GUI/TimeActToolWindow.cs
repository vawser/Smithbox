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
