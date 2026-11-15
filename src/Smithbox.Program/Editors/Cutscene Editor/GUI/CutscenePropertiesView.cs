#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
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
        Row(LOC.Get("CUTSCENE_Properties_Frames"), LOC.Get("CUTSCENE_Properties_Frames_Value", e.StartFrame, e.StartFrame + e.Duration, e.Duration));

        if (resource != null)
        {
            ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Resource"));
            Row(LOC.Get("CUTSCENE_Properties_Name"), resource.Name);
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

        DisplayParameters("event", e.Parameters);

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

    private static void DisplayParameters(string id, System.Collections.Generic.List<MQB.Parameter> parameters, bool header = true)
    {
        if (parameters.Count == 0)
            return;

        if (header)
            ImGui.SeparatorText(LOC.Get("CUTSCENE_Properties_Parameters", parameters.Count));

        if (!ImGui.BeginTable($"##cutsceneParams_{id}", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
            return;

        ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Name"));
        ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Type"));
        ImGui.TableSetupColumn(LOC.Get("CUTSCENE_Properties_Value"));
        ImGui.TableHeadersRow();

        foreach (var p in parameters)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(p.Name);
            ImGui.TableNextColumn();
            ImGui.TextDisabled(p.MemberCount > 1 ? $"{p.Type}{p.MemberCount}" : p.Type.ToString());
            ImGui.TableNextColumn();
            ImGui.Text(ValueText(p.Value));
            if (p.Sequences.Count > 0)
            {
                ImGui.SameLine();
                ImGui.TextDisabled(LOC.Get("CUTSCENE_Properties_Animated", p.Sequences.Sum(s => s.Points.Count)));
                if (ImGui.IsItemHovered())
                {
                    var points = string.Join("\n", p.Sequences.SelectMany(s => s.Points.Select(pt => $"{ValueText(pt.Value)} @ {pt.Unk08}")).Take(24));
                    ImGui.SetTooltip(points);
                }
            }
        }

        ImGui.EndTable();
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
