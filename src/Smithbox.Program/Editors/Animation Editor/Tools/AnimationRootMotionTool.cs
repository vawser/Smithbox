#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Numerics;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Root Motion tool: how far the selected clip moves the character (along the ground, up or down, and how far it
/// turns), and scaling each, as one undoable step. The poses are left as they are, so a large change makes the feet
/// slide. The path is drawn in the viewport (Display → Show Root Path), and Display → Travel moves the character
/// along it while playing.
/// </summary>
public class AnimationRootMotionTool
{
    private readonly AnimationEditorView View;

    // The scale fields, filled again when another clip is selected
    private float Ground = 1;
    private float Vertical = 1;
    private float Turn = 1;
    private AnimationClipSource? FieldsFor;

    public AnimationRootMotionTool(AnimationEditorView view)
    {
        View = view;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Root_Motion")}##animationRootMotionToolHeader"))
            return;

        ImGui.BeginChild("AnimationRootMotionToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        if (View.SelectedClip is not { } source || View.Bank.Get(source) is not { } clip)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clip_None"));
            return;
        }

        if (clip.RootMotion is not { Length: > 0 } motion)
        {
            GUI.WrappedText(LOC.Get("ANIM_Root_None"));
            return;
        }

        if (FieldsFor != source)
        {
            Ground = Vertical = Turn = 1;
            FieldsFor = source;
        }

        // Travel
        var up = clip.UpAxis;
        var end = new Vector3(motion[^1].X, motion[^1].Y, motion[^1].Z);
        var upward = Vector3.Dot(end, up);
        var along = (end - up * upward).Length();
        var turn = motion[^1].W * 180 / MathF.PI;
        var speed = clip.Duration > 0 ? along / clip.Duration : 0;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("ANIM_Root_Travel_Header"), LOC.Get("ANIM_Root_Travel_Header_TT"));
        ImGui.Text(LOC.Get("ANIM_Root_Travel_Ground", along.ToString("F3"), speed.ToString("F2")));
        ImGui.Text(LOC.Get("ANIM_Root_Travel_Vertical", upward.ToString("F3")));
        ImGui.Text(LOC.Get("ANIM_Root_Travel_Turn", turn.ToString("F1")));

        // Scale
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("ANIM_Root_Scale_Header"), LOC.Get("ANIM_Root_Scale_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat("##animationRootGround", ref Ground, 0.1f, 0.5f, LOC.Get("ANIM_Root_Scale_Ground_Format"));
        GUI.Tooltip(LOC.Get("ANIM_Root_Scale_Ground_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat("##animationRootVertical", ref Vertical, 0.1f, 0.5f, LOC.Get("ANIM_Root_Scale_Vertical_Format"));
        GUI.Tooltip(LOC.Get("ANIM_Root_Scale_Vertical_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat("##animationRootTurn", ref Turn, 0.1f, 0.5f, LOC.Get("ANIM_Root_Scale_Turn_Format"));
        GUI.Tooltip(LOC.Get("ANIM_Root_Scale_Turn_TT"));

        Ground = Math.Clamp(Ground, 0, 20);
        Vertical = Math.Clamp(Vertical, 0, 20);
        Turn = Math.Clamp(Turn, -20, 20);

        // Several fields, so only the button applies them
        var unchanged = Ground == 1 && Vertical == 1 && Turn == 1;
        ImGui.BeginDisabled(unchanged);
        if (ImGui.Button($"{LOC.Get("ANIM_Op_Apply")}##animationRootApply", new Vector2(-1, 0)))
        {
            Apply(source, clip, Ground, Vertical, Turn, LOC.Get("ANIM_Action_Root_Scale", source.Name,
                Ground.ToString("F2"), Vertical.ToString("F2"), Turn.ToString("F2")));
            Ground = Vertical = Turn = 1;
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Root_Scale_Apply_TT"));

        // Quick edits
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("ANIM_Root_Quick_Header"), LOC.Get("ANIM_Root_Quick_Header_TT"));

        ImGui.BeginDisabled(MathF.Abs(upward) < 1e-5f && !HasVertical(motion, up));
        if (ImGui.Button($"{LOC.Get("ANIM_Root_Flatten")}##animationRootFlatten", new Vector2(-1, 0)))
            Apply(source, clip, 1, 0, 1, LOC.Get("ANIM_Action_Root_Flatten", source.Name));
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Root_Flatten_TT"));

        ImGui.BeginDisabled(!HasTurn(motion));
        if (ImGui.Button($"{LOC.Get("ANIM_Root_Remove_Turn")}##animationRootRemoveTurn", new Vector2(-1, 0)))
            Apply(source, clip, 1, 1, 0, LOC.Get("ANIM_Action_Root_Remove_Turn", source.Name));
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Root_Remove_Turn_TT"));

        if (ImGui.Button($"{LOC.Get("ANIM_Root_Remove_All")}##animationRootRemoveAll", new Vector2(-1, 0)))
            Apply(source, clip, 0, 0, 0, LOC.Get("ANIM_Action_Root_Remove_All", source.Name));
        GUI.Tooltip(LOC.Get("ANIM_Root_Remove_All_TT"));

        GUI.Spacer();
        GUI.WrappedText(LOC.Get("ANIM_Root_Viewport_Hint"));
    }

    private void Apply(AnimationClipSource source, EditableClip clip, float ground, float vertical, float turn, string name)
    {
        EditableClip result;
        try
        {
            result = EditableClipOperations.ScaleRootMotion(clip, ground, vertical, turn);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("ANIM_Op_Failed", name, e.Message), e);
            return;
        }

        View.Execute(new AnimationClipEditAction(View.Bank, source, result, name));
    }

    private static bool HasVertical(Vector4[] motion, Vector3 up)
    {
        foreach (var s in motion)
        {
            if (MathF.Abs(Vector3.Dot(new Vector3(s.X, s.Y, s.Z), up)) > 1e-5f)
                return true;
        }

        return false;
    }

    private static bool HasTurn(Vector4[] motion)
    {
        foreach (var s in motion)
        {
            if (MathF.Abs(s.W) > 1e-6f)
                return true;
        }

        return false;
    }
}
