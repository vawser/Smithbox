using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Displays the events of an animation on a timeline. Each type of event has a track of
/// its own, which splits into further rows wherever events of that type overlap.
/// </summary>
public class TimeActGraphWindow
{
    private const float MinZoom = 0.1f;
    private const float MaxZoom = 8f;
    private const float ZoomStep = 1.25f;

    private const float GutterWidth = 200f;
    private const float RulerHeight = 24f;
    private const float RowHeight = 22f;
    private const float EdgeGrabWidth = 5f;
    private const float MinEventWidth = 3f;
    private const float MinFrameLineSpacing = 4f;
    private const float MinFrameLabelSpacing = 48f;

    // New events start out this long
    private const int DefaultEventFrames = 10;

    private enum DragMode
    {
        None,
        Playhead,
        Move,
        ResizeStart,
        ResizeEnd
    }

    private readonly record struct Placement(TAE.Event Event, int Row);

    public TimeActEditorView View;
    public ProjectEntry Project;

    private float Zoom = 1;

    private DragMode Drag = DragMode.None;
    private TAE.Event DragEvent;
    private float DragGrabTime;
    private float DragOriginalStart;
    private float DragOriginalEnd;

    private bool OpenAddEventMenu = false;
    private float AddEventTime;
    private string AddEventFilter = "";

    public TimeActGraphWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private bool CanEdit => Project.Handler.TimeActData.CanSaveTimeActs;

    public void Display(float width, float height)
    {
        DisplayToolbar();

        var flags = ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

        ImGui.BeginChild($"GraphSection", new Vector2(0, 0), ImGuiChildFlags.Borders, flags);

        DisplayEventGraph();

        ImGui.EndChild();
    }

    private void DisplayToolbar()
    {
        var playback = View.Playback;

        if (ImGui.Button($"{LOC.Get(playback.Playing ? "TAE_GraphWindow_Pause" : "TAE_GraphWindow_Play")}##graphPlay"))
        {
            playback.TogglePlaying();
        }

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("TAE_GraphWindow_Stop")}##graphStop"))
        {
            playback.Stop();
        }

        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("TAE_GraphWindow_Loop")}##graphLoop", ref playback.Loop);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(120 * DPI.UIScale());
        ImGui.SliderFloat("##graphSpeed", ref playback.Speed, 0.05f, 2f, LOC.Get("TAE_GraphWindow_Speed"));

        ImGui.SameLine();
        if (ImGui.Button("-##graphZoomOut"))
        {
            Zoom = Math.Clamp(Zoom / ZoomStep, MinZoom, MaxZoom);
        }

        ImGui.SameLine();
        if (ImGui.Button("+##graphZoomIn"))
        {
            Zoom = Math.Clamp(Zoom * ZoomStep, MinZoom, MaxZoom);
        }
        GUI.Tooltip(LOC.Get("TAE_GraphWindow_Zoom_TT"));

        ImGui.SameLine();
        ImGui.BeginDisabled(!CanEdit || View.Selection.SelectedAnimation == null);
        if (ImGui.Button($"{LOC.Get("TAE_GraphWindow_Add_Event")}##graphAddEvent"))
        {
            AddEventTime = Snap(playback.Time);
            OpenAddEventMenu = true;
        }
        ImGui.EndDisabled();

        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get("TAE_GraphWindow_Frame", (int)MathF.Round(playback.Time / playback.FrameDuration)));

        if (playback.IsClipMissing && View.Selection.SelectedAnimation != null)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("TAE_GraphWindow_No_Clip"));
        }

        if (!CanEdit)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("TAE_GraphWindow_Read_Only"));
        }
    }

    public void DisplayEventGraph()
    {
        var animation = View.Selection.SelectedAnimation;

        if (animation == null)
            return;

        var template = Project.Handler.TimeActData.GetTemplate(View.Selection.SelectedTimeActContainerWrapper);

        var scale = DPI.UIScale();

        var gutter = GutterWidth * scale;
        var ruler = RulerHeight * scale;
        var rowHeight = RowHeight * scale;

        var frameDuration = View.Playback.FrameDuration;
        var frameWidth = CFG.Current.TimeActEditor_Graph_Frame_Width * Zoom * scale;

        var placements = LayoutEvents(animation, template, out var rowLabels);

        var duration = Math.Max(Math.Max(View.Playback.ClipDuration, View.Playback.GetEventDuration()), View.Playback.Time);
        var timelineEnd = (MathF.Ceiling(duration / frameDuration) + 1) * frameDuration;

        var viewPos = ImGui.GetWindowPos();
        var viewSize = ImGui.GetWindowSize();
        var contentSize = new Vector2(gutter + timelineEnd / frameDuration * frameWidth,
            Math.Max(ruler + rowLabels.Count * rowHeight, viewSize.Y - ImGui.GetStyle().ScrollbarSize));

        var origin = ImGui.GetCursorScreenPos();

        ImGui.InvisibleButton("##graphCanvas", contentSize,
            ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight);

        var hovered = ImGui.IsItemHovered();
        var mouse = ImGui.GetMousePos();

        float TimeToX(float time) => origin.X + gutter + time / frameDuration * frameWidth;
        float XToTime(float x) => (x - origin.X - gutter) / frameWidth * frameDuration;
        float RowToY(int row) => origin.Y + ruler + row * rowHeight;

        // Events that last until the end of the animation are stored with a placeholder time
        float GetBarEnd(TAE.Event ev) => Math.Max(TimeToX(Math.Min(ev.EndTime, timelineEnd)), TimeToX(ev.StartTime) + MinEventWidth);

        HandleZoom(hovered, mouse, origin, gutter, frameWidth);

        var hoveredEvent = default(TAE.Event);

        if (hovered)
        {
            foreach (var placement in placements)
            {
                var y = RowToY(placement.Row);

                if (mouse.Y >= y && mouse.Y < y + rowHeight &&
                    mouse.X >= TimeToX(placement.Event.StartTime) && mouse.X < GetBarEnd(placement.Event))
                {
                    hoveredEvent = placement.Event;
                }
            }
        }

        var overRuler = mouse.Y < viewPos.Y + ruler;
        var overGutter = mouse.X < viewPos.X + gutter;

        if (hovered && !overRuler && !overGutter && hoveredEvent != null && CanEdit &&
            (mouse.X - TimeToX(hoveredEvent.StartTime) < EdgeGrabWidth || GetBarEnd(hoveredEvent) - mouse.X < EdgeGrabWidth))
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);
        }

        // Starting a drag
        if (hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !overGutter)
        {
            if (overRuler)
            {
                Drag = DragMode.Playhead;
                View.Playback.Pause();
            }
            else
            {
                View.Selection.SelectedEvent = hoveredEvent;

                if (hoveredEvent != null && CanEdit)
                {
                    Drag = mouse.X - TimeToX(hoveredEvent.StartTime) < EdgeGrabWidth ? DragMode.ResizeStart
                        : GetBarEnd(hoveredEvent) - mouse.X < EdgeGrabWidth && !IsOpenEnded(hoveredEvent) ? DragMode.ResizeEnd
                        : DragMode.Move;

                    DragEvent = hoveredEvent;
                    DragGrabTime = XToTime(mouse.X);
                    DragOriginalStart = hoveredEvent.StartTime;
                    DragOriginalEnd = hoveredEvent.EndTime;
                }
            }
        }

        if (Drag != DragMode.None)
        {
            UpdateDrag(XToTime(mouse.X), frameDuration);
        }

        if (hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Right) && !overRuler && !overGutter)
        {
            View.Selection.SelectedEvent = hoveredEvent;

            if (hoveredEvent != null)
            {
                ImGui.OpenPopup("##graphEventMenu");
            }
            else if (CanEdit)
            {
                AddEventTime = Math.Max(Snap(XToTime(mouse.X)), 0);
                OpenAddEventMenu = true;
            }
        }

        if (OpenAddEventMenu)
        {
            OpenAddEventMenu = false;
            AddEventFilter = "";

            ImGui.OpenPopup("##graphAddEventMenu");
        }

        if (hoveredEvent != null && Drag == DragMode.None)
        {
            GUI.Tooltip(GetEventName(hoveredEvent, template));
        }

        HandleShortcuts();

        DrawGraph(placements, rowLabels, template, viewPos, viewSize, origin, gutter, ruler, rowHeight,
            frameDuration, frameWidth, timelineEnd);

        KeepPlayheadInView(viewSize, gutter, frameWidth, frameDuration);

        DisplayEventMenu();
        DisplayAddEventMenu(template);
    }

    /// <summary>
    /// Sorts the events into rows. Rows are worked out each frame, as events
    /// can be moved over each other, and an animation only has so many of them.
    /// </summary>
    private List<Placement> LayoutEvents(TAE.Animation animation, TAE.Template.BankTemplate template, out List<string> rowLabels)
    {
        var placements = new List<Placement>(animation.Events.Count);
        var labels = new List<string>();

        foreach (var track in animation.Events.GroupBy(e => e.Type).OrderBy(e => e.Key))
        {
            var rowEnds = new List<float>();
            var firstRow = labels.Count;

            foreach (var ev in track.OrderBy(e => e.StartTime))
            {
                var row = rowEnds.FindIndex(end => end <= ev.StartTime);

                if (row < 0)
                {
                    row = rowEnds.Count;
                    rowEnds.Add(0);
                    labels.Add(row == 0 ? GetEventName(ev, template) : "");
                }

                rowEnds[row] = ev.EndTime;

                placements.Add(new Placement(ev, firstRow + row));
            }
        }

        rowLabels = labels;

        return placements;
    }

    /// <summary>
    /// Events that last until the end of the animation are stored with a placeholder as their end time
    /// </summary>
    private static bool IsOpenEnded(TAE.Event ev)
    {
        return ev.EndTime >= TAE.Event.EldenRingInfiniteLengthEventPlaceholder;
    }

    private string GetEventName(TAE.Event ev, TAE.Template.BankTemplate template)
    {
        var name = ev.TypeName;

        if (name == null && template != null && template.TryGetValue(ev.Type, out var eventTemplate))
        {
            name = eventTemplate.Name;
        }

        return name != null ? $"[{ev.Type}] {name}" : $"[{ev.Type}]";
    }

    private float Snap(float time)
    {
        var frameDuration = View.Playback.FrameDuration;

        return MathF.Round(time / frameDuration) * frameDuration;
    }

    private void HandleZoom(bool hovered, Vector2 mouse, Vector2 origin, float gutter, float frameWidth)
    {
        if (!hovered)
            return;

        var wheel = ImGui.GetIO().MouseWheel;

        if (wheel == 0)
            return;

        if (ImGui.GetIO().KeyCtrl)
        {
            var newZoom = Math.Clamp(wheel > 0 ? Zoom * ZoomStep : Zoom / ZoomStep, MinZoom, MaxZoom);

            // Keeps the frame under the cursor where it is
            var frame = (mouse.X - origin.X - gutter) / frameWidth;
            var frameMove = frame * frameWidth * (newZoom / Zoom - 1);

            Zoom = newZoom;

            ImGui.SetScrollX(ImGui.GetScrollX() + frameMove);
        }
        else
        {
            ImGui.SetScrollY(ImGui.GetScrollY() - wheel * RowHeight * DPI.UIScale() * 3);
        }
    }

    private void UpdateDrag(float time, float frameDuration)
    {
        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            EndDrag();
            return;
        }

        if (Drag == DragMode.Playhead)
        {
            View.Playback.Seek(Math.Max(Snap(time), 0));
            return;
        }

        var delta = time - DragGrabTime;

        var isOpenEnded = IsOpenEnded(DragEvent);

        switch (Drag)
        {
            case DragMode.Move:
                var start = Math.Max(Snap(DragOriginalStart + delta), 0);

                DragEvent.StartTime = start;

                // The placeholder has to stay as it is, or the event will stop short of the end
                if (!isOpenEnded)
                {
                    DragEvent.EndTime = start + DragOriginalEnd - DragOriginalStart;
                }
                break;

            case DragMode.ResizeStart:
                DragEvent.StartTime = Math.Clamp(Snap(DragOriginalStart + delta), 0, Math.Max(DragOriginalEnd - frameDuration, 0));
                break;

            case DragMode.ResizeEnd:
                DragEvent.EndTime = Math.Max(Snap(DragOriginalEnd + delta), DragOriginalStart + frameDuration);
                break;
        }
    }

    private void EndDrag()
    {
        if (DragEvent != null &&
            (DragEvent.StartTime != DragOriginalStart || DragEvent.EndTime != DragOriginalEnd))
        {
            View.ViewportActionManager.ExecuteAction(new TimeActMoveEventAction(View.Selection.SelectedTimeAct, DragEvent,
                DragOriginalStart, DragOriginalEnd, DragEvent.StartTime, DragEvent.EndTime));
        }

        Drag = DragMode.None;
        DragEvent = null;
    }

    private void HandleShortcuts()
    {
        if (!ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows) || ImGui.IsAnyItemActive() && Drag == DragMode.None)
            return;

        if (ImGui.IsKeyPressed(ImGuiKey.Space, false))
        {
            View.Playback.TogglePlaying();
        }

        if (InputManager.IsPressed(KeybindID.Delete))
        {
            DeleteSelectedEvent();
        }
    }

    private void DeleteSelectedEvent()
    {
        var selection = View.Selection;

        if (selection.SelectedEvent == null || !CanEdit)
            return;

        View.ViewportActionManager.ExecuteAction(new TimeActAddEventAction(selection, selection.SelectedTimeAct,
            selection.SelectedAnimation, selection.SelectedEvent, true));
    }

    private void DrawGraph(List<Placement> placements, List<string> rowLabels, TAE.Template.BankTemplate template,
        Vector2 viewPos, Vector2 viewSize, Vector2 origin, float gutter, float ruler, float rowHeight,
        float frameDuration, float frameWidth, float timelineEnd)
    {
        var drawList = ImGui.GetWindowDrawList();

        var textColor = ImGui.GetColorU32(ImGuiCol.Text);
        var lineColor = ImGui.GetColorU32(ImGuiCol.Border);
        var rowColor = ImGui.GetColorU32(ImGuiCol.FrameBg);
        var headerColor = ImGui.GetColorU32(ImGuiCol.TitleBg);
        var selectedColor = ImGui.GetColorU32(ImGuiCol.CheckMark);

        var viewMax = viewPos + viewSize;

        var tracksMin = new Vector2(viewPos.X + gutter, viewPos.Y + ruler);
        var timelineWidth = timelineEnd / frameDuration * frameWidth;

        var framesPerLine = Math.Max((int)MathF.Ceiling(MinFrameLineSpacing / frameWidth), 1);
        var framesPerLabel = GetLabelInterval(frameWidth);
        var frameCount = (int)MathF.Ceiling(timelineEnd / frameDuration);

        float FrameToX(float frame) => origin.X + gutter + frame * frameWidth;

        // Tracks
        drawList.PushClipRect(tracksMin, viewMax, true);

        for (int i = 0; i < rowLabels.Count; i += 2)
        {
            var y = origin.Y + ruler + i * rowHeight;

            drawList.AddRectFilled(new Vector2(tracksMin.X, y), new Vector2(FrameToX(0) + timelineWidth, y + rowHeight), rowColor);
        }

        for (int frame = 0; frame <= frameCount; frame += framesPerLine)
        {
            var x = FrameToX(frame);

            if (x >= tracksMin.X && x <= viewMax.X)
            {
                drawList.AddLine(new Vector2(x, tracksMin.Y), new Vector2(x, viewMax.Y), lineColor);
            }
        }

        // Events
        foreach (var placement in placements)
        {
            var ev = placement.Event;

            var min = new Vector2(FrameToX(ev.StartTime / frameDuration), origin.Y + ruler + placement.Row * rowHeight + 1);
            var max = new Vector2(
                Math.Max(FrameToX(Math.Min(ev.EndTime, timelineEnd) / frameDuration), min.X + MinEventWidth),
                min.Y + rowHeight - 2);

            if (max.X < tracksMin.X || min.X > viewMax.X || max.Y < tracksMin.Y || min.Y > viewMax.Y)
                continue;

            drawList.AddRectFilled(min, max, GetEventColor(ev.Type));

            if (ev == View.Selection.SelectedEvent)
            {
                drawList.AddRect(min, max, selectedColor, 0, ImDrawFlags.None, 2);
            }

            drawList.PushClipRect(min, Vector2.Min(max, viewMax), true);
            drawList.AddText(min + new Vector2(3, 2), textColor, GetEventName(ev, template));
            drawList.PopClipRect();
        }

        drawList.PopClipRect();

        // Track names stay in place as the timeline scrolls past them
        drawList.PushClipRect(new Vector2(viewPos.X, tracksMin.Y), new Vector2(tracksMin.X, viewMax.Y), true);

        drawList.AddRectFilled(new Vector2(viewPos.X, tracksMin.Y), new Vector2(tracksMin.X, viewMax.Y), headerColor);

        for (int i = 0; i < rowLabels.Count; i++)
        {
            drawList.AddText(new Vector2(viewPos.X + 4, origin.Y + ruler + i * rowHeight + 2), textColor, rowLabels[i]);
        }

        drawList.PopClipRect();

        // Ruler
        drawList.PushClipRect(viewPos, new Vector2(viewMax.X, tracksMin.Y), true);

        drawList.AddRectFilled(viewPos, new Vector2(viewMax.X, tracksMin.Y), headerColor);

        for (int frame = 0; frame <= frameCount; frame += framesPerLabel)
        {
            var x = FrameToX(frame);

            if (x >= tracksMin.X && x <= viewMax.X)
            {
                drawList.AddLine(new Vector2(x, tracksMin.Y - 6), new Vector2(x, tracksMin.Y), textColor);
                drawList.AddText(new Vector2(x + 3, viewPos.Y + 3), textColor, frame.ToString());
            }
        }

        drawList.PopClipRect();

        // Playhead
        var playheadX = FrameToX(View.Playback.Time / frameDuration);

        if (playheadX >= tracksMin.X && playheadX <= viewMax.X)
        {
            drawList.AddLine(new Vector2(playheadX, viewPos.Y), new Vector2(playheadX, viewMax.Y),
                ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0.25f, 0.25f, 1)), 2);
        }
    }

    private static int GetLabelInterval(float frameWidth)
    {
        foreach (var interval in new[] { 1, 5, 10, 30, 60, 150, 300 })
        {
            if (interval * frameWidth >= MinFrameLabelSpacing)
                return interval;
        }

        return 600;
    }

    /// <summary>
    /// Gives each type of event a colour of its own, spread out so that nearby IDs don't look alike
    /// </summary>
    private static uint GetEventColor(int type)
    {
        const float GoldenRatio = 0.618034f;
        const float Saturation = 0.45f;
        const float Value = 0.55f;

        var hue = (type * GoldenRatio % 1) * 6;

        var x = 1 - Math.Abs(hue % 2 - 1);

        var rgb = (int)hue switch
        {
            0 => new Vector3(1, x, 0),
            1 => new Vector3(x, 1, 0),
            2 => new Vector3(0, 1, x),
            3 => new Vector3(0, x, 1),
            4 => new Vector3(x, 0, 1),
            _ => new Vector3(1, 0, x)
        };

        var color = (new Vector3(1 - Saturation) + rgb * Saturation) * Value;

        return ImGui.ColorConvertFloat4ToU32(new Vector4(color, 1));
    }

    private void KeepPlayheadInView(Vector2 viewSize, float gutter, float frameWidth, float frameDuration)
    {
        if (!View.Playback.Playing)
            return;

        var x = View.Playback.Time / frameDuration * frameWidth;
        var scroll = ImGui.GetScrollX();
        var visibleWidth = viewSize.X - gutter;

        if (x < scroll || x > scroll + visibleWidth)
        {
            ImGui.SetScrollX(x);
        }
    }

    private void DisplayEventMenu()
    {
        if (ImGui.BeginPopup("##graphEventMenu"))
        {
            ImGui.BeginDisabled(!CanEdit);

            if (ImGui.Selectable($"{LOC.Get("TAE_GraphWindow_Delete_Event")}##deleteEvent"))
            {
                DeleteSelectedEvent();
            }

            ImGui.EndDisabled();

            ImGui.EndPopup();
        }
    }

    private void DisplayAddEventMenu(TAE.Template.BankTemplate template)
    {
        var popupSize = new Vector2(360, 400) * DPI.UIScale();

        ImGui.SetNextWindowSize(popupSize, ImGuiCond.Always);

        if (ImGui.BeginPopup("##graphAddEventMenu"))
        {
            if (template == null)
            {
                ImGui.TextDisabled(LOC.Get("TAE_GraphWindow_No_Templates"));
            }
            else
            {
                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint("##addEventFilter", LOC.Get("TAE_GraphWindow_Add_Event_Filter"), ref AddEventFilter, 255);

                ImGui.BeginChild("addEventList", new Vector2(0, 0), ImGuiChildFlags.Borders);

                foreach (var eventTemplate in template.Values.OrderBy(e => e.ID))
                {
                    var name = $"[{eventTemplate.ID}] {eventTemplate.Name}";

                    if (!name.Contains(AddEventFilter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (ImGui.Selectable($"{name}##addEvent{eventTemplate.ID}"))
                    {
                        AddEvent(eventTemplate);

                        ImGui.CloseCurrentPopup();
                    }
                }

                ImGui.EndChild();
            }

            ImGui.EndPopup();
        }
    }

    private void AddEvent(TAE.Template.EventTemplate eventTemplate)
    {
        var selection = View.Selection;
        var animation = selection.SelectedAnimation;
        var timeAct = selection.SelectedTimeAct.TimeAct;

        var endTime = AddEventTime + DefaultEventFrames * View.Playback.FrameDuration;

        // The meaning of this field is unknown, so go by how the type is used elsewhere in the animation
        var unk04 = animation.Events.FirstOrDefault(e => e.Type == eventTemplate.ID)?.Unk04 ?? 0;

        var ev = new TAE.Event(AddEventTime, endTime, eventTemplate.ID, unk04, timeAct.BigEndian, eventTemplate);

        View.ViewportActionManager.ExecuteAction(new TimeActAddEventAction(selection, selection.SelectedTimeAct, animation, ev));
    }
}
