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
        DisplayNewId(source, clip, character);
    }

    // ---- details

    private void DisplayInfo(AnimationClipSource source, EditableClip clip)
    {
        GUI.SimpleHeader(source.Name, LOC.Get("ANIM_Clip_Header_TT"));

        var state = source.IsNew ? LOC.Get("ANIM_Clip_State_New")
            : View.Bank.IsEdited(source) ? LOC.Get("ANIM_Clip_State_Edited")
            : LOC.Get("ANIM_Clip_State_Saved");
        ImGui.TextDisabled(state);

        var fps = clip.FrameDuration > 0 ? 1 / clip.FrameDuration : 0;
        ImGui.Text(LOC.Get("ANIM_Clip_Info_Frames", clip.FrameCount, clip.Duration.ToString("F2"), fps.ToString("F0")));
        ImGui.Text(LOC.Get("ANIM_Clip_Info_Tracks", clip.TrackCount, View.Bank.Character!.Skeleton.BoneCount));
        ImGui.Text(clip.RootMotion != null ? LOC.Get("ANIM_Clip_Info_Root_Motion") : LOC.Get("ANIM_Clip_Info_No_Root_Motion"));
        if (clip.BlendMode != HavokBlendMode.Normal)
            ImGui.Text(LOC.Get("ANIM_Clip_Info_Additive"));
        if (clip.HasOffsets)
            ImGui.Text(LOC.Get("ANIM_Clip_Info_Offsets", clip.Offsets.Sum(o => o.Value.Count), clip.Offsets.Count));

        ImGui.TextDisabled(source.BinderPath);
        GUI.Spacer();
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
            Apply(source, LOC.Get("ANIM_Action_Trim", source.Name, TrimFirst, TrimLast),
                () => EditableClipOperations.Trim(clip, TrimFirst, TrimLast));
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
                () => EditableClipOperations.Speed(clip, Speed));
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
                () => EditableClipOperations.Retime(clip, FrameCount));
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

    private void Apply(AnimationClipSource source, string name, Func<EditableClip> operation)
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

        View.Execute(new AnimationClipEditAction(View.Bank, source, result, name, OnClipSelected));
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

        ImGui.TextDisabled(LOC.Get("ANIM_New_Id_Time_Act_Note"));
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
