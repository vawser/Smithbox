using Hexa.NET.ImGui;
using SoulsFormats;
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
    private const float TimeActListShare = 0.4f;

    public TimeActEditorView View;
    public ProjectEntry Project;

    private string TimeActListFilter = "";
    private bool ExactTimeActFilter = false;

    private string AnimationListFilter = "";
    private bool ExactAnimationFilter = false;

    private bool _arrowKeyPressed = false;

    // Rebuilt only when the filter or the time acts change, as the aliases are matched as well
    private List<TimeActEntry> CachedFilteredTimeActs;
    private List<TimeActEntry> CachedTimeActs;
    private int CachedTimeActCount;
    private string CachedFilter = "";
    private bool CachedExactFilter = false;

    // A time act can have over a thousand animations, with a name to build for each
    private List<(TAE.Animation Animation, string Name)> CachedFilteredAnimations;
    private TAE CachedAnimationSource;
    private string CachedAnimationFilter = "";
    private bool CachedExactAnimationFilter = false;

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

        ImGui.BeginChild($"SelectionListSection", new Vector2(0, height * TimeActListShare), ImGuiChildFlags.Borders);

        DisplaySelectionList();

        ImGui.EndChild();

        GUI.TitleHeader(
            LOC.Get("TAE_SelectionWindow_Animation_Header"),
            LOC.Get("TAE_SelectionWindow_Animation_Header_TT"));

        EditorFilters.DisplayFramedListFilter("timeActEditor_AnimationList_Filter",
            ref AnimationListFilter, ref ExactAnimationFilter);

        ImGui.BeginChild($"AnimationListSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayAnimationList();

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

                var labelName = entry.IsDirty ? $"{entry.Name}*" : entry.Name;

                if (ImGui.Selectable($"{labelName}##timeActEntry{entry.BinderID}", selected))
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

    public void DisplayAnimationList()
    {
        var source = View.Selection.SelectedTimeActContainerWrapper;
        var entry = View.Selection.SelectedTimeAct;

        if (source == null || entry?.TimeAct == null)
            return;

        var filteredAnimations = GetFilteredAnimations(source, entry);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredAnimations.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var (animation, name) = filteredAnimations[i];

                var selected = View.Selection.SelectedAnimation == animation;

                var alias = AliasHelper.GetAnimationAlias(Project, name);

                if (ImGui.Selectable($"{name}##timeActAnimation{animation.ID}", selected))
                {
                    SelectAnimation(animation);
                }

                if (_arrowKeyPressed && ImGui.IsItemFocused() && !selected)
                {
                    SelectAnimation(animation);

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

    private List<(TAE.Animation Animation, string Name)> GetFilteredAnimations(TimeActContainerWrapper source, TimeActEntry entry)
    {
        if (CachedFilteredAnimations != null &&
            CachedAnimationSource == entry.TimeAct &&
            CachedAnimationFilter == AnimationListFilter &&
            CachedExactAnimationFilter == ExactAnimationFilter)
        {
            return CachedFilteredAnimations;
        }

        CachedAnimationSource = entry.TimeAct;
        CachedAnimationFilter = AnimationListFilter;
        CachedExactAnimationFilter = ExactAnimationFilter;

        CachedFilteredAnimations = entry.TimeAct.Animations
            .Select(e => (Animation: e, Name: source.GetAnimationName(entry, e)))
            .Where(e => EditorFilters.IsMatch(AnimationListFilter, e.Name, ExactAnimationFilter,
                AliasHelper.GetAnimationAlias(Project, e.Name), true, true))
            .ToList();

        return CachedFilteredAnimations;
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
        View.Selection.SelectedAnimation = null;
        View.Selection.SelectedEvent = null;

        var firstAnimation = entry.TimeAct?.Animations.FirstOrDefault();

        if (firstAnimation != null)
        {
            SelectAnimation(firstAnimation);
        }
        else
        {
            View.Playback.OnAnimationChanged();
        }
    }

    private void SelectAnimation(TAE.Animation animation)
    {
        var selection = View.Selection;

        var template = Project.Handler.TimeActData.GetTemplate(selection.SelectedTimeActContainerWrapper);

        selection.SelectedTimeAct.ApplyTemplate(animation, template);

        selection.SelectedAnimation = animation;
        selection.SelectedEvent = null;

        View.Playback.OnAnimationChanged();
    }
}
