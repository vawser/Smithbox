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
