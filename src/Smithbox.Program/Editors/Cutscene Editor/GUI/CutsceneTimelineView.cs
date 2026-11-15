#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The selected cut's timelines, as the MQB nests them (each under its ParentIndex), with a bar for each event and the
/// playhead, under the playback controls. Clicking a bar selects its event; dragging it moves it, dragging its right
/// edge sets its length (one undo step each); right-clicking it duplicates or deletes it. Dragging elsewhere scrubs.
/// </summary>
public class CutsceneTimelineView
{
    private readonly CutsceneEditorScreen Editor;

    // The rows of the cut last drawn: each timeline and how deep it is in the tree
    private MQB.Cut? RowsFor;
    private int RowsVersion = -1;
    private readonly List<(int Timeline, int Depth)> Rows = new();

    private bool Scrubbing;

    // A bar being dragged: the event, whether its end (else all of it) moves, and where it started
    private MQB.Event? DragEvent;
    private bool DragResize;
    private float DragMouseX;
    private int DragStart, DragDuration, DragUnk18;
    private bool DragMoved;

    // The bar right-clicked, for its menu
    private MQB.Event? MenuEvent;

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

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("CUTSCENE_Timeline_Add_Event")}##cutsceneAddEvent"))
        {
            Editor.Templates.Start();
            ImGui.OpenPopup("##cutsceneAddEventMenu");
        }
        GUI.Tooltip(LOC.Get("CUTSCENE_Timeline_Add_Event_TT"));
        AddEventMenu(scene);

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
        if (RowsFor == cut && RowsVersion == Editor.StructureVersion)
            return;

        RowsFor = cut;
        RowsVersion = Editor.StructureVersion;
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

        ImGui.InvisibleButton("##cutsceneTrackCanvas", size, ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight);
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
        var hoveredEdge = false;
        var edge = 5 * scale;

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

                if (hovered && mouse.X >= min.X && mouse.X <= max.X + edge && mouse.Y >= min.Y && mouse.Y <= max.Y)
                {
                    hoveredEvent = e;
                    hoveredTimeline = t;
                    hoveredEdge = mouse.X >= max.X - edge;
                }
            }
        }

        // Playhead
        var playX = FrameToX(Editor.Playback.Frame);
        drawList.AddLine(new Vector2(playX, origin.Y), new Vector2(playX, origin.Y + size.Y), selectedColor, 2 * scale);

        if (hoveredEdge && DragEvent == null)
            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEw);

        // Input: a click on a bar selects its event and starts dragging it; anywhere else in the tracks scrubs
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            if (hoveredEvent != null)
            {
                Editor.SelectEvent(hoveredEvent, hoveredTimeline);

                DragEvent = hoveredEvent;
                DragResize = hoveredEdge;
                DragMouseX = mouse.X;
                DragStart = hoveredEvent.StartFrame;
                DragDuration = hoveredEvent.Duration;
                DragUnk18 = hoveredEvent.Unk18;
                DragMoved = false;
            }
            else if (mouse.X > origin.X + gutter)
            {
                Scrubbing = true;
            }
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && hoveredEvent != null)
        {
            Editor.SelectEvent(hoveredEvent, hoveredTimeline);
            MenuEvent = hoveredEvent;
            ImGui.OpenPopup("##cutsceneEventMenu");
        }

        if (DragEvent != null)
        {
            if (active)
            {
                // Whole frames, live; the undo step comes when the drag ends
                var delta = (int)MathF.Round((mouse.X - DragMouseX) / trackWidth * frames);
                if (delta != 0)
                    DragMoved = true;

                if (DragResize)
                {
                    CutsceneMqbOps.SetDuration(DragEvent, Math.Max(1, DragDuration + delta));
                }
                else
                {
                    DragEvent.StartFrame = Math.Max(0, DragStart + delta);
                }
            }
            else
            {
                EndDrag();
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

        EventMenu(scene);

        if (hoveredEvent != null)
        {
            var res = hoveredEvent.ResourceIndex >= 0 && hoveredEvent.ResourceIndex < mqb.Resources.Count ? mqb.Resources[hoveredEvent.ResourceIndex] : null;
            ImGui.SetTooltip(LOC.Get("CUTSCENE_Timeline_Event_TT", res?.Name ?? "?", hoveredEvent.ID, hoveredEvent.StartFrame,
                hoveredEvent.StartFrame + hoveredEvent.Duration));
        }

        ImGui.EndChild();
    }

    private string AddFilter = "";

    private void AddEventMenu(CutsceneScene scene)
    {
        if (!ImGui.BeginPopup("##cutsceneAddEventMenu"))
            return;

        var templates = Editor.Templates;
        if (!templates.IsReady)
        {
            ImGui.TextDisabled(templates.IsBuilding ? LOC.Get("CUTSCENE_Timeline_Templates_Reading") : LOC.Get("CUTSCENE_Timeline_Templates_None"));
            ImGui.EndPopup();
            return;
        }

        var parent = Editor.SelectedTimeline;
        ImGui.TextDisabled(parent >= 0
            ? LOC.Get("CUTSCENE_Timeline_Add_Under", parent, (int)Editor.Playback.Frame)
            : LOC.Get("CUTSCENE_Timeline_Add_Root", (int)Editor.Playback.Frame));

        ImGui.SetNextItemWidth(220 * DPI.UIScale());
        ImGui.InputTextWithHint("##cutsceneAddFilter", LOC.Get("CUTSCENE_List_Filter_Hint"), ref AddFilter, 64);

        foreach (var template in templates.Templates)
        {
            var name = CutsceneResourceTypes.Name(template.Type);
            if (AddFilter.Length > 0 && !name.Contains(AddFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            if (ImGui.Selectable($"{name}##addType{template.Type}"))
                AddFromTemplate(scene, template, parent);

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(LOC.Get("CUTSCENE_Timeline_Template_TT", template.Source, template.Resource.Name, template.Event.Parameters.Count));
        }

        ImGui.EndPopup();
    }

    /// <summary>
    /// A new event of a template's type at the playhead, under the selected timeline, with a resource of its own (named
    /// as the template's, made unique). The template's values are its starting values.
    /// </summary>
    private void AddFromTemplate(CutsceneScene scene, CutsceneTemplate template, int parentTimeline)
    {
        if (Editor.SelectedFile is not { Mqb: { } mqb } file)
            return;

        var resource = CutsceneMqbOps.Clone(template.Resource);
        resource.Name = CutsceneMqbOps.UniqueName(mqb, resource.Name);

        // Its parent in the template's file means nothing here: effect resources are mostly at the root
        resource.ParentIndex = -1;

        var e = CutsceneMqbOps.Clone(template.Event);
        e.ID = CutsceneMqbOps.NextEventId(mqb);
        e.StartFrame = Math.Clamp((int)Editor.Playback.Frame, 0, Math.Max(0, scene.FrameCount - 1));
        CutsceneMqbOps.SetDuration(e, Math.Clamp(template.Event.Duration, 1, Math.Max(1, scene.FrameCount - e.StartFrame)));

        var parent = parentTimeline >= 0 && parentTimeline < scene.Cut.Timelines.Count ? parentTimeline : -1;

        Editor.Execute(CutsceneMqbOps.AddEvent(file, scene.Cut, e, parent,
            template.TimelineParameters.Select(CutsceneMqbOps.Clone).ToList(), resource));
        Editor.SelectEvent(e, scene.Cut.Timelines.Count - 1);
        ImGui.CloseCurrentPopup();
    }

    /// <summary>A drag let go: one undo step from where the event was to where it is</summary>
    private void EndDrag()
    {
        var e = DragEvent;
        DragEvent = null;

        if (e == null || Editor.SelectedFile is not { } file || !DragMoved)
            return;

        var (start, duration, unk18) = (DragStart, DragDuration, DragUnk18);
        var (newStart, newDuration, newUnk18) = (e.StartFrame, e.Duration, e.Unk18);
        if (start == newStart && duration == newDuration)
            return;

        Editor.Execute(new CutsceneEditAction(file,
            () => { e.StartFrame = newStart; e.Duration = newDuration; e.Unk18 = newUnk18; e.Unk1C = newUnk18; },
            () => { e.StartFrame = start; e.Duration = duration; e.Unk18 = unk18; e.Unk1C = unk18; }));
    }

    private void EventMenu(CutsceneScene scene)
    {
        if (!ImGui.BeginPopup("##cutsceneEventMenu"))
            return;

        var file = Editor.SelectedFile;
        var e = MenuEvent;
        var mqb = file?.Mqb;

        if (file == null || e == null || mqb == null)
        {
            ImGui.EndPopup();
            return;
        }

        var type = e.ResourceIndex >= 0 && e.ResourceIndex < mqb.Resources.Count ? mqb.Resources[e.ResourceIndex].Type : -1;

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Timeline_Duplicate"), InputManagerHint(StudioCore.Keybinds.KeybindID.Duplicate)))
            Duplicate(e, false);
        GUI.Tooltip(LOC.Get("CUTSCENE_Timeline_Duplicate_TT"));

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Timeline_Duplicate_New_Resource"), "", false, CutsceneMqbOps.CanTemplate(type)))
            Duplicate(e, true);
        GUI.Tooltip(LOC.Get("CUTSCENE_Timeline_Duplicate_New_Resource_TT"));

        ImGui.Separator();

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Timeline_Delete"), InputManagerHint(StudioCore.Keybinds.KeybindID.Delete)))
            Delete(e);
        GUI.Tooltip(LOC.Get("CUTSCENE_Timeline_Delete_TT"));

        ImGui.EndPopup();
    }

    private static string InputManagerHint(StudioCore.Keybinds.KeybindID id) => StudioCore.Keybinds.InputManager.GetHint(id);

    public void Duplicate(MQB.Event e, bool newResource)
    {
        if (Editor.SelectedFile is not { Mqb: not null } file || Editor.Scene is not { } scene)
            return;

        Editor.Execute(CutsceneMqbOps.Duplicate(file, scene.Cut, e, newResource));

        // The copy is the last event of the last timeline
        var copy = scene.Cut.Timelines[^1].Events[0];
        Editor.SelectEvent(copy, scene.Cut.Timelines.Count - 1);
    }

    public void Delete(MQB.Event e)
    {
        if (Editor.SelectedFile is not { } file || Editor.Scene is not { } scene)
            return;

        Editor.Execute(CutsceneMqbOps.Delete(file, scene.Cut, e));
    }
}
