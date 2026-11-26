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
/// The Timeline window: playback of the selected clip in the viewport (play, pause, step, loop, speed) and a scrub bar
/// with a ruler, the selected bone's offset keys and, fainter, the keys of every other bone. Clicking or dragging on the
/// bar pauses there. With the window focused, Space plays / pauses and the arrow keys step a frame. Nothing here
/// changes the clip.
/// </summary>
public class AnimationTimelineView
{
    private readonly AnimationEditorView View;
    private bool Scrubbing;

    public AnimationTimelineView(AnimationEditorView view)
    {
        View = view;
    }

    public void Display()
    {
        var viewport = View.ViewportWindow;

        if (View.SelectedClip == null)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clip_None"));
            return;
        }

        if (viewport.Status is { } status)
        {
            GUI.WrappedText(status);
            return;
        }

        if (viewport.Animator is not { CurrentClip: not null } animator || !viewport.HasClip)
        {
            GUI.WrappedText(LOC.Get("ANIM_Playback_Loading"));
            return;
        }

        var frames = viewport.ClipFrameCount;
        var current = viewport.CurrentWholeFrame;
        var clip = View.Bank.Get(View.SelectedClip);

        Shortcuts(animator, frames, current);
        DisplayTransport(animator, frames, current, clip);
        DisplayBar(animator, frames, clip);
    }

    // ---- transport

    private void DisplayTransport(ModelAnimator animator, int frames, int current, EditableClip? clip)
    {
        var viewport = View.ViewportWindow;
        var keyFrames = SelectedKeyFrames(clip);

        if (ImGui.Button($"|<##animationTimelineFirst"))
            viewport.SeekFrame(0);
        GUI.Tooltip(LOC.Get("ANIM_Timeline_First"));

        ImGui.SameLine();
        ImGui.BeginDisabled(!keyFrames.Any(f => f < current));
        if (ImGui.Button($"<<##animationTimelinePreviousKey"))
            viewport.SeekFrame(keyFrames.Where(f => f < current).DefaultIfEmpty(current).Max());
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Previous_Key"));

        ImGui.SameLine();
        if (ImGui.Button($"<##animationTimelineStepBack"))
            viewport.SeekFrame(Math.Max(current - 1, 0));
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Step_Back"));

        ImGui.SameLine();
        if (ImGui.Button(animator.Playing ? $"{LOC.Get("ANIM_Playback_Pause")}##animationTimelinePlay" : $"{LOC.Get("ANIM_Playback_Play")}##animationTimelinePlay"))
            TogglePlaying(animator);
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Play_TT"));

        ImGui.SameLine();
        if (ImGui.Button($">##animationTimelineStepForward"))
            viewport.SeekFrame(Math.Min(current + 1, frames - 1));
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Step_Forward"));

        ImGui.SameLine();
        ImGui.BeginDisabled(!keyFrames.Any(f => f > current));
        if (ImGui.Button($">>##animationTimelineNextKey"))
            viewport.SeekFrame(keyFrames.Where(f => f > current).DefaultIfEmpty(current).Min());
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Next_Key"));

        ImGui.SameLine();
        if (ImGui.Button($">|##animationTimelineLast"))
            viewport.SeekFrame(frames - 1);
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Last"));

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("ANIM_Playback_Loop")}##animationTimelineLoop", ref animator.Loop);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(140 * DPI.UIScale());
        ImGui.SliderFloat("##animationTimelineSpeed", ref animator.Speed, 0.05f, 2f, LOC.Get("ANIM_Playback_Speed"));
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Speed_TT"));

        ImGui.SameLine();
        var duration = animator.Duration;
        ImGui.TextDisabled(LOC.Get("ANIM_Timeline_Position", current, frames - 1,
            animator.Time.ToString("F2"), duration.ToString("F2")));
    }

    private void TogglePlaying(ModelAnimator animator)
    {
        var viewport = View.ViewportWindow;

        // A finished clip starts over
        if (!animator.Playing && animator.Time >= animator.Duration)
            viewport.Seek(0);

        viewport.SetPlaying(!animator.Playing);
    }

    private void Shortcuts(ModelAnimator animator, int frames, int current)
    {
        if (!FocusManager.IsFocus(EditorFocusContext.AnimationEditor_Timeline) || ImGui.GetIO().WantTextInput)
            return;

        if (ImGui.IsKeyPressed(ImGuiKey.Space, false))
            TogglePlaying(animator);

        if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow))
            View.ViewportWindow.SeekFrame(Math.Max(current - 1, 0));

        if (ImGui.IsKeyPressed(ImGuiKey.RightArrow))
            View.ViewportWindow.SeekFrame(Math.Min(current + 1, frames - 1));
    }

    // ---- scrub bar

    private void DisplayBar(ModelAnimator animator, int frames, EditableClip? clip)
    {
        var scale = DPI.UIScale();
        var ruler = 18 * scale;
        var row = 14 * scale;
        var width = Math.Max(ImGui.GetContentRegionAvail().X, 200 * scale);
        var size = new Vector2(width, ruler + row * 2);
        var origin = ImGui.GetCursorScreenPos();

        ImGui.InvisibleButton("##animationTimelineBar", size, ImGuiButtonFlags.MouseButtonLeft);
        var hovered = ImGui.IsItemHovered();
        var mouse = ImGui.GetMousePos();

        var last = Math.Max(frames - 1, 1);
        var pad = 6 * scale;
        var trackWidth = width - pad * 2;
        float FrameToX(float f) => origin.X + pad + f / last * trackWidth;
        float XToFrame(float x) => (x - origin.X - pad) / trackWidth * last;

        var drawList = ImGui.GetWindowDrawList();
        var textColor = ImGui.GetColorU32(ImGuiCol.Text);
        var dullColor = ImGui.GetColorU32(ImGuiCol.TextDisabled);
        var lineColor = ImGui.GetColorU32(ImGuiCol.Border);
        var rowColor = ImGui.GetColorU32(ImGuiCol.FrameBg);
        var playColor = ImGui.GetColorU32(ImGuiCol.CheckMark);
        var keyColor = ImGui.GetColorU32(new Vector4(1f, 0.78f, 0.16f, 1f));

        // Rows: the selected bone's keys, then every keyed bone's
        drawList.AddRectFilled(new Vector2(origin.X, origin.Y + ruler), new Vector2(origin.X + width, origin.Y + ruler + row * 2 - 2 * scale), rowColor);

        // Ruler: ticks spaced to stay readable, labelled in frames
        drawList.AddLine(new Vector2(origin.X + pad, origin.Y + ruler), new Vector2(origin.X + pad + trackWidth, origin.Y + ruler), lineColor);
        var step = TickStep(last, trackWidth / scale);
        for (var f = 0; f <= last; f += step)
        {
            var x = FrameToX(f);
            drawList.AddLine(new Vector2(x, origin.Y + ruler * 0.55f), new Vector2(x, origin.Y + ruler), lineColor);
            drawList.AddText(new Vector2(x + 2 * scale, origin.Y), dullColor, f.ToString());
        }

        // Keys
        var selectedKeys = SelectedKeyFrames(clip);
        var otherKeys = clip?.Offsets.Where(o => o.Key != SelectedTrack(clip)).SelectMany(o => o.Value).Select(k => k.Frame).Distinct()
                        ?? Enumerable.Empty<int>();

        var keyY = origin.Y + ruler + row * 0.5f;
        foreach (var f in selectedKeys)
            Diamond(drawList, new Vector2(FrameToX(f), keyY), 5 * scale, keyColor);

        var otherY = origin.Y + ruler + row * 1.5f;
        foreach (var f in otherKeys)
            Diamond(drawList, new Vector2(FrameToX(f), otherY), 3.5f * scale, dullColor);

        // Playhead
        var playX = FrameToX(MathF.Min(animator.Frame, last));
        drawList.AddLine(new Vector2(playX, origin.Y), new Vector2(playX, origin.Y + size.Y), playColor, 2 * scale);

        // Scrub: a click anywhere on the bar pauses at that frame, and dragging follows the mouse
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            Scrubbing = true;

        if (Scrubbing)
        {
            if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                View.ViewportWindow.SeekFrame((int)MathF.Round(Math.Clamp(XToFrame(mouse.X), 0, last)));
            else
                Scrubbing = false;
        }

        if (hovered && !Scrubbing)
        {
            var at = (int)MathF.Round(Math.Clamp(XToFrame(mouse.X), 0, last));
            GUI.Tooltip(LOC.Get("ANIM_Timeline_Hover_Frame", at));
        }
    }

    private static void Diamond(ImDrawListPtr drawList, Vector2 c, float r, uint color)
    {
        drawList.AddQuadFilled(new Vector2(c.X, c.Y - r), new Vector2(c.X + r, c.Y), new Vector2(c.X, c.Y + r),
            new Vector2(c.X - r, c.Y), color);
    }

    /// <summary>Frames between ruler ticks: 1, 2, 5, 10, 20, 50... so ticks stay at least ~40 pixels apart</summary>
    private static int TickStep(int frames, float pixels)
    {
        var steps = new[] { 1, 2, 5, 10, 15, 30, 60, 120, 300, 600 };
        foreach (var s in steps)
        {
            if (frames / (float)s * 40 <= pixels)
                return s;
        }

        return steps[^1];
    }

    private int SelectedTrack(EditableClip? clip)
    {
        if (clip == null || View.Bank.Character is not { } character || View.SelectedBone < 0 ||
            View.SelectedBone >= character.Skeleton.BoneCount)
            return -1;

        return clip.BoneToTrack(character.Skeleton.BoneCount)[View.SelectedBone];
    }

    private List<int> SelectedKeyFrames(EditableClip? clip)
    {
        var track = SelectedTrack(clip);
        return clip != null && track >= 0 && clip.Offsets.TryGetValue(track, out var keys)
            ? keys.Select(k => k.Frame).ToList()
            : new List<int>();
    }
}
