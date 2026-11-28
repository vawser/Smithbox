#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Linq;
using System.Numerics;
using static StudioCore.Editors.AnimationEditor.AnimationToolLayout;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Transition tool (Tools → Animation, §13.3 item 12): plays the end of another clip of the character crossfading
/// into the selected clip, as the game blends one into the next, so a pop at the clip's start shows before it is seen
/// in game. Preview only: nothing is written. While it plays, the Timeline shows the whole of it (the lead-in marked),
/// the skeleton overlay and the bone gizmo are hidden, and anything keyed lands on the selected clip's own frames. It
/// also measures the jump from the lead-in's last frame to the clip's first, bone by bone.
/// </summary>
public class AnimationTransitionTool
{
    private readonly AnimationEditorView View;

    private AnimationClipSource? LeadClip;
    private string LeadFilter = "";
    private int LeadFrames = 30;
    private int BlendFrames = 5;
    private bool Previewing;

    // The preview, made again when the clip, the lead-in or the numbers change
    private (EditableClip Edited, EditableClip? Lead, int Frames, int Blend)? MadeFor;
    private (EditableClip Clip, int LeadIn)? Made;
    private string? Problem;

    // The jump at the clip's start, worked out once per clip and lead-in
    private (EditableClip Edited, EditableClip Lead)? JumpFor;
    private (float Degrees, string Bone, float Move, string MoveBone)? Jump;

    public AnimationTransitionTool(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>Whether the viewport is playing the transition preview</summary>
    public bool IsPreviewing => Previewing && Made != null && View.ViewportWindow.LeadIn > 0;

    /// <summary>As a tool of the Tools window</summary>
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Transition")}##animationTransitionToolHeader"))
            return;

        ImGui.BeginChild("AnimationTransitionToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        if (View.Bank.Character is not { } character || View.SelectedClip is not { } source || View.Bank.Get(source) is not { } clip)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clip_None"));
            return;
        }

        // A lead-in picked for another character is dropped
        if (LeadClip != null && !character.Clips.Contains(LeadClip))
        {
            LeadClip = null;
            Previewing = false;
        }

        GUI.SimpleHeader(LOC.Get("ANIM_Transition_Header", source.Name), LOC.Get("ANIM_Transition_Header_TT"));

        if (BeginForm("animationTransitionForm"))
        {
            FormRow(LOC.Get("ANIM_Transition_Lead"), LOC.Get("ANIM_Transition_Lead_TT"));
            LeadPicker(source, character);
            FormRow(LOC.Get("ANIM_Transition_Lead_Frames"), LOC.Get("ANIM_Transition_Lead_Frames_TT"));
            ImGui.InputInt("##animationTransitionLeadFrames", ref LeadFrames, 5);
            FormRow(LOC.Get("ANIM_Op_Splice_Blend"), LOC.Get("ANIM_Transition_Blend_TT"));
            ImGui.InputInt("##animationTransitionBlend", ref BlendFrames, 1);
            ImGui.EndTable();
        }

        LeadFrames = Math.Clamp(LeadFrames, 2, 600);
        BlendFrames = Math.Clamp(BlendFrames, 0, Math.Min(LeadFrames - 1, 60));

        var lead = LeadClip != null ? View.Bank.Get(LeadClip) : null;
        if (lead != null)
            Build(clip, lead, character.Skeleton);

        if (Problem != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, Problem);

        // The jump from the lead-in's last frame to the clip's first, without the crossfade
        if (lead != null && Problem == null)
        {
            if (JumpFor is not { } jf || jf.Edited != clip || jf.Lead != lead)
            {
                Jump = Measure(lead.Baked(), clip.Baked(), character.Skeleton);
                JumpFor = (clip, lead);
            }

            if (Jump is { } jump)
                ImGui.TextDisabled(LOC.Get("ANIM_Transition_Jump", jump.Degrees.ToString("0.#"), jump.Bone, jump.Move.ToString("0.###"), jump.MoveBone));
            GUI.Tooltip(LOC.Get("ANIM_Transition_Jump_TT"));
        }

        var can = lead != null && Problem == null;
        var label = Previewing ? LOC.Get("ANIM_Transition_Stop") : LOC.Get("ANIM_Transition_Play", LeadClip?.Name ?? "-");
        if (WideButton(label, "animationTransitionToggle", Previewing || can, LOC.Get("ANIM_Transition_Play_TT")))
        {
            Previewing = !Previewing;
            if (Previewing)
                View.ViewportWindow.SeekPlayingFrame(0);
        }

        if (IsPreviewing && Made is { } made)
            GUI.WrappedTextColored(ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled],
                LOC.Get("ANIM_Transition_Playing", made.LeadIn - 1, made.LeadIn - BlendFrames, made.LeadIn + BlendFrames - 1));
    }

    /// <summary>
    /// The preview for the viewport (null when it isn't on, or can't be made): the lead-in's last frames, crossfading
    /// into the clip, and the frame the clip's own frame 0 is at
    /// </summary>
    public (EditableClip Clip, int LeadIn)? Preview(EditableClip clip, HavokSkeletonData skeleton)
    {
        // A lead-in picked for another character doesn't count
        if (!Previewing || LeadClip == null || View.Bank.Character?.Clips.Contains(LeadClip) != true ||
            View.Bank.Get(LeadClip) is not { } lead)
            return null;

        Build(clip, lead, skeleton);
        return Made;
    }

    private void Build(EditableClip clip, EditableClip lead, HavokSkeletonData skeleton)
    {
        if (MadeFor is { } f && f.Edited == clip && f.Lead == lead && f.Frames == LeadFrames && f.Blend == BlendFrames)
            return;

        MadeFor = (clip, lead, LeadFrames, BlendFrames);
        Made = null;
        Problem = Mismatch(lead, clip);
        if (Problem != null)
            return;

        try
        {
            var made = Compose(lead.Baked(), clip.Baked(), LeadFrames, BlendFrames);
            Made = made;
        }
        catch (Exception e)
        {
            Problem = e.Message;
        }
    }

    /// <summary>
    /// The last <paramref name="leadFrames" /> frames of <paramref name="lead" /> (all of it if shorter) crossfading into
    /// <paramref name="clip" /> over <paramref name="blend" /> frames (<see cref="EditableClipOperations.Splice" />, the
    /// root motion carried on), and the frame the clip's frame 0 is at
    /// </summary>
    public static (EditableClip Clip, int LeadIn) Compose(EditableClip lead, EditableClip clip, int leadFrames, int blend)
    {
        var last = lead.FrameCount - 1;
        var first = Math.Max(0, last - Math.Max(leadFrames, 2) + 1);
        if (last - first < 1)
            first = Math.Max(0, last - 1);

        var tail = EditableClipOperations.Trim(lead, first, last);
        blend = Math.Clamp(blend, 0, Math.Min(tail.FrameCount - 1, clip.FrameCount - 1));
        var spliced = EditableClipOperations.Splice(tail, clip, blend);
        return (spliced, tail.FrameCount - blend);
    }

    /// <summary>
    /// The largest jump from the lead-in's last frame to the clip's first: the bone that turns most (degrees, model
    /// space) and the one that moves most (units)
    /// </summary>
    public static (float Degrees, string Bone, float Move, string MoveBone)? Measure(EditableClip lead, EditableClip clip, HavokSkeletonData skeleton)
    {
        if (lead.FrameCount == 0 || clip.FrameCount == 0)
            return null;

        var a = lead.ModelPose(skeleton, lead.FrameCount - 1);
        var b = clip.ModelPose(skeleton, 0);
        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);
        var (turn, turnBone, move, moveBone) = (0f, "-", 0f, "-");
        for (var i = 0; i < skeleton.BoneCount && i < a.Length && i < b.Length; i++)
        {
            if (boneToTrack[i] < 0)
                continue;

            var dot = MathF.Min(MathF.Abs(Quaternion.Dot(Quaternion.Normalize(a[i].Rotation), Quaternion.Normalize(b[i].Rotation))), 1);
            var degrees = 2 * MathF.Acos(dot) * 180 / MathF.PI;
            if (degrees > turn)
                (turn, turnBone) = (degrees, skeleton.Names[i]);

            var d = (a[i].Position - b[i].Position).Length();
            if (d > move)
                (move, moveBone) = (d, skeleton.Names[i]);
        }

        return (turn, turnBone, move, moveBone);
    }

    private static string? Mismatch(EditableClip lead, EditableClip clip)
    {
        if (lead.TrackCount != clip.TrackCount || !lead.TrackToBone.SequenceEqual(clip.TrackToBone))
            return LOC.Get("ANIM_Op_Splice_Different_Bones");
        if (lead.FloatTrackCount != clip.FloatTrackCount)
            return LOC.Get("ANIM_Op_Splice_Different_Floats");
        if (lead.BlendMode != clip.BlendMode)
            return LOC.Get("ANIM_Op_Splice_Different_Additive");
        return null;
    }

    private void LeadPicker(AnimationClipSource source, AnimationCharacter character)
    {
        if (!ImGui.BeginCombo("##animationTransitionLead", LeadClip?.Name ?? LOC.Get("ANIM_Transition_Pick"), ImGuiComboFlags.HeightLarge))
            return;

        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##animationTransitionLeadFilter", LOC.Get("ANIM_Filter_Hint"), ref LeadFilter, 64);

        var shown = 0;
        foreach (var other in character.Clips)
        {
            if (other == source || LeadFilter.Length > 0 && !other.Name.Contains(LeadFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            if (++shown > 200)
            {
                ImGui.TextDisabled(LOC.Get("ANIM_Op_Splice_More"));
                break;
            }

            if (ImGui.Selectable($"{other.Name}##animationTransitionLead_{other.BinderId}", LeadClip == other))
                LeadClip = other;
        }

        ImGui.EndCombo();
    }
}
