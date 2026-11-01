using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
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

    private bool _arrowKeyPressed = false;

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
        var source = View.Selection.SelectedTimeActContainerWrapper;

        if (source == null)
            return;

        if (InputManager.HasArrowSelection())
        {
            _arrowKeyPressed = true;
        }

        foreach (var entry in source.TimeActs)
        {
            if (!EditorFilters.IsMatch(TimeActListFilter, entry.Name, ExactTimeActFilter))
                continue;

            var selected = View.Selection.SelectedTimeAct == entry;

            var label = entry.Name;

            // The animation count isn't known until the time act has been read
            if (entry.TimeAct != null)
            {
                label = LOC.Get("TAE_SelectionWindow_Entry_Details", entry.Name, entry.TimeAct.Animations.Count);
            }

            if (ImGui.Selectable($"{label}##timeActEntry{entry.BinderID}", selected))
            {
                SelectTimeAct(entry);
            }

            if (_arrowKeyPressed && ImGui.IsItemFocused() && !selected)
            {
                SelectTimeAct(entry);

                _arrowKeyPressed = false;
            }
        }
    }

    private void SelectTimeAct(TimeActEntry entry)
    {
        entry.Load();

        View.Selection.SelectedTimeAct = entry;
    }
}
