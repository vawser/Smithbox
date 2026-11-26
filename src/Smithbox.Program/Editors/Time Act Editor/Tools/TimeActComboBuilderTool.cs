using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Plays a list of animations one after another, to show what a combo looks like. Each step is selected
/// in turn, so the event graph and properties follow along with the model. A step can be cut short at a
/// frame, as an attack in a combo is usually cancelled into the next one before its clip ends.
/// </summary>
public partial class TimeActComboBuilderTool
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

        /// <summary>
        /// The action flag (FlagType) whose window the step cuts at, as a cancel window, or -1 to cut at
        /// <see cref="EndFrame" />
        /// </summary>
        public int CancelFlag = -1;
    }

    /// <summary>
    /// Where a step's cancel window opens (null if the animation has none for its flag from the step's start), and where
    /// an ActivateCancelEarly event opens it sooner (null if none does)
    /// </summary>
    private readonly record struct CancelPoint(float? Normal, float? Early)
    {
        public float? Get(bool applyEarly) => applyEarly && Early is { } early ? early : Normal;
    }

    // TAE events are timed at 30 frames a second, whatever rate the clip has
    private const float EventFramesPerSecond = 30f;

    // Each source keeps its own combo, so switching between characters doesn't lose it
    private readonly Dictionary<TimeActContainerWrapper, List<ComboStep>> Combos = new();

    public bool LoopCombo = false;

    /// <summary>
    /// Cut a step where an ActivateCancelEarly event opens its cancel window, rather than where the window opens
    /// </summary>
    public bool ApplyEarlyCancels = true;

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

            var start = GetStartTime(step);

            playback.Loop = false;
            playback.Seek(start);
            playback.StartFromCarriedRoot(start);

            // The crossfade from the last step began as the clip came in, at its first frame rather than where it starts
            playback.RestartBlendAt(start);
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
        return GetStartTime(step, View.Playback.FrameDuration, View.Playback.PlaybackDuration);
    }

    private float GetEndTime(ComboStep step)
    {
        return GetEndTime(step, ActiveAnimation, View.Playback.FrameDuration, View.Playback.PlaybackDuration);
    }

    /// <summary>
    /// Where a step starts in its clip, for a clip of the given frame length and duration
    /// </summary>
    private static float GetStartTime(ComboStep step, float frameDuration, float duration)
    {
        var start = Math.Max(step.StartFrame, 0) * frameDuration;

        return Math.Min(start, duration);
    }

    /// <summary>
    /// Where a step cuts to the next in its clip, for a clip of the given frame length and duration
    /// </summary>
    private float GetEndTime(ComboStep step, TAE.Animation animation, float frameDuration, float duration)
    {
        var start = GetStartTime(step, frameDuration, duration);

        // At the step's cancel window, while its animation still has one
        if (step.CancelFlag >= 0 && animation != null &&
            FindCancel(animation, step.CancelFlag, start).Get(ApplyEarlyCancels) is { } cut)
        {
            return Math.Clamp(cut, start, duration);
        }

        if (step.EndFrame < 0)
            return duration;

        var end = Math.Min(step.EndFrame * frameDuration, duration);

        return Math.Max(end, start);
    }

    #region Cancel Windows
    /// <summary>
    /// Where the window of an action flag opens in an animation, from a time on (a window already open by then counts
    /// from that time), and where an ActivateCancelEarly event naming the flag opens it sooner. Events switched off by
    /// their StateInfo don't count.
    /// </summary>
    private CancelPoint FindCancel(TAE.Animation animation, int flag, float from)
    {
        var data = Project.Handler.TimeActData;
        var simulation = View.Playback.Simulation;

        float? normal = null;
        float? early = null;

        if (data == null)
            return default;

        foreach (var ev in animation.Events)
        {
            if (!ev.IsInfiniteEnd && ev.EndTime <= from || !simulation.IsEventEnabled(ev))
                continue;

            var start = Math.Max(ev.StartTime, from);

            switch (data.GetSimulationRole(ev))
            {
                case TimeActSimulationRole.ActionFlag when GetFlag(ev, TimeActSimulation.FlagTypeField) == flag:
                    normal = Math.Min(normal ?? start, start);
                    break;

                case TimeActSimulationRole.ActivateCancelEarly when GetFlag(ev, TimeActSimulation.EarlyFlagField) == flag:
                    early = Math.Min(early ?? start, start);
                    break;
            }
        }

        // Only an early opening that comes before the window's own counts as early
        if (early is { } e && normal is { } n && e >= n)
            early = null;

        return new CancelPoint(normal, early);
    }

    private static int? GetFlag(TAE.Event ev, string field)
    {
        return TimeActSimulation.GetFloat(ev, field) is { } value ? (int)value : null;
    }

    /// <summary>
    /// The action flags of an animation a step can cut at, by their names in the FlagType enum, in the order their
    /// windows first open. Flags only an ActivateCancelEarly event names are included, by number.
    /// </summary>
    private List<(int Flag, string Name)> GetCancelOptions(TAE.Animation animation)
    {
        var options = new List<(int Flag, string Name)>();
        var data = Project.Handler.TimeActData;

        if (data == null)
            return options;

        foreach (var ev in animation.Events.OrderBy(e => e.StartTime))
        {
            var role = data.GetSimulationRole(ev);
            var field = role switch
            {
                TimeActSimulationRole.ActionFlag => TimeActSimulation.FlagTypeField,
                TimeActSimulationRole.ActivateCancelEarly => TimeActSimulation.EarlyFlagField,
                _ => null
            };

            if (field == null || GetFlag(ev, field) is not { } flag)
                continue;

            var name = role == TimeActSimulationRole.ActionFlag && ev.Template?.TryGetValue(field, out var parameter) == true &&
                       ev.Parameters?[field] is { } value
                ? parameter.ValueToString(value)
                : flag.ToString();

            var index = options.FindIndex(o => o.Flag == flag);

            if (index < 0)
            {
                options.Add((flag, name));
            }
            else if (role == TimeActSimulationRole.ActionFlag && options[index].Name == flag.ToString())
            {
                // Named once its own window turns up, after an early event that only gave its number
                options[index] = (flag, name);
            }
        }

        return options;
    }

    // The animation of each step, found again only once an edit or undo has replaced it
    private readonly Dictionary<ComboStep, (TimeActEntry Entry, TAE.Animation Animation)> StepAnimations = new();

    /// <summary>
    /// The animation of a step with its events read, for showing its cancel windows before it plays
    /// </summary>
    private TAE.Animation GetStepAnimation(TimeActContainerWrapper source, ComboStep step)
    {
        if (StepAnimations.TryGetValue(step, out var found) && found.Entry.TimeAct?.Animations.Contains(found.Animation) == true &&
            found.Entry.GetFullAnimationID(found.Animation, source.AnimationCategorySize) == step.AnimationID)
        {
            return found.Animation;
        }

        if (!source.TryFindAnimation(step.AnimationID, out var entry, out var animation))
        {
            StepAnimations.Remove(step);
            return null;
        }

        // The event parameters (FlagType) are only there once the template is on, as when it is selected
        entry.ApplyTemplate(animation, Project.Handler.TimeActData?.GetTemplate(source));

        StepAnimations[step] = (entry, animation);

        return animation;
    }

    private static int ToFrame(float time) => (int)MathF.Round(time * EventFramesPerSecond);
    #endregion

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

        // A combo starts where the character stands, at the origin
        View.Playback.ResetRootOffset();

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

        // The next step carries on from where this one left the character; a combo that loops starts over where it began
        if (next == 0)
        {
            View.Playback.ResetRootOffset();
        }
        else
        {
            View.Playback.CarryRootMotion();
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

        if (ImGui.BeginTable("timeActComboSteps", 6, flags))
        {
            var inputWidth = 60 * DPI.UIScale();

            ImGui.TableSetupColumn(LOC.Get("TAE_ComboBuilder_Column_Animation"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("TAE_ComboBuilder_Column_Start"), ImGuiTableColumnFlags.WidthFixed, inputWidth);
            ImGui.TableSetupColumn(LOC.Get("TAE_ComboBuilder_Column_End"), ImGuiTableColumnFlags.WidthFixed, inputWidth);
            ImGui.TableSetupColumn(LOC.Get("TAE_ComboBuilder_Column_Cancel"), ImGuiTableColumnFlags.WidthFixed, inputWidth * 2f);
            ImGui.TableSetupColumn(LOC.Get("TAE_ComboBuilder_Column_Cut"), ImGuiTableColumnFlags.WidthFixed, inputWidth * 1.25f);
            ImGui.TableSetupColumn("##actions", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            for (var i = 0; i < combo.Count; i++)
            {
                var step = combo[i];
                var stepAnimation = GetStepAnimation(source, step);
                var cancel = step.CancelFlag >= 0 && stepAnimation != null
                    ? FindCancel(stepAnimation, step.CancelFlag, Math.Max(step.StartFrame, 0) / EventFramesPerSecond)
                    : default;
                var cutsAtCancel = cancel.Get(ApplyEarlyCancels) != null;
                var name = source.FormatAnimationName(step.AnimationID);
                var alias = AliasHelper.GetAnimationAlias(Project, source.Name, name);

                ImGui.TableNextRow();

                // Animation
                ImGui.TableNextColumn();

                var label = $"{i + 1}. {name}";

                // The whole name, for when the column cuts it off
                var fullName = string.IsNullOrEmpty(alias) ? label : $"{label}\n{alias}";

                // One line, clipped by the column: wrapped, a narrow column would stack the name a letter a row
                if (playing && i == ActiveStep)
                {
                    ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, $"{Icons.Play} {label}");
                }
                else
                {
                    ImGui.TextUnformatted(label);
                }
                GUI.Tooltip(fullName);

                if (!string.IsNullOrEmpty(alias))
                {
                    GUI.DisplayAlias(alias, false);
                    GUI.Tooltip(fullName);
                }

                // Start
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);

                if (ImGui.InputInt($"##timeActComboStart{i}", ref step.StartFrame, 0))
                {
                    step.StartFrame = Math.Max(step.StartFrame, 0);
                }
                GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Start_TT"));

                // End: unused while the step cuts at its cancel window
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);

                ImGui.BeginDisabled(cutsAtCancel);
                if (ImGui.InputInt($"##timeActComboEnd{i}", ref step.EndFrame, 0))
                {
                    step.EndFrame = Math.Max(step.EndFrame, -1);
                }
                ImGui.EndDisabled();
                GUI.Tooltip(LOC.Get("TAE_ComboBuilder_End_TT"));

                // Cancel
                ImGui.TableNextColumn();
                DisplayCancelChoice(i, step, stepAnimation);

                // Cut
                ImGui.TableNextColumn();
                DisplayCut(step, cancel);

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
            StepAnimations.Remove(combo[remove]);
            combo.RemoveAt(remove);
        }

        if (playFrom >= 0)
        {
            StartCombo(playFrom);
        }
    }

    /// <summary>
    /// Picks the action flag whose window the step cuts at, from those its animation has
    /// </summary>
    private void DisplayCancelChoice(int index, ComboStep step, TAE.Animation animation)
    {
        var options = animation != null ? GetCancelOptions(animation) : new List<(int Flag, string Name)>();
        var none = LOC.Get("TAE_ComboBuilder_Cancel_None");

        // A flag the animation no longer has stays picked (it may come back with an undo), and says so
        var preview = step.CancelFlag < 0
            ? none
            : options.FirstOrDefault(o => o.Flag == step.CancelFlag).Name ?? LOC.Get("TAE_ComboBuilder_Cancel_Missing", step.CancelFlag);

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo($"##timeActComboCancel{index}", preview))
        {
            if (ImGui.Selectable($"{none}##timeActComboCancelNone{index}", step.CancelFlag < 0))
            {
                step.CancelFlag = -1;
            }

            foreach (var (flag, name) in options)
            {
                if (ImGui.Selectable($"{name}##timeActComboCancel{index}_{flag}", step.CancelFlag == flag))
                {
                    step.CancelFlag = flag;
                }
            }

            if (options.Count == 0)
            {
                ImGui.TextDisabled(LOC.Get("TAE_ComboBuilder_Cancel_No_Flags"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Cancel_TT"));
    }

    /// <summary>
    /// Shows the frame the step cuts at: with a cancel window, both where the window opens and where an early event
    /// opens it, the one in use highlighted and the other greyed out
    /// </summary>
    private void DisplayCut(ComboStep step, CancelPoint cancel)
    {
        if (step.CancelFlag < 0 || cancel.Normal == null && cancel.Early == null)
        {
            ImGui.TextDisabled(step.EndFrame >= 0 ? step.EndFrame.ToString() : LOC.Get("TAE_ComboBuilder_Cut_Clip_End"));
            GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Cut_TT"));
            return;
        }

        var used = UI.Current.ImGui_Benefit_Text_Color;
        var unused = ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled];

        var earlyUsed = ApplyEarlyCancels && cancel.Early != null;

        var normalText = cancel.Normal is { } normal
            ? LOC.Get("TAE_ComboBuilder_Cut_Normal", ToFrame(normal))
            : LOC.Get("TAE_ComboBuilder_Cut_Normal", LOC.Get("TAE_ComboBuilder_Cut_Clip_End"));

        ImGui.TextColored(earlyUsed ? unused : used, normalText);

        if (cancel.Early is { } early)
        {
            // How many frames sooner than the window's own opening (a step's clip length isn't known until it plays)
            var sooner = cancel.Normal is { } opens ? $" (-{ToFrame(opens) - ToFrame(early)})" : "";

            ImGui.TextColored(earlyUsed ? used : unused, LOC.Get("TAE_ComboBuilder_Cut_Early", ToFrame(early), sooner));
        }

        GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Cut_TT"));
    }

    private ComboStep GetActiveStep()
    {
        return IsPlaying && ActiveSource != null && ActiveStep < GetCombo(ActiveSource).Count ? GetCombo(ActiveSource)[ActiveStep] : null;
    }

    private void DisplayOptions()
    {
        // Options
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TAE_ComboBuilder_Options_Header"), LOC.Get("TAE_ComboBuilder_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_ComboBuilder_Loop")}##timeActComboLoop", ref LoopCombo);
        GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Loop_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_ComboBuilder_Early")}##timeActComboEarly", ref ApplyEarlyCancels);
        GUI.Tooltip(LOC.Get("TAE_ComboBuilder_Early_TT"));
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
            () =>
            {
                combo.ForEach(s => StepAnimations.Remove(s));
                combo.Clear();
            }, hasSteps && !IsPlaying);

        GUI.ConditionalMultiButtonInput("timeActComboBakeActions",
            "timeActComboBake", LOC.Get("TAE_ComboBuilder_Bake"), LOC.Get("TAE_ComboBuilder_Bake_TT"),
            () => BeginBake(View.Selection.SelectedTimeActContainerWrapper, combo), hasSteps && !IsPlaying);

        DisplayBakePopup();

        if (IsPlaying && ActiveSource == View.Selection.SelectedTimeActContainerWrapper)
        {
            var status = WaitingForClip
                ? LOC.Get("TAE_ComboBuilder_Status_Loading", ActiveStep + 1, combo.Count)
                : !View.Playback.Playing
                    ? LOC.Get("TAE_ComboBuilder_Status_Paused", ActiveStep + 1, combo.Count)
                    : LOC.Get("TAE_ComboBuilder_Status_Playing", ActiveStep + 1, combo.Count);

            GUI.WrappedText(status);

            // Where the step on show cuts, and how much sooner the early opening has it cut
            if (!WaitingForClip && ActiveAnimation != null && GetActiveStep() is { CancelFlag: >= 0 } step)
            {
                var cancel = FindCancel(ActiveAnimation, step.CancelFlag, GetStartTime(step));

                if (cancel.Get(ApplyEarlyCancels) is { } cut)
                {
                    var later = cancel.Normal ?? View.Playback.PlaybackDuration;

                    GUI.WrappedText(ApplyEarlyCancels && cancel.Early != null
                        ? LOC.Get("TAE_ComboBuilder_Status_Cut_Early", ToFrame(cut), ToFrame(later) - ToFrame(cut))
                        : LOC.Get("TAE_ComboBuilder_Status_Cut", ToFrame(cut)));
                }
            }
        }
    }
    #endregion
}
