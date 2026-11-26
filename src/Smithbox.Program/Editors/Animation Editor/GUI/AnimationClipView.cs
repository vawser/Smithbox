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

    /// <summary>
    /// A change of a clip's timing (trim, speed, frame count) not yet passed on to its time act events: an event time t
    /// becomes t × Scale + Offset (seconds). Later time changes of the same clip are folded in, so one step there
    /// matches every step here. It only holds while the clip is still <see cref="After" /> (an undo here drops it).
    /// </summary>
    private sealed record EventRetime(AnimationClipSource Source, EditableClip After, float Scale, float Offset);

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
        DisplayOperations(source, clip.Baked(), character);
        DisplayEventRetime(source, character);
        DisplayNewId(source, clip, character);
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
            ImGui.Text(LOC.Get("ANIM_Clip_Info_Offsets", clip.Offsets.Sum(o => o.Value.Count), clip.Offsets.Count));

        ImGui.TextDisabled(source.BinderPath);
        GUI.Spacer();
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

    private void DisplayOperations(AnimationClipSource source, EditableClip clip, AnimationCharacter character)
    {
        GUI.SimpleHeader(LOC.Get("ANIM_Operations_Header"), LOC.Get("ANIM_Operations_Header_TT"));

        // Trim
        ImGui.Text(LOC.Get("ANIM_Op_Trim"));
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.3f);
        ImGui.InputInt($"{LOC.Get("ANIM_Op_Trim_First")}##animationTrimFirst", ref TrimFirst);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.45f);
        ImGui.InputInt($"{LOC.Get("ANIM_Op_Trim_Last")}##animationTrimLast", ref TrimLast);
        TrimFirst = Math.Clamp(TrimFirst, 0, clip.FrameCount - 1);
        TrimLast = Math.Clamp(TrimLast, 0, clip.FrameCount - 1);
        ImGui.SameLine();
        ImGui.BeginDisabled(TrimLast - TrimFirst < 1 || TrimFirst == 0 && TrimLast == clip.FrameCount - 1);
        if (ImGui.Button($"{LOC.Get("ANIM_Op_Apply")}##animationTrimApply"))
        {
            // Taken now: applying selects the clip again, which resets the fields
            var trimOffset = -TrimFirst * clip.FrameDuration;
            Apply(source, LOC.Get("ANIM_Action_Trim", source.Name, TrimFirst, TrimLast),
                () => EditableClipOperations.Trim(clip, TrimFirst, TrimLast), (_, _) => (1, trimOffset));
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Op_Trim_TT"));

        // Speed
        ImGui.Text(LOC.Get("ANIM_Op_Speed"));
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
        ImGui.InputFloat("##animationSpeedValue", ref Speed, 0.1f, 0.5f, "%.2f");
        Speed = Math.Clamp(Speed, 0.05f, 20f);
        ImGui.SameLine();
        ImGui.BeginDisabled(Math.Abs(Speed - 1) < 1e-4f);
        if (ImGui.Button($"{LOC.Get("ANIM_Op_Apply")}##animationSpeedApply"))
            Apply(source, LOC.Get("ANIM_Action_Speed", source.Name, Speed.ToString("F2")),
                () => EditableClipOperations.Speed(clip, Speed), StretchTiming);
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Op_Speed_TT"));

        if (LargestStep * Speed > 180)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                LOC.Get("ANIM_Op_Speed_Warning", LargestStep.ToString("F0"), (LargestStep * Speed).ToString("F0")));

        // Frame count
        ImGui.Text(LOC.Get("ANIM_Op_Frames"));
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
        ImGui.InputInt("##animationFrameCount", ref FrameCount);
        FrameCount = Math.Clamp(FrameCount, 2, 100000);
        ImGui.SameLine();
        ImGui.BeginDisabled(FrameCount == clip.FrameCount);
        if (ImGui.Button($"{LOC.Get("ANIM_Op_Apply")}##animationFramesApply"))
            Apply(source, LOC.Get("ANIM_Action_Frames", source.Name, FrameCount),
                () => EditableClipOperations.Retime(clip, FrameCount), StretchTiming);
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Op_Frames_TT"));

        GUI.Spacer();

        // Reverse, mirror
        if (ImGui.Button($"{LOC.Get("ANIM_Op_Reverse")}##animationReverse"))
            Apply(source, LOC.Get("ANIM_Action_Reverse", source.Name), () => EditableClipOperations.Reverse(clip));
        GUI.Tooltip(LOC.Get("ANIM_Op_Reverse_TT"));

        ImGui.SameLine();
        ImGui.BeginDisabled(clip.BlendMode != HavokBlendMode.Normal);
        if (ImGui.Button($"{LOC.Get("ANIM_Op_Mirror")}##animationMirror"))
            Apply(source, LOC.Get("ANIM_Action_Mirror", source.Name), () => EditableClipOperations.Mirror(clip, character.Skeleton));
        ImGui.EndDisabled();
        GUI.Tooltip(clip.BlendMode != HavokBlendMode.Normal ? LOC.Get("ANIM_Op_Mirror_Additive") : LOC.Get("ANIM_Op_Mirror_TT"));

        if (Unpaired.Count > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ANIM_Op_Mirror_Unpaired", Unpaired.Count));
            GUI.Tooltip(string.Join("\n", Unpaired));
        }

        GUI.Spacer();

        // Splice
        ImGui.Text(LOC.Get("ANIM_Op_Splice"));
        DisplaySplicePicker(source, character);
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
        ImGui.InputInt($"{LOC.Get("ANIM_Op_Splice_Blend")}##animationSpliceBlend", ref BlendFrames);
        BlendFrames = Math.Clamp(BlendFrames, 0, 1000);
        ImGui.SameLine();

        var second = SpliceWith != null ? View.Bank.Get(SpliceWith)?.Baked() : null;
        var spliceProblem = SpliceProblem(clip, second);
        ImGui.BeginDisabled(second == null || spliceProblem != null);
        if (ImGui.Button($"{LOC.Get("ANIM_Op_Apply")}##animationSpliceApply") && second != null)
            Apply(source, LOC.Get("ANIM_Action_Splice", source.Name, SpliceWith!.Name, BlendFrames),
                () => EditableClipOperations.Splice(clip, second, BlendFrames));
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Op_Splice_TT"));

        if (spliceProblem != null && second != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, spliceProblem);

        GUI.Spacer();

        // Revert
        ImGui.BeginDisabled(!View.Bank.IsEdited(source) || source.IsNew);
        if (ImGui.Button($"{LOC.Get("ANIM_Op_Revert")}##animationRevert"))
            View.Execute(new AnimationClipEditAction(View.Bank, source, null, LOC.Get("ANIM_Action_Revert", source.Name), OnClipSelected));
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Op_Revert_TT"));

        GUI.Spacer();
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
        Func<EditableClip, EditableClip, (float Scale, float Offset)>? timing = null)
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

        var (scale, offset) = timing(before, result);

        // Folded into the time change still waiting for this clip: t → (t × s1 + o1) × s2 + o2
        if (PendingRetime is { } pending && pending.Source == source && pending.After == before)
            PendingRetime = new EventRetime(source, after, pending.Scale * scale, pending.Offset * scale + offset);
        else
            PendingRetime = new EventRetime(source, after, scale, offset);

        RetimeSent = null;
    }

    /// <summary>Whether a clip has a time change not yet passed on to its time act events (for the check before Export)</summary>
    public bool HasPendingRetime(AnimationClipSource source)
    {
        return PendingRetime is { } pending && pending.Source == source && View.Bank.Get(source) == pending.After &&
               (System.Math.Abs(pending.Scale - 1) >= 1e-5f || System.Math.Abs(pending.Offset) >= 1e-6f);
    }

    /// <summary>Speed and frame count stretch the clip: event times scale with its length</summary>
    private static (float Scale, float Offset) StretchTiming(EditableClip before, EditableClip after)
    {
        return (before.Duration > 0 ? after.Duration / before.Duration : 1, 0);
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
            GUI.WrappedText(LOC.Get("ANIM_Retime_Events_Sent"));
            GUI.Spacer();
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

        if (Math.Abs(pending.Scale - 1) < 1e-5f && Math.Abs(pending.Offset) < 1e-6f)
            return;

        var clip = pending.After;
        var shift = clip.FrameDuration > 0 ? -pending.Offset / clip.FrameDuration : 0;
        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
            LOC.Get("ANIM_Retime_Events_Offer", pending.Scale.ToString("0.###"), shift.ToString("0.#")));

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
            EditorCommandQueue.AddCommand($"tae/retime/{character.SetName}/{source.AnimationId}/" +
                $"{pending.Scale.ToString("R", culture)}/{pending.Offset.ToString("R", culture)}/" +
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

        GUI.Spacer();
    }

    // ---- new ID

    private void DisplayNewId(AnimationClipSource source, EditableClip clip, AnimationCharacter character)
    {
        GUI.SimpleHeader(LOC.Get("ANIM_New_Id_Header"), LOC.Get("ANIM_New_Id_Header_TT"));

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.6f);
        ImGui.InputText("##animationNewName", ref NewName, 32);
        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("ANIM_New_Id_Create")}##animationNewCreate"))
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
        GUI.Tooltip(LOC.Get("ANIM_New_Id_Create_TT"));

        if (NewNameError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, NewNameError);

        // Behavior clip generators for the selected clip's ID, made by the Behavior Editor (which owns that file)
        var canRegister = CanRegister(character.Name, out var reason);
        ImGui.BeginDisabled(!canRegister);
        if (ImGui.Button($"{LOC.Get("ANIM_Register_Behavior")}##animationRegister"))
            EditorCommandQueue.AddCommand($"behavior/register/{character.Name}/{source.AnimationId}");
        ImGui.EndDisabled();
        GUI.Tooltip(canRegister ? LOC.Get("ANIM_Register_Behavior_TT") : reason ?? "");

        ImGui.SameLine();
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
        ImGui.BeginDisabled(!hasTemplateEntry);
        if (ImGui.Button($"{LOC.Get("ANIM_Time_Act_Entry_Make")}##animationMakeTimeActEntry"))
        {
            EditorCommandQueue.AddCommand($"tae/copyanim/{character.SetName}/{template.AnimationId}/{source.AnimationId}");
            EntrySent = source;
        }
        ImGui.EndDisabled();
        GUI.Tooltip(hasTemplateEntry
            ? LOC.Get("ANIM_Time_Act_Entry_Make_TT", template.Name)
            : LOC.Get("ANIM_Time_Act_Entry_No_Template", template.Name));
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
