#nullable enable
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Timeline window: playback of the selected clip in the viewport (play, pause, step, loop, speed) and a bar with
/// a ruler, the selected bone's offset keys, a summary row of every key, optionally a row per keyed bone, then the
/// clip's time act events (read-only, View → Time Act Events).
/// <para>
/// Keys can be edited here: click / Ctrl+click / box-select them, drag to move, Shift+drag to scale about the playhead,
/// Alt+drag to copy, Alt+arrows to nudge, Delete, and copy / paste with Ctrl+C / Ctrl+V. Each change is one undo step.
/// Scrolling zooms around the mouse; Shift+scroll or a middle-drag pans. Clicking or dragging on the ruler or the event
/// rows pauses there. With the window or the viewport focused, Space plays / pauses and the arrow keys step a frame.
/// </para>
/// </summary>
public class AnimationTimelineView
{
    private readonly AnimationEditorView View;

    private readonly AnimationCurveView Curves;

    // The transition preview (Transition tool): the Timeline shows all of what plays, lead-in included, and edits nothing
    private bool Previewing => View.TransitionTool.IsPreviewing;
    private int Frames => Previewing ? View.ViewportWindow.PlayingFrameCount : View.ViewportWindow.ClipFrameCount;
    private int Current => Previewing ? View.ViewportWindow.PlayingWholeFrame : View.ViewportWindow.CurrentWholeFrame;

    private void SeekTo(int frame)
    {
        if (Previewing)
            View.ViewportWindow.SeekPlayingFrame(frame);
        else
            View.ViewportWindow.SeekFrame(frame);
    }

    // The curve view asked for the key menu on a key (it opens on the next frame, in the bar's popup scope)
    private bool KeyMenuPending;

    public AnimationTimelineView(AnimationEditorView view)
    {
        View = view;
        Curves = new AnimationCurveView(view, this);
    }

    /// <summary>Selects one key and opens the key menu (ease, copy, delete) for it: the curve view's right-click</summary>
    public void OpenKeyMenu(int track, int frame)
    {
        SelectedKeys.Clear();
        SelectedKeys.Add((track, frame));
        KeyMenuPending = true;
    }

    /// <summary>The wheel's zoom around a frame, or with <paramref name="pan" /> a pan (the bar's and the curves')</summary>
    public void Zoom(float at, float wheel, bool pan, int last)
    {
        if (pan)
        {
            PanView(-wheel * ViewFrames * 0.1f, last);
            return;
        }

        var factor = MathF.Pow(0.85f, wheel);
        var span = Math.Clamp(ViewFrames * factor, Math.Min(5, last), last);
        ViewStart = at - (at - ViewStart) * (span / ViewFrames);
        ViewFrames = span;
        PanView(0, last);
    }

    // ---- view range (zoom and pan)

    // The frame at the bar's left edge and how many frames the bar spans (the clip's last frame when fitted)
    private float ViewStart;
    private float ViewFrames = -1;

    // The clip and length the view was fitted for: another one fits it again and clears the key selection
    private AnimationClipSource? ViewClip;
    private int ViewLast = -1;

    // ---- key selection and drags

    private readonly HashSet<(int Track, int Frame)> SelectedKeys = new();

    private enum DragMode { None, Scrub, Keys, Box, Pan, Range }

    // ---- frame range (Shift+drag on the ruler), for the Clip tool's Range Timing

    /// <summary>The picked frame range, both ends included; null for none</summary>
    public (int First, int Last)? Range { get; private set; }

    /// <summary>Changed whenever <see cref="Range" /> is, so the Clip tool takes a new pick</summary>
    public int RangeVersion { get; private set; }

    private int RangeAnchor;

    public void SetRange(int first, int last)
    {
        Range = (Math.Min(first, last), Math.Max(first, last));
        RangeVersion++;
    }

    public void ClearRange()
    {
        Range = null;
        RangeVersion++;
    }

    private DragMode Drag;
    private Vector2 DragStart;
    private float PanStartView;

    // A press on a key that was already selected: on a release without moving, only that key stays selected
    private (int Track, int Frame)[]? PressedKeys;
    private bool PressedSelected;

    // Copied keys: bone name (RootName for the root motion), frame from the first copied key, the key
    private List<(string Bone, int Frame, BoneOffsetKey Key)>? KeyClipboard;
    private const string RootName = "<root>";

    // Diamonds drawn this frame, for hit tests and the box selection: centre, radius, the keys they stand for
    private readonly List<(Vector2 Centre, float Radius, (int Track, int Frame)[] Keys)> Drawn = new();

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

        var frames = Frames;
        var current = Current;
        var clip = Previewing ? null : View.Bank.Get(View.SelectedClip);

        DisplayTransport(animator, frames, current, clip);
        DisplayBar(animator, frames, clip);
    }

    // ---- transport

    private void DisplayTransport(ModelAnimator animator, int frames, int current, EditableClip? clip)
    {
        var viewport = View.ViewportWindow;
        var keyFrames = SelectedKeyFrames(clip);

        if (ImGui.Button($"|<##animationTimelineFirst"))
            SeekTo(0);
        GUI.Tooltip(LOC.Get("ANIM_Timeline_First"));

        ImGui.SameLine();
        ImGui.BeginDisabled(!keyFrames.Any(f => f < current));
        if (ImGui.Button($"<<##animationTimelinePreviousKey"))
            SeekTo(keyFrames.Where(f => f < current).DefaultIfEmpty(current).Max());
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Previous_Key"));

        ImGui.SameLine();
        if (ImGui.Button($"<##animationTimelineStepBack"))
            SeekTo(Math.Max(current - 1, 0));
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Step_Back"));

        ImGui.SameLine();
        if (ImGui.Button(animator.Playing ? $"{LOC.Get("ANIM_Playback_Pause")}##animationTimelinePlay" : $"{LOC.Get("ANIM_Playback_Play")}##animationTimelinePlay"))
            TogglePlaying(animator);
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Play_TT"));

        ImGui.SameLine();
        if (ImGui.Button($">##animationTimelineStepForward"))
            SeekTo(Math.Min(current + 1, frames - 1));
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Step_Forward"));

        ImGui.SameLine();
        ImGui.BeginDisabled(!keyFrames.Any(f => f > current));
        if (ImGui.Button($">>##animationTimelineNextKey"))
            SeekTo(keyFrames.Where(f => f > current).DefaultIfEmpty(current).Min());
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Next_Key"));

        ImGui.SameLine();
        if (ImGui.Button($">|##animationTimelineLast"))
            SeekTo(frames - 1);
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Last"));

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("ANIM_Playback_Loop")}##animationTimelineLoop", ref animator.Loop);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(140 * DPI.UIScale());
        ImGui.SliderFloat("##animationTimelineSpeed", ref animator.Speed, 0.05f, 2f, LOC.Get("ANIM_Playback_Speed"));
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Speed_TT"));

        // What the Timeline shows under the ruler
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("ANIM_Timeline_Show_Events")}##animationTimelineShowEvents",
            ref CFG.Current.Interface.Interface_AnimationEditor_Timeline_Events);
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Show_Events_TT"));

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("ANIM_Timeline_Bone_Rows")}##animationTimelineBoneRows",
            ref CFG.Current.Interface.Interface_AnimationEditor_Timeline_Bone_Rows);
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Bone_Rows_TT"));

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("ANIM_Timeline_Curves")}##animationTimelineCurves",
            ref CFG.Current.Interface.Interface_AnimationEditor_Timeline_Curves);
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Curves_TT"));

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

    /// <summary>
    /// Space plays / pauses and the arrow keys step a frame, with the Timeline or the viewport focused. With the
    /// Timeline focused also: Delete, Ctrl+C / Ctrl+V, Ctrl+A on keys, Alt+arrows nudge the selected keys, Home fits
    /// the view. Called every frame by the view, so the keys work whether or not the Timeline window is shown.
    /// </summary>
    public void Shortcuts()
    {
        var timeline = FocusManager.IsFocus(EditorFocusContext.AnimationEditor_Timeline);
        if (!timeline && !FocusManager.IsFocus(EditorFocusContext.AnimationEditor_Viewport))
            return;

        if (ImGui.GetIO().WantTextInput || View.SelectedClip is not { } source)
            return;

        var viewport = View.ViewportWindow;
        if (viewport.Animator is not { CurrentClip: not null } animator || !viewport.HasClip)
            return;

        var frames = Frames;
        var current = Current;
        var io = ImGui.GetIO();

        if (ImGui.IsKeyPressed(ImGuiKey.Space, false))
            TogglePlaying(animator);

        // Alt+arrows nudge the selected keys instead of stepping
        var nudge = timeline && io.KeyAlt && SelectedKeys.Count > 0 && Drag == DragMode.None;

        if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow))
        {
            if (nudge)
                NudgeKeys(source, -1, frames - 1);
            else
                SeekTo(Math.Max(current - 1, 0));
        }

        if (ImGui.IsKeyPressed(ImGuiKey.RightArrow))
        {
            if (nudge)
                NudgeKeys(source, 1, frames - 1);
            else
                SeekTo(Math.Min(current + 1, frames - 1));
        }

        if (!timeline || Previewing || View.Bank.Get(source) is not { } clip)
            return;

        if (ImGui.IsKeyPressed(ImGuiKey.Home, false))
            FitView(frames - 1);

        // Ctrl+Shift+C / V are the Bones tool's pose copy and paste, which InputManager also reports as Ctrl+C / V
        if (io.KeyShift)
            return;

        if (InputManager.IsPressed(KeybindID.Delete) && SelectedKeys.Count > 0)
            DeleteKeys(source, clip);
        else if (InputManager.IsPressed(KeybindID.Copy) && SelectedKeys.Count > 0)
            CopyKeys(source, clip);
        else if (InputManager.IsPressed(KeybindID.Paste) && KeyClipboard is { Count: > 0 })
            PasteKeys(source, clip, current);
        else if (InputManager.IsPressed(KeybindID.SelectAll))
        {
            SelectedKeys.Clear();
            foreach (var (track, keys) in clip.Offsets)
            foreach (var key in keys)
                SelectedKeys.Add((track, key.Frame));
        }
    }

    // ---- bar

    private void DisplayBar(ModelAnimator animator, int frames, EditableClip? clip)
    {
        var scale = DPI.UIScale();
        var io = ImGui.GetIO();
        var ruler = 18 * scale;
        var row = Math.Max(14 * scale, ImGui.GetTextLineHeight() + 2 * scale);
        var width = Math.Max(ImGui.GetContentRegionAvail().X, 260 * scale);
        var gutter = 120 * scale;
        var last = Math.Max(frames - 1, 1);

        if (!ReferenceEquals(ViewClip, View.SelectedClip) || ViewLast != last || ViewFrames <= 0)
        {
            // Another clip drops the range; the same clip made longer or shorter keeps it if it still fits
            if (!ReferenceEquals(ViewClip, View.SelectedClip) || Range is { } r && r.Last > last)
                ClearRange();

            ViewClip = View.SelectedClip;
            FitView(last);
            SelectedKeys.Clear();
            Drag = DragMode.None;
        }

        // Keys an undo or another edit took away leave the selection
        if (clip != null)
            SelectedKeys.RemoveWhere(k => !clip.Offsets.TryGetValue(k.Track, out var ks) || !ks.Any(x => x.Frame == k.Frame));
        else
            SelectedKeys.Clear();

        // Key rows: the selected bone, every key, then a row per keyed bone (Bone Rows)
        var selectedTrack = SelectedTrack(clip);
        var boneRows = CFG.Current.Interface.Interface_AnimationEditor_Timeline_Bone_Rows && clip != null
            ? clip.Offsets.Keys.OrderBy(t => t == EditableClip.RootTrack ? -1 : clip.BoneOf(t)).ToList()
            : new List<int>();
        var keyRowCount = 2 + boneRows.Count;
        var keysTop = ruler;
        var keysBottom = keysTop + row * keyRowCount;

        // Time act events, read-only, in rows under the keys (one or more rows per event type)
        var events = EventRows(clip, out var eventRowCount, out var eventStatus);
        var eventRow = ImGui.GetTextLineHeight() + 4 * scale;
        var eventGap = eventRowCount > 0 ? 4 * scale : 0;
        var eventsTop = keysBottom + eventGap;

        var size = new Vector2(width, eventsTop + eventRowCount * eventRow);
        var origin = ImGui.GetCursorScreenPos();

        ImGui.InvisibleButton("##animationTimelineBar", size,
            ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonMiddle | ImGuiButtonFlags.MouseButtonRight);
        var hovered = ImGui.IsItemHovered();
        var mouse = ImGui.GetMousePos();

        var pad = 6 * scale;
        var trackLeft = origin.X + gutter + pad;
        var trackWidth = Math.Max(width - gutter - pad * 2, 40 * scale);
        var trackRight = trackLeft + trackWidth;
        float FrameToX(float f) => trackLeft + (f - ViewStart) / ViewFrames * trackWidth;
        float XToFrame(float x) => ViewStart + (x - trackLeft) / trackWidth * ViewFrames;
        int FrameAt(float x) => (int)MathF.Round(Math.Clamp(XToFrame(x), 0, last));
        var pixelsPerFrame = trackWidth / ViewFrames;

        // Zoom around the mouse; Shift pans
        if (hovered && io.MouseWheel != 0 && Drag != DragMode.Keys)
            Zoom(XToFrame(mouse.X), io.MouseWheel, io.KeyShift, last);

        // While playing zoomed in, the view pages along with the playhead
        if (animator.Playing && Drag == DragMode.None && ViewFrames < last &&
            (animator.Frame < ViewStart || animator.Frame > ViewStart + ViewFrames))
        {
            ViewStart = animator.Frame;
            PanView(0, last);
        }

        var drawList = ImGui.GetWindowDrawList();
        var textColor = ImGui.GetColorU32(ImGuiCol.Text);
        var dullColor = ImGui.GetColorU32(ImGuiCol.TextDisabled);
        var lineColor = ImGui.GetColorU32(ImGuiCol.Border);
        var rowColor = ImGui.GetColorU32(ImGuiCol.FrameBg);
        var rowSelectedColor = ImGui.GetColorU32(ImGuiCol.FrameBgHovered);
        var playColor = ImGui.GetColorU32(ImGuiCol.CheckMark);
        var keyColor = ImGui.GetColorU32(new Vector4(1f, 0.78f, 0.16f, 1f));
        var keySelectedColor = ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f));
        var boxColor = ImGui.GetColorU32(new Vector4(0.4f, 0.6f, 1f, 0.25f));
        var boxLineColor = ImGui.GetColorU32(new Vector4(0.4f, 0.6f, 1f, 0.9f));

        // Row backgrounds and gutter labels
        string RowLabel(int r) => r switch
        {
            0 => selectedTrack == int.MinValue ? LOC.Get("ANIM_Timeline_Row_None") : TrackName(clip!, selectedTrack),
            1 => LOC.Get("ANIM_Timeline_Row_All"),
            _ => TrackName(clip!, boneRows[r - 2])
        };

        for (var r = 0; r < keyRowCount; r++)
        {
            var y0 = origin.Y + keysTop + r * row;
            var highlighted = r >= 2 && boneRows[r - 2] == selectedTrack;
            if (r % 2 == 0 || highlighted)
                drawList.AddRectFilled(new Vector2(origin.X, y0), new Vector2(origin.X + width, y0 + row), highlighted ? rowSelectedColor : rowColor);

            drawList.PushClipRect(new Vector2(origin.X, y0), new Vector2(origin.X + gutter, y0 + row), true);
            drawList.AddText(new Vector2(origin.X + 4 * scale, y0 + (row - ImGui.GetTextLineHeight()) / 2), r == 1 ? dullColor : textColor,
                clip == null && r != 1 ? "" : RowLabel(r));
            drawList.PopClipRect();
        }

        drawList.PushClipRect(new Vector2(trackLeft - pad, origin.Y), new Vector2(trackRight + pad, origin.Y + size.Y), true);

        // The transition preview's lead-in, a band up to where the clip's frame 0 plays
        if (Previewing && View.ViewportWindow.LeadIn > 0)
        {
            var leadEnd = FrameToX(View.ViewportWindow.LeadIn);
            drawList.AddRectFilled(new Vector2(FrameToX(0), origin.Y), new Vector2(leadEnd, origin.Y + size.Y),
                ImGui.GetColorU32(new Vector4(1f, 0.6f, 0.2f, 0.14f)));
            drawList.AddLine(new Vector2(leadEnd, origin.Y), new Vector2(leadEnd, origin.Y + size.Y),
                ImGui.GetColorU32(new Vector4(1f, 0.6f, 0.2f, 0.9f)), 2 * scale);
        }

        // The picked range, a band over every row
        if (Range is { } band)
        {
            var bandColor = ImGui.GetColorU32(new Vector4(0.35f, 0.75f, 1f, 0.16f));
            var bandEdge = ImGui.GetColorU32(new Vector4(0.35f, 0.75f, 1f, 0.8f));
            var (bx0, bx1) = (FrameToX(band.First), FrameToX(band.Last));
            drawList.AddRectFilled(new Vector2(bx0, origin.Y), new Vector2(bx1, origin.Y + size.Y), bandColor);
            drawList.AddLine(new Vector2(bx0, origin.Y), new Vector2(bx0, origin.Y + size.Y), bandEdge);
            drawList.AddLine(new Vector2(bx1, origin.Y), new Vector2(bx1, origin.Y + size.Y), bandEdge);
        }

        // Ruler: ticks spaced to stay readable, labelled in frames
        drawList.AddLine(new Vector2(FrameToX(0), origin.Y + ruler), new Vector2(FrameToX(last), origin.Y + ruler), lineColor);
        var step = TickStep((int)MathF.Ceiling(ViewFrames), trackWidth / scale);
        for (var f = (int)MathF.Ceiling(ViewStart / step) * step; f <= Math.Min(last, ViewStart + ViewFrames); f += step)
        {
            var x = FrameToX(f);
            drawList.AddLine(new Vector2(x, origin.Y + ruler * 0.55f), new Vector2(x, origin.Y + ruler), lineColor);
            drawList.AddText(new Vector2(x + 2 * scale, origin.Y), dullColor, f.ToString());
        }

        // Where the keys go while a drag moves, scales or copies them
        var keyMap = DragMap(mouse, FrameToX, pixelsPerFrame, last);
        var copying = Drag == DragMode.Keys && io.KeyAlt;

        // Keys; a Hold key (a step into it) is drawn as a square
        var holds = new HashSet<(int, int)>();
        if (clip != null)
        {
            foreach (var (track, list) in clip.Offsets)
            foreach (var key in list)
            {
                if (clip.EaseOf(key) == BoneOffsetEase.Hold)
                    holds.Add((track, key.Frame));
            }
        }

        Drawn.Clear();
        void DrawKeys(int r, IEnumerable<(int Track, int Frame)> keys, float radius, uint color)
        {
            var y = origin.Y + keysTop + (r + 0.5f) * row;
            foreach (var group in keys.GroupBy(k => k.Frame))
            {
                var members = group.ToArray();
                var selected = members.All(SelectedKeys.Contains);
                var anySelected = selected || members.Any(SelectedKeys.Contains);
                var shownFrame = anySelected && keyMap != null ? keyMap(group.Key) : group.Key;

                var square = members.All(holds.Contains);

                // A copy leaves the original where it was
                if (anySelected && keyMap != null && copying)
                    Mark(drawList, new Vector2(FrameToX(group.Key), y), radius, dullColor, square);

                var centre = new Vector2(FrameToX(shownFrame), y);
                if (anySelected)
                    Mark(drawList, centre, radius + 1.5f * scale, keySelectedColor, square);
                Mark(drawList, centre, radius, selected || !anySelected ? color : keySelectedColor, square);
                Drawn.Add((centre, radius, members));
            }
        }

        if (clip != null)
        {
            if (selectedTrack != int.MinValue && clip.Offsets.TryGetValue(selectedTrack, out var own))
                DrawKeys(0, own.Select(k => (selectedTrack, k.Frame)), 5 * scale, keyColor);

            DrawKeys(1, clip.Offsets.SelectMany(o => o.Value.Select(k => (o.Key, k.Frame))), 3.5f * scale, dullColor);

            for (var b = 0; b < boneRows.Count; b++)
                DrawKeys(2 + b, clip.Offsets[boneRows[b]].Select(k => (boneRows[b], k.Frame)), 4 * scale,
                    boneRows[b] == selectedTrack ? keyColor : textColor);
        }

        // Events: bars from start to end in the Time Act graph's colours, the name inside. Times are the time act's
        // own, so after a retime they show where the events now fall on the edited clip. One past the clip's end is
        // drawn as a sliver at the end, outlined.
        AnimationTimeActEvent? hoveredEvent = null;
        var hoveredPast = false;
        if (eventRowCount > 0 && clip is { FrameDuration: > 0 })
        {
            var top = origin.Y + eventsTop;
            for (var r = 0; r < eventRowCount; r += 2)
                drawList.AddRectFilled(new Vector2(origin.X, top + r * eventRow), new Vector2(origin.X + width, top + (r + 1) * eventRow), rowColor);

            var trackEnd = FrameToX(last);
            var minWidth = 3 * scale;

            foreach (var (ev, r) in events)
            {
                var past = ev.Start / clip.FrameDuration > last;
                var x0 = Math.Min(FrameToX(ev.Start / clip.FrameDuration), trackEnd - minWidth);
                var x1 = ev.OpenEnded ? trackEnd : Math.Min(FrameToX(ev.End / clip.FrameDuration), trackEnd);
                var min = new Vector2(x0, top + r * eventRow + 1);
                var max = new Vector2(Math.Max(x1, x0 + minWidth), top + (r + 1) * eventRow - 1);

                drawList.AddRectFilled(min, max, TimeActGraphWindow.GetEventColor(ev.Type));
                if (past)
                    drawList.AddRect(min, max, ImGui.ColorConvertFloat4ToU32(UI.Current.ImGui_Warning_Text_Color));

                drawList.PushClipRect(Vector2.Max(min, new Vector2(trackLeft - pad, min.Y)), Vector2.Min(max, new Vector2(trackRight + pad, max.Y)), true);
                drawList.AddText(min + new Vector2(3 * scale, 1 * scale), textColor, TimeActs!.NameOf(ev.Type));
                drawList.PopClipRect();

                if (hovered && mouse.X >= min.X && mouse.X <= max.X && mouse.Y >= min.Y && mouse.Y <= max.Y &&
                    mouse.X >= trackLeft - pad && mouse.X <= trackRight + pad)
                {
                    hoveredEvent = ev;
                    hoveredPast = past;
                }
            }
        }

        // Playhead
        var playX = FrameToX(MathF.Min(animator.Frame, last));
        drawList.AddLine(new Vector2(playX, origin.Y), new Vector2(playX, origin.Y + size.Y), playColor, 2 * scale);

        // The box being dragged out
        if (Drag == DragMode.Box)
        {
            var (bmin, bmax) = (Vector2.Min(DragStart, mouse), Vector2.Max(DragStart, mouse));
            drawList.AddRectFilled(bmin, bmax, boxColor);
            drawList.AddRect(bmin, bmax, boxLineColor);
        }

        drawList.PopClipRect();

        // ---- input

        var inKeyRows = mouse.Y >= origin.Y + keysTop && mouse.Y < origin.Y + keysBottom;
        var hoveredKey = hovered && inKeyRows ? HitKey(mouse) : null;

        if (ImGui.IsItemClicked(ImGuiMouseButton.Middle))
        {
            Drag = DragMode.Pan;
            DragStart = mouse;
            PanStartView = ViewStart;
        }
        else if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            DragStart = mouse;
            if (mouse.X < trackLeft - pad)
            {
                // A row's name selects that bone
                if (inKeyRows && clip != null)
                    SelectRowBone(clip, (int)((mouse.Y - origin.Y - keysTop) / row), selectedTrack, boneRows);
            }
            else if (inKeyRows && clip != null && hoveredKey is { } pressed)
            {
                PressedKeys = pressed;
                PressedSelected = pressed.All(SelectedKeys.Contains);

                if (io.KeyCtrl)
                {
                    if (PressedSelected)
                        SelectedKeys.ExceptWith(pressed);
                    else
                        SelectedKeys.UnionWith(pressed);
                }
                else if (!PressedSelected)
                {
                    SelectedKeys.Clear();
                    SelectedKeys.UnionWith(pressed);
                }

                // Ctrl+click only picks; a drag of what is selected moves it
                Drag = io.KeyCtrl ? DragMode.None : DragMode.Keys;
            }
            else if (inKeyRows && clip != null)
                Drag = DragMode.Box;
            else if (io.KeyShift && mouse.Y < origin.Y + ruler)
            {
                // Shift on the ruler picks a range
                Drag = DragMode.Range;
                RangeAnchor = FrameAt(mouse.X);
                SetRange(RangeAnchor, RangeAnchor);
            }
            else
                Drag = DragMode.Scrub;
        }

        // Right-click on a key: a menu for the selection (the key alone if it wasn't selected)
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && Drag == DragMode.None && hoveredKey is { } clicked && clip != null)
        {
            if (!clicked.All(SelectedKeys.Contains))
            {
                SelectedKeys.Clear();
                SelectedKeys.UnionWith(clicked);
            }

            ImGui.OpenPopup("##animationTimelineKeyMenu");
        }

        if (KeyMenuPending)
        {
            KeyMenuPending = false;
            if (SelectedKeys.Count > 0)
                ImGui.OpenPopup("##animationTimelineKeyMenu");
        }

        if (ImGui.BeginPopup("##animationTimelineKeyMenu"))
        {
            if (clip != null && View.SelectedClip is { } menuSource && SelectedKeys.Count > 0)
                DisplayKeyMenu(menuSource, clip);
            else
                ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }

        if (Drag != DragMode.None && ImGui.IsKeyPressed(ImGuiKey.Escape, false))
            Drag = DragMode.None;

        switch (Drag)
        {
            case DragMode.Scrub:
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    SeekTo(FrameAt(mouse.X));
                else
                    Drag = DragMode.None;
                break;

            case DragMode.Range:
                if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    var to = FrameAt(mouse.X);
                    if (Range is not { } now || (Math.Min(RangeAnchor, to), Math.Max(RangeAnchor, to)) != now)
                        SetRange(RangeAnchor, to);
                }
                else
                {
                    Drag = DragMode.None;

                    // A Shift+click without a drag is no range
                    if (Range is { } picked && picked.First == picked.Last)
                        ClearRange();
                }
                break;

            case DragMode.Pan:
                if (ImGui.IsMouseDown(ImGuiMouseButton.Middle))
                {
                    ViewStart = PanStartView - (mouse.X - DragStart.X) / pixelsPerFrame;
                    PanView(0, last);
                }
                else
                    Drag = DragMode.None;
                break;

            case DragMode.Box:
                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    Drag = DragMode.None;
                    if (Vector2.Distance(DragStart, mouse) < 3 * scale)
                    {
                        // A click on an empty row goes to that frame and drops the selection
                        if (!io.KeyCtrl)
                            SelectedKeys.Clear();
                        SeekTo(FrameAt(mouse.X));
                    }
                    else
                    {
                        if (!io.KeyCtrl)
                            SelectedKeys.Clear();
                        var (bmin, bmax) = (Vector2.Min(DragStart, mouse), Vector2.Max(DragStart, mouse));
                        foreach (var (centre, _, keys) in Drawn)
                        {
                            if (centre.X >= bmin.X && centre.X <= bmax.X && centre.Y >= bmin.Y && centre.Y <= bmax.Y)
                                SelectedKeys.UnionWith(keys);
                        }
                    }
                }
                break;

            case DragMode.Keys:
                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    Drag = DragMode.None;
                    if (Vector2.Distance(DragStart, mouse) < 3 * scale)
                    {
                        // A click on a key: just that key (or frame of keys), and the playhead to it
                        if (PressedSelected && PressedKeys != null)
                        {
                            SelectedKeys.Clear();
                            SelectedKeys.UnionWith(PressedKeys);
                        }

                        if (PressedKeys is { Length: > 0 } keys)
                            View.ViewportWindow.SeekFrame(keys[0].Frame);
                    }
                    else if (keyMap != null && clip != null && View.SelectedClip is { } source)
                        ApplyDrag(source, clip, keyMap, io.KeyShift, io.KeyAlt);
                }
                break;
        }

        // ---- hover

        if (hovered && Drag == DragMode.None)
        {
            if (hoveredKey is { } keys && clip != null)
            {
                var what = keys.Length == 1 ? TrackName(clip, keys[0].Track) : LOC.Get("ANIM_Timeline_Key_Count", keys.Length);
                ImGui.SetTooltip(LOC.Get("ANIM_Timeline_Hover_Key", what, keys[0].Frame));
            }
            else if (hoveredEvent is { } shown && clip is { FrameDuration: > 0 })
            {
                var start = (shown.Start / clip.FrameDuration).ToString("0.#");
                var end = shown.OpenEnded ? LOC.Get("ANIM_Timeline_Event_Open_End") : (shown.End / clip.FrameDuration).ToString("0.#");
                var text = LOC.Get("ANIM_Timeline_Event_Hover", TimeActs!.NameOf(shown.Type), start, end);
                if (hoveredPast)
                    text += "\n" + LOC.Get("ANIM_Timeline_Event_Past_End");

                ImGui.SetTooltip(text);
            }
            else if (mouse.X >= trackLeft - pad)
                GUI.Tooltip(LOC.Get(inKeyRows ? "ANIM_Timeline_Hover_Rows" : "ANIM_Timeline_Hover_Frame", FrameAt(mouse.X)));
            else if (inKeyRows)
                GUI.Tooltip(LOC.Get("ANIM_Timeline_Hover_Row_Name"));
        }

        DisplayPanBar(trackLeft, trackWidth, last);
        DisplayFooter(eventStatus);

        // The curve view, lined up with the bar
        if (CFG.Current.Interface.Interface_AnimationEditor_Timeline_Curves && clip != null && View.SelectedClip is { } curveSource)
        {
            Curves.Display(curveSource, clip, selectedTrack, selectedTrack == int.MinValue ? "" : TrackName(clip, selectedTrack),
                new AnimationCurveView.Frame(origin.X, trackLeft, trackWidth, ViewStart, ViewFrames, last));

            if (ImGui.IsItemHovered() && io.MouseWheel != 0)
                Zoom(XToFrame(ImGui.GetMousePos().X), io.MouseWheel, io.KeyShift, last);
        }
    }

    /// <summary>The right-click menu on the selected keys: their ease, copy, delete</summary>
    private void DisplayKeyMenu(AnimationClipSource source, EditableClip clip)
    {
        ImGui.TextDisabled(LOC.Get("ANIM_Timeline_Selected_Keys", SelectedKeys.Count));
        ImGui.Separator();

        if (ImGui.BeginMenu($"{LOC.Get("ANIM_Timeline_Keys_Ease")}##animationTimelineKeyEase"))
        {
            // Checked when every selected key has that ease
            var eases = SelectedKeys
                .Select(k => clip.Offsets.TryGetValue(k.Track, out var l) ? l.Find(x => x.Frame == k.Frame).Ease : null)
                .Distinct().ToList();

            foreach (var option in AnimationBonesView.EaseChoices)
            {
                var label = AnimationBonesView.EaseLabel(clip, option);
                if (ImGui.MenuItem($"{label}##animationTimelineKeyEase_{option}", "", eases.Count == 1 && eases[0] == option))
                {
                    View.Execute(new AnimationClipEditAction(View.Bank, source, clip.WithKeyEase(SelectedKeys.ToList(), option),
                        LOC.Get("ANIM_Action_Keys_Ease", SelectedKeys.Count, source.Name, label)));
                }
            }

            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("ANIM_Bones_Key_Ease_TT"));

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Timeline_Keys_Copy")}##animationTimelineKeyCopy", InputManager.GetHint(KeybindID.Copy)))
            CopyKeys(source, clip);

        if (ImGui.MenuItem($"{LOC.Get("ANIM_Timeline_Keys_Delete")}##animationTimelineKeyDelete", InputManager.GetHint(KeybindID.Delete)))
            DeleteKeys(source, clip);
    }

    /// <summary>The keys under the mouse: the nearest diamond within its radius and a few pixels</summary>
    private (int Track, int Frame)[]? HitKey(Vector2 mouse)
    {
        var slack = 3 * DPI.UIScale();
        (int, int)[]? best = null;
        var bestDistance = float.MaxValue;
        foreach (var (centre, radius, keys) in Drawn)
        {
            var d = Vector2.Distance(centre, mouse);
            if (d <= radius + slack && d < bestDistance)
            {
                best = keys;
                bestDistance = d;
            }
        }

        return best;
    }

    /// <summary>
    /// Where a drag of the selected keys puts each one (by its frame now), or null when no drag is under way: moved
    /// by whole frames, kept inside the clip; with Shift, scaled about the playhead.
    /// </summary>
    private Func<int, int>? DragMap(Vector2 mouse, Func<float, float> frameToX, float pixelsPerFrame, int last)
    {
        if (Drag != DragMode.Keys || SelectedKeys.Count == 0 || Vector2.Distance(DragStart, mouse) < 3 * DPI.UIScale())
            return null;

        if (ImGui.GetIO().KeyShift)
        {
            var pivot = View.ViewportWindow.CurrentWholeFrame;
            var from = DragStart.X - frameToX(pivot);
            if (MathF.Abs(from) < 2)
                return f => f;

            var factor = Math.Max((mouse.X - frameToX(pivot)) / from, 0);
            return f => Math.Clamp((int)MathF.Round(pivot + (f - pivot) * factor), 0, last);
        }

        var delta = (int)MathF.Round((mouse.X - DragStart.X) / pixelsPerFrame);
        var first = SelectedKeys.Min(k => k.Frame);
        var end = SelectedKeys.Max(k => k.Frame);
        delta = Math.Clamp(delta, -first, last - end);
        return f => f + delta;
    }

    private void ApplyDrag(AnimationClipSource source, EditableClip clip, Func<int, int> map, bool scaling, bool copy)
    {
        var count = SelectedKeys.Count;
        string name;
        if (scaling)
            name = LOC.Get("ANIM_Action_Scale_Keys", count, View.ViewportWindow.CurrentWholeFrame, source.Name);
        else
        {
            var delta = map(SelectedKeys.First().Frame) - SelectedKeys.First().Frame;
            name = LOC.Get(copy ? "ANIM_Action_Copy_Keys" : "ANIM_Action_Move_Keys", count, delta, source.Name);
        }

        RemapKeys(source, clip, map, copy, name);
    }

    private void NudgeKeys(AnimationClipSource source, int delta, int last)
    {
        if (View.Bank.Get(source) is not { } clip || SelectedKeys.Count == 0)
            return;

        delta = Math.Clamp(delta, -SelectedKeys.Min(k => k.Frame), last - SelectedKeys.Max(k => k.Frame));
        if (delta == 0)
            return;

        RemapKeys(source, clip, f => f + delta, false, LOC.Get("ANIM_Action_Move_Keys", SelectedKeys.Count, delta, source.Name));
    }

    /// <summary>
    /// Moves (or copies) the selected keys to the frames <paramref name="map" /> gives, as one undo step. A key of
    /// the same track already at a destination frame is replaced. Refused when two keys of a track would land on one
    /// frame (a scale squeezing them together).
    /// </summary>
    private void RemapKeys(AnimationClipSource source, EditableClip clip, Func<int, int> map, bool copy, string name)
    {
        var result = clip.Clone();
        var placed = new HashSet<(int, int)>();
        var changed = false;

        foreach (var group in SelectedKeys.GroupBy(k => k.Track))
        {
            if (!result.Offsets.TryGetValue(group.Key, out var keys))
                continue;

            var frames = group.Select(k => k.Frame).ToHashSet();
            var picked = keys.Where(k => frames.Contains(k.Frame)).ToList();
            var targets = picked.Select(k => k with { Frame = map(k.Frame) }).ToList();

            if (targets.Select(k => k.Frame).Distinct().Count() != targets.Count)
            {
                Smithbox.Log(this, LOC.Get("ANIM_Timeline_Scale_Collision", TrackName(clip, group.Key)), LogLevel.Warning);
                return;
            }

            changed |= targets.Where((t, i) => t.Frame != picked[i].Frame).Any();

            if (!copy)
                keys.RemoveAll(k => frames.Contains(k.Frame));

            var destinations = targets.Select(t => t.Frame).ToHashSet();
            keys.RemoveAll(k => destinations.Contains(k.Frame));
            keys.AddRange(targets);
            keys.Sort((a, b) => a.Frame.CompareTo(b.Frame));

            foreach (var t in targets)
                placed.Add((group.Key, t.Frame));
        }

        if (!changed)
            return;

        View.Execute(new AnimationClipEditAction(View.Bank, source, result, name));
        SelectedKeys.Clear();
        SelectedKeys.UnionWith(placed);
    }

    private void DeleteKeys(AnimationClipSource source, EditableClip clip)
    {
        var result = clip.Clone();
        var count = 0;
        foreach (var group in SelectedKeys.GroupBy(k => k.Track))
        {
            if (!result.Offsets.TryGetValue(group.Key, out var keys))
                continue;

            var frames = group.Select(k => k.Frame).ToHashSet();
            count += keys.RemoveAll(k => frames.Contains(k.Frame));
            if (keys.Count == 0)
                result.Offsets.Remove(group.Key);
        }

        if (count == 0)
            return;

        View.Execute(new AnimationClipEditAction(View.Bank, source, result, LOC.Get("ANIM_Action_Delete_Keys", count, source.Name)));
        SelectedKeys.Clear();
    }

    /// <summary>Copies the selected keys by bone name, their frames from the first one, so they paste into any clip</summary>
    private void CopyKeys(AnimationClipSource source, EditableClip clip)
    {
        var first = SelectedKeys.Min(k => k.Frame);
        KeyClipboard = new List<(string, int, BoneOffsetKey)>();
        foreach (var (track, frame) in SelectedKeys.OrderBy(k => k.Frame))
        {
            if (clip.Offsets.TryGetValue(track, out var keys) && keys.FindIndex(k => k.Frame == frame) is var i and >= 0)
                KeyClipboard.Add((BoneNameOf(clip, track), frame - first, keys[i]));
        }

        Smithbox.Log(this, LOC.Get("ANIM_Timeline_Keys_Copied", KeyClipboard.Count, source.Name));
    }

    /// <summary>
    /// Pastes the copied keys from the playhead on, each on the bone of the same name (replacing a key there), as one
    /// undo step. Bones without a track here and keys past the clip's end are passed over.
    /// </summary>
    private void PasteKeys(AnimationClipSource source, EditableClip clip, int at)
    {
        if (KeyClipboard is not { Count: > 0 } copied)
            return;

        var tracks = new Dictionary<string, int> { [RootName] = EditableClip.RootTrack };
        for (var t = 0; t < clip.TrackCount; t++)
            tracks.TryAdd(BoneNameOf(clip, t), t);

        var result = clip;
        var placed = new HashSet<(int, int)>();
        var missing = new HashSet<string>();
        foreach (var (bone, offset, key) in copied)
        {
            var frame = at + offset;
            if (frame >= clip.FrameCount)
                continue;

            if (!tracks.TryGetValue(bone, out var track))
            {
                missing.Add(bone);
                continue;
            }

            result = result.WithKey(track, key with { Frame = frame });
            placed.Add((track, frame));
        }

        if (missing.Count > 0)
            Smithbox.Log(this, LOC.Get("ANIM_Timeline_Keys_Paste_Missing", source.Name, string.Join(", ", missing)), LogLevel.Warning);

        if (placed.Count == 0)
            return;

        View.Execute(new AnimationClipEditAction(View.Bank, source, result, LOC.Get("ANIM_Action_Paste_Keys", placed.Count, at, source.Name)));
        SelectedKeys.Clear();
        SelectedKeys.UnionWith(placed);
    }

    private void SelectRowBone(EditableClip clip, int r, int selectedTrack, List<int> boneRows)
    {
        if (r < 2 || r - 2 >= boneRows.Count)
            return;

        var track = boneRows[r - 2];
        if (track == selectedTrack)
            return;

        View.SelectBone(track == EditableClip.RootTrack ? AnimationEditorView.RootBone : clip.BoneOf(track),
            ImGui.GetIO().KeyCtrl && track != EditableClip.RootTrack);
    }

    // ---- view range

    private void FitView(int last)
    {
        ViewLast = last;
        ViewStart = 0;
        ViewFrames = Math.Max(last, 1);
    }

    private void PanView(float by, int last)
    {
        ViewStart = Math.Clamp(ViewStart + by, 0, Math.Max(last - ViewFrames, 0));
    }

    /// <summary>A thin bar under the Timeline while zoomed: the shown part as a thumb, dragged or clicked to pan</summary>
    private void DisplayPanBar(float trackLeft, float trackWidth, int last)
    {
        if (ViewFrames >= last - 0.01f)
            return;

        var scale = DPI.UIScale();
        var height = 8 * scale;
        var start = ImGui.GetCursorScreenPos();
        ImGui.SetCursorScreenPos(new Vector2(trackLeft, start.Y + 2 * scale));
        var origin = ImGui.GetCursorScreenPos();
        ImGui.InvisibleButton("##animationTimelinePan", new Vector2(trackWidth, height));
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Pan_TT"));

        if (ImGui.IsItemActive())
        {
            var at = (ImGui.GetMousePos().X - origin.X) / trackWidth * last;
            ViewStart = at - ViewFrames / 2;
            PanView(0, last);
        }

        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(origin, origin + new Vector2(trackWidth, height), ImGui.GetColorU32(ImGuiCol.ScrollbarBg), height / 2);
        var x0 = origin.X + ViewStart / last * trackWidth;
        var x1 = origin.X + (ViewStart + ViewFrames) / last * trackWidth;
        drawList.AddRectFilled(new Vector2(x0, origin.Y), new Vector2(Math.Max(x1, x0 + height), origin.Y + height),
            ImGui.GetColorU32(ImGui.IsItemHovered() || ImGui.IsItemActive() ? ImGuiCol.ScrollbarGrabHovered : ImGuiCol.ScrollbarGrab), height / 2);
    }

    private void DisplayFooter(string? eventStatus)
    {
        if (ImGui.SmallButton($"{LOC.Get("ANIM_Timeline_Fit")}##animationTimelineFit"))
            FitView(ViewLast);
        GUI.Tooltip(LOC.Get("ANIM_Timeline_Fit_TT"));

        if (Previewing)
        {
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(1f, 0.6f, 0.2f, 1f), LOC.Get("ANIM_Timeline_Transition", View.ViewportWindow.LeadIn));
            GUI.Tooltip(LOC.Get("ANIM_Timeline_Transition_TT"));
        }


        // The keys shown are the active layer's
        if (View.SelectedClip is { } shown && View.Bank.Get(shown) is { Layers.Count: > 1 } layered)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("ANIM_Timeline_Layer", layered.Active.Name));
            GUI.Tooltip(LOC.Get("ANIM_Timeline_Layer_TT"));
        }

        if (Range is { } range)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("ANIM_Timeline_Range", range.First, range.Last, range.Last - range.First));
            GUI.Tooltip(LOC.Get("ANIM_Timeline_Range_TT"));
            ImGui.SameLine();
            if (ImGui.SmallButton($"{LOC.Get("ANIM_Timeline_Range_Clear")}##animationTimelineRangeClear"))
                ClearRange();
        }

        if (SelectedKeys.Count > 0)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("ANIM_Timeline_Selected_Keys", SelectedKeys.Count));
            GUI.Tooltip(LOC.Get("ANIM_Timeline_Selection_Hint"));
        }

        if (CFG.Current.Interface.Interface_AnimationEditor_Timeline_Events)
        {
            // Time act edits are seen here once saved in the Time Act Editor and read again
            ImGui.SameLine();
            ImGui.BeginDisabled(View.IsCheckingGravity);
            if (ImGui.SmallButton($"{LOC.Get("ANIM_Timeline_Events_Reload")}##animationTimelineEventsReload"))
                View.CheckGravityAgain();
            ImGui.EndDisabled();
            GUI.Tooltip(LOC.Get("ANIM_Timeline_Events_Reload_TT"));

            if (eventStatus != null)
            {
                ImGui.SameLine();
                ImGui.TextDisabled(eventStatus);
            }
        }
    }

    // ---- time act events

    private AnimationTimeActs? TimeActs => View.Bank.Character?.TimeActs;

    /// <summary>
    /// The selected clip's time act events placed in rows: grouped by type in type order, an event going on the first
    /// row of its type it doesn't overlap (as the Time Act Editor's graph lays them out). The status says when there
    /// is nothing to show, or where imported events came from; null otherwise.
    /// </summary>
    private List<(AnimationTimeActEvent Event, int Row)> EventRows(EditableClip? clip, out int rowCount, out string? status)
    {
        var placed = new List<(AnimationTimeActEvent, int)>();
        rowCount = 0;
        status = null;

        if (!CFG.Current.Interface.Interface_AnimationEditor_Timeline_Events || clip == null || View.SelectedClip is not { } source)
            return placed;

        if (TimeActs is not { } acts)
        {
            status = LOC.Get("ANIM_Timeline_Events_Loading");
            return placed;
        }

        if (acts.For(source.AnimationId) is not { } events)
        {
            status = LOC.Get("ANIM_Timeline_Events_None");
            return placed;
        }

        if (acts.ImportedFrom(source.AnimationId) is { } from)
        {
            var name = View.Bank.Character?.Clips.Find(c => c.AnimationId == from)?.Name ?? from.ToString();
            status = LOC.Get("ANIM_Timeline_Events_Imported", name);
        }

        if (events.Count == 0)
        {
            status ??= LOC.Get("ANIM_Timeline_Events_Empty");
            return placed;
        }

        foreach (var type in events.GroupBy(e => e.Type).OrderBy(g => g.Key))
        {
            var rowEnds = new List<float>();
            foreach (var ev in type.OrderBy(e => e.Start))
            {
                var r = rowEnds.FindIndex(end => end <= ev.Start + 1e-4f);
                if (r < 0)
                {
                    r = rowEnds.Count;
                    rowEnds.Add(0);
                }

                // An event takes at least a frame, however short it is; an open-ended one the rest of the row
                rowEnds[r] = ev.OpenEnded ? float.MaxValue : Math.Max(ev.End, ev.Start + clip.FrameDuration);
                placed.Add((ev, rowCount + r));
            }

            rowCount += rowEnds.Count;
        }

        return placed;
    }

    /// <summary>A key mark: a diamond, or a square for a Hold key</summary>
    private static void Mark(ImDrawListPtr drawList, Vector2 c, float r, uint color, bool square)
    {
        if (square)
            drawList.AddRectFilled(c - new Vector2(r * 0.8f), c + new Vector2(r * 0.8f), color);
        else
            Diamond(drawList, c, r, color);
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

    // ---- tracks

    /// <summary>A track's bone name as shown (Root Motion for the root)</summary>
    private string TrackName(EditableClip clip, int track) =>
        track == EditableClip.RootTrack ? LOC.Get("ANIM_Bones_Root") : BoneNameOf(clip, track);

    /// <summary>A track's bone name in the skeleton, as the clipboard keeps it (RootName for the root)</summary>
    private string BoneNameOf(EditableClip clip, int track)
    {
        if (track == EditableClip.RootTrack)
            return RootName;

        var bone = clip.BoneOf(track);
        return View.Bank.Character is { } character && bone >= 0 && bone < character.Skeleton.BoneCount
            ? character.Skeleton.Names[bone]
            : $"#{track}";
    }

    /// <summary>The selected bone's track (the root's with the root motion selected), int.MinValue for none</summary>
    private int SelectedTrack(EditableClip? clip)
    {
        if (clip != null && View.RootSelected)
            return EditableClip.RootTrack;

        if (clip == null || View.Bank.Character is not { } character || View.SelectedBone < 0 ||
            View.SelectedBone >= character.Skeleton.BoneCount)
            return int.MinValue;

        // A bone without a track is none (its -1 would read as the root)
        var track = clip.BoneToTrack(character.Skeleton.BoneCount)[View.SelectedBone];
        return track >= 0 ? track : int.MinValue;
    }

    private List<int> SelectedKeyFrames(EditableClip? clip)
    {
        var track = SelectedTrack(clip);
        return clip != null && (track >= 0 || track == EditableClip.RootTrack) && clip.Offsets.TryGetValue(track, out var keys)
            ? keys.Select(k => k.Frame).ToList()
            : new List<int>();
    }
}
