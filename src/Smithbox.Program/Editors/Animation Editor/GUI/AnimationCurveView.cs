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
/// The Timeline's curve view (§13.3 item 9): the selected bone's (or the root motion's) channels over the clip, drawn
/// under the key rows with the Timeline's zoom and pan. Channels are turn X / Y / Z (degrees about the parent's axes, as
/// the Bones tool's Turn fields) and move X / Y / Z, unwrapped across ±180. Two modes:
/// <list type="bullet">
/// <item>Result: the bone's local transform as it plays (every layer on).</item>
/// <item>Offset: the active layer's offset (its keys as typed).</item>
/// </list>
/// The active layer's keys are handles: dragging one up or down changes that channel of the key (in Result mode so the
/// result reaches the value, through the other layers), previewed live, one undo step on release; Shift for fine
/// steps, Escape cancels. Right-click a handle for the key's ease. Ease is per key (no free tangents), so the curve
/// between keys is what the ease gives.
/// </summary>
public class AnimationCurveView
{
    private readonly AnimationEditorView View;
    private readonly AnimationTimelineView Timeline;

    /// <summary>Where the Timeline's bar puts frames, so the curves line up with it</summary>
    public readonly record struct Frame(float GutterLeft, float TrackLeft, float TrackWidth, float ViewStart, float ViewFrames, int Last);

    private static readonly string[] ChannelKeys =
        ["ANIM_Curves_Turn_X", "ANIM_Curves_Turn_Y", "ANIM_Curves_Turn_Z", "ANIM_Curves_Move_X", "ANIM_Curves_Move_Y", "ANIM_Curves_Move_Z"];

    private static readonly Vector4[] ChannelColors =
    [
        new(0.95f, 0.35f, 0.35f, 1), new(0.40f, 0.90f, 0.40f, 1), new(0.40f, 0.60f, 1.00f, 1),
        new(1.00f, 0.65f, 0.55f, 1), new(0.70f, 1.00f, 0.65f, 1), new(0.65f, 0.80f, 1.00f, 1)
    ];

    // Values per channel per frame, made once per clip, track and mode
    private float[][]? Values;
    private EditableClip? ValuesFor;
    private int ValuesTrack = int.MinValue;
    private bool ValuesOffset;

    // A handle being dragged: the clip it started on (previews are made from it and it is put back before the step)
    private EditableClip? DragStart;
    private bool DragStartEdited;
    private AnimationClipSource? DragSource;
    private int DragTrack;
    private int DragFrame;
    private int DragChannel;
    private float DragValue;
    private float DragMouseY;
    private float DragUnitsPerPixel;
    private (float Min, float Max)? FrozenRange;

    public AnimationCurveView(AnimationEditorView view, AnimationTimelineView timeline)
    {
        View = view;
        Timeline = timeline;
    }

    private static bool Shown(int channel) => (CFG.Current.Interface.Interface_AnimationEditor_Curves_Channels & (1 << channel)) != 0;

    public void Display(AnimationClipSource source, EditableClip clip, int track, string trackName, Frame frame)
    {
        var scale = DPI.UIScale();
        var offsetMode = CFG.Current.Interface.Interface_AnimationEditor_Curves_Offset;

        // Header: mode and channel toggles
        ImGui.TextDisabled(LOC.Get("ANIM_Curves_Of", trackName));
        ImGui.SameLine();
        if (ImGui.RadioButton($"{LOC.Get("ANIM_Curves_Result")}##animationCurvesResult", !offsetMode))
            CFG.Current.Interface.Interface_AnimationEditor_Curves_Offset = offsetMode = false;
        GUI.Tooltip(LOC.Get("ANIM_Curves_Result_TT"));
        ImGui.SameLine();
        if (ImGui.RadioButton($"{LOC.Get("ANIM_Curves_Offset")}##animationCurvesOffset", offsetMode))
            CFG.Current.Interface.Interface_AnimationEditor_Curves_Offset = offsetMode = true;
        GUI.Tooltip(LOC.Get("ANIM_Curves_Offset_TT"));

        for (var c = 0; c < 6; c++)
        {
            ImGui.SameLine();
            var on = Shown(c);
            ImGui.PushStyleColor(ImGuiCol.Text, ChannelColors[c]);
            if (ImGui.Checkbox($"{LOC.Get(ChannelKeys[c])}##animationCurveChannel_{c}", ref on))
                CFG.Current.Interface.Interface_AnimationEditor_Curves_Channels ^= 1 << c;
            ImGui.PopStyleColor();
        }

        if (track == int.MinValue)
        {
            ImGui.TextDisabled(LOC.Get("ANIM_Curves_No_Bone"));
            return;
        }

        var basis = DragStart ?? clip;
        if (!ReferenceEquals(ValuesFor, clip) || ValuesTrack != track || ValuesOffset != offsetMode)
        {
            Values = Channels(clip, track, offsetMode);
            ValuesFor = clip;
            ValuesTrack = track;
            ValuesOffset = offsetMode;
        }

        var values = Values!;
        var keys = basis.Offsets.TryGetValue(track, out var list) ? list : new List<BoneOffsetKey>();
        var root = track == EditableClip.RootTrack;

        // The plot: the Timeline's width, the rest of the window's height
        var height = Math.Max(ImGui.GetContentRegionAvail().Y - 2 * scale, 120 * scale);
        var origin = new Vector2(frame.GutterLeft, ImGui.GetCursorScreenPos().Y);
        var width = frame.TrackLeft + frame.TrackWidth - frame.GutterLeft + 6 * scale;
        ImGui.InvisibleButton("##animationCurves", new Vector2(width, height), ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight);
        var hovered = ImGui.IsItemHovered();
        var mouse = ImGui.GetMousePos();

        float FrameToX(float f) => frame.TrackLeft + (f - frame.ViewStart) / frame.ViewFrames * frame.TrackWidth;

        // Value range: every shown channel's, kept while a handle is dragged (two groups: turns and moves share one
        // axis each, so each is fitted on its own half when both are shown)
        var turnShown = Enumerable.Range(0, 3).Any(Shown);
        var moveShown = Enumerable.Range(3, 3).Any(Shown);
        var top = origin.Y + 4 * scale;
        var bottom = origin.Y + height - 4 * scale;
        var split = turnShown && moveShown ? (top + bottom) / 2 : bottom;
        (float Top, float Bottom) Band(int c) => c < 3 ? (top, turnShown && moveShown ? split - 3 * scale : bottom)
            : (turnShown && moveShown ? split + 3 * scale : top, bottom);

        (float Min, float Max) Range(int group)
        {
            if (FrozenRange is { } frozen && DragStart != null && DragChannel / 3 == group)
                return frozen;

            var min = float.MaxValue;
            var max = float.MinValue;
            for (var c = group * 3; c < group * 3 + 3; c++)
            {
                if (!Shown(c))
                    continue;
                foreach (var v in values[c])
                {
                    min = Math.Min(min, v);
                    max = Math.Max(max, v);
                }
            }

            if (min > max)
                return (-1, 1);

            var pad = Math.Max((max - min) * 0.08f, group == 0 ? 1f : 0.005f);
            return (min - pad, max + pad);
        }

        var ranges = new[] { Range(0), Range(1) };
        float ToY(int c, float v)
        {
            var (bt, bb) = Band(c);
            var (lo, hi) = ranges[c / 3];
            return bb - (v - lo) / (hi - lo) * (bb - bt);
        }

        var drawList = ImGui.GetWindowDrawList();
        var lineColor = ImGui.GetColorU32(ImGuiCol.Border);
        var dullColor = ImGui.GetColorU32(ImGuiCol.TextDisabled);
        drawList.AddRectFilled(origin, origin + new Vector2(width, height), ImGui.GetColorU32(ImGuiCol.FrameBg));

        // Axis labels in the gutter: each group's range, and its zero line
        for (var g = 0; g < 2; g++)
        {
            if (g == 0 ? !turnShown : !moveShown)
                continue;

            var c = g * 3;
            var (lo, hi) = ranges[g];
            var format = g == 0 ? "0.#°" : "0.###";
            drawList.AddText(new Vector2(origin.X + 4 * scale, Band(c).Top), dullColor, hi.ToString(format));
            drawList.AddText(new Vector2(origin.X + 4 * scale, Band(c).Bottom - ImGui.GetTextLineHeight()), dullColor, lo.ToString(format));
            if (lo < 0 && hi > 0)
                drawList.AddLine(new Vector2(frame.TrackLeft, ToY(c, 0)), new Vector2(frame.TrackLeft + frame.TrackWidth, ToY(c, 0)), lineColor);
        }

        if (turnShown && moveShown)
            drawList.AddLine(new Vector2(origin.X, split), new Vector2(origin.X + width, split), lineColor);

        drawList.PushClipRect(new Vector2(frame.TrackLeft - 6 * scale, origin.Y), new Vector2(frame.TrackLeft + frame.TrackWidth + 6 * scale, origin.Y + height), true);

        // The curves, frame to frame, only over the frames in view
        var firstFrame = Math.Max((int)MathF.Floor(frame.ViewStart) - 1, 0);
        var lastFrame = Math.Min((int)MathF.Ceiling(frame.ViewStart + frame.ViewFrames) + 1, values[0].Length - 1);
        for (var c = 0; c < 6; c++)
        {
            if (!Shown(c) || root && (c == 0 || c == 2))
                continue;

            var color = ImGui.ColorConvertFloat4ToU32(ChannelColors[c]);
            for (var f = firstFrame; f < lastFrame; f++)
                drawList.AddLine(new Vector2(FrameToX(f), ToY(c, values[c][f])), new Vector2(FrameToX(f + 1), ToY(c, values[c][f + 1])), color, 1.5f * scale);
        }

        // Playhead
        var playX = FrameToX(View.ViewportWindow.CurrentWholeFrame);
        drawList.AddLine(new Vector2(playX, origin.Y), new Vector2(playX, origin.Y + height), ImGui.GetColorU32(ImGuiCol.CheckMark), 1.5f * scale);

        // Handles at the active layer's keys, on each shown channel
        (int Channel, int Frame)? hoveredHandle = null;
        var radius = 4 * scale;
        foreach (var key in keys)
        {
            if (key.Frame >= values[0].Length)
                continue;

            for (var c = 0; c < 6; c++)
            {
                if (!Shown(c) || root && (c == 0 || c == 2))
                    continue;

                var v = DragStart != null && DragFrame == key.Frame && DragChannel == c ? DragValue : values[c][key.Frame];
                var centre = new Vector2(FrameToX(key.Frame), ToY(c, v));
                var hot = hovered && Vector2.Distance(centre, mouse) <= radius + 3 * scale;
                if (hot && hoveredHandle == null)
                    hoveredHandle = (c, key.Frame);

                var hold = basis.EaseOf(key) == BoneOffsetEase.Hold;
                var fill = ImGui.ColorConvertFloat4ToU32(ChannelColors[c]);
                if (hold)
                    drawList.AddRectFilled(centre - new Vector2(radius), centre + new Vector2(radius), fill);
                else
                    drawList.AddCircleFilled(centre, radius, fill);
                if (hot)
                    drawList.AddCircle(centre, radius + 2 * scale, ImGui.GetColorU32(ImGuiCol.Text), 0, 1.5f * scale);
            }
        }

        drawList.PopClipRect();

        // ---- input

        var weightless = basis.ActiveWeight(track) < 1e-4f;
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && hoveredHandle is { } grab && !weightless)
        {
            DragStart = clip;
            DragStartEdited = View.Bank.IsEdited(source);
            DragSource = source;
            DragTrack = track;
            DragFrame = grab.Frame;
            DragChannel = grab.Channel;
            DragValue = values[grab.Channel][grab.Frame];
            DragMouseY = mouse.Y;
            var (bt, bb) = Band(grab.Channel);
            var (lo, hi) = ranges[grab.Channel / 3];
            DragUnitsPerPixel = (hi - lo) / Math.Max(bb - bt, 1);
            FrozenRange = (lo, hi);
            View.ViewportWindow.SeekFrame(grab.Frame);
        }

        if (DragStart != null)
        {
            if (ImGui.IsKeyPressed(ImGuiKey.Escape, false))
            {
                View.Bank.SetEdited(DragSource!, DragStartEdited ? DragStart : null);
                DragStart = null;
                FrozenRange = null;
            }
            else if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                var fine = ImGui.GetIO().KeyShift ? 0.1f : 1f;
                var start = Channels(DragStart, DragTrack, offsetMode)[DragChannel][DragFrame];
                var value = start - (mouse.Y - DragMouseY) * DragUnitsPerPixel * fine;
                if (MathF.Abs(value - DragValue) > 1e-7f)
                {
                    DragValue = value;
                    if (Edited(DragStart, DragTrack, DragFrame, DragChannel, value, offsetMode) is { } preview)
                        View.Bank.SetEdited(DragSource!, preview);
                }
            }
            else
            {
                var start = DragStart;
                View.Bank.SetEdited(DragSource!, DragStartEdited ? start : null);
                DragStart = null;
                FrozenRange = null;

                var was = Channels(start, DragTrack, offsetMode)[DragChannel][DragFrame];
                if (MathF.Abs(DragValue - was) > 1e-6f && Edited(start, DragTrack, DragFrame, DragChannel, DragValue, offsetMode) is { } result)
                    View.Execute(new AnimationClipEditAction(View.Bank, DragSource!, result,
                        LOC.Get("ANIM_Action_Curve_Key", trackName, LOC.Get(ChannelKeys[DragChannel]), DragFrame,
                            DragValue.ToString(DragChannel < 3 ? "0.#" : "0.###"))));
            }
        }

        // Right-click a handle: the Timeline's key menu for that key (ease, copy, delete)
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && hoveredHandle is { } menu)
            Timeline.OpenKeyMenu(track, menu.Frame);

        if (hovered && DragStart == null)
        {
            if (hoveredHandle is { } h)
                ImGui.SetTooltip(LOC.Get(weightless ? "ANIM_Curves_Handle_Weightless_TT" : "ANIM_Curves_Handle_TT",
                    LOC.Get(ChannelKeys[h.Channel]), h.Frame, values[h.Channel][h.Frame].ToString(h.Channel < 3 ? "0.##" : "0.####")));
            else
                GUI.Tooltip(LOC.Get("ANIM_Curves_TT"));
        }
    }

    // ---- values

    /// <summary>
    /// Each channel's value at each frame: turn X / Y / Z in degrees (unwrapped across ±180) and move X / Y / Z, of the
    /// track's local transform as it plays (Result) or of the active layer's offset. For the root motion: its position
    /// and its turn about up as turn Y.
    /// </summary>
    public static float[][] Channels(EditableClip clip, int track, bool offset)
    {
        var count = clip.FrameCount;
        var values = Enumerable.Range(0, 6).Select(_ => new float[count]).ToArray();
        var baked = offset ? clip : clip.Baked();
        var root = track == EditableClip.RootTrack;
        var motion = root && !offset ? clip.EffectiveRootMotion : null;

        for (var f = 0; f < count; f++)
        {
            Vector3 turn, move;
            if (root)
            {
                if (offset)
                {
                    var key = clip.OffsetAt(track, f);
                    (turn, move) = (new Vector3(0, EditableClip.TwistAngle(key.Rotation, clip.UpAxis) * 180 / MathF.PI, 0), key.Translation);
                }
                else
                {
                    var s = motion != null && f < motion.Length ? motion[f] : Vector4.Zero;
                    (turn, move) = (new Vector3(0, s.W * 180 / MathF.PI, 0), new Vector3(s.X, s.Y, s.Z));
                }
            }
            else if (offset)
            {
                var key = clip.OffsetAt(track, f);
                (turn, move) = (AnimationBonesView.ToEulerDegrees(key.Rotation), key.Translation);
            }
            else
            {
                var local = baked.Frames[f][track];
                (turn, move) = (AnimationBonesView.ToEulerDegrees(local.Rotation), local.Translation);
            }

            values[0][f] = turn.X;
            values[1][f] = turn.Y;
            values[2][f] = turn.Z;
            values[3][f] = move.X;
            values[4][f] = move.Y;
            values[5][f] = move.Z;
        }

        // Turns unwrapped, so a turn through 180 doesn't jump to -180 (the root's turn adds up already)
        for (var c = 0; c < 3; c++)
        {
            if (root && c == 1 && !offset)
                continue;

            for (var f = 1; f < count; f++)
            {
                var step = values[c][f] - values[c][f - 1];
                values[c][f] -= MathF.Round(step / 360) * 360;
            }
        }

        return values;
    }

    /// <summary>
    /// The clip with the active layer's key at <paramref name="frame" /> changed so one channel has
    /// <paramref name="value" />: the key itself (Offset), or the key that makes the played transform reach it, through
    /// the other layers (Result). Null when there is no key there or the active layer doesn't act on the track.
    /// </summary>
    public static EditableClip? Edited(EditableClip clip, int track, int frame, int channel, float value, bool offset)
    {
        if (!clip.Offsets.TryGetValue(track, out var keys) || keys.FindIndex(k => k.Frame == frame) is not (var index and >= 0))
            return null;

        var key = keys[index];

        if (track == EditableClip.RootTrack)
        {
            if (offset)
            {
                var move = key.Translation;
                var rotation = key.Rotation;
                if (channel == 1)
                    rotation = Quaternion.CreateFromAxisAngle(clip.UpAxis, value * MathF.PI / 180);
                else if (channel >= 3)
                    move = With(move, channel - 3, value);
                else
                    return null;

                return clip.WithKey(track, key with { Rotation = Quaternion.Normalize(rotation), Translation = move });
            }

            var sample = clip.RootSampleAt(frame);
            if (channel == 1)
                return clip.KeyedRoot(frame, null, value * MathF.PI / 180);
            if (channel >= 3)
                return clip.KeyedRoot(frame, With(new Vector3(sample.X, sample.Y, sample.Z), channel - 3, value), null);
            return null;
        }

        if (offset)
        {
            if (channel < 3)
            {
                var euler = AnimationBonesView.ToEulerDegrees(key.Rotation);
                return clip.WithKey(track, key with { Rotation = AnimationBonesView.FromEulerDegrees(With(euler, channel, value)) });
            }

            return clip.WithKey(track, key with { Translation = With(key.Translation, channel - 3, value) });
        }

        var local = clip.Baked().Frames[frame][track];
        BoneOffsetKey? aimed = channel < 3
            ? clip.ActiveKeyFor(track, frame, AnimationBonesView.FromEulerDegrees(With(AnimationBonesView.ToEulerDegrees(local.Rotation), channel, value)), null)
            : clip.ActiveKeyFor(track, frame, null, With(local.Translation, channel - 3, value));

        return aimed is { } k2 ? clip.WithKey(track, k2) : null;
    }

    private static Vector3 With(Vector3 v, int axis, float value) => axis switch
    {
        0 => v with { X = value },
        1 => v with { Y = value },
        _ => v with { Z = value }
    };
}
