using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActAnimationSelectionWindow
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    private string AnimationListFilter = "";
    private bool ExactAnimationFilter = false;

    private bool _arrowKeyPressed = false;

    // A time act can have over a thousand animations, with a name to build for each
    private List<(TAE.Animation Animation, string Name)> CachedFilteredAnimations;
    private TAE CachedAnimationSource;
    private int CachedAnimationsVersion;
    private string CachedAnimationFilter = "";
    private bool CachedExactAnimationFilter = false;

    public TimeActAnimationSelectionWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        GUI.TitleHeader(
            LOC.Get("TAE_SelectionWindow_Animation_Header"),
            LOC.Get("TAE_SelectionWindow_Animation_Header_TT"));

        EditorFilters.DisplayFramedListFilter("timeActEditor_AnimationList_Filter",
            ref AnimationListFilter, ref ExactAnimationFilter);

        ImGui.BeginChild($"AnimationListSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayAnimationList();

        ImGui.EndChild();
    }

    public void DisplayAnimationList()
    {
        var source = View.Selection.SelectedTimeActContainerWrapper;
        var entry = View.Selection.SelectedTimeAct;

        if (source == null || entry?.TimeAct == null)
            return;

        var filteredAnimations = GetFilteredAnimations(source, entry);

        if (InputManager.HasArrowSelection())
        {
            _arrowKeyPressed = true;
        }

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

    private List<(TAE.Animation Animation, string Name)> GetFilteredAnimations(TimeActContainerWrapper source, TimeActEntry entry)
    {
        if (CachedFilteredAnimations != null &&
            CachedAnimationSource == entry.TimeAct &&
            CachedAnimationsVersion == entry.AnimationsVersion &&
            CachedAnimationFilter == AnimationListFilter &&
            CachedExactAnimationFilter == ExactAnimationFilter)
        {
            return CachedFilteredAnimations;
        }

        CachedAnimationSource = entry.TimeAct;
        CachedAnimationsVersion = entry.AnimationsVersion;
        CachedAnimationFilter = AnimationListFilter;
        CachedExactAnimationFilter = ExactAnimationFilter;

        CachedFilteredAnimations = entry.TimeAct.Animations
            .Select(e => (Animation: e, Name: source.GetAnimationName(entry, e)))
            .Where(e => EditorFilters.IsMatch(AnimationListFilter, e.Name, ExactAnimationFilter,
                AliasHelper.GetAnimationAlias(Project, e.Name), true, true))
            .ToList();

        return CachedFilteredAnimations;
    }

    public void SelectAnimation(TAE.Animation animation)
    {
        var selection = View.Selection;

        var template = Project.Handler.TimeActData.GetTemplate(selection.SelectedTimeActContainerWrapper);

        selection.SelectedTimeAct.ApplyTemplate(animation, template);

        selection.SelectedAnimation = animation;
        selection.SelectedEvent = null;

        View.Playback.OnAnimationChanged();
    }
}
