using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActSelectionWindow
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    private string TimeActListFilter = "";
    private bool ExactTimeActFilter = false;

    public TimeActSelectionWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        GUI.TitleHeader(
            LOC.Get("TAE_SelectionWindow_List_Header"),
            LOC.Get("TAE_SelectionWindow_List_Header_TT"));

        EditorFilters.DisplayFramedListFilter("timeActEditor_SelectionList_Filter",
            ref TimeActListFilter, ref ExactTimeActFilter);

        ImGui.BeginChild($"SelectionListSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplaySelectionList();

        ImGui.EndChild();
    }

    public void DisplaySelectionList()
    {

    }
}
