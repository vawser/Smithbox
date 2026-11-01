using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
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

    // Rebuilt only when the filter or the time acts change, as the aliases are matched as well
    private List<TimeActEntry> CachedFilteredTimeActs;
    private List<TimeActEntry> CachedTimeActs;
    private int CachedTimeActCount;
    private string CachedFilter = "";
    private bool CachedExactFilter = false;

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

        var filteredTimeActs = GetFilteredTimeActs(source);

        if (InputManager.HasArrowSelection())
        {
            _arrowKeyPressed = true;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredTimeActs.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var entry = filteredTimeActs[i];

                var selected = View.Selection.SelectedTimeAct == entry;

                var alias = GetTimeActAlias(source, entry);

                if (ImGui.Selectable($"{entry.Name}##timeActEntry{entry.BinderID}", selected))
                {
                    SelectTimeAct(entry);
                }

                // The animation count isn't known until the time act has been read
                if (entry.TimeAct != null)
                {
                    GUI.Tooltip(LOC.Get("TAE_SelectionWindow_Entry_Details", entry.Name, entry.TimeAct.Animations.Count));
                }

                if (_arrowKeyPressed && ImGui.IsItemFocused() && !selected)
                {
                    SelectTimeAct(entry);

                    _arrowKeyPressed = false;
                }

                if (alias != "")
                {
                    GUI.DisplayAlias(alias, CFG.Current.Interface_Alias_Wordwrap_Model_Editor);
                }
            }
        }

        clipper.End();
    }

    private List<TimeActEntry> GetFilteredTimeActs(TimeActContainerWrapper source)
    {
        if (CachedFilteredTimeActs != null &&
            CachedTimeActs == source.TimeActs &&
            CachedTimeActCount == source.TimeActs.Count &&
            CachedFilter == TimeActListFilter &&
            CachedExactFilter == ExactTimeActFilter)
        {
            return CachedFilteredTimeActs;
        }

        CachedTimeActs = source.TimeActs;
        CachedTimeActCount = source.TimeActs.Count;
        CachedFilter = TimeActListFilter;
        CachedExactFilter = ExactTimeActFilter;

        CachedFilteredTimeActs = source.TimeActs
            .Where(e => EditorFilters.IsMatch(TimeActListFilter, e.Name, ExactTimeActFilter,
                GetTimeActAlias(source, e), true, true))
            .ToList();

        return CachedFilteredTimeActs;
    }

    /// <summary>
    /// Time act aliases are stored per character: a000 of c0000 is stored as c0000_000.
    /// </summary>
    private string GetTimeActAlias(TimeActContainerWrapper source, TimeActEntry entry)
    {
        return Project.GetAliasName(ProjectAliasType.TimeActs, $"{source.Name}_{entry.Name.Substring(1)}");
    }

    private void SelectTimeAct(TimeActEntry entry)
    {
        entry.Load();

        View.Selection.SelectedTimeAct = entry;
    }
}
