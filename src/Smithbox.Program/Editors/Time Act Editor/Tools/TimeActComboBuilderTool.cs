using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Plays a list of animations one after another, to show what a combo looks like. Each step is selected
/// in turn, so the event graph and properties follow along with the model. A step can be cut short at a
/// frame, as an attack in a combo is usually cancelled into the next one before its clip ends.
/// </summary>
public class TimeActComboBuilderTool
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    // A clip that takes longer than this to load is given up on, and the step plays on its events
    private const float MaxClipWait = 10f;

    private const float EndTolerance = 0.0001f;

    public class ComboStep
    {
        /// <summary>
        /// The full animation ID, as the animation objects are replaced by edits and undo
        /// </summary>
        public long AnimationID;

        public int StartFrame = 0;

        /// <summary>
        /// The frame the step cuts to the next one at, or -1 to play the clip to its end
        /// </summary>
        public int EndFrame = -1;
    }

    // Each source keeps its own combo, so switching between characters doesn't lose it
    private readonly Dictionary<TimeActContainerWrapper, List<ComboStep>> Combos = new();

    public bool LoopCombo = false;

    // The step being played, or -1 when the combo isn't playing
    private int ActiveStep = -1;
    private bool WaitingForClip;
    private float ClipWait;

    private TimeActContainerWrapper ActiveSource;
    private TAE.Animation ActiveAnimation;
    private bool SavedLoop;

    public TimeActComboBuilderTool(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool IsPlaying => ActiveStep >= 0;

    private List<ComboStep> GetCombo(TimeActContainerWrapper source)
    {
        if (!Combos.TryGetValue(source, out var combo))
        {
            combo = new List<ComboStep>();
            Combos[source] = combo;
        }

        return combo;
    }

    /// <summary>
    /// Adds an animation to the end of the combo of the selected source
    /// </summary>
    public void AddStep(TimeActEntry entry, TAE.Animation animation)
    {
        var source = View.Selection.SelectedTimeActContainerWrapper;

        if (source == null || entry == null || animation == null)
            return;

        GetCombo(source).Add(new ComboStep
        {
            AnimationID = entry.GetFullAnimationID(animation, source.AnimationCategorySize)
        });
    }

    #region Playback
    /// <summary>
    /// Moves the combo along. This runs every frame after the playback, even with the tool hidden.
    /// </summary>
    public void Update()
    {
        if (ActiveStep < 0)
            return;

        var selection = View.Selection;
        var playback = View.Playback;

        // Picking another source or animation takes over from the combo
        if (selection.SelectedTimeActContainerWrapper != ActiveSource || selection.SelectedAnimation != ActiveAnimation)
        {
            StopCombo(false);
            return;
        }

        var combo = GetCombo(ActiveSource);

        if (ActiveStep >= combo.Count)
        {
            StopCombo(false);
            return;
        }

        var step = combo[ActiveStep];

        if (WaitingForClip)
        {
            ClipWait += ImGui.GetIO().DeltaTime;

            if (playback.IsClipLoading && ClipWait < MaxClipWait)
                return;

            WaitingForClip = false;

            playback.Loop = false;
            playback.Seek(GetStartTime(step));
            playback.Play();

            return;
        }

        if (playback.Time >= GetEndTime(step) - EndTolerance)
        {
            NextStep();
        }

        // Otherwise a pause is the user's, and the combo carries on once they play again
    }

    private float GetStartTime(ComboStep step)
    {
        var start = Math.Max(step.StartFrame, 0) * View.Playback.FrameDuration;

        return Math.Min(start, View.Playback.PlaybackDuration);
    }

    private float GetEndTime(ComboStep step)
    {
        var duration = View.Playback.PlaybackDuration;

        if (step.EndFrame < 0)
            return duration;

        var end = Math.Min(step.EndFrame * View.Playback.FrameDuration, duration);

        return Math.Max(end, GetStartTime(step));
    }

    private void StartCombo(int index)
    {
        var source = View.Selection.SelectedTimeActContainerWrapper;

        if (source == null || GetCombo(source).Count == 0)
            return;

        if (ActiveStep < 0)
        {
            SavedLoop = View.Playback.Loop;
        }

        ActiveSource = source;

        BeginStep(index, 0);
    }

    private void NextStep()
    {
        var next = ActiveStep + 1;

        if (next >= GetCombo(ActiveSource).Count)
        {
            if (!LoopCombo)
            {
                // The last pose stays up, as the end of a combo is what it builds to
                StopCombo(false);
                return;
            }

            next = 0;
        }

        BeginStep(next, 0);
    }

    /// <summary>
    /// Selects the animation of a step, then waits for its clip before playing it. Steps whose
    /// animation can't be found are skipped.
    /// </summary>
    private void BeginStep(int index, int skipped)
    {
        var combo = GetCombo(ActiveSource);

        if (skipped >= combo.Count)
        {
            Smithbox.LogError(this, LOC.Get("TAE_ComboBuilder_No_Animations_Found"));
            StopCombo(false);
            return;
        }

        if (index >= combo.Count)
        {
            if (!LoopCombo)
            {
                StopCombo(false);
                return;
            }

            index = 0;
        }

        var step = combo[index];

        if (!ActiveSource.TryFindAnimation(step.AnimationID, out var entry, out var animation))
        {
            Smithbox.LogError(this, LOC.Get("TAE_ComboBuilder_Animation_Not_Found",
                ActiveSource.FormatAnimationName(step.AnimationID)));

            BeginStep(index + 1, skipped + 1);
            return;
        }

        var selection = View.Selection;

        if (selection.SelectedTimeAct != entry)
        {
            entry.Load();

            selection.SelectedTimeAct = entry;
            View.SelectionWindow.ScrollTo(entry);
        }

        ActiveStep = index;
        ActiveAnimation = animation;
        WaitingForClip = true;
        ClipWait = 0;

        View.AnimationSelectionWindow.SelectAnimation(animation);
        View.AnimationSelectionWindow.ScrollTo(animation);

        // Holds the clip still while it loads, rather than letting it play from the start by itself
        View.Playback.Pause();
    }

    /// <summary>
    /// Ends the combo, and gives the playback back its own loop setting
    /// </summary>
    private void StopCombo(bool pause)
    {
        if (ActiveStep < 0)
            return;

        ActiveStep = -1;
        ActiveAnimation = null;
        WaitingForClip = false;

        View.Playback.Loop = SavedLoop;

        if (pause)
        {
            View.Playback.Pause();
        }
    }
    #endregion

    #region Display
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("TAE_ComboBuilder_Header")}##timeActComboBuilderHeader"))
            return;

        ImGui.BeginChild("TimeActComboBuilderSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("TAE_ComboBuilder_Hint"));

        var source = View.Selection.SelectedTimeActContainerWrapper;

        if (source == null)
        {
            GUI.WrappedText(LOC.Get("TAE_ComboBuilder_No_Source"));
            return;
        }

        var combo = GetCombo(source);

        DisplayAdd();
        DisplaySteps(source, combo);
        DisplayOptions();
        DisplayActions(combo);
    }

    private void DisplayAdd()
    {
        var selection = View.Selection;

        // Add
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_ComboBuilder_Add_Header"), LOC.Get("TAE_ComboBuilder_Add_Header_TT"));

        var hasAnimation = selection.SelectedTimeAct != null && selection.SelectedAnimation != null;

        GUI.ConditionalMultiButtonInput("timeActComboAdd",
            "timeActComboAddSelected", LOC.Get("TAE_ComboBuilder_Add_Selected"), LOC.Get("TAE_ComboBuilder_Add_Selected_TT"),
            () => AddStep(selection.SelectedTimeAct, selection.SelectedAnimation), hasAnimation);
    }

    private void DisplaySteps(TimeActContainerWrapper source, List<ComboStep> combo)
    {
        // Steps
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_ComboBuilder_Steps_Header"), LOC.Get("TAE_ComboBuilder_Steps_Header_TT"));

        if (combo.Count == 0)
        {
            GUI.WrappedText(LOC.Get("TAE_ComboBuilder_Empty"));
            return;
        }

        var playing = IsPlaying && ActiveSource == source;

        // Taken while drawing, and applied after, as they change the list being drawn
        var moveFrom = -1;
        var moveTo = -1;
        var remove = -1;
        var playFrom = -1;

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable("timeActComboSteps", 4, flags))
        {
            var inputWidth = 60 * DPI.UIScale();

            ImGui.TableSetupColumn(LOC.Get("TAE_ComboBuilder_Column_Animation"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("TAE_ComboBuilder_Column_Start"), ImGuiTableColumnFlags.WidthFixed, inputWidth);
            ImGui.TableSetupColumn(LOC.Get("TAE_ComboBuilder_Column_End"), ImGuiTableColumnFlags.WidthFixed, inputWidth);
            ImGui.TableSetupColumn("##actions", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            for (var i = 0; i < combo.Count; i++)
            {
                var step = combo[i];
                var name = source.FormatAnimationName(step.AnimationID);
                var alias = AliasHelper.GetAnimationAlias(Project, source.Name, name);

                ImGui.TableNextRow();

                // Animation
                ImGui.TableNextColumn();

                var label = $"{i + 1}. {name}";

                if (playing && i == ActiveStep)
                {
                    GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{Icons.Play} {label}");
                }
                else
                {
                    GUI.WrappedText(label);
                }

                if (!string.IsNullOrEmpty(alias))
                {
                    GUI.DisplayAlias(alias);
                }

                // Start
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.InputInt($"##timeActComboStart{i}", ref step.StartFrame, 0))
                {
                    step.StartFrame = Math.Max(step.StartFrame, 0);
                }
                GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Start_TT"));

                // End
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.InputInt($"##timeActComboEnd{i}", ref step.EndFrame, 0))
                {
                    step.EndFrame = Math.Max(step.EndFrame, -1);
                }
                GUI.Tooltip(LOC.Get("TAE_ComboBuilder_End_TT"));

                // Actions
                ImGui.TableNextColumn();

                if (ImGui.Button($"{Icons.Play}##timeActComboPlayFrom{i}"))
                {
                    playFrom = i;
                }
                GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Play_From_TT"));

                ImGui.SameLine();

                ImGui.BeginDisabled(playing);

                ImGui.BeginDisabled(i == 0);
                if (ImGui.Button($"{Icons.ArrowUp}##timeActComboUp{i}"))
                {
                    moveFrom = i;
                    moveTo = i - 1;
                }
                ImGui.EndDisabled();
                GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Move_Up_TT"));

                ImGui.SameLine();

                ImGui.BeginDisabled(i == combo.Count - 1);
                if (ImGui.Button($"{Icons.ArrowDown}##timeActComboDown{i}"))
                {
                    moveFrom = i;
                    moveTo = i + 1;
                }
                ImGui.EndDisabled();
                GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Move_Down_TT"));

                ImGui.SameLine();

                if (ImGui.Button($"{Icons.TrashO}##timeActComboRemove{i}"))
                {
                    remove = i;
                }
                GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Remove_TT"));

                ImGui.EndDisabled();
            }

            ImGui.EndTable();
        }

        if (moveFrom >= 0)
        {
            (combo[moveFrom], combo[moveTo]) = (combo[moveTo], combo[moveFrom]);
        }

        if (remove >= 0)
        {
            combo.RemoveAt(remove);
        }

        if (playFrom >= 0)
        {
            StartCombo(playFrom);
        }
    }

    private void DisplayOptions()
    {
        // Options
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_ComboBuilder_Options_Header"), LOC.Get("TAE_ComboBuilder_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_ComboBuilder_Loop")}##timeActComboLoop", ref LoopCombo);
        GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Loop_TT"));
    }

    private void DisplayActions(List<ComboStep> combo)
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_ComboBuilder_Actions_Header"), LOC.Get("TAE_ComboBuilder_Actions_Header_TT"));

        var hasSteps = combo.Count > 0;

        GUI.ConditionalMultiButtonInput("timeActComboActions",
            "timeActComboPlay", LOC.Get("TAE_ComboBuilder_Play"), LOC.Get("TAE_ComboBuilder_Play_TT"),
            () => StartCombo(0), hasSteps,

            "timeActComboStop", LOC.Get("TAE_ComboBuilder_Stop"), LOC.Get("TAE_ComboBuilder_Stop_TT"),
            () => StopCombo(true), IsPlaying,

            "timeActComboClear", LOC.Get("TAE_ComboBuilder_Clear"), LOC.Get("TAE_ComboBuilder_Clear_TT"),
            combo.Clear, hasSteps && !IsPlaying);

        if (IsPlaying && ActiveSource == View.Selection.SelectedTimeActContainerWrapper)
        {
            var status = WaitingForClip
                ? LOC.Get("TAE_ComboBuilder_Status_Loading", ActiveStep + 1, combo.Count)
                : !View.Playback.Playing
                    ? LOC.Get("TAE_ComboBuilder_Status_Paused", ActiveStep + 1, combo.Count)
                    : LOC.Get("TAE_ComboBuilder_Status_Playing", ActiveStep + 1, combo.Count);

            GUI.WrappedText(status);
        }
    }
    #endregion
}
