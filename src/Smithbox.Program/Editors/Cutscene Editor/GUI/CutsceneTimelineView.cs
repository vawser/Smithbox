#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The selected cut's timelines, as the MQB nests them (each under its ParentIndex), with a bar for each event and the
/// playhead, under the playback controls. Read-only: clicking a bar selects its event, dragging elsewhere scrubs.
/// </summary>
public class CutsceneTimelineView
{
    private readonly CutsceneEditorScreen Editor;

    // The rows of the cut last drawn: each timeline and how deep it is in the tree
    private MQB.Cut? RowsFor;
    private readonly List<(int Timeline, int Depth)> Rows = new();

    private bool Scrubbing;

    public CutsceneTimelineView(CutsceneEditorScreen editor)
    {
        Editor = editor;
    }

    public void Display()
    {
        var file = Editor.SelectedFile;
        if (file == null)
        {
            ImGui.TextDisabled(LOC.Get("CUTSCENE_Timeline_Nothing_Selected"));
            return;
        }

        if (Editor.IsBuilding)
        {
            ImGui.Text(LOC.Get("CUTSCENE_Timeline_Building", file.Name));
            return;
        }

        if (Editor.BuildError is { } error)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, error);
            return;
        }

        var scene = Editor.Scene;
        if (scene == null || file.Mqb == null)
            return;

        DisplayTransport(scene);
        ImGui.Separator();
        DisplayTracks(file.Mqb, scene);
    }

    private void DisplayTransport(CutsceneScene scene)
    {
        var playback = Editor.Playback;
        var last = Math.Max(0, scene.FrameCount - 1);

        if (ImGui.Button(playback.Playing ? $"{LOC.Get("CUTSCENE_Timeline_Pause")}##cutscenePause" : $"{LOC.Get("CUTSCENE_Timeline_Play")}##cutscenePlay"))
            playback.Playing = !playback.Playing;

        ImGui.SameLine();
        if (ImGui.Button("|<##cutsceneRestart"))
            playback.Restart();

        ImGui.SameLine();
        if (ImGui.Button("<##cutsceneStepBack"))
        {
            playback.Playing = false;
            playback.Frame = MathF.Max(0, MathF.Floor(playback.Frame) - 1);
        }

        ImGui.SameLine();
        if (ImGui.Button(">##cutsceneStepForward"))
        {
            playback.Playing = false;
            playback.Frame = MathF.Min(last, MathF.Floor(playback.Frame) + 1);
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(Math.Max(120, ImGui.GetContentRegionAvail().X * 0.35f));
        var frame = playback.Frame;
        if (ImGui.SliderFloat("##cutsceneFrame", ref frame, 0, last, "%.0f"))
            playback.Frame = frame;

        ImGui.SameLine();
        ImGui.Text(LOC.Get("CUTSCENE_Timeline_Frame", (int)playback.Frame, last, playback.Frame / scene.Framerate, last / scene.Framerate));

        ImGui.SameLine();
        ImGui.SetNextItemWidth(70 * DPI.UIScale());
        ImGui.DragFloat("##cutsceneSpeed", ref CFG.Current.CutsceneEditor_Playback_Speed, 0.01f, 0.05f, 4f, "%.2fx");
        GUI.Tooltip(LOC.Get("CUTSCENE_Timeline_Speed_TT"));

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("CUTSCENE_Timeline_Loop")}##cutsceneLoop", ref CFG.Current.CutsceneEditor_Loop);

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("CUTSCENE_Timeline_Next_Cut")}##cutsceneNextCut", ref CFG.Current.CutsceneEditor_Continue_To_Next_Cut);
        GUI.Tooltip(LOC.Get("CUTSCENE_Timeline_Next_Cut_TT"));

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("CUTSCENE_Timeline_Look_Through")}##cutsceneLookThrough", ref CFG.Current.CutsceneEditor_Look_Through_Camera);
        GUI.Tooltip(LOC.Get("CUTSCENE_Menu_Look_Through_TT"));

        var camera = scene.ActiveCamera(playback.Frame);
        if (camera != null)
        {
            ImGui.SameLine();
            var fov = camera.FovRadians(playback.Frame);
            ImGui.TextDisabled(fov is { } f
                ? LOC.Get("CUTSCENE_Timeline_Camera_Fov", camera.Name, f * 180 / MathF.PI)
                : camera.Name);
        }
    }

    private void BuildRows(MQB.Cut cut)
    {
        if (RowsFor == cut)
            return;

        RowsFor = cut;
        Rows.Clear();

        // Children under their parent, in file order; timelines whose parent is missing are roots
        var children = new Dictionary<int, List<int>>();
        var roots = new List<int>();
        for (var t = 0; t < cut.Timelines.Count; t++)
        {
            var parent = cut.Timelines[t].ParentIndex;
            if (parent >= 0 && parent < cut.Timelines.Count && parent != t)
            {
                if (!children.TryGetValue(parent, out var list))
                    children[parent] = list = new();
                list.Add(t);
            }
            else
            {
                roots.Add(t);
            }
        }

        var visited = new HashSet<int>();
        void Add(int t, int depth)
        {
            if (!visited.Add(t))
                return;

            Rows.Add((t, depth));
            if (children.TryGetValue(t, out var list))
                foreach (var c in list)
                    Add(c, depth + 1);
        }

        foreach (var r in roots)
            Add(r, 0);

        // Anything left in a cycle
        for (var t = 0; t < cut.Timelines.Count; t++)
            Add(t, 0);
    }

    private void DisplayTracks(MQB mqb, CutsceneScene scene)
    {
        var cut = scene.Cut;
        BuildRows(cut);

        var scale = DPI.UIScale();
        var rowHeight = ImGui.GetTextLineHeight() + 4 * scale;
        var ruler = rowHeight;
        var gutter = 240 * scale;

        if (!ImGui.BeginChild("##cutsceneTracks", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar))
        {
            ImGui.EndChild();
            return;
        }

        var avail = ImGui.GetContentRegionAvail();
        var width = Math.Max(avail.X, gutter + 200 * scale);
        var size = new Vector2(width, ruler + Rows.Count * rowHeight);
        var origin = ImGui.GetCursorScreenPos();

        ImGui.InvisibleButton("##cutsceneTrackCanvas", size, ImGuiButtonFlags.MouseButtonLeft);
        var hovered = ImGui.IsItemHovered();
        var active = ImGui.IsItemActive();
        var mouse = ImGui.GetMousePos();

        var frames = Math.Max(1, scene.FrameCount);
        var trackWidth = width - gutter;
        float FrameToX(float f) => origin.X + gutter + f / frames * trackWidth;
        float XToFrame(float x) => (x - origin.X - gutter) / trackWidth * frames;

        var drawList = ImGui.GetWindowDrawList();
        var textColor = ImGui.GetColorU32(ImGuiCol.Text);
        var dullColor = ImGui.GetColorU32(ImGuiCol.TextDisabled);
        var lineColor = ImGui.GetColorU32(ImGuiCol.Border);
        var rowColor = ImGui.GetColorU32(ImGuiCol.FrameBg);
        var selectedColor = ImGui.GetColorU32(ImGuiCol.CheckMark);

        // Ruler: a tick a second
        drawList.AddLine(new Vector2(origin.X + gutter, origin.Y + ruler), new Vector2(origin.X + width, origin.Y + ruler), lineColor);
        var step = Math.Max(1, (int)scene.Framerate);
        for (var f = 0; f <= frames; f += step)
        {
            var x = FrameToX(f);
            drawList.AddLine(new Vector2(x, origin.Y + ruler * 0.5f), new Vector2(x, origin.Y + ruler), lineColor);
            drawList.AddText(new Vector2(x + 2, origin.Y), dullColor, $"{f / scene.Framerate:0}s");
        }

        MQB.Event? hoveredEvent = null;
        var hoveredTimeline = -1;

        for (var r = 0; r < Rows.Count; r++)
        {
            var (t, depth) = Rows[r];
            var timeline = cut.Timelines[t];
            var y = origin.Y + ruler + r * rowHeight;

            if (r % 2 == 0)
                drawList.AddRectFilled(new Vector2(origin.X + gutter, y), new Vector2(origin.X + width, y + rowHeight), rowColor);

            // The row's name: the resource of its first event, and its type
            var first = timeline.Events.Count > 0 ? timeline.Events[0] : null;
            var resource = first != null && first.ResourceIndex >= 0 && first.ResourceIndex < mqb.Resources.Count
                ? mqb.Resources[first.ResourceIndex]
                : null;
            var label = resource != null ? $"{resource.Name}  [{CutsceneResourceTypes.Name(resource.Type)}]" : LOC.Get("CUTSCENE_Timeline_Empty_Row", t);

            drawList.PushClipRect(new Vector2(origin.X, y), new Vector2(origin.X + gutter - 4, y + rowHeight), true);
            drawList.AddText(new Vector2(origin.X + 4 + depth * 12 * scale, y + 2 * scale), t == Editor.SelectedTimeline ? selectedColor : textColor, label);
            drawList.PopClipRect();

            foreach (var e in timeline.Events)
            {
                var res = e.ResourceIndex >= 0 && e.ResourceIndex < mqb.Resources.Count ? mqb.Resources[e.ResourceIndex] : null;
                var x0 = FrameToX(Math.Clamp(e.StartFrame, 0, frames));
                var x1 = Math.Max(x0 + 3, FrameToX(Math.Clamp(e.StartFrame + e.Duration, 0, frames)));
                var min = new Vector2(x0, y + 2 * scale);
                var max = new Vector2(x1, y + rowHeight - 2 * scale);

                var color = CutsceneResourceTypes.Color(res?.Type ?? 0);
                drawList.AddRectFilled(min, max, ImGui.GetColorU32(color with { W = 0.75f }), 2 * scale);
                if (e == Editor.SelectedEvent)
                    drawList.AddRect(min, max, selectedColor, 2 * scale, ImDrawFlags.None, 2 * scale);

                if (hovered && mouse.X >= min.X && mouse.X <= max.X && mouse.Y >= min.Y && mouse.Y <= max.Y)
                {
                    hoveredEvent = e;
                    hoveredTimeline = t;
                }
            }
        }

        // Playhead
        var playX = FrameToX(Editor.Playback.Frame);
        drawList.AddLine(new Vector2(playX, origin.Y), new Vector2(playX, origin.Y + size.Y), selectedColor, 2 * scale);

        // Input: a click on a bar selects its event; anywhere else in the tracks scrubs
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            if (hoveredEvent != null)
            {
                Editor.SelectedEvent = hoveredEvent;
                Editor.SelectedTimeline = hoveredTimeline;
            }
            else if (mouse.X > origin.X + gutter)
            {
                Scrubbing = true;
            }
        }

        if (Scrubbing && active)
        {
            Editor.Playback.Playing = false;
            Editor.Playback.Frame = Math.Clamp(XToFrame(mouse.X), 0, frames - 1);
        }
        else
        {
            Scrubbing = false;
        }

        if (hoveredEvent != null)
        {
            var res = hoveredEvent.ResourceIndex >= 0 && hoveredEvent.ResourceIndex < mqb.Resources.Count ? mqb.Resources[hoveredEvent.ResourceIndex] : null;
            ImGui.SetTooltip(LOC.Get("CUTSCENE_Timeline_Event_TT", res?.Name ?? "?", hoveredEvent.ID, hoveredEvent.StartFrame,
                hoveredEvent.StartFrame + hoveredEvent.Duration));
        }

        ImGui.EndChild();
    }
}
