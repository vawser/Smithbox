#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static StudioCore.Editors.AnimationEditor.AnimationToolLayout;

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
    private string LayerName = "";
    private string WeightLayerName = "";
    private float LayerWeight = 1;

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

        // Speed
        Group(LOC.Get("ANIM_Op_Speed"), LOC.Get("ANIM_Batch_Speed_TT"));
        if (BeginForm("animationBatchSpeedForm"))
        {
            FormRow(LOC.Get("ANIM_Op_Speed"), LOC.Get("ANIM_Batch_Speed_TT"));
            ImGui.InputFloat("##animationBatchSpeed", ref Speed, 0.1f, 0.5f, "%.2f");
            ImGui.EndTable();
        }
        Speed = Math.Clamp(Speed, 0.05f, 20f);

        if (WideButton(LOC.Get("ANIM_Batch_Speed_Button", Speed.ToString("0.##"), clips.Count), "animationBatchSpeedApply",
                Math.Abs(Speed - 1) >= 1e-4f, LOC.Get("ANIM_Batch_Speed_TT")))
        {
            var speed = Speed;
            Apply(LOC.Get("ANIM_Action_Batch_Speed", clips.Count, speed.ToString("F2")),
                clip => EditableClipOperations.Speed(clip.Baked(), speed));
        }

        // Root motion factors
        Group(LOC.Get("ANIM_Batch_Root"), LOC.Get("ANIM_Batch_Root_TT"));
        if (BeginForm("animationBatchRootForm"))
        {
            FormRow(LOC.Get("ANIM_Batch_Root_Ground"), LOC.Get("ANIM_Root_Scale_Ground_TT"));
            ImGui.InputFloat("##animationBatchGround", ref Ground, 0.1f, 0.5f, "x%.2f");
            FormRow(LOC.Get("ANIM_Batch_Root_Vertical"), LOC.Get("ANIM_Root_Scale_Vertical_TT"));
            ImGui.InputFloat("##animationBatchVertical", ref Vertical, 0.1f, 0.5f, "x%.2f");
            FormRow(LOC.Get("ANIM_Batch_Root_Turn"), LOC.Get("ANIM_Root_Scale_Turn_TT"));
            ImGui.InputFloat("##animationBatchTurn", ref Turn, 0.1f, 0.5f, "x%.2f");
            ImGui.EndTable();
        }
        Ground = Math.Clamp(Ground, 0f, 20f);
        Vertical = Math.Clamp(Vertical, 0f, 20f);
        Turn = Math.Clamp(Turn, 0f, 20f);

        var unchanged = Math.Abs(Ground - 1) < 1e-4f && Math.Abs(Vertical - 1) < 1e-4f && Math.Abs(Turn - 1) < 1e-4f;
        if (WideButton(LOC.Get("ANIM_Batch_Root_Button", clips.Count), "animationBatchRootApply", !unchanged, LOC.Get("ANIM_Batch_Root_TT")))
        {
            var (g, v, t) = (Ground, Vertical, Turn);
            Apply(LOC.Get("ANIM_Action_Batch_Root", clips.Count, g.ToString("F2"), v.ToString("F2"), t.ToString("F2")),
                clip => clip.EffectiveRootMotion == null ? null : EditableClipOperations.ScaleRootMotion(clip.BakedRoot(), g, v, t));
            Ground = Vertical = Turn = 1;
        }

        // Paste the offset clipboard, into a layer named so in every clip (made on top where a clip has none), or
        // each clip's active layer
        var copied = View.OffsetClipboard;
        Group(LOC.Get("ANIM_Batch_Paste"), copied != null ? LOC.Get("ANIM_Batch_Paste_TT", copied.From, copied.Frame) : LOC.Get("ANIM_Clipboard_Empty"));
        if (BeginForm("animationBatchPasteForm"))
        {
            FormRow(LOC.Get("ANIM_Batch_Paste_Frame"), LOC.Get("ANIM_Batch_Paste_Frame_TT"));
            ImGui.InputInt("##animationBatchPasteFrame", ref PasteFrame);
            FormRow(LOC.Get("ANIM_Batch_Layer"), LOC.Get("ANIM_Batch_Layer_TT"));
            ImGui.InputTextWithHint("##animationBatchLayer", LOC.Get("ANIM_Batch_Layer_Hint"), ref LayerName, 64);
            ImGui.EndTable();
        }
        PasteFrame = Math.Max(PasteFrame, 0);
        var layerName = LayerName.Trim();

        var pasteLabel = copied != null ? LOC.Get("ANIM_Batch_Paste_Button", copied.Offsets.Count, PasteFrame) : LOC.Get("ANIM_Batch_Paste_Button_Empty");
        if (WideButton(pasteLabel, "animationBatchPaste", copied != null,
                copied != null ? LOC.Get("ANIM_Batch_Paste_TT", copied.From, copied.Frame) : LOC.Get("ANIM_Clipboard_Empty")))
        {
            var frame = PasteFrame;
            var skipped = new HashSet<string>();
            Apply(LOC.Get("ANIM_Action_Batch_Paste", copied!.Offsets.Count, clips.Count, frame), clip =>
            {
                // Pasted goes onto the active layer: the named one is made active for it, then the clip's own again
                var active = clip.ActiveLayer;
                var target = clip;
                if (layerName.Length > 0)
                    target = clip.LayerNamed(layerName) is var i and >= 0 ? clip.WithActiveLayer(i) : clip.WithNewLayer(layerName);

                var missing = new List<string>();
                var result = View.BonesView.Pasted(target, Math.Min(frame, target.FrameCount - 1), missing, out _);
                skipped.UnionWith(missing);
                return result?.WithActiveLayer(active);
            });

            if (skipped.Count > 0)
                Smithbox.LogError(this, LOC.Get("ANIM_Batch_Paste_Skipped", string.Join(", ", skipped)));
        }

        // The weight of a named layer in every clip that has it
        Group(LOC.Get("ANIM_Batch_Layer_Weight"), LOC.Get("ANIM_Batch_Layer_Weight_TT"));
        if (BeginForm("animationBatchWeightForm"))
        {
            FormRow(LOC.Get("ANIM_Batch_Weight_Layer"), LOC.Get("ANIM_Batch_Layer_Weight_TT"));
            ImGui.InputTextWithHint("##animationBatchWeightLayer", LOC.Get("ANIM_Batch_Weight_Layer_Hint"), ref WeightLayerName, 64);
            FormRow(LOC.Get("ANIM_Layers_Weight"), LOC.Get("ANIM_Layers_Weight_TT"));
            ImGui.SliderFloat("##animationBatchLayerWeight", ref LayerWeight, 0, 1.5f, "%.2f");
            ImGui.EndTable();
        }

        var weightLayer = WeightLayerName.Trim();
        var weightLabel = weightLayer.Length > 0
            ? LOC.Get("ANIM_Batch_Weight_Button", weightLayer, LayerWeight.ToString("0.00"), clips.Count)
            : LOC.Get("ANIM_Batch_Weight_Button_None");
        if (WideButton(weightLabel, "animationBatchLayerWeightApply", weightLayer.Length > 0, LOC.Get("ANIM_Batch_Layer_Weight_TT")))
        {
            var weight = LayerWeight;
            Apply(LOC.Get("ANIM_Action_Batch_Layer_Weight", weightLayer, weight.ToString("0.00"), clips.Count),
                clip => clip.LayerNamed(weightLayer) is var i and >= 0 ? clip.WithLayer(i, l => l.Weight = weight) : null);
        }
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
