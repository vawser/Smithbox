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
/// The selected event: its resource, timing, parameters and transform keys, and its timeline's parameters. With no
/// event selected, what the cut was built from: actors and the state of their models, cameras, placements, problems.
/// Read-only for now.
/// </summary>
public class CutscenePropertiesView
{
    private readonly CutsceneEditorScreen Editor;

    public CutscenePropertiesView(CutsceneEditorScreen editor)
    {
        Editor = editor;
    }

    public void Display()
    {
        var mqb = Editor.SelectedFile?.Mqb;
        var scene = Editor.Scene;
        if (mqb == null || scene == null)
            return;

        if (Editor.SelectedPlacement is { } placement)
            DisplayPlacementEditor(placement);

        if (Editor.SelectedEvent is { } e)
        {
            if (ImGui.Button($"{LOC.Get("CUTSCENE_Properties_Back")}##cutsceneDeselect"))
            {
                Editor.SelectedEvent = null;
                Editor.SelectedTimeline = -1;
                Editor.SelectedPlacement = null;
                return;
            }

            DisplayEvent(mqb, scene, e);
        }
        else
        {
            DisplayScene(scene);
        }
    }

    private void DisplayEvent(MQB mqb, CutsceneScene scene, MQB.Event e)
    {
        var resource = e.ResourceIndex >= 0 && e.ResourceIndex < mqb.Resources.Count ? mqb.Resources[e.ResourceIndex] : null;

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Event"));
        Row(LOC.Get("CUTSCENE_Properties_Event_Id"), e.ID.ToString());

        // Start and length, in frames of the cut; the length carries Unk18/Unk1C along where they follow it
        ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Start"));
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.SetNextItemWidth(120 * DPI.UIScale());
        // Each value is taken before its widget, whose +/- buttons activate and edit in the same frame
        var startBefore = e.StartFrame;
        var start = e.StartFrame;
        if (ImGui.InputInt("##cutsceneEventStart", ref start))
            e.StartFrame = Math.Max(0, start);
        TrackEdit(("start", e), startBefore, () => e.StartFrame, x => e.StartFrame = x);

        ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Duration"));
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.SetNextItemWidth(120 * DPI.UIScale());
        var durationBefore = (e.Duration, e.Unk18);
        var duration = e.Duration;
        if (ImGui.InputInt("##cutsceneEventDuration", ref duration))
            CutsceneMqbOps.SetDuration(e, Math.Max(1, duration));
        TrackEdit(("duration", e), durationBefore, () => (e.Duration, e.Unk18),
            x => { e.Duration = x.Item1; e.Unk18 = x.Item2; e.Unk1C = x.Item2; });
        ImGui.SameLine();
        ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Frames_End", e.StartFrame + e.Duration));

        if (ImGui.TreeNode($"{LOC.Get("CUTSCENE_Properties_Advanced")}##cutsceneAdvanced"))
        {
            ImGui.TextDisabled("Unk18 / Unk1C");
            ImGui.SameLine(140 * DPI.UIScale());
            ImGui.SetNextItemWidth(120 * DPI.UIScale());
            var unkBefore = e.Unk18;
            var unk = e.Unk18;
            if (ImGui.InputInt("##cutsceneEventUnk18", ref unk))
            {
                e.Unk18 = unk;
                e.Unk1C = unk;
            }
            TrackEdit(("unk18", e), unkBefore, () => e.Unk18, x => { e.Unk18 = x; e.Unk1C = x; });
            GUI.Tooltip(LOC.Get("CUTSCENE_Properties_Unk18_TT"));
            ImGui.TreePop();
        }

        if (resource != null)
        {
            ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Resource"));

            // Models, cameras and the HKT are found in the Havok file by name, so only other resources can be renamed
            if (CutsceneMqbOps.CanTemplate(resource.Type))
            {
                ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Name"));
                ImGui.SameLine(140 * DPI.UIScale());
                ImGui.SetNextItemWidth(-1);
                var name = resource.Name;
                if (ImGui.InputText("##cutsceneResourceName", ref name, 31))
                    resource.Name = name;
                TrackEdit(("name", resource), resource.Name, () => resource.Name, x => resource.Name = x);
            }
            else
            {
                Row(LOC.Get("CUTSCENE_Properties_Name"), resource.Name);
            }
            Row(LOC.Get("CUTSCENE_Properties_Type"), $"{CutsceneResourceTypes.Name(resource.Type)} ({resource.Type})");
            Row(LOC.Get("CUTSCENE_Properties_Index"), e.ResourceIndex.ToString());

            if (resource.ParentIndex >= 0 && resource.ParentIndex < mqb.Resources.Count)
                Row(LOC.Get("CUTSCENE_Properties_Parent"), $"{mqb.Resources[resource.ParentIndex].Name} ({resource.ParentIndex})");

            if (!string.IsNullOrEmpty(resource.Path))
                Row(LOC.Get("CUTSCENE_Properties_Path"), resource.Path);

            DisplayParameters("resource", resource.Parameters);

            // What the viewport made of it
            if (scene.Actors.FirstOrDefault(a => a.ResourceIndex == e.ResourceIndex) is { } actor)
            {
                Row(LOC.Get("CUTSCENE_Properties_Model"), actor.Model?.ModelName ?? LOC.Get("CUTSCENE_Properties_None"));
                Row(LOC.Get("CUTSCENE_Properties_State"), $"{actor.State}{(actor.Status != null ? $": {actor.Status}" : "")}");
                Row(LOC.Get("CUTSCENE_Properties_Bones"), actor.Skeleton?.BoneCount.ToString() ?? LOC.Get("CUTSCENE_Properties_None"));
            }
        }

        DisplayParameters("event", e.Parameters, true, e);

        if (e.Transforms.Count > 0)
        {
            ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Transforms", e.Transforms.Count));
            foreach (var t in e.Transforms.Take(16))
                ImGui.TextWrapped($"f{t.Frame:0}: {Vec(t.Translation)}  rot {Vec(t.Rotation)}  scale {Vec(t.Scale)}");
        }

        if (Editor.SelectedTimeline >= 0 && Editor.SelectedTimeline < scene.Cut.Timelines.Count)
        {
            var timeline = scene.Cut.Timelines[Editor.SelectedTimeline];
            ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Timeline", Editor.SelectedTimeline, timeline.ParentIndex));
            DisplayParameters("timeline", timeline.Parameters, false);
        }
    }

    // ---- editing: a field's value when it started being edited, by what it edits, so an edit is one undo step

    private readonly Dictionary<object, object?> EditStart = new();

    /// <summary>
    /// After a widget that edits a value live: remembers the value it had when the widget was activated, and when it
    /// is let go after an edit, records the change as one undo step.
    /// </summary>
    private void TrackEdit<T>(object key, T before, Func<T> get, Action<T> set)
    {
        if (ImGui.IsItemActivated())
            EditStart[key] = before;

        if (ImGui.IsItemDeactivatedAfterEdit() && Editor.SelectedFile is { } file)
        {
            var start = EditStart.TryGetValue(key, out var s) && s is T t ? t : before;
            EditStart.Remove(key);

            var after = get();
            if (!Equals(start, after))
                Editor.Execute(new CutsceneEditAction(file, () => set(after), () => set(start)));
        }
    }

    private void DisplayParameters(string id, List<MQB.Parameter> parameters, bool header = true, MQB.Event? owner = null)
    {
        if (parameters.Count == 0)
            return;

        if (header)
            ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Parameters", parameters.Count));

        if (!ImGui.BeginTable($"##cutsceneParams_{id}", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
            return;

        ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Name"));
        ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Type"), ImGuiTableColumnFlags.WidthFixed, 70 * DPI.UIScale());
        ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Value"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        for (var i = 0; i < parameters.Count; i++)
        {
            var p = parameters[i];
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(p.Name);
            ImGui.TableNextColumn();
            ImGui.TextDisabled(p.MemberCount > 1 ? $"{p.Type}{p.MemberCount}" : p.Type.ToString());
            ImGui.TableNextColumn();

            ImGui.SetNextItemWidth(-1);
            EditValue($"##{id}_p{i}", p, () => p.Value, v => p.Value = v);

            if (p.Sequences.Count > 0)
                DisplaySequences($"{id}_p{i}", p, owner);
        }

        ImGui.EndTable();
    }

    /// <summary>A widget for a value of the parameter's type, which edits it live; one undo step per edit.</summary>
    private void EditValue(string id, object key, Func<object> get, Action<object> set)
    {
        var value = get();
        switch (value)
        {
            case bool b:
            {
                var v = b;
                if (ImGui.Checkbox(id, ref v)) set(v);
                TrackEdit<object>(key, b, get, set);
                break;
            }
            case sbyte sb:
                IntField(id, key, sb, sbyte.MinValue, sbyte.MaxValue, x => (sbyte)x, get, set);
                break;
            case byte by:
                IntField(id, key, by, byte.MinValue, byte.MaxValue, x => (byte)x, get, set);
                break;
            case short sh:
                IntField(id, key, sh, short.MinValue, short.MaxValue, x => (short)x, get, set);
                break;
            case int n:
                IntField(id, key, n, int.MinValue, int.MaxValue, x => (int)x, get, set);
                break;
            case uint u:
            {
                // Can be past int's range (event flags, colours as ARGB), so it is typed as text
                var text = u.ToString();
                if (ImGui.InputText(id, ref text, 12, ImGuiInputTextFlags.CharsDecimal) && uint.TryParse(text, out var parsed))
                    set(parsed);
                TrackEdit<object>(key, u, get, set);
                break;
            }
            case float f:
            {
                var v = f;
                if (ImGui.DragFloat(id, ref v, MathF.Max(0.001f, MathF.Abs(f) * 0.01f), 0, 0, "%.4f")) set(v);
                TrackEdit<object>(key, f, get, set);
                break;
            }
            case string s:
            {
                var v = s;
                if (ImGui.InputText(id, ref v, 256)) set(v);
                TrackEdit<object>(key, s, get, set);
                break;
            }
            case System.Drawing.Color c:
            // Color and IntColor both read into a Color, with channels of 0-255; the alpha (IntColor's fourth) is kept
            {
                var v = new Vector3(c.R, c.G, c.B) / 255f;
                if (ImGui.ColorEdit3(id, ref v))
                    set(System.Drawing.Color.FromArgb(c.A, (int)(v.X * 255 + 0.5f), (int)(v.Y * 255 + 0.5f), (int)(v.Z * 255 + 0.5f)));
                TrackEdit<object>(key, c, get, set);
                break;
            }
            case Vector2 v2:
            {
                var v = v2;
                if (ImGui.DragFloat2(id, ref v, 0.01f)) set(v);
                TrackEdit<object>(key, v2, get, set);
                break;
            }
            case Vector3 v3:
            {
                var v = v3;
                if (ImGui.DragFloat3(id, ref v, 0.01f)) set(v);
                TrackEdit<object>(key, v3, get, set);
                break;
            }
            case Vector4 v4:
            {
                var v = v4;
                if (ImGui.DragFloat4(id, ref v, 0.01f)) set(v);
                TrackEdit<object>(key, v4, get, set);
                break;
            }
            default:
                ImGui.TextDisabled(ValueText(value));
                break;
        }
    }

    private void IntField(string id, object key, long current, long min, long max, Func<long, object> make, Func<object> get, Action<object> set)
    {
        // Taken before the widget: its +/- buttons activate and edit in the same frame
        var before = get();
        var v = (int)Math.Clamp(current, int.MinValue, int.MaxValue);
        if (ImGui.InputInt(id, ref v))
            set(make(Math.Clamp(v, min, max)));
        TrackEdit(key, before, get, set);
    }

    /// <summary>
    /// The keys of an animated parameter: a frame (from the event's start) and a value each. Keys are added at the
    /// playhead, and taken away with their button; each is one undo step.
    /// </summary>
    private void DisplaySequences(string id, MQB.Parameter p, MQB.Event? owner)
    {
        if (!ImGui.TreeNode($"{LOC.Get("CUTSCENE_Properties_Animated", p.Sequences.Sum(s => s.Points.Count))}##{id}_seq"))
            return;

        for (var s = 0; s < p.Sequences.Count; s++)
        {
            var sequence = p.Sequences[s];
            if (p.Sequences.Count > 1)
                ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Sequence", s, sequence.ValueIndex));

            for (var k = 0; k < sequence.Points.Count; k++)
            {
                var point = sequence.Points[k];

                var frameBefore = point.Unk08;
                var frame = point.Unk08;
                ImGui.SetNextItemWidth(70 * DPI.UIScale());
                if (ImGui.InputInt($"##{id}_s{s}_k{k}_f", ref frame))
                    point.Unk08 = Math.Max(0, frame);
                TrackEdit($"{id}_s{s}_k{k}_f", frameBefore, () => point.Unk08, x => point.Unk08 = x);

                ImGui.SameLine();
                ImGui.SetNextItemWidth(110 * DPI.UIScale());
                EditValue($"##{id}_s{s}_k{k}_v", point, () => point.Value, x => point.Value = x);

                ImGui.SameLine();
                if (ImGui.SmallButton($"x##{id}_s{s}_k{k}_del") && Editor.SelectedFile is { } file && sequence.Points.Count > 1)
                {
                    var index = k;
                    Editor.Execute(new CutsceneEditAction(file,
                        () => sequence.Points.Remove(point),
                        () => sequence.Points.Insert(Math.Min(index, sequence.Points.Count), point)));
                    break;
                }
                GUI.Tooltip(LOC.Get("CUTSCENE_Properties_Key_Delete_TT"));
            }

            if (ImGui.SmallButton($"{LOC.Get("CUTSCENE_Properties_Key_Add")}##{id}_s{s}_add") && Editor.SelectedFile is { } addFile && sequence.Points.Count > 0)
            {
                // At the playhead, from the event's start, with the value of the key before it
                var at = Math.Max(0, (int)Editor.Playback.Frame - (owner?.StartFrame ?? 0));
                var before = sequence.Points.LastOrDefault(pt => pt.Unk08 <= at) ?? sequence.Points[0];
                var key = new MQB.Parameter.Sequence.Point { Value = before.Value, Unk08 = at, Unk10 = before.Unk10, Unk14 = before.Unk14 };
                var index = sequence.Points.Count(pt => pt.Unk08 <= at);

                Editor.Execute(new CutsceneEditAction(addFile,
                    () => sequence.Points.Insert(Math.Min(index, sequence.Points.Count), key),
                    () => sequence.Points.Remove(key)));
            }
            GUI.Tooltip(LOC.Get("CUTSCENE_Properties_Key_Add_TT"));
        }

        ImGui.TreePop();
    }

    private void DisplayScene(CutsceneScene scene)
    {
        ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Hint"));

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Cut", scene.Cut.Name, scene.FrameCount));

        foreach (var problem in scene.Problems)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, problem);

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Cameras", scene.Cameras.Count));
        foreach (var c in scene.Cameras)
        {
            var range = c.Events.Count > 0 ? $"{c.Events.Min(e => e.StartFrame)}-{c.Events.Max(e => e.StartFrame + e.Duration)}" : "-";
            ImGui.Text($"{c.Name}  f{range}  fov {(c.FovRadians(Editor.Playback.Frame) is { } f ? $"{f * 180 / MathF.PI:0.0}°" : "-")}  near {c.NearPlane:0.###} far {c.FarPlane:0}");
        }

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Actors", scene.Actors.Count));
        if (ImGui.BeginTable("##cutsceneActors", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Name"));
            ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Model"));
            ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Bones"));
            ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_State"));
            ImGui.TableHeadersRow();

            foreach (var a in scene.Actors)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(a.Name);
                ImGui.TableNextColumn();
                ImGui.Text(a.Model?.ModelName ?? "-");
                ImGui.TableNextColumn();
                // Bones of the clip, and once loaded, how many of the model's nodes follow one of them
                ImGui.Text(a.State == CutsceneActorState.Loaded
                    ? $"{a.Skeleton?.BoneCount ?? 0} ({a.MappedNodeCount}/{a.ModelNodeCount})"
                    : a.Skeleton?.BoneCount.ToString() ?? "-");
                ImGui.TableNextColumn();
                if (a.State == CutsceneActorState.Failed)
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, a.Status ?? a.State.ToString());
                else
                    ImGui.Text(a.State.ToString());
            }

            ImGui.EndTable();
        }

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Placements", scene.Placements.Count));
        foreach (var p in scene.Placements.Values)
        {
            var m = p.At(Editor.Playback.Frame);

            if (p.OffsetEvent != null)
            {
                if (ImGui.SmallButton($"{LOC.Get("CUTSCENE_Properties_Move")}##cutsceneMove{p.HktResourceIndex}"))
                    Editor.SelectedPlacement = p;
                GUI.Tooltip(LOC.Get("CUTSCENE_Properties_Move_TT"));
                ImGui.SameLine();
            }

            ImGui.Text($"{p.HktName}: {Vec(m.Translation)}{(p.Keys.Count == 0 ? $"  ({LOC.Get("CUTSCENE_Properties_Origin")})" : "")}");
        }
    }

    // The keys of the placement as they were when a drag field started being edited, for one undo step per edit
    private (Vector3, Vector3)[]? FieldEditStart;
    private CutscenePlacement? FieldEditPlacement;

    /// <summary>
    /// The selected placement's position and rotation, as its first key has them. An edit moves every key by as much,
    /// and becomes one undo step when the field is let go. The viewport's gizmo moves it too.
    /// </summary>
    private void DisplayPlacementEditor(CutscenePlacement placement)
    {
        if (placement.OffsetEvent == null || placement.Keys.Count == 0 || Editor.SelectedFile is not { } file)
            return;

        ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Placement", placement.HktName));
        ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Placement_Hint"));

        var first = placement.Keys[0];
        var translation = first.Translation;
        var rotation = first.Rotation;

        var changed = false;
        var t = translation;
        var r = rotation;

        ImGui.SetNextItemWidth(-1);
        changed |= ImGui.DragFloat3("##cutscenePlacementPos", ref t, 0.01f);
        var activated = ImGui.IsItemActivated();
        var deactivated = ImGui.IsItemDeactivatedAfterEdit();

        ImGui.SetNextItemWidth(-1);
        changed |= ImGui.DragFloat3("##cutscenePlacementRot", ref r, 0.25f);
        activated |= ImGui.IsItemActivated();
        deactivated |= ImGui.IsItemDeactivatedAfterEdit();
        GUI.Tooltip(LOC.Get("CUTSCENE_Properties_Placement_Fields_TT"));

        if (activated || FieldEditPlacement != placement)
        {
            FieldEditStart = CutscenePlacementAction.Snapshot(placement.Keys);
            FieldEditPlacement = placement;
        }

        // Live while dragging, from the keys as they were when the drag started
        if (changed && FieldEditStart != null)
        {
            var dt = t - FieldEditStart[0].Item1;
            var dr = r - FieldEditStart[0].Item2;
            CutscenePlacementAction.Apply(placement.Keys, CutscenePlacementAction.Shifted(FieldEditStart, dt, dr));
        }

        if (deactivated && FieldEditStart != null)
        {
            var after = CutscenePlacementAction.Snapshot(placement.Keys);
            Editor.ActionManager.ExecuteAction(new CutscenePlacementAction(file, placement.Keys, FieldEditStart, after));
            FieldEditStart = CutscenePlacementAction.Snapshot(placement.Keys);
        }

        if (file.IsDirty)
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("CUTSCENE_Properties_Unsaved"));

        if (ImGui.Button($"{LOC.Get("CUTSCENE_Properties_Placement_Done")}##cutscenePlacementDone"))
            Editor.SelectedPlacement = null;
    }

    private static void Row(string label, string value)
    {
        ImGui.TextDisabled(label);
        ImGui.SameLine(140 * DPI.UIScale());
        ImGui.TextWrapped(value);
    }

    private static string Vec(Vector3 v) => $"({v.X:0.###}, {v.Y:0.###}, {v.Z:0.###})";

    private static string ValueText(object? v) => v switch
    {
        null => "null",
        float f => f.ToString("0.####"),
        Vector2 v2 => $"({v2.X:0.###}, {v2.Y:0.###})",
        Vector3 v3 => Vec(v3),
        Vector4 v4 => $"({v4.X:0.###}, {v4.Y:0.###}, {v4.Z:0.###}, {v4.W:0.###})",
        string s => $"\"{s}\"",
        _ => v.ToString() ?? ""
    };
}
