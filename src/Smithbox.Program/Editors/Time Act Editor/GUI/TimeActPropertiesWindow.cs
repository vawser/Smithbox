using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActPropertiesWindow
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    private string PropertiesFilter = "";
    private bool ExactPropertiesFilter = false;

    public TimeActPropertiesWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        GUI.TitleHeader(
            LOC.Get("TAE_PropertiesWindow_List_Header"),
            LOC.Get("TAE_PropertiesWindow_List_Header_TT"));

        EditorFilters.DisplayFramedListFilter("timeActEditor_Properties_Filter",
            ref PropertiesFilter, ref ExactPropertiesFilter);

        ImGui.BeginChild($"PropertiesSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayProperties();

        ImGui.EndChild();
    }

    public void DisplayProperties()
    {

    }
}
