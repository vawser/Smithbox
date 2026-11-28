#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;
using static StudioCore.Editors.AnimationEditor.AnimationToolLayout;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Constraints tool (Tools → Transform, §13.3 item 11, D14): Child-of and Aim on the selected bone over a frame
/// range, each baked to a new layer named after it (<see cref="ClipConstraints" />), so the Layers tool mutes, weights
/// or removes it. One undo step each.
/// </summary>
public class AnimationConstraintsTool
{
    private readonly AnimationEditorView View;

    // The range, set again for another clip (to the Timeline's range when one is picked)
    private int First;
    private int Last;
    private int Blend = 3;
    private AnimationClipSource? RangeFor;
    private int RangeSeen = -1;

    // Child-of
    private int FollowBone = -1;
    private int Reference;
    private bool KeepRotation = true;

    // Aim
    private int TargetBone = -1;
    private Vector3 TargetOffset;

    private string BoneFilter = "";

    public AnimationConstraintsTool(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>As a tool of the Tools window</summary>
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Constraints")}##animationConstraintsToolHeader"))
            return;

        ImGui.BeginChild("AnimationConstraintsToolSection", ImGuiChildFlags.Borders);
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

        var skeleton = character.Skeleton;
        var bone = View.SelectedBone;
        if (bone < 0 || bone >= skeleton.BoneCount || clip.BoneToTrack(skeleton.BoneCount)[bone] < 0)
        {
            GUI.WrappedText(LOC.Get("ANIM_Constraints_No_Bone"));
            return;
        }

        var end = clip.FrameCount - 1;
        var frame = View.ViewportWindow.CurrentWholeFrame;
        var timeline = View.TimelineView;
        if (!ReferenceEquals(RangeFor, source) || timeline.RangeVersion != RangeSeen)
        {
            if (!ReferenceEquals(RangeFor, source))
                (First, Last, Reference) = (0, end, 0);
            if (timeline.Range is { } picked && picked.Last <= end)
                (First, Last, Reference) = (picked.First, picked.Last, picked.First);
            RangeFor = source;
            RangeSeen = timeline.RangeVersion;
        }

        GUI.SimpleHeader(skeleton.Names[bone], LOC.Get("ANIM_Constraints_Header_TT"));

        // The range
        Group(LOC.Get("ANIM_Constraints_Range"), LOC.Get("ANIM_Constraints_Range_TT"));
        if (BeginForm("animationConstraintRange"))
        {
            FormRow(LOC.Get("ANIM_Ik_Plant_From"), LOC.Get("ANIM_Constraints_Range_TT"));
            if (FrameWithHere("animationConstraintFirst", ref First, frame))
                First = frame;
            FormRow(LOC.Get("ANIM_Ik_Plant_To"), LOC.Get("ANIM_Constraints_Range_TT"));
            if (FrameWithHere("animationConstraintLast", ref Last, frame))
                Last = frame;
            FormRow(LOC.Get("ANIM_Ik_Plant_Blend"), LOC.Get("ANIM_Constraints_Blend_TT"));
            ImGui.InputInt("##animationConstraintBlend", ref Blend, 1);
            ImGui.EndTable();
        }

        First = Math.Clamp(First, 0, end);
        Last = Math.Clamp(Last, 0, end);
        Blend = Math.Clamp(Blend, 0, 30);
        var rangeOk = Last >= First;

        // Child-of
        Group(LOC.Get("ANIM_Constraints_Child_Of"), LOC.Get("ANIM_Constraints_Child_Of_TT"));
        if (BeginForm("animationConstraintChildOf"))
        {
            FormRow(LOC.Get("ANIM_Constraints_Follow"), LOC.Get("ANIM_Constraints_Follow_TT"));
            BoneCombo("animationConstraintFollow", skeleton, ref FollowBone, allowPoint: false, exclude: bone);
            FormRow(LOC.Get("ANIM_Constraints_Reference"), LOC.Get("ANIM_Constraints_Reference_TT"));
            if (FrameWithHere("animationConstraintReference", ref Reference, frame))
                Reference = frame;
            ImGui.EndTable();
        }
        Reference = Math.Clamp(Reference, 0, end);

        ImGui.Checkbox($"{LOC.Get("ANIM_Constraints_Keep_Rotation")}##animationConstraintKeepRotation", ref KeepRotation);
        GUI.Tooltip(LOC.Get("ANIM_Constraints_Keep_Rotation_TT"));

        var limb = LimbIk.ChainOf(clip, skeleton, bone);
        ImGui.TextDisabled(limb is { } chain
            ? LOC.Get("ANIM_Constraints_Limb", skeleton.Names[chain.Upper], skeleton.Names[chain.Lower])
            : LOC.Get("ANIM_Constraints_Not_Limb"));

        var follow = FollowBone >= 0 && FollowBone < skeleton.BoneCount ? skeleton.Names[FollowBone] : "-";
        if (WideButton(LOC.Get("ANIM_Constraints_Child_Of_Button", skeleton.Names[bone], follow, First, Last), "animationConstraintChildOfApply",
                rangeOk && FollowBone >= 0 && FollowBone != bone, LOC.Get("ANIM_Constraints_Child_Of_TT")))
        {
            var name = LOC.Get("ANIM_Constraints_Child_Of_Layer", skeleton.Names[bone], follow, First, Last);
            Run(source, name, () => ClipConstraints.ChildOf(clip, skeleton, bone, FollowBone, Reference, First, Last, Blend, KeepRotation, name));
        }

        // Aim
        Group(LOC.Get("ANIM_Constraints_Aim"), LOC.Get("ANIM_Constraints_Aim_TT"));
        if (BeginForm("animationConstraintAim"))
        {
            FormRow(LOC.Get("ANIM_Constraints_Target"), LOC.Get("ANIM_Constraints_Target_TT"));
            BoneCombo("animationConstraintTarget", skeleton, ref TargetBone, allowPoint: true, exclude: bone);
            FormRow(LOC.Get("ANIM_Constraints_Offset"), LOC.Get("ANIM_Constraints_Offset_TT"));
            ImGui.InputFloat3("##animationConstraintOffset", ref TargetOffset, "%.3f");
            GUI.Tooltip(LOC.Get("ANIM_Constraints_Offset_TT"));
            ImGui.EndTable();
        }

        var axis = ClipConstraints.AimAxis(clip, skeleton, bone, TargetBone, TargetOffset, First);
        ImGui.TextDisabled(LOC.Get("ANIM_Constraints_Aim_Axis", AxisName(axis)));

        var target = TargetBone >= 0 && TargetBone < skeleton.BoneCount ? skeleton.Names[TargetBone] : LOC.Get("ANIM_Constraints_Point");
        var aimable = TargetBone >= 0 || TargetOffset != Vector3.Zero;
        if (WideButton(LOC.Get("ANIM_Constraints_Aim_Button", skeleton.Names[bone], target, First, Last), "animationConstraintAimApply",
                rangeOk && aimable, LOC.Get("ANIM_Constraints_Aim_TT")))
        {
            var name = LOC.Get("ANIM_Constraints_Aim_Layer", skeleton.Names[bone], target, First, Last);
            Run(source, name, () => ClipConstraints.Aim(clip, skeleton, bone, TargetBone, TargetOffset, First, Last, Blend, name));
        }
    }

    private void Run(AnimationClipSource source, string name, Func<ClipConstraints.Result> constraint)
    {
        try
        {
            var result = constraint();
            View.Execute(new AnimationClipEditAction(View.Bank, source, result.Clip, LOC.Get("ANIM_Action_Constraint", name, source.Name)));
            Smithbox.Log(this, LOC.Get("ANIM_Constraints_Done", name, result.Frames));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("ANIM_Op_Failed", name, e.Message), e);
        }
    }

    /// <summary>A bone picker with a filter (and, for Aim, "a point" for none)</summary>
    private void BoneCombo(string id, HavokSkeletonData skeleton, ref int chosen, bool allowPoint, int exclude)
    {
        var label = chosen >= 0 && chosen < skeleton.BoneCount ? skeleton.Names[chosen]
            : allowPoint ? LOC.Get("ANIM_Constraints_Point") : LOC.Get("ANIM_Constraints_Pick_Bone");
        if (!ImGui.BeginCombo($"##{id}", label, ImGuiComboFlags.HeightLarge))
            return;

        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint($"##{id}Filter", LOC.Get("ANIM_Filter_Hint"), ref BoneFilter, 64);

        if (allowPoint && ImGui.Selectable($"{LOC.Get("ANIM_Constraints_Point")}##{id}_point", chosen < 0))
            chosen = -1;

        for (var b = 0; b < skeleton.BoneCount; b++)
        {
            if (b == exclude || BoneFilter.Length > 0 && !skeleton.Names[b].Contains(BoneFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            if (ImGui.Selectable($"{skeleton.Names[b]}##{id}_{b}", chosen == b))
                chosen = b;
        }

        ImGui.EndCombo();
    }

    /// <summary>A frame field with a Here button beside it (inside a form row); true when Here is pressed</summary>
    private static bool FrameWithHere(string id, ref int value, int frame)
    {
        var here = LOC.Get("ANIM_Ik_Here");
        var buttonWidth = ImGui.CalcTextSize(here).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.SetNextItemWidth(-(buttonWidth + ImGui.GetStyle().ItemSpacing.X));
        ImGui.InputInt($"##{id}", ref value, 0);
        ImGui.SameLine();
        var pressed = ImGui.Button($"{here}##{id}Here", new Vector2(buttonWidth, 0));
        GUI.Tooltip(LOC.Get("ANIM_Ik_Here_TT", frame));
        return pressed;
    }

    private static string AxisName(Vector3 axis) =>
        axis.X > 0.5f ? "+X" : axis.X < -0.5f ? "-X" : axis.Y > 0.5f ? "+Y" : axis.Y < -0.5f ? "-Y" : axis.Z > 0 ? "+Z" : "-Z";
}
