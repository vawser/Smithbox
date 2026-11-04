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

    // Times are stored as floats, so events that only touch can differ by a little either way
    private const float OverlapTolerance = 0.001f;

    // How many frames are always on show either side of everything in the animation
    private const int TimelinePaddingFrames = 10;

    private static readonly Vector4 OutsidePlaybackTint = new(0.35f, 0.35f, 0.35f, 0.55f);

    // New events start out this long
    private const int DefaultEventFrames = 10;

    private enum DragMode
    {
        None,
        Playhead,
        Move,
        ResizeStart,
        ResizeEnd,
        Box
    }

    private readonly record struct Placement(TAE.Event Event, int Row);

    private readonly record struct DragOriginal(TAE.Event Event, float Start, float End);

    public TimeActEditorView View;
    public ProjectEntry Project;

    private float Zoom = 1;

    private DragMode Drag = DragMode.None;

    // The event under the cursor when the drag began. Every selected event moves along with it.
    private TAE.Event DragEvent;
    private float DragGrabTime;
    private List<DragOriginal> DragOriginals = new();

    // A plain click on an event that is part of a larger selection keeps the rest selected, so they can
    // be dragged together. If the click doesn't turn into a drag, only that event stays selected.
    private TAE.Event ClickCollapseEvent;

    // Box selection. The start is kept in timeline space, so it stays put if the graph scrolls.
    private float BoxStartTime;
    private float BoxStartY;
    private List<TAE.Event> BoxBaseSelection = new();
    private Vector2 BoxMin;
    private Vector2 BoxMax;

    private List<Placement> LayoutPlacements;
    private List<string> LayoutRowLabels;
    private int LayoutFirstFrame;
    private int LayoutLastFrame;

    private bool OpenAddEventMenu = false;
    private float AddEventTime;
    private string AddEventFilter = "";

    // Events that don't pass it are drawn duller, rather than hidden, so the rest of the timeline stays in view
    private readonly TimeActEventFilter EventFilter;
    private string EventFilterText = "";

    public TimeActGraphWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        EventFilter = new TimeActEventFilter(project);
    }

    private bool CanEdit => Project.Handler.TimeActData.CanSaveTimeActs;

    public void Display(float width, float height)
    {
        DisplayToolbar();
        DisplayEventFilter();

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

    private void DisplayEventFilter()
    {
        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputTextWithHint("##graphEventFilter", LOC.Get("TAE_GraphWindow_Event_Filter"), ref EventFilterText, 255))
        {
            EventFilter.SetText(EventFilterText);
        }

        GUI.Tooltip(LOC.Get("TAE_GraphWindow_Event_Filter_TT"));
    }

    public void DisplayEventGraph()
    {
        var animation = View.Selection.SelectedAnimation;

        if (animation == null)
            return;

        var template = Project.Handler.TimeActData.GetTemplate(View.Selection.SelectedTimeActContainerWrapper);

        var scale = DPI.UIScale();

        var gutter = CFG.Current.TimeActEditor_Graph_Display_Event_Names ? GutterWidth * scale : 0;
        var ruler = RulerHeight * scale;
        var rowHeight = RowHeight * scale;

        var frameDuration = View.Playback.FrameDuration;
        var frameWidth = CFG.Current.TimeActEditor_Graph_Frame_Width * Zoom * scale;

        // The rows hold still during a drag, so that the event stays under the cursor
        if (DragEvent == null)
        {
            LayoutPlacements = LayoutEvents(animation, template, frameDuration, out LayoutRowLabels);

            LayoutTimeline(animation, frameDuration);
        }

        var placements = LayoutPlacements;
        var rowLabels = LayoutRowLabels;

        var firstFrame = LayoutFirstFrame;
        var lastFrame = LayoutLastFrame;
        var timelineEnd = lastFrame * frameDuration;

        var viewPos = ImGui.GetWindowPos();
        var viewSize = ImGui.GetWindowSize();
        var contentSize = new Vector2(gutter + (lastFrame - firstFrame) * frameWidth,
            Math.Max(ruler + rowLabels.Count * rowHeight, viewSize.Y - ImGui.GetStyle().ScrollbarSize));

        var origin = ImGui.GetCursorScreenPos();

        ImGui.InvisibleButton("##graphCanvas", contentSize,
            ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight);

        var hovered = ImGui.IsItemHovered();
        var mouse = ImGui.GetMousePos();

        float TimeToX(float time) => origin.X + gutter + (time / frameDuration - firstFrame) * frameWidth;
        float XToTime(float x) => ((x - origin.X - gutter) / frameWidth + firstFrame) * frameDuration;
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
                    mouse.X >= TimeToX(placement.Event.StartTime) && mouse.X < GetBarEnd(placement.Event) &&
                    mouse.X < TimeToX(timelineEnd))
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
                var io = ImGui.GetIO();
                var selection = View.Selection;

                if (hoveredEvent == null)
                {
                    // Ctrl or Shift adds the boxed events to what is already selected
                    if (!io.KeyCtrl && !io.KeyShift)
                    {
                        selection.SelectedEvents.Clear();
                    }

                    Drag = DragMode.Box;
                    BoxStartTime = XToTime(mouse.X);
                    BoxStartY = mouse.Y - origin.Y;
                    BoxBaseSelection = new List<TAE.Event>(selection.SelectedEvents);
                    BoxMin = BoxMax = mouse;
                }
                else if (io.KeyCtrl)
                {
                    // Ctrl toggles the event in or out of the selection
                    if (!selection.SelectedEvents.Remove(hoveredEvent))
                    {
                        selection.SelectedEvents.Add(hoveredEvent);
                    }
                }
                else
                {
                    if (!selection.IsEventSelected(hoveredEvent))
                    {
                        if (io.KeyShift)
                        {
                            selection.SelectedEvents.Add(hoveredEvent);
                        }
                        else
                        {
                            selection.SelectedEvent = hoveredEvent;
                        }
                    }
                    else if (!io.KeyShift && selection.SelectedEvents.Count > 1)
                    {
                        if (CanEdit)
                        {
                            ClickCollapseEvent = hoveredEvent;
                        }
                        else
                        {
                            selection.SelectedEvent = hoveredEvent;
                        }
                    }

                    if (CanEdit)
                    {
                        Drag = mouse.X - TimeToX(hoveredEvent.StartTime) < EdgeGrabWidth ? DragMode.ResizeStart
                            : GetBarEnd(hoveredEvent) - mouse.X < EdgeGrabWidth && !IsOpenEnded(hoveredEvent) ? DragMode.ResizeEnd
                            : DragMode.Move;

                        DragEvent = hoveredEvent;
                        DragGrabTime = XToTime(mouse.X);
                        DragOriginals = selection.SelectedEvents
                            .Select(e => new DragOriginal(e, e.StartTime, e.EndTime))
                            .ToList();
                    }
                }
            }
        }

        if (Drag == DragMode.Box)
        {
            UpdateBoxSelection(placements, mouse, origin, rowHeight, TimeToX, RowToY, GetBarEnd, TimeToX(timelineEnd));
        }
        else if (Drag != DragMode.None)
        {
            UpdateDrag(XToTime(mouse.X), frameDuration);
        }

        if (hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Right) && !overRuler && !overGutter)
        {
            // Right-clicking one of the selected events keeps the selection, so the menu acts on all of them
            if (hoveredEvent == null || !View.Selection.IsEventSelected(hoveredEvent))
            {
                View.Selection.SelectedEvent = hoveredEvent;
            }

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
            frameDuration, frameWidth, firstFrame, lastFrame);

        if (Drag == DragMode.Box)
        {
            var drawList = ImGui.GetWindowDrawList();

            drawList.AddRectFilled(BoxMin, BoxMax, ImGui.GetColorU32(ImGuiCol.CheckMark, 0.15f));
            drawList.AddRect(BoxMin, BoxMax, ImGui.GetColorU32(ImGuiCol.CheckMark));
        }

        KeepPlayheadInView(viewSize, gutter, frameWidth, frameDuration, firstFrame);

        DisplayEventMenu();
        DisplayAddEventMenu(template);
    }

    /// <summary>
    /// Events can start before the animation and run past its end, so the
    /// timeline reaches far enough either side of playback to show all of them.
    /// </summary>
    private void LayoutTimeline(TAE.Animation animation, float frameDuration)
    {
        if (!CFG.Current.TimeActEditor_Graph_Display_Outside_Playback)
        {
            LayoutFirstFrame = 0;
            LayoutLastFrame = (int)MathF.Ceiling(View.Playback.PlaybackDuration / frameDuration);
            return;
        }

        var earliest = animation.Events.Count > 0 ? Math.Min(animation.Events.Min(e => e.StartTime), 0) : 0;

        var latest = Math.Max(Math.Max(View.Playback.PlaybackDuration, View.Playback.GetEventDuration()), View.Playback.Time);

        LayoutFirstFrame = (int)MathF.Floor(earliest / frameDuration) - TimelinePaddingFrames;
        LayoutLastFrame = (int)MathF.Ceiling(latest / frameDuration) + TimelinePaddingFrames;
    }

    /// <summary>
    /// Gets the first multiple of the interval on the timeline, which keeps the frame lines on round numbers
    /// </summary>
    private static int AlignFrame(int firstFrame, int interval)
    {
        return (int)MathF.Ceiling(firstFrame / (float)interval) * interval;
    }

    /// <summary>
    /// Sorts the events into rows. Rows are worked out each frame, as events
    /// can be moved over each other, and an animation only has so many of them.
    /// </summary>
    private List<Placement> LayoutEvents(TAE.Animation animation, TAE.Template.BankTemplate template, float frameDuration,
        out List<string> rowLabels)
    {
        var placements = new List<Placement>(animation.Events.Count);
        var labels = new List<string>();

        foreach (var track in animation.Events.GroupBy(e => e.Type).OrderBy(e => e.Key))
        {
            var rowEnds = new List<float>();
            var firstRow = labels.Count;

            foreach (var ev in track.OrderBy(e => e.StartTime))
            {
                var row = rowEnds.FindIndex(end => end <= ev.StartTime + OverlapTolerance);

                if (row < 0)
                {
                    row = rowEnds.Count;
                    rowEnds.Add(0);
                    labels.Add(row == 0 ? GetEventName(ev, template) : "");
                }

                // An event takes up at least a frame on screen, however short it is
                rowEnds[row] = Math.Max(ev.EndTime, ev.StartTime + frameDuration);

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
            View.Playback.Seek(Math.Clamp(Snap(time), 0, View.Playback.PlaybackDuration));
            return;
        }

        // Events move by whole frames, so a click that doesn't drag leaves them exactly where they were
        var delta = Snap(time - DragGrabTime);

        var timelineStart = LayoutFirstFrame * frameDuration;

        // An event can already sit before the timeline, if the frames there are hidden
        float FirstAllowed(DragOriginal original) => Math.Min(timelineStart, original.Start);

        if (delta == 0)
        {
            foreach (var original in DragOriginals)
            {
                original.Event.StartTime = original.Start;
                original.Event.EndTime = original.End;
            }
            return;
        }

        switch (Drag)
        {
            case DragMode.Move:
                // The selection stops as one at the start of the timeline, so the events keep their spacing
                foreach (var original in DragOriginals)
                {
                    delta = Math.Max(delta, FirstAllowed(original) - original.Start);
                }

                foreach (var original in DragOriginals)
                {
                    var start = original.Start + delta;

                    original.Event.StartTime = start;

                    // The placeholder has to stay as it is, or the event will stop short of the end
                    if (!IsOpenEnded(original.Event))
                    {
                        original.Event.EndTime = start + original.End - original.Start;
                    }
                }
                break;

            case DragMode.ResizeStart:
                foreach (var original in DragOriginals)
                {
                    var firstAllowed = FirstAllowed(original);

                    original.Event.StartTime = Math.Clamp(original.Start + delta, firstAllowed,
                        Math.Max(original.End - frameDuration, firstAllowed));
                }
                break;

            case DragMode.ResizeEnd:
                foreach (var original in DragOriginals)
                {
                    // Events that last until the end of the animation have no end to move
                    if (IsOpenEnded(original.Event))
                        continue;

                    original.Event.EndTime = Math.Max(original.End + delta, original.Start + frameDuration);
                }
                break;
        }
    }

    private void EndDrag()
    {
        var actions = DragOriginals
            .Where(e => e.Event.StartTime != e.Start || e.Event.EndTime != e.End)
            .Select(e => (ViewportAction)new TimeActMoveEventAction(View.Selection.SelectedTimeAct, e.Event,
                e.Start, e.End, e.Event.StartTime, e.Event.EndTime))
            .ToList();

        if (actions.Count == 1)
        {
            View.ViewportActionManager.ExecuteAction(actions[0]);
        }
        else if (actions.Count > 1)
        {
            View.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));
        }
        else if (ClickCollapseEvent != null)
        {
            View.Selection.SelectedEvent = ClickCollapseEvent;
        }

        Drag = DragMode.None;
        DragEvent = null;
        DragOriginals.Clear();
        ClickCollapseEvent = null;
    }

    /// <summary>
    /// Selects every event the box touches, on top of whatever was selected when the box was started
    /// </summary>
    private void UpdateBoxSelection(List<Placement> placements, Vector2 mouse, Vector2 origin, float rowHeight,
        Func<float, float> timeToX, Func<int, float> rowToY, Func<TAE.Event, float> getBarEnd, float timelineEndX)
    {
        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            Drag = DragMode.None;
            BoxBaseSelection.Clear();
            return;
        }

        var start = new Vector2(timeToX(BoxStartTime), origin.Y + BoxStartY);

        BoxMin = Vector2.Min(start, mouse);
        BoxMax = Vector2.Max(start, mouse);

        var selected = new List<TAE.Event>(BoxBaseSelection);

        foreach (var placement in placements)
        {
            var ev = placement.Event;

            var minX = timeToX(ev.StartTime);
            var maxX = Math.Min(getBarEnd(ev), timelineEndX);
            var minY = rowToY(placement.Row);
            var maxY = minY + rowHeight;

            if (minX < BoxMax.X && maxX > BoxMin.X && minY < BoxMax.Y && maxY > BoxMin.Y && !selected.Contains(ev))
            {
                selected.Add(ev);
            }
        }

        View.Selection.SelectedEvents.Clear();
        View.Selection.SelectedEvents.AddRange(selected);
    }

    private void HandleShortcuts()
    {
        if (!ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows) || ImGui.IsAnyItemActive() && Drag == DragMode.None)
            return;

        if (ImGui.IsKeyPressed(ImGuiKey.Space, false))
        {
            View.Playback.TogglePlaying();
        }

        if (InputManager.IsPressed(KeybindID.SelectAll) && Drag == DragMode.None)
        {
            SelectAllEvents();
        }

        if (InputManager.IsPressed(KeybindID.Delete))
        {
            DeleteSelectedEvents();
        }

        if (InputManager.IsPressed(KeybindID.Duplicate))
        {
            DuplicateSelectedEvent();
        }

        if (InputManager.IsPressed(KeybindID.Copy))
        {
            CopySelectedEvent();
        }

        if (InputManager.IsPressed(KeybindID.Paste))
        {
            PasteEvent(Snap(View.Playback.Time));
        }
    }

    /// <summary>
    /// Selects every event, in the order they appear on the graph
    /// </summary>
    private void SelectAllEvents()
    {
        if (LayoutPlacements == null)
            return;

        View.Selection.SelectedEvents.Clear();
        View.Selection.SelectedEvents.AddRange(LayoutPlacements.Select(e => e.Event));
    }

    private void DeleteSelectedEvents()
    {
        var selection = View.Selection;

        if (selection.SelectedEvents.Count == 0 || !CanEdit || Drag != DragMode.None)
            return;

        View.ViewportActionManager.ExecuteAction(new TimeActDeleteEventsAction(selection, selection.SelectedTimeAct,
            selection.SelectedAnimation, selection.SelectedEvents));
    }

    private void DrawGraph(List<Placement> placements, List<string> rowLabels, TAE.Template.BankTemplate template,
        Vector2 viewPos, Vector2 viewSize, Vector2 origin, float gutter, float ruler, float rowHeight,
        float frameDuration, float frameWidth, int firstFrame, int lastFrame)
    {
        var drawList = ImGui.GetWindowDrawList();

        var textColor = ImGui.GetColorU32(ImGuiCol.Text);
        var dullTextColor = ImGui.GetColorU32(ImGuiCol.TextDisabled);
        var lineColor = ImGui.GetColorU32(ImGuiCol.Border);
        var rowColor = ImGui.GetColorU32(ImGuiCol.FrameBg);
        var headerColor = ImGui.GetColorU32(ImGuiCol.TitleBg);
        var selectedColor = ImGui.GetColorU32(ImGuiCol.CheckMark);

        var viewMax = viewPos + viewSize;

        var tracksMin = new Vector2(viewPos.X + gutter, viewPos.Y + ruler);

        var framesPerLine = Math.Max((int)MathF.Ceiling(MinFrameLineSpacing / frameWidth), 1);
        var framesPerLabel = GetLabelInterval(frameWidth);

        float FrameToX(float frame) => origin.X + gutter + (frame - firstFrame) * frameWidth;

        var timelineEnd = lastFrame * frameDuration;

        var playbackStartX = FrameToX(0);
        var playbackEndX = FrameToX(View.Playback.PlaybackDuration / frameDuration);

        var outsideColor = ImGui.ColorConvertFloat4ToU32(OutsidePlaybackTint);

        // Tracks. With the frames outside of playback hidden, events get cut off where the timeline ends.
        drawList.PushClipRect(tracksMin, new Vector2(Math.Min(FrameToX(lastFrame), viewMax.X), viewMax.Y), true);

        for (int i = 0; i < rowLabels.Count; i += 2)
        {
            var y = origin.Y + ruler + i * rowHeight;

            drawList.AddRectFilled(new Vector2(tracksMin.X, y), new Vector2(FrameToX(lastFrame), y + rowHeight), rowColor);
        }

        for (int frame = AlignFrame(firstFrame, framesPerLine); frame <= lastFrame; frame += framesPerLine)
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

            var eventName = GetEventName(ev, template);
            var isDull = !EventFilter.IsMatch(ev, eventName);

            drawList.AddRectFilled(min, max, GetEventColor(ev.Type, isDull));

            if (View.Selection.IsEventSelected(ev))
            {
                drawList.AddRect(min, max, selectedColor, 0, ImDrawFlags.None, 2);

                // The primary event is the one the properties show, so it gets a second outline inside the first
                if (ev == View.Selection.SelectedEvent && View.Selection.SelectedEvents.Count > 1)
                {
                    drawList.AddRect(min + new Vector2(2, 2), max - new Vector2(2, 2), textColor);
                }
            }

            drawList.PushClipRect(min, Vector2.Min(max, viewMax), true);
            drawList.AddText(min + new Vector2(3, 2), isDull ? dullTextColor : textColor, eventName);
            drawList.PopClipRect();
        }

        // This goes over the events, so that the parts of them outside of playback are tinted as well
        DrawOutsidePlayback(drawList, FrameToX(firstFrame), playbackStartX, playbackEndX, FrameToX(lastFrame),
            tracksMin.Y, viewMax.Y, outsideColor, textColor);

        drawList.PopClipRect();

        // Track names stay in place as the timeline scrolls past them
        if (gutter > 0)
        {
            drawList.PushClipRect(new Vector2(viewPos.X, tracksMin.Y), new Vector2(tracksMin.X, viewMax.Y), true);

            drawList.AddRectFilled(new Vector2(viewPos.X, tracksMin.Y), new Vector2(tracksMin.X, viewMax.Y), headerColor);

            for (int i = 0; i < rowLabels.Count; i++)
            {
                drawList.AddText(new Vector2(viewPos.X + 4, origin.Y + ruler + i * rowHeight + 2), textColor, rowLabels[i]);
            }

            drawList.PopClipRect();
        }

        // Ruler
        drawList.PushClipRect(viewPos, new Vector2(viewMax.X, tracksMin.Y), true);

        drawList.AddRectFilled(viewPos, new Vector2(viewMax.X, tracksMin.Y), headerColor);

        for (int frame = AlignFrame(firstFrame, framesPerLabel); frame <= lastFrame; frame += framesPerLabel)
        {
            var x = FrameToX(frame);

            if (x >= tracksMin.X && x <= viewMax.X)
            {
                drawList.AddLine(new Vector2(x, tracksMin.Y - 6), new Vector2(x, tracksMin.Y), textColor);
                drawList.AddText(new Vector2(x + 3, viewPos.Y + 3), textColor, frame.ToString());
            }
        }

        DrawOutsidePlayback(drawList, FrameToX(firstFrame), playbackStartX, playbackEndX, FrameToX(lastFrame),
            viewPos.Y, tracksMin.Y, outsideColor, textColor);

        drawList.PopClipRect();

        // Playhead
        var playheadX = FrameToX(View.Playback.Time / frameDuration);

        if (playheadX >= tracksMin.X && playheadX <= viewMax.X)
        {
            drawList.AddLine(new Vector2(playheadX, viewPos.Y), new Vector2(playheadX, viewMax.Y),
                ImGui.ColorConvertFloat4ToU32(new Vector4(1, 0.25f, 0.25f, 1)), 2);
        }
    }

    /// <summary>
    /// Tints the frames that are only there to show events, and marks where playback starts and ends
    /// </summary>
    private static void DrawOutsidePlayback(ImDrawListPtr drawList, float timelineStartX, float playbackStartX,
        float playbackEndX, float timelineEndX, float top, float bottom, uint tintColor, uint lineColor)
    {
        if (CFG.Current.TimeActEditor_Graph_Display_Outside_Playback)
        {
            drawList.AddRectFilled(new Vector2(timelineStartX, top), new Vector2(playbackStartX, bottom), tintColor);
            drawList.AddRectFilled(new Vector2(playbackEndX, top), new Vector2(timelineEndX, bottom), tintColor);
        }

        drawList.AddLine(new Vector2(playbackStartX, top), new Vector2(playbackStartX, bottom), lineColor);
        drawList.AddLine(new Vector2(playbackEndX, top), new Vector2(playbackEndX, bottom), lineColor);
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
    /// Gives each type of event a colour of its own, spread out so that nearby IDs don't look alike.
    /// Dull colours are for events that don't pass the filter: mostly grey and darker, but with a hint of the type left.
    /// </summary>
    private static uint GetEventColor(int type, bool dull = false)
    {
        const float DullGreyAmount = 0.75f;
        const float DullDarken = 0.55f;

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

        if (dull)
        {
            var grey = new Vector3((color.X + color.Y + color.Z) / 3);

            color = Vector3.Lerp(color, grey, DullGreyAmount) * DullDarken;
        }

        return ImGui.ColorConvertFloat4ToU32(new Vector4(color, 1));
    }

    private void KeepPlayheadInView(Vector2 viewSize, float gutter, float frameWidth, float frameDuration, int firstFrame)
    {
        if (!View.Playback.Playing)
            return;

        var x = (View.Playback.Time / frameDuration - firstFrame) * frameWidth;
        var scroll = ImGui.GetScrollX();
        var visibleWidth = viewSize.X - gutter;

        if (x < scroll || x > scroll + visibleWidth)
        {
            ImGui.SetScrollX(x);
        }
    }

    /// <summary>
    /// Places a copy of the selected events at the same times as them. As they overlap their sources,
    /// the layout puts each copy on a row of its own within the track.
    /// </summary>
    private void DuplicateSelectedEvent()
    {
        var sources = View.Selection.SelectedEvents;

        if (sources.Count == 0)
            return;

        AddCopiesOfEvents(sources, 0);
    }

    /// <summary>
    /// Stores a copy of the selected events. Later edits to the events don't carry over to the copies,
    /// and they don't depend on the source, so they can be pasted into any time act of the project.
    /// </summary>
    private void CopySelectedEvent()
    {
        var selection = View.Selection;

        if (selection.SelectedEvents.Count == 0)
            return;

        var bigEndian = selection.SelectedTimeAct.TimeAct.BigEndian;

        View.Editor.CopiedEvents = selection.SelectedEvents.Select(e => e.GetClone(bigEndian)).ToList();
    }

    /// <summary>
    /// Pastes the copied events so that the earliest of them starts at the time given, and the rest keep their spacing
    /// </summary>
    private void PasteEvent(float time)
    {
        var copied = View.Editor.CopiedEvents;

        if (copied.Count == 0 || View.Selection.SelectedAnimation == null)
            return;

        AddCopiesOfEvents(copied, time - copied.Min(e => e.StartTime));
    }

    private void AddCopiesOfEvents(IReadOnlyList<TAE.Event> sources, float offset)
    {
        if (!CanEdit)
            return;

        var selection = View.Selection;
        var bigEndian = selection.SelectedTimeAct.TimeAct.BigEndian;

        var copies = new List<TAE.Event>(sources.Count);

        foreach (var source in sources)
        {
            var copy = source.GetClone(bigEndian);

            copy.StartTime = source.StartTime + offset;

            // The placeholder has to stay as it is, or the copy will stop short of the end
            copy.EndTime = IsOpenEnded(source) ? source.EndTime : source.EndTime + offset;

            copies.Add(copy);
        }

        View.ViewportActionManager.ExecuteAction(new TimeActAddEventsAction(selection, selection.SelectedTimeAct,
            selection.SelectedAnimation, copies));
    }

    private void DisplayEventMenu()
    {
        if (ImGui.BeginPopup("##graphEventMenu"))
        {
            var selectedCount = View.Selection.SelectedEvents.Count;

            if (ImGui.MenuItem($"{GetCountLabel("TAE_GraphWindow_Copy_Event", selectedCount)}##copyEvent",
                InputManager.GetHint(KeybindID.Copy)))
            {
                CopySelectedEvent();
            }

            ImGui.BeginDisabled(!CanEdit);

            if (ImGui.MenuItem($"{GetCountLabel("TAE_GraphWindow_Duplicate_Event", selectedCount)}##duplicateEvent",
                InputManager.GetHint(KeybindID.Duplicate)))
            {
                DuplicateSelectedEvent();
            }

            if (ImGui.MenuItem($"{GetCountLabel("TAE_GraphWindow_Delete_Event", selectedCount)}##deleteEvent",
                InputManager.GetHint(KeybindID.Delete)))
            {
                DeleteSelectedEvents();
            }

            ImGui.EndDisabled();

            ImGui.Separator();

            DisplaySelectAllMenuItem();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Selecting is not an edit, so this stays available when the time acts are read-only
    /// </summary>
    private bool DisplaySelectAllMenuItem()
    {
        if (!ImGui.MenuItem($"{LOC.Get("TAE_GraphWindow_Select_All")}##selectAllEvents",
            InputManager.GetHint(KeybindID.SelectAll)))
        {
            return false;
        }

        SelectAllEvents();

        return true;
    }

    /// <summary>
    /// The singular label, or its plural with a count when there is more than one event. The plural key ends in an s.
    /// </summary>
    private static string GetCountLabel(string singularKey, int count)
    {
        return count > 1 ? LOC.Get($"{singularKey}s", count) : LOC.Get(singularKey);
    }

    private void DisplayAddEventMenu(TAE.Template.BankTemplate template)
    {
        var popupSize = new Vector2(360, 400) * DPI.UIScale();

        ImGui.SetNextWindowSize(popupSize, ImGuiCond.Always);

        if (ImGui.BeginPopup("##graphAddEventMenu"))
        {
            if (View.Selection.SelectedAnimation?.Events.Count > 0)
            {
                if (DisplaySelectAllMenuItem())
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.Separator();
            }

            var copiedCount = View.Editor.CopiedEvents.Count;

            if (copiedCount > 0)
            {
                if (ImGui.MenuItem($"{GetCountLabel("TAE_GraphWindow_Paste_Event", copiedCount)}##pasteEvent",
                    InputManager.GetHint(KeybindID.Paste)))
                {
                    PasteEvent(AddEventTime);

                    ImGui.CloseCurrentPopup();
                }

                ImGui.Separator();
            }

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
