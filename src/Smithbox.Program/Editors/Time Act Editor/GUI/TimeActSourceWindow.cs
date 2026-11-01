using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActSourceWindow
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    private string SourceListFilter = "";
    private bool ExactSourceListFilter = false;

    public TimeActSourceWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        GUI.TitleHeader(
            LOC.Get("TAE_SourceWindow_List_Header"),
            LOC.Get("TAE_SourceWindow_List_Header_TT"));

        EditorFilters.DisplayFramedListFilter("timeActEditor_SourceList_Filter",
            ref SourceListFilter, ref ExactSourceListFilter);

        ImGui.BeginChild($"SourceListSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplaySourceList();

        ImGui.EndChild();
    }

    public void DisplaySourceList()
    {

    }
}
