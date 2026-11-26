#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Batch tool (Animation Editor Analysis.md §8, W12): one change made to every clip picked in the Clips list with
/// Ctrl / Shift+click, as one undo step. Speed (on the clip as it plays, keys folded in, as the Clip tool's), the root
/// motion factors (as the Root Motion tool's, the bones' keys kept), and Paste of the offset clipboard at a frame (each
/// clip's last if it is shorter). A clip that can't be read, or has nothing to change (no root motion, none of the
/// copied bones), is passed over and counted. Time act events aren't retimed here; Export's check lists clips whose
/// events then fall past the end.
/// </summary>
public class AnimationBatchTool
{
    private readonly AnimationEditorView View;

    private float Speed = 1;
    private float Ground = 1;
    private float Vertical = 1;
    private float Turn = 1;
    private int PasteFrame;

    public AnimationBatchTool(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>As a tool of the Tools window</summary>
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Batch")}##animationBatchToolHeader"))
            return;

        ImGui.BeginChild("AnimationBatchToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        var clips = View.BatchClips;
        if (View.Bank.Character == null || clips.Count < 2)
        {
            GUI.WrappedText(LOC.Get("ANIM_Batch_Hint"));
            return;
        }

        GUI.SimpleHeader(LOC.Get("ANIM_Batch_Header", clips.Count), LOC.Get("ANIM_Batch_Header_TT"));
        ImGui.TextDisabled(string.Join(", ", clips.Take(6).Select(c => c.Name)) + (clips.Count > 6 ? ", ..." : ""));
        GUI.Spacer();

        // Speed
        ImGui.Text(LOC.Get("ANIM_Op_Speed"));
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
        ImGui.InputFloat("##animationBatchSpeed", ref Speed, 0.1f, 0.5f, "%.2f");
        Speed = Math.Clamp(Speed, 0.05f, 20f);
        ImGui.SameLine();
        ImGui.BeginDisabled(Math.Abs(Speed - 1) < 1e-4f);
        if (ImGui.Button($"{LOC.Get("ANIM_Op_Apply")}##animationBatchSpeedApply"))
        {
            var speed = Speed;
            Apply(LOC.Get("ANIM_Action_Batch_Speed", clips.Count, speed.ToString("F2")),
                clip => EditableClipOperations.Speed(clip.Baked(), speed));
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Batch_Speed_TT"));

        GUI.Spacer();

        // Root motion factors
        ImGui.Text(LOC.Get("ANIM_Batch_Root"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat("##animationBatchGround", ref Ground, 0.1f, 0.5f, LOC.Get("ANIM_Root_Scale_Ground_Format"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat("##animationBatchVertical", ref Vertical, 0.1f, 0.5f, LOC.Get("ANIM_Root_Scale_Vertical_Format"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat("##animationBatchTurn", ref Turn, 0.1f, 0.5f, LOC.Get("ANIM_Root_Scale_Turn_Format"));
        Ground = Math.Clamp(Ground, 0f, 20f);
        Vertical = Math.Clamp(Vertical, 0f, 20f);
        Turn = Math.Clamp(Turn, 0f, 20f);

        var unchanged = Math.Abs(Ground - 1) < 1e-4f && Math.Abs(Vertical - 1) < 1e-4f && Math.Abs(Turn - 1) < 1e-4f;
        ImGui.BeginDisabled(unchanged);
        if (ImGui.Button($"{LOC.Get("ANIM_Op_Apply")}##animationBatchRootApply"))
        {
            var (g, v, t) = (Ground, Vertical, Turn);
            Apply(LOC.Get("ANIM_Action_Batch_Root", clips.Count, g.ToString("F2"), v.ToString("F2"), t.ToString("F2")),
                clip => clip.EffectiveRootMotion == null ? null : EditableClipOperations.ScaleRootMotion(clip.BakedRoot(), g, v, t));
            Ground = Vertical = Turn = 1;
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Batch_Root_TT"));

        GUI.Spacer();

        // Paste the offset clipboard
        var copied = View.OffsetClipboard;
        ImGui.Text(LOC.Get("ANIM_Batch_Paste"));
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
        ImGui.InputInt($"{LOC.Get("ANIM_Batch_Paste_Frame")}##animationBatchPasteFrame", ref PasteFrame);
        PasteFrame = Math.Max(PasteFrame, 0);
        ImGui.BeginDisabled(copied == null);
        if (ImGui.Button($"{LOC.Get("ANIM_Clipboard_Paste", copied?.Offsets.Count ?? 0)}##animationBatchPaste"))
        {
            var frame = PasteFrame;
            var skipped = new HashSet<string>();
            Apply(LOC.Get("ANIM_Action_Batch_Paste", copied!.Offsets.Count, clips.Count, frame), clip =>
            {
                var missing = new List<string>();
                var result = View.BonesView.Pasted(clip, Math.Min(frame, clip.FrameCount - 1), missing, out _);
                skipped.UnionWith(missing);
                return result;
            });

            if (skipped.Count > 0)
                Smithbox.LogError(this, LOC.Get("ANIM_Batch_Paste_Skipped", string.Join(", ", skipped)));
        }
        ImGui.EndDisabled();
        GUI.Tooltip(copied != null ? LOC.Get("ANIM_Batch_Paste_TT", copied.From, copied.Frame) : LOC.Get("ANIM_Clipboard_Empty"));
    }

    /// <summary>
    /// Makes the change to each clip of the batch (null from <paramref name="change" />: nothing to change there) and
    /// runs them as one undo step; logs how many were changed and passed over
    /// </summary>
    private void Apply(string name, Func<EditableClip, EditableClip?> change)
    {
        var actions = new List<EditorAction>();
        var passed = 0;

        foreach (var source in View.BatchClips)
        {
            if (View.Bank.Get(source) is not { } clip)
            {
                passed++;
                continue;
            }

            EditableClip? result;
            try
            {
                result = change(clip);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("ANIM_Op_Failed", source.Name, e.Message), e);
                result = null;
            }

            if (result == null)
            {
                passed++;
                continue;
            }

            actions.Add(new AnimationClipEditAction(View.Bank, source, result, ""));
        }

        if (actions.Count > 0)
        {
            View.Execute(new CompoundAction(actions) { DisplayName = name });
            View.ClipView.OnClipSelected();
        }

        Smithbox.Log(this, LOC.Get("ANIM_Batch_Done", actions.Count, passed));
    }
}
