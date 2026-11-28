#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static StudioCore.Editors.AnimationEditor.AnimationToolLayout;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Clip window: the selected clip's details, its playback in the viewport, the operations on it (each an undoable
/// step, shown before it is saved) and Save as New ID. Operation fields only apply with their button, as the window
/// has several of them.
/// </summary>
public class AnimationClipView
{
    private readonly AnimationEditorView View;

    // Operation inputs, set again when another clip is selected
    private int TrimFirst;
    private int TrimLast;
    private float Speed = 1;
    private int FrameCount;
    private int BlendFrames = 5;
    private string SpliceFilter = "";
    private AnimationClipSource? SpliceWith;
    private string NewName = "";
    private string? NewNameError;

    // Range timing inputs (§13.3 item 5); the range follows the Timeline's when one is picked there
    private int RangeFirst;
    private int RangeLast;
    private int RangeLength;
    private int HoldCount = 5;
    private int RangeSeen = -1;

    /// <summary>
    /// A change of a clip's timing (trim, speed, frame count, a range retime, hold or removal) not yet passed on to its
    /// time act events: an event time t becomes Map(t) (seconds). Later time changes of the same clip are folded in, so
    /// one step there matches every step here. It only holds while the clip is still <see cref="After" /> (an undo here
    /// drops it).
    /// </summary>
    private sealed record EventRetime(AnimationClipSource Source, EditableClip After, TimeMap Map);

    private EventRetime? PendingRetime;

    // The clip whose retime was just sent to the Time Act Editor, for the note
    private AnimationClipSource? RetimeSent;

    // The warnings of the selected clip, worked out once per edit
    private EditableClip? WarningsFor;
    private float LargestStep;
    private IReadOnlyList<string> Unpaired = Array.Empty<string>();

    public AnimationClipView(AnimationEditorView view)
    {
        View = view;
    }

    public void OnClipSelected()
    {
        WarningsFor = null;
        NewNameError = null;
        SpliceWith = null;

        if (View.SelectedClip is { } source && View.Bank.Get(source) is { } clip)
        {
            TrimFirst = 0;
            TrimLast = clip.FrameCount - 1;
            FrameCount = clip.FrameCount;
            Speed = 1;
            NewName = source.Name;

            if (View.TimelineView.Range is { } range && range.Last < clip.FrameCount)
                (RangeFirst, RangeLast) = range;
            else
                (RangeFirst, RangeLast) = (0, clip.FrameCount - 1);
            RangeLength = RangeLast - RangeFirst;
        }
    }

    /// <summary>As a tool of the Tools window</summary>
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Clip")}##animationClipToolHeader"))
            return;

        ImGui.BeginChild("AnimationClipToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    public void Display()
    {
        var bank = View.Bank;
        if (bank.Character is not { } character)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clips_No_Character"));
            return;
        }

        if (View.SelectedClip is not { } source)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clip_None"));
            return;
        }

        var clip = bank.Get(source);
        if (clip == null)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clip_Unreadable", source.Name));
            return;
        }

        UpdateWarnings(clip, character.Skeleton);

        // Playback is in the Timeline window
        DisplayInfo(source, clip);

        // Whole-clip operations work on the frames as they play, so offset keys are folded in first (their frames
        // would mean something else after a trim or a retime)
        var baked = clip.Baked();
        DisplayTrim(source, baked);
        DisplayRetime(source, baked);
        DisplayEventRetime(source, character);
        DisplayReverseMirror(source, baked, character);
        DisplayAppend(source, baked, character);

        // Range timing and smoothing keep the keys (range timing moves them with their frames)
        DisplayRangeTiming(source, clip);
        DisplaySmooth(source, clip);

        DisplayNewId(source, clip, character);
        DisplayRevert(source);
    }

    // ---- details

    private void DisplayInfo(AnimationClipSource source, EditableClip clip)
    {
        GUI.SimpleHeader(source.Name, LOC.Get("ANIM_Clip_Header_TT"));

        ImGui.TextDisabled(StateText(View.Bank, source));

        var fps = clip.FrameDuration > 0 ? 1 / clip.FrameDuration : 0;
        ImGui.Text(LOC.Get("ANIM_Clip_Info_Frames", clip.FrameCount, clip.Duration.ToString("F2"), fps.ToString("F0")));
        ImGui.Text(LOC.Get("ANIM_Clip_Info_Tracks", clip.TrackCount, View.Bank.Character!.Skeleton.BoneCount));
        ImGui.Text(clip.EffectiveRootMotion != null ? LOC.Get("ANIM_Clip_Info_Root_Motion") : LOC.Get("ANIM_Clip_Info_No_Root_Motion"));
        if (clip.BlendMode != HavokBlendMode.Normal)
            ImGui.Text(LOC.Get("ANIM_Clip_Info_Additive"));
        if (clip.HasOffsets)
            ImGui.Text(LOC.Get("ANIM_Clip_Info_Offsets", clip.KeyCountAllLayers, clip.Layers.SelectMany(l => l.Keys.Keys).Distinct().Count()));
        if (clip.Layers.Count > 1)
            ImGui.Text(LOC.Get("ANIM_Clip_Info_Layers", clip.Layers.Count, clip.Active.Name));

        ImGui.TextDisabled(source.BinderPath);
    }

    /// <summary>Where a clip's work stands: not saved, saved to the work files, exported to the anibnd, or none</summary>
    public static string StateText(AnimationEditorBank bank, AnimationClipSource source)
    {
        if (bank.IsUnsaved(source))
            return LOC.Get(source.IsNew ? "ANIM_Clip_State_New" : "ANIM_Clip_State_Edited");

        if (bank.IsUnexported(source))
            return LOC.Get(source.IsNew ? "ANIM_Clip_State_New_Not_Exported" : "ANIM_Clip_State_Not_Exported");

        return LOC.Get(bank.IsEdited(source) ? "ANIM_Clip_State_Exported" : "ANIM_Clip_State_Saved");
    }

    // ---- operations

    private void UpdateWarnings(EditableClip clip, HavokSkeletonData skeleton)
    {
        if (WarningsFor == clip)
            return;

        WarningsFor = clip;
        LargestStep = EditableClipOperations.LargestRotationStep(clip);
        Unpaired = clip.BlendMode == HavokBlendMode.Normal
            ? EditableClipOperations.MirrorUnpairedBones(clip, skeleton)
            : Array.Empty<string>();
    }

    // Trim: the frames kept
    private void DisplayTrim(AnimationClipSource source, EditableClip clip)
    {
        Group(LOC.Get("ANIM_Op_Trim"), LOC.Get("ANIM_Op_Trim_TT"));

        if (BeginForm("animationTrimForm"))
        {
            FormRow(LOC.Get("ANIM_Op_Trim_First"), LOC.Get("ANIM_Op_Trim_TT"));
            ImGui.InputInt("##animationTrimFirst", ref TrimFirst);
            FormRow(LOC.Get("ANIM_Op_Trim_Last"), LOC.Get("ANIM_Op_Trim_TT"));
            ImGui.InputInt("##animationTrimLast", ref TrimLast);
            ImGui.EndTable();
        }

        TrimFirst = Math.Clamp(TrimFirst, 0, clip.FrameCount - 1);
        TrimLast = Math.Clamp(TrimLast, 0, clip.FrameCount - 1);

        var changed = TrimLast - TrimFirst >= 1 && !(TrimFirst == 0 && TrimLast == clip.FrameCount - 1);
        if (WideButton(LOC.Get("ANIM_Op_Trim_Button", TrimFirst, TrimLast), "animationTrimApply", changed, LOC.Get("ANIM_Op_Trim_TT")))
        {
            // Taken now: applying selects the clip again, which resets the fields
            var (first, last) = (TrimFirst, TrimLast);
            var trimOffset = -first * clip.FrameDuration;
            Apply(source, LOC.Get("ANIM_Action_Trim", source.Name, first, last),
                () => EditableClipOperations.Trim(clip, first, last), (_, _) => TimeMap.Affine(1, trimOffset));
        }
    }

    // Retime: the whole clip faster or slower, as a speed or a frame count (each follows the other)
    private void DisplayRetime(AnimationClipSource source, EditableClip clip)
    {
        Group(LOC.Get("ANIM_Op_Retime_Header"), LOC.Get("ANIM_Op_Retime_Header_TT"));

        var span = Math.Max(clip.FrameCount - 1, 1);
        if (BeginForm("animationRetimeForm"))
        {
            FormRow(LOC.Get("ANIM_Op_Speed"), LOC.Get("ANIM_Op_Speed_TT"));
            if (ImGui.InputFloat("##animationSpeedValue", ref Speed, 0.1f, 0.5f, "%.2f"))
            {
                Speed = Math.Clamp(Speed, 0.05f, 20f);
                FrameCount = Math.Max((int)MathF.Round(span / Speed) + 1, 2);
            }

            FormRow(LOC.Get("ANIM_Op_Frames"), LOC.Get("ANIM_Op_Frames_TT"));
            if (ImGui.InputInt("##animationFrameCount", ref FrameCount))
            {
                FrameCount = Math.Clamp(FrameCount, 2, 100000);
                Speed = (float)span / (FrameCount - 1);
            }

            ImGui.EndTable();
        }

        var speed = (float)span / Math.Max(FrameCount - 1, 1);
        if (LargestStep * speed > 180)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                LOC.Get("ANIM_Op_Speed_Warning", LargestStep.ToString("F0"), (LargestStep * speed).ToString("F0")));

        if (WideButton(LOC.Get("ANIM_Op_Retime_Button", FrameCount, speed.ToString("0.##")), "animationRetimeApply",
                FrameCount != clip.FrameCount, LOC.Get("ANIM_Op_Retime_TT")))
        {
            var count = FrameCount;
            Apply(source, LOC.Get("ANIM_Action_Retime", source.Name, count, speed.ToString("0.##")),
                () => EditableClipOperations.Retime(clip, count), StretchTiming);
        }
    }

    // Reverse and mirror
    private void DisplayReverseMirror(AnimationClipSource source, EditableClip clip, AnimationCharacter character)
    {
        Group(LOC.Get("ANIM_Op_Direction_Header"), LOC.Get("ANIM_Op_Direction_Header_TT"));

        if (WideButton(LOC.Get("ANIM_Op_Reverse"), "animationReverse", true, LOC.Get("ANIM_Op_Reverse_TT")))
            Apply(source, LOC.Get("ANIM_Action_Reverse", source.Name), () => EditableClipOperations.Reverse(clip));

        var additive = clip.BlendMode != HavokBlendMode.Normal;
        if (WideButton(LOC.Get("ANIM_Op_Mirror"), "animationMirror", !additive,
                additive ? LOC.Get("ANIM_Op_Mirror_Additive") : LOC.Get("ANIM_Op_Mirror_TT")))
            Apply(source, LOC.Get("ANIM_Action_Mirror", source.Name), () => EditableClipOperations.Mirror(clip, character.Skeleton));

        if (Unpaired.Count > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ANIM_Op_Mirror_Unpaired", Unpaired.Count));
            GUI.Tooltip(string.Join("\n", Unpaired));
        }
    }

    // Append another clip of the character, crossfading into it
    private void DisplayAppend(AnimationClipSource source, EditableClip clip, AnimationCharacter character)
    {
        Group(LOC.Get("ANIM_Op_Splice"), LOC.Get("ANIM_Op_Splice_TT"));

        if (BeginForm("animationAppendForm"))
        {
            FormRow(LOC.Get("ANIM_Op_Splice_Clip"), LOC.Get("ANIM_Op_Splice_TT"));
            DisplaySplicePicker(source, character);
            FormRow(LOC.Get("ANIM_Op_Splice_Blend"), LOC.Get("ANIM_Op_Splice_Blend_TT"));
            ImGui.InputInt("##animationSpliceBlend", ref BlendFrames);
            ImGui.EndTable();
        }

        BlendFrames = Math.Clamp(BlendFrames, 0, 1000);

        var second = SpliceWith != null ? View.Bank.Get(SpliceWith)?.Baked() : null;
        var spliceProblem = SpliceProblem(clip, second);
        if (spliceProblem != null && second != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, spliceProblem);

        var label = SpliceWith != null ? LOC.Get("ANIM_Op_Splice_Button", SpliceWith.Name) : LOC.Get("ANIM_Op_Splice_Button_None");
        if (WideButton(label, "animationSpliceApply", second != null && spliceProblem == null, LOC.Get("ANIM_Op_Splice_TT")) && second != null)
            Apply(source, LOC.Get("ANIM_Action_Splice", source.Name, SpliceWith!.Name, BlendFrames),
                () => EditableClipOperations.Splice(clip, second, BlendFrames));
    }

    // Revert to the clip as the anibnd has it
    private void DisplayRevert(AnimationClipSource source)
    {
        Group(LOC.Get("ANIM_Revert_Header"), LOC.Get("ANIM_Op_Revert_TT"));

        if (WideButton(LOC.Get("ANIM_Op_Revert"), "animationRevert", View.Bank.IsEdited(source) && !source.IsNew, LOC.Get("ANIM_Op_Revert_TT")))
            View.Execute(new AnimationClipEditAction(View.Bank, source, null, LOC.Get("ANIM_Action_Revert", source.Name), OnClipSelected));
    }

    private void DisplaySplicePicker(AnimationClipSource source, AnimationCharacter character)
    {
        ImGui.SetNextItemWidth(-1);
        if (!ImGui.BeginCombo("##animationSpliceWith", SpliceWith?.Name ?? LOC.Get("ANIM_Op_Splice_Pick")))
            return;

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##animationSpliceFilter", LOC.Get("ANIM_Filter_Hint"), ref SpliceFilter, 64);

        // The first matches only: a combo of thousands of rows is no use
        var shown = 0;
        foreach (var other in character.Clips)
        {
            if (other == source || SpliceFilter.Length > 0 && !other.Name.Contains(SpliceFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            if (++shown > 200)
            {
                ImGui.TextDisabled(LOC.Get("ANIM_Op_Splice_More"));
                break;
            }

            if (ImGui.Selectable($"{other.Name}##animationSplice_{other.BinderId}", SpliceWith == other))
                SpliceWith = other;
        }

        ImGui.EndCombo();
    }

    private static string? SpliceProblem(EditableClip first, EditableClip? second)
    {
        if (second == null)
            return null;

        if (first.TrackCount != second.TrackCount || !first.TrackToBone.SequenceEqual(second.TrackToBone))
            return LOC.Get("ANIM_Op_Splice_Different_Bones");

        if (first.FloatTrackCount != second.FloatTrackCount)
            return LOC.Get("ANIM_Op_Splice_Different_Floats");

        if (first.BlendMode != second.BlendMode)
            return LOC.Get("ANIM_Op_Splice_Different_Additive");

        return null;
    }

    /// <summary>
    /// Runs an operation as one undoable step. <paramref name="timing" />, for an operation that changes when things
    /// happen, gives the event time mapping (scale, offset) from the clip before and after, which is offered for the
    /// clip's time act events (<see cref="DisplayEventRetime" />).
    /// </summary>
    private void Apply(AnimationClipSource source, string name, Func<EditableClip> operation,
        Func<EditableClip, EditableClip, TimeMap>? timing = null)
    {
        EditableClip result;
        try
        {
            result = operation();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("ANIM_Op_Failed", name, e.Message), e);
            return;
        }

        var before = View.Bank.Get(source);
        View.Execute(new AnimationClipEditAction(View.Bank, source, result, name, OnClipSelected));

        if (timing == null || before == null || View.Bank.Get(source) is not { } after)
            return;

        var map = timing(before, result);

        // Folded into the time change still waiting for this clip: t → map2(map1(t))
        if (PendingRetime is { } pending && pending.Source == source && pending.After == before)
            PendingRetime = new EventRetime(source, after, pending.Map.Then(map));
        else
            PendingRetime = new EventRetime(source, after, map);

        RetimeSent = null;
    }

    /// <summary>Whether a clip has a time change not yet passed on to its time act events (for the check before Export)</summary>
    public bool HasPendingRetime(AnimationClipSource source)
    {
        return PendingRetime is { } pending && pending.Source == source && View.Bank.Get(source) == pending.After &&
               !pending.Map.IsIdentity;
    }

    /// <summary>Speed and frame count stretch the clip: event times scale with its length</summary>
    private static TimeMap StretchTiming(EditableClip before, EditableClip after)
    {
        return TimeMap.Affine(before.Duration > 0 ? after.Duration / before.Duration : 1, 0);
    }

    // ---- range timing

    /// <summary>
    /// Range Timing: a range of frames played over a new length, a held frame at the playhead, a range taken out. Each
    /// keeps the offset keys (moved with their frames) and offers its event mapping as the whole-clip time operations
    /// do. The range is the Timeline's (Shift+drag on its ruler) when one is picked there.
    /// </summary>
    private void DisplayRangeTiming(AnimationClipSource source, EditableClip clip)
    {
        Group(LOC.Get("ANIM_Range_Header"), LOC.Get("ANIM_Range_Header_TT"));

        var end = clip.FrameCount - 1;
        var timeline = View.TimelineView;
        if (timeline.RangeVersion != RangeSeen)
        {
            RangeSeen = timeline.RangeVersion;
            if (timeline.Range is { } picked && picked.Last <= end)
            {
                (RangeFirst, RangeLast) = picked;
                RangeLength = RangeLast - RangeFirst;
            }
        }

        // The range, its new length and the frames to hold at the playhead
        if (BeginForm("animationRangeForm"))
        {
            FormRow(LOC.Get("ANIM_Op_Trim_First"), LOC.Get("ANIM_Range_Frames_TT"));
            ImGui.InputInt("##animationRangeFirst", ref RangeFirst);
            FormRow(LOC.Get("ANIM_Op_Trim_Last"), LOC.Get("ANIM_Range_Frames_TT"));
            ImGui.InputInt("##animationRangeLast", ref RangeLast);
            FormRow(LOC.Get("ANIM_Range_Length"), LOC.Get("ANIM_Range_Retime_TT"));
            ImGui.InputInt("##animationRangeLength", ref RangeLength);
            ImGui.EndTable();
        }

        RangeFirst = Math.Clamp(RangeFirst, 0, end);
        RangeLast = Math.Clamp(RangeLast, 0, end);
        RangeLength = Math.Clamp(RangeLength, 1, 100000);
        var span = RangeLast - RangeFirst;

        if (span >= 1)
        {
            var speed = (float)span / RangeLength;
            ImGui.TextDisabled(LOC.Get("ANIM_Range_Speed", span, speed.ToString("0.##")));
            if (LargestStep * speed > 180)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                    LOC.Get("ANIM_Op_Speed_Warning", LargestStep.ToString("F0"), (LargestStep * speed).ToString("F0")));
        }

        // Retime: the range over the new length
        if (WideButton(LOC.Get("ANIM_Range_Retime_Button", RangeFirst, RangeLast, RangeLength), "animationRangeRetime",
                span >= 1 && RangeLength != span, LOC.Get("ANIM_Range_Retime_TT")))
        {
            var (first, last, length) = (RangeFirst, RangeLast, RangeLength);
            ApplyRange(source, LOC.Get("ANIM_Action_Range_Retime", source.Name, first, last, length),
                () => EditableClipOperations.RetimeRange(clip, first, last, length));
            (RangeLast, RangeLength) = (first + length, length);
            View.TimelineView.SetRange(first, first + length);
            RangeSeen = View.TimelineView.RangeVersion;
        }

        // Remove the range's frames
        var canRemove = RangeFirst >= 1 && RangeLast <= end - 1 && RangeLast >= RangeFirst;
        if (WideButton(LOC.Get("ANIM_Range_Remove", RangeFirst, RangeLast), "animationRangeRemove", canRemove,
                LOC.Get(canRemove ? "ANIM_Range_Remove_TT" : "ANIM_Range_Remove_Ends_TT")))
        {
            var (first, last) = (RangeFirst, RangeLast);
            ApplyRange(source, LOC.Get("ANIM_Action_Range_Remove", source.Name, first, last),
                () => EditableClipOperations.RemoveFrames(clip, first, last));
            View.TimelineView.ClearRange();
        }

        // Hold the playing frame
        var at = View.ViewportWindow.CurrentWholeFrame;
        if (BeginForm("animationHoldForm"))
        {
            FormRow(LOC.Get("ANIM_Range_Hold_Count"), LOC.Get("ANIM_Range_Hold_TT"));
            ImGui.InputInt("##animationHoldCount", ref HoldCount);
            ImGui.EndTable();
        }

        HoldCount = Math.Clamp(HoldCount, 1, 10000);
        if (WideButton(LOC.Get("ANIM_Range_Hold_Button", at, HoldCount), "animationHoldApply", true, LOC.Get("ANIM_Range_Hold_TT")))
        {
            var count = HoldCount;
            ApplyRange(source, LOC.Get("ANIM_Action_Range_Hold", source.Name, at, count),
                () => EditableClipOperations.HoldFrames(clip, at, count));
        }
    }

    // ---- smoothing (§13.3 item 8)

    private int SmoothRadius = 3;
    private bool SmoothSelectedOnly = true;
    private bool SmoothRoot;

    /// <summary>Smooth: the range's base frames of the selected bones (or every bone) smoothed, keys and layers kept</summary>
    private void DisplaySmooth(AnimationClipSource source, EditableClip clip)
    {
        Group(LOC.Get("ANIM_Smooth_Header"), LOC.Get("ANIM_Smooth_Header_TT"));

        if (BeginForm("animationSmoothForm"))
        {
            FormRow(LOC.Get("ANIM_Smooth_Radius"), LOC.Get("ANIM_Smooth_Radius_TT"));
            ImGui.SliderInt("##animationSmoothRadius", ref SmoothRadius, 1, 15);
            ImGui.EndTable();
        }

        ImGui.Checkbox($"{LOC.Get("ANIM_Smooth_Selected")}##animationSmoothSelected", ref SmoothSelectedOnly);
        GUI.Tooltip(LOC.Get("ANIM_Smooth_Selected_TT"));
        ImGui.BeginDisabled(clip.RootMotion == null);
        ImGui.Checkbox($"{LOC.Get("ANIM_Smooth_Root")}##animationSmoothRoot", ref SmoothRoot);
        ImGui.EndDisabled();
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(LOC.Get("ANIM_Smooth_Root_TT"));

        // The selected bones' tracks (the root motion only through its box)
        List<int>? tracks = null;
        if (SmoothSelectedOnly && View.Bank.Character is { } character)
        {
            var boneToTrack = clip.BoneToTrack(character.Skeleton.BoneCount);
            tracks = View.SelectedBones.Where(b => b >= 0 && b < boneToTrack.Length && boneToTrack[b] >= 0).Select(b => boneToTrack[b]).ToList();
        }

        var nothing = tracks is { Count: 0 } && !(SmoothRoot && clip.RootMotion != null);
        if (WideButton(LOC.Get("ANIM_Smooth_Apply", RangeFirst, RangeLast), "animationSmoothApply", !nothing && RangeLast >= RangeFirst,
                LOC.Get(nothing ? "ANIM_Smooth_Nothing_TT" : "ANIM_Smooth_Apply_TT")))
        {
            var (first, last, radius, root) = (RangeFirst, RangeLast, SmoothRadius, SmoothRoot && clip.RootMotion != null);
            var name = LOC.Get("ANIM_Action_Smooth", source.Name, first, last, tracks?.Count.ToString() ?? LOC.Get("ANIM_Smooth_All"));
            try
            {
                var result = EditableClipOperations.Smooth(clip, tracks, first, last, radius, root);
                View.Execute(new AnimationClipEditAction(View.Bank, source, result.Clip, name, OnClipSelected));
                Smithbox.Log(this, LOC.Get("ANIM_Smooth_Done", name, result.LargestTurn.ToString("0.##"), result.LargestMove.ToString("0.####")));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("ANIM_Op_Failed", name, e.Message), e);
            }
        }
    }

    /// <summary>Runs a range operation through <see cref="Apply" />, with its event mapping, and logs keys it merged or dropped</summary>
    private void ApplyRange(AnimationClipSource source, string name, Func<EditableClipOperations.RangeResult> operation)
    {
        EditableClipOperations.RangeResult? result = null;
        Apply(source, name, () => (result = operation()).Clip, (_, _) => result!.Events);

        if (result is { MergedKeys: > 0 } or { DroppedKeys: > 0 })
            Smithbox.Log(this, LOC.Get("ANIM_Range_Keys_Lost", name, result.MergedKeys, result.DroppedKeys));
    }

    // ---- time act events (W1)

    /// <summary>
    /// After a trim, speed or frame count change: offers to retime the clip's time act events to match. The Time Act
    /// Editor owns those, so it is sent there as a command (<c>tae/retime</c>) and becomes one undoable step of its
    /// history, saved with its time acts.
    /// </summary>
    private void DisplayEventRetime(AnimationClipSource source, AnimationCharacter character)
    {
        if (RetimeSent == source)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("ANIM_Retime_Events_Sent"));
            return;
        }

        if (PendingRetime is not { } pending || pending.Source != source)
            return;

        // Undone here, or edited some other way since: the mapping no longer says what happened
        if (View.Bank.Get(source) != pending.After)
        {
            PendingRetime = null;
            return;
        }

        if (pending.Map.IsIdentity)
            return;

        GUI.Spacer();
        var clip = pending.After;
        if (pending.Map.IsAffine(out var scale, out var offset))
        {
            var shift = clip.FrameDuration > 0 ? -offset / clip.FrameDuration : 0;
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                LOC.Get("ANIM_Retime_Events_Offer", scale.ToString("0.###"), shift.ToString("0.#")));
        }
        else
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ANIM_Retime_Events_Offer_Map"));
            GUI.Tooltip(LOC.Get("ANIM_Retime_Events_Offer_Map_TT", pending.Map.Describe(clip.FrameDuration)));
        }

        var acts = character.TimeActs;
        string? reason = null;
        if (acts?.For(source.AnimationId) == null)
            reason = LOC.Get("ANIM_Retime_Events_No_Entry");
        else if (acts.ImportedFrom(source.AnimationId) is { } from)
            reason = LOC.Get("ANIM_Retime_Events_Imported", character.Clips.Find(c => c.AnimationId == from)?.Name ?? from.ToString());

        ImGui.BeginDisabled(reason != null);
        if (ImGui.Button($"{LOC.Get("ANIM_Retime_Events_Apply")}##animationRetimeEvents"))
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            EditorCommandQueue.AddCommand($"tae/retimemap/{character.SetName}/{source.AnimationId}/" +
                $"{pending.Map.ToCommand()}/" +
                $"{clip.Duration.ToString("R", culture)}/{clip.FrameDuration.ToString("R", culture)}");

            PendingRetime = null;
            RetimeSent = source;
        }
        ImGui.EndDisabled();
        GUI.Tooltip(reason ?? LOC.Get("ANIM_Retime_Events_Apply_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("ANIM_Retime_Events_Dismiss")}##animationRetimeEventsDismiss"))
            PendingRetime = null;
        GUI.Tooltip(LOC.Get("ANIM_Retime_Events_Dismiss_TT"));

        if (reason != null)
            ImGui.TextDisabled(reason);
    }

    // ---- new ID

    private void DisplayNewId(AnimationClipSource source, EditableClip clip, AnimationCharacter character)
    {
        Group(LOC.Get("ANIM_New_Id_Header"), LOC.Get("ANIM_New_Id_Header_TT"));

        if (BeginForm("animationNewIdForm"))
        {
            FormRow(LOC.Get("ANIM_New_Id_Name"), LOC.Get("ANIM_New_Id_Create_TT"));
            ImGui.InputText("##animationNewName", ref NewName, 32);
            ImGui.EndTable();
        }

        if (WideButton(LOC.Get("ANIM_New_Id_Create_Button", NewName), "animationNewCreate", NewName.Trim().Length > 0, LOC.Get("ANIM_New_Id_Create_TT")))
        {
            var created = View.Bank.CreateClip(source, NewName, out NewNameError);
            if (created != null)
            {
                View.Execute(new AnimationAddClipAction(View.Bank, created, clip.Clone(),
                    LOC.Get("ANIM_Action_New_Id", created.Name, source.Name)));
                View.SelectClip(created);
                return;
            }
        }

        if (NewNameError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, NewNameError);

        // Links to the other editors: behavior clip generators for the clip's ID (the Behavior Editor owns that file),
        // the clip's time act entry
        Group(LOC.Get("ANIM_Links_Header"), LOC.Get("ANIM_Links_Header_TT"));

        var canRegister = CanRegister(character.Name, out var reason);
        if (WideButton(LOC.Get("ANIM_Register_Behavior"), "animationRegister", canRegister,
                canRegister ? LOC.Get("ANIM_Register_Behavior_TT") : reason ?? ""))
            EditorCommandQueue.AddCommand($"behavior/register/{character.Name}/{source.AnimationId}");

        AnimationClipListView.OpenInTimeActEditor(View, source, menu: false);

        DisplayTimeActEntry(source, character);
    }

    // The new clip a time act entry was last asked for, so the button says so until the time acts are read again
    private AnimationClipSource? EntrySent;

    /// <summary>
    /// For a clip under a new ID without a time act entry: Make Time Act Entry asks the Time Act Editor (which owns the
    /// time acts) to make one from the template clip's entry (<c>tae/copyanim</c>), as one undoable step there.
    /// </summary>
    private void DisplayTimeActEntry(AnimationClipSource source, AnimationCharacter character)
    {
        if (character.TimeActs is not { } acts || acts.For(source.AnimationId) != null)
            return;

        if (!source.IsNew || source.Template is not { } template)
        {
            ImGui.TextDisabled(LOC.Get("ANIM_New_Id_Time_Act_Note"));
            return;
        }

        if (EntrySent == source)
        {
            ImGui.TextDisabled(LOC.Get("ANIM_Time_Act_Entry_Sent"));
            return;
        }

        var hasTemplateEntry = acts.For(template.AnimationId) != null;
        if (WideButton(LOC.Get("ANIM_Time_Act_Entry_Make"), "animationMakeTimeActEntry", hasTemplateEntry, hasTemplateEntry
                ? LOC.Get("ANIM_Time_Act_Entry_Make_TT", template.Name)
                : LOC.Get("ANIM_Time_Act_Entry_No_Template", template.Name)))
        {
            EditorCommandQueue.AddCommand($"tae/copyanim/{character.SetName}/{template.AnimationId}/{source.AnimationId}");
            EntrySent = source;
        }
    }

    /// <summary>
    /// Whether the Behavior Editor can register the character's animations: the same conditions as the Time Act
    /// Editor's Register in Behavior (the surveyed games, a behavior binder for the character)
    /// </summary>
    private bool CanRegister(string character, out string? reason)
    {
        reason = null;
        var project = View.Project;

        if (!project.Handler.UsesHavokBehaviorEditor() || project.Handler.BehaviorEditor?.HavokBehaviors == null)
        {
            reason = LOC.Get("TAE_Register_No_Behavior_Editor");
            return false;
        }

        if (project.Descriptor.ProjectType is not (ProjectType.ER or ProjectType.NR or ProjectType.DS3 or ProjectType.SDT))
        {
            reason = LOC.Get("TAE_Register_Unsupported_Game");
            return false;
        }

        if (!project.Locator.HavokBehaviorFiles.Entries.Any(e => e.Filename.Equals(character, StringComparison.OrdinalIgnoreCase)))
        {
            reason = LOC.Get("TAE_Register_No_Behavior", character);
            return false;
        }

        return true;
    }
}
